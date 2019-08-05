using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class Speaker
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public List<AdjustedInstance> Instances { get; set; }
	}
}
