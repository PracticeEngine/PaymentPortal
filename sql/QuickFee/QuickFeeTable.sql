IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND name = 'tblQuickFeeIDs')
	BEGIN
	CREATE TABLE tblQuickFeeIDs(ID int NOT NULL IDENTITY(1,1), DebtTranIndex int NOT NULL, FeeGuid uniqueidentifier NOT NULL)
	END
