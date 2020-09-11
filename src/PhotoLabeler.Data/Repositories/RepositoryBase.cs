// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Data;
using System.Linq;
using Dapper;

namespace PhotoLabeler.Data.Repositories
{
	public abstract class RepositoryBase : IDisposable
	{

		private readonly IDbConnection _connection;

		protected IDbConnection Connection => _connection;

		protected RepositoryBase(IDbConnection connection)
		{
			_connection = connection;
			if (_connection.State != ConnectionState.Open)
			{
				_connection.Open();
			}
		}

		protected void CreateTableIfDoesNotExists(string tableName, string newTableCreationQuery)
		{
			string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName";
			var parameters = new DynamicParameters();
			parameters.Add("@tableName", tableName);
			var rows = _connection.Query<string>(query, parameters);
			if (!rows.Any())
			{
				_ = _connection.Execute(newTableCreationQuery);
			}
		}


		protected long GetLastInsertedRowid() => _connection.QuerySingle<long>("select last_insert_rowid()");

		public void Dispose()
		{
			Connection?.Dispose();
		}
	}
}
