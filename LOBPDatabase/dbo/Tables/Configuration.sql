﻿CREATE TABLE [dbo].[Configuration] (
    [ParamID]    VARCHAR (50)  NOT NULL,
    [ParamValue] VARCHAR (150) NULL,
    [Status]     VARCHAR (10)  NULL,
    PRIMARY KEY CLUSTERED ([ParamID] ASC)
);

