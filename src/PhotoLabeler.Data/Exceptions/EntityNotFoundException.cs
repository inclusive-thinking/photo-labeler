// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace PhotoLabeler.Data.Exceptions
{

	[Serializable]
	public class EntityNotFoundException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
		/// </summary>
		public EntityNotFoundException() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public EntityNotFoundException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public EntityNotFoundException(string message, Exception inner) : base(message, inner) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
		protected EntityNotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
