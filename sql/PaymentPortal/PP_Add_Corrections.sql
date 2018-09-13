CREATE PROCEDURE PP_Add_Corrections

@Invoices dbo.StringListType readonly,
@PaymentRef nvarchar(20),
@Amount money,
@PaymentDate date

AS

CREATE TABLE #Inv(ContIndex int, DebtTranIndex int, DebtTranRefAlpha nvarchar(15))

INSERT INTO #Inv(ContIndex, DebtTranIndex, DebtTranRefAlpha)
SELECT 0, 0, Name 
FROM @Invoices

DELETE
FROM #Inv
WHERE DebtTranRefAlpha = ''

IF EXISTS (SELECT I.DebtTranRefAlpha, COUNT(*) As NumInstances FROM #Inv I INNER JOIN tblTranDebtor D ON I.DebtTranRefAlpha = D.DebtTranRefAlpha WHERE D.DebtTranType IN (3,4) GROUP BY I.DebtTranRefAlpha HAVING COUNT(*) > 1)
	BEGIN
	PRINT 'Duplicates Found'
	END

IF EXISTS (SELECT TOP 1 1 FROM #Inv I LEFT JOIN tblTranDebtor D ON I.DebtTranRefAlpha = D.DebtTranRefAlpha AND D.DebtTranType IN (3,4) WHERE D.DebtTranIndex IS NULL)
	BEGIN
	PRINT 'Not all Refs have a match'
	END

UPDATE I 
SET I.ContIndex = D.ContIndex, I.DebtTranIndex = D.DebtTranIndex
FROM #Inv I 
INNER JOIN tblTranDebtor D ON I.DebtTranRefAlpha = D.DebtTranRefAlpha 
WHERE D.DebtTranType IN (3,4)

IF (SELECT COUNT(DISTINCT ContIndex) FROM #Inv) > 1
	BEGIN
	PRINT 'Multiple Clients Identified!'
	END

DECLARE @Bank int
DECLARE @StaffIdx int
DECLARE @Period datetime = NULL
DECLARE @Date datetime = NULL
DECLARE @BankCurrency varchar(3) = NULL
DECLARE @User varchar(255)= NULL
DECLARE @Card varchar(50) = NULL

SET @Bank = 1
SET @StaffIdx = 77

SET		@Period =		(select PeriodEndDate from tblControlPeriods where @PaymentDate between PeriodStartDate and PeriodEndDate)
IF		@Period IS NULL	BEGIN SET @Period = (Select PracPeriodEnd from tblControl where PracID = 1) END
IF		@Period <		(Select PracPeriodEnd from tblControl where PracID = 1) BEGIN SET @Period = (Select PracPeriodEnd from tblControl where PracID = 1) END
SET		@Date =			CASE WHEN GETDATE() > @Period THEN @Period ELSE DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) END
SET		@BankCurrency = (SELECT BankCurrency FROM tblTranBank WHERE BankIndex = @Bank)
SET		@User = (SELECT StaffName from tblStaff where StaffIndex = @StaffIdx)
SET		@Card = (SELECT TOP 1 Category from tblCategory where CatType = 'CARDTYPE' ORDER BY Category)
	
IF NOT EXISTS (SELECT * from tblLodgementHeader WHERE LodgeStatus = 'ACTIVE' AND PeriodEndDate = @Period AND LodgeBank = @Bank AND LodgeDepCurrency = @BankCurrency AND @PaymentRef = LodgeRef)
BEGIN
INSERT INTO tblLodgementHeader ( PeriodEndDate, LodgeDate, LodgeBank, LodgeRef, LodgeStatus, LodgeDepCurrency, LodgeDepRate, LodgeAccCurrency, LodgeAccRate, LodgeComments, LodgeUpdated, LodgeUpdatedBy )
VALUES (@Period, @Date, @Bank, @PaymentRef, 'ACTIVE', @BankCurrency,  dbo.udf_Currency_Rate(@BankCurrency, @Date), @BankCurrency , dbo.udf_Currency_Rate(@BankCurrency, @Date), '', GetDate(), @User)
END
	
DECLARE	@LdgIdx int
DECLARE	@ContIdx int
DECLARE @Payor nvarchar(255)
DECLARE	@LdgDetIdx int

SET		@LdgIdx = (SELECT TOP 1 LodgeIndex from tblLodgementHeader WHERE LodgeStatus = 'ACTIVE' AND PeriodEndDate = @Period AND LodgeBank = @Bank AND LodgeRef = @PaymentRef ORDER BY LodgeIndex DESC)
SET		@ContIdx = (SELECT TOP 1 ContIndex from #Inv)
		
INSERT INTO tblLodgementDetails
(LodgeIndex, ChequeDate, LodgeType, CreditCard, LodgeDebtor,
	ContIndex, StaffIndex, ClientCode, LodgeDebtorPosted, LodgePayor, LodgeAmount, ChequeNo,
	LodgeDepCurrency, LodgeDepRate, LodgeDepAmount, LodgeAccCurrency, LodgeAccRate, LodgeAccAmount)
SELECT @LdgIdx, COALESCE(@PaymentDate,@Period), 'DD', '', 1,
	@ContIdx, @StaffIdx, ClientCode, 0, ClientName, @Amount, @PaymentRef,
	@BankCurrency, dbo.udf_Currency_Rate(@BankCurrency, @Date), @Amount, @BankCurrency, dbo.udf_Currency_Rate(@BankCurrency, @Date), @Amount
FROM tblEngagement
WHERE ContIndex = @ContIdx
	
SET @LdgDetIdx = SCOPE_IDENTITY()

DECLARE @DebtTranIndex int
DECLARE @FeeAmount money
DECLARE @AllocAmount money
DECLARE @FeeNo nvarchar(50)

DECLARE csr_Fees CURSOR FOR
SELECT DebtTranIndex 
FROM #Inv

OPEN csr_Fees
FETCH csr_Fees INTO @DebtTranIndex

WHILE @@FETCH_STATUS = 0 AND @Amount > 0
	BEGIN
	SELECT @FeeAmount = DebtTranTotal - DebtTranUnpaid FROM tblTranDebtor D WHERE D.DebtTranIndex = @DebtTranIndex
	IF @FeeAmount > @Amount
		SET @AllocAmount = @Amount
	ELSE
		SET @AllocAmount = @FeeAmount
	SET @FeeNo = LTRIM(RTRIM(STR(@DebtTranIndex))) + '|' + LTRIM(RTRIM(STR(@AllocAmount))) + '|'

	EXEC pe6_Lodgement_Detail_Alloc_Update @LdgDetIdx, @FeeNo
	SET @Amount = @Amount - @AllocAmount

	FETCH csr_Fees INTO @DebtTranIndex
	END

CLOSE csr_Fees
DEALLOCATE csr_Fees

DROP TABLE #Inv

