namespace NDoc3.Documenter.Msdn
{
	internal class BuildAssemblyContext : BuildProjectContext
	{
		public readonly string CurrentAssemblyName;

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