using log4net;
using LOBP.DbEntities;
using LOBP.Models;
using Newtonsoft.Json.Linq;
using QRCoder;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;

namespace LOBP.Helper
{
    public class UtilityHelper
    {
        private LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities();
        private Object thisLock = new Object();
        private ILog log = log4net.LogManager.GetLogger(typeof(UtilityHelper));
        List<Lineitem> lineItemsList = new List<Lineitem>();
        private decimal totalAmt = 0;
        JObject values = new JObject();
        public UtilityHelper() { }

        public UtilityHelper(LubeBlendingDBEntities dbCtxt)
        {
            this.dbCtxt = dbCtxt;
        }

        public String zeroPadder(String text, int maxlenght)
        {
            String retText = text;

            for (int j = retText.Length; j < maxlenght; j++)
            {
                retText = "0" + retText;
            }
            return retText;
        }

        public String TruncateText(String text, int maxlenght)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (text.Length <= maxlenght)
                {
                    return text;
                }
                else
                {
                    return text.Substring(0, maxlenght);
                }
            }
            else
            {
                return "";
            }

        }

        public string GenerateApplicationNo()
        {
            string applicationNum;
            lock (thisLock)
            {
                applicationNum = DateTime.Now.ToString("MMddyyHHmmss");
                Thread.Sleep(1000);
                return applicationNum;
            }
        }

        public string GetApplicationFees(ApplicationRequest appRequest, out decimal processFee, out decimal statutoryFee)
        {

            processFee = 0;
            statutoryFee = 0;
            string feesCode;
            string errorMessage = "SUCCESS";
            if (appRequest.IsLegacy == "NO")
            {
                try
                {
                    feesCode = appRequest.LicenseTypeId + "_" + appRequest.ApplicationTypeId + "_PROCESS_FEES";
                    processFee = Convert.ToDecimal(dbCtxt.Configurations.Where(c => c.ParamID == feesCode).FirstOrDefault().ParamValue);
                    var nbroftnks = (from a in dbCtxt.ApplicationRequests join t in dbCtxt.Tanks on a.ApplicationId equals t.ApplicationId where t.ApplicationId == appRequest.ApplicationId select t.NbrOfTanks).FirstOrDefault();
                    var capacity = Convert.ToDouble(appRequest.AnnualCumuBaseOilRequirementCapacity);
                    if (appRequest.LicenseTypeId == "LTO")
                    {
                        statutoryFee = CalculateLicenseFee(appRequest.LicenseTypeId, capacity);
                    }

                    if (appRequest.LicenseTypeId == "LTOLFP")
                    {
                        statutoryFee = CalculateLTOLFPFee(capacity); 
                    }


                    if (appRequest.LicenseTypeId.Contains("TPBA"))
                    {
                        statutoryFee = CalculateTPBAFee(appRequest.LicenseTypeId, capacity);

                        if (appRequest.Quarter == 2)
                        {
                            statutoryFee = statutoryFee * 2;
                            processFee = processFee * 2;
                        }
                        else if (appRequest.Quarter == 3)
                        {
                            statutoryFee = statutoryFee * 3;
                            processFee = processFee * 3;
                        }
                        else if (appRequest.Quarter == 4)
                        {
                            statutoryFee = statutoryFee * 4;
                            processFee = processFee * 4;
                        }
                    }
                    if (appRequest.LicenseTypeId == "TITA" || appRequest.LicenseTypeId == "TCA")
                    {
                        processFee = processFee * nbroftnks;
                    }
                    if (appRequest.ApplicationCategory == "Lube Oil Blending Plant And Waste Recycling Plant")
                    {
                        processFee = processFee * 2;
                    }


                    log.Info("Processing Fee =>" + processFee + ",Statutory/License Fee =>" + statutoryFee);
                }
                catch (Exception ex)
                {
                    errorMessage = "Error Occured Getting Application Fees, Please Try again Later";
                    log.Error(ex.StackTrace);
                }
            }

            return errorMessage;
        }

        public IEnumerable<FieldlocationModel> FieldLocation(LubeBlendingDBEntities dbCtxt, string id, out string Lga, out string statename, out string address)
        {
            List<FieldlocationModel> field = new List<FieldlocationModel>();
            var fieldofficeaddress = (from a in dbCtxt.ApplicationRequests
                                      join s in dbCtxt.StateMasterLists on a.StateCode equals s.StateCode
                                      join l in dbCtxt.LgaMasterLists on a.LgaCode equals l.LgaCode
                                      where a.ApplicationId == id
                                      select new { s.StateAddress, s.StateName, l.LgaName, a.SiteLocationAddress }).ToList();
            Lga = fieldofficeaddress.FirstOrDefault().LgaName;
            statename = fieldofficeaddress.FirstOrDefault().StateName;
            address = fieldofficeaddress.FirstOrDefault().StateAddress;
            foreach (var item in fieldofficeaddress)
            {
                field.Add(new FieldlocationModel()
                {
                    LgaName = item.LgaName,
                    StateAddress = item.StateAddress,
                    StateName = item.StateName,
                    LocationAddress = item.SiteLocationAddress
                });
            }

            return field;
        }

        public string AppLicenseCodeType(string ApplicationId)
        {
            string Getcodetype;
            using (var dbCtxt = new LubeBlendingDBEntities())
            {
                Getcodetype = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == ApplicationId select a.LicenseTypeId).FirstOrDefault();
            }
            return Getcodetype;
        }

        public List<OutofOffice> GetReliverStaffOutofOffice(LubeBlendingDBEntities dbCtxt, string reliever)
        {
            var today = DateTime.Now.Date;
            var getpastwksapp = new List<OutofOffice>();
            var wksbackapp = (from p in dbCtxt.OutofOffices
                              join r in dbCtxt.ApplicationRequests on p.Relieved equals r.CurrentAssignedUser
                              where p.Reliever == reliever && p.Status == "Started"
                              select new
                              {
                                  p.Reliever,
                                  p.Relieved,
                                  p.StartDate,
                                  p.EndDate,
                                  p.Status,
                                  p.Comment
                              }).ToList();
            foreach (var item in wksbackapp)
            {
                getpastwksapp.Add(new OutofOffice()
                {
                    Reliever = item.Reliever,
                    Relieved = item.Relieved,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    Status = item.Status,
                    Comment = item.Comment
                });
            }
            return getpastwksapp;
        }

        public List<OutofOffice> GetAllOutofOffice(LubeBlendingDBEntities dbCtxt)
        {
            var today = DateTime.Now.Date;
            var getpastwksapp = new List<OutofOffice>();
            var wksbackapp = (from p in dbCtxt.OutofOffices
                              select new
                              {
                                  p.Reliever,
                                  p.Relieved,
                                  p.StartDate,
                                  p.EndDate,
                                  p.Status,
                                  p.Comment
                              }).ToList();
            foreach (var item in wksbackapp)
            {
                getpastwksapp.Add(new OutofOffice()
                {
                    Reliever = item.Reliever,
                    Relieved = item.Relieved,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    Status = item.Status,
                    Comment = item.Comment
                });
            }
            return getpastwksapp;
        }

        public List<InspectionHistory> GetInspectionReoprt(LubeBlendingDBEntities dbCtxt)
        {
            var inspectionreport = new List<InspectionHistory>();
            var allinspectionreport = (from p in dbCtxt.AppointmentReports
                                       join r in dbCtxt.Appointments on p.AppointmentId equals r.AppointmentId

                                       select new
                                       {
                                           r.ApplicationId,
                                           r.ScheduledBy,
                                           r.AppointmentDate,
                                           r.LicenseTypeId,
                                           p.AddedBy,
                                           p.AddedDateStamp
                                       }).ToList();
            foreach (var item in allinspectionreport)
            {
                inspectionreport.Add(new InspectionHistory()
                {
                    ApplicationId = item.ApplicationId,
                    ScheduledBy = item.ScheduledBy,
                    AppointmentDate = item.AppointmentDate,
                    LicenseTypeId = item.LicenseTypeId,
                    AddedBy = item.AddedBy,
                    AddedDateStamp = item.AddedDateStamp
                });
            }
            return inspectionreport;
        }

        public List<ApplicationRequest> GetApplicationForPastThreeWks(string staffemail, LubeBlendingDBEntities dbCtxt)
        {
            var Pastwksdate = DateTime.Now.AddDays(-21).Date;
            var getpastwksapp = new List<ApplicationRequest>();
            var location = (from l in dbCtxt.UserMasters where l.UserId == staffemail select l.UserLocation).FirstOrDefault();
            var wksbackapp = (from p in dbCtxt.ApplicationRequests
                              where DbFunctions.TruncateTime(p.ModifiedDate) <= Pastwksdate && p.CurrentAssignedUser != p.ApplicantUserId && p.LicenseReference == null && p.CurrentOfficeLocation == location
                              select new
                              {
                                  p.ApplicationId,
                                  p.ApplicantName,
                                  p.LicenseTypeId,
                                  p.ApplicationTypeId,
                                  ModifyDate = p.ModifiedDate
                              }).ToList();
            foreach (var item in wksbackapp)
            {
                getpastwksapp.Add(new ApplicationRequest()
                {
                    ApplicationId = item.ApplicationId,
                    ApplicantName = item.ApplicantName,
                    LicenseTypeId = item.LicenseTypeId,
                    ApplicationTypeId = item.ApplicationTypeId,
                    ModifyDate = Convert.ToDateTime(item.ModifyDate).ToLongDateString()
                });
            }
            return getpastwksapp;
        }

        public List<ApplicationRequest> GetApplicationForPastFiveDays(string staffemail, LubeBlendingDBEntities dbCtxt)
        {
            var Pastwksdate = DateTime.Now.AddDays(-5).Date;
            var getpastwksapp = new List<ApplicationRequest>();
            var location = (from l in dbCtxt.UserMasters where l.UserId == staffemail select l.UserLocation).FirstOrDefault();
            var wksbackapp = (from p in dbCtxt.ApplicationRequests
                              where DbFunctions.TruncateTime(p.ModifiedDate) <= Pastwksdate && p.CurrentAssignedUser != p.ApplicantUserId && p.LicenseReference == null && p.CurrentOfficeLocation == location
                              select new
                              {
                                  p.ApplicationId,
                                  p.ApplicantName,
                                  p.LicenseTypeId,
                                  p.ApplicationTypeId,
                                  p.CurrentAssignedUser,
                                  ModifyDate = p.ModifiedDate
                              }).ToList();
            foreach (var item in wksbackapp)
            {
                getpastwksapp.Add(new ApplicationRequest()
                {
                    ApplicationId = item.ApplicationId,
                    ApplicantName = item.ApplicantName,
                    LicenseTypeId = item.LicenseTypeId,
                    ApplicationTypeId = item.ApplicationTypeId,
                    CurrentAssignedUser = item.CurrentAssignedUser,
                    ModifyDate = Convert.ToDateTime(item.ModifyDate).ToLongDateString()
                });
            }
            return getpastwksapp;
        }

        public string GetApplicationCharges(ApplicationRequest appRequest, out decimal processingFee, out decimal LicenseFee, out decimal LicenseArrearsFee, out decimal LateRenewalPenaltyFee, out decimal NonRenewalPenaltyFee)
        {

            LicenseFee = 0;
            processingFee = 0;
            NonRenewalPenaltyFee = 0;
            LicenseArrearsFee = 0;
            LateRenewalPenaltyFee = 0;
            string errorMessage = "SUCCESS";

            try
            {
                processingFee = Convert.ToDecimal(dbCtxt.Configurations.Where(c => c.ParamID == appRequest.LicenseTypeId + "_" + appRequest.ApplicationTypeId + "_PROCESS_FEES").FirstOrDefault().ParamValue);
                var nbroftnks = (from a in dbCtxt.ApplicationRequests join t in dbCtxt.Tanks on a.ApplicationId equals t.ApplicationId where t.ApplicationId == appRequest.ApplicationId select t.NbrOfTanks).FirstOrDefault();

                LicenseArrearsFee = CalculateArrears(appRequest.ApplicationId, appRequest.ApplicantUserId, dbCtxt);
                if (appRequest.LicenseTypeId == "LTO")
                {

                    LicenseFee = CalculateLicenseFee(appRequest.LicenseTypeId, double.Parse(appRequest.AnnualCumuBaseOilRequirementCapacity, System.Globalization.CultureInfo.InvariantCulture));

                }
                else if(appRequest.LicenseTypeId == "LTOLFP")
                {
                    LicenseFee = CalculateLTOLFPFee(double.Parse(appRequest.AnnualCumuBaseOilRequirementCapacity, System.Globalization.CultureInfo.InvariantCulture));

                }
                else if (appRequest.LicenseTypeId == "TITA" || appRequest.LicenseTypeId == "TCA")
                {
                    processingFee = processingFee * nbroftnks;
                }
                else if (appRequest.LicenseTypeId.Contains("TPBA"))
                {
                    LicenseFee = CalculateTPBAFee(appRequest.LicenseTypeId, double.Parse(appRequest.AnnualCumuBaseOilRequirementCapacity, System.Globalization.CultureInfo.InvariantCulture));

                    if (appRequest.Quarter == 2)
                    {
                        LicenseFee = LicenseFee * 2;
                        processingFee = processingFee * 2;
                    }
                    else if (appRequest.Quarter == 3)
                    {
                        LicenseFee = LicenseFee * 3;
                        processingFee = processingFee * 3;
                    }
                    else if (appRequest.Quarter == 4)
                    {
                        LicenseFee = LicenseFee * 4;
                        processingFee = processingFee * 4;
                    }

                }




                else
                {
                    return errorMessage;
                }

            }
            catch (Exception ex)
            {
                errorMessage = "Error Occured Getting Application Fees, Please Try again Later";
                log.Error(ex.StackTrace);
            }
            finally
            {
                log.Info("Processing Fee =>" + processingFee + ",License Fee =>" + LicenseFee);
            }

            return errorMessage;
        }

        public string GenerateHashText(string inputString)
        {
            SHA512 sha512 = SHA512Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }

        public bool IsNumeric(string input)
        {
            int test;
            return int.TryParse(input, out test);
        }

        public static Uri BuildDebugUri(IRestClient client, IRestRequest request)
        {
            var uri = client.BuildUri(request);
            if (request.Method != Method.POST &&
             request.Method != Method.PUT &&
             request.Method != Method.PATCH)
            {
                return uri;
            }
            else
            {
                var queryParameters = from p in request.Parameters
                                      where p.Type == ParameterType.GetOrPost
                                      select string.Format("{0}={1}", Uri.EscapeDataString(p.Name), Uri.EscapeDataString(p.Value.ToString()));
                if (!queryParameters.Any())
                {
                    return uri;
                }
                else
                {
                    var path = uri.OriginalString.TrimEnd('/');
                    var query = string.Join("&", queryParameters);
                    return new Uri(path + "?" + query);
                }
            }
        }

        public static string NonBlankValueOf(string inputString)
        {
            if (String.IsNullOrEmpty(inputString))
                return "";
            else
                return inputString;
        }

        public static string GetElapsedTime(DateTime datetime)
        {
            TimeSpan ts = DateTime.Now.Subtract(datetime);
            DateTime date = DateTime.MinValue + ts;

            return ProcessPeriod(date.Year - 1, date.Month - 1, "year") ??
             ProcessPeriod(date.Month - 1, date.Day - 1, "month") ??
             ProcessPeriod(date.Day - 1, date.Hour, "day", "Yesterday") ??
             ProcessPeriod(date.Hour, date.Minute, "hour") ??
             ProcessPeriod(date.Minute, date.Second, "minute") ??
             ProcessPeriod(date.Second, 0, "second") ??
             "Right now";
        }

        public decimal CalculateArrears(string ApplicationId, string useremail, LubeBlendingDBEntities dbCtxt)
        {
            decimal amount = 0;
            decimal actualArrearAmount = 0;
            var NoArrearForExtraPayment = (from e in dbCtxt.ExtraPayments where e.ExtraPaymentAppRef == ApplicationId select e).FirstOrDefault();
            if (NoArrearForExtraPayment == null)
            {

                var capacity = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == ApplicationId && a.ApplicantUserId == useremail select new { a.AnnualCumuBaseOilRequirementCapacity }).ToList().LastOrDefault();
                var Lcode = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == ApplicationId select a.LicenseTypeId).FirstOrDefault();
                var appreq = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == ApplicationId select a).FirstOrDefault();
                var feesCode = appreq.LicenseTypeId + "_" + appreq.ApplicationTypeId + "_PROCESS_FEES";
                var exdate = (from a in dbCtxt.ApplicationRequests orderby a.LicenseExpiryDate descending where a.LicenseReference != null && a.LicenseTypeId == Lcode && a.SiteLocationAddress == appreq.SiteLocationAddress && a.ApplicantUserId == useremail select a.LicenseExpiryDate).FirstOrDefault();

                //var d = dbCtxt.ApplicationRequests.Where(a=>a.LicenseReference != null && a.LicenseTypeId == Lcode && a.SiteLocationAddress == appreq.SiteLocationAddress && a.ApplicantUserId == useremail).OrderByDescending(a=>a.LicenseExpiryDate).GroupBy(a => a.ApplicationId).Select(g => new { Name = g.Key, Time = g.Max(row => row.LicenseExpiryDate) });
                //var exdate = d.FirstOrDefault() == null? null: d.FirstOrDefault().Time;

                if (Lcode == "LTO")//LTO
                {
                    decimal fixamount = Convert.ToDecimal(dbCtxt.Configurations.Where(c => c.ParamID == feesCode).FirstOrDefault().ParamValue);

                    var val = Convert.ToInt64(Math.Round(Convert.ToDouble(capacity.AnnualCumuBaseOilRequirementCapacity)));

                    var licensecalamount = CalculateLicenseFee(appreq.LicenseTypeId, val);

                    amount = fixamount + licensecalamount;
                }

                else if (Lcode == "ATM" || Lcode == "ATO" || Lcode == "ATC" || Lcode == "SSA" || Lcode == "PTE")
                {
                    amount = Convert.ToDecimal(dbCtxt.Configurations.Where(c => c.ParamID == feesCode).FirstOrDefault().ParamValue);

                }

                if (exdate != null)
                {
                    var compareex = Convert.ToDateTime(exdate).Date;
                    var comparecurrentdate = DateTime.Now.Date;
                    if (comparecurrentdate > compareex)
                    {
                        var currentdate = DateTime.Now.Year;
                        var dateexpired = Convert.ToDateTime(exdate).Year;
                        var yearRange = currentdate - dateexpired;
                        var arrearRange = yearRange - 1;
                        var myamount = Convert.ToDecimal(amount);
                        var range = Convert.ToDecimal(arrearRange);
                        if (range >= 1)
                        {
                            actualArrearAmount = myamount * range;
                        }
                        else
                        {
                            actualArrearAmount = 0;
                        }
                    }

                }
            }
            return actualArrearAmount;
        }

        public static decimal CalculateLicenseFee(string licenseType, double cumulativeCapacity)
        {

            decimal AnnualStatutoryFee = 0;

            using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
            {

                var statfee = (from c in db.Configurations where c.ParamID == "LTO_STATUTORY_FEES" select c.ParamValue).FirstOrDefault();
                int statfees = Convert.ToInt32(statfee);

                if (licenseType.StartsWith("LTO"))
                {
                    double capacity = cumulativeCapacity;
                    int rate = (int)capacity / 20000;
                    int mod = (int)(capacity % 20000);
                    mod = (mod > 0) ? 1 : 0;
                    AnnualStatutoryFee = ((rate + mod) * statfees);
                }
                else
                {
                    AnnualStatutoryFee = 0;
                }
            }

            return AnnualStatutoryFee;
        }

        public static decimal CalculateLTOLFPFee(double cumulativeCapacity)
        {
            decimal AnnualStatutoryFee = 0;

            using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
            {
                var statfee = (from c in db.Configurations where c.ParamID == "LTOLFP_STATUTORY_FEES" select c.ParamValue).FirstOrDefault();

                int statfees = Convert.ToInt32(statfee);
                if (cumulativeCapacity >= 500000)
                {
                    double capacity = cumulativeCapacity;
                    int rate = (int)capacity / 500000;
                    int mod = (int)(capacity % 500000);
                    mod = (mod > 0) ? 1 : 0;
                    AnnualStatutoryFee = ((rate + mod) * statfees);
                }
                else
                {
                    AnnualStatutoryFee = statfees;
                }


            }

            return AnnualStatutoryFee;
        }

        public static decimal CalculateTPBAFee(string licenseType, double cumulativeCapacity)
        {
            decimal AnnualStatutoryFee = 0;

            using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
            {
                var statfee = licenseType == "TPBA-PLW" ? (from c in db.Configurations where c.ParamID == "TPBA-PLW_STATUTORY_FEES" select c.ParamValue).FirstOrDefault() : (from c in db.Configurations where c.ParamID == "TPBA-PRW_STATUTORY_FEES" select c.ParamValue).FirstOrDefault();

                int statfees = Convert.ToInt32(statfee);
                if (cumulativeCapacity >= 500000)
                {
                    double capacity = cumulativeCapacity;
                    int rate = (int)capacity / 500000;
                    int mod = (int)(capacity % 500000);
                    mod = (mod > 0) ? 1 : 0;
                    AnnualStatutoryFee = ((rate + mod) * statfees);
                }
                else
                {
                    AnnualStatutoryFee = statfees;
                }


            }

            return AnnualStatutoryFee;
        }

        public DateTime convertStringToDate(string dateString, string dateFormat)
        {
            return Convert.ToDateTime(dateString, CultureInfo.InvariantCulture);//DateTime.ParseExact(dateString, dateFormat, null);
        }

        public string GetLandTopologyNames(string landTopologyCodes)
        {
            List<string> landTopologyList = new List<string>();
            try
            {
                foreach (string typeId in landTopologyCodes.Split(','))
                {
                    landTopologyList.Add(dbCtxt.LandTopologyLookUps.Where(a => a.Code == typeId).FirstOrDefault().TopologyName);
                }
            }
            catch (Exception ex)
            {

            }
            return string.Join(",", landTopologyList.ToArray());
        }

        public string GetNatureOfAreaNames(string natureofAreaCodes)
        {
            List<string> natureOfAreaList = new List<string>();
            try
            {
                foreach (string typeId in natureofAreaCodes.Split(','))
                {
                    natureOfAreaList.Add(dbCtxt.NatureOfAreaLookUps.Where(a => a.AreaCode == typeId).FirstOrDefault().AreaName);
                }
            }
            catch (Exception ex)
            {

            }
            return string.Join(",", natureOfAreaList.ToArray());
        }

        public string GetLubePlantTypes(string lubePlantCodes)
        {
            List<string> lubePlantList = new List<string>();
            try
            {
                foreach (string typeId in lubePlantCodes.Split(','))
                {
                    lubePlantList.Add(dbCtxt.PlantTypeLookUps.Where(a => a.PlantID == Convert.ToInt16(typeId)).FirstOrDefault().Description);
                }
            }
            catch (Exception ex)
            {

            }
            return string.Join(",", lubePlantList.ToArray());
        }

        private static string ProcessPeriod(int value, int subValue, string name, string singularName = null)
        {
            if (value == 0)
            {
                return null;
            }
            if (value == 1)
            {
                if (!String.IsNullOrEmpty(singularName))
                {
                    return singularName;
                }
                string articleSuffix = name[0] == 'h' ? "n" : String.Empty;
                return subValue == 0 ? String.Format("A{0} {1} ago", articleSuffix, name) : String.Format("About a{0} {1} ago", articleSuffix, name);
            }
            return subValue == 0 ? String.Format("{0} {1}s ago", value, name) : String.Format("About {0} {1}s ago", value, name);
        }

        public bool isNewWorkFlowEnabled()
        {
            bool isNewworkflow = false;
            try
            {

                string cutOffdate = dbCtxt.Configurations.Where(c => c.ParamID == "NEW_WORKFLOW_CUTOFF").FirstOrDefault().ParamValue;
                DateTime newWorkFlowDate = DateTime.ParseExact(cutOffdate, "yyyy-MM-dd", null);

                if (DateTime.Today > newWorkFlowDate.Date)
                {
                    return true;
                }

            }
            catch (Exception ex)
            {

            }
            return isNewworkflow;
        }


        public bool isPaymentMade(string ApplicationId, out string errorMessage)
        {
            bool ispaymentmade = false;
            errorMessage = null;
            try
            {
                log.Info("About to Check If Payment is Made already");

                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ApplicationId).FirstOrDefault();
                if (appRequest !=
                 default(ApplicationRequest))
                {
                    if (!string.IsNullOrEmpty(appRequest.IsLegacy) && appRequest.IsLegacy == "YES")
                    {
                        return true;
                    }
                }

                log.Info("About to Check Payment Log with ID " + ApplicationId);
                //if(string.IsNullOrEmpty(appRequest.IsLegacy))
                PaymentLog paymentlog = dbCtxt.PaymentLogs.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();
                if (paymentlog !=
                 default(PaymentLog))
                {
                    if (paymentlog.Status == "AUTH")
                    {
                        ispaymentmade = true;
                    }
                }

                log.Info("Done with Payment Log");


            }
            catch (Exception ex)
            {
                errorMessage = "Error Occured Validating Payment, Please Try again Later";
                log.Info(ex.StackTrace);
            }

            log.Info("Is Payment Made =>" + ispaymentmade);

            return ispaymentmade;
        }

        public string GeneratePaymentReference(ElpServiceHelper serviceIntegrator, string ApplicationId, UserMaster userMaster, string callBackUrl)
        {
            string errorMessage = "SUCCESS";
            decimal totalCharge = 0, totalDue = 0;
            decimal processFeeAmt = 0, statutoryFeeAmt = 0, Arrears = 0;
            PaymentRequest paymentRequest = new PaymentRequest();
            decimal totalAmt = 0, servicecharge = 0;
            try
            {

                var Appreqid = (from e in dbCtxt.ExtraPayments where e.ExtraPaymentAppRef == ApplicationId select e.ApplicationID).FirstOrDefault();

                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault() == null ? dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == Appreqid.Trim()).FirstOrDefault() : dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();
                Arrears = CalculateArrears(ApplicationId, userMaster.UserId, dbCtxt);
                //FAIL WAS INTRODUCED SO AS TO ENSURE THAT ONLY ONE RECORD EXIST FOR PAYMENT (NO NARRATION), WE MIGHT REMOVE IT LATER
                PaymentLog paymentLog = dbCtxt.PaymentLogs.Where(c => c.ApplicationId == ApplicationId && (c.Status == "INIT" || c.Status == "AUTH" || c.Status == "FAIL")).FirstOrDefault();
                if (paymentLog != null)
                {
                    log.Info("RRR is Already Generated =>" + paymentLog.RRReference);
                    return paymentLog.RRReference;
                }

                errorMessage = GetApplicationFees(appRequest, out processFeeAmt, out statutoryFeeAmt);
                if (errorMessage == "SUCCESS")
                {
                    decimal processingfee = 0;
                    var extrapaycheck = (from e in dbCtxt.ExtraPayments where e.ExtraPaymentAppRef == ApplicationId && (e.Status == "Pending" || e.Status == "FAIL") select e).FirstOrDefault();
                    if (extrapaycheck == null)
                    {
                        processingfee = processFeeAmt;
                    }
                    if (extrapaycheck != null)
                    {
                        if ((appRequest.LicenseTypeId == "LTO" || appRequest.LicenseTypeId == "PTE" || appRequest.LicenseTypeId == "ATC") && (extrapaycheck.SanctionType == "INCOMPLETE FEE"))
                        {
                            totalDue = Convert.ToDecimal(extrapaycheck.TxnAmount);
                            totalAmt = Convert.ToDecimal(extrapaycheck.TxnAmount);

                            REMITA(totalAmt);
                        }
                        else
                        {
                            totalCharge = processingfee;
                            totalDue = Convert.ToDecimal(extrapaycheck.TxnAmount);
                            ApplicationId = extrapaycheck.ExtraPaymentAppRef;
                            servicecharge = Convert.ToDecimal(extrapaycheck.TxnAmount) * Convert.ToDecimal(0.05);
                            totalAmt = Convert.ToDecimal(extrapaycheck.TxnAmount) * Convert.ToDecimal(1.05);
                            var partner_charge_Amount = totalAmt * Convert.ToDecimal(0.1);
                            var IGRFee = totalAmt - partner_charge_Amount;
                            totalAmt = IGR_PARTNER(partner_charge_Amount, IGRFee);
                        }


                    }

                    else if (appRequest.LicenseTypeId == "ATM" || appRequest.LicenseTypeId == "ATO" || appRequest.LicenseTypeId == "ATC" || appRequest.LicenseTypeId == "SSA" || appRequest.LicenseTypeId == "PTE" || appRequest.LicenseTypeId == "LTO" || appRequest.LicenseTypeId.Contains("TPBA") || appRequest.LicenseTypeId == "TITA" || appRequest.LicenseTypeId == "TCA" || appRequest.LicenseTypeId == "ATCLFP" || appRequest.LicenseTypeId == "LTOLFP")
                    {
                        if (appRequest.LicenseTypeId == "LTO" || appRequest.LicenseTypeId == "PTE" || appRequest.LicenseTypeId == "ATC")
                        {
                            if (appRequest.LicenseTypeId == "LTO")
                            {
                                totalCharge = processingfee;
                                totalDue = statutoryFeeAmt;
                                totalAmt = Convert.ToDecimal(processingfee + statutoryFeeAmt + Arrears);
                            }
                            else
                            {
                                totalCharge = 0;
                                totalDue = processingfee;
                                totalAmt = Convert.ToDecimal(totalDue + Arrears);
                            }

                            REMITA(totalAmt);
                        }
                        else
                        {
                            if (appRequest.LicenseTypeId == "ATO")//Approval To Takeover 
                            {
                                totalCharge = 0;
                                totalDue = processingfee;
                                var approvalfee = Convert.ToInt32(dbCtxt.Configurations.Where(c => c.ParamID == "ATO_APPROVAL_FEES").FirstOrDefault().ParamValue);
                                servicecharge = Convert.ToDecimal(totalDue + Arrears + approvalfee) * Convert.ToDecimal(0.05);
                                totalAmt = Convert.ToDecimal(totalDue + Arrears + approvalfee) * Convert.ToDecimal(1.05);
                            }
                            else if (appRequest.LicenseTypeId.Contains("TPBA") || appRequest.LicenseTypeId == "LTOLFP")//Approval To Takeover 
                            {
                                totalCharge = processingfee;
                                totalDue = statutoryFeeAmt;
                                servicecharge = Convert.ToDecimal(processingfee + statutoryFeeAmt + Arrears) * Convert.ToDecimal(0.05);
                                totalAmt = Convert.ToDecimal(processingfee + statutoryFeeAmt + Arrears) * Convert.ToDecimal(1.05);
                            }
                            else
                            {
                                totalCharge = 0;
                                totalDue = processingfee;
                                servicecharge = Convert.ToDecimal(totalDue + Arrears) * Convert.ToDecimal(0.05);
                                totalAmt = Convert.ToDecimal(totalDue + Arrears) * Convert.ToDecimal(1.05);
                            }

                            decimal partner_charge_Amount = Convert.ToDecimal(totalAmt) * Convert.ToDecimal(0.1);
                            decimal IGRFee = Convert.ToDecimal(totalAmt) - Convert.ToDecimal(partner_charge_Amount);
                            totalAmt = IGR_PARTNER(partner_charge_Amount, IGRFee);

                        }



                    }




                    ElpsResponse elpsResponse1 = serviceIntegrator.GetCompanyDetailByEmail(userMaster.UserId);
                    CompanyDetail compDto = (CompanyDetail)elpsResponse1.value;
                    var statename = (from s in dbCtxt.StateMasterLists where s.StateCode == appRequest.StateCode select s.StateName).FirstOrDefault();
                    var field_zonal_office = (from f in dbCtxt.FieldLocations where f.StateLocated.Contains(appRequest.StateCode) select f.Description).FirstOrDefault();

                    values.Add("serviceTypeId", dbCtxt.Configurations.Where(c => c.ParamID == "SERVICE_TYPEID").FirstOrDefault().ParamValue);
                    values.Add("categoryName", "GENERAL");
                    values.Add("totalAmount", Decimal.ToInt32(totalAmt).ToString());
                    values.Add("amountDue", Decimal.ToInt32(totalAmt - servicecharge).ToString());
                    values.Add("payerName", userMaster.FirstName + " " + userMaster.LastName);
                    values.Add("payerEmail", userMaster.UserId);
                    values.Add("serviceCharge", Decimal.ToInt32(servicecharge).ToString());
                    values.Add("COMPANY BRANCH", appRequest.ApplicantName);
                    values.Add("FACILITY ADDRESS", appRequest.SiteLocationAddress);
                    values.Add("Field/Zonal Office", field_zonal_office);
                    values.Add("STATE", statename + " State");
                    values.Add("payerPhone", compDto.contact_Phone);
                    values.Add("orderId", ApplicationId);
                    values.Add("returnSuccessUrl", callBackUrl + "Company/PaymentSuccess/" + ApplicationId);
                    values.Add("returnFailureUrl", callBackUrl + "Company/PaymentSuccess/" + ApplicationId);
                    values.Add("returnBankPaymentUrl", callBackUrl + "Company/PaymentSuccess/" + ApplicationId);

                    JArray appItems = new JArray();
                    JObject appItem1 = new JObject();

                    appItem1.Add("name", "Lubricant Oil Blending Plant");
                    appItem1.Add("description", "Payment for Lubricant Oil Blending Plant");
                    appItem1.Add("group", "LUBE");
                    appItems.Add(appItem1);
                    values.Add("applicationItems", appItems);


                    ElpsResponse elpsResponse = serviceIntegrator.GetPaymentDetails(values, userMaster.ElpsId);
                    log.Info("Response from Elps =>" + elpsResponse.message);

                    if (elpsResponse.message != "SUCCESS")
                    {
                        return elpsResponse.message;
                    }
                    PaymentResponse paymentResponse = (PaymentResponse)elpsResponse.value;

                    log.Info("PaymentResponse RRR " + paymentResponse.rrr);
                    log.Info("PaymentResponse Status " + paymentResponse.status);
                    log.Info("PaymentResponse transactionId " + paymentResponse.transactionId);
                    log.Info("PaymentResponse AppId " + paymentResponse.appId);

                    if (paymentResponse.rrr == null || paymentResponse.transactionId == null || paymentResponse.appId == null)
                    {
                        return "Remita Retrieval Reference (RRR) Fails to Generate, Please Try again";
                    }

                    //lets create a new record in the payment tranasaction log
                    var paymentref = extrapaycheck != null ? extrapaycheck.ExtraPaymentAppRef : appRequest.ApplicationId;
                    var checkifexist = (from p in dbCtxt.PaymentLogs where p.ApplicationId == paymentref select p).FirstOrDefault();
                    paymentLog = new PaymentLog();
                    paymentLog.ApplicationId = paymentref;
                    paymentLog.TransactionDate = DateTime.UtcNow;
                    paymentLog.TransactionID = paymentResponse.transactionId;
                    paymentLog.LicenseTypeId = appRequest.LicenseTypeId;
                    paymentLog.ApplicantUserId = userMaster.UserId;
                    paymentLog.RRReference = paymentResponse.rrr;
                    paymentLog.AppReceiptID = paymentResponse.appId;
                    paymentLog.TxnAmount = totalAmt;
                    paymentLog.Arrears = Arrears;
                    paymentLog.StatutoryFee = statutoryFeeAmt;
                    paymentLog.ProcessingFee = processFeeAmt;
                    paymentLog.ServiceCharge = servicecharge;
                    paymentLog.Account = (appRequest.LicenseTypeId == "LTO" || appRequest.LicenseTypeId == "PTE" || appRequest.LicenseTypeId == "ATC") ? dbCtxt.Configurations.Where(c => c.ParamID == "ACCOUNT").FirstOrDefault().ParamValue : dbCtxt.Configurations.Where(c => c.ParamID == "ACCOUNT_IGR").FirstOrDefault().ParamValue;
                    paymentLog.BankCode = (appRequest.LicenseTypeId == "LTO" || appRequest.LicenseTypeId == "PTE" || appRequest.LicenseTypeId == "ATC") ? dbCtxt.Configurations.Where(c => c.ParamID == "BANKCODE").FirstOrDefault().ParamValue : dbCtxt.Configurations.Where(c => c.ParamID == "BANKCODE_IGR").FirstOrDefault().ParamValue;
                    paymentLog.RetryCount = 0;
                    paymentLog.Status = "INIT";
                    paymentLog.LastRetryDate = DateTime.UtcNow;

                    log.Info("About to Add Payment Log");
                    if (checkifexist == null)
                    {
                        dbCtxt.PaymentLogs.Add(paymentLog);
                    }

                    log.Info("Added Payment Log to Table");

                    if (extrapaycheck != null)
                    {
                        extrapaycheck.RRReference = paymentResponse.rrr;
                        extrapaycheck.TransactionDate = DateTime.UtcNow;
                        extrapaycheck.TransactionID = paymentResponse.transactionId;
                        extrapaycheck.AppReceiptID = paymentResponse.appId;
                        extrapaycheck.StatutoryFee = statutoryFeeAmt;
                        extrapaycheck.ProcessingFee = processFeeAmt;
                        extrapaycheck.ServiceCharge = servicecharge;
                        extrapaycheck.Account = (extrapaycheck?.LicenseTypeCode == "LTO" || extrapaycheck?.LicenseTypeCode == "PTE" || extrapaycheck?.LicenseTypeCode == "ATC") ? dbCtxt.Configurations.Where(c => c.ParamID == "ACCOUNT").FirstOrDefault().ParamValue : dbCtxt.Configurations.Where(c => c.ParamID == "ACCOUNT_IGR").FirstOrDefault().ParamValue;
                        extrapaycheck.BankCode = (extrapaycheck?.LicenseTypeCode == "LTO" || extrapaycheck?.LicenseTypeCode == "PTE" || extrapaycheck?.LicenseTypeCode == "ATC") ? dbCtxt.Configurations.Where(c => c.ParamID == "BANKCODE").FirstOrDefault().ParamValue : dbCtxt.Configurations.Where(c => c.ParamID == "BANKCODE_IGR").FirstOrDefault().ParamValue;
                    }



                    dbCtxt.SaveChanges();
                    log.Info("Saved it Successfully");
                    return paymentResponse.rrr;

                }


            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                return "An Error Occured Generating Payment Reference, Pls Try again Later";
            }

            return null;
        }

        public decimal IGR_PARTNER(decimal partner, decimal igr)
        {

            
            totalAmt = Decimal.ToInt32(igr) + Decimal.ToInt32(partner);
            JArray lineItems = new JArray();
            JObject lineItem1 = new JObject();
            lineItem1.Add("lineItemsId", "1");
            lineItem1.Add("beneficiaryName", "Beneficiary 2");
            lineItem1.Add("beneficiaryAccount", dbCtxt.Configurations.Where(c => c.ParamID == "ACCOUNT_IGR").FirstOrDefault().ParamValue);
            lineItem1.Add("beneficiaryAmount", Decimal.ToInt32(igr).ToString());
            lineItem1.Add("bankCode", dbCtxt.Configurations.Where(c => c.ParamID == "BANKCODE_IGR").FirstOrDefault().ParamValue);
            lineItem1.Add("deductFeeFrom", "2");
            lineItems.Add(lineItem1);

            JObject lineItem3 = new JObject();
            lineItem3.Add("lineItemsId", "2");
            lineItem3.Add("beneficiaryName", "Beneficiary 3");
            lineItem3.Add("beneficiaryAccount", dbCtxt.Configurations.Where(c => c.ParamID == "ACCOUNT_PARTNER").FirstOrDefault().ParamValue);
            lineItem3.Add("beneficiaryAmount", Decimal.ToInt32(partner).ToString());
            lineItem3.Add("bankCode", dbCtxt.Configurations.Where(c => c.ParamID == "BANKCODE_PARTNER").FirstOrDefault().ParamValue);
            lineItem3.Add("deductFeeFrom", "1");
            lineItems.Add(lineItem3);
            values.Add("lineItems", lineItems);

            return totalAmt;
        }

        public void REMITA(decimal totalamt)
        {
            JArray lineItems = new JArray();
            JObject lineItem2 = new JObject();
            lineItem2.Add("lineItemsId", "1");
            lineItem2.Add("beneficiaryName", "Beneficiary 1");
            lineItem2.Add("beneficiaryAccount", dbCtxt.Configurations.Where(c => c.ParamID == "ACCOUNT").FirstOrDefault().ParamValue);
            lineItem2.Add("beneficiaryAmount", Decimal.ToInt32(totalamt).ToString());
            lineItem2.Add("bankCode", dbCtxt.Configurations.Where(c => c.ParamID == "BANKCODE").FirstOrDefault().ParamValue);
            lineItem2.Add("deductFeeFrom", "1");
            lineItems.Add(lineItem2);
            values.Add("lineItems", lineItems);
        }

        public static String ConvertToWords(String numb)
        {
            String val = "", wholeNo = numb, points = "", andStr = "", pointStr = "";
            String endStr = "Only";
            try
            {
                int decimalPlace = numb.IndexOf(".");
                if (decimalPlace > 0)
                {
                    wholeNo = numb.Substring(0, decimalPlace);
                    points = numb.Substring(decimalPlace + 1);
                    if (Convert.ToInt32(points) > 0)
                    {
                        andStr = "and"; // just to separate whole numbers from points/cents  
                        endStr = "Kobo " + endStr; //Cents  
                        pointStr = ConvertDecimals(points);
                    }
                }
                val = String.Format("{0} {1}{2} {3}", ConvertWholeNumber(wholeNo).Trim(), andStr, pointStr, endStr);
            }
            catch { }
            return val;
        }

        private static String ConvertWholeNumber(String Number)
        {
            string word = "";
            try
            {
                bool beginsZero = false; //tests for 0XX
                bool isDone = false; //test if already translated
                double dblAmt = (Convert.ToDouble(Number));
                //if ((dblAmt > 0) && number.StartsWith("0"))
                if (dblAmt > 0)
                { //test for zero or digit zero in a nuemric
                    beginsZero = Number.StartsWith("0");

                    int numDigits = Number.Length;
                    int pos = 0; //store digit grouping
                    String place = ""; //digit grouping name:hundres,thousand,etc...
                    switch (numDigits)
                    {
                        case 1: //ones' range

                            word = ones(Number);
                            isDone = true;
                            break;
                        case 2: //tens' range
                            word = tens(Number);
                            isDone = true;
                            break;
                        case 3: //hundreds' range
                            pos = (numDigits % 3) + 1;
                            place = " Hundred ";
                            break;
                        case 4: //thousands' range
                        case 5:
                        case 6:
                            pos = (numDigits % 4) + 1;
                            place = " Thousand ";
                            break;
                        case 7: //millions' range
                        case 8:
                        case 9:
                            pos = (numDigits % 7) + 1;
                            place = " Million ";
                            break;
                        case 10: //Billions's range
                        case 11:
                        case 12:

                            pos = (numDigits % 10) + 1;
                            place = " Billion ";
                            break;
                        //add extra case options for anything above Billion...
                        default:
                            isDone = true;
                            break;
                    }
                    if (!isDone)
                    { //if transalation is not done, continue...(Recursion comes in now!!)
                        if (Number.Substring(0, pos) != "0" && Number.Substring(pos) != "0")
                        {
                            try
                            {
                                word = ConvertWholeNumber(Number.Substring(0, pos)) + place + ConvertWholeNumber(Number.Substring(pos));
                            }
                            catch { }
                        }
                        else
                        {
                            word = ConvertWholeNumber(Number.Substring(0, pos)) + ConvertWholeNumber(Number.Substring(pos));
                        }

                        //check for trailing zeros
                        //if (beginsZero) word = " and " + word.Trim();
                    }
                    //ignore digit grouping names
                    if (word.Trim().Equals(place.Trim())) word = "";
                }
            }
            catch { }
            return word.Trim();
        }

        private static String tens(String Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = null;
            switch (_Number)
            {
                case 10:
                    name = "Ten";
                    break;
                case 11:
                    name = "Eleven";
                    break;
                case 12:
                    name = "Twelve";
                    break;
                case 13:
                    name = "Thirteen";
                    break;
                case 14:
                    name = "Fourteen";
                    break;
                case 15:
                    name = "Fifteen";
                    break;
                case 16:
                    name = "Sixteen";
                    break;
                case 17:
                    name = "Seventeen";
                    break;
                case 18:
                    name = "Eighteen";
                    break;
                case 19:
                    name = "Nineteen";
                    break;
                case 20:
                    name = "Twenty";
                    break;
                case 30:
                    name = "Thirty";
                    break;
                case 40:
                    name = "Fourty";
                    break;
                case 50:
                    name = "Fifty";
                    break;
                case 60:
                    name = "Sixty";
                    break;
                case 70:
                    name = "Seventy";
                    break;
                case 80:
                    name = "Eighty";
                    break;
                case 90:
                    name = "Ninety";
                    break;
                default:
                    if (_Number > 0)
                    {
                        name = tens(Number.Substring(0, 1) + "0") + " " + ones(Number.Substring(1));
                    }
                    break;
            }
            return name;
        }

        private static String ones(String Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = "";
            switch (_Number)
            {

                case 1:
                    name = "One";
                    break;
                case 2:
                    name = "Two";
                    break;
                case 3:
                    name = "Three";
                    break;
                case 4:
                    name = "Four";
                    break;
                case 5:
                    name = "Five";
                    break;
                case 6:
                    name = "Six";
                    break;
                case 7:
                    name = "Seven";
                    break;
                case 8:
                    name = "Eight";
                    break;
                case 9:
                    name = "Nine";
                    break;
            }
            return name;
        }

        private static String ConvertDecimals(String number)
        {
            String cd = "", digit = "", engOne = "";
            for (int i = 0; i < number.Length; i++)
            {
                digit = number[i].ToString();
                if (digit.Equals("0"))
                {
                    engOne = "Zero";
                }
                else
                {
                    engOne = ones(digit);
                }
                cd += " " + engOne;
            }
            return cd;
        }

        public string NumberToWords(long number)
        {
            if (number == 0)
                return "zero";
            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";
            if ((number / 1000000000) > 0)
            {
                words += NumberToWords(number / 1000000000) + " billion ";
                number %= 1000000000;
            }
            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }

        private static Byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public Byte[] GenerateQR(string url)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            var imageResult = BitmapToBytes(qrCodeImage);
            return imageResult;
        }
    }
}