using System.Reflection;

namespace NDoc3.Core.Reflection {
	///<summary>
	/// Defines possible values for a method's contract.
	///</summary>
	public enum MethodContract {
		/// <summary>
		/// Method is an instance member.
		/// </summary>
		Normal,
		/// <summary>
		/// Method is static. (<see cref="MethodAttributes.Static"/>)
		/// </summary>
		Static,
		/// <summary>
		/// Method is abstract. (<see cref="MethodAttributes.Abstract"/>)
		/// </summary>
		Abstract,
		/// <summary>
		/// Method is final. (<see cref="MethodAttributes.Final"/>)
		/// </summary>
		Final,
		/// <summary>
		/// Method is virtual. (<see cref="MethodAttributes.Virtual"/>)
		/// </summary>
		Virtual,
		/// <summary>
		/// Method is an override. (<see cref="MethodAttributes.NewSlot"/>|<see cref="MethodAttributes.Virtual"/>).
		/// </summary>
		Override
	}
}