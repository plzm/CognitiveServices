using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace pelazem.azure.cognitive.luis
{
	public class LuisEntity
	{
		[JsonProperty(PropertyName = "entity")]
		public string Entity { get; set; }

		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "startIndex")]
		public int StartIndex { get; set; }

		[JsonProperty(PropertyName = "endIndex")]
		public int EndIndex { get; set; }

		[JsonProperty(PropertyName = "score")]
		public double Score { get; set; }

		[JsonProperty(PropertyName = "resolution")]
		public LuisResolution Resolution { get; set; }
	}
}
