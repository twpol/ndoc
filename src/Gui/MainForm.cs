// MainForm.cs - main GUI interface to NDoc
// Copyright (C) 2001  Kral Ferch
//
// Modified by: Keith Hill on Sep 28, 2001.  
//   Tweaked the layout quite a bit. Uses new HeaderGroupBox from Matthew Adams 
//   from DOTNET list.  Added to menu, added a toolbar and status bar.  Changed 
//   the way docs are built on separate thread so that you can cancel from the 
//   toolbar and so that the updates use the statusbar to indicate progress.
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
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;

using NDoc.Core;
using NDoc.Documenter.Xml;
using VS = NDoc.VisualStudio;

namespace NDoc.Gui
{
	/// <summary>The main application form.</summary>
	/// <remarks>The main application form contains a listview that holds 
	/// assembly and /doc file pairs. You can add, edit, or delete a row 
	/// in the listview. You can document multiple assemblies at one time.
	/// <para>NDoc provides for dynamic recognition of available 
	/// documenters.  It locates any available assemblies that are capable 
	/// of creating documentation by searching the directory for any 
	/// assemblies that contain a class that derives from 
	/// <see cref="IDocumenter"/> which is defined in the NDoc.Core 
	/// namespace.</para>
	/// <para>Currently there are 3 documenters supplied with NDoc:
	/// <list type="bullet">
	/// <item><term>Msdn</term><description>Compiled HTML Help like the
	/// .NET Framework SDK.</description></item>
	/// <item><term>JavaDoc</term><description>JavaDoc-like html 
	/// documentation.</description></item>
	/// <item><term>Xml</term><description>An XML file containing the 
	/// full documentation.</description></item>
	/// </list>
	/// </para>
	/// <para>NDoc allows you to save documentation projects. NDoc project 
	/// files have the .ndoc extension.</para>
	/// <para>The bottom part of the main application form contains 
	/// a property grid.  You can edit the properties of the selected 
	/// documenter via this property grid.</para>
	/// </remarks>
	public class MainForm : System.Windows.Forms.Form
	{
		#region Fields
		#region Required Designer Fields
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.MenuItem menuFileCloseItem;
		private System.Windows.Forms.MenuItem menuFileNewItem;
		private System.Windows.Forms.ColumnHeader slashDocHeader;
		private System.Windows.Forms.ColumnHeader assemblyHeader;
		private System.Windows.Forms.MenuItem menuFileRecentProjectsItem;
		private System.Windows.Forms.MenuItem menuSpacerItem3;
		private System.Windows.Forms.MenuItem menuSpacerItem2;
		private System.Windows.Forms.MenuItem menuSpacerItem1;
		private System.Windows.Forms.MenuItem menuFileExitItem;
		private System.Windows.Forms.MenuItem menuFileSaveAsItem;
		private System.Windows.Forms.MenuItem menuFileOpenItem;
		private System.Windows.Forms.MenuItem menuFileSaveItem;
		private System.Windows.Forms.MenuItem menuFileItem;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.ToolBar toolBar;
		private System.Windows.Forms.ToolBarButton openToolBarButton;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ToolBarButton newToolBarButton;
		private System.Windows.Forms.ToolBarButton saveToolBarButton;
		private System.Windows.Forms.ToolBarButton separatorToolBarButton;
		private System.Windows.Forms.ToolBarButton buildToolBarButton;
		private System.Windows.Forms.ToolBarButton viewToolBarButton;
		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.StatusBarPanel statusBarTextPanel;
		private System.Windows.Forms.MenuItem menuDocItem;
		private System.Windows.Forms.MenuItem menuDocBuildItem;
		private System.Windows.Forms.MenuItem menuDocViewItem;
		private NDoc.Gui.HeaderGroupBox assembliesHeaderGroupBox;
		private System.Windows.Forms.ListView assembliesListView;
		private System.Windows.Forms.Button editButton;
		private System.Windows.Forms.Button namespaceSummariesButton;
		private System.Windows.Forms.Button deleteButton;
		private System.Windows.Forms.Button addButton;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.ToolBarButton cancelToolBarButton;
		private System.Windows.Forms.MenuItem menuAboutItem;
		private System.Windows.Forms.ComboBox comboBoxDocumenters;
		private System.Windows.Forms.PropertyGrid propertyGrid;
		private NDoc.Gui.HeaderGroupBox documenterHeaderGroupBox;
		private System.Windows.Forms.Label labelDocumenters;
		#endregion // Required Designer Fields

		private Project project;
		private string processDirectory;
		private string projectFilename;
		private string untitledProjectName = "(Untitled)";
		private int maxMRU = 5;
		private Thread buildThread;
		private System.Windows.Forms.ToolBarButton solutionToolBarButton;
		private System.Windows.Forms.MenuItem menuFileOpenSolution;
		private StringCollection recentProjectFilenames = new StringCollection();
		#endregion // Fields

		#region Constructors / Dispose
		/// <summary>Initializes the main application form, locates 
		/// available documenters, and sets up the menus.</summary>
		/// <remarks>NDoc project files have a .ndoc extension which 
		/// could be a registered file type in the system.  If a .ndoc 
		/// project file is double-clicked from explorer then the NDoc 
		/// application is called and passed the project file as a command line 
		/// argument.  This project filename will get passed into this 
		/// constructor.  If no project filename is passed in then the 
		/// constructor selects the most recently used project file (from 
		/// the MRU list that's stored in the NDoc configuration file) and 
		/// initializes the main application form using the information 
		/// in that project file.</remarks>
		/// <param name="startingProjectFilename">A project filename passed 
		/// in as an argument to the NDoc application.</param>
		public MainForm(string startingProjectFilename)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.SetStyle(ControlStyles.DoubleBuffer, true);

			Thread.CurrentThread.Name = "GUI";

			project = new Project();

			foreach (IDocumenter documenter in project.Documenters)
			{
				comboBoxDocumenters.Items.Add(documenter.Name);
			}

			ReadConfig();

			EnableAssemblyItems();
			MakeMRUMenu();

			processDirectory = Directory.GetCurrentDirectory();

