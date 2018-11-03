using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace luis.lib
{
	public class LuisResolution
	{
		[JsonProperty(PropertyName = "unit")]
		public string Unit { get; set; }

		[JsonProperty(PropertyName = "value")]
		public string Value { get; set; }

		[JsonProperty(PropertyName = "values")]
		public List<string> Values { get; set; }
	}
}
