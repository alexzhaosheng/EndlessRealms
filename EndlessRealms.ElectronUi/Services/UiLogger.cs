using EndlessRealms.Core.Services;
using Serilog;
namespace EndlessRealms.ElectronUi.Services;

public class UILogger : ILogService
{
    public Serilog.ILogger Logger { get; private set; }

    public UILogger(Serilog.ILogger logger)
    {
        Logger = logger;
    }
}
