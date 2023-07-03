CREATE TABLE [dbo].[Tanks] (
    [TankId]        INT          IDENTITY (1, 1) NOT NULL,
    [ApplicationId] VARCHAR (30) NOT NULL,
    [NbrOfTanks]    INT          NOT NULL,
    CONSTRAINT [PK_Tanks] PRIMARY KEY CLUSTERED ([TankId] ASC)
);

