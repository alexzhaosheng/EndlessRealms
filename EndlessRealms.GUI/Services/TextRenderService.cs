using EndlessRealms.Models;
using EndlessRealms.Core.Services;
using EndlessRealms.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using EndlessRealms.Gui;

namespace EndlessRealms.Gui.Services
{

    public class TextRenderService : IRenderService
    {
        private readonly UiModel _uiModel;
        public TextRenderService(UiModel uiModel)
        {
            _uiModel = uiModel;
        }

        public void Render(World world, Region region, Scene scene)
        {
            _uiModel.SceneWindowInfo.Clear();
            if (world != null && scene != null)
            {
                _uiModel.WriteToScene(TextStyle.TitleStyle, $"[{world.Name}] {scene.Name}");
                _uiModel.WriteToScene(TextStyle.ContentStyle, scene.Description);
            }

            _uiModel.World = world;
            _uiModel.Scene = null;
            _uiModel.Scene = scene;
        }

        public void Render(World newWorld)
        {
            _uiModel.SceneWindowInfo.Clear();
            _uiModel.WriteToScene(TextStyle.TitleStyle, newWorld.Name);
            _uiModel.WriteSpaceToScene();

            _uiModel.WriteToScene(TextStyle.ContentStyle, newWorld.Description);            
        }
    }
}
