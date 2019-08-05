using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class Block
	{
		public int Id { get; set; }

		public List<AdjustedInstance> Instances { get; set; }
	}
}
