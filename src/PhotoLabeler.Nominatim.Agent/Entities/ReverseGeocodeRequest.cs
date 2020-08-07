// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

namespace PhotoLabeler.Nominatim.Agent.Entities
{
	public class ReverseGeocodeRequest
	{

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public string Language { get; set; }

	}
}
