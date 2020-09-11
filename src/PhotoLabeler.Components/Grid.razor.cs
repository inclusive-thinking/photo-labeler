// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using PhotoLabeler.Components.Extensions;
using Serilog;

namespace PhotoLabeler.Components
{
	internal class RenderedItem
	{

		public Entities.Grid.GridRow Model { get; set; }
 		public GridRow ComponentRef { get; set; }
	}

	public partial class Grid : IDisposable
	{
		[Inject] public IJSRuntime JSRuntime { get; set; }

		[Inject] public ILogger Logger { get; set; }

		[Inject] public IStringLocalizer<Grid> Localizer { get; set; }

		[Parameter]
		public Entities.Grid Model { get; set; }

		[Parameter]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		private bool _shouldRender = false;

		private string _accessibleMessage;

		private AccessibleAlert _accessibleAlertRef;

		private List<RenderedItem> _renderedItems;

		private uint _parmVersion = 0;

		private uint _parmVersionLastRendered = 0;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public void Cancel()
		{
			Log.Debug("Canceling current background operations...");
			_cancellationTokenSource.Cancel();
			Logger.Debug("cancellation token source cancelled.");
		}

		protected override bool ShouldRender() => _shouldRender;

		protected override Task OnParametersSetAsync()
		{
			Logger.Debug($"Entering {nameof(OnParametersSetAsync)}...");
			_parmVersion++;
			if (Model is null)
			{
				return Task.CompletedTask;
			}

			_renderedItems =
				Model
				.Body
				.Rows
				.Where(r => r.Visible)
				.Select(r => new RenderedItem { Model = r })
				.ToList();
			Logger.Debug($"Items to be rendered: {_renderedItems.Count}.");
			return Task.CompletedTask;
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			Logger.Debug("Entering in OnAfterRenderAsync...");
			await base.OnAfterRenderAsync(firstRender);
			await Task.Delay(1);
			_ = LoadImagesAndLocations();
			_shouldRender = false;
		}

		public void NotifyShouldRenderAfterSetParameter()
		{
			_shouldRender = true;
		}
		private async Task OnKeyDown(KeyboardEventArgs e)
		{
			Logger.Debug($"ONKeyDown on grid: {e.Key}.");
			var focus = false;
			Entities.Grid.GridCell newSelectedCell = null;
			var selectedCell = Model.SelectedCell;
			if (selectedCell is null)
			{
				selectedCell = Model.AllRows.SelectMany(r => r.Cells).SingleOrDefault(c => c.Selected);
			}

			switch (e.Key)
			{
				case "ArrowLeft":
					Logger.Debug("Pressed LeftArrow.");
					if (selectedCell.CellIndex == 0)
					{
						Logger.Debug("Left edge reached. Do nothing.");
						return;
					}
					newSelectedCell = selectedCell.Row.Cells[selectedCell.CellIndex - 1];
					break;
				case "ArrowRight":
					Logger.Debug("Pressed Right arrow.");
					if (selectedCell.CellIndex == selectedCell.Row.Cells.Count - 1)
					{
						Logger.Debug("Right edge reached. Do nothing.");
						return;
					}
					newSelectedCell = selectedCell.Row.Cells[selectedCell.CellIndex + 1];
					break;
				case "ArrowUp":
					Logger.Debug("Pressed Up arrow.");
					var previousRow = selectedCell.Row.GetPreviousRow();
					if (previousRow == null)
					{
						Logger.Debug("Top edge reached. Do nothing.");
						return;
					}
					newSelectedCell = previousRow.Cells[selectedCell.CellIndex];
					break;
				case "ArrowDown":
					Logger.Debug("Pressed Down arrow.");
					var nextRow = selectedCell.Row.GetNextRow();
					if (nextRow == null)
					{
						Logger.Debug("Botton edge reached. Do nothing.");
						return;
					}
					newSelectedCell = nextRow.Cells[selectedCell.CellIndex];
					break;
				default:
					Logger.Debug($"Pressed {e.Key}. Do nothing.");
					break;
			}

			if (newSelectedCell != null)
			{
				Logger.Debug($"New selected cell: Col {newSelectedCell.CellIndex}, row {newSelectedCell.Row.RowIndex}.");
				selectedCell.Selected = false;
				newSelectedCell.Selected = true;
				Model.SelectedCell = newSelectedCell;
				Model.PreviousSelectedCell = selectedCell;
				var selectedReference = _renderedItems.SingleOrDefault(it => it.Model.RowIndex == selectedCell.Row.RowIndex);
				var newSelectedReference = _renderedItems.SingleOrDefault(it => it.Model.RowIndex == newSelectedCell.Row.RowIndex);
				if (selectedReference?.ComponentRef != null)
				{
					await selectedReference.ComponentRef.NotifyShouldRender();
				}

				if (newSelectedReference?.ComponentRef != null && newSelectedReference != selectedReference)
				{
					await newSelectedReference.ComponentRef.NotifyShouldRender();
					await newSelectedReference.ComponentRef.ScrollIntoRow();
				}
				_accessibleAlertRef.Text = GetAriaText();
				_accessibleAlertRef.NotifyShouldRender();
			}
		}

		private async Task OnFocus()
		{
			_accessibleAlertRef.Text = GetAriaText(true);
			await _accessibleAlertRef.NotifyShouldRender();
		}


