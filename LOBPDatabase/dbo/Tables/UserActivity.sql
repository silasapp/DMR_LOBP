CREATE TABLE [dbo].[UserActivity] (
    [ActivityId]     INT           IDENTITY (1, 1) NOT NULL,
    [LicenseTypeId]  VARCHAR (10)  NULL,
    [UserId]         VARCHAR (150) NULL,
    [TxnCount]       INT           NULL,
    [ValueDate]      DATE          NULL,
    [CurrentStageID] SMALLINT      NULL,
    CONSTRAINT [PK__UserActi__45F4A7911FE31860] PRIMARY KEY CLUSTERED ([ActivityId] ASC)
);

