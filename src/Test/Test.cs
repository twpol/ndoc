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

		/// <summary>Holds an read-only<c>int</c> value.</summary>
		public readonly int ReadOnlyField = 12;

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

		/// <summary>Uses some parameter modifyers.</summary>
		public void ParameterModifyers( ref int refParam, out int outParam, params object[] paramArray )
		{
			outParam = 0;
		}

		/// <summary>An overload.</summary>
		public void ParameterModifyers( int a, ref int refParam, out int outParam, params object[] paramArray )
		{
			outParam = 0;
		}

		/// <summary>This is a simple event that uses the Handler delegate.</summary>
		public event Handler Event;

		/// <summary>Stop warning me about Event not being used.</summary>
		public void OnEvent()
		{
			Event(this, new EventArgs());
			ProtectedEvent(this, new EventArgs());
			StaticEvent(this, new EventArgs());
		}

		/// <summary>
		/// Raises some events.
		/// </summary>
		/// <remarks><para>
		/// Raises the <see cref="Event"/> event when <see cref="OnEvent"/> is called,
		/// if <see cref="Field"/> is greater than 0.
		/// </para><para>
		/// The above paragraph is only intended to test crefs on different member types...
		/// </para></remarks>
		/// <event cref="Event">
		/// Raised when something occurs.
		/// </event>
		/// <event cref="ProtectedEvent">
		/// Raised when something else occurs.
		/// </event>
		/// <exception cref="Exception">
		/// Some exception is thrown.
		/// </exception>
		/// <exception cref="MyException">
		/// Some other exception may also be thrown.
		/// </exception>
		public void RaisesSomeEvents()
		{
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

	/// <summary>This is a multicast delegate.</summary>
	public delegate int MulticastHandler(object sender, EventArgs e);

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

		/// <summary>This method is declared in the Base class.</summary>
		public void Overloaded(byte i) { }

		/// <summary>This virtual method is declared in the Base class.</summary>
		public virtual void TwoVirtualOverloads(string key) { }
		/// <summary>This virtual method is declared in the Base class.</summary>
		public virtual void TwoVirtualOverloads(int index) { }

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
		/// <remarks>This is a reference to a parent member: <see cref="Base.BaseProperty"/></remarks>
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

		/// <summary>This method is overriden in the Derived class.</summary>
		/// <remarks>Only one of the two overloads is overriden.</remarks>
		public override void TwoVirtualOverloads(string key) { }
	}

	/// <summary>Represents another derived class.</summary>
	public class Derived2 : Derived
	{
		/// <summary>
		/// This event is declared in the Derived2 class.
		/// </summary>
		public event Handler EventInDerived;

		private void UseDerivedEvent()
		{
			EventInDerived(this, new EventArgs());
		}

		/// <summary>This method is overriden in the Derived class.</summary>
		/// <remarks>Both overloads are overriden in this class.</remarks>
		public override void TwoVirtualOverloads(string key) { }

		/// <summary>This method is overriden in the Derived class.</summary>
		/// <remarks>Both overloads are overriden in this class.</remarks>
		public override void TwoVirtualOverloads(int key) { }

		/// <summary>
		/// Add only one overload in Derived2 class.
		/// </summary>
		public void Overloaded(object o) { }
	}

	/// <summary>Represents an outer class.</summary>
	public class Outer
	{
		/// <summary>Represents an inner class.</summary>
		/// <remarks>These are some remarks.</remarks>
		public class Inner
		{
			/// <summary>This is a field of the inner class.</summary>
			/// <remarks>These are some remarks</remarks>
			public int InnerField;

			/// <summary>This is a property of the inner class.</summary>
			/// <remarks>These are some remarks</remarks>
			public int InnerProperty { get { return 0; } }

			/// <summary>This is a method of the inner class.</summary>
			/// <remarks>These are some remarks</remarks>
			public void InnerMethod() { }

			/// <summary>This is an enumeration nested in a nested class.</summary>
			public enum InnerInnerEnum
			{
				/// <summary>Foo</summary>
				Foo
			}
		}

		/// <summary>Function returning a public inner class oject.</summary>
		public Inner GetInnerClassObject() { return new Inner(); }

		/// <summary>Function with a public inner class oject parameter.</summary>
		public void TestInnerClassObject(Inner TheInner) { }

		/// <summary>Represents a private inner class.</summary>
		private class PrivateInner
		{
		}

		/// <summary>This is a nested enumeration.</summary>
		public enum InnerEnum
		{
			/// <summary>Foo</summary>
			Foo
		}

		/// <summary>This is a nested interface.</summary>
		public interface InnerInterface {}

		/// <summary>This is a nested structure.</summary>
		public struct InnerStruct {}
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
	/// See <see href="http://ndoc.sf.net/" />.
	/// See <see href="http://ndoc.sf.net/">NDOC</see>.
	/// </remarks>
	public class See
	{
		/// <summary>
		/// This field's documentation references <see cref="RefProp1"/>.
		/// </summary>
		public int Field1 = 0;

		/// <summary>
		/// This properties' documentation references <see cref="System.IO.TextWriter"/>.
		/// </summary>
		public string Prop1
		{
			get { return "Prop1"; }
		}

		/// <summary>
		/// This method's documentation references <see cref="NDoc.Test.See.Prop1"/> and
		/// <see cref="Field1"/>.
		/// </summary>
		public void RefProp1()
		{
		}
	}

	/// <summary>This class has lots of &lt;seealso&gt; elements.</summary>
	/// <remarks>NDoc adds a special form of the &lt;seealso&gt; element.
	/// Instead of a cref attribute, you can specify a href attribute some text
	/// content just like a normal HTML &lt;a&gt; element.</remarks>
	/// <seealso href="http://ndoc.sf.net/">the ndoc homepage</seealso>
	/// <seealso cref="Class"/>
	/// <seealso cref="Interface"/>
	/// <seealso cref="Struct1"/>
	/// <seealso cref="Base.BaseMethod"/>
	/// <seealso cref="Derived.DerivedMethod"/>
	/// <seealso cref="Outer"/>
	/// <seealso cref="Outer.Inner"/>
	/// <seealso cref="Handler"/>
	/// <seealso cref="Enum"/>
	/// <seealso href="http://slashdot.org/">Slashdot</seealso>
	/// <seealso cref="System.Object"/>
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

		/// <summary>&lt;seealso cref="System.Object"/></summary>
		/// <seealso cref="System.Object"/>
		public void SeeSystemClass()
		{
		}

		/// <summary>&lt;seealso cref="System.String.Empty"/></summary>
		/// <seealso cref="System.String.Empty"/>
		public void SeeSystemField()
		{
		}

		/// <summary>&lt;seealso cref="System.String.Length"/></summary>
		/// <seealso cref="System.String.Length"/>
		public void SeeSystemProperty()
		{
		}

		/// <summary>&lt;seealso cref="System.Collections.ArrayList.Item"/></summary>
		/// <seealso cref="System.Collections.ArrayList.this"/>
		public void SeeSystemIndexer()
		{
		}

		/// <summary>&lt;seealso cref="System.Object.ToString"/></summary>
		/// <seealso cref="System.Object.ToString"/>
		public void SeeSystemMethod()
		{
		}

		/// <summary>&lt;seealso cref="System.Object.ToString"/></summary>
		/// <seealso cref="System.String.Equals"/>
		public void SeeSystemOverloadedMethod()
		{
		}

		/// <summary>&lt;seealso cref="System.Xml.XmlDocument.NodeChanged"/></summary>
		/// <seealso cref="System.Xml.XmlDocument.NodeChanged"/>
		public void SeeSystemEvent()
		{
		}

		/// <summary>&lt;seealso cref="System.IDisposable"/></summary>
		/// <seealso cref="System.IDisposable"/>
		public void SeeSystemInterface()
		{
		}

		/// <summary>&lt;seealso cref="System.DateTime"/></summary>
		/// <seealso cref="System.DateTime"/>
		public void SeeSystemStructure()
		{
		}

		/// <summary>&lt;seealso cref="System.EventHandler"/></summary>
		/// <seealso cref="System.EventHandler"/>
		public void SeeSystemDelegate()
		{
		}

		/// <summary>&lt;seealso cref="System.DayOfWeek"/></summary>
		/// <seealso cref="System.DayOfWeek"/>
		public void SeeSystemEnumeration()
		{
		}

		/// <summary>&lt;seealso cref="System.IO"/></summary>
		/// <seealso cref="System.IO"/>
		public void SeeSystemNamespace()
		{
		}
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
	///  static void Main() {
	///   System.Console.WriteLine("Hello, World!");
	///  }
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
		///  <list type="bullet">
		///   <item><description>Item One</description></item>
		///   <item><description>Item Two</description></item>
		///   <item><description>Item Three</description></item>
		///  </list>
		/// </remarks>
		public void BulletMethod()
		{
		}

		/// <summary>NumberMethodSummary</summary>
		/// <remarks>
		///  <list type="number">
		///   <item><description>Item One</description></item>
		///   <item><description>Item Two</description></item>
		///   <item><description>Item Three</description></item>
		///  </list>
		/// </remarks>
		public void NumberMethod()
		{
		}

		/// <summary>TermMethodSummary</summary>
		/// <remarks>
		///  <list type="bullet">
		///   <item><term>Term One</term><description>Item One</description></item>
		///   <item><term>Term Two</term><description>Item Two</description></item>
		///   <item><term>Term Three</term><description>Item Three</description></item>
		///  </list>
		/// </remarks>
		public void TermMethod()
		{
		}

		/// <summary>TableMethodSummary</summary>
		/// <remarks>
		///		<list type="table">
		///			<item><description>Cell One</description></item>
		///			<item><description>Cell Two</description></item>
		///			<item><description>Cell Three</description></item>
		///		</list>
		/// </remarks>
		public void TableMethod()
		{
		}

		/// <summary>TableWithHeaderMethodSummary</summary>
		/// <remarks>
		///		<list type="table">
		///			<listheader><description>Header</description></listheader>
		///			<item><description>Cell One</description></item>
		///			<item><description>Cell Two</description></item>
		///			<item><description>Cell Three</description></item>
		///		</list>
		/// </remarks>
		public void TableWithHeaderMethod()
		{
		}

		/// <summary>TwoColumnTableMethodSummary</summary>
		/// <remarks>
		///		<list type="table">
		///			<listheader>
		///				<term>Something</term>
		///				<description>Details</description>
		///			</listheader>
		///			<item>
		///				<term>Item 1</term>
		///				<description>This is the first item</description>
		///			</item>
		///			<item>
		///				<term>Item 2</term>
		///				<description>This is the second item</description>
		///			</item>
		///			<item>
		///				<term>Item 3</term>
		///				<description>This is the third item</description>
		///			</item>
		///		</list>
		/// </remarks>
		public void TwoColumnTableMethod()
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

	/// <summary>This class covers all member visibilities.</summary>
	public class VisibilityTester
	{
		/// <summary>Public constructor</summary>
		public VisibilityTester() {}

		/// <summary>Public method</summary>
		public void PublicMethod() {}

		/// <summary>Public read-only property</summary>
		public bool PublicReadOnlyProperty
		{
			get { return false; }
		}

		/// <summary>Public write-only property</summary>
		public bool PublicWriteOnlyProperty
		{
			set {}
		}

		/// <summary>Public field</summary>
		public bool publicField;

		/// <summary>Public event</summary>
		public event Handler PublicEvent;

		/// <summary>Protected constructor</summary>
		protected VisibilityTester(bool a) {}

		/// <summary>Protected method</summary>
		protected void ProtectedMethod() {}

		/// <summary>Protected read-only property</summary>
		protected bool ProtectedReadOnlyProperty
		{
			get { return false; }
		}

		/// <summary>Protected write-only property</summary>
		protected bool ProtectedWriteOnlyProperty
		{
			set {}
		}

		/// <summary>Protected field</summary>
		protected bool protectedField;

		/// <summary>Protected event</summary>
		protected event Handler ProtectedEvent;

		/// <summary>Private constructor</summary>
		private VisibilityTester(int a) {}

		/// <summary>Private method</summary>
		private void PrivateMethod() {}

		/// <summary>Private read-only property</summary>
		private bool PrivateReadOnlyProperty
		{
			get { return false; }
		}

		/// <summary>Private write-only property</summary>
		private bool PrivateWriteOnlyProperty
		{
			set {}
		}

		/// <summary>Private field</summary>
		private bool privateField = false;

		/// <summary>Private event</summary>
		private event Handler PrivateEvent;

		/// <summary>Protected Internal constructor</summary>
		protected internal VisibilityTester(short a) {}

		/// <summary>Protected Internal method</summary>
		protected internal void ProtectedInternalMethod() {}

		/// <summary>Protected Internal read-only property</summary>
		protected internal bool ProtectedInternalReadOnlyProperty
		{
			get { return false; }
		}

		/// <summary>Protected Internal write-only property</summary>
		protected internal bool ProtectedInternalWriteOnlyProperty
		{
			set {}
		}

		/// <summary>Protected Internal field</summary>
		protected internal bool protectedInternalField;

		/// <summary>Protected Internal event</summary>
		protected internal event Handler ProtectedInternalEvent;

		/// <summary>Internal constructor</summary>
		internal VisibilityTester(long a) {}

		/// <summary>Internal method</summary>
		internal bool InternalMethod()
		{
			return (PublicEvent != null ||
				ProtectedEvent != null ||
				PrivateEvent != null ||
				ProtectedInternalEvent != null ||
				InternalEvent != null ||
				publicField ||
				protectedField ||
				privateField ||
				protectedInternalField ||
				internalField);
		}

		/// <summary>Internal read-only property</summary>
		internal bool InternalReadOnlyProperty
		{
			get { return false; }
		}

		/// <summary>Internal write-only property</summary>
		internal bool InternalWriteOnlyProperty
		{
			set {}
		}

		/// <summary>Internal field</summary>
		internal bool internalField = false;

		/// <summary>Internal event</summary>
		internal event Handler InternalEvent;
	}

	/// <summary>
	/// </summary>
	public class MissingDocumentationBase
	{
		/// <summary>
		/// This one's documented!
		/// </summary>
		/// <param name="a">A param</param>
		/// <param name="b">Anotner param</param>
		/// <returns>returns something</returns>
		/// <remarks><para>
		/// This is a remark.
		/// </para></remarks>
		public int SomeMethod( int a, bool b ) { return 0; }

		/// <summary>
		/// This one's overloaded and documented!
		/// </summary>
		/// <param name="a">A param</param>
		/// <param name="b">Anotner param</param>
		/// <returns>returns something</returns>
		/// <remarks><para>
		/// This is a remark.
		/// </para></remarks>
		public int SomeMethod( int a, int b ) { return 0; }

		/// <summary>
		///
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		/// <remarks> </remarks>
		public int MethodWithEmptyDoc(int a, int b) { return 0; }
	}

	/// <summary>
	/// </summary>
	public class MissingDocumentationDerived : MissingDocumentationBase
	{
	}

	/// <summary>
	/// This is an exception.
	/// </summary>
	public class MyException : ApplicationException
	{
		/// <summary>
		/// This is a constructor for the exception.
		/// </summary>
		/// <param name="message">Message for this exception.</param>
		public MyException( string message ) : base( message ) {}
	}

	/// <summary>This class has custom attributes on it.</summary>
	[CLSCompliant(false)]
	public class CustomAttributes
	{
	}

	/// <summary>This class contains just an indexer so that we can see
	/// if that's what causes the DefaultMemberAttribute attribute to appear.</summary>
	public class JustIndexer
	{
		/// <summary>Am I the default member?</summary>
		public int this[int i]
		{
			get { return 0; }
		}
	}

	/// <summary>The remarks contain some &lt;para> and &lt;code> elements with lang attributes.</summary>
	/// <remarks>
	///		<para>This paragraph has no lang attribute.</para>
	///		<para lang="Visual Basic">This paragraph has a Visual Basic lang attribute.</para>
	///		<para lang="VB">This paragraph has a VB lang attribute.</para>
	///		<para lang="C#">This paragraph has a C# lang attribute.</para>
	///		<para lang="C++, JScript">This paragraph has a C++, JScript lang attribute.</para>
	///		<code lang="Visual Basic">
	///			' This is some Visual Basic code.
	///		</code>
	///		<code lang="VB">
	///			' This is some VB code.
	///		</code>
	///		<code lang="C#">
	///			// This is some C# code.
	///		</code>
	///		<code lang="C++, JScript">
	///			/* This is either C++ or JScript code. */
	///		</code>
	/// </remarks>
	public class LangAttributes
	{
	}

	/// <include file='include.xml' path='documentation/class[@name="IncludeExample"]/*'/>
	public class IncludeExample
	{
	}

	/// <summary>This class has two methods with the same name but one is an instance method
	/// and the other is static.</summary>
	public class BothInstanceAndStaticOverloads
	{
		/// <summary>This is the instance method.</summary>
		public void Foo()
		{
		}

		/// <summary>This is the static method.</summary>
		public static void Foo(int i)
		{
		}
	}

	// The following two examples were submitted by Ross.Nelson@devnet.ato.gov.au
	// in order to demonstrate two bugs.

	/// <summary> this is fred </summary>
	public enum fred {
		/// <summary>aaaa</summary>
		valuea,
		/// <summary>bbbb</summary>
		valueb
	}

	/// <summary>this is jjj</summary>
	public class jjj
	{
		/// <summary> this is fred </summary>
		public enum fred {
			/// <summary>aaaa</summary>
			valuea,
			/// <summary>bbbb</summary>
			valueb
		}

		/// <summary>jjj constructor</summary>
		/// <remarks>jjj blah</remarks>
		/// <param name="f">f blah</param>
		public jjj(fred f)
		{
		}

		/// <summary>mmm method</summary>
		/// <remarks>mmm blah</remarks>
		/// <param name="f">f blah</param>
		public void mmm(fred f)
		{
		}
	}

	/// <summary>This class has an event that throws an exception.</summary>
	public class EventWithException
	{
		/// <exception cref="System.Exception">Thrown when... .</exception>
		public event EventHandler ServiceRequest
		{
			add {}
			remove {}
		}
	}

	/// <summary>This class has a method that's overloaded where one of the
	/// overloads doesn't have any parameters.</summary>
	public class OverloadedWithNoParameters
	{
		/// <summary>This is an overloaded method.</summary>
		/// <remarks>This overload has no parameters.</remarks>
		public void Method() {}

		/// <summary>This is an overloaded method.</summary>
		/// <remarks>This overload has one parameter.</remarks>
		public void Method(int i) {}
	}

#warning The link to the method with parameters should point to that correct page.
	/// <summary>This class wants to ref the method with no parameters
	/// in the OverloadedWithNoParameters class.
	/// See <see cref="OverloadedWithNoParameters.Method" />
	/// ("OverloadedWithNoParameters.Method").
	/// See <see cref="OverloadedWithNoParameters.Method()" />
	/// ("OverloadedWithNoParameters.Method()").
	/// See <see cref="OverloadedWithNoParameters.Method(int)" />
	/// ("OverloadedWithNoParameters.Method(int)").
	/// </summary>
	public class CRefToOverloadWithNoParameters
	{
	}

	/// <summary>Explicit interface test (public)</summary>
	public interface ExplicitInterface
	{
		/// <summary>Explicit method test</summary>
		int ExplicitProperty { get; }

		/// <summary>Implicit method test</summary>
		int ImplicitProperty { get; }

		/// <summary>Explicit method test</summary>
		void ExplicitMethod();

		/// <summary>Implicit method test</summary>
		void ImplicitMethod();
	}

	/// <summary>Explicit interface test (internal)</summary>
	internal interface ExplicitInternalInterface
	{
		/// <summary>Explicit method test</summary>
		void ExplicitMethodOfInternalInterface();
	}

	/// <summary>Testing explicit interface implementations</summary>
	public class ExplicitImplementation : ExplicitInterface, ExplicitInternalInterface
	{
		/// <summary>an explicitly implemented property</summary>
		int ExplicitInterface.ExplicitProperty
		{
			get { return 0; }
		}

		/// <summary>an implicitely implemented property</summary>
		public int ImplicitProperty { get { return -1; } }

		/// <summary>an explicitly implemented method</summary>
		void ExplicitInterface.ExplicitMethod()
		{
		}

		/// <summary>an implicitely implemented method</summary>
		public void ImplicitMethod()
		{
		}

		/// <summary>an explicitly implemented method of an internal interface</summary>
		void ExplicitInternalInterface.ExplicitMethodOfInternalInterface()
		{
		}
	}

	/// <summary>Test the new overloads tag.</summary>
	public class OverloadsTag
	{
		/// <overloads>This constructor is overloaded.</overloads>
		/// 
		/// <summary>This overloaded constructor accepts no parameters.</summary>
		public OverloadsTag()
		{
		}
		/// <summary>This overloaded constructor accepts one int parameter.</summary>
		public OverloadsTag(int i)
		{
		}

		/// <overloads>This indexer is overloaded.</overloads>
		/// 
		/// <summary>This overloaded indexer accepts one int parameter.</summary>
		public int this[int i]
		{
			get { return 0; }
		}
		/// <summary>This overloaded indexer accepts one string parameter.</summary>
		public int this[string s]
		{
			get { return 0; }
		}

		/// <overloads>
		/// This method is overloaded.
		/// </overloads>
		/// 
		/// <summary>This overload accepts no parameters.</summary>
		public void OverloadedMethod()
		{
		}
		/// <summary>This overload accepts one int parameter.</summary>
		public void OverloadedMethod(int i)
		{
		}

		/// <overloads>
		///   <summary>This method is overloaded.</summary>
		///   <remarks>
		///		<para>This remark should also appear.</para>
		///     <note>This is a note.</note>
		///   </remarks>
		///   <example>
		///     <para>This is some example code.</para>
		///     <code>Foo.Bar.Baz.Quux();</code>
		///   </example>
		/// </overloads>
		/// 
		/// <summary>This overload accepts no parameters.</summary>
		public void FullDocOverloadedMethod()
		{
		}
		/// <summary>This overload accepts one int parameter.</summary>
		public void FullDocOverloadedMethod(int i)
		{
		}

		/// <overloads>The Addition for <b>OverloadsTag</b>.</overloads>
		/// 
		/// <summary>
		/// Addition that takes two <b>OverloadsTag</b>.
		/// </summary>
		public static OverloadsTag operator +(OverloadsTag x, OverloadsTag y)
		{
			return new OverloadsTag();
		}
		/// <summary>
		/// Addition that takes an <b>OverloadsTag</b> and an <b>Int32</b>.
		/// </summary>
		public static OverloadsTag operator +(OverloadsTag x, int i)
		{
			return new OverloadsTag(i);
		}
	}

	/// <summary>This class uses note elements on its members.</summary>
	public class NotesTest
	{
		/// <summary>
		///   <para>This summary has a note.</para>
		///   <note>This is a note.</note>
		/// </summary>
		public void NoteInSummary()
		{
		}

		/// <summary>This method has a note in its remarks.</summary>
		/// <remarks>
		///   <para>These remarks have a note.</para>
		///   <note>This is a note.</note>
		/// </remarks>
		public void NoteInRemarks()
		{
		}

		/// <summary>This method has cautionary note in its remarks.</summary>
		/// <remarks>
		///   <para>These remarks have a cautionary note.</para>
		///   <note type="caution">Watch out!</note>
		/// </remarks>
		public void CautionNote()
		{
		}
	}

	/// <summary>This class has an indexer with a name other than Item.</summary>
	public class IndexerNotNamedItem
	{
		/// <summary>This indexer is not named Item.</summary>
		[System.Runtime.CompilerServices.IndexerName("MyItem")]
		public int this[int i]
		{
			get { return 0; }
		}
	}

	/// <summary>This is a private class.</summary>
	class PrivateClass
	{
#warning This type should not appear when DocumentInternals is false.
		/// <summary>This is a public enum nested in a private class.</summary>
		public enum PublicEnumInPrivateClass
		{
			/// <summary>Foo</summary>
			Foo,
			/// <summary>Bar</summary>
			Bar
		}
	}

	/// <summary>This class has a member that uses 2D rectangular arrays.</summary>
	public class Matrix
	{
		/// <summary>Returns the inverse of a matrix.</summary>
		/// <param name="matrix">A matrix.</param>
		/// <returns>The inverted matrix.</returns>
		public static double[,] Inverse2(double[,] matrix)
		{
			return null;
		}

		/// <summary>Returns the inverse of a matrix.</summary>
		/// <param name="matrix">A matrix.</param>
		/// <returns>The inverted matrix.</returns>
		public static double[,,] Inverse3(double[,,] matrix)
		{
			return null;
		}
	}

	/// <summary>This public class contains a private enum.</summary>
	public class PublicClassWithPrivateEnum
	{
		/// <summary>This is a private enum in a public class.</summary>
		private enum PrivateEnum
		{
			/// <summary>foo</summary>
			Foo
		}
	}

	/// <summary>This class has a method that accepts a ref to a byte array.</summary>
	public class RefToByteArrayTest
	{
		/// <summary>This method that accepts a ref to a byte array.</summary>
		/// <param name="array">A ref to a byte array.</param>
		public void RefToByteArray(ref byte[] array)
		{
		}
	}

	/// <summary>This class causes the &lt;PrivateImplementationDetails> 
	/// class to appear in the compiled assembly.</summary>
	public class PrivateImplementationDetails
	{
		static byte[] bar = new byte[] {1,2,3};
	}

#warning This link points to a page that will never exist.
	/// <summary>See <see cref="Enum.Foo" />.</summary>
	public class LinkToEnumMember
	{
	}

	/// <summary>See <see cref="SeeOverloadedStatic.StaticOverload" />.</summary>
	public class SeeOverloadedStatic
	{
		/// <summary>Overload one.</summary>
		public static void StaticOverload(int i)
		{
		}

		/// <summary>Overload two.</summary>
		public static void StaticOverload(string s)
		{
		}
	}

	/// <summary>This class contains constant fields.</summary>
	public class ConstFields
	{
		/// <summary>This is a constant string.</summary>
		public const string ConstString = "ConstString";
	}
}

