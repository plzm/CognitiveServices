using System;
using System.Collections.Generic;
using System.Text;

namespace pelazem.azure.cognitive.textanalytics
{
	public class TextAnalyticsEntityMatch
	{
		public string Text { get; set; }
		public int Offset { get; set; }
		public int Length { get; set; }
	}
}
