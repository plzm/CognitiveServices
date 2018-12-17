using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using pelazem.util;

namespace pelazem.azure.cognitive.face
{
	public class FaceAttributes
	{
		public double Age { get; set; }

		public string Gender { get; set; }

		public double Smile { get; set; }

		public FacialHair FacialHair { get; set; }

		public string Glasses { get; set; }

		public Emotion Emotion { get; set; }

		public Hair Hair { get; set; }

		public MakeUp MakeUp { get; set; }

		public Occlusion Occlusion { get; set; }

		public List<Accessory> Accessories { get; set; }

		public Blur Blur { get; set; }

		public Exposure Exposure { get; set; }

		public Noise Noise { get; set; }
	}
}
