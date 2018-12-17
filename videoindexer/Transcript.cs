using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class Transcript
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public double Confidence { get; set; }
		public int SpeakerId { get; set; }
		public string Language { get; set; }

		public List<AdjustedInstance> Instances { get; set; }
	}
}
