using System.Reflection;

[assembly: AssemblyTitle("NDoc Console")]
[assembly: AssemblyDescription("Command-line NDoc code documentation generator.")]

#if !DEBUG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile(@"ndoc.snk")]
[assembly: AssemblyKeyName("")]
#endif
