using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nathanael.DI.Tests.Services
{
    internal interface IDependencyA { }
    internal interface IDependencyB { }

    internal class ServiceA : IDependencyA { }
    internal class ServiceB : IDependencyB { }
    internal class ServiceAB : IDependencyA, IDependencyB { }
    internal class ServiceDependingOnAAndB
    {
        public IDependencyA DependancyA { get; }
        public IDependencyB DependancyB { get; }

        public ServiceDependingOnAAndB(IDependencyA dependancyA, IDependencyB dependancyB)
        {
            DependancyA = dependancyA;
            DependancyB = dependancyB;
        }
    }
}
