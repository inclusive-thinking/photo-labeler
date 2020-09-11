// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using PhotoLabeler.Data.Exceptions;
using PhotoLabeler.Data.Interfaces;
using PhotoLabeler.Entities;

namespace PhotoLabeler.Data.Repositories
{
	/// <summary>
	/// Geo repository class
	/// </summary>
	public class GeolocationRepository : RepositoryBase, IGeolocationRepository
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="GeolocationRepository"/> class.
		/// </summary>
		/// <param name="connection">The connection.</param>
		public GeolocationRepository(IDbConnection connection)
			: base(connection)
		{
			CreateTableIfDoesNotExists(
				"Geolocations",
				$"create table Geolocations(Id INTEGER NOT NULL PRIMARY KEY, Latitude REAL, Longitude REAL, Error TEXT)");
			CreateTableIfDoesNotExists(
				"GeolocationsLocalizedInfo",
				$"create table GeolocationsLocalizedInfo(GeolocationId INTEGER, Language TEXT not null, Location TEXT, primary key (GeolocationId, Language), constraint fk_GeolocationsLocalizedInfo_GeolocationId_Geolocations_Id foreign key (GeolocationId) references Geolocations (Id) ON DELETE CASCADE)");
		}


		/// <summary>
		/// Gets all geolocations asynchronous.
		/// </summary>
		/// <returns></returns>
		public Task<IEnumerable<Geolocation>> GetAllGeolocationsAsync()
		{
			return GetGeolocationsByQueryAsync($"select g.*, gl.Language, gl.Location from Geolocations g left join GeolocationsLocalizedInfo gl on g.Id= gl.GeolocationId");
		}

		/// <summary>
		/// Gets the geolocation by coordinates asynchronous.
		/// </summary>
		/// <param name="latitude">The latitude.</param>
		/// <param name="longitude">The longitude.</param>
		/// <returns></returns>
		public async Task<Geolocation> GetGeolocationByCoordinatesAsync(double latitude, double longitude)
		{
			var parameters = new DynamicParameters();
			parameters.Add("@latitude", latitude);
			parameters.Add("@longitude", longitude);
			return (await GetGeolocationsByQueryAsync($"select g.*, gl.Language, gl.Location from Geolocations g left join GeolocationsLocalizedInfo gl on g.Id=gl.GeolocationId where g.Latitude=@latitude and g.Longitude=@longitude", parameters)).SingleOrDefault();
		}

		/// <summary>
		/// Gets the geolocations by coordinates list asynchronous.
		/// </summary>
		/// <param name="points">The points.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">Toints cannot be null - points</exception>
		public Task<IEnumerable<Geolocation>> GetGeolocationsByCoordinatesListAsync(List<GeolocationPoint> points)
		{
			if (points is null)
			{
				throw new ArgumentException("Toints cannot be null", nameof(points));
			}

			if (!points.Any())
			{
				return Task.FromResult((IEnumerable<Geolocation>)new List<Geolocation>());

			}
			return GetGeolocationsByCoordinatesListInternalAsync(points);
		}

		/// <summary>
		/// Adds the Geolocation asynchronous.
		/// </summary>
		/// <param name="geolocation">The Geolocation.</param>
		/// <exception cref="ArgumentNullException">Geolocation</exception>
		/// <exception cref="ArgumentException">The {Geolocation.Id} must be specified in the object. - Geolocation</exception>
		public async Task AddGeolocationAsync(Geolocation geolocation)
		{
			if (geolocation is null)
			{
				throw new ArgumentNullException(nameof(geolocation));
			}

			await AddGeolocationInternalAsync(geolocation);
		}

		/// <summary>
		/// Edits the geolocation asynchronous.
		/// </summary>
		/// <param name="geolocation">The geolocation.</param>
		/// <exception cref="ArgumentNullException">geolocation</exception>
		public async Task EditGeolocationAsync(Geolocation geolocation)
		{
			if (geolocation is null)
			{
				throw new ArgumentNullException(nameof(geolocation));
			}

			await EditGeolocationInternalAsync(geolocation);
		}

		/// <summary>
		/// Deletes the geolocation asynchronous.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <exception cref="EntityNotFoundException">The Geolocation with id={id} was not found.</exception>
		public async Task DeleteGeolocationAsync(int id)
		{
			var geolocationParameters = new DynamicParameters();
			geolocationParameters.Add("@id", id);
			var affectedRows = await Connection.ExecuteAsync("delete from Geolocations where Id=@id",
				geolocationParameters);

			if (affectedRows == 0)
			{
				throw new EntityNotFoundException($"The Geolocation with id={id} was not found.");
			}
		}

