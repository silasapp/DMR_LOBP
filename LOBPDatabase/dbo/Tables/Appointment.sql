CREATE TABLE [dbo].[Appointment] (
    [AppointmentId]        BIGINT         IDENTITY (1, 1) NOT NULL,
    [TypeOfAppoinment]     VARCHAR (30)   NOT NULL,
    [ApplicationId]        VARCHAR (30)   NOT NULL,
    [LicenseTypeId]        VARCHAR (10)   NULL,
    [AppointmentDate]      DATETIME       NULL,
    [AppointmentNote]      VARCHAR (350)  NULL,
    [AppointmentVenue]     VARCHAR (350)  NULL,
    [ScheduledBy]          VARCHAR (50)   NULL,
    [ScheduledDate]        DATETIME       NULL,
    [ContactPerson]        VARCHAR (150)  NULL,
    [ContactPhone]         VARCHAR (50)   NULL,
    [LastApprovedCustDate] DATETIME       NULL,
    [LastCustComment]      NVARCHAR (MAX) NULL,
    [Status]               VARCHAR (10)   NULL,
    [PrincipalOfficer]     VARCHAR (50)   NULL,
    [SchduleExpiryDate]    DATETIME       NULL,
    [InspectionTypeId]     VARCHAR (10)   NULL,
    CONSTRAINT [PK__Appointm__8ECDFCC213341AC3] PRIMARY KEY CLUSTERED ([AppointmentId] ASC)
);

