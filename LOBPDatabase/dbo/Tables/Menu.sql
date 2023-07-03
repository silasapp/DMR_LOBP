CREATE TABLE [dbo].[Menu] (
    [MenuId]      VARCHAR (20)  NOT NULL,
    [Description] VARCHAR (100) NULL,
    [IconName]    VARCHAR (50)  NULL,
    [SeqNo]       TINYINT       NULL,
    [Status]      VARCHAR (10)  NULL,
    CONSTRAINT [PK__Menu__C99ED230FB09E1E5] PRIMARY KEY CLUSTERED ([MenuId] ASC)
);

