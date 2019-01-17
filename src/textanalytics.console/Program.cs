using System;
using System.IO;
using System.Threading.Tasks;
using pelazem.azure.cognitive.textanalytics;

namespace textanalytics.console
{
	class Program
	{
		static void Main(string[] args)
		{
			ProcessTexts().Wait();

			Console.WriteLine("Done. Press any key to exit.");
			Console.ReadKey();
		}

		static async Task ProcessTexts()
		{
			string apiUrl = "";
			string apiKey = "";

			TextAnalyticsServiceClient svc = new TextAnalyticsServiceClient(apiUrl, apiKey);

			string text1 = File.ReadAllText("texts/text1.txt");

			TextAnalyticsServiceResult result1 = await svc.ProcessAsync(text1);
			string json1 = result1.ToJson();
			Console.WriteLine(json1);

			Console.WriteLine();
			Console.WriteLine();


			Console.WriteLine("George Washington Farewell Address (partial)");

			string text2 = File.ReadAllText("texts/text2.txt");

			TextAnalyticsServiceResult result2 = await svc.ProcessAsync(text2);
			string json2 = result2.ToJson();
			Console.WriteLine(json2);

			Console.WriteLine();
			Console.WriteLine();

			Console.WriteLine("Done - press any key to exit");
			Console.ReadKey();
		}
	}
}
