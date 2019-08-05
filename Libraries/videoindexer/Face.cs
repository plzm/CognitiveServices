using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class Face
	{
		public int Id { get; set; }
		public string VideoId { get; set; }
		public string Name { get; set; }
		public double Confidence { get; set; }
		public string Description { get; set; }
		public string ThumbNailId { get; set; }
		public string ReferenceId { get; set; }
		public string ReferenceType { get; set; }
		public string Title { get; set; }
		public string ImageUrl { get; set; }
		public string KnownPersonId { get; set; }

		public double SeenDuration { get; set; }
		public double SeenDurationRatio { get; set; }

		public List<Appearance> Appearances { get; set; }

		public List<FaceInstance> Instances { get; set; }
	}
}