namespace NDoc.Test.NewStuff
{
	/// <summary>no comment</summary>
	public interface IInterfaceA
	{
		/// <summary>no comment</summary>
		void InheritedImplicitInterfaceMethod();
	}
	/// <summary>no comment</summary>
	public interface IInterfaceB
	{
		/// <summary>no comment</summary>
		void InheritedExplicitInterfaceMethod();
	}
	/// <summary>no comment</summary>
	public interface IInterfaceC
	{
		/// <summary>no comment</summary>
		void ImplicitInterfaceMethod();
	}
	/// <summary>no comment</summary>
	public interface IInterfaceD
	{
		/// <summary>no comment</summary>
		void ExplicitInterfaceMethod();
	}
	/// <summary>no comment</summary>
	public interface IInterfaceE
	{
		/// <summary>no comment</summary>
		void NewInterfaceMethod();
	}
	/// <summary>no comment</summary>
	public interface IInterfaceF
	{
		/// <summary>no comment</summary>
		void InterfaceMethodOverride();
	}
	/// <summary>
	/// Base class used to test the new modifier. See <see cref="NewDerived"/>
	/// for details. Members in this class are named to demonstrate syntax
	/// in the derived class.
	/// </summary>
	public class NewBase : IInterfaceA, IInterfaceB, IInterfaceE, IInterfaceF
	{
		/// <summary>no comment</summary>
		public class NewClass
		{
		}
		/// <summary>no comment</summary>
		public int this[int n]
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		/// <summary>no comment</summary>
		public void NewPropertySNOKOM(int n)
		{
		}
		/// <summary>no comment</summary>
		public void NewVirtualPropertySNOKOM(int n)
		{
		}
		/// <summary>no comment</summary>
		public int NewMethodSNOKOM;
		/// <summary>no comment</summary>
		public int NewVirtualMethodSNOKOM;
		/// <summary>no comment</summary>
		public int NewFieldSNOKOM
		{
			get { return 0; }
			set { }
		}

