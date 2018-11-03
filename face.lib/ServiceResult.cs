using System;
using System.Collections.Generic;
using System.Text;

namespace face.lib
{
	public class ServiceResult
	{
		public bool Succeeded { get; set; } = false;

		public string ServiceMessage { get; set; }

		public List<FaceResult> Faces { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
