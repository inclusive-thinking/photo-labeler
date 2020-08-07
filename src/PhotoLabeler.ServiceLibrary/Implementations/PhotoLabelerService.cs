// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using MoreLinq.Extensions;
using PhotoLabeler.Croscutting.Interfaces;
using PhotoLabeler.Data.Interfaces;
using PhotoLabeler.Entities;
using PhotoLabeler.Nominatim.Agent;
using PhotoLabeler.Nominatim.Agent.Entities;
using PhotoLabeler.PhotoStorageReader.Interfaces;
using PhotoLabeler.ServiceLibrary.Exceptions;
using PhotoLabeler.ServiceLibrary.Interfaces;
using Serilog;

namespace PhotoLabeler.ServiceLibrary.Implementations
{
	public class PhotoLabelerService : IPhotoLabelerService
	{
		private class PhotoRetrievalResult
		{
			public List<Photo> Photos { get; set; }

			public List<Photo> PhotosToAddIntoDatabase { get; set; }

		}

		private const int MaxFileNameLength = 260;
		private readonly IPhotoInfoService _photoInfoService;

		private readonly IStringLocalizer<PhotoLabelerService> _localizer;

		private readonly IPhotoReader _photoReader;

		private readonly INominatimAgent _nominatimAgent;

		private readonly IPhotoRepository _photoRepository;

		private readonly ICryptoService _cryptoService;

		private readonly ILogger _logger;


		/// <summary>
		/// Initializes a new instance of the <see cref="PhotoLabelerService" /> class.
		/// </summary>
		/// <param name="photoInfoService">The photo information service.</param>
		/// <param name="localizer">The localizer.</param>
		/// <param name="photoReader">The photo reader.</param>
		/// <param name="nominatimAgent">The nominatim agent.</param>
		/// <param name="photoRepository">The photo repository.</param>
		/// <param name="logger">The logger.</param>
		public PhotoLabelerService(
			IPhotoInfoService photoInfoService,
			IStringLocalizer<PhotoLabelerService> localizer,
			IPhotoReader photoReader,
			INominatimAgent nominatimAgent,
			IPhotoRepository photoRepository,
			ICryptoService cryptoService,
			ILogger logger
			)
		{
			_photoInfoService = photoInfoService;
			_localizer = localizer;
			_photoReader = photoReader;
			_nominatimAgent = nominatimAgent;
			_photoRepository = photoRepository;
			_cryptoService = cryptoService;
			_logger = logger;
		}



