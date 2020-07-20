// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace PhotoLabeler.Components.Extensions
{
	public static class GridExtensions
	{
		public enum RowMovement
		{
			Up,
			Down
		}

		private static Entities.Grid.GridRow GetRow(Entities.Grid.GridRow row, RowMovement direction)
		{
			if (row is null)
			{
				throw new ArgumentNullException(nameof(row));
			}
			var allRows = row.Grid.AllRows;
			var rowIndex = allRows.IndexOf(row);
			if ((direction == RowMovement.Up && rowIndex == 0) || (direction == RowMovement.Down && rowIndex == allRows.Count - 1))
			{
				return null;
			}

			var newIndex = direction == RowMovement.Up ? rowIndex - 1 : rowIndex + 1;
			Entities.Grid.GridRow newRow;

			do
			{
				newRow = allRows[(direction == RowMovement.Up ? newIndex-- : newIndex++)];
			} while (!newRow.Visible && newIndex >= 0 && newIndex <= allRows.Count - 1);

			if (!newRow.Visible)
			{
				return null;
			}
			return newRow;
		}

		public static Entities.Grid.GridRow GetPreviousRow(this Entities.Grid.GridRow row)
		{
			if (row is null)
			{
				throw new ArgumentNullException(nameof(row));
			}
			return GetRow(row, RowMovement.Up);
		}

		public static Entities.Grid.GridRow GetNextRow(this Entities.Grid.GridRow row)
		{
			if (row is null)
			{
				throw new ArgumentNullException(nameof(row));
			}
			return GetRow(row, RowMovement.Down);
		}
	}
}
