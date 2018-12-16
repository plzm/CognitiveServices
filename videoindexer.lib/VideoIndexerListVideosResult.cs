using System;
using System.Collections.Generic;
using System.Text;

namespace videoindexer.lib
{
	public class VideoIndexerListVideosResult
	{
		public List<VideoIndexerVideo> Results { get; set; }

		public NextPage NextPage { get; set; }
	}

	public class NextPage
	{
		public int PageSize { get; set; }
		public int Skip { get; set; }
		public bool Done { get; set; }
	}
}
