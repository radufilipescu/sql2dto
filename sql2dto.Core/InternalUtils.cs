using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace sql2dto.Core
{
    internal class InternalUtils
    {
        internal static string GetPropertyName<T>(Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            MemberExpression memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
            {
                // The property access might be getting converted to object to match the func
                // If so, get the operand and see if that's a member expression
                memberExpression = (expression.Body as UnaryExpression)?.Operand as MemberExpression;
            }

            if (memberExpression == null)
            {
                throw new ArgumentException("Expression is not a MemberExpression");
            }

            return memberExpression.Member.Name;
        }
    }
}
