using System;
using NLog;

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
	{
	}

    /// <summary>
    /// Formats a message
    /// </summary>
    public delegate string FormatMessageHandler(string fmt, params object[] args);

    /// <summary>
    /// Reproducing an issue, where "inherited from" shows full typename instead of classname only
    /// </summary>
    public class MyTarget : TargetWithLayout
    {
        /// <summary>
        /// Example for generic delegate type argument
        /// </summary>
        /// <param name="fmtter">a fmtter callback</param>
        public string[] DoSomethingNoOverload(System.Action<IGenericInterface<FormatMessageHandler>> fmtter)
        {
        	return null;
        }

        /// <summary>
        /// Example for generic delegate type argument
        /// </summary>
        /// <param name="fmtter">a fmtter callback</param>
				public System.Action<FormatMessageHandler>[,] DoSomethingOverloaded(System.Action<FormatMessageHandler>[][,] fmtter)
        {
        	return null;
        }

        /// <summary>
        /// Overload to make things more interesting
        /// </summary>
        /// <param name="anotherArg"></param>
        public void DoSomethingOverloaded(string anotherArg)
        {}

        /// <summary>
        /// Send the loginfo event
        /// </summary>
        /// <param name="logEvent"></param>
        protected override void Write(LogEventInfo logEvent)
        {
            throw new NotImplementedException();
        }
    }
}
