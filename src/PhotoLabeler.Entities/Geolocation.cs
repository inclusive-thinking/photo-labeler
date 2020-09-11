// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace PhotoLabeler.Entities
{
	/// <summary>
	/// Represents the Geolocation entity
	/// </summary>
	public class Geolocation
	{

		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		public int Id { get; set; }

		/// <summary>
		/// Gets or sets the latitude.
		/// </summary>
		/// <value>
		/// The latitude.
		/// </value>
		public double Latitude { get; set; }

		/// <summary>
		/// Gets or sets the longitude.
		/// </summary>
		/// <value>
		/// The longitude.
		/// </value>
		public double Longitude { get; set; }

		/// <summary>
		/// Gets or sets the error.
		/// </summary>
		/// <value>
		/// The error.
		/// </value>
		public string Error { get; set; }

		/// <summary>
		/// Gets or sets the localized information.
		/// </summary>
		/// <value>
		/// The localized information.
		/// </value>
		public List<GeolocationLocalizedInfo> LocalizedInfo { get; set; } = new List<GeolocationLocalizedInfo>();

		public override string ToString() => $"Latitude: {Latitude}. Longitude: {Longitude}";
	}
}
