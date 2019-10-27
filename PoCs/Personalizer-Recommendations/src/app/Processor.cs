using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Personalizer;
using Microsoft.Azure.CognitiveServices.Personalizer.Models;
using pelazem.rndgen;
using pelazem.util;

namespace PersonalizerPoC
{
	public class Processor
	{
		#region Variables

		private IList<RankableAction> _actions = null;
		private IList<IList<object>> _userContexts = null;

		#endregion

		#region Properties

		public int HowManyActions { get; private set; }
		public int HowManyUserContexts { get; private set; }
		public int HowManyUsersPerSegment { get; private set; }
		public int SegmentPauseMilliseconds { get; private set; }

		public IList<RankableAction> Actions
		{
			get
			{
				if (_actions == null)
					_actions = GetActions();

				return _actions;
			}
		}

		// List of contexts to go through to get recommendation for each
		// This could be how a known historical list of "who did what when" is used to train the Personalizer model by rewarding it for recommending what the user actually did
		public IList<IList<object>> UserContexts
		{
			get
			{
				if (_userContexts == null)
					_userContexts = GetContexts();

				return _userContexts;
			}
		}

		public IDictionary<string, IDictionary<string, int>> ContextActionScores { get; } = new Dictionary<string, IDictionary<string, int>>();

		#endregion

		/// <summary>
		/// Approach
		/// 1. Generate actions
		/// 2. Generate user contexts
		///		For each user context
		///			Get actions
		///			For each action, calculate a score for the user
		///				If the top action recommended by the service also has the top like score, send 1 back to the API.
		///				Variant a: ScoreHalfRewards = false, send back 0 to the API otherwise.
		///				Variant b: ScoreHalfRewards = true, if the top action recommended by the service has the second-highest like score, send 0.5 back to the API.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="apiKey"></param>
		/// <param name="scoreHalfRewards"></param>
		/// <returns></returns>
		public async Task<List<SegmentScore>> ProcessAsync(string endpoint, string apiKey, bool scoreHalfRewards, int howManyActions, int howManyUserContexts, int howManyUsersPerSegment, int segmentPauseMilliseconds)
		{
			this.HowManyActions = howManyActions;
			this.HowManyUserContexts = howManyUserContexts;
			this.HowManyUsersPerSegment = howManyUsersPerSegment;
			this.SegmentPauseMilliseconds = segmentPauseMilliseconds;

			List<SegmentScore> result = new List<SegmentScore>();

			int overallCounter = 0;

			SegmentScore segmentScore = new SegmentScore();
			segmentScore.Segment = 1;

			using (PersonalizerClient client = InitializePersonalizerClient(endpoint, apiKey))
			{
				// Iterate through each context - i.e. each interaction where we want to get a ranked list of choices
				foreach (var context in UserContexts)
				{
					overallCounter++;
					segmentScore.Count++;

					// Each interaction (context + choices -> ranked choices -> user behavior -> send feedback) requires a unique ID to correlate throughout
					string eventId = Guid.NewGuid().ToString();

					var rankingRequest = new RankRequest(Actions, context, null, eventId, false);
					RankResponse response = await client.RankAsync(rankingRequest);

					IDictionary<string, int> actionScoresForContext = this.GetActionScoresForContext(context);

					double reward = CalculateReward(actionScoresForContext, response.RewardActionId, scoreHalfRewards);

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

					if (segmentScore.Count % this.HowManyUsersPerSegment == 0)
					{
						result.Add(segmentScore);

						int newSegment = segmentScore.Segment + 1;
						segmentScore = new SegmentScore() { Segment = newSegment };

						if (overallCounter < this.HowManyUserContexts)
						{
							Console.WriteLine();
							Console.WriteLine("Sleeping for service training...");
							Thread.Sleep(this.SegmentPauseMilliseconds);
							Console.WriteLine("Completed sleep, continuing");
							Console.WriteLine();
						}
					}
				}
			}

			return result;
		}

		#region API

		private PersonalizerClient InitializePersonalizerClient(string endpoint, string apiKey)
		{
			PersonalizerClient client = new PersonalizerClient(new ApiKeyServiceClientCredentials(apiKey)) { Endpoint = endpoint };

			return client;
		}

		#endregion

		#region Actions

