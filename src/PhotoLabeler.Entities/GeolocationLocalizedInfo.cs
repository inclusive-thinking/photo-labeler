// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

namespace PhotoLabeler.Entities
{
	/// <summary>
	/// Represents the specific geolocation data for specific language
	/// </summary>
	public class GeolocationLocalizedInfo
	{

		/// <summary>
		/// Gets or sets the geolocation.
		/// </summary>
		/// <value>
		/// The geolocation.
		/// </value>
		public Geolocation Geolocation { get; set; }

		/// <summary>
		/// Gets or sets the language.
		/// </summary>
		/// <value>
		/// The language.
		/// </value>
		public string Language { get; set; }

		/// <summary>
		/// Gets or sets the location.
		/// </summary>
		/// <value>
		/// The location.
		/// </value>
		public string Location { get; set; }
	}
}
