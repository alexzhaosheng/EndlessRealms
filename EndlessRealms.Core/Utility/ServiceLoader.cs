using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Utility
{
    public static class ServiceLoader
    {
        public static void LoadServices(this IServiceCollection services, Assembly asb)
        {
            foreach(var type in asb.GetTypes())
            {
                var attr = type.GetCustomAttribute<ServiceAttribute>();
                if (attr == null){
                    continue;
                }

                services.Add(new ServiceDescriptor(attr.ServiceType ?? type, type, attr.Lifetime));
            }
        }
    }
}
