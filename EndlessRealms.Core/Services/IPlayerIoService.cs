using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services;

public enum MessageType
{
    Title,
    Normal,
    Highlight,
    Notice,
    Warning,
    Hint
}

public enum InteractionType
{
    Talk,
    Move,
    OtherActions,
    SystemAction
}
public class InteractionInfo
{
    public InteractionType Type { get; private set; }

    public string Target { get; private set; }
    public string? Detail { get; private set; }

    public InteractionInfo(InteractionType type, string target, string? detail)
    {
        Type = type;
        Target = target;
        Detail = detail;
    }
}

public interface IPlayerIoServiceX
{
    Task<string> GeneralInput(MessageType messageType, string prompt);
    Task InteractiveMessage(MessageType messageType, string prompt);

    Task<bool> GeneralConfirm(MessageType messageType, string prompt);
    InteractionInfo WaitInputForScene(Scene scene);
    Task TalkToThing(string target);
    Task TalkToCharactor(CharacterInfo charInfo);
    void ShowWorldMessage(string message);    
}
