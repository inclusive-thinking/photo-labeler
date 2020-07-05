using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PhotoLabeler.Components
{
	public partial class Grid
	{
		[Inject]
		public IJSRuntime jsRuntime { get; set; }

		[Parameter]
		public Entities.Grid Model { get; set; }

		[Parameter]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		private bool focusOnItemAfterRender = false;
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);
			if (focusOnItemAfterRender)
			{
				await jsRuntime.InvokeVoidAsync("jsInteropFunctions.focusSelectedItemInsideContainer", this.Id);
			}
		}
		private void RefreshGrid(bool focus)
		{
			focusOnItemAfterRender = focus;
			InvokeAsync(() => StateHasChanged());
		}
	}
}
