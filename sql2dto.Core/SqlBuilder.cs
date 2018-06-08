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
        public abstract SqlInsert InsertInto(SqlTable table);
        public abstract SqlUpdate Update(SqlTable table);
        public abstract SqlDelete DeleteFrom(SqlTable table);

        #region ADO.NET
        public abstract DbConnection Connect(string connectionString);
        public abstract Task<DbConnection> ConnectAsync(string connectionString);
        public abstract DbConnection BuildDbConnection(string connectionString);

        #region COMMAND
        public abstract DbCommand BuildDbCommand(SqlQuery query);
        public abstract DbCommand BuildDbCommand(SqlQuery query, DbConnection connection);
        public abstract DbCommand BuildDbCommand(SqlQuery query, DbConnection connection, DbTransaction transaction);

        public abstract DbCommand BuildDbCommand(SqlInsert insert);
        public abstract DbCommand BuildDbCommand(SqlInsert insert, DbConnection connection);
        public abstract DbCommand BuildDbCommand(SqlInsert insert, DbConnection connection, DbTransaction transaction);

        public abstract DbCommand BuildDbCommand(SqlUpdate update);
        public abstract DbCommand BuildDbCommand(SqlUpdate update, DbConnection connection);
        public abstract DbCommand BuildDbCommand(SqlUpdate update, DbConnection connection, DbTransaction transaction);

        public abstract DbCommand BuildDbCommand(SqlDelete delete);
        public abstract DbCommand BuildDbCommand(SqlDelete delete, DbConnection connection);
        public abstract DbCommand BuildDbCommand(SqlDelete delete, DbConnection connection, DbTransaction transaction);
        #endregion

        #region EXEC
        public abstract ReadHelper ExecReadHelper(SqlQuery query, DbConnection connection);
        public abstract ReadHelper ExecReadHelper(SqlQuery query, DbConnection connection, DbTransaction transaction);
        public abstract Task<ReadHelper> ExecReadHelperAsync(SqlQuery query, DbConnection connection);
        public abstract Task<ReadHelper> ExecReadHelperAsync(SqlQuery query, DbConnection connection, DbTransaction transaction);

        public abstract int ExecInsert(SqlInsert insert, DbConnection connection);
        public abstract int ExecInsert(SqlInsert insert, DbConnection connection, DbTransaction transaction);
        public abstract Task<int> ExecInsertAsync(SqlInsert insert, DbConnection connection);
        public abstract Task<int> ExecInsertAsync(SqlInsert insert, DbConnection connection, DbTransaction transaction);

        public abstract int ExecUpdate(SqlUpdate update, DbConnection connection);
        public abstract int ExecUpdate(SqlUpdate update, DbConnection connection, DbTransaction transaction);
        public abstract Task<int> ExecUpdateAsync(SqlUpdate update, DbConnection connection);
        public abstract Task<int> ExecUpdateAsync(SqlUpdate update, DbConnection connection, DbTransaction transaction);

        public abstract int ExecDelete(SqlDelete delete, DbConnection connection);
        public abstract int ExecDelete(SqlDelete delete, DbConnection connection, DbTransaction transaction);
        public abstract Task<int> ExecDeleteAsync(SqlDelete delete, DbConnection connection);
        public abstract Task<int> ExecDeleteAsync(SqlDelete delete, DbConnection connection, DbTransaction transaction);
        #endregion
        #endregion

        #region SELECT
        public abstract string BuildExpressionString(IDbParametersBag parametersBag, SqlExpression expression, string expressionAlias = null);
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
        public abstract string BuildSqlWindowFrameBoundString(SqlWindowFrameBound windowFrameBound);
        #endregion

        #region INSERT
        public abstract string BuildInsertString(SqlInsert insert);
        #endregion

        #region UPDATE
        public abstract string BuildUpdateString(SqlUpdate update);
        #endregion

        #region DELETE
        public abstract string BuildDeleteString(SqlDelete delete);
        #endregion

        public abstract string EscapeConstantValue(string value);
        
        public abstract IReadHelperSettings ReadHelperSettings { get; }
    }
}