		/// <summary>no comment</summary>
		public const int NewConst = 500;
		/// <summary>no comment</summary>
		public int NewField;

		/// <summary>no comment</summary>
		public int NewProperty
		{
			get { return 0; }
			set {}
		}
		/// <summary>no comment</summary>
		public virtual int OverrideProperty
		{
			get { return 0; }
			set {}
		}
		/// <summary>no comment</summary>
		public virtual int NewVirtualProperty
		{
			get { return 0; }
			set {}
		}

		/// <summary>no comment</summary>
		public void NewMethod(int n)
		{
		}
		/// <summary>no comment</summary>
		public virtual void OverrideMethod(int n)
		{
		}
		/// <summary>no comment</summary>
		public virtual void NewVirtualMethod(int n)
		{
		}
		/// <summary>no comment</summary>
		public void NewMethodWithOverload(int n)
		{
		}
		/// <summary>no comment</summary>
		public virtual void NewMethodWithOverload(double d)
		{
		}
		/// <summary>no comment</summary>
		public virtual void NewMethodWithOverload(long l)
		{
		}
		/// <summary>
		/// public void NewMethodWithOverload(short h)
		/// </summary>
		/// <param name="h"></param>
		public void NewMethodWithOverload(short h)
		{
		}

		/// <summary>no comment</summary>
		public void NewStaticMethod(int n)
		{
		}

