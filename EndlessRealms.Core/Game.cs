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

        private async Task Reset()
        {
            await _persistedDataAccessor.ClearAllGameData();
            await _worldService.Reset();
            _gameContext.CurrentPlayerInfo = null!;
            await this.Start();
        }
    }
}
