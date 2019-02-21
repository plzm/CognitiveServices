using System;
using System.Collections.Generic;
using System.Text;

namespace pelazem.azure.cognitive.videoindexer
{
	public class Thumbnail
	{
		public string Id { get; set; }
		public string FileName { get; set; }

		public List<AdjustedInstance> Instances { get; set; }
	}
}
