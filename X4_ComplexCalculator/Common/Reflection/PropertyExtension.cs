using System;
using System.Reflection;

namespace X4_ComplexCalculator.Common.Reflection
{
    public static class PropertyExtension
    {
        public static IAccessor ToAccessor(this PropertyInfo pi)
        {
            var type = pi.DeclaringType ?? throw new InvalidOperationException();
            var getMethod = pi.GetGetMethod() ?? throw new InvalidOperationException();
            var setMethod = pi.GetSetMethod() ?? throw new InvalidOperationException();


            var getterDelegateType = typeof(Func<,>).MakeGenericType(type, pi.PropertyType);
            var getter = Delegate.CreateDelegate(getterDelegateType, getMethod);

            var setterDelegateType = typeof(Action<,>).MakeGenericType(type, pi.PropertyType);
            var setter = Delegate.CreateDelegate(setterDelegateType, setMethod);

            var accessorType = typeof(Accessor<,>).MakeGenericType(type, pi.PropertyType);

            return (IAccessor?)Activator.CreateInstance(accessorType, getter, setter) ?? throw new InvalidOperationException();
        }
    }
}
