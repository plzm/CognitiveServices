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
        log.LogInformation("Empty or null queue message passed. Exiting.");

    // //////////////////////////////////////////////////
    // Get info from app config
    string storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
    string storageAccountKey = Environment.GetEnvironmentVariable("StorageAccountKey");
    string storageQueueName = Environment.GetEnvironmentVariable("StorageQueueName");
    string sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
    string cogSvcEndpointFormRec = Environment.GetEnvironmentVariable("CogSvcEndpointFormRec");
    string cogSvcApiKeyFormRec = Environment.GetEnvironmentVariable("CogSvcApiKeyFormRec");

    if (!(cogSvcEndpointFormRec.EndsWith("/")))
        cogSvcEndpointFormRec += "/";

    // log.LogInformation($"{nameof(storageAccountName)} = {storageAccountName}");
    // log.LogInformation($"{nameof(storageAccountKey)} = {storageAccountKey}");
    // log.LogInformation($"{nameof(storageQueueName)} = {storageQueueName}");
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

    // dynamic formRecOutput = JsonConvert.DeserializeObject(responseBody);
    JObject formRecOutput = JObject.Parse(responseBody);

    string status = formRecOutput["status"].ToString().ToLowerInvariant();
    log.LogInformation($"{nameof(status)} = {status}");

    if (status == "succeeded")
    {
        // Save results to database

        // Prepare SQL query params
        var procParams = new DynamicParameters();
        procParams.Add("@ReceiptGuid", null, dbType: DbType.Guid, direction: ParameterDirection.Input, size: 32);
        procParams.Add("@ImageUrl", imageUrl, dbType: DbType.String, direction: ParameterDirection.Input, size: 1000);
        procParams.Add("@JsonFormRecognizer", responseBody, dbType: DbType.String, direction: ParameterDirection.Input, size: -1);

        // Exec SQL query
        using (IDbConnection db = new SqlConnection(sqlConnectionString))
        {
            var result = await db.ExecuteAsync("data.SaveReceipt", procParams, commandType: CommandType.StoredProcedure);
        }

        log.LogInformation("Succeeded: saved to database.");
    }
    else if (status == "failed")
    {
        log.LogError($"Failed: {queueMessage}");
    }
    else
    {
        // Not finished yet - requeue the message with a timeout before becoming visible in the queue

        double secondsBeforeVisibleOnQueue = 10;
        TimeSpan tsBeforeVisible = TimeSpan.FromSeconds(secondsBeforeVisibleOnQueue);

        var reQueueMessage = new
        {
            imageUrl = imageUrl,
            operationLocation = operationLocation,
            created = created,
            checkCount = checkCount + 1
        };

        string reQueueMessageText = JsonConvert.SerializeObject(reQueueMessage);

        StorageCredentials storageCredentials = pelazem.azure.storage.Common.GetStorageCredentials(storageAccountName, storageAccountKey);
        CloudStorageAccount storageAccount = pelazem.azure.storage.Common.GetStorageAccount(storageCredentials);
        pelazem.azure.storage.Queue queue = new pelazem.azure.storage.Queue();
        CloudQueue receiptQueue = (await queue.GetQueueAsync(storageAccount, storageQueueName, true)).Output as CloudQueue;
        OpResult enqueueResult = await queue.EnqueueMessageAsync(storageAccount, receiptQueue, reQueueMessageText, null, tsBeforeVisible);

        log.LogInformation(enqueueResult.Succeeded.ToString());
    }

    // //////////////////////////////////////////////////
}
