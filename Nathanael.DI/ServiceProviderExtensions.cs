using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nathanael.DI
{
    internal static class ServiceProviderExtensions
    {
        public static object GetRequiredService(this IServiceProvider sp, Type serviceType)
        {
            var t = sp.GetService(serviceType);
            if(t == null)
            {
                throw new ServiceResolutionException($"No service of type {serviceType} could be resolved. Ensure the service has been registered");
            }

            return t;
        }
    }
}
