using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace LOBP.Models
{
    public class ElpsResponse
    {
        public ElpsResponse() { }
        public string message { get; set; }
        public object value { get; set; }
    }

    public class LoginResponse
    {
        public string code { get; set; }
        public string message { get; set; }
    }

    public class ResponseWrapper
    {
        public ResponseWrapper() { }
        public bool status { get; set; }
        public string value { get; set; }

        public string nextStageId { get; set; }
        public string nextStateType { get; set; }
        public string receivedBy { get; set; }
        public string receivedByRole { get; set; }
        public string receivedLocation { get; set; }
    }

    public class CompanyDetail
    {
        public string user_Id { get; set; }
        public string name { get; set; }
        public string business_Type { get; set; }
        public string registered_Address_Id { get; set; }
        public string operational_Address_Id { get; set; }
        public string affiliate { get; set; }
        public string nationality { get; set; }
        public string contact_FirstName { get; set; }
        public string contact_LastName { get; set; }
        public string contact_Phone { get; set; }
        public string year_Incorporated { get; set; }
        public string rC_Number { get; set; }
        public string tin_Number { get; set; }
        public string no_Staff { get; set; }
        public string no_Expatriate { get; set; }
        public string total_Asset { get; set; }
        public string yearly_Revenue { get; set; }
        public int accident { get; set; }
        public string accident_Report { get; set; }
        public string training_Program { get; set; }
        public string mission_Vision { get; set; }
        public string hse { get; set; }
        public string hseDoc { get; set; }
        public string date { get; set; }
        public bool isCompleted { get; set; }
        public int elps_Id { get; set; }
        public int id { get; set; }
    }







    public class Facilities
    {
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public DateTime DateAdded { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public int StateId { get; set; }
        public int LGAId { get; set; }
        public string FacilityType { get; set; }
        public int Id { get; set; }
        //public List<FacilityDocument> facilityDocuments { get; set; }
    }






    public class AllDocumentTypes
    {
        public int id { get; set; }
        public string name { get; set; }
        public string type { get; set; }

    }




    public class FacilityDocument
    {
        public int Company_Id { get; set; }
        public int Document_Type_Id { get; set; }
        public string documentTypeName { get; set; }
        public int FacilityId { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public DateTime Date_Modified { get; set; }
        public DateTime Date_Added { get; set; }
        public bool Status { get; set; }
        public bool Archived { get; set; }
        public string UniqueId { get; set; }
        public string Document_Name { get; set; }
        public DateTime Expiry_Date { get; set; }
        public int Id { get; set; }
        public string Type { get; set; }
    }




    public class Document
    {
        public int id { get; set; }
        public int companyId { get; set; }
        public string fileName { get; set; }
        public string source { get; set; }
        public string document_type_id { get; set; }
        public string documentTypeName { get; set; }
        public string date_modified { get; set; }
        public string date_added { get; set; }
        public string status { get; set; }
        public string archived { get; set; }
        public string document_Name { get; set; }
        public string uniqueId { get; set; }
    }



    public class Lineitem
    {
        public string lineItemsId { get; set; }
        public string beneficiaryName { get; set; }
        public string beneficiaryAccount { get; set; }
        public string bankCode { get; set; }
        public string beneficiaryAmount { get; set; }
        public string deductFeeFrom { get; set; }
    }

    public class PaymentRequest
    {
        public string serviceTypeId { get; set; }
        public string categoryName { get; set; }
        public string totalAmount { get; set; }
        public string payerName { get; set; }
        public string payerEmail { get; set; }
        public string serviceCharge { get; set; }
        public string amountDue { get; set; }
        public string STATE { get; set; }
        [DisplayName("COMPANY BRANCH")]
        public string COMPANYBRANCH { get; set; }
        [DisplayName("FACILITY ADDRESS")]
        public string FACILITYADDRESS { get; set; }
        [DisplayName("Field/Zonal Office")]
        public string FieldZonalOffice { get; set; }
        public string orderId { get; set; }
        public string returnSuccessUrl { get; set; }
        public string returnFailureUrl { get; set; }
        public string returnBankPaymentUrl { get; set; }
        public Lineitem[] lineItems { get; set; }
        public int[] documentTypes { get; set; }
        //public Applicationitem[] applicationItems { get; set; }

    }

    public class PaymentResponse
    {
        public object statusMessage { get; set; }
        public string appId { get; set; }
        public string status { get; set; }
        public string rrr { get; set; }
        public string beneficiaryAccount { get; set; }
        public string bankCode { get; set; }
        public object transactiontime { get; set; }
        public string transactionId { get; set; }
        public List<object> requiredDocs { get; set; }
    }

    public class TransactionStatus
    {
        public string statusmessage { get; set; }
        public object merchantId { get; set; }
        public string status { get; set; }
        public string rrr { get; set; }
        public string transactiontime { get; set; }
        public string orderId { get; set; }
        public object statuscode { get; set; }
    }

    public class Branch
    {
        public string name { get; set; }
        public int stateId { get; set; }
        public string address { get; set; }
        public string stateName { get; set; }
        public string countryName { get; set; }
        public int countryId { get; set; }
        public bool isFieldOffice { get; set; }
        public bool selected { get; set; }
        public int id { get; set; }
    }

    public class Staff
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string userId { get; set; }
        public string email { get; set; }
        public object phoneNo { get; set; }
        public string name { get; set; }
        public string userid { get; set; }

        public int id { get; set; }
    }

    public class ChangePassword
    {
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
        public string confirmPassword { get; set; }
    }

    public class ChangePasswordResponse
    {
        public int code { get; set; }
        public string msg { get; set; }
    }

    public class StatesInZone
    {
        public int stateId { get; set; }
        public int zoneId { get; set; }
        public string stateName { get; set; }
        public string zoneName { get; set; }
        public int id { get; set; }
    }


    public class TransactionStatusResponse
    {
        public string statusmessage { get; set; }
        public object merchantId { get; set; }
        public string status { get; set; }
        public string rrr { get; set; }
        public string transactiontime { get; set; }
        public string orderId { get; set; }
        public object statuscode { get; set; }
    }

    public class ZoneMapping
    {
        public string name { get; set; }
        public string branchName { get; set; }
        public string code { get; set; }
        public int branchId { get; set; }
        public int stateId { get; set; }
        public string address { get; set; }
        public string stateName { get; set; }
        public string countryName { get; set; }
        public int countryId { get; set; }
        public List<StatesInZone> coveredStates { get; set; }
        public List<Branch> coveredFieldOffices { get; set; }
        public int id { get; set; }
    }

    public class Zones
    {
        public string name { get; set; }
        public string branchName { get; set; }
        public string code { get; set; }
        public int branchId { get; set; }
        public int stateId { get; set; }
        public string address { get; set; }
        public string stateName { get; set; }
        public string countryName { get; set; }
        public int countryId { get; set; }
        public object coveredStates { get; set; }
        public object coveredFieldOffices { get; set; }
        public int id { get; set; }
    }

    public class PermitDTO
    {
        public string permit_No { get; set; }
        public string orderId { get; set; }
        public int company_Id { get; set; }
        public string date_Issued { get; set; }
        public string date_Expire { get; set; }
        public string categoryName { get; set; }
        public string is_Renewed { get; set; }
        public int licenseId { get; set; }
        public int id { get; set; }
    }

    public class PermitsDTO
    {
        public string permit_No { get; set; }
        public string orderId { get; set; }
        public string companyName { get; set; }
        public int company_Id { get; set; }
        public string date_Issued { get; set; }
        public string date_Expire { get; set; }
        public string categoryName { get; set; }
        public int licenseId { get; set; }
        public string licenseName { get; set; }
        public string licenseShortName { get; set; }
        public string business_Type { get; set; }
        public string rrr { get; set; }
        public string city { get; set; }
        public string stateName { get; set; }
        public string jS_Combined { get; set; }
        public int id { get; set; }
    }

    public class CompanyAddressDTO
    {
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string country_Id { get; set; }
        public string stateId { get; set; }
        public string countryName { get; set; }
        public string stateName { get; set; }
        public string postal_code { get; set; }
        public string type { get; set; }
        public int id { get; set; }
    }

    
    public class CompanyInformationModel
    {
        public CompanyDetail company { get; set; }
        public CompanyAddressDTO registeredAddress { get; set; }
        public CompanyAddressDTO operationalAddress { get; set; }
    }

    
    public class CompanyChangeModel
    {
        public string Name { get; set; }
        public string RC_Number { get; set; }
        public string Business_Type { get; set; }
        public bool emailChange { get; set; }
        public int CompanyId { get; set; }
        public string NewEmail { get; set; }
    }



    public class CompanyAddress
    {
        public string Address1 { get; set; }
        public string Country { get; set; }
        public string Address2 { get; set; }
        public string State { get; set; }
        public string City { get; set; }
    }

    public class CompanyDirectors
    {
        public List<String> DirectorNames { get; set; }
    }

    public class CompanyKeyStaffs
    {
        public List<String> KeyStaffNames { get; set; }
    }

}