using System;
using System.Collections.Generic;
using System.Text;

namespace pelazem.azure.cognitive.videoindexer
{
	public class VideoIndexerGetVideoIndexResult
	{
		// Get-Video-Index output: https://api-portal.videoindexer.ai/docs/services/operations/operations/Get-Video-Index?

		public string AccountId { get; set; } // Also at Get-Index root
		public string Id { get; set; } // Also at Get-Index root
		public string Partition { get; set; } // Also at Get-Index root
		public string Name { get; set; } // Also at Get-Index root
		public string Description { get; set; } // Also at Get-Index root
		public string UserName { get; set; } // Also at Get-Index root
		public DateTime? Created { get; set; } // Also at Get-Index root
		public string PrivacyMode { get; set; } // Also at Get-Index root
		public string State { get; set; } // Also at Get-Index root
		public bool IsOwned { get; set; } // Also at Get-Index root
		public bool IsEditable { get; set; } // Also at Get-Index root
		public bool IsBase { get; set; } // Also at Get-Index root
		public int DurationInSeconds { get; set; } // Also at Get-Index root

		public SummarizedInsights SummarizedInsights { get; set; }

		/// <summary>
		/// Video Indexer API splits video index output into some top-level properties and the remainder in a list of videos... presumably that is intended for playlists??
		/// We flatten that out and hide the videos list behind an internal
		/// </summary>
		public List<VideoIndexerVideo> Videos { get; set; }
	}
}
