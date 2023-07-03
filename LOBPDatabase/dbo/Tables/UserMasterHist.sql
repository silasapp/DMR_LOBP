CREATE TABLE [dbo].[UserMasterHist] (
    [UserMasterHistId] INT           IDENTITY (1, 1) NOT NULL,
    [UserId]           VARCHAR (150) NULL,
    [FullName]         VARCHAR (150) NULL,
    [UserLocation]     VARCHAR (3)   NULL,
    [UserRoles]        VARCHAR (150) NULL,
    [MaintenanceDate]  DATETIME      NULL,
    [MaintainedBy]     VARCHAR (150) NULL,
    [Status]           VARCHAR (10)  NULL,
    [LastComment]      VARCHAR (350) NULL,
    PRIMARY KEY CLUSTERED ([UserMasterHistId] ASC)
);

