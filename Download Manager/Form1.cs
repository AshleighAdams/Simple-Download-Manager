using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using VistaControls;

namespace Download_Manager
{
    public partial class Form1 : Form
    {
        Downloader downloader;
        bool Exiting = false;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            VistaControls.Dwm.DwmManager.EnableGlassFrame(this, new VistaControls.Dwm.Margins(30000));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.FillRectangles(Brushes.Black, new Rectangle[] {
		        new Rectangle(0, 0, this.ClientSize.Width, 30000),
		        new Rectangle(this.ClientSize.Width - 30000, 0, 3000, this.ClientSize.Height),
		        new Rectangle(0, this.ClientSize.Height - 30000, this.ClientSize.Width, 30000),
		        new Rectangle(0, 0, 300, this.ClientSize.Height)
	        });
        }


        public Form1()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            downloader = new Downloader(this);
            
            
            //downloader.Add("http://xiatek.org/c0bra/MineView3.zip");
            
        }

        private void niIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DoFocus();
        }
        public void DoFocus()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Focus();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = !Exiting;
        }

        private void niIcon_MouseClick_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DoFocus();
                return;
            }
            //Point pos = Cursor.Position;
            //context.SetBounds(0, 0, Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
            //context.Show(pos);
            

        }

        private void cmExit_Click(object sender, EventArgs e)
        {
            this.Exiting = true;
            Application.Exit();
        }

        private void cmDownload_Click(object sender, EventArgs e)
        {
            NewDownload frm = new NewDownload();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                downloader.Add(frm.URL);
            }
        }
    }
}
