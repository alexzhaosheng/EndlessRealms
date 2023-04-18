using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services;
public interface ISceneChangedEvent
{
    Task NotifySceneChanged(Scene? from, Scene to);
}
