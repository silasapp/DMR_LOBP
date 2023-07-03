CREATE TABLE [dbo].[WorkFlowState] (
    [StateID]   SMALLINT      NOT NULL,
    [StateName] VARCHAR (150) NULL,
    [StateType] VARCHAR (10)  NULL,
    [Progress]  VARCHAR (10)  NULL,
    PRIMARY KEY CLUSTERED ([StateID] ASC)
);

