using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Models;

public enum TargetType
{
    Charactor,
    Thing
}

public enum TargetStatus
{
    Normal,
    Attack,
    Following
}
public interface IActionTarget
{
    public string Id { get; }

    public TargetType Type { get; }

    public string Name { get; }
    public string Description { get; }

    public int FriendnessLevel { get; }

    public string GetFullInfo();

    public TargetStatus Status { get; set; }
}
