using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services.ChatGptDto;
public class TalkToThingRespond
{
    public string Answer { get; set; } = null!;    
    public string ActionType { get; set; } = null!;
    public CharacterInfo? CharactorInfo { get; set; }
}
