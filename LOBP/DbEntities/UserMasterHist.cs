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

    public partial class UserMasterHist
    {
        public int UserMasterHistId { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string UserLocation { get; set; }
        public string UserRoles { get; set; }
        public Nullable<System.DateTime> MaintenanceDate { get; set; }
        public string MaintainedBy { get; set; }
        public string Status { get; set; }
        public string LastComment { get; set; }
    
        public virtual FieldLocation FieldLocation { get; set; }
    }
}
