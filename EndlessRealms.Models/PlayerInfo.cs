using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Models;
public class PlayerInfo
{
    public string Language { get; set; } = null!;
    public string CurrentWorldId { get; set; } = null!;
    public string CurrentSceneId { get; set; } = null!;

    public int Hp { get; set; }
}
