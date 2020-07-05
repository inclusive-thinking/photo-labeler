using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoLabeler.Entities
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TreeView<T>
	{
		/// <summary>
		/// Gets the unique identifier.
		/// </summary>
		/// <value>
		/// The unique identifier.
		/// </value>
		public Guid UniqueId { get; } = Guid.NewGuid();


		/// <summary>
		/// Gets or sets the items.
		/// </summary>
		/// <value>
		/// The items.
		/// </value>
		public List<TreeViewItem<T>> Items { get; set; }

		/// <summary>
		/// Gets or sets the flat items.
		/// </summary>
		/// <value>
		/// The flat items.
		/// </value>
		public Lazy<List<TreeViewItem<T>>> FlatItems { get; set; }

		public TreeViewItem<T> SelectedItem { get; set; }


	}
}
