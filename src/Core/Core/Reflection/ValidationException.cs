using System;

namespace NDoc3.Core.Reflection {
	/// <summary>
	/// Exception class used if validation of xml fails.
	/// </summary>
	[Serializable]
	public class ValidationException : Exception {
		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationException"/> class.
		/// </summary>
		public ValidationException() { }
		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public ValidationException(string message) : base(message) { }
		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public ValidationException(string message, Exception inner) : base(message, inner) { }
		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationException"/> class.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data.</param>
		/// <param name="context">The contextual information about the source or destination.</param>
		protected ValidationException(
			 System.Runtime.Serialization.SerializationInfo info,
			 System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}