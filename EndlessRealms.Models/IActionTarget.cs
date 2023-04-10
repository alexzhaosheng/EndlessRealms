using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Models;
public interface IActionTarget
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }

    public int FriendnessLevel { get; }

    public string GetFullInfo();
}
