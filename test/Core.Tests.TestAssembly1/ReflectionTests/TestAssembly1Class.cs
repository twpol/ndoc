using System;

namespace NDoc3.ReflectionTests
{
	/**
	 * Some Javadoc Comment
	 */
	public interface IGenericInterface<T> where T:class
	{}

	///<summary>
	/// A generic delegate type
	///</summary>
	///<typeparam name="X">the delegate's type</typeparam>
	public delegate void GenericDelegate<X>();

	/// <summary>
	/// Boy..
	/// </summary>
	/// <typeparam name="X">The generic arg doc</typeparam>
	public struct GenericStruct<X> : IGenericInterface<X> where X:TestAssembly1Class
	{
		///<summary>
		/// A nested delegate type
		///</summary>
		///<typeparam name="X">the delegate's type</typeparam>
		public delegate void NestedDelegate<X>();

		///<summary>
		/// A nested type
		///</summary>
		public class NestedType
		{}
	}

	/// <summary>
	/// TestAssembly1Class in TestAssembly1
	/// </summary>
	public class TestAssembly1Class
	{
		/// <summary>
		/// A Multi-dim array
		/// </summary>
		public string[,] MultiArray;
		/// <summary>
		/// An array of arrays
		/// </summary>
		public string[][] ArrayOfArray;
		/// <summary>
		/// A single dim array
		/// </summary>
		public string[] SingleArray;

		/// <summary>
		/// The documentation
		/// </summary>
		/// <typeparam name="T">some type</typeparam>
		/// <param name="x">some arg</param>
		/// <returns>T's default</returns>
		public T ReturnsGenericType<T>(T x)
		{
			return x;
		}

		/// <summary>
		/// The documentation
		/// </summary>
		/// <typeparam name="T">some type</typeparam>
		/// <param name="x">some arg</param>
		/// <returns>T's default</returns>
		public string TakesGenericArg<T>(T x)
		{
			return ""+x;
		}

		/// <summary>
		/// The documentation
		/// </summary>
		/// <typeparam name="T">some type</typeparam>
		/// <param name="x">some arg</param>
		/// <returns>T's default</returns>
		public string OverloadedMethod<T>(T x)
		{
			return ""+x;
		}

		/// <summary>
		/// The documentation
		/// </summary>
		/// <typeparam name="T">some type</typeparam>
		/// <param name="x">some arg</param>
		/// <param name="y"></param>
		/// <returns>T's default</returns>
		public string OverloadedMethod<T>(T x, Predicate<T> y)
		{
			return ""+x;
		}
	}

	/// <summary>
	/// Some generic class
	/// </summary>
	/// <typeparam name="S"></typeparam>
	/// <typeparam name="T"></typeparam>
	public class TestGenericClass<S, T> : TestAssembly1Class, IGenericInterface<S> where S:class
	{
		/**
		 * Some invalid comment
		 */
		public Predicate<T>[][] SomePredicate;

		/// <summary>
		/// Returns Generic Class Type
		/// </summary>
		/// <param name="s"></param>
		/// <param name="t">some arg</param>
		/// <param name="u">some other generic arg</param>
		/// <returns>default of T</returns>
		public T ReturnsGenericClassType<U>(S s, TestGenericClass<U,T> t, ref U u) where U:class
		{
			return default(T); 
		}

		/// <summary>
		/// Returns Generic Class Type
		/// </summary>
		/// <param name="s"></param>
		/// <returns>default of T</returns>
		public T ReturnsGenericClassType(S s) {
			return default(T); 
		}
	}
}
