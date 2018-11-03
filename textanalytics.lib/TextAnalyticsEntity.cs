﻿using System;
using System.Collections.Generic;
using System.Text;

namespace textanalytics.lib
{
	public class TextAnalyticsEntity
	{
		public string Name { get; set; }

		public List<TextAnalyticsEntityMatch> Matches { get; set; }

		public string WikipediaId { get; set; }
		public string WikipediaLanguage { get; set; }
		public string WikipediaUrl { get; set; }

		public string BingId { get; set; }
	}
}