		#region Implementation of IInterfaceA
		/// <summary>no comment</summary>
		public void InheritedImplicitInterfaceMethod()
		{
		
		}
		#endregion

		#region Implementation of IInterfaceB
		void IInterfaceB.InheritedExplicitInterfaceMethod()
		{
		
		}
		#endregion

		#region Implementation of IInterfaceE
		/// <summary>no comment</summary>
		public void NewInterfaceMethod()
		{
		
		}
		#endregion

		#region Implementation of IInterfaceF
		/// <summary>no comment</summary>
		public virtual void InterfaceMethodOverride()
		{
		
		}
		#endregion
	}
	/// <summary>
	/// This class provides new implementations for the base class members.
	/// </summary>
	public class NewDerived : NewBase, IInterfaceC, IInterfaceD
	{
		/// <summary>no comment</summary>
		new public class NewClass
		{
		}
		/// <summary>This indexer is new</summary>
		new public int this[int n]
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}
		/// <summary>This indexer is an overload</summary>
		public int this[string n]
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		/// <summary>no comment</summary>
		public const int Const = 500;
		/// <summary>no comment</summary>
		public new const int NewConst = 500;
		/// <summary>no comment</summary>
		public int Field;
		/// <summary>no comment</summary>
		public new int NewField;
		/// <summary>
		/// "Same Name Other Kind Of Member"
		/// </summary>
		public new int NewFieldSNOKOM;

