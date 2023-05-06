using EndlessRealms.Core.Services.ChatGptDto;
using EndlessRealms.Core.Utility;
using EndlessRealms.Core.Utility.Extensions;
using EndlessRealms.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EndlessRealms.Core.Services
{
    [Service]
    public class WorldService
    {
        private readonly ChatGPTService _gptService;
        private readonly IPersistedDataProvider _persistedDataAccessor;
        private readonly PlayerIoManager _playIoMgr;
        private readonly IRenderService _renderService;
        private readonly ILogService _logService;
        private readonly SystemStatusManager _statusManager;
        private readonly GameContext _gameContext;
        private readonly IServiceProvider _serviceProvider;

        public WorldService(ChatGPTService gptService, IPersistedDataProvider statusLoader, PlayerIoManager playIoService, IRenderService renderService, ILogService logService, SystemStatusManager statusManager, GameContext gameContext, IServiceProvider serviceProvider)
        {
            _gptService = gptService;
            _persistedDataAccessor = statusLoader;
            _playIoMgr = playIoService;
            _renderService = renderService;
            _logService = logService;
            _statusManager = statusManager;
            _gameContext = gameContext;
            _serviceProvider = serviceProvider;
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

            CurrentWorld = _worlds.FirstOrDefault(t => t.Id == _gameContext.CurrentPlayerInfo.CurrentWorldId)
                ?? _worlds.First();
            if (CurrentWorld.Scenes.Count == 0)
            {
                _logService.Logger.Information("No scene is detected. Create new scene automatically");
                await CreateScene(CurrentWorld, CurrentWorld.Regions.First(), null);
            }
            Current = CurrentWorld.Scenes.FirstOrDefault(t=>t.Id == _gameContext.CurrentPlayerInfo.CurrentSceneId)
                ?? CurrentWorld.Scenes.First();
        }


        private async Task CreateAWorld()
        {
            string userMessage = "Welcome to Endless Realms!\n\nTo start, can you tell me more about the type of world you have in mind. \nIt can be a fantasy realm with magic and mythical creatures, a futuristic sci-fi world with advanced technology, or perhaps a post-apocalyptic world where survival is the key? \nThe more details you can provide, the better I can assist you in creating the world you envision.";
            World newWorld = null!;
            while (true)
            {
                string? prompt = await _playIoMgr.Input(InputType.GeneralInput, userMessage, (s)=> (s.Length > 10, "Must be a longer text."));
                await _statusManager.ExecWithStatus(SystemStatus.Working, "Creating new world",
                    async () =>
                    {
                        var (w, _) = await _gptService.Call<World>(p => p.CREATE_WORLD, 
                            ("{PROMPT}", prompt!),
                            ("{PLAYER_LANGUAGE}", _gameContext.CurrentPlayerInfo.Language));
                        newWorld = w;
                    });

                
                if ((await _playIoMgr.Confirm($"{newWorld.Name}\n{newWorld.Description}\n\n Is this the world you'd like to start with?")).GetValueOrDefault())
                {
                    break;
                }
            }

            await _statusManager.ExecWithStatus(SystemStatus.Working, "Creating regions for the world",
                   async () =>
                   {
                       var regions = (await _gptService.CallForArray<Region>(p => p.CREATE_REGIONS, 
                           ("{PROMPT}", newWorld.Description),
                           ("{PLAYER_LANGUAGE}", _gameContext.CurrentPlayerInfo.Language)))!;
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
                        ("{PLAYER_LANGUAGE}", _gameContext.CurrentPlayerInfo.Language),
                        ("{LOCATION_PROMPT}", region.Description!),
                        ("{WORLD_PROMPT}", world.Description));
                    scene = s;
                });

            _playIoMgr.OutputMessage(OutputType.UiMessage, "New scene created.");

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

            SceneAdded?.Invoke(this, EventArgs.Empty);
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

            var target = CurrentWorld!.Scenes.First(s => s.Id == Current!.ConnectedScenes[direction]);
            var from = Current;
            Current = target;

            _playIoMgr.OutputMessage(OutputType.WorldMessage, $"Moved to {direction}");

            foreach(var srv in _serviceProvider.GetServices<ISceneChangedEvent>())
            {
                await srv.NotifySceneChanged(from, target);
            }

            _gameContext.CurrentPlayerInfo.CurrentSceneId = Current.Id;
            _gameContext.CurrentPlayerInfo.CurrentWorldId = CurrentWorld.Id;
            await _gameContext.SavePlayerInfo();

            SceneChanged?.Invoke(this, EventArgs.Empty);

            SaveChangedWorlds();
        }

        internal Task Reset()
        {
            Current = null;
            CurrentWorld = null;
            _worlds.Clear();
            return Task.CompletedTask;
        }     


        internal void Remove(IActionTarget target)
        {
            if(target.Type == TargetType.Charactor)
            {
                Current!.Characters.Remove((CharacterInfo)target);                
            }
            else
            {
                ((Something)target).Number--;
                Current!.Things.RemoveAll(t => t.Number <= 0);                
            }
            NotifyWorldChanged(CurrentWorld!);
        }


        private HashSet<World> _changedWorlds = new HashSet<World>();        
        public void NotifyWorldChanged(World world)
        {
            if (!_changedWorlds.Contains(world))
            {
                _changedWorlds.Add(world);
            }
        }

        public async void SaveChangedWorlds()
        {
            while(_changedWorlds.Count > 0)
            {
                var w = _changedWorlds.First();
                await _persistedDataAccessor.SaveWorld(w);
                _changedWorlds.Remove(w);
            }
        }

        public event EventHandler<EventArgs> SceneAdded;
        public event EventHandler<EventArgs> WorldChanged;
        public event EventHandler<EventArgs> SceneChanged;

        public void NotifyCurrentWorldChanged()
        {
            NotifyWorldChanged(CurrentWorld!);

            WorldChanged?.Invoke(this, EventArgs.Empty);
        }

    }
}
