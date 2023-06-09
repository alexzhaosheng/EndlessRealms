﻿using EndlessRealms.Core.Services.ChatGptDto;
using EndlessRealms.Core.Utility;
using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services.ActionResponseHandler;
[Service(typeof(IActionRespondHandler))]
public class DisappearHandler : IActionRespondHandler
{
    private readonly WorldService _worldService;

    public DisappearHandler(WorldService worldService)
    {
        _worldService = worldService;
    }
    
    public Task ProcessRespond(ActionRespond actionRespond, IActionTarget target)
    {
        if (!string.IsNullOrEmpty(actionRespond.NewStatus) && actionRespond.NewStatus.Equals("Disappear", StringComparison.OrdinalIgnoreCase))
        {
            _worldService.Remove(target);
        }
        return Task.CompletedTask;
    }
}
