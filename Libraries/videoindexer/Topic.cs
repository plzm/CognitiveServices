using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class Topic
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string ReferenceId { get; set; }
		public string ReferenceType { get; set; }
		public double Confidence { get; set; }
		public string Language { get; set; }

		public List<TopicInstance> Instances { get; set; }
	}
}
