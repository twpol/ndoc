namespace NDoc.Test
{
	using System;

	/// <summary>Represents a normal class.</summary>
	/// <remarks>Conceptualizing random endpoints in a access matrix 
	/// provides reach extentions enterprise wide. Respective divisions 
	/// historically insignificant, upscale trendlines in a management 
	/// inventory analysis survivabilty format.</remarks>
	public class Class
	{
		/// <summary>Holds an <c>int</c> value.</summary>
		public int Field;

		/// <summary>Holds a static <c>int</c> value.</summary>
		public static int StaticField;

		/// <summary>Gets a value.</summary>
		public int Property
		{
			get { return 0; }
		}

		/// <summary>Gets a static value.</summary>
		public static int StaticProperty
		{
			get { return 0; }
		}

		/// <summary>This overloaded indexer accepts an int.</summary>
		public int this[int i]
		{
			get { return 0; }
		}

		/// <summary>This overloaded indexer accepts a string.</summary>
		public int this[string s]
		{
			get { return 0; }
		}

		/// <summary>Executes some code.</summary>
		public void Method() { }

		/// <summary>Executes some static code.</summary>
		public static void StaticMethod() { }

		/// <summary>This is a simple event that uses the Handler delegate.</summary>
		public event Handler Event;

		/// <summary>Stop warning me about Event not being used.</summary>
		public void OnEvent()
		{
			Event(this, new EventArgs());
			ProtectedEvent(this, new EventArgs());
			StaticEvent(this, new EventArgs());
		}

		/// <summary>This event is protected.</summary>
		protected event Handler ProtectedEvent;

		/// <summary>Can you do this?</summary>
		public static event Handler StaticEvent;
	}

	/// <summary>This is a simple delegate used by Class.</summary>
	public delegate void Handler(object sender, EventArgs e);

	/// <summary>This is an interface.</summary>
	public interface Interface
	{
		/// <summary>This is a property in an interface.</summary>
		int InterfaceProperty
		{
			get;
		}

		/// <summary>This is a method in an interface.</summary>
		void InterfaceMethod();

		/// <summary>This event is decalred in an interface.</summary>
		event Handler InterfaceEvent;
	}

	/// <summary>Represents an abstract class.</summary>
	public abstract class Abstract
	{
		/// <summary>This event is decalred in the Abstract class.</summary>
		public abstract event Handler InterfaceEvent;
	}

	/// <summary>Represents a base class.</summary>
	public class Base
	{
		/// <summary>This property is declared in the Base class.</summary>
		public int BaseProperty
		{
			get { return 0; }
		}

		/// <summary>This method is declared in the Base class.</summary>
		public void BaseMethod() { }

		/// <summary>This method is declared in the Base class without the "new" keyword.</summary>
		public void NewMethod() { }

		/// <summary>This method is declared in the Base class.</summary>
		public void Overloaded(int i) { }

		/// <summary>This field is declared in the Base class.</summary>
		public int BaseField;

		/// <summary>This event is declared in the Base class.</summary>
		public event Handler BaseEvent;
	}

	/// <summary>Represents a derived class.</summary>
	public class Derived : Base
	{
		/// <summary>This property is declared in the Derived class.</summary>
		public int DerivedProperty
		{
			get { return 0; }
		}

		/// <summary>This method is declared in the Derived class.</summary>
		public void DerivedMethod() { }

		/// <summary>This method is declared in the Derived class with the "new" keyword.</summary>
		public new void NewMethod() { }

		/// <summary>This method is overloaded in the Derived class.</summary>
		public void Overloaded(string s) { }

		/// <summary>This method is also overloaded in the Derived class.</summary>
		public void Overloaded(double d) { }

		/// <summary>This method is also overloaded in the Derived class.</summary>
		public void Overloaded(char c) { }

		/// <summary>This method is also overloaded in the Derived class.</summary>
		/// <remarks>This method accepts a type declared in the same namespace.</remarks>
		public void Overloaded(Interface i) { }
	}

	/// <summary>Represents another derived class.</summary>
	public class Derived2 : Derived
	{
	}

	/// <summary>Represents an outer class.</summary>
	public class Outer
	{
		/// <summary>Represents an inner class.</summary>
		public class Inner
		{
		}

		/// <summary>Represents a private inner class.</summary>
		private class PrivateInner
		{
		}
	}

	/// <summary>This is an internal class.</summary>
	internal class Internal
	{
		/// <summary>This method is declared in the Internal class.</summary>
		internal void InternalMethod() { }
	}

	/// <summary>This is the first struct.</summary>
	public struct Struct1
	{
		/// <summary>This is the first field in the first struct.</summary>
		public int Field1;

		/// <summary>This is the second field in the first struct.</summary>
		public string Field2;
	}

	/// <summary>This is the second struct.</summary>
	public struct Struct2
	{
	}

	/// <summary>This is an enumeration.</summary>
	public enum Enum
	{
		/// <summary>Represents Foo.</summary>
		Foo,
		/// <summary>Represents Bar.</summary>
		Bar,
		/// <summary>Represents Baz.</summary>
		Baz,
		/// <summary>Represents Quux.</summary>
		Quux
	}

	/// <summary>This class has lots of &lt;see&gt; elements in the remarks.</summary>
	/// <remarks>See <see cref="Class"/>. 
	/// See <see cref="Interface"/>. 
	/// See <see cref="Struct1"/>.
	/// See <see cref="Base.BaseMethod"/>.
	/// See <see cref="Derived.DerivedMethod"/>.
	/// See <see cref="Outer"/>.
	/// See <see cref="Outer.Inner"/>.
	/// See <see cref="Handler"/>.
	/// See <see cref="Enum"/>.
	/// </remarks>
	public class See
	{
	}

	/// <summary>This class has lots of &lt;seealso&gt; elements.</summary>
	/// <seealso cref="Class"/>
	/// <seealso cref="Interface"/>
	/// <seealso cref="Struct1"/>
	/// <seealso cref="Base.BaseMethod"/>
	/// <seealso cref="Derived.DerivedMethod"/>
	/// <seealso cref="Outer"/>
	/// <seealso cref="Outer.Inner"/>
	/// <seealso cref="Handler"/>
	/// <seealso cref="Enum"/>
	public class SeeAlso
	{
		/// <summary>This method has lots of &lt;seealso&gt; elements.</summary>
		/// <seealso cref="Class"/>
		/// <seealso cref="Interface"/>
		/// <seealso cref="Struct1"/>
		/// <seealso cref="Base.BaseMethod"/>
		/// <seealso cref="Derived.DerivedMethod"/>
		/// <seealso cref="Outer"/>
		/// <seealso cref="Outer.Inner"/>
		/// <seealso cref="Handler"/>
		/// <seealso cref="Enum"/>
		public void AlsoSee() { }
	}

	/// <summary>Represents a class containing properties.</summary>
	public abstract class Properties
	{
		/// <summary>This property has a getter and a setter.</summary>
		public int GetterAndSetter
		{
			get { return 0; }
			set { }
		}

		/// <summary>This property has a getter only.</summary>
		public int GetterOnly
		{
			get { return 0; }
		}

		/// <summary>This property has a setter only.</summary>
		public int SetterOnly
		{
			set { }
		}

		/// <summary>This property is abstract.</summary>
		public abstract int AbstractProperty
		{
			get;
			set;
		}

		/// <summary>This property is virtual.</summary>
		public virtual int VirtualProperty
		{
			get { return 0; }
			set { }
		}

		/// <summary>This is an overloaded indexer.</summary>
		/// <remarks>This indexer accepts an int parameter.</remarks>
		public int this[int foo]
		{
			get { return 0; }
		}

		/// <summary>This is an overloaded indexer.</summary>
		/// <remarks>This indexer accepts a string parameter.</remarks>
		public int this[string foo]
		{
			get { return 0; }
		}
	}

	/// <summary>Represents a class that has lots of links 
	/// in its documentation.</summary>
	public class Links
	{
		/// <summary>Holds an integer.</summary>
		public int IntField;

		/// <summary>Gets or sets an integer.</summary>
		/// <value>an integer</value>
		public int IntProperty
		{
			get { return 0; }
			set { }
		}

		/// <summary>Returns nothing.</summary>
		/// <returns>Nada.</returns>
		public void VoidMethod() { }

		/// <summary>Returns an int.</summary>
		public int IntMethod() { return 0; }
		
		/// <summary>Returns a string.</summary>
		public string StringMethod() { return null; }

		/// <summary>This method accepts lots of parameters.</summary>
		/// <param name="i">an integer</param>
		/// <param name="s">a string</param>
		/// <param name="c">a character</param>
		/// <param name="d">a double</param>
		/// <remarks>The <paramref name="i"/> param is an integer.
		/// The <paramref name="s"/> param is a string.</remarks>
		public void LotsOfParams(int i, string s, char c, double d)
		{
		}
	}

	/// <summary>This class contains some example code.</summary>
	/// <example><code>
	/// public class HelloWorld {
	///		static void Main() {
	///			System.Console.WriteLine("Hello, World!");
	///		}
	/// }
	/// </code></example>
	public class Example
	{
	}

	/// <summary>This class contains a method that throws exceptions.</summary>
	public class Exceptions
	{
		/// <summary>This method throws exceptions.</summary>\
		/// <exception cref="Exception">A generic exception.</exception>
		/// <exception cref="ApplicationException">An application-specific exception.</exception>
		public void Throw()
		{
		}
	}

	/// <summary>This class contains &lt;see langword=""&gt; elements in the remarks.</summary>
	/// <remarks>The default style is <see langword="bold"/>. 
	/// But <see langword="null"/> and <see langword="sealed"/> do more.</remarks>
	public class Langword
	{
	}
}
