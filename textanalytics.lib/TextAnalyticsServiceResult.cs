using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace textanalytics.lib
{
	public class TextAnalyticsServiceResult
	{
		public bool Succeeded { get; set; } = false;

		public string ServiceMessage { get; set; }

		public List<TextAnalyticsDocument> Documents { get; set; } = new List<TextAnalyticsDocument>();
		public List<TextAnalyticsError> Errors { get; set; } = new List<TextAnalyticsError>();

		public string ToJson()
		{
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.NullValueHandling = NullValueHandling.Include;

			return JsonConvert.SerializeObject(this, settings);
		}
	}
}
