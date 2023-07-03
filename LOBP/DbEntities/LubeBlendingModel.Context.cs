﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LOBP.DbEntities
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class LubeBlendingDBEntities : DbContext
    {
        public LubeBlendingDBEntities()
            : base("name=LubeBlendingDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ActionHistory> ActionHistories { get; set; }
        public virtual DbSet<ApplicationTypeLookUp> ApplicationTypeLookUps { get; set; }
        public virtual DbSet<Appointment> Appointments { get; set; }
        public virtual DbSet<AppointmentReport> AppointmentReports { get; set; }
        public virtual DbSet<BaseOilStorageTnkFacility> BaseOilStorageTnkFacilities { get; set; }
        public virtual DbSet<BlendingKettle> BlendingKettles { get; set; }
        public virtual DbSet<Configuration> Configurations { get; set; }
        public virtual DbSet<CountriesMasterList> CountriesMasterLists { get; set; }
        public virtual DbSet<FieldLocation> FieldLocations { get; set; }
        public virtual DbSet<Functionality> Functionalities { get; set; }
        public virtual DbSet<InspectionDetailSummaryRpt> InspectionDetailSummaryRpts { get; set; }
        public virtual DbSet<InspectionEffluentHandlingDisposal> InspectionEffluentHandlingDisposals { get; set; }
        public virtual DbSet<InspectionQualityCtrlProp> InspectionQualityCtrlProps { get; set; }
        public virtual DbSet<InspectionSafetyFacility> InspectionSafetyFacilities { get; set; }
        public virtual DbSet<InspectionTypeMasterList> InspectionTypeMasterLists { get; set; }
        public virtual DbSet<IntermediateHoldingTank> IntermediateHoldingTanks { get; set; }
        public virtual DbSet<LandTopologyLookUp> LandTopologyLookUps { get; set; }
        public virtual DbSet<LegacyRequiredDocument> LegacyRequiredDocuments { get; set; }
        public virtual DbSet<LgaMasterList> LgaMasterLists { get; set; }
        public virtual DbSet<LicenseType> LicenseTypes { get; set; }
        public virtual DbSet<ListOfLubricant> ListOfLubricants { get; set; }
        public virtual DbSet<LubricantTypeLookUp> LubricantTypeLookUps { get; set; }
        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<NatureOfAreaLookUp> NatureOfAreaLookUps { get; set; }
        public virtual DbSet<PaymentLog> PaymentLogs { get; set; }
        public virtual DbSet<PlantTypeLookUp> PlantTypeLookUps { get; set; }
        public virtual DbSet<RequestFieldMapping> RequestFieldMappings { get; set; }
        public virtual DbSet<Session> Sessions { get; set; }
        public virtual DbSet<StateMasterList> StateMasterLists { get; set; }
        public virtual DbSet<TypeOfPlantLookUp> TypeOfPlantLookUps { get; set; }
        public virtual DbSet<UserActivity> UserActivities { get; set; }
        public virtual DbSet<UserActivityHist> UserActivityHists { get; set; }
        public virtual DbSet<UserLogin> UserLogins { get; set; }
        public virtual DbSet<UserMasterHist> UserMasterHists { get; set; }
        public virtual DbSet<ZoneFieldMapping> ZoneFieldMappings { get; set; }
        public virtual DbSet<Facility> Facilities { get; set; }
        public virtual DbSet<MissingDocument> MissingDocuments { get; set; }
        public virtual DbSet<OutofOffice> OutofOffices { get; set; }
        public virtual DbSet<SubmittedDocument> SubmittedDocuments { get; set; }
        public virtual DbSet<TakeoverApp> TakeoverApps { get; set; }
        public virtual DbSet<Penalty> Penalties { get; set; }
        public virtual DbSet<ExtraPayment> ExtraPayments { get; set; }
        public virtual DbSet<Tank> Tanks { get; set; }
        public virtual DbSet<MistdoData> MistdoDatas { get; set; }
        public virtual DbSet<ApplicationRequest> ApplicationRequests { get; set; }
        public virtual DbSet<LegacyDocument> LegacyDocuments { get; set; }
        public virtual DbSet<RequiredLicenseDocument> RequiredLicenseDocuments { get; set; }
        public virtual DbSet<UserMaster> UserMasters { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<RoleFunctionalityMapping> RoleFunctionalityMappings { get; set; }
        public virtual DbSet<WorkFlowNavigation> WorkFlowNavigations { get; set; }
        public virtual DbSet<WorkFlowState> WorkFlowStates { get; set; }
    }
}
