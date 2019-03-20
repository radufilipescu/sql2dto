using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace sql2dto.TableMappingGenerator.Forms
{
    public partial class frmGenerator : Form
    {
        private bool _authorizeCheck { get; set; }
        public DBStructure _dbStructure = null;
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
                using (var frmDBConnection = new frmDBConnection())
                {
                    LoadUserPref();
                    if (_userPref != null)
                    {
                        frmDBConnection.ServerName = _userPref.LastServerName;
                        frmDBConnection.DBName = _userPref.LastDBName;
                        frmDBConnection.Login = _userPref.LastLogin;
                    }

                    if (frmDBConnection.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        Close();
                        return;
                    }

                    _dbStructure = frmDBConnection.DBStruct;
                    _userPref.LastServerName = _dbServer = frmDBConnection.ServerName;
                    _userPref.LastDBName = _dbName = frmDBConnection.DBName;
                    _userPref.LastLogin = _dbLogin = frmDBConnection.Login;

                    WriteUserPrefToFile(JsonConvert.SerializeObject(_userPref));

                    lblServerName.Text = frmDBConnection.ServerName;
                    lblDatabaseName.Text = frmDBConnection.DBName;
                    LoadColumnMappingsFromUserPref();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "An error occurred: " + ex.Message, "Oups!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            clbObjects.Items.Clear();

            foreach (var schema in _dbStructure.Schemas)
            {
                foreach (var function in schema.ParamsByFunction)
                {
                    clbObjects.Items.Add(new DBItem(schema.Name, function.Key, DBItem.DBType.Function));
                }
            }

            foreach (var schema in _dbStructure.Schemas)
            {
                foreach (var table in schema.ColumnMappingsByTable)
                {
                    clbObjects.Items.Add(new DBItem(schema.Name, table.Key, DBItem.DBType.Table));
                }
            }

            if (clbObjects.Items.Count > 0)
            {
                clbObjects.SelectedIndex = 0;
            }
        }

        private void clbTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(clbObjects.SelectedItem is DBItem selectedObject))
            {
                return;
            }

            if (selectedObject.Type == DBItem.DBType.Function)
            {
                lblColumnMappingsHeader.Text = $"Function {selectedObject.Name} ";
                dgvColumnMappings.Visible = false;
            }
            else if (selectedObject.Type == DBItem.DBType.Table)
            {
                dgvColumnMappings.Rows.Clear();
                dgvColumnMappings.Visible = true;

                if (_dbStructure != null && selectedObject != null)
                {
                    lblColumnMappingsHeader.Text = $"{selectedObject.Name} column mappings";
                    var schema = _dbStructure.Schemas.FirstOrDefault(s => s.Name == selectedObject.Schema);
                    if (schema != null)
                    {
                        if (schema.ColumnMappingsByTable.TryGetValue(selectedObject.Name, out Dictionary<string, string> table))
                        {
                            foreach (var column in table)
                            {
                                dgvColumnMappings.Rows.Add(column.Key, column.Value);
                            }
                        }
                    }
                }
            }
        }

        private void cbCheckAllTables_CheckedChanged(object sender, EventArgs e)
        {
            _authorizeCheck = true;
            bool checkedStatus = cbCheckAllTables.Checked;
            for (int i = 0; i < clbObjects.Items.Count; i++)
            {
                clbObjects.SetItemChecked(i, checkedStatus);
            }

            btnGenerate.Enabled = clbObjects.CheckedItems.Count > 0 ? true : false;
            _authorizeCheck = false;
        }

        private void clbTables_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!_authorizeCheck)
            {
                e.NewValue = e.CurrentValue; //check state change was not through authorized actions
            }
        }

        private void clbTables_MouseDown(object sender, MouseEventArgs e)
        {
            Point loc = this.clbObjects.PointToClient(Cursor.Position);
            for (int i = 0; i < this.clbObjects.Items.Count; i++)
            {
                Rectangle rec = this.clbObjects.GetItemRectangle(i);
                rec.Width = 16; //checkbox itself has a default width of about 16 pixels

                if (rec.Contains(loc))
                {
                    _authorizeCheck = true;
                    bool newValue = !this.clbObjects.GetItemChecked(i);
                    this.clbObjects.SetItemChecked(i, newValue);//check 

                    cbCheckAllTables.CheckedChanged -= cbCheckAllTables_CheckedChanged;
                    if (clbObjects.CheckedItems.Count == clbObjects.Items.Count)
                    {
                        cbCheckAllTables.Checked = true;
                    }
                    else
                    {
                        cbCheckAllTables.Checked = false;
                    }
                    btnGenerate.Enabled = clbObjects.CheckedItems.Count > 0 ? true : false;
                    cbCheckAllTables.CheckedChanged += cbCheckAllTables_CheckedChanged;
                    _authorizeCheck = false;

                    return;
                }
            }
        }

        private void dgvColumnMappings_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }

            var row = dgvColumnMappings.Rows[e.RowIndex];
            var selectedTable = clbObjects.SelectedItem as DBItem;

            if (e.ColumnIndex == 1 && selectedTable != null)
            {
                var columnName = (string)row.Cells[e.ColumnIndex - 1].Value;
                var changedValue = (string)row.Cells[e.ColumnIndex].Value;
                var schema = _dbStructure.Schemas.FirstOrDefault(s => s.Name == selectedTable.Schema);
                if (schema != null)
                {
                    if (schema.ColumnMappingsByTable.TryGetValue(selectedTable.Name, out Dictionary<string, string> table))
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
            string mappingResult = GenerateSqlMappings(_dbName);

            using (var frmMappingGenerator = new frmDisplayMappingResult())
            {
                frmMappingGenerator.SetMappingResult(mappingResult);
                frmMappingGenerator.ShowDialog(this);
            }
        }

        private string GenerateSqlMappings(string database)
        {
            string result = "";

            foreach (var schema in _dbStructure.Schemas)
            {
                string schemaMappings = "";
                int leftPaddingCount = 4;
                string leftPadding = new string(' ', leftPaddingCount);
                var functionMappings = GenerateSQLFunctions(schema.Name, schema.ParamsByFunction, leftPaddingCount + 4);
                var tableMappings = GenerateSQLTables(schema.Name, schema.ColumnMappingsByTable, leftPaddingCount + 4);

                if (!string.IsNullOrEmpty(functionMappings))
                {
                    schemaMappings =
$@"{leftPadding}public class {schema.Name}
{leftPadding}{{
{functionMappings}";
                }

                if (!string.IsNullOrEmpty(tableMappings))
                {
                    if (string.IsNullOrEmpty(schemaMappings))
                    {
                        schemaMappings =
$@"{leftPadding}public class {schema.Name}
{leftPadding}{{
{tableMappings}";
                    }
                    else
                    {
                        schemaMappings += Environment.NewLine + Environment.NewLine + tableMappings;
                    }
                }

                if (!string.IsNullOrEmpty(schemaMappings))
                {
                    result =
$@"public class {database}
{{
{schemaMappings}
{leftPadding}}}";
                }
            }
            result += Environment.NewLine + "}";

            UpdateAndSaveUserPref();

            return result;
        }

        private string GenerateSQLFunctions(string schema, Dictionary<string, IEnumerable<string>> parametersByFunction, int leftPaddingCount)
        {
            string leftPadding = new string(' ', leftPaddingCount);
            string result = "";

            foreach (var func in parametersByFunction)
            {
                foreach (DBItem itemChecked in clbObjects.CheckedItems)
                {
                    if (itemChecked.Name == func.Key && itemChecked.Schema == schema)
                    {
                        string template =
$@"{leftPadding}    public static SqlFunctionCallExpression {func.Key}({string.Join(", ", func.Value.Select(param => $"SqlExpression {FunctionParamToCamelCase(param)}").ToArray())})
{leftPadding}    {{
{leftPadding}        return Sql.FuncCall($""{{nameof({schema})}}.{{nameof({func.Key})}}"", new List<SqlExpression>(){{{string.Join(", ", func.Value.Select(param => $"{FunctionParamToCamelCase(param)}").ToArray())}}});
{leftPadding}    }}";
                        if (string.IsNullOrEmpty(result))
                        {
                            result = 
$@"{leftPadding}public static class UserFuncs
{leftPadding}{{
{template}";
                        }
                        else
                        {
                            result += Environment.NewLine + template;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(result))
            {
                result += Environment.NewLine + $"{leftPadding}}}";
            }

            return result;
        }

        private string GenerateSQLTables(string schema, Dictionary<string, Dictionary<string, string>> columnMappingsByTable, int leftPaddingCount)
        {
            string leftPadding = new string(' ', leftPaddingCount);
            string result = "";

            foreach (var table in columnMappingsByTable)
            {
                foreach (DBItem itemChecked in clbObjects.CheckedItems)
                {
                    if (itemChecked.Name == table.Key && itemChecked.Schema == schema)
                    {
                        string template =
$@"{leftPadding}public class {table.Key} : SqlTable
{leftPadding}{{
{leftPadding}    public static {table.Key} As(string alias)
{leftPadding}    {{
{leftPadding}        return new {table.Key}(alias);
{leftPadding}    }}

{leftPadding}    private {table.Key}(string alias)
{leftPadding}        : base(nameof({schema}), nameof({table.Key}), alias)
{leftPadding}    {{
{leftPadding}        {string.Join(Environment.NewLine + new string(' ', leftPaddingCount + 8), table.Value.Select(col => GenerateFieldIntialization(col.Key, col.Value)))}
{leftPadding}    }}

{leftPadding}    {string.Join(Environment.NewLine + new string(' ', leftPaddingCount + 4), table.Value.Select(col => $"public SqlColumn {col.Value};"))}
{leftPadding}}}";
                        if (string.IsNullOrEmpty(result))
                        {
                            result = template;
                        }
                        else
                        {
                            result += Environment.NewLine + Environment.NewLine + template;
                        }
                    }
                }
            }

            return result;
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

        private void LoadUserPref()
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
            _userPref = _userPref ?? new UserPreferences();
        }

        private void LoadColumnMappingsFromUserPref()
        {
            var localPreferences = _userPref?.Environments.SingleOrDefault(item => item.DBName == _dbName && item.DBServerName == _dbServer);
            if (localPreferences != null)
            {
                foreach (var localSchema in localPreferences.ColumnMappings)
                {
                    var schema = _dbStructure.Schemas.FirstOrDefault(s => s.Name == localSchema.Key);
                    if (schema != null)
                    {
                        foreach (var localTable in localSchema.Value)
                        {
                            if (schema.ColumnMappingsByTable.TryGetValue(localTable.Key, out Dictionary<string, string> columnMappings))
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
            var modifiedColumnMappings = _dbStructure.Schemas.ToDictionary(
                                                (schema) => schema.Name,
                                                (schema) => schema.ColumnMappingsByTable.ToList().ToDictionary(
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

            DBEnv newColumnMappings = new DBEnv()
            {
                DBServerName = _dbServer,
                DBName = _dbName,
                ColumnMappings = modifiedColumnMappings
            };

            _userPref.Environments.Remove(_userPref.Environments.FirstOrDefault(item => item.DBName == _dbName && item.DBServerName == _dbServer));
            _userPref.Environments.Add(newColumnMappings);
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

        public static string FunctionParamToCamelCase(string param)
        {
            if (!string.IsNullOrEmpty(param) && param.Length > 1)
            {
                string result = param[0] == '@' ? param.Remove(0, 1) : param;
                return Char.ToLowerInvariant(result[0]) + result.Substring(1);
            }
            return param;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
