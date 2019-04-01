using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sql2dto.TableMappingGenerator.Forms
{
    public class DBItem
    {
        public enum DBType
        {
            Function,
            Table
        }

        public DBType Type { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }

        public DBItem(string schema, string table, DBType type)
        {
            Schema = schema;
            Name = table;
            Type = type;
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Schema) ? $"{Name}" : $"{Schema}.{Name} [{Type.ToString()}]";
        }
    }
}
