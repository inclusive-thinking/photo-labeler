// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using PhotoLabeler.Entities;

namespace PhotoLabeler.Data.Interfaces
{
	/// <summary>
	/// Abstract the methods to manage photos in persistent storage
	/// </summary>
	public interface IPhotoRepository
	{
		Task AddPhotoAsync(Photo photo);
		Task DeletePhotoAsync(string md5Sum);
		Task EditPhotoAsync(Photo photo);
		Task<IEnumerable<Photo>> GetAllPhotosAsync();
		Task<IEnumerable<Photo>> GetPhotosByCoordinatesAsync(double latitude, double longitude);
		Task<Photo> GetPhotoByMd5Async(string md5Sum);
		Task<IEnumerable<Photo>> GetPhotosByMd5ListAsync(string md5SumList);
	}
}
