#r "Microsoft.Azure.EventGrid"
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json;
using pelazem.azure.storage;
using pelazem.azure.cognitive.videoindexer;

public static async Task Run(EventGridEvent eventGridEvent, ILogger log)
{
	string storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
	string storageAccountKey = Environment.GetEnvironmentVariable("StorageAccountKey");
	string policyName = Environment.GetEnvironmentVariable("PolicyName");
	string containerNameVideoRaw = Environment.GetEnvironmentVariable("ContainerNameVideoRaw");

	string videoIndexerApiAccountId = Environment.GetEnvironmentVariable("VideoIndexerApiAccountId");
	string videoIndexerApiUrl = Environment.GetEnvironmentVariable("VideoIndexerApiUrl");
	string videoIndexerApiKey = Environment.GetEnvironmentVariable("VideoIndexerApiKey");
	string videoIndexerApiAzureRegion = Environment.GetEnvironmentVariable("VideoIndexerApiAzureRegion");

	log.LogInformation(nameof(storageAccountName) + "=" + storageAccountName);
	log.LogInformation(nameof(storageAccountKey) + "=" + storageAccountKey);
	log.LogInformation(nameof(policyName) + "=" + policyName);
	log.LogInformation(nameof(containerNameVideoRaw) + "=" + containerNameVideoRaw);

	log.LogInformation(nameof(videoIndexerApiAccountId) + "=" + videoIndexerApiAccountId);
	log.LogInformation(nameof(videoIndexerApiUrl) + "=" + videoIndexerApiUrl);
	log.LogInformation(nameof(videoIndexerApiKey) + "=" + videoIndexerApiKey);
	log.LogInformation(nameof(videoIndexerApiAzureRegion) + "=" + videoIndexerApiAzureRegion);

    log.LogInformation(nameof(eventGridEvent.Topic) + "=" + eventGridEvent.Topic);
    log.LogInformation(nameof(eventGridEvent.Subject) + "=" + eventGridEvent.Subject);
    log.LogInformation(nameof(eventGridEvent.EventType) + "=" + eventGridEvent.EventType);
    log.LogInformation(nameof(eventGridEvent.EventTime) + "=" + eventGridEvent.EventTime);

	dynamic eventPayload = JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());

	string api = eventPayload.api;
	// string blobType = eventPayload.blobType;
	string contentType = eventPayload.contentType;
	// string contentLength = eventPayload.contentLength.ToString();
	string url = eventPayload.url;

	log.LogInformation(nameof(api) + "=" + api);
	// log.LogInformation(nameof(blobType) + "=" + blobType);
	log.LogInformation(nameof(contentType) + "=" + contentType);
	// log.LogInformation(nameof(contentLength) + "=" + contentLength);
	log.LogInformation(nameof(url) + "=" + url);

    bool proceed = (api == "CopyBlob" || api == "PutBlob");
    if (!proceed)
    {
        log.LogInformation("Exiting");
        return;
    }

    string callbackUrl = "https://pzvideoindexer2.azurewebsites.net/api/PersistResults";

    Uri uri = new Uri(url);
    string videoName = Path.GetFileName(uri.AbsolutePath);

    // Get Shared Access Policy URL for the blob
    StorageConfig storageConfig = new StorageConfig() { ContainerName = containerNameVideoRaw, StorageAccountName = storageAccountName, StorageAccountKey = storageAccountKey };
    ServiceClient storageService = new ServiceClient();
    string sasUrl = await storageService.GetBlobSAPUrlFromBlobUrlAsync(storageConfig, url, policyName);

	log.LogInformation(nameof(sasUrl) + "=" + sasUrl);

    // Initiate upload to the Video Indexer Cognitive Service
    VideoIndexerService videoIndexerService = new VideoIndexerService(videoIndexerApiAccountId, videoIndexerApiUrl, videoIndexerApiKey, videoIndexerApiAzureRegion);
    VideoIndexerVideoInput videoInput = new VideoIndexerVideoInput(){ Name = videoName, UrlOriginal = url, UrlVisibleToVideoIndexer = sasUrl };
    string videoId = await videoIndexerService.UploadVideo(videoInput, string.Empty, callbackUrl);

	log.LogInformation(nameof(videoId) + "=" + videoId);
}
