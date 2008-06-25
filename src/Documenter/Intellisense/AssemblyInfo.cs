using System;
using System.Reflection;

[assembly: CLSCompliantAttribute(false)]
[assembly: AssemblyTitle("NDoc3 Intellisense Documenter")]
[assembly: AssemblyDescription("Intellisense Documenter for the NDoc3 code documentation generator.")]

#if (OFFICIAL_RELEASE)
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("NDoc3.snk")]
[assembly: AssemblyKeyName("")]
#endif
