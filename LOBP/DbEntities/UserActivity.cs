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
    [Serializable]

    public partial class UserActivity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserActivity()
        {
            this.UserActivityHists = new HashSet<UserActivityHist>();
        }
    
        public int ActivityId { get; set; }
        public string LicenseTypeId { get; set; }
        public string UserId { get; set; }
        public Nullable<int> TxnCount { get; set; }
        public Nullable<System.DateTime> ValueDate { get; set; }
        public Nullable<short> CurrentStageID { get; set; }
    
        public virtual LicenseType LicenseType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserActivityHist> UserActivityHists { get; set; }
    }
}