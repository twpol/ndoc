using System;
using System.Reflection;

[assembly: CLSCompliantAttribute(false)]
[assembly: AssemblyTitle("NDoc3 Test")]
[assembly: AssemblyDescription("Test class library for the NDoc3 code documentation generator.")]
[assembly: AssemblyFileVersion("99.88.77.66")]

#if (OFFICIAL_RELEASE)
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("NDoc3.snk")]
[assembly: AssemblyKeyName("")]
#endif
