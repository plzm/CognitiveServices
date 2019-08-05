using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace pelazem.azure.cognitive.textanalytics
{
	public class TextAnalyticsServiceClient
	{
		#region Variables

		private Dictionary<string, HttpClient> _httpClients = new Dictionary<string, HttpClient>();

		#endregion

		#region Properties

		public string ApiUrl { get; set; } = string.Empty;
		public string ApiKey { get; set; } = string.Empty;

		#endregion

		#region Constructors

		internal TextAnalyticsServiceClient() { }

		public TextAnalyticsServiceClient(string apiEndpointUrl, string apiKey)
		{
			if (string.IsNullOrWhiteSpace(apiEndpointUrl))
				throw new ArgumentException(apiEndpointUrl);

			if (string.IsNullOrWhiteSpace(apiKey))
				throw new ArgumentException(apiKey);

			this.ApiUrl = apiEndpointUrl;
			this.ApiKey = apiKey;
		}

		#endregion

		public async Task<TextAnalyticsServiceResult> ProcessAsync(string text, string language = "en", bool processSentiment = true, bool processLanguages = true, bool processKeyPhrases = true, bool processEntities = true)
		{
			TextAnalyticsServiceResult result = new TextAnalyticsServiceResult();

			if (string.IsNullOrWhiteSpace(text))
				return result;

			TextAnalyticsRequest request = new TextAnalyticsRequest() { Id = "1", Language = language, Text = text };

			TextAnalyticsRequest[] requests = { request };

			HttpContent content = GetRequestContent(requests);

			TextAnalyticsResponse response = new TextAnalyticsResponse();
			response.Request = request;

			if (processSentiment)
			{
				TextAnalyticsServiceResult sentimentResult = await ProcessSentiment(content);

				result.Errors.AddRange(sentimentResult.Errors.Select(e0 => new TextAnalyticsError() { Id = e0.Id, Message = "Sentiment: " + e0.Message }));

				response.SentimentScore = sentimentResult.Responses[0].SentimentScore;
			}

			if (processLanguages)
			{
				TextAnalyticsServiceResult languagesResult = await ProcessLanguages(content);

				result.Errors.AddRange(languagesResult.Errors.Select(e0 => new TextAnalyticsError() { Id = e0.Id, Message = "Languages: " + e0.Message }));

				response.DetectedLanguages.AddRange(languagesResult.Responses[0].DetectedLanguages);
			}

			if (processKeyPhrases)
			{
				TextAnalyticsServiceResult keyPhrasesResult = await ProcessKeyPhrases(content);

				result.Errors.AddRange(keyPhrasesResult.Errors.Select(e0 => new TextAnalyticsError() { Id = e0.Id, Message = "Key Phrases: " + e0.Message }));

				response.KeyPhrases.AddRange(keyPhrasesResult.Responses[0].KeyPhrases);
			}

			if (processEntities)
			{
				TextAnalyticsServiceResult entitiesResult = await ProcessEntities(content);

				result.Errors.AddRange(entitiesResult.Errors.Select(e0 => new TextAnalyticsError() { Id = e0.Id, Message = "Entities: " + e0.Message }));

				response.Entities.AddRange(entitiesResult.Responses[0].Entities);
			}

			result.Responses.Add(response);

			result.Succeeded = (result.Errors.Count == 0);

			return result;
		}

		private async Task<TextAnalyticsServiceResult> ProcessEntities(HttpContent content)
		{
			return await ProcessWorker(this.ApiUrl + "entities", content);
		}

		private async Task<TextAnalyticsServiceResult> ProcessKeyPhrases(HttpContent content)
		{
			return await ProcessWorker(this.ApiUrl + "keyPhrases", content);
		}

		private async Task<TextAnalyticsServiceResult> ProcessLanguages(HttpContent content)
		{
			return await ProcessWorker(this.ApiUrl + "languages", content);
		}

		private async Task<TextAnalyticsServiceResult> ProcessSentiment(HttpContent content)
		{
			return await ProcessWorker(this.ApiUrl + "sentiment", content);
		}

		private async Task<TextAnalyticsServiceResult> ProcessWorker(string apiUrl, HttpContent content)
		{
			HttpClient httpClient = GetHttpClient(apiUrl);

			HttpResponseMessage response = await httpClient.PostAsync(string.Empty, content);

			TextAnalyticsServiceResult result = await GetResult(response);

			return result;
		}

		private async Task<TextAnalyticsServiceResult> GetResult(HttpResponseMessage response)
		{
			TextAnalyticsServiceResult result = null;

			if (response == null || !response.IsSuccessStatusCode)
				return result;

			try
			{
				string responseContent = await response.Content.ReadAsStringAsync();

				result = JsonConvert.DeserializeObject<TextAnalyticsServiceResult>(responseContent);

				result.Succeeded = response.IsSuccessStatusCode;
				result.ServiceMessage = response.ReasonPhrase;
			}
			catch (Exception ex)
			{
				// TODO log exception

				result = null;
			}

			return result;
		}

		private HttpContent GetRequestContent(IEnumerable<TextAnalyticsRequest> documents)
		{
			HttpContent result = null;

			try
			{
				var param = JsonConvert.SerializeObject(new { documents = documents });

				result = new StringContent(param, Encoding.UTF8, "application/json");
			}
			catch (Exception ex)
			{
				// TODO log exception

				result = null;
			}

			return result;
		}

		private HttpClient GetHttpClient(string apiUrl)
		{
			if (_httpClients.ContainsKey(apiUrl))
				return _httpClients[apiUrl];

			HttpClient result = new HttpClient();
			result.BaseAddress = new Uri(apiUrl);
			result.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.ApiKey);

			_httpClients.Add(apiUrl, result);

			return result;
		}
	}
}
