#!/bin/bash

# ##################################################
# Variables

# Azure
subscription_id="$(az account show -o tsv --query "id")"
location="eastus"
external_ips_allowed="75.68.47.183"

# General
infix="bzg"

# Resource group
resource_group_name="$infix"

# Storage account and containers
storage_template_file="./arm/storage.template.json"
storage_acct_name="$infix""receiptssa"
storage_acct_sku=Standard_LRS
container_name_receipts="receipts-in"
container_name_assets="assets"

# Storage access policy
sap_name_receipts="receipts-in"

# Storage queues
queue_name_receipts="receipts-processing"
queue_name_addresses="addresses-processing"

# App service plan
app_service_plan_name="$infix""asp"
app_service_plan_sku="S1"

# App Insights
app_insights_template_file="./arm/app_insights.template.json"
app_insights_name="$infix""ai"

# Function app name
functionapp_name="$infix""fn"

# Function App MSI scope and role specific to storage (can list roles/role names with az role definition list)
functionapp_msi_scope_storage="/subscriptions/""$subscription_id""/resourceGroups/""$resource_group_name""/providers/Microsoft.Storage/storageAccounts/""$storage_acct_name"
functionapp_msi_role_storage_blob="Storage Blob Data Contributor"
functionapp_msi_role_storage_queue="Storage Queue Data Contributor"

# Event Grid
event_grid_subscription_name="$infix""-blob-receipts-in"

# Azure SQL
sql_admin_username="$infix""-sqladmin"
sql_admin_password=""

azure_sql_server_template_file="./arm/azuresqlserver.template.json"
azure_sql_server_name="$infix""-sql-""$location"

azure_sql_db_template_file="./arm/azuresqldb.template.json"

azure_sql_db_name="documents"
azure_sql_db_sku="S1"
azure_sql_db_tier="Standard"
azure_sql_db_max_size_bytes=268435456000
azure_sql_db_bacpac_file="documents.bacpac"
azure_sql_db_bacpac_path="./$azure_sql_db_bacpac_file"
azure_sql_db_bacpac_storage_uri="https://""$storage_acct_name"".blob.core.windows.net/""$container_name_assets""/""$azure_sql_db_bacpac_file"

azure_sql_security_role_name="DocumentsRole"  # This MUST match what's in the bacpac/database! Do not change this until/unless you know exactly what you're doing and have changed it in those other places!!!

# SQL user and password must match what is in documents.sql/bacpac and is actually deployed
azure_sql_conn_string="Server=tcp:""$azure_sql_server_name"".database.windows.net,1433;Initial Catalog=""$azure_sql_db_name"";Persist Security Info=False;User ID=DocumentsUser;Password=P@ssw0rd2019-;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"

# Cognitive Services
cogsvc_form_recognizer_name="$infix""-cs-formrecognizer"

# Azure Maps
azure_maps_account_name="$infix""maps"
azure_maps_api_endpoint="https://atlas.microsoft.com/search/fuzzy/json"
azure_maps_api_version="1.0"

# ##################################################

# ##################################################
echo "Create resource group"
az group create -l "$location" -n "$resource_group_name" --verbose

echo "Create storage account"
storage_acct_name="$(az group deployment create -g "$resource_group_name" -n "$storage_acct_name" --template-file "$storage_template_file" --verbose --parameters \
	location="$location" storage_account_name_infix="$infix")"
# az storage account create -l "$location" -g "$resource_group_name" -n "$storage_acct_name" --sku "$storage_acct_sku" --kind StorageV2 --verbose

echo "Get storage account key"
azure_storage_acct_key="$(az storage account keys list -g "$resource_group_name" -n "$storage_acct_name" -o tsv --query "[0].value")"

echo "Create storage containers"
az storage container create --account-name "$storage_acct_name" --account-key "$azure_storage_acct_key" -n "$container_name_receipts" --verbose
az storage container create --account-name "$storage_acct_name" --account-key "$azure_storage_acct_key" -n "$container_name_assets" --verbose

