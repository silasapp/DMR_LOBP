CREATE TABLE [dbo].[ActionHistory] (
    [ActionId]             BIGINT        IDENTITY (1, 1) NOT NULL,
    [LicenseTypeId]        VARCHAR (10)  NULL,
    [CurrentFieldLocation] VARCHAR (3)   NULL,
    [ApplicationId]        VARCHAR (30)  NULL,
    [CurrentStageID]       SMALLINT      NULL,
    [Action]               VARCHAR (30)  NULL,
    [ActionDate]           DATETIME      NULL,
    [TriggeredBy]          VARCHAR (50)  NULL,
    [TriggeredByRole]      VARCHAR (150) NULL,
    [MESSAGE]              TEXT          NULL,
    [TargetedTo]           VARCHAR (50)  NULL,
    [TargetedToRole]       VARCHAR (150) NULL,
    [NextStateID]          SMALLINT      NULL,
    [NextFieldLocation]    VARCHAR (3)   NULL,
    [StatusMode]           VARCHAR (50)  NULL,
    [ActionMode]           VARCHAR (50)  NULL,
    CONSTRAINT [PK__ActionHi__FFE3F4D9E21E6B55] PRIMARY KEY CLUSTERED ([ActionId] ASC),
    CONSTRAINT [ActionHistory_ApplicationID_FK] FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[ApplicationRequest] ([ApplicationId]),
    CONSTRAINT [ActionHistory_FK_CurrentStageID] FOREIGN KEY ([CurrentStageID]) REFERENCES [dbo].[WorkFlowState] ([StateID]),
    CONSTRAINT [ActionHistory_FK_NextStateID] FOREIGN KEY ([NextStateID]) REFERENCES [dbo].[WorkFlowState] ([StateID]),
    CONSTRAINT [FK_ActLicenseTypeCode] FOREIGN KEY ([LicenseTypeId]) REFERENCES [dbo].[LicenseType] ([LicenseTypeId])
);


GO
ALTER TABLE [dbo].[ActionHistory] NOCHECK CONSTRAINT [ActionHistory_ApplicationID_FK];


GO
ALTER TABLE [dbo].[ActionHistory] NOCHECK CONSTRAINT [ActionHistory_FK_CurrentStageID];


GO
ALTER TABLE [dbo].[ActionHistory] NOCHECK CONSTRAINT [ActionHistory_FK_NextStateID];


GO
ALTER TABLE [dbo].[ActionHistory] NOCHECK CONSTRAINT [FK_ActLicenseTypeCode];

