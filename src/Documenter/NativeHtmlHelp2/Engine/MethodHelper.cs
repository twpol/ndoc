// MsdnDocumenter.cs - a MSDN-like documenter
// Copyright (C) 2003 Don Kackman
// Parts copyright 2001  Kral Ferch, Jason Diamond
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
using System.Xml;

namespace NDoc.Documenter.NativeHtmlHelp2.Engine
{
	/// <summary>
	/// Helper functions to get information about a method
	/// </summary>
	public class MethodHelper
	{
		/// <summary>
		/// Determines if an overload exists
		/// </summary>
		/// <param name="methodNodes">The list of methods</param>
		/// <param name="indexes"></param>
		/// <param name="index"></param>
		/// <returns>True if no overload exists</returns>
		public static bool IsMethodAlone(XmlNodeList methodNodes, int[] indexes, int index)
		{
			string name = methodNodes[indexes[index]].Attributes["name"].Value;
			int lastIndex = methodNodes.Count - 1;
			if (lastIndex <= 0)
				return true;
			bool previousNameDifferent = (index == 0)
				|| (methodNodes[indexes[index - 1]].Attributes["name"].Value != name);
			bool nextNameDifferent = (index == lastIndex)
				|| (methodNodes[indexes[index + 1]].Attributes["name"].Value != name);
			return (previousNameDifferent && nextNameDifferent);
		}

		/// <summary>
		/// Determines if a method is the first overload
		/// </summary>
		/// <param name="methodNodes"></param>
		/// <param name="indexes"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool IsMethodFirstOverload(XmlNodeList methodNodes, int[] indexes, int index)
		{
			if ((methodNodes[indexes[index]].Attributes["declaringType"] != null)
				|| IsMethodAlone(methodNodes, indexes, index))
				return false;

			string name			= methodNodes[indexes[index]].Attributes["name"].Value;
			string previousName	= GetPreviousMethodName(methodNodes, indexes, index);
			return previousName != name;
		}

		/// <summary>
		/// Determines if a method is the alst overload
		/// </summary>
		/// <param name="methodNodes"></param>
		/// <param name="indexes"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool IsMethodLastOverload(XmlNodeList methodNodes, int[] indexes, int index)
		{
			if ((methodNodes[indexes[index]].Attributes["declaringType"] != null)
				|| IsMethodAlone(methodNodes, indexes, index))
				return false;

			string name		= methodNodes[indexes[index]].Attributes["name"].Value;
			string nextName	= GetNextMethodName(methodNodes, indexes, index);
			return nextName != name;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="methodNodes"></param>
		/// <param name="indexes"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static string GetPreviousMethodName(XmlNodeList methodNodes, int[] indexes, int index)
		{
			while ( --index >= 0 )
			{
				if (methodNodes[indexes[index]].Attributes["declaringType"] == null)
					return methodNodes[indexes[index]].Attributes["name"].Value;
			}
			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="methodNodes"></param>
		/// <param name="indexes"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static string GetNextMethodName(XmlNodeList methodNodes, int[] indexes, int index)
		{
			while (++index < methodNodes.Count)
			{
				if (methodNodes[indexes[index]].Attributes["declaringType"] == null)
					return methodNodes[indexes[index]].Attributes["name"].Value;
			}
			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameterNodes"></param>
		/// <returns></returns>
		public static string GetParamList(XmlNodeList parameterNodes)
		{
			int numberOfNodes = parameterNodes.Count;
			int nodeIndex = 1;
			string paramList = "(";

			foreach (XmlNode parameterNode in parameterNodes)
			{
				paramList += StripNamespace(parameterNode.Attributes["type"].Value);

				if (nodeIndex < numberOfNodes)				
					paramList += ", ";				

				nodeIndex++;
			}

			paramList += ")";

			return paramList;
		}

		/// <summary>
		/// Determines a human readable name for an operator method
		/// </summary>
		/// <param name="operatorNode">The operator Xml</param>
		/// <returns>Operator name</returns>
		public static string GetOperatorName( XmlNode operatorNode )
		{
			string name = operatorNode.Attributes["name"].Value;

			switch (name)
			{
				case "op_UnaryPlus":
					return "Unary Plus Operator";
				case "op_UnaryNegation":
					return "Unary Negation Operator";
				case "op_LogicalNot":
					return "Logical Not Operator";
				case "op_OnesComplement":
					return "Ones Complement Operator";
				case "op_Increment":
					return "Increment Operator";
				case "op_Decrement":
					return "Decrement Operator";
				case "op_True":
					return "True Operator";
				case "op_False":
					return "False Operator";
				case "op_Addition":
					return "Addition Operator";
				case "op_Subtraction":
					return "Subtraction Operator";
				case "op_Multiply":
					return "Multiplication Operator";
				case "op_Division":
					return "Division Operator";
				case "op_Modulus":
					return "Modulus Operator";
				case "op_BitwiseAnd":
					return "Bitwise And Operator";
				case "op_BitwiseOr":
					return "Bitwise Or Operator";
				case "op_ExclusiveOr":
					return "Exclusive Or Operator";
				case "op_LeftShift":
					return "Left Shift Operator";
				case "op_RightShift":
					return "Right Shift Operator";
				case "op_Equality":
					return "Equality Operator";
				case "op_Inequality":
					return "Inequality Operator";
				case "op_LessThan":
					return "Less Than Operator";
				case "op_GreaterThan":
					return "Greater Than Operator";
				case "op_LessThanOrEqual":
					return "Less Than Or Equal Operator";
				case "op_GreaterThanOrEqual":
					return "Greater Than Or Equal Operator";
				case "op_Explicit":
					XmlNode parameterNode = operatorNode.SelectSingleNode("parameter");
					string from = parameterNode.Attributes["type"].Value;
					string to = operatorNode.Attributes["returnType"].Value;
					return "Explicit " + MethodHelper.StripNamespace( from ) + " to " + MethodHelper.StripNamespace( to ) + " Conversion";
				case "op_Implicit":
					XmlNode parameterNode2 = operatorNode.SelectSingleNode("parameter");
					string from2 = parameterNode2.Attributes["type"].Value;
					string to2 = operatorNode.Attributes["returnType"].Value;
					return "Implicit " + MethodHelper.StripNamespace( from2 ) + " to " + MethodHelper.StripNamespace( to2 ) + " Conversion";
				default:
					return "ERROR";
			}
		}

		private static string StripNamespace(string name)
		{
			string result = name;

			int lastDot = name.LastIndexOf('.');

			if (lastDot != -1)
			{
				result = name.Substring(lastDot + 1);
			}

			return result;
		}
	}
}
