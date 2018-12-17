using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using pelazem.azure.cognitive.videoindexer;

namespace videoindexer.console
{
	class Program
	{
		static string videoIndexerApiAccountId = "";
		static string videoIndexerApiUrl = "";
		static string videoIndexerApiKey = ""; // Authorization
		static string videoIndexerApiAzureRegion = "";

		static async Task Main(string[] args)
		{
			VideoIndexerService svc = new VideoIndexerService(videoIndexerApiAccountId, videoIndexerApiUrl, videoIndexerApiKey, videoIndexerApiAzureRegion);

			var accounts = await svc.GetVideoIndexerApiAccounts();

			List<VideoIndexerVideo> videos = await svc.GetVideos(true, true);
		}
	}
}
