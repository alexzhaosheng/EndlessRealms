using Serilog.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EndlessRealms.ElectronUi.Pages;

namespace EndlessRealms.ElectronUi.Services;
public class UiLogSink : ILogEventSink
{
    private readonly IFormatProvider _formatProvider;
    private readonly LogModel _logModel;
    public UiLogSink(IFormatProvider formatProvider, LogModel uiModel)
    {
        _formatProvider = formatProvider;
        _logModel = uiModel;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(_formatProvider);
        _logModel.Log = message + "\n==================================================\n\n" + _logModel.Log;
        Console.WriteLine(message);
        Console.WriteLine();
    }
}

public static class UiLogSinkExtensions
{
    public static LoggerConfiguration UiLogSink(
              this LoggerSinkConfiguration loggerConfiguration,
              LogModel logModel,
              IFormatProvider? formatProvider = null)
    {
        return loggerConfiguration.Sink(new UiLogSink(formatProvider!, logModel));
    }
}
