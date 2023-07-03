using log4net;
using LOBP.DbEntities;
using LOBP.Models;
using LOBP.Helper;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rotativa;
using System.Globalization;

namespace LOBP.Helper
{
    public class LicenseHelper
    {

        private ILog log = log4net.LogManager.GetLogger(typeof(UtilityHelper));
        private UtilityHelper commonhelper = new UtilityHelper();
        ElpsResponse wrapper = new ElpsResponse();
        List<PermitModels> permitmodel = new List<PermitModels>();
        List<Permitmodel> permits = new List<Permitmodel>();
        string DayOfWeek = Convert.ToString(DateTime.UtcNow.DayOfWeek);
        int Day = Convert.ToInt32(DateTime.UtcNow.Day);
        string Year = Convert.ToString(DateTime.UtcNow.Year);




        public static class StringExt
        {
            public static IEnumerable<string> SmartSplit(string input, int maxLength)
            {
                int i = 0;
                while (i + maxLength < input.Length)
                {
                    int index = input.LastIndexOf(' ', i + maxLength);
                    if (index <= 0) //if word length > maxLength.
                    {
                        index = maxLength;
                    }
                    yield return input.Substring(i, index - i);

                    i = index + 1;
                }

                yield return input.Substring(i);
            }
            public static List<string> Truncate(string value, int maxLength)
            {
                List<string> valList = new List<string>();

                if (string.IsNullOrEmpty(value))
                {
                    valList.Add(" ");
                }
                else if (value.Length <= maxLength)
                {
                    valList.Add(value);
                }
                else
                {
                    valList.Add(value.Substring(0, maxLength));
                    valList.Add(value.Substring(maxLength));
                }

                return valList;
            }
            public static IEnumerable<String> TruncateAtWord(double partSize, string input)
            {
                int k = 0;
                var output = input.ToLookup(c => Math.Floor(k++ / partSize)).Select(e => new String(e.ToArray())).ToList();
                return output;
            }

        }
        public static class DateExt
        {
            public static string Truncate(string value, int maxLength)
            {
                if (string.IsNullOrEmpty(value)) return value;
                return value.Length <= maxLength ? value : value.Substring(0, maxLength);
            }
        }

        public ElpsResponse Postmypermit(string permitNO, string OrderID, string ElpsCompID, DateTime issuedDate, DateTime expiryDate, string Appname, string isRenew, int? permitID)
        {
            ElpServiceHelper serviceIntegrator = new ElpServiceHelper();


            var values = new JObject();
            values.Add("permit_No", permitNO);
            values.Add("orderId", OrderID);
            values.Add("company_Id", ElpsCompID);
            values.Add("date_Issued", issuedDate.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
            values.Add("date_Expire", expiryDate.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
            values.Add("categoryName", "Lube Oil Blending Plant");
            values.Add("is_Renewed", isRenew);
            values.Add("licenseId", permitID.ToString());
            values.Add("id", 0);

           
            wrapper = serviceIntegrator.PostPermit(ElpsCompID, values);

            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                var UpdatepermitElpsID = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == OrderID select a).FirstOrDefault();
                UpdatepermitElpsID.PermitElpsId = Convert.ToInt32(wrapper.value);
                dbCtxt.SaveChanges();
            }
            return null;
        }


        public void GenerateLicense(string applicationId, string staffemail)
        {

            using (LubeBlendingDBEntities dbCtxt1 = new LubeBlendingDBEntities())
            {
                decimal processFees = 0;
                decimal statutoryFees = 0;
                String errorMessage = null;
                DateTime currentDate = DateTime.UtcNow;
                ApplicationDocHelper appDocHelper = new ApplicationDocHelper(dbCtxt1);

                try
                {
                    log.Info("About to generate License");
                    var Signatureid = (from u in dbCtxt1.UserMasters where u.UserId == staffemail select u.SignatureID).FirstOrDefault();
                    ApplicationRequest appRequest = dbCtxt1.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                    var addedyear = Convert.ToDateTime(appRequest.AddedDate).Year;
                    var currentyear = DateTime.Now.Year;

                    string expiryDate = addedyear == currentyear? currentDate.ToString("yyyy") + "-12-31": addedyear + "-12-31";
                    string IssueDate = addedyear == currentyear ? Convert.ToString(DateTime.Now.Year + "-"+DateTime.Now.Month+ "-" + DateTime.Now.Day) : addedyear + "-12-31";

                    var ElpsId = (from u in dbCtxt1.UserMasters where u.UserId == appRequest.ApplicantUserId select u.ElpsId).FirstOrDefault();
                    var isRenewed = appRequest.ApplicationTypeId == "NEW" ? "NO" : "YES";
                    var LicenseId = (from l in dbCtxt1.LicenseTypes where l.LicenseTypeId == appRequest.LicenseTypeId select l.LicenseSerial).FirstOrDefault();
                    appRequest.LicenseIssuedDate = Convert.ToDateTime(IssueDate);
                    appRequest.LicenseExpiryDate = commonhelper.convertStringToDate(expiryDate, "yyyy-MM-dd");
                    log.Info("Here");
                     string LicenseRef = appDocHelper.GenerateLicenseNumber(dbCtxt1, appRequest.LicenseTypeId);
                    log.Info("Generated License Num => " + LicenseRef);
                    appRequest.LicenseReference =  LicenseRef;
                    appRequest.SignatureID = Convert.ToInt32(Signatureid);
                    errorMessage = commonhelper.GetApplicationFees(appRequest, out processFees, out statutoryFees);
                    log.Info("errorMessage From Fees => " + errorMessage);
                    log.Info("LicenseTypeCode => " + appRequest.LicenseTypeId);
                    dbCtxt1.SaveChanges();
                    //commonhelper.convertStringToDate(expiryDate, "yyyy-MM-dd")
                    Postmypermit(LicenseRef, applicationId, ElpsId, currentDate, DateTime.ParseExact(expiryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture), "Lube Oil Blending Plant", isRenewed, LicenseId);
                }
                catch (Exception ex)
                {
                    log.Error(ex.InnerException);
                }
            }
        }


