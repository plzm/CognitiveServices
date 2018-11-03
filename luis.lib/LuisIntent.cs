using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace luis.lib
{
	public class LuisIntent
	{
		[JsonProperty(PropertyName = "intent")]
		public string Intent { get; set; }

		[JsonProperty(PropertyName = "score")]
		public double Score { get; set; }
	}
}
