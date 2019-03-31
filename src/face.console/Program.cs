using System;
using System.IO;
using System.Threading.Tasks;
using pelazem.azure.cognitive.face;

namespace face.console
{
	// Quick test harness for face.lib
	class Program
	{
		static void Main(string[] args)
		{
			ProcessImages().Wait();

			Console.WriteLine("Done. Press any key to exit.");
			Console.ReadKey();
		}

		static async Task ProcessImages()
		{
			string apiUrl = "https://eastus.api.cognitive.microsoft.com/face/v1.0/detect";
			string apiKey = "";

			FaceService svc = new FaceService(apiUrl, apiKey);

			// Local File
			//FaceServiceResult result1local = await svc.ProcessImageAtLocal("images/image1.jpg");

			// Local File
			//FaceServiceResult result2local = await svc.ProcessImageAtLocal("images/image2.jpg");

			// URL
			// FaceServiceResult result1url = await svc.ProcessImageAtUrl("https://pzpubliceus.blob.core.windows.net/public/34.jpg");

			// URL
			// FaceServiceResult result2url = await svc.ProcessImageAtUrl("https://pzpubliceus.blob.core.windows.net/public/38.jpg");
		}
	}
}
