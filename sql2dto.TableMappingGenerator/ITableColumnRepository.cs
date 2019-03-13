using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sql2dto.TableMappingGenerator
{
    public interface ITableColumnRepository
    {
        Task<Dictionary<string, Dictionary<string, Dictionary<string, string>>>> GetColumnsBySchemaAndTable();
    }
}
