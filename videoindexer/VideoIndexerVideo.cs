using System;
using System.Text;
using Newtonsoft.Json;

namespace pelazem.azure.cognitive.videoindexer
{
	public class VideoIndexerVideo
	{
		// Main properties from Get-Video-Index output: https://api-portal.videoindexer.ai/docs/services/operations/operations/Get-Video-Index?
		public string AccountId { get; set; } // Also at Get-Index root
		public string Id { get; set; } // Also at Get-Index root
		public string Name { get; set; } // Also at Get-Index root
		public string Description { get; set; } // Also at Get-Index root
		public string UserName { get; set; } // Also at Get-Index root
		public DateTime? Created { get; set; } // Also at Get-Index root
		public string PrivacyMode { get; set; } // Also at Get-Index root
		public string State { get; set; } // Also at Get-Index root
		public string ReviewState { get; set; } // At Get-Index/videos/video
		public string ModerationState { get; set; } // At Get-Index/videos/video
		public bool IsOwned { get; set; } // Also at Get-Index root
		public bool IsEditable { get; set; } // Also at Get-Index root
		public bool IsBase { get; set; } // Also at Get-Index root
		public int DurationInSeconds { get; set; } // Also at Get-Index root

		// Main properties from List-Videos output: https://api-portal.videoindexer.ai/docs/services/operations/operations/List-videos?
		public string Partition { get; set; } // Also at Get-Index root
		public string ExternalId { get; set; } // At Get-Index/videos/video
		public string Metadata { get; set; } // At Get-Index/videos/video
		public DateTime? LastModified { get; set; } // Not in Get-Index
		public DateTime? LastIndexed { get; set; } // Not in Get-Index
		public string ProcessingProgress { get; set; } // At Get-Index/videos/video
		public string ThumbNailVideoId { get; set; } // At Get-Index/summarizedInsights
		public string ThumbNailId { get; set; } // At Get-Index/videos/video
		public Social Social { get; set; } // Not in Get-Index
		public string[] SearchMatches { get; set; } // Not in Get-Index
		public string IndexingPreset { get; set; } // At Get-Index/videos/video
		public string StreamingPreset { get; set; } // Not in Get-Index
		public string SourceLanguage { get; set; } // At Get-Index/videos/video

		public SummarizedInsights SummarizedInsights { get; set; }

		public string FailureCode { get; set; } // At Get-Index/videos/video
		public string FailureMessage { get; set; } // At Get-Index/videos/video
		public string ExternalUrl { get; set; } // At Get-Index/videos/video
		public bool IsAdult { get; set; } // At Get-Index/videos/video
		public string PublishedUrl { get; set; } // At Get-Index/videos/video
		public string PublishedUrlProxy { get; set; } // At Get-Index/videos/video
		public string ViewToken { get; set; } // At Get-Index/videos/video
		public string Language { get; set; } // At Get-Index/videos/video
		public string LinguisticModelId { get; set; } // At Get-Index/videos/video
		public Statistics Statistics { get; set; }

		public Insights Insights { get; set; }

		public string Captions { get; set; }

		[JsonIgnore]
		internal string RawJsonFromApi { get; set; }
	}
}
