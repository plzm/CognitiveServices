using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace pelazem.azure.cognitive.textanalytics
{
	public class TextAnalyticsServiceResult
	{
		public bool Succeeded { get; set; } = false;

		public string ServiceMessage { get; set; }

		[JsonProperty("Documents")]
		public List<TextAnalyticsResponse> Responses { get; set; } = new List<TextAnalyticsResponse>();

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
