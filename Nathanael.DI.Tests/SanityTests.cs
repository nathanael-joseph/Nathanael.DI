using FluentAssertions;
using Nathanael.DI.Tests.Services;
using System.Linq;

namespace Nathanael.DI.Tests
{
    public class SanityTests
    {
        
        [Fact]
        public void Sanity()
        {
            var spc = new ServiceProviderConfiguration();

            spc.ProvideTransient<ServiceAB>()
               .FromFactory(sp => new ServiceAB() /* example factory method */)
               .WhenResolving<IDependencyA>()
               .WhenResolving<IDependencyB>();

            var builder = new ServiceProviderBuilder();
            var sp = builder.Build(spc);

            var depa = sp.GetService(typeof(IDependencyA));
            var depb = sp.GetService(typeof(IDependencyB));

            spc.ServiceConfigurations.Should().HaveCount(3); // configuration always contains two entries for the ServiceProvider and IServiceProvider services
            spc.ServiceConfigurations.Last().GetServiceTypes().Should().HaveCount(2);
            spc.ServiceConfigurations.Last().FactoryMethod.Should().NotBeNull();

            depa.Should().BeOfType<ServiceAB>();
            depb.Should().BeOfType<ServiceAB>();
            depa.Should().NotBeSameAs(depb, "becuase the service is registered as a transient");
        }

        [Fact]
        public void Sanity2()
        {
            var spc = new ServiceProviderConfiguration();

            spc.ProvideTransient<ServiceA>()
               .WhenResolving<IDependencyA>();
            spc.ProvideTransient<ServiceB>()
               .WhenResolving<IDependencyB>();
            spc.ProvideTransient<ServiceDependingOnAAndB>();

            var builder = new ServiceProviderBuilder();
            var sp = builder.Build(spc);
            var service = sp.GetService(typeof(ServiceDependingOnAAndB));

            spc.ServiceConfigurations.Should().HaveCount(5);
            service.Should().BeOfType<ServiceDependingOnAAndB>();
            service.As<ServiceDependingOnAAndB>().DependancyA.Should().NotBeNull();
            service.As<ServiceDependingOnAAndB>().DependancyA.Should().BeOfType<ServiceA>();
            service.As<ServiceDependingOnAAndB>().DependancyB.Should().NotBeNull();
            service.As<ServiceDependingOnAAndB>().DependancyB.Should().BeOfType<ServiceB>();
        }


        [Fact]
        public void ServiceProvider_ProvidesTransientService()
        {
            var spc = new ServiceProviderConfiguration();
            
            spc.ProvideTransient<ServiceA>();

            var builder = new ServiceProviderBuilder();
            var sp = builder.Build(spc);

            var a1 = sp.GetService<ServiceA>(); 
            var a2 = sp.GetService<ServiceA>();

            Assert.NotNull(a1);
            Assert.NotNull(a2);
            Assert.NotSame(a1, a2);
        }

        [Fact]
        public void ServiceProvider_ProvidesScopedService()
        {
            var spc = new ServiceProviderConfiguration();

            spc.ProvideScoped<ServiceA>();

            var builder = new ServiceProviderBuilder();
            var sp = builder.Build(spc);
            
            ServiceA a1;
            ServiceA a2;
            ServiceA a3;

            using (var scope = sp.CreateScopedServiceProvider())
            {
                a1 = scope.GetService<ServiceA>();
                a2 = scope.GetService<ServiceA>();
            }

            using (var scope = sp.CreateScopedServiceProvider())
            {
                a3 = scope.GetService<ServiceA>();
            }


            Assert.NotNull(a1);
            Assert.NotNull(a2);
            Assert.NotNull(a3);
            Assert.Same(a1, a2);
            Assert.NotSame(a1, a3);
        }

        [Fact]
        public void ServiceProvider_ProvidesSingletonService()
        {
            var spc = new ServiceProviderConfiguration();

            spc.ProvideSingleton<ServiceA>();

            var builder = new ServiceProviderBuilder();
            var sp = builder.Build(spc);

            ServiceA a1;
            ServiceA a2;
            ServiceA a3;

            a1 = sp.GetService<ServiceA>();

            using (var scope = sp.CreateScopedServiceProvider())
            {
                a2 = scope.GetService<ServiceA>();
            }

            using (var scope = sp.CreateScopedServiceProvider())
            {
                a3 = scope.GetService<ServiceA>();
            }


            Assert.NotNull(a1);
            Assert.NotNull(a2);
            Assert.NotNull(a3);
            Assert.Same(a1, a2);
            Assert.Same(a1, a3);
        }


    }
}