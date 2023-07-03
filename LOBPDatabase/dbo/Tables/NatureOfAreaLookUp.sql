CREATE TABLE [dbo].[NatureOfAreaLookUp] (
    [AreaCode] NVARCHAR (10)  NOT NULL,
    [AreaName] NVARCHAR (200) NOT NULL,
    [Status]   NVARCHAR (10)  DEFAULT ('ACTIVE') NOT NULL,
    PRIMARY KEY CLUSTERED ([AreaCode] ASC)
);

