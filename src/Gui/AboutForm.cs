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
		private NDoc.Gui.HeaderGroupBox urlHeaderGroupBox;
		private System.Windows.Forms.LinkLabel mailLinkLabel;
		private System.Windows.Forms.LinkLabel webLinkLabel;
		private NDoc.Gui.HeaderGroupBox versionHeaderGroupBox;
		private System.Windows.Forms.ColumnHeader assemblyColumnHeader;
		private System.Windows.Forms.ListView assembliesListView;
		private System.Windows.Forms.ColumnHeader versionColumnHeader;
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
			mailLinkLabel.Links.Add(43, 18, "mailto:jc@manoli.net");

			// Set up web links
			webLinkLabel.Links.Add(12, 12, "http://sourceforge.net/projects/ndoc");
			webLinkLabel.Links.Add(26, 13, "http://sourceforge.net/tracker/?func=add&group_id=36057&atid=416078");
			webLinkLabel.Links.Add(41, 11, "http://sourceforge.net/mail/?group_id=36057");
			webLinkLabel.Links.Add(54, 10, "http://www.opensource.org/");

			try 
			{
				ArrayList ndocItems = new ArrayList();
				ListViewItem item;
				foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
				{
					item = new ListViewItem();
					item.Text = module.ModuleName;
					item.SubItems.Add(module.FileVersionInfo.FileVersion);
					assembliesListView.Items.Add(item);
					if (module.ModuleName.ToLower().StartsWith("ndoc"))
					{
						ndocItems.Add(item);
					}
				}

				assembliesListView.Sort();

				for (int i = ndocItems.Count; i > 0; i--)
				{
					ListViewItem ndocItem = (ListViewItem)ndocItems[i-1];
					assembliesListView.Items.Remove(ndocItem);
					assembliesListView.Items.Insert(0, ndocItem);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.ToString(), "NDoc", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
			this.closeButton = new System.Windows.Forms.Button();
			this.richTextBox = new System.Windows.Forms.RichTextBox();
			this.urlHeaderGroupBox = new NDoc.Gui.HeaderGroupBox();
			this.webLinkLabel = new System.Windows.Forms.LinkLabel();
			this.versionHeaderGroupBox = new NDoc.Gui.HeaderGroupBox();
			this.urlHeaderGroupBox.SuspendLayout();
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
			this.mailLinkLabel.Text = "Developers:  Jason Diamond, Kral Ferch and Jean-Claude Manoli";
			this.mailLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.mailLinkLabel_LinkClicked);
			// 
			// assembliesListView
			// 
			this.assembliesListView.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.assembliesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																								 this.assemblyColumnHeader,
																								 this.versionColumnHeader});
			this.assembliesListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.assembliesListView.Location = new System.Drawing.Point(8, 16);
			this.assembliesListView.Name = "assembliesListView";
			this.assembliesListView.Size = new System.Drawing.Size(504, 132);
			this.assembliesListView.TabIndex = 0;
			this.assembliesListView.View = System.Windows.Forms.View.Details;
			// 
			// assemblyColumnHeader
			// 
			this.assemblyColumnHeader.Text = "Assembly";
			this.assemblyColumnHeader.Width = 230;
			// 
			// versionColumnHeader
			// 
			this.versionColumnHeader.Text = "Version";
			this.versionColumnHeader.Width = 221;
			// 
			// closeButton
			// 
			this.closeButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.closeButton.Location = new System.Drawing.Point(448, 396);
			this.closeButton.Name = "closeButton";
			this.closeButton.TabIndex = 0;
			this.closeButton.Text = "&Close";
			// 
			// richTextBox
			// 
			this.richTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.richTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox.Location = new System.Drawing.Point(16, 8);
			this.richTextBox.Name = "richTextBox";
			this.richTextBox.ReadOnly = true;
			this.richTextBox.Size = new System.Drawing.Size(512, 136);
			this.richTextBox.TabIndex = 4;
			this.richTextBox.Text = "";
			// 
			// urlHeaderGroupBox
			// 
			this.urlHeaderGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.urlHeaderGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																							this.webLinkLabel,
																							this.mailLinkLabel});
			this.urlHeaderGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.urlHeaderGroupBox.Location = new System.Drawing.Point(8, 152);
			this.urlHeaderGroupBox.Name = "urlHeaderGroupBox";
			this.urlHeaderGroupBox.Padding = 0;
			this.urlHeaderGroupBox.Size = new System.Drawing.Size(520, 56);
			this.urlHeaderGroupBox.TabIndex = 5;
			this.urlHeaderGroupBox.TabStop = false;
			this.urlHeaderGroupBox.Text = "Contact Information:";
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
			// versionHeaderGroupBox
			// 
			this.versionHeaderGroupBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.versionHeaderGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																								this.assembliesListView});
			this.versionHeaderGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.versionHeaderGroupBox.Location = new System.Drawing.Point(8, 218);
			this.versionHeaderGroupBox.Name = "versionHeaderGroupBox";
			this.versionHeaderGroupBox.Padding = 0;
			this.versionHeaderGroupBox.Size = new System.Drawing.Size(520, 162);
			this.versionHeaderGroupBox.TabIndex = 1;
			this.versionHeaderGroupBox.TabStop = false;
			this.versionHeaderGroupBox.Text = "Version Information";
			// 
			// AboutForm
			// 
			this.AcceptButton = this.closeButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(542, 440);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.urlHeaderGroupBox,
																		  this.richTextBox,
																		  this.versionHeaderGroupBox,
																		  this.closeButton});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = null;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutForm";
			this.Text = "About NDoc";
			this.Load += new System.EventHandler(this.AboutForm_Load);
			this.urlHeaderGroupBox.ResumeLayout(false);
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

		private void AboutForm_Load(object sender, System.EventArgs e)
		{

		}
	}
}
