IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'quickfee_pe6_Fee_Bulk_Print')
BEGIN
	DROP PROCEDURE quickfee_pe6_Fee_Bulk_Print
END
GO
CREATE PROCEDURE [dbo].[quickfee_pe6_Fee_Bulk_Print]

@Selection nvarchar(MAX)

AS

	DECLARE @QFURL nvarchar(255)
	SET @QFURL = 'https://wafffcxktcftkcfgtleandtosh.com/payments/login'

	CREATE TABLE #SEL (DebtTranIndex int)
	
	INSERT INTO #SEL(DebtTranIndex)	
	SELECT Item
	FROM dbo.SplitString(@Selection, '|')
	WHERE ISNumeric(Item) = 1

	INSERT INTO tblQuickFeeIDs(DebtTranIndex, FeeGuid)
	SELECT S.DebtTranIndex, NEWID()
	FROM #SEL S 
	LEFT JOIN tblQuickFeeIDs D ON S.DebtTranIndex = D.DebtTranIndex
	WHERE D.DebtTranIndex IS NULL

	DECLARE @DisbNarr nvarchar(255)
	DECLARE @DisbPerc decimal(7,2)
	
	SELECT @DisbNarr = TranSetDisbNarrative, @DisbPerc = TranSetDisbPercent
	FROM tblTransactionSettings
	WHERE TranSetIndex = 1
	
	IF @DisbPerc IS NULL
		SET @DisbPerc = 0
	
	IF @DisbNarr IS NULL
		SET @DisbNarr = 'Credit Surcharge at'
	
	SET @DisbNarr = @DisbNarr + ' ' + LTrim(RTrim(Str(@DisbPerc, 7, 2))) + '%'
	
	SELECT	Coalesce(C.PracID, 1) As PracID, Coalesce(C.PracName, '') As PracName, Coalesce(C.PracAddress, '') As PracAddress, Coalesce(C.PracTitle, '') As PracTitle, Coalesce(C.PracTitleExt, '') As PracTitleExt, 
			Coalesce(C.PracTel, '') As PracTel, Coalesce(C.PracFax, '') As PracFax, Coalesce(C.PracEmail, '') As PracEmail, Coalesce(C.PracVATNumber, '') As PracVATNumber, BS.Blob As PracLogo,
			Coalesce(DC.TranPrinted, 0) As TranPrinted, 
			Coalesce(E.ClientTerms, '') As ClientTerms,
			Coalesce(TT.TransTypeName, '') As TransTypeName,
			Coalesce(Par.StaffCode, '') AS PartnerCode, 
			Coalesce(Man.StaffCode, '') AS ManagerCode, 
			Coalesce(Stf.StaffCode, '') AS ConfirmedCode, 
			Coalesce(J.Job_Name, '') As Job_Name, Coalesce(J.Job_Code, '') As Job_Code,
			Coalesce(E.ClientOffice, '') As ClientOffice, Coalesce(E.ClientVATNumber, '') As ClientVATNumber,
			Coalesce(O.OfficeName, '') As OfficeName, Coalesce(O.OfficeAddress, '') As OfficeAddress, Coalesce(O.OfficePhone, '') As OfficePhone, Coalesce(O.OfficeFax, '') As OfficeFax, Coalesce(O.OfficeEMail, '') As OfficeEMail, Coalesce(@DisbNarr, 'Surcharge') As SurchDesc,
			D.DebtTranIndex, D.DebtTranType, D.ContIndex, D.ClientCode, DateName(day, D.DebtTranDate) + ' ' + DateName(month, D.DebtTranDate) + ' ' + DateName(year, D.DebtTranDate) As FeeDate, D.DebtTranDate, D.DebtTranName, D.DebtTranAddress, 
			Coalesce(D.DebtTranAttention,'') AS DebtTranAttention, D.DebtTranRefAlpha, D.DebtTranRefNum, D.DebtTranMemo, D.DebtJob,
			Case When D.DebtTranStyle IN (1, 3, 5, 7) Then 0 Else 1 End AS DebtTranStyle , D.DebtTranAmount, D.DebtTranVAT, D.DebtTranSurch, D.DebtTranTotal, D.DebtTranUnpaid, 
			DD.DebtTranIndex, DD.DebtDetSeq, DD.DebtDetType, DD.DebtDetService, DD.DebtDetAnalysis, DD.Amount, DD.VATRate, DD.VATPercent, DD.VATAmount, Coalesce(CT.ChargeFee, '') As ChargeFee, DD.FeeNarrative,
			(SELECT COUNT(*) FROM tblTranDebtorDetail DD2 WHERE DD2.DebtDetType = DD.DebtDetType AND DD2.VATRate = DD.VATRate AND DD2.DebtTranIndex = DD.DebtTranIndex) As NumLinesByVATByType,
			(SELECT COUNT(DISTINCT VATRate) FROM tblTranDebtorDetail DD2 WHERE DD2.DebtDetType = DD.DebtDetType AND DD2.DebtTranIndex = DD.DebtTranIndex) As NumLinesByType,
			(SELECT COUNT(DISTINCT DebtDetType) FROM tblTranDebtorDetail DD2 WHERE DD2.DebtTranIndex = DD.DebtTranIndex) As NumTypes,
			(SELECT COUNT(*) FROM tblTranDebtorDetail DD2 WHERE DD2.DebtTranIndex = DD.DebtTranIndex) As NumLines,
			(D.DebtTranTotal - D.DebtTranUnpaid) AS Paid, DateAdd(dd, Cast(E.ClientTerms as int), D.DebtTranDate) As DateDue, 
			CASE WHEN DateAdd(dd, Cast(E.ClientTerms as int), D.DebtTranDate) < GetDate() THEN 'y' ELSE 'n' END As IsOverDue,
			CASE WHEN (SELECT Max(VATPercent) FROM tblTranDebtorDetail DD2 WHERE DD2.DebtTranIndex = DD.DebtTranIndex) > 0 THEN 'VAT at ' + Cast((SELECT Max(VATPercent) FROM tblTranDebtorDetail DD2 WHERE DD2.DebtTranIndex = DD.DebtTranIndex) as nvarchar) + '%' ELSE 'VAT Exempt' END As VATDesc,
			Cur.CurSymbol, 1 As Final, D.DeliveryFormat, Coalesce(E.ClientCreditEMail,'') As ClientCreditEMail, Coalesce(Cred.StaffEMail,'') As CreditControllerEMail,
			C.PracEMailOptions, DC.TranFileId,
			@QFURL + CAST(QF.FeeGuid as nvarchar(255)) As QuickFeeLink,
			'https://wafffcxktcftkcfgtleandtosh.com' AS QuickFeeDisplayLink
	FROM	tblTranDebtor D INNER JOIN
		#SEL ON D.DebtTranIndex = #SEL.DebtTranIndex INNER JOIN
		tblTranDebtor_Collection DC ON D.DebtTranIndex = DC.DebtTranIndex INNER JOIN
		tblTranDebtorDetail DD ON D.DebtTranIndex = DD.DebtTranIndex INNER JOIN
		tblControl C ON D.PracID = C.PracID INNER JOIN
		tblEngagement E ON D.ContIndex = E.ContIndex INNER JOIN
		tblOffices O ON E.ClientOffice = O.OfficeCode INNER JOIN
		tblTranTypes TT ON D.DebtTranType = TT.TransTypeIndex INNER JOIN
		tblChargeTypes CT ON DD.DebtDetType = CT.ChargeType INNER JOIN
		tblStaff Par ON D.DebtTranPartner = Par.StaffIndex INNER JOIN
		tblStaff Man ON D.DebtTranManager = Man.StaffIndex INNER JOIN
		tblStaff Stf ON DC.ConfirmedBy = Stf.StaffIndex INNER JOIN
		tblStaff Cred ON E.ClientCreditController = Cred.StaffIndex INNER JOIN
		tblJob_Header J ON D.DebtJob = J.Job_Idx INNER JOIN
		tblCurrency Cur ON D.DebtTranCur = Cur.Currency LEFT JOIN
		tblBlobStorage BS ON C.PracLogoId = BS.Id INNER JOIN
		tblQuickFeeIDs QF ON D.DebtTranIndex = QF.DebtTranIndex
