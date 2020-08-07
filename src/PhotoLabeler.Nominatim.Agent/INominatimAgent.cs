// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using PhotoLabeler.Nominatim.Agent.Entities;

namespace PhotoLabeler.Nominatim.Agent
{
	public interface INominatimAgent
	{
		Task<ReverseGeocodeResult> ReverseGeocodeAsync(ReverseGeocodeRequest request, CancellationToken cancellationToken = default);
	}
}