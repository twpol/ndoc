using System.Reflection;

[assembly: AssemblyTitle("NDoc VS.NET Solution Parser")]
[assembly: AssemblyDescription("Visual Studio solution parser for the NDoc code documentation generator.")]

#if !DEBUG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile(@"..\..\..\ndoc.snk")]
[assembly: AssemblyKeyName("")]
#endif
