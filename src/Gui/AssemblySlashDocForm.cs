// AssemblySlashDocForm.cs - form for adding assembly and /doc filename pairs
// Copyright (C) 2001  Kral Ferch
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

namespace NDoc.Gui
{
	using System;
	using System.Drawing;
	using System.Collections;
	using System.ComponentModel;
	using System.IO;
	using System.Windows.Forms;

	/// <summary>
	///    This form allows the user to select an assembly and it's matching /doc file.
	/// </summary>
	public class AssemblySlashDocForm : System.Windows.Forms.Form
	{
		/// <summary>
		///    Required designer variable.
		/// </summary>
		private System.Windows.Forms.Button slashDocButton;
		private System.Windows.Forms.Button assemblyButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.TextBox slashDocTextBox;
		private System.Windows.Forms.TextBox assemblyTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;

		/// <summary>Initializes a new instance of the AssemblySlashDocForm class.</summary>
		public AssemblySlashDocForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			okButton.Enabled = false;
		}

		/// <summary>Gets or sets the filename of the assembly to document.</summary>
		public string AssemblyFilename
		{
			get { return assemblyTextBox.Text; }
			
			set 
			{ 
				assemblyTextBox.Text = value; 
				CheckOKEnable();
			}
		}

		/// <summary>Gets or sets the filename of the /doc file associated with the assembly to document.</summary>
		public string SlashDocFilename
		{
			get { return slashDocTextBox.Text; }

			set 
			{ 
				slashDocTextBox.Text = value; 
				CheckOKEnable();
			}
		}

		/// <summary>Clean up any resources being used.</summary>
		public override void Dispose()
		{
			base.Dispose();
		}

		/// <summary>
		///    Required method for Designer support - do not modify
		///    the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.assemblyTextBox = new System.Windows.Forms.TextBox();
			this.slashDocTextBox = new System.Windows.Forms.TextBox();
			this.assemblyButton = new System.Windows.Forms.Button();
			this.slashDocButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.okButton.Location = new System.Drawing.Point(116, 102);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 4;
			this.okButton.Text = "OK";
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelButton.Location = new System.Drawing.Point(204, 102);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			// 
			// assemblyTextBox
			// 
			this.assemblyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.assemblyTextBox.Location = new System.Drawing.Point(120, 24);
			this.assemblyTextBox.Name = "assemblyTextBox";
			this.assemblyTextBox.Size = new System.Drawing.Size(242, 20);
			this.assemblyTextBox.TabIndex = 2;
			this.assemblyTextBox.Text = "";
			// 
			// slashDocTextBox
			// 
			this.slashDocTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.slashDocTextBox.Location = new System.Drawing.Point(120, 64);
			this.slashDocTextBox.Name = "slashDocTextBox";
			this.slashDocTextBox.Size = new System.Drawing.Size(242, 20);
			this.slashDocTextBox.TabIndex = 3;
			this.slashDocTextBox.Text = "";
			// 
			// assemblyButton
			// 
			this.assemblyButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.assemblyButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.assemblyButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.assemblyButton.Location = new System.Drawing.Point(362, 24);
			this.assemblyButton.Name = "assemblyButton";
			this.assemblyButton.Size = new System.Drawing.Size(16, 20);
			this.assemblyButton.TabIndex = 6;
			this.assemblyButton.Text = "...";
			this.assemblyButton.Click += new System.EventHandler(this.assemblyButton_Click);
			// 
			// slashDocButton
			// 
			this.slashDocButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.slashDocButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.slashDocButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.slashDocButton.Location = new System.Drawing.Point(362, 64);
			this.slashDocButton.Name = "slashDocButton";
			this.slashDocButton.Size = new System.Drawing.Size(16, 20);
			this.slashDocButton.TabIndex = 7;
			this.slashDocButton.Text = "...";
			this.slashDocButton.Click += new System.EventHandler(this.slashDocButton_Click);
			// 
			// label1
			// 
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label1.Location = new System.Drawing.Point(16, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "Assembly Filename:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.label2.Location = new System.Drawing.Point(16, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(96, 20);
			this.label2.TabIndex = 1;
			this.label2.Text = "XML Doc Filename:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// AssemblySlashDocForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(394, 148);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.slashDocButton,
																		  this.assemblyButton,
																		  this.cancelButton,
																		  this.okButton,
																		  this.slashDocTextBox,
																		  this.assemblyTextBox,
																		  this.label2,
																		  this.label1});
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(402, 182);
			this.Name = "AssemblySlashDocForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Select Assembly and XML Documentation File";
			this.ResumeLayout(false);

		}

		/// <summary>
		/// If a valid assembly filename and a valid /doc filename are in place then
		/// this routine enables the Ok button.
		/// </summary>
		protected void CheckOKEnable()
		{
			if (SlashDocFilename != "" && AssemblyFilename != "")
			{
				okButton.Enabled = true;
			}
		}

		/// <summary>
		/// Brings up a dialog for browsing the hard drive to select a /doc filename
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void slashDocButton_Click (object sender, System.EventArgs e)
		{
			OpenFileDialog  openFileDlg = new OpenFileDialog();

			openFileDlg.Filter = "/doc Output files (*.xml)|*.xml|All files (*.*)|*.*" ;

			if(openFileDlg.ShowDialog() == DialogResult.OK)
			{
				SlashDocFilename = openFileDlg.FileName;
			}

			CheckOKEnable();
		}

		/// <summary>Brings up a dialog for browsing the hard drive to 
		/// select an assembly filename.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void assemblyButton_Click (object sender, System.EventArgs e)
		{
			OpenFileDialog openFileDlg = new OpenFileDialog();

			openFileDlg.Filter = "Library and Executable files (*.dll, *.exe)|*.dll;*.exe|Library files (*.dll)|*.dll|Executable files (*.exe)|*.exe|All files (*.*)|*.*" ;

			if(openFileDlg.ShowDialog() == DialogResult.OK)
			{
				AssemblyFilename = openFileDlg.FileName;

				if ((AssemblyFilename.Length > 4) & (SlashDocFilename == ""))
				{
					string slashDocFilename = AssemblyFilename.Substring(0, AssemblyFilename.Length-4) + ".xml";

					if (File.Exists(slashDocFilename))
					{
						SlashDocFilename = slashDocFilename;
					}
				}
			}

			CheckOKEnable();
		}
	}
}
