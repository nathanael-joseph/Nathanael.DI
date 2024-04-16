using FluentAssertions;
using System.Linq;

namespace Nathanael.DI.Tests
{
    public class UnitTest1
    {
        public interface IDependancyA { }
        public interface IDependancyB { }

        public class ServiceA : IDependancyA { }
        public class ServiceB : IDependancyB { }
        public class ServiceAB : IDependancyA, IDependancyB { }

        [Fact]
        public void Sanity()
        {
            var spc = new ServiceProviderConfiguration();
            
            spc.ProvideTransient<ServiceAB>()
               .WhenResolving<IDependancyA>()
               .WhenResolving<IDependancyB>()
               .FromFactory(sp => new ServiceAB());

            var builder = new ServiceProviderBuilder();
            var sp = builder.Build(spc);

            var depa = sp.GetService(typeof(IDependancyA));
            var depb = sp.GetService(typeof(IDependancyB));

            spc.ServiceConfigurations.Should().HaveCount(1);
            spc.ServiceConfigurations.First().GetDependencyTypes().Should().HaveCount(2);
            spc.ServiceConfigurations.First().FactoryMethod.Should().NotBeNull();

            depa.Should().BeOfType<ServiceAB>();
            depb.Should().BeOfType<ServiceAB>();
            depa.Should().NotBe(depb, "becuase ther service is registered as a transient");
        }
    }
}