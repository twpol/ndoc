namespace NDoc3.ReflectionTests.DuplicateNamespace
{
	/// <summary>
	/// summary of DuplicateClass in TestAssembly2
	/// </summary>
	internal class DuplicateClass
	{}

	/// <summary>
	/// In TestAssembly2
	/// </summary>
	public class AnotherClass : TestAssembly1Class
	{
		public static void Undocumented<X>(X x)
		{}
	}

	public struct UndocumentedStruct
	{}

	///<summary>
	/// Interface deriving from other interface
	///</summary>
	public interface AnotherInterface : IGenericInterface<object>
	{}
}
