CREATE TABLE [dbo].[TakeoverApps] (
    [TakeoverId]       INT          IDENTITY (1, 1) NOT NULL,
    [ApplicationID]    VARCHAR (30) NOT NULL,
    [LicenseReference] VARCHAR (60) NOT NULL,
    CONSTRAINT [PK_TakeoverApps] PRIMARY KEY CLUSTERED ([TakeoverId] ASC)
);

