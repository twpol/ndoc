using System;
using System.Xml;

namespace NDoc3.Xml
{
	///<summary>
	///</summary>
	public sealed class XmlUtils
	{
		public static string GetNodeId(XmlNode node)
		{
			return XmlUtils.GetAttributeString(node, "id", true);
		}

		public static string GetNodeType(XmlNode node)
		{
			return XmlUtils.GetAttributeString(node, "type", true);
		}

		public static string GetNodeTypeId(XmlNode node)
		{
			return XmlUtils.GetAttributeString(node, "typeId", true);
		}

		public static string GetNodeName(XmlNode node)
		{
			return XmlUtils.GetAttributeString(node, "name", true);
		}

		public static string GetNodeDisplayName(XmlNode typeNode)
		{
			return XmlUtils.GetAttributeString(typeNode, "displayName", true);
		}

		public static TVal GetAttributeEnum<TVal>(XmlNode node, string attributeName)
		{
			XmlAttribute attribute = node.Attributes[attributeName];
			if (attribute == null) {
				throw new ArgumentException(string.Format("Required attribute {0} not found on node {1}: {2}", attributeName, node.Name, node.OuterXml));
			}
			return (TVal)Enum.Parse(typeof(TVal), attribute.Value);
		}

		public static TVal GetAttributeEnum<TVal>(XmlNode node, string attributeName, TVal defaultValue)
		{
			XmlAttribute attribute = node.Attributes[attributeName];
			if (attribute == null) {
				return defaultValue;
			}
			return (TVal)Enum.Parse(typeof(TVal), attribute.Value);
		}

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