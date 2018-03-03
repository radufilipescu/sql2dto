using sql2dto.Core;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace sql2dto.MSSqlServer
{
    public static class Extensions
    {
        #region SQL COMMAND EXTENSIONS
        public static ReadHelper ExecReadHelper(this SqlCommand cmd)
        {
            var reader = cmd.ExecuteReader();
            var readHelper = new ReadHelper(reader);
            return readHelper;
        }

        public static async Task<ReadHelper> ExecReadHelperAsync(this SqlCommand cmd)
        {
            var reader = await cmd.ExecuteReaderAsync();
            var readHelper = new ReadHelper(reader);
            return readHelper;
        }
        #endregion

        #region SQL CONNECTION EXTENSIONS
        #region EXEC SQL
        public static async Task<int> ExecSqlNonQueryAsync(this SqlConnection conn, SqlTransaction trans, string sql, params SqlParameter[] parameters)
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

        public static async Task<int> ExecSqlNonQueryAsync(this SqlConnection conn, string sql, params SqlParameter[] parameters)
        {
            return await conn.ExecSqlNonQueryAsync(null, sql, parameters);
        }

        public static async Task<int> ExecSqlNonQueryAsync(this SqlConnection conn, SqlTransaction trans, string sql, params (string, object)[] parameters)
        {
            return await conn.ExecSqlNonQueryAsync(trans, sql, parameters.Select(p => new SqlParameter(p.Item1, p.Item2)).ToArray());
        }

        public static async Task<int> ExecSqlNonQueryAsync(this SqlConnection conn, string sql, params (string, object)[] parameters)
        {
            return await conn.ExecSqlNonQueryAsync(null, sql, parameters);
        }

        public static async Task<T> ExecSqlScalarAsync<T>(this SqlConnection conn, SqlTransaction trans, string sql, params SqlParameter[] parameters)
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

        public static async Task<T> ExecSqlScalarAsync<T>(this SqlConnection conn, string sql, params SqlParameter[] parameters)
        {
            return await conn.ExecSqlScalarAsync<T>(null, sql, parameters);
        }

        public static async Task<T> ExecSqlScalarAsync<T>(this SqlConnection conn, SqlTransaction trans, string sql, params (string, object)[] parameters)
        {
            return await conn.ExecSqlScalarAsync<T>(trans, sql, parameters.Select(p => new SqlParameter(p.Item1, p.Item2)).ToArray());
        }

        public static async Task<T> ExecSqlScalarAsync<T>(this SqlConnection conn, string sql, params (string, object)[] parameters)
        {
            return await conn.ExecSqlScalarAsync<T>(null, sql, parameters);
        }

        public static async Task<ReadHelper> ExecSqlReadHelperAsync(this SqlConnection conn, SqlTransaction trans, string sql, params SqlParameter[] parameters)
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

        public static async Task<ReadHelper> ExecSqlReadHelperAsync(this SqlConnection conn, string sql, params SqlParameter[] parameters)
        {
            return await conn.ExecSqlReadHelperAsync(null, sql, parameters);
        }

        public static async Task<ReadHelper> ExecSqlReadHelperAsync(this SqlConnection conn, SqlTransaction trans, string sql, params (string, object)[] parameters)
        {
            return await conn.ExecSqlReadHelperAsync(trans, sql, parameters.Select(p => new SqlParameter(p.Item1, p.Item2)).ToArray());
        }

        public static async Task<ReadHelper> ExecSqlReadHelperAsync(this SqlConnection conn, string sql, params (string, object)[] parameters)
        {
            return await conn.ExecSqlReadHelperAsync(null, sql, parameters);
        }
        #endregion

        #region EXEC STORED PROCEDURE
        public static async Task<int> ExecSPNonQueryAsync(this SqlConnection conn, SqlTransaction trans, string spName, params SqlParameter[] parameters)
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

        public static async Task<int> ExecSPNonQueryAsync(this SqlConnection conn, string spName, params SqlParameter[] parameters)
        {
            return await conn.ExecSPNonQueryAsync(null, spName, parameters);
        }

        public static async Task<int> ExecSPNonQueryAsync(this SqlConnection conn, SqlTransaction trans, string spName, params (string, object)[] parameters)
        {
            return await conn.ExecSPNonQueryAsync(trans, spName, parameters.Select(p => new SqlParameter(p.Item1, p.Item2)).ToArray());
        }

        public static async Task<int> ExecSPNonQueryAsync(this SqlConnection conn, string spName, params (string, object)[] parameters)
        {
            return await conn.ExecSPNonQueryAsync(null, spName, parameters);
        }

        public static async Task<T> ExecSPScalarAsync<T>(this SqlConnection conn, SqlTransaction trans, string spName, params SqlParameter[] parameters)
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

        public static async Task<T> ExecSPScalarAsync<T>(this SqlConnection conn, string spName, params SqlParameter[] parameters)
        {
            return await conn.ExecSPScalarAsync<T>(null, spName, parameters);
        }

        public static async Task<T> ExecSPScalarAsync<T>(this SqlConnection conn, SqlTransaction trans, string spName, params (string, object)[] parameters)
        {
            return await conn.ExecSPScalarAsync<T>(trans, spName, parameters.Select(p => new SqlParameter(p.Item1, p.Item2)).ToArray());
        }

        public static async Task<T> ExecSPScalarAsync<T>(this SqlConnection conn, string spName, params (string, object)[] parameters)
        {
            return await conn.ExecSPScalarAsync<T>(null, spName, parameters);
        }

        public static async Task<ReadHelper> ExecSPReadHelperAsync(this SqlConnection conn, SqlTransaction trans, string spName, params SqlParameter[] parameters)
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

        public static async Task<ReadHelper> ExecSPReadHelperAsync(this SqlConnection conn, string spName, params SqlParameter[] parameters)
        {
            return await conn.ExecSPReadHelperAsync(null, spName, parameters);
        }

        public static async Task<ReadHelper> ExecSPReadHelperAsync(this SqlConnection conn, SqlTransaction trans, string spName, params (string, object)[] parameters)
        {
            return await conn.ExecSPReadHelperAsync(trans, spName, parameters.Select(p => new SqlParameter(p.Item1, p.Item2)).ToArray());
        }

        public static async Task<ReadHelper> ExecSPReadHelperAsync(this SqlConnection conn, string spName, params (string, object)[] parameters)
        {
            return await conn.ExecSPReadHelperAsync(null, spName, parameters);
        }
        #endregion
        #endregion

        #region SQL QUERY EXTENSIONS
        public static SqlCommand BuildSqlCommand(this SqlQuery query)
        {
            EnsureTSqlLanguageImplementations(query);
            return (SqlCommand)query.BuildDbCommand();
        }

        public static SqlCommand BuildSqlCommand(this SqlQuery query, SqlConnection connection)
        {
            EnsureTSqlLanguageImplementations(query);
            return (SqlCommand)query.BuildDbCommand(connection);
        }

        public static SqlCommand BuildSqlCommand(this SqlQuery query, SqlConnection connection, SqlTransaction transaction)
        {
            EnsureTSqlLanguageImplementations(query);
            return (SqlCommand)query.BuildDbCommand(connection, transaction);
        }

        private static void EnsureTSqlLanguageImplementations(SqlQuery query)
        {
            if (query.GetSqlBuilderLanguageImplementation() != TSqlBuilder.LANGUAGE_IMPLEMENTATION)
            {
                throw new InvalidOperationException("SqlQuery's builder is not a TSqlBuilder");
            }
        }
        #endregion
    }
}