echo "Create shared access policy on receipts in container"
az storage container policy create -c "$container_name_receipts" -n "$sap_name_receipts" --account-name "$storage_acct_name" --account-key "$azure_storage_acct_key" --permissions r --start "2019-7-1T00:00:00Z" --expiry "2022-1-1T00:00:00Z"

echo "Create storage queues"
az storage queue create --account-name "$storage_acct_name" --account-key "$azure_storage_acct_key" -n "$queue_name_receipts" --verbose
az storage queue create --account-name "$storage_acct_name" --account-key "$azure_storage_acct_key" -n "$queue_name_addresses" --verbose

# Create app service plan
echo "Create App Service Plan"
az appservice plan create -l $location -g $resource_group_name -n $app_service_plan_name --sku $app_service_plan_sku --verbose

# Create application insights instance and get instrumentation key
echo "Create Application Insights and get Instrumentation Key"
app_insights_key="$(az group deployment create -g $resource_group_name -n $app_insights_name --template-file "$app_insights_template_file" \
	-o tsv --query "properties.outputs.app_insights_instrumentation_key.value" \
	--parameters location="$location" instance_name="$app_insights_name")"

# https://docs.microsoft.com/en-us/cli/azure/functionapp
# Create function app with plan and app insights created above
# Using Windows at this point because MSI on Linux still in preview
echo "Create Function App and link to App Service Plan and App Insights instance created above"
az functionapp create -g $resource_group_name -n $functionapp_name --storage-account $storage_acct_name \
	--app-insights $app_insights_name --app-insights-key $app_insights_key \
	--plan $app_service_plan_name --os-type Windows --runtime dotnet

# https://docs.microsoft.com/en-us/cli/azure/functionapp/identity
# Assign managed identity to function app
echo "Assign managed identity to function app"
functionapp_msi_principal_id="$(az functionapp identity assign -g $resource_group_name -n $functionapp_name -o tsv --query "principalId")"
# echo $functionapp_msi_principal_id

# echo "Sleep to allow MSI identity to finish provisioning"
sleep 120s

# Get managed identity principal and tenant ID
# az functionapp identity show -g $resource_group_name -n $functionapp_name
echo "Get Function App identity Display Name - got Principal ID from az functionapp identity assign"
# functionapp_msi_principal_id="$(az functionapp identity show -g $resource_group_name -n $functionapp_name -o tsv --query "principalId")"
functionapp_msi_display_name="$(az ad sp show --id $functionapp_msi_principal_id -o tsv --query "displayName")"

# Assign Function App MSI rights to storage
echo "Assign MSI principal rights to storage account, blob and queue"
az role assignment create --scope "$functionapp_msi_scope_storage" --assignee-object-id "$functionapp_msi_principal_id" --role "$functionapp_msi_role_storage_blob"
az role assignment create --scope "$functionapp_msi_scope_storage" --assignee-object-id "$functionapp_msi_principal_id" --role "$functionapp_msi_role_storage_queue"


# Create Event Grid subscription for storage events
# This is not working yet, need to figure out what else to do - this fails cannot validate Fn app endpoint
# az eventgrid event-subscription create --name $event_grid_subscription_name \
# 	--source-resource-id "/subscriptions/""$subscription_id""/resourceGroups/""$resource_group_name""/providers/Microsoft.Storage/storageaccounts/""$storage_acct_name" \
# 	--subject-begins-with "/blobServices/default/containers/""$container_name_receipts""/blobs/" \
# 	--endpoint "https://""$functionapp_name"".azurewebsites.net/runtime/webhooks/EventGrid"



# echo "Upload bacpac file to import into Azure SQL database"
# az storage blob upload --account-name "$storage_acct_name" --account-key "$azure_storage_acct_key" -c "$storage_container_assets" -n "$azure_sql_db_bacpac_file" -f "$azure_sql_db_bacpac_path"

echo "Create Azure SQL virtual server"
az group deployment create -g "$resource_group_name" -n "$azure_sql_server_name" --template-file "$azure_sql_server_template_file" --parameters \
	location="$location" server_name="$azure_sql_server_name" server_admin_username="$sql_admin_username" server_admin_password="$sql_admin_password" \
	alerts_email_address="$azure_alerts_email_address" audit_storage_account_name="$storage_acct_name" audit_storage_account_key="$azure_storage_acct_key" \
	firewall_rule_start_ip="$external_ips_allowed" firewall_rule_end_ip="$external_ips_allowed"

