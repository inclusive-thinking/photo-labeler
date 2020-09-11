// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
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
using PhotoLabeler.Nominatim.Agent.Exceptions;
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

			public List<Geolocation> GeolocationsToAddIntoDatabase { get; set; }

		}

		private const int MaxFileNameLength = 260;
		private readonly IPhotoInfoService _photoInfoService;

		private readonly IStringLocalizer<PhotoLabelerService> _localizer;

		private readonly IPhotoReader _photoReader;

		private readonly INominatimAgent _nominatimAgent;

		private readonly IGeolocationRepository _geolocationRepository;

		private readonly IDebugService _debugService;

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
			IGeolocationRepository geolocationRepository,
			IDebugService debugService,
			ILogger logger
			)
		{
			_photoInfoService = photoInfoService;
			_localizer = localizer;
			_photoReader = photoReader;
			_nominatimAgent = nominatimAgent;
			_geolocationRepository = geolocationRepository;
			_debugService = debugService;
			_logger = logger;
		}

		/// <summary>
		/// Gets the photos from dir asynchronous.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="recursiveLoading">if set to <c>true</c> [load recursively].</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public Task<TreeView<Photo>> GetTreeViewFromDirAsync(string directory, bool recursiveLoading = false, CancellationToken cancellationToken = default)
		{
			_logger.Debug($"Enterin in GetTreeViewFromDirAsync. Directory: {directory}. Recursive loading: {recursiveLoading}.");
			return _debugService.MeasureExecutionAsync("GetTreeViewFromDirAsync", async () =>
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
						if (recursiveLoading)
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
						if (recursiveLoading)
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
			});
		}

		/// <summary>
		/// Gets the photos from dir asynchronous.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public async Task<IEnumerable<Photo>> GetPhotosFromDirAsync(string directory, CancellationToken cancellationToken)
		{
			_logger.Debug($"Enterin in GetPhotosFromDirAsync. Directory: {directory}.");
			var files = await Task.Run(() => Directory.GetFiles(directory, string.Empty, SearchOption.TopDirectoryOnly), cancellationToken);
			var supportedExtensions = new[] { ".jpg", ".heic", ".mov", ".png", ".gif", ".jpeg", ".tiff", ".raw", ".mp4" };
			var filteredFiles = files.Where(i => supportedExtensions.Contains(Path.GetExtension(i.ToLower()))).ToList();
			_logger.Debug($"{filteredFiles.Count} supported files found.");
			if (!filteredFiles.Any())
			{
				return new List<Photo>();
			}

			return await GetPhotosFromFilesAsync(filteredFiles, cancellationToken);
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
				var locationCell = new Grid.GridLocationCell(cellIndex: row.Cells.Count, row: row, grid: grid)
				{
					Latitude = photo.Latitude,
					Longitude = photo.Longitude
				};

				if (!photo.HasGPSInformation)
				{
					locationCell.Text = _localizer["No GPS information"];
					locationCell.LocationLoaded = true;
				}
				else if (!string.IsNullOrWhiteSpace(photo.LocationError))
				{
					locationCell.Text = locationCell.LocationError = photo.LocationError;
					locationCell.LocationLoaded = true;
				}
				else if (!string.IsNullOrWhiteSpace(photo.LocationInfo))
				{
					locationCell.Text = photo.LocationInfo;
					locationCell.LocationLoaded = true;
				}
				else
				{
					locationCell.Text = _localizer["Loading location..."];
					locationCell.Latitude = photo.Latitude;
					locationCell.Longitude = photo.Longitude;
					locationCell.LoadLocation = () => LoadLocation(locationCell);
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

		/// <summary>
		/// Loads the location for an specific location cell..
		/// </summary>
		/// <param name="locationCell">The location cell.</param>
		/// <returns><c>true</c> if the location has been loaded from external services, <c>false</c> if the location has been retrieved from database. This can be used to delay queries and avoid api penalties</returns>
		private async Task<bool> LoadLocation(Grid.GridLocationCell locationCell)
		{
			var picturePath = locationCell.Row.PicturePath;
			_logger.Debug($"Entering in LoadLocation for photo {picturePath}.");
			if (locationCell.LocationLoaded)
			{
				_logger.Debug($"The location for {picturePath} is already loaded. Exiting.");
				return false;
			}

			_logger.Debug($"Loading location for photo {picturePath}...");
			if (!locationCell.HasGPSInformation)
			{
				_logger.Debug($"The photo {picturePath} has not GPS information.");
				locationCell.LocationLoaded = true;
				return false;
			}
			try
			{
				var currentLanguage = CultureInfo.CurrentCulture.Name;

				var geolocation = await _geolocationRepository.GetGeolocationByCoordinatesAsync(locationCell.Latitude.Value, locationCell.Longitude.Value);
				if (geolocation != null && geolocation.LocalizedInfo.Any(l => l.Language == currentLanguage))
				{
					locationCell.Text = geolocation.LocalizedInfo.Single(l => l.Language == currentLanguage).Location;
					locationCell.LocationLoaded = true;
					return false;
				}
				var result = await _nominatimAgent.ReverseGeocodeAsync(new ReverseGeocodeRequest { Latitude = locationCell.Latitude.Value, Longitude = locationCell.Longitude.Value, Language = CultureInfo.CurrentCulture.Name });
				_logger.Debug($"Location retrieved from external API for {picturePath}: {result.DisplayName}.");
				locationCell.Text = result.DisplayName;
				locationCell.LocationLoaded = true;
				_logger.Debug($"Searching for this location ({locationCell.Latitude.Value}, {locationCell.Longitude.Value}) for photo {picturePath} into database...");
				if (geolocation is null)
				{
					_logger.Debug($"The location associated to {picturePath} does not exists into database.");
					geolocation = new Geolocation
					{
						Latitude = locationCell.Latitude.Value,
						Longitude = locationCell.Longitude.Value,
						LocalizedInfo = new List<GeolocationLocalizedInfo> {
							new GeolocationLocalizedInfo { Language = CultureInfo.CurrentCulture.Name, Location = result.DisplayName }
						}
					};
					await _geolocationRepository.AddGeolocationAsync(geolocation);
					_logger.Debug($"Location for {picturePath} ({locationCell.Latitude.Value}, {locationCell.Longitude.Value}) added into database.");
				}
				else
				{
					_logger.Debug($"Location {locationCell.Latitude.Value}, {locationCell.Longitude.Value} Already exists into database. Adding the information for {CultureInfo.CurrentCulture.Name}...");
					geolocation.LocalizedInfo.Add(new GeolocationLocalizedInfo { Language = CultureInfo.CurrentCulture.Name, Location = result.DisplayName });
					await _geolocationRepository.EditGeolocationAsync(geolocation);
					_logger.Debug($"Location for photo {picturePath} updated into database.");
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error when loading location.");
				locationCell.Text = _localizer["Error when retrieving location: {0}.", ex.Message];
				locationCell.LocationLoaded = true;
				// To avoid multiple queries to point with errors, we store into database the errors produced for a specific location
				// but only nominatim errors, not network errors which could be transient.
				if (ex is NominatimException nominatimException)
				{
					_logger.Warning($"Error while retrieving location due to nominatim issue: {nominatimException.Message}. Trying to add this error into database.");
					try
					{
						var geolocation = _geolocationRepository.GetGeolocationByCoordinatesAsync(locationCell.Latitude.Value, locationCell.Longitude.Value);
						if (geolocation is null)
						{
							await _geolocationRepository.AddGeolocationAsync(new Geolocation { Latitude = locationCell.Latitude.Value, Longitude = locationCell.Longitude.Value, Error = ex.Message });
						}
						// if the geolocation exists into database, it means that before now, we were able to retrieve the geolocation description for other language, so the error could be transient.
					}
					catch (Exception repoException)
					{
						_logger.Error(repoException, "Error when storing location with error into database.");
					}
				}
				return true;
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




		private Task<IEnumerable<Photo>> GetPhotosFromFilesAsync(IEnumerable<string> files, CancellationToken cancellationToken)
		{
			_logger.Debug($"Enterin in GetPhotosFromFilesAsync. {files.Count()} files will be loaded.");
			return _debugService.MeasureExecutionAsync("GetPhotosFromFilesAsync", async () =>
			{
				using var semaphore = new SemaphoreSlim(Environment.ProcessorCount * 30);
				var tasksRetrievingPhotos = files.Select(async f =>
				{
					CheckCancellationPending(cancellationToken);
					try
					{
						await semaphore.WaitAsync();
						_logger.Debug($"Retrieving metadatas from file {f}...");
						return await _photoInfoService.GetPhotoFromFileAsync(f, cancellationToken);
					}
					catch (Exception ex) when (!(ex is TaskCanceledException))
					{
						return new Photo
						{
							Path = f,
							Error = new LoadPhotoException($"Error while loading the photo {f}: {ex.Message}", f, ex),
						};
					}
					finally
					{
						semaphore.Release();
					}
				}).ToList();
				try
				{
					await Task.WhenAll(tasksRetrievingPhotos);
				}
				catch (Exception ex)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						throw new TaskCanceledException("The task was canceled.");
					}
					throw;
				}
				_logger.Debug("Finished photo retrieval.");
				var photos = tasksRetrievingPhotos.Select(t => t.Result);

				CheckCancellationPending(cancellationToken);

				// check existing locations
				var currentLanguage = CultureInfo.CurrentCulture.Name;
				var points = photos.Where(p => p.HasGPSInformation).Select(p => new GeolocationPoint { Latitude = p.Latitude.Value, Longitude = p.Longitude.Value }).Distinct().ToList();
				_logger.Debug($"Encontered {points.Count} distinct coordinates.");
				var existingGeolocations = (await _geolocationRepository.GetGeolocationsByCoordinatesListAsync(points)).Where(l => !string.IsNullOrWhiteSpace(l.Error) || l.LocalizedInfo.Any(li => li.Language == currentLanguage));
				_logger.Debug($"{existingGeolocations.Count()} found.");

				if (existingGeolocations.Any())
				{
					foreach (var existingGeolocation in existingGeolocations)
					{
						var photosWithGeo = photos.Where(p => p.HasGPSInformation && p.Latitude.Value == existingGeolocation.Latitude && p.Longitude.Value == existingGeolocation.Longitude).ToList();
						_logger.Debug($"Encontered {photosWithGeo.Count} photos for coordinates {existingGeolocation}.");
						if (photosWithGeo.Any())
						{
							photosWithGeo.ForEach(p =>
							{
								p.DbLocationExists = true;
								if (!string.IsNullOrWhiteSpace(existingGeolocation.Error))
								{
									p.LocationError = existingGeolocation.Error;
								}
								else
								{
									p.LocationInfo = existingGeolocation.LocalizedInfo.Single(l => l.Language == currentLanguage).Location;
								}
							});
						}
					}
				}
				return photos;
			});
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

			using (SemaphoreSlim semaphore = new SemaphoreSlim(Environment.ProcessorCount * 30))
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

		private void CheckCancellationPending(CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				throw new TaskCanceledException("Te task was canceled.");
			}
		}
		}
	}
