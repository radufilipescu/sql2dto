using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace sql2dto.Core
{
    public class SqlFetchQuery<TDto>
        where TDto : new()
    {
        private readonly SqlBuilder _builder;
        private readonly SqlQuery _sqlQuery;

        #region CONSTRUCTORS
        public SqlFetchQuery(SqlBuilder builder, SqlTable table, params SqlColumn[] exceptColumns)
        {
            _builder = builder;
            _sqlQuery = new SqlQuery(builder);
            _sqlQuery.Project<TDto>(table, exceptColumns);
        }

        public SqlFetchQuery(SqlBuilder builder, DtoMapper<TDto> mapper, SqlTable table, params SqlColumn[] exceptColumns)
        {
            _builder = builder;
            _sqlQuery = new SqlQuery(builder);
            _sqlQuery.Project<TDto>(mapper, table, exceptColumns);
        }

        public SqlFetchQuery(SqlBuilder builder, string columnsPrefix, SqlTable table, params SqlColumn[] exceptColumns)
        {
            _builder = builder;
            _sqlQuery = new SqlQuery(builder);
            _sqlQuery.Project<TDto>(columnsPrefix, table, exceptColumns);
        }

        public SqlFetchQuery(SqlBuilder builder, DtoMapper<TDto> mapper, string columnsPrefix, SqlTable table, params SqlColumn[] exceptColumns)
        {
            _builder = builder;
            _sqlQuery = new SqlQuery(builder);
            _sqlQuery.Project<TDto>(mapper, columnsPrefix, table, exceptColumns);
        }
        #endregion

        #region INCLUDE
        private List<Action<FetchOp<TDto>, ReadHelper>> _includes = new List<Action<FetchOp<TDto>, ReadHelper>>();

        public SqlFetchQuery<TDto> Include<TChildDto>(SqlTable table, Action<TDto, TChildDto> map, Action<FetchOp<TChildDto>> childIncludeConfig = null)
            where TChildDto : new()
        {
            _sqlQuery.Project<TChildDto>(table);
            var include = new Action<FetchOp<TDto>, ReadHelper>((parentFetchOp, helper) =>
            {
                var childFetchOp = FetchOp<TChildDto>.Create(helper);
                childIncludeConfig?.Invoke(childFetchOp);
                parentFetchOp.Include<TChildDto>(childFetchOp, map);
            });
            _includes.Add(include);
            return this;
        }

        public SqlFetchQuery<TDto> Include<TChildDto>(SqlTable table, string columnsPrefix, Action<TDto, TChildDto> map, Action<FetchOp<TChildDto>> childIncludeConfig = null)
            where TChildDto : new()
        {
            _sqlQuery.Project<TChildDto>(columnsPrefix, table);
            var include = new Action<FetchOp<TDto>, ReadHelper>((parentFetchOp, helper) =>
            {
                var childFetchOp = FetchOp<TChildDto>.Create(columnsPrefix, helper);
                childIncludeConfig?.Invoke(childFetchOp);
                parentFetchOp.Include<TChildDto>(childFetchOp, map);
            });
            _includes.Add(include);
            return this;
        }

        public SqlFetchQuery<TDto> Include<TChildDto>(SqlTable table, DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> map, Action<FetchOp<TChildDto>> childIncludeConfig = null)
            where TChildDto : new()
        {
            _sqlQuery.Project<TChildDto>(childMapper, table);
            var include = new Action<FetchOp<TDto>, ReadHelper>((parentFetchOp, helper) =>
            {
                var childFetchOp = FetchOp<TChildDto>.Create(helper, childMapper);
                childIncludeConfig?.Invoke(childFetchOp);
                parentFetchOp.Include<TChildDto>(childFetchOp, map);
            });
            _includes.Add(include);
            return this;
        }

        public SqlFetchQuery<TDto> Include<TChildDto>(SqlTable table, DtoMapper<TChildDto> childMapper, string columnsPrefix, Action<TDto, TChildDto> map, Action<FetchOp<TChildDto>> childIncludeConfig = null)
            where TChildDto : new()
        {
            _sqlQuery.Project<TChildDto>(childMapper, columnsPrefix, table);
            var include = new Action<FetchOp<TDto>, ReadHelper>((parentFetchOp, helper) =>
            {
                var childFetchOp = FetchOp<TChildDto>.Create(columnsPrefix, helper, childMapper);
                childIncludeConfig?.Invoke(childFetchOp);
                parentFetchOp.Include<TChildDto>(childFetchOp, map);
            });
            _includes.Add(include);
            return this;
        }
        #endregion

        #region TABLE DIRECT PROJECTION
        public SqlFetchQuery<TDto> Project<TProjDto>(SqlTable table, params SqlColumn[] exceptColumns)
            where TProjDto : new()
        {
            _sqlQuery.Project<TProjDto>(table, exceptColumns);
            return this;
        }

        public SqlFetchQuery<TDto> Project<TProjDto>(string columnsPrefix, SqlTable table, params SqlColumn[] exceptColumns)
            where TProjDto : new()
        {
            _sqlQuery.Project<TProjDto>(columnsPrefix, table, exceptColumns);
            return this;
        }

        public SqlFetchQuery<TDto> Project<TProjDto>(DtoMapper<TProjDto> mapper, SqlTable table, params SqlColumn[] exceptColumns)
            where TProjDto : new()
        {
            _sqlQuery.Project<TProjDto>(mapper, table, exceptColumns);
            return this;
        }

        public SqlFetchQuery<TDto> Project<TProjDto>(DtoMapper<TProjDto> mapper, string columnsPrefix, SqlTable table, params SqlColumn[] exceptColumns)
            where TProjDto : new()
        {
            _sqlQuery.Project<TProjDto>(mapper, columnsPrefix, table, exceptColumns);
            return this;
        }
        #endregion

        #region EXPRESSION PROJECTION
        public SqlFetchQuery<TDto> Project<TProjDto>(params (SqlExpression, Expression<Func<TProjDto, object>>)[] projectExpressions)
            where TProjDto : new()
        {
            _sqlQuery.Project<TProjDto>(projectExpressions);
            return this;
        }

        public SqlFetchQuery<TDto> Project<TProjDto>(params (SqlExpression, string)[] projectExpressions)
            where TProjDto : new()
        {
            _sqlQuery.Project<TProjDto>(projectExpressions);
            return this;
        }

        public SqlFetchQuery<TDto> Project<TProjDto>(DtoMapper<TProjDto> mapper, params (SqlExpression, Expression<Func<TProjDto, object>>)[] projectExpressions)
            where TProjDto : new()
        {
            _sqlQuery.Project<TProjDto>(mapper, projectExpressions);
            return this;
        }

        public SqlFetchQuery<TDto> Project<TProjDto>(DtoMapper<TProjDto> mapper, params (SqlExpression, string)[] projectExpressions)
            where TProjDto : new()
        {
            _sqlQuery.Project<TProjDto>(mapper, projectExpressions);
            return this;
        }

        public SqlFetchQuery<TDto> Project<TProjDto>(string columnsPrefix, params (SqlExpression, Expression<Func<TProjDto, object>>)[] projectExpressions)
            where TProjDto : new()
        {
            _sqlQuery.Project<TProjDto>(columnsPrefix, projectExpressions);
            return this;
        }

        public SqlFetchQuery<TDto> Project<TProjDto>(string columnsPrefix, params (SqlExpression, string)[] projectExpressions)
            where TProjDto : new()
        {
            _sqlQuery.Project<TProjDto>(columnsPrefix, projectExpressions);
            return this;
        }

        public SqlFetchQuery<TDto> Project<TProjDto>(DtoMapper<TProjDto> mapper, string columnsPrefix, params (SqlExpression, Expression<Func<TProjDto, object>>)[] projectExpressions)
            where TProjDto : new()
        {
            _sqlQuery.Project<TProjDto>(mapper, columnsPrefix, projectExpressions);
            return this;
        }

        public SqlFetchQuery<TDto> Project<TProjDto>(DtoMapper<TProjDto> mapper, string columnsPrefix, params (SqlExpression, string)[] projectExpressions)
            where TProjDto : new()
        {
            _sqlQuery.Project<TProjDto>(mapper, columnsPrefix, projectExpressions);
            return this;
        }
        #endregion

        #region EXECUTE
        public async Task<List<TDto>> ExecAsync(DbConnection connection)
        {
            using (var h = await _builder.ExecReadHelperAsync(_sqlQuery, connection))
            {
                var fetch = h.Fetch<TDto>();
                foreach (var include in _includes)
                {
                    include(fetch, h);
                }
                return fetch.All();
            }
        }

        public async Task<List<TDto>> ExecAsync(DbConnection connection, DbTransaction transaction)
        {
            using (var h = await _builder.ExecReadHelperAsync(_sqlQuery, connection, transaction))
            {
                var fetch = h.Fetch<TDto>();
                foreach (var include in _includes)
                {
                    include(fetch, h);
                }
                return fetch.All();
            }
        }
        #endregion

        public string BuildQueryString()
        {
            return _sqlQuery.BuildQueryString();
        }

        #region SQL CLAUSES
        public SqlFetchQuery<TDto> Distinct()
        {
            _sqlQuery.Distinct();
            return this;
        }

        public SqlFetchQuery<TDto> From(SqlTabularSource fromPart)
        {
            _sqlQuery.From(fromPart);
            return this;
        }

        public SqlFetchQuery<TDto> FromSubQuery(Func<SqlQuery, SqlQuery> subQueryBuilder)
        {
            _sqlQuery.FromSubQuery(subQueryBuilder);
            return this;
        }

        public SqlFetchQuery<TDto> Join(SqlTabularSource innerJoinPart, SqlExpression on)
        {
            _sqlQuery.Join(innerJoinPart, on);
            return this;
        }

        public SqlFetchQuery<TDto> JoinSubquery(Func<SqlQuery, SqlQuery> subQueryBuilder, Func<SqlQuery, SqlExpression> on)
        {
            _sqlQuery.JoinSubquery(subQueryBuilder, on);
            return this;
        }

        public SqlFetchQuery<TDto> LeftJoin(SqlTabularSource innerJoinPart, SqlExpression on)
        {
            _sqlQuery.LeftJoin(innerJoinPart, on);
            return this;
        }

        public SqlFetchQuery<TDto> LeftJoinSubquery(Func<SqlQuery, SqlQuery> subQueryBuilder, Func<SqlQuery, SqlExpression> on)
        {
            _sqlQuery.LeftJoinSubquery(subQueryBuilder, on);
            return this;
        }

        public SqlFetchQuery<TDto> RightJoin(SqlTabularSource innerJoinPart, SqlExpression on)
        {
            _sqlQuery.RightJoin(innerJoinPart, on);
            return this;
        }

        public SqlFetchQuery<TDto> RightJoinSubquery(Func<SqlQuery, SqlQuery> subQueryBuilder, Func<SqlQuery, SqlExpression> on)
        {
            _sqlQuery.RightJoinSubquery(subQueryBuilder, on);
            return this;
        }

        public SqlFetchQuery<TDto> FullJoin(SqlTabularSource innerJoinPart, SqlExpression on)
        {
            _sqlQuery.FullJoin(innerJoinPart, on);
            return this;
        }

        public SqlFetchQuery<TDto> FullJoinSubquery(Func<SqlQuery, SqlQuery> subQueryBuilder, Func<SqlQuery, SqlExpression> on)
        {
            _sqlQuery.FullJoinSubquery(subQueryBuilder, on);
            return this;
        }

        public SqlFetchQuery<TDto> CrossJoin(SqlTabularSource innerJoinPart)
        {
            _sqlQuery.CrossJoin(innerJoinPart);
            return this;
        }

        public SqlFetchQuery<TDto> CrossJoinSubquery(Func<SqlQuery, SqlQuery> subQueryBuilder)
        {
            _sqlQuery.CrossJoinSubquery(subQueryBuilder);
            return this;
        }

        public SqlFetchQuery<TDto> Where(SqlExpression condition)
        {
            _sqlQuery.Where(condition);
            return this;
        }

        public SqlFetchQuery<TDto> GroupBy(params SqlExpression[] by)
        {
            _sqlQuery.GroupBy(by);
            return this;
        }

        public SqlFetchQuery<TDto> Having(SqlExpression having)
        {
            _sqlQuery.Having(having);
            return this;
        }

        public SqlFetchQuery<TDto> OrderBy(params SqlExpression[] by)
        {
            _sqlQuery.OrderBy(by);
            return this;
        }

        public SqlFetchQuery<TDto> OrderByDescending(params SqlExpression[] by)
        {
            _sqlQuery.OrderByDescending(by);
            return this;
        }

        public SqlFetchQuery<TDto> With(string expressionName, Func<SqlQuery> buildQuery, params string[] columnNames)
        {
            _sqlQuery.With(expressionName, buildQuery, columnNames);
            return this;
        }

        public SqlFetchQuery<TDto> With(SqlCommonTable commonTable)
        {
            _sqlQuery.With(commonTable);
            return this;
        }
        #endregion
    }
}
