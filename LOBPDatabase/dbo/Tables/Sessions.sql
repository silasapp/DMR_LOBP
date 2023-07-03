CREATE TABLE [dbo].[Sessions] (
    [SessionId]   NVARCHAR (88)   NOT NULL,
    [Created]     DATETIME        NOT NULL,
    [Expires]     DATETIME        NOT NULL,
    [LockDate]    DATETIME        NOT NULL,
    [LockCookie]  INT             NOT NULL,
    [Locked]      BIT             NOT NULL,
    [SessionItem] VARBINARY (MAX) NULL,
    [Flags]       INT             NOT NULL,
    [Timeout]     INT             NOT NULL,
    PRIMARY KEY CLUSTERED ([SessionId] ASC)
);

