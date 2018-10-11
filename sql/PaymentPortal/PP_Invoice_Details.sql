IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'PP_Invoice_Details')
BEGIN
	DROP PROCEDURE PP_Invoice_Details
END
GO
CREATE PROCEDURE PP_Invoice_Details

@ContIndex int

AS 

SELECT D.DebtTranIndex, D.DebtTranDate, D.DebtTranRefAlpha, D.ClientCode, D.DebtTranName, D.DebtForTotal, D.DebtForUnpaid, D.DebtTranCur
FROM tblTranDebtor D
WHERE D.ContIndex = @ContIndex AND D.DebtForUnpaid <> 0 AND D.DebtTranType IN (3,4)
UNION ALL
SELECT D.DebtTranIndex, D.DebtTranDate, 'FC' + LTRIM(RTRIM(STR(DebtTranIndex))), D.ClientCode, D.DebtTranName, D.DebtForTotal, D.DebtForUnpaid, D.DebtTranCur
FROM tblTranDebtor D
WHERE D.ContIndex = @ContIndex AND D.DebtForUnpaid <> 0 AND D.DebtTranType IN (10)
ORDER BY DebtTranDate ASC
