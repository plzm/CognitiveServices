using System;
using System.IO;
using System.Threading.Tasks;
using face.lib;

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
			string apiUrl = File.ReadAllText(@"security\face.apiUrl.private");
			string apiKey = File.ReadAllText(@"security\face.apiKey.private");

			FaceService svc = new FaceService(apiUrl, apiKey);

			// Local File
			FaceServiceResult result1local = await svc.ProcessImageAtLocal("images/image1.jpg");

			// Local File
			FaceServiceResult result2local = await svc.ProcessImageAtLocal("images/image2.jpg");

			// URL
			FaceServiceResult result1url = await svc.ProcessImageAtUrl("https://destinonegocio.com/wp-content/uploads/2015/12/ico-destinonegocio-empowerment-istock-getty-images-1030x696.jpg");

			// URL
			FaceServiceResult result2url = await svc.ProcessImageAtUrl("https://i.ytimg.com/vi/R_CYkvXdYXE/maxresdefault.jpg");
		}
	}
}
