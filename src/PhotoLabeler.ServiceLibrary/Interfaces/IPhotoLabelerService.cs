// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using PhotoLabeler.Entities;

namespace PhotoLabeler.ServiceLibrary.Interfaces
{
	/// <summary>
	/// Exposes methods to manage photo labels and other relevant information
	/// </summary>
	public interface IPhotoLabelerService
	{
		Task<TreeView<Photo>> GetPhotosFromDirAsync(string directory, bool loadRecursively = false);

		Task AddFilesToTreeViewItemAsync(TreeViewItem<Photo> item);

		Task<Grid> GetGridFromTreeViewItemAsync(TreeViewItem<Photo> item);

		Task<RenamingResult> RenamePhotosInFolder(TreeViewItem<Photo> directory, bool addPrefixForSorting = true);
	}
}
