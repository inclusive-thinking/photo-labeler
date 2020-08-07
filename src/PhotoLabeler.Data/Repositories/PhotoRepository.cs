// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PhotoLabeler.Data.Exceptions;
using PhotoLabeler.Data.Interfaces;
using PhotoLabeler.Entities;

namespace PhotoLabeler.Data.Repositories
{
	/// <summary>
	/// Photo repository class
	/// </summary>
	public class PhotoRepository : RepositoryBase, IPhotoRepository
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="AppConfigRepository"/> class.
		/// </summary>
		/// <param name="connection">The connection.</param>
		public PhotoRepository(IDbConnection connection)
			: base(connection)
		{
			CreateTableIfDoesNotExists(
				"Photos",
				$"create table Photos (Md5Sum TEXT NOT NULL PRIMARY KEY, Path TEXT not null, Label TEXT, TakenDate TEXT, ModifiedDate TEXT, Latitude REAL, Longitude REAL, AltitudeInMeters REAL)");
			CreateTableIfDoesNotExists(
				"PhotosLocalizedInfo",
				$"create table PhotosLocalizedInfo (Md5Sum TEXT NOT NULL, Language TEXT not null, Location TEXT, primary key (Md5Sum, Language), constraint fk_PhotosLocalizedInfo_Md5Sum_Photos_Md5Sum foreign key (Md5Sum) references Photos (Md5Sum) ON DELETE CASCADE)");
		}


		public Task<Photo> GetPhotoByMd5Async(string md5Sum)
		{
			if (md5Sum is null)
			{
				throw new ArgumentNullException(nameof(md5Sum));
			}

			return GetPhotoByMd5InternalAsync(md5Sum);
		}

		public Task<IEnumerable<Photo>> GetAllPhotosAsync()
		{
			return GetPhotosByQueryAsync($"select p.*, pl.Language, pl.Location from Photos p left join PhotosLocalizedInfo pl on p.Md5Sum = pl.Md5Sum");
		}

		public Task<IEnumerable<Photo>> GetPhotosByMd5ListAsync(string md5SumList)
		{
			if (string.IsNullOrEmpty(md5SumList))
			{
				throw new ArgumentException("message", nameof(md5SumList));
			}

			return GetPhotosByMd5ListInternalAsync(md5SumList);
		}

		public Task<IEnumerable<Photo>> GetPhotosByCoordinatesAsync(double latitude, double longitude)
		{
			var parameters = new DynamicParameters();
			parameters.Add("@latitude", latitude);
			parameters.Add("@longitude", longitude);
			return GetPhotosByQueryAsync($"select p.*, pl.Language, pl.Location from Photos p left join PhotosLocalizedInfo pl on p.Md5Sum=pl.Md5Sum where p.Latitude=@latitude and p.Longitude=@longitude", parameters);
		}

		/// <summary>
		/// Adds the photo asynchronous.
		/// </summary>
		/// <param name="photo">The photo.</param>
		/// <exception cref="ArgumentNullException">photo</exception>
		/// <exception cref="ArgumentException">The {photo.Md5Sum} must be specified in the object. - Photo</exception>
		public async Task AddPhotoAsync(Photo photo)
		{
			if (photo is null)
			{
				throw new ArgumentNullException(nameof(photo));
			}

			if (string.IsNullOrWhiteSpace(photo.Md5Sum))
			{
				throw new ArgumentException($"The {nameof(photo.Md5Sum)} must be specified in the object.", nameof(photo));
			}
			await AddPhotoInternalAsync(photo);
		}

		public async Task EditPhotoAsync(Photo photo)
		{
			if (photo is null)
			{
				throw new ArgumentNullException(nameof(photo));
			}

			if (string.IsNullOrWhiteSpace(photo.Md5Sum))
			{
				throw new ArgumentException($"The {photo.Md5Sum} must be specified in the object.", nameof(photo));
			}
			await EditPhotoInternalAsync(photo);
		}

		public async Task DeletePhotoAsync(string md5Sum)
		{
			if (string.IsNullOrEmpty(md5Sum))
			{
				throw new ArgumentException($"The {nameof(md5Sum)} must be specified.", nameof(md5Sum));
			}

			await DeletePhotoInternalAsync(md5Sum);
		}

		private async Task<IEnumerable<Photo>> GetPhotosByQueryAsync(string query, DynamicParameters parameters = null)
		{
			var photoDict = new Dictionary<string, Photo>();
			return (await Connection.QueryAsync<Photo, PhotoLocalizedInfo, Photo>(query,
				(photo, photoLocalizedInfo) =>
				{
					Photo existingPhoto;
					if (!photoDict.TryGetValue(photo.Md5Sum, out existingPhoto))
					{
						existingPhoto = photo;
						photoDict.Add(existingPhoto.Md5Sum, existingPhoto);
					}
					if (photoLocalizedInfo != null)
					{
						photoLocalizedInfo.Photo = existingPhoto;
						existingPhoto.LocalizedInfo.Add(photoLocalizedInfo);
					}
					return existingPhoto;
				}, param: parameters, splitOn: "Language")).Distinct().ToList();
		}

