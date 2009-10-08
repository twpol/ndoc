using System;
using System.Collections.Generic;

namespace NDoc3.Test.Generics
{
	/// <summary>
	/// Test case for #2871847
	/// </summary>
	/// <typeparam name="T">Type parameter</typeparam>
	public interface Producer_i<T> {
		event Action<T> OnItem1Produced;
	}

	/// <summary>
	/// Test case for #2871847. This failed in r300.
	/// </summary>
	/// <typeparam name="T">Type parameter</typeparam>
	public class ThreadedConsumer_c<T> : Producer_i<T> {
		public event Action<T> OnItem1Produced;
	}

	/// <summary>
	/// Test case for #2871847. It works in r300.
	/// </summary>
	/// <typeparam name="T">Type parameter</typeparam>
	public class ThreadedConsumer_c2<T> : Producer_i<int> {
		public event Action<int> OnItem1Produced;
	}

    /// <summary>
    /// Very generic simple item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleItem<T>
    {
        /// <summary>
        /// 
        /// </summary>
        public SimpleItem()
        {
        }
    }

    /// <summary>
    /// Generic list of generic items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleItemList<T> : List<SimpleItem<T>>
    {
        /// <summary>
        /// 
        /// </summary>
        public SimpleItemList()
        {
        }
    }

    /// <summary>
    /// Generic interface wiht a single generic parameter
    /// </summary>
    /// <typeparam name="T">T Type</typeparam>
    public interface GenericInterface<T>
    {
    }

    /// <summary>
    /// Generic interface with a single generic parameter which
    /// implements another generic interface
    /// </summary>
    /// <typeparam name="T">T type</typeparam>
    public interface GenericInterfaceInherited<T> : GenericInterface<T>
    {
    }

    /// <summary>
    /// Tests a basic generic class with three generic parameters
    /// </summary>
    /// <typeparam name="T">T type</typeparam>
    /// <typeparam name="U">Type U</typeparam>
    /// <typeparam name="Y">Typy Y</typeparam>
    public class Generic_Multiple<T, U, Y>
    {
    }

    /// <summary>
    /// Generic class with a single parameter
    /// </summary>
    /// <typeparam name="T">Type T</typeparam>
    public class Generic_Single<T>
    {
    }

    /// <summary>
    /// Generic class with class constraint
    /// </summary>
    /// <typeparam name="T">Type T</typeparam>
    public class Generic_ClassCon<T> where T : class
    {
    }

    /// <summary>
    /// Generic class with multiple constraints
    /// </summary>
    /// <typeparam name="T">Type T</typeparam>
    /// <typeparam name="U">Type U</typeparam>
    public class Generic_ClassCon2<T, U>
        where T : U
        where U : Generic, new()
    {
    }

    /// <summary>
    /// Contains tests for generics
    /// </summary>
    public class Generic
    {
        /// <summary>
        /// Generic delegate
        /// </summary>
        /// <typeparam name="T">Type T</typeparam>
        /// <param name="item">The item parameter</param>
        public delegate void GenericDelegate<T>(T item) where T : class;

        /// <summary>
        /// Test of generic field
        /// </summary>
        public Generic_Multiple<string, string, string> test_field;

        /// <summary>
        /// Property test with generic return value and asymmetric accessor accessibility
        /// </summary>
        public Generic_Single<int> Test
        {
            get
            {
                return null;
            }
            private set
            {
            }
        }

        /// <summary>
        /// Another generic delegate with a return value
        /// </summary>
        /// <typeparam name="T">Type T</typeparam>
        /// <param name="item">The item</param>
        /// <returns>An instance of Generic_Single</returns>
        public delegate Generic_Single<T> GenericDelegate2<T>(T item);

        /// <summary>
        /// Generic method test
        /// </summary>
        /// <typeparam name="T">The type of test</typeparam>
        /// <param name="test">A test parameter</param>
        public void GenericMeth<T>(T test)
        {
        }

        /// <summary>
        /// Generic method which returns a generic class
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <returns>A instance of Generic_ClassCon with the same
        /// generic template as received</returns>
        public Generic_Single<T> GenericMeth2<T>()
        {
            return null;
        }

        /// <summary>
        /// Combination of the two other generic method tests
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="test">Test parameter</param>
        /// <returns>An instance of Generic_ClassCon</returns>
        public Generic_Single<T> GenericMeth3<T>(Generic_Single<int> test)
        {
            return null;
        }

        /// <summary>
        /// Combination of the two other generic method tests and a constraint
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="test">Test parameter</param>
        /// <returns>An instance of Generic_ClassCon</returns>
        public Generic_ClassCon<T> GenericMeth_Con<T>(T test) where T : class
        {
            return null;
        }

        /// <summary>
        /// Generic return type
        /// </summary>
        /// <typeparam name="T">Type T</typeparam>
        /// <returns>Generic type</returns>
        public Generic_ClassCon<Generic_Single<T>> GenericMethTwo<T>()
        {
            return null;
        }
    }
}