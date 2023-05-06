using EndlessRealms.Core.Services;
using EndlessRealms.Core.Utility;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.IO.Compression;
using EndlessRealms.Models;
using System.Numerics;
using EndlessRealms.Core.Utility.Extensions;
using EndlessRealms.WebUi.Pages;

namespace EndlessRealms.WebUi.Services;

[Service]
public class MapService: INotifyPropertyChanged
{
    private WorldService _worldService;
    private Core.Game _game;

    public int Width { get; private set; }
    public int Height { get; private set; }

    private List<(Scene scene, ScenePosition position)>? _scene;
    public IEnumerable<(Scene Scene, ScenePosition Position, Direction[] Directions)> Scenes
    {
        get
        {
            if(_scene == null)
            {
                UpdateMap();
            }

            if (_scene != null) 
            {
                var sp = new ScenePosition(_scene!.Min(t => t.position.X), _scene!.Min(t => t.position.Y));
                List<string> passed = new List<string>();
                foreach (var s in _scene)
                {
                    passed.Add(s.scene.Id);
                    yield return (
                        s.scene, 
                        s.position - sp, 
                        s.scene.ConnectedScenes
                            .Where(c=> c.Value == null || !passed.Contains(c.Value!))
                            .Select(t=>t.Key)
                            .ToArray());
                }
            }            
        }
    }

    public MapService(WorldService worldService, Core.Game game)
    {
        _worldService = worldService;
        _worldService.SceneAdded += _worldService_SceneAdded;
        _game = game;
        _game.GameStarted += _game_GameStarted;

        UpdateMap();        
    }

    private void _game_GameStarted(object? sender, EventArgs e)
    {
        UpdateMap();
    }

    private void _worldService_SceneAdded(object? sender, EventArgs e)
    {
        UpdateMap();
    }

    private void UpdateMap()
    {
        if (_worldService.CurrentWorld != null)
        {
            _scene = _worldService.CurrentWorld!.GetScenePositions();
        }
        else
        {
            _scene = null;
        }

        if (_scene is { Count: > 0 })
        {
            Width = (_scene!.Max(t => t.position.X) - _scene!.Min(t => t.position.X)) + 1;
            Height = (_scene!.Max(t => t.position.Y) - _scene!.Min(t => t.position.Y)) + 1;
        }
        else
        {
            Width = Height = 0;
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Scenes)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;



}
