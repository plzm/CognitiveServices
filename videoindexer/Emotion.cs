using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class Emotion
	{
		public int Id { get; set; }
		public string Type { get; set; }

		public List<EmotionInstance> Instances { get; set; }
	}
}
