using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace sql2dto.Attributes
{
    // https://docs.microsoft.com/en-us/dotnet/api/system.type.getproperties?view=netframework-4.7.2
    // The Type.GetProperties method does not return properties in a particular order, such as alphabetical or declaration order.
    // Your code must not depend on the order in which properties are returned, because that order varies.
    // That is why there is a KeyPropsAttribute.

    // https://stackoverflow.com/questions/9062235/get-properties-in-order-of-declaration-using-reflection
    // On .net 4.5 (and even .net 4.0 in vs2012) you can do much better with reflection 
    // using clever trick with [CallerLineNumber] attribute, letting compiler insert order into your properties for you

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropMapAttribute : Attribute
    {
        public PropMapAttribute([CallerLineNumber] int declarationOrder = -1)
        {
            DeclarationOrder = declarationOrder;
        }

        public int DeclarationOrder { get; private set; }

        public string ColumnName { get; set; }
        public int? ColumnOrdinal { get; set; }
        public bool IsKey { get; set; }
        public bool IsNullableKey { get; set; }
    }
}
