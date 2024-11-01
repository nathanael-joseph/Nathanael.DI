using Microsoft.Extensions.DependencyInjection;

namespace Nathanael.DI.Hosting
{
    public class ServiceScope : IServiceScope
    {
        /// <inheritdoc/>
        public IServiceProvider ServiceProvider { get; }

        internal ServiceScope(IServiceProvider serviceProvider)
        {
           ServiceProvider = serviceProvider;
        }

        public void Dispose()
        {
            if (ServiceProvider is IDisposable d) 
            {
                d.Dispose();
            }
        }
    }
}