		/// <summary>no comment</summary>
		public int Property
		{
			get { return 0; }
			set {}
		}
		/// <summary>no comment</summary>
		public new int NewProperty
		{
			get { return 0; }
			set {}
		}
		/// <summary>no comment</summary>
		public virtual int VirtualProperty
		{
			get { return 0; }
			set {}
		}
		/// <summary>no comment</summary>
		public override int OverrideProperty
		{
			get { return 0; }
			set {}
		}
		/// <summary>no comment</summary>
		public new virtual int NewVirtualProperty
		{
			get { return 0; }
			set {}
		}
		/// <summary>
		/// "Same Name Other Kind Of Member"
		/// </summary>
		public new int NewPropertySNOKOM
		{
			get { return 0; }
			set { }
		}
		/// <summary>
		/// "Same Name Other Kind Of Member"
		/// </summary>
		public new virtual int NewVirtualPropertySNOKOM
		{
			get { return 0; }
			set { }
		}
		/// <summary>no comment</summary>
		public void Method(int n)
		{
		}
		/// <summary>no comment</summary>
		public new void NewMethod(int n)
		{
		}
		/// <summary>no comment</summary>
		public virtual void VirtualMethod(int n)
		{
		}
		/// <summary>no comment</summary>
		public override void OverrideMethod(int n)
		{
		}
		/// <summary>no comment</summary>
		public new virtual void NewVirtualMethod(int n)
		{
		}
		/// <summary>
		/// "Same Name Other Kind Of Member"
		/// </summary>
		public new void NewMethodSNOKOM(int n)
		{
		}
		/// <summary>
		/// "Same Name Other Kind Of Member"
		/// </summary>
		public new virtual void NewVirtualMethodSNOKOM(int n)
		{
		}
		/// <summary>
		/// public void NewMethodWithOverload(string s)
		/// </summary>
		/// <param name="s"></param>
		public void NewMethodWithOverload(string s)
		{
		}
		/// <summary>
		/// new public void NewMethodWithOverload(int n)
		/// </summary>
		/// <param name="n"></param>
		public new void NewMethodWithOverload(int n)
		{
		}
		/// <summary>
		/// public virtual void NewMethodWithOverload(object o)
		/// </summary>
		/// <param name="o"></param>
		public virtual void NewMethodWithOverload(object o)
		{
		}
		/// <summary>
		/// public override void NewMethodWithOverload(double d)
		/// </summary>
		/// <param name="d"></param>
		public override void NewMethodWithOverload(double d)
		{
		}
		/// <summary>
		/// new public virtual void NewMethodWithOverload(long l)
		/// </summary>
		/// <param name="l"></param>
		public new virtual void NewMethodWithOverload(long l)
		{
		}

