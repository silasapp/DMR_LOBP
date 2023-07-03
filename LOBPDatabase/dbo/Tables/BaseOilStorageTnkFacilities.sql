CREATE TABLE [dbo].[BaseOilStorageTnkFacilities] (
    [BaseOilStTnkFacId]         INT           IDENTITY (1, 1) NOT NULL,
    [ApplicationId]             VARCHAR (30)  NOT NULL,
    [NoOfTanks]                 INT           NOT NULL,
    [TypeOfBaseOil]             VARCHAR (MAX) NOT NULL,
    [StorageCapacity]           INT           NOT NULL,
    [SurfaceCondition]          VARCHAR (500) NOT NULL,
    [LightArrestor]             VARCHAR (500) NOT NULL,
    [SafetyGauge]               VARCHAR (500) NOT NULL,
    [LevelIndcSamplePtt]        VARCHAR (500) NOT NULL,
    [BundWall]                  VARCHAR (500) NOT NULL,
    [DateDtlLstIntegTestOfTnks] VARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK__BaseOilS__333A076F4B0F9888] PRIMARY KEY CLUSTERED ([BaseOilStTnkFacId] ASC),
    CONSTRAINT [FK_BaseOilStrgTnkFac_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[ApplicationRequest] ([ApplicationId])
);

