using EndlessRealms.Core.Utility;
using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services;

[Service]
public class GameContext
{
    public PlayerInfo CurrentPlayerInfo { get; set; } = null!;

}
