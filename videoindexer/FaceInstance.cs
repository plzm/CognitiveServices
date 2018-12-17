using System.Collections.Generic;

namespace pelazem.azure.cognitive.videoindexer
{
	public class FaceInstance : AdjustedInstance
	{
		public virtual List<string> ThumbnailsIds { get; set; }
	}
}
