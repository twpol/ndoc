using System;
using System.Collections;
using System.Xml;

namespace NDoc.Core
{
	/// <summary>Specifies the methods that are common to all documenter configs.</summary>
	public interface IDocumenterConfig
	{
		/// <summary>Gets a list of property names.</summary>
		/// <returns>An enumerable list of property names.</returns>
		IEnumerable GetProperties();

		/// <summary>Sets the value of a property.</summary>
		/// <param name="name">The name of the property.</param>
		/// <param name="value">The value of the property.</param>
		void SetValue(string name, string value);

		/// <summary>Reads the previously serialized state of the documenter into memory.</summary>
		/// <param name="reader">An XmlReader positioned on a documenter element.</param>
		/// <remarks>This method uses reflection to set all of the public properties in the documenter.</remarks>
		void Read(XmlReader reader);

		/// <summary>Writes the current state of the documenter to the specified XmlWrtier.</summary>
		/// <param name="writer">An XmlWriter.</param>
		/// <remarks>This method uses reflection to serialize all of the public properties in the documenter.</remarks>
		void Write(XmlWriter writer);
	}
}
