// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Components;

namespace PhotoLabeler.Components
{
	public partial class AccessibleAlert
	{

		public enum AlertType
		{
			Alert,
			Assertive,
			Polite,
			Status
		}

		[Parameter]
		public string Text { get; set; }

		[Parameter]
		public AlertType Type { get; set; } = AlertType.Assertive;

		private bool _textRemoved = false;

		private Timer _timer;

		private bool _beforeFirstRender = true;

		private string GetText()
		{
			if (_textRemoved || _beforeFirstRender)
			{
				return string.Empty;
			}
			return Text;
		}


		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			_timer = new Timer();
			_timer.Interval = 300;
			_timer.Elapsed += async (sender, e) =>
			{
				_textRemoved = true;
				_timer.Stop();
				await InvokeAsync(() => { StateHasChanged(); });
			};
		}

		protected override async Task OnParametersSetAsync()
		{
			await base.OnParametersSetAsync();
			_textRemoved = false;
			if (Type == AlertType.Alert)
			{
				_timer.Start();
			}
			else
			{
				_timer.Stop();
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);
			if (firstRender)
			{
				_beforeFirstRender = false;
				StateHasChanged();
			}
		}

		public Task NotifyShouldRender()
		{
			return InvokeAsync(StateHasChanged);
		}
	}
}
