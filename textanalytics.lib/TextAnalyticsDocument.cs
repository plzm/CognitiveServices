using System;
using System.Collections.Generic;
using System.Text;

namespace textanalytics.lib
{
	public class TextAnalyticsDocument
	{
		public string Id { get; set; }
		public string Text { get; set; }
		public string Language { get; set; }

		public double Score { get; set; }

		public List<TextAnalyticsEntity> Entities { get; set; } = new List<TextAnalyticsEntity>();

		public List<string> KeyPhrases { get; set; } = new List<string>();

		public List<TextAnalyticsLanguage> DetectedLanguages { get; set; } = new List<TextAnalyticsLanguage>();
	}
}
