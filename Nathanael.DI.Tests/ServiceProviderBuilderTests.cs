using FluentAssertions;

namespace Nathanael.DI.Tests
{
    public class ServiceProviderBuilderTests
    {

        [Fact]
        public void Build_CreatesServiceProvider_FromEmptyConfiguration()
        {
            var config = new ServiceProviderConfiguration();
            var builder = new ServiceProviderBuilder();
            ServiceProvider serviceProvider = builder.Build(config);

            serviceProvider.Should().NotBeNull();
        }
    }
}