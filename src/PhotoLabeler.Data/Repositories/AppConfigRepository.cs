// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using PhotoLabeler.Data.Repositories.Interfaces;
using PhotoLabeler.Entities;

namespace PhotoLabeler.Data.Repositories
{
	/// <summary>
	/// App config repository class
	/// </summary>
	public class AppConfigRepository : RepositoryBase, IAppConfigRepository
	{

		private const string TableName = "AppConfig";

		/// <summary>
		/// Initializes a new instance of the <see cref="AppConfigRepository"/> class.
		/// </summary>
		/// <param name="connection">The connection.</param>
		public AppConfigRepository(IDbConnection connection)
			: base(connection)
		{
			CreateTableIfDoesNotExists(
				TableName,
				$"create table {TableName} (Key text not null primary key, Value text not null)");
		}


		/// <summary>
		/// Gets the application configuration by key asynchronous.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">key</exception>
		public Task<AppConfig> GetAppConfigByKeyAsync(string key)
		{
			if (key is null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			return GetAppConfigByKeyInternalAsync(key);
		}

		/// <summary>
		/// Sets the entry asynchronous.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public Task<bool> SetEntryAsync(string key, string value)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("The key cannot be null or empty.", nameof(key));
			}

			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("The value cannot be null or empty", nameof(value));
			}

			return SetEntryInternalAsync(key, value);
		}


		/// <summary>
		/// Gets the application configs.
		/// </summary>
		/// <returns></returns>
		public Task<IEnumerable<AppConfig>> GetAppConfigsAsync() => Connection.QueryAsync<AppConfig>($"select * from {TableName}");

		/// <summary>
		/// Refreshes the configuration asynchronous.
		/// </summary>
		/// <param name="entries">The entries.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">entries</exception>
		public Task RefreshConfigAsync(IEnumerable<AppConfig> entries)
		{
			if (entries is null)
			{
				throw new ArgumentNullException(nameof(entries));
			}

			return RefreshConfigInternalAsync(entries);
		}


		private async Task<AppConfig> GetAppConfigByKeyInternalAsync(string key)
		{
			var parameters = new DynamicParameters();
			parameters.Add("@key", key);
			return (await Connection.QueryAsync<AppConfig>($"select * from {TableName} where Key=@key", parameters)).SingleOrDefault();
		}

		private async Task<bool> SetEntryInternalAsync(string key, string value)
		{
			var parameters = new DynamicParameters();
			string query;

			parameters.Add("@key", key);
			parameters.Add("@value", value);
			var existingKey = await GetAppConfigByKeyAsync(key);
			if (existingKey == null)
			{
				query = $"insert into {TableName} (Key, Value) values (@key, @value)";
			}
			else
			{
				query = $"update {TableName} set value=@value where key=@key";
			}
			var affectedRows = await Connection.ExecuteAsync(query, parameters);
			return affectedRows > 0;
		}

		private async Task RefreshConfigInternalAsync(IEnumerable<AppConfig> entries)
		{
			var queryBuilder = new StringBuilder();
			var parameters = new DynamicParameters();

			var allConfig = await GetAppConfigsAsync();
			var paramOffset = 0;

			foreach (var entry in entries)
			{
				if (allConfig.Any(cf => cf.Key.Equals(entry.Key)))
				{
					queryBuilder.AppendLine($"update {TableName} set Value=@value{paramOffset} where Key=@key{paramOffset}");
				}
				else
				{
					queryBuilder.AppendLine($"insert into {TableName} values(@key{paramOffset}, @value{paramOffset})");
				}
				parameters.Add($"@key{paramOffset}", entry.Key);
				parameters.Add($"@value{paramOffset}", entry.Value);
				paramOffset++;
			}

			foreach (var entry in allConfig.Where(cf => !entries.Any(e => cf.Key == e.Key)))
			{
				queryBuilder.AppendLine($"Delete from {TableName} where key = @key{paramOffset}");
				parameters.Add($"@key{paramOffset}", entry.Key);
				paramOffset++;
			}

			_ = await Connection.ExecuteAsync(queryBuilder.ToString(), parameters);
		}
	}
}
