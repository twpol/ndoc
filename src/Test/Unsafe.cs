using System;

namespace NDoc.Test.Unsafe
{
	/// <summary>
	/// This class has various mebers that are marked as unsafe and that return
	/// pointers
	/// </summary>
	public class ClassWithUnsafeMembers
	{
		/// <summary>
		/// An unsafe constructor
		/// </summary>
		/// <param name="p">a pointer</param>
		unsafe public ClassWithUnsafeMembers( Int32* p )
		{

		}

		/// <summary>
		/// A public pointer field
		/// </summary>
		unsafe public Int32* pointerField;

		/// <summary>
		/// A property that is a pointer type
		/// </summary>
		unsafe public Int32* PointerProperty
		{
			get
			{
				return pointerField;
			}
			set
			{
				pointerField = value;
			}
		}
		/// <summary>
		/// Pass an unsafe pointer as a paramater
		/// </summary>
		/// <param name="p">A pointer to an int32</param>
		unsafe public void PassAPointer( Int32* p )
		{

		}
		/// <summary>
		/// unsafe method return
		/// </summary>
		/// <returns>The address of an int32</returns>
		unsafe public Int32* GetIntPointer()
		{
			return pointerField;
		}
	}
}