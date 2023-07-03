CREATE TABLE [dbo].[WorkFlowNavigation] (
    [WorkFlowId]         INT           IDENTITY (1, 1) NOT NULL,
    [LicenseTypeId]      VARCHAR (10)  NOT NULL,
    [FieldLocationApply] VARCHAR (3)   NOT NULL,
    [ApplicationType]    VARCHAR (10)  NOT NULL,
    [Action]             VARCHAR (30)  NOT NULL,
    [ActionRole]         VARCHAR (30)  NULL,
    [CurrentStageID]     SMALLINT      NOT NULL,
    [NextStateID]        SMALLINT      NOT NULL,
    [TargetRole]         VARCHAR (150) NULL,
    [NotifyAction]       VARCHAR (30)  NULL,
    [NextActionRole]     VARCHAR (50)  NULL,
    CONSTRAINT [PK_WorkFlowNavigation] PRIMARY KEY CLUSTERED ([WorkFlowId] ASC, [LicenseTypeId] ASC, [FieldLocationApply] ASC, [ApplicationType] ASC, [Action] ASC, [CurrentStageID] ASC, [NextStateID] ASC)
);