		private string GetAriaText(bool readAllInformation = false)
		{
			if (Model.SelectedCell is null)
			{
				if (readAllInformation)
				{
					return Localizer["Table with {0} columns and {1} rows.", Model.Header.Row.Cells.Count, Model.Body.Rows.Count(r => r.Visible)];
				}
				else
				{
					return string.Empty;
				}
			}

			string text;
			int currentColumn = Model.SelectedCell.CellIndex + 1, currentRow = Model.SelectedCell.Row.GetRowIndexWithFilters() + 1;
			var cellText = Model.SelectedCell.ToString().Trim(new[] { '.', ',', ':' });
			string order = string.Empty;
			var correspondingHeader = Model.Header.Row.Cells[currentColumn - 1];
			var headerText = correspondingHeader.ToString();
			var allInfoText = "{2}: {3}. Column {5}, row {6}.";
			if (Model.SelectedCell is Entities.Grid.GridHeaderCell headerCell)
			{
				order = headerCell.Order switch
				{
					Entities.Grid.Order.Default => string.Empty,
					Entities.Grid.Order.Ascending => "sorted ascending",
					Entities.Grid.Order.Descending => "sorted descending",
					_ => throw new ArgumentException(message: "Invalid enum value", paramName: nameof(headerCell.Order))
				};
				text = "{2}, column header" +
					(string.IsNullOrWhiteSpace(order) ? string.Empty : ", {4}") +
					". Column {5}.";
			}
			else
			{
				if (readAllInformation)
				{
					text = allInfoText;
				}
				else if (Model.PreviousSelectedCell != null)
				{
					int previousColumn = Model.PreviousSelectedCell.CellIndex + 1, previousRow = Model.PreviousSelectedCell.Row.RowIndex + 1;

					if (previousColumn != currentColumn)
					{
						if (previousRow == currentRow)
						{
							text = "{2}: {3}. Column {5}.";
						}
						else
						{
							text = allInfoText;
						}
					}
					else
					{
						if (previousRow != currentRow)
						{
							text = "{3}. Row {6}.";
						}
						else
						{
							text = allInfoText;
						}
					}
				}
				else
				{
					text = allInfoText;
				}
			}

			if (readAllInformation)
			{
				text = "Table with {0} columns and {1} rows. " + text.Trim();
			}

			return Localizer[
				text,
				Model.Header.Row.Cells.Count,
				Model.Body.Rows.Count(r => r.Visible),
				headerText,
				cellText,
				order,
				currentColumn,
				currentRow
				];
		}

		private async Task LoadImagesAndLocations()
		{
			Logger.Debug($"Entering in {nameof(LoadImagesAndLocations)}...");

			// current grid parameters version
			var loadingVersion = _parmVersion;

			// is loading needed?
			var noNeedToLoadImages = _parmVersion <= _parmVersionLastRendered;
			if (noNeedToLoadImages)
			{
				Logger.Debug($"{nameof(_parmVersion)}: {_parmVersion}. {nameof(_parmVersionLastRendered)}: {_parmVersionLastRendered}. {nameof(noNeedToLoadImages)}: {noNeedToLoadImages}.");
				return;
			}

			// local version of items
			var items = _renderedItems.ToList();

			// are references available?
			var referencesAreAvailable = items.FirstOrDefault()?.ComponentRef != null;
			if (!referencesAreAvailable)
			{
				Logger.Information("There are no references to use.");
				return;
			}

			_cancellationTokenSource.Cancel();
			_cancellationTokenSource = new CancellationTokenSource();
			Logger.Debug("Cancelled the previous operation.");
			// let's try to fill
			_parmVersionLastRendered = loadingVersion;
			var loadImagesTask = LoadImages(items, loadingVersion, _cancellationTokenSource.Token);
			var loadLocationsTask = LoadLocations(items, loadingVersion, _cancellationTokenSource.Token);
			await Task.WhenAll(loadImagesTask, loadLocationsTask);
		}

		private async Task LoadImages(List<RenderedItem> items, uint filling, CancellationToken cancellationToken)
		{
			Logger.Information("Starting image loading...");
			for (int i = 0; i < items.Count; i++)
			{
				var item = items[i];
				Logger.Debug($"Loading image {(i + 1)} of {items.Count}: {item.Model.PicturePath}...");
				var fillingWrongVersion = filling != _parmVersion;
				if (fillingWrongVersion || _disposed || cancellationToken.IsCancellationRequested)
				{
					var reason = GetCancellationReason(fillingWrongVersion);
					Logger.Debug($"Aborting image loading: {reason}");
					return;
				}
				await Task.Delay(1);
				await item.ComponentRef.ReloadImage();
			}
		}

		private async Task LoadLocations(List<RenderedItem> items, uint filling, CancellationToken cancellationToken)
		{
			Logger.Debug($"Entering in {nameof(LoadLocations)}. There are {items.Count} items to analize.");
			for (int i = 0; i < items.Count; i++)
			{
				var reference = items[i];
				Logger.Debug($"Loading location {reference.Model.PicturePath} (item {(i + 1)} of {items.Count})...");
				var fillingWrongVersion = filling != _parmVersion;
				if (fillingWrongVersion || _disposed || cancellationToken.IsCancellationRequested)
				{
					var reason = GetCancellationReason(fillingWrongVersion);
					Logger.Debug($"Aborting location loading: {reason}");
					return;
				}
				await reference.ComponentRef.ReloadLocation();
			}
		}

		private string GetCancellationReason(bool fillingWrongVersion)
		{
			string reason;
			if (fillingWrongVersion)
			{
				reason = "filling wrong rendered version.";
			}
			else if (_disposed)
			{
				reason = "The object was disposed.";
			}
			else
			{
				reason = "The operation was canceled.";
			}

			return reason;
		}

		private bool _disposed = false;

		public void Dispose() => Dispose(true);
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				_cancellationTokenSource.Cancel();
			}

			_disposed = true;
		}
	}
}
