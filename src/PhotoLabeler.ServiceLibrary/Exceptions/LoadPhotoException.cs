// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Runtime.Serialization;

namespace PhotoLabeler.ServiceLibrary.Exceptions
{
	/// <summary>
	/// Exception thrown when an error is produced while loading a photo
	/// </summary>
	/// <seealso cref="Exception" />
	[Serializable]
	public class LoadPhotoException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LoadPhotoException"/> class.
		/// </summary>
		public LoadPhotoException() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="LoadPhotoException"/> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public LoadPhotoException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="LoadPhotoException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="path">The path.</param>
		/// <param name="inner">The inner.</param>
		public LoadPhotoException(string message, string path, Exception inner) : base(message, inner)
		{
			Path = path;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoadPhotoException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
		protected LoadPhotoException(
		  SerializationInfo info,
		  StreamingContext context) : base(info, context) { }

		public string Path { get; set; }

	}
}
