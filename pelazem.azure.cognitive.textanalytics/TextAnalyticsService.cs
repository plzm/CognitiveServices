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
	public class TextAnalyticsService
	{
		#region Variables

		private Dictionary<string, HttpClient> _httpClients = new Dictionary<string, HttpClient>();

		#endregion

		#region Properties

		public string ApiUrlCommonBase { get; set; } = string.Empty;
		public string ApiKey { get; set; } = string.Empty;

		#endregion

		#region Constructors

		internal TextAnalyticsService() { }

		public TextAnalyticsService(string apiUrlCommonBase, string apiKey)
		{
			if (string.IsNullOrWhiteSpace(apiUrlCommonBase))
				throw new ArgumentException(apiUrlCommonBase);

			if (string.IsNullOrWhiteSpace(apiKey))
				throw new ArgumentException(apiKey);

			this.ApiUrlCommonBase = apiUrlCommonBase;
			this.ApiKey = apiKey;
		}

		#endregion

		public async Task<TextAnalyticsServiceResult> Process(string text, string language = "en")
		{
			TextAnalyticsServiceResult result = new TextAnalyticsServiceResult();

			if (string.IsNullOrWhiteSpace(text))
				return result;

			List<TextAnalyticsDocument> requests = new List<TextAnalyticsDocument>() { new TextAnalyticsDocument() { Id = "1", Language = language, Text = text } };

			HttpContent content = GetRequestContent(requests);

			TextAnalyticsServiceResult entitiesResult = await ProcessEntities(content);
			TextAnalyticsServiceResult keyPhrasesResult = await ProcessKeyPhrases(content);
			TextAnalyticsServiceResult languagesResult = await ProcessLanguages(content);
			TextAnalyticsServiceResult sentimentResult = await ProcessSentiment(content);

			// Merge errors
			result.Errors.AddRange(entitiesResult.Errors.Select(e0 => new TextAnalyticsError() { Id = e0.Id, Message = "Entities: " + e0.Message }));
			result.Errors.AddRange(keyPhrasesResult.Errors.Select(e0 => new TextAnalyticsError() { Id = e0.Id, Message = "Key Phrases: " + e0.Message }));
			result.Errors.AddRange(languagesResult.Errors.Select(e0 => new TextAnalyticsError() { Id = e0.Id, Message = "Languages: " + e0.Message }));
			result.Errors.AddRange(sentimentResult.Errors.Select(e0 => new TextAnalyticsError() { Id = e0.Id, Message = "Sentiment: " + e0.Message }));

			// Merge documents - since we're calling for exactly one (ID = 1 in this method where we merge the four API endpoints' results)
			TextAnalyticsDocument doc = new TextAnalyticsDocument();
			doc.Id = sentimentResult.Documents[0].Id;
			doc.Language = language;
			doc.Text = text;

			doc.Entities.AddRange(entitiesResult.Documents[0].Entities);
			doc.KeyPhrases.AddRange(keyPhrasesResult.Documents[0].KeyPhrases);
			doc.DetectedLanguages.AddRange(languagesResult.Documents[0].DetectedLanguages);
			doc.Score = sentimentResult.Documents[0].Score;

			result.Documents.Add(doc);

			result.Succeeded = (result.Errors.Count == 0);

			return result;
		}

		private async Task<TextAnalyticsServiceResult> ProcessEntities(HttpContent content)
		{
			return await ProcessWorker(this.ApiUrlCommonBase + "/v2.1-preview/entities", content);
		}

		private async Task<TextAnalyticsServiceResult> ProcessKeyPhrases(HttpContent content)
		{
			return await ProcessWorker(this.ApiUrlCommonBase + "/v2.0/keyPhrases", content);
		}

		private async Task<TextAnalyticsServiceResult> ProcessLanguages(HttpContent content)
		{
			return await ProcessWorker(this.ApiUrlCommonBase + "/v2.0/languages", content);
		}

		private async Task<TextAnalyticsServiceResult> ProcessSentiment(HttpContent content)
		{
			return await ProcessWorker(this.ApiUrlCommonBase + "/v2.0/sentiment", content);
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

		private HttpContent GetRequestContent(List<TextAnalyticsDocument> documents)
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
