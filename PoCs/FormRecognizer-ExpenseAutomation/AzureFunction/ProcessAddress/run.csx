#r "Newtonsoft.Json"

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pelazem.azure.storage;
using pelazem.http;
using pelazem.util;

public static async Task Run(string queueMessage, ILogger log)
{
    log.LogInformation($"Process Address: {queueMessage}");

    if (string.IsNullOrEmpty(queueMessage))
    {
        log.LogInformation("Empty or null queue message passed. Exiting.");
        return;
    }

    // //////////////////////////////////////////////////
    // Queue message is JSON. Let's deserialize it and get what we need
    JObject jpayload = JObject.Parse(queueMessage);
    
    string documentGuid = jpayload["documentGuid"].ToString();
    string addressText = jpayload["address"].ToString();
    
    log.LogInformation($"{nameof(documentGuid)} = {documentGuid}");
    log.LogInformation($"{nameof(addressText)} = {addressText}");

    if (string.IsNullOrWhiteSpace(addressText))
    {
        log.LogInformation("Empty address passed. Exiting.");
        return;
    }
    // //////////////////////////////////////////////////
    // Get info from app config
    string azureMapsApiEndpoint = Environment.GetEnvironmentVariable("AzureMapsApiEndpoint");
    string azureMapsApiVersion = Environment.GetEnvironmentVariable("AzureMapsApiVersion");
    string azureMapsApiKey = Environment.GetEnvironmentVariable("AzureMapsApiKey");

    // //////////////////////////////////////////////////
    // Azure Maps
    string azureMapsUrl = $"{azureMapsApiEndpoint}?api-version={azureMapsApiVersion}&subscription-key={azureMapsApiKey}&query={addressText}";
    // log.LogInformation($"{nameof(azureMapsUrl)}: {azureMapsUrl}");

    HttpUtil httpUtil = new HttpUtil();

    OpResult result = await httpUtil.GetAsync(azureMapsUrl);

    // log.LogInformation(result.Succeeded.ToString());
    // log.LogInformation(result.Message);

    HttpResponseMessage httpResponseMessage = result.Output as HttpResponseMessage;

    string azureMapsResultJson = await httpUtil.GetHttpResponseContentAsync(httpResponseMessage);

    JObject azureMapsResultPayload = JObject.Parse(azureMapsResultJson);

    var azureMapsSummary = azureMapsResultPayload["summary"];
    var azureMapsResults = azureMapsResultPayload["results"];

    foreach (var azureMapsResult in azureMapsResults)
    {
        var addressRecord = new
        {
            summary = azureMapsSummary,
            address = azureMapsResult
        };

        string addressJson = JsonConvert.SerializeObject(addressRecord);

        await SaveAddress(documentGuid, addressJson, log);
    }
}

internal static async Task SaveAddress(string documentGuid, string addressJson, ILogger log)
{
    // log.LogInformation("SaveAddress");

    string sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

    // Prepare SQL query params
    var procParams = new DynamicParameters();
    procParams.Add("@DocumentGuid", new Guid(documentGuid), dbType: DbType.Guid, direction: ParameterDirection.Input, size: 32);
    procParams.Add("@AddressJson", addressJson, dbType: DbType.String, direction: ParameterDirection.Input, size: -1);

    // Exec SQL query
    using (IDbConnection db = new SqlConnection(sqlConnectionString))
    {
        var result = await db.ExecuteAsync("data.CreateAddress", procParams, commandType: CommandType.StoredProcedure);
    }
}