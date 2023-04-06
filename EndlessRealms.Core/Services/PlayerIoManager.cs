using EndlessRealms.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services;

public enum InputType
{
    SystemCommand,
    GeneralInput,
    DirectionInput 
}

public enum OutputType
{
    WorldMessage,
    UiMessage
}

[Service]
public class PlayerIoManager
{
    [Service]
    public class Handler
    {
        private readonly PlayerIoManager _manager;

        public Handler(PlayerIoManager manager)
        {
            _manager = manager;
            _manager._currentHandler = this;
        }

        public event EventHandler<InputType>? InputLisenterChanged;

        internal void NotifyInputLisenterChanged(InputType input)
        {
            InputLisenterChanged?.Invoke(this, input);
        }

        public IEnumerable<InputType> ActiveInputTypes { get => _manager._currentListeners.Keys; }
        public string? GetInputMessage(InputType inputType)
        {
            if (_manager._currentListeners.ContainsKey(inputType))
            {
                return _manager._currentListeners[inputType].Message;
            }
            return null;
        }
        public Func<string, (bool, string?)>? GetInputValidator(InputType inputType)
        {
            if (_manager._currentListeners.ContainsKey(inputType))
            {
                return _manager._currentListeners[inputType].Validator;
            }
            return null;
        }

        public void NotifyInput(InputType inputType, string message)
        {
            if (_manager._currentListeners.ContainsKey(inputType))
            {
                _manager._currentListeners[inputType].Value = message;
            }
        }

        public event EventHandler<(OutputType, string)>? OutputMessage;
        internal void NotifyOutputMessage(OutputType outputType, string message)
        {
            OutputMessage?.Invoke(this, (outputType, message));
        }
    }

    private Handler? _currentHandler;
    private readonly Dictionary<InputType, InputInfo> _currentListeners = new Dictionary<InputType, InputInfo>();

    public PlayerIoManager() 
    {
      
    }

    public async Task<string?> Input(InputType input, string message, Func<string, (bool, string?)> validator)
    {
        if (_currentListeners.ContainsKey(input))
        {
            throw new EndlessRealmsException($"The listener already {input} exists.");
        }

        _currentListeners[input] = new InputInfo(input, message, validator);
        _currentHandler?.NotifyInputLisenterChanged(input);
        while (_currentListeners.ContainsKey(input) 
            && _currentListeners[input].Value == null)
        {
            await Task.Delay(50);
        }
        
        if(_currentListeners.ContainsKey(input))
        {
            var result = _currentListeners[input].Value!;
            _currentListeners.Remove(input);
            return result;
        }
        else
        {
            return null;
        }
    }

    public async Task<bool?> Confirm(string message)
    {
        var r = await Input(InputType.GeneralInput, $"{message}\nInput 'y' for yes and 'n' for no.", (s) => (s == "n" || s == "no" || s == "y" || s == "yes", "Please input 'y' or 'n'"));
        if (r == null)
            return null;
        return r == "y" || r == "yes";
    }

    public void OutputMessage(OutputType outputType, string message)
    {
        _currentHandler?.NotifyOutputMessage(outputType, message);
    }

    internal void ClearAllInputListener()
    {
        _currentListeners.Clear();
    }
}

class InputInfo
{
    public string Message { get; }
    public InputType InputType { get; }
    public Func<string, (bool, string?)> Validator { get; }
    public string? Value { get; set; }
    public InputInfo(InputType inputType, string message, Func<string, (bool, string?)> validator)
    {
        Message = message;
        InputType = inputType;
        Validator = validator;
    }
    
}


