
using System.Threading.Tasks;

namespace PhotoLabeler.PhotoStorageReader.Interfaces
{
	public interface IPhotoReader
	{
		string GetPictureImageSrc();
		string GetImgSrc(string path);
		Task<string> GetImgSrcAsync(string path);
	}
}
