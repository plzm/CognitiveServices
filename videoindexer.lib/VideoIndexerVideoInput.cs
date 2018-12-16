using System;
using System.Collections.Generic;
using System.Text;

namespace videoindexer.lib
{
	public class VideoIndexerVideoInput
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string UrlOriginal { get; set; }
		public string ExternalId { get; set;}
		public string Language { get; set; }

		public string UrlVisibleToVideoIndexer { get; set; }

		public string VideoIndexerVideoId { get; set; }

		public string VideoIndexerProcessingState { get; set; }
	}
}
