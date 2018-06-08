using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sql2dto.Core
{
    public class SqlFunctionOver
    {
        internal SqlFunctionOver()
        {
            _partitionByExpressions = new List<SqlExpression>();
            _orderByExpressions = new List<(SqlExpression, SqlOrderByDirection)>();
            _windowingType = SqlWindowingType.NOT_SPECIFIED;
        }

        private List<SqlExpression> _partitionByExpressions;
        public List<SqlExpression> PartitionByExpressions => _partitionByExpressions;

        private List<(SqlExpression, SqlOrderByDirection)> _orderByExpressions;
        public List<(SqlExpression, SqlOrderByDirection)> OrderByExpressions => _orderByExpressions;

        private SqlWindowingType _windowingType;
        public SqlWindowingType WindowingType => _windowingType;

        private SqlWindowFrameBound _windowingBeginFrameBound;
        public SqlWindowFrameBound WindowingBeginFrameBound => _windowingBeginFrameBound;

        private SqlWindowFrameBound _windowingEndFrameBound;
        public SqlWindowFrameBound WindowingEndFrameBound => _windowingEndFrameBound;

        public SqlFunctionOver PartitionBy(params SqlExpression[] by)
        {
            _partitionByExpressions.AddRange(by);
            return this;
        }

        public SqlFunctionOver OrderBy(params SqlExpression[] by)
        {
            _orderByExpressions.AddRange(by.Select(item => (item, SqlOrderByDirection.ASCENDING)));
            return this;
        }

        public SqlFunctionOver OrderByDescending(params SqlExpression[] by)
        {
            _orderByExpressions.AddRange(by.Select(item => (item, SqlOrderByDirection.DESCENDING)));
            return this;
        }

        public SqlFunctionOver Rows(Func<SqlWindowFrameBound, SqlWindowFrameBound> boundTo)
        {
            _windowingType = SqlWindowingType.ROWS;
            _windowingBeginFrameBound = boundTo(new SqlWindowFrameBound());
            return this;
        }

        public SqlFunctionOver RowsBetween(Func<SqlWindowFrameBound, SqlWindowFrameBound> begin, Func<SqlWindowFrameBound, SqlWindowFrameBound> end)
        {
            _windowingType = SqlWindowingType.ROWS;
            _windowingBeginFrameBound = begin(new SqlWindowFrameBound());
            _windowingEndFrameBound = end(new SqlWindowFrameBound());
            return this;
        }

        public SqlFunctionOver Range(Func<SqlWindowFrameBound, SqlWindowFrameBound> boundTo)
        {
            _windowingType = SqlWindowingType.RANGE;
            _windowingBeginFrameBound = boundTo(new SqlWindowFrameBound());
            return this;
        }

        public SqlFunctionOver RangeBetween(Func<SqlWindowFrameBound, SqlWindowFrameBound> begin, Func<SqlWindowFrameBound, SqlWindowFrameBound> end)
        {
            _windowingType = SqlWindowingType.RANGE;
            _windowingBeginFrameBound = begin(new SqlWindowFrameBound());
            _windowingEndFrameBound = end(new SqlWindowFrameBound());
            return this;
        }
    }

    public enum SqlWindowingType
    {
        NOT_SPECIFIED,
        ROWS,
        RANGE,
    }

    public enum SqlWindowFrameBoundType
    {
        NOT_SPECIFIED,
        CURRENT_ROW,
        PRECEDING_UNBOUNDED,
        PRECEDING_COUNT,
        FOLLOWING_UNBOUNDED,
        FOLLOWING_COUNT,
    }

    public class SqlWindowFrameBound
    {
        internal SqlWindowFrameBound()
        {
            _windowFrameType = SqlWindowFrameBoundType.NOT_SPECIFIED;
        }

        private int? _count;
        public int? Count => _count;

        private SqlWindowFrameBoundType _windowFrameType;
        public SqlWindowFrameBoundType WindowFrameType => _windowFrameType;

        public SqlWindowFrameBound CurrentRow()
        {
            _windowFrameType = SqlWindowFrameBoundType.CURRENT_ROW;
            return this;
        }

        public SqlWindowFrameBound PrecedingUnbounded()
        {
            _windowFrameType = SqlWindowFrameBoundType.PRECEDING_UNBOUNDED;
            return this;
        }

        public SqlWindowFrameBound PrecedingCount(int count)
        {
            _windowFrameType = SqlWindowFrameBoundType.PRECEDING_COUNT;
            _count = count;
            return this;
        }

        public SqlWindowFrameBound FollowingUnbounded()
        {
            _windowFrameType = SqlWindowFrameBoundType.FOLLOWING_UNBOUNDED;
            return this;
        }

        public SqlWindowFrameBound FollowingCount(int count)
        {
            _windowFrameType = SqlWindowFrameBoundType.FOLLOWING_COUNT;
            _count = count;
            return this;
        }
    }
}
