using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class Insights
	{
		public string Version { get; set; }
		public string Duration { get; set; }
		public string SourceLanguage { get; set; }
		public string Language { get; set; }
		public double SourceLanguageConfidence { get; set; }

		public List<Transcript> Transcript { get; set; }
		public List<Ocr> Ocr { get; set; }
		public List<Keyword> Keywords { get; set; }
		public List<Block> Blocks { get; set; }
		public List<Face> Faces { get; set; }
		public List<Label> Labels { get; set; }
		public List<Shot> Shots { get; set; }
		public List<Brand> Brands { get; set; }
		public List<AudioEffect> AudioEffects { get; set; }
		public List<Sentiment> Sentiments { get; set; }
		public List<VisualContentModeration> VisualContentModeration { get; set; }
		public TextualContentModeration TextualContentModeration { get; set; }
		public List<Emotion> Emotions { get; set; }
		public List<Topic> Topics { get; set; }
	}
}
