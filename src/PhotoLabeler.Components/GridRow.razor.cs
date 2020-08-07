// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace PhotoLabeler.Components
{
	public partial class GridRow
	{
		[Parameter] public Entities.Grid.GridRow Row { get; set; }

		public async Task ReloadImage()
		{
			foreach (var cell in Row.Cells)
			{
				if (cell is Entities.Grid.GridPictCell cellPicct)
				{
					await cellPicct.ReloadImage();
					await InvokeAsync(StateHasChanged);
				}
			}
		}

		public async Task ReloadLocation()
		{
			var locationCell = Row.Cells.FirstOrDefault(c => c is Entities.Grid.GridLocationCell) as Entities.Grid.GridLocationCell;
			if (locationCell != null && locationCell.LoadLocation != null)
			{
				await locationCell.LoadLocation?.Invoke(locationCell);
				await InvokeAsync(StateHasChanged);
			}
		}

		public Task NotifyShouldRender()
		{
			return InvokeAsync(StateHasChanged);
		}

		}
	}
