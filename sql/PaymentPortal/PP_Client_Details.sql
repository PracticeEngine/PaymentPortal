IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'PP_Client_Details')
BEGIN
	DROP PROCEDURE PP_Client_Details
END
GO
CREATE PROCEDURE PP_Client_Details

@ContIndex int

AS

SELECT E.ContIndex, E.ClientCode, E.ClientName, C.ContAddress, C.ContEmail, C.ContPhone, C.ContFax, C.ContPostCode, C.ContTownCity, C.ContCounty, E.ClientGovCode,
COALESCE(CP.ContName, C.ContName) AS PrimName,
COALESCE(CP.ContPhone, C.ContPhone) AS PrimPhone,
COALESCE(CP.ContFax, C.ContFax) AS PrimFax,
COALESCE(CP.ContMobile, C.ContMobile) AS PrimMobile,
COALESCE(CP.ContEmail, C.ContEmail) AS PrimEmail

FROM tblEngagement E 
INNER JOIN tblContacts C ON E.ClientRef = C.ContIndex
LEFT OUTER JOIN tblContacts CP ON E.ClientPartner = CP.ContIndex
WHERE E.ContIndex = @ContIndex
