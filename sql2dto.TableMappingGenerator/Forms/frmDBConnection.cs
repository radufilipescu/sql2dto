using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sql2dto.TableMappingGenerator.Forms
{
    public partial class frmDBConnection : Form
    {
        private ITableColumnRepository _repository;
        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> ColumnsBySchemaAndTable = null;

        public frmDBConnection()
        {
            InitializeComponent();
        }

        public string ServerName
        {
            get
            {
                return this.tbServerName.Text;
            }

            set
            {
                this.tbServerName.Text = value;
            }
        }

        public string DBName
        {
            get
            {
                return this.tbDBName.Text;
            }

            set
            {
                this.tbDBName.Text = value;
            }
        }

        public string Login
        {
            get
            {
                return this.tbLogin.Text;
            }

            set
            {
                this.tbLogin.Text = value;
            }
        }

        public string Password
        {
            get
            {
                return this.tbPassword.Text;
            }
        }

        public string GetConnectionString()
        {
            return $@"Server={ServerName};Database={DBName};User Id={Login};Password={Password};";
        }

        private void frmDBConnectionForm_Shown(object sender, EventArgs e)
        {
            tbPassword.Focus();
        }

        private void DBConnectionForm_Load(object sender, EventArgs e)
        {
            tbPassword.Focus();
        }

        private async void btnConnect_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                using (var frmProgress = new frmProgress())
                {
                    frmProgress.Show(this);

                    _repository = new MSSQLTableColumnsRepository(this);
                    ColumnsBySchemaAndTable = await _repository.GetColumnsBySchemaAndTable();
                }

                this.DialogResult = DialogResult.OK;
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, $"Could not fetch the database table structure! {Environment.NewLine} {Environment.NewLine}" + ex.Message, 
                    "Oups!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
