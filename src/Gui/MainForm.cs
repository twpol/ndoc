// MainForm.cs - main GUI interface to NDoc
// Copyright (C) 2001  Kral Ferch, Keith Hill
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
		private System.Windows.Forms.ComboBox comboBoxDocumenters;
		private System.Windows.Forms.PropertyGrid propertyGrid;
		private NDoc.Gui.HeaderGroupBox documenterHeaderGroupBox;
		private System.Windows.Forms.Label labelDocumenters;
		#endregion // Required Designer Fields

		private Project project;
		private string processDirectory;
		private string projectFilename;
		private string untitledProjectName = "(Untitled)";
		private int maxMRU = 8;
		private Thread buildThread;
		private System.Windows.Forms.ToolBarButton solutionToolBarButton;
		private System.Windows.Forms.MenuItem menuFileOpenSolution;
		private System.Windows.Forms.MenuItem menuHelpItem;
		private System.Windows.Forms.MenuItem menuReleaseNotes;
		private System.Windows.Forms.MenuItem menuAboutItem;
		private System.Windows.Forms.MenuItem menuSpacerItem5;
		private System.Windows.Forms.MenuItem menuHelpIndexItem;
		private System.Windows.Forms.MenuItem menuTagReferenceItem;
		private System.Windows.Forms.MenuItem menuSpacerItem4;
		private System.Windows.Forms.MenuItem menuSpacerItem6;
		private System.Windows.Forms.MenuItem menuCancelBuildItem;
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

			// Allow developers to continue to compile their assemblies while NDoc is running.
			AppDomain.CurrentDomain.SetShadowCopyFiles();

			this.SetStyle(ControlStyles.DoubleBuffer, true);

			Thread.CurrentThread.Name = "GUI";

			project = new Project();
			project.Modified += new ProjectModifiedEventHandler(OnProjectModified);

			foreach (IDocumenter documenter in project.Documenters)
			{
				comboBoxDocumenters.Items.Add(documenter.Name);
			}

			ReadConfig();

			processDirectory = Directory.GetCurrentDirectory();

			// If a project document wasn't passed in on the command line
			// then try loading up the most recently used project file.
			if (startingProjectFilename == null)
			{
				while (recentProjectFilenames.Count > 0)
				{
					if (File.Exists(recentProjectFilenames[0]))
					{
						FileOpen(recentProjectFilenames[0]);
						break;
					}
					else
					{
						//the project file was not found, remove it from the MRU
						recentProjectFilenames.RemoveAt(0);
					}
				}
				if (recentProjectFilenames.Count == 0)
				{
					//there was no project to load
					projectFilename = untitledProjectName;
					EnableMenuItems(false);
				}
			}
			else
			{
				//load project passed on the command line
				if (File.Exists(startingProjectFilename))
				{
					FileOpen(startingProjectFilename);
				}
				else
				{
					MessageBox.Show(
						this, 
						"The NDoc project file '" + startingProjectFilename 
							+ "' does not exist.", "Error loading NDoc project file",
						MessageBoxButtons.OK,
						MessageBoxIcon.Stop
					);
					Clear();
				}
			}

			EnableAssemblyItems();
			MakeMRUMenu();

			menuFileCloseItem.Visible = false;

			SetWindowTitle();
		}

		/// <summary>Calls <see cref="WriteConfig"/> to write out the config
		/// file and calls Dispose() on base and components.</summary>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
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
			this.menuDocBuildItem = new System.Windows.Forms.MenuItem();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.menuFileExitItem = new System.Windows.Forms.MenuItem();
			this.newToolBarButton = new System.Windows.Forms.ToolBarButton();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.menuFileSaveItem = new System.Windows.Forms.MenuItem();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuFileItem = new System.Windows.Forms.MenuItem();
			this.menuFileNewItem = new System.Windows.Forms.MenuItem();
			this.menuFileOpenSolution = new System.Windows.Forms.MenuItem();
			this.menuFileOpenItem = new System.Windows.Forms.MenuItem();
			this.menuFileCloseItem = new System.Windows.Forms.MenuItem();
			this.menuSpacerItem1 = new System.Windows.Forms.MenuItem();
			this.menuFileSaveAsItem = new System.Windows.Forms.MenuItem();
			this.menuSpacerItem2 = new System.Windows.Forms.MenuItem();
			this.menuFileRecentProjectsItem = new System.Windows.Forms.MenuItem();
			this.menuSpacerItem3 = new System.Windows.Forms.MenuItem();
			this.menuDocItem = new System.Windows.Forms.MenuItem();
			this.menuDocViewItem = new System.Windows.Forms.MenuItem();
			this.menuSpacerItem6 = new System.Windows.Forms.MenuItem();
			this.menuCancelBuildItem = new System.Windows.Forms.MenuItem();
			this.menuHelpItem = new System.Windows.Forms.MenuItem();
			this.menuHelpIndexItem = new System.Windows.Forms.MenuItem();
			this.menuTagReferenceItem = new System.Windows.Forms.MenuItem();
			this.menuSpacerItem4 = new System.Windows.Forms.MenuItem();
			this.menuReleaseNotes = new System.Windows.Forms.MenuItem();
			this.menuSpacerItem5 = new System.Windows.Forms.MenuItem();
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
			// menuDocBuildItem
			// 
			this.menuDocBuildItem.Index = 0;
			this.menuDocBuildItem.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftB;
			this.menuDocBuildItem.Text = "&Build";
			this.menuDocBuildItem.Click += new System.EventHandler(this.menuDocBuildItem_Click);
			// 
			// progressBar
			// 
			this.progressBar.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.progressBar.Location = new System.Drawing.Point(334, 593);
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
			// menuFileSaveItem
			// 
			this.menuFileSaveItem.Index = 5;
			this.menuFileSaveItem.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
			this.menuFileSaveItem.Text = "&Save";
			this.menuFileSaveItem.Click += new System.EventHandler(this.menuFileSaveItem_Click);
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuFileItem,
																					  this.menuDocItem,
																					  this.menuHelpItem});
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
			this.menuFileNewItem.Shortcut = System.Windows.Forms.Shortcut.CtrlN;
			this.menuFileNewItem.Text = "&New";
			this.menuFileNewItem.Click += new System.EventHandler(this.menuFileNewItem_Click);
			// 
			// menuFileOpenSolution
			// 
			this.menuFileOpenSolution.Index = 1;
			this.menuFileOpenSolution.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftN;
			this.menuFileOpenSolution.Text = "New from &Visual Studio Solution...";
			this.menuFileOpenSolution.Click += new System.EventHandler(this.menuFileOpenSolution_Click);
			// 
			// menuFileOpenItem
			// 
			this.menuFileOpenItem.Index = 2;
			this.menuFileOpenItem.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
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
			// menuSpacerItem2
			// 
			this.menuSpacerItem2.Index = 7;
			this.menuSpacerItem2.Text = "-";
			// 
			// menuFileRecentProjectsItem
			// 
			this.menuFileRecentProjectsItem.Index = 8;
			this.menuFileRecentProjectsItem.Text = "&Recent Projects";
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
																						this.menuDocViewItem,
																						this.menuSpacerItem6,
																						this.menuCancelBuildItem});
			this.menuDocItem.Text = "&Documentation";
			// 
			// menuDocViewItem
			// 
			this.menuDocViewItem.Index = 1;
			this.menuDocViewItem.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftV;
			this.menuDocViewItem.Text = "&View";
			this.menuDocViewItem.Click += new System.EventHandler(this.menuDocViewItem_Click);
			// 
			// menuSpacerItem6
			// 
			this.menuSpacerItem6.Index = 2;
			this.menuSpacerItem6.Text = "-";
			// 
			// menuCancelBuildItem
			// 
			this.menuCancelBuildItem.Enabled = false;
			this.menuCancelBuildItem.Index = 3;
			this.menuCancelBuildItem.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftC;
			this.menuCancelBuildItem.Text = "&Cancel Build";
			this.menuCancelBuildItem.Click += new System.EventHandler(this.menuCancelBuildItem_Click);
			// 
			// menuHelpItem
			// 
			this.menuHelpItem.Index = 2;
			this.menuHelpItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuHelpIndexItem,
																						 this.menuTagReferenceItem,
																						 this.menuSpacerItem4,
																						 this.menuReleaseNotes,
																						 this.menuSpacerItem5,
																						 this.menuAboutItem});
			this.menuHelpItem.Text = "&Help";
			// 
			// menuHelpIndexItem
			// 
			this.menuHelpIndexItem.Enabled = false;
			this.menuHelpIndexItem.Index = 0;
			this.menuHelpIndexItem.Text = "&NDoc Help";
			this.menuHelpIndexItem.Click += new System.EventHandler(this.menuHelpIndexItem_Click);
			// 
			// menuTagReferenceItem
			// 
			this.menuTagReferenceItem.Index = 1;
			this.menuTagReferenceItem.Shortcut = System.Windows.Forms.Shortcut.F1;
			this.menuTagReferenceItem.Text = "&Documentation Tag Reference";
			this.menuTagReferenceItem.Click += new System.EventHandler(this.menuTagReferenceItem_Click);
			// 
			// menuSpacerItem4
			// 
			this.menuSpacerItem4.Index = 2;
			this.menuSpacerItem4.Text = "-";
			// 
			// menuReleaseNotes
			// 
			this.menuReleaseNotes.Index = 3;
			this.menuReleaseNotes.Text = "Open &Release Notes";
			this.menuReleaseNotes.Click += new System.EventHandler(this.menuReleaseNotes_Click);
			// 
			// menuSpacerItem5
			// 
			this.menuSpacerItem5.Index = 4;
			this.menuSpacerItem5.Text = "-";
			// 
			// menuAboutItem
			// 
			this.menuAboutItem.Index = 5;
			this.menuAboutItem.Text = "&About NDoc";
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
			this.cancelToolBarButton.ImageIndex = 5;
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
			this.documenterHeaderGroupBox.Size = new System.Drawing.Size(480, 397);
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
			this.propertyGrid.Size = new System.Drawing.Size(456, 333);
			this.propertyGrid.TabIndex = 0;
			this.propertyGrid.Text = "PropertyGrid";
			this.propertyGrid.ViewBackColor = System.Drawing.SystemColors.Window;
			this.propertyGrid.ViewForeColor = System.Drawing.SystemColors.WindowText;
			this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
			// 
			// viewToolBarButton
			// 
			this.viewToolBarButton.ImageIndex = 6;
			this.viewToolBarButton.ToolTipText = "View Documentation (Ctrl+Shift+V)";
			// 
			// statusBar
			// 
			this.statusBar.Location = new System.Drawing.Point(0, 590);
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
			this.openToolBarButton.ImageIndex = 2;
			this.openToolBarButton.ToolTipText = "Open ";
			// 
			// separatorToolBarButton
			// 
			this.separatorToolBarButton.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// solutionToolBarButton
			// 
			this.solutionToolBarButton.ImageIndex = 1;
			this.solutionToolBarButton.ToolTipText = "New from Visual Studio Solution";
			// 
			// saveToolBarButton
			// 
			this.saveToolBarButton.ImageIndex = 3;
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
			this.namespaceSummariesButton.Location = new System.Drawing.Point(397, 120);
			this.namespaceSummariesButton.Name = "namespaceSummariesButton";
			this.namespaceSummariesButton.Size = new System.Drawing.Size(75, 32);
			this.namespaceSummariesButton.TabIndex = 17;
			this.namespaceSummariesButton.Text = "Namespace\nSummaries";
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
			this.assembliesListView.Size = new System.Drawing.Size(368, 120);
			this.assembliesListView.TabIndex = 13;
			this.assembliesListView.View = System.Windows.Forms.View.List;
			this.assembliesListView.DoubleClick += new System.EventHandler(this.assembliesListView_DoubleClick);
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
			this.buildToolBarButton.ImageIndex = 4;
			this.buildToolBarButton.ToolTipText = "Build Documentation (Ctrl+Shift+B)";
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(496, 610);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.progressBar,
																		  this.assembliesHeaderGroupBox,
																		  this.statusBar,
																		  this.toolBar,
																		  this.documenterHeaderGroupBox});
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
		private void OnProjectModified(object sender, EventArgs e)
		{
			SetWindowTitle();
		}

		private void SetWindowTitle()
		{
			string projectName;

			if (projectFilename != null)
			{
				if (projectFilename == untitledProjectName)
				{
					projectName = projectFilename;
				}
				else
				{
					projectName = Path.GetFileName(projectFilename);
					projectName = projectName.Substring(0, projectName.LastIndexOf('.'));
				}

				this.Text = "NDoc - " + projectName + (project.IsDirty ? "*" : "");
			}
		}

		/// <summary>
		/// Enables/disables the Save and SaveAs menu items.
		/// </summary>
		/// <param name="bEnable"><b>true</b> for enabling the menu items, <b>false</b> for disabling.</param>
		private void EnableMenuItems(bool bEnable)
		{
			menuFileSaveItem.Enabled = bEnable;
			menuFileSaveAsItem.Enabled = bEnable;
			saveToolBarButton.Enabled = bEnable;
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
									if (!reader.MoveToNextAttribute()) break;
									this.Left = int.Parse(reader.Value);

									if (!reader.MoveToNextAttribute()) break;
									this.Top = int.Parse(reader.Value);

									if (!reader.MoveToNextAttribute()) break;
									this.Width = int.Parse(reader.Value);

									if (!reader.MoveToNextAttribute()) break;
									//HACK: subtract 20 to last height to keep it constant
									this.Height = int.Parse(reader.Value) - 20;
									
									if (!reader.MoveToNextAttribute()) break;
									if (bool.Parse(reader.Value))
									{
										this.WindowState = FormWindowState.Maximized;
									}
									break;
							}
						}
					}
				}
				catch (XmlException)
				{
					//config file is corrupted, delete it
					reader.Close();
					File.Delete(guiConfigFilename);
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
			
			bool max = (this.WindowState == FormWindowState.Maximized);
			//restore the window state before saving it's location
			//this might be an annoyance if the config is not saved 
			//during application exit
			this.WindowState = FormWindowState.Normal;
			
			writer.WriteStartElement("ndoc.gui");
			WriteRecentProjects(writer);
			writer.WriteElementString("documenter", ((IDocumenter)project.Documenters[comboBoxDocumenters.SelectedIndex]).Name);
			writer.WriteStartElement("window");
			writer.WriteAttributeString("left", this.Location.X.ToString());
			writer.WriteAttributeString("top", this.Location.Y.ToString());
			writer.WriteAttributeString("width", this.Width.ToString());
			writer.WriteAttributeString("height", this.Height.ToString());
			writer.WriteAttributeString("maximized", max.ToString()); 
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
				foreach (AssemblySlashDoc assemblySlashDoc2 in project.GetAssemblySlashDocs())
				{
					AddRowToListView(assemblySlashDoc2);
				}

				UpdateMRUList();

				EnableMenuItems(true);

				bFailed = false;
			}
			catch (DocumenterException docEx)
			{
				ErrorForm errorForm = new ErrorForm("Unable to read in project file", docEx);
				errorForm.ShowDialog();
			}
			catch (Exception ex)
			{
				string msg = "An error occured while trying to read in project file:\n" + fileName + ".";
				ErrorForm errorForm = new ErrorForm(msg, ex);
				errorForm.ShowDialog();
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
			try
			{
				project.Write(fileName);
				SetWindowTitle();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.InnerException.Message, "Project Save", 
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private void FileSaveAs()
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

			if(saveFileDlg.ShowDialog() == DialogResult.OK)
			{
				FileSave(saveFileDlg.FileName);

				projectFilename = saveFileDlg.FileName;
				SetWindowTitle();
				UpdateMRUList();
				EnableMenuItems(true);
			}
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
			#warning This code assumes there is always more than one documenter.

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

			if (currentIndex != -1)
			{
				comboBoxDocumenters.SelectedIndex = currentIndex;
			}
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
			openFileDlg.InitialDirectory = Directory.GetCurrentDirectory();
			openFileDlg.Filter = "Visual Studio Solution files (*.sln)|*.sln|All files (*.*)|*.*" ;

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
						project.AddAssemblySlashDoc(asd);
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
			openFileDlg.InitialDirectory = Directory.GetCurrentDirectory();
			openFileDlg.Filter = "Project files (*.ndoc)|*.ndoc|All files (*.*)|*.*" ;

			if(openFileDlg.ShowDialog() == DialogResult.OK)
			{
				FileOpen(openFileDlg.FileName);
			}
		}

		private void menuFileSaveItem_Click (object sender, System.EventArgs e)
		{
			SaveOrSaveAs();
		}

		private void SaveOrSaveAs()
		{
			if (projectFilename == untitledProjectName)
			{
				FileSaveAs();
			}
			else
			{
				FileSave(projectFilename);
			}
		}

		private void menuFileSaveAsItem_Click (object sender, System.EventArgs e)
		{
			FileSaveAs();
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
			string fileName = ((MenuItem)sender).Text.Substring(3);

			if (File.Exists(fileName))
			{
				FileOpen(fileName);
			}
			else
			{
				try
				{
					MessageBox.Show(this, "Project file doesn't exist.", "NDoc Unable to Open Project File",
						            MessageBoxButtons.OK, MessageBoxIcon.Information);
					recentProjectFilenames.Remove(fileName);
					MakeMRUMenu();
				}
				catch
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

			//make sure the current directory is the project directory
			if (projectFilename != untitledProjectName)
			{
				Directory.SetCurrentDirectory(Path.GetDirectoryName(projectFilename));
			}

			string message = documenter.CanBuild(project);

			if (message != null)
			{
				MessageBox.Show(
					this,
					message,
					"NDoc",
					MessageBoxButtons.OK,
					MessageBoxIcon.Stop);

				return;
			}

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
				buildThread.Join(200);
			}
			finally
			{
				// Just in case some weird exception happens, we don't get stuck
				// with a busy cursor.
				this.Cursor = Cursors.Default;
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
			Exception innermostException;
			do
			{
				if (iex is ThreadAbortException)
				{
					return;
				}
				innermostException = iex;
				iex = iex.InnerException;
			} while (iex != null);

			// Process exception
			string msg = "An error occured while trying to build the documentation.";
			if (innermostException is DocumenterException)
			{
				ErrorForm errorForm = new ErrorForm(msg, innermostException);
				errorForm.Text = "NDoc Documenter Error";
				errorForm.ShowDialog(this);
			}
			else
			{
				ErrorForm errorForm = new ErrorForm(msg, innermostException);
				errorForm.ShowDialog();
			}
		}

		private void menuCancelBuildItem_Click(object sender, System.EventArgs e)
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

			foreach (MenuItem subMenuItem in menuFileItem.MenuItems)
			{
				subMenuItem.Enabled = !starting;
			}
			menuDocBuildItem.Enabled = !starting;
			menuDocViewItem.Enabled = !starting;
			menuCancelBuildItem.Enabled = starting;
			menuAboutItem.Enabled = !starting;

			assembliesHeaderGroupBox.Enabled = !starting;
			documenterHeaderGroupBox.Enabled = !starting;
            
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
			IDocumenter documenter = 
				(IDocumenter)project.Documenters[comboBoxDocumenters.SelectedIndex];

			//make sure the current directory is the project directory
			if (projectFilename != untitledProjectName)
			{
				Directory.SetCurrentDirectory(Path.GetDirectoryName(projectFilename));
			}

			try
			{
				documenter.View();
			}
			catch (FileNotFoundException)
			{
				DialogResult result = MessageBox.Show(
					this,
					"The documentation has not been built yet.\n"
					+ "Would you like to build it now?",
					"NDoc",
					MessageBoxButtons.YesNoCancel,
					MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					menuDocBuildItem_Click(sender, e);
					menuDocViewItem_Click(sender, e);
				}
			}
		}

		private void menuHelpIndexItem_Click(object sender, System.EventArgs e)
		{
			//TODO: open help here
		}

		private void menuTagReferenceItem_Click(object sender, System.EventArgs e)
		{
			try
			{
				System.Diagnostics.Process.Start(Path.Combine(Application.StartupPath, "tags.html"));
			}
			catch(System.ComponentModel.Win32Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Help", 
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private void menuReleaseNotes_Click(object sender, System.EventArgs e)
		{
			System.Diagnostics.Process.Start("notepad",
				Path.Combine(Application.StartupPath, "README.txt"));
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
				menuCancelBuildItem_Click(sender, EventArgs.Empty);
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
				menuFileSaveItem_Click(sender, EventArgs.Empty);
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
				try
				{
					assemblySlashDoc.AssemblyFilename = form.AssemblyFilename;
					assemblySlashDoc.SlashDocFilename = form.SlashDocFilename;
					project.AddAssemblySlashDoc(assemblySlashDoc);
					AddRowToListView(assemblySlashDoc);
					EnableMenuItems(true);
				}
				catch(Project.AssemblyAlreadyExistsException)
				{
					//ignore this exception
				}
			}

			EnableAssemblyItems();
		}

		private void assembliesListView_DoubleClick(object sender, System.EventArgs e)
		{
			editButton_Click(sender, e);
		}

		private void editButton_Click (object sender, System.EventArgs e)
		{
			if (assembliesListView.SelectedItems.Count > 0)
			{
				AssemblySlashDocForm form = new AssemblySlashDocForm();
				int nIndex = assembliesListView.SelectedItems[0].Index;

				form.Text = "Edit Assembly Filename and XML Documentation Filename";
				form.StartPosition = FormStartPosition.CenterParent;
				form.AssemblyFilename = project.GetAssemblySlashDoc(nIndex).AssemblyFilename;
				form.SlashDocFilename = project.GetAssemblySlashDoc(nIndex).SlashDocFilename;

				if (form.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
				{
					project.GetAssemblySlashDoc(nIndex).AssemblyFilename = form.AssemblyFilename;
					project.GetAssemblySlashDoc(nIndex).SlashDocFilename = form.SlashDocFilename;

					string[] subItems = new string[1];

					assembliesListView.SelectedItems[0].Text = Path.GetFileName(project.GetAssemblySlashDoc(nIndex).AssemblyFilename);
					subItems[0] = Path.GetFileName(project.GetAssemblySlashDoc(nIndex).SlashDocFilename);
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

				project.RemoveAssemblySlashDoc(listViewItem.Index);
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

			this.Cursor = Cursors.WaitCursor;
			try
			{

				IDocumenter documenter =
					(IDocumenter)project.Documenters[comboBoxDocumenters.SelectedIndex];

				string message = documenter.CanBuild(project, true);
				if (message != null)
				{
					MessageBox.Show(
						this,
						message,
						"NDoc",
						MessageBoxButtons.OK,
						MessageBoxIcon.Stop);

					return;
				}
				
				form = new NamespaceSummariesForm(project);
				form.StartPosition = FormStartPosition.CenterParent;

			}
			finally
			{
				this.Cursor = Cursors.Arrow;
			}

			form.ShowDialog(this);
		}

		private void comboBoxDocumenters_SelectedIndexChanged (object sender, System.EventArgs e)
		{
			if (propertyGrid.SelectedObject != null)
			{
				((IDocumenterConfig)propertyGrid.SelectedObject).SetProject(null);
			}

			IDocumenterConfig documenterConfig = ((IDocumenter)project.Documenters[comboBoxDocumenters.SelectedIndex]).Config;
			documenterConfig.SetProject(project);
			propertyGrid.SelectedObject = documenterConfig;
		}

		/// <summary>Prompts the user to save the project if it's dirty.</summary>
		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			WriteConfig();

			if (project.IsDirty)
			{
				DialogResult result = MessageBox.Show(
					"Save changes to " + projectFilename + "?",
					"NDoc",
					MessageBoxButtons.YesNoCancel,
					MessageBoxIcon.Exclamation,
					MessageBoxDefaultButton.Button1);

				switch (result)
				{
					case DialogResult.Yes:
						SaveOrSaveAs();
						break;
					case DialogResult.No:
						break;
					case DialogResult.Cancel:
						e.Cancel = true;
						break;
				}
			}
		}

		private void propertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
		{
			propertyGrid.Refresh();
		}

		#endregion // Event Handlers

	}
}
