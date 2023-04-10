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
    private IActionTarget _actionTarget = null!;

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

    public async Task Initialize(IActionTarget target)
    {
        _actionTarget = target;

        ActionHistory = await _persistedDataProvider.GetActionHistory(_actionTarget.Id);
    }

    public Task<string> Talk(string text)
    {
        return Perform($"Talk to you:{text}");
    }

    public async Task<string> Perform(string action)
    {        
        var session = string.Join("\n", ActionHistory.History.Select(t => $"A:{t.Action}\nR:{t.Response}"));
        session += $"\nFriendnessLevel:{_actionTarget.FriendnessLevel}\nA:{action}\nR:";
        var charInfo = _actionTarget.GetFullInfo();
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
