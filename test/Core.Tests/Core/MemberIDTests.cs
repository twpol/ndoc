#region License

/*
 * Copyright 2002-2009 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Reflection;
using System.Threading;
using NDoc3.Core.Reflection;
using NUnit.Framework;

namespace NDoc3.Core
{
	/// <summary>
	/// </summary>
	/// <author>Erich Eichinger</author>
	[TestFixture]
	public class MemberIDTests
	{
		private MethodInfo GetMethod<T1>( Func<T1> func )
		{
			return func.Method;
		}

		[Test]
		public void MemberDisplayNameTests()
		{
			Type genericTypeDefinition = typeof(GenericClass<,>);
			Type[] genericTypeDefinitionArgs = genericTypeDefinition.GetGenericArguments();

			Type genericType = typeof(GenericClass<object,WaitCallback>);
			Type[] genericTypeArgs = genericType.GetGenericArguments();

			string displayName2 = MemberDisplayName.GetMemberDisplayName(genericTypeDefinition);
			Assert.AreEqual("GenericClass(S,T)", displayName2);

			// Note: Nested partial types are not allowed - we don't need to care about them
//			Type t = typeof(System.Action<System.Action<,>>);

			Type t;
			string displayName;

			t = typeof(System.Action<System.Action<WaitCallback>>);
			displayName = MemberDisplayName.GetMemberDisplayName(t);
			Assert.AreEqual("Action`1", t.Name);
			Assert.AreEqual("Action(Action(WaitCallback))", displayName);

			t = typeof(System.Action<System.Action<WaitCallback>>).MakeByRefType();
			displayName = MemberDisplayName.GetMemberDisplayName(t);
			Assert.AreEqual("Action`1&", t.Name);
			Assert.AreEqual("Action(Action(WaitCallback))", displayName);

			t = typeof(System.Action<System.Action<WaitCallback>>[]).MakeByRefType();
			displayName = MemberDisplayName.GetMemberDisplayName(t);
			Assert.AreEqual("Action`1[]&", t.Name);
			Assert.AreEqual("Action(Action(WaitCallback))[]", displayName);

			t = typeof(System.Action<System.Action<WaitCallback>>[]);
			displayName = MemberDisplayName.GetMemberDisplayName(t);
			Assert.AreEqual("Action`1[]", t.Name);
			Assert.AreEqual("Action(Action(WaitCallback))[]", displayName);

			t = typeof(System.Action<System.Action<WaitCallback>>[,]);
			displayName = MemberDisplayName.GetMemberDisplayName(t);
			Assert.AreEqual("Action`1[,]", t.Name);
			Assert.AreEqual("Action(Action(WaitCallback))[,]", displayName);

			t = typeof(System.Action<System.Action<WaitCallback>>[][]);
			displayName = MemberDisplayName.GetMemberDisplayName(t);
			Assert.AreEqual("Action`1[][]", t.Name);
			Assert.AreEqual("Action(Action(WaitCallback))[][]", displayName);
		}

		[Test]
		public void GenericClassMethodWithGenericMethodArgs()
		{
			Type declaringType = typeof(GenericClass<,>);
			MethodInfo method = declaringType.GetMethod("ReturnsGenericClassType");

//			MethodInfo normalMethod = typeof (NormalClass).GetMethod("NormalMethod");
//			ParameterInfo[] normalPars = normalMethod.GetParameters();
//
//			Assert.IsTrue( method.IsGenericMethod );
//			Assert.IsTrue( method.IsGenericMethodDefinition );
//
//			ParameterInfo[] pars = method.GetParameters();

			Assert.AreEqual("M:NDoc3.Core.GenericClass`2.ReturnsGenericClassType``2(`0,`1,``1,``0)", MemberID.GetMemberID(method, false));
		}

		[Test]
		public void GenericClassMethodWithComplexGenericMethodArgs()
		{
			Type declaringType = typeof(GenericClass<,>);
			MethodInfo method = declaringType.GetMethod("ReturnsGenericClassTypeWithComplexGenericArg");

			Assert.AreEqual("M:NDoc3.Core.GenericClass`2.ReturnsGenericClassTypeWithComplexGenericArg``1(`0@,`1[][],NDoc3.Core.GenericClass{``0,NDoc3.Core.GenericClass{`1,`0}},``0[0:,0:]@)", MemberID.GetMemberID(method, false));
		}

		[Test]
		public void GenericClassMethodWithNestedTypeArg()
		{
			Type declaringType = typeof(NormalClass);
			MethodInfo method = declaringType.GetMethod("TakesNestedTypeArg");

			string memberId = MemberID.GetMemberID(method, false);
			Assert.AreEqual("M:NDoc3.Core.NormalClass.TakesNestedTypeArg``1(NDoc3.Core.NormalClass.Function{``0},System.String,System.Object[])", memberId);
		}
	}

	/// <summary>
	/// TestAssembly1Class in TestAssembly1
	/// </summary>
	public class NormalClass
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

		static NormalClass()
		{
		}

		public NormalClass()
		{
		}

		public string NormalProperty
		{
			get { return null; }
			set {}
		}

		public string NormalMethod(MemberIDTests i)
		{
			return null; 
		}

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
			return "" + x;
		}

        /// <summary>
        /// An anonymous action delegate with no arguments and no return value.
        /// </summary>
        public delegate T Function<T>();

        /// <summary>
        /// Ensures any exception thrown by the given <paramref name="function"/> is wrapped with an
        /// </summary>
        public static T TakesNestedTypeArg<T>(Function<T> function, string messageFormat, params object[] args)
		{
        	return function();
		}
	}

	/// <summary>
	/// Some generic class
	/// </summary>
	/// <typeparam name="S"></typeparam>
	/// <typeparam name="T"></typeparam>
	public class GenericClass<S, T>
	{
		static GenericClass()
		{}

		public GenericClass(S s, T t)
		{}

		/// <summary>
		/// Returns Generic Class Type
		/// </summary>
		public T ReturnsGenericClassType<U, V>(S s, T t, V v, U u)
		{
			return default(T);
		}

		/// <summary>
		/// Returns Generic Class Type
		/// </summary>
		public T ReturnsGenericClassTypeWithComplexGenericArg<U>(out S s, T[][] t, GenericClass<U,GenericClass<T,S>> v, ref U[,] u)
		{
			s = default(S);
			return default(T);
		}
	}
}