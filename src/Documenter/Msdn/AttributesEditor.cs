using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.ComponentModel;
using System.Windows.Forms.Design;

namespace NDoc.Documenter.Msdn
{
	/// <summary>
	/// Class which implements a custom UITypeEditor for attributes.
	/// </summary>
	public class AttributesEditor : System.Drawing.Design.UITypeEditor 
	{
		/// <summary>
		/// Handler called when editing a value.
		/// </summary>
		/// <param name="context">Context</param>
		/// <param name="provider">Provider</param>
		/// <param name="value">Current Value</param>
		/// <returns>New value</returns>
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) 
		{
			if (context != null
				&& context.Instance != null
				&& provider != null) 
			{
				AttributesForm dlg = new AttributesForm(value);
				if(dlg.ShowDialog() == DialogResult.OK)
				{
					return dlg.Value;
				}
			}

			return value;
		}

		/// <summary>
		/// Returns the edit style for the type.
		/// </summary>
		/// <param name="context">Context</param>
		/// <returns>Edit Style</returns>
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) 
		{
			if (context != null && context.Instance != null) 
			{
				return UITypeEditorEditStyle.Modal;
			}
			return base.GetEditStyle(context);
		}
	}
}
