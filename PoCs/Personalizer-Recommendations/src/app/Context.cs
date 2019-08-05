using System;
using System.Collections.Generic;
using System.Text;

namespace app
{
	public class Context
	{
		public string Id { get; set; }

		public User User { get; set; }

		public State State { get; set; }
	}

	public class User
	{
		public string DeviceType { get; set; }
		public bool IsInternalUser { get; set; }
		public bool IsDecisionMaker { get; set; }
		public bool IsInfluencer { get; set; }
		public string AgeGroup { get; set; }
		public string Gender { get; set; }
	}

	public class State
	{
		public string TimeOfDay { get; set; }
		public string DayOfWeek { get; set; }
		public string Country { get; set; }
	}
}
