using System;
using System.Reflection;

[assembly: CLSCompliant(true)]

// Common
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("ndoc.sourceforge.net")]
[assembly: AssemblyProduct("NDoc")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]			

[assembly: AssemblyTitle("NDoc Test")]
[assembly: AssemblyDescription("Test class library for the NDoc code documentation generator.")]

// When kept constant, results of tests can be compared (windiffed) against older output.
[assembly: AssemblyVersion("1.2.1285.524")]

#if !DEBUG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile(@"..\..\..\ndoc.snk")]
[assembly: AssemblyKeyName("")]
#endif
