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
    
    public partial class ExtraPayment
    {
        public long ExtraPaymentID { get; set; }
        public string ApplicationID { get; set; }
        public string ApplicantID { get; set; }
        public string LicenseTypeCode { get; set; }
        public Nullable<System.DateTime> TransactionDate { get; set; }
        public string TransactionID { get; set; }
        public string Description { get; set; }
        public string RRReference { get; set; }
        public string AppReceiptID { get; set; }
        public Nullable<decimal> TxnAmount { get; set; }
        public decimal Arrears { get; set; }
        public string BankCode { get; set; }
        public string Account { get; set; }
        public string TxnMessage { get; set; }
        public string Status { get; set; }
        public Nullable<int> RetryCount { get; set; }
        public Nullable<System.DateTime> LastRetryDate { get; set; }
        public string ExtraPaymentAppRef { get; set; }
        public string SanctionType { get; set; }
        public string ExtraPaymentBy { get; set; }
        public Nullable<int> PenaltyCode { get; set; }
        public Nullable<decimal> StatutoryFee { get; set; }
        public Nullable<decimal> ProcessingFee { get; set; }
        public Nullable<decimal> ServiceCharge { get; set; }
        public Nullable<int> Qty { get; set; }
    }
}
