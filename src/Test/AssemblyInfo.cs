using System;
using System.Reflection;

[assembly: CLSCompliantAttribute(false)]
[assembly: AssemblyTitle("NDoc Test")]
[assembly: AssemblyDescription("Test class library for the NDoc code documentation generator.")]

#if (!DEBUG)
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("NDoc.snk")]
[assembly: AssemblyKeyName("")]
#endif
