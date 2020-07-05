// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using PhotoLabeler.Entities;

namespace PhotoLabeler.Data.Repositories.Interfaces
{

	/// <summary>
	/// Exposes methods to get configuration entries
	/// </summary>
	public interface IAppConfigRepository
	{

		/// <summary>
		/// Gets the application configuration by key asynchronous.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		Task<AppConfig> GetAppConfigByKeyAsync(string key);

		/// <summary>
		/// Gets the application configs asynchronous.
		/// </summary>
		/// <returns></returns>
		Task<IEnumerable<AppConfig>> GetAppConfigsAsync();

		/// <summary>
		/// Refreshes the configuration asynchronous.
		/// </summary>
		/// <param name="entries">The entries.</param>
		/// <returns></returns>
		Task RefreshConfigAsync(IEnumerable<AppConfig> entries);

		/// <summary>
		/// Sets the entry asynchronous.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		Task<bool> SetEntryAsync(string key, string value);
	}
}
