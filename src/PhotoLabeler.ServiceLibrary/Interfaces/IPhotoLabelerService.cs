using PhotoLabeler.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PhotoLabeler.ServiceLibrary.Interfaces
{
	public interface IPhotoLabelerService
	{
		Task<TreeView<Photo>> GetPhotosFromDirAsync(string directory, bool loadRecursively = false);
		
		Task AddFilesToTreeViewItemAsync(TreeViewItem<Photo> item);

		Task<Grid> GetGridFromTreeViewItemAsync(TreeViewItem<Photo> item);

		Task<RenamingResult>  RenamePhotosInFolder(TreeViewItem<Photo> directory, bool addPrefixForSorting = true);
	}
}
