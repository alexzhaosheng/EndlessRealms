using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Models;

public enum CharacterStatus
{
    Normal,
    Attack,
    Follow
}
public class CharacterInfo
{
    public string Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? Race { get; set; }
    public string? Title { get; set; } = null!;
    public string? Sex { get; set; } = null!;
    public string? Age { get; set; } = null!;
    public string? Appearance { get; set; } = null!;
    public string? Personality { get; set; } = null!;
    public CharacterStatus Status { get; set; } = CharacterStatus.Normal;
    public int FriendnessLevel { get; set; } = 5;

    public CharacterInfo()
    {
        Id = Guid.NewGuid().ToString();
    }
}
