CREATE TABLE [dbo].[LubricantTypeLookUp] (
    [LubricantId]   INT            IDENTITY (1, 1) NOT NULL,
    [LubricantName] NVARCHAR (250) NOT NULL,
    [Status]        NVARCHAR (10)  DEFAULT ('ACTIVE') NOT NULL,
    PRIMARY KEY CLUSTERED ([LubricantId] ASC)
);

