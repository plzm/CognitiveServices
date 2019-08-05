using System;
using System.Collections.Generic;
using System.Text;

namespace pelazem.azure.cognitive.face
{
	public struct Occlusion
	{
		public bool ForeheadOccluded { get; set; }

		public bool EyeOccluded { get; set; }

		public bool MouthOccluded { get; set; }
	}
}
