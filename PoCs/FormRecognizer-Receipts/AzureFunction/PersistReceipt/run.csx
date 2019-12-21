#r "Newtonsoft.Json"

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Queue;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pelazem.azure.storage;
using pelazem.http;
using pelazem.util;

public static async Task Run(string queueMessage, ILogger log)
{
    log.LogInformation($"C# Queue trigger function processed: {queueMessage}");

    if (string.IsNullOrEmpty(queueMessage))
    {
        log.LogInformation("Empty or null queue message passed. Exiting.");
        return;
    }

    // //////////////////////////////////////////////////
    // Get info from app config
    string storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
    string storageAccountKey = Environment.GetEnvironmentVariable("StorageAccountKey");
    string storageQueueNameReceipts = Environment.GetEnvironmentVariable("StorageQueueNameReceipts");
    string storageQueueNameAddresses = Environment.GetEnvironmentVariable("StorageQueueNameAddresses");
    string sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
    string cogSvcEndpointFormRec = Environment.GetEnvironmentVariable("CogSvcEndpointFormRec");
    string cogSvcApiKeyFormRec = Environment.GetEnvironmentVariable("CogSvcApiKeyFormRec");

    if (!(cogSvcEndpointFormRec.EndsWith("/")))
        cogSvcEndpointFormRec += "/";

    // log.LogInformation($"{nameof(storageAccountName)} = {storageAccountName}");
    // log.LogInformation($"{nameof(storageAccountKey)} = {storageAccountKey}");
    // log.LogInformation($"{nameof(storageQueueNameReceipts)} = {storageQueueNameReceipts}");
    // log.LogInformation($"{nameof(storageQueueNameAddresses)} = {storageQueueNameAddresses}");
    // log.LogInformation($"{nameof(sqlConnectionString)} = {sqlConnectionString}");
    // log.LogInformation($"{nameof(cogSvcEndpointFormRec)} = {cogSvcEndpointFormRec}");
    // log.LogInformation($"{nameof(cogSvcApiKeyFormRec)} = {cogSvcApiKeyFormRec}");
    // //////////////////////////////////////////////////

    // //////////////////////////////////////////////////
    // Queue message is JSON. Let's deserialize it and get what we need
    dynamic payload = JsonConvert.DeserializeObject(queueMessage);

	string imageUrl = payload.imageUrl;
    string operationLocation = payload.operationLocation;
    string created = payload.created;
    int checkCount = pelazem.util.Converter.GetInt32(payload.checkCount);

    log.LogInformation($"{nameof(operationLocation)} = {operationLocation}");
    log.LogInformation($"{nameof(created)} = {created}");
    log.LogInformation($"{nameof(checkCount)} = {checkCount}");
    // //////////////////////////////////////////////////

    // //////////////////////////////////////////////////
    // Form Recognizer

    HttpUtil httpUtilFormRec = new HttpUtil();
    httpUtilFormRec.AddRequestHeader("Ocp-Apim-Subscription-Key", cogSvcApiKeyFormRec);

    OpResult resultFormRec = await httpUtilFormRec.GetAsync(operationLocation);

    HttpResponseMessage responseFormRec = resultFormRec.Output as HttpResponseMessage;

	string responseBody = await httpUtilFormRec.GetHttpResponseContentAsync(responseFormRec);

	// log.LogInformation($"{nameof(responseBody)} = {responseBody}");

    JObject formRecOutput = JObject.Parse(responseBody);

    string status = formRecOutput["status"].ToString().ToLowerInvariant();
    log.LogInformation($"{nameof(status)} = {status}");

    if (status == "succeeded")
    {
        Guid documentGuid = await SaveDocument(responseBody, imageUrl, log);
    	log.LogInformation($"{nameof(documentGuid)} = {documentGuid}");

        var address = formRecOutput["understandingResults"][0]["fields"]["MerchantAddress"];

        if (address.HasValues)
        {
            string addressText = address["text"].ToString();

            await EnqueueAddress(documentGuid, addressText, log);
        }
    }
    else if (status == "failed")
    {
        log.LogError($"Failed: {queueMessage}");
    }
    else
    {
        // Not finished yet - requeue the message with a timeout before becoming visible in the queue
        await RequeueDocument(imageUrl, operationLocation, created, checkCount, log);
    }
}