        public void GenerateLTOLFP(string applicationId, string staffemail)
        {

            using (LubeBlendingDBEntities dbCtxt1 = new LubeBlendingDBEntities())
            {
                decimal processFees = 0;
                decimal statutoryFees = 0;
                String errorMessage = null;
                DateTime currentDate = DateTime.UtcNow;
                ApplicationDocHelper appDocHelper = new ApplicationDocHelper(dbCtxt1);

                try
                {
                    log.Info("About to generate License");
                    var Signatureid = (from u in dbCtxt1.UserMasters where u.UserId == staffemail select u.SignatureID).FirstOrDefault();
                    ApplicationRequest appRequest = dbCtxt1.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                    var addedyear = Convert.ToDateTime(appRequest.AddedDate).Year;
                    var currentyear = DateTime.Now.Year;

                    string expiryDate = addedyear == currentyear ? currentDate.ToString("yyyy") + "-12-31" : addedyear + "-12-31";
                    string IssueDate = addedyear == currentyear ? Convert.ToString(DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day) : addedyear + "-12-31";

                    var ElpsId = (from u in dbCtxt1.UserMasters where u.UserId == appRequest.ApplicantUserId select u.ElpsId).FirstOrDefault();
                    var isRenewed = appRequest.ApplicationTypeId == "NEW" ? "NO" : "YES";
                    var LicenseId = (from l in dbCtxt1.LicenseTypes where l.LicenseTypeId == appRequest.LicenseTypeId select l.LicenseSerial).FirstOrDefault();
                    appRequest.LicenseIssuedDate = Convert.ToDateTime(IssueDate);
                    appRequest.LicenseExpiryDate = commonhelper.convertStringToDate(expiryDate, "yyyy-MM-dd");
                    log.Info("Here");
                    string LicenseRef = appDocHelper.GenerateLicenseNumber(dbCtxt1, appRequest.LicenseTypeId);
                    log.Info("Generated License Num => " + LicenseRef);
                    appRequest.LicenseReference = LicenseRef;
                    appRequest.SignatureID = Convert.ToInt32(Signatureid);
                    errorMessage = commonhelper.GetApplicationFees(appRequest, out processFees, out statutoryFees);
                    log.Info("errorMessage From Fees => " + errorMessage);
                    log.Info("LicenseTypeCode => " + appRequest.LicenseTypeId);
                    dbCtxt1.SaveChanges();
                    //commonhelper.convertStringToDate(expiryDate, "yyyy-MM-dd")
                    Postmypermit(LicenseRef, applicationId, ElpsId, currentDate, DateTime.ParseExact(expiryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture), "Lubricant Filling Plant", isRenewed, LicenseId);
                }
                catch (Exception ex)
                {
                    log.Error(ex.InnerException);
                }
            }
        }


        public void GenerateSuitabilityLetter(string applicationId, string staffemail)
        {
            try
            {
                using (LubeBlendingDBEntities dbCtxt1 = new LubeBlendingDBEntities())
                {
                    ApplicationDocHelper appDocHelper = new ApplicationDocHelper(dbCtxt1);

                    ApplicationRequest appRequest = dbCtxt1.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                    var fieldofficeaddress = (from a in dbCtxt1.ApplicationRequests join s in dbCtxt1.StateMasterLists on a.StateCode equals s.StateCode where applicationId == a.ApplicationId select s.StateAddress).FirstOrDefault();
                    var ElpsId = (from u in dbCtxt1.UserMasters where u.UserId == appRequest.ApplicantUserId select u.ElpsId).FirstOrDefault();
                    var isRenewed = appRequest.ApplicationTypeId == "NEW" ? "NO" : "YES";
                    var LicenseId = (from l in dbCtxt1.LicenseTypes where l.LicenseTypeId == appRequest.LicenseTypeId select l.LicenseSerial).FirstOrDefault();
                    string LicenseRef = appDocHelper.GenerateLicenseNumber(dbCtxt1, appRequest.LicenseTypeId); 
                    var Signatureid = (from u in dbCtxt1.UserMasters where u.UserId == staffemail select u.SignatureID).FirstOrDefault();
                    var Issueddate = DateTime.UtcNow;
                    //var DateIssued = d.ToString("dd-MM-yyyy");
                    //var Issueddate = commonhelper.convertStringToDate(DateIssued, "dd-MM-yyyy");

                    var expirydate = Issueddate.AddMonths(6);
                    //var ex = expirydate.ToString("dd-MM-yyyy");
                    //var expireddate = commonhelper.convertStringToDate(ex, "dd-MM-yyyy");
                    string issuedletterdate = Convert.ToString(appRequest.LicenseIssuedDate);
                    if (appRequest != null)
                    {
                        appRequest.LicenseReference =  LicenseRef;
                        appRequest.LicenseIssuedDate = Issueddate;
                        appRequest.LicenseExpiryDate = expirydate;
                        appRequest.SignatureID = Convert.ToInt32(Signatureid);
                    }
                    dbCtxt1.SaveChanges();
                    Postmypermit(LicenseRef, applicationId, ElpsId, Issueddate, expirydate, "Lube Oil Blending Plant", isRenewed, LicenseId);

                }
            }
            catch (Exception ex)
            {
                log.Error(ex.InnerException);
            }

        }




        public void GeneratePTELetter(string applicationId, string staffemail)
        {
            try
            {
                using (LubeBlendingDBEntities dbCtxt1 = new LubeBlendingDBEntities())
                {
                    ApplicationDocHelper appDocHelper = new ApplicationDocHelper(dbCtxt1);

                    ApplicationRequest appRequest = dbCtxt1.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                    var fieldofficeaddress = (from a in dbCtxt1.ApplicationRequests join s in dbCtxt1.StateMasterLists on a.StateCode equals s.StateCode where applicationId == a.ApplicationId select s.StateAddress).FirstOrDefault();
                    var ElpsId = (from u in dbCtxt1.UserMasters where u.UserId == appRequest.ApplicantUserId select u.ElpsId).FirstOrDefault();
                    var isRenewed = appRequest.ApplicationTypeId == "NEW" ? "NO" : "YES";
                    var LicenseId = (from l in dbCtxt1.LicenseTypes where l.LicenseTypeId == appRequest.LicenseTypeId select l.LicenseSerial).FirstOrDefault();
                    string LicenseRef = appDocHelper.GenerateLicenseNumber(dbCtxt1, appRequest.LicenseTypeId);
                    var Signatureid = (from u in dbCtxt1.UserMasters where u.UserId == staffemail select u.SignatureID).FirstOrDefault();
                    var Issueddate = DateTime.UtcNow;
                    //var DateIssued = d.ToString("dd-MM-yyyy");
                    //var Issueddate = commonhelper.convertStringToDate(DateIssued, "dd-MM-yyyy");

                    var expirydate = Issueddate.AddMonths(3);
                    //var ex = expirydate.ToString("dd-MM-yyyy");
                    //var expireddate = commonhelper.convertStringToDate(ex, "dd-MM-yyyy");
                    string issuedletterdate = Convert.ToString(appRequest.LicenseIssuedDate);
                    if (appRequest != null)
                    {
                        appRequest.LicenseReference = LicenseRef;
                        appRequest.LicenseIssuedDate = Issueddate;
                        appRequest.LicenseExpiryDate = expirydate;
                        appRequest.SignatureID = Convert.ToInt32(Signatureid);
                    }
                    dbCtxt1.SaveChanges();
                    Postmypermit(LicenseRef, applicationId, ElpsId, Issueddate, expirydate, "Lube Oil Blending Plant", isRenewed, LicenseId);

                }
            }
            catch (Exception ex)
            {
                log.Error(ex.InnerException);
            }

        }


