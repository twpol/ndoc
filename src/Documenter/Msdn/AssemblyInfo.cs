using System;
using System.Reflection;

[assembly: AssemblyTitle("NDoc MSDN Documenter")]
[assembly: AssemblyDescription("MSDN-like documenter for the NDoc code documentation generator.")]

[assembly: CLSCompliant(true)]

#if !DEBUG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile(@"..\..\..\..\ndoc.snk")]
[assembly: AssemblyKeyName("")]
#endif
