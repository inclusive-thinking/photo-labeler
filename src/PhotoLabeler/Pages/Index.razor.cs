// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Components;
using PhotoLabeler.Components.Extensions;
using PhotoLabeler.Entities;

namespace PhotoLabeler.Pages
{
	public partial class Index : IDisposable
	{
		private string _selectedFile;

		private TreeView<Photo> _treeViewItems;

		private Grid _gridData = null;


		private string _statusText = string.Empty;

		private Components.Grid _gridRef = null;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		protected override async Task OnInitializedAsync()
		{
			menuService.MnuFileOpenFolderClick += SelectDirectory;

			menuService.MnuLanguagesItemClick += SelectLanguage;

			menuService.CreateMenus();
			QuerySelectorToFocusAfterRendering = "button";
			await base.OnInitializedAsync();
		}

		private async Task SelectLanguage(LanguageEventArgs e)
		{
			await appConfigRepository.SetEntryAsync(Constants.ConfigConstants.LanguageConfigKey, e.CultureName);
			_cancellationTokenSource.Cancel();
			navigationManager.NavigateTo($"/Language/?cultureName={HttpUtility.UrlEncode(e.CultureName)}&redirectUri=%2F", true);
		}

		private async Task SelectDirectory()
		{
			try
			{
				var mainWindow = Electron.WindowManager.BrowserWindows.First();
				var options = new OpenDialogOptions
				{
					Properties = new OpenDialogProperty[] {
						OpenDialogProperty.openDirectory,
					},
					Title = localizer["Choose the directory which contains the photos"]
				};
				string[] files = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
				if (files.Length > 0)
				{
					_selectedFile = files[0];
					_cancellationTokenSource.Cancel();
					_cancellationTokenSource = new CancellationTokenSource();
					_statusText = localizer["Loading directories..."];
					try
					{
						_treeViewItems = await photoLabelerService.GetTreeViewFromDirAsync(_selectedFile, false, _cancellationTokenSource.Token);
					}
					catch (TaskCanceledException)
					{
						logger.Debug("The task to load all directories of a specified path has been canceled.");
						_statusText = localizer["Operation canceled."];
						return;
					}
					_statusText = localizer["Directories loaded"];
					QuerySelectorToFocusAfterRendering = "#treeViewPhotos [tabindex=\"0\"]";
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex, "Error when opening a folder.");
				_ = await Electron.Dialog.ShowMessageBoxAsync(new MessageBoxOptions(localizer["Error when opening the folder: {0}", ex.Message])
				{
					Type = MessageBoxType.error
				});
			}
		}

		private void ExitApp()
		{
			_cancellationTokenSource.Cancel();
			Electron.App.Quit();
		}

		private async Task Item_Selected(TreeViewItem<Photo> item)
		{
			try
			{
				// clear previous data
				_gridRef?.Cancel();
				_cancellationTokenSource.Cancel();
				_gridData = null;
				StateHasChanged();
				await Task.Delay(200);
				_cancellationTokenSource = new CancellationTokenSource();

				// new data
				_gridRef?.NotifyShouldRenderAfterSetParameter();
				_statusText = localizer["Loading photos metadata..."];
				StateHasChanged();
				_gridData = await photoLabelerService.GetGridFromTreeViewItemAsync(item, _cancellationTokenSource.Token);
				_statusText = localizer["Metadata loaded."];
				if (_gridData.HasErrors)
				{
					var errorsCount = _gridData.Errors.InnerExceptions.Count();
					var photosLoadedSuccessfully = _gridData.AllRows.Count() - errorsCount;
					await CreateErrorDialogAsync(
						title: localizer["Error loading some photos."],
						text: localizer["There were {0} errors while loading the photos. {1} photos were loaded correctly.", errorsCount, photosLoadedSuccessfully]
						);
				}
			}
			catch (TaskCanceledException)
			{
				logger.Debug("The task to retrieve all photos from a directory has been canceled.");
			}
			catch (Exception ex)
			{
				logger.Error(ex, "Error when retrieving photo list.");
				await CreateErrorDialogAsync(
					title: localizer["Error while getting the photos."],
					text: localizer["Error getting the photos from the directory: {0}.", ex.Message]
					);
			}
		}

