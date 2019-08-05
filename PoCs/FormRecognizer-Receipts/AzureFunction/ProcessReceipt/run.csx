#r "Microsoft.Azure.EventGrid"
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
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pelazem.azure.storage;
using pelazem.http;
using pelazem.util;

public static async Task Run(EventGridEvent eventGridEvent, ILogger log)
{
    // Reference: https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.eventgrid.models.eventgridevent?view=azure-dotnet

    // The event grid event subject is useful to know if a "subject starts with" filter will be applied to the event grid subscription
    log.LogInformation($"{nameof(eventGridEvent.Subject)} = {eventGridEvent.Subject}");
    // Event grid event data
    log.LogInformation(eventGridEvent.Data.ToString());

    // //////////////////////////////////////////////////
    // Get info from app config
    string sharedAccessPolicyName = Environment.GetEnvironmentVariable("StorageSharedAccessPolicyName");
    string storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
    string storageAccountKey = Environment.GetEnvironmentVariable("StorageAccountKey");
    string sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
    string cogSvcEndpointCompVis = Environment.GetEnvironmentVariable("CogSvcEndpointCompVis");
    string cogSvcApiKeyCompVis = Environment.GetEnvironmentVariable("CogSvcApiKeyCompVis");
    // string cogSvcEndpointFormRec = Environment.GetEnvironmentVariable("CogSvcEndpointFormRec");
    // string cogSvcApiKeyFormRec = Environment.GetEnvironmentVariable("CogSvcApiKeyFormRec");

    if (!(cogSvcEndpointCompVis.EndsWith("/")))
        cogSvcEndpointCompVis += "/";

    // if (!(cogSvcEndpointFormRec.EndsWith("/")))
    //     cogSvcEndpointFormRec += "/";

    // log.LogInformation($"{nameof(sharedAccessPolicyName)} = {sharedAccessPolicyName}");
    // log.LogInformation($"{nameof(storageAccountName)} = {storageAccountName}");
    // log.LogInformation($"{nameof(storageAccountKey)} = {storageAccountKey}");
    // log.LogInformation($"{nameof(sqlConnectionString)} = {sqlConnectionString}");
    // log.LogInformation($"{nameof(cogSvcEndpointCompVis)} = {cogSvcEndpointCompVis}");
    // log.LogInformation($"{nameof(cogSvcApiKeyCompVis)} = {cogSvcApiKeyCompVis}");
    // log.LogInformation($"{nameof(cogSvcEndpointFormRec)} = {cogSvcEndpointFormRec}");
    // log.LogInformation($"{nameof(cogSvcApiKeyFormRec)} = {cogSvcApiKeyFormRec}");


    // //////////////////////////////////////////////////

    // //////////////////////////////////////////////////
    // The event data payload is JSON. Let's deserialize it and get our blob API and blob URL.
    string payload = eventGridEvent.Data.ToString();
    JObject jpayload = JObject.Parse(payload);
    
    string api = jpayload["api"].ToString();
    string blobUrl = jpayload["url"].ToString();
    int contentLength = pelazem.util.Converter.GetInt32(jpayload["contentLength"]);

    // log.LogInformation($"{nameof(api)} = {api}");
    // log.LogInformation($"{nameof(blobUrl)} = {blobUrl}");
    // log.LogInformation($"{nameof(contentLength)} = {contentLength}");

    // We only want to operate on PutBlob events, not DeleteBlob; and content length > 0
    if (api != "PutBlob" || contentLength <= 0)
    {
        log.LogInformation("Exiting - this Function only applies on PutBlob API events, and for blobs of content length > 0.");
        return;
    }
    // //////////////////////////////////////////////////

    // //////////////////////////////////////////////////
    // Get URL enriched with storage access policy for secure access

    StorageCredentials storageCredentials = pelazem.azure.storage.Common.GetStorageCredentials(storageAccountName, storageAccountKey);

    CloudStorageAccount storageAccount = pelazem.azure.storage.Common.GetStorageAccount(storageCredentials);

    Blob blob = new pelazem.azure.storage.Blob();

    string blobSapUrl = await blob.GetBlobSAPUrlFromBlobUrlAsync(storageAccount, blobUrl, sharedAccessPolicyName);

    // log.LogInformation($"{nameof(blobSapUrl)} = {blobSapUrl}");
    // //////////////////////////////////////////////////


    // //////////////////////////////////////////////////
    // Invoke cognitive services

    string contentType = "application/json";

    // Computer vision
    string urlCompVis = cogSvcEndpointCompVis + "vision/v2.0/analyze?language=en&visualFeatures=Adult,Brands,Categories,Color,Description,Faces,ImageType,Objects,Tags";

    log.LogInformation($"{nameof(urlCompVis)} = {urlCompVis}");

    HttpUtil httpUtilCompVis = new HttpUtil();
    httpUtilCompVis.AddRequestHeader("Ocp-Apim-Subscription-Key", cogSvcApiKeyCompVis);

    string bodyCompVis = $"{{\"url\":\"{blobSapUrl}\"}}";

    // log.LogInformation($"{nameof(bodyCompVis)} = {bodyCompVis}");

    HttpContent contentCompVis = httpUtilCompVis.GetHttpContent(bodyCompVis, contentType);

    OpResult resultCompVis = await httpUtilCompVis.PostAsync(urlCompVis, contentCompVis);

    log.LogInformation(resultCompVis.Succeeded.ToString());
    log.LogInformation(resultCompVis.Message);
    // log.LogInformation(resultCompVis.Output?.ToString());
    // //////////////////////////////////////////////////


    // //////////////////////////////////////////////////
    // Save results to database

    // Prepare SQL query params
    var procParams = new DynamicParameters();
    procParams.Add("@ReceiptGuid", null, dbType: DbType.Guid, direction: ParameterDirection.Input, size: 32);
    procParams.Add("@ImageUrl", blobSapUrl, dbType: DbType.String, direction: ParameterDirection.Input, size: 1000);
    procParams.Add("@JsonCustomVision", resultCompVis.Output?.ToString(), dbType: DbType.String, direction: ParameterDirection.Input, size: -1);
    procParams.Add("@JsonFormsRecognizer", string.Empty, dbType: DbType.String, direction: ParameterDirection.Input, size: -1);

    // Exec SQL query
    using (IDbConnection db = new SqlConnection(sqlConnectionString))
    {
        var result = await db.ExecuteAsync("data.SaveReceipt", procParams, commandType: CommandType.StoredProcedure);
    }
    // //////////////////////////////////////////////////
}
