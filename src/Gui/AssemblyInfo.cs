using System.Reflection;

[assembly: AssemblyTitle("NDoc Gui")]
[assembly: AssemblyDescription("Winform user interface for the NDoc code documentation generator.")]

#if !DEBUG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile(@"ndoc.snk")]
[assembly: AssemblyKeyName("")]
#endif
