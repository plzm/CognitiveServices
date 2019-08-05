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
	public class Approach2
	{
		private const string APIKEY = "";

		public bool ScoreHalfRewards { get; set; } = false;

		public async Task ProcessAsync()
		{
			List<Approach2SegmentScore> segmentScores = new List<Approach2SegmentScore>();

			PersonalizerClient client = Common.InitializePersonalizerClient(Common.ENDPOINT, APIKEY);

			int overallCounter = 0;

			Approach2SegmentScore segmentScore = new Approach2SegmentScore();
			segmentScore.Segment = 1;

			foreach (var context in Common.UserContexts)
			{
				overallCounter++;
				segmentScore.Count++;

				string eventId = Guid.NewGuid().ToString();

				var rankingRequest = new RankRequest(Common.Actions, context, null, eventId);
				RankResponse response = await client.RankAsync(rankingRequest);

				IDictionary<string, int> actionScoresForContext = Common.GetActionScoresForContext(context);

				double reward = CalculateReward(actionScoresForContext, response.RewardActionId, response.Ranking);

				Console.WriteLine($"Iteration {overallCounter} = reward {reward}");

				await client.RewardAsync(response.EventId, new RewardRequest(reward));

				segmentScore.TotalReward += reward;

				if (reward > 0)
				{
					if (reward == 1)
						segmentScore.CountRewardFull++;
					else if (reward < 1)
						segmentScore.CountRewardHalf++;
				}

				if (segmentScore.Count % Common.HowManyUsersPerSegment == 0)
				{
					segmentScores.Add(segmentScore);

					int newSegment = segmentScore.Segment + 1;
					segmentScore = new Approach2SegmentScore() { Segment = newSegment };

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

		private double CalculateReward(IDictionary<string, int> actionScoresForContext, string recommendedActionId, IList<RankedAction> rankedActions)
		{
			double result = 0;

			// Sort the action scores from highest to lowest
			IOrderedEnumerable<KeyValuePair<string, int>> orderedActionsAndScores = actionScoresForContext.OrderByDescending(a => a.Value);

			// Get the highest-scoring KVP
			KeyValuePair<string, int> top = orderedActionsAndScores.ElementAt(0);

			// Did the highest-scoring KVP action ID match the recommended action ID?
			bool topMatched = (top.Key == recommendedActionId);

			if (topMatched)
				result = 1;
			else if (this.ScoreHalfRewards)
			{
				// If not, AND if we are set to calculate half-scores, check whether the second-scoring KVP action ID matches the recommended action ID
				bool halfMatched = (orderedActionsAndScores.ElementAt(1).Key == recommendedActionId);

				if (halfMatched)
					result = 0.5;
				else
					result = -1;
			}
			else
				result = -1;

			return result;
		}

		private string PersistResults(List<Approach2SegmentScore> segmentScores)
		{
			StringBuilder sb = new StringBuilder();

			foreach (Approach2SegmentScore segmentScore in segmentScores)
				sb.AppendLine(segmentScore.ToString());

			string results = sb.ToString();

			string filePath = Common.PersistResults(results, Path.Combine(Common.OUTPUTPATHROOT, "approach2"));

			return filePath;
		}

	}
}
