using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class VisualContentModeration
	{
		public int Id { get; set; }
		public double AdultScore { get; set; }
		public double RacyScore { get; set; }

		public List<AdjustedInstance> Instances { get; set; }
	}
}
