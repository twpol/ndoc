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
		private System.Windows.Forms.TextBox m_messageTextBox;
		private System.Windows.Forms.Button m_closeButton;
		private System.Windows.Forms.Label m_stackTraceLabel;
		private System.Windows.Forms.TextBox m_stackTraceTextBox;
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
			this.m_messageTextBox = new System.Windows.Forms.TextBox();
			this.m_closeButton = new System.Windows.Forms.Button();
			this.m_stackTraceLabel = new System.Windows.Forms.Label();
			this.m_stackTraceTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// m_messageTextBox
			// 
			this.m_messageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_messageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_messageTextBox.Location = new System.Drawing.Point(16, 12);
			this.m_messageTextBox.Multiline = true;
			this.m_messageTextBox.Name = "m_messageTextBox";
			this.m_messageTextBox.ReadOnly = true;
			this.m_messageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_messageTextBox.Size = new System.Drawing.Size(432, 116);
			this.m_messageTextBox.TabIndex = 6;
			this.m_messageTextBox.Text = "";
			// 
			// m_closeButton
			// 
			this.m_closeButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.m_closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_closeButton.Location = new System.Drawing.Point(360, 316);
			this.m_closeButton.Name = "m_closeButton";
			this.m_closeButton.TabIndex = 4;
			this.m_closeButton.Text = "&Close";
			// 
			// m_stackTraceLabel
			// 
			this.m_stackTraceLabel.Location = new System.Drawing.Point(8, 132);
			this.m_stackTraceLabel.Name = "m_stackTraceLabel";
			this.m_stackTraceLabel.Size = new System.Drawing.Size(88, 16);
			this.m_stackTraceLabel.TabIndex = 8;
			this.m_stackTraceLabel.Text = "Stack Trace:";
			// 
			// m_stackTraceTextBox
			// 
			this.m_stackTraceTextBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_stackTraceTextBox.Location = new System.Drawing.Point(8, 148);
			this.m_stackTraceTextBox.Multiline = true;
			this.m_stackTraceTextBox.Name = "m_stackTraceTextBox";
			this.m_stackTraceTextBox.ReadOnly = true;
			this.m_stackTraceTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.m_stackTraceTextBox.Size = new System.Drawing.Size(440, 160);
			this.m_stackTraceTextBox.TabIndex = 7;
			this.m_stackTraceTextBox.Text = "";
			this.m_stackTraceTextBox.WordWrap = false;
			// 
			// ErrorForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(456, 350);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.m_messageTextBox,
																		  this.m_closeButton,
																		  this.m_stackTraceLabel,
																		  this.m_stackTraceTextBox});
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(400, 300);
			this.Name = "ErrorForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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
