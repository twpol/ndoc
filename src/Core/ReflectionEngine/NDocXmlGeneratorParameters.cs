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

namespace NDoc3.Core.Reflection
{
	/// <summary>
	/// Summary description for NDocXmlGeneratorParameters.
	/// </summary>
	[Serializable]
	internal class NDocXmlGeneratorParameters
	{
		#region Project Data

		/// <summary>
		/// 
		/// </summary>
		public readonly List<string> AssemblyFileNames = new List<string>();
		/// <summary>
		/// 
		/// </summary>
		public readonly List<string> XmlDocFileNames = new List<string>();
//		/// <summary>
//		/// 
//		/// </summary>
//		public ReferencePathCollection ReferencePaths;
		/// <summary>
		/// 
		/// </summary>
        public readonly IDictionary<string,string> NamespaceSummaries = new SortedStringDictionary();
		#endregion

        

		#region documentation control
		/// <summary>
		/// 
		/// </summary>
		public AssemblyVersionInformationType AssemblyVersionInfo ;
		/// <summary>
		/// 
		/// </summary>
		public bool UseNamespaceDocSummaries ;
		/// <summary>
		/// 
		/// </summary>
		public bool AutoPropertyBackerSummaries ;
		/// <summary>
		/// 
		/// </summary>
		public bool AutoDocumentConstructors ;
		/// <summary>
		/// 
		/// </summary>
		public SdkLanguage SdkDocLanguage;
		#endregion
		
		#region missing
		/// <summary>
		/// 
		/// </summary>
		public bool ShowMissingSummaries ;
		/// <summary>
		/// 
		/// </summary>
		public bool ShowMissingRemarks ;
		/// <summary>
		/// 
		/// </summary>
		public bool ShowMissingParams ;
		/// <summary>
		/// 
		/// </summary>
		public bool ShowMissingReturns ;
		/// <summary>
		/// 
		/// </summary>
		public bool ShowMissingValues ;
		#endregion

		#region visibility
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentInheritedMembers ;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentInheritedFrameworkMembers ;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentExplicitInterfaceImplementations ;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentInternals ;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentProtected ;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentSealedProtected ;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentPrivates ;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentProtectedInternalAsProtected ;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentEmptyNamespaces ;
		/// <summary>
		/// 
		/// </summary>
		public bool SkipNamespacesWithoutSummaries ;
		/// <summary>
		/// 
		/// </summary>
		public EditorBrowsableFilterLevel EditorBrowsableFilter ;
		#endregion

		#region Attributes
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentAttributes ;
		/// <summary>
		/// 
		/// </summary>
		public bool DocumentInheritedAttributes ;
		/// <summary>
		/// 
		/// </summary>
		public bool ShowTypeIdInAttributes ;
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
		public bool Preliminary ;
		#endregion

		#region threadsafety
		/// <summary>
		/// 
		/// </summary>
		public bool IncludeDefaultThreadSafety ;
		/// <summary>
		/// 
		/// </summary>
		public bool StaticMembersDefaultToSafe ;
		/// <summary>
		/// 
		/// </summary>
		public bool InstanceMembersDefaultToSafe ;

		#endregion
	}
}
