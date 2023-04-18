using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Models;
public class Something: IActionTarget
{
    public string Id { get; set; }
    public string Name { get; set; } = null!;
    public TargetType Type => TargetType.Thing;
    public string Description { get; set; } = null!;
    public bool IsIanimate { get; set; }

    public int Number { get; set; }

    int IActionTarget.FriendnessLevel => Const.NeutralFriendnessLevel;

    public TargetStatus Status { get; set; } = TargetStatus.Normal;

    public Something() 
    {
        Id = Guid.NewGuid().ToString();
    }

    string IActionTarget.GetFullInfo()
    {
        return $"Name:{Name}\nDescription:\n{Description}\nIsIanimate:{IsIanimate}";
    }
}
