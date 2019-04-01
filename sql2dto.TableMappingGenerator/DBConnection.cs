using System;
using System.Data.Common;
using System.Threading.Tasks;
using sql2dto.Core;
using sql2dto.MSSqlServer;
using sql2dto.TableMappingGenerator.Forms;

namespace sql2dto.TableMappingGenerator
{
    public static class DBConnection
    {
        public static readonly SqlBuilder SqlBuilder = new TSqlBuilder();
        public static frmDBConnection FrmDBConnectionForm = null;

        public static async Task<DbConnection> ConnectAsync()
        {
            return await SqlBuilder.ConnectAsync(FrmDBConnectionForm.GetConnectionString());
        }
    }
}