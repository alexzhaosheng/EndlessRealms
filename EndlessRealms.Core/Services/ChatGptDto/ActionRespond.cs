using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services.ChatGptDto;
public class ActionRespond
{
    public string Reaction { get; set; } = null!;
    public int FriendnessLevelChange { get; set; } = 0;
    public string NewStatus { get; set; } = null!;
    public Something[]? Derivatives { get; set;}
}
