using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using pelazem.azure.cognitive.videoindexer;

namespace videoindexer.console
{
	class Program
	{
		static string videoIndexerApiUrl = "https://api.videoindexer.ai";
		static string videoIndexerApiAzureRegion = "eastus2";

		static string videoIndexerApiAccountId = "";

		// Authorization API
		// Create a subscription here: https://api-portal.videoindexer.ai/Products/authorization
		// Then get API key from the subscription's page 
		static string videoIndexerApiKey = "";

		static async Task Main(string[] args)
		{
			VideoIndexerService svc = new VideoIndexerService(videoIndexerApiAccountId, videoIndexerApiUrl, videoIndexerApiKey, videoIndexerApiAzureRegion);

			// var accounts = await svc.GetVideoIndexerApiAccounts();

			List<VideoIndexerVideo> videos = await svc.GetVideosAsync(true, true);

			string captions = await svc.GetVideoCaptionsAsync(videos.First().Id);

		}
	}
}
