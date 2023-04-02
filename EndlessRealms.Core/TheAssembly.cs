using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core
{
    public static class TheAssembly
    {
        public static readonly Assembly Assembly = typeof(TheAssembly).Assembly;
    }
}
