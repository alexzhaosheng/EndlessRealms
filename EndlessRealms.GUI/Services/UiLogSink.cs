using Serilog.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Gui.Services;
public class UiLogSink : ILogEventSink
{
    private readonly IFormatProvider _formatProvider;
    private readonly UiModel _uiModel;
    public UiLogSink(IFormatProvider formatProvider, UiModel uiModel)
    {
        _formatProvider = formatProvider;
        _uiModel = uiModel;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(_formatProvider);
        _uiModel.Log = message + "\n=========================n\n" + _uiModel.Log;
    }
}

public static class UiLogSinkExtensions
{
    public static LoggerConfiguration UiLogSink(
              this LoggerSinkConfiguration loggerConfiguration,
              UiModel uiModel,
              IFormatProvider? formatProvider = null)
    {
        return loggerConfiguration.Sink(new UiLogSink(formatProvider!, uiModel));
    }
}
