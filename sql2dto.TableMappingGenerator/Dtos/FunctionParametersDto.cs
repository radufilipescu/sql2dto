using System;

namespace sql2dto.TableMappingGenerator.Dtos
{
    public class FunctionParametersDto
    {
        public string SchemaName { get; set; }
        public string FunctionName { get; set; }
        public string FunctionParameter { get; set; }
    }
}
