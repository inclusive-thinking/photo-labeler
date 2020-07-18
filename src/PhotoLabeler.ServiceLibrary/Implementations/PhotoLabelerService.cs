// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MetadataExtractor;
using MetadataExtractor.Formats.Xmp;
using Microsoft.Extensions.Localization;
using PhotoLabeler.Entities;
using PhotoLabeler.PhotoStorageReader.Interfaces;
using PhotoLabeler.ServiceLibrary.Exceptions;
using PhotoLabeler.ServiceLibrary.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
namespace PhotoLabeler.ServiceLibrary.Implementations
{
	public class PhotoLabelerService : IPhotoLabelerService
	{

		private const string QuickTimeMetadataDescriptionTag = "Description";

		private const string QuickTimeMetaDataCreationDateTag = "Creation Date";

		private const int MaxFileNameLength = 260;

		private readonly IStringLocalizer<PhotoLabelerService> _localizer;
		private readonly IPhotoReader _photoReader;

		/// <summary>
		/// Initializes a new instance of the <see cref="PhotoLabelerService"/> class.
		/// </summary>
		/// <param name="localizer">The localizer.</param>
		public PhotoLabelerService(
			IStringLocalizer<PhotoLabelerService> localizer,
			IPhotoReader photoReader
			)
		{
			_localizer = localizer;
			_photoReader = photoReader;
		}