		private IList<RankableAction> GetActions()
		{
			List<RankableAction> actions = new List<RankableAction>();

			var resourceTypes = GetResourceTypes();
			var lengths = GetLengths();
			var levels = GetLevels();
			var languages = GetLanguages();

			for (int i = 1; i <= this.HowManyActions; i++)
			{
				string resourceType = RandomGenerator.Categorical.GetCategorical(resourceTypes);

				RankableAction action = new RankableAction()
				{
					Id = $"{i}",
					Features = new List<object>
					{
						new
						{
							resourceType = resourceType,
							isPublic = RandomGenerator.Boolean.GetBoolean(),
							isPaywall = RandomGenerator.Boolean.GetBoolean(),
							isFirstParty = RandomGenerator.Boolean.GetBoolean(),
							length = RandomGenerator.Categorical.GetCategorical(lengths),
							level = RandomGenerator.Categorical.GetCategorical(levels),
							language = RandomGenerator.Categorical.GetCategorical(languages),
							pubYear = Converter.GetInt32(RandomGenerator.Numeric.GetUniform(2000, 2019)),
							pubMonth = Converter.GetInt32(RandomGenerator.Numeric.GetUniform(1, 12)),
							createdBy = $"Creator {Converter.GetInt32(RandomGenerator.Numeric.GetUniform(1, 10))}",
							title = $"Title {resourceType} {i}",
							summary = $"Summary {resourceType} {i}"
						}
					}
				};

				actions.Add(action);
			}

			return actions;
		}

		private IList<Category<string>> GetResourceTypes()
		{
			List<Category<string>> resourceTypes = new List<Category<string>>
			{
				new Category<string>() {Value = "article", RelativeWeight=0.7},
				new Category<string>() {Value = "website", RelativeWeight=0.3}
			};

			return resourceTypes;
		}

		private IList<Category<string>> GetLengths()
		{
			List<Category<string>> lengths = new List<Category<string>>
			{
				new Category<string>() {Value = "short", RelativeWeight=0.3},
				new Category<string>() {Value = "medium", RelativeWeight=0.35},
				new Category<string>() {Value = "long", RelativeWeight=0.1},
				new Category<string>() {Value = "whitepaper", RelativeWeight=0.2},
				new Category<string>() {Value = "book", RelativeWeight=0.05}
			};

			return lengths;
		}

		private IList<Category<string>> GetLevels()
		{
			List<Category<string>> levels = new List<Category<string>>
			{
				new Category<string>() {Value = "introductory", RelativeWeight=0.2},
				new Category<string>() {Value = "intermediate", RelativeWeight=0.4},
				new Category<string>() {Value = "advanced", RelativeWeight=0.2},
				new Category<string>() {Value = "expert", RelativeWeight=0.2}
			};

			return levels;
		}

		private IList<Category<string>> GetLanguages()
		{
			List<Category<string>> levels = new List<Category<string>>
			{
				new Category<string>() {Value = "en", RelativeWeight=0.9},
				new Category<string>() {Value = "cn", RelativeWeight=0.03},
				new Category<string>() {Value = "ru", RelativeWeight=0.04},
				new Category<string>() {Value = "de", RelativeWeight=0.03}
			};

			return levels;
		}

		#endregion

		#region Context

		private IList<IList<object>> GetContexts()
		{
			IList<IList<object>> contexts = new List<IList<object>>();

			var timesOfDay = GetTimesOfDay();
			var daysOfWeek = GetDaysOfWeek();
			var countries = GetCountries();
			var devices = GetDevices();
			var ageGroups = GetAgeGroups();
			var genders = GetGenders();

			for (int i = 1; i <= this.HowManyUserContexts; i++)
			{
				string contextId = i.ToString();

				// Prepare new context and add to list
				var context = new List<object>()
				{
					new
					{
						id = contextId,
						deviceType = RandomGenerator.Categorical.GetCategorical(devices),
						isInternalUser = RandomGenerator.Boolean.GetBoolean(),
						isDecisionMaker = RandomGenerator.Boolean.GetBoolean(),
						isInfluencer = RandomGenerator.Boolean.GetBoolean(),
						ageGroup = RandomGenerator.Categorical.GetCategorical(ageGroups),
						gender = RandomGenerator.Categorical.GetCategorical(genders)
					},
					new
					{
						timeOfDay = RandomGenerator.Categorical.GetCategorical(timesOfDay),
						dayOfWeek = RandomGenerator.Categorical.GetCategorical(daysOfWeek),
						country = RandomGenerator.Categorical.GetCategorical(countries),
					}
				};

				contexts.Add(context);


				// Calculate points for each action for this context and store
				Dictionary<string, int> actionScoresForThisContext = new Dictionary<string, int>();

				foreach(RankableAction action in this.Actions)
					actionScoresForThisContext.Add(action.Id, this.GetPoints(context, action.Id));

				ContextActionScores.Add(contextId, actionScoresForThisContext);
			}

			return contexts;
		}

