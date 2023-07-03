using LOBP.DbEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LOBP.Models
{
    public class GlobalModel
    {
        public static string appId;
        public static string appKey;
        public static string appEmail;
        public static string elpsUrl;
        public static string appName;
        public static List<StateMasterList> AllStates { get; set; }
        public static List<LgaMasterList> AllLgas { get; set; }

        
        public static List<LicenseType> AllLicenseTypes { get; set; }
        
        public static List<RequiredLicenseDocument> AllDocApplicationType { get; set; }
        public static List<LegacyDocument> AllLegacyDocApplicationType { get; set; }
        public static List<WorkFlowNavigation> AllWorkFlowNavigation { get; set; }

    }

    public class ResultModel
    {
        public string Key { get; set; }
        public string Description { get; set; }
    }


}