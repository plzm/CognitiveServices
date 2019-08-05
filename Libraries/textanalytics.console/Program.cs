using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pelazem.azure.cognitive.textanalytics;

namespace textanalytics.console
{
	class Program
	{
		private static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
		{
			Formatting = Formatting.Indented,
			NullValueHandling = NullValueHandling.Include
		};

		static void Main(string[] args)
		{
			ProcessTexts().Wait();

			Console.WriteLine("Done. Press any key to exit.");
			Console.ReadKey();
		}

		static async Task ProcessTexts()
		{
			string msg = "Today is an excellent day! Every day is; you see, rain makes plants grow and sun makes us happy. It's all good, either way.";

			string apiUrl = "https://eastus.api.cognitive.microsoft.com/text/analytics/v2.1/";
			string apiKey = "2b7db267d498432e80061c5c8146b7e7";

			TextAnalyticsServiceClient svc = new TextAnalyticsServiceClient(apiUrl, apiKey);

			TextAnalyticsServiceResult svcResult = svc.ProcessAsync(msg).GetAwaiter().GetResult();

			if (svcResult.Responses.Count > 0)
			{
				TextAnalyticsResponse response = svcResult.Responses.First();

				Console.WriteLine("Sentiment Score: " + response.SentimentScore.ToString());
				Console.WriteLine();

				Console.WriteLine("Key Phrases: " + response.KeyPhrases.Count.ToString());

				foreach (string keyPhrase in response.KeyPhrases)
					Console.WriteLine(keyPhrase);

				Console.WriteLine();

				Console.WriteLine("Detected Languages: " + response.DetectedLanguages.Count.ToString());

				foreach (TextAnalyticsLanguage detectedLanguage in response.DetectedLanguages)
					Console.WriteLine(detectedLanguage.Name);

				Console.WriteLine();

				Console.WriteLine("Entities: " + response.Entities.Count.ToString());

				foreach (TextAnalyticsEntity entity in response.Entities)
					Console.WriteLine(entity.Name + " | " + entity.Type + " | " + entity.SubType);

				Console.WriteLine();

				Console.WriteLine("Errors: " + svcResult.Errors.Count.ToString());

				foreach (TextAnalyticsError error in svcResult.Errors)
					Console.WriteLine(error.Message);

				Console.WriteLine();
			}



			//Console.WriteLine("George Washington Farewell Address (partial)");

			//string text2 = File.ReadAllText("texts/text2.txt");

			//TextAnalyticsServiceResult result2 = await svc.ProcessAsync(text2);
			//string json2 = result2.ToJson();
			//Console.WriteLine(json2);

			//Console.WriteLine();
			//Console.WriteLine();
		}

		//private static string GetTripMessage()
		//{
		//	TripMessage message = new TripMessage();

		//	message.trip_type = 1;
		//	message.trip_year = DateTime.Now.Year.ToString();
		//	message.trip_month = string.Format("{0:MM}", DateTime.Now);
		//	message.taxi_type = "Yellow";
		//	message.vendor_id = 1;
		//	message.pickup_datetime = DateTime.Now.AddMinutes(-30);
		//	message.dropoff_datetime = message.pickup_datetime.AddMinutes(15);
		//	message.passenger_count = 2;
		//	message.trip_distance = 5;
		//	message.rate_code_id = 1;
		//	message.store_and_fwd_flag = "";
		//	message.pickup_location_id = 66;
		//	message.dropoff_location_id = 99;
		//	message.pickup_longitude = "77.7777";
		//	message.pickup_latitude = "33.3333";
		//	message.dropoff_longitude = "77.9999";
		//	message.dropoff_latitude = "33.6666";
		//	message.payment_type = 2;
		//	message.fare_amount = 13;
		//	message.extra = 1.5;
		//	message.mta_tax = 2.2;
		//	message.tip_amount = 3;
		//	message.tolls_amount = 1.75;
		//	message.improvement_surcharge = 0.6;
		//	message.ehail_fee = 0.99;

		//	message.customer_comments = "Ride was BAD!! Car was dirty. SUspenson vry hard. Avoid.";

		//	return JsonConvert.SerializeObject(message, _jsonSerializerSettings);
		//}

		//jo["textanalytics_customer_key_phrases"] = new JArray(response.KeyPhrases);

		//jo["textanalytics_customer_detected_languages"] = (JArray)JToken.FromObject(response.DetectedLanguages);

		//jo["textanalytics_customer_entities"] = (JArray)JToken.FromObject(response.Entities);

		//jo["textanalytics_errors"] = (JArray)JToken.FromObject(result1.Errors);
	}
}
