using sql2dto.Core;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace sql2dto.MSSqlServer
{
    public static class Extensions
    {
        public static ReadHelper ExecuteReadHelper(this SqlCommand cmd)
        {
            var reader = cmd.ExecuteReader();
            var readHelper = new ReadHelper(reader);
            return readHelper;
        }

        public static async Task<ReadHelper> ExecuteReadHelperAsync(this SqlCommand cmd)
        {
            var reader = await cmd.ExecuteReaderAsync();
            var readHelper = new ReadHelper(reader);
            return readHelper;
        }
    }
}
