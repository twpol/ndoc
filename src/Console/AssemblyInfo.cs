using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("NDoc Console")]
[assembly: AssemblyDescription(
"Command-line NDoc code documentation generator.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("ndoc.sourceforge.net")]
[assembly: AssemblyProduct("NDoc")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]		

[assembly: AssemblyVersion("1.2.*")]

#if !DEBUG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile(@"..\..\..\ndoc.snk")]
[assembly: AssemblyKeyName("")]
#endif
