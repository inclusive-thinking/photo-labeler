// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MetadataExtractor;
using MetadataExtractor.Formats.Xmp;
using PhotoLabeler.Entities;
using PhotoLabeler.ServiceLibrary.Interfaces;

namespace PhotoLabeler.ServiceLibrary.Implementations
{
	public class PhotoInfoService : IPhotoInfoService
	{

		private const string ExifDateTimeFormat = "yyyy-MM-dd HH:mm:SS";

		private const string QuickTimeMetaDataCreationDateTag = "Creation Date";

		private const string QuickTimeMetadataDescriptionTag = "Description";

		private const string GPSDirectoryName = "GPS";

		private const string GPSLatitudeTagName = "GPS Latitude";

		private const string GPSLongitudeTagName = "GPS Longitude";

		private const string GPSAltitudeTagName = "GPS Altitude";

		private readonly Regex _regexDegrees = new Regex(@"^(?<hours>\-?\d+)°\s(?<minutes>[\d,]+?)'\s(?<seconds>[\d\,]+)", RegexOptions.Compiled);

		private readonly Regex _regexAltitude = new Regex(@"^(?<altitude>\-?\d+\.?\d+) metres?", RegexOptions.Compiled);

		private readonly NumberFormatInfo _pointFormatInfo = new NumberFormatInfo
		{
			NegativeSign = "-",
			NumberDecimalSeparator = "."
		};

		private readonly NumberFormatInfo _comaFormatInfo = new NumberFormatInfo
		{
			NegativeSign = "-",
			NumberDecimalSeparator = ","
		};


		/// <summary>
		/// Gets the photo from file asynchronous.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <returns></returns>
		public async Task<Photo> GetPhotoFromFileAsync(string file)
		{
			List<string> labels = new List<string>();
			IReadOnlyList<Directory> data = null;
			data = ImageMetadataReader.ReadMetadata(file);

			var photo = new Photo { Path = file };
			data.Where(d => d.Name == "XMP").ToList().ForEach(xmpData =>
			{
				AddLabelFromXmpDescription(labels, xmpData);
				AddDatesFromXmpDir(xmpData, photo);
			});

			var exifDir = data.SingleOrDefault(i => i.Name == "Exif IFD0");
			if (exifDir != null)
			{
				AddMetadataLabel(labels, exifDir, "Image Description");
				AddMetadataLabel(labels, exifDir, "Windows XP Subject");
				AddMetadataLabel(labels, exifDir, "Windows XP Title");
				AddMetadataLabel(labels, exifDir, "Windows XP Comment");
				if (!photo.TakenDate.HasValue)
				{
					AddDatesFromExifDir(exifDir, photo);
				}
			}

			var ipcDir = data.SingleOrDefault(d => d.Name == "IPTC");
			if (ipcDir != null)
			{
				AddMetadataLabel(labels, ipcDir, "Caption/Abstract");
			}

			var quickTimeDir = data.SingleOrDefault(d => d.Name == "QuickTime Metadata Header");
			if (quickTimeDir != null)
			{
				AddLabelFromQuickTimeDir(quickTimeDir, labels);
				if (!photo.TakenDate.HasValue)
				{
					AddDatesFromQuickTimeDir(quickTimeDir, photo);
				}
			}

			labels = labels.Distinct().ToList();
			if (labels.Any())
			{
				photo.Label = string.Join(Environment.NewLine, labels);
			}
			AddGpsInformation(data, photo);
			return photo;
		}

		private static void AddLabelFromQuickTimeDir(Directory quickTimeDir, List<string> labels)
		{
			var descriptionTag = quickTimeDir.Tags.SingleOrDefault(t => t.Name == QuickTimeMetadataDescriptionTag);
			if (descriptionTag != null)
			{
				var value = (StringValue)quickTimeDir.GetObject(descriptionTag.Type);
				labels.Add(value.ToString());
			}
		}

		private void AddDatesFromQuickTimeDir(Directory quickTimeMeta, Photo photo)
		{
			var tag = quickTimeMeta.Tags.SingleOrDefault(t => t.Name == QuickTimeMetaDataCreationDateTag);
			if (tag != null)
			{
				var tagValue = (StringValue)quickTimeMeta.GetObject(tag.Type);
				if (DateTime.TryParse(tagValue.ToString(), out DateTime creationDate))
				{
					photo.TakenDate = creationDate;
				}
			}
		}

