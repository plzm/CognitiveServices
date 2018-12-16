using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace videoindexer.lib
{
	public class VideoIndexerService
	{
		#region Variables

		private HttpClient _httpClient = null;
		private HttpClient _httpClientWithAuthHeader = null;

		#endregion

		#region Properties

		/// <summary>
		/// Use this when interacting with the Video Indexer Account API
		/// </summary>
		private HttpClient HttpClientWithAuthHeader
		{
			get
			{
				if (_httpClientWithAuthHeader == null)
				{
					ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | SecurityProtocolType.Tls12;

					var handler = new HttpClientHandler();
					handler.AllowAutoRedirect = false;

					_httpClientWithAuthHeader = new HttpClient(handler);

					_httpClientWithAuthHeader.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.VideoIndexerApiKey);
				}

				return _httpClientWithAuthHeader;
			}
		}

		/// <summary>
		/// Use this when interacting with the Video Indexer Operations API, which does not require the auth header with the API key, but instead requires an auth token issued by the Account API
		/// </summary>
		private HttpClient HttpClient
		{
			get
			{
				if (_httpClient == null)
				{
					ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | SecurityProtocolType.Tls12;

					var handler = new HttpClientHandler();
					handler.AllowAutoRedirect = false;

					_httpClient = new HttpClient(handler);
				}

				return _httpClient;
			}
		}

		public string VideoIndexerApiAccountId { get; set; }
		public string VideoIndexerApiUrl { get; set; }
		public string VideoIndexerApiKey { get; set; }
		public string VideoIndexerApiAzureRegion { get; set; }

		#endregion

		#region Constructors

		private VideoIndexerService() { }

		public VideoIndexerService(string apiAccountId, string apiUrl, string apiKey, string apiAzureRegion)
		{
			this.VideoIndexerApiAccountId = apiAccountId;
			this.VideoIndexerApiUrl = apiUrl;
			this.VideoIndexerApiKey = apiKey;
			this.VideoIndexerApiAzureRegion = apiAzureRegion;
		}

		#endregion

		#region Access Tokens

		public async Task<string> GetVideoIndexerApiAccessTokenForAccount(bool allowEdit = true)
		{
			var url = $"{this.VideoIndexerApiUrl}/auth/{this.VideoIndexerApiAzureRegion}/Accounts/{this.VideoIndexerApiAccountId}/AccessToken?allowEdit={(allowEdit ? "true" : "false")}";

			HttpResponseMessage response = await this.HttpClientWithAuthHeader.GetAsync(url);

			string token = (await response.Content.ReadAsStringAsync()).Replace("\"", "");

			return token;
		}

		public async Task<string> GetVideoIndexerApiAccessTokenForVideo(string videoIndexerVideoId, bool allowEdit = true)
		{
			var url = $"{this.VideoIndexerApiUrl}/auth/{this.VideoIndexerApiAzureRegion}/Accounts/{this.VideoIndexerApiAccountId}/Videos/{videoIndexerVideoId}/AccessToken?allowEdit={(allowEdit ? "true" : "false")}";

			HttpResponseMessage response = await this.HttpClientWithAuthHeader.GetAsync(url);

			string token = (await response.Content.ReadAsStringAsync()).Replace("\"", "");

			return token;
		}

		#endregion

		#region Account

		public async Task<List<VideoIndexerAccount>> GetVideoIndexerApiAccounts(bool allowEdit = false)
		{
			var url = $"{this.VideoIndexerApiUrl}/auth/{this.VideoIndexerApiAzureRegion}/Accounts?generateAccessTokens=false&allowEdit={(allowEdit ? "true" : "false")}";

			HttpResponseMessage response = await this.HttpClientWithAuthHeader.GetAsync(url);

			string json = await response.Content.ReadAsStringAsync();

			var accounts = JsonConvert.DeserializeObject<VideoIndexerAccount[]>(json).ToList<VideoIndexerAccount>();

			return accounts;
		}

		#endregion

		public async Task<string> UploadVideo(VideoIndexerVideoInput video, string accountAccessToken = "", string callbackUrlOnComplete = "")
		{
			if (string.IsNullOrWhiteSpace(accountAccessToken))
				accountAccessToken = await this.GetVideoIndexerApiAccessTokenForAccount();

			var content = new MultipartFormDataContent();
			content.Headers.Add("Content-Length", "0"); // Have to set this header to avoid HTTP 411 error from video indexer API (not documented)

			string queryString =
				(!string.IsNullOrWhiteSpace(video.Name) ? $"name={video.Name}&" : string.Empty) +
				(!string.IsNullOrWhiteSpace(video.Description) ? $"description={video.Description}&" : string.Empty) +
				(!string.IsNullOrWhiteSpace(video.ExternalId) ? $"externalId={video.ExternalId}&" : string.Empty) +
				(!string.IsNullOrWhiteSpace(video.Language) ? $"language={video.Language}&" : string.Empty) +
				"privacy=private&" +
				"indexingPreset=Default&" +
				"priority=Normal&" +
				"streamingPreset=AdaptiveBitrate&" +
				$"accessToken={accountAccessToken}&" +
				(!string.IsNullOrWhiteSpace(callbackUrlOnComplete) ? $"callbackUrl={HttpUtility.UrlEncode(callbackUrlOnComplete)}&" : string.Empty) +
				$"videoUrl={HttpUtility.UrlEncode(video.UrlVisibleToVideoIndexer)}"
			;

			var uploadRequestResult = await this.HttpClient.PostAsync($"{this.VideoIndexerApiUrl}/{this.VideoIndexerApiAzureRegion}/Accounts/{this.VideoIndexerApiAccountId}/Videos?{queryString}", content);

			// TODO log request result/success

			var uploadResult = await uploadRequestResult.Content.ReadAsStringAsync();

			// get the video id from the upload result
			string videoId = JsonConvert.DeserializeObject<dynamic>(uploadResult)["id"];

			video.VideoIndexerVideoId = videoId;

			return videoId;
		}

		public async Task<List<VideoIndexerVideo>> GetVideos(bool withDetailsAndInsights, bool withCaptions)
		{
			List<VideoIndexerVideo> interim = new List<VideoIndexerVideo>();

			string accountAccessToken = await this.GetVideoIndexerApiAccessTokenForAccount();

			int pageSize = 50;
			int skip = 0;

			while (true)
			{
				var url = $"{this.VideoIndexerApiUrl}/{this.VideoIndexerApiAzureRegion}/Accounts/{this.VideoIndexerApiAccountId}/Videos?accessToken={accountAccessToken}&pageSize={pageSize.ToString()}&skip={skip.ToString()}";

				HttpResponseMessage response = await this.HttpClient.GetAsync(url);

				string json = await response.Content.ReadAsStringAsync();

				VideoIndexerListVideosResult page = JsonConvert.DeserializeObject<VideoIndexerListVideosResult>(json);

				interim.AddRange(page.Results);

				if (page.NextPage.Done)
					break;
				else
					skip++;
			}

			if (withDetailsAndInsights)
			{
				List<VideoIndexerVideo> full = new List<VideoIndexerVideo>();

				foreach (VideoIndexerVideo v in interim)
					full.Add(await GetVideo(v.Id, withCaptions));

				return full;
			}
			else
			{
				if (withCaptions)
					await SetVideoCaptions(interim);

				return interim;
			}
		}

		public async Task<VideoIndexerVideo> GetVideo(string videoId, bool withCaptions, string accessToken = "")
		{
			if (string.IsNullOrWhiteSpace(accessToken))
				accessToken = await this.GetVideoIndexerApiAccessTokenForVideo(videoId, false);

			var url = $"{this.VideoIndexerApiUrl}/{this.VideoIndexerApiAzureRegion}/Accounts/{this.VideoIndexerApiAccountId}/Videos/{videoId}/Index?accessToken={accessToken}";

			var response = await this.HttpClient.GetAsync(url);
			var json = await response.Content.ReadAsStringAsync();

			VideoIndexerGetVideoIndexResult result = JsonConvert.DeserializeObject<VideoIndexerGetVideoIndexResult>(json);

			VideoIndexerVideo video = result?.Videos?.FirstOrDefault(v => v.Id == result.Id);
			VideoIndexerVideo final = null;

			if (video != null)
			{
				// Have to even out the Video Indexer API return structure
				// https://api-portal.videoindexer.ai/docs/services/operations/operations/Get-Video-Index?
				// https://docs.microsoft.com/en-us/azure/media-services/video-indexer/video-indexer-output-json-v2

				video.Partition = result.Partition;
				video.Name = result.Name;
				video.Description = result.Description;
				video.UserName = result.UserName;
				video.Created = result.Created;
				video.PrivacyMode = result.PrivacyMode;
				video.State = result.State;
				video.IsOwned = result.IsOwned;
				video.IsEditable = result.IsEditable;
				video.IsBase = result.IsBase;
				video.DurationInSeconds = result.DurationInSeconds;
				video.SummarizedInsights = result.SummarizedInsights;

				// We effectively do a deep copy here so that we can set interim to null, as it may be rather large and all we want is the merged video item
				// We serialize video to JSON, then deserialize back to a new Video instance
				JsonSerializerSettings settings = new JsonSerializerSettings() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Include };
				string finalJson = JsonConvert.SerializeObject(video, settings);
				final = JsonConvert.DeserializeObject<VideoIndexerVideo>(finalJson, settings);
				final.RawJsonFromApi = finalJson; // Keep the JSON around for debugging etc. if needed

				result = null;

				if (withCaptions)
					final.Captions = await GetVideoCaptions(videoId, accessToken);
			}

			return final;
		}

		public async Task SetVideoCaptions(IEnumerable<VideoIndexerVideo> videos)
		{
			foreach (VideoIndexerVideo video in videos)
				video.Captions = await GetVideoCaptions(video.Id);
		}

		public async Task<string> GetVideoCaptions(string videoId, string accessToken = "")
		{
			if (string.IsNullOrWhiteSpace(accessToken))
				accessToken = await this.GetVideoIndexerApiAccessTokenForVideo(videoId, false);

			var url = $"{this.VideoIndexerApiUrl}/{this.VideoIndexerApiAzureRegion}/Accounts/{this.VideoIndexerApiAccountId}/Videos/{videoId}/Captions?accessToken={accessToken}";

			var result = await this.HttpClient.GetAsync(url);
			var content = await result.Content.ReadAsStringAsync();

			return content;
		}

		public async Task SetCurrentVideoProcessingStatus(IEnumerable<VideoIndexerVideoInput> videos)
		{
			foreach (VideoIndexerVideoInput video in videos)
			{
				string accessToken = await this.GetVideoIndexerApiAccessTokenForVideo(video.VideoIndexerVideoId, false);

				var url = $"{this.VideoIndexerApiUrl}/{this.VideoIndexerApiAzureRegion}/Accounts/{this.VideoIndexerApiAccountId}/Videos/{video.VideoIndexerVideoId}/Index?accessToken={accessToken}";

				var getIndexRequestResult = await this.HttpClient.GetAsync(url);
				var getIndexContent = await getIndexRequestResult.Content.ReadAsStringAsync();

				video.VideoIndexerProcessingState = JsonConvert.DeserializeObject<dynamic>(getIndexContent)["state"];
			}
		}
	}
}