        public void GeneratePressureTestRefNo(string applicationId, string staffemail)
        {
            try
            {
                using (LubeBlendingDBEntities dbCtxt1 = new LubeBlendingDBEntities())
                {
                    ApplicationDocHelper appDocHelper = new ApplicationDocHelper(dbCtxt1);

                    ApplicationRequest appRequest = dbCtxt1.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                    var ElpsId = (from u in dbCtxt1.UserMasters where u.UserId == appRequest.ApplicantUserId select u.ElpsId).FirstOrDefault();
                    var isRenewed = appRequest.ApplicationTypeId == "NEW" ? "NO" : "YES";
                    var LicenseId = (from l in dbCtxt1.LicenseTypes where l.LicenseTypeId == appRequest.LicenseTypeId select l.LicenseSerial).FirstOrDefault();
                    string LicenseRef = appDocHelper.GenerateLicenseNumber(dbCtxt1, appRequest.LicenseTypeId);
                    var Signatureid = (from u in dbCtxt1.UserMasters where u.UserId == staffemail select u.SignatureID).FirstOrDefault();
                    var d = DateTime.UtcNow;
                    var DateIssued = d.ToString("dd-MM-yyyy");
                    var Issueddate = commonhelper.convertStringToDate(DateIssued, "dd-MM-yyyy");

                    var expirydate = d.AddMonths(30);
                    var ex = expirydate.ToString("dd-MM-yyyy");
                    var expireddate = commonhelper.convertStringToDate(ex, "dd-MM-yyyy");
                    string issuedletterdate = Convert.ToString(appRequest.LicenseIssuedDate);
                    if (appRequest != null)
                    {
                        appRequest.LicenseReference =LicenseRef;
                        appRequest.LicenseIssuedDate =Issueddate;
                        appRequest.LicenseExpiryDate =expireddate;
                        appRequest.SignatureID = Convert.ToInt32(Signatureid);
                    }
                    dbCtxt1.SaveChanges();
                    Postmypermit(LicenseRef, applicationId, ElpsId, Issueddate, expireddate, "Lube Oil Blending Plant", isRenewed, LicenseId);

                }
            }
            catch (Exception ex)
            {
                log.Error(ex.InnerException);
            }

        }
        public void GenerateATCLetter(string applicationId, string staffemail)
        {
            try
            {
                using (LubeBlendingDBEntities dbCtxt1 = new LubeBlendingDBEntities())
                {
                    ApplicationDocHelper appDocHelper = new ApplicationDocHelper(dbCtxt1);

                    ApplicationRequest appRequest = dbCtxt1.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                    var fieldofficeaddress = (from a in dbCtxt1.ApplicationRequests join s in dbCtxt1.StateMasterLists on a.StateCode equals s.StateCode where applicationId == a.ApplicationId select s.StateAddress).FirstOrDefault();
                    var ElpsId = (from u in dbCtxt1.UserMasters where u.UserId == appRequest.ApplicantUserId select u.ElpsId).FirstOrDefault();
                    var isRenewed = appRequest.ApplicationTypeId == "NEW" ? "NO" : "YES";
                    var LicenseId = (from l in dbCtxt1.LicenseTypes where l.LicenseTypeId == appRequest.LicenseTypeId select l.LicenseSerial).FirstOrDefault();
                    DateTime currentDateTime = DateTime.UtcNow;
                    var Signatureid = (from u in dbCtxt1.UserMasters where u.UserId == staffemail select u.SignatureID).FirstOrDefault();
                    string LicenseRef = appDocHelper.GenerateLicenseNumber(dbCtxt1, appRequest.LicenseTypeId);
                    var expiryd = currentDateTime.AddYears(1);
                    
                    if (appRequest != null)
                    {
                        appRequest.LicenseReference = LicenseRef;
                        appRequest.LicenseIssuedDate = DateTime.UtcNow;
                        appRequest.LicenseExpiryDate = expiryd; 
                        appRequest.SignatureID = Convert.ToInt32(Signatureid);
                    }
                    dbCtxt1.SaveChanges();
                    Postmypermit(LicenseRef, applicationId, ElpsId, DateTime.UtcNow, expiryd, "Lube Oil Blending Plant", isRenewed, LicenseId);

                }

            }

            catch (Exception ex)
            {
                log.Error(ex.InnerException);
            }
        }


        public void GenerateATCLFPLetter(string applicationId, string staffemail)
        {
            try
            {
                using (LubeBlendingDBEntities dbCtxt1 = new LubeBlendingDBEntities())
                {
                    ApplicationDocHelper appDocHelper = new ApplicationDocHelper(dbCtxt1);

                    ApplicationRequest appRequest = dbCtxt1.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                    var fieldofficeaddress = (from a in dbCtxt1.ApplicationRequests join s in dbCtxt1.StateMasterLists on a.StateCode equals s.StateCode where applicationId == a.ApplicationId select s.StateAddress).FirstOrDefault();
                    var ElpsId = (from u in dbCtxt1.UserMasters where u.UserId == appRequest.ApplicantUserId select u.ElpsId).FirstOrDefault();
                    var isRenewed = appRequest.ApplicationTypeId == "NEW" ? "NO" : "YES";
                    var LicenseId = (from l in dbCtxt1.LicenseTypes where l.LicenseTypeId == appRequest.LicenseTypeId select l.LicenseSerial).FirstOrDefault();
                    DateTime currentDateTime = DateTime.UtcNow;
                    var Signatureid = (from u in dbCtxt1.UserMasters where u.UserId == staffemail select u.SignatureID).FirstOrDefault();
                    string LicenseRef = appDocHelper.GenerateLicenseNumber(dbCtxt1, appRequest.LicenseTypeId);
                    var expiryd = currentDateTime.AddYears(1);

                    if (appRequest != null)
                    {
                        appRequest.LicenseReference = LicenseRef;
                        appRequest.LicenseIssuedDate = DateTime.UtcNow;
                        appRequest.LicenseExpiryDate = expiryd;
                        appRequest.SignatureID = Convert.ToInt32(Signatureid);
                    }
                    dbCtxt1.SaveChanges();
                    Postmypermit(LicenseRef, applicationId, ElpsId, DateTime.UtcNow, expiryd, "Lubricant Filling Plant", isRenewed, LicenseId);

                }

            }

            catch (Exception ex)
            {
                log.Error(ex.InnerException);
            }
        }









        public void GenerateTPBALetter(string applicationId, string staffemail)
        {
            try
            {
                using (LubeBlendingDBEntities dbCtxt1 = new LubeBlendingDBEntities())
                {
                    string expiryDate;
                    ApplicationDocHelper appDocHelper = new ApplicationDocHelper(dbCtxt1);

                    ApplicationRequest appRequest = dbCtxt1.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                    var fieldofficeaddress = (from a in dbCtxt1.ApplicationRequests join s in dbCtxt1.StateMasterLists on a.StateCode equals s.StateCode where applicationId == a.ApplicationId select s.StateAddress).FirstOrDefault();
                    var ElpsId = (from u in dbCtxt1.UserMasters where u.UserId == appRequest.ApplicantUserId select u.ElpsId).FirstOrDefault();
                    var isRenewed = appRequest.ApplicationTypeId == "NEW" ? "NO" : "YES";
                    var LicenseId = (from l in dbCtxt1.LicenseTypes where l.LicenseTypeId == appRequest.LicenseTypeId select l.LicenseSerial).FirstOrDefault();
                    DateTime currentDateTime = DateTime.UtcNow;
                    var Signatureid = (from u in dbCtxt1.UserMasters where u.UserId == staffemail select u.SignatureID).FirstOrDefault();
                    string LicenseRef = appDocHelper.GenerateLicenseNumber(dbCtxt1, appRequest.LicenseTypeId);
                    expiryDate = Convert.ToString(currentDateTime.AddMonths(3).Year + "-" + currentDateTime.AddMonths(3).Month + "-" + currentDateTime.AddMonths(3).Day);
                    var currentyear = DateTime.Now.Year;
                    if (appRequest.Quarter == 2)
                    {
                         expiryDate = currentDateTime.AddMonths(6).Year == currentyear ? Convert.ToString(currentDateTime.AddMonths(6).Year+"-"+currentDateTime.AddMonths(6).Month+"-"+ currentDateTime.AddMonths(6).Day) : currentyear.ToString() + "-12-31";
                    }else if(appRequest.Quarter == 3)
                    {
                         expiryDate = currentDateTime.AddMonths(9).Year == currentyear ? currentDateTime.AddMonths(9).ToString() : currentyear.ToString() + "-12-31";
                        
                    }
                    else if(appRequest.Quarter == 4)
                    {
                         expiryDate = currentDateTime.AddMonths(12).Year == currentyear ? currentDateTime.AddMonths(12).ToString() : currentyear.ToString() + "-12-31";
                    }
                    var expiredd = Convert.ToDateTime(expiryDate, CultureInfo.InvariantCulture);
                    if (appRequest != null)
                    {
                        appRequest.LicenseReference = LicenseRef;
                        appRequest.LicenseIssuedDate = DateTime.UtcNow;
                        appRequest.LicenseExpiryDate =expiredd; 
                        appRequest.SignatureID = Convert.ToInt32(Signatureid);
                    }
                    dbCtxt1.SaveChanges();
                    Postmypermit(LicenseRef, applicationId, ElpsId, DateTime.UtcNow, expiredd, "Lube Oil Blending Plant", isRenewed, LicenseId);

                }

            }

            catch (Exception ex)
            {
                log.Error(ex.InnerException);
            }
        }



