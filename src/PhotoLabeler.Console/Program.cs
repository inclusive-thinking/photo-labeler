using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MetadataExtractor;
using MetadataExtractor.Formats.Xmp;
using PhotoLabeler.ServiceLibrary;

namespace PhotoLabeler.Console
{
	class Program
	{
		static void Main()
		{
			var data = MetadataExtractor.ImageMetadataReader.ReadMetadata(@"C:\Users\jmontiel\Documents\MEGA\tonterías\26. Escaneo de Juanjo con visión artificial..jpg");
			var gpsData = data.SingleOrDefault(d => d.Name == "GPS");
			if (gpsData != null)
			{
				var latitudeTag = gpsData.Tags.SingleOrDefault(t => t.Name == "GPS Latitude");
				var longitudeTag = gpsData.Tags.SingleOrDefault(t => t.Name == "GPS Longitude");
				var altitudeTag = gpsData.Tags.SingleOrDefault(t => t.Name == "GPS Altitude");
				if (latitudeTag != null)
				{
					var latitude = GetCoordinateFromDegrees(latitudeTag);
				}

				if (longitudeTag != null)
				{
					var longitude = GetCoordinateFromDegrees(longitudeTag);
				}
			}

			var xmpData = data.SingleOrDefault(d => d.Name == "XMP");
			if (xmpData != null)
			{
				var xmpDirectory = xmpData as XmpDirectory;
				var artworkDesc = xmpDirectory.XmpMeta.Properties.SingleOrDefault(p => p.Path == "Iptc4xmpExt:ArtworkContentDescription");
				if (artworkDesc != null)
				{
					System.Console.WriteLine(artworkDesc.Value);
				}
			}
			System.Console.ReadLine();
		}

		private static double GetCoordinateFromDegrees(Tag tag)
		{
			var regexDegrees = new Regex(@"^(?<hours>\-?\d+)°\s(?<minutes>[\d,]+?)'\s(?<seconds>[\d\,]+)", RegexOptions.Compiled);
			var cultureInfo = CultureInfo.CreateSpecificCulture("en-US");
			var fmt = new NumberFormatInfo();
			fmt.NegativeSign = "-";
			fmt.NumberDecimalSeparator = ",";
			var tagString = tag.Description;
			var result = regexDegrees.Match(tagString);
			var hours = double.Parse(result.Groups["hours"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, fmt);
			var minutes = double.Parse(result.Groups["minutes"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, fmt);
			var seconds = double.Parse(result.Groups["seconds"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, fmt);
			return hours + (minutes / 60) + (seconds / 3600);
		}

	}
}
