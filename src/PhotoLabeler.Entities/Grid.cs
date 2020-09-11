// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

			/// <summary>
			/// Converts to string.
			/// </summary>
			/// <returns>
			/// A <see cref="string" /> that represents this instance.
			/// </returns>
			public override string ToString() => Text ?? string.Empty;
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

		public class GridLabelCell : GridCell
		{
			public GridLabelCell(int cellIndex, GridRow row, Grid grid)
				: base(cellIndex, row, grid)
			{
			}

			public bool HasLabel { get; set; }
		}

		public class GridLinkCell : GridCell
		{
			public GridLinkCell(int cellIndex, GridRow row, Grid grid)
				: base(cellIndex, row, grid)
			{
			}

			public string Link { get; set; }
		}

		public class GridFileNameCell : GridCell
		{
			public GridFileNameCell(int cellIndex, GridRow row, Grid grid)
				: base(cellIndex, row, grid)
			{
			}
		}

		public class GridTakenDataCell : GridCell
		{
			public GridTakenDataCell(int cellIndex, GridRow row, Grid grid
				) : base(cellIndex, row, grid)
			{
			}
		}

		public class GridPictCell : GridCell
		{
			public GridPictCell(int cellIndex, GridRow row, Grid grid)
				: base(cellIndex, row, grid)
			{
			}

			public string Src { get; set; }

			public string SrcBase64 { get; set; }
			public Func<Task> ReloadImage { get; set; } = null;
		}

		public class GridLocationCell : GridCell
		{

			public GridLocationCell(int cellIndex, GridRow row, Grid grid)
				: base(cellIndex, row, grid)
			{
			}

			public double? Latitude { get; set; }

			public double? Longitude { get; set; }

			public bool HasGPSInformation => Latitude.HasValue && Longitude.HasValue;

			public bool LocationLoaded { get; set; }

			public Func<Task<bool>> LoadLocation { get; set; }

			public string LocationError { get; set; }
		}

		public class GridRow
		{
			public GridRow(int rowIndex, Grid grid)
			{
				RowIndex = rowIndex;
				Grid = grid;
			}

			public string PicturePath { get; set; } = null;

			public List<GridCell> Cells { get; set; } = new List<GridCell>();

			public int RowIndex { get; set; }

			public bool Visible { get; set; } = true;

			public Grid Grid { get; set; }

			public int GetRowIndexWithFilters() =>
				Grid.Body.Rows.Where(r => r.Visible).ToList().IndexOf(this);
		}

		public class GridHeader
		{
			public GridRow Row { get; set; }
		}

		public class GridBody
		{
			public List<GridRow> Rows { get; set; } = new List<GridRow>();
		}

		/// <summary>
		/// Gets or sets the caption.
		/// </summary>
		/// <value>
		/// The caption.
		/// </value>
		public string Caption { get; set; }

		/// <summary>
		/// Gets or sets the header.
		/// </summary>
		/// <value>
		/// The header.
		/// </value>
		public GridHeader Header { get; set; }

		/// <summary>
		/// Gets or sets the body.
		/// </summary>
		/// <value>
		/// The body.
		/// </value>
		public GridBody Body { get; set; }

		private bool _selectedCellInitialized = false;
		private GridCell _selectedCell;

		/// <summary>
		/// Gets the selected cell.
		/// </summary>
		/// <value>
		/// The selected cell.
		/// </value>
		public GridCell SelectedCell
		{
			get
			{
				if (!_selectedCellInitialized)
				{
					var allRows = AllRows;
					if (allRows is null)
					{
						return null;
					}
					var selectedCell = allRows.SelectMany(r => r.Cells).SingleOrDefault(c => c.Selected);
					if (selectedCell is null)
					{
						selectedCell = Header?.Row?.Cells?.First();
					}
					if (selectedCell != null)
					{
						_selectedCellInitialized = true;
						_selectedCell = selectedCell;
						return _selectedCell;
					}
					return null;
				}
				return _selectedCell;
			}
			set
			{
				_selectedCellInitialized = true;
				_selectedCell = value;
			}
		}

		/// <summary>
		/// Gets or sets the previous selected cell.
		/// </summary>
		/// <value>
		/// The previous selected cell.
		/// </value>
		public GridCell PreviousSelectedCell { get; set; }

		/// <summary>
		/// Gets or sets the errors produced while generating the grid.
		/// </summary>
		/// <value>
		/// The errors.
		/// </value>
		public AggregateException Errors { get; set; }

		/// <summary>
		/// Gets a value indicating whether this instance has errors.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance has errors; otherwise, <c>false</c>.
		/// </value>
		public bool HasErrors => Errors != null;

		/// <summary>
		/// Gets all grid rows.
		/// </summary>
		/// <value>
		/// All rows.
		/// </value>
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
