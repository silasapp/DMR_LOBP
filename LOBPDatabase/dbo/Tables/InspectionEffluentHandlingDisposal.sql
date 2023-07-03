CREATE TABLE [dbo].[InspectionEffluentHandlingDisposal] (
    [EffluentHandDisposalId] INT           IDENTITY (1, 1) NOT NULL,
    [ApplicationId]          VARCHAR (30)  NOT NULL,
    [EffluentCompound]       VARCHAR (150) NOT NULL,
    [EffluentSource]         VARCHAR (500) NOT NULL,
    [EffluentHandlingMethod] VARCHAR (500) NOT NULL,
    CONSTRAINT [PK__Inspecti__1617A84111E4E515] PRIMARY KEY CLUSTERED ([EffluentHandDisposalId] ASC),
    CONSTRAINT [FK_EffluentHandlingDisposal_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[ApplicationRequest] ([ApplicationId])
);

