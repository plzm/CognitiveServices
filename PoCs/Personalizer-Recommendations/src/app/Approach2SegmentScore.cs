using System;
using System.Collections.Generic;
using System.Text;

namespace app
{
	public class Approach2SegmentScore
	{
		public int Segment { get; set; } = 0;
		public int Count { get; set; } = 0;
		public double TotalReward { get; set; } = 0;
		public int CountRewardFull { get; set; } = 0;
		public int CountRewardHalf { get; set; } = 0;

		public override string ToString()
		{
			return
				$"Segment: {this.Segment} | " +
				$"Count: {this.Count} | " +
				$"Total Reward: {this.TotalReward} | " +
				$"Count Reward Half: {this.CountRewardHalf} | " +
				$"Count Reward Full: {this.CountRewardFull}"
			;
		}
	}
}
