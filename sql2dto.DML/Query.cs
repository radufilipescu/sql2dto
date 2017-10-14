using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public class Query
    {
        private bool _selectAll;
        private Dictionary<string, int> _selectDict;
        private List<List<SelectDetails>> _selectList;

        private string _tableAlias;
        private SqlTable _table;
        private List<JoinDetails> _joinedQueries;

        internal Query(SqlTable table, string tableAlias)
        {
            _selectDict = new Dictionary<string, int>();
            _selectList = new List<List<SelectDetails>>();

            _table = table;
            _tableAlias = tableAlias;
            _joinedQueries = new List<JoinDetails>();
        }

        public Query SelectAll()
        {
            _selectAll = true;
            return this;
        }

        public Query SelectAll(string fromTableAlias)
        {
            if (_selectAll)
            {
                return this;
            }

            if (_selectDict.TryGetValue(fromTableAlias, out int index))
            {
                return this;
            }
            else
            {
                _selectList.Add(null);
                _selectDict.Add(fromTableAlias, _selectList.Count - 1);
            }
            return this;
        }

        public Query Select(string fromTableAlias, params SqlColumn[] columns)
        {
            if (_selectAll)
            {
                return this;
            }

            foreach (var column in columns)
            {
                if (_selectDict.TryGetValue(fromTableAlias, out int index))
                {
                    if (_selectList[index] == null)
                    {
                        continue;
                    }
                    else
                    {
                        _selectList[index].Add(new SelectDetails(null, column, null));
                    }
                }
                else
                {
                    _selectList.Add(new List<SelectDetails> { new SelectDetails(null, column, null) });
                    _selectDict.Add(fromTableAlias, _selectList.Count - 1);
                }
            }

            return this;
        }

        public Query Select(string fromTableAlias, string columnsPrefix, params SqlColumn[] columns)
        {
            if (_selectAll)
            {
                return this;
            }

            foreach (var column in columns)
            {
                if (_selectDict.TryGetValue(fromTableAlias, out int index))
                {
                    if (_selectList[index] == null)
                    {
                        continue;
                    }
                    else
                    {
                        _selectList[index].Add(new SelectDetails(columnsPrefix, column, null));
                    }
                }
                else
                {
                    _selectList.Add(new List<SelectDetails> { new SelectDetails(columnsPrefix, column, null) });
                    _selectDict.Add(fromTableAlias, _selectList.Count - 1);
                }
            }

            return this;
        }

        public Query Select(string fromTableAlias, params (SqlColumn, string)[] aliasedColumns)
        {
            if (_selectAll)
            {
                return this;
            }

            foreach (var column in aliasedColumns)
            {
                if (_selectDict.TryGetValue(fromTableAlias, out int index))
                {
                    if (_selectList[index] == null)
                    {
                        continue;
                    }
                    else
                    {
                        _selectList[index].Add(new SelectDetails(null, column.Item1, column.Item2));
                    }
                }
                else
                {
                    _selectList.Add(new List<SelectDetails> { new SelectDetails(null, column.Item1, null) });
                    _selectDict.Add(fromTableAlias, _selectList.Count - 1);
                }
            }

            return this;
        }

        public static Query From(SqlTable table, string tableAlias)
        {
            return new Query(table, tableAlias);
        }

        public Query Join<TLeft, TRight>(SqlTable joinedTable, string leftTableAlias, SqlColumn leftColumn, SqlOp op, string rightTableAlias, SqlColumn rightColumn)
            where TLeft : ISelectable
            where TRight : ISelectable
        {
            _joinedQueries.Add(new JoinDetails(JoinType.INNER, joinedTable, leftTableAlias, joinExpression, rightTableAlias));
            return this;
        }

        public Query Join<TLeft, TRight>(SqlTable joinedTable, string leftTableAlias, SqlExpression<TLeft, TRight> joinExpression, string rightTableAlias)
            where TLeft : ISelectable
            where TRight : ISelectable
        {
            _joinedQueries.Add(new JoinDetails(JoinType.INNER, joinedTable, leftTableAlias, joinExpression, rightTableAlias));
            return this;
        }

        public Query LeftJoin<TLeft, TRight>(SqlTable joinedTable, string leftTableAlias, SqlExpression<TLeft, TRight> joinExpression, string rightTableAlias)
            where TLeft : ISelectable
            where TRight : ISelectable
        {
            _joinedQueries.Add(new JoinDetails(JoinType.LEFT, joinedTable, leftTableAlias, joinExpression, rightTableAlias));
            return this;
        }

        public Query RightJoin<TLeft, TRight>(SqlTable joinedTable, string leftTableAlias, SqlExpression<TLeft, TRight> joinExpression, string rightTableAlias)
            where TLeft : ISelectable
            where TRight : ISelectable
        {
            _joinedQueries.Add(new JoinDetails(JoinType.RIGHT, joinedTable, leftTableAlias, joinExpression, rightTableAlias));
            return this;
        }

        public Query FullJoin<TLeft, TRight>(SqlTable joinedTable, string leftTableAlias, SqlExpression<TLeft, TRight> joinExpression, string rightTableAlias)
            where TLeft : ISelectable
            where TRight : ISelectable
        {
            _joinedQueries.Add(new JoinDetails(JoinType.FULL, joinedTable, leftTableAlias, joinExpression, rightTableAlias));
            return this;
        }

        public string BuildQuery()
        {
            var sb = new StringBuilder("SELECT");
            if (_selectAll)
            {
                sb.AppendLine();
                sb.Append(" * ");
            }
            else
            {
                var orderedAliases = _selectDict.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
                for (int i = 0; i < _selectList.Count; i++)
                {
                    bool isFirst = i == 0;
                    foreach (var c in _selectList[i])
                    {
                        sb.AppendLine();
                        string alias = null;
                        if (!String.IsNullOrWhiteSpace(c.ColumnPrefix))
                        {
                            alias = $" AS {c.ColumnPrefix}{c.Column.ColumnName}";
                        }
                        else if (!String.IsNullOrWhiteSpace(c.ColumnAlias))
                        {
                            alias = $" AS {c.ColumnAlias}";
                        }
                        else
                        {
                            alias = $" AS {orderedAliases[i]}_{c.Column.ColumnName}";
                        }
                        sb.Append($"{(isFirst ? "" : ", ")}{c.Column.BuildSqlSelect(orderedAliases[i])}");
                        if (!String.IsNullOrWhiteSpace(alias))
                        {
                            sb.Append(alias);
                        }
                    }
                }

                sb.AppendLine();
                sb.Append($" FROM [{_table.TableSchema}].[{_table.TableName}] {_tableAlias}");

                foreach (var j in _joinedQueries)
                {
                    sb.AppendLine();
                    sb.Append($" {j.JoinType} JOIN [{j.JoinedTable.TableSchema}].[{j.JoinedTable.TableName}] {j.LeftTableAlias} ON {j.LeftColumn.BuildSqlSelect(j.LeftTableAlias, j.RightTableAlias)}");
                }
            }
            return sb.ToString();
        }

        public JoinClause Join(SqlTable joinedTable)
        {
            return new JoinClause(this, SqlJoinType.INNER, joinedTable);
        }
    }
}
