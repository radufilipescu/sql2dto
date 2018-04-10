using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace sql2dto.MSSqlServer
{
    public class TSqlBuilder : SqlBuilder
    {
        public const string LANGUAGE_IMPLEMENTATION = "T-SQL";

        public override string GetLanguageImplementation() => LANGUAGE_IMPLEMENTATION;

        public override string BuildExpressionString(SqlQuery query, SqlExpression expression, string expressionAlias = null)
        {
            var type = expression.GetExpressionType();
            string result = null;
            switch (type)
            {
                case SqlExpressionType.COLUMN:
                    {
                        var column = (SqlColumn)expression;

                        result = $"{BuildAliasString(column.GetSqlTable())}.{$"[{column.GetColumnName()}]"}";
                    }
                    break;
                case SqlExpressionType.FUNCTION_CALL:
                    {
                        var functionCallExpression = (SqlFunctionCallExpression)expression;

                        var functionName = BuildSqlFuncNameString(functionCallExpression.GetFunctionName());
                        var distinct = functionCallExpression.GetIsDistinct() ? "DISTINCT " : "";
                        var innerExpression = BuildExpressionString(query, functionCallExpression.GetInnerExpression());

                        result = $"{functionName}({distinct}{innerExpression})";
                    }
                    break;
                case SqlExpressionType.CONSTANT:
                    {
                        var constantExpression = (SqlConstantExpression)expression;

                        result = constantExpression.GetConstantType() == SqlConstantType.NUMBER 
                            ? constantExpression.GetValue() 
                            : $"'{EscapeConstantValue(constantExpression.GetValue())}'";
                    }
                    break;
                case SqlExpressionType.BINARY:
                    {
                        var binaryExpression = (SqlBinaryExpression)expression;
                        var firstTerm = binaryExpression.GetFirstTerm();
                        var op = binaryExpression.GetOperator();
                        var secondTerm = binaryExpression.GetSecondTerm();

                        string firstTermString = BuildExpressionString(query, firstTerm);
                        switch (firstTerm.GetExpressionType())
                        {
                            case SqlExpressionType.BINARY:
                                {
                                    firstTermString = $"({firstTermString})";
                                }
                                break;
                        }
                        string secondTermString = BuildExpressionString(query, secondTerm);
                        switch (secondTerm.GetExpressionType())
                        {
                            case SqlExpressionType.BINARY:
                                {
                                    secondTermString = $"({secondTermString})";
                                }
                                break;
                        }

                        result = $"{firstTermString} {BuildSqlOperatorString(op)} {secondTermString}";
                    }
                    break;
                case SqlExpressionType.CASE_WHEN:
                    {
                        var caseWhenExpression = (SqlCaseWhenExpression)expression;
                        var onExpression = caseWhenExpression.GetOnExpression();
                        var elseExpression = caseWhenExpression.GetElseExpression();

                        var onExpressionString = onExpression is null ? "" : $" {BuildExpressionString(query, onExpression)}";
                        result = $"CASE{onExpressionString}";
                        foreach (var whenThenExpression in caseWhenExpression.GetWhenThenExpressions())
                        {
                            result += $" WHEN {BuildExpressionString(query, whenThenExpression.Item1)} THEN {BuildExpressionString(query, whenThenExpression.Item2)}";
                        }
                        if (!(elseExpression is null))
                        {
                            result += $" ELSE {BuildExpressionString(query, elseExpression)}";
                        }
                        result += " END";
                    }
                    break;
                case SqlExpressionType.LIKE:
                    {
                        var likeExpression = (SqlLikeExpression)expression;
                        var inputExpression = likeExpression.GetInputExpression();
                        var patternExpression = likeExpression.GetPatternExpression();
                        result = $"{BuildExpressionString(query, inputExpression)} LIKE {BuildExpressionString(query, patternExpression)}";
                    }
                    break;
                case SqlExpressionType.PARAMETER:
                    {
                        var parameterExpression = (SqlParameterExpression)expression;
                        var dbParameter = parameterExpression.GetDbParameter();
                        if (!(dbParameter is SqlParameter))
                        {
                            throw new InvalidOperationException("Expected System.Data.SqlClient.SqlParameter");
                        }

                        result = dbParameter.ParameterName;
                        query.AddDbParameterIfNotFound(dbParameter);
                        if (!result.StartsWith("@"))
                        {
                            result = $"@{result}";
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException($"SqlExpressionType: {type}");
            }

            if (!String.IsNullOrWhiteSpace(expressionAlias))
            {
                result += $" AS [{expressionAlias}]";
            }

            return result;
        }

        public override string BuildAliasString(SqlTable table)
        {
            return $"[{table.GetTableAlias()}]";
        }

        public override string BuildTableAsString(SqlQuery query, SqlTable table, SqlJoinType joinType, SqlExpression condition = null)
        {
            string result = $"{BuildSqlJoinTypeString(joinType)} [{table.GetTableSchema()}].[{table.GetTableName()}] AS [{table.GetTableAlias()}]";
            if (!(condition is null))
            {
                result += $" ON {BuildExpressionString(query, condition)}";
            }
            return result;
        }

        public override string BuildQueryString(SqlQuery query)
        {
            var fromAndJoinClauses = query.GetFromAndJoinClauses();
            if (fromAndJoinClauses.Count == 0)
            {
                throw new InvalidOperationException("FROM clause was not specified");
            }
            if (fromAndJoinClauses[0].Item1 != SqlJoinType.NONE)
            {
                throw new InvalidOperationException("FROM clause must be specified before any JOINS");
            }

            var sb = new StringBuilder();
            sb.AppendLine("SELECT");
            sb.Append("    ");
            sb.AppendLine(String.Join($@",{Environment.NewLine}    ", query.GetSelectExpressions().Select(e => BuildExpressionString(query, e.Item1, e.Item2))));
            sb.AppendLine(String.Join(Environment.NewLine, query.GetFromAndJoinClauses().Select(fj =>
            {
                switch (fj.Item2.TabularType)
                {
                    case SqlTabularSourceType.TABLE:
                        return BuildTableAsString(query, (SqlTable)fj.Item2, fj.Item1, fj.Item3);
                    case SqlTabularSourceType.QUERY:
                        return BuildQueryAsString(query, (SqlQuery)fj.Item2, fj.Item1, fj.Item3);
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

            return sb.ToString().Trim();
        }

        public override string BuildAliasString(SqlQuery query)
        {
            return $"[{query.GetQueryAlias()}]";
        }

        public override string BuildQueryAsString(SqlQuery query, SqlQuery subQuery, SqlJoinType joinType, SqlExpression condition = null)
        {
            string result = 
$@"{BuildSqlJoinTypeString(joinType)} 
(
{BuildQueryString(subQuery)}
) AS {BuildAliasString(subQuery)}";

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
                default:
                    throw new NotImplementedException($"SqlOperator: {op}");
            }
        }

        public override string BuildSqlFuncNameString(SqlFunctionName func)
        {
            switch (func)
            {
                case SqlFunctionName.SUM:
                    return "SUM";
                case SqlFunctionName.AVERAGE:
                    return "AVG";
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

        public override string EscapeConstantValue(string value)
        {
            if (Regex.IsMatch(value, "[()[]]"))
            {
                throw new InvalidOperationException($"Constant value not safe: {value}");
            }

            return value.Replace("'", "''"); //TODO
        }

        public override DbCommand BuildDbCommand(SqlQuery query)
        {
            var sqlCommand = new SqlCommand();
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

        public override SqlParameterExpression Parameter(string name, object value)
        {
            return new SqlParameterExpression(new SqlParameter(name, value));
        }
    }
}