		private Task CreateErrorDialogAsync(string title, string text)
		{
			var options = new MessageBoxOptions(text)
			{
				Type = MessageBoxType.error,
				Title = title,
			};
			return Electron.Dialog.ShowMessageBoxAsync(options);
		}

		private void CheckPhotoFilter(ChangeEventArgs e)
		{
			if (_gridData != null && _gridData.Body.Rows.Any())
			{
				if (e.Value is bool bValue && bValue)
				{
					_gridData.Body.Rows.ForEach(r =>
					{
						var labelCell = r.Cells.Single(c => c is Grid.GridLabelCell) as Grid.GridLabelCell;
						if (!labelCell.HasLabel)
						{
							r.Visible = false;
							var selectedCell = r.Cells.SingleOrDefault(c => c.Selected);
							if (selectedCell != null)
							{
								var otherRow = r.GetPreviousRow();
								if (r is null)
								{
									otherRow = r.GetNextRow();
								}
								if (otherRow == null)
								{
									otherRow = r.Grid.Header.Row;
								}
								selectedCell.Selected = false;
								otherRow.Cells[selectedCell.CellIndex].Selected = true;
							}
						}
					});
				}
				else
				{
					_gridData.Body.Rows.ForEach(r => r.Visible = true);
				}
			}
		}

		private async Task RenamePhotosAsync()
		{
			var numberOfLabeledPhotos = _treeViewItems.SelectedItem.Items.Count(i => !string.IsNullOrWhiteSpace(i.Label));
			if (numberOfLabeledPhotos == 0)
			{
				var options = new MessageBoxOptions(localizer["There are no photos labeled for renaming."])
				{
					Type = MessageBoxType.warning,
				};
				_ = await Electron.Dialog.ShowMessageBoxAsync(options);
				return;
			}
			var renamingOptions = new MessageBoxOptions(
				localizer["{0} photos are to be renamed. " +
				"Do you wish to continue? Please note that the original names of the photos will be changed " +
				"and the operation cannot be undone.", numberOfLabeledPhotos])
			{
				Type = MessageBoxType.question,
				Buttons = new string[] { localizer["Rename photos"], localizer["Cancel"] },
				Title = localizer["Question"],
			};
			var result = await Electron.Dialog.ShowMessageBoxAsync(renamingOptions);
			if (result.Response == 0)
			{
				string operationResultMessage;
				var renamingResult = await photoLabelerService.RenamePhotosInFolder(_treeViewItems.SelectedItem);
				if (renamingResult.ErrorCount > 0)
				{
					operationResultMessage = localizer["Operation performed with errors. {0} photos have been renamed, and  {1}. Error/s have occurred: {2}.",
				   renamingResult.FilesRenamed, renamingResult.ErrorCount,
				   string.Join(Environment.NewLine, renamingResult.Errors)];
				}
				else
				{
					operationResultMessage = localizer["Operation successfully completed! {0} photos have been renamed.", renamingResult.FilesRenamed];
				}

				var resultDialogOptions = new MessageBoxOptions(operationResultMessage)
				{
					Type = renamingResult.ErrorCount > 0 ? MessageBoxType.warning : MessageBoxType.info,
					Title = localizer[renamingResult.ErrorCount > 0 ? "Operation performed with errors" : "Done!"],
				};
				_ = await Electron.Dialog.ShowMessageBoxAsync(resultDialogOptions);
				_treeViewItems = null;
				_gridData = null;
			}
		}

		private bool _disposed = false;
		public override void Dispose() => Dispose(true);
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				menuService.MnuFileOpenFolderClick -= SelectDirectory;
				menuService.MnuLanguagesItemClick -= SelectLanguage;
				base.Dispose();
			}

			_disposed = true;
		}



	}
}
