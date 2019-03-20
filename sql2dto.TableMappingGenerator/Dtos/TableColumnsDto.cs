using System;

namespace sql2dto.TableMappingGenerator.Dtos
{
    class TableColumnsDto
    {
        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
    }
}
