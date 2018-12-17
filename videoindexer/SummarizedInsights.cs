using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class SummarizedInsights
	{
		public string Name { get; set; }
		public string Id { get; set; }
		public string PrivacyMode { get; set; }
		public Duration Duration { get; set; }
		public string ThumbNailVideoId { get; set; }
		public string ThumbNailId { get; set; }

		public List<Face> Faces { get; set; }
		public List<Keyword> Keywords { get; set; }
		public List<Sentiment> Sentiments { get; set; }
		public List<AudioEffect> AudioEffects { get; set; }
		public List<Label> Labels { get; set; }
		public List<Brand> Brands { get; set; }
		public Statistics Statistics { get; set; }
		public List<Emotion> Emotions { get; set; }
		public List<Topic> Topics { get; set; }
	}
}
