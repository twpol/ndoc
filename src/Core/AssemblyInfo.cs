using System.Reflection;

[assembly: AssemblyTitle("NDoc Documenter Core")]
[assembly: AssemblyDescription("Core components for the NDoc code documentation generator.")]

#if (!DEBUG)
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("NDoc.snk")]
[assembly: AssemblyKeyName("")]
#endif
