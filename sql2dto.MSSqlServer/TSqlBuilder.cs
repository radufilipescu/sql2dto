using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sql2dto.MSSqlServer
{
    public class TSqlBuilder : SqlBuilder
    {
        public static readonly TSqlBuilder Instance = new TSqlBuilder();

        private TSqlBuilder() { }

        public override string BuildExpressionString(SqlExpression expression, string expressionAlias = null)
        {
            var type = expression.GetExpressionType();
            switch (type)
            {
                case SqlExpressionType.COLUMN:
                    {
                        var column = (SqlColumn)expression;
                        var result = $"{BuildTableAliasString(column.GetSqlTable())}.{$"[{column.GetColumnName()}]"}";
                        if (!String.IsNullOrWhiteSpace(expressionAlias))
                        {
                            result += $" AS [{expressionAlias}]";
                        }
                        return result;
                    }
                case SqlExpressionType.BINARY:
                    {
                        var binaryExpression = (SqlBinaryExpression)expression;
                        var firstTerm = binaryExpression.GetFirstTerm();
                        var op = binaryExpression.GetOperator();
                        var secondTerm = binaryExpression.GetSecondTerm();

                        string firstTermString = BuildExpressionString(firstTerm);
                        string secondTermString = BuildExpressionString(secondTerm);

                        string result = $"({firstTermString}) {BuildSqlOperatorString(op)} ({secondTermString})";
                        if (!String.IsNullOrWhiteSpace(expressionAlias))
                        {
                            result = $"({result}) AS [{expressionAlias}]";
                        }
                        return result;
                    }
                case SqlExpressionType.FUNCTION_CALL:
                    {
                        var functionCallExpression = (SqlFuncExpression)expression;
                        string result = $"{BuildSqlFuncNameString(functionCallExpression.FunctionName)}({(functionCallExpression.IsDistinct ? "DISTINCT " : "")}{BuildExpressionString(functionCallExpression.InnerExpression)})";
                        if (!String.IsNullOrWhiteSpace(expressionAlias))
                        {
                            result += $" AS [{expressionAlias}]";
                        }
                        return result;
                    }
                default:
                    throw new NotImplementedException($"SqlExpressionType: {type}");
            }
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
    }
}
