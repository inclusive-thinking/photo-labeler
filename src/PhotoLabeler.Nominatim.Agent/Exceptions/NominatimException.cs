// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace PhotoLabeler.Nominatim.Agent.Exceptions
{

	/// <summary>
	/// Represents an error when querying nominatim openmap API
	/// </summary>
	/// <seealso cref="System.Exception" />
	[Serializable]
	public class NominatimException : Exception
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="NominatimException"/> class.
		/// </summary>
		public NominatimException() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="NominatimException"/> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public NominatimException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="NominatimException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public NominatimException(string message, Exception inner) : base(message, inner) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="NominatimException" /> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
		protected NominatimException(

		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
