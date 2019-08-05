using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace pelazem.azure.cognitive.luis
{
	public class LuisSentimentAnalysis
	{
		[JsonProperty(PropertyName = "label")]
		public string Label { get; set; }

		[JsonProperty(PropertyName = "score")]
		public double? Score { get; set; }
	}
}
