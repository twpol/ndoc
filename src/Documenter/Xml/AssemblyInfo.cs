using System.Reflection;

[assembly: AssemblyTitle("NDoc XML Documenter")]
[assembly: AssemblyDescription("XML documenter for the NDoc code documentation generator.")]

#if !DEBUG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile(@"..\..\..\..\ndoc.snk")]
[assembly: AssemblyKeyName("")]
#endif
