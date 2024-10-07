using System;
using System.Collections.Generic;

namespace Nathanael.DI;

public class ServiceProviderConfiguration
{
    public List<ServiceConfiguration> ServiceConfigurations { get; }

    public ServiceProviderConfiguration()
    {
        var spConfig = new ServiceConfiguration(typeof(IServiceProvider), Lifetime.Transient, sp => sp);
        ServiceConfigurations = new List<ServiceConfiguration>() { spConfig };
    }

}

