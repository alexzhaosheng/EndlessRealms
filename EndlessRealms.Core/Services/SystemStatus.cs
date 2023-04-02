using EndlessRealms.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services;
public enum SystemStatus
{
    Normal,
    Working
}

[Service]
public class SystemStatusManager
{
    private readonly Stack<(SystemStatus, string)> _statusStack = new Stack<(SystemStatus, string)>();


    public event EventHandler<StatusChangedEventArg>? StatusChanged;

    public SystemStatus CurrentStatus => _statusStack.Count > 0? _statusStack.Peek().Item1: SystemStatus.Normal;
    public string CurrentStatusMessage => string.Join(" => ", _statusStack.Reverse().Select(s => s.Item2));

    public void PushStatus(SystemStatus status, string message)
    {
        var oldStatus = CurrentStatus;
        _statusStack.Push((status, message));
        StatusChanged?.Invoke(this, new StatusChangedEventArg(CurrentStatus, oldStatus));
    }
    public void PopStatus()
    {
        var oldStatus = CurrentStatus;
        _statusStack.Pop();
        StatusChanged?.Invoke(this, new StatusChangedEventArg(CurrentStatus, oldStatus));
    }

    public async Task ExecWithStatus(SystemStatus status, string message, Func<Task> action)
    {
        try
        {
            PushStatus(status, message);
            await action();
        }
        finally
        {
            PopStatus();
        }
    }

    public event EventHandler<Exception>? ErrorHappened;
    public void NotifyError(Exception error)
    {
        ErrorHappened?.Invoke(this, error);
    }
}

public class StatusChangedEventArg
{
    public SystemStatus TheOld { get; }
    public SystemStatus TheCurrent { get; }

    public StatusChangedEventArg(SystemStatus theNew, SystemStatus theOld)
    {
        TheCurrent = theNew;
        TheOld = theOld;
    }
}