internal static async Task EnqueueAddress(Guid documentGuid, string address, ILogger log)
{
    log.LogInformation("EnqueueAddress");

    string queueName = Environment.GetEnvironmentVariable("StorageQueueNameAddresses");

    var queueMessage = new
    {
        documentGuid = documentGuid,
        address = address
    };

    await Enqueue(queueMessage, queueName, 0, log);
}

internal static async Task RequeueDocument(string imageUrl, string operationLocation, string created, int checkCount, ILogger log)
{
    log.LogInformation("RequeueDocument");

    string queueName = Environment.GetEnvironmentVariable("StorageQueueNameReceipts");

    double secondsBeforeVisibleOnQueue = 10;

    var queueMessage = new
    {
        imageUrl = imageUrl,
        operationLocation = operationLocation,
        created = created,
        checkCount = checkCount + 1
    };

    await Enqueue(queueMessage, queueName, secondsBeforeVisibleOnQueue, log);
}

internal static async Task Enqueue(object queueMessage, string queueName, double secondsBeforeVisible = 0, ILogger log = null)
{
    log.LogInformation("Enqueue");

    string storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
    string storageAccountKey = Environment.GetEnvironmentVariable("StorageAccountKey");

    string queueMessageText = JsonConvert.SerializeObject(queueMessage);

    StorageCredentials storageCredentials = pelazem.azure.storage.Common.GetStorageCredentials(storageAccountName, storageAccountKey);
    CloudStorageAccount storageAccount = pelazem.azure.storage.Common.GetStorageAccount(storageCredentials);
    pelazem.azure.storage.Queue queue = new pelazem.azure.storage.Queue();
    CloudQueue cloudQueue = (await queue.GetQueueAsync(storageAccount, queueName, true)).Output as CloudQueue;
    
    OpResult enqueueResult = null;

    if (secondsBeforeVisible > 0)
    {
        log.LogInformation($"Enqueueing with delay {secondsBeforeVisible}");
        enqueueResult = await queue.EnqueueMessageAsync(storageAccount, cloudQueue, queueMessageText, null, TimeSpan.FromSeconds(secondsBeforeVisible));
    }
    else
    {
        log.LogInformation($"Enqueueing with delay {secondsBeforeVisible}");
        enqueueResult = await queue.EnqueueMessageAsync(storageAccount, cloudQueue, queueMessageText, null);
    }

    log.LogInformation("Enqueue: " + enqueueResult.Succeeded.ToString());
}

internal static async Task<Guid> SaveDocument(string documentJson, string imageUrl, ILogger log)
{
    log.LogInformation("SaveDocument");

    string sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

    Guid documentGuid = Guid.NewGuid();

    // Prepare SQL query params
    var procParams = new DynamicParameters();
    procParams.Add("@DocumentType", "Receipt", dbType: DbType.String, direction: ParameterDirection.Input, size: 50);
    procParams.Add("@ImageUrl", imageUrl, dbType: DbType.String, direction: ParameterDirection.Input, size: 1000);
    procParams.Add("@DocumentJson", documentJson, dbType: DbType.String, direction: ParameterDirection.Input, size: -1);
    procParams.Add("@DocumentGuid", documentGuid, dbType: DbType.Guid, direction: ParameterDirection.InputOutput, size: 32);

    // Exec SQL query
    using (IDbConnection db = new SqlConnection(sqlConnectionString))
    {
        var result = await db.ExecuteAsync("data.CreateDocument", procParams, commandType: CommandType.StoredProcedure);
    }

    return documentGuid;
}