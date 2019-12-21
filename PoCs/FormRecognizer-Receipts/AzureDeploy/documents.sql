CREATE SCHEMA [data];
GO

CREATE ROLE [DocumentsRole];
GO

CREATE USER DocumentsUser WITH PASSWORD = N'P@ssw0rd2019-', DEFAULT_SCHEMA=[data];
GO
ALTER ROLE [DocumentsRole] ADD MEMBER [DocumentsUser];
GO

GRANT EXECUTE, SELECT, UPDATE, INSERT, DELETE ON SCHEMA :: [data] TO [DocumentsRole];
GO
ALTER ROLE [db_datareader] ADD MEMBER [DocumentsRole];
ALTER ROLE [db_datawriter] ADD MEMBER [DocumentsRole];
GO

CREATE TABLE data.Documents
(
	DocumentGuid [uniqueidentifier] not null,
	DocumentType [nvarchar](50) null,
	ImageUrl [nvarchar](1000) null,
	DocumentJson [nvarchar](max) null,
	DateCreated [datetime2] null,
	DateUpdated [datetime2] null
);
GO

ALTER TABLE data.Documents ADD CONSTRAINT [DF_data_Documents_DocumentGuid] DEFAULT (newid()) FOR [DocumentGuid];
ALTER TABLE data.Documents ADD CONSTRAINT [DF_data_Documents_DateCreated] DEFAULT (getutcdate()) FOR [DateCreated];
ALTER TABLE data.Documents ADD CONSTRAINT [DF_data_Documents_DateUpdated] DEFAULT (getutcdate()) FOR [DateUpdated];
GO

CREATE TABLE data.Addresses
(
	AddressGuid [uniqueidentifier] not null,
	DocumentGuid [uniqueidentifier] not null,
	AddressJson [nvarchar](max) null,
	DateCreated [datetime2] null,
	DateUpdated [datetime2] null
);
GO

ALTER TABLE data.Addresses ADD CONSTRAINT [DF_data_Addresses_AddressGuid] DEFAULT (newid()) FOR [AddressGuid];
ALTER TABLE data.Addresses ADD CONSTRAINT [DF_data_Addresses_DateCreated] DEFAULT (getutcdate()) FOR [DateCreated];
ALTER TABLE data.Addresses ADD CONSTRAINT [DF_data_Addresses_DateUpdated] DEFAULT (getutcdate()) FOR [DateUpdated];
GO

CREATE VIEW data.ReceiptsView
AS
	SELECT
		DocumentGuid,
		Subtotal = ISNULL(JSON_VALUE(DocumentJson, '$.understandingResults[0].fields.Subtotal.value'), 0),
		Tax = ISNULL(JSON_VALUE(DocumentJson, '$.understandingResults[0].fields.Tax.value'), 0),
		Total = ISNULL(JSON_VALUE(DocumentJson, '$.understandingResults[0].fields.Total.value'), 0),
        MerchantName = JSON_VALUE(DocumentJson, '$.understandingResults[0].fields.MerchantName.value'),
        MerchantAddress = JSON_VALUE(DocumentJson, '$.understandingResults[0].fields.MerchantAddress.value'),
        MerchantPhoneNumber = JSON_VALUE(DocumentJson, '$.understandingResults[0].fields.MerchantPhoneNumber.value'),
        TransactionDate = JSON_VALUE(DocumentJson, '$.understandingResults[0].fields.TransactionDate.value'),
        TransactionTime = JSON_VALUE(DocumentJson, '$.understandingResults[0].fields.TransactionTime.value'),
		DocumentType,
		ImageUrl,
		DocumentJson,
		DateCreated,
		DateUpdated
	FROM
		data.Documents
    WHERE
        DocumentType = 'Receipt'
	;
GO

CREATE VIEW data.AddressesView
AS
	SELECT
		AddressGuid,
		DocumentGuid,
		RawText = ISNULL(JSON_VALUE(AddressJson, '$.summary.query'), ''),
		AddressType = ISNULL(JSON_VALUE(AddressJson, '$.address.type'), ''),
		AddressScore = CONVERT(NUMERIC(18,5), ISNULL(JSON_VALUE(AddressJson, '$.address.score'), 0)),
		StreetNumber = ISNULL(JSON_VALUE(AddressJson, '$.address.address.streetNumber'), ''),
		StreetName = ISNULL(JSON_VALUE(AddressJson, '$.address.address.streetName'), ''),
		Municipality = ISNULL(JSON_VALUE(AddressJson, '$.address.address.municipality'), ''),
		CountrySubdivision = ISNULL(JSON_VALUE(AddressJson, '$.address.address.countrySubdivision'), ''),
		CountrySubdivisionName = ISNULL(JSON_VALUE(AddressJson, '$.address.address.countrySubdivisionName'), ''),
		PostalCode = ISNULL(JSON_VALUE(AddressJson, '$.address.address.postalCode'), ''),
		CountryCode = ISNULL(JSON_VALUE(AddressJson, '$.address.address.countryCode'), ''),
		Country = ISNULL(JSON_VALUE(AddressJson, '$.address.address.country'), ''),
		CountryCodeISO3 = ISNULL(JSON_VALUE(AddressJson, '$.address.address.countryCodeISO3'), ''),
		FreeFormAddress = ISNULL(JSON_VALUE(AddressJson, '$.address.address.freeformAddress'), ''),
		Latitude = CONVERT(NUMERIC(18,5), ISNULL(JSON_VALUE(AddressJson, '$.address.position.lat'), 0)),
		Longitude = CONVERT(NUMERIC(18,5), ISNULL(JSON_VALUE(AddressJson, '$.address.position.lon'), 0)),
		AddressJson,
		DateCreated,
		DateUpdated
	FROM
		data.Addresses
	;
