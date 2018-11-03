using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace luis.lib
{
	public class ServiceResult
	{
		[JsonProperty(PropertyName = "succeeded")]
		public bool Succeeded { get; set; } = false;

		[JsonProperty(PropertyName = "serviceMessage")]
		public string ServiceMessage { get; set; }

		[JsonProperty(PropertyName = "query")]
		public string Query { get; set; }

		[JsonProperty(PropertyName = "topScoringIntent")]
		public LuisIntent TopScoringIntent { get; set; }

		[JsonProperty(PropertyName = "intents")]
		public List<LuisIntent> Intents { get; set; }

		[JsonProperty(PropertyName = "entities")]
		public List<LuisEntity> Entities { get; set; }

		[JsonProperty(PropertyName = "sentimentAnalysis")]
		public SentimentAnalysis SentimentAnalysis { get; set; }

		public string ToJson()
		{
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.NullValueHandling = NullValueHandling.Include;

			return JsonConvert.SerializeObject(this, settings);
		}
	}
}
