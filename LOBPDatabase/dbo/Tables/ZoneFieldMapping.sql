CREATE TABLE [dbo].[ZoneFieldMapping] (
    [ZoneFieldMappId] INT          IDENTITY (1, 1) NOT NULL,
    [ZoneFieldID]     VARCHAR (3)  NULL,
    [FieldLocationID] VARCHAR (3)  NULL,
    [AddedDate]       DATETIME     NULL,
    [Status]          VARCHAR (10) NULL,
    PRIMARY KEY CLUSTERED ([ZoneFieldMappId] ASC)
);

