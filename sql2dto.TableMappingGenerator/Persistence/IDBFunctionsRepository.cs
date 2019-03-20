using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sql2dto.TableMappingGenerator.Persistence
{
    public interface IDBFunctionsRepository
    {
        Task<Dictionary<string, Dictionary<string, IEnumerable<string>>>> FetchFunctionParams();
    }
}
