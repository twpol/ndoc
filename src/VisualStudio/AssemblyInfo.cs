using System.Reflection;

[assembly: AssemblyTitle("NDoc VS.NET Solution Parser")]
[assembly: AssemblyDescription("Visual Studio solutionparser for the NDoc code documentation generator.")]

#if (!DEBUG)
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("NDoc.snk")]
[assembly: AssemblyKeyName("")]
#endif
