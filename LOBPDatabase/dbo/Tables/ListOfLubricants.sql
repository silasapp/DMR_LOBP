CREATE TABLE [dbo].[ListOfLubricants] (
    [ListOfLubricantsId]  INT           IDENTITY (1, 1) NOT NULL,
    [ApplicationId]       VARCHAR (30)  NOT NULL,
    [LubricantId]         VARCHAR (150) NULL,
    [GradeSpecifications] VARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([ListOfLubricantsId] ASC),
    CONSTRAINT [FK_ListOfLubricants_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[ApplicationRequest] ([ApplicationId])
);

