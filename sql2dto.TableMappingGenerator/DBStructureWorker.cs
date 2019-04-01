using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sql2dto.TableMappingGenerator.Persistence;

namespace sql2dto.TableMappingGenerator
{
    public class DBStructureWorker
    {
        private IDBTablesRepository _repDBTableColumns;
        private IDBFunctionsRepository _repDBFunctions;

        public DBStructureWorker()
        {
            _repDBTableColumns = new MSSQLDBTablesRepository();
            _repDBFunctions = new MSSQLDBFunctionsRepository();
        }

        public async Task<DBStructure> GetDBStructure()
        {
            var columnsByTableAndSchema = await _repDBTableColumns.FetchColumnTables();
            var functionParamsBySchema = await _repDBFunctions.FetchFunctionParams();
            DBStructure dbStructure = new DBStructure
            {
                Schemas = functionParamsBySchema.Select(schema => new DBSchema()
                {
                    Name = schema.Key,
                    ParamsByFunction = schema.Value
                }).ToList()
            };

            foreach (var schema in dbStructure.Schemas)
            {
                if (columnsByTableAndSchema.TryGetValue(schema.Name, out Dictionary<string, Dictionary<string, string>> colMappingsByTable))
                {
                    schema.ColumnMappingsByTable = colMappingsByTable;
                    columnsByTableAndSchema.Remove(schema.Name);
                }
            }

            dbStructure.Schemas = dbStructure.Schemas.Concat(columnsByTableAndSchema.Select(schema => new DBSchema()
            {
                Name = schema.Key,
                ColumnMappingsByTable = schema.Value
            })).ToList();

            return dbStructure;
        }
    }

    public class DBStructure
    {
        public List<DBSchema> Schemas;

        public DBStructure()
        {
            Schemas = new List<DBSchema>();
        }
    }

    public class DBSchema
    {
        public string Name { get; set; }
        public Dictionary<string, Dictionary<string, string>> ColumnMappingsByTable { get; set; }
        public Dictionary<string, IEnumerable<string>> ParamsByFunction { get; set; }

        public DBSchema()
        {
            ColumnMappingsByTable = new Dictionary<string, Dictionary<string, string>>();
            ParamsByFunction = new Dictionary<string, IEnumerable<string>>();
        }
    }
}
