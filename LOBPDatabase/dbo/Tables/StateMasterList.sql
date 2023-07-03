CREATE TABLE [dbo].[StateMasterList] (
    [StateCode]    NVARCHAR (3)   NOT NULL,
    [StateName]    NVARCHAR (80)  NOT NULL,
    [Latitude]     NVARCHAR (30)  NULL,
    [Longitude]    NVARCHAR (30)  NULL,
    [StateAddress] NVARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([StateCode] ASC)
);

