CREATE TABLE [dbo].[Functionality] (
    [FuncId]      VARCHAR (20)  NOT NULL,
    [Description] VARCHAR (200) NULL,
    [MenuId]      VARCHAR (50)  NULL,
    [Action]      VARCHAR (100) NULL,
    [SeqNo]       TINYINT       NULL,
    [Status]      VARCHAR (10)  NULL,
    [IconName]    VARCHAR (100) NULL,
    CONSTRAINT [PK__Function__834DE213C6023C93] PRIMARY KEY CLUSTERED ([FuncId] ASC)
);

