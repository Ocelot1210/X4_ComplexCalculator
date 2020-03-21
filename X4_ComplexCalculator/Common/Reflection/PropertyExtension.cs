using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace X4_ComplexCalculator.Common.Reflection
{
    public static class PropertyExtension
    {
        public static IAccessor ToAccessor(this PropertyInfo pi)
        {
            var getterDelegateType = typeof(Func<,>).MakeGenericType(pi.DeclaringType, pi.PropertyType);
            var getter = Delegate.CreateDelegate(getterDelegateType, pi.GetGetMethod());

            var setterDelegateType = typeof(Action<,>).MakeGenericType(pi.DeclaringType, pi.PropertyType);
            var setter = Delegate.CreateDelegate(setterDelegateType, pi.GetSetMethod());

            var accessorType = typeof(Accessor<,>).MakeGenericType(pi.DeclaringType, pi.PropertyType);

            return (IAccessor)Activator.CreateInstance(accessorType, getter, setter);
        }
    }
}
