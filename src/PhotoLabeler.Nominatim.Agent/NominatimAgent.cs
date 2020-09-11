// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using PhotoLabeler.Nominatim.Agent.Entities;
using PhotoLabeler.Nominatim.Agent.Exceptions;

namespace PhotoLabeler.Nominatim.Agent
{
	public class NominatimAgent : INominatimAgent
	{

		private readonly NominatimAgentConfig _config;

		private readonly HttpClient _client;

		public NominatimAgent(NominatimAgentConfig config, HttpClient client)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
			_client = client ?? throw new ArgumentNullException(nameof(client));
		}

		public async Task<ReverseGeocodeResult> ReverseGeocodeAsync(ReverseGeocodeRequest request, CancellationToken cancellationToken = default)
		{
			if (request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			return await ReverseGeocodeInternalAsync(request, cancellationToken);
		}

		private async Task<ReverseGeocodeResult> ReverseGeocodeInternalAsync(ReverseGeocodeRequest request, CancellationToken cancellationToken)
		{
			var enUsCulture = CultureInfo.CreateSpecificCulture("en-US");
			var uri = $"{_config.UriBase}/reverse?" +
				$"lat={HttpUtility.UrlEncode(request.Latitude.ToString(enUsCulture))}" +
				$"&lon={HttpUtility.UrlEncode(request.Longitude.ToString(enUsCulture))}" +
				$"&accept-language={HttpUtility.UrlEncode(request.Language)}&format=json";
			var message = new HttpRequestMessage();
			message.RequestUri = new Uri(uri);
			message.Headers.Add("User-Agent", "Photo-Labeler");
			message.Headers.Add("Accept", "application/json");
			message.Method = HttpMethod.Get;
			var result = await _client.SendAsync(message, cancellationToken);
			result.EnsureSuccessStatusCode();
			var content = await result.Content.ReadAsStringAsync();
			var reverseGeocodeResult = JsonConvert.DeserializeObject<ReverseGeocodeResult>(content);
			if (reverseGeocodeResult.HasErrors)
			{
				throw new NominatimException(reverseGeocodeResult.Error);
			}
			return reverseGeocodeResult;
		}
	}
}
