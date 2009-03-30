namespace NDoc3.Documenter.Msdn
{
	internal class BuildAssemblyContext : BuildProjectContext
	{
		// TODO (EE): set assemblyname during generating html
#pragma warning disable 649
		public readonly string CurrentAssemblyName;
#pragma warning restore 649

		public BuildAssemblyContext(BuildProjectContext other, string assemblyName)
			: base(other)
		{
			this.CurrentAssemblyName = assemblyName;
		}

		public BuildAssemblyContext(BuildAssemblyContext other)
			: base(other)
		{
			this.CurrentAssemblyName = other.CurrentAssemblyName;
		}
	}
}