// Copyright (C) 2004  Kevin Downs
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
using System.Windows.Forms;

namespace NDoc.Gui
{
	/// <summary>
	/// A PropertyGrid with context menu to <b>Reset</b> values and show/hide the <b>Description</b> pane.
	/// </summary>
	public class RuntimePropertyGrid : PropertyGrid
	{
		private ContextMenu contextMenu;
		private MenuItem menuReset;
		private MenuItem menuDisplay;

		/// <summary>
		/// Creates a new instance of the RuntimePropertyGrid class
		/// </summary>
		public RuntimePropertyGrid() : base()
		{
			this.contextMenu = new ContextMenu();
			this.menuReset = new MenuItem();
			this.menuDisplay = new MenuItem();

			this.ContextMenu = this.contextMenu;
			this.contextMenu.MenuItems.AddRange(new MenuItem[] { this.menuReset, this.menuDisplay});
			this.contextMenu.Popup += new System.EventHandler(this.contextMenu_Popup);

			this.menuReset.Index = 0;
			this.menuReset.Text = "Reset";
			this.menuReset.Click += new System.EventHandler(this.menuReset_Click);

			this.menuDisplay.Index = 1;
			this.menuDisplay.Text = "Description";
			this.menuDisplay.Click += new System.EventHandler(this.menuDisplay_Click);
		}

		private void menuReset_Click(object sender, System.EventArgs e)
		{
			GridItem item = this.SelectedGridItem;
			item.PropertyDescriptor.ResetValue(this.SelectedObject);
			this.Refresh();
		}

		private void menuDisplay_Click(object sender, System.EventArgs e)
		{
			this.HelpVisible = !this.HelpVisible;
			menuDisplay.Checked = this.HelpVisible;
		}

		private void contextMenu_Popup(object sender, System.EventArgs e)
		{
			GridItem item = this.SelectedGridItem;
			menuReset.Enabled = item.PropertyDescriptor.CanResetValue(this.SelectedObject);
			menuDisplay.Checked = this.HelpVisible;
		}

	}
}
