CREATE TABLE [dbo].[PP_Receipt_Processor_Log]
(
	[LogId] INT NOT NULL PRIMARY KEY IDENTITY,
	[BankFileLogId] INT NOT NULL,
	[Instruction] varchar(15) NULL,
	[Record] varchar(MAX) NOT NULL,
	[Invoices] varchar(100) NULL,
	[PaymentReference] varchar(20) NULL,
	[Amount] money NULL,
	[PaymentDate] date NULL,
	[ErrorMessage] varchar(MAX) NULL,
	[Processed] bit NOT NULL,
	[TimeStamp] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_BankFileLogId] FOREIGN KEY ([BankFileLogId]) REFERENCES [dbo].[PP_Bank_File_Log] ([LogId])
)