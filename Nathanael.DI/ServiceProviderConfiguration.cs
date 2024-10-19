using System;
using System.Collections.Generic;

namespace Nathanael.DI;

public class ServiceProviderConfiguration
{
    public List<ServiceConfiguration> ServiceConfigurations { get; }

    public ServiceProviderConfiguration()
    {
        var ispConfig = new ServiceConfiguration(typeof(IServiceProvider), Lifetime.Transient, sp => sp);
        var spConfig = new ServiceConfiguration(typeof(ServiceProvider), Lifetime.Transient, sp => sp as ServiceProvider);
        ServiceConfigurations = new List<ServiceConfiguration>() { ispConfig, spConfig };
    }
}

