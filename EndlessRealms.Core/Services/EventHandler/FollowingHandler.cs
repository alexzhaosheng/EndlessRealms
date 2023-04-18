using EndlessRealms.Core.Utility;
using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services.EventHandler;

[Service(typeof(ISceneChangedEvent))]
internal class FollowingHandler : ISceneChangedEvent
{
    private readonly WorldService _worldService;
    private readonly PlayerIoManager _playerIoManager;
    public FollowingHandler(WorldService worldService, PlayerIoManager playerIoManager)
    {
        _worldService = worldService;
        _playerIoManager = playerIoManager;
    }
    public Task NotifySceneChanged(Scene? from, Scene to)
    {
        if(from == null)
        {
            return Task.CompletedTask;
        }

        var changed = from.Characters.RemoveAll(c =>
        {
            if (c.Status == TargetStatus.Following)
            {
                to.Characters.Add(c);
                _playerIoManager.OutputMessage(OutputType.WorldMessage, $"{c.FullName} is following you to {to.Name}.");
                return true;
            }
            return false;
        });

        if (changed > 0)
        {
            _worldService.NotifyCurrentWorldChanged();
        }
        return Task.CompletedTask;
    }
}
