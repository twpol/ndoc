using System;
using System.Reflection;

[assembly: CLSCompliantAttribute(true)]
[assembly: AssemblyTitle("NDoc3 VS.NET Solution Parser")]
[assembly: AssemblyDescription("Visual Studio solutionparser for the NDoc3 code documentation generator.")]

#if (OFFICIAL_RELEASE)
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("NDoc3.snk")]
[assembly: AssemblyKeyName("")]
#endif
