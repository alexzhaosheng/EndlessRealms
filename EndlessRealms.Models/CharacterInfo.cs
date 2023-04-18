using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Models;

public class CharacterInfo: IActionTarget
{
    public string Id { get; set; }
    
    public string FullName { get; set; } = null!;
    public string Race { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Sex { get; set; } = null!;
    public string Age { get; set; } = null!;
    public string Appearance { get; set; } = null!;
    public string Personality { get; set; } = null!;
    public TargetStatus Status { get; set; } = TargetStatus.Normal;
    public int FriendnessLevel { get; set; } = 5;

    string IActionTarget.Name => FullName;

    string IActionTarget.Description => Appearance;

    public TargetType Type => TargetType.Charactor;

    public CharacterInfo()
    {
        Id = Guid.NewGuid().ToString();
    }

    string IActionTarget.GetFullInfo()
    {
        return $"Name:{FullName}\nAppearance:{Appearance}\nPersonality:{Personality}";
    }
}