echo "Deploy database (read scale-out and zone redundancy only available for Azure SQL DB Premium)"
az group deployment create -g "$resource_group_name" -n "$azure_sql_db_name" --template-file "$azure_sql_db_template_file" --parameters \
	location="$location" server_name="$azure_sql_server_name" db_name="$azure_sql_db_name" \
	db_sku="$azure_sql_db_sku" db_tier="$azure_sql_db_tier" db_max_size_bytes="$azure_sql_db_max_size_bytes" \
	db_read_scale="Disabled" db_zone_redundant=false audit_storage_account_name="$storage_acct_name" audit_storage_account_key="$azure_storage_acct_key"

echo "Restore bacpac to Azure SQL ref database"
az sql db import -g "$resource_group_name" -s "$azure_sql_server_name" -n "$azure_sql_db_name" \
    -u "$sql_admin_username" -p "$sql_admin_password" \
    --storage-uri "$azure_sql_db_bacpac_storage_uri" --storage-key "$azure_storage_acct_key" --storage-key-type "StorageAccessKey"


echo "Deploy cognitive services"
az cognitiveservices account create -l "westus2" -g $resource_group_name -n $cogsvc_form_recognizer_name --kind "FormRecognizer" --sku "S0" --yes --verbose

# echo "Show cognitive services"
# az cognitiveservices account show -g $resource_group_name -n $cogsvc_form_recognizer_name

echo "Get form recognizer cognitive service endpoint and key"
cogsvc_form_recognizer_endpoint="$(az cognitiveservices account show -g $resource_group_name -n $cogsvc_form_recognizer_name -o tsv --query "endpoint")""formrecognizer/v1.0-preview/prebuilt/receipt/"
cogsvc_form_recognizer_key="$(az cognitiveservices account keys list -g "$resource_group_name" -n "$cogsvc_form_recognizer_name" -o tsv --query "key1")"

echo "Deploy Azure Maps account"
az maps account create -g "$resource_group_name" -n "$azure_maps_account_name" --accept-tos --sku S0 --verbose
azure_maps_api_key="$(az maps account keys list -g "$resource_group_name" -n "$azure_maps_account_name" -o tsv --query "primaryKey")"

echo "Set Function App settings"
az functionapp config appsettings set -g $resource_group_name -n $functionapp_name --settings "StorageSharedAccessPolicyName=$sap_name_receipts"
az functionapp config appsettings set -g $resource_group_name -n $functionapp_name --settings "StorageAccountName=$storage_acct_name"
az functionapp config appsettings set -g $resource_group_name -n $functionapp_name --settings "StorageAccountKey=$azure_storage_acct_key"
az functionapp config appsettings set -g $resource_group_name -n $functionapp_name --settings "StorageQueueNameReceipts=$queue_name_receipts"
az functionapp config appsettings set -g $resource_group_name -n $functionapp_name --settings "StorageQueueNameAddresses=$queue_name_addresses"
az functionapp config appsettings set -g $resource_group_name -n $functionapp_name --settings "SqlConnectionString=$azure_sql_conn_string"
az functionapp config appsettings set -g $resource_group_name -n $functionapp_name --settings "CogSvcEndpointFormRec=$cogsvc_form_recognizer_endpoint"
az functionapp config appsettings set -g $resource_group_name -n $functionapp_name --settings "CogSvcApiKeyFormRec=$cogsvc_form_recognizer_key"
az functionapp config appsettings set -g $resource_group_name -n $functionapp_name --settings "AzureMapsApiEndpoint=$azure_maps_api_endpoint"
az functionapp config appsettings set -g $resource_group_name -n $functionapp_name --settings "AzureMapsApiVersion=$azure_maps_api_version"
az functionapp config appsettings set -g $resource_group_name -n $functionapp_name --settings "AzureMapsApiKey=$azure_maps_api_key"

