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
    
    public partial class AppointmentReport
    {
        public int AppointmentReportID { get; set; }
        public long AppointmentId { get; set; }
        public string LandSize { get; set; }
        public string BeaconLocations { get; set; }
        public string LandTopology { get; set; }
        public string NatureOfArea { get; set; }
        public string AdjoiningProperties { get; set; }
        public string AccessRoadToFromSite { get; set; }
        public string ProposedPlantCapacity { get; set; }
        public string EquipmentOnSite { get; set; }
        public string PipelineRightOfWay { get; set; }
        public string RelationshipWithStreams { get; set; }
        public string RelationshipWithPHCNTensionLines { get; set; }
        public string RelationshipWithRailwayLine { get; set; }
        public string RelationshipWithSensitiveInstitutions { get; set; }
        public string MemberNames { get; set; }
        public string OfficerObservation { get; set; }
        public string OfficerFieldRecomm { get; set; }
        public string SupervisorFieldRecomm { get; set; }
        public string HODFieldRecomm { get; set; }
        public string AdOPFieldRecomm { get; set; }
        public string ZOpsconRecomm { get; set; }
        public string OfficerRecomm { get; set; }
        public string SupervisorRecomm { get; set; }
        public string ADOpsRecomm { get; set; }
        public string HODRecomm { get; set; }
        public string DirectorRecomm { get; set; }
        public string AddedBy { get; set; }
        public Nullable<System.DateTime> AddedDateStamp { get; set; }
        public string UploadedImage { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public Nullable<System.DateTime> Cali_Int_TestDate { get; set; }
    
        public virtual Appointment Appointment { get; set; }
    }
}