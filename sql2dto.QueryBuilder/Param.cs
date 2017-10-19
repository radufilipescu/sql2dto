using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public class Param
    {
        public Param(string name, object value, DataType dataType = DataType.UNKOWN)
        {
            Name = name;
            Value = value;
            DataType = dataType;
        }

        public DataType DataType { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
    }
}
