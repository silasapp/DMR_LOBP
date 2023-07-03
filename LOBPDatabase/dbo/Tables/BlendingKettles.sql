CREATE TABLE [dbo].[BlendingKettles] (
    [BlendingKettlesId]     INT           IDENTITY (1, 1) NOT NULL,
    [ApplicationId]         VARCHAR (30)  NOT NULL,
    [NoOfKettles]           INT           NOT NULL,
    [Capacity]              INT           NOT NULL,
    [BlendingMethod]        VARCHAR (500) NOT NULL,
    [OperConditionPressure] VARCHAR (500) NOT NULL,
    [OperConditionTemp]     VARCHAR (500) NOT NULL,
    CONSTRAINT [PK__Blending__5D348B58DFFA68E4] PRIMARY KEY CLUSTERED ([BlendingKettlesId] ASC),
    CONSTRAINT [FK_BlendingKettles_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[ApplicationRequest] ([ApplicationId])
);

