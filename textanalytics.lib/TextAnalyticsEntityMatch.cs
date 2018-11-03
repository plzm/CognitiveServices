using System;
using System.Collections.Generic;
using System.Text;

namespace textanalytics.lib
{
	public class TextAnalyticsEntityMatch
	{
		public string Text { get; set; }
		public int Offset { get; set; }
		public int Length { get; set; }
	}
}
