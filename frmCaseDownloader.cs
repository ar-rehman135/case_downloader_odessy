using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CaseDownloader
{
	public class frmCaseDownloader : Form
    {
        #region fields
        private WindowsFormsSynchronizationContext mUiContext;

        private Project prj;

        List<Locate> listLoacte;

        string ProfileFileName = "user_profile.info";

		private IContainer components = null;

		private Button btnDownload;

		private TextBox txtRefNum;

		private Label label2;

		private TextBox txtUserName;

		private Label label1;

		private Label label3;

		private TextBox txtPassword;

		private NumericUpDown numThreads;

		private Label label4;

		private DataGridView grdCases;

		private Label lblStart;

		private Label lblFinish;

		private Label lblCompleted;

		private Label lblTotal;

		private TextBox txtConsole;

		private DataGridViewTextBoxColumn colRefNum;

		private DataGridViewTextBoxColumn colCaseCount;
        private TextBox textBox1;
        private Label label5;
        private Button button1;
        private FolderBrowserDialog folderBrowserDialog1;
        private Button button2;
        private Button button3;
        private CheckBox checkBox1;

		private DataGridViewTextBoxColumn colStatus;

        #endregion

        #region constructors

        public frmCaseDownloader()
		{
			this.InitializeComponent();
             listLoacte = new List<Locate>();
		}

        #endregion

        #region events

        private void btnDownload_Click(object sender, EventArgs e)
		{
			int count;
			this.mUiContext = new WindowsFormsSynchronizationContext();
            string path = "";
            if (this.txtRefNum.Text == "")
			{
				MessageBox.Show("Enter Case Number");
                return;
			}
			else if (this.txtUserName.Text == "")
			{
				MessageBox.Show("Enter User Name");
                return;
			}
			else if (this.txtPassword.Text == "")
			{
                MessageBox.Show("Password");
                return;
            }
            else if(this.textBox1.Text == "")
            {
                MessageBox.Show("Please Select path for downloading Cases");
                return;
            }
            else
            {
                
				this.btnDownload.Enabled = false;
                string username = txtUserName.Text;
                string password = txtPassword.Text;
                path = textBox1.Text;
                prj.Username = username;
                prj.Password = password;
                prj.Path = path;
                prj.thread_count = (int)this.numThreads.Value;

                if (checkBox1.CheckState == CheckState.Checked)
                {

                }

                string text = this.txtRefNum.Text.ToUpper();
                List<string> strs = calculateCases(text);
				if (strs.Count <= 5000)
				{
					this.lblCompleted.Text = "0";
					Label label = this.lblTotal;
					count = strs.Count;
					label.Text = string.Concat("/  ", count.ToString());
					Thread.Sleep(100);
					this.grdCases.Visible = true;
					this.grdCases.Rows.Clear();
					foreach (string str1 in strs)
					{
						this.grdCases.Rows.Add(new object[] { str1 });
					}
					this.grdCases.Refresh();
                    DateTime now = DateTime.Now;
                    TimeSpan procTimeTot = new TimeSpan();
					this.lblStart.Text = string.Concat("Started at: ", now.ToShortTimeString());
					this.lblFinish.Text = "";
					TextBox textBox = this.txtConsole;
					string[] shortTimeString = new string[] { "Starting with ", null, null, null, null, null, null };
					shortTimeString[1] = this.numThreads.Value.ToString();
					shortTimeString[2] = " threads.  Need to download ";
					count = strs.Count;
					shortTimeString[3] = count.ToString();
					shortTimeString[4] = " cross reference numbers. (";
					shortTimeString[5] = now.ToShortTimeString();
					shortTimeString[6] = ")";
					textBox.Text = string.Concat(shortTimeString);

                    start_process(strs, prj, now);
				}
				else
				{
					count = strs.Count;
					MessageBox.Show(string.Concat("You are trying to download ", count.ToString(), " cases.  Please select 5000 or less."));
				}
			}
		}

        private void button1_Click(object sender, EventArgs e)
        {
            var res = folderBrowserDialog1.ShowDialog();
            textBox1.Text = folderBrowserDialog1.SelectedPath;
        }

        private void frmCaseDownloader_Load(object sender, EventArgs e)
        {
            loadProject();
            txtUserName.Text = prj.Username;
            txtPassword.Text = prj.Password;
            numThreads.Value = (decimal)prj.thread_count;
            textBox1.Text = prj.Path;
        }

        private void frmCaseDownloader_FormClosed(object sender, FormClosedEventArgs e)
        {
            saveProject();
        }

        private void frmCaseDownloader_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var locator in listLoacte)
            {
                if (locator != null)
                    locator.Dispose();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }


        #endregion

        #region overrides
        protected override void Dispose(bool disposing)
		{
			if ((!disposing ? false : this.components != null))
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

        #endregion

        #region Private Members

        private void loadProject()
        {
            prj = new Project();
            if (File.Exists(ProfileFileName))
            {
                using (var fd = File.Open(ProfileFileName, FileMode.OpenOrCreate, FileAccess.Read))
                using (StreamReader sfd = new StreamReader(fd))
                {
                    prj.Username = sfd.ReadLine();
                    prj.Password = sfd.ReadLine();
                    prj.Path = sfd.ReadLine();
                    prj.thread_count = Convert.ToInt32(sfd.ReadLine());
                }
            }
        }

        private void saveProject()
        {
            if(checkBox1.Checked)
            {
                using(var fd = File.Open(ProfileFileName,FileMode.OpenOrCreate,FileAccess.Write))
                using (StreamWriter sfd = new StreamWriter(fd))
                {
                    sfd.WriteLine(prj.Username);
                    sfd.WriteLine(prj.Password);
                    sfd.WriteLine(prj.Path);
                    sfd.WriteLine(prj.thread_count);
                }
            }
        }

        private List<string> calculateCases(string text)
        {
            List<string> strs = new List<string>();
            var strarr = text.Split(new char[] { ',' }).ToList<string>();
            foreach (var str in strarr)
            {
                if (!str.Contains("-"))
                {
                    strs.Add(str);
                }
                else
                {
                    List<int> leng = new List<int>();
                    var strs1 = str.Split(new char[] { '-' }).ToList<string>();
                    string[] sub = new string[0];
                    for (int i = 0; i < strs1.Count; i++)
                    {
                        sub = strs1[i].Split(new char[] { 'A' });
                        for (int j = 0; j < (int)sub.Length; j++)
                        {
                            string[] arrf = sub[j].Split(new char[] { ' ' });
                            if (arrf[0] != "")
                            {
                                leng.Add(Convert.ToInt32(arrf[0]));
                            }
                        }
                    }
                    for (int i = leng[0]; i <= leng[1]; i++)
                    {
                        strs1.Add(string.Concat("A", i));
                    }
                    strs.AddRange(strs1);
                }
            }
            strs = strs.Distinct<string>().ToList<string>();
            strs = (
                from x in strs
                orderby int.Parse(x.Substring(1))
                select x).ToList<string>();
            return strs;
        }

        private void start_process( List<string> strs, Project prj, DateTime now)
        {
            var part = Partitioner.Create(strs);
            var task_factory = Task.Factory.StartNew<ParallelLoopResult>(() => Parallel.ForEach(part, new ParallelOptions()
            {
                MaxDegreeOfParallelism = prj.thread_count
            }, (refNum) =>
            {
                //string refNum = strs[i_str];
                //ShowMessageBox("  "+refNum);
                DataGridViewRow row = (
                    from DataGridViewRow dataGridViewRow in this.grdCases.Rows
                    where dataGridViewRow.Cells[0].Value.ToString().Equals(refNum)
                    select dataGridViewRow).First<DataGridViewRow>();
                TimeSpan procTime = new TimeSpan();
                string result = null;
                while (result != "0")
                {
                    WindowsFormsSynchronizationContext windowsFormsSynchronizationContext = this.mUiContext;
                    SendOrPostCallback sendOrPostCallback = new SendOrPostCallback(this.UpdateGUIConsole);
                    string str = refNum.ToString();
                    int managedThreadId = Thread.CurrentThread.ManagedThreadId;
                    windowsFormsSynchronizationContext.Post(sendOrPostCallback, string.Concat("Processing ", str, " on thread ", managedThreadId.ToString()));
                    if (row.Cells[2].Value != null)
                    {
                        DataGridViewCell item = row.Cells[2];
                        managedThreadId = Thread.CurrentThread.ManagedThreadId;
                        item.Value = string.Concat("Re-Processing on thread ", managedThreadId.ToString());
                    }
                    else
                    {
                        DataGridViewCell dataGridViewCell = row.Cells[2];
                        managedThreadId = Thread.CurrentThread.ManagedThreadId;
                        dataGridViewCell.Value = string.Concat("Processing on thread ", managedThreadId.ToString());
                    }
                    this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUI), null);
                    DateTime begin = DateTime.Now;
                    Locate locator = new Locate();
                    listLoacte.Add(locator);
                    locator.showMessage = ShowMessageBox;
                    bool is_logged_in = locator.Login(prj.Username, prj.Password);
                    if (!is_logged_in)
                    {
                        result = "Login Failed";
                        locator.quit();
                    }
                    else
                    {
                        result = locator.LocateCase(refNum, this.grdCases, prj.Path);
                        locator.logout();
                    }
                    procTime = DateTime.Now - begin;
                    if (result != "0")
                    {
                        row.Cells[2].Value = "Error Processing";
                        this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUI), null);
                        this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUIConsole), string.Concat("Error Processing ", refNum.ToString(), ":"));
                        this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUIConsole), result);
                    }
                }
                row.Cells[2].Value = string.Concat(new object[] { "Completed in ", procTime.Minutes, " minutes ", procTime.Seconds, " seconds" });
                this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUIConsole), string.Concat(new object[] { "Completed ", refNum, " in ", procTime.Minutes, " minutes ", procTime.Seconds, " seconds" }));
                this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUIComplete), null);
                this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUI), null);
                Thread.Sleep(100);
            })).ContinueWith((Task<ParallelLoopResult> tsk) => this.EndTweets(tsk, now));
        }

        private void EndTweets(Task tsk, DateTime start)
		{
			TimeSpan procTimeTot = DateTime.Now - start;
			this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUI), null);
			this.mUiContext.Post(new SendOrPostCallback(this.UpdateFinish), string.Concat(new object[] { "Processing complete. Total time: ", procTimeTot.Hours, " hours ", procTimeTot.Minutes, " minutes ", procTimeTot.Seconds, " seconds" }));
			this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUIConsole), string.Concat(new object[] { "Processing complete. Total time: ", procTimeTot.Hours, " hours ", procTimeTot.Minutes, " minutes ", procTimeTot.Seconds, " seconds" }));
		}

		private void grdCases_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
		}

		private void InitializeComponent()
		{
            this.btnDownload = new System.Windows.Forms.Button();
            this.txtRefNum = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.numThreads = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.grdCases = new System.Windows.Forms.DataGridView();
            this.colRefNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCaseCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblStart = new System.Windows.Forms.Label();
            this.lblFinish = new System.Windows.Forms.Label();
            this.lblCompleted = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.txtConsole = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numThreads)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grdCases)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(577, 53);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(115, 29);
            this.btnDownload.TabIndex = 0;
            this.btnDownload.Text = "Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // txtRefNum
            // 
            this.txtRefNum.Location = new System.Drawing.Point(91, 13);
            this.txtRefNum.Name = "txtRefNum";
            this.txtRefNum.Size = new System.Drawing.Size(178, 20);
            this.txtRefNum.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label2.Location = new System.Drawing.Point(12, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Cross Ref. No";
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(349, 12);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(103, 20);
            this.txtUserName.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label1.Location = new System.Drawing.Point(280, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "User Name:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label3.Location = new System.Drawing.Point(472, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Password:";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(534, 13);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(118, 20);
            this.txtPassword.TabIndex = 15;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // numThreads
            // 
            this.numThreads.Location = new System.Drawing.Point(750, 12);
            this.numThreads.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numThreads.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numThreads.Name = "numThreads";
            this.numThreads.Size = new System.Drawing.Size(78, 20);
            this.numThreads.TabIndex = 17;
            this.numThreads.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label4.Location = new System.Drawing.Point(665, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Threads (1-20):";
            // 
            // grdCases
            // 
            this.grdCases.AllowUserToAddRows = false;
            this.grdCases.AllowUserToDeleteRows = false;
            this.grdCases.AllowUserToResizeColumns = false;
            this.grdCases.AllowUserToResizeRows = false;
            this.grdCases.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdCases.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRefNum,
            this.colCaseCount,
            this.colStatus});
            this.grdCases.Location = new System.Drawing.Point(15, 122);
            this.grdCases.MultiSelect = false;
            this.grdCases.Name = "grdCases";
            this.grdCases.ReadOnly = true;
            this.grdCases.RowHeadersVisible = false;
            this.grdCases.Size = new System.Drawing.Size(468, 521);
            this.grdCases.TabIndex = 19;
            this.grdCases.Visible = false;
            this.grdCases.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdCases_CellContentClick);
            // 
            // colRefNum
            // 
            this.colRefNum.HeaderText = "Cross Ref. No";
            this.colRefNum.Name = "colRefNum";
            this.colRefNum.ReadOnly = true;
            // 
            // colCaseCount
            // 
            this.colCaseCount.HeaderText = "Cases";
            this.colCaseCount.Name = "colCaseCount";
            this.colCaseCount.ReadOnly = true;
            this.colCaseCount.Width = 65;
            // 
            // colStatus
            // 
            this.colStatus.HeaderText = "Status";
            this.colStatus.MinimumWidth = 283;
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            this.colStatus.Width = 283;
            // 
            // lblStart
            // 
            this.lblStart.AutoSize = true;
            this.lblStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblStart.Location = new System.Drawing.Point(318, 94);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(0, 13);
            this.lblStart.TabIndex = 20;
            // 
            // lblFinish
            // 
            this.lblFinish.AutoSize = true;
            this.lblFinish.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblFinish.Location = new System.Drawing.Point(447, 94);
            this.lblFinish.Name = "lblFinish";
            this.lblFinish.Size = new System.Drawing.Size(0, 13);
            this.lblFinish.TabIndex = 21;
            // 
            // lblCompleted
            // 
            this.lblCompleted.AutoSize = true;
            this.lblCompleted.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblCompleted.Location = new System.Drawing.Point(39, 94);
            this.lblCompleted.Name = "lblCompleted";
            this.lblCompleted.Size = new System.Drawing.Size(0, 13);
            this.lblCompleted.TabIndex = 22;
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblTotal.Location = new System.Drawing.Point(74, 94);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(0, 13);
            this.lblTotal.TabIndex = 23;
            // 
            // txtConsole
            // 
            this.txtConsole.Location = new System.Drawing.Point(489, 122);
            this.txtConsole.Multiline = true;
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.ReadOnly = true;
            this.txtConsole.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtConsole.Size = new System.Drawing.Size(469, 521);
            this.txtConsole.TabIndex = 24;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(91, 58);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(178, 20);
            this.textBox1.TabIndex = 25;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label5.Location = new System.Drawing.Point(12, 61);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 13);
            this.label5.TabIndex = 26;
            this.label5.Text = "Choose Folder";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(283, 56);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(113, 23);
            this.button1.TabIndex = 27;
            this.button1.Text = "Browse";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(708, 53);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(115, 29);
            this.button2.TabIndex = 28;
            this.button2.Text = "Pause";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(834, 53);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(112, 29);
            this.button3.TabIndex = 29;
            this.button3.Text = "Resume";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(858, 13);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(95, 17);
            this.checkBox1.TabIndex = 30;
            this.checkBox1.Text = "Remember Me";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // frmCaseDownloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(971, 655);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.txtConsole);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.lblCompleted);
            this.Controls.Add(this.lblFinish);
            this.Controls.Add(this.lblStart);
            this.Controls.Add(this.grdCases);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numThreads);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtUserName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtRefNum);
            this.Controls.Add(this.btnDownload);
            this.Name = "frmCaseDownloader";
            this.Text = "Case Downloader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCaseDownloader_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmCaseDownloader_FormClosed);
            this.Load += new System.EventHandler(this.frmCaseDownloader_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numThreads)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grdCases)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private void UpdateFinish(object userData)
		{
			this.lblFinish.Text = userData.ToString();
			this.btnDownload.Enabled = true;
		}

		private void UpdateGUI(object userData)
		{
			this.grdCases.Refresh();
		}

		private void UpdateGUIComplete(object userData)
		{
			if (this.lblCompleted.Text != "")
			{
				this.lblCompleted.Text = Convert.ToString(Convert.ToInt16(this.lblCompleted.Text) + 1);
			}
			else
			{
				this.lblCompleted.Text = "1";
			}
		}

		private void UpdateGUIConsole(object userData)
		{
			TextBox textBox = this.txtConsole;
			textBox.Text = string.Concat(textBox.Text, Environment.NewLine, userData.ToString());
			this.txtConsole.SelectionStart = this.txtConsole.TextLength;
			this.txtConsole.ScrollToCaret();
        }

        private void ShowMessageBox(string data)
        {
            MessageBox.Show(data);
        }

        #endregion

    }
}
