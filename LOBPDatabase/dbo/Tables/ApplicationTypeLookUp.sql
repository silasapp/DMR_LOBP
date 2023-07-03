CREATE TABLE [dbo].[ApplicationTypeLookUp] (
    [ApplicationTypeId] VARCHAR (10) NOT NULL,
    [Description]       VARCHAR (30) NULL,
    [Status]            VARCHAR (10) NULL,
    PRIMARY KEY CLUSTERED ([ApplicationTypeId] ASC)
);