		private async Task<Photo> GetPhotoByMd5InternalAsync(string md5Sum)
		{
			var parameters = new DynamicParameters();
			parameters.Add("@md5Sum", md5Sum);
			return (await GetPhotosByQueryAsync("select p.*, pl.Language, pl.Location from Photos p left join PhotosLocalizedInfo pl on p.Md5Sum=pl.Md5Sum where p.Md5Sum=@md5Sum", parameters)).FirstOrDefault();
		}

		private Task<IEnumerable<Photo>> GetPhotosByMd5ListInternalAsync(string md5SumList)
		{
			var parameters = new DynamicParameters();
			var md5Items = md5SumList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			for (var i = 0; i < md5Items.Count; i++)
			{
				parameters.Add($"p{i}", md5Items[i]);
			}
			return GetPhotosByQueryAsync($"select p.*, pl.Language, pl.Location from Photos p left join PhotosLocalizedInfo pl on p.Md5Sum=pl.Md5Sum where p.Md5Sum in (@{string.Join(",@", parameters.ParameterNames)})", parameters);
		}

		private async Task AddPhotoInternalAsync(Photo photo)
		{
			var photoParameters = new DynamicParameters();
			photoParameters.Add("@md5Sum", photo.Md5Sum);
			photoParameters.Add("@path", photo.Path);
			photoParameters.Add("@label", photo.Label);
			photoParameters.Add("@takenDate", photo.TakenDate);
			photoParameters.Add("@modifiedDate", photo.ModifiedDate);
			photoParameters.Add("@latitude", photo.Latitude);
			photoParameters.Add("@longitude", photo.Longitude);
			photoParameters.Add(@"altitudeInMeters", photo.AltitudeInMeters);

			var transaction = Connection.BeginTransaction();

			try
			{
				await Connection.ExecuteAsync("insert into Photos (Md5Sum, Path, Label, TakenDate, ModifiedDate, Latitude, Longitude, AltitudeInMeters) " +
					$"values (@md5Sum, @path, @label, @takenDate, @modifiedDate, @latitude, @longitude, @altitudeInMeters)",
					photoParameters, transaction);
				if (photo.LocalizedInfo != null && photo.LocalizedInfo.Any())
				{
					var localizedParameters = new DynamicParameters();
					var insertList = new List<string>();

					localizedParameters.Add($"@md5Sum", photo.Md5Sum);
					for (var i = 0; i < photo.LocalizedInfo.Count; i++)
					{
						var localizedInfo = photo.LocalizedInfo[i];
						localizedParameters.Add($"@language{i}", localizedInfo.Language);
						localizedParameters.Add($"@location{i}", localizedInfo.Location);
						insertList.Add($"(@md5Sum, @language{i}, @location{i})");
					}
					var dynamicQuery = "insert into PhotosLocalizedInfo (md5Sum, language, location) values " +
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

		private async Task EditPhotoInternalAsync(Photo photo)
		{
			var photoParameters = new DynamicParameters();
			photoParameters.Add("@md5Sum", photo.Md5Sum);
			photoParameters.Add("@path", photo.Path);
			photoParameters.Add("@label", photo.Label);
			photoParameters.Add("@takenDate", photo.TakenDate);
			photoParameters.Add("@modifiedDate", photo.ModifiedDate);
			photoParameters.Add("@latitude", photo.Latitude);
			photoParameters.Add("@longitude", photo.Longitude);
			photoParameters.Add(@"altitudeInMeters", photo.AltitudeInMeters);

			var transaction = Connection.BeginTransaction();

			try
			{
				var affectedRows = await Connection.ExecuteAsync("update Photos set Path=@path, Label=@label, TakenDate=@takenDate, ModifiedDate=@modifiedDate, Latitude=@latitude, Longitude=@longitude, AltitudeInMeters=@altitudeInMeters where Md5Sum=@md5Sum",
					photoParameters, transaction);

				if (affectedRows == 0)
				{
					throw new EntityNotFoundException($"The photo with md5Sum={photo.Md5Sum} was not found.");
				}
				await Connection.ExecuteAsync("delete from PhotosLocalizedInfo where Md5Sum=@md5Sum", photoParameters, transaction);
				if (photo.LocalizedInfo != null)
				{
					var localizedParameters = new DynamicParameters();
					var insertList = new List<string>();

					localizedParameters.Add($"@md5Sum", photo.Md5Sum);
					for (var i = 0; i < photo.LocalizedInfo.Count; i++)
					{
						var localizedInfo = photo.LocalizedInfo[i];
						localizedParameters.Add($"@language{i}", localizedInfo.Language);
						localizedParameters.Add($"@location{i}", localizedInfo.Location);
						insertList.Add($"(@md5Sum, @language{i}, @location{i})");
					}
					var dynamicQuery = "insert into PhotosLocalizedInfo (md5Sum, language, location) values " +
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

		private async Task DeletePhotoInternalAsync(string md5Sum)
		{
			var photoParameters = new DynamicParameters();
			photoParameters.Add("@md5Sum", md5Sum);
			var affectedRows = await Connection.ExecuteAsync("delete from Photos where Md5Sum=@md5Sum",
				photoParameters);

			if (affectedRows == 0)
			{
				throw new EntityNotFoundException($"The photo with md5Sum={md5Sum} was not found.");
			}
		}
	}
}
