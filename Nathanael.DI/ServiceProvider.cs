using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Nathanael.DI.Internal;

namespace Nathanael.DI
{
    public class ServiceProvider : IServiceProvider, IDisposable
    {
        private bool _disposed;
        private readonly bool _isScoped;
        private readonly ConcurrentDictionary<Type, IEnumerable<ServiceAccessor>> _serviceAccessors;

        internal ServiceProvider(ConcurrentDictionary<Type, IEnumerable<ServiceAccessor>> serviceAccessors)
        {
            _serviceAccessors = serviceAccessors;
        }

        public object? GetService(Type serviceType)
        {
            if (_serviceAccessors.TryGetValue(serviceType, out var accessor))
            {
                return accessor.First().GetService(this);
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (var accessor in _serviceAccessors.SelectMany(kvp => kvp.Value))
                    {
                        accessor.Dispose();
                    }
                }
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _serviceAccessors.Clear();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
