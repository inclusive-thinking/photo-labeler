// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PhotoLabeler.Components
{
	internal class VisibleItem
	{
		public Entities.Grid.GridRow Item { get; set; }
		public GridRow Reference { get; set; }
	}

	public partial class Grid : IDisposable
	{
		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		[Parameter]
		public Entities.Grid Model { get; set; }

		[Parameter]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		private bool _focusOnItemAfterRender = false;

		private List<VisibleItem> _visibleItems;

		private ulong _parmVersion = 0;

		private ulong _parmVersionLastRendered = 0;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public void Cancel() => _cancellationTokenSource.Cancel();

		protected override Task OnParametersSetAsync()
		{
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
			await base.OnAfterRenderAsync(firstRender);

			if (_focusOnItemAfterRender)
			{
				await JSRuntime.InvokeVoidAsync("jsInteropFunctions.focusSelectedItemInsideContainer", Id);
			}

			_cancellationTokenSource.Cancel();
			_cancellationTokenSource = new CancellationTokenSource();
			_ = RefillImagesAsync(_cancellationTokenSource.Token);
		}

		private async Task RefillImagesAsync(CancellationToken cancellationToken)
		{
			// current grid parameters version
			var filling = _parmVersion;

			// is refilling needed?
			var noNeedToRefillImages = _parmVersion <= _parmVersionLastRendered;
			if (noNeedToRefillImages) return;

			// local version of items
			var copyOfVisibleItems = _visibleItems.ToList();

			// are references available?
			var referencesAreAvailable = copyOfVisibleItems.FirstOrDefault()?.Reference != null;
			if (!referencesAreAvailable) return;

			// let's try to fill
			_parmVersionLastRendered = filling;
			foreach (var vi in copyOfVisibleItems)
			{
				var fillingWrongVersion = filling != _parmVersion;
				if (fillingWrongVersion || _disposed || cancellationToken.IsCancellationRequested) return;
				await Task.Delay(1);
				await vi.Reference.ReloadImage();
			}
		}

		private void RefreshGrid(bool focus)
		{
			_focusOnItemAfterRender = focus;
			InvokeAsync(() => StateHasChanged());
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
