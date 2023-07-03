CREATE TABLE [dbo].[LandTopologyLookUp] (
    [Code]         NVARCHAR (20)  NOT NULL,
    [TopologyName] NVARCHAR (200) NOT NULL,
    [Status]       NVARCHAR (10)  DEFAULT ('ACTIVE') NOT NULL,
    PRIMARY KEY CLUSTERED ([Code] ASC)
);

