using sql2dto.Core;
using sql2dto.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using sql2dto.TableMappingGenerator.Forms;

namespace sql2dto.TableMappingGenerator
{
    class MSSQLTableColumnsRepository : ITableColumnRepository
    {
        public static readonly SqlBuilder SqlBuilder = new TSqlBuilder();
        private frmDBConnection _frmDBConnectionForm;

        public MSSQLTableColumnsRepository(frmDBConnection frmDBConnectionForm)
        {
            _frmDBConnectionForm = frmDBConnectionForm;
        }

        public async Task<Dictionary<string, Dictionary<string, Dictionary<string, string>>>> GetColumnsBySchemaAndTable()
        {
            var query =
@"SELECT TT.TABLE_NAME AS TABLENAME, TT.TABLE_SCHEMA as TABLESCHEMA, TC.COLUMN_NAME as COLUMNNAME
FROM INFORMATION_SCHEMA.TABLES TT
JOIN INFORMATION_SCHEMA.COLUMNS TC on TT.TABLE_NAME = TC.TABLE_NAME AND TT.TABLE_SCHEMA = TC.TABLE_SCHEMA
WHERE TABLE_TYPE = 'BASE TABLE' AND TT.TABLE_CATALOG = @Catalogue
ORDER BY TT.TABLE_SCHEMA";

            try
            {
                using (var conn = await SqlBuilder.ConnectAsync(_frmDBConnectionForm.GetConnectionString()))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;

                    cmd.CommandType = CommandType.Text;
                    var catalogue = cmd.CreateParameter();
                    catalogue.ParameterName = "@Catalogue";
                    catalogue.DbType = DbType.String;
                    catalogue.Value = _frmDBConnectionForm.DBName;
                    cmd.Parameters.Add(catalogue);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    using (var helper = new ReadHelper(reader, new TSqlReadHelperSettings()))
                    {
                        var fetch = helper.Fetch<TableColumns>();
                        var result = fetch.All();
                        var columnsBySchemaAndTable = result.GroupBy(schema => schema.TableSchema)
                                                            .ToDictionary(
                                                             (schemaGroup) => schemaGroup.Key,
                                                             (schemaGroup) => schemaGroup.GroupBy(column => column.TableName)
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
