// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using PhotoLabeler.Entities;

namespace PhotoLabeler.Data.Interfaces
{
	/// <summary>
	/// Interface to abstract the GeolocationRepository methods
	/// </summary>
	public interface IGeolocationRepository
	{

		/// <summary>
		/// Adds the geolocation asynchronous.
		/// </summary>
		/// <param name="geolocation">The geolocation.</param>
		/// <returns></returns>
		Task AddGeolocationAsync(Geolocation geolocation);

		/// <summary>
		/// Deletes the geolocation asynchronous.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		Task DeleteGeolocationAsync(int id);

		/// <summary>
		/// Edits the geolocation asynchronous.
		/// </summary>
		/// <param name="geolocation">The geolocation.</param>
		/// <returns></returns>
		Task EditGeolocationAsync(Geolocation geolocation);

		/// <summary>
		/// Gets all geolocations asynchronous.
		/// </summary>
		/// <returns></returns>
		Task<IEnumerable<Geolocation>> GetAllGeolocationsAsync();

		/// <summary>
		/// Gets the geolocation by coordinates asynchronous.
		/// </summary>
		/// <param name="latitude">The latitude.</param>
		/// <param name="longitude">The longitude.</param>
		/// <returns></returns>
		Task<Geolocation> GetGeolocationByCoordinatesAsync(double latitude, double longitude);

		/// <summary>
		/// Gets the geolocations by coordinates list asynchronous.
		/// </summary>
		/// <param name="points">The points.</param>
		/// <returns></returns>
		Task<IEnumerable<Geolocation>> GetGeolocationsByCoordinatesListAsync(List<GeolocationPoint> points);
	}
}
