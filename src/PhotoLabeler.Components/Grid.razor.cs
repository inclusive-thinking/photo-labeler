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
	internal class VisibleItem
	{
		public Entities.Grid.GridRow Item { get; set; }
		public GridRow Reference { get; set; }
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

		private bool _focusOnItemAfterRender = false;

		private AccessibleAlert _accessibleAlertRef;

		private bool _shouldRender = false;

		private List<VisibleItem> _visibleItems;

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
			Logger.Debug("Entering OnParameterSetAsync...");
			_parmVersion++;
			_visibleItems =
				Model
				.Body
				.Rows
				.Where(r => r.Visible)
				.Select(r => new VisibleItem { Item = r })
				.ToList();
			return Task.CompletedTask;
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			Logger.Debug("Entering in OnAfterRenderAsync...");
			await base.OnAfterRenderAsync(firstRender);

			if (_focusOnItemAfterRender)
			{
				await JSRuntime.InvokeVoidAsync("jsInteropFunctions.focusSelectedItemInsideContainer", Id);
			}

			_ = RefillImagesAndLocations();
			_shouldRender = false;
		}

		private async Task OnKeyDown(KeyboardEventArgs e)
		{
			Logger.Debug($"ONKeyDown in grid: {e.Key}.");
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
				var selectedReference = _visibleItems.SingleOrDefault(it => it.Item.RowIndex == selectedCell.Row.RowIndex);
				var newSelectedReference = _visibleItems.SingleOrDefault(it => it.Item.RowIndex == newSelectedCell.Row.RowIndex);
				if (selectedReference?.Reference != null)
				{
					await selectedReference.Reference.NotifyShouldRender();
				}

				if (newSelectedReference?.Reference != null && newSelectedReference != selectedReference)
				{
					await newSelectedReference.Reference.NotifyShouldRender();
				}
				_accessibleAlertRef.Text = GetAriaText();
				_accessibleAlertRef.NotifyShouldRender();
				// await _accessibleAlertRef.NotifyShouldRender();
			}
		}
		private string GetAriaText(bool readAllInformation = false)
		{
			if (Model.SelectedCell is null)
			{
				if (readAllInformation)
				{
					return Localizer["Table with {0} columns and {1} rows.", Model.Header.Row.Cells.Count, Model.Body.Rows.Count];
				}
				else
				{
					return string.Empty;
				}
			}

			string text;
			int currentColumn = Model.SelectedCell.CellIndex + 1, currentRow = Model.SelectedCell.Row.RowIndex + 1;
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
				Model.Body.Rows.Count,
				headerText,
				cellText,
				order,
				currentColumn,
				currentRow
				];
		}

		private async Task RefillImagesAndLocations()
		{
			Logger.Debug("Entering in RefillImagesAsync...");

			// current grid parameters version
			var filling = _parmVersion;

			// is refilling needed?
			var noNeedToRefillImages = _parmVersion <= _parmVersionLastRendered;
			if (noNeedToRefillImages)
			{
				Logger.Debug($"{nameof(_parmVersion)}: {_parmVersion}. {nameof(_parmVersionLastRendered)}: {_parmVersionLastRendered}. {nameof(noNeedToRefillImages)}: {noNeedToRefillImages}.");
				return;
			}

			// local version of items
			var copyOfVisibleItems = _visibleItems.ToList();

			// are references available?
			var referencesAreAvailable = copyOfVisibleItems.FirstOrDefault()?.Reference != null;
			if (!referencesAreAvailable)
			{
				Logger.Information("There are no references to use.");
				return;
			}

			_cancellationTokenSource.Cancel();
			_cancellationTokenSource = new CancellationTokenSource();
			Logger.Debug("Cancelled the previous operation.");
			// let's try to fill
			_parmVersionLastRendered = filling;
			var loadImagesTask = LoadImages(copyOfVisibleItems, filling, _cancellationTokenSource.Token);
			var loadLocationsTask = LoadLocations(copyOfVisibleItems, filling, _cancellationTokenSource.Token);
			await Task.WhenAll(loadImagesTask, loadLocationsTask);
		}

		private async Task LoadImages(List<VisibleItem> copyOfVisibleItems, uint filling, CancellationToken cancellationToken)
		{
			Logger.Information("Starting image loading...");
			foreach (var vi in copyOfVisibleItems)
			{
				Logger.Debug($"Loading image {vi.Item.PicturePath}...");
				var fillingWrongVersion = filling != _parmVersion;
				if (fillingWrongVersion || _disposed || cancellationToken.IsCancellationRequested)
				{
					var reason = GetCancellationReason(fillingWrongVersion);
					Logger.Debug($"Aborting image loading: {reason}");
					return;
				}
				await Task.Delay(1);
				await vi.Reference.ReloadImage();
			}
		}

		private async Task LoadLocations(List<VisibleItem> copyOfVisibleItems, uint filling, CancellationToken cancellationToken)
		{
			Logger.Debug("Starting location loading...");
			foreach (var vi in copyOfVisibleItems)
			{
				Logger.Debug($"Loading image {vi.Item.PicturePath}...");
				var fillingWrongVersion = filling != _parmVersion;
				if (fillingWrongVersion || _disposed || cancellationToken.IsCancellationRequested)
				{
					var reason = GetCancellationReason(fillingWrongVersion);
					Logger.Debug($"Aborting image loading: {reason}");
					return;
				}
				await vi.Reference.ReloadLocation();
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
