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
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;

using NDoc.Core;
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
		#endregion // Required Designer Fields

		private Project project;
		private string processDirectory;
		private string projectFilename;
		private string untitledProjectName = "(Untitled)";
		private Thread buildThread;
		private System.Windows.Forms.ToolBarButton solutionToolBarButton;
		private System.Windows.Forms.MenuItem menuFileOpenSolution;
		private System.Windows.Forms.MenuItem menuHelpItem;
		private System.Windows.Forms.MenuItem menuAboutItem;
		private System.Windows.Forms.MenuItem menuSpacerItem4;
		private System.Windows.Forms.MenuItem menuSpacerItem6;
		private System.Windows.Forms.MenuItem menuCancelBuildItem;
		private System.Windows.Forms.MenuItem menuViewLicense;
		private System.Windows.Forms.Splitter splitter1;
		private NDoc.Gui.HeaderGroupBox documenterHeaderGroupBox;
		private System.Windows.Forms.Label labelDocumenters;
		private System.Windows.Forms.ComboBox comboBoxDocumenters;
		private System.Windows.Forms.PropertyGrid propertyGrid;
		private NDoc.Gui.TraceWindowControl traceWindow1;
		private System.Windows.Forms.MenuItem menuView;
		private System.Windows.Forms.MenuItem menuViewBuildProgress;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuViewOptions;
		private System.Windows.Forms.MenuItem menuViewStatusBar;
		private NDocOptions options;
		private System.Windows.Forms.MenuItem menuHelpContents;
		private System.Windows.Forms.MenuItem menuHelpIndex;
		private System.Windows.Forms.MenuItem menuNDocOnline;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuViewDescriptions;
		private System.Windows.Forms.Timer timer1;

		private StringCollection recentProjectFilenames = new StringCollection();
		private Size windowSize;
		private int traceWindowHeight;
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
			this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint, true);
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			if ( DesignMode )
				return;

			// Allow developers to continue to compile their assemblies while NDoc is running.
			AppDomain.CurrentDomain.SetShadowCopyFiles();

			Thread.CurrentThread.Name = "GUI";

			project = new Project();
			project.Modified += new ProjectModifiedEventHandler(OnProjectModified);

			foreach (IDocumenter documenter in project.Documenters)
			{
				// build a development status string (alpha, beta, etc)
				string devStatus = string.Empty;
				if (documenter.DevelopmentStatus != DocumenterDevelopmentStatus.Stable)
				{
					devStatus = documenter.DevelopmentStatus.ToString();
					// want it uncapitalized
					devStatus = " (" + Char.ToLower(devStatus[0]) + devStatus.Substring(1)
						+ ")";
				}

				comboBoxDocumenters.Items.Add(documenter.Name + devStatus);
			}

			options = new NDocOptions();
			ReadConfig();

			//remember the initial size after reading config, 
			//but before the form is autoscaled
			windowSize = this.Size;
			traceWindowHeight = traceWindow1.Height;

			processDirectory = Directory.GetCurrentDirectory();

			// If a project document wasn't passed in on the command line
			// then try loading up the most recently used project file.
			if (startingProjectFilename == null)
			{
				if ( this.options.LoadLastProjectOnStart )
				{
					while (recentProjectFilenames.Count > 0)
					{
						if ( File.Exists(recentProjectFilenames[0]) )
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
				}
				if ( recentProjectFilenames.Count == 0 )
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
		
			this.traceWindow1.TraceText = string.Format( "[NDoc version {0}]\n", Assembly.GetExecutingAssembly().GetName().Version );
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			timer1.Enabled = false;

			//HACK
			//if the form has been resized with autoscaling,
			//restore the original size.
			if (this.Size != windowSize)
			{
				this.Size = windowSize;
			}
			//same with trace window height
			if (traceWindow1.Height != traceWindowHeight)
			{
				traceWindow1.Height = traceWindowHeight;
			}

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
			this.menuView = new System.Windows.Forms.MenuItem();
			this.menuViewBuildProgress = new System.Windows.Forms.MenuItem();
			this.menuViewStatusBar = new System.Windows.Forms.MenuItem();
			this.menuViewDescriptions = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuViewOptions = new System.Windows.Forms.MenuItem();
			this.menuHelpItem = new System.Windows.Forms.MenuItem();
			this.menuHelpContents = new System.Windows.Forms.MenuItem();
			this.menuHelpIndex = new System.Windows.Forms.MenuItem();
			this.menuSpacerItem4 = new System.Windows.Forms.MenuItem();
			this.menuViewLicense = new System.Windows.Forms.MenuItem();
			this.menuNDocOnline = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuAboutItem = new System.Windows.Forms.MenuItem();
			this.addButton = new System.Windows.Forms.Button();
			this.slashDocHeader = new System.Windows.Forms.ColumnHeader();
			this.cancelToolBarButton = new System.Windows.Forms.ToolBarButton();
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
			this.traceWindow1 = new NDoc.Gui.TraceWindowControl();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.documenterHeaderGroupBox = new NDoc.Gui.HeaderGroupBox();
			this.labelDocumenters = new System.Windows.Forms.Label();
			this.comboBoxDocumenters = new System.Windows.Forms.ComboBox();
			this.propertyGrid = new System.Windows.Forms.PropertyGrid();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.statusBarTextPanel)).BeginInit();
			this.assembliesHeaderGroupBox.SuspendLayout();
			this.documenterHeaderGroupBox.SuspendLayout();
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
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
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
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.Images.Add((System.Drawing.Bitmap) resources.GetObject("imageList1.Images.New"));
            this.imageList1.Images.Add((System.Drawing.Bitmap) resources.GetObject("imageList1.Images.Solution"));
            this.imageList1.Images.Add((System.Drawing.Bitmap) resources.GetObject("imageList1.Images.Open"));
            this.imageList1.Images.Add((System.Drawing.Bitmap) resources.GetObject("imageList1.Images.Save"));
            this.imageList1.Images.Add((System.Drawing.Bitmap) resources.GetObject("imageList1.Images.Build"));
            this.imageList1.Images.Add((System.Drawing.Bitmap) resources.GetObject("imageList1.Images.Cancel"));
            this.imageList1.Images.Add((System.Drawing.Bitmap) resources.GetObject("imageList1.Images.View"));
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
																					  this.menuView,
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
			// menuView
			// 
			this.menuView.Index = 2;
			this.menuView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuViewBuildProgress,
																					 this.menuViewStatusBar,
																					 this.menuViewDescriptions,
																					 this.menuItem1,
																					 this.menuViewOptions});
			this.menuView.Text = "View";
			// 
			// menuViewBuildProgress
			// 
			this.menuViewBuildProgress.Checked = true;
			this.menuViewBuildProgress.Index = 0;
			this.menuViewBuildProgress.Text = "Build Window";
			this.menuViewBuildProgress.Click += new System.EventHandler(this.menuViewBuildProgress_Click);
			// 
			// menuViewStatusBar
			// 
			this.menuViewStatusBar.Checked = true;
			this.menuViewStatusBar.Index = 1;
			this.menuViewStatusBar.Text = "Status Bar";
			this.menuViewStatusBar.Click += new System.EventHandler(this.menuViewStatusBar_Click);
			// 
			// menuViewDescriptions
			// 
			this.menuViewDescriptions.Checked = true;
			this.menuViewDescriptions.Index = 2;
			this.menuViewDescriptions.Text = "Descriptions";
			this.menuViewDescriptions.Click += new System.EventHandler(this.menuViewDescriptions_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 3;
			this.menuItem1.Text = "-";
			// 
			// menuViewOptions
			// 
			this.menuViewOptions.Index = 4;
			this.menuViewOptions.Text = "Options...";
			this.menuViewOptions.Click += new System.EventHandler(this.menuViewOptions_Click);
			// 
			// menuHelpItem
			// 
			this.menuHelpItem.Index = 3;
			this.menuHelpItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuHelpContents,
																						 this.menuHelpIndex,
																						 this.menuSpacerItem4,
																						 this.menuViewLicense,
																						 this.menuNDocOnline,
																						 this.menuItem3,
																						 this.menuAboutItem});
			this.menuHelpItem.Text = "&Help";
			// 
			// menuHelpContents
			// 
			this.menuHelpContents.Index = 0;
			this.menuHelpContents.Shortcut = System.Windows.Forms.Shortcut.F1;
			this.menuHelpContents.Text = "Contents...";
			this.menuHelpContents.Click += new System.EventHandler(this.menuHelpContents_Click);
			// 
			// menuHelpIndex
			// 
			this.menuHelpIndex.Index = 1;
			this.menuHelpIndex.Text = "Index...";
			this.menuHelpIndex.Click += new System.EventHandler(this.menuHelpIndex_Click);
			// 
			// menuSpacerItem4
			// 
			this.menuSpacerItem4.Index = 2;
			this.menuSpacerItem4.Text = "-";
			// 
			// menuViewLicense
			// 
			this.menuViewLicense.Index = 3;
			this.menuViewLicense.Text = "View License";
			this.menuViewLicense.Click += new System.EventHandler(this.menuViewLicense_Click);
			// 
			// menuNDocOnline
			// 
			this.menuNDocOnline.Index = 4;
			this.menuNDocOnline.Text = "NDoc Online";
			this.menuNDocOnline.Click += new System.EventHandler(this.menuNDocOnline_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 5;
			this.menuItem3.Text = "-";
			// 
			// menuAboutItem
			// 
			this.menuAboutItem.Index = 6;
			this.menuAboutItem.Text = "&About NDoc...";
			this.menuAboutItem.Click += new System.EventHandler(this.menuAboutItem_Click);
			// 
			// addButton
			// 
			this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.addButton.Location = new System.Drawing.Point(408, 18);
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
			this.statusBar.VisibleChanged += new System.EventHandler(this.statusBar_VisibleChanged);
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
			this.editButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.editButton.Enabled = false;
			this.editButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.editButton.Location = new System.Drawing.Point(408, 50);
			this.editButton.Name = "editButton";
			this.editButton.TabIndex = 15;
			this.editButton.Text = "Edit";
			this.editButton.Click += new System.EventHandler(this.editButton_Click);
			// 
			// namespaceSummariesButton
			// 
			this.namespaceSummariesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.namespaceSummariesButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.namespaceSummariesButton.Location = new System.Drawing.Point(408, 114);
			this.namespaceSummariesButton.Name = "namespaceSummariesButton";
			this.namespaceSummariesButton.Size = new System.Drawing.Size(75, 32);
			this.namespaceSummariesButton.TabIndex = 17;
			this.namespaceSummariesButton.Text = "Namespace\nSummaries";
			this.namespaceSummariesButton.Click += new System.EventHandler(this.namespaceSummariesButton_Click);
			// 
			// assembliesHeaderGroupBox
			// 
			this.assembliesHeaderGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.assembliesHeaderGroupBox.Controls.Add(this.assembliesListView);
			this.assembliesHeaderGroupBox.Controls.Add(this.editButton);
			this.assembliesHeaderGroupBox.Controls.Add(this.namespaceSummariesButton);
			this.assembliesHeaderGroupBox.Controls.Add(this.deleteButton);
			this.assembliesHeaderGroupBox.Controls.Add(this.addButton);
			this.assembliesHeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.assembliesHeaderGroupBox.Location = new System.Drawing.Point(0, 28);
			this.assembliesHeaderGroupBox.Name = "assembliesHeaderGroupBox";
			this.assembliesHeaderGroupBox.Padding = 0;
			this.assembliesHeaderGroupBox.Size = new System.Drawing.Size(496, 152);
			this.assembliesHeaderGroupBox.TabIndex = 22;
			this.assembliesHeaderGroupBox.TabStop = false;
			this.assembliesHeaderGroupBox.Text = "Select Assemblies to Document";
			// 
			// assembliesListView
			// 
			this.assembliesListView.AllowDrop = true;
			this.assembliesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.assembliesListView.ForeColor = System.Drawing.SystemColors.WindowText;
			this.assembliesListView.Location = new System.Drawing.Point(16, 24);
			this.assembliesListView.Name = "assembliesListView";
			this.assembliesListView.Size = new System.Drawing.Size(384, 120);
			this.assembliesListView.TabIndex = 13;
			this.assembliesListView.View = System.Windows.Forms.View.List;
			this.assembliesListView.DoubleClick += new System.EventHandler(this.assembliesListView_DoubleClick);
			this.assembliesListView.DragDrop += new System.Windows.Forms.DragEventHandler(this.assembliesListView_DragDrop);
			this.assembliesListView.DragEnter += new System.Windows.Forms.DragEventHandler(this.assembliesListView_DragEnter);
			this.assembliesListView.SelectedIndexChanged += new System.EventHandler(this.assembliesListView_SelectedIndexChanged);
			// 
			// deleteButton
			// 
			this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.deleteButton.Enabled = false;
			this.deleteButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.deleteButton.Location = new System.Drawing.Point(408, 82);
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
			this.toolBar.Location = new System.Drawing.Point(0, 0);
			this.toolBar.Name = "toolBar";
			this.toolBar.ShowToolTips = true;
			this.toolBar.Size = new System.Drawing.Size(496, 28);
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
			// traceWindow1
			// 
			this.traceWindow1.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.traceWindow1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.traceWindow1.Location = new System.Drawing.Point(0, 462);
			this.traceWindow1.Name = "traceWindow1";
			this.traceWindow1.Size = new System.Drawing.Size(496, 128);
			this.traceWindow1.TabIndex = 25;
			this.traceWindow1.TabStop = false;
			this.traceWindow1.TraceText = "";
			this.traceWindow1.VisibleChanged += new System.EventHandler(this.traceWindow1_VisibleChanged);
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter1.Location = new System.Drawing.Point(0, 459);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(496, 3);
			this.splitter1.TabIndex = 26;
			this.splitter1.TabStop = false;
			// 
			// documenterHeaderGroupBox
			// 
			this.documenterHeaderGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.documenterHeaderGroupBox.Controls.Add(this.labelDocumenters);
			this.documenterHeaderGroupBox.Controls.Add(this.comboBoxDocumenters);
			this.documenterHeaderGroupBox.Controls.Add(this.propertyGrid);
			this.documenterHeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.documenterHeaderGroupBox.Location = new System.Drawing.Point(0, 180);
			this.documenterHeaderGroupBox.Name = "documenterHeaderGroupBox";
			this.documenterHeaderGroupBox.Padding = 0;
			this.documenterHeaderGroupBox.Size = new System.Drawing.Size(496, 279);
			this.documenterHeaderGroupBox.TabIndex = 27;
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
			// propertyGrid
			// 
			this.propertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.propertyGrid.CommandsVisibleIfAvailable = true;
			this.propertyGrid.LargeButtons = false;
			this.propertyGrid.LineColor = System.Drawing.SystemColors.ScrollBar;
			this.propertyGrid.Location = new System.Drawing.Point(8, 56);
			this.propertyGrid.Name = "propertyGrid";
			this.propertyGrid.Size = new System.Drawing.Size(480, 216);
			this.propertyGrid.TabIndex = 0;
			this.propertyGrid.Text = "PropertyGrid";
			this.propertyGrid.ViewBackColor = System.Drawing.SystemColors.Window;
			this.propertyGrid.ViewForeColor = System.Drawing.SystemColors.WindowText;
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 100;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// MainForm
			// 
			this.AllowDrop = true;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(496, 610);
			this.Controls.Add(this.documenterHeaderGroupBox);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.traceWindow1);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.assembliesHeaderGroupBox);
			this.Controls.Add(this.statusBar);
			this.Controls.Add(this.toolBar);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mainMenu1;
			this.MinimumSize = new System.Drawing.Size(504, 460);
			this.Name = "MainForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "NDoc";
			((System.ComponentModel.ISupportInitialize)(this.statusBarTextPanel)).EndInit();
			this.assembliesHeaderGroupBox.ResumeLayout(false);
			this.documenterHeaderGroupBox.ResumeLayout(false);
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

					if (count > this.options.MRUSize)
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

		private Point GetOnScreenLocation( Point pt )
		{
			// look for a screen that contains this point
			// if one is found the point is ok so return it
			foreach ( Screen screen in Screen.AllScreens )
			{
				if ( screen.Bounds.Contains( pt ) )
					return pt;
			}

			// otherwise return the upper left point of the primary screen
			return new Point( Screen.PrimaryScreen.WorkingArea.X, Screen.PrimaryScreen.WorkingArea.Y );
		}

		/// <summary>Reads in the NDoc configuration file from the
		/// application directory.</summary>
		/// <remarks>The config file stores the most recently used (MRU)
		/// list of project files.  It also stores which documenter was
		/// being used last.</remarks>
		private void ReadConfig()
		{
			Settings settings = new Settings( Settings.UserSettingsFile );

			this.Location = GetOnScreenLocation( (Point)settings.GetSetting( "gui", "location", new Point( Screen.PrimaryScreen.WorkingArea.Top, Screen.PrimaryScreen.WorkingArea.Left ) ) );

			Screen screen = Screen.FromControl( this );
			this.Size = (Size)settings.GetSetting( "gui", "size", new Size( screen.WorkingArea.Width / 3, screen.WorkingArea.Height - 20 ) );
			
			// size the window to the working area if it is larger (can happen when resolution changes)
			if ( this.Height > screen.WorkingArea.Height )
				this.Height = screen.WorkingArea.Height;
			if ( this.Width > screen.WorkingArea.Width )
				this.Width = screen.WorkingArea.Width;

			if ( settings.GetSetting( "gui", "maximized", false ) )
				this.WindowState = FormWindowState.Maximized;			

			this.traceWindow1.Visible = settings.GetSetting( "gui", "viewTrace", true );
			this.traceWindow1.Height = settings.GetSetting( "gui", "traceWindowHeight", this.traceWindow1.Height );
			this.statusBar.Visible = settings.GetSetting( "gui", "statusBar", true );
			this.ShowDescriptions = settings.GetSetting( "gui", "showDescriptions", true );

			IList list = recentProjectFilenames;
			settings.GetSettingList( "gui", "mru", typeof( string ), ref list );		
	
			string documenterName = settings.GetSetting( "gui", "documenter", "MSDN" );

			this.options.LoadLastProjectOnStart = settings.GetSetting( "gui", "loadLastProjectOnStart", true );
			this.options.ShowProgressOnBuild = settings.GetSetting( "gui", "showProgressOnBuild", false );
			this.options.MRUSize = settings.GetSetting( "gui", "mruSize", 8 );

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
			using( Settings settings = new Settings( Settings.UserSettingsFile ) )
			{
				if ( this.WindowState == FormWindowState.Maximized )
				{
					settings.SetSetting( "gui", "maximized", true );
				}
				else if ( this.WindowState == FormWindowState.Normal )
				{
					settings.SetSetting( "gui", "maximized", false );
					settings.SetSetting( "gui", "location", this.Location );
					settings.SetSetting( "gui", "size", this.Size );
				}
				settings.SetSetting( "gui", "viewTrace", this.traceWindow1.Visible );
				settings.SetSetting( "gui", "traceWindowHeight", this.traceWindow1.Height );
				settings.SetSetting( "gui", "statusBar", this.statusBar.Visible );
				settings.SetSetting( "gui", "showDescriptions", this.ShowDescriptions );
				
				if ( comboBoxDocumenters.SelectedIndex >= 0 )
					settings.SetSetting( "gui", "documenter", ((IDocumenter)project.Documenters[comboBoxDocumenters.SelectedIndex]).Name );

				// Trim our MRU list down to max amount before writing the config.
				while (recentProjectFilenames.Count > this.options.MRUSize)
					recentProjectFilenames.RemoveAt(this.options.MRUSize);

				settings.SetSettingList( "gui", "mru", "project", recentProjectFilenames );			
			}
		}

		private void FileOpen(string fileName)
		{
			bool  bFailed = true;

			try
			{
				string directoryName = Path.GetDirectoryName(fileName);
				Directory.SetCurrentDirectory(directoryName);

				try
				{
					project.Read(fileName);
				}
				catch (CouldNotLoadAllAssembliesException e)
				{
					WarningForm warningForm = new WarningForm("Could not load assembly.",
						e.Message);
					warningForm.ShowDialog(this);
				}
				catch (DocumenterPropertyFormatException e)
				{
					WarningForm warningForm = new WarningForm("Invalid Properties in Project File.",
						e.Message  + "Documenter defaults will be used....");
					warningForm.ShowDialog(this);
				}

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
				errorForm.ShowDialog(this);
			}
			catch (Exception ex)
			{
				string msg = "An error occured while trying to read in project file:\n" + fileName + ".";
				ErrorForm errorForm = new ErrorForm(msg, ex);
				errorForm.ShowDialog(this);
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
				MessageBox.Show(this, ex.InnerException.Message, "Save", 
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				FileSaveAs();
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

			saveFileDlg.Filter = "NDoc Project files (*.ndoc)|*.ndoc|All files (*.*)|*.*" ;

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
			if ((comboBoxDocumenters.SelectedIndex == -1)
				&& (comboBoxDocumenters.Items.Count > 0))
				comboBoxDocumenters.SelectedIndex = 0;

			SelectedDocumenterChanged();
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
			if (project.IsDirty)
			{
				DialogResult result = PromptToSave();
				switch (result)
				{
					case DialogResult.Yes:
						SaveOrSaveAs();
						break;
					case DialogResult.No:
						break;
					case DialogResult.Cancel:
						return;
				}
			}
			Clear();
		}

		private void menuFileOpenSolution_Click (object sender, System.EventArgs e)
		{
			if (project.IsDirty)
			{
				DialogResult result = PromptToSave();
				switch (result)
				{
					case DialogResult.Yes:
						SaveOrSaveAs();
						break;
					case DialogResult.No:
						break;
					case DialogResult.Cancel:
						return;
				}
			}
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
			if ( QueryContinueOpen() )
			{
				OpenFileDialog openFileDlg = new OpenFileDialog();
				openFileDlg.InitialDirectory = Directory.GetCurrentDirectory();
				openFileDlg.Filter = "NDoc Project files (*.ndoc)|*.ndoc|All files (*.*)|*.*" ;

				if(openFileDlg.ShowDialog() == DialogResult.OK)
				{
					FileOpen(openFileDlg.FileName);
				}
			}
		}

		private bool QueryContinueOpen()
		{
			bool continueOpen = true;

			if ( project.IsDirty )
			{
				switch ( PromptToSave() )
				{
					case DialogResult.Yes:
						SaveOrSaveAs();
						break;
					case DialogResult.No:
						break;
					case DialogResult.Cancel:
						continueOpen = false;
						break;
				}
			}

			return continueOpen;
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
				if (project.IsDirty)
				{
					DialogResult result = PromptToSave();
					switch (result)
					{
						case DialogResult.Yes:
							SaveOrSaveAs();
							break;
						case DialogResult.No:
							break;
						case DialogResult.Cancel:
							return;
					}
				}
				FileOpen(fileName);
			}
			else
			{
				try
				{
					MessageBox.Show(this, "Project file doesn't exist.", "Open",
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
				// disconnect from the documenter's events
				documenter.DocBuildingStep -= new DocBuildingEventHandler(OnStepUpdate);

				// keep us from accessing parts of the window when it is closed while a build is in progress
				if ( !this.IsDisposed )
				{
					// Just in case some weird exception happens, we don't get stuck
					// with a busy cursor.
					this.Cursor = Cursors.Default;

					ConfigureUIForBuild(false);
					statusBarTextPanel.Text = "Ready";
					if ( !this.traceWindow1.IsDisposed && this.traceWindow1.Visible )
						this.traceWindow1.Disconnect();
				}
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

			// we do not want to show any dialogs if the app is shutting down
			if ( !this.IsDisposed && innermostException is DocumenterException )
			{
				ErrorForm errorForm = new ErrorForm(msg, innermostException);
				errorForm.Text = "NDoc Documenter Error";
				errorForm.ShowDialog(this);
			}
			else if ( !this.IsDisposed )
			{
				ErrorForm errorForm = new ErrorForm(msg, innermostException);
				errorForm.ShowDialog(this);
			}
		}

		private void menuCancelBuildItem_Click(object sender, System.EventArgs e)
		{
			statusBarTextPanel.Text = "Cancelling build ...";
			buildThread.Abort();
			Trace.WriteLine( "Build cancelled" );
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
			menuViewBuildProgress.Enabled = !starting;
			menuViewOptions.Enabled = !starting;

			assembliesHeaderGroupBox.Enabled = !starting;
			documenterHeaderGroupBox.Enabled = !starting;
            
			if ( starting )
			{
				this.traceWindow1.Clear();

				if ( this.options.ShowProgressOnBuild && !this.traceWindow1.Visible )
					this.traceWindow1.Visible = true;

				if ( this.traceWindow1.Visible )
					this.traceWindow1.Connect();
			}
			
			progressBar.Visible = starting;
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
			catch (DocumenterException ex)
			{
				MessageBox.Show( this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
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
				AssemblySlashDoc assemblySlashDoc = new AssemblySlashDoc();
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
				catch(ReflectionTypeLoadException ex)
				{
					string msg = string.Format(
						"Unable to load types from {0}.\nIs {1} missing a dependency?", 
						form.AssemblyFilename, Path.GetFileName(form.AssemblyFilename));
					ErrorForm errorForm = new ErrorForm(msg, ex);
					errorForm.ShowDialog(this);
				}
				catch(BadImageFormatException ex)
				{
					string msg = string.Format(
						"Unable to load the file {0}.\n{1} doesn't appear to be a managed assembly.", 
						form.AssemblyFilename, Path.GetFileName(form.AssemblyFilename));
					ErrorForm errorForm = new ErrorForm(msg, ex);
					errorForm.ShowDialog(this);
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
			foreach(ListViewItem listViewItem in assembliesListView.SelectedItems)
			{
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
			SelectedDocumenterChanged();
		}

		private void SelectedDocumenterChanged()
		{
			if (propertyGrid.SelectedObject != null)
			{
				((IDocumenterConfig)propertyGrid.SelectedObject).SetProject(null);
			}

			if (comboBoxDocumenters.SelectedIndex != -1)
			{
				IDocumenterConfig documenterConfig = ((IDocumenter)project.Documenters[comboBoxDocumenters.SelectedIndex]).Config;
				documenterConfig.SetProject(project);
				propertyGrid.SelectedObject = documenterConfig;
			}
		}

		private DialogResult PromptToSave()
		{
			return MessageBox.Show( this,
				"Save changes to project " + projectFilename + "?",
				"Save?",
				MessageBoxButtons.YesNoCancel,
				MessageBoxIcon.Exclamation,
				MessageBoxDefaultButton.Button1);
		}

		/// <summary>Prompts the user to save the project if it's dirty.</summary>
		protected override void OnClosing(CancelEventArgs e)
		{
			if ( buildThread != null && buildThread.IsAlive )
				buildThread.Abort();

			WriteConfig();

			if (project.IsDirty)
			{
				DialogResult result = PromptToSave();
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
			base.OnClosing(e);
		}

		private void propertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
		{
			propertyGrid.Refresh();
		}

		#endregion // Event Handlers

        /// <summary>
        /// Opens the license file in its associates application.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs" /> that contains the event data.</param>
        private void menuViewLicense_Click(object sender, System.EventArgs e) {
            Uri uri = new Uri(Assembly.GetExecutingAssembly().CodeBase, true);
            // first try to locate license file in directory in which NDocGui is
            // located
            string path = Path.Combine(Path.GetDirectoryName(uri.AbsolutePath), "gpl.rtf");
            if (!File.Exists(path)) {
                // if not found, try to look in NDoc main directory, which is 3 
                // levels up (from <ndoc root>/bin/<framework>/<framework version> 
                // to <ndoc root>)
                path = Path.Combine(
                    Path.GetDirectoryName(uri.AbsolutePath), 
                    string.Format(CultureInfo.InvariantCulture, "..{0}..{0}..{0}gpl.rtf", 
                    Path.DirectorySeparatorChar));
                if (!File.Exists(path)) {
                    MessageBox.Show(this, "Could not find the license file.", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // skip further processing
                    return;
                }
            }

            // license file exists, so open it in associated application
            Process.Start(path);
		}

		private void traceWindow1_VisibleChanged(object sender, System.EventArgs e)
		{
			splitter1.Visible = traceWindow1.Visible;

			// make sure the splitter splits the trace window, not some other docked control
			splitter1.Top = traceWindow1.Top - splitter1.Height;
			menuViewBuildProgress.Checked = traceWindow1.Visible;
			
			// disconnect from trace events when the trace window is being hidden
			if ( !traceWindow1.Visible )
				this.traceWindow1.Disconnect();
		}

		private void menuViewBuildProgress_Click(object sender, System.EventArgs e)
		{
			traceWindow1.Visible = !traceWindow1.Visible;
		}

		private void menuViewOptions_Click(object sender, System.EventArgs e)
		{
			using( OptionsForm optionsForm = new OptionsForm( (NDocOptions)this.options.Clone() ) )
			{
				if ( optionsForm.ShowDialog() == DialogResult.OK )
				{
					this.options = optionsForm.Options;

					// save the user settings
					using( Settings settings = new Settings( Settings.UserSettingsFile ) )
					{
						settings.SetSetting( "gui", "loadLastProjectOnStart", this.options.LoadLastProjectOnStart );
						settings.SetSetting( "gui", "showProgressOnBuild", this.options.ShowProgressOnBuild );
						settings.SetSetting( "gui", "mruSize", this.options.MRUSize );
					}

					// save machine settings
					using( Settings settings = new Settings( Settings.MachineSettingsFile ) )
					{
						settings.SetSetting( "compilers", "htmlHelpWorkshopLocation", this.options.HtmlHelpWorkshopLocation );
					}
				}
			}
		}

		private void menuViewStatusBar_Click(object sender, System.EventArgs e)
		{
			this.statusBar.Visible = !this.statusBar.Visible;
		}

		private void statusBar_VisibleChanged(object sender, System.EventArgs e)
		{
			this.menuViewStatusBar.Checked = this.statusBar.Visible;		
		}

		private void assembliesListView_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
			foreach ( AssemblySlashDoc assemblySlashDoc in DragDropHandler.GetAssemblySlashDocs( files ) )
			{			
				this.Cursor = Cursors.WaitCursor;
				try
				{
					project.AddAssemblySlashDoc(assemblySlashDoc);
					AddRowToListView(assemblySlashDoc);
					EnableMenuItems(true);
				}
				catch(Project.AssemblyAlreadyExistsException)
				{
					//ignore this exception
				}
				catch(ReflectionTypeLoadException ex)
				{
					string msg = string.Format(
						"Unable to load types from {0}.\nIs {1} missing a dependency?", 
						assemblySlashDoc.AssemblyFilename, Path.GetFileName(assemblySlashDoc.AssemblyFilename));
					ErrorForm errorForm = new ErrorForm(msg, ex);
					errorForm.ShowDialog(this);
				}
				finally
				{
					this.Cursor = Cursors.Default;
				}
			}
		}

		private void assembliesListView_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if( e.Data.GetDataPresent(DataFormats.FileDrop) && DragDropHandler.CanDrop( (string[])e.Data.GetData( DataFormats.FileDrop ) ) == DropFileType.Assembly )
			{
				e.Effect = DragDropEffects.Link;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}		
		}

		private void menuNDocOnline_Click(object sender, System.EventArgs e)
		{
			Process.Start( "http://ndoc.sourceforge.net" );
		}


		private static string HelpFilePath
		{
			get
			{
                Uri uri = new Uri(Assembly.GetExecutingAssembly().CodeBase, true);
                // first try to locate help file in directory in which NDocGui is
                // located
                string path = Path.Combine( 
                    Path.GetDirectoryName(uri.AbsolutePath),
                    "NDocUsersGuide.chm");
                if (!File.Exists(path)) {
                    // if not found, try to look in NDoc main directory, which is 3 
                    // levels up (from <ndoc root>/bin/<framework>/<framework version> 
                    // to <ndoc root>/doc/help)
                    path = Path.Combine(Path.GetDirectoryName(uri.AbsolutePath), 
                        string.Format(CultureInfo.InvariantCulture, "..{0}..{0}..{0}doc{0}help{0}NDocUsersGuide.chm", 
                        Path.DirectorySeparatorChar));
                }
                return path;
			}
		}

		private void menuHelpContents_Click(object sender, System.EventArgs e)
		{
			Help.ShowHelp( this, HelpFilePath );
		}

		private void menuHelpIndex_Click(object sender, System.EventArgs e)
		{
			Help.ShowHelpIndex( this, HelpFilePath );
		}

		private void menuViewDescriptions_Click(object sender, System.EventArgs e)
		{
			ShowDescriptions = !ShowDescriptions;
		}

		/// <summary>
		/// Handles drag enter and raises the DragEnter event
		/// </summary>
		/// <param name="drgevent">Drag arguments</param>
		protected override void OnDragEnter(DragEventArgs drgevent)
		{
			if( drgevent.Data.GetDataPresent( DataFormats.FileDrop ) && DragDropHandler.CanDrop( (string[])drgevent.Data.GetData( DataFormats.FileDrop ) ) == DropFileType.Project )
			{
				drgevent.Effect = DragDropEffects.Link;
			}
			else
			{
				drgevent.Effect = DragDropEffects.None;
			}		
			base.OnDragEnter (drgevent);
		}

		/// <summary>
		/// Handles drag drop and raises the DragDrop event
		/// </summary>
		/// <param name="drgevent">Drag arguments</param>
		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			// ask the user if they want to save if the current project if dirty
			if( QueryContinueOpen() )
			{
				string[] files = (string[])drgevent.Data.GetData( DataFormats.FileDrop );
				FileOpen( DragDropHandler.GetProjectFilePath( files ) );
			}
			base.OnDragDrop (drgevent);
		}

		private bool ShowDescriptions
		{
			get
			{
				return this.propertyGrid.HelpVisible;
			}
			set
			{
				this.propertyGrid.HelpVisible = value;
				menuViewDescriptions.Checked = value;
			}
		}
	}
}