		/// <summary>
		/// Gets the photos from dir asynchronous.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="loadRecursively">if set to <c>true</c> [load recursively].</param>
		/// <returns></returns>
		public async Task<TreeView<Photo>> GetPhotosFromDirAsync(string directory, bool loadRecursively = false)
		{
			TreeView<Photo> treeView = new TreeView<Photo>();
			directory = directory.TrimEnd(new[] { Path.DirectorySeparatorChar });
			var directoriesFound = await Task.Run(() => System.IO.Directory.GetDirectories(directory, string.Empty, SearchOption.AllDirectories));
			var dirLength = directory.Length;
			var directories = directoriesFound.Select(i => i.Substring(dirLength + 1)).OrderBy(i => i.Length).ThenBy(i => i).ToList();
			directories.Insert(0, string.Empty);
			var items = new List<TreeViewItem<Photo>>();
			var flatItems = new List<TreeViewItem<Photo>>();
			var levelBase = directory.Length - directory.Replace(Path.DirectorySeparatorChar.ToString(), string.Empty).Length;
			foreach (var dir in directories)
			{
				var fullDir = Path.Combine(directory, dir);
				var dirName = fullDir.TrimEnd(new[] { Path.DirectorySeparatorChar });
				dirName = dirName.Substring(dirName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
				var level = fullDir.Length - fullDir.Replace(Path.DirectorySeparatorChar.ToString(), string.Empty).Length - levelBase;
				if (level == 0)
				{
					var treeViewItem = new TreeViewItem<Photo> { Path = fullDir, Children = new List<TreeViewItem<Photo>>(), Name = dirName, TreeView = treeView, Level = 0 };
					if (loadRecursively)
					{
						await AddFilesToTreeViewItemAsync(treeViewItem);
					}

					treeViewItem.ItemIndex = items.Count;
					items.Add(treeViewItem);
					flatItems.Add(treeViewItem);
				}
				else
				{
					var parentDir = fullDir.Substring(0, fullDir.LastIndexOf(Path.DirectorySeparatorChar.ToString()));
					TreeViewItem<Photo> parentItem = null;
					parentItem = flatItems.Single(i => i.Path == parentDir);
					var treeViewItem = new TreeViewItem<Photo>() { Path = fullDir, Parent = parentItem, Children = new List<TreeViewItem<Photo>>(), Name = dirName, TreeView = treeView, Level = level };
					if (loadRecursively)
					{
						await AddFilesToTreeViewItemAsync(treeViewItem);
					}

					treeViewItem.ItemIndex = parentItem.Children.Count;
					parentItem.Children.Add(treeViewItem);
					flatItems.Add(treeViewItem);
				}
			}
			items[0].Selected = items[0].Expanded = true;
			treeView.Items = items;
			treeView.SelectedItem = items[0];
			treeView.FlatItems = new Lazy<List<TreeViewItem<Photo>>>(() =>
			{
				var list = new List<TreeViewItem<Photo>>();
				foreach (var item in items)
				{
					list.AddRange(FlattensItems(item));
				}
				return list;
			});
			return treeView;
		}

		/// <summary>
		/// Adds the files to TreeView item asynchronous.
		/// </summary>
		/// <param name="item">The item.</param>
		public async Task AddFilesToTreeViewItemAsync(TreeViewItem<Photo> item)
		{
			var files = await Task.Run(() => System.IO.Directory.GetFiles(item.Path, string.Empty, SearchOption.TopDirectoryOnly));
			var supportedExtensions = new[] { ".jpg", ".heic", ".mov", ".png", ".gif", ".jpeg", ".tiff", ".raw", ".mp4" };
			var filteredFiles = files.Where(i => supportedExtensions.Contains(Path.GetExtension(i.ToLower())));
			using (var semaphore = new SemaphoreSlim(200))
			{
				var allTasks = filteredFiles.Select(async i =>
				{
					try
					{
						await semaphore.WaitAsync();
						return await GetPhotoFromFileAsync(i);
					}
					finally
					{
						semaphore.Release();
					}
				}).ToList();
				await Task.WhenAll(allTasks);
				item.Items = allTasks.Select(t => t.Result).ToList();
				item.ItemsLoaded = true;
			}
		}


		/// <summary>
		/// Gets the grid from TreeView item asynchronous.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public async Task<Grid> GetGridFromTreeViewItemAsync(TreeViewItem<Photo> item)
		{
			if (!item.ItemsLoaded)
			{
				await AddFilesToTreeViewItemAsync(item);
			}
			var grid = new Grid
			{
				Caption = _localizer["List of photos in the current directory."]
			};

			var headerRow = new Grid.GridRow(0, grid);
			headerRow.Cells.Add(new Grid.GridHeaderCell(cellIndex: headerRow.Cells.Count, row: headerRow, grid: grid)
			{
				Text = _localizer["Picture"]
			});
			headerRow.Cells.Add(new Grid.GridHeaderCell(cellIndex: headerRow.Cells.Count, row: headerRow, grid: grid)
			{
				Text = _localizer["Label"]
			});
			headerRow.Cells.Add(new Grid.GridHeaderCell(cellIndex: headerRow.Cells.Count, row: headerRow, grid: grid)
			{
				Text = _localizer["Filename"]
			});
			headerRow.Cells.Add(new Grid.GridHeaderCell(cellIndex: headerRow.Cells.Count, row: headerRow, grid: grid)
			{
				Text = _localizer["Creation date"]
			});

			//
			grid.Header = new Grid.GridHeader { Row = headerRow };
			grid.Body = new Grid.GridBody();

			foreach (var photo in item.Items)
			{
				var row = new Grid.GridRow(rowIndex: grid.Body.Rows.Count, grid);

				//
				var img = _photoReader.GetImgSrc(photo.Path);

				var pictCell = new Grid.GridCellPict(cellIndex: row.Cells.Count, row: row, grid: grid)
				{
					Text = Path.GetFileName(photo.Path),
					SrcBase64 = img,
				};
				row.Cells.Add(pictCell);

				//
				var labelCell = new Grid.GridCellLabel(cellIndex: row.Cells.Count, row: row, grid: grid)
				{
					Text = photo.Label ?? _localizer["Unlabeled"]
				};
				row.Cells.Add(labelCell);

				//
				var nameCell = new Grid.GridCellFileName(cellIndex: row.Cells.Count, row: row, grid: grid)
				{
					Text = Path.GetFileName(photo.Path)
				};
				row.Cells.Add(nameCell);

				//
				var dateTakenCell = new Grid.GridCellTakenData(cellIndex: row.Cells.Count, row: row, grid: grid)
				{
					Text = photo.TakenDate?.ToString("F") ?? _localizer["unknown"]
				};
				row.Cells.Add(dateTakenCell);

				//
				grid.Body.Rows.Add(row);

			}
			if (grid.Body.Rows.Any())
			{
				grid.Body.Rows[0].Cells[0].Selected = true;
			}
			return grid;
		}

		/// <summary>
		/// Renames the photos in folder.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="addPrefixForSorting">if set to <c>true</c> [add prefix for sorting].</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">directory</exception>
		public Task<RenamingResult> RenamePhotosInFolder(TreeViewItem<Photo> directory, bool addPrefixForSorting = true)
		{
			if (directory is null)
			{
				throw new ArgumentNullException(nameof(directory));
			}

			return RenamePhotosInFolderInternalAsync(directory, addPrefixForSorting);
		}

		private List<TreeViewItem<Photo>> FlattensItems(TreeViewItem<Photo> item)
		{
			List<TreeViewItem<Photo>> items = new List<TreeViewItem<Photo>>
			{
				item
			};
			if (item.Children != null && item.Children.Any())
			{
				foreach (var childItem in item.Children)
				{
					items.AddRange(FlattensItems(childItem));
				}
			}
			return items;
		}



		private async Task<Photo> GetPhotoFromFileAsync(string file)
		{
			List<string> labels = new List<string>();
			IReadOnlyList<MetadataExtractor.Directory> data = null;
			try
			{
				data = await Task.Run(() => ImageMetadataReader.ReadMetadata(file));
			}
			catch
			{
				return null;
			}
			if (data == null)
			{
				return null;
			}

			var photo = new Photo { Path = file };
			data.Where(d => d.Name == "XMP").ToList().ForEach(xmpData =>
			{
				AddLabelFromXmpDescription(labels, xmpData);
				AddDates(xmpData, photo);
			});

			var exifDir = data.SingleOrDefault(i => i.Name == "Exif IFD0");
			if (exifDir != null)
			{
				AddMetadataLabel(labels, exifDir, "Image Description");
				AddMetadataLabel(labels, exifDir, "Windows XP Subject");
				AddMetadataLabel(labels, exifDir, "Windows XP Title");
				AddMetadataLabel(labels, exifDir, "Windows XP Comment");
				if (!photo.TakenDate.HasValue)
				{
					AddExifDates(exifDir, photo);
				}
			}

			var quickTimeMeta = data.SingleOrDefault(d => d.Name == "QuickTime Metadata Header");
			if (quickTimeMeta != null)
			{
				var descriptionTag = quickTimeMeta.Tags.SingleOrDefault(t => t.Name == QuickTimeMetadataDescriptionTag);
				if (descriptionTag != null)
				{
					var value = (StringValue)quickTimeMeta.GetObject(descriptionTag.Type);
					labels.Add(value.ToString());
				}
				if (!photo.TakenDate.HasValue)
				{
					AddQuickTimeMetaCreationDate(quickTimeMeta, photo);
				}
			}

			labels = labels.Distinct().ToList();
			if (labels.Any())
			{
				photo.Label = string.Join(Environment.NewLine, labels);
			}
			return photo;
		}

		private void AddQuickTimeMetaCreationDate(MetadataExtractor.Directory quickTimeMeta, Photo photo)
		{
			var tag = quickTimeMeta.Tags.SingleOrDefault(t => t.Name == QuickTimeMetaDataCreationDateTag);
			if (tag != null)
			{
				var tagValue = (StringValue)quickTimeMeta.GetObject(tag.Type);
				if (DateTime.TryParse(tagValue.ToString(), out DateTime creationDate))
				{
					photo.TakenDate = creationDate;
				}
			}
		}

		private void AddExifDates(MetadataExtractor.Directory directory, Photo photo)
		{
			var tag = directory.Tags.SingleOrDefault(t => t.Name == "Date/Time");
			if (!string.IsNullOrWhiteSpace(tag?.Description) && DateTime.TryParse(tag.Description, out DateTime creationDate))
			{
				photo.TakenDate = creationDate;
			}
		}

		private void AddDates(MetadataExtractor.Directory xmpData, Photo photo)
		{
			var xmpDirectory = xmpData as XmpDirectory;
			var dateCreated = xmpDirectory.XmpMeta.Properties.SingleOrDefault(p => p.Path == "xmp:CreateDate");
			var photoshopDateCreated = xmpDirectory.XmpMeta.Properties.SingleOrDefault(p => p.Path == "photoshop:DateCreated");
			var dateModified = xmpDirectory.XmpMeta.Properties.SingleOrDefault(p => p.Path == "xmp:ModifyDate");
			if (!string.IsNullOrWhiteSpace(dateCreated?.Value) && DateTime.TryParse(dateCreated.Value, out DateTime takenDate))
			{
				photo.TakenDate = takenDate;
			}

			if (!photo.TakenDate.HasValue && !string.IsNullOrWhiteSpace(photoshopDateCreated?.Value) && DateTime.TryParse(photoshopDateCreated.Value, out DateTime photoshopTakenDate))
			{
				photo.TakenDate = photoshopTakenDate;
			}

			if (!string.IsNullOrWhiteSpace(dateModified?.Value) && DateTime.TryParse(dateModified.Value, out DateTime modifiedDate))
			{
				photo.ModifiedDate = modifiedDate;
			}
		}

		private void AddLabelFromXmpDescription(List<string> labels, MetadataExtractor.Directory xmpData)
		{
			var xmpDirectory = xmpData as XmpDirectory;
			var artworkDesc = xmpDirectory.XmpMeta.Properties.SingleOrDefault(p => p.Path == "Iptc4xmpExt:ArtworkContentDescription");
			if (artworkDesc != null)
			{
				labels.Add(artworkDesc.Value);
			}
		}

		private void AddMetadataLabel(List<string> labels, MetadataExtractor.Directory exifDir, string tagName)
		{
			var tag = exifDir.Tags.SingleOrDefault(t => t.Name == tagName);
			if (!string.IsNullOrWhiteSpace(tag?.Description))
			{
				labels.Add(tag.Description);
			}
		}
		private async Task<bool> RenameItemAsync(string basePath, Photo photo, int prefixIndex, int totalFiles, bool addPrefix)
		{
			if (string.IsNullOrWhiteSpace(photo.Label))
			{
				throw new LabelNotFoundException(nameof(photo));
			}

			var newFileName = GetNewName(basePath, photo, 0, prefixIndex, totalFiles, addPrefix);
			var oldName = Path.Combine(basePath, photo.Path);
			if (newFileName == oldName)
			{
				ApplyMetadataDateTimesFromPhoto(photo, newFileName);
				return false;
			}
			var duplicatedIndex = 1;
			while (File.Exists(newFileName))
			{
				newFileName = GetNewName(basePath, photo, duplicatedIndex++, prefixIndex, totalFiles, addPrefix);
			}
			await Task.Run(() => File.Move(oldName, newFileName));
			ApplyMetadataDateTimesFromPhoto(photo, newFileName);
			return true;

		}

		private void ApplyMetadataDateTimesFromPhoto(Photo photo, string newFileName)
		{
			if (photo.TakenDate.HasValue)
			{
				try
				{
					File.SetCreationTime(newFileName, photo.TakenDate.Value);
				}
				catch
				{
					// Unable to set creation time. Never mind ;)
				}
			}
			if (photo.ModifiedDate.HasValue || photo.TakenDate.HasValue)
			{
				try
				{
					File.SetLastWriteTime(newFileName, photo.ModifiedDate ?? photo.TakenDate.Value);
				}
				catch
				{
					// Impossible to adjust the file creation date to the photo creation date... but not serious either.
				}
			}
		}

		private string GetNewName(string basePath, Photo photo, int duplicatedIndex, int prefixIndex, int totalFiles, bool addPrefix)
		{
			var extension = Path.GetExtension(photo.Path);
			string newFileName;
			int finalMaxFileNameLength = MaxFileNameLength - extension.Length - (duplicatedIndex > 0 ? $" ({duplicatedIndex})".Length : 0);
			string prefix = string.Empty;
			if (addPrefix)
			{
				int prefixLength = totalFiles.ToString().Length;
				prefix = prefixIndex.ToString().PadLeft(prefixLength, '0') + ". ";
				finalMaxFileNameLength -= prefix.Length;
			}

			if (photo.Label.Length > finalMaxFileNameLength)
			{
				newFileName = photo.Label.Substring(0, finalMaxFileNameLength - 3) + "...";
			}
			else
			{
				newFileName = photo.Label;
			}

			newFileName = Regex.Replace(newFileName, $"[{string.Join(string.Empty, Path.GetInvalidFileNameChars().Select(c => "\\" + c.ToString()))}]", "_");
			if (duplicatedIndex > 0)
			{
				newFileName += $" ({duplicatedIndex})";
			}
			if (!string.IsNullOrWhiteSpace(prefix))
			{
				newFileName = prefix + newFileName;
			}

			newFileName += extension;
			newFileName = Path.Combine(basePath, newFileName);
			return newFileName;
		}

		private async Task<RenamingResult> RenamePhotosInFolderInternalAsync(TreeViewItem<Photo> directory, bool addPrefixForSorting)
		{
			var renamingResult = new RenamingResult();
			int totalRenamed = 0;

			using (SemaphoreSlim semaphore = new SemaphoreSlim(200))
			{
				var itemsWithLabel = directory.Items.Where(i => !string.IsNullOrWhiteSpace(i.Label)).ToList();
				renamingResult.TotalFiles = itemsWithLabel.Count;
				itemsWithLabel = itemsWithLabel.OrderBy(i => i.TakenDate ?? new FileInfo(i.Path).CreationTime).ToList();

				var allTasks = new List<Task>();

				for (int i = 0; i < renamingResult.TotalFiles; i++)
				{
					var index = i;
					allTasks.Add(Task.Run(async () =>
					{
						try
						{
							await semaphore.WaitAsync();
							if (await RenameItemAsync(directory.Path, itemsWithLabel[index], index + 1, renamingResult.TotalFiles, addPrefixForSorting))
							{
								Interlocked.Increment(ref totalRenamed);
							}
						}
						finally
						{
							semaphore.Release();
						}
					}));
				}

				try
				{
					await Task.WhenAll(allTasks);
				}
				catch (AggregateException agEx)
				{
					foreach (var ex in agEx.InnerExceptions)
					{
						renamingResult.Errors.Add(ex.Message);
					}
				}
				catch (Exception ex)
				{
					renamingResult.Errors.Add(ex.Message);
				}
			}
			renamingResult.FilesRenamed = totalRenamed;

			return renamingResult;
		}

	}
}
