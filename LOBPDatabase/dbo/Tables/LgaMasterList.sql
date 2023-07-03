CREATE TABLE [dbo].[LgaMasterList] (
    [LgaCode]   NVARCHAR (6)  NOT NULL,
    [LgaName]   NVARCHAR (80) NOT NULL,
    [StateCode] NVARCHAR (3)  NOT NULL,
    PRIMARY KEY CLUSTERED ([LgaCode] ASC),
    CONSTRAINT [fk_LgaMasterList] FOREIGN KEY ([StateCode]) REFERENCES [dbo].[StateMasterList] ([StateCode])
);


GO
ALTER TABLE [dbo].[LgaMasterList] NOCHECK CONSTRAINT [fk_LgaMasterList];

