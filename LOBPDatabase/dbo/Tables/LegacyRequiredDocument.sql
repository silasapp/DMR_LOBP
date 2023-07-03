CREATE TABLE [dbo].[LegacyRequiredDocument] (
    [LicenseApply] VARCHAR (5)   NOT NULL,
    [TypeID]       INT           NOT NULL,
    [Description]  VARCHAR (150) NULL,
    [SeqNo]        TINYINT       NULL,
    [Status]       VARCHAR (10)  NULL,
    PRIMARY KEY CLUSTERED ([LicenseApply] ASC, [TypeID] ASC)
);

