CREATE TABLE [dbo].[MistdoData] (
    [MistdoAutoId]      INT           IDENTITY (1, 1) NOT NULL,
    [fullName]          VARCHAR (150) NULL,
    [phoneNumber]       VARCHAR (20)  NULL,
    [email]             VARCHAR (150) NULL,
    [certificateNo]     VARCHAR (MAX) NULL,
    [certificateIssue]  DATETIME      NULL,
    [certificateExpiry] DATETIME      NULL,
    [AddedDate]         DATETIME      NULL,
    [ElpsId]            VARCHAR (20)  NULL,
    [HasExpired]        VARCHAR (5)   NULL,
    [mistdoId]          VARCHAR (MAX) NULL,
    CONSTRAINT [PK_MistdoData] PRIMARY KEY CLUSTERED ([MistdoAutoId] ASC)
);

