using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PhotoLabeler.Components
{
	public partial class Grid: IDisposable
	{
		[Inject]
		public IJSRuntime jsRuntime { get; set; }

		[Parameter]
		public Entities.Grid Model { get; set; }

		[Parameter]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		private bool focusOnItemAfterRender = false;
		
		private List<(Entities.Grid.GridRow row, GridRow reference)> VisibleItems {set; get;}

		protected override async Task OnParametersSetAsync()
		{
			VisibleItems = 
				Model
				.Body
				.Rows
				.Where(r => r.Visible)
				.Select(r => (r, (GridRow)null ) )
				.ToList();			
			newParm = true;
			await Task.CompletedTask;
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);
			if (focusOnItemAfterRender)
			{
				await jsRuntime.InvokeVoidAsync("jsInteropFunctions.focusSelectedItemInsideContainer", this.Id);
			}
			if (newParm)
			{
				newParm= false;
				await RefillImages();				
			}
		}

        private async Task RefillImages()
        {
            foreach(var row in Model.Body.Rows.Where(r => r.Visible).ToList())
			{
				var cell = row.Cells.First() as Entities.Grid.GridCellPict;
				await cell.ReloadImage();
				if (_disposed) return;
				StateHasChanged();
			}
        }

        private void RefreshGrid(bool focus)
		{
			focusOnItemAfterRender = focus;
			InvokeAsync(() => StateHasChanged());
		}

		private bool _disposed = false;
        private bool newParm = false;

        public  void Dispose() => Dispose(true);
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				
			}

			_disposed = true;
		}

	}
}
