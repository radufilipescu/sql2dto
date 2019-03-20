using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using sql2dto.Core;
using sql2dto.MSSqlServer;
using sql2dto.TableMappingGenerator.Dtos;

namespace sql2dto.TableMappingGenerator.Persistence
{
    public class MSSQLDBFunctionsRepository : IDBFunctionsRepository
    {
        public async Task<Dictionary<string, Dictionary<string, IEnumerable<string>>>> FetchFunctionParams()
        {
            var query =
@"SELECT SCH.NAME SCHEMANAME, SO.NAME FUNCTIONNAME, SP.NAME FUNCTIONPARAMETER
FROM sys.sql_modules SM
INNER JOIN sys.objects SO ON SM.object_id=SO.object_id
INNER JOIN sys.schemas SCH ON SO.schema_id=SCH.schema_id
LEFT JOIN sys.parameters SP ON SO.OBJECT_ID = SP.OBJECT_ID
WHERE SO.type_desc like '%function%'
AND SP.NAME <> ''";

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
                        var fetch = helper.Fetch<FunctionParametersDto>();
                        var result = fetch.All();
                        var columnsBySchemaAndTable = result.GroupBy(schema => schema.SchemaName)
                                                            .ToDictionary(
                                                             (schemaGroup) => schemaGroup.Key,
                                                             (schemaGroup) => schemaGroup.GroupBy(function => function.FunctionName)
                                                                                         .ToDictionary(
                                                                                          (funcGroup) => funcGroup.Key,
                                                                                          (funcGroup) => funcGroup.Select(fp => fp.FunctionParameter)
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
