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
    
    public partial class LgaMasterList
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public LgaMasterList()
        {
            this.ApplicationRequests = new HashSet<ApplicationRequest>();
        }
    
        public string LgaCode { get; set; }
        public string LgaName { get; set; }
        public string StateCode { get; set; }
    
        public virtual StateMasterList StateMasterList { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ApplicationRequest> ApplicationRequests { get; set; }
    }
}