GO





CREATE PROC data.CreateDocument
	@DocumentType nvarchar(50) = null,
	@ImageUrl nvarchar(1000) = null,
	@DocumentJson nvarchar(max) = null,
	@DocumentGuid uniqueidentifier = null output
AS
BEGIN
	IF	@DocumentGuid is NULL
		SELECT @DocumentGuid = newid();

	INSERT INTO	data.Documents
	(
		DocumentGuid,
		DocumentType,
		ImageUrl,
		DocumentJson
	)
	VALUES
	(
		@DocumentGuid,
		@DocumentType,
		@ImageUrl,
		@DocumentJson
	);
END
GO

CREATE PROC data.GetReceipts
AS
BEGIN
	SELECT
		DocumentGuid,
		Subtotal = ISNULL(JSON_VALUE(DocumentJson, '$.understandingResults[0].fields.Subtotal.value'), 0),
		Tax = ISNULL(JSON_VALUE(DocumentJson, '$.understandingResults[0].fields.Tax.value'), 0),
		Total = ISNULL(JSON_VALUE(DocumentJson, '$.understandingResults[0].fields.Total.value'), 0),
        MerchantName = JSON_VALUE(DocumentJson, '$.understandingResults[0].fields.MerchantName.value'),
        MerchantAddress = JSON_VALUE(DocumentJson, '$.understandingResults[0].fields.MerchantAddress.value'),
        MerchantPhoneNumber = JSON_VALUE(DocumentJson, '$.understandingResults[0].fields.MerchantPhoneNumber.value'),
        TransactionDate = JSON_VALUE(DocumentJson, '$.understandingResults[0].fields.TransactionDate.value'),
        TransactionTime = JSON_VALUE(DocumentJson, '$.understandingResults[0].fields.TransactionTime.value'),
		DocumentType,
		ImageUrl,
		DocumentJson,
		DateCreated,
		DateUpdated
	FROM
		data.Documents
	WHERE
		DocumentType = 'Receipt'
	;
END
GO

CREATE PROC data.GetDocuments
AS
BEGIN
	SELECT
		DocumentGuid,
		DocumentType,
		ImageUrl,
		DocumentJson,
		DateCreated,
		DateUpdated
	FROM
		data.Documents
	;
END
GO

CREATE PROC data.CreateAddress
	@DocumentGuid uniqueidentifier = null,
	@AddressJson nvarchar(max) = null,
	@AddressGuid uniqueidentifier = null output
AS
BEGIN
	IF	@AddressGuid is NULL
		SELECT @AddressGuid = newid();

	INSERT INTO	data.Addresses
	(
		AddressGuid,
		DocumentGuid,
		AddressJson
	)
	VALUES
	(
		@AddressGuid,
		@DocumentGuid,
		@AddressJson
	);
END
GO

CREATE PROC data.GetAddresses
	@DocumentGuid uniqueidentifier = NULL
AS
BEGIN
	SELECT
		AddressGuid,
		DocumentGuid,
		RawText = ISNULL(JSON_VALUE(AddressJson, '$.summary.query'), ''),
		AddressType = ISNULL(JSON_VALUE(AddressJson, '$.address.type'), ''),
		AddressScore = CONVERT(NUMERIC(18,5), ISNULL(JSON_VALUE(AddressJson, '$.address.score'), 0)),
		StreetNumber = ISNULL(JSON_VALUE(AddressJson, '$.address.address.streetNumber'), ''),
		StreetName = ISNULL(JSON_VALUE(AddressJson, '$.address.address.streetName'), ''),
		Municipality = ISNULL(JSON_VALUE(AddressJson, '$.address.address.municipality'), ''),
		CountrySubdivision = ISNULL(JSON_VALUE(AddressJson, '$.address.address.countrySubdivision'), ''),
		CountrySubdivisionName = ISNULL(JSON_VALUE(AddressJson, '$.address.address.countrySubdivisionName'), ''),
		PostalCode = ISNULL(JSON_VALUE(AddressJson, '$.address.address.postalCode'), ''),
		CountryCode = ISNULL(JSON_VALUE(AddressJson, '$.address.address.countryCode'), ''),
		Country = ISNULL(JSON_VALUE(AddressJson, '$.address.address.country'), ''),
		CountryCodeISO3 = ISNULL(JSON_VALUE(AddressJson, '$.address.address.countryCodeISO3'), ''),
		FreeFormAddress = ISNULL(JSON_VALUE(AddressJson, '$.address.address.freeformAddress'), ''),
		Latitude = CONVERT(NUMERIC(18,5), ISNULL(JSON_VALUE(AddressJson, '$.address.position.lat'), 0)),
		Longitude = CONVERT(NUMERIC(18,5), ISNULL(JSON_VALUE(AddressJson, '$.address.position.lon'), 0)),
		AddressJson,
		DateCreated,
		DateUpdated
	FROM
		data.Addresses
	WHERE
		@DocumentGuid IS NULL
		OR
		DocumentGuid = @DocumentGuid
	ORDER BY
		DocumentGuid,
		AddressScore DESC
	;
END
GO
