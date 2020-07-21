// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace PhotoLabeler.Components
{
	public partial class GridRow
	{
		[Parameter] public Entities.Grid.GridRow Row { get; set; }

		[Parameter] public Action<bool> RefreshGrid { get; set; }

		public async Task ReloadImage()
		{
			foreach (var cell in Row.Cells)
			{
				if (cell is Entities.Grid.GridCellPict cellPicct)
				{
					await cellPicct.ReloadImage();
					await InvokeAsync(StateHasChanged);
				}
			}
		}

	}
}
