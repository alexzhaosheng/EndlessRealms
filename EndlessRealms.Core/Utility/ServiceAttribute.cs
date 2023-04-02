using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Utility
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute: Attribute
    {
        public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Singleton;
        public Type? ServiceType { get; set; }
        public ServiceAttribute(Type? serviceType = null)
        {            
            ServiceType = serviceType; 
        }
    }
}
