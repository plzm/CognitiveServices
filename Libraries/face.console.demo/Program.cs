using System;
using System.IO;
using System.Threading.Tasks;

namespace face.demoapp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			string apiUrl = "https://eastus.api.cognitive.microsoft.com/face/v1.0/detect";
			string apiKey = "";

			Demo demo = new Demo(apiUrl, apiKey);

			Console.WriteLine("Face API - using image URLs");
			await demo.RunWithUrls();
			Console.WriteLine("Press any key to continue");
			Console.ReadKey();

			//Console.WriteLine();
			//Console.WriteLine("Face API - using image files");
			//await demo.RunWithFiles();
			//Console.WriteLine("Press any key to continue");
			//Console.ReadKey();

			Console.WriteLine("Done - press any key to exit");
			Console.ReadKey();
		}
	}
}
