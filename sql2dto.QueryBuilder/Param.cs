using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public class Param
    {
        public DataType DataType { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
    }
}
