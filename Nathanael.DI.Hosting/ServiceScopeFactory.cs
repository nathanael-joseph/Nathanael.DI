using Microsoft.Extensions.DependencyInjection;

namespace Nathanael.DI.Hosting
{
    public class ServiceScopeFactory : IServiceScopeFactory
    {
        private readonly ServiceProvider _serviceProvider;

        public ServiceScopeFactory(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IServiceScope CreateScope()
        {
            return new ServiceScope(_serviceProvider.CreateScopedServiceProvider());
        }
    }
}
