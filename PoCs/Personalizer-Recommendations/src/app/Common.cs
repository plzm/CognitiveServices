using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Azure.CognitiveServices.Personalizer;
using Microsoft.Azure.CognitiveServices.Personalizer.Models;
using pelazem.rndgen;
using pelazem.util;

namespace app
{
	public class Common
	{
		public const string ENDPOINT = "https://westus2.api.cognitive.microsoft.com/";
		public const string OUTPUTPATHROOT = @"c:\personalizer\";

		private static IList<RankableAction> _actions = null;
		private static IList<IList<object>> _userContexts = null;

		public static int HowManyActions { get; set; } = 5;
		public static int HowManyUserContexts { get; set; } = 5000;
		public static int HowManyUsersPerSegment { get; set; } = 1000;
		public static int SegmentPauseMilliseconds { get; set; } = 30000;

		public static IList<RankableAction> Actions
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
		public static IList<IList<object>> UserContexts
		{
			get
			{
				if (_userContexts == null)
					_userContexts = GetContexts();

				return _userContexts;
			}
		}

		public static IDictionary<string, IDictionary<string, int>> ContextActionScores { get; } = new Dictionary<string, IDictionary<string, int>>();


		public static PersonalizerClient InitializePersonalizerClient(string endpoint, string apiKey)
		{
			PersonalizerClient client = new PersonalizerClient(new ApiKeyServiceClientCredentials(apiKey)) { Endpoint = endpoint };

			return client;
		}

		#region Actions

		private static IList<RankableAction> GetActions()
		{
			List<RankableAction> actions = new List<RankableAction>();

			var resourceTypes = GetResourceTypes();
			var lengths = GetLengths();
			var levels = GetLevels();
			var languages = GetLanguages();

			for (int i = 1; i <= Common.HowManyActions; i++)
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

		private static IList<Category<string>> GetResourceTypes()
		{
			List<Category<string>> resourceTypes = new List<Category<string>>
			{
				new Category<string>() {Value = "article", RelativeWeight=0.7},
				new Category<string>() {Value = "website", RelativeWeight=0.3}
			};

			return resourceTypes;
		}

		private static IList<Category<string>> GetLengths()
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

		private static IList<Category<string>> GetLevels()
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

		private static IList<Category<string>> GetLanguages()
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

		private static IList<IList<object>> GetContexts()
		{
			IList<IList<object>> contexts = new List<IList<object>>();

			var timesOfDay = GetTimesOfDay();
			var daysOfWeek = GetDaysOfWeek();
			var countries = GetCountries();
			var devices = GetDevices();
			var ageGroups = GetAgeGroups();
			var genders = GetGenders();

			for (int i = 1; i <= Common.HowManyUserContexts; i++)
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

				foreach(RankableAction action in Common.Actions)
					actionScoresForThisContext.Add(action.Id, Common.GetPoints(context, action.Id));

				ContextActionScores.Add(contextId, actionScoresForThisContext);
			}

			return contexts;
		}

		public static IDictionary<string, int> GetActionScoresForContext(IList<object> context)
		{
			dynamic user = context[0];

			IDictionary<string, int> actionScores = Common.ContextActionScores[user.id];

			return actionScores;
		}

		private static IList<Category<string>> GetTimesOfDay()
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

		private static IList<Category<string>> GetDaysOfWeek()
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

		private static IList<Category<string>> GetCountries()
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

		private static IList<Category<string>> GetDevices()
		{
			List<Category<string>> devices = new List<Category<string>>
			{
				new Category<string>() {Value = "phone", RelativeWeight=0.5},
				new Category<string>() {Value = "tablet", RelativeWeight=0.2},
				new Category<string>() {Value = "desktop", RelativeWeight=3}
			};

			return devices;
		}

		private static IList<Category<string>> GetAgeGroups()
		{
			List<Category<string>> ageGroups = new List<Category<string>>
			{
				new Category<string>() {Value = "18-30", RelativeWeight=0.15},
				new Category<string>() {Value = "31-49", RelativeWeight=0.6},
				new Category<string>() {Value = "50+", RelativeWeight=0.25}
			};

			return ageGroups;
		}

		private static IList<Category<string>> GetGenders()
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

		private static int GetPoints(IList<object> context, string actionId)
		{
			RankableAction action = Common.Actions.SingleOrDefault(a => a.Id == actionId);

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

		private static int GetPoints_TimeOfDay(string timeOfDay, dynamic features)
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

		private static int GetPoints_DayOfWeek(string dayOfWeek, dynamic features)
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

		private static int GetPoints_Country(string country, dynamic features)
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

		private static int GetPoints_UserAttributes(dynamic user, dynamic features)
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

		public static string PersistResults(string results, string folderPath)
		{
			string fileName = DateTime.Now.Ticks.ToString() + ".txt";
			string filePath = Path.Combine(folderPath, fileName);

			File.WriteAllText(filePath, results);

			return filePath;
		}

	}
}