		private async Task<IEnumerable<Geolocation>> GetGeolocationsByQueryAsync(string query, DynamicParameters parameters = null)
		{
			var geolocationDict = new Dictionary<int, Geolocation>();
			return (await Connection.QueryAsync<Geolocation, GeolocationLocalizedInfo, Geolocation>(query,
				(geolocation, geolocationLocalizedInfo) =>
				{
					Geolocation existingGeolocation;
					if (!geolocationDict.TryGetValue(geolocation.Id, out existingGeolocation))
					{
						existingGeolocation = geolocation;
						geolocationDict.Add(existingGeolocation.Id, existingGeolocation);
					}
					if (geolocationLocalizedInfo != null)
					{
						geolocationLocalizedInfo.Geolocation = existingGeolocation;
						existingGeolocation.LocalizedInfo.Add(geolocationLocalizedInfo);
					}
					return existingGeolocation;
				}, param: parameters, splitOn: "Language")).Distinct().ToList();
		}

		private Task<IEnumerable<Geolocation>> GetGeolocationsByCoordinatesListInternalAsync(List<GeolocationPoint> points)
		{
			var parameters = new DynamicParameters();
			var dynamicQuery = new StringBuilder();
			points = points.Distinct().ToList();
			for (var i = 0; i < points.Count; i++)
			{
				parameters.Add($"l{i}", points[i].Latitude);
				parameters.Add($"lg{i}", points[i].Longitude);
				dynamicQuery.AppendLine((i == 0 ? string.Empty : "or ") +
					$"(g.Latitude=@l{i} and g.Longitude=@lg{i})");
			}
			return GetGeolocationsByQueryAsync($"select g.*, gl.Language, gl.Location from Geolocations g left join GeolocationsLocalizedInfo gl on g.Id=gl.GeolocationId where" + Environment.NewLine + dynamicQuery.ToString(), parameters);
		}

		private async Task AddGeolocationInternalAsync(Geolocation geolocation)
		{
			var geolocationParameters = new DynamicParameters();
			geolocationParameters.Add("@latitude", geolocation.Latitude);
			geolocationParameters.Add("@longitude", geolocation.Longitude);
			geolocationParameters.Add("@error", geolocation.Error);
			var transaction = Connection.BeginTransaction();

			try
			{
				await Connection.ExecuteAsync("insert into Geolocations (Latitude, Longitude, Error) " +
					$"values (@latitude, @longitude, @error)",
					geolocationParameters, transaction);
				if (geolocation.LocalizedInfo != null && geolocation.LocalizedInfo.Any())
				{
					var rowid = GetLastInsertedRowid();
					var localizedParameters = new DynamicParameters();
					var insertList = new List<string>();

					localizedParameters.Add("@id", rowid);
					for (var i = 0; i < geolocation.LocalizedInfo.Count; i++)
					{
						var localizedInfo = geolocation.LocalizedInfo[i];
						localizedParameters.Add($"@language{i}", localizedInfo.Language);
						localizedParameters.Add($"@location{i}", localizedInfo.Location);
						insertList.Add($"(@id, @language{i}, @location{i})");
					}
					var dynamicQuery = "insert into GeolocationsLocalizedInfo (geolocationId, language, location) values " +
						string.Join(", " + Environment.NewLine, insertList);
					await Connection.ExecuteAsync(dynamicQuery, localizedParameters, transaction);
				}
				transaction.Commit();
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
			finally
			{
				transaction.Dispose();
			}
		}

		private async Task EditGeolocationInternalAsync(Geolocation geolocation)
		{
			var geolocationParameters = new DynamicParameters();
			geolocationParameters.Add("@id", geolocation.Id);
			geolocationParameters.Add("@latitude", geolocation.Latitude);
			geolocationParameters.Add("@longitude", geolocation.Longitude);
			geolocationParameters.Add("@error", geolocation.Error);

			var transaction = Connection.BeginTransaction();

			try
			{
				var affectedRows = await Connection.ExecuteAsync("update Geolocations set Latitude=@latitude, Longitude=@longitude, Error=@error where Id=@id",
					geolocationParameters, transaction);

				if (affectedRows == 0)
				{
					throw new EntityNotFoundException($"The Geolocation with Id={geolocation.Id} was not found.");
				}
				await Connection.ExecuteAsync("delete from GeolocationsLocalizedInfo where GeolocationId=@id",
					geolocationParameters, transaction);
				if (geolocation.LocalizedInfo != null && geolocation.LocalizedInfo.Any())
				{
					var localizedParameters = new DynamicParameters();
					var insertList = new List<string>();

					localizedParameters.Add($"@id", geolocation.Id);
					for (var i = 0; i < geolocation.LocalizedInfo.Count; i++)
					{
						var localizedInfo = geolocation.LocalizedInfo[i];
						localizedParameters.Add($"@language{i}", localizedInfo.Language);
						localizedParameters.Add($"@location{i}", localizedInfo.Location);
						insertList.Add($"(@id, @language{i}, @location{i})");
					}
					var dynamicQuery = "insert into GeolocationsLocalizedInfo (geolocationId, language, location) values " +
						string.Join(", " + Environment.NewLine, insertList);
					await Connection.ExecuteAsync(dynamicQuery, localizedParameters, transaction);
				}
				transaction.Commit();
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
			finally
			{
				transaction.Dispose();
			}
		}
	}
}
