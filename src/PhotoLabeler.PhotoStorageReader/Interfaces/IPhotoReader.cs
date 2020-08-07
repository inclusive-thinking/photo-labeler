// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace PhotoLabeler.PhotoStorageReader.Interfaces
{
	public interface IPhotoReader
	{
		string GetGenericImageSrc();
		string GetImgSrc(string path);
		Task<string> GetImgSrcAsync(string path);
	}
}
