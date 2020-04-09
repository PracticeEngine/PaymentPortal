CREATE TABLE [dbo].[PP_Bank_File_Log]
(
	[LogId] INT NOT NULL PRIMARY KEY IDENTITY,
	[BankFile] varchar(MAX) NOT NULL,
	[ErrorMessage] varchar(MAX) NULL,
	[Processed] bit NOT NULL,
	[TimeStamp] DATETIME NOT NULL DEFAULT GETDATE()
)