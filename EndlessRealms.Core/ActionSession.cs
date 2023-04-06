using EndlessRealms.Core.Services;
using EndlessRealms.Core.Services.ChatGptDto;
using EndlessRealms.Core.Utility;
using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EndlessRealms.Core;

[Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient)]
public class ActionSession
{
    private CharacterInfo _character = null!;

    public ActionHistory ActionHistory { get; private set; } = null!;

    private readonly IPersistedDataProvider _persistedDataProvider;
    private readonly ChatGPTService _chatGPTService;
    private readonly GameContext _gameContext;
    public ActionSession(IPersistedDataProvider persistedDataProvider, ChatGPTService chatGPTService, GameContext gameContext)
    {
        _persistedDataProvider = persistedDataProvider;
        _chatGPTService = chatGPTService;
        _gameContext = gameContext;
    }

    public async Task Initialize(CharacterInfo targetChar)
    {
        _character = targetChar;

        ActionHistory = await _persistedDataProvider.GetActionHistory(_character.Id);
    }

    public Task<string> Talk(string text)
    {
        return Perform($"Talk to you:{text}");
    }

    public async Task<string> Perform(string action)
    {        
        var session = string.Join("\n", ActionHistory.History.Select(t => $"A:{t.Action}\nR:{t.Response}"));
        session += $"\nFriendnessLevel:{_character.FriendnessLevel}\nA:{action}\nR:";
        var charInfo = $"Name:{_character.FullName}\nAppearance:{_character.Appearance}\nPersonality:{_character.Personality}";
        var (response, origMsg) = await _chatGPTService.Call<TalkToCharRespond>(
                t => t.TALK_TO_CHARACTOR,
                ("{ACTION_SESSION}", session),
                ("{PLAYER_LANGUAGE}", _gameContext.CurrentPlayerInfo.Language),
                ("{CHARACTER}", charInfo));

        ActionHistory.Add(action, response.Reaction, origMsg);
        await _persistedDataProvider.SaveActionHistory(ActionHistory);
        return response.Reaction;
    }
}
