using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace sql2dto.Core
{
    public class SqlUpdate : ISqlStatement
    {
        public SqlStatementType StatementType => SqlStatementType.UPDATE;

        public SqlUpdate(SqlBuilder builder, SqlTable table)
        {
            _builder = builder;
            _table = table;
            _columnsToSet = new List<(SqlColumn, SqlExpression)>();
            _dbParameters = new Dictionary<string, DbParameter>(StringComparer.OrdinalIgnoreCase);
        }

        private SqlBuilder _builder;

        private SqlTable _table;
        public SqlTable Table => _table;

        public List<(SqlColumn, SqlExpression)> _columnsToSet;
        public List<(SqlColumn, SqlExpression)> ColumnsToSet => _columnsToSet;

        private SqlExpression _whereExpression;
        public SqlExpression WhereExpression => _whereExpression;

        public SqlUpdate Set(SqlColumn column, SqlExpression valueExpression)
        {
            _columnsToSet.Add((column, valueExpression));
            return this;
        }

        public SqlUpdate Where(SqlExpression condition)
        {
            _whereExpression = condition;
            return this;
        }

        private Dictionary<string, DbParameter> _dbParameters;
        public Dictionary<string, DbParameter> GetDbParameters() => _dbParameters;

        public bool AddDbParameterIfNotFound(DbParameter parameter)
        {
            if (!_dbParameters.ContainsKey(parameter.ParameterName))
            {
                _dbParameters.Add(parameter.ParameterName, parameter);
                return true;
            }
            return false;
        }

        public int Exec(DbConnection connection)
        {
            return _builder.ExecUpdate(this, connection);
        }

        public int Exec(DbConnection connection, DbTransaction transaction)
        {
            return _builder.ExecUpdate(this, connection, transaction);
        }

        public async Task<int> ExecAsync(DbConnection connection)
        {
            return await _builder.ExecUpdateAsync(this, connection);
        }

        public async Task<int> ExecAsync(DbConnection connection, DbTransaction transaction)
        {
            return await _builder.ExecUpdateAsync(this, connection, transaction);
        }

        //TODO
        //public SqlUpdate Dto<TDto>(TDto dto)
        //    where TDto : new()
        //{

        //    return this;
        //}
    }
}
