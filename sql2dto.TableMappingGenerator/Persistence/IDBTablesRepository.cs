using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sql2dto.TableMappingGenerator.Persistence
{
    public interface IDBTablesRepository
    {
        Task<Dictionary<string, Dictionary<string, Dictionary<string, string>>>> FetchColumnTables();
    }
}
