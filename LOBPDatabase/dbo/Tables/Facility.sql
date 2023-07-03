CREATE TABLE [dbo].[Facility] (
    [FacilityId]      INT           IDENTITY (1, 1) NOT NULL,
    [CompanyUserId]   VARCHAR (50)  NULL,
    [FalicityName]    VARCHAR (MAX) NULL,
    [LocationAddress] VARCHAR (MAX) NULL,
    [ElpsFacilityId]  INT           NULL,
    CONSTRAINT [PK_Facility] PRIMARY KEY CLUSTERED ([FacilityId] ASC)
);

