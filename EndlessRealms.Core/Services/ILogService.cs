using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services;
public interface ILogService
{
    ILogger Logger { get; }
}
