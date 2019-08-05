#!/bin/bash

# Login first
# az login

# ##################################################
# Variables

# Azure
azure_subscription_id="$(az account show -o tsv --query "id")"
azure_region="eastus"  # PROVIDE
azure_external_ips_allowed=""  # PROVIDE

# General
prefix="aab"   # This is just a naming prefix used to create other variable values, like resource group names and such

# Resource group
azure_resource_group_name="$prefix""-receipts"

# Storage account and containers
azure_storage_acct_name="$prefix""receiptssa"
azure_storage_acct_sku=Standard_LRS
azure_container_name_receipts_in="receipts-in"
azure_container_name_receipts_out="receipts-out"
azure_container_name_assets="assets"

# Storage access policy
azure_storage_access_policy_name="receipts-in"

# App service plan
app_service_plan_name="$prefix""asp"
app_service_plan_sku="S1"

app_insights_name="$prefix""ai"
app_insights_key="DO NOT SET EXPLICITLY"

# Function app name
functionapp_name="$prefix""fn"

# Azure SQL
azure_sql_server_admin_sql_username="$prefix""-sqladmin"  # PROVIDE
azure_sql_server_admin_sql_password=""  # PROVIDE

azure_template_path_sql_server="azuresqlserver.template.json"
azure_sql_server_name="$prefix""-sql-""$azure_region"

azure_template_path_sql_database="azuresqldb.template.json"

azure_sql_db_name="receipts"
azure_sql_db_sku="S1"
azure_sql_db_tier="Standard"
azure_sql_db_max_size_bytes=268435456000
azure_sql_db_bacpac_file="receipts.bacpac"
azure_sql_db_bacpac_path="./$azure_sql_db_bacpac_file"
azure_sql_db_bacpac_storage_uri="https://""$storage_acct_name"".blob.core.windows.net/""$azure_container_name_assets""/""$azure_sql_db_bacpac_file"

azure_sql_security_role_name="ReceiptsRole"  # This MUST match what's in the bacpac/database! Do not change this until/unless you know exactly what you're doing and have changed it in those other places!!!

# Cognitive Services
cogsvc_computer_vision_name="$prefix""-cs-compvis"

# ##################################################

# ##################################################
# https://docs.microsoft.com/en-us/cli/azure/group
echo "Create resource group"
az group create -l "$azure_region" -n "$azure_resource_group_name" --verbose

# https://docs.microsoft.com/en-us/cli/azure/storage/account
echo "Create storage account"
az storage account create -l "$azure_region" -g "$azure_resource_group_name" -n "$azure_storage_acct_name" --sku "$azure_storage_acct_sku" --kind StorageV2 --verbose

echo "Get storage account key"
azure_storage_acct_key="$(az storage account keys list -g "$azure_resource_group_name" -n "$azure_storage_acct_name" -o tsv --query "[0].value")"

# https://docs.microsoft.com/en-us/cli/azure/storage/container
echo "Create storage containers"
az storage container create --account-name "$azure_storage_acct_name" --account-key "$azure_storage_acct_key" -n "$azure_container_name_receipts_in" --verbose
az storage container create --account-name "$azure_storage_acct_name" --account-key "$azure_storage_acct_key" -n "$azure_container_name_receipts_out" --verbose
az storage container create --account-name "$azure_storage_acct_name" --account-key "$azure_storage_acct_key" -n "$azure_container_name_assets" --verbose

echo "Create shared access policy on receipts in container"
az storage container policy create -c "$azure_container_name_receipts_in" -n "$azure_storage_access_policy_name" --account-name "$azure_storage_acct_name" --account-key "$azure_storage_acct_key" --permissions r --start "2019-7-1T00:00:00Z" --expiry "2022-1-1T00:00:00Z"

# https://docs.microsoft.com/en-us/cli/azure/appservice/plan
# Create app service plan
echo "Create App Service Plan"
az appservice plan create -l $azure_region -g $azure_resource_group_name -n $app_service_plan_name --sku $app_service_plan_sku

# https://docs.microsoft.com/en-us/cli/azure/group/deployment
# Create application insights instance and get instrumentation key
echo "Create Application Insights and get Instrumentation Key"
app_insights_key="$(az group deployment create -g $azure_resource_group_name -n $app_insights_name --template-file "app_insights.template.json" \
	-o tsv --query "properties.outputs.app_insights_instrumentation_key.value" \
	--parameters location="$azure_region" instance_name="$app_insights_name")"

# https://docs.microsoft.com/en-us/cli/azure/functionapp
# Create function app with plan and app insights created above
# Using Windows at this point because MSI on Linux still in preview
echo "Create Function App and link to App Service Plan and App Insights instance created above"
az functionapp create -g $azure_resource_group_name -n $functionapp_name --storage-account $azure_storage_acct_name \
	--app-insights $app_insights_name --app-insights-key $app_insights_key \
	--plan $app_service_plan_name --os-type Windows --runtime dotnet

# echo "Upload bacpac file to import into Azure SQL database"
# az storage blob upload --account-name "$storage_acct_name" --account-key "$azure_storage_acct_key" -c "$storage_container_assets" -n "$azure_sql_db_bacpac_file" -f "$azure_sql_db_bacpac_path"

echo "Create Azure SQL virtual server"
az group deployment create -g "$azure_resource_group_name" -n "$azure_sql_server_name" --template-file "$azure_template_path_sql_server" --parameters \
	location="$azure_region" server_name="$azure_sql_server_name" server_admin_username="$azure_sql_server_admin_sql_username" server_admin_password="$azure_sql_server_admin_sql_password" \
	alerts_email_address="$azure_alerts_email_address" audit_storage_account_name="$azure_storage_acct_name" audit_storage_account_key="$azure_storage_acct_key" \
	firewall_rule_start_ip="$azure_external_ips_allowed" firewall_rule_end_ip="$azure_external_ips_allowed"

echo "Deploy database (read scale-out and zone redundancy only available for Azure SQL DB Premium)"
az group deployment create -g "$azure_resource_group_name" -n "$azure_sql_db_name" --template-file "$azure_template_path_sql_database" --parameters \
	location="$azure_region" server_name="$azure_sql_server_name" db_name="$azure_sql_db_name" \
	db_sku="$azure_sql_db_sku" db_tier="$azure_sql_db_tier" db_max_size_bytes="$azure_sql_db_max_size_bytes" \
	db_read_scale="Disabled" db_zone_redundant=false audit_storage_account_name="$azure_storage_acct_name" audit_storage_account_key="$azure_storage_acct_key"

# echo "Restore bacpac to Azure SQL ref database"
# az sql db import -g "$azure_resource_group_name" -s "$azure_sql_server_name" -n "$azure_sql_db_name" \
#     -u "$azure_sql_server_admin_sql_username" -p "$azure_sql_server_admin_sql_password" \
#     --storage-uri "$azure_sql_db_bacpac_storage_uri" --storage-key "$azure_storage_acct_key" --storage-key-type "StorageAccessKey"

echo "Deploy computer vision cognitive service"
az cognitiveservices account create -l $azure_region -g $azure_resource_group_name -n $cogsvc_computer_vision_name --kind "ComputerVision" --sku "S1" --yes --verbose

echo "Show cognitive services accounts"
az cognitiveservices account show -g $azure_resource_group_name -n $cogsvc_computer_vision_name

echo "List cognitive services keys: computer vision"
az cognitiveservices account keys list -g $azure_resource_group_name -n $cogsvc_computer_vision_name


# Azure Function Code
# Azure Function event grid subscription for storage events
