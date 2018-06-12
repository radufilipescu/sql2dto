using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace sql2dto.Core
{
    public class SqlDelete : ISqlStatement
    {
        public SqlStatementType StatementType => SqlStatementType.DELETE;

        public SqlDelete(SqlBuilder builder, SqlTable table)
        {
            _builder = builder;
            _table = table;
            _dbParameters = new Dictionary<string, DbParameter>();
        }

        private SqlBuilder _builder;

        private SqlTable _table;
        public SqlTable Table => _table;

        private SqlExpression _whereExpression;
        public SqlExpression WhereExpression => _whereExpression;

        public SqlDelete Where(SqlExpression condition)
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
            return _builder.ExecDelete(this, connection);
        }

        public int Exec(DbConnection connection, DbTransaction transaction)
        {
            return _builder.ExecDelete(this, connection, transaction);
        }

        public async Task<int> ExecAsync(DbConnection connection)
        {
            return await _builder.ExecDeleteAsync(this, connection);
        }

        public async Task<int> ExecAsync(DbConnection connection, DbTransaction transaction)
        {
            return await _builder.ExecDeleteAsync(this, connection, transaction);
        }
    }
}
