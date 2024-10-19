using System;

namespace Nathanael.DI
{
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        /// <exception cref="ServiceResolutionException"></exception>
        internal static object GetRequiredServiceInternal(this IServiceProvider sp, Type serviceType)
        {
            var s = sp.GetService(serviceType);
            if(s == null)
            {
                throw new ServiceResolutionException($"No service of type {serviceType} could be resolved. Ensure the service has been registered");
            }

            return s;
        }

        /// <summary>
        /// Gets a service from the service provider. If no service can be resolved, an exception is thrown
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        /// <exception cref="ServiceResolutionException"></exception>
        public static object GetRequiredService(this ServiceProvider sp, Type serviceType)
        {
            return GetRequiredServiceInternal((IServiceProvider)sp, serviceType);
        }

        /// <summary>
        /// Gets a service from the service provider. If no service can be resolved, an exception is thrown
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        /// <exception cref="ServiceResolutionException"></exception>
        public static TService GetRequiredService<TService>(this ServiceProvider sp)
        {
            return (TService)GetRequiredService(sp, typeof(TService));
        }

        public static TService? GetService<TService>(this ServiceProvider sp)
        {
            var s = sp.GetService(typeof(TService));
            return s != null ? (TService)s : default;
        }
        
    }
}
