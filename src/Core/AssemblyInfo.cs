using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: CLSCompliantAttribute(true)]
[assembly: AssemblyTitle("NDoc3 Documenter Core")]
[assembly: AssemblyDescription("Core components for the NDoc3 code documentation generator.")]

#if (OFFICIAL_RELEASE)
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("NDoc3.snk")]
[assembly: AssemblyKeyName("")]
#endif

// for nunit testing
[assembly: InternalsVisibleTo("NDoc3.Core.Tests")]