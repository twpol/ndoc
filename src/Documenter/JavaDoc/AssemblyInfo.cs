using System;
using System.Reflection;

[assembly: AssemblyTitle("NDoc JavaDoc Documenter")]
[assembly: AssemblyDescription("JavaDoc documenter for the NDoc code documentation generator.")]

[assembly: CLSCompliant(true)]

#if !DEBUG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile(@"ndoc.snk")]
[assembly: AssemblyKeyName("")]
#endif
