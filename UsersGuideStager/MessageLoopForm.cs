using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace NDoc.UsersGuideStager
{
	/// <summary>
	/// Summary description for MessageLoopForm.
	/// </summary>
	public class MessageLoopForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;

		private string[] args;

		public MessageLoopForm( string[] s )
		{
			InitializeComponent();
		
			args = s;
		}

		protected override void OnLoad(EventArgs e)
		{
			try
			{
				if ( args.Length != 0 )
				{
					Stager stager = new Stager( args[0] );
					stager.Stage();
				}
				else
				{
					Console.WriteLine( "Usage: UsersGuideTranslator <path to HHC file>" );
				}
			}
			finally
			{
				this.Close();
			}
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 64);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(240, 136);
			this.label1.TabIndex = 0;
			this.label1.Text = "We need this form because mshtml requires a running message loop";
			// 
			// MessageLoopForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this.label1);
			this.Name = "MessageLoopForm";
			this.ShowInTaskbar = false;
			this.Text = "MessageLoopForm";
			this.ResumeLayout(false);

		}
		#endregion
	}
}
