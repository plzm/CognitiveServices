using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class Ocr
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public double Confidence { get; set; }
		public int Left { get; set; }
		public int Top { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public string Language { get; set; }

		public List<AdjustedInstance> Instances { get; set; }
	}
}
