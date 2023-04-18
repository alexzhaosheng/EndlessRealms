using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using EndlessRealms.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Gui;
public class UiModel: ReactiveObject
{
    public class Line 
    {
        public string Description { get; set; }
        public TextStyle? Style { get; } = null;
        public Line(TextStyle style, string text)
        {
            Description = text;
            Style = style;            
        }        
    }

    private string _log = null!;
    public string Log { get => _log; set => this.RaiseAndSetIfChanged(ref _log, value); }

    public ObservableCollection<Line> SceneWindowInfo { get; } = new ObservableCollection<Line>();

    public void WriteToScene(TextStyle style, string text)
    {
        SceneWindowInfo.Add(new Line(style, text));
    }

    public void WriteSpaceToScene()
    {
        SceneWindowInfo.Add(new Line(TextStyle.SpaceStyle, " ")); 
    }    

    private Scene? _scene = null;
    public Scene? Scene { get => _scene; set => this.RaiseAndSetIfChanged(ref _scene, value); }

    private World? _world = null;
    public World? World { get => _world; set => this.RaiseAndSetIfChanged(ref _world, value); }

    private string _historyInteractiveMessage = null!;
    public string HistoryInteractiveMessage
    {
        get => _historyInteractiveMessage;
        set => this.RaiseAndSetIfChanged(ref _historyInteractiveMessage, value);
    }

    private string _interactiveMessage = null!;
    public string InteractiveMessage
    {
        get => _interactiveMessage;
        set
        {
            RecordHistoryInteractiveMessage();
            this.RaiseAndSetIfChanged(ref _interactiveMessage, value);
        }
    }

    private Queue<string> _interactiveHistory  = new Queue<string>();
    private void RecordHistoryInteractiveMessage()
    {
        if(!string.IsNullOrWhiteSpace(_interactiveMessage))
        {
            _interactiveHistory.Enqueue(_interactiveMessage);
            if(_interactiveHistory.Count > 5)
            {
                _interactiveHistory.Dequeue();
            }
            HistoryInteractiveMessage = string.Join('\n', _interactiveHistory);
        }
    }

    private Queue<string> _worldHistory = new Queue<string>();
    private void RecordWorldMessage()
    {
        if (!string.IsNullOrWhiteSpace(_worldMessage))
        {
            _worldHistory.Enqueue(_worldMessage);
            if (_worldHistory.Count > 5)
            {
                _worldHistory.Dequeue();
            }
            HistoryWorldMessage = string.Join('\n', _worldHistory);
        }
    }

    private string _worldMessage = null!;
    public string WorldMessage
    {
        get => _worldMessage;
       
        set 
        {
            RecordWorldMessage();
            this.RaiseAndSetIfChanged(ref _worldMessage, value);
        }
    }

    private string _historyWorldMessage = null!;
    public string HistoryWorldMessage
    {
        get => _historyWorldMessage;
        set => this.RaiseAndSetIfChanged(ref _historyWorldMessage, value);
    }


    private string _currentSystemStatus = null!;
    public string? CurrentSystemStatus { get => _currentSystemStatus; set => this.RaiseAndSetIfChanged(ref _currentSystemStatus!, value); }


}



public class TextStyle
{
    public static readonly TextStyle TitleStyle = new TextStyle() { FontSize = 22, Padding = new Thickness(5), ForegroundBrush = Brushes.LightCoral };
    public static readonly TextStyle ContentStyle = new TextStyle() { FontSize = 18, Padding = new Thickness(3), ForegroundBrush = Brushes.White };
    public static readonly TextStyle NoticeStyle = new TextStyle() { FontSize = 18, Padding = new Thickness(3), ForegroundBrush = Brushes.Yellow };
    public static readonly TextStyle SpaceStyle = new TextStyle() { Padding = new Thickness(10), FontSize = 18, ForegroundBrush = Brushes.DarkGray };


    public int FontSize { get; set; }
    public ISolidColorBrush? ForegroundBrush { get; set; }
    public ISolidColorBrush? BackgroundBrush { get; set; }

    public Thickness? Padding { get; set; }
    public HorizontalAlignment? HorizontalAlignment { get; set; }
}

public class DirectionInfo
{
    public Direction? Direction  { get; set; }
    public string Description { get; set; } = null!;
    
}

