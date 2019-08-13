using Oracle.ManagedDataAccess.Client;
using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sql2dto.Oracle
{
    public class PLSqlBuilder : SqlBuilder
    {
        public const string LANGUAGE_IMPLEMENTATION = "PL/SQL";

        public override string GetLanguageImplementation() => LANGUAGE_IMPLEMENTATION;

        public override SqlParameterExpression Parameter(string name, object value)
        {
            return new SqlParameterExpression(new OracleParameter(name, value));
        }

        public override SqlQuery Query()
        {
            return new SqlQuery(this);
        }

        public override SqlFetchQuery<TDto> FetchQuery<TDto>(SqlTable table)
        {
            return new SqlFetchQuery<TDto>(this, table);
        }

        public override SqlInsert InsertInto(SqlTable table)
        {
            return new SqlInsert(this, table);
        }

        public override SqlUpdate Update(SqlTable table)
        {
            return new SqlUpdate(this, table);
        }

        public override SqlDelete DeleteFrom(SqlTable table)
        {
            return new SqlDelete(this, table);
        }

        #region ADO.NET
        public override DbConnection Connect(string connectionString)
        {
            var dbConn = BuildDbConnection(connectionString);
            dbConn.Open();
            return dbConn;
        }

        public override async Task<DbConnection> ConnectAsync(string connectionString)
        {
            var dbConn = BuildDbConnection(connectionString);
            await dbConn.OpenAsync();
            return dbConn;
        }

        public override DbConnection BuildDbConnection(string connectionString)
        {
            var dbConn = new OracleConnection(connectionString);
            return dbConn;
        }

        public override DbCommand BuildDbCommand(SqlQuery query)
        {
            var sqlCommand = new OracleCommand();
            sqlCommand.CommandType = System.Data.CommandType.Text;
            sqlCommand.CommandText = BuildQueryString(query);
            sqlCommand.Parameters.AddRange(query.GetDbParameters().Values.ToArray());
            return sqlCommand;
        }

        public override DbCommand BuildDbCommand(SqlQuery query, DbConnection connection)
        {
            var cmd = BuildDbCommand(query);
            cmd.Connection = connection;
            return cmd;
        }

        public override DbCommand BuildDbCommand(SqlQuery query, DbConnection connection, DbTransaction transaction)
        {
            var cmd = BuildDbCommand(query, connection);
            cmd.Transaction = transaction;
            return cmd;
        }

        public override DbCommand BuildDbCommand(SqlInsert insert)
        {
            var sqlCommand = new OracleCommand();
            sqlCommand.CommandType = System.Data.CommandType.Text;
            sqlCommand.CommandText = BuildInsertString(insert);
            sqlCommand.Parameters.AddRange(insert.GetDbParameters().Values.ToArray());
            return sqlCommand;
        }

        public override DbCommand BuildDbCommand(SqlInsert insert, DbConnection connection)
        {
            var cmd = BuildDbCommand(insert);
            cmd.Connection = connection;
            return cmd;
        }

        public override DbCommand BuildDbCommand(SqlInsert insert, DbConnection connection, DbTransaction transaction)
        {
            var cmd = BuildDbCommand(insert, connection);
            cmd.Transaction = transaction;
            return cmd;
        }

        public override DbCommand BuildDbCommand(SqlUpdate update)
        {
            var sqlCommand = new OracleCommand();
            sqlCommand.CommandType = System.Data.CommandType.Text;
            sqlCommand.CommandText = BuildUpdateString(update);
            sqlCommand.Parameters.AddRange(update.GetDbParameters().Values.ToArray());
            return sqlCommand;
        }

        public override DbCommand BuildDbCommand(SqlUpdate update, DbConnection connection)
        {
            var cmd = BuildDbCommand(update);
            cmd.Connection = connection;
            return cmd;
        }

        public override DbCommand BuildDbCommand(SqlUpdate update, DbConnection connection, DbTransaction transaction)
        {
            var cmd = BuildDbCommand(update, connection);
            cmd.Transaction = transaction;
            return cmd;
        }

        public override DbCommand BuildDbCommand(SqlDelete delete)
        {
            var sqlCommand = new OracleCommand();
            sqlCommand.CommandType = System.Data.CommandType.Text;
            sqlCommand.CommandText = BuildDeleteString(delete);
            sqlCommand.Parameters.AddRange(delete.GetDbParameters().Values.ToArray());
            return sqlCommand;
        }

        public override DbCommand BuildDbCommand(SqlDelete delete, DbConnection connection)
        {
            var cmd = BuildDbCommand(delete);
            cmd.Connection = connection;
            return cmd;
        }

        public override DbCommand BuildDbCommand(SqlDelete delete, DbConnection connection, DbTransaction transaction)
        {
            var cmd = BuildDbCommand(delete, connection);
            cmd.Transaction = transaction;
            return cmd;
        }

        public override ReadHelper ExecReadHelper(SqlQuery query, DbConnection connection)
        {
            using (var cmd = BuildDbCommand(query, connection))
            {
                var reader = cmd.ExecuteReader();
                var readHelper = new ReadHelper(reader, query.GetReadHelperSettings());
                return readHelper;
            }
        }

        public override ReadHelper ExecReadHelper(SqlQuery query, DbConnection connection, DbTransaction transaction)
        {
            using (var cmd = BuildDbCommand(query, connection, transaction))
            {
                var reader = cmd.ExecuteReader();
                var readHelper = new ReadHelper(reader, query.GetReadHelperSettings());
                return readHelper;
            }
        }

        public override async Task<ReadHelper> ExecReadHelperAsync(SqlQuery query, DbConnection connection)
        {
            using (var cmd = BuildDbCommand(query, connection))
            {
                var reader = await cmd.ExecuteReaderAsync();
                var readHelper = new ReadHelper(reader, query.GetReadHelperSettings());
                return readHelper;
            }
        }

        public override async Task<ReadHelper> ExecReadHelperAsync(SqlQuery query, DbConnection connection, DbTransaction transaction)
        {
            using (var cmd = BuildDbCommand(query, connection, transaction))
            {
                var reader = await cmd.ExecuteReaderAsync();
                var readHelper = new ReadHelper(reader, query.GetReadHelperSettings());
                return readHelper;
            }
        }

        public override int ExecInsert(SqlInsert insert, DbConnection connection)
        {
            using (var cmd = BuildDbCommand(insert, connection))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public override int ExecInsert(SqlInsert insert, DbConnection connection, DbTransaction transaction)
        {
            using (var cmd = BuildDbCommand(insert, connection, transaction))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public override async Task<int> ExecInsertAsync(SqlInsert insert, DbConnection connection)
        {
            using (var cmd = BuildDbCommand(insert, connection))
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public override async Task<int> ExecInsertAsync(SqlInsert insert, DbConnection connection, DbTransaction transaction)
        {
            using (var cmd = BuildDbCommand(insert, connection, transaction))
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public override int ExecUpdate(SqlUpdate update, DbConnection connection)
        {
            using (var cmd = BuildDbCommand(update, connection))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public override int ExecUpdate(SqlUpdate update, DbConnection connection, DbTransaction transaction)
        {
            using (var cmd = BuildDbCommand(update, connection, transaction))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public override async Task<int> ExecUpdateAsync(SqlUpdate update, DbConnection connection)
        {
            using (var cmd = BuildDbCommand(update, connection))
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public override async Task<int> ExecUpdateAsync(SqlUpdate update, DbConnection connection, DbTransaction transaction)
        {
            using (var cmd = BuildDbCommand(update, connection, transaction))
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public override int ExecDelete(SqlDelete delete, DbConnection connection)
        {
            using (var cmd = BuildDbCommand(delete, connection))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public override int ExecDelete(SqlDelete delete, DbConnection connection, DbTransaction transaction)
        {
            using (var cmd = BuildDbCommand(delete, connection, transaction))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public override async Task<int> ExecDeleteAsync(SqlDelete delete, DbConnection connection)
        {
            using (var cmd = BuildDbCommand(delete, connection))
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public override async Task<int> ExecDeleteAsync(SqlDelete delete, DbConnection connection, DbTransaction transaction)
        {
            using (var cmd = BuildDbCommand(delete, connection, transaction))
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }
        #endregion

        //TODO (SEE OVER)
        public override string BuildExpressionString(ISqlStatement statement, SqlExpression expression, string expressionAlias = null)
        {
            var type = expression.GetExpressionType();
            string result = null;
            switch (type)
            {
                case SqlExpressionType.COLUMN:
                    {
                        var column = (SqlColumn)expression;

                        if (statement.StatementType == SqlStatementType.SELECT)
                        {
                            result = $"{BuildAliasString(column.GetSqlTabularSource())}.{$"\"{column.GetColumnName()}\""}";
                        }
                        else
                        {
                            result = $"\"{column.GetColumnName()}\"";
                        }
                    }
                    break;
                case SqlExpressionType.FUNCTION_CALL:
                    {
                        var functionCallExpression = (SqlFunctionCallExpression)expression;

                        var functionName = BuildSqlFuncNameString(functionCallExpression.GetFunctionName());
                        if (functionName == null)
                        {
                            functionName = functionCallExpression.GetStringFunctionName()
                                                                 .Replace("'", "")
                                                                 .Replace("\"", "");
                        }

                        var distinct = functionCallExpression.GetIsDistinct() ? "DISTINCT " : "";
                        var parameterExpressionsSB = new StringBuilder();
                        bool isFirstParameter = true;
                        foreach (var paramExpression in functionCallExpression.GetParameterExpressions())
                        {
                            if (!isFirstParameter)
                            {
                                parameterExpressionsSB.Append(", ");
                            }
                            parameterExpressionsSB.Append(BuildExpressionString(statement, paramExpression));
                            isFirstParameter = false;
                        }

                        result = $"{functionName}({distinct}{parameterExpressionsSB.ToString()})";

                        var over = functionCallExpression.GetOverClause();
                        if (over != null)
                        {
                            //TODO
                            result += " OVER(";

                            if (over.PartitionByExpressions.Count > 0)
                            {
                                result += $"PARTITION BY {String.Join(", ", over.PartitionByExpressions.Select(i => BuildExpressionString(statement, i)))}";
                            }

                            if (over.OrderByExpressions.Count > 0)
                            {
                                if (over.PartitionByExpressions.Count > 0)
                                {
                                    result += " ";
                                }

                                result += $"ORDER BY {String.Join(", ", over.OrderByExpressions.Select(item => $"{BuildExpressionString(statement, item.Item1)} {BuildSqlOrderByDirectionString(item.Item2)}"))}";
                            }

                            if (over.WindowingType != SqlWindowingType.NOT_SPECIFIED)
                            {
                                result += $" {over.WindowingType.ToString()}";

                                if (over.WindowingEndFrameBound == null)
                                {
                                    result += $" {BuildSqlWindowFrameBoundString(over.WindowingBeginFrameBound)}";
                                }
                                else
                                {
                                    result += $" BETWEEN {BuildSqlWindowFrameBoundString(over.WindowingBeginFrameBound)} AND {BuildSqlWindowFrameBoundString(over.WindowingEndFrameBound)}";
                                }
                            }

                            result += ")";
                        }
                    }
                    break;
                case SqlExpressionType.CONSTANT:
                    {
                        var constantExpression = (SqlConstantExpression)expression;
                        var constType = constantExpression.GetConstantType();
                        var constRawValue = constantExpression.GetValue();

                        if (constType == SqlConstantType.STRING)
                        {
                            result = EscapeConstantValue(constRawValue);
                        }
                        else if (constType == SqlConstantType.NUMBER)
                        {
                            result = constRawValue;
                        }
                        else if (constType == SqlConstantType.BOOLEAN)
                        {
                            result = BuildBooleanString(constantExpression.BooleanValue.Value);
                        }
                        else
                        {
                            throw new NotImplementedException($"SqlConstantType: {constType}");
                        }
                    }
                    break;
                case SqlExpressionType.BINARY:
                    {
                        var binaryExpression = (SqlBinaryExpression)expression;
                        var firstTerm = binaryExpression.GetFirstTerm();
                        var op = binaryExpression.GetOperator();
                        var secondTerm = binaryExpression.GetSecondTerm();

                        string firstTermString = BuildExpressionString(statement, firstTerm);
                        switch (firstTerm.GetExpressionType())
                        {
                            case SqlExpressionType.BINARY:
                                {
                                    firstTermString = $"({firstTermString})";
                                }
                                break;
                        }
                        string secondTermString = BuildExpressionString(statement, secondTerm);
                        switch (secondTerm.GetExpressionType())
                        {
                            case SqlExpressionType.BINARY:
                                {
                                    secondTermString = $"({secondTermString})";
                                }
                                break;
                        }

                        result = $"{firstTermString} {BuildSqlOperatorString(op)} {secondTermString}";

                        if ((op == SqlOperator.LIKE || op == SqlOperator.NOT_LIKE)
                            && binaryExpression.Metadata.TryGetValue("ESCAPE", out string likeEscapeChar))
                        {
                            result += $" ESCAPE '{likeEscapeChar}'";
                        }
                    }
                    break;
                case SqlExpressionType.CASE_WHEN:
                    {
                        var caseWhenExpression = (SqlCaseWhenExpression)expression;
                        var onExpression = caseWhenExpression.GetOnExpression();
                        var elseExpression = caseWhenExpression.GetElseExpression();

                        var onExpressionString = onExpression is null ? "" : $" {BuildExpressionString(statement, onExpression)}";
                        result = $"CASE{onExpressionString}";
                        foreach (var whenThenExpression in caseWhenExpression.GetWhenThenExpressions())
                        {
                            result += $" WHEN {BuildExpressionString(statement, whenThenExpression.Item1)} THEN {BuildExpressionString(statement, whenThenExpression.Item2)}";
                        }
                        if (!(elseExpression is null))
                        {
                            result += $" ELSE {BuildExpressionString(statement, elseExpression)}";
                        }
                        result += " END";
                    }
                    break;
                case SqlExpressionType.LIKE:
                    {
                        var likeExpression = (SqlLikeExpression)expression;
                        var inputExpression = likeExpression.GetInputExpression();
                        var patternExpression = likeExpression.GetPatternExpression();
                        result = $"{BuildExpressionString(statement, inputExpression)} LIKE {BuildExpressionString(statement, patternExpression)}";
                    }
                    break;
                case SqlExpressionType.PARAMETER:
                    {
                        var parameterExpression = (SqlParameterExpression)expression;
                        var dbParameter = parameterExpression.GetDbParameter();
                        if (!(dbParameter is OracleParameter))
                        {
                            throw new InvalidOperationException("Expected Oracle.ManagedDataAccess.Client.OracleParameter");
                        }

                        result = dbParameter.ParameterName;
                        statement.AddDbParameterIfNotFound(dbParameter);
                        if (!result.StartsWith(":"))
                        {
                            result = $":{result}";
                        }
                    }
                    break;
                case SqlExpressionType.IS_NULL:
                    {
                        var isNullExpression = (SqlIsNullExpression)expression;
                        var innerExpression = isNullExpression.GetInnerExpression();
                        result = $"{BuildExpressionString(statement, innerExpression)} IS NULL";
                    }
                    break;
                case SqlExpressionType.CAST:
                    {
                        var castExpression = (SqlCastExpression)expression;
                        var expressionToCast = castExpression.GetExpressionToCast();

                        string sqlTypeString = castExpression.GetSqlTypeString()
                                                             .Replace("\"", "");

                        result = $"CAST({BuildExpressionString(statement, expressionToCast)} AS {sqlTypeString})";
                    }
                    break;
                case SqlExpressionType.SUB_QUERY:
                    {
                        var subQuery = (SqlQuery)expression;
                        if (subQuery.GetCommonTableExpressions().Count > 0)
                        {
                            throw new InvalidOperationException("CTEs are not allowed inside subqueries");
                        }
                        string subQueryString = BuildQueryString(subQuery);
                        subQueryString = subQueryString
                                            .Replace($"{Environment.NewLine}    ", " ")
                                            .Replace(Environment.NewLine, " ");
                        result = $"({subQueryString})";
                    }
                    break;
                case SqlExpressionType.NOT:
                    {
                        var notExpression = (SqlNotExpression)expression;
                        result = $"NOT {BuildExpressionString(statement, notExpression.GetInnerExpression())}";
                    }
                    break;
                case SqlExpressionType.IN:
                case SqlExpressionType.EXISTS:
                case SqlExpressionType.ANY:
                case SqlExpressionType.ALL:
                    {
                        var expr = (IGetInnerExpressionsList)expression;
                        bool isFirstExpression = true;
                        var expressionsListSB = new StringBuilder();
                        foreach (var expressionItem in expr.GetInnerExpressionsList())
                        {
                            if (!isFirstExpression)
                            {
                                expressionsListSB.Append(", ");
                            }
                            expressionsListSB.Append(BuildExpressionString(statement, expressionItem));
                            isFirstExpression = false;
                        }

                        if (type == SqlExpressionType.IN)
                        {
                            var inExpr = (SqlInExpression)expression;
                            var notString = inExpr.GetIsNotIn() ? "NOT " : "";
                            result = $"{BuildExpressionString(statement, inExpr.GetCheckExpression())} {notString}{type.ToString()} ({expressionsListSB.ToString()})";
                        }
                        else
                        {
                            result = $"{type.ToString()} ({expressionsListSB.ToString()})";
                        }
                    }
                    break;
                case SqlExpressionType.TABLE:
                case SqlExpressionType.CTE:
                    throw new InvalidOperationException("Invalid use of TABLE or CTE as sql expression");

                case SqlExpressionType.TUPLE:
                default:
                    throw new NotImplementedException($"SqlExpressionType: {type}");
            }

            if (!String.IsNullOrWhiteSpace(expressionAlias))
            {
                result += $" \"{expressionAlias}\"";
            }

            return result;
        }

        public override string BuildBooleanString(bool value)
        {
            return value ?
                ReadHelperSettings.BooleanTranslator.BooleanTrueStringExpression
                : ReadHelperSettings.BooleanTranslator.BooleanFalseStringExpression;
        }

        public override string BuildAliasString(SqlTabularSource tabularSource)
        {
            return $"\"{tabularSource.GetAlias()}\"";
        }

        public override string BuildCTEJoinString(SqlQuery query, SqlCTE cte, SqlJoinType joinType, SqlExpression condition = null)
        {
            string result = $"{BuildSqlJoinTypeString(joinType)} \"{cte.GetAlias()}\"";
            if (!(condition is null))
            {
                result += $" ON {BuildExpressionString(query, condition)}";
            }
            return result;
        }

        public override string BuildTableJoinString(SqlQuery query, SqlTable table, SqlJoinType joinType, SqlExpression condition = null)
        {
            string tableSchemaDot = String.IsNullOrEmpty(table.GetTableSchema()) ? "" : $"\"{table.GetTableSchema()}\".";
            string result = $"{BuildSqlJoinTypeString(joinType)} {tableSchemaDot}\"{table.GetTableName()}\" \"{table.GetAlias()}\"";
            if (!(condition is null))
            {
                result += $" ON {BuildExpressionString(query, condition)}";
            }
            return result;
        }

        public override string BuildQueryString(SqlQuery query)
        {
            var fromAndJoinClauses = query.GetFromAndJoinClauses();

            if (fromAndJoinClauses.Count > 1
                && fromAndJoinClauses[0].Item1 != SqlJoinType.NONE)
            {
                throw new InvalidOperationException("FROM clause must be specified before any JOINS");
            }

            var sb = new StringBuilder();
            var ctes = query.GetCommonTableExpressions();
            if (ctes.Count > 0)
            {
                sb.Append("WITH ");
                for (int i = 0; i < ctes.Count; i++)
                {
                    var cte = ctes[i];

                    sb.Append($"\"{cte.Item1}\"");
                    sb.AppendLine($" ({String.Join(", ", cte.Item3.Select(col => $"\"{col}\""))}) ");
                    sb.AppendLine(" AS ");
                    sb.AppendLine("(");
                    sb.AppendLine(BuildQueryString(cte.Item2));
                    sb.Append(")");

                    if (i != ctes.Count - 1)
                    {
                        sb.AppendLine(",");
                    }
                    else
                    {
                        sb.AppendLine();
                    }

                    foreach (var cteDbParam in cte.Item2.GetDbParameters().Values)
                    {
                        query.AddDbParameterIfNotFound(cteDbParam);
                    }
                }
            }

            if (query.GetIsDistinct())
            {
                sb.AppendLine("SELECT DISTINCT");
            }
            else
            {
                sb.AppendLine("SELECT");
            }

            sb.Append("    ");
            sb.AppendLine(String.Join($@",{Environment.NewLine}    ", query.GetSelectExpressions().Select(e => BuildExpressionString(query, e.Item1, e.Item2))));

            var fromAndJoins = query.GetFromAndJoinClauses().ToList(); // without ToList() you would get the same List reference as query has, and inserting into that one would be bad
            if (fromAndJoins.Count == 0)
            {
                // missing FROM DUAL
                var dual = new DUAL();
                fromAndJoins.Insert(0, (SqlJoinType.NONE, dual, (SqlExpression)null));
            }

            sb.AppendLine(String.Join(Environment.NewLine, fromAndJoins.Select(fj =>
            {
                switch (fj.Item2.TabularType)
                {
                    case SqlTabularSourceType.TABLE:
                        return BuildTableJoinString(query, (SqlTable)fj.Item2, fj.Item1, fj.Item3);
                    case SqlTabularSourceType.QUERY:
                        return BuildQueryJoinString(query, (SqlQuery)fj.Item2, fj.Item1, fj.Item3);
                    case SqlTabularSourceType.CTE:
                        return BuildCTEJoinString(query, (SqlCTE)fj.Item2, fj.Item1, fj.Item3);
                    default:
                        throw new NotImplementedException($"SqlTabularSourceType: {fj.Item2.TabularType}");
                }
            })));

            var whereExpression = query.GetWhereExpression();
            if (!(whereExpression is null))
            {
                sb.AppendLine($"WHERE {BuildExpressionString(query, whereExpression)}");
            }

            var groupByExpressions = query.GetGroupByExpressions();
            if (groupByExpressions.Count > 0)
            {
                sb.AppendLine($"GROUP BY {String.Join(", ", groupByExpressions.Select(item => BuildExpressionString(query, item)))}");
            }

            var havingExpression = query.GetHavingExpressions();
            if (!(havingExpression is null))
            {
                sb.AppendLine($"HAVING {String.Join(", ", BuildExpressionString(query, havingExpression))}");
            }

            var orderByExpressions = query.GetOrderByExpressions();
            if (orderByExpressions.Count > 0)
            {
                sb.AppendLine($"ORDER BY {String.Join(", ", orderByExpressions.Select(item => $"{BuildExpressionString(query, item.Item1)} {BuildSqlOrderByDirectionString(item.Item2)}"))}");
            }

            if (query.GetTakeRowsCount() >= 0)
            {
                int skipRows = query.GetSkipRowsCount();
                int takeRows = query.GetTakeRowsCount();

                if (skipRows > 0)
                {
                    sb.AppendLine($"OFFSET {skipRows} ROWS");
                }

                if (takeRows > 0)
                {
                    sb.AppendLine($"FETCH FIRST {takeRows} ROWS ONLY");
                }
            }

            return sb.ToString().Trim();
        }

        public override string BuildAliasString(SqlQuery query)
        {
            return $"\"{query.GetAlias()}\"";
        }

        public override string BuildQueryJoinString(SqlQuery query, SqlQuery subQuery, SqlJoinType joinType, SqlExpression condition = null)
        {
            string result =
$@"{BuildSqlJoinTypeString(joinType)} 
(
{BuildQueryString(subQuery)}
) {BuildAliasString(subQuery)}";

            if (!(condition is null))
            {
                result += $" ON {BuildExpressionString(query, condition)}";
            }

            foreach (var innerDbParam in subQuery.GetDbParameters().Values)
            {
                query.AddDbParameterIfNotFound(innerDbParam);
            }

            return result;
        }

        public override string BuildSqlJoinTypeString(SqlJoinType joinType)
        {
            switch (joinType)
            {
                case SqlJoinType.NONE:
                    return "FROM";
                case SqlJoinType.INNER:
                    return "INNER JOIN";
                case SqlJoinType.LEFT:
                    return "LEFT OUTER JOIN";
                case SqlJoinType.RIGHT:
                    return "RIGHT OUTER JOIN";
                case SqlJoinType.FULL:
                    return "FULL OUTER JOIN";
                case SqlJoinType.CROSS:
                    return "CROSS JOIN";
                default:
                    throw new NotImplementedException($"SqlJoinType: {joinType}");
            }
        }

        public override string BuildSqlOperatorString(SqlOperator op)
        {
            switch (op)
            {
                case SqlOperator.AND:
                    return "AND";
                case SqlOperator.OR:
                    return "OR";
                case SqlOperator.EQUALS:
                    return "=";
                case SqlOperator.NOT_EQUALS:
                    return "!=";
                case SqlOperator.PLUS:
                    return "+";
                case SqlOperator.MINUS:
                    return "-";
                case SqlOperator.TIMES:
                    return "*";
                case SqlOperator.DIVIDE:
                    return "/";
                case SqlOperator.MOD:
                    return "%";
                case SqlOperator.GREATER_THAN:
                    return ">";
                case SqlOperator.GREATER_OR_EQUAL_THAN:
                    return ">=";
                case SqlOperator.LESS_THAN:
                    return "<";
                case SqlOperator.LESS_OR_EQUAL_THAN:
                    return "<=";
                case SqlOperator.LIKE:
                    return "LIKE";
                case SqlOperator.NOT_LIKE:
                    return "NOT LIKE";
                default:
                    throw new NotImplementedException($"SqlOperator: {op}");
            }
        }

        public override string BuildSqlFuncNameString(SqlFunctionName func)
        {
            switch (func)
            {
                case SqlFunctionName.NONE:
                    return null;
                case SqlFunctionName.SUM:
                    return "SUM";
                case SqlFunctionName.AVERAGE:
                    return "AVG";
                case SqlFunctionName.CONCAT:
                    return "CONCAT";
                default:
                    throw new NotImplementedException($"SqlFunctionName: {func}");
            }
        }

        public override string BuildSqlOrderByDirectionString(SqlOrderByDirection direction)
        {
            switch (direction)
            {
                case SqlOrderByDirection.ASCENDING:
                    return "ASC";
                case SqlOrderByDirection.DESCENDING:
                    return "DESC";
                default:
                    throw new NotImplementedException($"SqlOrderByDirection: {direction}");
            }
        }

        //TODO
        public override string BuildSqlWindowFrameBoundString(SqlWindowFrameBound windowFrameBound)
        {
            string result = "";
            switch (windowFrameBound.WindowFrameType)
            {
                case SqlWindowFrameBoundType.CURRENT_ROW:
                    {
                        result += " CURRENT ROW";
                    }
                    break;
                case SqlWindowFrameBoundType.PRECEDING_UNBOUNDED:
                    {
                        result += " UNBOUNDED PRECEDING";
                    }
                    break;
                case SqlWindowFrameBoundType.PRECEDING_COUNT:
                    {
                        result += $" {windowFrameBound.Count} PRECEDING";
                    }
                    break;
                case SqlWindowFrameBoundType.FOLLOWING_UNBOUNDED:
                    {
                        result += " UNBOUNDED FOLLOWING";
                    }
                    break;
                case SqlWindowFrameBoundType.FOLLOWING_COUNT:
                    {
                        result += $" {windowFrameBound.Count} FOLLOWING";
                    }
                    break;
            }
            return result;
        }

        public override string EscapeConstantValue(string value)
        {
            return $"'{value.Replace("'", "''")}'";
        }

        private IReadHelperSettings _readHelperSettings = new PLSqlReadHelperSettings();
        public override IReadHelperSettings ReadHelperSettings => _readHelperSettings;

        public override string BuildInsertString(SqlInsert insert)
        {
            string colsString = String.Join(", ", insert.ColumnsToSet.Select(colToSet => $"\"{colToSet.Item1.GetColumnName()}\""));
            string setString = String.Join(", ", insert.ColumnsToSet.Select(colToSet => $"{BuildExpressionString(insert, colToSet.Item2)}"));
            return $"INSERT INTO \"{insert.Table.GetTableSchema()}\".\"{insert.Table.GetTableName()}\" ({colsString}) VALUES ({setString})";
        }

        public override string BuildUpdateString(SqlUpdate update)
        {
            string setString = String.Join(", ", update.ColumnsToSet.Select(colToSet => $"\"{colToSet.Item1.GetColumnName()}\" = {BuildExpressionString(update, colToSet.Item2)}"));
            var result = $"UPDATE \"{update.Table.GetTableSchema()}\".\"{update.Table.GetTableName()}\" SET {setString}";
            if (!(update.WhereExpression is null))
            {
                string whereString = BuildExpressionString(update, update.WhereExpression);
                result += $" WHERE {whereString}";
            }
            return result;
        }

        public override string BuildDeleteString(SqlDelete delete)
        {
            string result = $"DELETE FROM \"{delete.Table.GetTableSchema()}\".\"{delete.Table.GetTableName()}\"";
            if (!(delete.WhereExpression is null))
            {
                string whereString = BuildExpressionString(delete, delete.WhereExpression);
                result += $" WHERE {whereString}";
            }
            return result;
        }
    }
}
