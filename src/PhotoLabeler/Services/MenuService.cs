// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Localization;
using PhotoLabeler.Entities;
using PhotoLabeler.Interfaces;

namespace PhotoLabeler.Services
{
	/// <summary>
	/// Service to manage application menus.
	/// </summary>
	public class MenuService : IMenuService
	{

		private readonly IStringLocalizer<MenuService> _localizer;

		private readonly RequestLocalizationOptions _localizationOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="MenuService" /> class.
		/// </summary>
		/// <param name="localizer">The localizer.</param>
		/// <param name="localizationOptions">The localization options.</param>
		public MenuService(IStringLocalizer<MenuService> localizer, RequestLocalizationOptions localizationOptions)
		{
			_localizer = localizer;
			_localizationOptions = localizationOptions;
		}

		/// <summary>
		/// Occurs when a new language is selected
		/// </summary>
		public event Func<LanguageEventArgs, Task> MnuLanguagesItemClick;

		/// <summary>
		/// Occurs when the Open Folder menu item has been clicked.
		/// </summary>
		public event Func<Task> MnuFileOpenFolderClick;

		/// <summary>
		/// Creates the application menus.
		/// </summary>
		/// <returns></returns>
		public void CreateMenus()
		{
			if (HybridSupport.IsElectronActive)
			{
				var languageMenuItems = new List<MenuItem>();
				for (int i = 0; i < _localizationOptions.SupportedCultures.Count; i++)
				{
					var culture = _localizationOptions.SupportedCultures[i];
					var currentCulture = CultureInfo.CurrentCulture.Name == culture.Name;
					var languageItem = new MenuItem
					{
						Label = culture.NativeName,
						Type = MenuType.checkbox,
						Checked = currentCulture,
					};
					languageItem.Click = async () =>
					{
						await OnLanguageChange(new LanguageEventArgs { Item = languageItem, CultureName = culture.Name });
					};
					languageMenuItems.Add(languageItem);
				}

				var menuItems = new List<MenuItem>()
				{
					new MenuItem
					{
						Label = _localizer["File"],
						Accelerator = _localizer["Alt+F"],
						Type = MenuType.submenu,
						Submenu = new MenuItem[]
						{
							new MenuItem
							{
								Label = _localizer["Open folder..."],
								Accelerator = "CmdOrCtrl+O",
								Click = async () => { await OnOpenFolder(); }
							},
							new MenuItem
							{
								Label = _localizer["Exit"],
								Role = MenuRole.quit,
							},
						},
					},
					new MenuItem
					{
						Label = _localizer["Language"],
						Type = MenuType.submenu,
						Submenu = languageMenuItems.ToArray()
					}
				};
#if DEBUG
				menuItems.Add(new MenuItem
				{
					Label = _localizer["Debug"],
					Type = MenuType.submenu,
					Submenu = new[]
					{
						new MenuItem
						{
							Label = _localizer["Developer tools..."],
							Accelerator = "f12",
							Click = () => { Electron.WindowManager.BrowserWindows.First().WebContents.OpenDevTools(); }
						},
					}
				});
#endif

				Electron.Menu.SetApplicationMenu(menuItems.ToArray());
			}
		}

		private Task OnLanguageChange(LanguageEventArgs e)
		{
			if (e.Item.Checked)
			{
				return Task.CompletedTask;
			}
			e.Item.Checked = true;
			return MnuLanguagesItemClick?.Invoke(e);
		}

		private Task OnOpenFolder()
		{
			return MnuFileOpenFolderClick?.Invoke();
		}
	}
}
