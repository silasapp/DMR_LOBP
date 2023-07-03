CREATE TABLE [dbo].[IntermediateHoldingTanks] (
    [IntermeHoldingTanksId] INT           IDENTITY (1, 1) NOT NULL,
    [ApplicationId]         VARCHAR (30)  NOT NULL,
    [NoOfTanks]             INT           NOT NULL,
    [TypeOfProduct]         VARCHAR (30)  NOT NULL,
    [StorageCapacity]       INT           NOT NULL,
    [SurfaceCondition]      VARCHAR (500) NOT NULL,
    [LevelIndicator]        VARCHAR (500) NOT NULL,
    CONSTRAINT [PK__Intermed__2AA8FA6847F697FE] PRIMARY KEY CLUSTERED ([IntermeHoldingTanksId] ASC),
    CONSTRAINT [FK_InterHldTanks_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[ApplicationRequest] ([ApplicationId])
);

