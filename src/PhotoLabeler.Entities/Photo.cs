// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace PhotoLabeler.Entities
{
	/// <summary>
	/// Represents a photo
	/// </summary>
	public class Photo
	{

		/// <summary>
		/// Gets or sets the path.
		/// </summary>
		/// <value>
		/// The path.
		/// </value>
		public string Path { get; set; }

		/// <summary>
		/// Gets or sets the label.
		/// </summary>
		/// <value>
		/// The label.
		/// </value>
		public string Label { get; set; }

		/// <summary>
		/// Gets or sets the taken date.
		/// </summary>
		/// <value>
		/// The taken date.
		/// </value>
		public DateTime? TakenDate { get; set; }

		/// <summary>
		/// Gets or sets the modified date.
		/// </summary>
		/// <value>
		/// The modified date.
		/// </value>
		public DateTime? ModifiedDate { get; set; }

		/// <summary>
		/// Gets or sets the latitude.
		/// </summary>
		/// <value>
		/// The latitude.
		/// </value>
		public double? Latitude { get; set; }

		/// <summary>
		/// Gets or sets the longitude.
		/// </summary>
		/// <value>
		/// The longitude.
		/// </value>
		public double? Longitude { get; set; }

		/// <summary>
		/// Gets or sets the altitude.
		/// </summary>
		/// <value>
		/// The altitude.
		/// </value>
		public double? AltitudeInMeters { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the current location exists into database.
		/// </summary>
		/// <value>
		///   <c>true</c> if [database location exists]; otherwise, <c>false</c>.
		/// </value>
		public bool DbLocationExists { get; set; }

		/// <summary>
		/// Gets or sets the location error.
		/// </summary>
		/// <value>
		/// The location error.
		/// </value>
		public string LocationError { get; set; }

		/// <summary>
		/// Gets or sets the location information.
		/// </summary>
		/// <value>
		/// The location information.
		/// </value>
		public string LocationInfo { get; set; }

		/// <summary>
		/// Gets or sets the error associated to this entity retrieval.
		/// </summary>
		/// <value>
		/// The error.
		/// </value>
		public Exception Error { get; set; }

		/// <summary>
		/// Gets a value indicating whether this instance has errors.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance has errors; otherwise, <c>false</c>.
		/// </value>
		public bool HasErrors => Error != null;

		/// <summary>
		/// Gets a value indicating whether this instance has GPS information.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance has GPS information; otherwise, <c>false</c>.
		/// </value>
		public bool HasGPSInformation => Latitude.HasValue && Longitude.HasValue;


	}
}
