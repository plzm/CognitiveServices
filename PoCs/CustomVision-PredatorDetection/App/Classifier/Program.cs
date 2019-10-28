using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;

namespace PredatorDetection
{
	class Program
	{
		private static string _apiEndpoint = "https://PROVIDE.cognitiveservices.azure.com/";
		private static string _predictionKey = "PROVIDE";

		private static Guid _projectGuid_Classification = new Guid("PROVIDE");
		private static string _modelName_Classification = "PROVIDE";

		private static Guid _projectGuid_Detection = new Guid("PROVIDE");
		private static string _modelName_Detection = "PROVIDE";

		static void Main(string[] args)
		{
			Predict().Wait();

			Console.WriteLine("Done!");
			Console.ReadKey();
		}

		public static async Task Predict()
		{
			List<string> filePaths = Directory.GetFiles(@"..\..\..\..\..\Images\test\", "*.*", SearchOption.TopDirectoryOnly).ToList();

			CustomVisionPredictionClient endpoint = new CustomVisionPredictionClient()
			{
				ApiKey = _predictionKey,
				Endpoint = _apiEndpoint
			};

			StringBuilder sb = new StringBuilder();

			foreach (string filePath in filePaths)
			{
				sb.AppendLine(Path.GetFileName(filePath));

				using (var stream = File.OpenRead(filePath))
				{
					ImagePrediction classificationResult = await endpoint.ClassifyImageAsync(_projectGuid_Classification, _modelName_Classification, stream);
					
					// Loop over each classification and write out the results
					foreach (var c in classificationResult.Predictions.OrderByDescending(p => p.Probability))
					{
						string line = $"Classification: {c.TagName}: {c.Probability:P1} " + (c.BoundingBox != null ? $"[ {c.BoundingBox.Left}, {c.BoundingBox.Top}, {c.BoundingBox.Width}, {c.BoundingBox.Height} ]" : "[No bounding box]");

						sb.AppendLine(line);
					}
				}

				using (var stream = File.OpenRead(filePath))
				{
					ImagePrediction detectionResult = await endpoint.DetectImageAsync(_projectGuid_Detection, _modelName_Detection, stream);

					// Loop over each detection and write out the results
					foreach (var c in detectionResult.Predictions.OrderByDescending(p => p.Probability))
					{
						string line = $"Detection: {c.TagName}: {c.Probability:P1} " + (c.BoundingBox != null ? $"[ {c.BoundingBox.Left}, {c.BoundingBox.Top}, {c.BoundingBox.Width}, {c.BoundingBox.Height} ]" : "[No bounding box]");

						sb.AppendLine(line);
					}
				}

				sb.AppendLine();
			}

			string final = sb.ToString();
			File.WriteAllText(@"C:\Users\paelaz\Desktop\hack.txt", final);
		}
	}
}
