using System;
using System.Collections.Generic;
using System.Text;
using ElectronNET.API.Entities;

namespace PhotoLabeler.Entities
{
	/// <summary>
	/// Represents a Language event when someone changes language
	/// </summary>
	/// <seealso cref="System.EventArgs" />
	public class LanguageEventArgs : EventArgs
	{

		/// <summary>
		/// Gets or sets the clicked menu item.
		/// </summary>
		/// <value>
		/// The item.
		/// </value>
		public MenuItem Item { get; set; }

		/// <summary>
		/// Gets or sets the name of the culture.
		/// </summary>
		/// <value>
		/// The name of the culture.
		/// </value>
		public string CultureName { get; set; }
	}
}
