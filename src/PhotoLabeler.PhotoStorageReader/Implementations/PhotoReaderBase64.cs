using System;
using System.IO;
using PhotoLabeler.PhotoStorageReader.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace PhotoLabeler.PhotoStorageReader.Implementations
{
    public class PhotoReaderBase64: IPhotoReader
    {
        public string GetImgSrc(string path)
        {
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
                    image.Mutate(x => x
                        .Resize(100, 100)
                        );
                    return image.ToBase64String(JpegFormat.Instance);
                }
            }
            return (string)null;

        }
    }
}