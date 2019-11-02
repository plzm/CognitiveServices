using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;

namespace PredatorDetection
{
	class Program
	{
		private static string _apiEndpoint = "https://PROVIDE-prediction.cognitiveservices.azure.com/";
		private static string _predictionKey = "PROVIDE";

		private static Guid _projectGuid_Classification_Multiclass = new Guid("PROVIDE");
		private static string _modelName_Classification_Multiclass = "PROVIDE";

		private static Guid _projectGuid_Classification_Multilabel = new Guid("PROVIDE");
		private static string _modelName_Classification_Multilabel = "PROVIDE";

		private static Guid _projectGuid_Detection = new Guid("PROVIDE");
		private static string _modelName_Detection = "PROVIDE";

		private static string _resultsFilePath = $@"PROVIDE\results-{DateTime.UtcNow.Ticks}.txt";

		static void Main(string[] args)
		{
			Predict().Wait();

			Console.WriteLine("Done!");
			Console.ReadKey();
		}

		public static async Task Predict()
		{
			// Use test images in this repo - substitute other as needed. Note, the API also takes URLs to pictures, in this case I am sending picture as request payload.
			List<string> filePaths = Directory.GetFiles(@"..\..\..\..\..\Images\test\", "*.*", SearchOption.TopDirectoryOnly).ToList();

			StringBuilder sb = new StringBuilder();

			using (CustomVisionPredictionClient endpoint = new CustomVisionPredictionClient() { ApiKey = _predictionKey, Endpoint = _apiEndpoint })
			{
				foreach (string filePath in filePaths)
				{
					sb.AppendLine(Path.GetFileName(filePath));
					sb.AppendLine(DateTime.UtcNow.ToString());

					using (var stream = File.OpenRead(filePath))
					{
						ImagePrediction classificationResult = await endpoint.ClassifyImageAsync(_projectGuid_Classification_Multiclass, _modelName_Classification_Multiclass, stream);

						// Loop over each classification and write out the results
						foreach (var c in classificationResult.Predictions.OrderByDescending(p => p.Probability))
							sb.AppendLine($"Classification (multi-class): {c.TagName}: {c.Probability:P1}");
					}

					sb.AppendLine("--------------------");

					using (var stream = File.OpenRead(filePath))
					{
						ImagePrediction classificationResult = await endpoint.ClassifyImageAsync(_projectGuid_Classification_Multilabel, _modelName_Classification_Multilabel, stream);

						// Loop over each classification and write out the results
						foreach (var c in classificationResult.Predictions.OrderByDescending(p => p.Probability))
							sb.AppendLine($"Classification (multi-label): {c.TagName}: {c.Probability:P1}");
					}

					sb.AppendLine("--------------------");

					using (var stream = File.OpenRead(filePath))
					{
						ImagePrediction detectionResult = await endpoint.DetectImageAsync(_projectGuid_Detection, _modelName_Detection, stream);

						// Loop over each detection and write out the results
						foreach (var c in detectionResult.Predictions.OrderByDescending(p => p.Probability))
							sb.AppendLine($"Detection: {c.TagName}: {c.Probability:P1} " + $"[ {c.BoundingBox.Left}, {c.BoundingBox.Top}, {c.BoundingBox.Width}, {c.BoundingBox.Height} ]");
					}

					sb.AppendLine("--------------------");
					sb.AppendLine();
				}
			}

			string final = sb.ToString();
			File.WriteAllText(_resultsFilePath, final);
		}
	}
}
