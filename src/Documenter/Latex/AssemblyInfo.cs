using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: CLSCompliant(true)]

[assembly: AssemblyTitle("LaTeX documenter")]
[assembly: AssemblyDescription(
"LaTeX documenter implementation for the NDoc code documentation generator.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("ndoc.sourceforge.net")]
[assembly: AssemblyProduct("NDoc")]
[assembly: AssemblyCopyright("(c) 2002 Thong NGuyen")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("0.1.*")]

#if !DEBUG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile(@"..\..\..\..\ndoc.snk")]
[assembly: AssemblyKeyName("")] 
#endif