        public void GenerateTITA_TCALetter(string applicationId, string staffemail)
        {
            try
            {
                using (LubeBlendingDBEntities dbCtxt1 = new LubeBlendingDBEntities())
                {
                    ApplicationDocHelper appDocHelper = new ApplicationDocHelper(dbCtxt1);

                    ApplicationRequest appRequest = dbCtxt1.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                    var fieldofficeaddress = (from a in dbCtxt1.ApplicationRequests join s in dbCtxt1.StateMasterLists on a.StateCode equals s.StateCode where applicationId == a.ApplicationId select s.StateAddress).FirstOrDefault();
                    var ElpsId = (from u in dbCtxt1.UserMasters where u.UserId == appRequest.ApplicantUserId select u.ElpsId).FirstOrDefault();
                    var isRenewed = appRequest.ApplicationTypeId == "NEW" ? "NO" : "YES";
                    var LicenseId = (from l in dbCtxt1.LicenseTypes where l.LicenseTypeId == appRequest.LicenseTypeId select l.LicenseSerial).FirstOrDefault();
                    var testdate = (from a in dbCtxt1.Appointments join r in dbCtxt1.AppointmentReports on a.AppointmentId equals r.AppointmentId where a.ApplicationId == applicationId select r.Cali_Int_TestDate).FirstOrDefault();
                    DateTime currentDateTime = Convert.ToDateTime(testdate);
                    var Signatureid = (from u in dbCtxt1.UserMasters where u.UserId == staffemail select u.SignatureID).FirstOrDefault();
                    string LicenseRef = appDocHelper.GenerateLicenseNumber(dbCtxt1, appRequest.LicenseTypeId);
                    var expiryd = currentDateTime.AddYears(5);
                    //var es = expiryd.ToString("dd-MM-yyyy");
                   // var expiredd = commonhelper.convertStringToDate(es, "dd-MM-yyyy");
                    if (appRequest != null)
                    {
                        appRequest.LicenseReference = LicenseRef;
                        appRequest.LicenseIssuedDate = DateTime.UtcNow;
                        appRequest.LicenseExpiryDate = expiryd;
                        appRequest.SignatureID = Convert.ToInt32(Signatureid);
                    }
                    dbCtxt1.SaveChanges();
                    Postmypermit(LicenseRef, applicationId, ElpsId, DateTime.UtcNow, expiryd, "Lube Oil Blending Plant", isRenewed, LicenseId);

                }

            }

            catch (Exception ex)
            {
                log.Error(ex.InnerException);
            }
        }











        public void GenerateTakeOverRefNo(string applicationId, string staffemail)
        {
            try
            {
                using (LubeBlendingDBEntities dbCtxt1 = new LubeBlendingDBEntities())
                {
                    ApplicationRequest appRequest = dbCtxt1.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                    var appcode = commonhelper.AppLicenseCodeType(applicationId);
                    var ElpsId = (from u in dbCtxt1.UserMasters where u.UserId == appRequest.ApplicantUserId select u.ElpsId).FirstOrDefault();
                    var isRenewed = appRequest.ApplicationTypeId == "NEW" ? "NO" : "YES";
                    var LicenseId = (from l in dbCtxt1.LicenseTypes where l.LicenseTypeId == appRequest.LicenseTypeId select l.LicenseSerial).FirstOrDefault();
                    var TakeoverRefNo = (from t in dbCtxt1.TakeoverApps where t.ApplicationID == applicationId select t.LicenseReference).FirstOrDefault();
                    var getoldpermitinfo = (from a in dbCtxt1.ApplicationRequests where a.LicenseReference == TakeoverRefNo select a).ToList().LastOrDefault();
                    DateTime currentDateTime = DateTime.UtcNow;
                    var Signatureid = (from u in dbCtxt1.UserMasters where u.UserId == staffemail select u.SignatureID).FirstOrDefault();
                    //string LicenseRef = appDocHelper.GenerateLicenseNumber(appRequest.LinkedReference, appRequest.LicenseTypeCode, appRequest.ApplicationType, dbCtxt1);
                    var Issueddate = DateTime.UtcNow;
                    //var DateIssued = d.ToString("dd-MM-yyyy");
                    //var Issueddate = commonhelper.convertStringToDate(DateIssued, "dd-MM-yyyy");

                    var expiredd = currentDateTime.AddMonths(12);
                    //var es = expiryd.ToString("dd-MM-yyyy");
                    //var expiredd = commonhelper.convertStringToDate(es, "dd-MM-yyyy");
                    if (getoldpermitinfo != null && appRequest != null)
                    {
                        appRequest.LicenseReference = TakeoverRefNo + "/T";
                        appRequest.LicenseIssuedDate = Issueddate;
                        appRequest.LicenseExpiryDate = null;
                        appRequest.SignatureID = Convert.ToInt32(Signatureid);
                    }
                    int done = dbCtxt1.SaveChanges();
                    if (done > 0)
                    {
                        Postmypermit(TakeoverRefNo + "/T", applicationId, ElpsId, Convert.ToDateTime(getoldpermitinfo.LicenseIssuedDate), Convert.ToDateTime(getoldpermitinfo.LicenseExpiryDate), "Lube Oil Blending Plant", isRenewed, LicenseId);
                    }
                }

            }

            catch (Exception ex)
            {
                log.Error(ex.InnerException);
            }
        }



