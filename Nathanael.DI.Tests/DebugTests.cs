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
            var constructed_generic_type = typeof(IAmOf<int, string>);
            var generic_args = constructed_generic_type.GetGenericArguments();
            var generic_type_args = constructed_generic_type.GetGenericTypeDefinition().GetGenericArguments();

            var ctr = constructed_generic_type.GetConstructors().First();
            var ctr_parameter = ctr.GetParameters().First();

            var ctr_parameter_generic_arg = ctr_parameter.ParameterType.GetGenericArguments().First();

            generic_args.Should().Contain(ctr_parameter_generic_arg);
            generic_type_args.Should().NotContain(ctr_parameter_generic_arg);
        }
    }
}
