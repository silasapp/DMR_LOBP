CREATE TABLE [dbo].[Penalty] (
    [PenaltyId]     INT             IDENTITY (1, 1) NOT NULL,
    [PenaltyType]   VARCHAR (200)   NULL,
    [PenaltyAmount] DECIMAL (20, 2) NULL,
    [PenaltyCode]   INT             NULL,
    CONSTRAINT [PK_Penalty] PRIMARY KEY CLUSTERED ([PenaltyId] ASC)
);

