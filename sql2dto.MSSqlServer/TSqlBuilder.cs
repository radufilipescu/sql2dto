using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace sql2dto.MSSqlServer
{
    public class TSqlBuilder : SqlBuilder
    {
        public static readonly TSqlBuilder Instance = new TSqlBuilder();

        private TSqlBuilder() { }

        public override string BuildExpressionString(SqlExpression expression, string expressionAlias = null)
        {
            var type = expression.GetExpressionType();
            string result = null;
            switch (type)
            {
                case SqlExpressionType.COLUMN:
                    {
                        var column = (SqlColumn)expression;

                        result = $"{BuildTableAliasString(column.GetSqlTable())}.{$"[{column.GetColumnName()}]"}";
                    }
                    break;
                case SqlExpressionType.FUNCTION_CALL:
                    {
                        var functionCallExpression = (SqlFunctionCallExpression)expression;

                        var functionName = BuildSqlFuncNameString(functionCallExpression.GetFunctionName());
                        var distinct = functionCallExpression.GetIsDistinct() ? "DISTINCT " : "";
                        var innerExpression = BuildExpressionString(functionCallExpression.GetInnerExpression());

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

                        string firstTermString = BuildExpressionString(firstTerm);
                        switch (firstTerm.GetExpressionType())
                        {
                            case SqlExpressionType.BINARY:
                                {
                                    firstTermString = $"({firstTermString})";
                                }
                                break;
                        }
                        string secondTermString = BuildExpressionString(secondTerm);
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

                        var onExpressionString = onExpression is null ? "" : $" {BuildExpressionString(onExpression)}";
                        result = $"CASE{onExpressionString}";
                        foreach (var whenThenExpression in caseWhenExpression.GetWhenThenExpressions())
                        {
                            result += $" WHEN {BuildExpressionString(whenThenExpression.Item1)} THEN {BuildExpressionString(whenThenExpression.Item2)}";
                        }
                        if (!(elseExpression is null))
                        {
                            result += $" ELSE {BuildExpressionString(elseExpression)}";
                        }
                        result += " END";
                    }
                    break;
                case SqlExpressionType.PARAMETER:
                    {
                        var parameterExpression = (SqlParameterExpression)expression;
                        var dbParameter = parameterExpression.GetDbParameter();

                        result = dbParameter.ParameterName;
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

        public override string BuildTableAliasString(SqlTable table)
        {
            return $"[{table.GetTableAlias()}]";
        }

        public override string BuildTableAsAliasString(SqlTable table, SqlJoinType joinType, SqlExpression condition = null)
        {
            string result = $"{BuildSqlJoinTypeString(joinType)} [{table.GetTableSchema()}].[{table.GetTableName()}] AS [{table.GetTableAlias()}]";
            if (!(condition is null))
            {
                result += $" ON {BuildExpressionString(condition)}";
            }
            return result;
        }

        public override string BuildQueryString(SqlQuery query)
        {
            string result =

$@"SELECT
    {String.Join($@",{Environment.NewLine}    ", query.GetSelectExpressions().Select(e => BuildExpressionString(e.Item1, e.Item2)))}
{String.Join(Environment.NewLine, query.GetFromAndJoinClauses().Select(fj =>
{
    switch (fj.Item2.TabularType)
    {
        case SqlTabularSourceType.TABLE:
            return BuildTableAsAliasString((SqlTable)fj.Item2, fj.Item1, fj.Item3);
        case SqlTabularSourceType.QUERY:
            return BuildQueryAsAliasString((SqlQuery)fj.Item2, fj.Item1, fj.Item3);
        default:
            throw new NotImplementedException($"SqlTabularSourceType: {fj.Item2.TabularType}");
    }
}))}
";

            return result;
        }

        public override string BuildQueryAliasString(SqlQuery query)
        {
            return $"[{query.GetQueryAlias()}]";
        }

        public override string BuildQueryAsAliasString(SqlQuery query, SqlJoinType joinType, SqlExpression condition = null)
        {
            string result = $"{BuildSqlJoinTypeString(joinType)} ({BuildQueryString(query)}) AS [{BuildQueryAliasString(query)}]";
            if (!(condition is null))
            {
                result += $" ON {BuildExpressionString(condition)}";
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

        public override string EscapeConstantValue(string value)
        {
            if (Regex.IsMatch(value, "[()[]]"))
            {
                throw new InvalidOperationException($"Constant value not safe: {value}");
            }

            return value.Replace("'", "''"); //TODO
        }
    }
}
