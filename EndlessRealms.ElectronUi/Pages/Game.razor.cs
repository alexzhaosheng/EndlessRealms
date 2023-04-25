using EndlessRealms.Core.Services;
using EndlessRealms.Core.Utility;
using EndlessRealms.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;

namespace EndlessRealms.ElectronUi.Pages
{
    [Service]
    public class GameModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly PlayerIoManager.Handler _ioHandler;

        public GameModel(PlayerIoManager.Handler ioHandler)
        {
            _ioHandler = ioHandler;

            _ioHandler.OutputMessage += _ioHandler_OutputMessage;
        }

        public Queue<(OutputType, string)> Messages { get; } = new Queue<(OutputType, string)>();


        private void _ioHandler_OutputMessage(object? sender, (OutputType, string) e)
        {
            Messages.Enqueue(e);

            if (Messages.Count > 10)
            {
                Messages.Dequeue();
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Messages)));
        }

        private Scene? _scene;
        public Scene? Scene
        {
            get => _scene;
            set
            {
                _scene = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Scene)));
            }
        }

        private World? _world;
        public World? World
        {
            get => _world;
            set
            {
                _world = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(World)));
            }
        }
    }
}
