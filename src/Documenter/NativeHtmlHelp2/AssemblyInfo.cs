using System.Reflection;

[assembly: AssemblyTitle("NDoc Native HTML Help 2 Documenter")]
[assembly: AssemblyDescription("Native HTML Help 2.0 documenter for the NDoc code documentation generator.")]

#if (!DEBUG)
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("NDoc.snk")]
[assembly: AssemblyKeyName("")]
#endif
