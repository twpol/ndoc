using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: CLSCompliantAttribute(true)]
[assembly: AssemblyTitle("NDoc3 Console")]
[assembly: AssemblyDescription("Command-line NDoc code documentation generator.")]
[assembly: AssemblyVersion("3.0.0")]
[assembly: NeutralResourcesLanguageAttribute("")]

#if (OFFICIAL_RELEASE)
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("NDoc3.snk")]
[assembly: AssemblyKeyName("")]
#endif
