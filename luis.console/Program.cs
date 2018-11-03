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

			ServiceResult result = await svc.Query("I want to drink coffee");

			Console.WriteLine(result.ToJson());
		}
	}
}
