using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class Sentiment
	{
		public int Id { get; set; }
		public double Score { get; set; }
		public double AverageScore { get; set; }
		public string SentimentType { get; set; }

		public List<AdjustedInstance> Instances { get; set; }

		public string SentimentKey { get; set; }
		public double SeenDurationRatio { get; set; }

		public List<Appearance> Appearances { get; set; }
	}
}
