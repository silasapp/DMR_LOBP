﻿CREATE TABLE [dbo].[AppointmentReport] (
    [AppointmentReportID]                   INT            IDENTITY (1, 1) NOT NULL,
    [AppointmentId]                         BIGINT         NOT NULL,
    [LandSize]                              NVARCHAR (50)  NULL,
    [BeaconLocations]                       NVARCHAR (MAX) NULL,
    [LandTopology]                          NVARCHAR (50)  NULL,
    [NatureOfArea]                          NVARCHAR (50)  NULL,
    [AdjoiningProperties]                   NVARCHAR (MAX) NULL,
    [AccessRoadToFromSite]                  NVARCHAR (250) NULL,
    [ProposedPlantCapacity]                 NVARCHAR (50)  NULL,
    [EquipmentOnSite]                       NVARCHAR (250) NULL,
    [PipelineRightOfWay]                    NVARCHAR (250) NULL,
    [RelationshipWithStreams]               NVARCHAR (250) NULL,
    [RelationshipWithPHCNTensionLines]      NVARCHAR (250) NULL,
    [RelationshipWithRailwayLine]           NVARCHAR (250) NULL,
    [RelationshipWithSensitiveInstitutions] NVARCHAR (250) NULL,
    [MemberNames]                           VARCHAR (MAX)  NULL,
    [OfficerObservation]                    NVARCHAR (MAX) NULL,
    [OfficerFieldRecomm]                    VARCHAR (MAX)  NULL,
    [SupervisorFieldRecomm]                 VARCHAR (MAX)  NULL,
    [HODFieldRecomm]                        VARCHAR (MAX)  NULL,
    [AdOPFieldRecomm]                       VARCHAR (MAX)  NULL,
    [ZOpsconRecomm]                         VARCHAR (MAX)  NULL,
    [OfficerRecomm]                         VARCHAR (MAX)  NULL,
    [SupervisorRecomm]                      VARCHAR (MAX)  NULL,
    [ADOpsRecomm]                           VARCHAR (MAX)  NULL,
    [HODRecomm]                             VARCHAR (MAX)  NULL,
    [DirectorRecomm]                        VARCHAR (MAX)  NULL,
    [AddedBy]                               NVARCHAR (150) NULL,
    [AddedDateStamp]                        DATETIME       NULL,
    [Cali_Int_TestDate]                     DATETIME       NULL,
    [UploadedImage]                         VARCHAR (MAX)  NULL,
    [Latitude]                              VARCHAR (150)  NULL,
    [Longitude]                             VARCHAR (150)  NULL,
    CONSTRAINT [PK__Appointm__CD272E7EB5FFF8D9] PRIMARY KEY CLUSTERED ([AppointmentReportID] ASC)
);
