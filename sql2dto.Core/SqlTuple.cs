using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class SqlTuple : SqlExpression
    {
        public SqlTuple(params SqlExpression[] items)
        {
            _items = items;
        }

        public override SqlExpressionType GetExpressionType() => SqlExpressionType.TUPLE;

        private SqlExpression[] _items;

        public int GetItemsCount() => _items.Length;
        public SqlExpression GetItem(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex > _items.Length - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(itemIndex));
            }

            return _items[itemIndex];
        }
    }
}
