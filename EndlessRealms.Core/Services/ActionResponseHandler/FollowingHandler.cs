using EndlessRealms.Core.Services.ChatGptDto;
using EndlessRealms.Core.Utility;
using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services.ActionResponseHandler;

[Service(typeof(IActionRespondHandler))]
public class FollowingHandler : IActionRespondHandler
{
    private readonly WorldService _worldService;

    public FollowingHandler(WorldService worldService)
    {
        _worldService = worldService;
    }

    public Task ProcessRespond(ActionRespond actionRespond, IActionTarget target)
    {
        if(actionRespond.NewStatus == "Follow")
        {
            target.Status = TargetStatus.Following;
            _worldService.NotifyCurrentWorldChanged();
        }
        else if(actionRespond.NewStatus == "StopFollowing")
        {
            target.Status = TargetStatus.Normal;
            _worldService.NotifyCurrentWorldChanged();
        }

        return Task.CompletedTask;
    }
}
