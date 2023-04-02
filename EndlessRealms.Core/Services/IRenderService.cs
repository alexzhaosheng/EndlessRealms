using EndlessRealms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services
{
    public interface IRenderService
    {
        void Render(World world, Region region, Scene scene);
        void Render(World newWorld);
    }
}
