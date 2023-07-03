CREATE TABLE [dbo].[LegacyDocument] (
    [LegacyDocId]     INT           IDENTITY (1, 1) NOT NULL,
    [LicenseTypeCode] VARCHAR (20)  NOT NULL,
    [ApplicationType] VARCHAR (10)  NOT NULL,
    [TypeID]          INT           NOT NULL,
    [Description]     VARCHAR (MAX) NULL,
    [SeqNo]           TINYINT       NULL,
    [IsBaseTran]      VARCHAR (1)   NULL,
    [IsMandatory]     VARCHAR (1)   NULL,
    [Status]          VARCHAR (10)  NULL,
    CONSTRAINT [PK_LegacyDocument] PRIMARY KEY CLUSTERED ([LegacyDocId] ASC)
);

