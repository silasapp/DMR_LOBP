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
    
    public partial class InspectionEffluentHandlingDisposal
    {
        public int EffluentHandDisposalId { get; set; }
        public string ApplicationId { get; set; }
        public string EffluentCompound { get; set; }
        public string EffluentSource { get; set; }
        public string EffluentHandlingMethod { get; set; }
    
        public virtual ApplicationRequest ApplicationRequest { get; set; }
    }
}
