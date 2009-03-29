using NDoc3.ReflectionTests.DuplicateNamespace;

/// <summary>
/// GlobalAssembly2Class pointing to <see cref="DuplicateClass"/> in TestAssembly2
/// </summary>
public class GlobalAssembly2Class
{
#pragma warning disable 169
	private GlobalAssembly1Class _just2ForceAnAssemblyReference;
#pragma warning restore 169
}
