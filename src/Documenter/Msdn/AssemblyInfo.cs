using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: CLSCompliant(true)]

[assembly: AssemblyTitle("NDoc MSDN Documenter")]
[assembly: AssemblyDescription(
"MSDN-like documenter for the NDoc code documentation generator.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("ndoc.sourceforge.net")]
[assembly: AssemblyProduct("NDoc")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]		

[assembly: AssemblyVersion("1.2.*")]

#if !DEBUG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile(@"..\..\..\..\ndoc.snk")]
[assembly: AssemblyKeyName("")]
#endif
