using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CaseLocator;

namespace CaseDownloader
{
	public class frmCaseDownloader : Form
    {
        #region fields
        private WindowsFormsSynchronizationContext mUiContext;

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

		private DataGridViewTextBoxColumn colStatus;

        #endregion

        #region constructors

        public frmCaseDownloader()
		{
			this.InitializeComponent();
		}

        #endregion

        #region events

        private void btnDownload_Click(object sender, EventArgs e)
		{
			int count;
			this.mUiContext = new WindowsFormsSynchronizationContext();
			if (this.txtRefNum.Text == "")
			{
				MessageBox.Show("Enter Case Number");
			}
			else if (this.txtUserName.Text == "")
			{
				MessageBox.Show("Enter User Name");
			}
			else if (this.txtPassword.Text == "")
			{
                MessageBox.Show("Password");
            }
            else
            {
                Locate locator = new Locate();
                string username = txtUserName.Text;
                string password = txtPassword.Text;
                bool is_logged_in = locator.Login(username,password);
                if (!is_logged_in)
                {
                    locator.logout();
                    MessageBox.Show("Login Failed Please Enter Valid credentials");
                    return;
                }
				this.btnDownload.Enabled = false;
				List<string> strs = new List<string>();
				string text = this.txtRefNum.Text.ToUpper();
				if (text.Contains(","))
				{
					strs = text.Split(new char[] { ',' }).ToList<string>();
				}
				else if (!text.Contains("-"))
				{
					strs.Add(text);
				}
				else
				{
					List<int> leng = new List<int>();
					strs = text.Split(new char[] { '-' }).ToList<string>();
					string[] sub = new string[0];
					for (int i = 0; i < strs.Count; i++)
					{
						sub = strs[i].Split(new char[] { 'C' });
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
						strs.Add(string.Concat("C", i));
					}
				}
				strs = strs.Distinct<string>().ToList<string>();
				strs = (
					from x in strs
					orderby x
					select x).ToList<string>();
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
					Task.Factory.StartNew<ParallelLoopResult>(() => Parallel.ForEach<string>(strs, new ParallelOptions()
					{
						MaxDegreeOfParallelism = Convert.ToInt32(this.numThreads.Value)
					}, (string refNum) => {
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
                            result = locator.LocateCase(refNum, this.grdCases);
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
                        locator.logout();
					})).ContinueWith((Task<ParallelLoopResult> tsk) => this.EndTweets(tsk, now));
				}
				else
				{
					count = strs.Count;
					MessageBox.Show(string.Concat("You are trying to download ", count.ToString(), " cases.  Please select 5000 or less."));
				}
			}
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
            ((System.ComponentModel.ISupportInitialize)(this.numThreads)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grdCases)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(845, 12);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(113, 23);
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
            this.txtRefNum.Text = "A238989";
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
            this.txtUserName.Text = "tclaus@rgrouplaw.com";
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
            this.txtPassword.Text = "Tacannie1@";
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
            2,
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
            this.grdCases.Location = new System.Drawing.Point(15, 68);
            this.grdCases.MultiSelect = false;
            this.grdCases.Name = "grdCases";
            this.grdCases.ReadOnly = true;
            this.grdCases.RowHeadersVisible = false;
            this.grdCases.Size = new System.Drawing.Size(468, 575);
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
            this.lblStart.Location = new System.Drawing.Point(318, 46);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(0, 13);
            this.lblStart.TabIndex = 20;
            // 
            // lblFinish
            // 
            this.lblFinish.AutoSize = true;
            this.lblFinish.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblFinish.Location = new System.Drawing.Point(447, 46);
            this.lblFinish.Name = "lblFinish";
            this.lblFinish.Size = new System.Drawing.Size(0, 13);
            this.lblFinish.TabIndex = 21;
            // 
            // lblCompleted
            // 
            this.lblCompleted.AutoSize = true;
            this.lblCompleted.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblCompleted.Location = new System.Drawing.Point(39, 46);
            this.lblCompleted.Name = "lblCompleted";
            this.lblCompleted.Size = new System.Drawing.Size(0, 13);
            this.lblCompleted.TabIndex = 22;
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblTotal.Location = new System.Drawing.Point(74, 46);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(0, 13);
            this.lblTotal.TabIndex = 23;
            // 
            // txtConsole
            // 
            this.txtConsole.Location = new System.Drawing.Point(489, 68);
            this.txtConsole.Multiline = true;
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.ReadOnly = true;
            this.txtConsole.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtConsole.Size = new System.Drawing.Size(469, 575);
            this.txtConsole.TabIndex = 24;
            this.txtConsole.Text = "tclaus@rgrouplaw.com";
            // 
            // frmCaseDownloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(971, 655);
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

        #endregion
    }
}