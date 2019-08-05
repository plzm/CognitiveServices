using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class TextualContentModeration
	{
		public int Id { get; set; }
		public int BannedWordsCount { get; set; }
		public double BannedWordsRatio { get; set; }

		public List<AdjustedInstance> Instances { get; set; }
	}
}
