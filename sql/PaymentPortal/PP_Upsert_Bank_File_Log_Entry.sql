CREATE PROCEDURE PP_Upsert_Bank_File_Log_Entry

@LogId int = NULL,
@BankFile nvarchar(MAX) = NULL,
@ErrorMessage nvarchar(MAX) = NULL,
@Processed bit,
@NewLogId int OUTPUT

AS

IF @LogId IS NULL
	BEGIN
	INSERT INTO PP_Bank_File_Log (BankFile, Processed)
	VALUES(@BankFile, @Processed)

	SET @NewLogId = SCOPE_IDENTITY()
	END
ELSE
	BEGIN
	UPDATE PP_Bank_File_Log SET ErrorMessage = @ErrorMessage, Processed = @Processed WHERE LogId = @LogId

	SET @NewLogId = @LogId
	END