		private void AddDatesFromExifDir(Directory directory, Photo photo)
		{
			var tag = directory.Tags.SingleOrDefault(t => t.Name == "Date/Time");
			if (!string.IsNullOrWhiteSpace(tag?.Description) && DateTime.TryParseExact(tag.Description, ExifDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime creationDate))
			{
				photo.TakenDate = creationDate;
			}
		}

		private void AddDatesFromXmpDir(Directory xmpData, Photo photo)
		{
			var invariantCulture = CultureInfo.InvariantCulture;
			var xmpDirectory = xmpData as XmpDirectory;
			var dateCreated = xmpDirectory.XmpMeta.Properties.SingleOrDefault(p => p.Path == "xmp:CreateDate");
			var photoshopDateCreated = xmpDirectory.XmpMeta.Properties.SingleOrDefault(p => p.Path == "photoshop:DateCreated");
			var dateModified = xmpDirectory.XmpMeta.Properties.SingleOrDefault(p => p.Path == "xmp:ModifyDate");
			if (!string.IsNullOrWhiteSpace(dateCreated?.Value)
				&& DateTime.TryParseExact(dateCreated.Value, "s", invariantCulture, DateTimeStyles.RoundtripKind, out var takenDate))
			{
				photo.TakenDate = takenDate;
			}

			if (!photo.TakenDate.HasValue && !string.IsNullOrWhiteSpace(photoshopDateCreated?.Value)
				&& DateTime.TryParseExact(photoshopDateCreated.Value, "s", invariantCulture, DateTimeStyles.RoundtripKind, out var photoshopTakenDate))
			{
				photo.TakenDate = photoshopTakenDate;
			}

			if (!string.IsNullOrWhiteSpace(dateModified?.Value)
				&& DateTime.TryParseExact(dateModified.Value, "s", invariantCulture, DateTimeStyles.RoundtripKind, out var modifiedDate))
			{
				photo.ModifiedDate = modifiedDate;
			}
		}

		private void AddLabelFromXmpDescription(List<string> labels, Directory xmpData)
		{
			var xmpDirectory = xmpData as XmpDirectory;
			var artworkDesc = xmpDirectory.XmpMeta.Properties.SingleOrDefault(p => p.Path == "Iptc4xmpExt:ArtworkContentDescription");
			if (artworkDesc != null)
			{
				labels.Add(artworkDesc.Value);
			}
		}

		private void AddMetadataLabel(List<string> labels, Directory exifDir, string tagName)
		{
			var tag = exifDir.Tags.SingleOrDefault(t => t.Name == tagName);
			if (!string.IsNullOrWhiteSpace(tag?.Description))
			{
				labels.Add(tag.Description);
			}
		}

		private void AddGpsInformation(IReadOnlyList<Directory> data, Photo photo)
		{
			var gpsData = data.SingleOrDefault(d => d.Name == GPSDirectoryName);
			if (gpsData != null)
			{
				var latitudeTag = gpsData.Tags.SingleOrDefault(t => t.Name == GPSLatitudeTagName);
				var longitudeTag = gpsData.Tags.SingleOrDefault(t => t.Name == GPSLongitudeTagName);
				var altitudeTag = gpsData.Tags.SingleOrDefault(t => t.Name == GPSAltitudeTagName);
				if (latitudeTag != null)
				{
					photo.Latitude = GetCoordinateFromDegrees(latitudeTag);
				}

				if (longitudeTag != null)
				{
					photo.Longitude = GetCoordinateFromDegrees(longitudeTag);
				}

				if (altitudeTag != null)
				{
					photo.AltitudeInMeters = GetAltitudeFromTag(altitudeTag);
				}
			}
		}

		private double? GetAltitudeFromTag(Tag altitudeTag)
		{
			var tagValue = altitudeTag.Description;
			var altitudeMatch = _regexAltitude.Match(tagValue);
			if (!altitudeMatch.Success)
			{
				return null;
			}
			var altitudeValue = altitudeMatch.Groups["altitude"].Value;
			return double.Parse(altitudeValue, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, _pointFormatInfo);
		}

		private double GetCoordinateFromDegrees(Tag tag)
		{
			var tagValue = tag.Description;
			var result = _regexDegrees.Match(tagValue);
			var hours = double.Parse(result.Groups["hours"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, _comaFormatInfo);
			var minutes = double.Parse(result.Groups["minutes"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, _comaFormatInfo);
			var seconds = double.Parse(result.Groups["seconds"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, _comaFormatInfo);
			return hours + (minutes / 60) + (seconds / 3600);
		}
	}
}
