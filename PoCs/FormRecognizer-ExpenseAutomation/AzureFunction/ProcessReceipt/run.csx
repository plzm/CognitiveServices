#r "Microsoft.Azure.EventGrid"
#r "Newtonsoft.Json"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pelazem.azure.storage;
using pelazem.http;
using pelazem.util;

public static async Task<string> Run(EventGridEvent eventGridEvent, ILogger log)
{
    log.LogInformation(eventGridEvent.Data.ToString());

    // //////////////////////////////////////////////////
    // Get info from app config
    string sharedAccessPolicyName = Environment.GetEnvironmentVariable("StorageSharedAccessPolicyNameReadList");
    string storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
    string storageAccountKey = Environment.GetEnvironmentVariable("StorageAccountKey");
    string cogSvcEndpointFormRecReceiptAnalyze = Environment.GetEnvironmentVariable("CogSvcEndpointFormRecReceiptAnalyze");
    string cogSvcApiKeyFormRec = Environment.GetEnvironmentVariable("CogSvcApiKeyFormRec");

    log.LogInformation($"{nameof(sharedAccessPolicyName)} = {sharedAccessPolicyName}");
    log.LogInformation($"{nameof(storageAccountName)} = {storageAccountName}");
    log.LogInformation($"{nameof(storageAccountKey)} = {storageAccountKey}");
    log.LogInformation($"{nameof(cogSvcEndpointFormRecReceiptAnalyze)} = {cogSvcEndpointFormRecReceiptAnalyze}");
    log.LogInformation($"{nameof(cogSvcApiKeyFormRec)} = {cogSvcApiKeyFormRec}");
    // //////////////////////////////////////////////////

    // //////////////////////////////////////////////////
    // Reference: https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.eventgrid.models.eventgridevent?view=azure-dotnet

    // The event grid event subject is useful to know if a "subject starts with" filter will be applied to the event grid subscription
    log.LogInformation($"{nameof(eventGridEvent.Subject)} = {eventGridEvent.Subject}");
    // Event grid event data
    log.LogInformation(eventGridEvent.Data.ToString());

    // The event data payload is JSON. Let's deserialize it and get our blob API and blob URL.
    string payload = eventGridEvent.Data.ToString();
    JObject jpayload = JObject.Parse(payload);
    
    string api = jpayload["api"].ToString();
    string blobUrl = jpayload["url"].ToString();
    int contentLength = pelazem.util.Converter.GetInt32(jpayload["contentLength"]);

    log.LogInformation($"{nameof(api)} = {api}");
    log.LogInformation($"{nameof(blobUrl)} = {blobUrl}");
    log.LogInformation($"{nameof(contentLength)} = {contentLength}");

    // We only want to operate on PutBlob events, not DeleteBlob; and content length > 0
    if (api != "PutBlob" || contentLength <= 0)
    {
        log.LogInformation("Exiting - this Function only applies on PutBlob API events, and for blobs of content length > 0.");
        return "";
    }
    // //////////////////////////////////////////////////

    // //////////////////////////////////////////////////
    // Get URL enriched with storage access policy for secure access

    StorageCredentials storageCredentials = pelazem.azure.storage.Common.GetStorageCredentials(storageAccountName, storageAccountKey);

    CloudStorageAccount storageAccount = pelazem.azure.storage.Common.GetStorageAccount(storageCredentials);

    Blob blob = new pelazem.azure.storage.Blob();

    string blobSapUrl = await blob.GetBlobSAPUrlFromBlobUrlAsync(storageAccount, blobUrl, sharedAccessPolicyName);

    log.LogInformation($"{nameof(blobSapUrl)} = {blobSapUrl}");
    // //////////////////////////////////////////////////


    // //////////////////////////////////////////////////
    // Invoke cognitive services

    string contentType = "application/json";

    // Form Recognizer
    HttpUtil httpUtilFormRec = new HttpUtil();
    httpUtilFormRec.AddRequestHeader("Ocp-Apim-Subscription-Key", cogSvcApiKeyFormRec);

    string bodyFormRec = $"{{\"url\":\"{blobSapUrl}\"}}";

    HttpContent contentFormRec = httpUtilFormRec.GetHttpContent(bodyFormRec, contentType);

    OpResult resultFormRec = await httpUtilFormRec.PostAsync(cogSvcEndpointFormRecReceiptAnalyze, contentFormRec);

    log.LogInformation(resultFormRec.Succeeded.ToString());
    log.LogInformation(resultFormRec.Message);

    HttpResponseMessage responseFormRec = resultFormRec.Output as HttpResponseMessage;

    string operationLocation = responseFormRec?.Headers?.GetValues("Operation-Location")?.First();
    log.LogInformation($"{nameof(operationLocation)} = {operationLocation}");

    var queueMessage = new
    {
        imageUrl = blobSapUrl,
        operationLocation = operationLocation,
        created = string.Format("{0:O}", DateTime.UtcNow),
        checkCount = 0
    };

	// Write our queue message to, well, the output queue. Since this Function is async, we have to return, not use an output variable.
    return JsonConvert.SerializeObject(queueMessage);
}
