using EndlessRealms.Core.Services;
using EndlessRealms.Core.Utility;
using EndlessRealms.ElectronUi.Pages;
using EndlessRealms.Models;

namespace EndlessRealms.ElectronUi.Services;

[Service(typeof(IRenderService))]
public class RenderService : IRenderService
{
    private readonly GameModel _gameModel;
    public RenderService(GameModel gameModel)
    {
        _gameModel = gameModel;
    }
    public void Render(World world, Region region, Scene scene)
    {
        _gameModel.World = world;
        _gameModel.Scene = scene;
    }

    public void Render(World newWorld)
    {
        
    }
}
