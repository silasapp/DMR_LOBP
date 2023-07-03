CREATE TABLE [dbo].[LicenseType] (
    [LicenseTypeId]  VARCHAR (10)    NOT NULL,
    [Description]    VARCHAR (150)   NULL,
    [Tenor]          INT             NULL,
    [SequencNo]      TINYINT         NULL,
    [LicenseSerial]  INT             NULL,
    [ProcessingFees] DECIMAL (18)    NULL,
    [GracePeriod]    INT             NULL,
    [Status]         VARCHAR (10)    NULL,
    [Duration]       VARCHAR (50)    NULL,
    [Penalty]        DECIMAL (20, 2) NULL,
    [ShortName]      VARCHAR (50)    NULL,
    CONSTRAINT [PK__LicenseT__48F794F86F3894E5] PRIMARY KEY CLUSTERED ([LicenseTypeId] ASC)
);

