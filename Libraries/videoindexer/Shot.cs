using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class Shot
	{
		public int Id { get; set; }

		public List<KeyFrame> KeyFrames { get; set; }
		public List<ShotInstance> Instances { get; set; }
	}
}
