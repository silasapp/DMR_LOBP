using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LOBP.Models
{

    public class LgaData
    {
        public string lga_name { get; set; }
        public string lga_code { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
    }


    public class PasswordModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class InspectionModel
    {
        public string ApplicationId { get; set; }
        public short? CurrentStageID { get; set; }
        public DateTime? SchduleExpiryDate { get; set; }
        public DateTime? ScheduledDate { get; set; }

    }

    public class DocumentModel
    {
        public string DocumentName { get; set; }
        public int DocId { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateModified { get; set; }
        public string Source { get; set; }
        public string UniqueId { get; set; }
        public int FileId { get; set; }
        public string DocumentTypeName { get; set; }
        public string BaseorTran { get; set; }
        public string IsMandatory { get; set; }
        public string UploadDocumentUrl { get; set; }

    }

    public class DocumentUploadModel
    {
        public string ApplicationId { get; set; }
        public List<DocumentModel> DocumentList { get; set; }
        public List<DocumentModel> AdditionalDocumentList { get; set; }
        public List<DocumentModel> CompanyDocumentList { get; set; }
        public List<DocumentModel> CompanyFacilityMissingDocumentList { get; set; }
        public List<DocumentModel> FacilityDocumentList { get; set; }
        public string ElpsId { get; set; }
        public string ApplicationHash { get; set; }
        public string UploadDocumentUrl { get; set; }
        public string Email { get; set; }
        public string ElpsUrl { get; set; }
    }


    public class CompanyMessage
    {
        public string ApplicationId { get; set; }
        public string MessageId { get; set; }
        public string Message { get; set; }
        public string MessageType { get; set; }
        public string Subject { get; set; }
        public String Date { get; set; }
    }



    public class ApplicationFormModel
    {
        public String NameOfCompany { get; set; }
        public string ApplicationId { get; set; }
        public string ApplicationType { get; set; }
        public string LicenseToRenew { get; set; }
        public string LicenseTypeId { get; set; }
        public string State { get; set; }
        public string LGA { get; set; }
        public string StorageType { get; set; }
        public string GPS { get; set; }
        public String RegisteredAddress { get; set; }
        public String LocationAddress { get; set; }
        public String Capacity { get; set; }
        public String SponsorCompanyDetail { get; set; }
        public String ManagementAgreemnt { get; set; }
        public String AdditionalInfo { get; set; }
    }



        public class PermitToEstbalishModel
    {
        public GeneralFeatures GeneralFeatures { get; set; }
        public ExistingFacilities ExistingFacilities { get; set; }
        public string ApplicationId { get; set; }
        public string ApplicationCategory { get; set; }
        public string ApprovalCategory { get; set; }
        public int NumberOfTanks { get; set; }
        public string AppRef { get; set; }
        public string Status { get; set; }
        public string ApplicantName { get; set; }
        public Int32 AppointmentId { get; set; }
        public string InspectionFormName { get; set; }
        public string FacilityName { get; set; }
        public Nullable <int> Quarter { get; set; }
        public string RegisteredAddress { get; set; }
        public string PLW_PRW_Name { get; set; }
        public string PLW_PRW_Address { get; set; }
        public string ProposedDate { get; set; }
        public string ProposedTime { get; set; }

        [Display(Name = "State Site Located")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Select State")]
        public string State { get; set; }

        [Display(Name = "LGA Site Located")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Select LGA")]
        public string LGA { get; set; }

        [Display(Name = "Site Location")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Select Location")]
        public string SiteLocation { get; set; }
        public string LicenseTypeId { get; set; }

    }

    public class GeneralFeatures
    {
        [Display(Name = "Land Size/Area")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Select LandSize or Area")]
        public string LandSize { get; set; }

        [Display(Name = "Beacon Locations")]
        public string BeaconLocations { get; set; }

        [Display(Name = "Land Topology")]
        public string LandTopology { get; set; }

        [Display(Name = "Nature of Site Area")]
        public string NatureOfArea { get; set; }
        public string AdjoiningProperties { get; set; }

        [Display(Name = "Name of Access Road")]
        public string AccessRoad { get; set; }

        //[Display(Name = "Proposed Plant Capacity (Storage)")]
        [Display(Name = "Total Plant Processing Capacity")]
        public string ProposedPlantCapacity { get; set; }
        public string NoReasonText { get; set; }

        [Display(Name = "Other Site Information")]
        public string OtherSiteInfo { get; set; }

        public string GPS { get; set; }
        public string UTM { get; set; }

        //[Display(Name = "Annual Capacity Projections(Storage)")]
        [Display(Name = "Annual Production Projections etc 100% or 50%")]
        public string PropAnnProd { get; set; }
        public string AdditionalInfo { get; set; }
    }

    public class ExistingFacilities
    {
        [Display(Name = "Any Structure on Site?")]
        public string EquipmentOnSite { get; set; }

        [Display(Name = "Traverse Pipeline Right of Way?")]
        public string ROW { get; set; }

        [Display(Name = "Any Stream on Site?")]
        public string Streams { get; set; }

        [Display(Name = "Any Electric High Tension?")]
        public string PHCN { get; set; }

        [Display(Name = "Any Railway Line?")]
        public string RailwayLine { get; set; }

        [Display(Name = "Any Sensitive Institution?")]
        public string SensitiveInstitution { get; set; }
        public string StructuresName { get; set; }
        public string ROWName { get; set; }
        public string StreamName { get; set; }
        public string PhcnName { get; set; }
        public string RailwayName { get; set; }
        public string SensitiveInfo { get; set; }        
    }



    public class MistdoModel
    {
        public bool success { get; set; }
        public string message { get; set; }
        public string fullName { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string certificateNo { get; set; }
        public string certificateIssue { get; set; }
        public string certificateExpiry { get; set; }
        public string mistdoId { get; set; }

    }



    public class GeneralCommentModel
    {
        public string Comment { get; set; }
        public string ApplicationID { get; set; }
    }






    public class LubePlantModel
    {

        public string ApplicationId { get; set; }
        public string LicenseTypeId { get; set; }
        public string ApplicantTypeId { get; set; }
        public string FacilityName { get; set; }
        public string Status { get; set; }
        public string AppRef { get; set; }
        public string ApplicationCategory { get; set; }
        public string RegisteredAddress { get; set; }
        public string LandSize { get; set; }
        public string GPS { get; set; }
        public string UTM { get; set; }
        public string ElpsId { get; set; }
        public string ApplicationHash { get; set; }
        public string ElpsUrl { get; set; }
        public string Email { get; set; }        
        public string ATCReference { get; set; }
        public string PTEReference { get; set; }
        public string ExpiredReference { get; set; }
        public string LinkedReference { get; set; }
        public string IssuedDate { get; set; }
        public string ExpiryDate { get; set; }
         public string StorageCapacity { get; set; }
        public List<DocumentModel> CompanyDocumentList1 { get; set; }
        public List<DocumentModel> FacilityDocumentList1 { get; set; }
        public List<DocumentModel> CompanyFacilityMissingDocumentList1 { get; set; }
        public List<DocumentModel> AdditionalDocumentList1 { get; set; }


        


        [Display(Name = "Name Of Plant / Company")]
        [Editable(true, AllowInitialValue = false)]
        [DataType(DataType.Text, ErrorMessage = "Invalid Data Type")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter Name Of Plant / Company")]
        public string CompanyName { get; set; }

        [Display(Name = "Postal Address In Nigeria")]
        [Editable(true, AllowInitialValue = false)]
        [DataType(DataType.Text, ErrorMessage = "Invalid Data Type")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter Postal Address In Nigeria")]
        public string PostalAddress { get; set; }


        [Display(Name = "Location Address Of Plant")]
        [Editable(true, AllowInitialValue = true)]
        [DataType(DataType.MultilineText, ErrorMessage = "Invalid Data Type")]
        public string LocationAddress { get; set; }

        [Display(Name = "Plant Type")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Select Plant Type")]
        public string PlantType { get; set; }

        [Display(Name = "State")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Select State")]
        public string State { get; set; }

        [Display(Name = "LGA")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Select LGA")]
        public string LGA { get; set; }

        [Display(Name = "Town")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Town")]
        public string Town { get; set; }

        [Display(Name = "Application Type")]
        [Required(AllowEmptyStrings=false,ErrorMessage="Select Application Type")]
        public string ApplicationType { get; set; }

        [Display(Name = "Annual Cumulative Base Oil Requirements")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Input Annual Cumulative Base Oil Requirements")]
        public string AnnualCumulativeBaseOilRequirementsInLitres { get; set; }

        [Display(Name = "Base Oil Requirements")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Base Oil Requirements")]
        public string BaseOilRequirements { get; set; }

        [Display(Name = "Lubricants Produced by Company")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Lubricants Produced by Company")]
        public string LubricantsProducedByCompany { get; set; }

        [Display(Name = "Annual Products Production Projections")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Annual Products Production Projections")]
        public string AnnualProductsProductionProjections { get; set; }

        [Display(Name = "Training & Maintenance Programme")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Training & Maintenance Programme")]
        public string TrainingNMaintenanceProgramme { get; set; }

        [Display(Name = "Preventive Annual Maintenance")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Preventive Annual Maintenance")]
        public string PreventiveAnnualMaintenance { get; set; }

        public string AdditionalInformation { get; set; }
    }





}