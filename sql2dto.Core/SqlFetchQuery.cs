using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace sql2dto.Core
{
    public class SqlFetchQuery<TDto>
        where TDto : new()
    {
        private readonly SqlBuilder _builder;
        private readonly SqlQuery _sqlQuery;
        public SqlFetchQuery(SqlBuilder builder, SqlTable table)
        {
            _builder = builder;
            _sqlQuery = new SqlQuery(builder);
            _sqlQuery.Project<TDto>(table);
        }

        public SqlFetchQuery<TDto> Include<TChildDto>(SqlTable table, Action<TDto, TChildDto> map, Action<FetchOp<TChildDto>> childIncludeConfig = null)
            where TChildDto : new()
        {
            _sqlQuery.Project<TDto>(table);
            return this;
        }

        public async Task<List<TDto>> ExecAsync(DbConnection connection)
        {
            using (var h = await _builder.ExecReadHelperAsync(_sqlQuery, connection))
            {
                var fetch = h.Fetch<TDto>();
                return fetch.All();
            }
        }

        public async Task<List<TDto>> ExecAsync(DbConnection connection, DbTransaction transaction)
        {
            using (var h = await _builder.ExecReadHelperAsync(_sqlQuery, connection, transaction))
            {
                var fetch = h.Fetch<TDto>();
                return fetch.All();
            }
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
