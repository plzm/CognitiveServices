using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class KeyFrame
	{
		public int Id { get; set; }

		public List<KeyFrameInstance> Instances { get; set; }
	}
}
