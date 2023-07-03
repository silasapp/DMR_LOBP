//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class ApplicationRequest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ApplicationRequest()
        {
            this.ActionHistories = new HashSet<ActionHistory>();
            this.BaseOilStorageTnkFacilities = new HashSet<BaseOilStorageTnkFacility>();
            this.BlendingKettles = new HashSet<BlendingKettle>();
            this.InspectionEffluentHandlingDisposals = new HashSet<InspectionEffluentHandlingDisposal>();
            this.InspectionQualityCtrlProps = new HashSet<InspectionQualityCtrlProp>();
            this.InspectionSafetyFacilities = new HashSet<InspectionSafetyFacility>();
            this.InspectionDetailSummaryRpts = new HashSet<InspectionDetailSummaryRpt>();
            this.IntermediateHoldingTanks = new HashSet<IntermediateHoldingTank>();
            this.ListOfLubricants = new HashSet<ListOfLubricant>();
            this.PaymentLogs = new HashSet<PaymentLog>();
        }
    
        public string LicenseTypeId { get; set; }
        public string ApplicationTypeId { get; set; }
        public string ApplicationId { get; set; }
        public string ModifyDate { get; set; }
        public string ApplicantUserId { get; set; }
        public string ApplicantName { get; set; }
        public string RegisteredAddress { get; set; }
        public string SiteLocationAddress { get; set; }
        public string StateCode { get; set; }
        public string LgaCode { get; set; }
        public string GPSCordinates { get; set; }
        public string LandSize { get; set; }
        public string BeaconLocations { get; set; }
        public string LandTopology { get; set; }
        public string NatureOfArea { get; set; }
        public string AdjoiningProperties { get; set; }
        public string AccessRoadToFromSite { get; set; }
        public string AnyEquipmentOnSite { get; set; }
        public string RelationshipWithPipelineRightOfWay { get; set; }
        public string RelationshipWithStreams { get; set; }
        public string RelationshipWithPHCNTensionLines { get; set; }
        public string RelationshipWithRailwayLine { get; set; }
        public string RelationshipWithSensitiveInstitutions { get; set; }
        public string NoReasonText { get; set; }
        public string GeoLocation { get; set; }
        public string PlantTypes { get; set; }
        public string ProdBaseOilRequirement { get; set; }
        public string LubricantProdByCompany { get; set; }
        public string AnnualCumuBaseOilRequirementCapacity { get; set; }
        public string AnnualProductionProjectionCapacity { get; set; }
        public string ListOfPropTrainingforStaff { get; set; }
        public string PreventiveAnnualMaintPrgmPlant { get; set; }
        public string AdditionalInfo { get; set; }
        public Nullable<short> CurrentStageID { get; set; }
        public string CurrentAssignedUser { get; set; }
        public string CurrentOfficeLocation { get; set; }
        public Nullable<System.DateTime> LicenseIssuedDate { get; set; }
        public Nullable<System.DateTime> LicenseExpiryDate { get; set; }
        public string LicenseReference { get; set; }
        public string LinkedReference { get; set; }
        public Nullable<System.DateTime> AddedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string IsLegacy { get; set; }
        public string Status { get; set; }
        public string StorageCapacity { get; set; }
        public Nullable<decimal> NetAmount { get; set; }
        public Nullable<int> SignatureID { get; set; }
        public Nullable<int> FacilityId { get; set; }
        public Nullable<int> PermitElpsId { get; set; }
        public string PrintedStatus { get; set; }
        public string ApplicationCategory { get; set; }
        public Nullable<int> Quarter { get; set; }
        public string PLW_PRW_Name { get; set; }
        public string PLW_PRW_Address { get; set; }
        public int ApplicationRequestId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ActionHistory> ActionHistories { get; set; }
        public virtual LicenseType LicenseType { get; set; }
        public virtual LgaMasterList LgaMasterList { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BaseOilStorageTnkFacility> BaseOilStorageTnkFacilities { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BlendingKettle> BlendingKettles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<InspectionEffluentHandlingDisposal> InspectionEffluentHandlingDisposals { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<InspectionQualityCtrlProp> InspectionQualityCtrlProps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<InspectionSafetyFacility> InspectionSafetyFacilities { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<InspectionDetailSummaryRpt> InspectionDetailSummaryRpts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<IntermediateHoldingTank> IntermediateHoldingTanks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ListOfLubricant> ListOfLubricants { get; set; }
        public virtual FieldLocation FieldLocation { get; set; }
        public virtual StateMasterList StateMasterList { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PaymentLog> PaymentLogs { get; set; }
        public virtual WorkFlowState WorkFlowState { get; set; }
    }
}
