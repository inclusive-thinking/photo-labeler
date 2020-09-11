// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoLabeler.Entities
{
	/// <summary>
	/// Represents a geolocation point
	/// </summary>
	public class GeolocationPoint
	{

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

		public override bool Equals(object obj)
		{
			return Equals(obj as GeolocationPoint);
		}

		public bool Equals(GeolocationPoint other)
		{
			return other != null &&
				   Latitude == other.Latitude &&
				   Longitude == other.Longitude;
		}

		public override int GetHashCode()
		{
			var hashCode = -1416534245;
			hashCode = hashCode * -1521134295 + Latitude.GetHashCode();
			hashCode = hashCode * -1521134295 + Longitude.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(GeolocationPoint left, GeolocationPoint right)
		{
			return EqualityComparer<GeolocationPoint>.Default.Equals(left, right);
		}

		public static bool operator !=(GeolocationPoint left, GeolocationPoint right)
		{
			return !(left == right);
		}
	}
}
