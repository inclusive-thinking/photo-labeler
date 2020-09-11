// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace PhotoLabeler.Nominatim.Agent.Entities
{
	public class ReverseGeocodeResult
	{
		[JsonProperty("place_id")]
		public int PlaceId { get; set; }

		[JsonProperty("licence")]
		public string Licence { get; set; }

		[JsonProperty("osm_type")]
		public string OsmType { get; set; }

		[JsonProperty("osm_id")]
		public long OsmId { get; set; }

		[JsonProperty("lat")]
		public double Lat { get; set; }

		[JsonProperty("lon")]
		public double lon { get; set; }

		[JsonProperty("display_name")]
		public string DisplayName { get; set; }

		[JsonProperty("address")]
		public Address Address { get; set; }

		[JsonProperty("boundingbox")]
		public List<string> BoundingBox { get; set; }

		[JsonProperty("error")]
		public string Error { get; set; }

		public bool HasErrors => !string.IsNullOrWhiteSpace(Error);

	}
}
