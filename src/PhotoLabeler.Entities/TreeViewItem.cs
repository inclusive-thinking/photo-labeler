// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace PhotoLabeler.Entities
{
	public class TreeViewItem<T>
	{


		public List<T> Items { get; set; }

		public List<TreeViewItem<T>> Children { get; set; }

		public TreeViewItem<T> Parent { get; set; }

		public bool ItemsLoaded { get; set; }

		public string Path { get; set; }

		public string Name { get; set; }

		public bool Expanded { get; set; }

		public bool Selected { get; set; }

		public int Level { get; set; }

		public int ItemIndex { get; set; }
		public TreeView<T> TreeView { get; set; }




	}
}
