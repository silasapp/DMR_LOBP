CREATE TABLE [dbo].[FieldLocation] (
    [FieldLocationID] VARCHAR (3)   NOT NULL,
    [Description]     VARCHAR (MAX) NULL,
    [Address]         VARCHAR (MAX) NULL,
    [FieldType]       VARCHAR (3)   NULL,
    [StateLocated]    VARCHAR (80)  NULL,
    [AddedDate]       DATETIME      NULL,
    [Status]          VARCHAR (10)  NULL,
    PRIMARY KEY CLUSTERED ([FieldLocationID] ASC)
);

