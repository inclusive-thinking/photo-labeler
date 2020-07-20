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
		public Entities.Grid.GridRow item {get; set;} = null;
		public GridRow reference {get; set;} = null;
	}

	public partial class Grid: IDisposable
	{
		[Inject]
		public IJSRuntime jsRuntime { get; set; }

		[Parameter]
		public Entities.Grid Model { get; set; }

		[Parameter]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		private bool focusOnItemAfterRender = false;
		
		private List<VisibleItem> VisibleItems {set; get;}

		private ulong _parmVersion = 0;
		private ulong _parmVersionLastRendered = 0;
		CancellationTokenSource cts = new CancellationTokenSource();
		public void Cancel() => cts.Cancel();		
		protected override async Task OnParametersSetAsync()
		{			
			_parmVersion++;
			VisibleItems = 
				Model
				.Body
				.Rows
				.Where(r => r.Visible)
				.Select(r => new VisibleItem { item= r } )
				.ToList();						
			await Task.CompletedTask;
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);

			if (focusOnItemAfterRender)
			{
				await jsRuntime.InvokeVoidAsync("jsInteropFunctions.focusSelectedItemInsideContainer", this.Id);
			}

			cts.Cancel();		
			cts = new CancellationTokenSource();
			RefillImages(cts.Token);				

		}

        private async void RefillImages(CancellationToken myToken)
        {	
			// current grid parameters version
			var filling = _parmVersion;

			// is refilling needed?
			var noNeedToRefillImages = _parmVersion <= _parmVersionLastRendered;
			if (noNeedToRefillImages) return;

			// local version of items
			var copyOfVisibleItems = VisibleItems.ToList();
			
			// are references availables?
			var referencesAreAvailables = copyOfVisibleItems.FirstOrDefault()?.reference == null;
			if (referencesAreAvailables) return;

			// let's try to fill
			_parmVersionLastRendered=filling;
            foreach(var vi in copyOfVisibleItems)
			{
				var fillingWrongVersion = filling != _parmVersion;
				if (fillingWrongVersion || _disposed || myToken.IsCancellationRequested) return;
				await Task.Delay(1);
				await vi.reference.ReloadImage();				
			}
        }

        private void RefreshGrid(bool focus)
		{
			focusOnItemAfterRender = focus;
			InvokeAsync(() => StateHasChanged());
		}

		private bool _disposed = false;        

        public  void Dispose() => Dispose(true);
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				cts.Cancel();
			}

			_disposed = true;
		}

	}
}
