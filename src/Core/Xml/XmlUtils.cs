using System;
using System.Xml;

namespace NDoc3.Xml
{
	///<summary>
	///</summary>
	public sealed class XmlUtils
	{
		///<summary>
		/// Get the node id
		///</summary>
		///<param name="node">The node</param>
		///<returns>Node id</returns>
		public static string GetNodeId(XmlNode node)
		{
			return GetAttributeString(node, "id", true);
		}

		///<summary>
		/// Gets the node type
		///</summary>
		///<param name="node">The node</param>
		///<returns>The node type</returns>
		public static string GetNodeType(XmlNode node)
		{
			return GetAttributeString(node, "type", true);
		}

		///<summary>
		/// Gets the node type id
		///</summary>
		///<param name="node">The node</param>
		///<returns>The node type id</returns>
		public static string GetNodeTypeId(XmlNode node)
		{
			return GetAttributeString(node, "typeId", true);
		}

		///<summary>
		/// Gets the node name
		///</summary>
		///<param name="node">The node</param>
		///<returns>The node name</returns>
		public static string GetNodeName(XmlNode node)
		{
			return GetAttributeString(node, "name", true);
		}

		///<summary>
		/// Gets the node display name
		///</summary>
		///<param name="typeNode">The node</param>
		///<returns>The node display name</returns>
		///<remarks>Replace {} with (), to support multiple language, just like MSDN does.</remarks>
		public static string GetNodeDisplayName(XmlNode typeNode)
		{
			return GetAttributeString(typeNode, "displayName", true).Replace('{', '(').Replace('}', ')');
		}

		///<summary>
		/// Get attribute enumeration value
		///</summary>
		///<param name="node">The node</param>
		///<param name="attributeName">The attribute name</param>
		///<typeparam name="TVal">The enumeration type</typeparam>
		///<returns>The enumeration value</returns>
		///<exception cref="ArgumentException">Throws an exception if the attribute doesn't exists in the enumeration</exception>
		public static TVal GetAttributeEnum<TVal>(XmlNode node, string attributeName)
		{
			XmlAttribute attribute = node.Attributes[attributeName];
			if (attribute == null) {
				throw new ArgumentException(string.Format("Required attribute {0} not found on node {1}: {2}", attributeName, node.Name, node.OuterXml));
			}
			return (TVal)Enum.Parse(typeof(TVal), attribute.Value);
		}

		///<summary>
		/// Gets attribute enumeration value, if the value doesn't exists a default value are supplied
		///</summary>
		///<param name="node">The node</param>
		///<param name="attributeName">The attribute name</param>
		///<param name="defaultValue">The default value</param>
		///<typeparam name="TVal">The enumeration value</typeparam>
		///<returns>Enumeration value</returns>
		public static TVal GetAttributeEnum<TVal>(XmlNode node, string attributeName, TVal defaultValue)
		{
			XmlAttribute attribute = node.Attributes[attributeName];
			if (attribute == null) {
				return defaultValue;
			}
			return (TVal)Enum.Parse(typeof(TVal), attribute.Value);
		}

		///<summary>
		/// Gets attribute string
		///</summary>
		///<param name="node">The node</param>
		///<param name="attributeName">The attribute name</param>
		///<param name="required">Indicates if the attribute are required to exists</param>
		///<returns>The attribute string</returns>
		///<exception cref="ArgumentException">An exception are thrown if the attribute requested are indicated as required</exception>
		public static string GetAttributeString(XmlNode node, string attributeName, bool required)
		{
			string attributeString = null;
			XmlAttribute attribute = node.Attributes[attributeName];
			if (attribute == null && required) {
				throw new ArgumentException(string.Format("Required attribute {0} not found on node {1}: {2}", attributeName, node.Name, node.OuterXml));
			}
			if (attribute != null) {
				attributeString = attribute.Value;
			}
			return attributeString;
		}
	}
}