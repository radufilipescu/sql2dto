namespace sql2dto.TableMappingGenerator.Forms
{
    partial class frmGenerator
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.clbObjects = new System.Windows.Forms.CheckedListBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gbDBObjects = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbCheckAllTables = new System.Windows.Forms.CheckBox();
            this.gbColumnMappings = new System.Windows.Forms.GroupBox();
            this.lblColumnMappingsHeader = new System.Windows.Forms.Label();
            this.dgvColumnMappings = new System.Windows.Forms.DataGridView();
            this.SourceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DestinationField = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblServer = new System.Windows.Forms.Label();
            this.lblServerName = new System.Windows.Forms.Label();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.lblDatabaseName = new System.Windows.Forms.Label();
            this.gbDBObjects.SuspendLayout();
            this.gbColumnMappings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumnMappings)).BeginInit();
            this.SuspendLayout();
            // 
            // clbObjects
            // 
            this.clbObjects.CheckOnClick = true;
            this.clbObjects.FormattingEnabled = true;
            this.clbObjects.Location = new System.Drawing.Point(6, 47);
            this.clbObjects.Name = "clbObjects";
            this.clbObjects.Size = new System.Drawing.Size(291, 334);
            this.clbObjects.TabIndex = 0;
            this.clbObjects.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbTables_ItemCheck);
            this.clbObjects.SelectedIndexChanged += new System.EventHandler(this.clbTables_SelectedIndexChanged);
            this.clbObjects.MouseDown += new System.Windows.Forms.MouseEventHandler(this.clbTables_MouseDown);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Enabled = false;
            this.btnGenerate.Location = new System.Drawing.Point(645, 448);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(86, 23);
            this.btnGenerate.TabIndex = 2;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(753, 448);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // gbDBObjects
            // 
            this.gbDBObjects.Controls.Add(this.label1);
            this.gbDBObjects.Controls.Add(this.cbCheckAllTables);
            this.gbDBObjects.Controls.Add(this.clbObjects);
            this.gbDBObjects.Location = new System.Drawing.Point(12, 48);
            this.gbDBObjects.Name = "gbDBObjects";
            this.gbDBObjects.Size = new System.Drawing.Size(303, 389);
            this.gbDBObjects.TabIndex = 5;
            this.gbDBObjects.TabStop = false;
            this.gbDBObjects.Text = "Objects";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select All:";
            // 
            // cbCheckAllTables
            // 
            this.cbCheckAllTables.AutoSize = true;
            this.cbCheckAllTables.Location = new System.Drawing.Point(60, 22);
            this.cbCheckAllTables.Name = "cbCheckAllTables";
            this.cbCheckAllTables.Size = new System.Drawing.Size(15, 14);
            this.cbCheckAllTables.TabIndex = 1;
            this.cbCheckAllTables.UseVisualStyleBackColor = true;
            this.cbCheckAllTables.CheckedChanged += new System.EventHandler(this.cbCheckAllTables_CheckedChanged);
            // 
            // gbColumnMappings
            // 
            this.gbColumnMappings.Controls.Add(this.lblColumnMappingsHeader);
            this.gbColumnMappings.Controls.Add(this.dgvColumnMappings);
            this.gbColumnMappings.Location = new System.Drawing.Point(339, 48);
            this.gbColumnMappings.Name = "gbColumnMappings";
            this.gbColumnMappings.Size = new System.Drawing.Size(489, 389);
            this.gbColumnMappings.TabIndex = 6;
            this.gbColumnMappings.TabStop = false;
            // 
            // lblColumnMappingsHeader
            // 
            this.lblColumnMappingsHeader.AutoSize = true;
            this.lblColumnMappingsHeader.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblColumnMappingsHeader.Location = new System.Drawing.Point(8, 12);
            this.lblColumnMappingsHeader.Name = "lblColumnMappingsHeader";
            this.lblColumnMappingsHeader.Size = new System.Drawing.Size(0, 16);
            this.lblColumnMappingsHeader.TabIndex = 3;
            // 
            // dgvColumnMappings
            // 
            this.dgvColumnMappings.AllowUserToAddRows = false;
            this.dgvColumnMappings.AllowUserToDeleteRows = false;
            this.dgvColumnMappings.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dgvColumnMappings.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvColumnMappings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvColumnMappings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SourceColumn,
            this.DestinationField});
            this.dgvColumnMappings.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvColumnMappings.Location = new System.Drawing.Point(8, 35);
            this.dgvColumnMappings.Name = "dgvColumnMappings";
            this.dgvColumnMappings.RowHeadersVisible = false;
            this.dgvColumnMappings.Size = new System.Drawing.Size(475, 346);
            this.dgvColumnMappings.TabIndex = 0;
            this.dgvColumnMappings.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvColumnMappings_CellValueChanged);
            // 
            // SourceColumn
            // 
            this.SourceColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.SourceColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.SourceColumn.FillWeight = 60.9137F;
            this.SourceColumn.HeaderText = "Source Column";
            this.SourceColumn.Name = "SourceColumn";
            this.SourceColumn.ReadOnly = true;
            // 
            // DestinationField
            // 
            this.DestinationField.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.DestinationField.FillWeight = 59.0863F;
            this.DestinationField.HeaderText = "Destination Field";
            this.DestinationField.Name = "DestinationField";
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Location = new System.Drawing.Point(18, 13);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(41, 13);
            this.lblServer.TabIndex = 7;
            this.lblServer.Text = "Server:";
            // 
            // lblServerName
            // 
            this.lblServerName.AutoSize = true;
            this.lblServerName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblServerName.Location = new System.Drawing.Point(55, 11);
            this.lblServerName.Name = "lblServerName";
            this.lblServerName.Size = new System.Drawing.Size(0, 15);
            this.lblServerName.TabIndex = 8;
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(245, 13);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(56, 13);
            this.lblDatabase.TabIndex = 9;
            this.lblDatabase.Text = "Database:";
            // 
            // lblDatabaseName
            // 
            this.lblDatabaseName.AutoSize = true;
            this.lblDatabaseName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDatabaseName.Location = new System.Drawing.Point(296, 11);
            this.lblDatabaseName.Name = "lblDatabaseName";
            this.lblDatabaseName.Size = new System.Drawing.Size(0, 15);
            this.lblDatabaseName.TabIndex = 10;
            // 
            // frmGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(840, 483);
            this.Controls.Add(this.lblDatabaseName);
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.lblServerName);
            this.Controls.Add(this.lblServer);
            this.Controls.Add(this.gbColumnMappings);
            this.Controls.Add(this.gbDBObjects);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnGenerate);
            this.Name = "frmGenerator";
            this.Text = "Mapping Generator";
            this.Load += new System.EventHandler(this.MappingGeneratorForm_LoadAsync);
            this.gbDBObjects.ResumeLayout(false);
            this.gbDBObjects.PerformLayout();
            this.gbColumnMappings.ResumeLayout(false);
            this.gbColumnMappings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumnMappings)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox clbObjects;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox gbDBObjects;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbCheckAllTables;
        private System.Windows.Forms.GroupBox gbColumnMappings;
        private System.Windows.Forms.DataGridView dgvColumnMappings;
        private System.Windows.Forms.Label lblColumnMappingsHeader;
        private System.Windows.Forms.DataGridViewTextBoxColumn SourceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DestinationField;
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.Label lblServerName;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.Label lblDatabaseName;
    }
}