		/// <summary>no comment</summary>
		public new static void NewStaticMethod(int n)
		{
		}

		#region Implementation of IInterfaceC
		/// <summary>no comment</summary>
		public void ImplicitInterfaceMethod()
		{
		
		}
		#endregion

		#region Implementation of IInterfaceD
		void IInterfaceD.ExplicitInterfaceMethod()
		{
		
		}
		#endregion

		/// <summary>no comment</summary>
		public new void NewInterfaceMethod()
		{
		
		}

		/// <summary>no comment</summary>
		public override void InterfaceMethodOverride()
		{
		
		}
	}

	/// <summary>
	/// Do you see F?
	/// </summary>
	public class Base
	{
		/// <summary>no comment</summary>
		public static void F() {}
	}
	/// <summary>
	/// Now F is gone (private)!
	/// </summary>
	public class Derived: Base
	{
		new private static void F() {}   // Hides Base.F in Derived only
	}
	/// <summary>
	/// Where is F?
	/// </summary>
	public class MoreDerived: Derived
	{
	}
	/// <summary>
	/// Uses MoreDerived.F (actually Base.F)
	/// </summary>
	public class SomeClass
	{
		/// <summary>
		/// Works, so MoreDerived.F is there, but where?
		/// </summary>
		static void G() { MoreDerived.F(); }         // Invokes Base.F
	}
}

