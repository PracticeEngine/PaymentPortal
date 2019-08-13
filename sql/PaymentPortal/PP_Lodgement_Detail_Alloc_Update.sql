CREATE PROCEDURE [dbo].[PP_Lodgement_Detail_Alloc_Update]

@LodgeDetIdx int,
@Alloc nvarchar(MAX)

AS

DECLARE @Idx int
DECLARE @Val money
DECLARE @ValStr nvarchar(20)
DECLARE @Start int
DECLARE @End int
DECLARE @Len int
DECLARE @Total int
DECLARE @Para nvarchar(MAX)
DECLARE @Staff nvarchar(256)
DECLARE @Desc nvarchar(255)

	CREATE TABLE #SEL (DRSIndex int, AllocAmt money)

	SET @Start = 1
	SET @Total = LEN(@Alloc)
	IF @Total < 8000
		SET @End = @Total
	ELSE
		SET @End = 8000

	SET @Para = Substring(@Alloc,@Start, @End)
	SET @Len = 0

	WHILE @Len < @Total
		BEGIN
		WHILE Substring(@Para, @End, 1) <> '|'
			BEGIN
			SET @End = @End - 1
			SET @Para = Substring(@Alloc,@Start, @End)
			END
	
		WHILE Len(@Para) > 0
			BEGIN
				SET @Idx = SubString(@Para,1,CharIndex('|',@Para)-1)
				SET @Para = Right(@Para,Len(@Para)-Len(@Idx)-1)
				SET @ValStr = SubString(@Para,1,CharIndex('|',@Para)-1)
				SET @Para = Right(@Para,Len(@Para)-Len(@ValStr)-1)
				SET @Val = @ValStr
				INSERT INTO #SEL(DRSIndex, AllocAmt)
				VALUES (@Idx, @Val)
			END
		SET @Len = @Len + @End
		SET @Start = @Start + @End
		SET @End = 8000
		SET @Para = Substring(@Alloc,@Start, @End)
		END

	UPDATE A
	SET A.AllocAmount = S.AllocAmt
	FROM tblLodgementAllocSum A
	INNER JOIN #SEL S ON A.DebtTranIndex = S.DRSIndex
	WHERE A.LodgeDetIndex = @LodgeDetIdx 
	
	INSERT INTO tblLodgementAllocSum(LodgeDetIndex, DebtTranIndex, DebtTranTotal, DebtTranTotalOS, AllocAmount, DRSSelect)
	SELECT @LodgeDetIdx, D.DebtTranIndex, D.DebtForTotal, D.DebtForUnpaid, S.AllocAmt, 1
	FROM #SEL S
	INNER JOIN tblTranDebtor D ON S.DRSIndex = D.DebtTranIndex
	LEFT OUTER JOIN tblLodgementAllocSum A ON S.DRSIndex = A.DebtTranIndex AND A.LodgeDetIndex = @LodgeDetIdx
	WHERE A.LodgeDetIndex IS NULL
	
GO