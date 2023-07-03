CREATE TABLE [dbo].[UserActivityHist] (
    [ActivityHistId] INT           IDENTITY (1, 1) NOT NULL,
    [ActivityId]     INT           NULL,
    [UserId]         VARCHAR (150) NULL,
    [ApplicationId]  VARCHAR (30)  NULL,
    [LicenseTypeId]  VARCHAR (10)  NULL,
    [ActivityOn]     DATETIME      NULL,
    [CurrentStageID] SMALLINT      NULL,
    CONSTRAINT [PK__UserActi__C5612E781498C1EF] PRIMARY KEY CLUSTERED ([ActivityHistId] ASC)
);

