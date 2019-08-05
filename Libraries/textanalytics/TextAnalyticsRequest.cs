using System;
using System.Collections.Generic;
using System.Text;

namespace pelazem.azure.cognitive.textanalytics
{
	public class TextAnalyticsRequest
	{
		public string Id { get; set; }
		public string Text { get; set; }
		public string Language { get; set; }
	}
}
