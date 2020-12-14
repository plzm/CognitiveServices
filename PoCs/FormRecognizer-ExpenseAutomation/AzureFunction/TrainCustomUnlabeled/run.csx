#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pelazem.azure.storage;
using pelazem.http;
using pelazem.util;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");

    // string name = req.Query["name"];

    // string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    // dynamic data = JsonConvert.DeserializeObject(requestBody);
    // name = name ?? data?.name;

    // return name != null
    //     ? (ActionResult)new OkObjectResult($"Hello, {name}")
    //     : new BadRequestObjectResult("Please pass a name on the query string or in the request body");

    // //////////////////////////////////////////////////
    // Get info from app config
    string storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
    string storageAccountKey = Environment.GetEnvironmentVariable("StorageAccountKey");
    
    string sharedAccessPolicyName = Environment.GetEnvironmentVariable("StorageSharedAccessPolicyNameReadList");

    string containerNameHotelFoliosTrainUnlabeled = Environment.GetEnvironmentVariable("ContainerNameHotelFoliosTrainUnlabeled");

    string cogSvcApiKeyFormRec = Environment.GetEnvironmentVariable("CogSvcApiKeyFormRec");
    string cogSvcEndpointFormRecCustomUnlabeledTrain = Environment.GetEnvironmentVariable("CogSvcEndpointFormRecCustomUnlabeledTrain");

    log.LogInformation($"{nameof(storageAccountName)} = {storageAccountName}");
    log.LogInformation($"{nameof(storageAccountKey)} = {storageAccountKey}");

    log.LogInformation($"{nameof(sharedAccessPolicyName)} = {sharedAccessPolicyName}");

    log.LogInformation($"{nameof(containerNameHotelFoliosTrainUnlabeled)} = {containerNameHotelFoliosTrainUnlabeled}");

    log.LogInformation($"{nameof(cogSvcApiKeyFormRec)} = {cogSvcApiKeyFormRec}");
    log.LogInformation($"{nameof(cogSvcEndpointFormRecCustomUnlabeledTrain)} = {cogSvcEndpointFormRecCustomUnlabeledTrain}");
    // //////////////////////////////////////////////////

    // //////////////////////////////////////////////////
    // Source container URL Prep
    StorageCredentials cred = new StorageCredentials(storageAccountName, storageAccountKey);
    CloudStorageAccount sa = pelazem.azure.storage.Common.GetStorageAccount(cred);
	Blob blob = new pelazem.azure.storage.Blob();
    string url = await blob.GetContainerSAPUrlAsync(sa, containerNameHotelFoliosTrainUnlabeled, sharedAccessPolicyName);
    log.LogInformation($"{nameof(url)}={url}");
    // //////////////////////////////////////////////////

    // //////////////////////////////////////////////////
    // Form Recognizer

    string contentType = "application/json";

    HttpUtil httpUtilFormRec = new HttpUtil();
    httpUtilFormRec.AddRequestHeader("Ocp-Apim-Subscription-Key", cogSvcApiKeyFormRec);

    string bodyFormRec = $"{{\"source\":\"{url}\"}}";

    HttpContent contentFormRec = httpUtilFormRec.GetHttpContent(bodyFormRec, contentType);

    OpResult resultFormRec = await httpUtilFormRec.PostAsync(cogSvcEndpointFormRecCustomUnlabeledTrain, contentFormRec);

    log.LogInformation(resultFormRec.Succeeded.ToString());
    log.LogInformation(resultFormRec.Message);

    HttpResponseMessage responseFormRec = resultFormRec.Output as HttpResponseMessage;

    string location = responseFormRec?.Headers?.GetValues("Location")?.First();
    string[] delims = {"/"};
    string[] rawParts = location.Split(delims, StringSplitOptions.RemoveEmptyEntries);
    string modelId = rawParts.LastOrDefault();
    
    log.LogInformation($"{nameof(modelId)} = {modelId}");
    // //////////////////////////////////////////////////

    // PERSIST THIS! YOU WILL NEED IT FOR ANALYZE AND ANALYZE RESULTS CALLS
    return (ActionResult)new OkObjectResult(modelId);
}
