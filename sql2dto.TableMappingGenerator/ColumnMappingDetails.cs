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
        public List<EnvironmentColumnMappings> EnvironmentColumnMappings{get; set;}

        public UserPreferences()
        {
            EnvironmentColumnMappings = new List<EnvironmentColumnMappings>();
        }
    }

    class EnvironmentColumnMappings
    {
        public string DBServerName { get; set; }
        public string DBName { get; set; }
        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> ColumnMappings { get; set; }

        public EnvironmentColumnMappings()
        {
            ColumnMappings = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        }
    }
}
