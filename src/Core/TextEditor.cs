using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace NDoc.Core
{
	/// <summary>
	/// Provides editing facilities for large blocks of text in the <see cref="PropertyGrid"/>.
	/// </summary>
	public class TextEditor : UITypeEditor
	{
		/// <summary>
		/// Creates a new instance of the <see cref="TextEditor"/> class.
		/// </summary>
		public TextEditor()
		{
		}

		/// <summary>
		/// Edits the specified object's value using the editor style indicated by <see cref="GetEditStyle"/>.
		/// </summary>
		/// <param name="context">An <see cref="ITypeDescriptorContext"/> that can be used to gain additional context information.</param>
		/// <param name="provider">An <see cref="IServiceProvider"/> that this editor can use to obtain services.</param>
		/// <param name="value">The object to edit.</param>
		/// <returns>The new value of the object.</returns>
		public override object EditValue(ITypeDescriptorContext context,
			IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null)
			{
				TextEditorForm form = new TextEditorForm();
				form.Value = (string)value;
				DialogResult result = form.ShowDialog();

				if (result == DialogResult.OK)
					value = form.Value;
			}

			return value;
		}

		/// <summary>
		/// Gets the editor style used by the <see cref="EditValue"/> method.
		/// </summary>
		/// <param name="context">An <see cref="ITypeDescriptorContext"/> that can be used to gain additional context information.</param>
		/// <returns>A <see cref="UITypeEditorEditStyle"/> value that indicates the style of editor used by <see cref="EditValue"/>.</returns>
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance != null)
				return UITypeEditorEditStyle.Modal;

			return base.GetEditStyle(context);
		}
	}
}
