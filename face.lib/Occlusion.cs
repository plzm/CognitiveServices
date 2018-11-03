using System;
using System.Collections.Generic;
using System.Text;

namespace face.lib
{
	public struct Occlusion
	{
		public bool ForeheadOccluded { get; set; }

		public bool EyeOccluded { get; set; }

		public bool MouthOccluded { get; set; }
	}
}