		public IDictionary<string, int> GetActionScoresForContext(IList<object> context)
		{
			dynamic user = context[0];

			IDictionary<string, int> actionScores = ContextActionScores[user.id];

			return actionScores;
		}

		private IList<Category<string>> GetTimesOfDay()
		{
			List<Category<string>> timesOfDay = new List<Category<string>>
			{
				new Category<string>() {Value = "morning", RelativeWeight=0.4},
				new Category<string>() {Value = "afternoon", RelativeWeight=0.4},
				new Category<string>() {Value = "evening", RelativeWeight=0.15},
				new Category<string>() {Value = "night", RelativeWeight=0.05}
			};

			return timesOfDay;
		}

		private IList<Category<string>> GetDaysOfWeek()
		{
			List<Category<string>> daysOfWeek = new List<Category<string>>
			{
				new Category<string>() {Value = "Monday", RelativeWeight=0.2},
				new Category<string>() {Value = "Tuesday", RelativeWeight=0.2},
				new Category<string>() {Value = "Wednesday", RelativeWeight=0.15},
				new Category<string>() {Value = "Thursday", RelativeWeight=0.15},
				new Category<string>() {Value = "Friday", RelativeWeight=0.15},
				new Category<string>() {Value = "Saturday", RelativeWeight=0.1},
				new Category<string>() {Value = "Sunday", RelativeWeight=0.05}
			};

			return daysOfWeek;
		}

		private IList<Category<string>> GetCountries()
		{
			List<Category<string>> countries = new List<Category<string>>
			{
				new Category<string>() {Value = "US", RelativeWeight=0.7},
				new Category<string>() {Value = "UK", RelativeWeight=0.1},
				new Category<string>() {Value = "Germany", RelativeWeight=0.05},
				new Category<string>() {Value = "Russia", RelativeWeight=0.05},
				new Category<string>() {Value = "China", RelativeWeight=0.1}
			};

			return countries;
		}

		private IList<Category<string>> GetDevices()
		{
			List<Category<string>> devices = new List<Category<string>>
			{
				new Category<string>() {Value = "phone", RelativeWeight=0.5},
				new Category<string>() {Value = "tablet", RelativeWeight=0.2},
				new Category<string>() {Value = "desktop", RelativeWeight=3}
			};

			return devices;
		}

		private IList<Category<string>> GetAgeGroups()
		{
			List<Category<string>> ageGroups = new List<Category<string>>
			{
				new Category<string>() {Value = "18-30", RelativeWeight=0.15},
				new Category<string>() {Value = "31-49", RelativeWeight=0.6},
				new Category<string>() {Value = "50+", RelativeWeight=0.25}
			};

			return ageGroups;
		}

		private IList<Category<string>> GetGenders()
		{
			List<Category<string>> genders = new List<Category<string>>
			{
				new Category<string>() {Value = "m", RelativeWeight=0.49},
				new Category<string>() {Value = "f", RelativeWeight=0.49},
				new Category<string>() {Value = "u", RelativeWeight=0.02}
			};

			return genders;
		}

		#endregion

		#region Points

		private int GetPoints(IList<object> context, string actionId)
		{
			RankableAction action = this.Actions.SingleOrDefault(a => a.Id == actionId);

			if (action == null)
				return 0;

			dynamic user = context[0];
			dynamic state = context[1];
			dynamic features = action.Features[0];

			int points =
				GetPoints_TimeOfDay(state.timeOfDay, features) +
				GetPoints_DayOfWeek(state.dayOfWeek, features) +
				GetPoints_Country(state.country, features) +
				GetPoints_UserAttributes(user, features)
			;

			return points;
		}

