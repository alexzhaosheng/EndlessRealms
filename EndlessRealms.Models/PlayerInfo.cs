using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Models;
public class PlayerInfo
{
    public string Language { get; set; } = null!;
    public string CurrentWorldId { get; set; }
    public string CurrentSceneId { get; set; }

    public int Hp { get; set; }
}
