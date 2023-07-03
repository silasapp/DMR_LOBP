CREATE TABLE [dbo].[CountriesMasterList] (
    [CountryCode] NVARCHAR (2)  NOT NULL,
    [CountryName] NVARCHAR (50) NOT NULL,
    [Value]       INT           NULL,
    PRIMARY KEY CLUSTERED ([CountryCode] ASC)
);