		private int GetPoints_TimeOfDay(string timeOfDay, dynamic features)
		{
			int points = 0;

			switch (timeOfDay)
			{
				case "morning":
					if (features.resourceType == "website")
						points++;

					if (features.isPublic)
						points++;

					if (features.length == "short" || features.length == "medium")
						points++;

					if (features.level == "introductory" || features.level == "intermediate")
						points++;

					break;
				case "afternoon":
					if (features.resourceType == "article")
						points++;

					if (features.length == "medium" || features.length == "long" || features.length == "whitepaper")
						points++;

					if (features.level == "intermediate" || features.level == "advanced" || features.level == "expert")
						points++;

					break;
				case "evening":
				case "night":
					if (features.resourceType == "article")
						points++;

					if (features.length == "long" || features.length == "whitepaper" || features.length == "book")
						points++;

					if (features.level == "advanced" || features.level == "expert")
						points++;

					break;
				default:
					break;
			}

			return points;
		}

		private int GetPoints_DayOfWeek(string dayOfWeek, dynamic features)
		{
			int points = 0;

			switch (dayOfWeek)
			{
				case "Monday":
				case "Tuesday":
				case "Wednesday":
					if (features.resourceType == "website")
						points++;

					if (features.length == "short" || features.length == "medium")
						points++;

					if (features.level == "introductory" || features.level == "intermediate")
						points++;

					break;
				case "Thursday":
				case "Friday":
					if (features.resourceType == "article")
						points++;

					if (features.length == "medium" || features.length == "long" || features.length == "whitepaper")
						points++;

					if (features.level == "intermediate" || features.level == "advanced" || features.level == "expert")
						points++;

					break;
				case "Saturday":
				case "Sunday":
					if (features.resourceType == "article")
						points++;

					if (features.length == "long" || features.length == "whitepaper" || features.length == "book")
						points++;

					if (features.level == "advanced" || features.level == "expert")
						points++;

					break;
				default:
					break;
			}

			return points;
		}

		private int GetPoints_Country(string country, dynamic features)
		{
			int points = 0;

			switch (country)
			{
				case "US":
				case "UK":
					if (features.language == "en")
						points++;

					break;
				case "Germany":
					if (features.language == "de")
						points++;

					break;
				case "Russia":
					if (features.language == "ru")
						points++;

					break;
				case "China":
					if (features.language == "cn")
						points++;

					break;
				default:
					break;
			}

			return points;
		}

		private int GetPoints_UserAttributes(dynamic user, dynamic features)
		{
			int points = 0;

			string deviceType = user.deviceType;
			bool isInternal = user.isInternalUser;
			bool isDM = user.isDecisionMaker;
			bool isInfluencer = user.isInfluencer;
			string ageGroup = user.ageGroup;

			if (!isInternal && features.isFirstParty)
				points++;
			else if (isInternal && features.isPublic)
				points++;

			if (isDM && features.isPaywall)
				points++;

			if (isInfluencer && features.isPublic)
			{
				bool reshared = (RandomGenerator.Numeric.GetUniform(1, 12) >= 10);

				if (reshared)
					points += 5;
				else
					points += 2;
			}

			switch (ageGroup)
			{
				case "18-30":
					if (features.isPublic)
						points++;

					if (features.length == "short" || features.length == "medium")
					{
						if (deviceType == "phone")
							points += 5;
						else
							points++;
					}

					if (features.level == "introductory" || features.level == "intermediate")
						points++;

					break;
				case "31-49":
					if (features.length == "short" || features.length == "medium")
					{
						if (deviceType == "phone" || deviceType == "tablet")
							points += 5;
						else
							points++;
					}

					if (features.length == "long" || features.length == "whitepaper")
						points++;

					if (features.level == "advanced" || features.level == "expert")
						points++;

					break;
				case "50+":
					if (features.length == "medium" || features.length == "long" || features.length == "whitepaper" || features.length == "book")
					{
						if (deviceType == "tablet" || deviceType == "desktop")
							points += 5;
						else
							points++;
					}

					if (features.isPublic)
						points++;

					if (features.isPaywall)
						points++;

					if (features.length == "long" || features.length == "whitepaper" || features.length == "book")
						points++;

					if (features.level == "advanced" || features.level == "expert")
						points++;

					break;
				default:
					break;
			}

			return points;
		}

		#endregion

		#region Reward

		private double CalculateReward(IDictionary<string, int> actionScoresForContext, string recommendedActionId, bool scoreHalfRewards)
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
			else if (scoreHalfRewards)
			{
				// If not, AND if we are set to calculate half-scores, check whether the second-scoring KVP action ID matches the recommended action ID
				bool halfMatched = (orderedActionsAndScores.ElementAt(1).Key == recommendedActionId);

				if (halfMatched)
					result = 0.5;
				else
					result = 0;
			}
			else
				result = 0;

			return result;
		}

		#endregion
	}
}
