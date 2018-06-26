using Oracle.ManagedDataAccess.Client;
using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sql2dto.Oracle
{
    public static class Extensions
    {
        #region SQL COMMAND EXTENSIONS
        public static ReadHelper ExecReadHelper(this OracleCommand cmd, IReadHelperSettings readHelperSettings = null)
        {
            var reader = cmd.ExecuteReader();
            var readHelper = new ReadHelper(reader, readHelperSettings);
            return readHelper;
        }

        public static async Task<ReadHelper> ExecReadHelperAsync(this OracleCommand cmd, IReadHelperSettings readHelperSettings = null)
        {
            var reader = await cmd.ExecuteReaderAsync();
            var readHelper = new ReadHelper(reader, readHelperSettings);
            return readHelper;
        }
        #endregion

        #region SQL CONNECTION EXTENSIONS
        #region EXEC SQL
        public static async Task<int> ExecSqlNonQueryAsync(this OracleConnection conn, OracleTransaction trans, string sql, params OracleParameter[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                if (trans != null)
                {
                    cmd.Transaction = trans;
                }
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.Parameters.AddRange(parameters);

                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<int> ExecSqlNonQueryAsync(this OracleConnection conn, string sql, params OracleParameter[] parameters)
        {
            return await conn.ExecSqlNonQueryAsync(null, sql, parameters);
        }

        public static async Task<int> ExecSqlNonQueryAsync(this OracleConnection conn, OracleTransaction trans, string sql, params (string, object)[] parameters)
        {
            return await conn.ExecSqlNonQueryAsync(trans, sql, parameters.Select(p => new OracleParameter(p.Item1, p.Item2)).ToArray());
        }

        public static async Task<int> ExecSqlNonQueryAsync(this OracleConnection conn, string sql, params (string, object)[] parameters)
        {
            return await conn.ExecSqlNonQueryAsync(null, sql, parameters);
        }

        public static async Task<T> ExecSqlScalarAsync<T>(this OracleConnection conn, OracleTransaction trans, string sql, params OracleParameter[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                if (trans != null)
                {
                    cmd.Transaction = trans;
                }
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.Parameters.AddRange(parameters);

                return (T)await cmd.ExecuteScalarAsync();
            }
        }

        public static async Task<T> ExecSqlScalarAsync<T>(this OracleConnection conn, string sql, params OracleParameter[] parameters)
        {
            return await conn.ExecSqlScalarAsync<T>(null, sql, parameters);
        }

        public static async Task<T> ExecSqlScalarAsync<T>(this OracleConnection conn, OracleTransaction trans, string sql, params (string, object)[] parameters)
        {
            return await conn.ExecSqlScalarAsync<T>(trans, sql, parameters.Select(p => new OracleParameter(p.Item1, p.Item2)).ToArray());
        }

        public static async Task<T> ExecSqlScalarAsync<T>(this OracleConnection conn, string sql, params (string, object)[] parameters)
        {
            return await conn.ExecSqlScalarAsync<T>(null, sql, parameters);
        }

        public static async Task<ReadHelper> ExecSqlReadHelperAsync(this OracleConnection conn, OracleTransaction trans, string sql, params OracleParameter[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                if (trans != null)
                {
                    cmd.Transaction = trans;
                }
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.Parameters.AddRange(parameters);

                return await cmd.ExecReadHelperAsync();
            }
        }

        public static async Task<ReadHelper> ExecSqlReadHelperAsync(this OracleConnection conn, string sql, params OracleParameter[] parameters)
        {
            return await conn.ExecSqlReadHelperAsync(null, sql, parameters);
        }

        public static async Task<ReadHelper> ExecSqlReadHelperAsync(this OracleConnection conn, OracleTransaction trans, string sql, params (string, object)[] parameters)
        {
            return await conn.ExecSqlReadHelperAsync(trans, sql, parameters.Select(p => new OracleParameter(p.Item1, p.Item2)).ToArray());
        }

        public static async Task<ReadHelper> ExecSqlReadHelperAsync(this OracleConnection conn, string sql, params (string, object)[] parameters)
        {
            return await conn.ExecSqlReadHelperAsync(null, sql, parameters);
        }
        #endregion

        #region EXEC STORED PROCEDURE
        public static async Task<int> ExecSPNonQueryAsync(this OracleConnection conn, OracleTransaction trans, string spName, params OracleParameter[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                if (trans != null)
                {
                    cmd.Transaction = trans;
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = spName;
                cmd.Parameters.AddRange(parameters);

                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<int> ExecSPNonQueryAsync(this OracleConnection conn, string spName, params OracleParameter[] parameters)
        {
            return await conn.ExecSPNonQueryAsync(null, spName, parameters);
        }

        public static async Task<int> ExecSPNonQueryAsync(this OracleConnection conn, OracleTransaction trans, string spName, params (string, object)[] parameters)
        {
            return await conn.ExecSPNonQueryAsync(trans, spName, parameters.Select(p => new OracleParameter(p.Item1, p.Item2)).ToArray());
        }

        public static async Task<int> ExecSPNonQueryAsync(this OracleConnection conn, string spName, params (string, object)[] parameters)
        {
            return await conn.ExecSPNonQueryAsync(null, spName, parameters);
        }

        public static async Task<T> ExecSPScalarAsync<T>(this OracleConnection conn, OracleTransaction trans, string spName, params OracleParameter[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                if (trans != null)
                {
                    cmd.Transaction = trans;
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = spName;
                cmd.Parameters.AddRange(parameters);

                return (T)await cmd.ExecuteScalarAsync();
            }
        }

        public static async Task<T> ExecSPScalarAsync<T>(this OracleConnection conn, string spName, params OracleParameter[] parameters)
        {
            return await conn.ExecSPScalarAsync<T>(null, spName, parameters);
        }

        public static async Task<T> ExecSPScalarAsync<T>(this OracleConnection conn, OracleTransaction trans, string spName, params (string, object)[] parameters)
        {
            return await conn.ExecSPScalarAsync<T>(trans, spName, parameters.Select(p => new OracleParameter(p.Item1, p.Item2)).ToArray());
        }

        public static async Task<T> ExecSPScalarAsync<T>(this OracleConnection conn, string spName, params (string, object)[] parameters)
        {
            return await conn.ExecSPScalarAsync<T>(null, spName, parameters);
        }

        public static async Task<ReadHelper> ExecSPReadHelperAsync(this OracleConnection conn, OracleTransaction trans, string spName, params OracleParameter[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                if (trans != null)
                {
                    cmd.Transaction = trans;
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = spName;
                cmd.Parameters.AddRange(parameters);

                return await cmd.ExecReadHelperAsync();
            }
        }

        public static async Task<ReadHelper> ExecSPReadHelperAsync(this OracleConnection conn, string spName, params OracleParameter[] parameters)
        {
            return await conn.ExecSPReadHelperAsync(null, spName, parameters);
        }

        public static async Task<ReadHelper> ExecSPReadHelperAsync(this OracleConnection conn, OracleTransaction trans, string spName, params (string, object)[] parameters)
        {
            return await conn.ExecSPReadHelperAsync(trans, spName, parameters.Select(p => new OracleParameter(p.Item1, p.Item2)).ToArray());
        }

        public static async Task<ReadHelper> ExecSPReadHelperAsync(this OracleConnection conn, string spName, params (string, object)[] parameters)
        {
            return await conn.ExecSPReadHelperAsync(null, spName, parameters);
        }
        #endregion
        #endregion

        #region SQL QUERY EXTENSIONS
        public static OracleCommand BuildSqlCommand(this SqlQuery query)
        {
            EnsureTSqlLanguageImplementations(query);
            return (OracleCommand)query.BuildDbCommand();
        }

        public static OracleCommand BuildSqlCommand(this SqlQuery query, OracleConnection connection)
        {
            EnsureTSqlLanguageImplementations(query);
            return (OracleCommand)query.BuildDbCommand(connection);
        }

        public static OracleCommand BuildSqlCommand(this SqlQuery query, OracleConnection connection, OracleTransaction transaction)
        {
            EnsureTSqlLanguageImplementations(query);
            return (OracleCommand)query.BuildDbCommand(connection, transaction);
        }

        private static void EnsureTSqlLanguageImplementations(SqlQuery query)
        {
            if (query.GetSqlBuilderLanguageImplementation() != PLSqlBuilder.LANGUAGE_IMPLEMENTATION)
            {
                throw new InvalidOperationException("SqlQuery's builder is not a TSqlBuilder");
            }
        }
        #endregion
    }
}
