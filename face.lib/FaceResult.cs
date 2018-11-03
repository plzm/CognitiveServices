using System;
using System.Collections.Generic;
using System.Text;

namespace face.lib
{
	// See https://eastus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f30395236 for REST API documentation
	// This class is a DTO for some of the Face API outputs

	public class FaceResult
	{
		public string FaceId { get; set; }

		public FaceRectangle FaceRectangle { get; set; }

		public FaceAttributes FaceAttributes { get; set; }
	}
}
