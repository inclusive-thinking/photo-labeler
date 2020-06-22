using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoLabeler.Entities
{
	public class RenamingResult
	{

		public int TotalFiles { get; set; }

		public int FilesRenamed { get; set; }

		public int ErrorCount => Errors.Count;

		public List<string> Errors { get; set; } = new List<string>();

	}
}
