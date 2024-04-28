using FluentAssertions;
using System.Linq;

namespace Nathanael.DI.Tests
{
    public class SanityTests
    {
        private interface IDependancyA { }
        private interface IDependancyB { }

        private class ServiceA : IDependancyA { }
        private class ServiceB : IDependancyB { }
        private class ServiceAB : IDependancyA, IDependancyB { }
        private class ServiceWithDependencies
        {
            public IDependancyA DependancyA { get; }
            public IDependancyB DependancyB { get; }
            
            public ServiceWithDependencies(IDependancyA dependancyA, IDependancyB dependancyB)
            {
                DependancyA = dependancyA;
                DependancyB = dependancyB;
            }
        }

        [Fact]
        public void Sanity()
        {
            var spc = new ServiceProviderConfiguration();
         
            spc.ProvideTransient<ServiceAB>()
               .FromFactory(sp => new ServiceAB() /* example factory method */)
               .WhenResolving<IDependancyA>()
               .WhenResolving<IDependancyB>();

            var builder = new ServiceProviderBuilder();
            var sp = builder.Build(spc);

            var depa = sp.GetService(typeof(IDependancyA));
            var depb = sp.GetService(typeof(IDependancyB));

            spc.ServiceConfigurations.Should().HaveCount(1);
            spc.ServiceConfigurations.First().GetDependencyTypes().Should().HaveCount(2);
            spc.ServiceConfigurations.First().FactoryMethod.Should().NotBeNull();

            depa.Should().BeOfType<ServiceAB>();
            depb.Should().BeOfType<ServiceAB>();
            depa.Should().NotBeSameAs(depb, "becuase the service is registered as a transient");
        }

        [Fact]
        public void Sanity2()
        {
            var spc = new ServiceProviderConfiguration();

            spc.ProvideTransient<ServiceA>()
               .WhenResolving<IDependancyA>();
            spc.ProvideTransient<ServiceB>()
               .WhenResolving<IDependancyB>();
            spc.ProvideTransient<ServiceWithDependencies>();

            var builder = new ServiceProviderBuilder();
            var sp = builder.Build(spc);
            var service = sp.GetService(typeof(ServiceWithDependencies));

            spc.ServiceConfigurations.Should().HaveCount(3);
            service.Should().BeOfType<ServiceWithDependencies>();
            service.As<ServiceWithDependencies>().DependancyA.Should().NotBeNull();
            service.As<ServiceWithDependencies>().DependancyA.Should().BeOfType<ServiceA>();
            service.As<ServiceWithDependencies>().DependancyB.Should().NotBeNull();
            service.As<ServiceWithDependencies>().DependancyB.Should().BeOfType<ServiceB>();
        }

    }
}