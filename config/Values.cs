using System;

namespace config
{
	// TODO refactor this to use managed svc identity and Azure Key Vault to eliminate hard-coded secrets here as well as the stored-secret (bootstrap) problem
	public static class Values
	{
		public static string FaceApiUrl { get; } = "https://eastus.api.cognitive.microsoft.com/face/v1.0/detect";
		public static string FaceApiKey { get; } = "e50f33f3e988461d9f7d0760bac90d4e";

		public static string StorageAccountName { get; } = "pzcse1files";
		public static string StorageAccountKey { get; } = "Hi7ng9TDrMfWFr+fCzZrZwN97NiHSlUceFgjXgrufQ06bb4Vg1hcNQOX2PdXghRudxpHV6uPlqqIFYIM+3LC3w==";
		public static string StorageContainerName { get; } = "photos";

		public static string SqlHostName { get; } = "pzcse1sql.database.windows.net";
		public static string SqlDatabaseName { get; } = "cse1db";
		public static string SqlAppUserName { get; } = "AppUser";
		public static string SqlAppUserPwd { get; } = "CSE1@2018!";
		public static string SqlConnectionString { get; } = $"Server=tcp:{SqlHostName},1433;Initial Catalog={SqlDatabaseName};Persist Security Info=False;User ID={SqlAppUserName};Password={SqlAppUserPwd};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";
	}
}
