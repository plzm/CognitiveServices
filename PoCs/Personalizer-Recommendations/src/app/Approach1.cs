using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Personalizer;
using Microsoft.Azure.CognitiveServices.Personalizer.Models;
using pelazem.rndgen;

namespace app
{
	public class Approach1
	{
		private const string APIKEY = "";

		public async Task ProcessAsync()
		{
			List<Approach1SegmentScore> segmentScores = new List<Approach1SegmentScore>();

			PersonalizerClient client = Common.InitializePersonalizerClient(Common.ENDPOINT, APIKEY);

			int overallCounter = 0;

			Approach1SegmentScore segmentScore = new Approach1SegmentScore();
			segmentScore.Segment = 1;

			foreach (var context in Common.UserContexts)
			{
				overallCounter++;
				segmentScore.Count++;

				string eventId = Guid.NewGuid().ToString();

				var request = new RankRequest(Common.Actions, context, null, eventId);
				RankResponse response = await client.RankAsync(request);

				IDictionary<string, int> actionScoresForContext = Common.GetActionScoresForContext(context);

				int points = actionScoresForContext[response.RewardActionId];

				double reward = this.GetReward(points);

				Console.WriteLine($"Iteration {overallCounter} = points {points}, reward {reward}");

				await client.RewardAsync(response.EventId, new RewardRequest(reward));

				segmentScore.TotalReward += reward;

				if (reward > 0)
				{
					if (reward < 0.5)
						segmentScore.CountRewardWeak++;
					else if (reward < 1)
						segmentScore.CountRewardModerate++;
					else
						segmentScore.CountRewardStrong++;
				}

				if (segmentScore.Count % Common.HowManyUsersPerSegment == 0)
				{
					segmentScores.Add(segmentScore);

					int newSegment = segmentScore.Segment + 1;
					segmentScore = new Approach1SegmentScore() { Segment = newSegment };

					if (overallCounter < Common.HowManyUserContexts)
					{
						Console.WriteLine();
						Console.WriteLine("Sleeping for service training...");
						Thread.Sleep(Common.SegmentPauseMilliseconds);
						Console.WriteLine("Completed sleep, continuing");
						Console.WriteLine();
					}
				}
			}

			Console.WriteLine("Writing Segment Scores:");
			string filePath = this.PersistResults(segmentScores);
			Console.WriteLine(filePath);
			Console.WriteLine();
		}

		private string PersistResults(List<Approach1SegmentScore> segmentScores)
		{
			StringBuilder sb = new StringBuilder();

			foreach (Approach1SegmentScore segmentScore in segmentScores)
				sb.AppendLine(segmentScore.ToString());

			string results = sb.ToString();

			string filePath = Common.PersistResults(results, Path.Combine(Common.OUTPUTPATHROOT, "approach1"));

			return filePath;
		}

		#region Compute Reward

		private double GetReward(int points)
		{
			double result = 0;

			if (points <= 10)
				result = 0;
			else if (points <= 12)
				result = 0.33;
			else if (points <= 14)
				result = 0.67;
			else
				result = 1;

			return result;
		}


		#endregion

	}
}
