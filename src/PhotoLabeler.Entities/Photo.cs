// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;

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
		/// Gets or sets the location desc.
		/// </summary>
		/// <value>
		/// The location desc.
		/// </value>
		public string LocationDesc { get; set; }

		/// <summary>
		/// Gets or sets the MD5 sum.
		/// </summary>
		/// <value>
		/// The MD5 sum.
		/// </value>
		public string Md5Sum { get; set; }

	}
}
