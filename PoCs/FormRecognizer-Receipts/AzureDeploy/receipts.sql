CREATE SCHEMA [data];
GO

CREATE ROLE [ReceiptsRole];
GO

CREATE USER ReceiptsUser1 WITH PASSWORD = N'P@ssw0rd2019!', DEFAULT_SCHEMA=[data];
GO
ALTER ROLE [ReceiptsRole] ADD MEMBER [ReceiptsUser1];
GO

GRANT EXECUTE, SELECT, UPDATE, INSERT, DELETE ON SCHEMA :: [data] TO [ReceiptsRole];
GO
ALTER ROLE [db_datareader] ADD MEMBER [ReceiptsRole];
ALTER ROLE [db_datawriter] ADD MEMBER [ReceiptsRole];
GO

CREATE TABLE data.Receipts
(
	ReceiptGuid [uniqueidentifier] not null,
	ImageUrl [nvarchar](1000) null,
	JsonCustomVision [nvarchar](max) null,
	JsonFormsRecognizer [nvarchar](max) null,
	DateCreated [datetime2] null,
	DateUpdated [datetime2] null
);
GO

ALTER TABLE data.Receipts ADD CONSTRAINT [DF_data_Receipts_ReceiptGuid] DEFAULT (newsequentialid()) FOR [ReceiptGuid];
ALTER TABLE data.Receipts ADD CONSTRAINT [DF_data_Receipts_DateCreated] DEFAULT (getutcdate()) FOR [DateCreated];
ALTER TABLE data.Receipts ADD CONSTRAINT [DF_data_Receipts_DateUpdated] DEFAULT (getutcdate()) FOR [DateUpdated];
GO

CREATE PROC data.SaveReceipt
	@ReceiptGuid uniqueidentifier = null,
	@ImageUrl nvarchar(1000) = null,
	@JsonCustomVision nvarchar(max) = null,
	@JsonFormsRecognizer nvarchar(max) = null
AS
BEGIN

	-- IF	@JsonCustomVision IS NOT null
	-- BEGIN
	-- 	SELECT
	-- 		@value1 = JSON_VALUE(@JsonCustomVision, '$.something.value1')
	-- 	;
	-- END

	-- IF	@JsonFormsRecognizer IS NOT null
	-- BEGIN
	-- 	SELECT
	-- 		@value2 = JSON_VALUE(@JsonCustomVision, '$.something.value2')
	-- 	;
	-- END


	IF	@ReceiptGuid IS null
		BEGIN
				INSERT INTO	data.Receipts
				(
					ImageUrl,
					JsonCustomVision,
					JsonFormsRecognizer
				)
				VALUES
				(
					@ImageUrl,
					@JsonCustomVision,
					@JsonFormsRecognizer
				);
		END
	ELSE
		BEGIN
			UPDATE	data.Receipts
			SET		ImageUrl = @ImageUrl,
					JsonCustomVision = @JsonCustomVision,
					JsonFormsRecognizer = @JsonFormsRecognizer,
					DateUpdated = getutcdate()
			WHERE	ReceiptGuid = @ReceiptGuid;
		END
END
GO