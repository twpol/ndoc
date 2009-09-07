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
using System.Collections.Generic;
using System.IO;

namespace NDoc3.Core.Reflection {
	/// <summary>
	/// Summary description for NDocXmlGeneratorParameters.
	/// </summary>
	[Serializable]
	internal class NDocXmlGeneratorParameters {
		// wrapper to use as keys in the set
		[Serializable]
		private class ComparableFileInfo {
			private readonly string _filePath;

			public ComparableFileInfo(FileInfo file) {
				_filePath = PathUtilities.NormalizePath(file.FullName);
			}

			public bool Equals(ComparableFileInfo other) {
				if (ReferenceEquals(null, other))
					return false;
				if (ReferenceEquals(this, other))
					return true;
				return Equals(other._filePath, _filePath);
			}

			public override bool Equals(object obj) {
				if (ReferenceEquals(null, obj))
					return false;
				if (ReferenceEquals(this, obj))
					return true;
				if (obj.GetType() != typeof(ComparableFileInfo))
					return false;
				return Equals((ComparableFileInfo)obj);
			}

			public override int GetHashCode() { return (_filePath != null ? _filePath.GetHashCode() : 0); }
			public override string ToString() { return _filePath; }
			public FileInfo FileInfo { get { return new FileInfo(_filePath); } }
		}

		#region Project Data

		// use a dictionary to easily avoid duplicates
		private readonly IDictionary<ComparableFileInfo, object> _assemblyFileNames = new Dictionary<ComparableFileInfo, object>();

		public ICollection<FileInfo> AssemblyFileNames {
			get {
				return ConvertAll(_assemblyFileNames.Keys, cfi => cfi.FileInfo);
			}
		}

		// use a dictionary to easily avoid duplicates
		private readonly Dictionary<ComparableFileInfo, object> _slashDocFileNames = new Dictionary<ComparableFileInfo, object>();

		public ICollection<FileInfo> SlashDocFileNames {
			get {
				return ConvertAll(_slashDocFileNames.Keys, cfi => cfi.FileInfo);
			}
		}

		private static ICollection<TReturn> ConvertAll<TReturn, TInput>(ICollection<TInput> input, Converter<TInput, TReturn> converter) {
			List<TReturn> result = new List<TReturn>();
			foreach (TInput element in input) {
				result.Add(converter(element));
			}
			return result;
		}

		//		/// <summary>
		//		/// 
		//		/// </summary>
		//		public ReferencePathCollection ReferencePaths;
		/// <summary>
		/// 
		/// </summary>
		public readonly IDictionary<string, string> NamespaceSummaries = new SortedStringDictionary();

		/// <summary>
		/// Adds the type's assembly to the list of assemblies to document.
		/// </summary>
		public void AddAssemblyToDocument(Type assemblyType) {
			string testAssembly1AssemblyFileName = new Uri(assemblyType.Assembly.CodeBase).AbsolutePath;
			AddAssemblyToDocument(new FileInfo(testAssembly1AssemblyFileName));
		}

		/// <summary>
		/// Adds the assembly to the list of assemblies to document.
		/// Adds the assembly's slashdoc file as well using the convention myassembly.dll + myassembly.xml
		/// </summary>
		public void AddAssemblyToDocument(FileInfo assemblyFile) {
			string ext = assemblyFile.Extension;
			_assemblyFileNames[new ComparableFileInfo(assemblyFile)] = assemblyFile;
			FileInfo slashDocFile = new FileInfo(assemblyFile.FullName.Substring(0, assemblyFile.FullName.Length - ext.Length) + ".xml");
			if(File.Exists(slashDocFile.FullName))
				AddAssemblySlashDoc(slashDocFile);
		}

		public void AddAssemblySlashDoc(FileInfo slashDocPath) {
				_slashDocFileNames[new ComparableFileInfo(slashDocPath)] = slashDocPath;
		}
		#endregion

		#region documentation control
		/// <summary>
		/// 
		/// </summary>
		public AssemblyVersionInformationType AssemblyVersionInfo;
		/// <summary>
		/// 
		/// </summary>
		public bool UseNamespaceDocSummaries;
		/// <summary>
		/// 
		/// </summary>
		public bool AutoPropertyBackerSummaries;
		/// <summary>
		/// 
		/// </summary>
		public bool AutoDocumentConstructors;
		/// <summary>
		/// 
		/// </summary>
		public SdkLanguage SdkDocLanguage;
		#endregion

		#region missing
		/// <summary>
		/// 
		/// </summary>
		public bool ShowMissingSummaries;
		/// <summary>
		/// 
		/// </summary>
		public bool ShowMissingRemarks;
		/// <summary>
		/// 
		/// </summary>
		public bool ShowMissingParams;
		/// <summary>
		/// 
		/// </summary>
		public bool ShowMissingReturns;
		/// <summary>
		/// 
		/// </summary>
		public bool ShowMissingValues;
		#endregion

		#region visibility
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentInheritedMembers;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentInheritedFrameworkMembers;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentExplicitInterfaceImplementations;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentInternals;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentProtected;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentSealedProtected;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentPrivates;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentProtectedInternalAsProtected;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentEmptyNamespaces;
		/// <summary>
		/// 
		/// </summary>
		public bool SkipNamespacesWithoutSummaries;
		/// <summary>
		/// 
		/// </summary>
		public EditorBrowsableFilterLevel EditorBrowsableFilter;
		#endregion

		#region Attributes
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentAttributes;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentInheritedAttributes;
		/// <summary>
		/// 
		/// </summary>
		public bool ShowTypeIdInAttributes;
		/// <summary>
		/// 
		/// </summary>
		public string DocumentedAttributes = string.Empty;
		#endregion

		// the following are not esential to reflection
		// 

		#region Additional Info
		/// <summary>
		/// 
		/// </summary>
		public string CopyrightText = string.Empty;
		/// <summary>
		/// 
		/// </summary>
		public string CopyrightHref = string.Empty;
		/// <summary>
		/// 
		/// </summary>
		public string FeedbackEmailAddress = string.Empty;
		/// <summary>
		/// 
		/// </summary>
		public bool Preliminary;
		#endregion

		#region threadsafety
		/// <summary>
		/// 
		/// </summary>
		public bool IncludeDefaultThreadSafety;
		/// <summary>
		/// 
		/// </summary>
		public bool StaticMembersDefaultToSafe;
		/// <summary>
		/// 
		/// </summary>
		public bool InstanceMembersDefaultToSafe;

		#endregion
	}
}
