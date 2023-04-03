using Avalonia.Controls;
using EndlessRealms.Core.Services;
using EndlessRealms.Core.Utility;
using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace EndlessRealms.Gui.Services;
[Service(typeof(IPlayerIoService))]
internal class TextPlayerIOService : IPlayerIoService
{
    public Action<string>? _currentHandler = null;

    private readonly UiModel _uiModel;

    public TextPlayerIOService(UiModel uiModel)
    {
        _uiModel = uiModel;
        
    }

    

    public void InputText(string input)
    {
        _currentHandler?.Invoke(input);
    }
    public Task<bool> GeneralConfirm(MessageType messageType, string prompt)
    {
        _uiModel.InteractiveMessage = $"{prompt}\nInput y for yes, n for no";
        string? input = null;
        _currentHandler = (s) => input = s;

        while (true)
        {            
            Task.Delay(TimeSpan.FromSeconds(50));

            if(!string.IsNullOrWhiteSpace(input))
            {
                if (input?.ToLower() == "y" || input?.ToLower() == "yes")
                {
                    return Task.FromResult(true);
                }
                else if (input?.ToLower() == "no" || input?.ToLower() == "n")
                {
                    return Task.FromResult(false);
                }
                input = null;
            }          
        }
    }

    public Task<string> GeneralInput(MessageType messageType, string prompt)
    {
        string? input = null;
        _currentHandler = (s) => input = s;
        _uiModel.InteractiveMessage = prompt;

        while (string.IsNullOrWhiteSpace(input))
        {
            Task.Delay(50);
        }

        return Task.FromResult(input);
    }

    public Task InteractiveMessage(MessageType messageType, string message)
    {
        _uiModel.InteractiveMessage = message; 
        return Task.CompletedTask;
    }
 

    public InteractionInfo WaitInputForScene(Scene scene)
    {
        string? input = null;
        _currentHandler = (s) => input = s;

        while (true)
        {
            Task.Delay(10);
            
            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input.StartsWith("?"))
            {
                ShowHelp();
            }
            else if (input.StartsWith('>') && input.Length > 1)
            {
                var direction = FromChar(input.ToUpper().Replace(" ", "")[1]);
                if (direction != null)
                {
                    if (scene.ConnectedScenes.ContainsKey(direction.Value))
                    {
                        return new InteractionInfo(InteractionType.Move, direction.ToString()!, null);
                    }
                    else
                    {
                        InteractiveMessage(MessageType.Notice, "You can't go that direcion");
                    }
                }
            }
            else if (input.StartsWith('@'))
            {
                var target = input.Substring(1).Trim();
                if (target.EndsWith(":"))
                {
                    target = target.Substring(0, target.Length - 1);
                }
                if (target != null)
                {
                    return new InteractionInfo(InteractionType.Talk, target, null);
                }
            }
            else if (input.StartsWith("!"))
            {
                var (target, message) = ParseInput(scene, input, "What do you want to do to {0}?");
                if (target != null)
                {
                    return new InteractionInfo(InteractionType.OtherActions, target, message);
                }
            }
            else if (input.StartsWith("&") && input.Length > 0)
            {
                var cmd = input.Substring(1).Trim();
                return new InteractionInfo(InteractionType.SystemAction, cmd, null);
            }
            else
            {
                InteractiveMessage(MessageType.Notice, "Invalid input, User ? to get help.");
            }
            input = null;
        }
    }

    private (string? target, string? detail) ParseInput(Scene scene, string fullInput, string missingDetailMessage)
    {
        var input = fullInput.Substring(1);
        if (input.IndexOf(":") < 0)
        {
            InteractiveMessage(MessageType.Notice, "Invalid command. User ? to get help.");
        }
        else
        {
            var target = input.Substring(0, input.IndexOf(":")).Trim();

            var name = scene.Characters.FirstOrDefault(t => t.FullName.ToLower() == target.ToLower())?.FullName
                ?? scene.Things.FirstOrDefault(t => t.Name.ToLower() == target.ToLower())?.Name;
            if (name == null)
            {
                InteractiveMessage(MessageType.Notice, "Invalid target. User ? to get help.");
            }
            else
            {
                var message = input.Substring(input.IndexOf(":") + 1).Trim();
                if (string.IsNullOrWhiteSpace(message))
                {
                    InteractiveMessage(MessageType.Notice, string.Format(missingDetailMessage, name));
                }
                else
                {
                    return (name, message);
                }
            }
        }

        return (null, null);
    }

    public static Direction? FromChar(char c)
    {
        foreach (var e in Enum.GetValues(typeof(Direction)))
        {
            if (e.ToString()!.StartsWith(c))
            {
                return (Direction)e;
            }
        }
        return null;
    }

    private void ShowHelp()
    {
        InteractiveMessage(MessageType.Hint, @"-------------
?: this help"");
>[direction], move to the direction. >e to east, >w to west, >s to sourth, >n to north
@[target]:[content], talk with this target. ex: '@Tom:Hello', say 'Hello' to Tom
![target]:[action], do something to target. ex:'@cat:catch', catch the cat
-------------");

    }

    public Func<string, Task>? TalkToThingCallback { get; set; }
    public async Task TalkToThing(string target)
    {
        if (TalkToThingCallback != null)
        {
            await TalkToThingCallback(target);
        }        
    }

    public Func<CharacterInfo, Task>? TalkToCharCallback { get; set; }    
    public async Task TalkToCharactor(CharacterInfo charInfo)
    {
        if (TalkToCharCallback != null)
        {
            await TalkToCharCallback(charInfo);
        }
    }

    private Queue<string> _worldMessages = new Queue<string>();
    public void ShowWorldMessage(string message)
    {
        _worldMessages.Enqueue(message);
        if(_worldMessages.Count > 5)
        {
            _worldMessages.Dequeue();
        }
        _uiModel.WorldMessage = string.Join('\n', _worldMessages);
    }

    
}
