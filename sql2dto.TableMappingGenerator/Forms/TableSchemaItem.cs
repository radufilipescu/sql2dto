using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sql2dto.TableMappingGenerator.Forms
{
    public class TableSchemaItem
    {
        public string Schema { get; set; }
        public string Table { get; set; }

        public TableSchemaItem(string schema, string table)
        {
            Schema = schema;
            Table = table;
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Schema) ? $"{Table}" : $"{Schema}.{Table}";
        }
    }
}
