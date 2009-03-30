using System;

namespace NDoc3.Support
{
	///<summary>
	/// Utility methods for checking method arguments.
	///</summary>
	public sealed class ArgUtils
	{
		///<summary>
		/// checks the argument against <c>null</c>.
		///</summary>
		///<exception cref="ArgumentNullException">in case the given argument was <c>null</c></exception>
		public static TArg AssertNotNull<TArg>(TArg arg, string argName) where TArg:class
		{
			if (arg == null) throw new ArgumentNullException(argName);
			return arg;
		}

		///<summary>
		/// checks the argument against <c>null</c>.
		///</summary>
		///<exception cref="ArgumentNullException">in case the given argument was <c>null</c></exception>
		public static void Assert<TArg>(bool condition, string argName, string message) where TArg:class
		{
			if (!condition) throw new ArgumentException(message, argName);
		}
	}
}
