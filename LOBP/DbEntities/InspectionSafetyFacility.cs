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
    
    public partial class InspectionSafetyFacility
    {
        public int InspectionFacilityId { get; set; }
        public string ApplicationId { get; set; }
        public int C02DryPowderExtinguisherTotal { get; set; }
        public string C02DryPowderExtSizeTypeLocation { get; set; }
        public int FoamingExtinguisherTotal { get; set; }
        public string FoamingExtSizeTypeLocation { get; set; }
        public int WaterHydrantTotal { get; set; }
        public string WaterHydrantSizeTypeLocation { get; set; }
        public int SmokeDetectorsTotal { get; set; }
        public string SmokeDetectorsSizeTypeLocation { get; set; }
        public int AlarmUnitsTotal { get; set; }
        public string AlarmUnitsSizeTypeLocation { get; set; }
        public int EmergencyShutOffUnitTotal { get; set; }
        public string EmergencyShutOffUnitSizeTypeLocation { get; set; }
        public int SafetyWarningSignTotal { get; set; }
        public string SafetyWarningSignSizeTypeLocation { get; set; }
        public int EmergencyProcedureBrdTotal { get; set; }
        public string EmergencyProcedureBrdSizeTypeLocation { get; set; }
        public int FireMusterPointTotal { get; set; }
        public string FireMusterPointSizeTypeLocation { get; set; }
        public int ListOfPpeTechStaffTotal { get; set; }
        public string ListOfPpeTechStaffSizeTypeLocation { get; set; }
    
        public virtual ApplicationRequest ApplicationRequest { get; set; }
    }
}