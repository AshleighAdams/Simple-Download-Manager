using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using VistaControls;
using System.Diagnostics;

namespace Download_Manager
{
    class Download
    {
        public string URL;
        public string Name;
        public string SaveTo;
        public long BPS;
        public Panel _Panel;
        private VistaControls.ProgressBar _Pb;
        private VistaControls.ThemeText.ThemedLabel _LblSpeed;
        private VistaControls.ThemeText.ThemedLabel _LblName;
        private VistaControls.ThemeText.ThemedLabel _LblPercent;
        private Timer _TimerStart;
        
        private WebClient _Client;
        private DateTime _LastUpdateTime;
        private long _LastUpdateBytes;
        private VistaControls.Button _BtnCancel;
        bool stoped = false;

        public Download(string url, string saveto, Panel panel, VistaControls.ProgressBar pb, VistaControls.ThemeText.ThemedLabel name,
            VistaControls.ThemeText.ThemedLabel speed,
            VistaControls.ThemeText.ThemedLabel percent,
            VistaControls.Button cancel)
        {
            _Panel = panel;
            this.URL = url;
            this.Name = Path.GetFileName(url);
            this.SaveTo = saveto;
            this._Pb = pb;
            this._LblPercent = percent;
            this._LblName = name;
            this._LblSpeed = speed;
            this._BtnCancel = cancel;

            _LblName.Text = Name;
            _LblPercent.Text = "0% 0B/0B";
            _LblSpeed.Text = "0B/s";

            this._Client = new WebClient();
            _Client.UseDefaultCredentials = true;
            
            avg = new long[1000];
        }

        public void RemoveControls()
        {
            _Pb.Dispose();
            _BtnCancel.Dispose();
            _LblName.Dispose();
            _LblPercent.Dispose();
            _LblSpeed.Dispose();
        }

        public void Start(DateTime time, Action action)
        {
            this._TimerStart = new Timer();
            int mils = (int)(time - DateTime.Now).TotalMilliseconds;
            if (mils < 1)
                mils = 1;
            this._TimerStart.Interval = mils;
            this._TimerStart.Tick += delegate(object sender, EventArgs e)
            {
                this._TimerStart.Enabled = false;
                this._Pb.Style = ProgressBarStyle.Marquee;
                this._LblSpeed.Text = "Starting download...";


                _Client.DownloadProgressChanged += delegate(object s, DownloadProgressChangedEventArgs args)
                {
                    if (stoped)
                        return;
                    double ms = (DateTime.Now - _LastUpdateTime).TotalMilliseconds;
                    if (_LastUpdateTime.Year != 1 && ms < 200)
                        return;
                    double sec = (DateTime.Now - _LastUpdateTime).TotalSeconds;
                    long bytesdone = args.BytesReceived - _LastUpdateBytes;
                    _LastUpdateBytes = args.BytesReceived;
                    _LastUpdateTime = DateTime.Now;
                    long bytespersec = (long)(bytesdone / sec);
                    BPS = _DoAverage(bytespersec);

                    _LblSpeed.Text = BytesToString(BPS) + "/s";
                    _LblPercent.Text = String.Format("{0}  {1}/{2}",
                        args.ProgressPercentage.ToString() + "%",
                        BytesToString(args.BytesReceived),
                        BytesToString(args.TotalBytesToReceive));


                    _Pb.Style = ProgressBarStyle.Continuous;
                    _Pb.Value = args.ProgressPercentage;
                    //_Lbl.Text = args.ProgressPercentage.ToString() + "%";


                };
                _Client.DownloadFileCompleted += delegate(object s, System.ComponentModel.AsyncCompletedEventArgs args)
                {
                    Exception err = args.Error;
                    if (err != null)
                    {
                        if (err.Message == "The request was aborted: The request was canceled.")
                            return;
                        Error(err);
                        return;
                    }
                    action.Invoke();
                };
                try
                {
                    _Client.DownloadFileAsync(new Uri(URL), SaveTo);
                }
                catch(Exception err)
                {
                    Error(err);
                    return;
                }
            };
            this._TimerStart.Enabled = true;
        }
        public void Error(Exception err)
        {
            _Pb.Style = ProgressBarStyle.Blocks;
            _Pb.Value = 100;
            _Pb.ProgressState = VistaControls.ProgressBar.States.Error;
            _LblPercent.Text = err.Message;
            _LblPercent.Width = 406;
            _LblPercent.Location = new System.Drawing.Point(0, _LblPercent.Top);
            _LblSpeed.Dispose();
            
        }
        public void Stop()
        {
            _Client.CancelAsync();
            _Client.Dispose();
            stoped = true;
        }

        int pos = 0;
        private long[] avg;
        private const int avgsize = 25;
        private int _DoAverage(long newint)
        {
            avg[++pos] = newint;
            pos = pos % avgsize;

            long total = 0;
            foreach (int val in avg)
                total += val;
            return (int)(total / avgsize);

        }
        public static string BytesToString(long bytes)
        {
            int n = 0;
            double val = bytes;
            while (val > 1024)
            {
                val /= 1024;
                n++;
            }
            val = Math.Round((double)val * 100.0) / 100;
            switch (n)
            {
                case 1:
                    return val.ToString() + "KB";
                case 2:
                    return val.ToString() + "MB";
                case 3:
                    return val.ToString() + "GB";
                case 4:
                    return val.ToString() + "TB";
                case 5:
                    return val.ToString() + "PB";
                default:
                    return val.ToString() + "B";
            }
        }
    }

