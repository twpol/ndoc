using System;
using System.Collections;

namespace NDoc.Core
{
	/// <summary>
	/// Summary description for ReflectionEngineParameters.
	/// </summary>
	[Serializable]
	public class ReflectionEngineParameters
	{
		/// <summary>
		/// constructor for ReflectionEngineParameters.
		/// </summary>
		public ReflectionEngineParameters(Project project)
		{
			CopyProject(project);
		}

		/// <summary>
		/// constructor for ReflectionEngineParameters.
		/// </summary>
		public ReflectionEngineParameters(Project project, BaseReflectionDocumenterConfig config)
		{
			CopyProject(project);
			CopyConfig(config);
		}

		private void CopyProject(Project project)
		{
			AssemblyFileNames = new ArrayList();
			XmlDocFileNames = new ArrayList();
			foreach(AssemblySlashDoc assemblySlashDoc in project.GetAssemblySlashDocs())
			{
				if (assemblySlashDoc.AssemblyFilename!=null && assemblySlashDoc.AssemblyFilename.Length>0)
				{
					string assemblyFileName = project.GetFullPath(assemblySlashDoc.AssemblyFilename);
					AssemblyFileNames.Add(assemblyFileName);
				}
				if (assemblySlashDoc.SlashDocFilename!=null && assemblySlashDoc.SlashDocFilename.Length>0)
				{
					string xmlDocFileName = project.GetFullPath(assemblySlashDoc.SlashDocFilename);
					XmlDocFileNames.Add(xmlDocFileName);
				}
			}
			ReferencePaths  = new ArrayList();
			foreach(string dir in project.GetReferencePaths())
			{
				ReferencePaths.Add(dir);
			}

			if (project.Namespaces==null)
			{
				NamespaceSummaries  = new SortedList();
			}
			else
			{
				NamespaceSummaries = project.Namespaces;
			}
		}

		private void CopyConfig(BaseReflectionDocumenterConfig config)
		{
			#region Documentation Control
			this.IncludeAssemblyVersion=config.IncludeAssemblyVersion;
			this.UseNamespaceDocSummaries=config.UseNamespaceDocSummaries;
			this.AutoPropertyBackerSummaries=config.AutoPropertyBackerSummaries;
			this.AutoDocumentConstructors=config.AutoDocumentConstructors;
			this.SdkDocLanguage=config.SdkDocLanguage;
			#endregion

			#region missing 
			this.ShowMissingSummaries=config.ShowMissingSummaries;
			this.ShowMissingRemarks=config.ShowMissingRemarks;
			this.ShowMissingParams=config.ShowMissingParams;
			this.ShowMissingReturns=config.ShowMissingReturns;
			this.ShowMissingValues=config.ShowMissingValues;
			#endregion

			#region visibility
			this.DocumentInheritedMembers=config.DocumentInheritedMembers;
			this.DocumentInheritedFrameworkMembers=config.DocumentInheritedFrameworkMembers;
			this.DocumentExplicitInterfaceImplementations=config.DocumentExplicitInterfaceImplementations;
			this.DocumentInternals=config.DocumentInternals;
			this.DocumentProtected=config.DocumentProtected;
			this.DocumentSealedProtected=config.DocumentSealedProtected;
			this.DocumentPrivates=config.DocumentPrivates;
			this.DocumentProtectedInternalAsProtected=config.DocumentProtectedInternalAsProtected;
			this.DocumentEmptyNamespaces=config.DocumentEmptyNamespaces;
			this.SkipNamespacesWithoutSummaries=config.SkipNamespacesWithoutSummaries;
			this.EditorBrowsableFilter=config.EditorBrowsableFilter;
			#endregion

			#region Attributes
			this.DocumentAttributes=config.DocumentAttributes;
			this.DocumentInheritedAttributes=config.DocumentInheritedAttributes;
			this.ShowTypeIdInAttributes=config.ShowTypeIdInAttributes;
			this.DocumentedAttributes=config.DocumentedAttributes;
			#endregion

			#region additional info
			this.CopyrightText=config.CopyrightText;
			this.CopyrightHref=config.CopyrightHref;
			this.FeedbackEmailAddress=config.FeedbackEmailAddress;
			this.Preliminary=config.Preliminary;
			#endregion

			#region threadsafety
			this.IncludeDefaultThreadSafety=config.IncludeDefaultThreadSafety;
			this.StaticMembersDefaultToSafe=config.StaticMembersDefaultToSafe;
			this.InstanceMembersDefaultToSafe=config.InstanceMembersDefaultToSafe;
			#endregion

			#region platform/framework
			this.InheritPlatformSupport=config.InheritPlatformSupport;
			this.DefaultOSSupport=config.DefaultOSSupport;
			this.SupportCompactFrameworkByDefault=config.SupportCompactFrameworkByDefault;
			this.SupportMONOFrameworkByDefault=config.SupportMONOFrameworkByDefault;
			this.AdditionalFrameworkList=config.AdditionalFrameworkList;
			this.AdditionalOSList=config.AdditionalOSList;
			#endregion
		}


		#region Project Data
		/// <summary>
		/// 
		/// </summary>
		public ArrayList AssemblyFileNames;
		/// <summary>
		/// 
		/// </summary>
		public ArrayList XmlDocFileNames;
		/// <summary>
		/// 
		/// </summary>
		public ArrayList ReferencePaths;
		/// <summary>
		/// 
		/// </summary>
		public SortedList NamespaceSummaries;
		#endregion

		#region documentation control
		/// <summary>
		/// 
		/// </summary>
		public bool IncludeAssemblyVersion ;
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
		public SdkLanguage SdkDocLanguage ;
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
		public DocumentedInheritedMembers DocumentInheritedMembers ;
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
		public string DocumentedAttributes ;
		#endregion

		// the following are not esential to reflection
		// 

		#region Additional Info
		/// <summary>
		/// 
		/// </summary>
		public string CopyrightText ;
		/// <summary>
		/// 
		/// </summary>
		public string CopyrightHref ;
		/// <summary>
		/// 
		/// </summary>
		public string FeedbackEmailAddress ;
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

		#region platform/framework
		/// <summary>
		/// 
		/// </summary>
		public bool InheritPlatformSupport ;
		/// <summary>
		/// 
		/// </summary>
		public OSSupport DefaultOSSupport ;
		/// <summary>
		/// 
		/// </summary>
		public bool SupportCompactFrameworkByDefault ;
		/// <summary>
		/// 
		/// </summary>
		public bool SupportMONOFrameworkByDefault ;
		/// <summary>
		/// 
		/// </summary>
		public string AdditionalFrameworkList ;
		/// <summary>
		/// 
		/// </summary>
		public string AdditionalOSList ;

		#endregion



	}
}