        public void GenerateModificationRefNo(string applicationId, string staffemail)
        {
            try
            {
                using (LubeBlendingDBEntities dbCtxt1 = new LubeBlendingDBEntities())
                {
                    ApplicationDocHelper appDocHelper = new ApplicationDocHelper(dbCtxt1);



                    ApplicationRequest appRequest = dbCtxt1.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                    var appcode = commonhelper.AppLicenseCodeType(applicationId);
                    var ElpsId = (from u in dbCtxt1.UserMasters where u.UserId == appRequest.ApplicantUserId select u.ElpsId).FirstOrDefault();
                    var isRenewed = appRequest.ApplicationTypeId == "NEW" ? "NO" : "YES";
                    var LicenseId = (from l in dbCtxt1.LicenseTypes where l.LicenseTypeId == appRequest.LicenseTypeId select l.LicenseSerial).FirstOrDefault();
                    var getoldpermitinfo = (from a in dbCtxt1.ApplicationRequests where a.FacilityId == appRequest.FacilityId && a.LicenseTypeId == appcode && a.LicenseTypeId != null select a).FirstOrDefault();
                    DateTime currentDateTime = DateTime.UtcNow;
                    var Signatureid = (from u in dbCtxt1.UserMasters where u.UserId == staffemail select u.SignatureID).FirstOrDefault();
                    string LicenseRef = appDocHelper.GenerateLicenseNumber(dbCtxt1, appRequest.LicenseTypeId);
                    var Issueddate = DateTime.UtcNow;
                    //var DateIssued = d.ToString("dd-MM-yyyy");
                    //var Issueddate = commonhelper.convertStringToDate(DateIssued, "dd-MM-yyyy");


                    if (appRequest != null)
                    {
                        appRequest.LicenseReference = LicenseRef;
                        appRequest.LicenseIssuedDate = Issueddate;
                        appRequest.LicenseExpiryDate = null;
                        appRequest.SignatureID = Convert.ToInt32(Signatureid);
                    }
                    dbCtxt1.SaveChanges();
                    Postmypermit(LicenseRef, applicationId, ElpsId, Convert.ToDateTime(getoldpermitinfo.LicenseIssuedDate), Convert.ToDateTime(getoldpermitinfo.LicenseExpiryDate), "Lube Oil Blending Plant", isRenewed, LicenseId);

                }

            }

            catch (Exception ex)
            {
                log.Error(ex.InnerException);
            }
        }




        public ViewAsPdf ViewATCMOD(string id)
        {
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == id.Trim()).FirstOrDefault();

                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();


                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.StorageCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();



                string Lga = null, statename = null, address = null;
                var fieldofficeaddress = commonhelper.FieldLocation(dbCtxt, id, out Lga, out statename, out address);
                var TelePhoneNumber = (from c in dbCtxt.Configurations where c.ParamID == "TelePhone" select c.ParamValue).FirstOrDefault();

                var PMB = (from c in dbCtxt.Configurations where c.ParamID == "P_M_B" select c.ParamValue).FirstOrDefault();

