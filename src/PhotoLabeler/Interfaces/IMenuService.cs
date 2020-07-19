using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API.Entities;
using PhotoLabeler.Entities;

namespace PhotoLabeler.Interfaces
{
	/// <summary>
	/// Interface to manage application menus.
	/// </summary>
	public interface IMenuService
	{

		/// <summary>
		/// Occurs when a new language is selected
		/// </summary>
		event Func<LanguageEventArgs, Task> MnuLanguagesItemClick;

		/// <summary>
		/// Occurs when the Open Folder menu item has been clicked.
		/// </summary>
		event Func<Task> MnuFileOpenFolderClick;

		/// <summary>
		/// Creates the application menus.
		/// </summary>
		/// <returns></returns>
		void CreateMenus();
	}
}
