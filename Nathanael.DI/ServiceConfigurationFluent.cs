using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nathanael.DI
{
    public class ServiceConfigurationFluent<TService>
    {
        public ServiceConfiguration ServiceConfiguration { get; }

        internal ServiceConfigurationFluent(Lifetime lifetime)
        {
            ServiceConfiguration = new(typeof(TService), lifetime);
        }

        public ServiceConfigurationFluent<TService> WhenResolving<TDep>()
        {
            ServiceConfiguration.RegisterDependencyType(typeof(TDep));
            return this;
        }

        public ServiceConfigurationFluent<TService> FromFactory(Func<IServiceProvider, TService> factoryMethod)
        {
            ArgumentNullException.ThrowIfNull(factoryMethod, nameof(factoryMethod));
            ServiceConfiguration.FactoryMethod = sp => factoryMethod(sp);
            return this;
        }
    }

    public static class FluentExtensions
    {
        public static ServiceConfigurationFluent<TService> ProvideTransient<TService>(this ServiceProviderConfiguration serviceProviderConfiguration)
        {
            var fluent = new ServiceConfigurationFluent<TService>(Lifetime.Transient);
            serviceProviderConfiguration.ServiceConfigurations.Add(fluent.ServiceConfiguration);
            return fluent;
        }

        public static ServiceConfigurationFluent<TService> ProvideScoped<TService>(this ServiceProviderConfiguration serviceProviderConfiguration)
        {
            var fluent = new ServiceConfigurationFluent<TService>(Lifetime.Scoped);
            serviceProviderConfiguration.ServiceConfigurations.Add(fluent.ServiceConfiguration);
            return fluent;
        }

        public static ServiceConfigurationFluent<TService> ProvideSingleton<TService>(this ServiceProviderConfiguration serviceProviderConfiguration)
        {
            var fluent = new ServiceConfigurationFluent<TService>(Lifetime.Singleton);
            serviceProviderConfiguration.ServiceConfigurations.Add(fluent.ServiceConfiguration);
            return fluent;
        }

        public static ServiceConfigurationFluent<TService> Provide<TService>(this ServiceProviderConfiguration serviceProviderConfiguration, TService service)
        {
            var fluent = new ServiceConfigurationFluent<TService>(Lifetime.Singleton).FromFactory(sp => service);
            serviceProviderConfiguration.ServiceConfigurations.Add(fluent.ServiceConfiguration);
            return fluent;
        }
    }

  
}
