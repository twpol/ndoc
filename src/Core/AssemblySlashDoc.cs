// AssemblySlashDoc.cs - represents an assembly and /doc pair
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
using System.ComponentModel;

namespace NDoc.Core
{
	/// <summary>Represents an assembly and /doc pair.</summary>
	[Serializable]
	public class AssemblySlashDoc
	{
		private FilePath assembly;
		private FilePath slashDoc;

		/// <overrides>Initializes a new instance of the <see cref="AssemblySlashDoc"/> class.</overrides>
		/// <summary>Initializes a blank instance of the <see cref="AssemblySlashDoc"/> class.</summary>
		public AssemblySlashDoc()
		{
			this.assembly = new FilePath();
			this.slashDoc = new FilePath();
		}

		/// <summary>Initializes a new instance of the <see cref="AssemblySlashDoc"/> class
		/// with the specified Assembly and SlashDoc paths.</summary>
		/// <param name="assemblyFilename">An assembly filename.</param>
		/// <param name="slashDocFilename">A /doc filename.</param>
		public AssemblySlashDoc(string assemblyFilename, string slashDocFilename)
		{
			this.assembly = new FilePath(assemblyFilename);
			
			if(slashDocFilename.Length>0)
				this.slashDoc = new FilePath(slashDocFilename);
			else
				this.slashDoc = new FilePath();
		}

		/// <summary>
		/// Gets or sets the assembly.
		/// </summary>
		/// <value></value>
		[NDoc.Core.PropertyGridUI.FilenameEditor.FileDialogFilter
			 ("Select Assembly", 
			 "Library and Executable files (*.dll, *.exe)|*.dll;*.exe|Library files (*.dll)|*.dll|Executable files (*.exe)|*.exe|All files (*.*)|*.*")]
		public FilePath Assembly 
		{
			get { return assembly; }
			set { assembly = value; }
		} 
		void ResetAssembly() { assembly = new FilePath(); }

		/// <summary>
		/// Gets or sets the slash doc.
		/// </summary>
		/// <value></value>
		[NDoc.Core.PropertyGridUI.FilenameEditor.FileDialogFilter
			 ("Select Assembly", 
			 "/doc Output files (*.xml)|*.xml|All files (*.*)|*.*")]
		public FilePath SlashDoc 
		{
			get { return slashDoc; }
			set { slashDoc = value; }
		} 
		void ResetSlashDoc() { slashDoc = new FilePath(); }
	}
}
