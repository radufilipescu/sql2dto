using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sql2dto.TableMappingGenerator
{
    class UserPreferences
    {
        public string LastServerName { get; set; }
        public string LastDBName { get; set; }
        public string LastLogin { get; set; }
        public List<DBEnv> Environments{get; set;}

        public UserPreferences()
        {
            Environments = new List<DBEnv>();
        }
    }

    class DBEnv
    {
        public string DBServerName { get; set; }
        public string DBName { get; set; }
        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> ColumnMappings { get; set; }

        public DBEnv()
        {
            ColumnMappings = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        }
    }
}
