using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class Keyword
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public double Confidence { get; set; }
		public string Language { get; set; }

		public List<Instance> Instances { get; set; }
	}
}
