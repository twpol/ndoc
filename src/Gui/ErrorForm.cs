using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;

namespace NDoc.Gui
{
	/// <summary>
	/// Summary description for ErrorForm.
	/// </summary>
	public class ErrorForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.TextBox m_stackTraceTextBox;
		private System.Windows.Forms.Label m_stackTraceLabel;
		private System.Windows.Forms.Button m_closeButton;
		private System.Windows.Forms.TextBox m_messageTextBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		internal ErrorForm(string message, Exception ex)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			StringBuilder strBld = new StringBuilder();
			if ((message != null) && (message.Length > 0))
			{
				strBld.Append(message);
			}

			if (ex != null)
			{
				strBld.Append("\n\n");
				Exception tmpEx = ex;
				while (tmpEx != null)
				{
					strBld.AppendFormat("Exception: {0}\n", tmpEx.GetType().ToString());
					strBld.Append(tmpEx.Message);
					strBld.Append("\n\n");
					tmpEx = tmpEx.InnerException;
				}
			}
			string[] lines = strBld.ToString().Split('\n');
			m_messageTextBox.Lines = lines;

			if (ex != null) 
			{
				strBld.Remove(0, strBld.Length);
				Exception tmpEx = ex;
				while (tmpEx != null)
				{
					strBld.AppendFormat("Exception: {0}\n", tmpEx.GetType().ToString());
					strBld.Append(tmpEx.StackTrace);
					strBld.Append("\n\n");
					tmpEx = tmpEx.InnerException;
				}
				lines = strBld.ToString().Split('\n');
				m_stackTraceTextBox.Lines = lines;
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ErrorForm));
			this.m_stackTraceLabel = new System.Windows.Forms.Label();
			this.m_stackTraceTextBox = new System.Windows.Forms.TextBox();
			this.m_closeButton = new System.Windows.Forms.Button();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.m_messageTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// m_stackTraceLabel
			// 
			this.m_stackTraceLabel.Location = new System.Drawing.Point(8, 136);
			this.m_stackTraceLabel.Name = "m_stackTraceLabel";
			this.m_stackTraceLabel.Size = new System.Drawing.Size(88, 16);
			this.m_stackTraceLabel.TabIndex = 3;
			this.m_stackTraceLabel.Text = "Stack Trace:";
			// 
			// m_stackTraceTextBox
			// 
			this.m_stackTraceTextBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_stackTraceTextBox.Location = new System.Drawing.Point(8, 152);
			this.m_stackTraceTextBox.Multiline = true;
			this.m_stackTraceTextBox.Name = "m_stackTraceTextBox";
			this.m_stackTraceTextBox.ReadOnly = true;
			this.m_stackTraceTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.m_stackTraceTextBox.Size = new System.Drawing.Size(392, 128);
			this.m_stackTraceTextBox.TabIndex = 2;
			this.m_stackTraceTextBox.Text = "";
			this.m_stackTraceTextBox.WordWrap = false;
			// 
			// m_closeButton
			// 
			this.m_closeButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.m_closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_closeButton.Location = new System.Drawing.Point(312, 288);
			this.m_closeButton.Name = "m_closeButton";
			this.m_closeButton.TabIndex = 0;
			this.m_closeButton.Text = "&Close";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Bitmap)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(16, 16);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(32, 40);
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			// 
			// m_messageTextBox
			// 
			this.m_messageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_messageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_messageTextBox.Location = new System.Drawing.Point(62, 16);
			this.m_messageTextBox.Multiline = true;
			this.m_messageTextBox.Name = "m_messageTextBox";
			this.m_messageTextBox.ReadOnly = true;
			this.m_messageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_messageTextBox.Size = new System.Drawing.Size(336, 104);
			this.m_messageTextBox.TabIndex = 1;
			this.m_messageTextBox.Text = "";
			// 
			// ErrorForm
			// 
			this.AcceptButton = this.m_closeButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.m_closeButton;
			this.ClientSize = new System.Drawing.Size(408, 318);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.m_messageTextBox,
																		  this.m_closeButton,
																		  this.m_stackTraceLabel,
																		  this.m_stackTraceTextBox,
																		  this.pictureBox1});
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(400, 300);
			this.Name = "ErrorForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "NDoc Error";
			this.Load += new System.EventHandler(this.ErrorForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void ErrorForm_Load(object sender, System.EventArgs e)
		{
			m_closeButton.Focus();
		}
	}
}
