using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PersonalizerPoC
{
	class Program
	{
		// Eventually load these from some config repo
		public const string ENDPOINT = "https://PROVIDE.cognitiveservices.azure.com/";
		public const string APIKEY = "PROVIDE";

		public const bool SCOREHALFREWARDS = true;
		public const int HOWMANYACTIONS = 5;
		public const int HOWMANYUSERCONTEXTS = 1000;
		public const int HOWMANYUSERSPERSEGMENT = 100;
		public const int SEGMENTPAUSEMILLISECONDS = 60000;

		public const string OUTPUTPATH = @"PROVIDE\output\";


		static void Main(string[] args)
		{
			Process().Wait();

			Console.WriteLine("Done! Press any key to exit.");
			Console.ReadKey();
		}

		static async Task Process()
		{
			// Run through a list of contexts and choices and generate segment results
			Processor processor = new Processor();
			List<SegmentScore> segmentScores = await processor.ProcessAsync(ENDPOINT, APIKEY, SCOREHALFREWARDS, HOWMANYACTIONS, HOWMANYUSERCONTEXTS, HOWMANYUSERSPERSEGMENT, SEGMENTPAUSEMILLISECONDS);

			// Persist segment scores to a file
			string filePath = PersistResults(segmentScores);
			Console.WriteLine($"Completed writing Segment Scores: {filePath}");
			Console.WriteLine();
		}

		private static string PersistResults(List<SegmentScore> segmentScores)
		{
			StringBuilder sb = new StringBuilder();

			foreach (SegmentScore segmentScore in segmentScores)
				sb.AppendLine(segmentScore.ToString());

			string results = sb.ToString();

			string fileName = DateTime.Now.Ticks.ToString() + ".txt";
			string filePath = Path.Combine(OUTPUTPATH, fileName);

			File.WriteAllText(filePath, results);

			return filePath;
		}
	}
}
	