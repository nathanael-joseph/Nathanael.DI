using System;

namespace Nathanael.DI
{
    public static class ServiceProviderBuilderExtensions
    {
        public static ServiceProviderBuilder Configure(this ServiceProviderBuilder builder, Action<ServiceProviderConfiguration> configure)
        {
            ArgumentNullException.ThrowIfNull(configure, nameof(configure));
            configure(builder.Configuration);
            return builder;
        }
    }
}
