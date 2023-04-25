using EndlessRealms.Core.Services;
using EndlessRealms.Core.Services.ActionResponseHandler;
using EndlessRealms.Core.Services.ChatGptDto;
using EndlessRealms.Core.Utility;
using EndlessRealms.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EndlessRealms.Core;

[Service(Lifetime = ServiceLifetime.Transient)]
public class ActionSession
{
    private IActionTarget _actionTarget = null!;

    public ActionHistory ActionHistory { get; private set; } = null!;

    private readonly IPersistedDataProvider _persistedDataProvider;
    private readonly ChatGPTService _chatGPTService;
    private readonly GameContext _gameContext;
    private readonly IServiceProvider _serviceProvider;
    public ActionSession(IPersistedDataProvider persistedDataProvider, ChatGPTService chatGPTService, GameContext gameContext, IServiceProvider serviceProvider)
    {
        _persistedDataProvider = persistedDataProvider;
        _chatGPTService = chatGPTService;
        _gameContext = gameContext;
        _serviceProvider = serviceProvider;
    }

    public async Task Initialize(IActionTarget target)
    {
        _actionTarget = target;

        ActionHistory = await _persistedDataProvider.GetActionHistory(_actionTarget.Id);
    }


    public async Task<string> Perform(string action)
    {        
        var session = string.Join("\n", ActionHistory.History.Select(t => $"A:{t.Action}\nR:{t.Response}"));
        var actionReq = action.StartsWith("!")
            ? $"Perform Action:  {action.Substring(1)}"
            : $"Talk to you: {action}";
        session += $"\nFriendnessLevel:{_actionTarget.FriendnessLevel}\nA:{actionReq}\nR:";
        var charInfo = _actionTarget.GetFullInfo();
        var (response, origMsg) = await _chatGPTService.Call<ActionRespond>(
                t => t.PERFORM_ACTION_ON,
                ("{ACTION}", session),
                ("{PLAYER_LANGUAGE}", _gameContext.CurrentPlayerInfo.Language),
                ("{CHAR_INFO}", charInfo));

        ActionHistory.Add(action, response.Reaction, origMsg);
        await _persistedDataProvider.SaveActionHistory(ActionHistory);

        var handlers = _serviceProvider.GetServices<IActionRespondHandler>();
        foreach (var handler in handlers)
        {
            await handler.ProcessRespond(response, _actionTarget);
        }
        
        return response.Reaction;
    }
}
