IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'PP_Validate_Login')
BEGIN
	DROP PROCEDURE PP_Validate_Login
END
GO
CREATE PROCEDURE PP_Validate_Login

@Guid uniqueidentifier,
@ClientCode nvarchar(10)

AS

DECLARE @ContIndex int

IF EXISTS (SELECT TOP 1 1 FROM tblTranDebtor D INNER JOIN tblQuickFeeIDs Q ON D.DebtTranIndex = Q.DebtTranIndex WHERE D.ClientCode = @ClientCode AND Q.FeeGuid = @Guid)
	BEGIN
	SELECT @ContIndex = ContIndex 
	FROM tblTranDebtor D 
	INNER JOIN tblQuickFeeIDs Q ON D.DebtTranIndex = Q.DebtTranIndex 
	WHERE D.ClientCode = @ClientCode AND Q.FeeGuid = @Guid
	END
ELSE
	BEGIN
	SET @ContIndex = NULL
	END

SELECT @ContIndex As ValidatedClientId
