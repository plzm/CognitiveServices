using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;

namespace Classifier
{
	class Program
	{
		private static string _apiEndpoint = "https://PROVIDE-prediction.cognitiveservices.azure.com/";

		private static string _predictionKey = "PROVIDE";

		private static string _modelName = "Iteration3";
		private static Guid _projectGuid = new Guid("PROVIDE");

		static void Main(string[] args)
		{
			Predict().Wait();

			Console.WriteLine("Done!");
			Console.ReadKey();
		}

		public static async Task Predict()
		{
			CustomVisionPredictionClient endpoint = new CustomVisionPredictionClient()
			{
				ApiKey = _predictionKey,
				Endpoint = _apiEndpoint
			};

			List<string> filePaths = Directory.GetFiles(@"images\test\", "*.*", SearchOption.TopDirectoryOnly).ToList();

			StringBuilder sb = new StringBuilder();

			foreach (string filePath in filePaths)
			{
				using (var stream = File.OpenRead(filePath))
				{
					sb.AppendLine(Path.GetFileName(filePath));

					ImagePrediction result = await endpoint.ClassifyImageAsync(_projectGuid, _modelName, stream);
					//var result = await endpoint.DetectImageAsync(_projectGuid, _modelName, stream);

					// Loop over each prediction and write out the results
					foreach (var c in result.Predictions.OrderByDescending(p => p.Probability))
					{
						string line = $"{c.TagName}: {c.Probability:P1} " + (c.BoundingBox != null ? $"[ {c.BoundingBox.Left}, {c.BoundingBox.Top}, {c.BoundingBox.Width}, {c.BoundingBox.Height} ]" : "[No bounding box]");

						sb.AppendLine(line);
					}

					sb.AppendLine();
				}
			}

			string final = sb.ToString();
			File.WriteAllText(@"C:\Users\paelaz\Desktop\hack.txt", final);
		}
	}
}
