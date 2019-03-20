using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using sql2dto.Core;
using sql2dto.MSSqlServer;
using sql2dto.TableMappingGenerator.Dtos;

namespace sql2dto.TableMappingGenerator.Persistence
{
    class MSSQLDBTablesRepository : IDBTablesRepository
    {
        public static readonly SqlBuilder SqlBuilder = new TSqlBuilder();

        public MSSQLDBTablesRepository()
        {
        }

        public async Task<Dictionary<string, Dictionary<string, Dictionary<string, string>>>> FetchColumnTables()
        {
            var query =
@"SELECT TT.TABLE_NAME AS TABLENAME, TT.TABLE_SCHEMA as TABLESCHEMA, TC.COLUMN_NAME as COLUMNNAME
FROM INFORMATION_SCHEMA.TABLES TT
INNER JOIN INFORMATION_SCHEMA.COLUMNS TC on TT.TABLE_NAME = TC.TABLE_NAME AND TT.TABLE_SCHEMA = TC.TABLE_SCHEMA
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TT.TABLE_SCHEMA";

            try
            {
                using (var conn = await DBConnection.ConnectAsync())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    using (var helper = new ReadHelper(reader, new TSqlReadHelperSettings()))
                    {
                        var fetch = helper.Fetch<TableColumnsDto>();
                        var result = fetch.All();
                        var columnsBySchemaAndTable = result.GroupBy(schema => schema.TableSchema)
                                                            .ToDictionary(
                                                             (schemaGroup) => schemaGroup.Key,
                                                             (schemaGroup) => schemaGroup.GroupBy(table => table.TableName)
                                                                                         .ToDictionary(
                                                                                          (tableGroup) => tableGroup.Key,
                                                                                          (tableGroup) => tableGroup.Select(column => column.ColumnName).ToDictionary(k => k, v => v)
                                                                                         )
                                                            );

                        return columnsBySchemaAndTable;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
