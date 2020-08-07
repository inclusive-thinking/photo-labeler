// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using PhotoLabeler.Nominatim.Agent;
using PhotoLabeler.ServiceLibrary.Implementations;

namespace PhotoLabeler.Console
{
	public static class Program
	{
		static async Task Main()
		{

			/*
			 * var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "./appconfig.db" };
			var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
			var photoRepository = new PhotoRepository(connection);
			var allPhotos = await photoRepository.GetAllPhotosAsync();
			// await photoRepository.AddPhotoAsync(new Photo { Path = "c:/tmp/juanjo.jpg", Label = "Juanjo en casa", Latitude = 44.222333444, Longitude = 4.2133322, AltitudeInMeters = 22, Md5Sum = "topota" });
			allPhotos = await photoRepository.GetAllPhotosAsync();
			allPhotos = await photoRepository.GetPhotosByMd5ListAsync("topota,tepete");
			*/

			var agent = new NominatimAgent(new Nominatim.Agent.Entities.NominatimAgentConfig { UriBase = "https://nominatim.openstreetmap.org" }, new HttpClient());
			var result = await agent.ReverseGeocodeAsync(new Nominatim.Agent.Entities.ReverseGeocodeRequest { Language = "es-ES", Latitude = 42.34234234323, Longitude = 2.334112223 });

			var service = new PhotoInfoService();
			var photo = await service.GetPhotoFromFileAsync(@"c:\users\jmontiel\google drive\fotos\img_0566.jpg");
			System.Console.ReadLine();
		}


	}
}
