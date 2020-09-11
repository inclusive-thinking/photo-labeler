// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhotoLabeler.Entities;

namespace PhotoLabeler.ServiceLibrary.Interfaces
{
	/// <summary>
	/// Exposes methods to manage photo labels and other relevant information
	/// </summary>
	public interface IPhotoLabelerService
	{
		/// <summary>
		/// Gets the photos from dir asynchronous.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="recursiveLoading">if set to <c>true</c> [load recursively].</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		Task<TreeView<Photo>> GetTreeViewFromDirAsync(string directory, bool recursiveLoading = false, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gets the photos from dir asynchronous.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		Task<IEnumerable<Photo>> GetPhotosFromDirAsync(string directory, CancellationToken cancellationToken);

		/// <summary>
		/// Gets the grid from TreeView item asynchronous.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		Task<Grid> GetGridFromTreeViewItemAsync(TreeViewItem<Photo> item, CancellationToken cancellationToken);

		Task<RenamingResult> RenamePhotosInFolder(TreeViewItem<Photo> directory, bool addPrefixForSorting = true);
	}
}
