// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace PhotoLabeler.Nominatim.Agent.Entities
{
	public class Address
	{
		[JsonProperty("house_number")]
		public string HouseNumber { get; set; }

		[JsonProperty("road")]
		public string Road { get; set; }

		[JsonProperty("neighbourhood")]
		public string Neighbourhood { get; set; }

		[JsonProperty("city_district")]
		public string CityDistrict { get; set; }

		[JsonProperty("city")]
		public string City { get; set; }

		[JsonProperty("municipality")]
		public string Municipality { get; set; }

		[JsonProperty("county")]
		public string County { get; set; }

		[JsonProperty("state")]
		public string State { get; set; }

		[JsonProperty("postcode")]
		public string Postcode { get; set; }

		[JsonProperty("country")]
		public string Country { get; set; }

		[JsonProperty("country_code")]
		public string CountryCode { get; set; }
	}
}
