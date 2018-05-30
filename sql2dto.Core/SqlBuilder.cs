using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace sql2dto.Core
{
    public abstract class SqlBuilder
    {
        public abstract string GetLanguageImplementation();

        public abstract SqlParameterExpression Parameter(string name, object value);
        public abstract SqlQuery Query();

        #region ADO.NET
        public abstract DbConnection Connect(string connectionString);
        public abstract Task<DbConnection> ConnectAsync(string connectionString);
        public abstract DbConnection BuildDbConnection(string connectionString);
        public abstract DbCommand BuildDbCommand(SqlQuery query);
        public abstract DbCommand BuildDbCommand(SqlQuery query, DbConnection connection);
        public abstract DbCommand BuildDbCommand(SqlQuery query, DbConnection connection, DbTransaction transaction);
        public abstract ReadHelper ExecReadHelper(SqlQuery query, DbConnection connection);
        public abstract ReadHelper ExecReadHelper(SqlQuery query, DbConnection connection, DbTransaction transaction);
        public abstract Task<ReadHelper> ExecReadHelperAsync(SqlQuery query, DbConnection connection);
        public abstract Task<ReadHelper> ExecReadHelperAsync(SqlQuery query, DbConnection connection, DbTransaction transaction);
        #endregion

        public abstract string BuildExpressionString(SqlQuery query, SqlExpression expression, string expressionAlias = null);
        public abstract string BuildBooleanString(bool value);
        public abstract string BuildAliasString(SqlTabularSource table);
        public abstract string BuildCTEJoinString(SqlQuery query, SqlCTE cte, SqlJoinType joinType, SqlExpression condition = null);
        public abstract string BuildTableJoinString(SqlQuery query, SqlTable table, SqlJoinType joinType, SqlExpression condition = null);
        public abstract string BuildQueryString(SqlQuery query);
        public abstract string BuildAliasString(SqlQuery query);
        public abstract string BuildQueryJoinString(SqlQuery query, SqlQuery subQuery, SqlJoinType joinType, SqlExpression condition = null);
        public abstract string BuildSqlJoinTypeString(SqlJoinType joinType);
        public abstract string BuildSqlOperatorString(SqlOperator op);
        public abstract string BuildSqlFuncNameString(SqlFunctionName func);
        public abstract string BuildSqlOrderByDirectionString(SqlOrderByDirection direction);
        public abstract string EscapeConstantValue(string value);
        
        public abstract IReadHelperSettings ReadHelperSettings { get; }
    }
}