		/// <summary>
		/// Gets the photos from dir asynchronous.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="loadRecursively">if set to <c>true</c> [load recursively].</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public async Task<TreeView<Photo>> GetTreeViewFromDirAsync(string directory, bool loadRecursively = false, CancellationToken cancellationToken = default)
		{
			TreeView<Photo> treeView = new TreeView<Photo>();
			directory = directory.TrimEnd(new[] { Path.DirectorySeparatorChar });
			var directoriesFound = await Task.Run(() => Directory.GetDirectories(directory, string.Empty, SearchOption.AllDirectories), cancellationToken);
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
						var photos = await GetPhotosFromDirAsync(treeViewItem.Path, cancellationToken);
						treeViewItem.Items = photos.ToList();
						treeViewItem.ItemsLoaded = true;
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
						var photos = await GetPhotosFromDirAsync(treeViewItem.Path, cancellationToken);
						treeViewItem.Items = photos.ToList();
						treeViewItem.ItemsLoaded = true;
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
		/// Gets the photos from dir asynchronous.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public async Task<IEnumerable<Photo>> GetPhotosFromDirAsync(string directory, CancellationToken cancellationToken)
		{
			_logger.Debug($"Loading photos for the directory {directory}...");
			var files = await Task.Run(() => Directory.GetFiles(directory, string.Empty, SearchOption.TopDirectoryOnly), cancellationToken);
			var supportedExtensions = new[] { ".jpg", ".heic", ".mov", ".png", ".gif", ".jpeg", ".tiff", ".raw", ".mp4" };
			var filteredFiles = files.Where(i => supportedExtensions.Contains(Path.GetExtension(i.ToLower()))).ToList();
			_logger.Debug($"{filteredFiles.Count} supported files found.");
			if (!filteredFiles.Any())
			{
				return new List<Photo>();
			}

			var md5SignaturesDict = await GetMd5SignatureFromFilesAsync(filteredFiles, cancellationToken);
			_logger.Debug("Get stored photos in database with this list of MD5 sum...");
			var storedPhotos = await _photoRepository.GetPhotosByMd5ListAsync(string.Join(",", md5SignaturesDict.Values));
			_logger.Debug($"Photos retrieved successfully from database. {storedPhotos.Count()} exist.");
			_logger.Debug("Retrieving photo information from database or file metadata...");
			var photosInfo = await GetPhotosFromFileOrDatabaseAsync(md5SignaturesDict, storedPhotos);
			var distinctPhotosToAdd = photosInfo.PhotosToAddIntoDatabase.DistinctBy(d => d.Md5Sum).ToList();
			_logger.Debug($"There are {distinctPhotosToAdd.Count} photos to add into database...");
			foreach (var photo in distinctPhotosToAdd)
			{
				_logger.Debug($"Adding photo {photo.Path} into database...");
				await _photoRepository.AddPhotoAsync(photo);
				_logger.Debug($"Photo {photo.Path} added into database.");
			}
			return photosInfo.Photos;
		}

		/// <summary>
		/// Gets the grid from TreeView item asynchronous.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public async Task<Grid> GetGridFromTreeViewItemAsync(TreeViewItem<Photo> item, CancellationToken cancellationToken)
		{
			if (!item.ItemsLoaded)
			{
				var photos = await GetPhotosFromDirAsync(item.Path, cancellationToken);
				item.Items = photos.ToList();
				item.ItemsLoaded = true;
			}
			var grid = new Grid
			{
				Caption = _localizer["List of photos in the current directory."]
			};
			if (item.Items.Any(i => i.HasErrors))
			{
				var errors = new AggregateException(item.Items.Where(i => i.HasErrors).Select(i => i.Error));
				grid.Errors = errors;
			}
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
				Text = _localizer["Location"]
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

				row.PicturePath = photo.Path;
				var path = row.PicturePath;

				//
				var img = _photoReader.GetGenericImageSrc();

				var pictCell = new Grid.GridPictCell(cellIndex: row.Cells.Count, row: row, grid: grid)
				{
					Text = photo.Label,
					Src = path,
					SrcBase64 = img,
				};
				pictCell.ReloadImage = () => RedrawPicture(pictCell);
				row.Cells.Add(pictCell);

				//
				var labelCell = new Grid.GridLabelCell(cellIndex: row.Cells.Count, row: row, grid: grid)
				{
					Text = photo.Label ?? _localizer["Unlabeled"],
					HasLabel = !string.IsNullOrWhiteSpace(photo.Label),
				};
				row.Cells.Add(labelCell);

				//
				var locationCell = new Grid.GridLocationCell(cellIndex: row.Cells.Count, row: row, grid: grid);
				var language = CultureInfo.CurrentCulture.Name;
				var localizedInfo = photo.LocalizedInfo.SingleOrDefault(lc => lc.Language == language);
				if (localizedInfo != null)
				{
					locationCell.Text = localizedInfo.Location;
					locationCell.LocationLoaded = true;
				}
				else
				{
					locationCell.Text = photo.Latitude.HasValue && photo.Longitude.HasValue ? _localizer["Loading location..."] : _localizer["No GPS information"];
					locationCell.Latitude = photo.Latitude;
					locationCell.Longitude = photo.Longitude;
					locationCell.LoadLocation = LoadLocation;
				}
				row.Cells.Add(locationCell);

				//
				var nameCell = new Grid.GridFileNameCell(cellIndex: row.Cells.Count, row: row, grid: grid)
				{
					Text = Path.GetFileName(path)
				};
				row.Cells.Add(nameCell);

				//
				var dateTakenCell = new Grid.GridTakenDataCell(cellIndex: row.Cells.Count, row: row, grid: grid)
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

		private async Task RedrawPicture(Grid.GridPictCell cell)
		{
			var path = cell.Src;
			if (string.IsNullOrEmpty(path)) return;
			var img = await _photoReader.GetImgSrcAsync(path);
			cell.SrcBase64 = img;
		}

		private async Task LoadLocation(Grid.GridLocationCell locationCell)
		{
			if (locationCell.LocationLoaded)
			{
				return;
			}
			_logger.Debug($"Loading location for photo {locationCell.Row.PicturePath}...");
			if (!locationCell.Latitude.HasValue || !locationCell.Longitude.HasValue)
			{
				_logger.Debug($"The location cell has not latitude or longitude. Exiting.");
				locationCell.LocationLoaded = true;
				return;
			}
			try
			{
				var result = await _nominatimAgent.ReverseGeocodeAsync(new ReverseGeocodeRequest { Latitude = locationCell.Latitude.Value, Longitude = locationCell.Longitude.Value, Language = CultureInfo.CurrentCulture.Name });
				locationCell.Text = result.DisplayName;
				locationCell.LocationLoaded = true;
				var photos = (await _photoRepository.GetPhotosByCoordinatesAsync(locationCell.Latitude.Value, locationCell.Longitude.Value))
					.Where(p => p.LocalizedInfo != null && p.LocalizedInfo.Any(li => li.Language == CultureInfo.CurrentCulture.Name));
				if (photos.Any())
				{
					var localizedInfo = new PhotoLocalizedInfo { Language = CultureInfo.CurrentCulture.Name, Location = result.DisplayName };
					foreach (var photo in photos)
					{
						photo.LocalizedInfo.Add(localizedInfo);
						await _photoRepository.EditPhotoAsync(photo);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error when loading location.");
				locationCell.Text = _localizer["Error when retrieving location: {0}.", ex.Message];
			}
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



		private async Task<Dictionary<string, string>> GetMd5SignatureFromFilesAsync(List<string> filteredFiles, CancellationToken cancellationToken)
		{
			var md5SignaturesDict = new Dictionary<string, string>();
			_logger.Debug("Calculating MD5 sum for all supported files...");
			using (var semaphore = new SemaphoreSlim(200))
			{
				var tasksRetrievingMd5 = filteredFiles.Select(async i =>
				{
					if (cancellationToken.IsCancellationRequested)
					{
						throw new TaskCanceledException("The task was canceled.");
					}
					try
					{
						await semaphore.WaitAsync();
						return await _cryptoService.GetMd5FromFileAsync(i, cancellationToken);
					}
					finally
					{
						semaphore.Release();
					}
				}).ToList();
				await Task.WhenAll(tasksRetrievingMd5);
				for (var i = 0; i < filteredFiles.Count; i++)
				{
					md5SignaturesDict.Add(filteredFiles[i], tasksRetrievingMd5[i].Result);
				}
			}
			_logger.Debug("All mD5 sum calculated.");
			return md5SignaturesDict;
		}

		private async Task<PhotoRetrievalResult> GetPhotosFromFileOrDatabaseAsync(Dictionary<string, string> md5SignaturesDict, IEnumerable<Photo> storedPhotos)
		{
			var photosToAdd = new ConcurrentBag<Photo>();
			using var semaphore = new SemaphoreSlim(200);
			var tasksRetrievingPhotos = md5SignaturesDict.Select(async i =>
			{
				try
				{
					await semaphore.WaitAsync();
					var photo = storedPhotos.SingleOrDefault(p => p.Md5Sum == i.Value);
					if (photo != null)
					{
						_logger.Debug($"The photo with MD5 {i.Value} is in database. Returning this photo.");
						return photo;
					}
					_logger.Debug($"The photo with MD5 {i.Value} is not in the database. Getting the information from file metadata...");
					photo = await _photoInfoService.GetPhotoFromFileAsync(i.Key);
					photo.Md5Sum = i.Value;
					photosToAdd.Add(photo);
					return photo;
				}
				catch (Exception ex)
				{
					return new Photo
					{
						Md5Sum = i.Value,
						Path = i.Key,
						Error = new LoadPhotoException($"Error while loading the photo {i.Key}: {ex.Message}", i.Key, ex),
					};
				}
				finally
				{
					semaphore.Release();
				}
			}).ToList();
			await Task.WhenAll(tasksRetrievingPhotos);
			_logger.Debug("Finished photo retrieval.");
			return new PhotoRetrievalResult
			{
				Photos = tasksRetrievingPhotos.Select(t => t.Result).ToList(),
				PhotosToAddIntoDatabase = photosToAdd.ToList(),
			};
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
