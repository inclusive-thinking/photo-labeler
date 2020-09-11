// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PhotoLabeler.Data.Interfaces;
using PhotoLabeler.Data.Repositories;
using PhotoLabeler.Data.Repositories.Interfaces;
using PhotoLabeler.Entities;
using PhotoLabeler.Nominatim.Agent;
using PhotoLabeler.Nominatim.Agent.Entities;
using PhotoLabeler.PhotoStorageReader.Implementations;
using PhotoLabeler.PhotoStorageReader.Interfaces;
using PhotoLabeler.ServiceLibrary.Implementations;
using PhotoLabeler.ServiceLibrary.Interfaces;

namespace PhotoLabeler.Console
{
	public static class Program
	{
		static async Task Main()
		{
			var services = ConfigureServices();
			var provider = services.BuildServiceProvider();
			var photoLabelerService = provider.GetRequiredService<IPhotoLabelerService>();
			var grid = await photoLabelerService.GetGridFromTreeViewItemAsync(new TreeViewItem<Photo> { Path = @"c:\tmp\dcim\dcim\100apple" }, CancellationToken.None);
			var rowsWithGps = grid.Body.Rows.Where(r => r.Cells.Any(c =>
			{
				if (c is Grid.GridLocationCell lc)
				{
					return lc.HasGPSInformation;
				}
				return false;
			}));
			var rowsWithErrors = rowsWithGps.Where(r => r.Cells.Any(c =>
			{
				if (c is Grid.GridLocationCell lc)
				{
					return !string.IsNullOrWhiteSpace(lc.LocationError);
				}
				return false;
			}));



			foreach (var r in rowsWithGps)
			{
				var locationCell = r.Cells.Single(c => c is Grid.GridLocationCell) as Grid.GridLocationCell;
				if (!locationCell.HasGPSInformation || locationCell.LocationLoaded)
				{
					continue;
				}
				var fromExternalApi = await locationCell.LoadLocation();
				if (fromExternalApi)
				{
					await Task.Delay(1000);
				}
			}

			System.Console.ReadLine();
		}

		private static IServiceCollection ConfigureServices()
		{
			var services = new ServiceCollection();
			var configurationBuilder = new ConfigurationBuilder();
			configurationBuilder.AddJsonFile("appsettings.json");
			var configuration = configurationBuilder.Build();
			services.AddLocalization();
			services.AddLogging();
			services.AddSingleton(typeof(Serilog.ILogger), (provider) => new Mock<Serilog.ILogger>().Object);
			services.AddHttpClient();
			services.AddHttpClient();
			services.AddTransient(typeof(HttpClient), (serviceProvider) =>
			{
				return serviceProvider.GetService<IHttpClientFactory>().CreateClient();
			});


			services.AddSingleton<IPhotoLabelerService, PhotoLabelerService>();
			services.AddSingleton<IPhotoInfoService, PhotoInfoService>();
			services.AddSingleton<IPhotoReader, PhotoReaderBase64>();
			services.AddSingleton<INominatimAgent, NominatimAgent>();
			services.AddSingleton<IDbConnection>((serviceProvider) =>
			{
				var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "./PhotoLabeler.db" };
				var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
				return connection;
			});
			services.AddSingleton<IAppConfigRepository, AppConfigRepository>();
			services.AddSingleton<IGeolocationRepository, GeolocationRepository>();
			services.AddSingleton(typeof(NominatimAgentConfig), (provider) =>
			{
				var cf = new NominatimAgentConfig();
				configuration.GetSection("Nominatim").Bind(cf);
				return cf;
			});
			return services;
		}
	}
}
