using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace luis.lib
{
	public class LuisService
	{
#if DEBUG
		private const string STAGING = "true";
#else
		private const string STAGING = "false";
#endif

		#region Constants

		private const string LUIS_VERBOSE = "true";
		private const string LUIS_TZ_OFFSET = "-360";

		#endregion

		#region Variables

		private HttpClient _httpClient = null;

		#endregion

		#region Properties

		internal string ApiUrl { get; set; }
		internal string ApiKey { get; set; }
		internal string AppId { get; set; }
		internal string SpellCheckKey { get; set; }

		public HttpClient HttpClient
		{
			get
			{
				// Use an instance-lifetime HttpClient rather than re-instantiating at each call

				// Delay instantiation of HttpClient until needed
				if (_httpClient == null)
					_httpClient = this.GetHttpClient();

				return _httpClient;
			}
		}

		#endregion

		#region ctors

		private LuisService() { }

		public LuisService(string apiUrl, string apiKey, string appId, string spellCheckKey)
		{
			this.ApiUrl = apiUrl;
			this.ApiKey = apiKey;
			this.AppId = appId;
			this.SpellCheckKey = spellCheckKey;
		}

		#endregion

		public async Task<LuisServiceResult> Query(string queryText)
		{
			LuisServiceResult result = null;

			if (string.IsNullOrWhiteSpace(queryText))
				return null;

			try
			{
				string fullUrl = this.GetApiUrl() + HttpUtility.UrlEncode(queryText);

				HttpResponseMessage response = await this.HttpClient.GetAsync(fullUrl);

				string json = await response.Content.ReadAsStringAsync();

				result = JsonConvert.DeserializeObject<LuisServiceResult>(json);

				result.Succeeded = response.IsSuccessStatusCode;
				result.ServiceMessage = response.ReasonPhrase;
			}
			catch (Exception ex)
			{
				// TODO log exception

				result.Succeeded = false;
			}

			return result;
		}

		private string GetApiUrl()
		{
			string result =
				this.ApiUrl +
				this.AppId +
				"?subscription-key=" +
				this.ApiKey +
				"&staging=" + STAGING +
				"&spellcheck=" +
				(string.IsNullOrWhiteSpace(this.SpellCheckKey) ? "false" : "true") +
				(string.IsNullOrWhiteSpace(this.SpellCheckKey) ? string.Empty : "&bing-spell-check-subscription-key=" + this.SpellCheckKey) +
				"&verbose=" + LUIS_VERBOSE +
				"&timezoneOffset=" + LUIS_TZ_OFFSET +
				"&q="
			;

			return result;
		}

		private HttpClient GetHttpClient()
		{
			var client = new HttpClient();
			client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.ApiKey);

			return client;
		}
	}
}
