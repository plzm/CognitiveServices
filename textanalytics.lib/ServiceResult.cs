using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace textanalytics.lib
{
	public class ServiceResult
	{
		public bool Succeeded { get; set; } = false;

		public string ServiceMessage { get; set; }

		public List<TextAnalyticsDocument> Documents { get; set; } = new List<TextAnalyticsDocument>();
		public List<TextAnalyticsError> Errors { get; set; } = new List<TextAnalyticsError>();

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
