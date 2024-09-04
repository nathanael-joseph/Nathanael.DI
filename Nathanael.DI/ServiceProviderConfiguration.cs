using System;
using System.Collections.Generic;

namespace Nathanael.DI;

public class ServiceProviderConfiguration
{
    public List<ServiceConfiguration> ServiceConfigurations { get; } = new();

    public ServiceProviderConfiguration()
    {
    }

}

