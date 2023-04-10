using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services.ChatGptDto;
public class TalkToCharRespond
{
    public string Reaction { get; set; } = null!;
    public int FriendlinessChange { get; set; }
    public string NewStatus { get; set; } = null!;
}
