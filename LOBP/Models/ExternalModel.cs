using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LOBP.Models
{

    public class InspectionSchedule
    {
        public string ApplicationId { get; set; }
        public string AppointmentId { get; set; }
        public string Type { get; set; }
        public string LicenseTypeId { get; set; }
        public string Category { get; set; }
        public string Capacity { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public string State { get; set; }
        public string Lga { get; set; }
        public string CompanyRepresentative { get; set; }
        public string SheduledDateTime { get; set; }
        public string InspectorEmail { get; set; }
    }



    public class PTEInspectionDetail
    {
        public string ApplicationId { get; set; }
        public string InspectorEmail { get; set; }
        public string CompanyRepresentative { get; set; }
        public string AppointmentDateTime { get; set; }
        public string AppointmentId { get; set; }
        public string LandSize { get; set; }
        public string ProposedPlantCapacity { get; set; }
        public string BeaconLocations { get; set; }
        public string AccessRoadSite { get; set; }
        public string LandTopology { get; set; }
        public string NatureOfArea { get; set; }
        public string AdjoiningProperties { get; set; }
        public string EquipmentOnSite { get; set; }
        
        public string RelationshipWithPipelineRow { get; set; }
        public string RelationshipWithStreams { get; set; }
        public string RelationshipWithPhcn { get; set; }
        public string RelationshipWithRailwayLine { get; set; }
        public string RelationshipWithSensitiveIns { get; set; }
        public string ParticipatingOfficers { get; set; }
        public string Observations { get; set; }
        public string Recommendation { get; set; }
        public string InspectionPhoto1 { get; set; }
        public string InspectionPhoto2 { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }

    }




     public class LTOInspectionDetail
    {
        public string ApplicationId { get; set; }
        public string InspectorEmail { get; set; }
        public string CompanyRepresentative { get; set; }
        public string AppointmentDateTime { get; set; }
        public string AppointmentId { get; set; }
       
        public string ParticipatingOfficers { get; set; }
        public string Observations { get; set; }
        public string Recommendation { get; set; }
        public string InspectionPhoto1 { get; set; }
        public string InspectionPhoto2 { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }

    }

    ////////////////////////////////////
    //////////////////////////////////////


    public class LTOInspectionDetail2
    {
        public string ApplicationId { get; set; }
        public string InspectorEmail { get; set; }
        public string facilityType { get; set; }
        public string SourceBaseOil { get; set; }
        public string SourceAdditives { get; set; }
        public string AnnualEstBaseOilDmd { get; set; }
        public string AnnualEstAdditiveDmd { get; set; }		
        public string ModePkgCanLube { get; set;}
        public string ModePKgDrumLube { get; set; }	
        public string ModePKgBulkLube { get; set; }		
        public string NumOfFillHeadCan { get; set; }
		public string NumOfFillHeadCanActive { get; set; }		
        public string NumOfFillHeadDrum { get; set; }
        public string NumOfFillHeadDrumActive  { get; set; }		
        public string NumOfFillHeadBulk { get; set; }
        public string NumOfFillHeadBulkActive  { get; set; }
		//public List <LubricantDetail> ListOfLubricants  { get; set; }		
	    public string DesignAndActualOptnDiff  { get; set; }
		public string NumOf50kgCylinder  { get; set; }
	    public string NumOf25kgCylinder { get; set; }
		public string NumOf12_5kgCylinder { get; set; }
		public string NumOf6kgCylinder  { get; set; }
		public string NumOf3kgCylinder  { get; set; }
        public string TotalNumOfCylinders { get; set; }
	    public string DryChemicalPowder { get; set; }
	    public string CarbonDioxide  { get; set; }
	   	 	  
        public string GeneralObservations { get; set; }
		public string Recommendations { get; set; }
		public string ParticipatingOfficers { get; set; }
		public string CompanyRepresentative { get; set; }
		public string InspectionDate { get; set; }
        public string InspectionPhoto1 { get; set; }
        public string InspectionPhoto2 { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }

    }







    /// <summary>
    /// ////////////////////////////
    /// </summary>

    public class AllInspection
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<InspectionSchedule> InspectionSchedules { get; set; }
    }


    public class PTEInspectionForm
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public PTEInspectionDetail inspectionDetail { get; set; }
    }



    public class LTOInspectionForm
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public LTOInspectionDetail inspectionDetail { get; set; }
    }











}