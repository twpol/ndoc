// Documenter.cs - base XML documenter code
// Copyright (C) 2001  Kral Ferch, Jason Diamond
// Parts Copyright (C) 2004  Kevin Downs
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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.ComponentModel;

namespace NDoc.Core
{
	/// <summary>The base documenter class.</summary>
	/// <remarks>
	/// This is a base class for NDoc Documenters.  
	/// It implements all the methods required by the <see cref="IDocumenter"/> interface. 
	/// It also provides some basic properties which are shared by all documenters. 
	/// </remarks>
	abstract public class BaseDocumenter : IDocumenter, IComparable
	{
		IDocumenterConfig		config;

		private BaseDocumenterConfig MyConfig
		{
			get
		{
				return (BaseDocumenterConfig)Config;
			}
		}

		/// <summary>Initialized a new BaseDocumenter instance.</summary>
		protected BaseDocumenter(string name)
		{
			_Name = name;
		}

		/// <summary>
		/// The development status (alpha, beta, stable) of this documenter.
		/// Documenters should override this if they aren't stable.
		/// </summary>
		public virtual DocumenterDevelopmentStatus DevelopmentStatus
		{
			get { return(DocumenterDevelopmentStatus.Stable); }
		}

		/// <summary>Compares the currrent document to another documenter.</summary>
		public int CompareTo(object obj)
		{
			return String.Compare(Name, ((IDocumenter)obj).Name);
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public IDocumenterConfig Config
		{
			get { return config; }
			set { config = value; }
		}

		private string _Name;

		/// <summary>Gets the display name for the documenter.</summary>
		public string Name
		{
			get { return _Name; }
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public abstract string MainOutputFile { get; }

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public virtual void View()
		{
			if (File.Exists(this.MainOutputFile))
			{
				Process.Start(this.MainOutputFile);
			}
			else
			{
				throw new FileNotFoundException("Documentation not built.",
					this.MainOutputFile);
			}
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public event DocBuildingEventHandler DocBuildingStep;
		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public event DocBuildingEventHandler DocBuildingProgress;

		/// <summary>Raises the DocBuildingStep event.</summary>
		/// <param name="step">The overall percent complete value.</param>
		/// <param name="label">A description of the work currently beeing done.</param>
		protected void OnDocBuildingStep(int step, string label)
		{
			if (DocBuildingStep != null)
				DocBuildingStep(this, new ProgressArgs(step, label));
		}

		/// <summary>Raises the DocBuildingProgress event.</summary>
		/// <param name="progress">Percentage progress value</param>
		protected void OnDocBuildingProgress(int progress)
		{
			if (DocBuildingProgress != null)
				DocBuildingProgress(this, new ProgressArgs(progress, ""));
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		abstract public void Clear();

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public virtual string CanBuild(Project project)
		{
			return this.CanBuild(project, false);
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public virtual string CanBuild(Project project, bool checkInputOnly)
		{
			StringBuilder xfiles = new StringBuilder();
			foreach (AssemblySlashDoc asd in project.GetAssemblySlashDocs())
			{
				if (!File.Exists(Path.GetFullPath(asd.AssemblyFilename)))
				{
					xfiles.Append("\n" + asd.AssemblyFilename);
				}
				if (asd.SlashDocFilename!=null && asd.SlashDocFilename.Length>0)
				{
					if (!File.Exists(Path.GetFullPath(asd.SlashDocFilename)))
					{
						xfiles.Append("\n" + asd.SlashDocFilename);
					}
				}
			}

			if (xfiles.Length > 0)
			{
				return "One of more source files not found:\n" + xfiles.ToString();
			}
			else
			{
				return null;
			}
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		abstract public void Build(Project project);

	}
}
