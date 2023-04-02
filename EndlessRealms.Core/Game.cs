using EndlessRealms.Core.Services;
using EndlessRealms.Core.Services.ActionResponseHandler;
using EndlessRealms.Core.Services.ChatGptDto;
using EndlessRealms.Core.Utility;
using EndlessRealms.Core.Utility.Extensions;
using EndlessRealms.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core
{
    [Service]
    public class Game
    {
        private readonly IRenderService _renderService;
        private readonly IServiceProvider _serviceProvider;
        private readonly WorldService _worldService;
        private readonly IPlayerIoService _playerIoService;
        private readonly ILogService _logService;
        private readonly ChatGPTService _gptService;
        private readonly IPersistedDataProvider _persistedDataAccessor;
        private readonly SystemStatusManager _systemStatusManager;

        public PlayerInfo CurrentPlayer { get; set; } = null!;

        public Game(IRenderService renderService, WorldService worldService, IPlayerIoService playerIoService, ILogService logService, ChatGPTService chatGPTService, IPersistedDataProvider persistedDataAccessor, IServiceProvider serviceProvider, SystemStatusManager systemStatusManager)
        {
            _renderService = renderService;
            _worldService = worldService;
            _playerIoService = playerIoService;
            _logService = logService;
            _gptService = chatGPTService;
            _persistedDataAccessor = persistedDataAccessor;
            _serviceProvider = serviceProvider;
            _systemStatusManager = systemStatusManager;
        }
        public async Task Start()
        {
            try
            {
                CurrentPlayer = (await _persistedDataAccessor.LoadPlayerInfo())!;
                if(CurrentPlayer == null)
                {
                    await InitializePlayerInfo();
                }

                await _worldService.Initialize();

                while (true)
                {
                    await UpdateScene();
                }

            }
            catch (Exception ex)
            {
                _logService.Logger.Error(ex.ToString());
                _systemStatusManager.NotifyError(ex);
                
            }
        }

        private async Task InitializePlayerInfo()
        {
            CurrentPlayer = new PlayerInfo();
            var greeting = await _playerIoService.GeneralInput(MessageType.Notice, "Hi, please greet me in your language");
            var (s, _) = await _gptService.Call<string>(t => t.LANGUAGE_ANALYSIS, ("PROMPT", greeting));
            CurrentPlayer.Language = s;
            await _persistedDataAccessor.SavePlayerInfo(CurrentPlayer);
        }

        private async Task UpdateScene()
        {
            try
            {
                _renderService.Render(_worldService.CurrentWorld!, _worldService.CurrentRegion!, _worldService.Current!);
                while (true)
                {
                    await _playerIoService.InteractiveMessage(MessageType.Notice, "Input the command.");
                    var command = _playerIoService.WaitInputForScene(_worldService.Current!);
                    if (command.Type == InteractionType.Move)
                    {
                        await _worldService.MoveTo(command.Target.ToDirection());
                        return;
                    }
                    else if (command.Type == InteractionType.Talk)
                    {
                        var charInfo  = this._worldService.Current!.Characters.FirstOrDefault(t => t.FullName == command.Target);
                        if (charInfo != null)
                        {
                            await _playerIoService.TalkToCharactor(charInfo);
                        }
                        else
                        {
                            await _playerIoService.TalkToThing(command.Target);
                        }
                        return;
                    }
                    else if(command.Type == InteractionType.OtherActions)
                    {
                        await PerformAction(command);
                        return;
                    }
                    else if (command.Type == InteractionType.SystemAction)
                    {
                        await ExecuteSystemCommand(command);
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                _logService.Logger.Error(ex.ToString());
                _systemStatusManager.NotifyError(ex);
            }
        }


        public async Task<TalkToThingRespond?> TalkToThing(string target, string content)
        {
            var pmtInfo = $"[World] {_worldService.CurrentWorld!.Description}\n[You] {target}[Q] {content}\n[A] ";

            var (response, _) = await _gptService.Call<TalkToThingRespond>(
                (pmt) => pmt.TALK_TO_THING,
                ("{CHAT_SESSION}", pmtInfo));

            if (response != null)
            {
                _playerIoService.ShowWorldMessage(response.Answer);

                if (response.CharactorInfo != null)
                {
                    _worldService.ReduceThing(target);
                    _worldService.Current!.Characters.Add(response.CharactorInfo);
                    await _worldService.SaveCurrentWorld();

                    var chatHistory = new ChatHistory(response.CharactorInfo.Id);
                    chatHistory.Add(content, response.Answer, $"[A]{response.Answer}");
                    await _persistedDataAccessor.SaveChatHistory(chatHistory);
                }
                else
                {
                    if (response.ActionType?.ToLower() == "disappear")
                    {
                        _worldService.ReduceThing(target);
                    }
                }
            }
            return response;
        }
        public async Task<string> TalkToCharactor(CharacterInfo targetChar, string text)
        {
            var history = _persistedDataAccessor!.GetChatHistory(targetChar.Id);
            var session = string.Join("\n", history.History.Select(t => $"Q:{t.Question}\nA:{t.OriginalAnswer}"));
            session += $"\nFriendnessLevel:{targetChar.FriendnessLevel}\nQ:{text}\nA:";

            var (response, origMsg) = await _gptService.Call<TalkToCharRespond>(
                (pmt) => pmt.TALK_TO_CHARACTOR,
                ("{CHAT_SESSION}", session),
                ("{CHARACTER}", targetChar.Personality!),
                ("{APPEARANCE}", targetChar.Appearance!));

            if (response.FriendlinessChange != 0)
            {
                targetChar.FriendnessLevel += response.FriendlinessChange;
                targetChar.FriendnessLevel = Math.Min(10, Math.Max(0, targetChar.FriendnessLevel));
                await _worldService.SaveCurrentWorld();
            }

            history.Add(text, response.Answer, origMsg);
            await _persistedDataAccessor.SaveChatHistory(history);
            return response.Answer;

        }
 

        private async Task PerformAction(InteractionInfo command)
        {
            try
            {
                string charInfoStr;
                var charInfo = _worldService.Current!.Characters.FirstOrDefault(t => t.FullName == command.Target);
                Something? thing = null;
                if (charInfo != null)
                {
                    charInfoStr = $"Name:{charInfo.FullName}\nTitle:{charInfo.Title}\nPersonality:{charInfo.Personality}\nSex:{charInfo.Sex}\n"
                        + $"Age:{charInfo.Age}\nStatus:{charInfo.Status.ToString()}\nApperience:{charInfo.Appearance}\nFriendlinessLevel:{charInfo.FriendnessLevel}";
                }
                else
                {
                    thing = _worldService.Current!.Things.FirstOrDefault(t => t.Name == command.Target);
                    if (thing == null)
                    {
                        return;
                    }
                    charInfoStr = $"Species:{thing.Name}\nLocation:{_worldService.CurrentRegion!.Description}\nWorld:{_worldService.CurrentWorld!.Description}:nFriendlinessLevel:{5}";
                }

                await _playerIoService.InteractiveMessage(MessageType.Normal, $"You perform \"{command.Detail}\" on {command.Target}");
                var (respond, _) = await _gptService.Call<ActionRespond>((pmt) => pmt.PERFORM_ACTION_ON,
                    ("{CHAR_INFO}", charInfoStr),
                    ("{ACTION}", command.Detail!));

                _playerIoService.ShowWorldMessage($"{respond.Reaction}");
                await ProcessActionRespond(respond, charInfo, thing);
            }
            catch (Exception ex)
            {
                _logService.Logger.Error(ex.ToString());
                _systemStatusManager.NotifyError(ex);
            }
        }

        private async Task ProcessActionRespond(ActionRespond respond, CharacterInfo? charInfo, Something? thing)
        {
            foreach(var ah in _serviceProvider.GetServices<IActionRespondHandler>())
            {
                await ah.ProcessRespond(respond, charInfo, thing);
            }
        }

        private async Task ExecuteSystemCommand(InteractionInfo command)
        {
            if(command.Target.ToLower() == "reset_all")
            {
                await Reset();
            }
            else
            {
                await _playerIoService.InteractiveMessage(MessageType.Notice, "Invalid system command:" +  command.Target);
            }
        }

        private async Task Reset()
        {
            await _persistedDataAccessor.ClearAllGameData();
            await _worldService.Reset();            
            CurrentPlayer = null!;
            await this.Start();
        }
    }
}
