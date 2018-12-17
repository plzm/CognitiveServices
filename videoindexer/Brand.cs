using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class Brand
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string ReferenceId { get; set; }
		public string ReferenceUrl { get; set; }
		public string ReferenceType { get; set; }
		public string Description { get; set; }
		List<string> Tags { get; set; }
		public double Confidence { get; set; }

		public List<BrandInstance> Instances { get; set; }
	}
}
