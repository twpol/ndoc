using System;
using System.Reflection;

[assembly: AssemblyTitle("LaTeX documenter")]
[assembly: AssemblyDescription("LaTeX documenter implementation for the NDoc code documentation generator.")]

[assembly: CLSCompliant(true)]

#if !DEBUG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile(@"ndoc.snk")]
[assembly: AssemblyKeyName("")] 
#endif
