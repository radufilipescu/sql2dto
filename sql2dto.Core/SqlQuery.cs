using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace sql2dto.Core
{
    public class SqlQuery : SqlTabularSource
    {
        public SqlQuery(SqlBuilder builder)
        {
            _builder = builder;
            _dbParameters = new Dictionary<string, DbParameter>();
            _selectExpressions = new List<(SqlExpression, string)>();
            _selectExpresionAliasesToIndexes = new Dictionary<string, int>();
            _fromAndJoinClauses = new List<(SqlJoinType, SqlTabularSource, SqlExpression)>();
            _whereExpression = null;
            _groupByExpressions = new List<SqlExpression>();
            _havingExpression = null;
            _orderByExpressions = new List<(SqlExpression, SqlOrderByDirection)>();
            _commonTableExpressions = new List<(string, SqlQuery, HashSet<string>)>();
        }

        private SqlBuilder _builder;
        public string GetSqlBuilderLanguageImplementation() => _builder.GetLanguageImplementation();
        public IReadHelperSettings GetReadHelperSettings() => _builder.ReadHelperSettings;

        private Dictionary<string, DbParameter> _dbParameters;
        public Dictionary<string, DbParameter> GetDbParameters() => _dbParameters;
        public void AddDbParameterIfNotFound(DbParameter parameter)
        {
            if (!_dbParameters.ContainsKey(parameter.ParameterName))
            {
                _dbParameters.Add(parameter.ParameterName, parameter);
            }
        }

        private string _queryAlias;
        public override string GetAlias() => _queryAlias;

        private List<(SqlExpression, string)> _selectExpressions;
        public List<(SqlExpression, string)> GetSelectExpressions() => _selectExpressions;

        private Dictionary<string, int> _selectExpresionAliasesToIndexes;
        private void AddSelectExpression(SqlExpression sqlExpression, string alias = null)
        {
            if (!String.IsNullOrWhiteSpace(alias))
            {
                if (_selectExpresionAliasesToIndexes.TryGetValue(alias, out int foundIndex))
                {
                    _selectExpressions[foundIndex] = (sqlExpression, alias);
                }
                else
                {
                    _selectExpressions.Add((sqlExpression, alias));
                    _selectExpresionAliasesToIndexes.Add(alias, _selectExpressions.Count - 1);
                }
            }
            else
            {
                _selectExpressions.Add((sqlExpression, null));
            }
        }

        private List<(SqlJoinType, SqlTabularSource, SqlExpression)> _fromAndJoinClauses;
        public List<(SqlJoinType, SqlTabularSource, SqlExpression)> GetFromAndJoinClauses() => _fromAndJoinClauses;

        private SqlExpression _whereExpression;
        public SqlExpression GetWhereExpression() => _whereExpression;

        public List<SqlExpression> _groupByExpressions;
        public List<SqlExpression> GetGroupByExpressions() => _groupByExpressions;

        public SqlExpression _havingExpression;
        public SqlExpression GetHavingExpressions() => _havingExpression;

        private List<(SqlExpression, SqlOrderByDirection)> _orderByExpressions;
        public List<(SqlExpression, SqlOrderByDirection)> GetOrderByExpressions() => _orderByExpressions;

        private List<(string, SqlQuery, HashSet<string>)> _commonTableExpressions;
        public List<(string, SqlQuery, HashSet<string>)> GetCommonTableExpressions() => _commonTableExpressions;

        public sealed override SqlTabularSourceType TabularType => SqlTabularSourceType.QUERY;

        #region SELECT
        public SqlQuery Select(SqlExpression expression, string columnAlias = null)
        {
            if (String.IsNullOrEmpty(columnAlias))
            {
                AddSelectExpression(expression, null);
            }
            else
            {
                AddSelectExpression(expression, columnAlias);
            }
            
            return this;
        }

        public SqlQuery Select(params SqlExpression[] selectExpressions)
        {
            foreach (var expression in selectExpressions)
            {
                AddSelectExpression(expression, null);
            }
            return this;
        }

        public SqlQuery Select(params (SqlExpression, string)[] selectExpressions)
        {
            foreach (var expression in selectExpressions)
            {
                AddSelectExpression(expression.Item1, expression.Item2);
            }
            return this;
        }
        #endregion

        #region TABLE DIRECT PROJECTION
        public SqlQuery Project<TDto>(SqlTable table, params SqlColumn[] exceptColumns)
             where TDto : new()
        {
            return Project<TDto>(mapper: null, columnsPrefix: DtoMapper<TDto>.DefaultColumnsPrefix, table: table, exceptColumns: exceptColumns);
        }

        public SqlQuery Project<TDto>(string columnsPrefix, SqlTable table, params SqlColumn[] exceptColumns)
             where TDto : new()
        {
            return Project<TDto>(mapper: null, columnsPrefix: columnsPrefix, table: table, exceptColumns: exceptColumns);
        }

        public SqlQuery Project<TDto>(DtoMapper<TDto> mapper, SqlTable table, params SqlColumn[] exceptColumns)
            where TDto : new()
        {
            return Project<TDto>(mapper: mapper, columnsPrefix: DtoMapper<TDto>.DefaultColumnsPrefix, table: table, exceptColumns: exceptColumns);
        }

        public SqlQuery Project<TDto>(DtoMapper<TDto> mapper, string columnsPrefix, SqlTable table, params SqlColumn[] exceptColumns)
            where TDto : new()
        {
            var except = new HashSet<string>(exceptColumns.Select(col => col.GetColumnName()));
            foreach (var col in table.ListAllColumns())
            {
                if (except.Contains(col.GetColumnName()))
                {
                    continue;
                }

                string propName = col.GetPropertyName();
                if (mapper == null)
                {
                    if (DtoMapper<TDto>.TryGetDefaultInnerPropMapConfig(propName, out PropMapConfig propMapConfig))
                    {
                        AddSelectExpression(col, $"{columnsPrefix}{(propMapConfig.ColumnName ?? propName)}");
                    }
                }
                else
                {
                    if (mapper.TryGetInnerPropMapConfig(propName, out PropMapConfig propMapConfig))
                    {
                        AddSelectExpression(col, $"{columnsPrefix}{(propMapConfig.ColumnName ?? propName)}");
                    }
                }
            }
            return this;
        }

        #endregion

        #region EXPRESSION PROJECTION
        public SqlQuery Project<TDto>(params (SqlExpression, Expression<Func<TDto, object>>)[] projectExpressions)
            where TDto : new()
        {
            var expressions = projectExpressions.Select(item => (item.Item1, InternalUtils.GetPropertyName(item.Item2))).ToArray();
            return Project<TDto>(mapper: null, columnsPrefix: DtoMapper<TDto>.DefaultColumnsPrefix, projectExpressions: expressions);
        }

        public SqlQuery Project<TDto>(params (SqlExpression, string)[] projectExpressions)
            where TDto : new()
        {
            return Project<TDto>(mapper: null, columnsPrefix: DtoMapper<TDto>.DefaultColumnsPrefix, projectExpressions: projectExpressions);
        }

        public SqlQuery Project<TDto>(DtoMapper<TDto> mapper, params (SqlExpression, Expression<Func<TDto, object>>)[] projectExpressions)
            where TDto : new()
        {
            var expressions = projectExpressions.Select(item => (item.Item1, InternalUtils.GetPropertyName(item.Item2))).ToArray();
            return Project<TDto>(mapper: mapper, columnsPrefix: DtoMapper<TDto>.DefaultColumnsPrefix, projectExpressions: expressions);
        }

        public SqlQuery Project<TDto>(DtoMapper<TDto> mapper, params (SqlExpression, string)[] projectExpressions)
            where TDto : new()
        {
            return Project<TDto>(mapper: mapper, columnsPrefix: DtoMapper<TDto>.DefaultColumnsPrefix, projectExpressions: projectExpressions);
        }

        public SqlQuery Project<TDto>(string columnsPrefix, params (SqlExpression, Expression<Func<TDto, object>>)[] projectExpressions)
            where TDto : new()
        {
            var expressions = projectExpressions.Select(item => (item.Item1, InternalUtils.GetPropertyName(item.Item2))).ToArray();
            return Project<TDto>(mapper: null, columnsPrefix: columnsPrefix, projectExpressions: expressions);
        }

        public SqlQuery Project<TDto>(string columnsPrefix, params (SqlExpression, string)[] projectExpressions)
            where TDto : new()
        {
            return Project<TDto>(mapper: null, columnsPrefix: columnsPrefix, projectExpressions: projectExpressions);
        }

        public SqlQuery Project<TDto>(DtoMapper<TDto> mapper, string columnsPrefix, params (SqlExpression, Expression<Func<TDto, object>>)[] projectExpressions)
            where TDto : new()
        {
            var expressions = projectExpressions.Select(item => (item.Item1, InternalUtils.GetPropertyName(item.Item2))).ToArray();
            return Project<TDto>(mapper: mapper, columnsPrefix: columnsPrefix, projectExpressions: expressions);
        }

        public SqlQuery Project<TDto>(DtoMapper<TDto> mapper, string columnsPrefix, params (SqlExpression, string)[] projectExpressions)
            where TDto : new()
        {
            if (projectExpressions.Length == 0)
            {
                throw new ArgumentException("No select expressions found", nameof(projectExpressions));
            }

            foreach (var tuple in projectExpressions)
            {
                if (mapper == null)
                {
                    AddSelectExpression(tuple.Item1, $"{columnsPrefix}{(DtoMapper<TDto>.GetDefaultInnerPropMapConfig(tuple.Item2).ColumnName ?? tuple.Item2)}");
                }
                else
                {
                    AddSelectExpression(tuple.Item1, $"{columnsPrefix}{(mapper.GetInnerPropMapConfig(tuple.Item2).ColumnName ?? tuple.Item2)}");
                }
            }
            return this;
        }
        #endregion

        public override SqlColumn GetColumn(string columnNameOrAlias)
        {
            var col = new SqlColumn(this, null, columnNameOrAlias);
            return col;
        }

        public SqlQuery As(string subqueryAlias)
        {
            _queryAlias = subqueryAlias;
            return this;
        }

        public SqlQuery From(SqlTabularSource fromPart)
        {
            _fromAndJoinClauses.Add((SqlJoinType.NONE, fromPart, (SqlExpression)null));
            return this;
        }

        public SqlQuery Join(SqlTabularSource innerJoinPart, SqlExpression on)
        {
            _fromAndJoinClauses.Add((SqlJoinType.INNER, innerJoinPart, on));
            return this;
        }

        public SqlQuery LeftJoin(SqlTabularSource innerJoinPart, SqlExpression on)
        {
            _fromAndJoinClauses.Add((SqlJoinType.LEFT, innerJoinPart, on));
            return this;
        }

        public SqlQuery RightJoin(SqlTabularSource innerJoinPart, SqlExpression on)
        {
            _fromAndJoinClauses.Add((SqlJoinType.RIGHT, innerJoinPart, on));
            return this;
        }

        public SqlQuery FullJoin(SqlTabularSource innerJoinPart, SqlExpression on)
        {
            _fromAndJoinClauses.Add((SqlJoinType.FULL, innerJoinPart, on));
            return this;
        }

        public SqlQuery CrossJoin(SqlTabularSource innerJoinPart)
        {
            _fromAndJoinClauses.Add((SqlJoinType.CROSS, innerJoinPart, (SqlExpression)null));
            return this;
        }

        public SqlQuery Where(SqlExpression condition)
        {
            _whereExpression = condition;
            return this;
        }

        public SqlQuery GroupBy(params SqlExpression[] by)
        {
            _groupByExpressions.AddRange(by);
            return this;
        }

        public SqlQuery Having(SqlExpression having)
        {
            _havingExpression = having;
            return this;
        }

        public SqlQuery OrderBy(params SqlExpression[] by)
        {
            _orderByExpressions.AddRange(by.Select(item => (item, SqlOrderByDirection.ASCENDING)));
            return this;
        }

        public SqlQuery OrderByDescending(params SqlExpression[] by)
        {
            _orderByExpressions.AddRange(by.Select(item => (item, SqlOrderByDirection.DESCENDING)));
            return this;
        }

        public SqlQuery With(string expressionName, Func<SqlQuery> buildQuery, params string[] columnNames)
        {
            var query = buildQuery();
            _commonTableExpressions.Add((expressionName, query, new HashSet<string>(columnNames)));
            return this;
        }

        public string BuildQueryString()
        {
            return _builder.BuildQueryString(this);
        }

        public DbCommand BuildDbCommand()
        {
            return _builder.BuildDbCommand(this);
        }

        public DbCommand BuildDbCommand(DbConnection connection)
        {
            return _builder.BuildDbCommand(this, connection);
        }

        public DbCommand BuildDbCommand(DbConnection connection, DbTransaction transaction)
        {
            return _builder.BuildDbCommand(this, connection, transaction);
        }

        public ReadHelper ExecReadHelper(DbConnection connection)
        {
            return _builder.ExecReadHelper(this, connection);
        }

        public ReadHelper ExecReadHelper(DbConnection connection, DbTransaction transaction)
        {
            return _builder.ExecReadHelper(this, connection, transaction);
        }

        public async Task<ReadHelper> ExecReadHelperAsync(DbConnection connection)
        {
            return await _builder.ExecReadHelperAsync(this, connection);
        }

        public async Task<ReadHelper> ExecReadHelperAsync(DbConnection connection, DbTransaction transaction)
        {
            return await _builder.ExecReadHelperAsync(this, connection, transaction);
        }
    }
}
