using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class Label
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Language { get; set; }

		public List<Appearance> Appearances { get; set; }

		public List<LabelInstance> Instances { get; set; }
	}
}
