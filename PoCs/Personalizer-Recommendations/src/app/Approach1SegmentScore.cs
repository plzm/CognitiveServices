using System;
using System.Collections.Generic;
using System.Text;

namespace app
{
	public class Approach1SegmentScore
	{
		public int Segment { get; set; } = 0;
		public int Count { get; set; } = 0;
		public double TotalReward { get; set; } = 0;
		public int CountRewardWeak { get; set; } = 0;
		public int CountRewardModerate { get; set; } = 0;
		public int CountRewardStrong { get; set; } = 0;

		public override string ToString()
		{
			return
				$"Segment: {this.Segment} | " +
				$"Count: {this.Count} | " +
				$"Total Reward: {this.TotalReward} | " +
				$"Count Reward Weak: {this.CountRewardWeak} | " +
				$"Count Reward Moderate: {this.CountRewardModerate} | " +
				$"Count Reward Strong: {this.CountRewardStrong}"
			;
		}
	}
}
