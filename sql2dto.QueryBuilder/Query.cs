using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace sql2dto.QueryBuilder
{
    public abstract class Query<TQueryImpl> 
        where TQueryImpl : Query<TQueryImpl>
    {
        protected List<Param> _parameters;
        private StringBuilder _sb;

        public DatabaseType DatabaseType { get; private set; }
        public List<Param> Parameters
        {
            get
            {
                return new List<Param>(_parameters);
            }
        }

        public string ColumnPrefixTableAliasFormat { get; private set; } = "{0}_";

        protected Query(DatabaseType databaseType)
        {
            DatabaseType = databaseType;
            _parameters = new List<Param>();
            _sb = new StringBuilder();
        }

        internal TQueryImpl Parent { get; set; }

        internal void AddSqlPart(string sqlPart)
        {
            _sb.Append(sqlPart);
        }

        protected static SelectClause<TQueryImpl> SelectAll(TQueryImpl query)
        {
            return new SelectClause<TQueryImpl>(query).All();
        }

        protected static SelectClause<TQueryImpl> SelectColumns(TQueryImpl query, string tableAlias, params string[] columnNames)
        {
            return new SelectClause<TQueryImpl>(query).Columns(tableAlias, columnNames);
        }

        public FromClause<TQueryImpl> From(string tableName)
        {
            return new FromClause<TQueryImpl>((TQueryImpl)this, tableName);
        }

        public JoinClause<TQueryImpl> Join(string tableName)
        {
            return new JoinClause<TQueryImpl>((TQueryImpl)this, tableName);
        }

        public SqlExpressionBuilder<TQueryImpl> Where(params string[] path)
        {
            return new WhereClause<TQueryImpl>((TQueryImpl)this).ToExpressionBuilder(path);
        }

        public SqlExpressionBuilder<TQueryImpl> WhereSub()
        {
            return new WhereClause<TQueryImpl>((TQueryImpl)this).ToExpressionBuilderSub();
        }

        public SqlExpressionBuilder<TQueryImpl> WhereSub(params string[] path)
        {
            return new WhereClause<TQueryImpl>((TQueryImpl)this).ToExpressionBuilderSub(path);
        }

        public override string ToString()
        {
            return _sb.ToString().Trim();
        }

        public TQueryImpl UsingParameter(string parameterName, object value)
        {
            _parameters.Add(new Param(parameterName, value));
            return (TQueryImpl)this;
        }

        public TQueryImpl UsingParameter(string parameterName, object value, DataType dataType)
        {
            _parameters.Add(new Param(parameterName, value, dataType));
            return (TQueryImpl)this;
        }

        public TQueryImpl UsingParametersRange(params (string, object)[] tuples)
        {
            foreach (var tuple in tuples)
            {
                _parameters.Add(new Param(tuple.Item1, tuple.Item2));
            }
            return (TQueryImpl)this;
        }

        public TQueryImpl UsingParametersRange(params (string, object, DataType)[] tuples)
        {
            foreach (var tuple in tuples)
            {
                _parameters.Add(new Param(tuple.Item1, tuple.Item2, tuple.Item3));
            }
            return (TQueryImpl)this;
        }
    }
}
