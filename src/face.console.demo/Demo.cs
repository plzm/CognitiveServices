using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace face.demoapp
{
	public class Demo
	{
		#region Constants

		/// Parameters we will append to the call to the cog svc endpoint to determine what the service returns
		private const string SERVICEPARAMS = "?returnFaceId=true&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise&returnFaceLandmarks=false";

		#endregion

		#region Variables

		private List<string> _imageUrls = new List<string>()
		{
			"https://pzpubliceus.blob.core.windows.net/public/34.jpg",
			"https://pzpubliceus.blob.core.windows.net/public/38.jpg"
		};

		private HttpClient _httpClient = null;

		#endregion

		#region Properties

		private string ApiUrl { get; set;  }
		private string ApiKey { get; set;  }

		private HttpClient HttpClient
		{
			get
			{
				// Use an instance-lifetime HttpClient rather than re-instantiating at each call

				if (_httpClient == null)
					_httpClient = this.GetHttpClient();

				return _httpClient;
			}
		}

		#endregion

		#region ctors

		private Demo() { }

		public Demo(string apiUrl, string apiKey)
		{
			this.ApiUrl = apiUrl;
			this.ApiKey = apiKey;
		}

		#endregion

		/// <summary>
		/// This is a minimal implementation that flattens code found in face.lib, where it includes error handling and is better broken up.
		/// </summary>
		/// <returns></returns>
		public async Task RunWithUrls()
		{
			// Iterate through each image URL
			foreach (string imageUrl in _imageUrls)
			{
				Console.WriteLine(imageUrl);

				// URL-encode the image URL and prepare to pass it to the API endpoint as JSON
				string json = JsonConvert.SerializeObject(new { url = imageUrl });
				HttpContent imageUrlContent = new StringContent(json, Encoding.UTF8, "application/json");

				// Call the Cognitive Service REST API endpoint and get an HTTP response message
				HttpResponseMessage response = await this.HttpClient.PostAsync(string.Empty, imageUrlContent);

				// Get the JSON response from the API
				string responseContent = await response.Content.ReadAsStringAsync();

				Console.WriteLine(responseContent);
				Console.WriteLine();
			}
		}

		public async Task RunWithFiles()
		{
			List<string> imageFilePaths = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "images"), "*.jpg", SearchOption.TopDirectoryOnly).ToList();

			foreach (string imageFilePath in imageFilePaths)
			{
				byte[] imageBytes = GetImageAsByteArray(imageFilePath);

				ByteArrayContent httpRequestContent = GetImageHttpRequestContent(imageBytes);

				HttpResponseMessage response = await this.HttpClient.PostAsync(string.Empty, httpRequestContent);

				// Get the JSON response from the API
				string responseContent = await response.Content.ReadAsStringAsync();

				Console.WriteLine(responseContent);
				Console.WriteLine();
			}
		}

		private byte[] GetImageAsByteArray(string imageFilePath)
		{
			byte[] result = null;

			using (FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
			{
				using (BinaryReader binaryReader = new BinaryReader(fileStream))
				{
					result = binaryReader.ReadBytes((int)fileStream.Length);
				}
			}

			return result;
		}

		private ByteArrayContent GetImageHttpRequestContent(byte[] image)
		{
			ByteArrayContent result = new ByteArrayContent(image);

			result.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

			return result;
		}

		private HttpClient GetHttpClient()
		{
			var client = new HttpClient();
			client.BaseAddress = new Uri(this.ApiUrl + SERVICEPARAMS);
			client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.ApiKey);

			return client;
		}
	}
}
