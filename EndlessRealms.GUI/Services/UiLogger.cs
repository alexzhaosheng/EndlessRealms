using EndlessRealms.Core.Services;
using EndlessRealms.Core.Utility;
using EndlessRealms.Gui;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Gui.Services;
public class UILogger : ILogService
{
    public ILogger Logger { get; private set; }

    public UILogger(ILogger logger)
    {
       Logger = logger;
    }
}
