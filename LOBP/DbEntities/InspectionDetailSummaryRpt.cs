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
    
    public partial class InspectionDetailSummaryRpt
    {
        public int InspectionDetailSummReptId { get; set; }
        public string ApplicationId { get; set; }
        public string TypeOfPlantId { get; set; }
        public int AnnualEstBaseOilDmd { get; set; }
        public int AnnualEstAdditiveDmd { get; set; }
        public string SourceOfBaseOil { get; set; }
        public string SourceOfAdditives { get; set; }
        public int ModeofPkgLubeCanSize { get; set; }
        public int ModeofPkgLubeDrumSize { get; set; }
        public string ModeofPkgLubeBulkDest { get; set; }
        public int NbrFillingHeadCan { get; set; }
        public int NbrFillingHeadCanOprtnl { get; set; }
        public int NbrFillingHeadDrum { get; set; }
        public int NbrFillingHeadDrumOprtnl { get; set; }
        public int NbrFillingHeadBulk { get; set; }
        public int NbrFillingHeadBulkOprtnl { get; set; }
        public string AnalDesignProdCapVsActlProd { get; set; }
        public string HasLabForQualityControlAssurance { get; set; }
        public string HasLabQualityControlProfessional { get; set; }
        public string IstheLaboraryEquippedForLubeProp { get; set; }
        public string HasFunctionalClinicOrFAidBox { get; set; }
        public string HasEmergencyResponsePlan { get; set; }
        public string GeneralHouseKeepingofPlantPremises { get; set; }
        public string TreatmentPriorToEffluentDisposal { get; set; }
        public string NameDetailThirdPartyLabEffluentAnalysis { get; set; }
        public string GeneralImpressionsFindings { get; set; }
        public string Recommendations { get; set; }
        public System.DateTime DateOfInspection { get; set; }
        public string NameOfDPROfficers { get; set; }
        public string NameOfPlantRepresentative { get; set; }
    
        public virtual TypeOfPlantLookUp TypeOfPlantLookUp { get; set; }
        public virtual ApplicationRequest ApplicationRequest { get; set; }
    }
}
