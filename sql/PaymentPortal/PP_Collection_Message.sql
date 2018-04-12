IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'PP_Collection_Message')
BEGIN
	DROP PROCEDURE PP_Collection_Message
END
GO
CREATE PROCEDURE PP_Collection_Message

@ContIndex int,
@Invoices nvarchar(max),
@Provider nvarchar(max)

AS

DECLARE @debtalphas table ([output] nvarchar(max));

INSERT INTO @debtalphas
SELECT [output]
FROM [dbo].[StringSplit](@Invoices, ',')

UPDATE C 
SET C.TranStatus = 4, C.TranComments = 'Collected by ' + @Provider
FROM tblTranDebtor_Collection C 
INNER JOIN tblTranDebtor D ON C.DebtTranIndex = D.DebtTranIndex
INNER JOIN @debtalphas A ON D.DebtTranRefAlpha = A.[output]
WHERE D.ContIndex = @ContIndex AND C.TranStatus <> 4

-- insert to tblTranDebtor_Collection  'Collected by @Provider'