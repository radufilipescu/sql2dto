using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public class Query
    {
        private StringBuilder _sb;

        internal Query()
        {
            _sb = new StringBuilder();
        }

        internal Query Parent { get; set; }

        internal void AddSqlPart(string sqlPart)
        {
            _sb.Append(sqlPart);
        }

        public static SelectClause SelectAll()
        {
            var q = new Query();
            return new SelectClause(q).All();
        }

        public static SelectClause SelectColumns(string tableAlias, params string[] columnNames)
        {
            var q = new Query();
            return new SelectClause(q).Columns(tableAlias, columnNames);
        }

        public FromClause From(string tableName)
        {
            return new FromClause(this, tableName);
        }

        public JoinClause Join(string tableName)
        {
            return new JoinClause(this, tableName);
        }

        public SqlExpressionBuilder Where(params string[] path)
        {
            return new WhereClause(this).ToExpressionBuilder(path);
        }

        public SqlExpressionBuilder WhereSub()
        {
            return new WhereClause(this).ToExpressionBuilderSub();
        }

        public SqlExpressionBuilder WhereSub(params string[] path)
        {
            return new WhereClause(this).ToExpressionBuilderSub(path);
        }

        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
