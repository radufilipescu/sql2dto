using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sql2dto.TableMappingGenerator.Forms
{
    public partial class frmGenerator : Form
    {
        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> _columnsBySchemaAndTable = null;
        private string _dbName = "";
        private string _dbServer = "";
        private string _dbLogin = "";
        private string _userPrefFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sql2dto";
        private string _userPrefFileName = "user_pref.txt";
        private UserPreferences _userPref = null;

        public frmGenerator()
        {
            InitializeComponent();
        }

        private void MappingGeneratorForm_LoadAsync(object sender, EventArgs e)
        {
            try
            {
                using (var frmDBConnectionForm = new frmDBConnection())
                {
                    LoadUserPref();
                    if (_userPref!= null)
                    {
                        frmDBConnectionForm.ServerName = _userPref.LastServerName;
                        frmDBConnectionForm.DBName = _userPref.LastDBName;
                        frmDBConnectionForm.Login = _userPref.LastLogin;
                    }

                    if (frmDBConnectionForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        this.Close();
                        return;
                    }

                    _dbServer = frmDBConnectionForm.ServerName;
                    _dbName = frmDBConnectionForm.DBName;
                    _dbLogin = frmDBConnectionForm.Login;
                    _columnsBySchemaAndTable = frmDBConnectionForm.ColumnsBySchemaAndTable;
                    lblServerName.Text = frmDBConnectionForm.ServerName;
                    lblDatabaseName.Text = frmDBConnectionForm.DBName;
                    LoadColumnMappingsFromUserPref();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "An error occurred: " + ex.Message, "Oups!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            clbTables.Items.Clear();
            foreach (var schema in _columnsBySchemaAndTable)
            {
                foreach (var table in schema.Value)
                {
                    clbTables.Items.Add(new TableSchemaItem(schema.Key, table.Key));
                }
            }

            if (clbTables.Items.Count > 0)
            {
                clbTables.SelectedIndex = 0;
            }
        }

        private void clbTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedTable = clbTables.SelectedItem as TableSchemaItem;
            dgvColumnMappins.Rows.Clear();

            if (_columnsBySchemaAndTable != null && selectedTable != null)
            {
                lblColumnMappingsHeader.Text = $"{selectedTable.Table} column mappings";
                gbColumnMappings.Visible = true;
                if (_columnsBySchemaAndTable.TryGetValue(selectedTable.Schema, out Dictionary<string, Dictionary<string, string>> schema))
                {
                    if (schema.TryGetValue(selectedTable.Table, out Dictionary<string, string> table))
                    {
                        foreach (var column in table)
                        {
                            dgvColumnMappins.Rows.Add(column.Key, column.Value);
                        }
                    }
                }
            }
        }

        private void cbCheckAllTables_CheckedChanged(object sender, EventArgs e)
        {
            AuthorizeCheck = true;
            bool checkedStatus = cbCheckAllTables.Checked;
            for (int i = 0; i < clbTables.Items.Count; i++)
            {
                clbTables.SetItemChecked(i, checkedStatus);
            }

            btnGenerate.Enabled = clbTables.CheckedItems.Count > 0 ? true : false;
            AuthorizeCheck = false;
        }


        bool AuthorizeCheck { get; set; }

        private void clbTables_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!AuthorizeCheck)
            {
                e.NewValue = e.CurrentValue; //check state change was not through authorized actions
            }
        }

        private void clbTables_MouseDown(object sender, MouseEventArgs e)
        {
            Point loc = this.clbTables.PointToClient(Cursor.Position);
            for (int i = 0; i < this.clbTables.Items.Count; i++)
            {
                Rectangle rec = this.clbTables.GetItemRectangle(i);
                rec.Width = 16; //checkbox itself has a default width of about 16 pixels

                if (rec.Contains(loc))
                {
                    AuthorizeCheck = true;
                    bool newValue = !this.clbTables.GetItemChecked(i);
                    this.clbTables.SetItemChecked(i, newValue);//check 

                    cbCheckAllTables.CheckedChanged -= cbCheckAllTables_CheckedChanged;
                    if (clbTables.CheckedItems.Count == clbTables.Items.Count)
                    {
                        cbCheckAllTables.Checked = true;
                    }
                    else
                    {
                        cbCheckAllTables.Checked = false;
                    }
                    btnGenerate.Enabled = clbTables.CheckedItems.Count > 0 ? true : false;
                    cbCheckAllTables.CheckedChanged += cbCheckAllTables_CheckedChanged;
                    AuthorizeCheck = false;

                    return;
                }
            }
        }

        private void dgvColumnMappings_ellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }

            var row = dgvColumnMappins.Rows[e.RowIndex];
            var selectedTable = clbTables.SelectedItem as TableSchemaItem;

            if (e.ColumnIndex == 1 && selectedTable != null)
            {
                var columnName = (string)row.Cells[e.ColumnIndex - 1].Value;
                var changedValue = (string)row.Cells[e.ColumnIndex].Value;

                if (_columnsBySchemaAndTable.TryGetValue(selectedTable.Schema, out Dictionary<string, Dictionary<string, string>> schema))
                {
                    if (schema.TryGetValue(selectedTable.Table, out Dictionary<string, string> table))
                    {
                        if (table.TryGetValue(columnName, out string mappedField))
                        {
                            table[columnName] = changedValue;
                        }
                    }
                }
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            string mappingResult = GenerateMapping(_dbName);

            using (var frmMappingGenerator = new frmDisplayMappingResult())
            {
                frmMappingGenerator.SetMappingResult(mappingResult);
                frmMappingGenerator.ShowDialog(this);
            }
        }

        private string GenerateMapping(string database)
        {
            string result = "";

            foreach (var schema in _columnsBySchemaAndTable)
            {
                string schemaMapping = "";
                int leftPaddingCount = 4;
                string schemaLeftPadding = new string(' ', leftPaddingCount);
                foreach (var table in schema.Value)
                {
                    foreach (TableSchemaItem itemChecked in clbTables.CheckedItems)
                    {
                        if (itemChecked.Table == table.Key && itemChecked.Schema == schema.Key)
                        {
                            string tableMapping = GenerateTableMapping(schema.Key, table.Key, table.Value, leftPaddingCount + 4);
                            if (string.IsNullOrEmpty(schemaMapping))
                            {
                                schemaMapping = 
$@"{schemaLeftPadding}public class {schema.Key}
{schemaLeftPadding}{{
{tableMapping}";
                            }
                            else
                            {
                                schemaMapping += Environment.NewLine + Environment.NewLine + tableMapping;
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(schemaMapping))
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result =
$@"public class {database}
{{
{schemaMapping}
{schemaLeftPadding}}}";
                    }
                    else
                    {
                        result +=
$@"

{schemaMapping}
{schemaLeftPadding}}}";
                    }
                };
            }
            result += $@"
}}";

            UpdateAndSaveUserPref();

            return result;
        }

        private string GenerateTableMapping(string schema, string table, Dictionary<string, string> columnMappings, int leftPaddingCount)
        {
            string leftPadding = new string(' ', leftPaddingCount);
            string template =
$@"{leftPadding}public class {table} : SqlTable
{leftPadding}{{
{leftPadding}    public static {table} As(string alias)
{leftPadding}    {{
{leftPadding}        return new {table}(alias);
{leftPadding}    }}

{leftPadding}    private {table}(string alias)
{leftPadding}        : base(nameof({schema}), nameof({table}), alias)
{leftPadding}    {{
{leftPadding}        {string.Join(Environment.NewLine + new string(' ', leftPaddingCount + 8), columnMappings.Select(col => GenerateFieldIntialization(col.Key, col.Value)))}
{leftPadding}    }}

{leftPadding}    {string.Join(Environment.NewLine + new string(' ', leftPaddingCount + 4), columnMappings.Select(col => $"public SqlColumn {col.Value};"))}
{leftPadding}}}";

            return template;
        }

        private string GenerateFieldIntialization(string sourceColumn, string destinationField)
        {
            if (sourceColumn == destinationField)
            {
                return $"{destinationField} = DefineColumn(nameof({sourceColumn}));";
            }
            else
            {
                return $"{destinationField} = DefineColumn(nameof({destinationField}), \"{sourceColumn}\");";
            }
        }

        private  void LoadUserPref()
        {
            string userPrefPath = $"{ _userPrefFolder}\\{_userPrefFileName}";
            if (File.Exists(userPrefPath))
            {
                using (var tw = new StreamReader(userPrefPath, false))
                {
                    var userPrefJSON = tw.ReadToEnd();
                    try
                    {
                        _userPref = JsonConvert.DeserializeObject<UserPreferences>(userPrefJSON);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            _userPref = _userPref?? new UserPreferences();
        }

        private void LoadColumnMappingsFromUserPref()
        {
            var localPreferences = _userPref?.EnvironmentColumnMappings.SingleOrDefault(item => item.DBName == _dbName && item.DBServerName == _dbServer);
            if (localPreferences != null)
            {
                foreach (var localSchema in localPreferences.ColumnMappings)
                {
                    if (_columnsBySchemaAndTable.TryGetValue(localSchema.Key, out Dictionary<string, Dictionary<string, string>> tables))
                    {
                        foreach (var localTable in localSchema.Value)
                        {
                            if (tables.TryGetValue(localTable.Key, out Dictionary<string, string> columnMappings))
                            {
                                foreach (var localColumn in localTable.Value)
                                {
                                    if (columnMappings.TryGetValue(localColumn.Key, out string colMappingValue))
                                    {
                                        columnMappings[localColumn.Key] = localColumn.Value;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private void UpdateAndSaveUserPref()
        {
            var modifiedColumnMappings = _columnsBySchemaAndTable.ToDictionary(
                                                (schema) => schema.Key,
                                                (schema) => schema.Value.ToList().ToDictionary(
                                                                                    (table) => table.Key,
                                                                                    (table) => table.Value.ToList()
                                                                                                          .Where(column => column.Key != column.Value)
                                                                                                          .ToDictionary(
                                                                                                            (column) => column.Key,
                                                                                                            (column) => column.Value
                                                                                                           )
                                                                                  ).Where(item => item.Value.Count > 0)
                                                                                   .ToDictionary(item => item.Key, item => item.Value)

                                               ).Where(item => item.Value.Count > 0)
                                                .ToDictionary(item => item.Key, item => item.Value);

            EnvironmentColumnMappings newColumnMappings = new EnvironmentColumnMappings()
            {
                DBServerName = _dbServer,
                DBName = _dbName,
                ColumnMappings = modifiedColumnMappings
            };

            _userPref.EnvironmentColumnMappings.Remove(_userPref.EnvironmentColumnMappings.FirstOrDefault(item => item.DBName == _dbName && item.DBServerName == _dbServer));
            _userPref.EnvironmentColumnMappings.Add(newColumnMappings);
            _userPref.LastServerName = _dbServer;
            _userPref.LastDBName = _dbName;
            _userPref.LastLogin = _dbLogin;

            WriteUserPrefToFile(JsonConvert.SerializeObject(_userPref));
        }

        private void WriteUserPrefToFile(string userPrefJSON)
        {
            string userPrefFilePath = $"{ _userPrefFolder}\\{_userPrefFileName}";

            if (!Directory.Exists(_userPrefFolder))
            {
                Directory.CreateDirectory(_userPrefFolder);
            }

            File.WriteAllText(userPrefFilePath, userPrefJSON);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
