// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using PhotoLabeler.Entities;

namespace PhotoLabeler.ServiceLibrary.Interfaces
{
	public interface IPhotoInfoService
	{
		Task<Photo> GetPhotoFromFileAsync(string file, CancellationToken cancellationToken);
	}
}
