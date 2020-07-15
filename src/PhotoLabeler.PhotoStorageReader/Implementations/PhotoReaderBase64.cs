using System;
using System.IO;
using PhotoLabeler.PhotoStorageReader.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace PhotoLabeler.PhotoStorageReader.Implementations
{
	public class PhotoReaderBase64 : IPhotoReader
	{
		public string GetImgSrc(string path)
		{
			const int uiSize = 100;
			var postedFileExtension = Path.GetExtension(path);
			var isImage = (string.Equals(postedFileExtension, ".jpg", StringComparison.OrdinalIgnoreCase)
							|| string.Equals(postedFileExtension, ".png", StringComparison.OrdinalIgnoreCase)
							|| string.Equals(postedFileExtension, ".gif", StringComparison.OrdinalIgnoreCase)
							|| string.Equals(postedFileExtension, ".bmp", StringComparison.OrdinalIgnoreCase)
							|| string.Equals(postedFileExtension, ".jpeg", StringComparison.OrdinalIgnoreCase));
			if (isImage)
			{
				using (var image = Image.Load(path))
				{
					var (x, y) = (image.Width, image.Height);
					var minSide = Math.Min(x,y);
					var (resizedX, resizedY) = (uiSize*x/minSide, uiSize*y/minSide);
					var centerArea = new Rectangle( (resizedX-uiSize)/2, (resizedY-uiSize)/2, uiSize, uiSize);

					image.Mutate(x => x
						.Resize(resizedX, resizedY)
						.Crop(centerArea)
						);

					return image.ToBase64String(JpegFormat.Instance);
				}
			}
			return (string)null;

		}
	}
}
