CREATE TABLE [dbo].[PaymentLog] (
    [PaymentId]          BIGINT          IDENTITY (1, 1) NOT NULL,
    [ApplicationId]      VARCHAR (30)    NOT NULL,
    [ApplicantUserId]    VARCHAR (150)   NOT NULL,
    [LicenseTypeId]      VARCHAR (10)    NULL,
    [TransactionDate]    DATETIME        NULL,
    [TransactionID]      VARCHAR (30)    NULL,
    [Description]        VARCHAR (150)   NULL,
    [RRReference]        VARCHAR (30)    NULL,
    [AppReceiptID]       VARCHAR (30)    NULL,
    [TxnAmount]          DECIMAL (20, 2) NULL,
    [Arrears]            DECIMAL (20, 2) NOT NULL,
    [BankCode]           VARCHAR (5)     NULL,
    [Account]            VARCHAR (20)    NULL,
    [TxnMessage]         VARCHAR (150)   NULL,
    [Status]             VARCHAR (10)    NULL,
    [RetryCount]         INT             NULL,
    [LastRetryDate]      DATETIME        NULL,
    [LateRenewalPenalty] DECIMAL (20, 2) NULL,
    [NonRenewalPenalty]  DECIMAL (20, 2) NULL,
    [TotalAmount]        DECIMAL (20, 2) NULL,
    [AmountTag]          VARCHAR (10)    NULL,
    [PaymentType]        VARCHAR (10)    NULL,
    [CreatedBy]          VARCHAR (100)   NULL,
    [CreatedOn]          DATETIME        NULL,
    [ApprovedBy]         VARCHAR (100)   NULL,
    [ApprovedOn]         DATETIME        NULL,
    [StatutoryFee]       DECIMAL (20, 2) NULL,
    [ProcessingFee]      DECIMAL (20, 2) NULL,
    [ServiceCharge]      DECIMAL (20, 2) NULL,
    CONSTRAINT [PK__PaymentL__9B556A38C445FC14] PRIMARY KEY CLUSTERED ([PaymentId] ASC),
    CONSTRAINT [FK_PaymentLog_ApplicationRequest] FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[ApplicationRequest] ([ApplicationId])
);


GO
ALTER TABLE [dbo].[PaymentLog] NOCHECK CONSTRAINT [FK_PaymentLog_ApplicationRequest];

