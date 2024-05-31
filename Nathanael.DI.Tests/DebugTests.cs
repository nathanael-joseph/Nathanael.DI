using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nathanael.DI.Tests
{
    public class DebugTests
    {
        public class A { }

        public void MyMethod(int? nullablePrimitive, A? nullableReference, double primitive, A reference)
        {

        }

        [Fact]
        public void TestNullibilityInfo_ForParameterInfo()
        {
            var nc = new NullabilityInfoContext();
            var mi = typeof(DebugTests).GetMethod(nameof(MyMethod), BindingFlags.Public | BindingFlags.Instance)!;
            var pis = mi.GetParameters();

            nc.Create(pis[0]).WriteState.Should().Be(NullabilityState.Nullable);
            nc.Create(pis[0]).ReadState.Should().Be(NullabilityState.Nullable);

            nc.Create(pis[1]).WriteState.Should().Be(NullabilityState.Nullable);
            nc.Create(pis[1]).ReadState.Should().Be(NullabilityState.Nullable);

            nc.Create(pis[2]).WriteState.Should().Be(NullabilityState.NotNull);
            nc.Create(pis[2]).ReadState.Should().Be(NullabilityState.NotNull);

            nc.Create(pis[3]).WriteState.Should().Be(NullabilityState.NotNull);
            nc.Create(pis[3]).ReadState.Should().Be(NullabilityState.NotNull);
        }

        public interface IUse<T>
        {

        }

        public class IAmOf<T, k>
        {
            public IAmOf(IUse<T> use) 
            { 
            }
        }

        [Fact]
        public void CanInstanciateAny()
        {
            var gtd_ga = typeof(IAmOf<int, string>).GetGenericArguments().ToArray();

            var gtdci = typeof(IAmOf<,>).GetConstructors().First();
            var gtdci_params = gtdci.GetParameters().ToArray();

            var iscgm = gtdci.IsConstructedGenericMethod;

            var concreteci = typeof(IAmOf<int, string>).GetConstructors().First();
            var concreteci_params = concreteci.GetParameters().ToArray();
        }
    }
}
