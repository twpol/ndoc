using System.Reflection;

[assembly: AssemblyTitle("NDoc Test")]
[assembly: AssemblyDescription("Test class library for the NDoc code documentation generator.")]

#if !DEBUG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile(@"..\..\..\ndoc.snk")]
[assembly: AssemblyKeyName("")]
#endif