namespace NDoc.Test.InternalStuff
{
	internal class InternalClass
	{
		public InternalClass() {}
	}
}

namespace NDoc.Test.Attributes
{
	using System;
	using System.Collections;
	using System.Xml.Serialization;

	/// <summary>
	/// "IsTested" custom attribute class
	/// </summary>
	public class IsTestedAttribute : Attribute
	{
	}

	/// <summary>
	/// "Author" custom attribute class
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class AuthorAttribute : Attribute
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"></param>
		public AuthorAttribute(string name)
		{
			this.name = name;
			this.version = 0;
		}

		/// <summary>
		/// Name property of the Author attribute
		/// </summary>
		public string Name 
		{
			get 
			{
				return name;
			}
		}

		/// <summary>
		/// "IsTested" Attribute applied onto a property.
		/// </summary>
		[IsTested]
		public int Version
		{
			get 
			{
				return version;
			}
			set 
			{
				version = value;
			}
		}

		private string name;
		private int version;
	}

	/// <summary>
	/// Class with the "Author" attribute
	/// </summary>
	[Author("Joe Programmer")]
	public class Account
	{
		/// <summary>
		/// Method with the "IsTested" attribute
		/// </summary>
		[IsTested]
		public void AddOrder(Order orderToAdd)
		{
		}
	}

	/// <summary>
	/// Class with 3 attributes:  Author(name="Jane Programmer", Version=2), IsTested and XmlType.
	/// </summary>
	/// <remarks>This class has the [Serializable] attribute.</remarks>
	[Author("Jane Programmer", Version = 2), IsTested()]
	[XmlType(Namespace="NDoc/Test/Order")]
	[Serializable]
	public class Order
	{
		/// <summary>
		/// Field with XmlElement attribute.
		/// </summary>
		[XmlElement]
		public int Number = 0;

		/// <summary>
		/// Another field with XmlElement attribute.
		/// </summary>
		[XmlElement]
		public string What = "";

		/// <summary>
		/// This field has the [NonSerialized] attribute.
		/// </summary>
		[NonSerialized]
		public bool dummy = true;
	}

}