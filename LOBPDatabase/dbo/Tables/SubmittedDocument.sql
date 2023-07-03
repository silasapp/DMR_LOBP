CREATE TABLE [dbo].[SubmittedDocument] (
    [SubmitedDocId] INT           IDENTITY (1, 1) NOT NULL,
    [ApplicationID] VARCHAR (30)  NOT NULL,
    [FileId]        INT           NOT NULL,
    [DocId]         INT           NOT NULL,
    [DocSource]     VARCHAR (MAX) NOT NULL,
    [BaseorTrans]   VARCHAR (2)   NOT NULL,
    [DocName]       VARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_SubmittedDocument] PRIMARY KEY CLUSTERED ([SubmitedDocId] ASC)
);

