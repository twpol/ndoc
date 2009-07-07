// MemberDisplayName.cs
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
using System.Text;

namespace NDoc3.Core {
	/// <summary>
	/// 
	/// </summary>
	public static class MemberDisplayName {
		/// <summary>
		/// 
		/// </summary>
		/// <param name="realType"></param>
		/// <returns></returns>
		public static string GetMemberDisplayName(Type realType) {
			if (realType.IsByRef) {
				realType = realType.GetElementType();
			}

			Type type = DereferenceType(realType);
			string result;
			if (type.DeclaringType != null) //IsNested?
			{
				result = GetTypeDisplayName(type);
				Type declaringType = type.DeclaringType;
				while (declaringType != null) {
					result = GetTypeDisplayName(declaringType) + "." + result;
					declaringType = declaringType.DeclaringType;
				}
			} else {
				result = GetTypeDisplayName(type);
			}

			// append array indexer/byRef indicator
			if (realType.IsArray) {
				string suffix = realType.Name.Substring(type.Name.Length);
				result += suffix;
			}
			return result;
		}

		private static Type DereferenceType(Type type) {
			if (NeedsDereference(type)) {
				type = type.GetElementType();
				return DereferenceType(type);
			}

			return type;
		}

		private static bool NeedsDereference(Type type) {
			return type.IsArray
				|| type.IsByRef
				;
		}

		private static string GetTypeDisplayName(Type type) {
			if (type.IsGenericType) {
				int i = type.Name.IndexOf('`');
				string result = i > -1 ? type.Name.Substring(0, type.Name.IndexOf('`')) : type.Name;
				result += GetTypeArgumentsList(type);
				return result;
			}
			return type.Name;
		}

		private static string GetTypeArgumentsList(Type type) {
			StringBuilder argList = new StringBuilder();

			int genArgLowerBound = 0;
			if (type.IsNested) {
				Type parent = type.DeclaringType;
				Type[] parentGenArgs = parent.GetGenericArguments();
				genArgLowerBound = parentGenArgs.Length;
			}

			Type[] genArgs = type.GetGenericArguments();
			int i = 0;
			for (int k = genArgLowerBound; k < genArgs.Length; k++) {
				Type argType = genArgs[k];
				if (i == 0) {
					argList.Append('(');
				} else {
					argList.Append(',');
				}
				if (argType.FullName == null) {
					if (type.IsGenericType && !type.IsGenericTypeDefinition) {
						Type[] types = type.GetGenericArguments();
						foreach (Type t in types)
							argList.Append(GetTypeDisplayName(t));
					} else
						argList.Append(argType.Name);
				} else {
					argList.Append(GetMemberDisplayName(argType));
				}

				++i;
			}

			if (i > 0) {
				argList.Append(')');
			}

			return argList.ToString();
		}
	}
}
