// AboutForm.cs - About box form for NDoc GUI interface.
// Copyright (C) 2001  Keith Hill
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
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace NDoc.Gui
{
	/// <summary>
	/// Summary description for AboutForm.
	/// </summary>
	public class AboutForm : System.Windows.Forms.Form
	{
		#region Fields
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.RichTextBox richTextBox;
		private System.Windows.Forms.LinkLabel mailLinkLabel;
		private System.Windows.Forms.LinkLabel webLinkLabel;
		private NDoc.Gui.HeaderGroupBox versionHeaderGroupBox;
		private System.Windows.Forms.ColumnHeader assemblyColumnHeader;
		private System.Windows.Forms.ListView assembliesListView;
		private System.Windows.Forms.ColumnHeader versionColumnHeader;
		private NDoc.Gui.HeaderGroupBox contactInfoHeaderGroupBox;
		private System.Windows.Forms.ColumnHeader dateColumnHeader;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion // Fields

		#region Constructor / Dispose
		/// <summary>
		/// 
		/// </summary>
		public AboutForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Read RTF file from manifest resource stream and display it in the
			// RichTextBox.  NOTE: Edit the About.RTF with WordPad or Word.
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream stream = assembly.GetManifestResourceStream("NDoc.Gui.About.rtf");
			richTextBox.LoadFile(stream, RichTextBoxStreamType.RichText);

			// Set up email links
			mailLinkLabel.Links.Add(13, 13, "mailto:jason@injektilo.org");
			mailLinkLabel.Links.Add(28, 10, "mailto:kral_ferch@hotmail.com");
			mailLinkLabel.Links.Add(40, 18, "mailto:jc@manoli.net");
			mailLinkLabel.Links.Add(63, 10, "mailto:r_keith_hill@hotmail.com");

			// Set up web links
			webLinkLabel.Links.Add(12, 12, "http://sourceforge.net/projects/ndoc");
			webLinkLabel.Links.Add(26, 13, "http://sourceforge.net/tracker/?func=add&group_id=36057&atid=416078");
			webLinkLabel.Links.Add(41, 11, "http://sourceforge.net/mail/?group_id=36057");
			webLinkLabel.Links.Add(54, 10, "http://www.opensource.org/");

			// Fill in loaded modules / version number info list view.
			try 
			{
				// Get all modules
				ArrayList ndocItems = new ArrayList();
				foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
				{
					ListViewItem item = new ListViewItem();
					item.Text = module.ModuleName;

					// Get version info
					FileVersionInfo verInfo = module.FileVersionInfo;
					string versionStr = String.Format("{0}.{1}.{2}.{3}", 
						                              verInfo.FileMajorPart,
					                                  verInfo.FileMinorPart,
					                                  verInfo.FileBuildPart,
					                                  verInfo.FilePrivatePart);
					item.SubItems.Add(versionStr);

					// Get file date info
					DateTime lastWriteDate = File.GetLastWriteTime(module.FileName);
					string dateStr = lastWriteDate.ToString("MMM dd, yyyy");
					item.SubItems.Add(dateStr);

					assembliesListView.Items.Add(item);

					// Stash ndoc related list view items for later
					if (module.ModuleName.ToLower().StartsWith("ndoc"))
					{
						ndocItems.Add(item);
					}
				}

				// Extract the NDoc related modules and move them to the top
				for (int i = ndocItems.Count; i > 0; i--)
				{
					ListViewItem ndocItem = (ListViewItem)ndocItems[i-1];
					assembliesListView.Items.Remove(ndocItem);
					assembliesListView.Items.Insert(0, ndocItem);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.ToString(), "NDoc Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		#endregion // Constructor / Dispose

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.mailLinkLabel = new System.Windows.Forms.LinkLabel();
			this.assembliesListView = new System.Windows.Forms.ListView();
			this.assemblyColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.versionColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.dateColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.contactInfoHeaderGroupBox = new NDoc.Gui.HeaderGroupBox();
			this.webLinkLabel = new System.Windows.Forms.LinkLabel();
			this.closeButton = new System.Windows.Forms.Button();
			this.richTextBox = new System.Windows.Forms.RichTextBox();
			this.versionHeaderGroupBox = new NDoc.Gui.HeaderGroupBox();
			this.contactInfoHeaderGroupBox.SuspendLayout();
			this.versionHeaderGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// mailLinkLabel
			// 
			this.mailLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.mailLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.mailLinkLabel.LinkArea = new System.Windows.Forms.LinkArea(0, 0);
			this.mailLinkLabel.Location = new System.Drawing.Point(8, 16);
			this.mailLinkLabel.Name = "mailLinkLabel";
			this.mailLinkLabel.Size = new System.Drawing.Size(504, 16);
			this.mailLinkLabel.TabIndex = 0;
			this.mailLinkLabel.Text = "Developers:  Jason Diamond, Kral Ferch, Jean-Claude Manoli and Keith Hill";
			this.mailLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.mailLinkLabel_LinkClicked);
			// 
			// assembliesListView
			// 
			this.assembliesListView.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.assembliesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																								 this.assemblyColumnHeader,
																								 this.versionColumnHeader,
																								 this.dateColumnHeader});
			this.assembliesListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.assembliesListView.Location = new System.Drawing.Point(8, 16);
			this.assembliesListView.Name = "assembliesListView";
			this.assembliesListView.Size = new System.Drawing.Size(504, 114);
			this.assembliesListView.TabIndex = 0;
			this.assembliesListView.View = System.Windows.Forms.View.Details;
			// 
			// assemblyColumnHeader
			// 
			this.assemblyColumnHeader.Text = "Assembly";
			this.assemblyColumnHeader.Width = 208;
			// 
			// versionColumnHeader
			// 
			this.versionColumnHeader.Text = "Version";
			this.versionColumnHeader.Width = 147;
			// 
			// dateColumnHeader
			// 
			this.dateColumnHeader.Text = "Date";
			this.dateColumnHeader.Width = 124;
			// 
			// contactInfoHeaderGroupBox
			// 
			this.contactInfoHeaderGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																									this.webLinkLabel,
																									this.mailLinkLabel});
			this.contactInfoHeaderGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.contactInfoHeaderGroupBox.Location = new System.Drawing.Point(8, 152);
			this.contactInfoHeaderGroupBox.Name = "contactInfoHeaderGroupBox";
			this.contactInfoHeaderGroupBox.Padding = 0;
			this.contactInfoHeaderGroupBox.Size = new System.Drawing.Size(520, 56);
			this.contactInfoHeaderGroupBox.TabIndex = 5;
			this.contactInfoHeaderGroupBox.TabStop = false;
			this.contactInfoHeaderGroupBox.Text = "Contact Information:";
			// 
			// webLinkLabel
			// 
			this.webLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.webLinkLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.webLinkLabel.LinkArea = new System.Windows.Forms.LinkArea(0, 0);
			this.webLinkLabel.Location = new System.Drawing.Point(8, 32);
			this.webLinkLabel.Name = "webLinkLabel";
			this.webLinkLabel.Size = new System.Drawing.Size(504, 16);
			this.webLinkLabel.TabIndex = 1;
			this.webLinkLabel.Text = "Web Links:  NDoc Project, Submit Defect, List Server, OpenSource";
			this.webLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.webLinkLabel_LinkClicked);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.closeButton.Location = new System.Drawing.Point(452, 372);
			this.closeButton.Name = "closeButton";
			this.closeButton.TabIndex = 0;
			this.closeButton.Text = "&Close";
			// 
			// richTextBox
			// 
			this.richTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox.Location = new System.Drawing.Point(16, 8);
			this.richTextBox.Name = "richTextBox";
			this.richTextBox.ReadOnly = true;
			this.richTextBox.Size = new System.Drawing.Size(512, 136);
			this.richTextBox.TabIndex = 4;
			this.richTextBox.Text = "";
			// 
			// versionHeaderGroupBox
			// 
			this.versionHeaderGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left);
			this.versionHeaderGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																								this.assembliesListView});
			this.versionHeaderGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.versionHeaderGroupBox.Location = new System.Drawing.Point(8, 218);
			this.versionHeaderGroupBox.Name = "versionHeaderGroupBox";
			this.versionHeaderGroupBox.Padding = 0;
			this.versionHeaderGroupBox.Size = new System.Drawing.Size(520, 144);
			this.versionHeaderGroupBox.TabIndex = 1;
			this.versionHeaderGroupBox.TabStop = false;
			this.versionHeaderGroupBox.Text = "Version Information";
			// 
			// AboutForm
			// 
			this.AcceptButton = this.closeButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(538, 408);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.contactInfoHeaderGroupBox,
																		  this.richTextBox,
																		  this.versionHeaderGroupBox,
																		  this.closeButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = null;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutForm";
			this.ShowInTaskbar = false;
			this.Text = "About NDoc";
			this.contactInfoHeaderGroupBox.ResumeLayout(false);
			this.versionHeaderGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers
		private void mailLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			mailLinkLabel.Links[mailLinkLabel.Links.IndexOf(e.Link)].Visited = true;
			string url = e.Link.LinkData.ToString();
			Process.Start(url);
		}

		private void webLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			webLinkLabel.Links[webLinkLabel.Links.IndexOf(e.Link)].Visited = true;
			string url = e.Link.LinkData.ToString();
			Process.Start(url);
		}
		#endregion // Event Handlers
	}
}
