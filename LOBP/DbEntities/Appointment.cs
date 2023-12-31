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
    
    public partial class Appointment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Appointment()
        {
            this.AppointmentReports = new HashSet<AppointmentReport>();
        }
    
        public long AppointmentId { get; set; }
        public string TypeOfAppoinment { get; set; }
        public string ApplicationId { get; set; }
        public string LicenseTypeId { get; set; }
        public Nullable<System.DateTime> AppointmentDate { get; set; }
        public string AppointmentNote { get; set; }
        public string AppointmentVenue { get; set; }
        public string ScheduledBy { get; set; }
        public Nullable<System.DateTime> ScheduledDate { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPhone { get; set; }
        public Nullable<System.DateTime> LastApprovedCustDate { get; set; }
        public string LastCustComment { get; set; }
        public string Status { get; set; }
        public string PrincipalOfficer { get; set; }
        public Nullable<System.DateTime> SchduleExpiryDate { get; set; }
        public string InspectionTypeId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AppointmentReport> AppointmentReports { get; set; }
        public virtual LicenseType LicenseType { get; set; }
    }
}
