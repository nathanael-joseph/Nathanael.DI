using System;
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
            : this(serviceAccessors, false)
        { }

        private ServiceProvider(ConcurrentDictionary<Type, IEnumerable<ServiceAccessor>> serviceAccessors, bool isScoped)
        {
            _serviceAccessors = serviceAccessors;
            _isScoped = isScoped;
        }

        public ServiceProvider CreateScopedServiceProvider()
        {
            ConcurrentDictionary<Type, IEnumerable<ServiceAccessor>> scopedServiceAccessors = new();
            foreach(var serviceAccessor in _serviceAccessors)
            {
                scopedServiceAccessors[serviceAccessor.Key] = serviceAccessor.Value.Select(a => a.CreateScopedServiceAccessor()).ToArray();
            }

            return new ServiceProvider(scopedServiceAccessors, true);
        }

        /// <inheritdoc/>
        public object? GetService(Type serviceType)
        {
            if (_serviceAccessors.TryGetValue(serviceType, out var accessors))
            {
                return accessors.First().GetService(this);
            }

            if (IsIEnumerable(serviceType))
            {
                var gtp = serviceType.GetGenericArguments().First();
                var lstType = typeof(List<>).MakeGenericType(gtp);
                var lst = Activator.CreateInstance(lstType);
                
                if (_serviceAccessors.TryGetValue(gtp, out accessors))
                {
                    var add = lstType.GetMethod("Add");
                    foreach (var a in accessors)
                    {
                        add.Invoke(lst, new[] { a.GetService(this, gtp) });
                    }
                }

                return lst;
            }

            if (serviceType.IsGenericType && _serviceAccessors.TryGetValue(serviceType.GetGenericTypeDefinition(), out accessors))
            {
                return accessors.First().GetService(this, serviceType);
            }

            return null;
        }

        private bool IsIEnumerable(Type serviceType)
        {
            return serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>);    
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (var accessor in _serviceAccessors.SelectMany(kvp => kvp.Value))
                    {
                        if(!_isScoped || accessor is ScopedServiceAccessor)
                        {
                            accessor.Dispose();
                        }
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
