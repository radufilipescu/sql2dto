using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sql2dto.Core
{
    public class SqlQuery : SqlTabularSource
    {
        public SqlQuery(SqlBuilder builder)
        {
            _builder = builder;
            _selectExpressions = new List<(SqlExpression, string)>();
            _fromAndJoinClauses = new List<(SqlJoinType, SqlTabularSource, SqlExpression)>();
        }

        private SqlBuilder _builder;

        private string _queryAlias;
        public string GetQueryAlias() => _queryAlias;

        private List<(SqlExpression, string)> _selectExpressions;
        public List<(SqlExpression, string)> GetSelectExpressions() => _selectExpressions;

        private List<(SqlJoinType, SqlTabularSource, SqlExpression)> _fromAndJoinClauses;
        public List<(SqlJoinType, SqlTabularSource, SqlExpression)> GetFromAndJoinClauses() => _fromAndJoinClauses;

        public sealed override SqlTabularSourceType TabularType => SqlTabularSourceType.QUERY;

        public string BuildQueryString()
        {
            return _builder.BuildQueryString(this);
        }

        public SqlQuery Select(SqlExpression expression, string columnAlias = null)
        {
            if (expression.GetExpressionType() == SqlExpressionType.COLUMN)
            {
                var column = (SqlColumn)expression;
                _selectExpressions.Add((column, columnAlias ?? column.GetColumnName()));
            }
            else
            {
                _selectExpressions.Add((expression, columnAlias));
            }
            
            return this;
        }

        public SqlQuery Select(params SqlExpression[] selectExpressions)
        {
            foreach (var expression in selectExpressions)
            {
                if (expression.GetExpressionType() == SqlExpressionType.COLUMN)
                {
                    var column = (SqlColumn)expression;
                    _selectExpressions.Add((column, column.GetColumnName()));
                }
                else
                {
                    _selectExpressions.Add((expression, null));
                }
            }
            return this;
        }

        public SqlQuery Select(params (SqlExpression, string)[] selectExpressions)
        {
            _selectExpressions.AddRange(selectExpressions);
            return this;
        }

        public SqlQuery As(string subqueryAlias)
        {
            _queryAlias = subqueryAlias;
            return this;
        }

        public SqlQuery From(SqlTabularSource fromPart)
        {
            _fromAndJoinClauses.Add((SqlJoinType.NONE, fromPart, (SqlExpression)null));
            return this;
        }

        public SqlQuery InnerJoin(SqlTabularSource innerJoinPart, SqlExpression on)
        {
            _fromAndJoinClauses.Add((SqlJoinType.INNER, innerJoinPart, on));
            return this;
        }
    }
}
