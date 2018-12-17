using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace pelazem.azure.cognitive.luis
{
	public class LuisIntent
	{
		[JsonProperty(PropertyName = "intent")]
		public string Intent { get; set; }

		[JsonProperty(PropertyName = "score")]
		public double Score { get; set; }
	}
}