                var compdetails = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == id && a.IsLegacy == "NO" select new { a.RegisteredAddress, a.ApplicantName, a.SiteLocationAddress, a.LicenseIssuedDate, a.LicenseReference, a.LicenseTypeId, a.ApplicationCategory }).ToList();
                var Host = HttpContext.Current.Request.Url.Authority;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                if (compdetails.Count > 0)
                {
                    var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                    Pstatus.PrintedStatus = "Printed";
                    dbCtxt.SaveChanges();
                    permits.Add(new Permitmodel()
                    {
                        Address = address,
                        LGA = Lga,
                        State = statename,
                        PMB = PMB,
                        IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                        IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                        IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                        Signature = signature,
                        TelePhoneNumber = TelePhoneNumber,
                        CompanyName = compdetails.FirstOrDefault().ApplicantName,
                        RegisteredAddress = compdetails.FirstOrDefault().RegisteredAddress,
                        LocationAddress = compdetails.FirstOrDefault().SiteLocationAddress,
                        IssuedDate = Convert.ToDateTime(compdetails.FirstOrDefault().LicenseIssuedDate).ToString("MMMM dd, yyyy"),
                        PermitNumber = compdetails.FirstOrDefault().LicenseReference,
                        ApplicationCategory = compdetails.FirstOrDefault().ApplicationCategory,
                        QrCode = QrCode
                    });

                    permitmodel.Add(new PermitModels()
                    {
                        permitmodels = permits.ToList()
                    });

                }
            }
            return new ViewAsPdf("ViewATCMOD", permitmodel.ToList())
            {
                PageSize = Rotativa.Options.Size.A4,
                FileName = id + ".pdf"
            };
        }








        public ViewAsPdf ViewTO(string id)
        {
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == id.Trim()).FirstOrDefault();

                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();


                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.StorageCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();



                var TakeoverRefNo = (from t in dbCtxt.TakeoverApps where t.ApplicationID == appRequest.ApplicationId select t.LicenseReference).FirstOrDefault();


                string[] TakeoverRef = TakeoverRefNo.Split('/');
                var TakeoverLicenseRef = TakeoverRef[0];
                var TakeoverFacilityName_Location = (from a in dbCtxt.ApplicationRequests where a.LicenseReference == TakeoverLicenseRef select a).FirstOrDefault();

                string Lga = null, statename = null, address = null;
                var fieldofficeaddress = commonhelper.FieldLocation(dbCtxt, id, out Lga, out statename, out address);
                var TelePhoneNumber = (from c in dbCtxt.Configurations where c.ParamID == "TelePhone" select c.ParamValue).FirstOrDefault();

                var PMB = (from c in dbCtxt.Configurations where c.ParamID == "P_M_B" select c.ParamValue).FirstOrDefault();

                var compdetails = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == id && a.IsLegacy == "NO" select new { a.RegisteredAddress, a.ApplicantName, a.SiteLocationAddress, a.LicenseIssuedDate, a.LicenseReference, a.LicenseTypeId, a.ApplicationCategory }).ToList();
                var Host = HttpContext.Current.Request.Url.Authority;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                if (compdetails.Count > 0)
                {
                    var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                    Pstatus.PrintedStatus = "Printed";
                    dbCtxt.SaveChanges();
                    permits.Add(new Permitmodel()
                    {
                        Address = address,
                        LGA = Lga,
                        State = statename,
                        PMB = PMB,
                        IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                        IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                        IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                        Signature = signature,
                        TelePhoneNumber = TelePhoneNumber,
                        CompanyName = compdetails.FirstOrDefault()?.ApplicantName,
                        RegisteredAddress = compdetails.FirstOrDefault()?.RegisteredAddress,
                        LocationAddress = compdetails.FirstOrDefault()?.SiteLocationAddress,
                        IssuedDate = Convert.ToDateTime(compdetails.FirstOrDefault().LicenseIssuedDate).ToString("MMMM dd, yyyy"),
                        PermitNumber = compdetails.FirstOrDefault()?.LicenseReference,
                        ApplicationCategory = compdetails.FirstOrDefault()?.ApplicationCategory,
                        TakeoverPlantLocation = TakeoverFacilityName_Location?.SiteLocationAddress,
                        TakeoverPlantName = TakeoverFacilityName_Location?.ApplicantName,
                        QrCode = QrCode
                    });

                    permitmodel.Add(new PermitModels()
                    {
                        permitmodels = permits.ToList()
                    });

                }
            }
            return new ViewAsPdf("ViewTO", permitmodel.ToList())
            {
                PageSize = Rotativa.Options.Size.A4,
                FileName = id + ".pdf"
            };
        }









        public ViewAsPdf ViewSUI(string id)
        {
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();
                string Lga = null, statename = null, address = null;
                var fieldofficeaddress = commonhelper.FieldLocation(dbCtxt, id, out Lga, out statename, out address);
                var TelePhoneNumber = (from c in dbCtxt.Configurations where c.ParamID == "TelePhone" select c.ParamValue).FirstOrDefault();

                var PMB = (from c in dbCtxt.Configurations where c.ParamID == "P_M_B" select c.ParamValue).FirstOrDefault();



                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.StorageCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();



                var compdetails = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == id && a.IsLegacy == "NO" select new { a.RegisteredAddress, a.ApplicantName, a.SiteLocationAddress, a.LicenseIssuedDate, a.LicenseReference, a.LicenseTypeId }).ToList();
                var Host = HttpContext.Current.Request.Url.Authority; ;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                if (compdetails.Count > 0)
                {
                    var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                    Pstatus.PrintedStatus = "Printed";
                    dbCtxt.SaveChanges();

                    permits.Add(new Permitmodel()
                    {
                        Address = address,
                        LGA = Lga,
                        State = statename,
                        PMB = PMB,
                        Signature = signature,
                        IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                        IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                        IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                        TelePhoneNumber = TelePhoneNumber,
                        CompanyName = compdetails.FirstOrDefault().ApplicantName,
                        RegisteredAddress = compdetails.FirstOrDefault().RegisteredAddress,
                        LocationAddress = compdetails.FirstOrDefault().SiteLocationAddress,
                        IssuedDate = Convert.ToDateTime(compdetails.FirstOrDefault().LicenseIssuedDate).ToString("MMMM dd, yyyy"),
                        PermitNumber = compdetails.FirstOrDefault().LicenseReference,
                        QrCode = QrCode
                    });

                    permitmodel.Add(new PermitModels()
                    {
                        permitmodels = permits.ToList()
                    });

                }
            }
            return new ViewAsPdf("ViewSUI", permitmodel.ToList())
            {
                PageSize = Rotativa.Options.Size.A4,
                FileName = id + ".pdf"

            };
        }










        public ViewAsPdf ViewPTE(string id)
        {
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == id.Trim()).FirstOrDefault();

                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();


                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.StorageCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();



                string Lga = null, statename = null, address = null;
                var fieldofficeaddress = commonhelper.FieldLocation(dbCtxt, id, out Lga, out statename, out address);
                var TelePhoneNumber = (from c in dbCtxt.Configurations where c.ParamID == "TelePhone" select c.ParamValue).FirstOrDefault();

                var PMB = (from c in dbCtxt.Configurations where c.ParamID == "P_M_B" select c.ParamValue).FirstOrDefault();

                var compdetails = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == id && a.IsLegacy == "NO" select new { a.RegisteredAddress, a.ApplicantName, a.SiteLocationAddress, a.LicenseIssuedDate, a.LicenseReference, a.LicenseTypeId, a.ApplicationCategory }).ToList();
                var Host = HttpContext.Current.Request.Url.Authority;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                if (compdetails.Count > 0)
                {
                    var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                    Pstatus.PrintedStatus = "Printed";
                    dbCtxt.SaveChanges();
                    permits.Add(new Permitmodel()
                    {
                        Address = address,
                        LGA = Lga,
                        State = statename,
                        PMB = PMB,
                        Signature = signature,
                        IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                        IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                        IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                        TelePhoneNumber = TelePhoneNumber,
                        CompanyName = compdetails.FirstOrDefault().ApplicantName,
                        RegisteredAddress = compdetails.FirstOrDefault().RegisteredAddress,
                        LocationAddress = compdetails.FirstOrDefault().SiteLocationAddress,
                        IssuedDate = Convert.ToDateTime(compdetails.FirstOrDefault().LicenseIssuedDate).ToString("MMMM dd, yyyy"),
                        PermitNumber = compdetails.FirstOrDefault().LicenseReference,
                        ApplicationCategory = compdetails.FirstOrDefault().ApplicationCategory,
                        QrCode = QrCode
                    });

                    permitmodel.Add(new PermitModels()
                    {
                        permitmodels = permits.ToList()
                    });
                }
            }
            return new ViewAsPdf("ViewPTE", permitmodel.ToList())
            {
                PageSize = Rotativa.Options.Size.A4,
                FileName = id + ".pdf"
            };
        }










        public ViewAsPdf ViewATC(string id)
        {
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == id.Trim()).FirstOrDefault();

                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();


                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.StorageCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();



                string Lga = null, statename = null, address = null;
                var fieldofficeaddress = commonhelper.FieldLocation(dbCtxt, id, out Lga, out statename, out address);
                var TelePhoneNumber = (from c in dbCtxt.Configurations where c.ParamID == "TelePhone" select c.ParamValue).FirstOrDefault();

                var PMB = (from c in dbCtxt.Configurations where c.ParamID == "P_M_B" select c.ParamValue).FirstOrDefault();

                var compdetails = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == id && a.IsLegacy == "NO" select new { a.RegisteredAddress, a.ApplicantName, a.SiteLocationAddress, a.LicenseIssuedDate, a.LicenseReference, a.LicenseTypeId, a.ApplicationCategory }).ToList();
                var Host = HttpContext.Current.Request.Url.Authority;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                if (compdetails.Count > 0)
                {
                    var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                    Pstatus.PrintedStatus = "Printed";
                    dbCtxt.SaveChanges();
                    permits.Add(new Permitmodel()
                    {
                        Address = address,
                        LGA = Lga,
                        State = statename,
                        PMB = PMB,
                        Signature = signature,
                        IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                        IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                        IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                        TelePhoneNumber = TelePhoneNumber,
                        CompanyName = compdetails.FirstOrDefault().ApplicantName,
                        RegisteredAddress = compdetails.FirstOrDefault().RegisteredAddress,
                        LocationAddress = compdetails.FirstOrDefault().SiteLocationAddress,
                        IssuedDate = Convert.ToDateTime(compdetails.FirstOrDefault().LicenseIssuedDate).ToString("MMMM dd, yyyy"),
                        PermitNumber = compdetails.FirstOrDefault().LicenseReference,
                        ApplicationCategory = compdetails.FirstOrDefault().ApplicationCategory,
                        QrCode = QrCode
                    });

                    permitmodel.Add(new PermitModels()
                    {
                        permitmodels = permits.ToList()
                    });
                }
            }
            return new ViewAsPdf("ViewATC", permitmodel.ToList())
            {
                PageSize = Rotativa.Options.Size.A4,
                FileName = id + ".pdf"
            };
        }




        public ViewAsPdf ViewATCLFP(string id)
        {
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == id.Trim()).FirstOrDefault();

                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();
                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.StorageCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();
                string Lga = null, statename = null, address = null;
                var fieldofficeaddress = commonhelper.FieldLocation(dbCtxt, id, out Lga, out statename, out address);
                var TelePhoneNumber = (from c in dbCtxt.Configurations where c.ParamID == "TelePhone" select c.ParamValue).FirstOrDefault();

                var PMB = (from c in dbCtxt.Configurations where c.ParamID == "P_M_B" select c.ParamValue).FirstOrDefault();

                var compdetails = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == id && a.IsLegacy == "NO" select new { a.RegisteredAddress, a.ApplicantName, a.SiteLocationAddress, a.LicenseIssuedDate, a.LicenseReference, a.LicenseTypeId, a.ApplicationCategory }).ToList();
                var Host = HttpContext.Current.Request.Url.Authority;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                if (compdetails.Count > 0)
                {
                    var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                    Pstatus.PrintedStatus = "Printed";
                    dbCtxt.SaveChanges();
                    permits.Add(new Permitmodel()
                    {
                        Address = address,
                        LGA = Lga,
                        State = statename,
                        PMB = PMB,
                        Signature = signature,
                        IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                        IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                        IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                        TelePhoneNumber = TelePhoneNumber,
                        CompanyName = compdetails.FirstOrDefault().ApplicantName,
                        RegisteredAddress = compdetails.FirstOrDefault().RegisteredAddress,
                        LocationAddress = compdetails.FirstOrDefault().SiteLocationAddress,
                        IssuedDate = Convert.ToDateTime(compdetails.FirstOrDefault().LicenseIssuedDate).ToString("MMMM dd, yyyy"),
                        PermitNumber = compdetails.FirstOrDefault().LicenseReference,
                        ApplicationCategory = compdetails.FirstOrDefault().ApplicationCategory,
                        QrCode = QrCode
                    });

                    permitmodel.Add(new PermitModels()
                    {
                        permitmodels = permits.ToList()
                    });
                }
            }
            return new ViewAsPdf("ViewATCLFP", permitmodel.ToList())
            {
                PageSize = Rotativa.Options.Size.A4,
                FileName = id + ".pdf"
            };
        }





        public ViewAsPdf ViewTPBA(string id)
        {
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == id.Trim()).FirstOrDefault();

                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();


                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.StorageCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();



                string Lga = null, statename = null, address = null;
                var fieldofficeaddress = commonhelper.FieldLocation(dbCtxt, id, out Lga, out statename, out address);
                var TelePhoneNumber = (from c in dbCtxt.Configurations where c.ParamID == "TelePhone" select c.ParamValue).FirstOrDefault();

                var PMB = (from c in dbCtxt.Configurations where c.ParamID == "P_M_B" select c.ParamValue).FirstOrDefault();
                var hqaddress = (from c in dbCtxt.Configurations where c.ParamID == "HQAddress" select c.ParamValue).FirstOrDefault();

                var compdetails = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == id && a.IsLegacy == "NO" select new { a.RegisteredAddress, a.ApplicantName, a.SiteLocationAddress, a.LicenseIssuedDate, a.LicenseReference, a.LicenseTypeId, a.ApplicationCategory, a.AnnualCumuBaseOilRequirementCapacity, a.LicenseExpiryDate, a.PLW_PRW_Name, a.PLW_PRW_Address }).ToList();
                var Host = HttpContext.Current.Request.Url.Authority;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                if (compdetails.Count > 0)
                {
                    var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                    Pstatus.PrintedStatus = "Printed";
                    dbCtxt.SaveChanges();
                    string TPBA_Label = compdetails.FirstOrDefault().LicenseTypeId == "TPBA-PLW" ? "PRODUCT OWNER" : "PLANT OWNER";

                    permits.Add(new Permitmodel()
                    {
                        Address = hqaddress,
                        LGA = Lga,
                        State = statename,
                        PMB = PMB,
                        Signature = signature,
                        IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                        IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                        IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                        TelePhoneNumber = TelePhoneNumber,
                        CompanyName = compdetails.FirstOrDefault().ApplicantName,
                        RegisteredAddress = compdetails.FirstOrDefault().RegisteredAddress,
                        LocationAddress = compdetails.FirstOrDefault().SiteLocationAddress,
                        Capacity = double.Parse(compdetails.FirstOrDefault().AnnualCumuBaseOilRequirementCapacity, System.Globalization.CultureInfo.InvariantCulture),
                        IssuedDate = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM dd, yyyy"),
                        ExpiryDate = Convert.ToDateTime(details.LicenseExpiryDate).ToString("MMMM dd, yyyy"),
                        PermitNumber = compdetails.FirstOrDefault().LicenseReference,
                        ApplicationCategory = compdetails.FirstOrDefault().ApplicationCategory,
                        Prw_Plw_Name = compdetails.FirstOrDefault().PLW_PRW_Name == null ? compdetails.FirstOrDefault().ApplicantName: compdetails.FirstOrDefault().PLW_PRW_Name,
                        Prw_Plw_Address = compdetails.FirstOrDefault().PLW_PRW_Address == null ? compdetails.FirstOrDefault().SiteLocationAddress : compdetails.FirstOrDefault().PLW_PRW_Address,
                        TPBA_LABEL = TPBA_Label,
                        QrCode = QrCode
                    });



                    permitmodel.Add(new PermitModels()
                    {
                        permitmodels = permits.ToList()
                    });
                }
            }
            return new ViewAsPdf("ViewTPBA", permitmodel.ToList())
            {
                PageSize = Rotativa.Options.Size.A4,
                FileName = id + ".pdf"
            };
        }




        public ViewAsPdf ViewTITA(string id)
        {
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == id.Trim()).FirstOrDefault();
                var testdade = (from a in dbCtxt.Appointments join r in dbCtxt.AppointmentReports on a.AppointmentId equals r.AppointmentId where a.ApplicationId == id select r.Cali_Int_TestDate).FirstOrDefault();
                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();


                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.StorageCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();



                string Lga = null, statename = null, address = null;
                var fieldofficeaddress = commonhelper.FieldLocation(dbCtxt, id, out Lga, out statename, out address);
                var TelePhoneNumber = (from c in dbCtxt.Configurations where c.ParamID == "TelePhone" select c.ParamValue).FirstOrDefault();
                var tankcount = (from a in dbCtxt.ApplicationRequests join t in dbCtxt.Tanks on a.ApplicationId equals t.ApplicationId select t.NbrOfTanks).FirstOrDefault();
                var PMB = (from c in dbCtxt.Configurations where c.ParamID == "P_M_B" select c.ParamValue).FirstOrDefault();

                var compdetails = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == id && a.IsLegacy == "NO" select new { a.RegisteredAddress, a.ApplicantName, a.SiteLocationAddress, a.LicenseIssuedDate, a.LicenseReference, a.LicenseTypeId, a.ApplicationCategory, a.LicenseExpiryDate, a.AnnualCumuBaseOilRequirementCapacity }).ToList();
                var Host = HttpContext.Current.Request.Url.Authority;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                if (compdetails.Count > 0)
                {
                    var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                    Pstatus.PrintedStatus = "Printed";
                    dbCtxt.SaveChanges();
                    permits.Add(new Permitmodel()
                    {
                        Address = address,
                        LGA = Lga,
                        State = statename,
                        PMB = PMB,
                        Signature = signature,
                        IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                        IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                        IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                        TelePhoneNumber = TelePhoneNumber,
                        CompanyName = compdetails.FirstOrDefault().ApplicantName,
                        RegisteredAddress = compdetails.FirstOrDefault().RegisteredAddress,
                        LocationAddress = compdetails.FirstOrDefault().SiteLocationAddress,
                        IssuedDate = Convert.ToDateTime(compdetails.FirstOrDefault().LicenseIssuedDate).ToString("MMMM dd, yyyy"),
                        ExpiryDate = Convert.ToDateTime(compdetails.FirstOrDefault().LicenseExpiryDate).ToString("MMMM dd, yyyy"),
                        PermitNumber = compdetails.FirstOrDefault().LicenseReference,
                        TestDate = Convert.ToDateTime(testdade).ToString("MMMM dd, yyyy"),
                        NUmberOfTanks = tankcount,
                        NUmberOfTanksToWord = commonhelper.NumberToWords(tankcount),
                        Capacity = Convert.ToInt64(compdetails.FirstOrDefault().AnnualCumuBaseOilRequirementCapacity),
                        QrCode = QrCode
                    });

                    permitmodel.Add(new PermitModels()
                    {
                        permitmodels = permits.ToList()
                    });
                }
            }
            return new ViewAsPdf("ViewTITA", permitmodel.ToList())
            {
                PageSize = Rotativa.Options.Size.A4,
                FileName = id + ".pdf"
            };
        }



        public ViewAsPdf ViewTCA(string id)
        {
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == id.Trim()).FirstOrDefault();
                var testdade = (from a in dbCtxt.Appointments join r in dbCtxt.AppointmentReports on a.AppointmentId equals r.AppointmentId where a.ApplicationId == id select r.Cali_Int_TestDate).FirstOrDefault();
                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();


                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.StorageCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();



                string Lga = null, statename = null, address = null;
                var fieldofficeaddress = commonhelper.FieldLocation(dbCtxt, id, out Lga, out statename, out address);
                var TelePhoneNumber = (from c in dbCtxt.Configurations where c.ParamID == "TelePhone" select c.ParamValue).FirstOrDefault();

                var PMB = (from c in dbCtxt.Configurations where c.ParamID == "P_M_B" select c.ParamValue).FirstOrDefault();
                var tankcount = (from a in dbCtxt.ApplicationRequests join t in dbCtxt.Tanks on a.ApplicationId equals t.ApplicationId select t.NbrOfTanks).FirstOrDefault();
                var compdetails = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == id && a.IsLegacy == "NO" select new { a.RegisteredAddress, a.ApplicantName, a.SiteLocationAddress, a.LicenseIssuedDate, a.LicenseReference, a.LicenseTypeId, a.ApplicationCategory, a.LicenseExpiryDate, a.AnnualCumuBaseOilRequirementCapacity }).ToList();
                var Host = HttpContext.Current.Request.Url.Authority;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                if (compdetails.Count > 0)
                {
                    var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                    Pstatus.PrintedStatus = "Printed";
                    dbCtxt.SaveChanges();
                    permits.Add(new Permitmodel()
                    {
                        Address = address,
                        LGA = Lga,
                        State = statename,
                        PMB = PMB,
                        Signature = signature,
                        IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                        IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                        IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                        TelePhoneNumber = TelePhoneNumber,
                        CompanyName = compdetails.FirstOrDefault().ApplicantName,
                        RegisteredAddress = compdetails.FirstOrDefault().RegisteredAddress,
                        LocationAddress = compdetails.FirstOrDefault().SiteLocationAddress,
                        IssuedDate = Convert.ToDateTime(compdetails.FirstOrDefault().LicenseIssuedDate).ToString("MMMM dd, yyyy"),
                        ExpiryDate = Convert.ToDateTime(compdetails.FirstOrDefault().LicenseExpiryDate).ToString("MMMM dd, yyyy"),
                        PermitNumber = compdetails.FirstOrDefault().LicenseReference,
                        TestDate = Convert.ToDateTime(testdade).ToString("MMMM dd, yyyy"),
                        NUmberOfTanks = tankcount,
                        NUmberOfTanksToWord = commonhelper.NumberToWords(tankcount),
                        Capacity = Convert.ToInt64(compdetails.FirstOrDefault().AnnualCumuBaseOilRequirementCapacity),
                        QrCode = QrCode
                    });

                    permitmodel.Add(new PermitModels()
                    {
                        permitmodels = permits.ToList()
                    });
                }
            }
            return new ViewAsPdf("ViewTCA", permitmodel.ToList())
            {
                PageSize = Rotativa.Options.Size.A4,
                FileName = id + ".pdf"
            };
        }



        public ViewAsPdf ViewLTOLFP(string id)
        {
            DateTime currentDateTime = DateTime.UtcNow.AddYears(1);
            DateTime currentDate = DateTime.UtcNow;
            string expiryDate = currentDateTime.ToString("yyyy") + "-12-31";
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == id.Trim()).FirstOrDefault();
                var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                Pstatus.PrintedStatus = "Printed";

                var Host = HttpContext.Current.Request.Url.Authority;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                dbCtxt.SaveChanges();
                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, f.Arrears, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.AnnualCumuBaseOilRequirementCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();

                var capacity = double.Parse(details.AnnualCumuBaseOilRequirementCapacity, System.Globalization.CultureInfo.InvariantCulture);

                permits.Add(new Permitmodel()
                {
                    Amountpaid = Convert.ToDecimal(details.TxnAmount),
                    Arrear = Convert.ToDecimal(details.Arrears),
                    Signature = signature,
                    CompanyName = details.ApplicantName,
                    CompanyIdentity = details.ElpsId,
                    LicenseNumber = details.LicenseReference,
                    RegisteredAddress = details.RegisteredAddress,
                    LocationAddress = details.SiteLocationAddress,
                    ExpiryYear = Convert.ToDateTime(details.LicenseExpiryDate).ToString("yyyy"),
                    CapacityToWord = commonhelper.NumberToWords(Convert.ToInt64(capacity)),
                    AmountToWord = commonhelper.NumberToWords(Convert.ToInt64(details.TxnAmount)),
                    IssuedDate = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM dd, yyyy"),
                    IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                    IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                    IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                    Capacity = capacity,
                    ApprefNo = details.ApplicationId,
                    ApplicationCategory = details.ApplicationCategory,
                    QrCode = QrCode

                });
                permitmodel.Add(new PermitModels()
                {
                    permitmodels = permits.ToList()
                });
            }
            return new ViewAsPdf("ViewLTOLFP", permitmodel.ToList())
            {
                PageSize = Rotativa.Options.Size.A4,
                FileName = id + ".pdf"

            };
        }





        public ViewAsPdf ViewLTO(string id)
        {
            DateTime currentDateTime = DateTime.UtcNow.AddYears(1);
            DateTime currentDate = DateTime.UtcNow;
            string expiryDate = currentDateTime.ToString("yyyy") + "-12-31";
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == id.Trim()).FirstOrDefault();
                var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                Pstatus.PrintedStatus = "Printed";

                var Host = HttpContext.Current.Request.Url.Authority;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                dbCtxt.SaveChanges();
                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, f.Arrears, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.AnnualCumuBaseOilRequirementCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();

                var capacity = double.Parse(details.AnnualCumuBaseOilRequirementCapacity, System.Globalization.CultureInfo.InvariantCulture);

                permits.Add(new Permitmodel()
                {
                    Amountpaid = Convert.ToDecimal(details.TxnAmount),
                    Arrear = Convert.ToDecimal(details.Arrears),
                    Signature = signature,
                    CompanyName = details.ApplicantName,
                    CompanyIdentity = details.ElpsId,
                    LicenseNumber = details.LicenseReference,
                    RegisteredAddress = details.RegisteredAddress,
                    LocationAddress = details.SiteLocationAddress,
                    ExpiryYear = Convert.ToDateTime(details.LicenseExpiryDate).ToString("yyyy"),
                    CapacityToWord = commonhelper.NumberToWords(Convert.ToInt64(capacity)),
                    AmountToWord = commonhelper.NumberToWords(Convert.ToInt64(details.TxnAmount)),
                    IssuedDate = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM dd, yyyy"),
                    IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                    IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                    IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                    Capacity = capacity,
                    ApprefNo = details.ApplicationId,
                    ApplicationCategory = details.ApplicationCategory,
                    QrCode = QrCode

                }) ;
                permitmodel.Add(new PermitModels()
                {
                    permitmodels = permits.ToList()
                });
            }
            return new ViewAsPdf("ViewLTO", permitmodel.ToList())
            {
                PageSize = Rotativa.Options.Size.A4,
                FileName = id + ".pdf"

            };
        }

    }
}
