CREATE PROCEDURE PP_Upsert_Receipt_Processor_Log_Entry

@LogId int = NULL,
@BankFileLogId int = NULL,
@Record nvarchar(MAX) = NULL,
@ErrorMessage nvarchar(MAX) = NULL,
@Processed bit,
@NewLogId int OUTPUT

AS

IF @LogId IS NULL
	BEGIN
	INSERT INTO PP_Receipt_Processor_Log (BankFileLogId, Record, Processed)
	VALUES(@BankFileLogId, @Record, @Processed)

	SET @NewLogId = SCOPE_IDENTITY()
	END
ELSE
	BEGIN
	UPDATE PP_Receipt_Processor_Log SET ErrorMessage = @ErrorMessage, Processed = @Processed WHERE LogId = @LogId

	SET @NewLogId = @LogId
	END