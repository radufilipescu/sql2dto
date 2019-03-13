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
    public partial class frmDisplayMappingResult : Form
    {
        public frmDisplayMappingResult()
        {
            InitializeComponent();
        }

        public void SetMappingResult(string text)
        {
            tbGeneratorResult.Text = text;
            //Remove textbox highlight
            this.tbGeneratorResult.SelectionStart = 0;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.DefaultExt = "txt";
            save.AddExtension = true;

            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamWriter write = new StreamWriter(File.Create(save.FileName)))
                {
                    write.Write(tbGeneratorResult.Text);
                }
            }

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
