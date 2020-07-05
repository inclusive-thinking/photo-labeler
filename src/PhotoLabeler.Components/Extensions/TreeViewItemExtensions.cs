using System;
using System.Collections.Generic;
using System.Text;
using PhotoLabeler.Entities;

namespace PhotoLabeler.Components.Extensions
{
	public static class TreeViewItemExtensions
	{

		public static bool IsLast<T>(this TreeViewItem<T> item)
		{
			if (item.Parent != null)
			{
				return item.ItemIndex >= item.Parent.Children.Count - 1;
			}
			return item.ItemIndex >= item.TreeView.Items.Count - 1;
		}

		public static bool IsVisible<T>(this TreeViewItem<T> item)
		{
			if (item.Parent == null)
			{
				return true;
			}

			var iteratingItem = item.Parent;
			while (iteratingItem != null)
			{
				if (!iteratingItem.Expanded)
				{
					return false;
				}
				iteratingItem = iteratingItem.Parent;
			}
			return true;
		}

	}
}
