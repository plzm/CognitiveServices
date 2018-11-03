using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace face.lib
{
	public class ServiceResult
	{
		public bool Succeeded { get; set; } = false;

		public string ServiceMessage { get; set; }

		public List<FaceResult> Faces { get; set; }

		public string ToJson()
		{
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.NullValueHandling = NullValueHandling.Include;

			return JsonConvert.SerializeObject(this, settings);
		}
	}
}
