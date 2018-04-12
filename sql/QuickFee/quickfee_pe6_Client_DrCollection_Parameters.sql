IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'quickfee_pe6_Client_DrCollection_Parameters')
BEGIN
	DROP PROCEDURE quickfee_pe6_Client_DrCollection_Parameters
END
GO
CREATE PROCEDURE [dbo].[quickfee_pe6_Client_DrCollection_Parameters]

AS

	select		CreditCode, Title
	from		tblCreditCodes
	where		ClientInvGrp = 1
	order by	CodeOrder

	select 0 as TranStatus, 'Outstanding' as TranStatusDesc
	union
	select 1 as TranStatus, 'In Dispute' as TranStatusDesc
	union
	select 2 as TranStatus, 'Provided For' as TranStatusDesc
	union
	select 3 as TranStatus, 'Bad Debt' as TranStatusDesc
	union
	select 4 as TranStatus, 'Quick Fee' as TranStatusDesc
	order by TranStatus	

	return @@ROWCOUNT
