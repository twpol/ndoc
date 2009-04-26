// MemberID.cs
// Copyright (C) 2005  Kevin Downs
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NDoc3.Core.Reflection
{
	/// <summary>
	/// 
	/// </summary>
	public static class MemberID
	{
		/// <summary>
		/// Gets the member ID of a type member
		/// </summary>
		/// <param name="member">TODO</param>
		/// <returns>TODO</returns>
		public static string GetDeclaringMemberID(MemberInfo member)
		{
			if (member is MethodInfo)
			{
				return GetMemberID((MethodInfo) member, true);
			} else if (member is EventInfo)
			{
				return GetMemberID((EventInfo) member, true);
			} else if (member is PropertyInfo)
			{
				return GetMemberID((PropertyInfo) member, true);
			}
			else if (member is FieldInfo)
			{
				return GetMemberID((FieldInfo) member, true);
			}
			else
			{
				throw new ArgumentException(string.Format("Invalid member type {0}", member.GetType().FullName));
			}
		}

		/// <summary>
		/// Get the member ID of a type
		/// </summary>
		/// <param name="type">The type</param>
		/// <returns>The member ID</returns>
		public static string GetMemberID(Type type)
		{
			return "T:" + GetTypeNamespaceName(type);
		}

		/// <summary>
		/// Get the member ID of a field
		/// </summary>
		/// <param name="field">The field</param>
		/// <param name="useDeclaringType">TODO</param>
		/// <returns>The member ID</returns>
		public static string GetMemberID(FieldInfo field, bool useDeclaringType)
		{
			return "F:" + GetFullNamespaceName(field, useDeclaringType) + "." + field.Name;
		}

		/// <summary>
		/// Get the member ID of a property
		/// </summary>
		/// <param name="property">The property</param>
		/// <param name="useDeclaringType">TODO</param>
		/// <returns>The member ID</returns>
		public static string GetMemberID(PropertyInfo property, bool useDeclaringType)
		{
			string memberName = "P:" + GetFullNamespaceName(property, useDeclaringType) +
								"." + property.Name.Replace('.', '#').Replace('+', '#');

			try
			{
				if (property.GetIndexParameters().Length > 0)
				{
					memberName += "(";

					int i = 0;

					foreach (ParameterInfo parameter in property.GetIndexParameters())
					{
						if (i > 0)
						{
							memberName += ",";
						}

						Type type = parameter.ParameterType;

						if (type.ContainsGenericParameters)
						{
							memberName += "`" + type.GenericParameterPosition;
						}
						else
						{
							memberName += type.FullName;
						}

						++i;
					}

					memberName += ")";
				}
			}
			catch (System.Security.SecurityException) { }

			return memberName;
		}

		/// <summary>
		/// Get the member ID of a method
		/// </summary>
		/// <param name="method">The method</param>
		/// <param name="useDeclaringType">TODO</param>
		/// <returns>The member ID</returns>
		public static string GetMemberID(MethodBase method, bool useDeclaringType)
		{
			string memberName =
				"M:" +
				GetFullNamespaceName(method, useDeclaringType) +
				"." +
				method.Name.Replace('.', '#').Replace('+', '#');

			if (method.IsGenericMethod)
				memberName = memberName + "``" + method.GetGenericArguments().Length;

			memberName += GetParameterList(method);

			if (method is MethodInfo)
			{
				MethodInfo mi = (MethodInfo)method;
				if (mi.Name == "op_Implicit" || mi.Name == "op_Explicit")
				{
					memberName += "~" + mi.ReturnType;
				}
			}

			return memberName;
		}

		/// <summary>
		/// Get the member ID of an event
		/// </summary>
		/// <param name="eventInfo">The event</param>
		/// <param name="useDeclaringType">TODO</param>
		/// <returns>The member ID</returns>
		public static string GetMemberID(EventInfo eventInfo, bool useDeclaringType)
		{
			return "E:" + GetFullNamespaceName(eventInfo, useDeclaringType) +
				   "." + eventInfo.Name.Replace('.', '#').Replace('+', '#');
		}

		/// <summary>
		/// Get the types namespace name (full name)
		/// </summary>
		/// <param name="type">The type</param>
		/// <returns>The namespace name (full name)</returns>
		private static string GetTypeNamespaceName(Type type)
		{
			type = DereferenceType(type);

			if (type.GetGenericArguments().Length > 0 && type.GetGenericTypeDefinition() != typeof(Nullable<>))
				return type.GetGenericTypeDefinition().FullName.Replace('+', '.');
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				return type.GetGenericArguments()[0].FullName.Replace('+', '.');
			if (type.IsGenericParameter)
				return type.Name;
			// seems to happen
			string fullName = type.FullName;
			if (type.FullName == null)
			{
				fullName = type.Namespace + "." + type.Name;
			}
			return fullName.Replace('+', '.');
		}

		/// <summary>
		/// Returns the underlying element type in case of reference or array types.
		/// </summary>
		public static Type DereferenceType(Type type)
		{
			if (type.IsByRef || type.IsArray)
			{
				type = type.GetElementType();
				return DereferenceType(type);
			}

			return type;
		}

		/// <summary>
		/// Returns the declaring type name of a member
		/// </summary>
		/// <param name="member">The member</param>
		/// <returns>The declaring type name</returns>
		public static string GetDeclaringTypeName(MemberInfo member)
		{
			return GetTypeNamespaceName(DereferenceType(member.DeclaringType));
		}

		/// <summary>
		/// Returns the name of the assembly declaring this member
		/// </summary>
		/// <param name="member">The member</param>
		/// <returns>The declaring's assembly name</returns>
		public static string GetDeclaringAssemblyName(MemberInfo member)
		{
			return DereferenceType(member.DeclaringType).Assembly.GetName().Name;
		}

		/// <summary>
		/// Returns the full namespace name of a member
		/// </summary>
		/// <param name="member">The member</param>
		/// <param name="useDeclaringType">whether to generate the name using the <see cref="MemberInfo.ReflectedType"/> or <see cref="MemberInfo.DeclaringType"/></param>
		/// <returns>The full namespace name</returns>
		private static string GetFullNamespaceName(MemberInfo member, bool useDeclaringType)
		{
			return GetTypeNamespaceName(DereferenceType( useDeclaringType?member.DeclaringType:member.ReflectedType ));
		}

		/// <summary>
		/// Returns the language-agnostic name of the given type
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static string GetTypeDisplayName(Type t)
		{
			return GetTypeName(t);
		}

		/// <summary>
		/// Get the type name of a type
		/// </summary>
		/// <param name="type">The type</param>
		/// <returns>The type name</returns>
		public static string GetTypeName(Type type)
		{
			return GetTypeName(type, false);
		}

		/// <summary>
		/// Returns the type name
		/// </summary>
		/// <param name="type">The type</param>
		/// <param name="UsePositionalNumber"></param>
		/// <returns>The type name</returns>
		public static string GetTypeName(Type type, bool UsePositionalNumber)
		{
			//TODO Nullable type
			string result = "";
			if (type.GetGenericArguments().Length > 0)
			{
				string typeNamespace = null;
				typeNamespace = GetTypeNamespace(type);
				if (!string.IsNullOrEmpty(typeNamespace) )
				{
					typeNamespace += ".";
				}
				//************ end of hack *************************

				string typeName;
				string typeBounds = String.Empty;
				int lastSquareBracket = type.Name.LastIndexOf("[");
				if (lastSquareBracket > -1)
				{
					typeName = type.Name.Substring(0, lastSquareBracket);
					typeBounds = type.Name.Substring(lastSquareBracket);
					typeBounds = typeBounds.Replace(",", ",0:").Replace("[,", "[0:,");
				}
				else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					Type[] types = type.GetGenericArguments();
					typeName = types[0].Name;
					type = types[0];
				}
				else
				{
					typeName = type.Name;
				}

				int genParmCountPos = typeName.IndexOf("`");
				if (genParmCountPos > -1)
					typeName = typeName.Substring(0, genParmCountPos);

				result = String.Concat(typeNamespace, typeName, GetTypeArgumentsList(type), typeBounds);
			}
			else
			{
				if (type.ContainsGenericParameters)
				{
					if (type.HasElementType)
					{
						Type eleType = type.GetElementType();
						if (UsePositionalNumber)
						{
							result = "`" + eleType.GenericParameterPosition;
						}
						else
						{
							result = eleType.Name;
						}

						if (type.IsArray)
						{
							int rank = type.GetArrayRank();
							result += "[";
							if (rank > 1)
							{
								int i = 0;
								while (i < rank)
								{
									if (i > 0)
										result += ",";
									result += "0:";
									i++;
								}
							}
							result += "]";
						}
						else if (type.IsByRef)
						{
							result += "@";
						}
						else if (type.IsPointer)
						{
							result += "*";
						}
					}
					else
					{
						if (UsePositionalNumber)
						{
							result = "`" + type.GenericParameterPosition;
						}
						else
						{
							result = type.Name;
						}
					}
				}
				else
				{
					result = type.FullName.Replace("&", "").Replace('+', '.');
				}
			}
			return result;
		}

		/// <summary>
		/// Gets the type's namespace
		/// </summary>
		/// <remarks>
		/// This due to a reflection issue where type.Namespace occasionally is null.
		/// </remarks>
		public static string GetTypeNamespace(Type type)
		{
			// HACK: bug in reflection - namespace sometimes returns null
			type = DereferenceType(type);

			string typeNamespace;
			if (type.Namespace != null)
			{
				typeNamespace = type.Namespace + ".";
			}
			else
			{
				int lastDot = type.FullName.LastIndexOf(".");
				typeNamespace = lastDot > -1 ? type.FullName.Substring(0, lastDot + 1) : string.Empty;
			}
			return typeNamespace.TrimEnd('.');
		}

		/// <summary>
		/// Get the generic argument list of a type
		/// </summary>
		/// <param name="type">The type</param>
		/// <returns>The generic argument list</returns>
		private static string GetTypeArgumentsList(Type type)
		{
			StringBuilder argList = new StringBuilder();
			int i = 0;

			foreach (Type argType in type.GetGenericArguments())
			{
				if (i == 0)
				{
					argList.Append('{');
				}
				else
				{
					argList.Append(',');
				}

				if (argType.GetGenericArguments().Length > 0 | argType.HasElementType)
				{
					argList.Append(GetTypeName(argType));
				}
				else if (argType.ContainsGenericParameters)
				{
					argList.Append(argType.Name);
				}
				else
				{
					argList.Append(argType.FullName);
				}

				++i;
			}

			if (i > 0)
			{
				argList.Append('}');
			}

			// XML Documentation file appends a "@" to reference and out types, not a "&"
			argList.Replace('&', '@');
			argList.Replace('+', '.');

			return argList.ToString();
		}

		/// <summary>
		/// Return a string representation of method parameters
		/// </summary>
		/// <param name="method">The method</param>
		/// <returns>String representation of the method parameters</returns>
		public static string GetParameterList(MethodBase method)
		{
			ParameterInfo[] parameters = method.GetParameters();
			StringBuilder paramList = new StringBuilder();

			int i = 0;

			foreach (ParameterInfo parameter in parameters)
			{
				if (i == 0)
				{
					paramList.Append('(');
				}
				else
				{
					paramList.Append(',');
				}

				Type paramType = parameter.ParameterType;
				if (paramType.IsByRef)
				{
					paramType = paramType.GetElementType();
				}
				while (paramType.IsArray)
					paramType = paramType.GetElementType();

				string paramName = GetParamTypeName(method, paramType);
				paramList.Append(paramName);

				paramType = parameter.ParameterType;
				if (paramType.IsByRef)
				{
					paramType = paramType.GetElementType();
				}
				while (paramType.IsArray)
				{
					paramList.Append('[');
					if (paramType.GetArrayRank() > 1)
					{
						List<string> dims = new List<string>();
						for(int c=0;c<paramType.GetArrayRank();c++)
						{
							dims.Add("0:");
						}
						paramList.Append(string.Join( ",", dims.ToArray() ));
					}
					paramList.Append(']');
					paramType = paramType.GetElementType();
				}

				if (parameter.ParameterType.IsByRef)
				{
					paramList.Append('@');
				}
				++i;
			}

			if (i > 0)
			{
				paramList.Append(')');
			}

			//			if (method.ContainsGenericParameters)
			//				paramList.Replace("`", "``");

			return paramList.ToString();
		}

		private static string GetParamTypeName(MethodBase method, Type paramType)
		{
			string paramName = null;
			Type[] typeGenericArgs = (method.DeclaringType.IsGenericType)
										? method.DeclaringType.GetGenericArguments()
										: new Type[0];
			Type[] methodGenericArgs = method.IsGenericMethod
										? method.GetGenericArguments()
										: new Type[0];

			if (paramType.ContainsGenericParameters)
			{
				// class type arg?
				for (int ix = 0; ix < typeGenericArgs.Length; ix++)
				{
					if (typeGenericArgs[ix] == paramType)
					{
						paramName = "`" + ix;
						break;
					}
				}
				if (paramName == null)
				{
					// method type arg?
					for (int ixm = 0; ixm < methodGenericArgs.Length; ixm++)
					{
						if (methodGenericArgs[ixm] == paramType)
						{
							paramName = "``" + ixm;
							break;
						}
					}
				}
				if (paramName == null)
				{
					// HACK: FullName sometimes seems null - wtf...
					paramName = GetParamTypeFullName(paramType);
					Type[] paramTypeArgs = paramType.GetGenericArguments();
					if (paramTypeArgs.Length > 0)
					{
						paramName += "{";
						for (int ixp = 0; ixp < paramTypeArgs.Length; ixp++)
						{
							paramName += GetParamTypeName(method, paramTypeArgs[ixp]);
							if (ixp < paramTypeArgs.Length - 1)
								paramName += ",";
						}
						paramName += "}";
					}
				}
			}
			else
			{
				paramName = GetTypeName(paramType, false);
			}
			return paramName;
		}

		private static string GetParamTypeFullName(Type paramType)
		{
			string paramName;
			if (paramType.FullName == null)
			{
				string name = string.Empty;
				if (paramType.IsNested)
				{
					name += paramType.DeclaringType.Name + ".";
				}
				name += paramType.Name;
				int ix2 = name.IndexOf('`');
				if (ix2 > -1)
				{
					name = name.Substring(0, ix2);
				}
				paramName = paramType.Namespace + "." + name;
			}
			else
			{
				paramName = paramType.FullName;
			}
			return paramName;
		}
	}
}