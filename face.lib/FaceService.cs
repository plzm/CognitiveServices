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

[assembly: InternalsVisibleTo("face.lib.tests")]
namespace face.lib
{
	public class FaceService
	{
		#region Constants

		/// Parameters we will append to the call to the cog svc endpoint to determine what the service returns
		private const string SERVICEPARAMS = "?returnFaceId=true&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise&returnFaceLandmarks=false";

		#endregion

		#region Variables

		private HttpClient _httpClient = null;

		#endregion

		#region Properties

		public string ApiUrl { get; set; } = string.Empty;
		public string ApiKey { get; set; } = string.Empty;

		public HttpClient HttpClient
		{
			get
			{
				// Use an instance-lifetime HttpClient rather than re-instantiating at each call

				// Delay instantiation of HttpClient until needed
				if (_httpClient == null)
					_httpClient = GetHttpClient();

				return _httpClient;
			}
		}

		#endregion

		#region Constructors

		internal FaceService() { }

		public FaceService(string apiUrl, string apiKey)
		{
			if (string.IsNullOrWhiteSpace(apiUrl))
				throw new ArgumentException(apiUrl);

			if (string.IsNullOrWhiteSpace(apiKey))
				throw new ArgumentException(apiKey);

			this.ApiUrl = apiUrl;
			this.ApiKey = apiKey;
		}

		#endregion

		/// <summary>
		/// Process a local file
		/// </summary>
		/// <param name="localImagePath">Local file path</param>
		/// <returns></returns>
		public async Task<ServiceResult> ProcessImageAtLocal(string localImagePath)
		{
			ServiceResult result = new ServiceResult();

			if (string.IsNullOrWhiteSpace(localImagePath) || !File.Exists(localImagePath))
				return result;

			byte[] imageBytes = GetImageAsByteArray(localImagePath);

			if (imageBytes == null || imageBytes.Length == 0)
				return result;

			try
			{
				result = await ProcessImageAsByteArray(imageBytes);
			}
			catch (Exception ex)
			{
				// TODO log exception

				result.Succeeded = false;
			}

			return result;
		}

		/// <summary>
		/// Process an image byte array
		/// </summary>
		/// <param name="imageBytes">Local file path</param>
		/// <returns></returns>
		public async Task<ServiceResult> ProcessImageAsByteArray(byte[] imageBytes)
		{
			ServiceResult result = new ServiceResult();

			if (imageBytes == null || imageBytes.Length == 0)
				return result;

			try
			{
				using (var content = GetImagePayload(imageBytes))
				{
					HttpResponseMessage response = await this.HttpClient.PostAsync(string.Empty, content);

					result = await GetFaceResult(response);
				}
			}
			catch (Exception ex)
			{
				// TODO log exception

				result.Succeeded = false;
			}

			return result;
		}

		/// <summary>
		/// Process a file that's at a public URI
		/// </summary>
		/// <param name="imageUrl"></param>
		/// <returns></returns>
		public async Task<ServiceResult> ProcessImageAtUrl(string imageUrl)
		{
			ServiceResult result = new ServiceResult();

			if (string.IsNullOrWhiteSpace(imageUrl))
				return result;

			try
			{
				HttpContent content = GetImageUriContent(imageUrl);

				if (content != null)
				{
					HttpResponseMessage response = await this.HttpClient.PostAsync(string.Empty, content);

					result = await GetFaceResult(response);
				}
			}
			catch (Exception ex)
			{
				// TODO log exception

				result.Succeeded = false;
			}


			return result;
		}

		private async Task<ServiceResult> GetFaceResult(HttpResponseMessage response)
		{
			ServiceResult result = new ServiceResult();

			if (response == null)
				return result;

			result.Succeeded = response.IsSuccessStatusCode;
			result.ServiceMessage = response.ReasonPhrase;

			try
			{
				if (response.IsSuccessStatusCode)
				{
					string responseContent = await response.Content.ReadAsStringAsync();

					result.Faces = JsonConvert.DeserializeObject<List<FaceResult>>(responseContent);
				}
			}
			catch (Exception ex)
			{
				// TODO log exception

				result.Succeeded = false;
			}

			return result;
		}

		/// <summary>
		/// Utility method to deserialize an image from the filesystem to a byte array, needed to pass to the cognitive service REST endpoint
		/// </summary>
		/// <param name="imageFilePath"></param>
		/// <returns></returns>
		private byte[] GetImageAsByteArray(string imageFilePath)
		{
			if (string.IsNullOrWhiteSpace(imageFilePath) || !File.Exists(imageFilePath))
				return null;

			if (!ValidateFileSize(imageFilePath))
				throw new ArgumentException("This file exceeds 2GB in size. Please specify a file smaller than 2GB.", nameof(imageFilePath));


			byte[] result = null;

			try
			{
				using (FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
				{
					using (BinaryReader binaryReader = new BinaryReader(fileStream))
					{
						result = binaryReader.ReadBytes((int)fileStream.Length);
					}
				}
			}
			catch (Exception ex)
			{
				// TODO log exception

				result = null;
			}

			return result;
		}

		private HttpClient GetHttpClient()
		{
			var client = new HttpClient();
			client.BaseAddress = new Uri(this.ApiUrl + SERVICEPARAMS);
			client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.ApiKey);

			return client;
		}

		private ByteArrayContent GetImagePayload(byte[] image)
		{
			if (image == null || image.Length == 0)
				return null;

			ByteArrayContent result = null;

			try
			{
				result = new ByteArrayContent(image);

				result.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
			}
			catch (Exception ex)
			{
				// TODO log exception

				result = null;
			}

			return result;
		}

		private HttpContent GetImageUriContent(string imageUri)
		{
			if (string.IsNullOrWhiteSpace(imageUri))
				return null;

			HttpContent result = null;

			try
			{
				var param = JsonConvert.SerializeObject(new { url = imageUri });

				result = new StringContent(param, Encoding.UTF8, "application/json");
			}
			catch (Exception ex)
			{
				// TODO log exception

				result = null;
			}

			return result;
		}

		private bool ValidateFileSize(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
				return false;

			bool result = false;

			try
			{
				FileInfo info = new FileInfo(filePath);

				result = (info.Length <= Int32.MaxValue);
			}
			catch (Exception ex)
			{
				// TODO log exception

				result = false;
			}

			return result;
		}
	}
}
