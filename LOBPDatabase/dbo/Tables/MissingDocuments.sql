CREATE TABLE [dbo].[MissingDocuments] (
    [MissingDocId]  INT          IDENTITY (1, 1) NOT NULL,
    [ApplicationID] VARCHAR (30) NOT NULL,
    [DocId]         INT          NOT NULL,
    CONSTRAINT [PK_MissingDocuments] PRIMARY KEY CLUSTERED ([MissingDocId] ASC)
);

