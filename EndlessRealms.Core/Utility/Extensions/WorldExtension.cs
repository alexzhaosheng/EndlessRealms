using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Utility.Extensions;
public static class WorldExtension
{

    public static List<(Scene, ScenePosition)> GetScenePositions(this World world)
    {
        List<(Scene, ScenePosition)> scenesWithPositions = new List<(Scene, ScenePosition)>();
        
        var scene = world.Scenes.First();

        FillPositionInfo(scenesWithPositions, world, scene, new ScenePosition(0,0));
        return scenesWithPositions;
    }

    private static void FillPositionInfo(List<(Scene, ScenePosition)> scenesWithPositions, World world, Scene scene, ScenePosition pos)
    {
        scenesWithPositions.Add((scene, pos));
        foreach(var kv in scene.ConnectedScenes)
        {
            if (string.IsNullOrEmpty(kv.Value))
            {
                continue;
            }
            var nextScene = world!.Scenes.First(s => s.Id == kv.Value);
            if(!scenesWithPositions.Any(p=>p.Item1 == nextScene))
            {
                FillPositionInfo(scenesWithPositions, world, nextScene, pos + kv.Key.ToPosition());
            }
        }
    }
}
