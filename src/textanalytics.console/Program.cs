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
			string msg = GetTripMessage();

			string apiUrl = "";
			string apiKey = "";

			TextAnalyticsServiceClient svc = new TextAnalyticsServiceClient(apiUrl, apiKey);

			JObject jo = JObject.Parse(msg);
			string text1 = jo["customer_comments"].ToString();
			Console.WriteLine(text1);
			Console.WriteLine();

			TextAnalyticsServiceResult result1 = await svc.ProcessAsync(text1);

			if (result1.Responses.Count > 0)
			{
				TextAnalyticsResponse response = result1.Responses.First();

				jo["textanalytics_customer_sentiment_score"] = response.SentimentScore;

				jo["textanalytics_customer_key_phrases"] = new JArray(response.KeyPhrases);

				jo["textanalytics_customer_detected_languages"] = (JArray)JToken.FromObject(response.DetectedLanguages);

				jo["textanalytics_customer_entities"] = (JArray)JToken.FromObject(response.Entities);

				jo["textanalytics_errors"] = (JArray)JToken.FromObject(result1.Errors);
			}

			Console.WriteLine(jo.ToString());

			Console.WriteLine();
			Console.WriteLine();


			//Console.WriteLine("George Washington Farewell Address (partial)");

			//string text2 = File.ReadAllText("texts/text2.txt");

			//TextAnalyticsServiceResult result2 = await svc.ProcessAsync(text2);
			//string json2 = result2.ToJson();
			//Console.WriteLine(json2);

			//Console.WriteLine();
			//Console.WriteLine();
		}

		private static string GetTripMessage()
		{
			TripMessage message = new TripMessage();

			message.trip_type = 1;
			message.trip_year = DateTime.Now.Year.ToString();
			message.trip_month = string.Format("{0:MM}", DateTime.Now);
			message.taxi_type = "Yellow";
			message.vendor_id = 1;
			message.pickup_datetime = DateTime.Now.AddMinutes(-30);
			message.dropoff_datetime = message.pickup_datetime.AddMinutes(15);
			message.passenger_count = 2;
			message.trip_distance = 5;
			message.rate_code_id = 1;
			message.store_and_fwd_flag = "";
			message.pickup_location_id = 66;
			message.dropoff_location_id = 99;
			message.pickup_longitude = "77.7777";
			message.pickup_latitude = "33.3333";
			message.dropoff_longitude = "77.9999";
			message.dropoff_latitude = "33.6666";
			message.payment_type = 2;
			message.fare_amount = 13;
			message.extra = 1.5;
			message.mta_tax = 2.2;
			message.tip_amount = 3;
			message.tolls_amount = 1.75;
			message.improvement_surcharge = 0.6;
			message.ehail_fee = 0.99;

			message.customer_comments = "Ride was BAD!! Car was dirty. SUspenson vry hard. Avoid.";

			return JsonConvert.SerializeObject(message, _jsonSerializerSettings);
		}

	}
}
