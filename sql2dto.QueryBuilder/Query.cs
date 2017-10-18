using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public abstract class Query
    {
        private List<Param> _parameters;
        private StringBuilder _sb;

        public DatabaseType DatabaseType { get; private set; }

        protected Query(DatabaseType databaseType)
        {
            DatabaseType = databaseType;
            _parameters = new List<Param>();
            _sb = new StringBuilder();
        }

        internal Query Parent { get; set; }

        internal void AddSqlPart(string sqlPart)
        {
            _sb.Append(sqlPart);
        }

        protected static SelectClause SelectAll(Query query)
        {
            return new SelectClause(query).All();
        }

        protected static SelectClause SelectColumns(Query query, string tableAlias, params string[] columnNames)
        {
            return new SelectClause(query).Columns(tableAlias, columnNames);
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
            return _sb.ToString().Trim();
        }

        public Query UsingParameter(string parameterName, object value)
        {
            _parameters.Add(new Param
            {
                Name = parameterName,
                Value = value
            });
            return this;
        }

        public Query UsingParametersRange(params (string, object)[] tuples)
        {
            foreach (var tuple in tuples)
            {
                _parameters.Add(new Param
                {
                    Name = tuple.Item1,
                    Value = tuple.Item2
                });
            }
            return this;
        }

        public Query UsingParametersRange(params (string, object, DataType)[] tuples)
        {
            foreach (var tuple in tuples)
            {
                _parameters.Add(new Param
                {
                    Name = tuple.Item1,
                    Value = tuple.Item2,
                    DataType = tuple.Item3
                });
            }
            return this;
        }
    }
}
