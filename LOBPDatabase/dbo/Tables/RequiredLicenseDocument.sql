CREATE TABLE [dbo].[RequiredLicenseDocument] (
    [LicenseDocId]      INT           IDENTITY (1, 1) NOT NULL,
    [LicenseTypeId]     VARCHAR (10)  NOT NULL,
    [ApplicationTypeId] VARCHAR (10)  NOT NULL,
    [TypeID]            INT           NOT NULL,
    [Description]       VARCHAR (MAX) NULL,
    [SeqNo]             TINYINT       NULL,
    [IsBaseTran]        VARCHAR (1)   NULL,
    [IsMandatory]       VARCHAR (1)   NULL,
    [IsLicenseDoc]      VARCHAR (1)   NULL,
    [Status]            VARCHAR (10)  NULL,
    CONSTRAINT [PK__Required__6E24EEE898A5B832] PRIMARY KEY CLUSTERED ([LicenseDocId] ASC)
);

