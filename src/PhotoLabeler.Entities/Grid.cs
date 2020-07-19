// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace PhotoLabeler.Entities
{
	public class Grid
	{

		public enum Order
		{
			Ascending,
			Descending,
			Default
		}

		public abstract class GridCell
		{
			protected GridCell(int cellIndex, GridRow row, Grid grid)
			{
				CellIndex = cellIndex;
				Row = row;
				Grid = grid;
			}

			public bool Selected { get; set; }

			public int CellIndex { get; set; }

			public GridRow Row { get; set; }

			public Grid Grid { get; set; }

			public string Text { get; set; }
		}

		public class GridHeaderCell : GridCell
		{
			public GridHeaderCell(int cellIndex, GridRow row, Grid grid)
			 : base(cellIndex, row, grid)
			{
			}

			public Order Order { get; set; }
				= Order.Default;
		}

		public class GridCellLabel : GridCell
		{
			public GridCellLabel(int cellIndex, GridRow row, Grid grid)
				: base(cellIndex, row, grid)
			{
			}
		}

		public class GridCellLink : GridCell
		{
			public GridCellLink(int cellIndex, GridRow row, Grid grid)
				: base(cellIndex, row, grid)
			{
			}

			public string Link { get; set; }
		}

		public class GridCellFileName : GridCell
		{
			public GridCellFileName(int cellIndex, GridRow row, Grid grid)
				: base(cellIndex, row, grid)
			{
			}
		}

		public class GridCellTakenData : GridCell
		{
			public GridCellTakenData(int cellIndex, GridRow row, Grid grid
				) : base(cellIndex, row, grid)
			{
			}
		}

		public class GridCellPict : GridCell
		{
			public GridCellPict(int cellIndex, GridRow row, Grid grid)
				: base(cellIndex, row, grid)
			{
			}

			public string Src { get; set; }

			public string SrcBase64 { get; set; }
		}

		public class GridRow
		{
			public GridRow(int rowIndex, Grid grid)
			{
				RowIndex = rowIndex;
				Grid = grid;
			}

			public List<GridCell> Cells { get; set; } = new List<GridCell>();

			public int RowIndex { get; set; }

			public bool Visible { get; set; } = true;

			public Grid Grid { get; set; }


		}

		public class GridHeader
		{
			public GridRow Row { get; set; }
		}

		public class GridBody
		{
			public List<GridRow> Rows { get; set; } = new List<GridRow>();
		}

		public string Caption { get; set; }

		public GridHeader Header { get; set; }

		public GridBody Body { get; set; }

		public List<GridRow> AllRows
		{
			get
			{
				var allRowsTmp =
					Body?.Rows?.ToList() ??
					new List<GridRow>();

				if (Header?.Row != null)
				{
					allRowsTmp.Insert(0, Header.Row);
				}
				return allRowsTmp;
			}
		}
	}
}
