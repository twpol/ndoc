using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace NDoc.Core
{
	/// <summary>
	/// EnumConverter supporting System.ComponentModel.DescriptionAttribute
	/// </summary>
	public class EnumDescriptionConverter : System.ComponentModel.EnumConverter
	{
		private System.Type enumType;

		/// <summary>
		/// Gets the Description of the given Enumeration value 
		/// </summary>
		/// <param name="value">The enumeration value</param>
		/// <returns>The Description from the DescriptionAttribute attached to the value, otherwise the enumeration value's name</returns>
		public static string GetEnumDescription(Enum value)
		{
			FieldInfo fi = value.GetType().GetField(value.ToString());
			DescriptionAttribute[] attributes = 
				(DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
			if (attributes.Length > 0) 
			{
				return attributes[0].Description;
			}
			else
			{
				return value.ToString();
			}
		}
    
		/// <summary>
		/// Gets the Description of a named value in an Enumeration
		/// </summary>
		/// <param name="value">The type of the Enumeration</param>
		/// <param name="name">The name of the Enumeration value</param>
		/// <returns>The description, if any, else the passed name</returns>
		public static string GetEnumDescription(System.Type value, string name)
		{
			FieldInfo fi = value.GetField(name);
			DescriptionAttribute[] attributes = 
				(DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
			if (attributes.Length > 0) 
			{
				return attributes[0].Description;
			}
			else
			{
				return name;
			}
		}
    
		/// <summary>
		/// Gets the value of an Enum, based on it's Description Attribute or named value
		/// </summary>
		/// <param name="value">The Enum type</param>
		/// <param name="description">The description or name of the element</param>
		/// <returns>The value, or the passed in description, if it was not found</returns>
		public static object GetEnumValue(System.Type value, string description)
		{
			FieldInfo[] fis = value.GetFields();
			foreach (FieldInfo fi in fis) 
			{
				DescriptionAttribute[] attributes = 
					(DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
				if (attributes.Length > 0) 
				{
					if (attributes[0].Description == description)
					{
						return fi.GetValue(fi.Name);
					}
				}
				if (fi.Name == description)
				{
					return fi.GetValue(fi.Name);
				}
			}
			return description;
		}

		/// <summary>
		/// Constructs EnumDescriptionConverter for a given Enum
		/// </summary>
		/// <param name="type"></param>
		public EnumDescriptionConverter(System.Type type) : base(type)
		{
			enumType = type;
		}

		/// <summary>
		/// <para>Converts the given value object to the specified type, using the specified context and culture information.</para>
		/// <para>This member overrides <see cref="TypeConverter.ConvertTo(ITypeDescriptorContext, CultureInfo, object, Type)"/>.</para>
		/// </summary>
		/// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">A <see cref="CultureInfo"/> object. If a <see langword="null"/> is passed, the current culture is assumed.</param>
		/// <param name="value">The <see cref="Object"/> to convert.</param>
		/// <param name="destinationType">The <see cref="Type"/> to convert the <paramref name="value"/> parameter to.</param>
		/// <returns>An <see cref="Object"/> that represents the converted value.</returns>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{

			if (destinationType == typeof(string)) 
			{
				//enum value => Description
				if (value is Enum) 
				{
					return GetEnumDescription((Enum)value);
				}

				//enum named value => Description
				if (value is string) 
				{
					return GetEnumDescription(enumType, (string)value);
				}
			}

			//default to EnumConverter behavior
			return base.ConvertTo(context, culture, value, destinationType);
		}

		/// <summary>
		/// <para>Converts the given object to the type of this converter, using the specified context and culture information.</para>
		/// <para>This member overrides <see cref="TypeConverter.ConvertFrom(ITypeDescriptorContext, CultureInfo, object)"/>.</para>
		/// </summary>
		/// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">A <see cref="CultureInfo"/> object. If a <see langword="null"/> is passed, the current culture is assumed.</param>
		/// <param name="value">The <see cref="Object"/> to convert.</param>
		/// <returns>An <see cref="Object"/> that represents the converted value.</returns>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			//Descripton or named-value => enum
			if (value is string) 
			{
				return GetEnumValue(enumType, (string)value);
			}

			//enum value => Description
			if (value is Enum) 
			{
				return GetEnumDescription((Enum)value);
			}

			//default to EnumConverter behavior
			return base.ConvertFrom(context, culture, value);
		}
	}
}
