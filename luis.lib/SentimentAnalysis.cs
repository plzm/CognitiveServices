using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace luis.lib
{
	public class SentimentAnalysis
	{
		[JsonProperty(PropertyName = "label")]
		public string Label { get; set; }

		[JsonProperty(PropertyName = "score")]
		public double? Score { get; set; }
	}
}
