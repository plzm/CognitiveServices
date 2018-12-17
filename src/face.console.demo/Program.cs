using System;
using System.IO;
using System.Threading.Tasks;

namespace face.demoapp
{
	class Program
	{
		static void Main(string[] args)
		{
			string apiUrl = File.ReadAllText(@"security\face.apiUrl.private");
			string apiKey = File.ReadAllText(@"security\face.apiKey.private");

			Demo demo = new Demo(apiUrl, apiKey);

			//Console.WriteLine("Face API - using image URLs");
			//demo.RunWithUrls().Wait();
			//Console.WriteLine("Press any key to continue");
			//Console.ReadKey();

			Console.WriteLine();
			Console.WriteLine("Face API - using image files");
			demo.RunWithFiles().Wait();
			Console.WriteLine("Press any key to continue");
			Console.ReadKey();

			Console.WriteLine("Done - press any key to exit");
			Console.ReadKey();
		}
	}
}
