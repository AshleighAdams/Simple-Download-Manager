using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Download_Manager
{
    public partial class NewDownload : Form
    {
        public string URL;

        public NewDownload()
        {
            InitializeComponent();
        }

        private void NewDownload_Load(object sender, EventArgs e)
        {

        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            
        }

        private void btnDownload_Click_1(object sender, EventArgs e)
        {
            URL = tbURL.Text;
            if (URL.Length > 0)
                DialogResult = DialogResult.OK;
            else
                DialogResult = DialogResult.Abort;
            Close();
        }
    }
}
