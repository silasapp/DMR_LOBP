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
    
    public partial class BlendingKettle
    {
        public int BlendingKettlesId { get; set; }
        public string ApplicationId { get; set; }
        public int NoOfKettles { get; set; }
        public int Capacity { get; set; }
        public string BlendingMethod { get; set; }
        public string OperConditionPressure { get; set; }
        public string OperConditionTemp { get; set; }
    
        public virtual ApplicationRequest ApplicationRequest { get; set; }
    }
}
