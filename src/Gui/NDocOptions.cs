using System;
using System.ComponentModel;

namespace NDoc.Gui
{
	public enum Documenters
	{
		VSNET,
		MSDN,
		Latex,
		XML,
		LinearHTML,
		JavaDoc
	}

	/// <summary>
	/// Configurable application settings
	/// </summary>
	public class NDocOptions
	{
		/// <summary>
		/// Creates a new instance of the NDocOptions class
		/// </summary>
		public NDocOptions()
		{

		}

		private bool _ShowProgressOnBuild = false;

		/// <summary>
		/// Get/Set the ShowProgressOnBuild property
		/// </summary>
		[Browsable(true)]
		[DefaultValue(false)]
		[Description("If true, the build progress trace window will automatically be shown whenever a build is started.")]
		[Category("User Interface")]
		public bool ShowProgressOnBuild
		{
			get{ return _ShowProgressOnBuild; }
			set{ _ShowProgressOnBuild = value; }
		}


		private bool _LoadLastProjectOnStart = true;

		/// <summary>
		/// Get/Set the LoadLastProjectOnStart property
		/// </summary>
		[Browsable(true)]
		[DefaultValue(true)]
		[Description("If true, NDoc will open the last loaded project when it starts.")]
		[Category("User Interface")]
		public bool LoadLastProjectOnStart
		{
			get{ return _LoadLastProjectOnStart; }
			set{ _LoadLastProjectOnStart = value; }
		}


		private Documenters _DefaultDocumenter = Documenters.MSDN;

		/// <summary>
		/// Get/Set the LoadLastProjectOnStart property
		/// </summary>
		[Browsable(true)]
		[DefaultValue(Documenters.MSDN)]
		[Description("The default documenter to use when a specific documenter is not specified.")]
		[Category("Console Application")]
		public Documenters DefaultDocumenter
		{
			get{ return _DefaultDocumenter; }
			set{ _DefaultDocumenter = value; }
		}

		/// <summary>
		/// Create a clone of this object
		/// </summary>
		/// <returns>The clone</returns>
		public NDocOptions Clone()
		{
			return (NDocOptions)base.MemberwiseClone();
		}
	}
}
