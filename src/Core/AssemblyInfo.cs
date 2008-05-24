using System;
using System.Reflection;

[assembly: CLSCompliantAttribute(true)]
[assembly: AssemblyTitle("NDoc3 Documenter Core")]
[assembly: AssemblyDescription("Core components for the NDoc3 code documentation generator.")]

#if (OFFICIAL_RELEASE)
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("NDoc.snk")]
[assembly: AssemblyKeyName("")]
#endif
