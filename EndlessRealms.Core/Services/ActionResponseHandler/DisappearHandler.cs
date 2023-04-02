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
public class DisappearHandler : IActionRespondHandler
{
    private readonly WorldService _worldService;

    public DisappearHandler(WorldService worldService)
    {
        _worldService = worldService;
    }
    
    public async Task ProcessRespond(ActionRespond actionRespond, CharacterInfo? charInfo, Something? thing)
    {
        if (actionRespond.NewStatus.Equals("Disappear", StringComparison.OrdinalIgnoreCase))
        {
            if (charInfo == null)
            {
                _worldService.ReduceThing(thing!.Name);
            }
            else
            {
                _worldService.Remove(charInfo);
            }

            await _worldService.SaveCurrentWorld();
        }
    }
}
