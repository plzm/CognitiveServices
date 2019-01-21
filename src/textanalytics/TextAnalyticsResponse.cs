using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace pelazem.azure.cognitive.textanalytics
{
	public class TextAnalyticsResponse
	{
		public TextAnalyticsRequest Request { get; internal set; }

		[JsonProperty("Score")]
		public double SentimentScore { get; set; }

		public List<TextAnalyticsEntity> Entities { get; set; } = new List<TextAnalyticsEntity>();

		public List<string> KeyPhrases { get; set; } = new List<string>();

		public List<TextAnalyticsLanguage> DetectedLanguages { get; set; } = new List<TextAnalyticsLanguage>();
	}
}
