using LOBP.DbEntities;
using LOBP.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LOBP.Models
{

    public class eventData
    {
        public string title { get; set; }
        public string start { get; set; }
        public string venue { get; set; }
    }


     public class ValueData
    {
        public string id { get; set; }
        public string Company { get; set; }
        public string RRR { get; set; }
    }



    public class NominatedStaff
    {
        public string Email { get; set; }
        
    }


    public class FacilityInfo
    {
        public string CompanyEmail { get; set; }
        public string CompanyName { get; set; }
        public string Action { get; set; }
        public Nullable<int> Quarter { get; set; }
        public string ApplicationID { get; set; }
        public int ApplicationRequestId { get; set; }
        public Nullable<int> FacilityId { get; set; }
        public Nullable<int> ElpsFacilityId { get; set; }
        public string FacilityName { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string AnnualStorageCapacity { get; set; }
        public string StorageCapacity { get; set; }
        public string AppliedDate { get; set; }
        public string AplicationCodeType { get; set; }
        public string LocationAddress { get; set; }
        public string GPS { get; set; }
        public string CurrentDeskEmail { get; set; }
        public string LicenseReference{ get; set; }
        public string LGACode { get; set; }
        public string StateCode { get; set; }
        public string ApplicationCategory { get; set; }
        public int CurrentDeskId { get; set; }
        public string CurrentOfficeLocationId { get; set; }

        public Nullable<DateTime> LicenseExpiryDate { get; set; }
        public Nullable<DateTime> LicenseIssuedDate { get; set; }

    }


    public class WorkFlows
    {

        public int WorkFlowId { get; set; }
        public string FieldLocationApply { get; set; }
        public string ApplicationType { get; set; }
        public string Action { get; set; }
        public string ActionRole { get; set; }
        public short CurrentStageID { get; set; }
        public short NextStateID { get; set; }
        public string TargetRole { get; set; }
        public string NotifyAction { get; set; }
    }





    public class InspectionHistory
    {
        
        public string ApplicationId { get; set; }
        public string ScheduledBy { get; set; }
        public Nullable<DateTime> AppointmentDate { get; set; }
        public string LicenseTypeId { get; set; }
        public string AddedBy { get; set; }
        public Nullable<DateTime> AddedDateStamp { get; set; }

    }




    public class RevenueItemViewModel
    {
        public int RevenueItemId { get; set; }
        public decimal? Amount { get; set; }

        public int Quantity { get; set; }
    }


    public class IGRResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }

    }


    public class PaymentReportViewModel
    {
        public string ApplicationID { get; set; }
        public string Status { get; set; }
        public string RRReference { get; set; }
        public string pay { get; set; }
        public string servicecharge { get; set; }
        public string processingfee { get; set; }
        public string statutoryfee { get; set; }
        public string amt { get; set; }
        public string companyemail { get; set; }
        public string companyname { get; set; }
        public string category { get; set; }
        public string transdate { get; set; }
        public DateTime? transDATE { get; set; }
        public string StateName { get; set; }
        public string StateCode { get; set; }
        public string Category { get; set; }
        public string ApplicationType { get; set; }
    }





    public class RatioDash
    {
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int OnDesk { get; set; }
        public int LegacyInProgress { get; set; }
        public int OnlineInProgress { get; set; }
        public int Processed { get; set; }
    }






    public class ScheduleModel
    {
        public string ApplicationId { get; set; }
        public string ScheduledBy { get; set; }
        public string ApplicationType { get; set; }
        public string CompanyName { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime ScheduleExpiredDate { get; set; }
        public DateTime AppointmentDate { get; set; }
    }







    public class PaymentModel
    {

        public string RRReference { get; set; }
        public string SanctionType { get; set; }
        public long ExtraPaymentID { get; set; }
        public Nullable <int> PenaltyCode { get; set; }
        public string Status { get; set; }
        public Nullable<DateTime> TransactionDate { get; set; }
        public string TransactionID { get; set; }
        public Nullable<decimal> TxnAmount { get; set; }
        public string ApplicantName { get; set; }
        public string ApplicantId { get; set; }
        public string ApplicationID { get; set; }
        public string LicenseTypeCode { get; set; }
        public string ExtraPaymentAppRef { get; set; }
        public string ExtraApplicationID { get; set; }
        public string CompanyUserId { get; set; }
        public decimal Arrears { get; set; }

    }








    public class Permitmodel
    {
        public string TelePhoneNumber { get; set; }
        public string Fieldofficeaddress { get; set; }
        public string Signature { get; set; }
        public string PMB { get; set; }
        public string Address { get; set; }
        public string LGA { get; set; }
        public string ApprefNo { get; set; }
        public string ApplicationCategory { get; set; }
        public int NUmberOfTanks { get; set; }
        public string NUmberOfTanksToWord { get; set; }
        public string CompanyName { get; set; }
        public string State { get; set; }
        public string RegisteredAddress { get; set; }
        public string LocationAddress { get; set; }
        public string IssuedDate { get; set; }
        public string ExpiryDate { get; set; }
        public string PermitNumber { get; set; }
        public decimal Amountpaid { get; set; }
        public double Capacity { get; set; }
        public decimal Arrear { get; set; }
        public string TakeoverPlantName { get; set; }
        public string TakeoverPlantLocation { get; set; }
        public string CompanyIdentity { get; set; }
        public string LicenseNumber { get; set; }
        public string CapacityToWord { get; set; }
        public string AmountToWord { get; set; }
        public string IssuedDay { get; set; }
        public DateTime DateIssued { get; set; }
        public string IssuedMonth { get; set; }
        public string IssuedYear { get; set; }
        public string ExpiryYear { get; set; }
        public DateTime Expiry { get; set; }
        public string TestDate { get; set; }
        public string Prw_Plw_Name { get; set; }
        public string Prw_Plw_Address { get; set; }
        public string TPBA_LABEL { get; set; }        
        public byte[] QrCode { get; internal set; }



    }





    public class FieldlocationModel
    {
        public string StateAddress { get; set; }
        public string StateName { get; set; }
        public string LgaName { get; set; }
        public string LocationAddress { get; set; }

    }






    public class PermitModels
    {
        public List<Permitmodel> permitmodels { get; set; }
    }
    public class StaffDeskModel
    {
        public List<StaffDesk> StaffDeskList { get; set; }
    }


    public class StaffDesk
    {
        public string StaffEmail { get; set; }
        public string StaffName { get; set; }
        public string BranchName { get; set; }
        public string Department { get; set; }
        public string Role { get; set; }
        public string Location { get; set; }
        public int OnDesk { get; set; }
        public string status { get; set; }
    }


    public class ApplicationOnStaffDesk
    {
        public string ReferenceId { get; set; }
        public string CompanyName { get; set; }
        public string LicenseType { get; set; }
        public string Stage { get; set; }
        public string DateAdded { get; set; }
    }

    public class LicenseRatio
    {
        public int totalLicense { get; set; }
        public int Online { get; set; }
        public int Legacy { get; set; }
    }
    public class StateChart
    {
        public string ApplicationId { get; set; }
        public Nullable<long> Abia { get; set; }
        public Nullable<long> Abuja { get; set; }
        public Nullable<long> Adamawa { get; set; }
        public Nullable<long> AkwaIbom { get; set; }
        public Nullable<long> Anambra { get; set; }
        public Nullable<long> Bauchi { get; set; }
        public Nullable<long> Bayelsa { get; set; }
        public Nullable<long> Benue { get; set; }
        public Nullable<long> Borno { get; set; }
        public Nullable<long> CrossRiver { get; set; }
        public Nullable<long> Delta { get; set; }
        public Nullable<long> Ebonyi { get; set; }
        public Nullable<long> Edo { get; set; }
        public Nullable<long> Ekiti { get; set; }
        public Nullable<long> Enugu { get; set; }
        public Nullable<long> Gombe { get; set; }
        public Nullable<long> Imo { get; set; }
        public Nullable<long> Jigawa { get; set; }
        public Nullable<long> Kaduna { get; set; }
        public Nullable<long> Kano { get; set; }
        public Nullable<long> Katsina { get; set; }
        public Nullable<long> Kebbi { get; set; }
        public Nullable<long> Kogi { get; set; }
        public Nullable<long> Kwara { get; set; }
        public Nullable<long> Lagos { get; set; }
        public Nullable<long> Nasarawa { get; set; }
        public Nullable<long> Niger { get; set; }
        public Nullable<long> Ogun { get; set; }
        public Nullable<long> Ondo { get; set; }
        public Nullable<long> Osun { get; set; }
        public Nullable<long> Oyo { get; set; }
        public Nullable<long> Plateau { get; set; }
        public Nullable<long> Rivers { get; set; }
        public Nullable<long> Sokoto { get; set; }
        public Nullable<long> Taraba { get; set; }
        public Nullable<long> Yobe { get; set; }
        public Nullable<long> Zamfara { get; set; }

    }
    public class StaffRatio
    {
        public int totalstaff { get; set; }
        public int ADOPERATIONRBP { get; set; }
        public int FIELDADMIN { get; set; }
        public int HEADADMIN { get; set; }
        public int HEADGAS { get; set; }
        public int OFFICER { get; set; }
        public int SUPERADMIN { get; set; }
        public int SUPERVISOR { get; set; }
        public int ZONALADMIN { get; set; }
        public int ZOPSCON { get; set; }
        public int REVIEWER { get; set; }
        public int OPSCON { get; set; }
        public int HOD { get; set; }
        public int HOOD { get; set; }

    }
    public class ApplicationRatio
    {
        public int totalapplication { get; set; }
        public int CompleteApplication { get; set; }
        public int UnCompleteApplication { get; set; }
    }


    public class AppointmentReportModel
    {
        public string ApplicationId { get; set; }
        public string LandSize { get; set; }
        public string UploadedImage { get; set; }
        public string LicenseType { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public string BeaconLocations { get; set; }
        public string LandTopology { get; set; }
        public string NatureOfArea { get; set; }
        public string AdjoiningProperties { get; set; }
        public string AccessRoadSite { get; set; }
        public string ProposedPlantCapacity { get; set; }
        public string EquipmentOnSite { get; set; }
        public string RelationshipWithPipelineRow { get; set; }
        public string RelationshipWithStreams { get; set; }
        public string RelationshipWithPhcn { get; set; }
        public string RelationshipWithRailwayLine { get; set; }
        public string RelationshipWithSensitiveIns { get; set; }
        public string AppointmentDateTime { get; set; }
        public string MemberOfTeams { get; set; }
        public string CompanyReps { get; set; }
        public string OfficerFieldObservations { get; set; }
        public string OfficerFieldRecommendation { get; set; }
        public string SuperviorFieldRecommendation { get; set; }
        public string HodFieldRecommendation { get; set; }
        public string AdOpsFieldRecommendation { get; set; }
        public string ZopsconFieldRecommendation { get; set; }

        public string OfficerHORecommendation { get; set; }
        public string SupervisorHORecommendation { get; set; }
        public string AdOpsHORecommendation { get; set; }
        public string HodHORecommendation { get; set; }        
        public Nullable<System.DateTime> Cali_Int_TestDate { get; set; }

    }


    public class RecommendationModel
    {
        public string PlantName { get; set; }
        public string PlantLocation { get; set; }
        public string OfficerObservation { get; set; }
        public string DprMemberTeams { get; set; }

        public string PrincipalOfficer { get; set; }
        public string AppointmentDate { get; set; }

        public string OfficerFieldRecommendation { get; set; }
        public string SuperviorFieldRecommendation { get; set; }
        public string HodFieldRecommendation { get; set; }
        public string AdOpsFieldRecommendation { get; set; }
        public string ZopsconFieldRecommendation { get; set; }

        public string OfficerHORecommendation { get; set; }
        public string SupervisorHORecommendation { get; set; }
        public string AdOpsHORecommendation { get; set; }
        public string HodHORecommendation { get; set; }
    }



    


}