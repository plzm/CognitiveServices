using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class AudioEffect
	{
		public int Id { get; set; }
		public string Type { get; set; }

		public List<Instance> Instances { get; set; }
	}
}
