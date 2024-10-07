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

            spc.ServiceConfigurations.Should().HaveCount(1);
            spc.ServiceConfigurations.First().GetServiceTypes().Should().HaveCount(2);
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
               .WhenResolving<IDependencyA>();
            spc.ProvideTransient<ServiceB>()
               .WhenResolving<IDependencyB>();
            spc.ProvideTransient<ServiceDependingOnAAndB>();

            var builder = new ServiceProviderBuilder();
            var sp = builder.Build(spc);
            var service = sp.GetService(typeof(ServiceDependingOnAAndB));

            spc.ServiceConfigurations.Should().HaveCount(3);
            service.Should().BeOfType<ServiceDependingOnAAndB>();
            service.As<ServiceDependingOnAAndB>().DependancyA.Should().NotBeNull();
            service.As<ServiceDependingOnAAndB>().DependancyA.Should().BeOfType<ServiceA>();
            service.As<ServiceDependingOnAAndB>().DependancyB.Should().NotBeNull();
            service.As<ServiceDependingOnAAndB>().DependancyB.Should().BeOfType<ServiceB>();
        }

    }
}