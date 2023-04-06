using EndlessRealms.Core.Services;
using EndlessRealms.Core.Services.ActionResponseHandler;
using EndlessRealms.Core.Services.ChatGptDto;
using EndlessRealms.Core.Utility;
using EndlessRealms.Core.Utility.Extensions;
using EndlessRealms.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly IRenderService _renderService;        
        private readonly WorldService _worldService;
        private readonly PlayerIoManager _playerIoMgr;
        private readonly ILogService _logService;
        private readonly ChatGPTService _gptService;
        private readonly IPersistedDataProvider _persistedDataAccessor;
        private readonly SystemStatusManager _systemStatusManager;
        private readonly GameContext _gameContext;
        

        public Game(IRenderService renderService, WorldService worldService, PlayerIoManager playerIoMgr, ILogService logService, ChatGPTService chatGPTService, IPersistedDataProvider persistedDataAccessor, IServiceProvider serviceProvider, SystemStatusManager systemStatusManager, GameContext gameContext)
        {
            _renderService = renderService;
            _worldService = worldService;
            _playerIoMgr = playerIoMgr;
            _logService = logService;
            _gptService = chatGPTService;
            _persistedDataAccessor = persistedDataAccessor;
            _serviceProvider = serviceProvider;
            _systemStatusManager = systemStatusManager;
            _gameContext = gameContext;
        }

        public event EventHandler? QuitGame;
        private bool _gameIsRuning;
        public async Task Start()
        {
            try
            {
                _gameIsRuning = true;

                _ = ListenToSystemCommand();

                _gameContext.CurrentPlayerInfo = (await _persistedDataAccessor.LoadPlayerInfo())!;
                if(_gameContext.CurrentPlayerInfo == null)
                {
                    await InitializePlayerInfo();
                }

                await _worldService.Initialize();
                
                await GameCircle();

            }
            catch (Exception ex)
            {
                _logService.Logger.Error(ex.ToString());
                _systemStatusManager.NotifyError(ex);
                
            }
        }

        public Task Stop()
        {
            _playerIoMgr.ClearAllInputListener();
            _gameIsRuning = false;
            return Task.CompletedTask;
        }

        private async Task GameCircle()
        {
            while (_gameIsRuning)
            {
                try
                {
                    _renderService.Render(_worldService.CurrentWorld!, _worldService.CurrentRegion!, _worldService.Current!);

                    var directionStr = await _playerIoMgr.Input(InputType.DirectionInput, "Where do you want to go?", (cmd) =>
                    {
                        var d = cmd.ToDirection();
                        if (d.HasValue && _worldService.Current!.ConnectedScenes.ContainsKey(d.Value))
                        {
                            return (true, null);
                        }
                        return (false, "You can't go that way.");
                    });

                    await _worldService.MoveTo(directionStr!.ToDirection().GetValueOrDefault());
                }
                catch (Exception ex)
                {
                    _logService.Logger.Error(ex.ToString());
                    _systemStatusManager.NotifyError(ex);
                }
            }
        }

        private async Task ListenToSystemCommand()
        {
            var commands = new(string, Func<Task>)[] 
            {
                ("reset_all", () => Reset()),
                ("quit", ()=> 
                {
                    QuitGame?.Invoke(this, EventArgs.Empty);
                    return Task.CompletedTask;
                })
            };

            while (_gameIsRuning)
            {
                var cmd = await _playerIoMgr.Input(InputType.SystemCommand,
                    "Input system command",
                    (value) =>
                    {
                        if (commands.Any(c => c.Item1 == value.Trim()))
                        {
                            return (true, null);
                        }
                        else
                        {
                            return (false, "Invalid system command");
                        }
                    });

                var (c, action) = commands.FirstOrDefault(t => t.Item1 == cmd);
                if(action != null)
                {
                    await action();
                }

                await Task.Delay(10);
            }
        }

        private async Task InitializePlayerInfo()
        {
            _gameContext.CurrentPlayerInfo = new PlayerInfo();
            string? greeting;
            do 
            {
                greeting = await _playerIoMgr.Input(
                    InputType.GeneralInput,
                    "Hello, please say a sentence in your language to me, and from then on, I will generate the world in your language. Please don't make it too short to avoid me mistaking your language",
                    (value) => (!string.IsNullOrEmpty(value), null));
            }
            while(greeting == null);

            var (s, _) = await _gptService.Call<string>(t => t.LANGUAGE_ANALYSIS, ("PROMPT", greeting!));
            _gameContext.CurrentPlayerInfo.Language = s;
            await _persistedDataAccessor.SavePlayerInfo(_gameContext.CurrentPlayerInfo);
        }
        

        //public async Task<TalkToThingRespond?> TalkToThing(string target, string content)
        //{
        //    var pmtInfo = $"[World] {_worldService.CurrentWorld!.Description}\n[You] {target}[Q] {content}\n[A] ";

        //    var (response, _) = await _gptService.Call<TalkToThingRespond>(
        //        (pmt) => pmt.TALK_TO_THING,
        //        ("{CHAT_SESSION}", pmtInfo),
        //        ("{PLAYER_LANGUAGE}", _gameContext.CurrentPlayerInfo.Language));

        //    if (response != null)
        //    {
        //        _playerIoMgr.ShowWorldMessage(response.Answer);

        //        if (response.CharactorInfo != null)
        //        {
        //            _worldService.ReduceThing(target);
        //            _worldService.Current!.Characters.Add(response.CharactorInfo);
        //            await _worldService.SaveCurrentWorld();

        //            var chatHistory = new ChatHistory(response.CharactorInfo.Id);
        //            chatHistory.Add(content, response.Answer, $"[A]{response.Answer}");
        //            await _persistedDataAccessor.SaveChatHistory(chatHistory);
        //        }
        //        else
        //        {
        //            if (response.ActionType?.ToLower() == "disappear")
        //            {
        //                _worldService.ReduceThing(target);
        //            }
        //        }
        //    }
        //    return response;
        //}
        //public async Task<string> TalkToCharactor(CharacterInfo targetChar, string text)
        //{
        //    var history = _persistedDataAccessor!.GetChatHistory(targetChar.Id);
        //    var session = string.Join("\n", history.History.Select(t => $"Q:{t.Question}\nA:{t.OriginalAnswer}"));
        //    session += $"\nFriendnessLevel:{targetChar.FriendnessLevel}\nQ:{text}\nA:";

        //    var (response, origMsg) = await _gptService.Call<TalkToCharRespond>(
        //        (pmt) => pmt.TALK_TO_CHARACTOR,
        //        ("{CHAT_SESSION}", session),
        //        ("{PLAYER_LANGUAGE}", _gameContext.CurrentPlayerInfo.Language),
        //        ("{CHARACTER}", targetChar.Personality!),
        //        ("{APPEARANCE}", targetChar.Appearance!));

        //    if (response.FriendlinessChange != 0)
        //    {
        //        targetChar.FriendnessLevel += response.FriendlinessChange;
        //        targetChar.FriendnessLevel = Math.Min(10, Math.Max(0, targetChar.FriendnessLevel));
        //        await _worldService.SaveCurrentWorld();
        //    }

        //    history.Add(text, response.Answer, origMsg);
        //    await _persistedDataAccessor.SaveChatHistory(history);
        //    return response.Answer;

        //}
 

        //private async Task PerformAction(InteractionInfo command)
        //{
        //    try
        //    {
        //        string charInfoStr;
        //        var charInfo = _worldService.Current!.Characters.FirstOrDefault(t => t.FullName == command.Target);
        //        Something? thing = null;
        //        if (charInfo != null)
        //        {
        //            charInfoStr = $"Name:{charInfo.FullName}\nTitle:{charInfo.Title}\nPersonality:{charInfo.Personality}\nSex:{charInfo.Sex}\n"
        //                + $"Age:{charInfo.Age}\nStatus:{charInfo.Status.ToString()}\nApperience:{charInfo.Appearance}\nFriendlinessLevel:{charInfo.FriendnessLevel}";
        //        }
        //        else
        //        {
        //            thing = _worldService.Current!.Things.FirstOrDefault(t => t.Name == command.Target);
        //            if (thing == null)
        //            {
        //                return;
        //            }
        //            charInfoStr = $"Species:{thing.Name}\nLocation:{_worldService.CurrentRegion!.Description}\nWorld:{_worldService.CurrentWorld!.Description}:nFriendlinessLevel:{5}";
        //        }

        //        await _playerIoMgr.InteractiveMessage(MessageType.Normal, $"You perform \"{command.Detail}\" on {command.Target}");
        //        var (respond, _) = await _gptService.Call<ActionRespond>((pmt) => pmt.PERFORM_ACTION_ON,
        //            ("{PLAYER_LANGUAGE}", _gameContext.CurrentPlayerInfo.Language),
        //            ("{CHAR_INFO}", charInfoStr),
        //            ("{ACTION}", command.Detail!));

        //        _playerIoMgr.ShowWorldMessage($"{respond.Reaction}");
        //        await ProcessActionRespond(respond, charInfo, thing);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logService.Logger.Error(ex.ToString());
        //        _systemStatusManager.NotifyError(ex);
        //    }
        //}

        private async Task ProcessActionRespond(ActionRespond respond, CharacterInfo? charInfo, Something? thing)
        {
            foreach(var ah in _serviceProvider.GetServices<IActionRespondHandler>())
            {
                await ah.ProcessRespond(respond, charInfo, thing);
            }
        }


        private async Task Reset()
        {
            await _persistedDataAccessor.ClearAllGameData();
            await _worldService.Reset();
            _gameContext.CurrentPlayerInfo = null!;
            await this.Start();
        }
    }
}
