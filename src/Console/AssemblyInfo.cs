using System;
using System.Reflection;

[assembly: CLSCompliantAttribute(true)]
[assembly: AssemblyTitle("NDoc3 Console")]
[assembly: AssemblyDescription("Command-line NDoc code documentation generator.")]

#if (OFFICIAL_RELEASE)
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("NDoc3.snk")]
[assembly: AssemblyKeyName("")]
#endif
