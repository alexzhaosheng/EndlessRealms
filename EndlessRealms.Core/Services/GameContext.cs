using EndlessRealms.Core.Utility;
using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services;

[Service]
public class GameContext
{
    public PlayerInfo CurrentPlayerInfo { get; private set; } = null!;

    private IPersistedDataProvider _persistedDataProvider;
    private PlayerIoManager _playerIoManager;
    private ChatGPTService _chatGPTService;
    public GameContext(IPersistedDataProvider persistedDataProvider, PlayerIoManager playerIoManager, ChatGPTService chatGPTService)
    {
        _persistedDataProvider = persistedDataProvider;
        _playerIoManager = playerIoManager;
        _chatGPTService = chatGPTService;
    }

    public async Task Initialize()
    {
        CurrentPlayerInfo = (await _persistedDataProvider.LoadPlayerInfo())!;
        if(CurrentPlayerInfo == null)
        {
            await InitializePlayerInfo();
        }
    }

    private async Task InitializePlayerInfo()
    {
        CurrentPlayerInfo = new PlayerInfo();
        string? greeting;
        do
        {
            greeting = await _playerIoManager.Input(
                InputType.GeneralInput,
                "Hello, please say a sentence in your language to me, and from then on, I will generate the world in your language. Please don't make it too short to avoid me mistaking your language",
                (value) => (!string.IsNullOrEmpty(value), null));
        }
        while (greeting == null);

        var (s, _) = await _chatGPTService.Call<string>(t => t.LANGUAGE_ANALYSIS, ("PROMPT", greeting!));
        CurrentPlayerInfo.Language = s;
        await _persistedDataProvider.SavePlayerInfo(CurrentPlayerInfo);
    }

    public void ResetData()
    {
        CurrentPlayerInfo = null!;
    }

    public async Task SavePlayerInfo()
    {
        await _persistedDataProvider.SavePlayerInfo(CurrentPlayerInfo);
    }

}
