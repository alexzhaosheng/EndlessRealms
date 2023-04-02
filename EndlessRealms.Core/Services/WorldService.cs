using EndlessRealms.Core.Services.ChatGptDto;
using EndlessRealms.Core.Utility;
using EndlessRealms.Core.Utility.Extensions;
using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services
{
    [Service]
    public class WorldService
    {
        private readonly ChatGPTService _gptService;
        private readonly IPersistedDataProvider _persistedDataAccessor;
        private readonly IPlayerIoService _playIoService;
        private readonly IRenderService _renderService;
        private readonly ILogService _logService;
        private readonly SystemStatusManager _statusManager;

        public WorldService(ChatGPTService gptService, IPersistedDataProvider statusLoader, IPlayerIoService playIoService, IRenderService renderService, ILogService logService, SystemStatusManager statusManager)
        {
            _gptService = gptService;
            _persistedDataAccessor = statusLoader;
            _playIoService = playIoService;
            _renderService = renderService;
            _logService = logService;
            _statusManager = statusManager;
        }


        private List<World> _worlds = null!;

        public World? CurrentWorld { get; private set; }
        public Scene? Current { get; private set; }
        public Region? CurrentRegion => CurrentWorld?.Regions?.FirstOrDefault(r => r.Id == Current?.RegionId);

        public async Task Initialize()
        {
            _worlds = _persistedDataAccessor.LoadWorlds().ToList();

            if (_worlds.Count == 0)
            {
                _logService.Logger.Information("No world is detected. Create new world automatically");
                await CreateAWorld();
            }

            CurrentWorld = _worlds.First();
            if (CurrentWorld.Scenes.Count == 0)
            {
                _logService.Logger.Information("No scene is detected. Create new scene automatically");
                await CreateScene(CurrentWorld, CurrentWorld.Regions.First(), null);
            }
            Current = CurrentWorld.Scenes.First();
        }


        private async Task CreateAWorld()
        {
            string userMessage = "Welcome to Endless Realms!\n\nTo start, can you tell me more about the type of world you have in mind. \nIt can be a fantasy realm with magic and mythical creatures, a futuristic sci-fi world with advanced technology, or perhaps a post-apocalyptic world where survival is the key? \nThe more details you can provide, the better I can assist you in creating the world you envision.";
            World newWorld = null!;
            while (true)
            {
                string prompt = await _playIoService.GeneralInput(MessageType.Notice, userMessage)!;
                await _statusManager.ExecWithStatus(SystemStatus.Working, "Creating new world",
                    async () =>
                    {
                        var (w, _) = await _gptService.Call<World>(p => p.CREATE_WORLD, ("{PROMPT}", prompt));
                        newWorld = w;
                    });

                _renderService.Render(newWorld!);

                if (await _playIoService.GeneralConfirm(MessageType.Notice, "Is this the world you'd like to start with?"))
                {
                    break;
                }
            }

            await _statusManager.ExecWithStatus(SystemStatus.Working, "Creating regions for the world",
                   async () =>
                   {
                       var regions = (await _gptService.CallForArray<Region>(p => p.CREATE_REGIONS, ("{PROMPT}", newWorld.Description)))!;
                       newWorld.Regions = regions;
                   });

            _worlds.Add(newWorld);

            await _persistedDataAccessor.SaveWorld(newWorld);
        }

        private async Task<Scene> CreateScene(World world, Region region, (Scene, Direction)? fromInfo)
        {
            Scene scene = null!;
            await _statusManager.ExecWithStatus(SystemStatus.Working, "Creating new scene",
                async () =>
                {
                    var (s, _) = await _gptService.Call<Scene>(pmt => pmt.CREATE_SCENE,
                        ("{LOCATION_PROMPT}", region.Description!),
                        ("{WORLD_PROMPT}", world.Description));
                    scene = s;
                });

            await _playIoService.InteractiveMessage(MessageType.Notice, "New scene created.");

            scene.RegionId = region.Id;
            world.Scenes.Add(scene);
            scene.Name = $"{region.Name} {world.Scenes.Where(s => s.RegionId == region.Id).Count()}";

            if (fromInfo != null)
            {
                ConnectScenes(fromInfo.Value.Item1, fromInfo.Value.Item2, scene);

                List<(Scene, Vector2)> scenesAround = new List<(Scene, Vector2)>();
                List<string> passedSceneIds = new List<string>() { scene.Id };
                FindScenesAround(world, fromInfo.Value.Item1, 0, fromInfo.Value.Item2.GetOpposite().ToVector(), passedSceneIds, scenesAround);

                foreach (var sa in scenesAround)
                {
                    var direction = sa.Item2.ToDirection();
                    if (!scene.ConnectedScenes.ContainsKey(direction) && !sa.Item1.ConnectedScenes.ContainsKey(direction.GetOpposite()))
                    {
                        if (Math.Sqrt((float)RandomGenerator.Random.Next(100)) < (float)region.Complexity)  //generate random number in 1~100 space then sqrt to reduce the connection chance.
                        {
                            ConnectScenes(scene, direction, sa.Item1);
                        }
                    }
                }

            }

            var freeDirections = ((Direction[])typeof(Direction)
                            .GetEnumValues())
                            .Where(t => !scene.ConnectedScenes.ContainsKey(t))
                            .ToList();

            //in order to make sure this world is growable, we try to make it having at least 2 free exits

            var freeExitNum = world.Scenes.Select(t => t.ConnectedScenes.Where(t => t.Value == null).Count()).Sum();  
            if (freeExitNum < 2 && freeDirections.Count > 0)
            {
                var dIndex = RandomGenerator.Random.Next(0, freeDirections.Count());
                scene.ConnectedScenes[freeDirections[dIndex]] = null;
                freeDirections.RemoveAt(dIndex);
                if (freeDirections.Count > 0 && freeExitNum == 0)
                {
                    dIndex = RandomGenerator.Random.Next(0, freeDirections.Count());
                    scene.ConnectedScenes[freeDirections[dIndex]] = null;
                }
            }

            foreach (var direction in freeDirections)
            {
                if (Math.Sqrt((float)RandomGenerator.Random.Next(100)) < (float)region.Complexity)  //generate random number in 1~100 space then sqrt to reduce the connection chance.
                {
                    scene.ConnectedScenes[direction] = null;
                }
            }
            await _persistedDataAccessor.SaveWorld(world);
            return scene;
        }

        private void ConnectScenes(Scene scene, Direction direction, Scene target)
        {
            scene.ConnectedScenes[direction] = target.Id;
            target.ConnectedScenes[direction.GetOpposite()] = scene.Id;
        }

        private void FindScenesAround(World world, Scene scene, int distance, Vector2 vector, List<string> passedSceneIds, List<(Scene, Vector2)> scenesAround)
        {
            if (Math.Ceiling(vector.Length()) == 1 && distance > 1)
            {
                scenesAround.Add((scene, vector));
            }

            passedSceneIds.Add(scene.Id);

            foreach (var kv in scene.ConnectedScenes)
            {
                if (string.IsNullOrEmpty(kv.Value))
                {
                    continue;
                }

                if (!passedSceneIds.Contains(kv.Value))
                {
                    var nextScene = world!.Scenes.First(s => s.Id == kv.Value);
                    FindScenesAround(world, nextScene, distance + 1, vector + kv.Key.ToVector(), passedSceneIds, scenesAround);
                }
            }
        }

        public async Task MoveTo(Direction direction)
        {
            if (!Current!.ConnectedScenes.ContainsKey(direction))
            {
                return;
            }
            if (Current!.ConnectedScenes[direction] == null)
            {
                await CreateScene(CurrentWorld!, CurrentRegion!, (Current!, direction));
            }

            Current = CurrentWorld!.Scenes.First(s => s.Id == Current!.ConnectedScenes[direction]);
            _playIoService.ShowWorldMessage($"Moved to {direction}");
        }

        internal async Task Reset()
        {
            Current = null;
            CurrentWorld = null;
            _worlds.Clear();

            await _persistedDataAccessor.ClearALlGameData();

            await Initialize();
        }


        public void ReduceThing(string target)
        {
            var t = Current!.Things?.FirstOrDefault(t => t.Name == target);
            if (t != null)
            {
                t.Number--;
                if (t.Number <= 0)
                {
                    Current?.Things?.Remove(t);
                }
            }
        }
       

        public void Remove(CharactorInfo charInfo)
        {
            this.Current!.Characters?.Remove(charInfo);
        }

        public async Task SaveCurrentWorld()
        {
            await _persistedDataAccessor.SaveWorld(CurrentWorld!);
        }
    }
}
