using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sql2dto.TableMappingGenerator.Forms
{
    public partial class frmProgress : Form
    {
        public string Message
        {
            get
            {
                return lblMessage.Text;
            }

            set
            {
                lblMessage.Text = value;
            }
        }

        public frmProgress()
        {
            InitializeComponent();

            lblMessage.Text = String.Empty;
        }

        public int Increment
        {
            set
            {
                if (progressBar.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate { progressBar.Increment(value); });
                }
                else
                {
                    progressBar.Increment(value);
                }
            }
        }

        public int Step
        {
            get
            {
                return progressBar.Step;
            }
            set
            {
                if (progressBar.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate { progressBar.Step = value; });
                }
                else
                {
                    progressBar.Step = value;
                }
            }
        }

        public int Maximum
        {
            get
            {
                return progressBar.Maximum;
            }
            set
            {
                if (progressBar.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate { progressBar.Maximum = value; });
                }
                else
                {
                    progressBar.Maximum = value;
                }
            }
        }

        public int Value
        {
            get
            {
                return progressBar.Value;
            }
            set
            {
                if (progressBar.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate { progressBar.Value = value; });
                }
                else
                {
                    progressBar.Value = value;
                }
            }
        }

        public ProgressBarStyle Style
        {
            get
            {
                return progressBar.Style;
            }
            set
            {
                if (progressBar.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate { progressBar.Style = value; });
                }
                else
                {
                    progressBar.Style = value;
                }
            }
        }
    }
}