    class Downloader
    {
        public Form1 _Form;
        public LinkedList<Download> downloads;
        public LinkedList<Panel> panels;
        public Downloader(Form1 form)
        {
            _Form = form;
            
            downloads = new LinkedList<Download>();
            panels = new LinkedList<Panel>();
            RePosition();
        }
        public void Focus()
        {
            _Form.DoFocus();
        }
        public void RePosition()
        {
            if (panels.Count != 0)
                Focus();
            
            int lastposphight = 0;
            int y = 0;
            int i = 0;
            foreach (Panel pan in panels)
            {
                y = (pan.Size.Height - 10) * i;
                pan.Location = new System.Drawing.Point(12, y);
                i++;
                lastposphight = y + pan.Height;
            }
            _Form.Height = lastposphight + 25;
        }

        public void Add(string url)
        {
            #region GENERATED CODE
            VistaControls.ProgressBar pbPercent = new VistaControls.ProgressBar();
            VistaControls.ThemeText.ThemedLabel lblName = new VistaControls.ThemeText.ThemedLabel();
            VistaControls.ThemeText.ThemedLabel lblPercent = new VistaControls.ThemeText.ThemedLabel();
            VistaControls.ThemeText.ThemedLabel lblSizeInfo = new VistaControls.ThemeText.ThemedLabel();
            VistaControls.Button btnCancel = new VistaControls.Button();
            
            

            System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel();
            panel.SuspendLayout();
            // 
            // pbPercent
            // 
            pbPercent.Location = new System.Drawing.Point(3, 40);
            pbPercent.Name = "pbPercent";
            pbPercent.Size = new System.Drawing.Size(373, 23);
            pbPercent.TabIndex = 6;
            // 
            // lblName
            // 
            lblName.BorderSize = 10;
            lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lblName.Location = new System.Drawing.Point(3, 4);
            lblName.Name = "lblName";
            lblName.Padding = new System.Windows.Forms.Padding(10, 9, 0, 0);
            lblName.Size = new System.Drawing.Size(334, 34);
            lblName.TabIndex = 9;
            lblName.Text = "Loading";
            //
            // btnCancel
            //
            btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnCancel.Image = Download_Manager.Properties.Resources.icon_cross.ToBitmap();  
            btnCancel.Location = new System.Drawing.Point(376-22, 10);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(22, 23);
            btnCancel.TabIndex = 11;
            btnCancel.Text = " ";
            btnCancel.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Parent = panel;
            btnCancel.Show();
            // 
            // lblPercent
            // 
            lblPercent.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lblPercent.Location = new System.Drawing.Point(3, 63);
            lblPercent.Name = "lblPercent";
            lblPercent.Size = new System.Drawing.Size(196, 27);
            lblPercent.TabIndex = 7;
            lblPercent.Text = ".............";
            lblPercent.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblSizeInfo
            // 
            lblSizeInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lblSizeInfo.Location = new System.Drawing.Point(205, 63);
            lblSizeInfo.Name = "lblSizeInfo";
            lblSizeInfo.Size = new System.Drawing.Size(172, 27);
            lblSizeInfo.TabIndex = 8;
            lblSizeInfo.Text = ".............";
            lblSizeInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // panel
            // 
            panel.BackColor = System.Drawing.Color.Transparent;
            panel.Controls.Add(pbPercent);
            panel.Controls.Add(lblSizeInfo);
            panel.Controls.Add(lblName);
            //panel.Controls.Add(btnCancel);
            panel.Controls.Add(lblPercent);
            panel.ForeColor = System.Drawing.Color.Black;
            panel.Location = new System.Drawing.Point(12, 12);
            panel.Name = "panel";
            panel.Size = new System.Drawing.Size(376, 99);
            panel.TabIndex = 10;
            #endregion

            panel.Parent = this._Form;

            //btnCancel.Parent = _Form;
            Download dl = new Download(url, "", panel, pbPercent, lblName, lblSizeInfo, lblPercent, btnCancel);
            dl.SaveTo = Environment.GetEnvironmentVariable("USERPROFILE") + Path.DirectorySeparatorChar + "Downloads" + Path.DirectorySeparatorChar + dl.Name;
            btnCancel.Click += delegate(object sender, EventArgs e)
            {
                dl.Stop();
                dl.RemoveControls();
                panels.Remove(panel);
                panel.Dispose();
                RePosition();
            };
            
            DateTime start = DateTime.Now;
            dl.Start(start, delegate()
            {
                dl.RemoveControls();
                CommandLink btn = new CommandLink();
                btn.Parent = dl._Panel;
                btn.SetBounds(1, 1, dl._Panel.Width - 1, dl._Panel.Height - 10);
                btn.Text = "Open";
                btn.Note = "Open " + dl.Name;
                btn.Click += delegate(object sender, EventArgs args)
                {
                    try
                    {
                        ProcessStartInfo info = new ProcessStartInfo(dl.SaveTo);
                        Process.Start(info);
                    }
                    catch { }
                    dl.RemoveControls();
                    panels.Remove(dl._Panel);
                    dl._Panel.Dispose();
                    RePosition();
                };

            });

            downloads.AddLast(dl);
            panels.AddLast(panel);
            RePosition();
        }
    }
}
