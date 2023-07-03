using log4net;
using LOBP.DbEntities;
using LOBP.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Web.Mvc;
using System.Threading.Tasks;

namespace LOBP.Helper
{
    public class UserHelper
    {
        private UtilityHelper utilityHelper = new UtilityHelper();
        private LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities();
        private ILog log = log4net.LogManager.GetLogger(typeof(UserHelper));

        public UserHelper() { }

        public UserHelper(LubeBlendingDBEntities dbCtxt)
        {
            this.dbCtxt = dbCtxt;
            utilityHelper = new UtilityHelper(dbCtxt);
        }


        public bool CodeCheck(string email, string code)
        {
            using (var db = new LubeBlendingDBEntities())
            {
                var public_key = db.Configurations.Where(c => c.ParamID == "APP_ID").FirstOrDefault().ParamValue;
                var _elpsAppKey = db.Configurations.Where(c => c.ParamID == "APP_KEY").FirstOrDefault().ParamValue;
                var StringCode = public_key.ToUpper() + "." + email.ToUpper() + "." + _elpsAppKey.ToUpper();
                var hashcode = GenerateSHA512(StringCode);

                if (hashcode == code)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }



        public string AppLicenseCodeType(string ApplicationId)
        {
            string Getcodetype;
            using (var db = new LubeBlendingDBEntities())
            {
                Getcodetype = (from a in db.ApplicationRequests where a.ApplicationId == ApplicationId select a.LicenseTypeId).FirstOrDefault();
            }
            return Getcodetype;
        }



        public bool IsNumeric(string input)
        {
            int test;
            return int.TryParse(input, out test);
        }





        public string StaffMessageTemplate(string subject, string content)
        {
            string body = "<div>";
            body += "<div style='width: 700px; background-color: #ece8d4; padding: 5px 0 5px 0;'><img style='width: 98%; height: 120px; display: block; margin: 0 auto;' src='https://ROMS.dpr.gov.ng/Content/Images/mainlogo.png' alt='Logo'/></div>";
            body += "<div class='text-left' style='background-color: #ece8d4; width: 700px; min-height: 200px;'>";
            body += "<div style='padding: 10px 30px 30px 30px;'>";
            body += "<h5 style='text-align: center; font-weight: 300; padding-bottom: 10px; border-bottom: 1px solid #ddd;'>" + subject + "</h5>";
            body += "<p>Dear Sir/Madam,</p>";
            body += "<p style='line-height: 30px; text-align: justify;'>" + content + "</p>";
            body += "<br>";
            body += "<p>Kindly go to <a href='https://lobp.dpr.gov.ng/'>LUBE BLENDING PLANT PORTAL(CLICK HERE)</a></p>";
            body += "<p>Department of Petroleum Resources<br/> <small>(LUBE BLENDING PLANT) </small></p> </div>";
            body += "<div style='padding:10px 0 10px; 10px; background-color:#888; color:#f9f9f9; width:700px;'> &copy; " + DateTime.Now.Year + " Department of Petroleum Resources &minus; DPR Nigeria</div></div></div>";
            return body;
        }


        public async Task<string> SendStaffEmailMessage(string staffemail, string subject, string content)
        {
            var result = "";
            var apiKey = "BNW5He3DoWQAJVMkeMlEzPTtbYIXNveS4t+GuGtXzxQJ";
            var username = "AKIAQCM2OPFBW35OSTFV";
            var emailFrom = "no-reply@dpr.gov.ng";
            var Host = "email-smtp.us-west-2.amazonaws.com";
            var Port = 587;

            //var apiKey = "BNW5He3DoWQAJVMkeMlEzPTtbYIXNveS4t+GuGtXzxQJ";
            //var username = "AKIAQCM2OPFBW35OSTFV";
            //var emailFrom = "no-reply@nuprc.gov.ng";
            //var Host = "email-smtp.us-west-2.amazonaws.com";
            //var Port = 25;//587;



            var msgBody = StaffMessageTemplate(subject, content);

            MailMessage _mail = new MailMessage();
            SmtpClient client = new SmtpClient(Host, Port);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential(username, apiKey);
            _mail.From = new MailAddress(emailFrom);
            _mail.To.Add(new MailAddress(staffemail));
            _mail.Subject = subject;
            _mail.IsBodyHtml = true;
            _mail.Body = msgBody;
            try
            {
                client.Send(await Task.FromResult(_mail));
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }







        public string GenerateSHA512(string inputString)
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




        public ApplicationRequest UpdateCompanyUser(LubeBlendingDBEntities dbCtxt, ElpServiceHelper serviceIntegrator, string Email, string role)
        {
            if (role != null)
            {
                if (role.Contains("COMPANY"))
                {
                    using (var dbCtxt1 = new LubeBlendingDBEntities())
                    {
                        ElpsResponse elpsResponse = serviceIntegrator.GetCompanyDetailByEmail(Email);
                        CompanyDetail companyDetail = (CompanyDetail)elpsResponse.value;
                        CompanyAddressDTO compDetailsModel = new CompanyAddressDTO();

                        ElpsResponse elpsResponseaddress = serviceIntegrator.GetAddressByID(Convert.ToString(companyDetail.registered_Address_Id));
                        compDetailsModel = (CompanyAddressDTO)elpsResponseaddress.value;


                        var updateapplication = (from a in dbCtxt1.ApplicationRequests where a.ApplicantUserId == Email select a).ToList();
                        var updateusermaster = (from u in dbCtxt1.UserMasters where u.UserId == Email select u).ToList();

                        if (companyDetail.registered_Address_Id != null)
                        {
                            foreach (var item in updateapplication)
                            {
                                item.ApplicantName = companyDetail.name;
                                item.RegisteredAddress = compDetailsModel.address_1;
                            }

                            foreach (var item in updateusermaster)
                            {
                                item.FirstName = companyDetail.name;
                            }
                            dbCtxt1.SaveChanges();
                        }

                    }
                }
            }
            return null;
        }




        public List<Menu> GetUserMenuList(UserMaster userMaster, out List<Functionality> functionalityList)
        {
            functionalityList = new List<Functionality>();
            List<Menu> userMenuList = new List<Menu>();
            List<string> myRoles = new List<string>();

            try
            {


                string[] role = userMaster.UserRoles.Split(',');
                foreach (string s in role)
                {
                    myRoles.Add("'" + s + "'");
                }

                //log.Info("MyRoles Join =>" + string.Join(",",myRoles));

                using (var dbCtxt2 = new LubeBlendingDBEntities())
                {
                    functionalityList = dbCtxt2.Database.SqlQuery<Functionality>(string.Format("SELECT DISTINCT A.* from Functionality A,RoleFunctionalityMapping B WHERE A.FuncId = B.FuncId AND A.Status ='{0}' AND B.RoleId IN ({1}) ", "ACTIVE", string.Join(",", myRoles))).ToList();

                    foreach (Menu menudefn in dbCtxt2.Menus.Where(c => c.Status == "ACTIVE").OrderBy(c => c.SeqNo).ToList())
                    {
                        foreach (Functionality functionality in functionalityList)
                        {
                            if (menudefn.MenuId == functionality.MenuId)
                            {
                                userMenuList.Add(menudefn);
                                break;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
            }

            return userMenuList;
        }



        public List<ApplicationRequest> GetApprovalRequest(LubeBlendingDBEntities dbCtxt, UserMaster userMaster, out String errorMessage)
        {
            errorMessage = "SUCCESS";
            List<ApplicationRequest> allRequest = new List<ApplicationRequest>();
            log.Info("About to fetch Requests due for User => " + userMaster.UserId + " With Roles =>" + userMaster.UserRoles);

            try
            {
                List<string> UserRoles = userMaster.UserRoles.Split(',').ToList();
                string headOfficeBranch = dbCtxt.FieldLocations.Where(f => f.FieldType == "HQ").FirstOrDefault().FieldLocationID;
                List<short> AllStages = dbCtxt.WorkFlowNavigations.Where(c => UserRoles.Contains(c.ActionRole)).Select(c => c.CurrentStageID).Distinct().ToList();
                log.Info("User Stages =>" + string.Join(",", AllStages.ToArray()));
                ApplicationRequest appmaster = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == allRequest.FirstOrDefault().ApplicationId).FirstOrDefault();
                var currentstage = (from u in dbCtxt.ApplicationRequests where u.CurrentAssignedUser == userMaster.UserId select new { u.CurrentStageID, u.CurrentOfficeLocation }).ToList();
                var currentstageid = (currentstage.Select(x => x.CurrentStageID)).ToList();
                var currentofficeid = (currentstage.Select(x => x.CurrentOfficeLocation)).ToList();
                var fld = currentstage != null ? (from w in dbCtxt.WorkFlowNavigations where currentstageid.Contains(w.CurrentStageID) select w).FirstOrDefault() : null;
                var trackuserlocation = (from u in dbCtxt.UserMasters where u.UserId == userMaster.UserId select u.UserLocation).FirstOrDefault();
                //var fld = (from u in dbCtxt.ApplicationRequests join w in dbCtxt.WorkFlowNavigations on u.CurrentStageID equals w.CurrentStageID where w.ActionRole ==userMaster.UserRoles && u.CurrentStageID == w.CurrentStageID select new { w.FieldLocationApply, u.LastOfficeLocation}).FirstOrDefault();
                var zone = currentstage != null ? (from z in dbCtxt.ZoneFieldMappings where currentofficeid.Contains(z.FieldLocationID) select z.ZoneFieldID).Distinct().ToList() : null;
                var fields = (from f in dbCtxt.ZoneFieldMappings where zone.Contains(f.ZoneFieldID) select f.FieldLocationID).Distinct().ToList();
                foreach (ApplicationRequest appRequest in dbCtxt.ApplicationRequests.Where(a => AllStages.Contains((short)a.CurrentStageID)).ToList())
                {
                    if (appRequest.IsLegacy == "YES")
                    {
                        foreach (var item in AllStages)
                        {
                            if ((appRequest.CurrentStageID == item && (fld.FieldLocationApply == "HQ" || fld.FieldLocationApply == "FD")) && appRequest.CurrentOfficeLocation == userMaster.UserLocation && userMaster.UserId == appRequest.CurrentAssignedUser)//appRequest.LastOfficeLocation == userMaster.UserLocation
                            {
                                if (UserRoles.Contains("SUPERVISOR") || UserRoles.Contains("AD RBP") || UserRoles.Contains("ADOPERATION") || UserRoles.Contains("REVIEWER"))//|| UserRoles.Contains("ADOPERATIONS") || UserRoles.Contains("ZOPSCON") || UserRoles.Contains("ADGAS")
                                {
                                    allRequest.Add(appRequest);
                                }
                            }

                        }

                        foreach (var item in fields)
                        {
                            if (userMaster.UserLocation.ToString() == item)
                            {
                                foreach (var item1 in AllStages)
                                {
                                    if (appRequest.CurrentStageID == item1 && (fld.FieldLocationApply == "ZN" || fld.FieldLocationApply == "FD") && trackuserlocation != 2.ToString() && appRequest.CurrentOfficeLocation == userMaster.UserLocation && userMaster.UserId == appRequest.CurrentAssignedUser)
                                    {
                                        if (UserRoles.Contains("ADOPERATION") || UserRoles.Contains("ZOPSCON") || UserRoles.Contains("HOD") || UserRoles.Contains("SUPERVISOR") || UserRoles.Contains("REVIEWER"))
                                        {
                                            allRequest.Add(appRequest);
                                        }
                                    }
                                }
                            }

                        };

                        //if ((userMaster.UserLocation == headOfficeBranch && fld.FieldLocationApply == "HQ") && appRequest.LastOfficeLocation == userMaster.UserLocation)
                        //{
                        //    allRequest.Add(appRequest);
                        //}
                    }
                    else
                    {
                        if (utilityHelper.isPaymentMade(appRequest.ApplicationId, out errorMessage))
                        {
                            foreach (var item in AllStages)
                            {
                                if ((appRequest.CurrentStageID == item && fld.FieldLocationApply == "FD") && appRequest.CurrentOfficeLocation == userMaster.UserLocation && userMaster.UserId == appRequest.CurrentAssignedUser)
                                {
                                    if (UserRoles.Contains("SUPERVISOR") || UserRoles.Contains("REVIEWER") || UserRoles.Contains("OPSCON") || UserRoles.Contains("HOD") || UserRoles.Contains("AD RBP") || UserRoles.Contains("ADOPERATION"))
                                    {
                                        allRequest.Add(appRequest);
                                    }
                                }
                            }
                            foreach (var item in fields)
                            {
                                if (userMaster.UserLocation.ToString() == item)
                                {
                                    foreach (var item1 in AllStages)
                                    {
                                        if (appRequest.CurrentStageID == item1 && (fld.FieldLocationApply == "ZN") && appRequest.CurrentOfficeLocation == userMaster.UserLocation && userMaster.UserId == appRequest.CurrentAssignedUser)
                                        {
                                            if (UserRoles.Contains("ADOPERATION") || UserRoles.Contains("ZOPSCON") || UserRoles.Contains("HOD") || UserRoles.Contains("SUPERVISOR") || UserRoles.Contains("REVIEWER"))
                                            {
                                                allRequest.Add(appRequest);
                                            }
                                        }
                                    }
                                    break;
                                }

                            };
                            foreach (var item in AllStages)
                            {
                                if ((appRequest.CurrentStageID == item && userMaster.UserLocation == headOfficeBranch && (fld.FieldLocationApply == "HQ")) && appRequest.CurrentOfficeLocation == userMaster.UserLocation && userMaster.UserId == appRequest.CurrentAssignedUser)
                                {
                                    allRequest.Add(appRequest);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                errorMessage = "Error Occured When Searching For User Applications, Please try again Later";
            }

            return allRequest;
        }





        public List<ApplicationRequest> GetAssignedApplication(UserMaster userMaster, out String errorMessage)
        {
            errorMessage = "SUCCESS";
            List<ApplicationRequest> allRequest = new List<ApplicationRequest>();
            log.Info("About to fetch Requests due for User => " + userMaster.UserId + " With Roles =>" + userMaster.UserRoles);

            try
            {
                List<string> UserRoles = userMaster.UserRoles.Split(',').ToList();
                string headOfficeBranch = dbCtxt.FieldLocations.Where(f => f.FieldType == "HQ").FirstOrDefault().FieldLocationID;
                List<short> AllStages = dbCtxt.WorkFlowNavigations.Where(c => UserRoles.Contains(c.ActionRole)).Select(c => c.CurrentStageID).Distinct().ToList();

                log.Info("User Stages =>" + string.Join(",", AllStages.ToArray()));
                log.Info("Stage Count =>" + AllStages.Count);

                if (AllStages.Count > 0)
                {
                    foreach (ApplicationRequest appRequest in dbCtxt.ApplicationRequests.Where(a => AllStages.Contains((short)a.CurrentStageID)).ToList())
                    {
                        log.Info("About to check if Payment is made for ApplicationId =>" + appRequest.ApplicationId);

                        if (utilityHelper.isPaymentMade(appRequest.ApplicationId, out errorMessage))
                        {
                            log.Info("CurrentAssignedUser =>" + appRequest.CurrentAssignedUser + " UserMaster =>" + userMaster.UserId);
                            if (appRequest.CurrentAssignedUser == userMaster.UserId)
                            {
                                allRequest.Add(appRequest);
                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                errorMessage = "Error Occured When Searching For User Applications, Please try again Later";
            }

            return allRequest;
        }



        public void AutoAssignApplication(string ApplicationId, string role)
        {
            try
            {
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();
                int OfficerAllocInterval = Convert.ToInt32(dbCtxt.Configurations.Where(c => c.ParamID == "APP_ALLOC_INTERVAL").FirstOrDefault().ParamValue);

                Dictionary<string, int> UserActivityCountTbl = new Dictionary<string, int>();
                var today = DateTime.Today.Date;
                var intervalDate = today.AddDays(-OfficerAllocInterval);

                int UserActivityCount = 0;
                foreach (var userlist in (from item in dbCtxt.UserMasters
                                          where item.UserLocation == appRequest.CurrentOfficeLocation && item.UserRoles.Contains(role) && item.Status == "ACTIVE"
                                          select item).OrderBy(x => Guid.NewGuid()).Take(100).ToList())
                {

                    UserActivity useract = dbCtxt.UserActivities.Where(u => u.UserId == userlist.UserId && DbFunctions.TruncateTime(u.ValueDate) >= intervalDate && DbFunctions.TruncateTime(u.ValueDate) <= today && u.CurrentStageID == 1).FirstOrDefault();
                    if (useract !=
                     default(UserActivity))
                    {
                        UserActivityCount = useract.TxnCount.Value;
                    }
                    else
                    {
                        UserActivityCount = 0;
                    }

                    UserActivityCountTbl.Add(userlist.UserId, UserActivityCount);
                }

                if (UserActivityCountTbl.Keys.Count != 0)
                {
                    string returnuser = UserActivityCountTbl.OrderBy(a => a.Value).ToList().First().Key;
                    UserActivity userac = dbCtxt.UserActivities.Where(a => a.UserId == returnuser && a.CurrentStageID == appRequest.CurrentStageID && a.ValueDate == today).FirstOrDefault();
                    if (userac ==
                     default(UserActivity))
                    {
                        var Usractivity = new UserActivity()
                        {
                            UserId = returnuser,
                            TxnCount = 1,
                            ValueDate = DateTime.Today.Date,
                            CurrentStageID = appRequest.CurrentStageID
                        };
                        dbCtxt.UserActivities.Add(Usractivity);
                        dbCtxt.SaveChanges();
                        userac = dbCtxt.UserActivities.Where(a => a.UserId == returnuser && a.CurrentStageID == appRequest.CurrentStageID && a.ValueDate == today).FirstOrDefault();

                    }
                    else
                    {
                        userac.TxnCount = userac.TxnCount + 1;
                    }

                    var UsrActivityHistory = new UserActivityHist()
                    {
                        ActivityId = userac.ActivityId,
                        UserId = returnuser,
                        ApplicationId = appRequest.ApplicationId,
                        LicenseTypeId = appRequest.LicenseTypeId,
                        ActivityOn = DateTime.Today,
                        CurrentStageID = appRequest.CurrentStageID
                    };
                    dbCtxt.UserActivityHists.Add(UsrActivityHistory);
                    appRequest.CurrentAssignedUser = returnuser;
                    var subject = "Application waiting for Approval";
                    var content = "Application with the reference number " + ApplicationId + " is currently on your desk waiting for approval.";
                    var sendemail = SendStaffEmailMessage(returnuser, subject, content);

                    dbCtxt.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

        }


        public string GetNextProcessingStaff(LubeBlendingDBEntities dbCtxt, ApplicationRequest appRequest, string targetRole, string locationId, string action, string ActionRole)
        {
            int UserActivityCount = 0;
            string assignedUser = null;
            Dictionary<string, int> UserActivityCountTbl = new Dictionary<string, int>();

            try
            {

                int AllocInterval = Convert.ToInt32(dbCtxt.Configurations.Where(c => c.ParamID == "APP_ALLOC_INTERVAL").FirstOrDefault().ParamValue);
                var todayDate = DateTime.Today.Date;
                var intervalDate = todayDate.AddDays(-AllocInterval);

               
                        foreach (var userlist in (from item in dbCtxt.UserMasters
                                                  where item.UserLocation == locationId && item.UserRoles == targetRole && item.Status == "ACTIVE"
                                                  select item).OrderBy(x => Guid.NewGuid()).Take(100).ToList())
                        {


                            UserActivity useractivity = dbCtxt.UserActivities.Where(u => u.UserId == userlist.UserId && DbFunctions.TruncateTime(u.ValueDate) >= intervalDate && DbFunctions.TruncateTime(u.ValueDate) <= todayDate && u.CurrentStageID == appRequest.CurrentStageID).FirstOrDefault();
                            if (useractivity !=
                             default(UserActivity))
                            {
                                UserActivityCount = useractivity.TxnCount.Value;
                            }
                            else
                            {
                                UserActivityCount = 0;
                            }

                            UserActivityCountTbl.Add(userlist.UserId, UserActivityCount);
                        }
                        if (UserActivityCountTbl.Keys.Count != 0)
                        {
                            string returnuser = UserActivityCountTbl.OrderBy(a => a.Value).ToList().First().Key;
                            UserActivity userac = dbCtxt.UserActivities.Where(a => a.UserId == returnuser && a.CurrentStageID == appRequest.CurrentStageID && a.ValueDate == todayDate).FirstOrDefault();
                            if (userac ==
                             default(UserActivity))
                            {
                                var Usractivity = new UserActivity()
                                {
                                    UserId = returnuser,
                                    TxnCount = 1,
                                    ValueDate = DateTime.Today.Date,
                                    CurrentStageID = appRequest.CurrentStageID
                                };
                                dbCtxt.UserActivities.Add(Usractivity);
                                dbCtxt.SaveChanges();
                                userac = dbCtxt.UserActivities.Where(a => a.UserId == returnuser && a.CurrentStageID == appRequest.CurrentStageID && a.ValueDate == todayDate).FirstOrDefault();

                            }
                            else
                            {
                                userac.TxnCount = userac.TxnCount + 1;
                            }

                            var UsrActivityHistory = new UserActivityHist()
                            {
                                ActivityId = userac.ActivityId,
                                UserId = returnuser,
                                ApplicationId = appRequest.ApplicationId,
                                LicenseTypeId = appRequest.LicenseTypeId,
                                ActivityOn = DateTime.Today,
                                CurrentStageID = appRequest.CurrentStageID
                            };
                            dbCtxt.UserActivityHists.Add(UsrActivityHistory);
                            assignedUser = returnuser;
                            dbCtxt.SaveChanges();
                        }
                    
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

            return assignedUser;
        }


        public void UpdateNewEmailDomain(string email)
        {

            string[] splitemail = email.Split('@');
            string firstsplit = splitemail[0].ToString();
            string secondsplit = splitemail[1].ToString();
                var checkifuserexist = (from u in dbCtxt.UserMasters where u.UserId.Contains(firstsplit) select u).FirstOrDefault();
                List<ActionHistory> updatetriggeredby = (from u in dbCtxt.ActionHistories where u.TriggeredBy.Contains(firstsplit) select u).ToList();
                List<ActionHistory> updatetargetto = (from u in dbCtxt.ActionHistories where u.TargetedTo.Contains(firstsplit) select u).ToList();
                List<ApplicationRequest> updatecurrentdesk = (from u in dbCtxt.ApplicationRequests where u.CurrentAssignedUser.Contains(firstsplit) select u).ToList();
                 List<Appointment> appointmentschedule = (from u in dbCtxt.Appointments where u.ScheduledBy.Contains(firstsplit) select u).ToList();

            if (checkifuserexist != null)
                {

                    if (checkifuserexist.UserId != email)
                    {
                        checkifuserexist.UserId = email;
                        dbCtxt.SaveChanges();

                        if (updatetriggeredby.Count > 0)
                        {
                            foreach (var item in updatetriggeredby)
                            {
                                item.TriggeredBy = email;
                                dbCtxt.SaveChanges();
                            }
                        }
                        if (updatetargetto.Count > 0)
                        {
                            foreach (var item1 in updatetargetto)
                            {
                                item1.TargetedTo = email;
                                dbCtxt.SaveChanges();
                            }
                        }
                        if (updatecurrentdesk.Count > 0)
                        {
                            foreach (var item2 in updatecurrentdesk)
                            {
                                item2.CurrentAssignedUser = email;
                                dbCtxt.SaveChanges();
                            }
                        }
                        if (appointmentschedule.Count > 0)
                        {
                            foreach (var item3 in appointmentschedule)
                            {
                                item3.ScheduledBy = email;
                                dbCtxt.SaveChanges();
                            }
                        }
                }
                }
        }



        public void UpdateAllCompanyApplicationEmail(string newemail, string oldemail, string companyname)
        {
            var appemail = (from a in dbCtxt.ApplicationRequests where a.ApplicantUserId == oldemail select a).ToList();
            var currentassigned = (from a in dbCtxt.ApplicationRequests where a.CurrentAssignedUser == oldemail select a).ToList();
            var userdetails = (from a in dbCtxt.UserMasters where a.UserId == oldemail select a).FirstOrDefault();

            var actionhistoryTriggeredBy = (from a in dbCtxt.ActionHistories where a.TriggeredBy == oldemail select a).ToList();
            var actionhistoryTargetedTo = (from a in dbCtxt.ActionHistories where a.TargetedTo == oldemail select a).ToList();
            userdetails.FirstName = companyname;
            dbCtxt.SaveChanges();
            foreach (var item in appemail)
            {
                item.ApplicantUserId = newemail;
                dbCtxt.SaveChanges();
            }
            foreach (var item0 in appemail)
            {
                item0.CurrentAssignedUser = newemail;
                dbCtxt.SaveChanges();
            }
            foreach (var item1 in actionhistoryTriggeredBy)
            {
                item1.TriggeredBy = newemail;
                dbCtxt.SaveChanges();                
            }
            foreach (var item2 in actionhistoryTargetedTo)
            {
                item2.TargetedTo = newemail;
                dbCtxt.SaveChanges();
            }
        }



        public UserMaster AddCompanyUser(ElpServiceHelper serviceIntegrator, string Email, out string errorMessage)
        {
            errorMessage = "SUCCESS";
            UserMaster userMaster = null;

            try
            {

                log.Info("Fetch Company Information from ELPS Platform with Parameter => " + Email);

                ElpsResponse elpsResponse = serviceIntegrator.GetCompanyDetailByEmail(Email);
                if (elpsResponse.message != "SUCCESS")
                {
                    log.Error(elpsResponse.message);
                    errorMessage = "An Error occured getting Company Information From Elps";
                    return null;
                }

                log.Info("About To Marshall the Company Detail Value");
                CompanyDetail companyDetail = (CompanyDetail)elpsResponse.value;
                if (companyDetail ==
                 default(CompanyDetail))
                {
                    errorMessage = "The User is Not Maintained On Elps System,Please Contact Administrator";
                    return null;
                }



                log.Info("About To Create UserMaster Information");

                userMaster = new UserMaster();
                userMaster.UserId = Email.Trim();
                userMaster.UserType = "COMPANY";
                userMaster.ElpsId = Convert.ToString(companyDetail.id);
                userMaster.CACNumber = companyDetail.rC_Number;
                userMaster.FirstName = companyDetail.name;
                userMaster.LastName = companyDetail.contact_LastName;
                userMaster.UserRoles = "COMPANY";
                userMaster.CreatedBy = "SYSTEM";
                userMaster.CreatedOn = DateTime.UtcNow;
                userMaster.Status = "ACTIVE";
                userMaster.LastLogin = DateTime.UtcNow;
                userMaster.LoginCount = 1;

                log.Info("Done with UserMaster Information");
                dbCtxt.UserMasters.Add(userMaster);
                dbCtxt.SaveChanges();

                log.Info("UserMaster Information Added to the System");

                return userMaster;

            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                userMaster = null;
                log.Error(ex.StackTrace);
            }

            return null;
        }



        public string VerifyCompanyProfile(CompanyDetail companyDetail)
        {
            string errorMessage = "SUCCESS";

            try
            {

                if (string.IsNullOrEmpty(companyDetail.contact_FirstName))
                {
                    errorMessage = "Kindly Maintain All Information On Company Profile - Contact FirstName";
                }
                else if (string.IsNullOrEmpty(companyDetail.contact_LastName))
                {
                    errorMessage = "Kindly Maintain All Information On Company Profile - Contact LastName";
                }
                else if (string.IsNullOrEmpty(companyDetail.contact_Phone))
                {
                    errorMessage = "Kindly Maintain All Information On Company Profile - Contact Phone";
                }



            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
            }

            return errorMessage;
        }





        public List<FieldLocation> GetFieldsBasedOnLoggedRole(UserMaster userMaster)
        {
            List<FieldLocation> fieldList = new List<FieldLocation>();
            List<string> userCoverageList = new List<string>();

            try
            {

                if (userMaster.UserRoles.Contains("ZONALADMIN"))
                {
                    userCoverageList = dbCtxt.ZoneFieldMappings.Where(z => z.ZoneFieldID == userMaster.UserLocation).Select(z => z.FieldLocationID).ToList<string>();
                    fieldList = dbCtxt.FieldLocations.Where(u => userCoverageList.Contains(u.FieldLocationID)).ToList();
                }
                else if (userMaster.UserRoles.Contains("HEADADMIN"))
                {
                    fieldList = dbCtxt.FieldLocations.ToList();
                }
                else
                {
                    fieldList = dbCtxt.FieldLocations.ToList();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.InnerException);
            }

            return fieldList;
        }


        public List<UserMaster> GetMaintainedStaffList(UserMaster userMaster)
        {
            List<UserMaster> userMasterList = new List<UserMaster>();
            try
            {
                log.Info("About to Get Staffs From Database based on Role =>" + userMaster.UserRoles + " with Location =>" + userMaster.UserLocation);

                if (userMaster.UserRoles.Contains("FIELDADMIN"))
                {
                    log.Info("About to Search for Field Admin");
                    userMasterList = dbCtxt.UserMasters.Where(u => (u.UserRoles.Trim() != "COMPANY") && (u.UserLocation == userMaster.UserLocation)).ToList();
                    log.Info("Done Search for Field Admin");
                }
                else if (userMaster.UserRoles.Contains("ZONALADMIN"))
                {
                    List<string> zonalFieldsList = dbCtxt.ZoneFieldMappings.Where(z => z.ZoneFieldID == userMaster.UserLocation).Select(z => z.FieldLocationID).ToList<string>();
                    if (zonalFieldsList.Count > 0)
                    {
                        userMasterList = dbCtxt.UserMasters.Where(u => u.UserRoles.Trim() != "COMPANY" && zonalFieldsList.Contains(u.UserLocation)).ToList();
                    }
                }
                else
                {
                    userMasterList = dbCtxt.UserMasters.Where(u => u.UserRoles.Trim() != "COMPANY").ToList();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);

            }

            log.Info("No of Staffs Fetched From Database for Role (" + userMaster.UserRoles + ") =>" + userMasterList.Count);

            return userMasterList;
        }




        public List<ApplicationRequest> GetFieldRoleApplications(UserMaster userMaster)
        {
            List<ApplicationRequest> applicationList = new List<ApplicationRequest>();
            try
            {
                log.Info("About to Get Applications based on Role =>" + userMaster.UserRoles);

                if (userMaster.UserRoles.Contains("FIELDADMIN"))
                {
                    applicationList = dbCtxt.ApplicationRequests.Where(u => u.CurrentOfficeLocation == userMaster.UserLocation).ToList();
                }
                else if (userMaster.UserRoles.Contains("ZONALADMIN"))
                {
                    List<string> zonalFieldsList = dbCtxt.ZoneFieldMappings.Where(z => z.ZoneFieldID == userMaster.UserLocation).Select(z => z.FieldLocationID).ToList<string>();
                    if (zonalFieldsList.Count > 0)
                    {
                        applicationList = dbCtxt.ApplicationRequests.Where(u => zonalFieldsList.Contains(u.CurrentOfficeLocation)).ToList();
                    }
                }
                else if (userMaster.UserRoles.Contains("SUPERVISOR"))
                {
                    applicationList = dbCtxt.ApplicationRequests.Where(u => u.CurrentOfficeLocation == userMaster.UserLocation).ToList();
                }


                else
                {
                    applicationList = dbCtxt.ApplicationRequests.ToList();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

            log.Info("No of Applications Fetched From Database for Role (" + userMaster.UserRoles + ") =>" + applicationList.Count);

            return applicationList;
        }



        public List<Role> GetRolesBasedOnLoggedRole(UserMaster userMaster)
        {
            List<Role> roleList = new List<Role>();
            List<string> exceptList = new List<string> {
    "HEADADMIN",
    "SUPERADMIN",
    "COMPANY"
   };

            try
            {

                if (userMaster.UserRoles.Contains("ZONALADMIN"))
                {
                    roleList = dbCtxt.Roles.Where(r => !exceptList.Contains(r.RoleId)).ToList();
                }
                else if (userMaster.UserRoles.Contains("HEADADMIN"))
                {
                    roleList = dbCtxt.Roles.Where(r => r.RoleId != "COMPANY").ToList();
                }
                else
                {
                    roleList = dbCtxt.Roles.Where(r => r.RoleId != "COMPANY").ToList();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.InnerException);
            }

            return roleList;
        }












        private List<SelectListItem> GetUsersBasedOnRoles(string userId, string branch, string roleToSearchFor)
        {
            List<SelectListItem> userListItem = new List<SelectListItem>();
            var allusers = (from u in dbCtxt.UserMasters join f in dbCtxt.FieldLocations on u.UserLocation equals f.FieldLocationID where u.UserRoles == roleToSearchFor && u.UserLocation == branch && u.Status == "ACTIVE" select new { u, f }).ToList();

            log.Info("UserID =>" + userId);
            log.Info("Input Branch =>" + branch);
            log.Info("SearchRole =>" + roleToSearchFor);

            foreach (var item in allusers)
            {

                string branchName = string.Empty;
                //UserMaster uu = dbCtxt.UserMasters.Where(c => c.UserId.Trim() == u.Trim() && c.Status == "ACTIVE").FirstOrDefault();

                if (allusers.Count > 0)
                {
                    log.Info("Checking For Retrieved User =>" + item.u.UserId);

                    string dwvalue = item.u.UserId;
                    branchName = item.u.UserId + " ( " + item.f.Description + " )";
                    userListItem.Add(new SelectListItem()
                    {
                        Text = branchName,
                        Value = dwvalue
                    });


                }
            }

            return userListItem;
        }


        public StateChart StateChart(StateChart statechart)
        {
            string res = string.Empty;

           var  paymentchart = (from l in dbCtxt.PaymentLogs 
                                join a in dbCtxt.ApplicationRequests on l.ApplicationId equals a.ApplicationId
                                select new 
                                {
                                    ApplicationId = l.ApplicationId,
                                    Abia = (Nullable<long>)(from abi in dbCtxt.PaymentLogs where a.StateCode == "ABI" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Abuja = (Nullable<long>)(from abu in dbCtxt.PaymentLogs where a.StateCode == "ABU" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Adamawa = (Nullable<long>)(from ada in dbCtxt.PaymentLogs where a.StateCode == "ADA" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    AkwaIbom = (Nullable<long>)(from akw in dbCtxt.PaymentLogs where a.StateCode == "AKW" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Anambra = (Nullable<long>)(from ana in dbCtxt.PaymentLogs where a.StateCode == "ANA" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Bauchi = (Nullable<long>)(from bau in dbCtxt.PaymentLogs where a.StateCode == "BAU" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Bayelsa = (Nullable<long>)(from bay in dbCtxt.PaymentLogs where a.StateCode == "BAY" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Benue = (Nullable<long>)(from ben in dbCtxt.PaymentLogs where a.StateCode == "BEN" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Borno = (Nullable<long>)(from bor in dbCtxt.PaymentLogs where a.StateCode == "BOR" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    CrossRiver = (Nullable<long>)(from cro in dbCtxt.PaymentLogs where a.StateCode == "CRO" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Delta = (Nullable<long>)(from del in dbCtxt.PaymentLogs where a.StateCode == "DEL" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Ebonyi = (Nullable<long>)(from ebo in dbCtxt.PaymentLogs where a.StateCode == "EBO" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Edo = (Nullable<long>)(from edo in dbCtxt.PaymentLogs where a.StateCode == "EDO" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Ekiti = (Nullable<long>)(from eki in dbCtxt.PaymentLogs where a.StateCode == "EKI" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Enugu = (Nullable<long>)(from enu in dbCtxt.PaymentLogs where a.StateCode == "ENU" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Gombe = (Nullable<long>)(from gom in dbCtxt.PaymentLogs where a.StateCode == "GOM" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Imo = (Nullable<long>)(from imo in dbCtxt.PaymentLogs where a.StateCode == "IMO" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Jigawa = (Nullable<long>)(from jig in dbCtxt.PaymentLogs where a.StateCode == "JIG" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Kaduna = (Nullable<long>)(from kad in dbCtxt.PaymentLogs where a.StateCode == "KAD" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Kano = (Nullable<long>)(from kan in dbCtxt.PaymentLogs where a.StateCode == "KAN" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Katsina = (Nullable<long>)(from kas in dbCtxt.PaymentLogs where a.StateCode == "KAT" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Kebbi = (Nullable<long>)(from keb in dbCtxt.PaymentLogs where a.StateCode == "KEB" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Kogi = (Nullable<long>)(from kog in dbCtxt.PaymentLogs where a.StateCode == "kOG" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Kwara = (Nullable<long>)(from kwa in dbCtxt.PaymentLogs where a.StateCode == "KWA" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Lagos = (Nullable<long>)(from lag in dbCtxt.PaymentLogs where a.StateCode == "LAG" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Nasarawa = (Nullable<long>)(from nas in dbCtxt.PaymentLogs where a.StateCode == "NAS" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Niger = (Nullable<long>)(from nig in dbCtxt.PaymentLogs where a.StateCode == "NIG" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Ogun = (Nullable<long>)(from ogu in dbCtxt.PaymentLogs where a.StateCode == "OGU" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Ondo = (Nullable<long>)(from ond in dbCtxt.PaymentLogs where a.StateCode == "OND" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Osun = (Nullable<long>)(from osu in dbCtxt.PaymentLogs where a.StateCode == "OSU" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Oyo = (Nullable<long>)(from oyo in dbCtxt.PaymentLogs where a.StateCode == "OYO" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Plateau = (Nullable<long>)(from pla in dbCtxt.PaymentLogs where a.StateCode == "PLA" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Rivers = (Nullable<long>)(from riv in dbCtxt.PaymentLogs where a.StateCode == "RIV" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Sokoto = (Nullable<long>)(from sok in dbCtxt.PaymentLogs where a.StateCode == "SOK" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Taraba = (Nullable<long>)(from tar in dbCtxt.PaymentLogs where a.StateCode == "TAR" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Yobe = (Nullable<long>)(from yob in dbCtxt.PaymentLogs where a.StateCode == "YOB" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                    Zamfara = (Nullable<long>)(from zam in dbCtxt.PaymentLogs where a.StateCode == "ZAM" && l.Status == "AUTH" select l.TxnAmount).FirstOrDefault(),
                                }).ToList().GroupBy(x => x.ApplicationId).Select(x => x.LastOrDefault()).ToList();

            statechart.Abia = paymentchart.ToList().Sum(x =>x.Abia);
            statechart.Abuja = paymentchart.ToList().Sum(x => x.Abuja);
            statechart.Adamawa = paymentchart.ToList().Sum(x => x.Adamawa);
            statechart.AkwaIbom = paymentchart.ToList().Sum(x => x.AkwaIbom);
            statechart.Anambra = paymentchart.ToList().Sum(x => x.Anambra);
            statechart.Bauchi = paymentchart.ToList().Sum(x => x.Bauchi);
            statechart.Bayelsa = paymentchart.ToList().Sum(x => x.Bayelsa);
            statechart.Benue = paymentchart.ToList().Sum(x => x.Benue);
            statechart.Borno = paymentchart.ToList().Sum(x => x.Borno);
            statechart.CrossRiver = paymentchart.ToList().Sum(x => x.CrossRiver);
            statechart.Delta = paymentchart.ToList().Sum(x => x.Delta);
            statechart.Ebonyi = paymentchart.ToList().Sum(x => x.Ebonyi);
            statechart.Edo = paymentchart.ToList().Sum(x => x.Edo);
            statechart.Ekiti = paymentchart.ToList().Sum(x => x.Ekiti);
            statechart.Enugu = paymentchart.ToList().Sum(x => x.Enugu);
            statechart.Gombe = paymentchart.ToList().Sum(x => x.Gombe);
            statechart.Imo = paymentchart.ToList().Sum(x => x.Imo);
            statechart.Jigawa = paymentchart.ToList().Sum(x => x.Jigawa);
            statechart.Kaduna = paymentchart.ToList().Sum(x => x.Kaduna);
            statechart.Kano = paymentchart.ToList().Sum(x => x.Kano);
            statechart.Katsina = paymentchart.ToList().Sum(x => x.Katsina);
            statechart.Kebbi = paymentchart.ToList().Sum(x => x.Kebbi);
            statechart.Kogi = paymentchart.ToList().Sum(x => x.Kogi);
            statechart.Kwara = paymentchart.ToList().Sum(x => x.Kwara);
            statechart.Lagos = paymentchart.ToList().Sum(x => x.Lagos);
            statechart.Nasarawa = paymentchart.ToList().Sum(x => x.Nasarawa);
            statechart.Niger = paymentchart.ToList().Sum(x => x.Niger);
            statechart.Ogun = paymentchart.ToList().Sum(x => x.Ogun);
            statechart.Ondo = paymentchart.ToList().Sum(x => x.Ondo);
            statechart.Osun = paymentchart.ToList().Sum(x => x.Osun);
            statechart.Oyo = paymentchart.ToList().Sum(x => x.Oyo);
            statechart.Plateau = paymentchart.ToList().Sum(x => x.Plateau);
            statechart.Rivers = paymentchart.ToList().Sum(x => x.Rivers);
            statechart.Sokoto = paymentchart.ToList().Sum(x => x.Sokoto);
            statechart.Taraba = paymentchart.ToList().Sum(x => x.Taraba);
            statechart.Yobe = paymentchart.ToList().Sum(x => x.Yobe);
            statechart.Zamfara = paymentchart.ToList().Sum(x => x.Zamfara);
            return statechart;
        }
    }
}