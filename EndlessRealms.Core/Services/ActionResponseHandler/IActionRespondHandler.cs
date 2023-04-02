using EndlessRealms.Core.Services.ChatGptDto;
using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services.ActionResponseHandler;
public interface IActionRespondHandler
{
    Task ProcessRespond(ActionRespond actionRespond, CharactorInfo? charInfo, Something? thing);
}
