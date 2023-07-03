CREATE TABLE [dbo].[RequestFieldMapping] (
    [RequestFieldMappId] INT          IDENTITY (1, 1) NOT NULL,
    [StateCode]          NVARCHAR (3) NOT NULL,
    [ApplicationType]    VARCHAR (10) NOT NULL,
    [FieldLocationID]    VARCHAR (3)  NOT NULL,
    [AddedDate]          DATETIME     NULL,
    [Status]             VARCHAR (10) NULL,
    CONSTRAINT [PK__RequestF__C4BCA9A71A7CC54F] PRIMARY KEY CLUSTERED ([RequestFieldMappId] ASC),
    CONSTRAINT [RequestFieldMapping_StateCode_FK] FOREIGN KEY ([StateCode]) REFERENCES [dbo].[StateMasterList] ([StateCode])
);


GO
ALTER TABLE [dbo].[RequestFieldMapping] NOCHECK CONSTRAINT [RequestFieldMapping_StateCode_FK];