			if (startingProjectFilename != null || recentProjectFilenames.Count > 0)
			{
				// If a project document wasn't passed in on the command line
				// then load up the most recently used project file.
				if (startingProjectFilename == null)
				{
					startingProjectFilename = recentProjectFilenames[0];
				}

				if (File.Exists(startingProjectFilename))
				{
					try
					{
						FileOpen(startingProjectFilename);
					}
					catch(Exception)
					{
						MessageBox.Show("Error loading the NDoc project file '" + startingProjectFilename + "'.", "Error loading NDoc project file");
						Clear();
					}
				}
				else
				{
					MessageBox.Show("The NDoc project file '" + startingProjectFilename + "' doesn't exist.", "Error loading NDoc project file");
					Clear();
				}
			}
			else
			{
				projectFilename = untitledProjectName;
				EnableMenuItems(false);
			}

			menuFileCloseItem.Visible = false;

			SetWindowTitle();
		}

		/// <summary>Calls <see cref="WriteConfig"/> to write out the config 
		/// file and calls Dispose() on base and components.</summary>
		public override void Dispose()
		{
			WriteConfig();
			base.Dispose();
		}
		#endregion // Constructors / Dispose

		#region InitializeComponent
		/// <summary>
		///    Required method for Designer support - do not modify
		///    the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.menuFileExitItem = new System.Windows.Forms.MenuItem();
			this.newToolBarButton = new System.Windows.Forms.ToolBarButton();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.menuSpacerItem3 = new System.Windows.Forms.MenuItem();
			this.menuFileSaveItem = new System.Windows.Forms.MenuItem();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuFileItem = new System.Windows.Forms.MenuItem();
			this.menuFileNewItem = new System.Windows.Forms.MenuItem();
			this.menuFileOpenSolution = new System.Windows.Forms.MenuItem();
			this.menuFileOpenItem = new System.Windows.Forms.MenuItem();
			this.menuFileCloseItem = new System.Windows.Forms.MenuItem();
			this.menuSpacerItem1 = new System.Windows.Forms.MenuItem();
			this.menuFileSaveAsItem = new System.Windows.Forms.MenuItem();
			this.menuFileRecentProjectsItem = new System.Windows.Forms.MenuItem();
			this.menuSpacerItem2 = new System.Windows.Forms.MenuItem();
			this.menuDocItem = new System.Windows.Forms.MenuItem();
			this.menuDocBuildItem = new System.Windows.Forms.MenuItem();
			this.menuDocViewItem = new System.Windows.Forms.MenuItem();
			this.menuAboutItem = new System.Windows.Forms.MenuItem();
			this.comboBoxDocumenters = new System.Windows.Forms.ComboBox();
			this.addButton = new System.Windows.Forms.Button();
			this.slashDocHeader = new System.Windows.Forms.ColumnHeader();
			this.cancelToolBarButton = new System.Windows.Forms.ToolBarButton();
			this.documenterHeaderGroupBox = new NDoc.Gui.HeaderGroupBox();
			this.labelDocumenters = new System.Windows.Forms.Label();
			this.propertyGrid = new System.Windows.Forms.PropertyGrid();
			this.viewToolBarButton = new System.Windows.Forms.ToolBarButton();
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.statusBarTextPanel = new System.Windows.Forms.StatusBarPanel();
			this.assemblyHeader = new System.Windows.Forms.ColumnHeader();
			this.openToolBarButton = new System.Windows.Forms.ToolBarButton();
			this.separatorToolBarButton = new System.Windows.Forms.ToolBarButton();
			this.solutionToolBarButton = new System.Windows.Forms.ToolBarButton();
			this.saveToolBarButton = new System.Windows.Forms.ToolBarButton();
			this.editButton = new System.Windows.Forms.Button();
			this.namespaceSummariesButton = new System.Windows.Forms.Button();
			this.assembliesHeaderGroupBox = new NDoc.Gui.HeaderGroupBox();
			this.assembliesListView = new System.Windows.Forms.ListView();
			this.deleteButton = new System.Windows.Forms.Button();
			this.toolBar = new System.Windows.Forms.ToolBar();
			this.buildToolBarButton = new System.Windows.Forms.ToolBarButton();
			this.documenterHeaderGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.statusBarTextPanel)).BeginInit();
			this.assembliesHeaderGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// progressBar
			// 
			this.progressBar.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.progressBar.Location = new System.Drawing.Point(334, 592);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(144, 15);
			this.progressBar.TabIndex = 24;
			this.progressBar.Visible = false;
			// 
			// menuFileExitItem
			// 
			this.menuFileExitItem.Index = 10;
			this.menuFileExitItem.Text = "&Exit";
			this.menuFileExitItem.Click += new System.EventHandler(this.menuFileExitItem_Click);
			// 
			// newToolBarButton
			// 
			this.newToolBarButton.ImageIndex = 0;
			this.newToolBarButton.ToolTipText = "New";
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// menuSpacerItem3
			// 
			this.menuSpacerItem3.Index = 8;
			this.menuSpacerItem3.Text = "-";
			// 
			// menuFileSaveItem
			// 
			this.menuFileSaveItem.Index = 5;
			this.menuFileSaveItem.Text = "&Save";
			this.menuFileSaveItem.Click += new System.EventHandler(this.menuFileSaveItem_Click);
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuFileItem,
																					  this.menuDocItem,
																					  this.menuAboutItem});
			// 
			// menuFileItem
			// 
			this.menuFileItem.Index = 0;
			this.menuFileItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuFileNewItem,
																						 this.menuFileOpenSolution,
																						 this.menuFileOpenItem,
																						 this.menuFileCloseItem,
																						 this.menuSpacerItem1,
																						 this.menuFileSaveItem,
																						 this.menuFileSaveAsItem,
																						 this.menuSpacerItem2,
																						 this.menuFileRecentProjectsItem,
																						 this.menuSpacerItem3,
																						 this.menuFileExitItem});
			this.menuFileItem.Text = "&Project";
			// 
			// menuFileNewItem
			// 
			this.menuFileNewItem.Index = 0;
			this.menuFileNewItem.Text = "&New";
			this.menuFileNewItem.Click += new System.EventHandler(this.menuFileNewItem_Click);
			// 
			// menuFileOpenSolution
			// 
			this.menuFileOpenSolution.Index = 1;
			this.menuFileOpenSolution.Text = "New from &Visual Studio Solution...";
			this.menuFileOpenSolution.Click += new System.EventHandler(this.menuFileOpenSolution_Click);
			// 
			// menuFileOpenItem
			// 
			this.menuFileOpenItem.Index = 2;
			this.menuFileOpenItem.Text = "&Open...";
			this.menuFileOpenItem.Click += new System.EventHandler(this.menuFileOpenItem_Click);
			// 
			// menuFileCloseItem
			// 
			this.menuFileCloseItem.Index = 3;
			this.menuFileCloseItem.Text = "&Close";
			this.menuFileCloseItem.Click += new System.EventHandler(this.menuFileCloseItem_Click);
			// 
			// menuSpacerItem1
			// 
			this.menuSpacerItem1.Index = 4;
			this.menuSpacerItem1.Text = "-";
			// 
			// menuFileSaveAsItem
			// 
			this.menuFileSaveAsItem.Index = 6;
			this.menuFileSaveAsItem.Text = "Save &As...";
			this.menuFileSaveAsItem.Click += new System.EventHandler(this.menuFileSaveAsItem_Click);
			// 
			// menuFileRecentProjectsItem
			// 
			this.menuFileRecentProjectsItem.Index = 8;
			this.menuFileRecentProjectsItem.Text = "&Recent Projects";
			// 
			// menuSpacerItem2
			// 
			this.menuSpacerItem2.Index = 7;
			this.menuSpacerItem2.Text = "-";
			// 
			// menuSpacerItem3
			// 
			this.menuSpacerItem3.Index = 9;
			this.menuSpacerItem3.Text = "-";
			// 
			// menuDocItem
			// 
			this.menuDocItem.Index = 1;
			this.menuDocItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						this.menuDocBuildItem,
																						this.menuDocViewItem});
			this.menuDocItem.Text = "&Documentation";
			// 
			// menuDocBuildItem
			// 
			this.menuDocBuildItem.Index = 0;
			this.menuDocBuildItem.Text = "&Build";
			this.menuDocBuildItem.Click += new System.EventHandler(this.menuDocBuildItem_Click);
			// 
			// menuDocViewItem
			// 
			this.menuDocViewItem.Index = 1;
			this.menuDocViewItem.Text = "&View";
			this.menuDocViewItem.Click += new System.EventHandler(this.menuDocViewItem_Click);
			// 
			// menuAboutItem
			// 
			this.menuAboutItem.Index = 2;
			this.menuAboutItem.Text = "&About";
			this.menuAboutItem.Click += new System.EventHandler(this.menuAboutItem_Click);
			// 
			// comboBoxDocumenters
			// 
			this.comboBoxDocumenters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxDocumenters.DropDownWidth = 160;
			this.comboBoxDocumenters.Location = new System.Drawing.Point(128, 24);
			this.comboBoxDocumenters.Name = "comboBoxDocumenters";
			this.comboBoxDocumenters.Size = new System.Drawing.Size(160, 21);
			this.comboBoxDocumenters.TabIndex = 9;
			this.comboBoxDocumenters.SelectedIndexChanged += new System.EventHandler(this.comboBoxDocumenters_SelectedIndexChanged);
			// 
			// addButton
			// 
			this.addButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.addButton.Location = new System.Drawing.Point(397, 24);
			this.addButton.Name = "addButton";
			this.addButton.TabIndex = 14;
			this.addButton.Text = "Add";
			this.addButton.Click += new System.EventHandler(this.addButton_Click);
			// 
			// slashDocHeader
			// 
			this.slashDocHeader.Text = "/doc Filename";
			this.slashDocHeader.Width = 200;
			// 
			// cancelToolBarButton
			// 
			this.cancelToolBarButton.Enabled = false;
			this.cancelToolBarButton.ImageIndex = 4;
			this.cancelToolBarButton.ToolTipText = "Cancel";
			// 
			// documenterHeaderGroupBox
			// 
			this.documenterHeaderGroupBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.documenterHeaderGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.documenterHeaderGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																								   this.labelDocumenters,
																								   this.comboBoxDocumenters,
																								   this.propertyGrid});
			this.documenterHeaderGroupBox.Location = new System.Drawing.Point(8, 192);
			this.documenterHeaderGroupBox.Name = "documenterHeaderGroupBox";
			this.documenterHeaderGroupBox.Padding = 0;
			this.documenterHeaderGroupBox.Size = new System.Drawing.Size(480, 396);
			this.documenterHeaderGroupBox.TabIndex = 23;
			this.documenterHeaderGroupBox.TabStop = false;
			this.documenterHeaderGroupBox.Text = "Select and Configure Documenter";
			// 
			// labelDocumenters
			// 
			this.labelDocumenters.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelDocumenters.Location = new System.Drawing.Point(16, 26);
			this.labelDocumenters.Name = "labelDocumenters";
			this.labelDocumenters.Size = new System.Drawing.Size(112, 21);
			this.labelDocumenters.TabIndex = 10;
			this.labelDocumenters.Text = "Documentation Type:";
			this.labelDocumenters.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// propertyGrid
			// 
			this.propertyGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.propertyGrid.CommandsVisibleIfAvailable = true;
			this.propertyGrid.LargeButtons = false;
			this.propertyGrid.LineColor = System.Drawing.SystemColors.ScrollBar;
			this.propertyGrid.Location = new System.Drawing.Point(16, 56);
			this.propertyGrid.Name = "propertyGrid";
			this.propertyGrid.Size = new System.Drawing.Size(456, 332);
			this.propertyGrid.TabIndex = 0;
			this.propertyGrid.Text = "PropertyGrid";
			this.propertyGrid.ViewBackColor = System.Drawing.SystemColors.Window;
			this.propertyGrid.ViewForeColor = System.Drawing.SystemColors.WindowText;
			// 
			// viewToolBarButton
			// 
			this.viewToolBarButton.ImageIndex = 5;
			this.viewToolBarButton.ToolTipText = "View Documentation";
			// 
			// statusBar
			// 
			this.statusBar.Location = new System.Drawing.Point(0, 589);
			this.statusBar.Name = "statusBar";
			this.statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																						 this.statusBarTextPanel});
			this.statusBar.ShowPanels = true;
			this.statusBar.Size = new System.Drawing.Size(496, 20);
			this.statusBar.TabIndex = 21;
			// 
			// statusBarTextPanel
			// 
			this.statusBarTextPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.statusBarTextPanel.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
			this.statusBarTextPanel.Text = "Ready";
			this.statusBarTextPanel.Width = 480;
			// 
			// assemblyHeader
			// 
			this.assemblyHeader.Text = "Assembly Filename";
			this.assemblyHeader.Width = 200;
			// 
			// openToolBarButton
			// 
			this.openToolBarButton.ImageIndex = 1;
			this.openToolBarButton.ToolTipText = "Open ";
			// 
			// separatorToolBarButton
			// 
			this.separatorToolBarButton.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// solutionToolBarButton
			// 
			this.solutionToolBarButton.ImageIndex = 6;
			this.solutionToolBarButton.ToolTipText = "New from Visual Studio Solution";
			// 
			// saveToolBarButton
			// 
			this.saveToolBarButton.ImageIndex = 2;
			this.saveToolBarButton.ToolTipText = "Save";
			// 
			// editButton
			// 
			this.editButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.editButton.Enabled = false;
			this.editButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.editButton.Location = new System.Drawing.Point(397, 56);
			this.editButton.Name = "editButton";
			this.editButton.TabIndex = 15;
			this.editButton.Text = "Edit";
			this.editButton.Click += new System.EventHandler(this.editButton_Click);
			// 
			// namespaceSummariesButton
			// 
			this.namespaceSummariesButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.namespaceSummariesButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.namespaceSummariesButton.Location = new System.Drawing.Point(320, 120);
			this.namespaceSummariesButton.Name = "namespaceSummariesButton";
			this.namespaceSummariesButton.Size = new System.Drawing.Size(152, 23);
			this.namespaceSummariesButton.TabIndex = 17;
			this.namespaceSummariesButton.Text = "Edit Namespace Summary...";
			this.namespaceSummariesButton.Click += new System.EventHandler(this.namespaceSummariesButton_Click);
			// 
			// assembliesHeaderGroupBox
			// 
			this.assembliesHeaderGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.assembliesHeaderGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.assembliesHeaderGroupBox.Controls.AddRange(new System.Windows.Forms.Control[] {
																								   this.assembliesListView,
																								   this.editButton,
																								   this.namespaceSummariesButton,
																								   this.deleteButton,
																								   this.addButton});
			this.assembliesHeaderGroupBox.Location = new System.Drawing.Point(8, 32);
			this.assembliesHeaderGroupBox.Name = "assembliesHeaderGroupBox";
			this.assembliesHeaderGroupBox.Padding = 0;
			this.assembliesHeaderGroupBox.Size = new System.Drawing.Size(480, 152);
			this.assembliesHeaderGroupBox.TabIndex = 22;
			this.assembliesHeaderGroupBox.TabStop = false;
			this.assembliesHeaderGroupBox.Text = "Select Assemblies to Document";
			// 
			// assembliesListView
			// 
			this.assembliesListView.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.assembliesListView.ForeColor = System.Drawing.SystemColors.WindowText;
			this.assembliesListView.Location = new System.Drawing.Point(16, 24);
			this.assembliesListView.Name = "assembliesListView";
			this.assembliesListView.Size = new System.Drawing.Size(296, 120);
			this.assembliesListView.TabIndex = 13;
			this.assembliesListView.View = System.Windows.Forms.View.List;
			this.assembliesListView.SelectedIndexChanged += new System.EventHandler(this.assembliesListView_SelectedIndexChanged);
			// 
			// deleteButton
			// 
			this.deleteButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.deleteButton.Enabled = false;
			this.deleteButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.deleteButton.Location = new System.Drawing.Point(397, 88);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.TabIndex = 16;
			this.deleteButton.Text = "Remove";
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// toolBar
			// 
			this.toolBar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.toolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																					   this.newToolBarButton,
																					   this.solutionToolBarButton,
																					   this.openToolBarButton,
																					   this.saveToolBarButton,
																					   this.separatorToolBarButton,
																					   this.buildToolBarButton,
																					   this.cancelToolBarButton,
																					   this.viewToolBarButton});
			this.toolBar.DropDownArrows = true;
			this.toolBar.ImageList = this.imageList1;
			this.toolBar.Name = "toolBar";
			this.toolBar.ShowToolTips = true;
			this.toolBar.Size = new System.Drawing.Size(496, 25);
			this.toolBar.TabIndex = 20;
			this.toolBar.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
			this.toolBar.Wrappable = false;
			this.toolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBarButton_Click);
			// 
			// buildToolBarButton
			// 
			this.buildToolBarButton.ImageIndex = 3;
			this.buildToolBarButton.ToolTipText = "Build Documentation";
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(496, 609);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.progressBar,
																		  this.assembliesHeaderGroupBox,
																		  this.statusBar,
																		  this.toolBar,
																		  this.documenterHeaderGroupBox});
			this.Menu = this.mainMenu1;
			this.MinimumSize = new System.Drawing.Size(504, 460);
			this.Name = "MainForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "NDoc";
			this.documenterHeaderGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.statusBarTextPanel)).EndInit();
			this.assembliesHeaderGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion // InitializeComponent

		#region Main
		/// <summary>The main entry point for the application.</summary>
		[STAThread]
		public static void Main(string[] args)
		{
			string projectFilename = (args.Length == 1) ? args[0] : null;

			Application.Run(new MainForm(projectFilename));
		}
		#endregion // Main

		#region Methods
		private void SetWindowTitle()
		{
			string projectName;

			if (projectFilename == untitledProjectName)
			{
				projectName = projectFilename;
			}
			else
			{
				projectName = Path.GetFileName(projectFilename);
				projectName = projectName.Substring(0, projectName.LastIndexOf('.'));
			}

			this.Text = "NDoc - " + projectName;
		}

		/// <summary>
		/// Enables/disables the Save and SaveAs menu items.
		/// </summary>
		/// <param name="bEnable"><b>true</b> for enabling the menu items, <b>false</b> for disabling.</param>
		private void EnableMenuItems(bool bEnable)
		{
			if (!(bEnable == true && projectFilename == untitledProjectName))
			{
				menuFileSaveItem.Enabled = bEnable;
			}

			menuFileSaveAsItem.Enabled = bEnable;
		}

		/// <summary>
		/// Enable/disable the buttons in the GUI based on whether there any assemblies to document.
		/// </summary>
		private void EnableAssemblyItems()
		{
			bool  bEnable = (assembliesListView.Items.Count > 0) ? true : false;

			menuDocBuildItem.Enabled = bEnable;
			menuDocViewItem.Enabled = bEnable;
			buildToolBarButton.Enabled = bEnable;
			viewToolBarButton.Enabled = bEnable;
			namespaceSummariesButton.Enabled = bEnable;
		}

		/// <summary>
		/// Clears and recreates the most recently used files (MRU) menu.
		/// </summary>
		private void MakeMRUMenu()
		{
			if (recentProjectFilenames.Count > 0)
			{
				int   count = 1;

				menuFileRecentProjectsItem.MenuItems.Clear();
				menuFileRecentProjectsItem.Enabled = true;

				foreach (string project in recentProjectFilenames)
				{
					MenuItem  menuItem = new MenuItem ();

					menuItem.Text = "&" + count.ToString() + " " + project;
					menuItem.Click += new System.EventHandler (this.menuMRUItem_Click);
					menuFileRecentProjectsItem.MenuItems.Add(menuItem);

					count++;

					if (count > maxMRU)
					{
						break;
					}
				}
			}
			else
			{
				menuFileRecentProjectsItem.Enabled = false;
			}
		}

		/// <summary>
		/// Updates the MRU menu to reflect the project that was just opened.
		/// </summary>
		private void UpdateMRUList()
		{
			try
			{
				recentProjectFilenames.Remove(projectFilename);
			}
			catch(Exception)
			{
				// Remove throws an exception if the item isn't in the list.
				// But that's ok for us so do nothing.
			}

			recentProjectFilenames.Insert(0, projectFilename);
			MakeMRUMenu();
			EnableAssemblyItems();
		}

		/// <summary>
		/// Get the application data directory.
		/// </summary>
		/// <returns>
		/// The application data folder appended with a NDoc folder.
		/// </returns>
		private string GetApplicationDataDirectory()
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\NDoc\";
		}

		/// <summary>Reads in the NDoc configuration file from the 
		/// application directory.</summary>
		/// <remarks>The config file stores the most recently used (MRU) 
		/// list of project files.  It also stores which documenter was 
		/// being used last.</remarks>
		private void ReadConfig()
		{
			string directory = GetApplicationDataDirectory();

			string guiConfigFilename = directory + "NDoc.Gui.xml";

			string documenterName = "MSDN";

			if (File.Exists(guiConfigFilename))
			{
				XmlTextReader reader = null;

				try
				{
					StreamReader streamReader = new StreamReader(File.OpenRead(guiConfigFilename));

					reader = new XmlTextReader(streamReader);
					reader.MoveToContent();
					reader.ReadStartElement("ndoc.gui");

					while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
					{
						if (reader.NodeType == XmlNodeType.Element)
						{
							switch (reader.Name)
							{
								case "project":
									recentProjectFilenames.Add(reader.ReadString());
									break;
								case "documenter":
									documenterName = reader.ReadString();
									break;
								case "window":
									reader.MoveToNextAttribute();
									this.Left = int.Parse(reader.Value);
									reader.MoveToNextAttribute();
									this.Top = int.Parse(reader.Value);
									reader.MoveToNextAttribute();
									this.Width = int.Parse(reader.Value);
									reader.MoveToNextAttribute();
									//HACK: subtract 20 to last height to keep it constant 
									this.Height = int.Parse(reader.Value) - 20;
									break;
							}
						}
					}
				}
				finally
				{
					if (reader != null)
					{
						reader.Close();
					}
				}
			}

			int index = 0;

			foreach (IDocumenter documenter in project.Documenters)
			{
				if (documenter.Name == documenterName)
				{
					comboBoxDocumenters.SelectedIndex = index;
					break;
				}

				++index;
			}
		}

		/// <summary>Writes out the NDoc configuration file to the 
		/// application directory.</summary>
		/// <remarks>The config file stores the most recently used (MRU) 
		/// list of project files.  It also stores which documenter was 
		/// being used last.</remarks>
		private void WriteConfig()
		{
			string directory = GetApplicationDataDirectory();

			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			// Trim our MRU list down to max amount before writing the config.
			while (recentProjectFilenames.Count > maxMRU)
			{
				recentProjectFilenames.RemoveAt(maxMRU);
			}

			// Write our NDoc config file.
			string guiConfigFilename = directory + "NDoc.Gui.xml";
			StreamWriter streamWriter = new StreamWriter(guiConfigFilename);

			XmlTextWriter writer = new XmlTextWriter(streamWriter);
			writer.Formatting = Formatting.Indented;
			writer.Indentation = 2;

			writer.WriteStartElement("ndoc.gui");
			WriteRecentProjects(writer);
			writer.WriteElementString("documenter", ((IDocumenter)project.Documenters[comboBoxDocumenters.SelectedIndex]).Name);
			writer.WriteStartElement("window");
			writer.WriteAttributeString("", "left", "", this.Location.X.ToString());
			writer.WriteAttributeString("", "top", "", this.Location.Y.ToString());
			writer.WriteAttributeString("", "width", "", this.Width.ToString());
			writer.WriteAttributeString("", "height", "", this.Height.ToString());
			writer.WriteEndElement();
			writer.WriteEndElement();

			writer.Close();
		}

		private void WriteRecentProjects(XmlWriter writer)
		{
			foreach (string project in recentProjectFilenames)
			{
				writer.WriteElementString("project", project);
			}
		}


		private void FileOpen(string fileName)
		{
			bool  bFailed = true;

			try
			{
				string directoryName = Path.GetDirectoryName(fileName);
				Directory.SetCurrentDirectory(directoryName);

				project.Read(fileName);

				projectFilename = fileName;
				SetWindowTitle();

				RefreshPropertyGrid();

				// Update the ListView
				assembliesListView.Items.Clear();
				foreach (AssemblySlashDoc assemblySlashDoc2 in project.AssemblySlashDocs)
				{
					AddRowToListView(assemblySlashDoc2);
				}

				UpdateMRUList();

				EnableMenuItems(true);

				bFailed = false;
			}
			catch (DocumenterException docEx)
			{
				MessageBox.Show(
					docEx.Message,
					"Unable to read in project file",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				MessageBox.Show(
					"An error occured while trying to read in project file '" + fileName + "'.",
					"NDoc Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}

			if (bFailed)
			{
				recentProjectFilenames.Remove(fileName);
				MakeMRUMenu();
				Clear();
			}
		}

		private void FileSave(string fileName)
		{
			project.Write(fileName);
		}

		private void AddRowToListView(AssemblySlashDoc assemblySlashDoc)
		{
			ListViewItem  listItem;
			string[]  subItems = new string[1];

			subItems[0] = Path.GetFileName(assemblySlashDoc.SlashDocFilename);
			listItem = new ListViewItem(
				Path.GetFileName(assemblySlashDoc.AssemblyFilename));
			assembliesListView.Items.Add(listItem);
		}

		private void Clear()
		{
			projectFilename = untitledProjectName;
			project.Clear();

			RefreshPropertyGrid();

			assembliesListView.Items.Clear();

			EnableAssemblyItems();

			projectFilename = untitledProjectName;
			EnableMenuItems(false);
			SetWindowTitle();
		}

		private void RefreshPropertyGrid()
		{
			// Can't figure out how to get the propertyGrid to update
			// to the newly updated documenter except for the hack
			// below.  Tried Refresh(), Reset(), Invalidate()/Update().
			int currentIndex = comboBoxDocumenters.SelectedIndex;
			if (currentIndex != 0)
			{
				comboBoxDocumenters.SelectedIndex = 0;
			}
			else
			{
				comboBoxDocumenters.SelectedIndex = 1;
			}
			comboBoxDocumenters.SelectedIndex = currentIndex;
		}
		#endregion // Methods

		#region Event Handlers
		/// <summary>
		/// Resets NDoc to an empty project by calling <see cref="Clear"/>.
		/// </summary>
		/// <param name="sender">The File->New menu item (not used).</param>
		/// <param name="e">Event arguments (not used).</param>
		/// <seealso cref="Clear"/>
		protected void menuFileNewItem_Click (object sender, System.EventArgs e)
		{
			Clear();
		}

		private void menuFileOpenSolution_Click (object sender, System.EventArgs e)
		{
			OpenFileDialog openFileDlg = new OpenFileDialog();

			//openFileDlg.InitialDirectory = processDirectory;
			openFileDlg.Filter = "Visual Studio Solution files (*.sln)|*.sln|All files (*.*)|*.*" ;
			openFileDlg.RestoreDirectory = true ;

			if(openFileDlg.ShowDialog() == DialogResult.OK)
			{
				VS.Solution sol = new VS.Solution(openFileDlg.FileName);

				try
				{
					this.Cursor = Cursors.WaitCursor;

					SolutionForm sf = new SolutionForm();
					sf.Text = "Solution " + sol.Name;

					sf.ConfigList.Items.Clear();
					foreach (string configkey in sol.GetConfigurations())
					{
						sf.ConfigList.Items.Add(configkey);
					}

					sf.ShowDialog(this);
					if (sf.ConfigList.SelectedIndex < 0)
						return;

					string solconfig = (string)sf.ConfigList.SelectedItem;

					//clear current ndoc project settings
					Clear();

					foreach (VS.Project p in sol.GetProjects())
					{
						string projid = p.ID.ToString();
						string projconfig = sol.GetProjectConfigName(solconfig, projid);

						if (projconfig == null)
							continue;

						string apath = p.GetRelativeOutputPathForConfiguration(projconfig);
						string xpath = p.GetRelativePathToDocumentationFile(projconfig);
						string spath = sol.Directory;

						if ((apath == null) || (xpath == null))
						{
							Debug.WriteLine("! " + apath);
							continue;
						}

						AssemblySlashDoc asd = new AssemblySlashDoc(
							Path.Combine(spath, apath), 
							Path.Combine(spath, xpath));
						project.AssemblySlashDocs.Add(asd);
						AddRowToListView(asd);
					}

					EnableMenuItems(true);
					EnableAssemblyItems();
		
					projectFilename =  Path.Combine(
						sol.Directory, 
						sol.Name + ".ndoc");
					//FileSave(projectFilename);
				}
				finally
				{
					this.Cursor = Cursors.Arrow;
				}
			}
		}

		private void menuFileOpenItem_Click (object sender, System.EventArgs e)
		{
			OpenFileDialog openFileDlg = new OpenFileDialog();
			//TODO: set the initial directory to the last place where we opened/saved a project
			//openFileDlg.InitialDirectory = processDirectory;
			openFileDlg.Filter = "Project files (*.ndoc)|*.ndoc|All files (*.*)|*.*" ;

			if(openFileDlg.ShowDialog() == DialogResult.OK)
			{
				FileOpen(openFileDlg.FileName);
			}
		}

		private void menuFileSaveItem_Click (object sender, System.EventArgs e)
		{
			FileSave(projectFilename);
		}

		private void menuFileSaveAsItem_Click (object sender, System.EventArgs e)
		{
			SaveFileDialog saveFileDlg = new SaveFileDialog();

			if (projectFilename == untitledProjectName)
			{
				//TODO: set the initial directory to the last place used to save a project
				//saveFileDlg.InitialDirectory = processDirectory;
				saveFileDlg.FileName = @".\Untitled.ndoc";
			}
			else
			{
				saveFileDlg.InitialDirectory = Path.GetDirectoryName(projectFilename);
				saveFileDlg.FileName = Path.GetFileName(projectFilename);
			}

			saveFileDlg.Filter = "Project files (*.ndoc)|*.ndoc|All files (*.*)|*.*" ;
			saveFileDlg.RestoreDirectory = true ;

			if(saveFileDlg.ShowDialog() == DialogResult.OK)
			{
				FileSave(saveFileDlg.FileName);

				projectFilename = saveFileDlg.FileName;
				SetWindowTitle();
				UpdateMRUList();
				EnableMenuItems(true);
			}
		}

		/// <summary>
		/// Not implemented yet.
		/// </summary>
		/// <param name="sender">The menu item.</param>
		/// <param name="e">The event arguments.</param>
		protected void menuFileCloseItem_Click (object sender, System.EventArgs e)
		{
		}

		/// <summary>
		/// Opens the project file of the selected MRU menu item.
		/// </summary>
		/// <remarks>
		/// If the project file exists, it opens it.  Otherwise that project
		/// file is removed from the MRU menu.
		/// </remarks>
		/// <param name="sender">The selected menu item.</param>
		/// <param name="e">Event arguments (not used).</param>
		protected void menuMRUItem_Click (object sender, System.EventArgs e)
		{
			string    fileName = ((MenuItem)sender).Text.Substring(3);

			if (File.Exists(fileName))
			{
				FileOpen(fileName);
			}
			else
			{
				try
				{
					MessageBox.Show("NDoc project file doesn't exist.", "Unable to open project file");
					recentProjectFilenames.Remove(fileName);
					MakeMRUMenu();
				}
				catch(Exception)
				{
					// Remove throws an exception if the item isn't in the list.
					// But that's ok for us so do nothing.
				}
			}
		}

		private void menuFileExitItem_Click (object sender, System.EventArgs e)
		{
			Close();
		}

		private void menuDocBuildItem_Click(object sender, System.EventArgs e)
		{
			IDocumenter documenter = 
				(IDocumenter)project.Documenters[comboBoxDocumenters.SelectedIndex];

			documenter.DocBuildingProgress += new DocBuildingEventHandler(OnProgressUpdate);
			documenter.DocBuildingStep += new DocBuildingEventHandler(OnStepUpdate);

			BuildWorker buildWorker = new BuildWorker(documenter, project);
			buildThread = new Thread(new ThreadStart(buildWorker.ThreadProc));
			buildThread.Name = "Build";
			buildThread.IsBackground = true;
			buildThread.Priority = ThreadPriority.BelowNormal;

			ConfigureUIForBuild(true);

			try
			{
				this.Cursor = Cursors.AppStarting;

				UpdateProgress("Building documentation...", 0);

				buildThread.Start();

				// Wait for thread to start
				while (!buildThread.IsAlive);

				// Now wait for thread to complete
				while (!buildWorker.IsComplete && buildThread.IsAlive)
				{
					// Keep GUI responsive
					Application.DoEvents();

					// Don't chew up all CPU cycles
					Thread.Sleep(100);
				}

				// Wait a little for the thread to die
				buildThread.Join(2000);

				this.Cursor = Cursors.Default;
			}
			finally
			{
				ConfigureUIForBuild(false);
				statusBarTextPanel.Text = "Ready";
			}

			// If no exception occurred during the build, then blow outta here
			Exception ex = buildWorker.Exception;
			if (ex == null) 
			{
				return;
			}

			//check if thread has been aborted
			Exception iex = ex;
			do
			{
				if (iex is ThreadAbortException)
				{
					return;
				}
				iex = iex.InnerException;
			} while (iex != null);

			// Process exception
			string msg = String.Format("An error occured while trying to build the " +
  			 	                       "documentation.\n\n{0}", ex.ToString());
			if (ex is DocumenterException)
			{
				MessageBox.Show(this, msg, "NDoc Documenter Error",
						        MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else 
			{
				MessageBox.Show(this, msg, "NDoc Error", MessageBoxButtons.OK,
					            MessageBoxIcon.Error);
			}
		}

		private void CancelBuild()
		{
			statusBarTextPanel.Text = "Cancelling build ...";
			buildThread.Abort();
		}

		private void ConfigureUIForBuild(bool starting)
		{
			foreach (ToolBarButton button in toolBar.Buttons)
			{
				if (button == cancelToolBarButton)
				{
					button.Enabled = starting;
				}
				else
				{
					button.Enabled = !starting;
				}
			}

			foreach (MenuItem menuItem in mainMenu1.MenuItems)
			{
				menuItem.Enabled = !starting;
			}

			progressBar.Visible = starting;
		}

		private void OnProgressUpdate(object sender, ProgressArgs e)
		{
			// This gets called from another thread so we must thread
			// marhal back to the GUI thread.
			//string text = e.Status;
			//int percent = e.Progress;
			//object[] args = new Object[] { null, percent };
			//Delegate d = new UpdateProgressDelegate(UpdateProgress);
			//this.Invoke(d, args);
		}

		private void OnStepUpdate(object sender, ProgressArgs e)
		{
			// This gets called from another thread so we must thread
			// marhal back to the GUI thread.
			string text = e.Status;
			int percent = e.Progress;
			object[] args = new Object[] { text, percent };
			Delegate d = new UpdateProgressDelegate(UpdateProgress);
			this.Invoke(d, args);
		}

		private delegate void UpdateProgressDelegate(string text, int percent);
		private void UpdateProgress(string text, int percent)
		{
			if (text != null)
			{
				statusBarTextPanel.Text = text;
			}
			percent = Math.Max(percent, 0);
			percent = Math.Min(percent, 100);
			progressBar.Value = percent;
			statusBar.Update();
		}

		private void menuDocViewItem_Click(object sender, System.EventArgs e)
		{
			try
			{
				((IDocumenter)project.Documenters[comboBoxDocumenters.SelectedIndex]).View();
			}
			catch (DocumenterException ex)
			{
				string msg = String.Format("An error occured while trying " +
					"to view the documentation.\n\n{0}", ex.ToString());

				MessageBox.Show(this, msg, "NDoc Documenter Errorw",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				string msg = String.Format("An error occured while trying " +
					"to view the documentation.\n\n{0}", ex.ToString());

				MessageBox.Show(this, msg, "NDoc Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void menuAboutItem_Click(object sender, System.EventArgs e)
		{
			AboutForm aboutForm = new AboutForm();
			aboutForm.StartPosition = FormStartPosition.CenterParent;
			aboutForm.ShowDialog(this);
		}

		private void toolBarButton_Click(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			if (e.Button == cancelToolBarButton)
			{
				CancelBuild();
			}
			else if (e.Button == newToolBarButton)
			{
				menuFileNewItem_Click(sender, EventArgs.Empty);
			}
			else if (e.Button == solutionToolBarButton)
			{
				menuFileOpenSolution_Click(sender, EventArgs.Empty);
			}
			else if (e.Button == openToolBarButton)
			{
				menuFileOpenItem_Click(sender, EventArgs.Empty);
			}
			else if (e.Button == saveToolBarButton)
			{
				if (menuFileSaveItem.Enabled)
				{
					menuFileSaveItem_Click(sender, EventArgs.Empty);
				}
				else
				{
					menuFileSaveAsItem_Click(sender, EventArgs.Empty);
				}
			}
			else if (e.Button == buildToolBarButton)
			{
				menuDocBuildItem_Click(sender, EventArgs.Empty);
			}
			else if (e.Button == viewToolBarButton)
			{
				menuDocViewItem_Click(sender, EventArgs.Empty);
			}
		}

		private void assembliesListView_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (assembliesListView.SelectedIndices.Count > 0)
			{
				editButton.Enabled = true;
				deleteButton.Enabled = true;
			}
			else
			{
				editButton.Enabled = false;
				deleteButton.Enabled = false;
			}
		}

		private void addButton_Click (object sender, System.EventArgs e)
		{
			AssemblySlashDocForm  form = new AssemblySlashDocForm();

			form.Text = "Add Assembly Filename and XML Documentation Filename";
			form.StartPosition = FormStartPosition.CenterParent;

			if (form.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
			{
				AssemblySlashDoc  assemblySlashDoc = new AssemblySlashDoc();

				assemblySlashDoc.AssemblyFilename = form.AssemblyFilename;
				assemblySlashDoc.SlashDocFilename = form.SlashDocFilename;
				project.AssemblySlashDocs.Add(assemblySlashDoc);
				AddRowToListView(assemblySlashDoc);
				EnableMenuItems(true);
			}

			EnableAssemblyItems();
		}

		private void editButton_Click (object sender, System.EventArgs e)
		{
			if (assembliesListView.SelectedItems.Count > 0)
			{
				AssemblySlashDocForm form = new AssemblySlashDocForm();
				int nIndex = assembliesListView.SelectedItems[0].Index;

				form.Text = "Edit Assembly Filename and XML Documentation Filename";
				form.StartPosition = FormStartPosition.CenterParent;
				form.AssemblyFilename = ((AssemblySlashDoc)project.AssemblySlashDocs[nIndex]).AssemblyFilename;
				form.SlashDocFilename = ((AssemblySlashDoc)project.AssemblySlashDocs[nIndex]).SlashDocFilename;

				if (form.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
				{
					((AssemblySlashDoc)project.AssemblySlashDocs[nIndex]).AssemblyFilename = form.AssemblyFilename;
					((AssemblySlashDoc)project.AssemblySlashDocs[nIndex]).SlashDocFilename = form.SlashDocFilename;

					string[] subItems = new string[1];

					assembliesListView.SelectedItems[0].Text = Path.GetFileName(((AssemblySlashDoc)project.AssemblySlashDocs[nIndex]).AssemblyFilename);
					subItems[0] = Path.GetFileName(((AssemblySlashDoc)project.AssemblySlashDocs[nIndex]).SlashDocFilename);
				}
			}
		}

		/// <summary>
		/// Removes the selected assembly and /doc file pair from the listview.
		/// </summary>
		/// <remarks>
		/// If the row being deleted was the only one left in the listview then
		/// the documentation buttons are disabled.
		/// </remarks>
		/// <param name="sender">The sender (not used).</param>
		/// <param name="e">The event arguments (not used).</param>
		protected void deleteButton_Click (object sender, System.EventArgs e)
		{
			if (assembliesListView.SelectedItems.Count > 0)
			{
				ListViewItem  listViewItem = assembliesListView.SelectedItems[0];

				project.AssemblySlashDocs.RemoveAt(listViewItem.Index);
				listViewItem.Remove();
			}

			EnableAssemblyItems();
		}

		/// <summary>
		/// Brings up the form for entering namespace summaries.
		/// </summary>
		/// <remarks>
		/// Calls XmlDocumenter to build an XML file documenting the assemblies
		/// currently in the project.  This file is used to discover all of the
		/// namespaces currently being documented in case any new ones have been
		/// added.  A <see cref="System.Collections.Hashtable"/> with the namespace
		/// names as keys and any existing summaries as values is passed in to
		/// a form which allows editing of the namespace summaries.  If the ok button
		/// is selected in the form then the Hashtable becomes the main one used by
		/// NDoc and passed into documenters for building documentation.
		/// </remarks>
		/// <param name="sender">The sender (not used).</param>
		/// <param name="e">The event arguments (not used).</param>
		protected void namespaceSummariesButton_Click (object sender, System.EventArgs e)
		{
			NamespaceSummariesForm form;
			Hashtable editNamespaceSummaries = new Hashtable();
			XmlDocumenter xmlDocumenter = new XmlDocumenter();
			XmlDocument xmlDocumentation = new XmlDocument();

			// Check for any new namespaces and add them to the Hashtable
			((XmlDocumenterConfig)xmlDocumenter.Config).OutputFile = @"./namespaceSummaries.xml";
			xmlDocumenter.Build(project);
			xmlDocumentation.Load(@"./namespaceSummaries.xml");
			File.Delete(@"./namespaceSummaries.xml");

			XmlNodeList namespaceNodes = xmlDocumentation.SelectNodes("/ndoc/assembly/module/namespace");
			
			foreach (XmlNode namespaceNode in namespaceNodes)
			{
				string namespaceName = (string)namespaceNode.Attributes["name"].Value;

				if (project.NamespaceSummaries.ContainsKey(namespaceName))
				{
					editNamespaceSummaries[namespaceName] = project.NamespaceSummaries[namespaceName];
				}
				else
				{
					editNamespaceSummaries[namespaceName] = "";
				}
			}

			form = new NamespaceSummariesForm(editNamespaceSummaries);
			form.StartPosition = FormStartPosition.CenterParent;

			if (form.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
			{
				project.NamespaceSummaries = editNamespaceSummaries;
			}
		}

		private void comboBoxDocumenters_SelectedIndexChanged (object sender, System.EventArgs e)
		{
			propertyGrid.SelectedObject = ((IDocumenter)project.Documenters[comboBoxDocumenters.SelectedIndex]).Config;
		}
		#endregion // Event Handlers

	}
}
