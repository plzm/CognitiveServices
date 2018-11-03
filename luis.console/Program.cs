using System;
using System.IO;
using System.Threading.Tasks;
using luis.lib;

namespace luis.console
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
			string spellCheckKey = File.ReadAllText(@"security\bingSpellcheck.apiKey.private");
			string apiUrl = File.ReadAllText(@"security\luis.apiUrl.private");
			string apiKey = File.ReadAllText(@"security\luis.apiKey.private");
			string appId = File.ReadAllText(@"security\luis.appId.private");

			LuisService svc = new LuisService(apiUrl, apiKey, appId, spellCheckKey);

			string query1 = "I want to drink coffee";
			ServiceResult result1 = await svc.Query(query1);
			Console.WriteLine(result1.ToJson());
			Console.WriteLine();

			string query2 = "I am bored and I want to go out";
			ServiceResult result2 = await svc.Query(query2);
			Console.WriteLine(result2.ToJson());
			Console.WriteLine();

			string query3 = "I am SO HUNGRY - need food!";
			ServiceResult result3 = await svc.Query(query3);
			Console.WriteLine(result3.ToJson());
			Console.WriteLine();

		}
	}
}
