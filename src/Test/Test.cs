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
		/// <summary>Initializes a new instance of the Class class.</summary>
		public Class() { }

		/// <summary>Initializes a new instance of the Class class.</summary>
		public Class(int i) { }
		
		/// <summary>Initializes a new instance of the Class class.</summary>
		public Class(string s) { }
		
		/// <summary>Initializes a new instance of the Class class.</summary>
		protected Class(double d) { }

		/// <summary>Initializes a new instance of the Class class.</summary>
		/// <param name="i1">This is the first integer parameter. 
		/// This is the first integer parameter. This is the first integer 
		/// parameter. This is the first integer parameter.</param>
		/// <param name="i2">This is the second integer parameter.</param>
		/// <param name="i3">This is the third integer parameter.</param>
		public Class(int i1, int i2, int i3) { }

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

		/// <summary>This overloaded indexer accepts three ints.</summary>
		public int this[int i1, int i2, int i3]
		{
			get { return 0; }
		}

		/// <summary>Executes some code.</summary>
		public void Method() { }

		/// <summary>Executes some code.</summary>
		public void Method(int i1, int i2, int i3) { }

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

		/// <summary>This is my first overloaded operator.</summary>
		/// <remarks>Why do we have to declare them as static?</remarks>
		public static bool operator !(Class x)
		{
			return false;
		}
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

		/// <summary>This event is declared in an interface.</summary>
		event Handler InterfaceEvent;
	}

	/// <summary>This is an empty interface.</summary>
	public interface Interface1
	{
	}

	/// <summary>This is also an empty interface.</summary>
	public interface Interface2
	{
	}

	/// <summary>This class implements two empty interfaces.</summary>
	public class ImplementsTwoInterfaces : Interface1, Interface2
	{
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

		private void UseBaseEvent()
		{
			BaseEvent(this, new EventArgs());
		}
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
		/// <summary>This method throws exceptions.</summary>
		/// <exception cref="Exception">A generic exception.</exception>
		/// <exception cref="ApplicationException">An application-specific exception.</exception>
		public void Throw()
		{
		}
	}

	/// <summary>This class contains &lt;see langword=""&gt; elements in the remarks.</summary>
	/// <remarks>The default style is <see langword="bold"/>. 
	/// But <see langword="null"/>, <see langword="sealed"/>,
	/// <see langword="static"/>, <see langword="abstract"/>, 
	/// and <see langword="virtual"/> do more.</remarks>
	public class Langword
	{
	}

	/// <summary>This class contains all the overloadable operators.</summary>
	public class Operators
	{
		/// <summary>Unary plus operator.</summary>
		public static int operator +(Operators o) { return 0; }

		/// <summary>Unary minus operator.</summary>
		public static int operator -(Operators o) { return 0; }

		/// <summary>Logical negation operator.</summary>
		public static int operator !(Operators o) { return 0; }

		/// <summary>Bitwise complement operator.</summary>
		public static int operator ~(Operators o) { return 0; }

		/// <summary>Increment operator.</summary>
		public static Operators operator ++(Operators o) { return null; }

		/// <summary>Decrement operator.</summary>
		public static Operators operator --(Operators o) { return null; }

		/// <summary>Definitely true operator.</summary>
		public static bool operator true(Operators o) { return true; }

		/// <summary>Definitely false operator.</summary>
		public static bool operator false(Operators o) { return false; }

		/// <summary>Addition operator.</summary>
		public static int operator +(Operators x, Operators y) { return 0; }

		/// <summary>Subtraction operator.</summary>
		public static int operator -(Operators x, Operators y) { return 0; }

		/// <summary>Multiplication operator.</summary>
		public static int operator *(Operators x, Operators y) { return 0; }

		/// <summary>Division operator.</summary>
		public static int operator /(Operators x, Operators y) { return 0; }

		/// <summary>Remainder operator.</summary>
		public static int operator %(Operators x, Operators y) { return 0; }

		/// <summary>And operator.</summary>
		public static int operator &(Operators x, Operators y) { return 0; }

		/// <summary>Or operator.</summary>
		public static int operator |(Operators x, Operators y) { return 0; }

		/// <summary>Exclusive-or operator.</summary>
		public static int operator ^(Operators x, Operators y) { return 0; }

		/// <summary>Left-shift operator.</summary>
		public static int operator <<(Operators x, Operators y) { return 0; }

		/// <summary>Right-shift operator.</summary>
		public static int operator >>(Operators x, Operators y) { return 0; }

		/// <summary>Equality operator.</summary>
		public static bool operator ==(Operators x, Operators y) { return false; }

		/// <summary>Inequality operator.</summary>
		public static bool operator !=(Operators x, Operators y) { return false; }

		/// <summary>Equals method.</summary>
		public override bool Equals(Object o) { return false; }

		/// <summary>GetHashCode method.</summary>
		public override int GetHashCode() { return 0; }

		/// <summary>Less-than operator.</summary>
		public static bool operator <(Operators x, Operators y) { return false; }

		/// <summary>Greater-than operator.</summary>
		public static bool operator >(Operators x, Operators y) { return false; }

		/// <summary>Less-than-or-equal operator.</summary>
		public static bool operator <=(Operators x, Operators y) { return false; }

		/// <summary>Greater-than-or-equal operator.</summary>
		public static bool operator >=(Operators x, Operators y) { return false; }
	}

	/// <summary>This class contains various type conversions.</summary>
	public class TypeConversions
	{
		/// <summary>Explicit conversion to an int.</summary>
		public static explicit operator int(TypeConversions t) { return 0; }
	}

	/// <summary>The remarks in this class contains examples of list elements.</summary>
	public class Lists
	{
		/// <summary>BulletMethodSummary</summary>
		/// <remarks>
		///		<list type="bullet">
		///			<item><description>Item One</description></item>
		///			<item><description>Item Two</description></item>
		///			<item><description>Item Three</description></item>
		///		</list>
		/// </remarks>
		public void BulletMethod()
		{
		}

		/// <summary>NumberMethodSummary</summary>
		/// <remarks>
		///		<list type="number">
		///			<item><description>Item One</description></item>
		///			<item><description>Item Two</description></item>
		///			<item><description>Item Three</description></item>
		///		</list>
		/// </remarks>
		public void NumberMethod()
		{
		}

		/// <summary>TermMethodSummary</summary>
		/// <remarks>
		///		<list type="bullet">
		///			<item><term>Term One</term><description>Item One</description></item>
		///			<item><term>Term Two</term><description>Item Two</description></item>
		///			<item><term>Term Three</term><description>Item Three</description></item>
		///		</list>
		/// </remarks>
		public void TermMethod()
		{
		}
	}

	/// <summary>This class has para elements in its remarks.</summary>
	/// <remarks><para>This is the first paragraph.</para>
	/// <para>This is the second paragraph.</para></remarks>
	public class Paragraphs
	{
	}

	/// <summary>This class shows how permission elements are used.</summary>
	public class Permissions
	{
		/// <permission cref="System.Security.PermissionSet">Everyone can access this method.</permission>
		public void PermissionsRequired()
		{
		}
	}

	/// <summary>This is a sealed class.</summary>
	public sealed class SealedClass
	{
	}
}
