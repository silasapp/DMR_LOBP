using log4net;
using LOBP.DbEntities;
using LOBP.Helper;
using LOBP.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data.Entity;
using Newtonsoft.Json.Linq;
using Microsoft.Ajax.Utilities;

namespace LOBP.Controllers
{

    [LOBP.Filters.SessionExpireTracker]
    public class AdminController : Controller
    {

        private string companyElpsId;
        private UserMaster userMaster = new UserMaster();
        private UtilityHelper commonHelper = new UtilityHelper();
        private UserHelper userMasterHelper = new UserHelper();
        private WorkFlowHelper workflowHelper = new WorkFlowHelper();
        private ApplicationDocHelper appDocHelper;
        private ConfigurationHelper configurationHelper = new ConfigurationHelper();
        private ElpServiceHelper serviceIntegrator;
        private PermitDTO permitDto = new PermitDTO();
        private CultureInfo ukCulture = new CultureInfo("en-GB");
        public static List<Staff> AllStaffs = new List<Staff>();
        private LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities();
        private LicenseHelper licenseHelper = new LicenseHelper();
        private ILog logger = log4net.LogManager.GetLogger(typeof(AdminController));
        private ElpServiceHelper elpServiceHelper = new ElpServiceHelper();
        long SSARevenue = 0, ATCRevenue = 0, LTORevenue = 0, ATORevenue = 0, PTERevenue = 0, TITARevenue = 0, ATMRevenue = 0, ATCLFPRevenue = 0,
        LTOLFPRevenue = 0, TPBAPLWRevenue = 0, TPBAPRWRevenue = 0, RevenueGrandTotal = 0, TCARevenue = 0; 
          string SSAIGR = "", SSABrandOne = "", ATOBrandOne = "", ATOIGR = "", ATMBrandOne = "", ATMIGR = "", ATCLFPBrandOne = "", ATCLFPIGR = "", LTOLFPBrandOne = "", LTOLFPIGR = "",
            TPBAPLWBrandOne = "", TPBAPLWIGR = "", TPBAPRWBrandOne = "", TPBAPRWIGR = "", TITABrandOne = "", TITAIGR = "", TCABrandOne = "", TCAIGR = "", GrandTotalIGR = "", GrandTotalBrandOne = "", GrandTotalFG = "";

        public AdminController() { }
        protected override void Initialize(RequestContext requestContext)
        {
            try
            {
                base.Initialize(requestContext);
                userMaster = (UserMaster)Session["UserID"];
                userMasterHelper = new UserHelper();
                commonHelper = new UtilityHelper();
                // workflowHelper = new WorkFlowHelper(dbCtxt);
                appDocHelper = new ApplicationDocHelper(dbCtxt);
                serviceIntegrator = new ElpServiceHelper(GlobalModel.elpsUrl, GlobalModel.appEmail, GlobalModel.appKey);
            }
            catch (Exception ex)
            {
                logger.Error(ex.InnerException);
            }
        }

        // GET: /Admin/
        public ActionResult Index()
        {
            int totalPermitCount = 0;
            int totalAppWorkedOn = 0;
            int totalCancelled = 0;
            String errorMessage = null;
            string branchName = string.Empty;



            try
            {
                branchName = dbCtxt.FieldLocations.Where(f => f.FieldLocationID == userMaster.UserLocation).FirstOrDefault().Description;
                ViewBag.UserBranch = branchName;
                Session["UserBranch"] = branchName;

                foreach (ApplicationRequest appRequest in dbCtxt.ApplicationRequests.ToList())
                {
                    switch (dbCtxt.WorkFlowStates.Where(w => w.StateID == appRequest.CurrentStageID).FirstOrDefault().StateType)
                    {
                        case "COMPLETE":
                            totalPermitCount++;
                            break;
                        case "PROGRESS":
                            totalAppWorkedOn++;
                            break;
                        case "REJECTED":
                            totalCancelled++;
                            break;
                        default:
                            break;
                    }
                }
                logger.Info("totalPermitCount =>" + totalPermitCount);
                logger.Info("totalAppWorkedOn =>" + totalAppWorkedOn);
                logger.Info("totalCancelled =>" + totalCancelled);

                List<ApplicationRequest> myAppDeskCountList = userMasterHelper.GetApprovalRequest(dbCtxt, userMaster, out errorMessage);
                logger.Info("OnMyDeskCount =>" + myAppDeskCountList.Count);
                ViewBag.OnMyDeskCount = myAppDeskCountList.Count;
                Session["OnMyDeskCount"] = ViewBag.OnMyDeskCount;
                ViewBag.TotalApplicationWorkedOn = (from a in dbCtxt.ApplicationRequests where a.Status == "Processing" select a).ToList().Count(); //totalAppWorkedOn;
                ViewBag.PermitCount = (from a in dbCtxt.ApplicationRequests where a.Status == "Approved" select a).ToList().Count();//totalPermitCount;
                ViewBag.TotalRejection = (from a in dbCtxt.ApplicationRequests where a.Status == "Rejected" select a).ToList().Count(); //totalCancelled;
                ViewBag.TotalExpired = (from a in dbCtxt.ApplicationRequests where a.LicenseReference != null && a.LicenseExpiryDate <= DateTime.Now && a.IsLegacy == "NO" select a).ToList().Count(); //totalCancelled;
                ViewBag.TotalNonExpired = (from a in dbCtxt.ApplicationRequests where a.LicenseReference != null && a.LicenseExpiryDate > DateTime.Now && a.IsLegacy == "NO" select a).ToList().Count(); //totalCancelled;
                ViewBag.ErrorMessage = errorMessage;
                var pastthreeweek = commonHelper.GetApplicationForPastThreeWks(userMaster.UserId, dbCtxt);
                var pastfivedays = commonHelper.GetApplicationForPastFiveDays(userMaster.UserId, dbCtxt);
                ViewBag.Pastfivedays = pastfivedays;
                ViewBag.Pastwksapp = pastthreeweek;
            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error Occured on Admin DashBoard, Please try again Later";
            }

            return View();
        }

        // GET: /Admin/MyDesk
        [HttpGet]
        public ActionResult MyDesk()
        {
            String errorMessage = null;
            List<ApplicationRequest> appRequestList = new List<ApplicationRequest>();
            List<ApplicationRequest> appRequestList1 = new List<ApplicationRequest>();
            try
            {

                //appRequestList = userMasterHelper.GetApprovalRequest(dbCtxt, userMaster, out errorMessage);
                appRequestList = dbCtxt.ApplicationRequests.Where(x => x.CurrentAssignedUser.Trim().ToLower().Equals(userMaster.UserId.Trim().ToLower())).ToList();
                logger.Info("Application Returned Count =>" + appRequestList.Count);
                foreach (ApplicationRequest item in dbCtxt.ApplicationRequests.Where(a => a.CurrentStageID == 5 && a.CurrentAssignedUser == userMaster.UserId))
                {
                    appRequestList1.Add(new ApplicationRequest() { CurrentStageID = item.CurrentStageID });
                }
                ViewBag.StaffAppCount = appRequestList1.ToList().Count();
                ViewBag.ErrorMessage = errorMessage;
                ViewBag.Loggedinuser = userMaster.UserRoles;
            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error Occured when calling MyDesk, Please try again Later";
            }

            return View(appRequestList);
        }

        public ActionResult AllApplications(string location)
        {
            var locationnum = userMaster.UserLocation;
            List<ApplicationRequest> apps = null;
            apps = (from a in dbCtxt.ApplicationRequests where a.IsLegacy == "NO" select a).ToList();
            if (location == "Stafflocation")
            {
                apps = (from a in dbCtxt.ApplicationRequests where a.IsLegacy == "NO" && a.CurrentOfficeLocation == locationnum select a).ToList();
            }
            else if (location == "Allstafflocation")
            {
                apps = (from a in dbCtxt.ApplicationRequests where a.IsLegacy == "NO" select a).ToList();

            }
            if (userMaster.UserRoles == "EDSTA")
            {
                apps = (from a in dbCtxt.ApplicationRequests where a.CurrentStageID == 19 select a).ToList();
            }
            else if (userMaster.UserRoles == "ACESTA")
            {
                apps = (from a in dbCtxt.ApplicationRequests where a.CurrentStageID == 20 select a).ToList();
            }
            ViewBag.LoggedOnRole = userMaster.UserRoles;
            return View(apps);
        }

        [HttpPost]
        public ActionResult EditMySchdule(FormCollection coll)
        {
            try
            {
                var appid = coll.Get("appref");
                var Appntdate = coll.Get("appntdate");
                var updateschdule = (from s in dbCtxt.Appointments where s.ApplicationId == appid select s).FirstOrDefault();
                var companyEmail = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == appid select a.ApplicantUserId).FirstOrDefault();
                updateschdule.AppointmentDate = Convert.ToDateTime(Appntdate);
                updateschdule.ScheduledDate = DateTime.Now;
                updateschdule.SchduleExpiryDate = DateTime.Now.AddDays(3);
                dbCtxt.SaveChanges();
                TempData["Saved"] = "saved";

                var subject = "Inspection Re-Scheduled";
                var content = " Inspection date " + Convert.ToDateTime(Appntdate) + " has been rescheduled for Application with the reference number " + appid + " please note: Your inspection approval will expire on " + DateTime.Now.AddDays(3);
                var sendmail = userMasterHelper.SendStaffEmailMessage(companyEmail, subject, content);

            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }
            return RedirectToAction("ActiveSchedule");
        }

        public ActionResult ActiveSchedule()
        {
            List<ScheduleModel> Myschedulelist = new List<ScheduleModel>();
            var schedules = (from s in dbCtxt.Appointments join a in dbCtxt.ApplicationRequests on s.ApplicationId equals a.ApplicationId where s.ScheduledBy == userMaster.UserId select new { s, a }).ToList();

            foreach (var item in schedules)
            {
                Myschedulelist.Add(new ScheduleModel()
                {
                    ApplicationId = item.s.ApplicationId,
                    CompanyName = item.a.ApplicantName,
                    AppointmentDate = Convert.ToDateTime(item.s.AppointmentDate),
                    ScheduledDate = Convert.ToDateTime(item.s.ScheduledDate),
                    ApplicationType = item.s.LicenseTypeId,
                    ScheduledBy = item.s.ScheduledBy,
                    ScheduleExpiredDate = Convert.ToDateTime(item.s.SchduleExpiryDate)


                });
            }

            return View(Myschedulelist);
        }

        [HttpGet]
        public ActionResult StaffTaskAssignment(string userid, string role)
        {
            string ErrorMessage = "";
            ViewBag.UserId = userid;
            ViewBag.Rolestaff = role;
            List<ApplicationRequest> appRequestList;
            UserMaster up = dbCtxt.UserMasters.Where(c => c.UserId.Trim() == userid.Trim()).FirstOrDefault();
            appRequestList = userMasterHelper.GetApprovalRequest(dbCtxt, up, out ErrorMessage);
            ViewBag.ErrorMessage = ErrorMessage;
            return PartialView("TaskAssignment", appRequestList);
        }

        [HttpPost]
        public JsonResult GetNewAssignedUser(string myrole, string myoldemail)
        {


            var userlocation = (from u in dbCtxt.UserMasters where u.UserId == myoldemail select u.UserLocation).FirstOrDefault();
            var Newuseremail = (from r in dbCtxt.UserMasters
                                where r.UserLocation == userlocation && r.UserRoles == myrole && r.Status == "ACTIVE" && r.UserId != myoldemail
                                select new
                                {
                                    newuser = r.UserId
                                }).ToList();
            return Json(Newuseremail, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Rerouteuser(FormCollection coll)
        {
            try
            {
                var arrayitem = coll.Get("arrayitem");
                var adcomments = coll.Get("adcomment");
                string[] itemlist = arrayitem.Split(',');
                var newassigned = coll.Get("newassigned");
                if (arrayitem == "" || arrayitem == null)
                {
                    TempData["Message"] = "Application(s) to be Assigned Was not Selected";
                }
                else if (newassigned == "" || newassigned == null)
                {
                    TempData["Message"] = "Please Select New Assigned User";
                }
                else
                {
                    using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
                    {
                        foreach (var item in itemlist)
                        {
                            var apprequest = (from a in db.ApplicationRequests where a.ApplicationId == item select a).FirstOrDefault();
                            var apphistry = (from a in db.ActionHistories where a.ApplicationId == item select a).ToList().LastOrDefault();

                            if (apprequest != null)
                            {
                                apprequest.CurrentAssignedUser = newassigned;
                                apprequest.ModifiedDate = DateTime.Now;
                                db.SaveChanges();
                                var subject = "Re-Routed Application";
                                string content = "Application with the reference number " + apprequest.ApplicationId + " has been re-routed to your desk.";
                                var sendemail = userMasterHelper.SendStaffEmailMessage(newassigned, subject, content);

                            }
                            if (apphistry != null)
                            {
                                var v = adcomments == "" ? "Application was Re-Assigned to " + newassigned : adcomments;
                                apphistry.MESSAGE = v;
                                db.SaveChanges();
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            return RedirectToAction("StaffDesk");

        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetChart(RatioDash ratio)
        {

            int inprogressLegacy = (from a in dbCtxt.ApplicationRequests join w in dbCtxt.WorkFlowStates on a.CurrentStageID equals w.StateID where a.IsLegacy == "YES" && w.StateType == "PROGRESS" select a).ToList().Count();
            int inprogressOnline = (from a in dbCtxt.ApplicationRequests where a.Status == "Processing" select a).ToList().Count();//(from a in dbCtxt.ApplicationRequests join w in dbCtxt.WorkFlowStates on a.CurrentStageID equals w.StateID where a.LicenseReference == null && a.IsLegacy == "NO" && w.StateType == "PROGRESS" select a).ToList().Count();
            int issued = (from a in dbCtxt.ApplicationRequests where a.LicenseReference != null select a).ToList().Count();
            int approved = (from a in dbCtxt.ApplicationRequests where a.Status == "Approved" select a).ToList().Count();//(from a in dbCtxt.ActionHistories where a.Action == "Accept" && a.TriggeredBy == userMaster.UserId select a).ToList().Count();
            int rejected = (from a in dbCtxt.ApplicationRequests where a.Status == "Rejected" select a).ToList().Count();//(from a in dbCtxt.ActionHistories where a.Action == "Reject" && a.TriggeredBy == userMaster.UserId select a).ToList().Count();
            ratio.Approved = approved;
            ratio.OnDesk = Convert.ToInt32(Session["OnMyDeskCount"]);
            ratio.Processed = issued;
            ratio.Rejected = rejected;
            ratio.OnlineInProgress = inprogressOnline;
            ratio.LegacyInProgress = inprogressLegacy;
            return Json(ratio, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult TaskDelegation()
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var searchTxt = Request.Form.GetValues("search[value]")[0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var useremail = (from u in dbCtxt.UserMasters where u.UserRoles == "AD RBP" select u.UserId).ToList();
            var Applications = (from a in dbCtxt.ApplicationRequests
                                where a.WorkFlowState.StateType == "PROGRESS" && a.CurrentStageID == 5 && (a.LicenseReference == null || a.LicenseReference == "") && useremail.Contains(a.CurrentAssignedUser)
                                select new { a.ApplicationId, a.ApplicantName, category = a.ApplicationTypeId + ", " + a.LicenseTypeId, a.ApplicantUserId, a.CurrentAssignedUser });
            if (!string.IsNullOrEmpty(searchTxt))
            {
                Applications = Applications.Where(s => s.ApplicationId.Contains(searchTxt) || s.ApplicantName.Contains(searchTxt) || s.category.Contains(searchTxt) || s.ApplicantUserId.Contains(searchTxt) || s.CurrentAssignedUser.Contains(searchTxt));
            }
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                Applications = Applications.OrderBy(p => p.ApplicationId + " " + sortColumnDir);
            }


            totalRecords = Applications.Count();
            var data = Applications.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult TaskDelegationList()
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var searchTxt = Request.Form.GetValues("search[value]")[0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            string staffemail = Request.Form.Get("mystaffemail");

            var Applications = (from a in dbCtxt.ApplicationRequests
                                where a.WorkFlowState.StateType == "PROGRESS" && staffemail.Contains(a.CurrentAssignedUser)
                                select new { a.ApplicationId, a.ApplicantName, a.ApplicationTypeId, a.ApplicantUserId, a.CurrentAssignedUser });

            if (!string.IsNullOrEmpty(searchTxt))
            {
                Applications = Applications.Where(s => s.ApplicationId.Contains(searchTxt) || s.ApplicantName.Contains(searchTxt) || s.ApplicationTypeId.Contains(searchTxt) || s.ApplicantUserId.Contains(searchTxt) || s.CurrentAssignedUser.Contains(searchTxt));
            }
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                Applications = Applications.OrderBy(p => p.ApplicationId + " " + sortColumnDir);
            }


            totalRecords = Applications.Count();
            var data = Applications.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult TaskDelegationUsers(string ApplicationId)
        {

            var userRole = (from r in dbCtxt.UserMasters
                            where ((r.UserRoles.Contains("SUPERVISOR") || r.UserRoles.Contains("REVIEWER")) && r.Status == "ACTIVE") && (r.UserLocation == userMaster.UserLocation)
                            select new
                            {
                                role = r.UserId + " (" + r.FirstName + " " + r.LastName + ")"
                            }).ToList();
            return Json(userRole, JsonRequestBehavior.AllowGet);

        }

        public ActionResult PrintedLicense()
        {
            return View();
        }

        [HttpGet]
        public ActionResult NonPrintedLicense()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetLicensePrinted(FormCollection collection)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var searchTxt = Request.Form.GetValues("search[value]")[0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;

            var staff = (from p in dbCtxt.ApplicationRequests
                         where p.LicenseReference != null && p.LicenseReference != "" && p.PrintedStatus == "Printed" && p.IsLegacy != "YES"
                         select new
                         {
                             p.ApplicationId,
                             p.LicenseReference,
                             p.ApplicantName,
                             p.LicenseTypeId,
                             p.ApplicationTypeId,
                             p.ApplicantUserId,
                             IssuedDate = p.LicenseIssuedDate.ToString(),
                             ExpiryDate = p.LicenseExpiryDate.ToString(),
                             AppliedDate = p.AddedDate.ToString(),
                             description = (from t in dbCtxt.LicenseTypes where t.LicenseTypeId == p.LicenseReference select t.ShortName).FirstOrDefault(),
                             takeoverappid = (from t in dbCtxt.TakeoverApps where t.LicenseReference == p.LicenseReference select p.LicenseTypeId).FirstOrDefault()

                         });

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.ApplicantName.Contains(searchTxt) || a.AppliedDate.Contains(searchTxt)
               || a.LicenseTypeId.Contains(searchTxt) || a.ApplicationTypeId.Contains(searchTxt) || a.ApplicantUserId.Contains(searchTxt) || a.IssuedDate.Contains(searchTxt) || a.ExpiryDate.Contains(searchTxt));
            }
            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            switch (sortColumn)
            {
                case "0":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationId).ToList() : data.OrderBy(p => p.ApplicationId).ToList();
                    break;
                case "1":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicantName).ToList() : data.OrderBy(p => p.ApplicantName).ToList();
                    break;
                case "2":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.LicenseTypeId).ToList() : data.OrderBy(p => p.LicenseTypeId).ToList();
                    break;
                case "3":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationTypeId).ToList() : data.OrderBy(p => p.ApplicationTypeId).ToList();
                    break;
                case "4":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicantUserId).ToList() : data.OrderBy(p => p.ApplicantUserId).ToList();
                    break;
                case "5":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.LicenseReference).ToList() : data.OrderBy(p => p.LicenseReference).ToList();
                    break;
            }
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetNonPrintedLicense(FormCollection collection)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var searchTxt = Request.Form.GetValues("search[value]")[0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;


            var staff = (from p in dbCtxt.ApplicationRequests
                         where p.LicenseReference != null && p.LicenseReference != "" && p.PrintedStatus == "Not Printed" && p.IsLegacy != "YES"
                         select new
                         {
                             p.ApplicationId,
                             p.LicenseReference,
                             p.ApplicantName,
                             p.LicenseTypeId,
                             p.LinkedReference,
                             p.ApplicationTypeId,
                             p.ApplicantUserId,
                             IssuedDate = p.LicenseIssuedDate.ToString(),
                             ExpiryDate = p.LicenseExpiryDate.ToString(),
                             AppliedDate = p.AddedDate.ToString(),
                             description = (from t in dbCtxt.LicenseTypes where t.LicenseTypeId == p.LicenseReference select t.ShortName).FirstOrDefault(),
                             takeoverappid = (from t in dbCtxt.TakeoverApps where t.LicenseReference == p.LicenseReference select p.LicenseTypeId).FirstOrDefault()

                         });

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.ApplicantName.Contains(searchTxt) || a.AppliedDate.Contains(searchTxt)
               || a.LicenseTypeId.Contains(searchTxt) || a.ApplicationTypeId.Contains(searchTxt) || a.ApplicantUserId.Contains(searchTxt) || a.IssuedDate.Contains(searchTxt) || a.ExpiryDate.Contains(searchTxt));
            }
            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            switch (sortColumn)
            {
                case "0":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationId).ToList() : data.OrderBy(p => p.ApplicationId).ToList();
                    break;
                case "1":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicantName).ToList() : data.OrderBy(p => p.ApplicantName).ToList();
                    break;
                case "2":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.LicenseTypeId).ToList() : data.OrderBy(p => p.LicenseTypeId).ToList();
                    break;
                case "3":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationTypeId).ToList() : data.OrderBy(p => p.ApplicationTypeId).ToList();
                    break;
                case "4":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicantUserId).ToList() : data.OrderBy(p => p.ApplicantUserId).ToList();
                    break;
                case "5":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.LicenseReference).ToList() : data.OrderBy(p => p.LicenseReference).ToList();
                    break;
            }
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult AssignTaskModal(FormCollection getvalue)
        {
            try
            {
                var arrayitem = getvalue.Get("arrayitem");
                var adcomments = getvalue.Get("adcomment");
                string[] itemlist = arrayitem.Split(',');
                var newassigned = getvalue.Get("Newtask");
                if (arrayitem == "" || arrayitem == null)
                {
                    ViewBag.Message = "Application(s) to be Assigned Was not Selected";
                }
                else if (newassigned == "" || newassigned == null)
                {
                    ViewBag.Message = "Please Select Supervisor to Assign Task";
                }
                using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
                {
                    foreach (var item in itemlist)
                    {
                        ///var apprequest = (from a in db.ApplicationRequests where a.ApplicationID == item select a).FirstOrDefault();
                       // var apphistry = (from a in db.ActionHistories where a.ApplicationID == item && a.CurrentStageID == 5 select a).ToList().LastOrDefault();
                        var comments = adcomments == "" ? "Documents Uploaded by Markerter" : adcomments;
                        var fieldlocation = userMaster.UserLocation;
                        if (item != null && item != "")
                        {
                            ResponseWrapper responseWrapper = workflowHelper.processAction(dbCtxt, item, "Delegate", userMaster.UserId, comments, fieldlocation, newassigned);

                        }
                        else
                        {
                            ViewBag.Message = "Something went wrong please try again later";
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

            return RedirectToAction("Index");

        }

        [HttpGet]
        public ActionResult ConfirmExtraPayment()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetConfirmExtraPayment()
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var searchTxt = Request.Form.GetValues("search[value]")[0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;
            var staff = (from p in dbCtxt.ExtraPayments
                         join a in dbCtxt.ApplicationRequests on p.ApplicationID equals a.ApplicationId
                         // where p.Status != "AUTH"
                         select new
                         {
                             p.ExtraPaymentAppRef,
                             a.ApplicantName,
                             p.Status,
                             p.RRReference,
                             pay = p.ExtraPaymentID.ToString(),
                             amt = p.TxnAmount.ToString(),
                             transdate = p.TransactionDate.ToString(),
                             transDATE = p.TransactionDate
                         });
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ExtraPaymentAppRef + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(a => a.ExtraPaymentAppRef.Contains(searchTxt) || a.ExtraPaymentAppRef.Contains(searchTxt)
               || a.Status.Contains(searchTxt) || a.RRReference.Contains(searchTxt) || a.pay.Contains(searchTxt) || a.amt.Contains(searchTxt)
               || a.transdate.Contains(searchTxt));
            }
            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            switch (sortColumn)
            {
                case "0":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ExtraPaymentAppRef).ToList() : data.OrderBy(p => p.ExtraPaymentAppRef).ToList();
                    break;
                case "1":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.RRReference).ToList() : data.OrderBy(p => p.RRReference).ToList();
                    break;
                case "2":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.pay).ToList() : data.OrderBy(p => p.pay).ToList();
                    break;
                case "3":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.amt).ToList() : data.OrderBy(p => p.amt).ToList();
                    break;
                case "4":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.transdate).ToList() : data.OrderBy(p => p.transdate).ToList();
                    break;
                case "5":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.transDATE).ToList() : data.OrderBy(p => p.transDATE).ToList();
                    break;
            }
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult ExtraPaymentConfirmation(FormCollection coll)
        {
            try
            {
                var RRR = coll.Get("myrrr");
                var Appid = coll.Get("myappid");
                ResponseWrapper responseWrapper = null;
                var rrr = (from p in dbCtxt.ExtraPayments where p.ExtraPaymentAppRef == Appid || p.RRReference == RRR select p.RRReference).FirstOrDefault();
                var changestatus = (from p in dbCtxt.ExtraPayments where p.ExtraPaymentAppRef == Appid select p).FirstOrDefault();
                var changecurrentstage = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == Appid select a).FirstOrDefault();
                var rrrCheck = GlobalModel.elpsUrl + "/Payment/checkifpaid?id=r" + rrr;
                var res = serviceIntegrator.CheckRRR(rrr);
                var resp = JsonConvert.DeserializeObject<JObject>(res.value.ToString());
                if (resp.GetValue("message").ToString() == "Approved" || resp.GetValue("status").ToString() == "01")
                {

                    if (changecurrentstage != null)
                    {
                        if (changecurrentstage.Status == "Rejected")
                        {
                            responseWrapper = workflowHelper.processAction(dbCtxt, Appid, "ReSubmit", changecurrentstage.ApplicantUserId, "Application Resubmited For Reprocessing By Company", changecurrentstage.CurrentOfficeLocation, "");

                        }
                        else if (changecurrentstage.CurrentStageID < 5)
                        {
                            responseWrapper = workflowHelper.processAction(dbCtxt, Appid, "Proceed", userMaster.UserId, "Document Submitted", changecurrentstage.CurrentOfficeLocation, "");

                            responseWrapper = workflowHelper.processAction(dbCtxt, Appid, "GenerateRRR", changecurrentstage.ApplicantUserId, "Remita Retrieval Reference Generated", changecurrentstage.CurrentOfficeLocation, "");

                            responseWrapper = workflowHelper.processAction(dbCtxt, Appid, "Submit", changecurrentstage.ApplicantUserId, "Application Reference " + Appid + " have been Submitted to DPR", changecurrentstage.CurrentOfficeLocation, "");

                        }
                    }
                    changestatus.Status = "AUTH";
                    changestatus.TxnMessage = "Confirmed";
                    dbCtxt.SaveChanges();
                    var Elps = (from a in dbCtxt.ApplicationRequests
                                join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                                where a.ApplicationId == changestatus.ApplicationID && a.ApplicantUserId == u.UserId
                                select new { u.ElpsId, a.Status }).FirstOrDefault();
                    serviceIntegrator.GetElpAppUpdateStatus(changestatus.ApplicationID, Elps.ElpsId, Elps.Status);

                    //userMasterHelper.AutoAssignApplication(dbCtxt, Appid, "SUPERVISOR");

                }
                else { ViewBag.Message = "pending"; }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

            return RedirectToAction("ConfirmExtraPayment");
        }

        [HttpPost]
        public ActionResult PaymentConfirmation(FormCollection coll)
        {
            try
            {
                var RRR = coll.Get("myrrr");
                var Appid = coll.Get("myappid");
                ResponseWrapper responseWrapper = null;
                var rrr = (from p in dbCtxt.PaymentLogs where p.ApplicationId == Appid || p.RRReference == RRR select p.RRReference).FirstOrDefault();
                var changestatus = (from p in dbCtxt.PaymentLogs where p.ApplicationId == Appid select p).FirstOrDefault();
                var changecurrentstage = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == Appid select a).FirstOrDefault();
                var rrrCheck = GlobalModel.elpsUrl + "/Payment/checkifpaid?id=r" + rrr;
                var res = serviceIntegrator.CheckRRR(rrr);
                var resp = JsonConvert.DeserializeObject<JObject>(res.value.ToString());
                if (resp.GetValue("message").ToString() == "Approved" || resp.GetValue("message").ToString() == "Successful" || resp.GetValue("status").ToString() == "01")
                {
                    if (changecurrentstage != null)
                    {
                        if (changecurrentstage.Status == "Rejected")
                        {
                            responseWrapper = workflowHelper.processAction(dbCtxt, Appid, "ReSubmit", changecurrentstage.ApplicantUserId, "Application Resubmited For Reprocessing By Company", changecurrentstage.CurrentOfficeLocation, "");

                        }
                        else if (changecurrentstage.CurrentStageID < 5)
                        {
                            responseWrapper = workflowHelper.processAction(dbCtxt, Appid, "Proceed", userMaster.UserId, "Document Submitted", changecurrentstage.CurrentOfficeLocation, "");

                            responseWrapper = workflowHelper.processAction(dbCtxt, Appid, "GenerateRRR", changecurrentstage.ApplicantUserId, "Remita Retrieval Reference Generated", changecurrentstage.CurrentOfficeLocation, "");

                            responseWrapper = workflowHelper.processAction(dbCtxt, Appid, "Submit", changecurrentstage.ApplicantUserId, "Application Reference " + Appid + " have been Submitted to DPR", changecurrentstage.CurrentOfficeLocation, "");

                        }
                    }
                    changestatus.Status = "AUTH";
                    changestatus.TxnMessage = "Confirmed";
                    dbCtxt.SaveChanges();

                    var Elps = (from a in dbCtxt.ApplicationRequests
                                join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                                where a.ApplicationId == Appid && a.ApplicantUserId == u.UserId
                                select new { u.ElpsId, a.Status }).FirstOrDefault();

                    serviceIntegrator.GetElpAppUpdateStatus(Appid, Elps.ElpsId, Elps.Status);

                    // userHelper.AutoAssignApplication(Appid, "SUPERVISOR");

                }
                else { ViewBag.Message = "pending"; }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return RedirectToAction("ConfirmPayment");
        }

        [HttpGet]
        public ActionResult ConfirmPayment()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetConfirmPayment()
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var searchTxt = Request.Form.GetValues("search[value]")[0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;
            var staff = (from p in dbCtxt.PaymentLogs
                         join a in dbCtxt.ApplicationRequests on p.ApplicationId equals a.ApplicationId
                         // where p.Status != "AUTH"
                         select new
                         {
                             p.ApplicationId,
                             a.ApplicantName,
                             p.Status,
                             p.RRReference,
                             pay = p.PaymentId.ToString(),
                             amt = p.TxnAmount.ToString(),
                             transdate = p.TransactionDate.ToString(),
                             transDATE = p.TransactionDate
                         });
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.ApplicationId.Contains(searchTxt)
               || a.Status.Contains(searchTxt) || a.RRReference.Contains(searchTxt) || a.pay.Contains(searchTxt) || a.amt.Contains(searchTxt)
               || a.transdate.Contains(searchTxt) || a.ApplicantName.Contains(searchTxt));
            }
            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            switch (sortColumn)
            {
                case "0":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationId).ToList() : data.OrderBy(p => p.ApplicationId).ToList();
                    break;
                case "1":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.RRReference).ToList() : data.OrderBy(p => p.RRReference).ToList();
                    break;
                case "2":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.pay).ToList() : data.OrderBy(p => p.pay).ToList();
                    break;
                case "3":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.amt).ToList() : data.OrderBy(p => p.amt).ToList();
                    break;
                case "4":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.transdate).ToList() : data.OrderBy(p => p.transdate).ToList();
                    break;
                case "5":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.transDATE).ToList() : data.OrderBy(p => p.transDATE).ToList();
                    break;
            }
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult Checkpayment(string Appid, string RRR)
        {
            string status = "";
            try
            {

                var rrr = (from p in dbCtxt.PaymentLogs where p.ApplicationId == Appid || p.RRReference == RRR select p.RRReference).FirstOrDefault();
                var changestatus = (from p in dbCtxt.PaymentLogs where p.ApplicationId == Appid select p).FirstOrDefault();
                var rrrCheck = GlobalModel.elpsUrl + "/Payment/checkifpaid?id=r" + rrr;
                var res = serviceIntegrator.CheckRRR(rrr);
                var resp = JsonConvert.DeserializeObject<JObject>(res.value.ToString());
                if (resp.GetValue("message").ToString() == "Approved" || resp.GetValue("status").ToString() == "01")
                {
                    status = "paid";
                }
                else { status = "pending"; }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return Json(new { Status = status }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAllElpsDocTypes()
        {
            ElpsResponse elpsResponse = new ElpsResponse();
            var doclist = configurationHelper.GetAllElpsDoc();

            List<DocumentModel> DocumentList = doclist;

            var AllDocumentNames = (from u in DocumentList select new { docid = u.DocId, docname = u.DocumentName }).ToList();
            return Json(AllDocumentNames, JsonRequestBehavior.AllowGet);

        }

        public ActionResult DocumentConfiguration()
        {
            ViewBag.AllRequiredDoc = configurationHelper.RequiredDocumentConfiguration();
            ViewBag.AllLegacyDoc = configurationHelper.LegacyDocumentConfiguration();

            return View();
        }

        public JsonResult AddOnlineDocument(string LicenseTypeId, string ApplicationType, int Docid, string docname, int Serialnum, string IsbaseTrans, string Ismandatory, string Islicensedoc, string status)
        {
            string Response = configurationHelper.AddDocumentOnline(LicenseTypeId, ApplicationType, Docid, docname, Serialnum, IsbaseTrans, Ismandatory, Islicensedoc, status);
            return Json(Response);
        }
        public JsonResult DeleteOnlineDocument(string LicenseTypeId, string ApplicationTypeId, int docID)
        {
            string Response = configurationHelper.DeleteDocumentOnline(LicenseTypeId, ApplicationTypeId, docID);

            return Json(Response);
        }

        public JsonResult AddLegacyDocument(string LicenseTypeId, int Docid, string docname, int Serialnum, string IsbaseTrans, string Ismandatory, string status)
        {
            string Response = configurationHelper.AddDocumentLegacy(LicenseTypeId, Docid, docname, Serialnum, IsbaseTrans, Ismandatory, status);
            return Json(Response);
        }
        public JsonResult DeleteLegacyDocument(string LicenseTypeId, int docID)
        {
            string Response = configurationHelper.DeleteDocumentLegacy(LicenseTypeId, docID);

            return Json(Response);
        }

        [HttpGet]
        public JsonResult GetStaffForInspection()
        {

            var Staffnames = (from u in dbCtxt.UserMasters where ((u.UserRoles == "REVIEWER" || u.UserRoles == "SUPERVISOR") && (u.UserLocation == userMaster.UserLocation)) select new { staffnames = u.UserId, namerole = u.UserId + " " + "(" + u.FieldLocation.Description + ")" }).ToList();
            return Json(Staffnames, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetInspectionType(string ApplicationId)
        {
            var licensetype = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == ApplicationId select a.LicenseTypeId).FirstOrDefault();
            var Inspectiontypes = (licensetype == "LTO" || licensetype == "LTOLFP" || licensetype == "SSA" || licensetype == "TITA" || licensetype == "TCA") ? (from u in dbCtxt.InspectionTypeMasterLists select new { inspectionid = u.Id, inspectionname = u.Description }).ToList() : (from u in dbCtxt.InspectionTypeMasterLists where u.Id == "HOI" select new { inspectionid = u.Id, inspectionname = u.Description }).ToList();
            return Json(Inspectiontypes, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult AddExtraPayment(FormCollection collection)
        {
            var appid = collection.Get("myappid");
            var status = collection.Get("status");
            var sanctype = collection.Get("penaltytype");
            var descrip = collection.Get("penaltydescription");
            var qty = collection.Get("Qty");
            string Message = "";
            try
            {
                ApplicationRequest appmaster = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == appid).FirstOrDefault();
                Penalty appPenalty = dbCtxt.Penalties.Where(c => c.PenaltyType.Trim() == sanctype).FirstOrDefault();


                var amount = Convert.ToDecimal(collection.Get("penaltyamount"));
                string genApplicationId = commonHelper.GenerateApplicationNo();
                var ExtraAmount = new ExtraPayment()
                {
                    ApplicationID = appid,
                    Description = descrip,
                    TxnAmount = amount,
                    Qty = Convert.ToInt32(qty),
                    ExtraPaymentAppRef = genApplicationId,
                    Arrears = 0,
                    LastRetryDate = DateTime.Now,
                    RetryCount = 1,
                    Status = status,
                    SanctionType = sanctype,
                    ApplicantID = appmaster.ApplicantUserId,
                    LicenseTypeCode = appmaster.LicenseTypeId,
                    PenaltyCode = appPenalty.PenaltyCode,
                    ExtraPaymentBy = userMaster.UserId
                };
                dbCtxt.ExtraPayments.Add(ExtraAmount);
                int done = dbCtxt.SaveChanges();
                if (done > 0)
                {
                    //TempData["GeneSuccess"]
                    Message = "Extra payment was successfully generated for application with the reference number " + appid;
                    var extrapaymentdetails = (from e in dbCtxt.ExtraPayments where e.ApplicationID == appid select e).FirstOrDefault();
                    if (extrapaymentdetails != null)
                    {

                        var subject = "Extra Payment Generated";
                        var content = "YOU ARE REQUIRED TO MAKE EXTRA PAYMENT OF " + extrapaymentdetails.TxnAmount + " NAIRA FOR THE APPLICATION WITH REFERENCE NUMBER " + extrapaymentdetails.ApplicationID + ", AND YOUR REMITA REFRENCE NUMBER IS " + extrapaymentdetails.RRReference;
                        var sendmail = userMasterHelper.SendStaffEmailMessage(extrapaymentdetails.ApplicantID, subject, content);

                    }
                }
                else
                {
                    Message = "something went wrong trying to generate extra payment";
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
            return Json(new { message = Message, AppId = appid }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GenerateExtraPayment(FormCollection collection)
        {
            List<ApplicationRequest> apps = new List<ApplicationRequest>();
            var extra = (from a in dbCtxt.ApplicationRequests where a.CurrentStageID == 2 || a.CurrentStageID == 3 || a.CurrentStageID == 4 select a).ToList();

            foreach (var item in extra)
            {
                apps.Add(new ApplicationRequest()
                {
                    ApplicationId = item.ApplicationId,
                    ApplicantUserId = item.ApplicantUserId,
                    ApplicantName = item.ApplicantName,
                    ApplicationTypeId = item.ApplicationTypeId,
                    LicenseTypeId = item.LicenseTypeId,
                    AddedDate = item.AddedDate
                });
            }
            ViewBag.ExtraPaymentStage = apps;
            return View();

        }

        public ActionResult UpdateCompanyRecord(CompanyInformationModel compDetailsModel, FormCollection formCollection)
        {
            string status = "success";
            string jsonRequest = null;
            ElpsResponse wrapper = null;
            CompanyInformationModel companyInfoModel = new CompanyInformationModel();
            CompanyChangeModel companyChangeEmail = new CompanyChangeModel();

            var companysemail = (from u in dbCtxt.UserMasters where u.ElpsId.Contains(compDetailsModel.company.id.ToString()) select u).FirstOrDefault();
            string actionType = formCollection["actionType"];
            string companyId = formCollection["companyId"];

            bool emailChange = false;
            var elpsid = compDetailsModel.company.id.ToString();
            var companyemail = (from u in dbCtxt.UserMasters where u.ElpsId == elpsid select u).ToList();

            if (companyemail.Count > 0)
            {
                if (companyemail.FirstOrDefault().UserId == compDetailsModel.company.user_Id)
                {
                    emailChange = false;
                }
                else
                {
                    emailChange = true;
                }
            }


            companyChangeEmail.Name = compDetailsModel.company.name;
            companyChangeEmail.RC_Number = compDetailsModel.company.rC_Number;
            companyChangeEmail.Business_Type = compDetailsModel.company.business_Type;
            companyChangeEmail.emailChange = emailChange;
            companyChangeEmail.CompanyId = compDetailsModel.company.id;
            companyChangeEmail.NewEmail = compDetailsModel.company.user_Id;

            if (actionType.Contains("UPDATE_PROFILE"))
            {
                jsonRequest = JsonConvert.SerializeObject(compDetailsModel);
                logger.Info("JsonRequest =>" + jsonRequest);

                wrapper = serviceIntegrator.maintainCompanyInformationNew(actionType, companyId, jsonRequest, companyChangeEmail);
                logger.Info("Response From Elps =>" + wrapper.message);

                if (wrapper.message != "SUCCESS")
                {
                    status = wrapper.message;
                    logger.Error(status);
                }
                else
                {
                    userMasterHelper.UpdateAllCompanyApplicationEmail(compDetailsModel.company.user_Id, companyemail.FirstOrDefault().UserId, compDetailsModel.company.name);
                    companysemail.UserId = compDetailsModel.company.user_Id;
                    dbCtxt.SaveChanges();
                }
            }
            else if (actionType.Contains("ADDRESS"))
            {
                List<CompanyAddressDTO> addressLTO = new List<CompanyAddressDTO>();

                CompanyAddressDTO inaddressDTO = compDetailsModel.registeredAddress;

                inaddressDTO.type = "registered";
                inaddressDTO.address_1 = compDetailsModel.registeredAddress.address_1;
                inaddressDTO.city = compDetailsModel.registeredAddress.city;
                inaddressDTO.postal_code = compDetailsModel.registeredAddress.postal_code;
                inaddressDTO.country_Id = "156";
                inaddressDTO.stateId = "2412";

                wrapper = serviceIntegrator.GetCompanyDetailByEmail(userMaster.UserId);
                if (wrapper.message == "SUCCESS")
                {
                    CompanyDetail compDetail = (CompanyDetail)wrapper.value;

                    if (compDetail.registered_Address_Id == null)
                    {

                        addressLTO.Add(inaddressDTO);
                        jsonRequest = JsonConvert.SerializeObject(addressLTO);
                        logger.Info("JsonRequest =>" + jsonRequest);

                        wrapper = serviceIntegrator.maintainCompanyInformationNew("ADD_ADDRESS", companyId, jsonRequest, companyChangeEmail);
                        if (wrapper.message == "SUCCESS")
                        {
                            logger.Info("Successfully Created Address");
                            List<CompanyAddressDTO> compaddressList = (List<CompanyAddressDTO>)wrapper.value;
                            CompanyAddressDTO compaddress = compaddressList.FirstOrDefault();

                            logger.Info("Address Response  =>" + JsonConvert.SerializeObject(compaddressList));

                            compDetail.registered_Address_Id = compaddress.id.ToString();
                            companyInfoModel = new CompanyInformationModel();
                            companyInfoModel.company = compDetail;
                            logger.Info("About to Link Created Address");

                            logger.Info("Company To Link  =>" + JsonConvert.SerializeObject(companyInfoModel));
                            wrapper = serviceIntegrator.maintainCompanyInformationNew("UPDATE_PROFILE", companyId, JsonConvert.SerializeObject(companyInfoModel), companyChangeEmail);
                            if (wrapper.message != "SUCCESS")
                            {
                                status = wrapper.message;
                            }
                            else
                            {
                                logger.Info("Company Link Is SUCESS  =>" + JsonConvert.SerializeObject((CompanyDetail)wrapper.value));
                                logger.Info("Address is Linked");
                            }
                        }
                        else
                        {
                            status = wrapper.message;
                        }
                    }
                    else
                    {

                        inaddressDTO.id = Convert.ToInt32(compDetail.registered_Address_Id);
                        addressLTO.Add(inaddressDTO);
                        jsonRequest = JsonConvert.SerializeObject(addressLTO);
                        logger.Info("JsonRequest =>" + jsonRequest);

                        wrapper = serviceIntegrator.maintainCompanyInformationNew("UPDATE_ADDRESS", companyId, jsonRequest, companyChangeEmail);
                        status = wrapper.message;

                        var apps = dbCtxt.ApplicationRequests.Where(x => x.ApplicantUserId.Trim().Equals(companyId.Trim()) && !string.IsNullOrEmpty(x.RegisteredAddress));
                        if(apps.Any())
                        {
                            apps.ForEach(x => x.RegisteredAddress = compDetailsModel.registeredAddress.address_1);
                            dbCtxt.SaveChanges();
                        }
                    }

                }
                else
                {
                    status = wrapper.message;
                    logger.Error(status);
                }

            }
            return RedirectToAction("CompanyProfile", new { ApplicationId = "", CompanyEmail = companysemail?.UserId });
            //return Json(new
            //{
            //    Status = status
            //},
            // JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CompanyProfile(string ApplicationId, string CompanyEmail)
        {
            CompanyDetail compDto = null;

            //List<SelectListItem> StateList = new List<SelectListItem>();
            List<SelectListItem> Nationality = new List<SelectListItem>();
            List<SelectListItem> StateAddress = new List<SelectListItem>();
            List<SelectListItem> CountryAddress = new List<SelectListItem>();
            CompanyInformationModel compDetailsModel = new CompanyInformationModel();
            List<DocumentModel> companyDocuments = new List<DocumentModel>();
            ElpsResponse elpsResponse;
            try
            {
                ViewBag.ApplicantName = userMaster.FirstName;
                ViewBag.LoginRole = userMaster.UserRoles;
                var companyemail = (ApplicationId == "" || ApplicationId == null) ? CompanyEmail : (from c in dbCtxt.ApplicationRequests where c.ApplicationId == ApplicationId select c.ApplicantUserId).FirstOrDefault();
                var localcomp = dbCtxt.UserMasters.FirstOrDefault(x => x.UserId.ToLower().Equals(companyemail.ToLower()));
                //elpsResponse = serviceIntegrator.GetCompanyDetailByEmail(companyemail);
                elpsResponse = serviceIntegrator.GetCompanyDetailByID(localcomp.ElpsId);
                logger.Info("Result From CompanyDetail =>" + elpsResponse.message);

                try
                {
                    ViewBag.ApplicantName = userMaster.FirstName;
                    //var userid = (from u in dbCtxt.ApplicationRequests where u.ApplicationID == ApplicationId select u.CompanyUserId).FirstOrDefault();
                    var elpsid = (from u in dbCtxt.UserMasters where u.UserId == companyemail select u.ElpsId).FirstOrDefault();
                    companyDocuments = appDocHelper.GetAllCompanyDocuments(elpsid, GlobalModel.elpsUrl, serviceIntegrator);
                    ViewBag.ResponseMessage = "SUCCESS";
                    ViewBag.AllDocument = companyDocuments;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.StackTrace);
                    ViewBag.ResponseMessage = "An Error Occured Getting Company Document From Elps, Pls Try again Later";
                }
                //return Json(companyDocuments, JsonRequestBehavior.AllowGet);

                if (elpsResponse.message == "SUCCESS")
                {
                    // var res = elpsResponse.value;
                    //compDto = JsonConvert.DeserializeObject<CompanyDetail>(res.ToString());
                    compDto = (CompanyDetail)elpsResponse.value;
                    if (compDto != null)
                    {
                        compDetailsModel.company = compDto;
                    }
                }

                //logger.Info("Company =>" + JsonConvert.SerializeObject(compDto));

                if (compDto.registered_Address_Id != null)
                {
                    logger.Info("compDto.registered_Address_Id");
                    elpsResponse = serviceIntegrator.GetAddressByID(compDto.registered_Address_Id);
                    logger.Info("Result for GetAddressByID =>" + elpsResponse.message);
                    if (elpsResponse.message == "SUCCESS")
                    {
                        compDetailsModel.registeredAddress = (CompanyAddressDTO)elpsResponse.value;
                        logger.Info("CompanyAddressDTO Address =>" + compDetailsModel.registeredAddress.address_1);
                    }
                }

                if (compDetailsModel.registeredAddress == null)
                {
                    compDetailsModel.registeredAddress = new CompanyAddressDTO();
                }

                if (compDto.operational_Address_Id != null)
                {
                    elpsResponse = serviceIntegrator.GetAddressByID(compDto.operational_Address_Id);
                    if (elpsResponse.message == "SUCCESS")
                    {
                        compDetailsModel.operationalAddress = (CompanyAddressDTO)elpsResponse.value;
                    }
                }

                if (compDetailsModel.operationalAddress == null)
                {
                    compDetailsModel.operationalAddress = new CompanyAddressDTO();
                }

                foreach (CountriesMasterList r in dbCtxt.CountriesMasterLists.ToList().OrderBy(s => s.CountryName))
                {
                    if (r.CountryName == compDetailsModel.company.nationality)
                    {
                        Nationality.Add(new SelectListItem
                        {
                            Text = r.CountryName,
                            Value = r.CountryName,
                            Selected = true
                        });
                    }
                    else
                    {
                        Nationality.Add(new SelectListItem
                        {
                            Text = r.CountryName,
                            Value = r.CountryName
                        });
                    }

                    //////////////////////////////////////////////////

                    if (r.CountryName == compDetailsModel.registeredAddress.countryName)
                    {
                        CountryAddress.Add(new SelectListItem
                        {
                            Text = r.CountryName,
                            Value = r.CountryName,
                            Selected = true
                        });
                    }
                    else
                    {
                        CountryAddress.Add(new SelectListItem
                        {
                            Text = r.CountryName,
                            Value = r.CountryName
                        });
                    }

                }

                foreach (StateMasterList r in dbCtxt.StateMasterLists.ToList().OrderBy(s => s.StateName))
                {
                    if (r.StateName == compDetailsModel.registeredAddress.stateName)
                    {
                        StateAddress.Add(new SelectListItem
                        {
                            Text = r.StateName,
                            Value = r.StateName,
                            Selected = true
                        });
                    }
                    else StateAddress.Add(new SelectListItem
                    {
                        Text = r.StateName,
                        Value = r.StateName
                    });
                }

                ViewBag.CountryAddress = CountryAddress;
                ViewBag.Nationality = Nationality;
                ViewBag.StateAddress = StateAddress;
                ViewBag.ErrorMessage = "SUCCESS";
            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error Occured Getting CompanyProfile From Elps, Pls Try again Later";
            }

            return View(compDetailsModel);
        }

        [HttpGet]
        public ActionResult ApplicationHistory(string applicationId)
        {
            List<ActionHistory> NotificationList = new List<ActionHistory>();
            try
            {
                NotificationList = dbCtxt.ActionHistories.Where(a => a.ApplicationId == applicationId).OrderByDescending(a => a.ActionDate).Take(10).ToList();
                ViewBag.ApplicationID = applicationId;
                ViewBag.ErrorMessage = "SUCCESS";
            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error Occured on Application History, Please try again Later";
            }

            return View(NotificationList);
        }

        [HttpGet]
        public ActionResult Companies() => View(dbCtxt.UserMasters.Where(u => u.UserType == "COMPANY" && u.Status == "ACTIVE").ToList());

        [HttpGet]
        public ActionResult DeleteCompanyByEmail(string email)
        {
            var user = dbCtxt.UserMasters.FirstOrDefault(x => x.UserId.Equals(email));
            if(user != null)
            {
                var useract = dbCtxt.UserActivities.Where(x => x.UserId.Equals(email)).ToList();
                var userhist = dbCtxt.UserActivityHists.Where(x => x.UserId.Equals(email)).ToList();

                if (useract.Count > 0)
                    dbCtxt.UserActivities.RemoveRange(useract);

                if (userhist.Count > 0)
                    dbCtxt.UserActivityHists.RemoveRange(userhist);

                dbCtxt.UserMasters.Remove(user);
                dbCtxt.SaveChanges();
            }
            return RedirectToAction("Companies");
        }

        [HttpGet]
        public ActionResult CompanyPermits(string userId)
        {
            string ErrorMessage = "";
            UserMaster up = dbCtxt.UserMasters.Where(c => c.UserId.Trim() == userId.Trim()).FirstOrDefault();
            ViewBag.CompanyName = up.FirstName;
            return View(appDocHelper.GetApplications(up.UserId, "PEM", out ErrorMessage));
        }

        [HttpGet]
        public ActionResult CompanyApplications(string userId)
        {

            UserMaster up = dbCtxt.UserMasters.Where(c => c.UserId.Trim() == userId.Trim()).FirstOrDefault();
            var CompanyApps = (from c in dbCtxt.ApplicationRequests where c.ApplicantUserId == userId select c).ToList();
            ViewBag.CompanyName = up.FirstName;
            return View(CompanyApps);
        }



        [HttpGet]
        public ActionResult CompanyDocuments(string compId)
        {
            List<DocumentModel> documents = new List<DocumentModel>();
            UserMaster up = dbCtxt.UserMasters.Where(c => c.ElpsId.Trim() == compId.Trim()).FirstOrDefault();
            ViewBag.ApplicantName = up.FirstName;
            ElpsResponse elpsResponse = serviceIntegrator.GetCompanyDocumentListById(up.ElpsId);

            if (elpsResponse.message != "SUCCESS")
            {
                logger.Error(elpsResponse.message);
                ViewBag.ResponseMessage = "An Error Occured Getting Company Document From Elps, Pls Try again Later";
                return View(documents);
            }
            else
            {
                foreach (Models.Document doc in (List<Models.Document>)elpsResponse.value)
                {
                    DocumentModel d = new DocumentModel();
                    d.DocId = Convert.ToInt32(doc.document_type_id);
                    d.DocumentName = doc.fileName;
                    d.DateAdded = Convert.ToDateTime(doc.date_added);
                    d.DateModified = Convert.ToDateTime(doc.date_modified);
                    d.Source = doc.source;
                    d.UniqueId = doc.uniqueId;
                    d.FileId = doc.id;
                    d.DocumentTypeName = doc.documentTypeName;
                    d.UploadDocumentUrl = GlobalModel.elpsUrl;

                    documents.Add(d);
                }
            }

            return View(documents);
        }







        //[HttpGet]
        //public ActionResult AllApplications()
        //{
        //    return View(dbCtxt.ApplicationRequests.ToList());
        //}








        [HttpGet]
        public ActionResult ApprovedPermits()
        {
            List<ApplicationRequest> applicationRequest = dbCtxt.ApplicationRequests.Where(a => a.IsLegacy == "NO" && !string.IsNullOrEmpty(a.LicenseReference) && a.CurrentStageID == 21).ToList();
            return View(applicationRequest);
        }









        [HttpGet]
        public ActionResult ReassignApplication()
        {
            ViewBag.ErrorMessage = "SUCCESS";

            List<ApplicationRequest> applicationRequest = userMasterHelper.GetFieldRoleApplications(userMaster).Where(a => a.WorkFlowState.StateType == "PROGRESS").ToList();
            return View(applicationRequest);
        }








        [HttpGet]
        public JsonResult GetStaffRoles()
        {
            var userRole = (from r in dbCtxt.Roles
                            where r.RoleId != "COMPANY"
                            select new
                            {
                                role = r.RoleId
                            }).ToList();
            return Json(userRole, JsonRequestBehavior.AllowGet);
        }






        [HttpGet]
        public ActionResult StaffMaintenance()
        {

            ElpsResponse elpsResponse = serviceIntegrator.GetAllStaff();
            logger.Info("Response from Elps =>" + elpsResponse.message);
            if (elpsResponse.message == "SUCCESS")
            {
                AllStaffs = (List<Staff>)elpsResponse.value;
            }


            ViewBag.AllCompany = (from u in dbCtxt.UserMasters where u.UserType == "COMPANY" select u).ToList();

            ViewBag.SigninUser = userMaster.UserRoles;

            return View();
        }





        [HttpGet]
        public ActionResult ViewStaffDesk(string userid)
        {
            string ErrorMessage = "";
            ViewBag.UserId = userid;
            List<ApplicationRequest> appRequestList;
            UserMaster up = dbCtxt.UserMasters.Where(c => c.UserId.Trim() == userid.Trim()).FirstOrDefault();
            appRequestList = userMasterHelper.GetApprovalRequest(dbCtxt, up, out ErrorMessage);
            ViewBag.ErrorMessage = ErrorMessage;

            return PartialView("_StaffDesk", appRequestList);
        }







        public ActionResult ViewSUI(string id)
        {
            return licenseHelper.ViewSUI(id);
        }


        public ActionResult ViewPTE(string id)
        {
            return licenseHelper.ViewPTE(id);
        }

        public ActionResult ViewATC(string id)
        {

            return licenseHelper.ViewATC(id);
        }

        public ActionResult ViewLTO(string id)
        {
            return licenseHelper.ViewLTO(id);

        }
        public ActionResult ViewLTOLFP(string id)
        {

            return licenseHelper.ViewLTOLFP(id);

        }
        public ActionResult ViewATCLFP(string id)
        {

            return licenseHelper.ViewATCLFP(id);

        }
        public ActionResult ViewTITA(string id)
        {

            return licenseHelper.ViewTITA(id);

        }
        public ActionResult ViewTCA(string id)
        {

            return licenseHelper.ViewTCA(id);

        }
        public ActionResult ViewTPBA(string id)
        {

            return licenseHelper.ViewTPBA(id);

        }

        public ActionResult ViewATCMOD(string id)
        {

            return licenseHelper.ViewATCMOD(id);

        }
        public ActionResult ViewTO(string id)
        {

            return licenseHelper.ViewTO(id);

        }



        [HttpPost]
        public ActionResult EditStaff(UserMaster usr, FormCollection collect, HttpPostedFileBase postedFile)
        {
            try
            {
                using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
                {

                    string fileName = "";
                    string filePath = "";
                    if (postedFile != null)
                    {
                        fileName = System.IO.Path.GetFileName(postedFile.FileName);
                        filePath = "~/Images/" + fileName;
                        //Save the Image File in Folder.
                        postedFile.SaveAs(Server.MapPath(filePath));
                    }

                    var user = (from u in db.UserMasters where u.UserId == usr.UserId select u).FirstOrDefault();
                    var staffname = collect.Get("Fullname");
                    var sname = staffname.Split(' ');
                    var firstname = collect.Get("FirstName");
                    var lastname = collect.Get("LastName");
                    var staffrole = collect.Get("Userrole");
                    var userlocation = collect.Get("UserLocation");
                    user.UserLocation = userlocation;
                    user.UserRoles = staffrole;
                    user.SignatureImage = filePath;
                    user.FirstName = firstname;
                    user.LastName = lastname;
                    db.SaveChanges();
                    TempData["success"] = "Staff Update was Successful";
                }
            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }

            return RedirectToAction("StaffMaintenance", "Admin", "");
        }








        [HttpGet]
        public JsonResult GetUserRole()
        {

            var userRole = (from r in dbCtxt.Roles
                            where r.RoleId != "COMPANY"
                            select new
                            {
                                role = r.RoleId
                            }).ToList();
            return Json(userRole, JsonRequestBehavior.AllowGet);

        }






        [HttpGet]
        public JsonResult GetUserLocation()
        {
            var userRole = (from r in dbCtxt.FieldLocations
                            select new
                            {
                                Fieldlocationid = r.FieldLocationID,
                                Description = r.Description
                            }).ToList();
            return Json(userRole, JsonRequestBehavior.AllowGet);

        }




        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetAllUser()
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var searchTxt = Request.Form.GetValues("search[value]")[0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;
            var staff = (from u in dbCtxt.UserMasters
                         join f in dbCtxt.FieldLocations on u.UserLocation equals f.FieldLocationID into field
                         where field.FirstOrDefault().FieldLocationID == u.UserLocation && u.UserType != "COMPANY"
                         select new
                         {
                             u.UserId,
                             fullname = u.FirstName + " " + u.LastName,
                             u.UserRoles,
                             u.UserLocation,
                             FieldLocation = field.FirstOrDefault().Description,
                             u.UserType,
                             u.SignatureImage,
                             u.Status
                         });

            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(u => u.fullname.Contains(searchTxt) || u.UserId.Contains(searchTxt)
               || u.UserRoles.Contains(searchTxt) || u.FieldLocation.Contains(searchTxt) || u.UserType.Contains(searchTxt) || u.Status.Contains(searchTxt));
            }
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.UserId + " " + sortColumnDir);
            }


            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public ActionResult DeactivateUser(FormCollection collect)
        {
            try
            {
                using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
                {
                    var usr = collect.Get("userid");
                    var deactive = collect.Get("DeactivateComment");
                    var Location = collect.Get("userlocationDeactivate");
                    var usermas = (from u in db.UserMasters where u.UserId == usr && u.UserLocation == Location select u).FirstOrDefault();
                    usermas.Status = "PASSIVE";
                    usermas.LastComment = deactive;
                    db.SaveChanges();
                    TempData["success"] = usr + " was Successfully Deactivated";
                }
            }

            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }


            return RedirectToAction("StaffMaintenance", "Admin", "");
        }



        [HttpPost]
        public ActionResult ActivateUser(FormCollection collect)
        {
            try
            {
                using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
                {
                    var usr = collect.Get("userID");
                    var deactive = collect.Get("Activate");
                    var location = collect.Get("UserLocationActivate");
                    var usermas = (from u in db.UserMasters where u.UserId == usr && u.UserLocation == location select u).FirstOrDefault();
                    usermas.Status = "ACTIVE";
                    usermas.LastComment = deactive;
                    db.SaveChanges();
                    TempData["success"] = usr + " was Successfully Activated";
                }
            }

            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }
            return RedirectToAction("StaffMaintenance", "Admin", "");
        }












        [HttpPost]
        public ActionResult DeleteUser(FormCollection collect)
        {
            var email = collect.Get("useremail");
            try
            {
                var useremail = (from u in dbCtxt.UserMasters where u.UserId == email select u).ToList();

                if (useremail != null)
                {
                    dbCtxt.UserMasters.Remove(useremail.FirstOrDefault());
                    dbCtxt.SaveChanges();
                }
                TempData["success"] = email + " was successfully deleted";
            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message + " Unable to delete user " + email;
            }
            return RedirectToAction("StaffMaintenance");
        }







        [HttpPost]
        public ActionResult NewUser(FormCollection collect, UserMaster user)
        {
            try
            {
                using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
                {
                    var checkexistence = (from u in db.UserMasters where u.UserId == user.UserId select u).FirstOrDefault();
                    Random rnd = new Random();
                    int randomnbrs = rnd.Next(100000, 999999);
                    if (checkexistence != null)
                    {
                        TempData["AlreadyExist"] = "User Already Exist";
                    }
                    else
                    {
                        var splitfullname = collect.Get("staffFullName");
                        var name = splitfullname.Split(' ');
                        var firstname = name[0];
                        var lastname = name[1];
                        var locations = collect.Get("UserLocations");
                        var role = collect["staffrole"];
                        var userdata = new UserMaster()
                        {
                            UserId = user.UserId,
                            UserType = "ADMIN",
                            UserRoles = role,
                            FirstName = firstname,
                            LastName = lastname,
                            UserLocation = locations,
                            CreatedBy = "SYSTEM",
                            CreatedOn = DateTime.Now,
                            UpdatedBy = userMaster.UserId,
                            UpdatedOn = DateTime.Now,
                            Status = "ACTIVE",
                            LastLogin = DateTime.Now,
                            NotificationAllowed = "Y",
                            SignatureID = randomnbrs
                        };
                        db.UserMasters.Add(userdata);
                        db.SaveChanges();
                        var userdata1 = new UserMasterHist()
                        {
                            UserId = userdata.UserId,
                            UserRoles = userdata.UserRoles,
                            FullName = splitfullname,
                            UserLocation = userdata.UserLocation,
                            MaintenanceDate = DateTime.Now,
                            Status = "ACTIVE",
                            MaintainedBy = userMaster.UserId
                        };
                        db.UserMasterHists.Add(userdata1);
                        db.SaveChanges();
                        TempData["success"] = "User Was Successfully Added";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }


            return RedirectToAction("StaffMaintenance", "Admin", "");
        }



        //[HttpGet]
        //public ActionResult StaffMaintenance()
        //{

        //    logger.Info("Calling StaffMaintenance With Staff Count On Elps =>" + AllStaffs.Capacity);
        //    try
        //    {

        //        ViewBag.ErrorMessage = "SUCCESS";
        //        ViewBag.UserRole = userMaster.UserRoles;
        //        ViewBag.AllUserList = userMasterHelper.GetMaintainedStaffList(userMaster);

        //        if (AllStaffs.Count == 0)
        //        {
        //            logger.Info("About to GetAllStaff on Elps");
        //            ElpsResponse elpsResponse = serviceIntegrator.GetAllStaff();
        //            logger.Info("Response from Elps =>" + elpsResponse.message);
        //            if (elpsResponse.message != "SUCCESS")
        //            {
        //                logger.Error(elpsResponse.message);
        //                ViewBag.ErrorMessage = "Error When Getting All Staff List From Elps, Please Try again Later";
        //            }
        //            else
        //            {
        //                AllStaffs = (List<Staff>)elpsResponse.value;
        //                logger.Info("Final Returned Staff Count On Elps =>" + AllStaffs.Capacity);
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex.StackTrace);
        //        ViewBag.ErrorMessage = "Error Occured Getting List of Staffs, Please try again Later";
        //    }
        //    return View();
        //}




        [HttpGet]
        public ActionResult UserIdAutosearch(string term = "")
        {
            logger.Info("Staff Count =>" + AllStaffs.Count);
            List<Staff> staffJsonList = new List<Staff>();

            foreach (Staff staff in AllStaffs.Where(s => s.email.ToLower().Contains(term.ToLower())))
            {
                staffJsonList.Add(new Staff() { userid = staff.email, name = staff.firstName + " " + staff.lastName });
            }

            logger.Info("Fetched Staff Count =>" + staffJsonList.Count);
            return Json(staffJsonList, JsonRequestBehavior.AllowGet);

        }





        [HttpGet]

        public ActionResult AllPenalty()
        {
            List<Penalty> PenaltyInfo = new List<Penalty>();

            var Penaltylist = (from a in dbCtxt.Penalties select a).ToList();


            foreach (var item in Penaltylist)
            {
                PenaltyInfo.Add(new Penalty()
                {
                    PenaltyId = item.PenaltyId,
                    PenaltyAmount = item.PenaltyAmount,
                    PenaltyCode = item.PenaltyCode,
                    PenaltyType = item.PenaltyType,

                });
            }

            ViewBag.UserLoginEmail = userMaster.UserRoles;

            return View(Penaltylist);
        }



        [HttpPost]
        public JsonResult AllPenaltyEdit(int Penaltycodeid, decimal Penaltyamount, int Penaltycode, string Penaltytype)
        {
            string status = string.Empty;
            string message = string.Empty;
            try
            {
                var Record = (from e in dbCtxt.Penalties where e.PenaltyId == Penaltycodeid select e).FirstOrDefault();
                Record.PenaltyCode = Penaltycode;
                Record.PenaltyType = Penaltytype;
                Record.PenaltyAmount = Penaltyamount;
                dbCtxt.SaveChanges();

                status = "success";
                message = "Update was successful";

            }
            catch (Exception ex)
            {
                status = "failed";
                message = "Unable to update record " + ex.Message;
            }

            return Json(new { Status = status, Message = message }, JsonRequestBehavior.AllowGet);
        }






        [HttpGet]

        public ActionResult AllIssuedPenalty()
        {
            List<Penalty> PenaltyInfo = new List<Penalty>();

            var workflowlist = (from a in dbCtxt.Penalties select a).ToList();


            foreach (var item in workflowlist)
            {
                PenaltyInfo.Add(new Penalty()
                {
                    PenaltyAmount = item.PenaltyAmount,
                    PenaltyCode = item.PenaltyCode,
                    PenaltyType = item.PenaltyType,

                });
            }

            ViewBag.UserLoginEmail = userMaster.UserRoles;

            return View(workflowlist);
        }




        public ActionResult WorkFlow()
        {
            List<WorkFlowNavigation> workflowInfo = new List<WorkFlowNavigation>();

            var workflowlist = (from a in dbCtxt.WorkFlowNavigations select a).ToList();


            foreach (var item in workflowlist)
            {
                workflowInfo.Add(new WorkFlowNavigation()
                {
                    WorkFlowId = item.WorkFlowId,
                    FieldLocationApply = item.FieldLocationApply,
                    LicenseTypeId = item.LicenseTypeId,
                    Action = item.Action,
                    ActionRole = item.ActionRole,
                    CurrentStageID = item.CurrentStageID,
                    NextStateID = item.NextStateID,
                    TargetRole = item.TargetRole,
                    NotifyAction = item.NotifyAction
                });
            }

            ViewBag.UserLoginEmail = userMaster.UserRoles;

            return View(workflowInfo);
        }



        [HttpPost]
        public ActionResult UpdateWorkFlowRecord(int WorkFlowId, string FieldLocationApply, string Action, string ActionRole, short CurrentStageID, short NextStateID, string TargetRole, string NotifyAction)
        {
            var result = "";
            try
            {
                var WorkflowRec = (from w in dbCtxt.WorkFlowNavigations where w.WorkFlowId == WorkFlowId select w).FirstOrDefault();

                if (WorkflowRec != null)
                {
                    WorkflowRec.ApplicationType = "ALL";
                    WorkflowRec.LicenseTypeId = "ALL";
                    WorkflowRec.FieldLocationApply = FieldLocationApply;
                    WorkflowRec.Action = Action;
                    WorkflowRec.ActionRole = ActionRole;
                    WorkflowRec.CurrentStageID = CurrentStageID;
                    WorkflowRec.NextStateID = NextStateID;
                    WorkflowRec.TargetRole = TargetRole;
                    WorkflowRec.NotifyAction = NotifyAction;
                    dbCtxt.SaveChanges();
                    result = "success";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult AddWorkFlowRecord(string FieldLocationApply, string Action, string ActionRole, short CurrentStageID, short NextStateID, string TargetRole, string NotifyAction)
        {
            var result = "";
            try
            {
                WorkFlowNavigation WorkflowRec = new WorkFlowNavigation();

                WorkflowRec.ApplicationType = "ALL";
                WorkflowRec.FieldLocationApply = FieldLocationApply;
                WorkflowRec.LicenseTypeId = "ALL";
                WorkflowRec.Action = Action;
                WorkflowRec.ActionRole = ActionRole;
                WorkflowRec.CurrentStageID = CurrentStageID;
                WorkflowRec.NextStateID = NextStateID;
                WorkflowRec.TargetRole = TargetRole;
                WorkflowRec.NotifyAction = "NORMAL";
                dbCtxt.WorkFlowNavigations.Add(WorkflowRec);
                dbCtxt.SaveChanges();
                result = "success";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteWorkFlow(int workFlowId)
        {
            var result = "";
            try
            {
                var delete = (from w in dbCtxt.WorkFlowNavigations where w.WorkFlowId == workFlowId select w).FirstOrDefault();
                dbCtxt.WorkFlowNavigations.Remove(delete);
                dbCtxt.SaveChanges();
                result = "success";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult WorkFlowState()
        {
            List<WorkFlowState> workflowstateInfo = new List<WorkFlowState>();

            var workflowlist = (from a in dbCtxt.WorkFlowStates select a).ToList();


            foreach (var item in workflowlist)
            {
                workflowstateInfo.Add(new WorkFlowState()
                {
                    StateID = item.StateID,
                    StateName = item.StateName,
                    StateType = item.StateType.ToUpper(),
                    Progress = item.Progress,
                });
            }

            ViewBag.UserLoginEmail = userMaster.UserRoles;
            return View(workflowstateInfo);
        }


        [HttpPost]
        public ActionResult AddWorkFlowState(short Stateid, string Statename, string Statetype, string rate)
        {
            var result = "";
            try
            {
                WorkFlowState WorkflowRec = new WorkFlowState();
                WorkflowRec.StateID = Stateid;
                WorkflowRec.StateName = Statename;
                WorkflowRec.StateType = Statetype.ToUpper();
                WorkflowRec.Progress = rate;
                dbCtxt.WorkFlowStates.Add(WorkflowRec);
                dbCtxt.SaveChanges();
                result = "success";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateWorkFlowState(short Stateid, string Statename, string Statetype, string rate)
        {
            var result = "";
            try
            {
                var WorkflowRec = (from w in dbCtxt.WorkFlowStates where w.StateID == Stateid select w).FirstOrDefault();

                if (WorkflowRec != null)
                {

                    WorkflowRec.StateID = Stateid;
                    WorkflowRec.StateName = Statename;
                    WorkflowRec.StateType = Statetype.ToUpper();
                    WorkflowRec.Progress = rate;
                    dbCtxt.SaveChanges();
                    result = "success";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteWorkFlowState(int workFlowStateId)
        {
            var result = "";
            try
            {
                var delete = (from w in dbCtxt.WorkFlowStates where w.StateID == workFlowStateId select w).FirstOrDefault();
                dbCtxt.WorkFlowStates.Remove(delete);
                dbCtxt.SaveChanges();
                result = "success";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult StaffRoles()
        {
            List<Role> role = new List<Role>();

            var rolelist = (from a in dbCtxt.Roles select a).ToList();


            foreach (var item in rolelist)
            {
                role.Add(new Role()
                {
                    RoleId = item.RoleId,
                    Description = item.Description.ToUpper()
                });
            }

            ViewBag.UserLoginEmail = userMaster.UserRoles;
            return View(role);
        }

        [HttpPost]
        public ActionResult UpdateStaffRole(string Roleid, string roledescription)
        {
            var result = "";
            try
            {
                var updatestaffrole = (from w in dbCtxt.Roles where w.RoleId == Roleid select w).FirstOrDefault();

                if (updatestaffrole != null)
                {

                    updatestaffrole.RoleId = Roleid;
                    updatestaffrole.Description = roledescription.ToUpper();
                    dbCtxt.SaveChanges();
                    result = "success";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddStaffRole(string Roleid, string roledescription)
        {
            var result = "";
            try
            {
                Role addstaffrole = new Role();

                addstaffrole.RoleId = Roleid;
                addstaffrole.Description = roledescription.ToUpper();
                dbCtxt.Roles.Add(addstaffrole);
                dbCtxt.SaveChanges();
                result = "success";

            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult DeleteStaffRole(string staffRoleid)
        {
            var result = "";
            try
            {
                var delete = (from w in dbCtxt.Roles where w.RoleId == staffRoleid select w).FirstOrDefault();
                dbCtxt.Roles.Remove(delete);
                dbCtxt.SaveChanges();
                result = "success";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RoleMappingMenu()
        {
            List<RoleFunctionalityMapping> role = new List<RoleFunctionalityMapping>();

            var rolelist = (from a in dbCtxt.RoleFunctionalityMappings select a).ToList();


            foreach (var item in rolelist)
            {
                role.Add(new RoleFunctionalityMapping()
                {
                    RoleId = item.RoleId.ToUpper(),
                    FuncId = item.FuncId.ToUpper()
                });
            }

            ViewBag.UserLoginEmail = userMaster.UserRoles;
            return View(role);
        }

        [HttpPost]
        public ActionResult UpdateRoleMappingMenu(string Roleid, string Funcid)
        {
            var result = "";
            try
            {
                var updatestaffrole = (from w in dbCtxt.RoleFunctionalityMappings where w.RoleId == Roleid select w).FirstOrDefault();

                if (updatestaffrole != null)
                {

                    updatestaffrole.RoleId = Roleid.ToUpper();
                    updatestaffrole.FuncId = Funcid.ToUpper();
                    dbCtxt.SaveChanges();
                    result = "success";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddRoleMappingMenu(string Roleid, string Funcid)
        {
            var result = "";
            try
            {
                RoleFunctionalityMapping roleFunctionalityRef = new RoleFunctionalityMapping();

                roleFunctionalityRef.RoleId = Roleid.ToUpper();
                roleFunctionalityRef.FuncId = Funcid.ToUpper();
                dbCtxt.RoleFunctionalityMappings.Add(roleFunctionalityRef);
                dbCtxt.SaveChanges();
                result = "success";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteRoleMappingMenu(string staffRoleid)
        {
            var result = "";
            try
            {
                var delete = (from w in dbCtxt.RoleFunctionalityMappings where w.RoleId == staffRoleid select w).FirstOrDefault();
                dbCtxt.RoleFunctionalityMappings.Remove(delete);
                dbCtxt.SaveChanges();
                result = "success";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult MaintainUser(FormCollection coll)
        {

            string actionType = coll["ActionType"];
            string userId = coll["UserId"];
            string fullName = coll["FullName"];
            string role = coll["Role"];
            string location = coll["Location"];
            string comment = coll["Comment"]; ;

            try
            {

                logger.Info("ActionType =>" + actionType + ",UserId =>" + userId + ",FullName =>" + fullName + ",Roles =>" + role + ",Location =>" + location + ",Comment =>" + comment);
                UserMaster up = dbCtxt.UserMasters.Where(c => c.UserId.Trim() == userId).FirstOrDefault();
                logger.Info("UserMaster =>" + up);

                if (up !=
                 default(UserMaster) && actionType == "CREATE")
                {
                    logger.Error(userId + " Already EXIST on the System");
                    return Json(new
                    {
                        Status = "failure",
                        Message = userId + " Already EXIST on the System"
                    },
                     JsonRequestBehavior.AllowGet);
                }
                else if (up ==
               default(UserMaster) && actionType == "CREATE")
                {
                    up = new UserMaster();
                }

                up.CreatedBy = userMaster.UserId;
                up.CreatedOn = DateTime.UtcNow;
                up.FirstName = fullName;
                up.Status = "ACTIVE";
                up.UserId = userId;
                up.UserType = "ADMIN";
                up.UserLocation = location;
                up.UserRoles = role;

                if (actionType == "CREATE")
                {
                    dbCtxt.UserMasters.Add(up);
                }

                dbCtxt.SaveChanges();
                logger.Info("User Maintenance Successfull");

                UserMasterHist userMasterHist = new UserMasterHist();
                userMasterHist.UserId = up.UserId;
                userMasterHist.FullName = up.FirstName;
                userMasterHist.UserRoles = up.UserRoles;
                userMasterHist.UserLocation = up.UserLocation;
                userMasterHist.MaintainedBy = userMaster.UserId;
                userMasterHist.MaintenanceDate = DateTime.UtcNow;
                userMasterHist.Status = up.Status;
                userMasterHist.LastComment = comment;
                dbCtxt.UserMasterHists.Add(userMasterHist);
                dbCtxt.SaveChanges();
                logger.Info("User Maintenance History Successfull");

            }
            catch (Exception ex)
            {

                logger.Error(ex.InnerException);
                return Json(new
                {
                    Status = "failure",
                    Message = "Error occured creating User (" + fullName + ") with Id =>" + userMaster.UserId + ", Kindly contact Support"
                },
                 JsonRequestBehavior.AllowGet);

            }

            logger.Info("User Maintance Successfull on the System");
            return Json(new
            {
                Status = "success"
            },
             JsonRequestBehavior.AllowGet);
        }








        [HttpGet]
        public ActionResult ActionUser(string useraction, string user, string comment)
        {
            try
            {
                logger.Info("UserActionType =>" + useraction + ",UserId =>" + user + ",Comment =>" + comment);

                UserMaster up = dbCtxt.UserMasters.Where(c => c.UserId.Trim() == user.Trim()).FirstOrDefault();
                up.UpdatedBy = userMaster.UserId;
                up.UpdatedOn = DateTime.UtcNow;
                up.LastComment = comment;
                up.Status = (useraction.Trim() == "Activate") ? "ACTIVE" : "PASSIVE";
                dbCtxt.SaveChanges();
                logger.Info("Update Done, About to Add to History");

                UserMasterHist userMasterHist = new UserMasterHist();
                userMasterHist.UserId = up.UserId;
                userMasterHist.FullName = up.FirstName;
                userMasterHist.UserRoles = up.UserRoles;
                userMasterHist.UserLocation = up.UserLocation;
                userMasterHist.MaintainedBy = userMaster.UserId;
                userMasterHist.MaintenanceDate = DateTime.UtcNow;
                userMasterHist.Status = up.Status;
                userMasterHist.LastComment = comment;
                dbCtxt.UserMasterHists.Add(userMasterHist);
                dbCtxt.SaveChanges();
                logger.Info("Done with All Action");
            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
                return Json(new
                {
                    Status = "failure",
                    Message = "Error Occured For User With Id =>" + user + ", Kindly contact Support"
                },
                 JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                Status = "success"
            },
             JsonRequestBehavior.AllowGet);
        }





        [HttpGet]
        public JsonResult GetDelegatedUser(string applicationId, string fromRole, string toRole)
        {

            if (fromRole.Contains("OPSCON") && toRole.Contains("ADOPERATION"))
            {
                return Json(GetUsersBasedOnRoles(userMaster.UserId, userMaster.UserLocation, "ADOPERATION"), JsonRequestBehavior.AllowGet);
            }
            else if (fromRole.Contains("ADOPERATION") && toRole.Contains("OPSCON"))
            {
                return Json(GetUsersBasedOnRoles(userMaster.UserId, userMaster.UserLocation, "OPSCON"), JsonRequestBehavior.AllowGet);
            }

            else if (fromRole.Contains("ADOPERATION") && toRole.Contains("SUPERVISOR"))
            {
                return Json(GetUsersBasedOnRoles(userMaster.UserId, userMaster.UserLocation, "SUPERVISOR"), JsonRequestBehavior.AllowGet);
            }
            else if (fromRole.Contains("ZOPSCON") && toRole.Contains("ADOPERATION"))
            {
                return Json(GetUsersBasedOnRoles(userMaster.UserId, userMaster.UserLocation, "ADOPERATION"), JsonRequestBehavior.AllowGet);
            }
            else if (fromRole.Contains("SUPERVISOR") && toRole.Contains("REVIEWER"))
            {
                return Json(GetUsersBasedOnRoles(userMaster.UserId, userMaster.UserLocation, "REVIEWER"), JsonRequestBehavior.AllowGet);
            }

            return null;
        }






        public ActionResult GetLicenseChart(LicenseRatio licenseobj)
        {
            int totalLicense = userMaster.UserLocation == "2" ? (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.IsLegacy == "NO" select s).ToList().Count()
                                                            : (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.IsLegacy == "NO" && s.FieldLocation.StateLocated.Contains(s.StateCode) && s.FieldLocation.FieldLocationID == userMaster.UserLocation select s).ToList().Count();


            int Legacy = userMaster.UserLocation == "2" ? (from s in dbCtxt.ApplicationRequests where s.IsLegacy == "YES" select s).ToList().Count()
                                                        : (from s in dbCtxt.ApplicationRequests where s.IsLegacy == "YES" && s.FieldLocation.StateLocated.Contains(s.StateCode) && s.FieldLocation.StateLocated.Contains(s.StateCode) && s.FieldLocation.FieldLocationID == userMaster.UserLocation select s).ToList().Count();

            licenseobj.totalLicense = totalLicense;
            licenseobj.Legacy = Legacy;
            return Json(licenseobj, JsonRequestBehavior.AllowGet);
        }






        public ActionResult LicenseReport()
        {

            //var applicantlocation = (from a in dbCtxt.ActionHistories where a.TriggeredByRole == "COMPANY" && a.TargetedToRole == "COMPANY" select a.CurrentFieldLocation).FirstOrDefault();

            ViewBag.totalNewApp = userMaster.UserLocation == "2" ? (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.ApplicationTypeId == "NEW" select s).ToList().Count()
                                                             : (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.ApplicationTypeId == "NEW" && s.FieldLocation.StateLocated.Contains(s.StateCode) && s.FieldLocation.FieldLocationID == userMaster.UserLocation select s).ToList().Count();

            ViewBag.totalReNewApp = userMaster.UserLocation == "2" ? (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.ApplicationTypeId == "RENEW" select s).ToList().Count()
                                                                  : (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.ApplicationTypeId == "RENEW" && s.FieldLocation.StateLocated.Contains(s.StateCode) && s.FieldLocation.FieldLocationID == userMaster.UserLocation select s).ToList().Count();

            ViewBag.totalOnlineApp = userMaster.UserLocation == "2" ? (from s in dbCtxt.ApplicationRequests where s.IsLegacy == "NO" select s).ToList().Count()
                                                                 : (from s in dbCtxt.ApplicationRequests where s.IsLegacy == "NO" && s.FieldLocation.StateLocated.Contains(s.StateCode) && s.FieldLocation.FieldLocationID == userMaster.UserLocation select s).ToList().Count();

            ViewBag.totalLegacyApp = userMaster.UserLocation == "2" ? (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.IsLegacy == "YES" select s).ToList().Count()
                                                                  : (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.IsLegacy == "YES" && s.FieldLocation.StateLocated.Contains(s.StateCode) && s.FieldLocation.FieldLocationID == userMaster.UserLocation select s).ToList().Count();

            ViewBag.totalLTOApp = userMaster.UserLocation == "2" ? (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.LicenseTypeId == "LTO" select s).ToList().Count()
                                                               : (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.LicenseTypeId == "LTO" && s.FieldLocation.StateLocated.Contains(s.StateCode) && s.FieldLocation.FieldLocationID == userMaster.UserLocation select s).ToList().Count();

            ViewBag.totalATCApp = userMaster.UserLocation == "2" ? (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.LicenseTypeId == "ATC" select s).ToList().Count()
                                                               : (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.LicenseTypeId == "ATC" && s.FieldLocation.StateLocated.Contains(s.StateCode) && s.FieldLocation.FieldLocationID == userMaster.UserLocation select s).ToList().Count();

            ViewBag.totalSUIApp = userMaster.UserLocation == "2" ? (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.LicenseTypeId == "SSA" select s).ToList().Count()
                                                               : (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.LicenseTypeId == "SSA" && s.FieldLocation.StateLocated.Contains(s.StateCode) && s.FieldLocation.FieldLocationID == userMaster.UserLocation select s).ToList().Count();


            ViewBag.totalTITAApp = userMaster.UserLocation == "2" ? (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.LicenseTypeId == "TITA" select s).ToList().Count()
                                                              : (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.LicenseTypeId == "TITA" && s.FieldLocation.StateLocated.Contains(s.StateCode) && s.FieldLocation.FieldLocationID == userMaster.UserLocation select s).ToList().Count();


            ViewBag.totalTCAApp = userMaster.UserLocation == "2" ? (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.LicenseTypeId == "TCA" select s).ToList().Count()
                                                              : (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.LicenseTypeId == "TCA" && s.FieldLocation.StateLocated.Contains(s.StateCode) && s.FieldLocation.FieldLocationID == userMaster.UserLocation select s).ToList().Count();


            ViewBag.totalTPBAApp = userMaster.UserLocation == "2" ? (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.LicenseTypeId.Contains("TPBA") select s).ToList().Count()
                                                              : (from s in dbCtxt.ApplicationRequests where s.LicenseReference != null && s.LicenseTypeId.Contains("TPBA") && s.FieldLocation.StateLocated.Contains(s.StateCode) && s.FieldLocation.FieldLocationID == userMaster.UserLocation select s).ToList().Count();


            int totalLicense = userMaster.UserLocation == "2" ? (from s in dbCtxt.ApplicationRequests select s).ToList().Count()
                                                            : (from s in dbCtxt.ApplicationRequests where s.FieldLocation.StateLocated.Contains(s.StateCode) && s.FieldLocation.StateLocated.Contains(s.StateCode) && s.FieldLocation.FieldLocationID == userMaster.UserLocation select s).ToList().Count();
            TempData["totalLicense"] = totalLicense;

            return View();
        }










        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetLicenseReport()
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var searchTxt = Request.Form.GetValues("search[value]")[0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;
            var staff = userMaster.UserLocation == "2" ? (from p in dbCtxt.ApplicationRequests
                                                          where p.LicenseReference != null
                                                          select new
                                                          {
                                                              p.ApplicationId,
                                                              category = p.ApplicationTypeId + " , " + p.LicenseTypeId,
                                                              p.ApplicantUserId,
                                                              p.ApplicantName,
                                                              capacity = p.StorageCapacity.ToString(),
                                                              issueddate = p.LicenseIssuedDate.ToString(),
                                                              expirydate = p.LicenseExpiryDate.ToString(),
                                                              issuedDATE = p.LicenseIssuedDate,
                                                              expiryDATE = p.LicenseExpiryDate,
                                                              legacy = p.IsLegacy,
                                                              StateName = (from s in dbCtxt.StateMasterLists where s.StateCode == p.StateCode select s.StateName).FirstOrDefault()
                                                          }) :
                         (from p in dbCtxt.ApplicationRequests
                          where p.LicenseReference != null && p.FieldLocation.StateLocated.Contains(p.StateCode) && p.FieldLocation.FieldLocationID == userMaster.UserLocation
                          select new
                          {
                              p.ApplicationId,
                              category = p.ApplicationTypeId + " , " + p.LicenseTypeId,
                              p.ApplicantUserId,
                              p.ApplicantName,
                              capacity = p.StorageCapacity.ToString(),
                              issueddate = p.LicenseIssuedDate.ToString(),
                              expirydate = p.LicenseExpiryDate.ToString(),
                              issuedDATE = p.LicenseIssuedDate,
                              expiryDATE = p.LicenseExpiryDate,
                              legacy = p.IsLegacy,
                              StateName = (from s in dbCtxt.StateMasterLists where s.StateCode == p.StateCode select s.StateName).FirstOrDefault()
                          });
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                if (searchTxt == "All License")
                {
                    staff = staff.Where(s => s.ApplicationId == s.ApplicationId);
                }
                else
                {
                    staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.ApplicantUserId.Contains(searchTxt)
                    || a.category.Contains(searchTxt) || a.capacity.Contains(searchTxt) || a.ApplicantName.Contains(searchTxt)
                    || a.issueddate.Contains(searchTxt) || a.expirydate.Contains(searchTxt));
                }
            }
            string firstdate = Request.Form.Get("mymin");
            string lastdate = Request.Form.Get("mymax");
            if ((!string.IsNullOrEmpty(firstdate) && (!string.IsNullOrEmpty(lastdate))))
            {
                var mindate = Convert.ToDateTime(firstdate).Date;
                var maxdate = Convert.ToDateTime(lastdate).Date;
                staff = staff.Where(a => a.ApplicationId == a.ApplicationId && DbFunctions.TruncateTime(a.issuedDATE) >= mindate && DbFunctions.TruncateTime(a.issuedDATE) <= maxdate);
            }

            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }









        [HttpGet]
        public JsonResult GetSupervisorAndOfficersByLocation()
        {

            logger.Info("Calling GetSupervisorAndOfficersByLocation");
            logger.Info("UserMaster Location =>" + userMaster.UserLocation);

            List<string> staffRoleList = new List<string> { "OFFICER", "SUPERVISOR" };
            var userRole = (from r in dbCtxt.UserMasters.Where(u => staffRoleList.Contains(u.UserRoles) && u.UserLocation == userMaster.UserLocation && u.Status == "ACTIVE")
                            select new
                            {
                                Userid = r.UserId,
                                Description = r.UserId + " (" + r.FirstName + ")" + " (" + r.UserRoles + ")"
                            }).ToList();

            logger.Info("Supervisor and Officer Count =>" + userRole.Count);

            return Json(userRole, JsonRequestBehavior.AllowGet);

        }








        [HttpGet]
        public JsonResult GetNonAdminUsersByLocation()
        {

            logger.Info("Calling GetNonAdminUsersByLocation ");
            logger.Info("UserMaster Location =>" + userMaster.UserLocation);

            List<string> staffRoleList = new List<string> { "OFFICER", "SUPERVISOR", "ZOPSCON", "HOD", "ADOPERATION" };
            var userRole = (from r in dbCtxt.UserMasters.Where(u => staffRoleList.Contains(u.UserRoles) && u.UserLocation == userMaster.UserLocation && u.Status == "ACTIVE")
                            select new
                            {
                                Userid = r.UserId,
                                Description = r.UserId + " (" + r.FirstName + ")" + " (" + r.UserRoles + ")"
                            }).ToList();

            logger.Info("Supervisor and Officer Count =>" + userRole.Count);

            return Json(userRole, JsonRequestBehavior.AllowGet);

        }



        [HttpGet]
        public JsonResult GetUsersLocationByRole(string UserLocation)
        {
            var userRole = (from r in dbCtxt.UserMasters.Where(u => u.UserRoles.Contains("OFFICER") && u.UserLocation == UserLocation && u.Status == "ACTIVE")
                            select new
                            {
                                Userid = r.UserId,
                                Description = r.UserId + " (" + r.FirstName + ")"
                            }).ToList();
            return Json(userRole, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetInspectionTypeList()
        {
            var inspectionType = (from r in dbCtxt.InspectionTypeMasterLists.Where(u => u.Status == "ACTIVE")
                                  select new
                                  {
                                      Id = r.Id,
                                      Description = r.Description
                                  }).ToList();
            return Json(inspectionType, JsonRequestBehavior.AllowGet);

        }



        [HttpGet]
        public JsonResult GetAssignedInspectionTypeUser(string applicationId, string inspectionType)
        {

            logger.Info("applicationId =>" + applicationId + ", inspectionType =>" + inspectionType);

            ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();

            if (inspectionType == "HOI" || inspectionType == "JOI")
            {
                return GetSupervisorAndOfficersByLocation();
            }
            else if (inspectionType == "FOI")
            {
                string userlocation = dbCtxt.RequestFieldMappings.Where(r => r.StateCode == appRequest.StateCode).FirstOrDefault().FieldLocationID;

                var userRole = (from r in dbCtxt.UserMasters.Where(u => u.UserRoles.Contains("ZOPSCON") && u.UserLocation == userlocation && u.Status == "ACTIVE")
                                select new
                                {
                                    Userid = r.UserId,
                                    Description = r.UserId + " (" + r.FirstName + ")" + " (" + r.UserRoles + ")"
                                }).ToList();
                return Json(userRole, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return null;
            }
        }



        [HttpGet]
        public JsonResult GetFieldLocation()
        {
            var FieldLocation = (from f in dbCtxt.FieldLocations
                                 where f.FieldType == "FD"
                                 select new
                                 {
                                     Fieldlocationid = f.FieldLocationID,
                                     Description = f.Description
                                 }).ToList();
            return Json(FieldLocation, JsonRequestBehavior.AllowGet);

        }






        [HttpGet]
        public JsonResult GetZonalLocation()
        {
            var ZonalLocation = (from z in dbCtxt.FieldLocations
                                 where z.FieldType == "ZN"
                                 select new
                                 {
                                     Fieldlocationid = z.FieldLocationID,
                                     Description = z.Description
                                 }).ToList();
            return Json(ZonalLocation, JsonRequestBehavior.AllowGet);

        }




        public ActionResult IGRPaymentList()
        {


            var mypayment = (from s in dbCtxt.ExtraPayments.AsEnumerable() where s.Status == "AUTH" && s.BankCode != "033" select s).GroupBy(x => x.RRReference).Select(x => x.LastOrDefault()).ToList();
            var extrapaymentid = mypayment.Select(c => c.RRReference);
            var mypayment1 = (from s in dbCtxt.PaymentLogs.AsEnumerable() where (s.Status == "AUTH" && (!extrapaymentid.Contains(s.RRReference))) && (s.LicenseTypeId == "SSA" || s.LicenseTypeId == "ATO" || s.LicenseTypeId == "ATM" || s.LicenseTypeId == "TCA" || s.LicenseTypeId == "TITA" || s.LicenseTypeId.Contains("TPBA")) select s).GroupBy(x => x.RRReference).Select(x => x.LastOrDefault()).ToList();


            var totalpayment = mypayment.Sum(s => s.TxnAmount);
            var totalpayment1 = mypayment1.Sum(s => s.TxnAmount);
            var totalservicechargepayment = mypayment.Sum(s => s.ServiceCharge);
            decimal Sumtotalpayment = Convert.ToDecimal(totalpayment + totalpayment1 + totalservicechargepayment);

            ViewBag.LoginUsersId = userMaster.UserRoles;


            TempData["totaligrpaymentcount"] = mypayment.ToList().Count() + mypayment1.ToList().Count();

            TempData["IgrSumtotalpayment"] = Sumtotalpayment.ToString("N") + " (" + commonHelper.NumberToWords(Convert.ToInt64(Sumtotalpayment)) + ")";
            return View();
        }







        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetIGRPaymentReport()
        {

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var searchTxt = Request.Form.GetValues("search[value]")[0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;

            var staff =
                (from p in dbCtxt.ExtraPayments.AsEnumerable()
                 where p.Status == "AUTH" && p.BankCode != "033"
                 select new
                 {
                     p.ExtraPaymentAppRef,
                     p.Status,
                     p.RRReference,
                     pay = p.TransactionID.ToString(),
                     amt = p.TxnAmount.ToString(),
                     totamt = (p.ServiceCharge + p.TxnAmount).ToString(),
                     servicecharge = p.ServiceCharge.ToString(),
                     //processingfee = p.ProcessingFee.ToString(),
                     //statutoryfee = p.StatutoryFee.ToString(),
                     transdate = p.TransactionDate.ToString(),
                     transDATE = p.TransactionDate,
                     category = p.SanctionType,
                     companyname = (from a in dbCtxt.ApplicationRequests where a.ApplicantUserId == p.ApplicantID select a.ApplicantName).FirstOrDefault(),
                     companyemail = p.ApplicantID

                 }).GroupBy(x => x.RRReference).Select(x => x.LastOrDefault());

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ExtraPaymentAppRef + " " + sortColumnDir);
            }
            if (searchTxt == "All Payment")
            {
                staff = staff.Where(s => s.ExtraPaymentAppRef == s.ExtraPaymentAppRef);
            }
            else
            {
                staff = staff.Where(a => a.ExtraPaymentAppRef.Contains(searchTxt) || a.ExtraPaymentAppRef.Contains(searchTxt)
               || a.Status.Contains(searchTxt) || a.RRReference.Contains(searchTxt) || a.companyemail.Contains(searchTxt) || a.companyname.Contains(searchTxt) || a.pay.Contains(searchTxt) || a.amt.Contains(searchTxt)
               || a.transdate.Contains(searchTxt));
            }


            string firstdate = Request.Form.Get("mymin");
            string lastdate = Request.Form.Get("mymax");
            if ((!string.IsNullOrEmpty(firstdate) && (!string.IsNullOrEmpty(lastdate))))
            {
                var mindate = Convert.ToDateTime(firstdate).Date;
                var maxdate = Convert.ToDateTime(lastdate).Date;
                staff = staff.Where(a => a.ExtraPaymentAppRef == a.ExtraPaymentAppRef && DbFunctions.TruncateTime(a.transDATE) >= mindate && DbFunctions.TruncateTime(a.transDATE) <= maxdate);
            }

            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }












        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetIGRPaymentReport1()
        {

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var searchTxt = Request.Form.GetValues("search[value]")[0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;

            var mypayment = (from s in dbCtxt.ExtraPayments.AsEnumerable() where s.Status == "AUTH" && s.BankCode != "033" select s).GroupBy(x => x.RRReference).Select(x => x.LastOrDefault()).ToList();

            var extrapaymentid = mypayment.Select(c => c.RRReference);

            var staff =
                (from p in dbCtxt.PaymentLogs.AsEnumerable()
                 where (p.Status == "AUTH" && (!extrapaymentid.Contains(p.RRReference))) && (p.LicenseTypeId == "SSA" || p.LicenseTypeId == "ATO" || p.LicenseTypeId == "ATM" || p.LicenseTypeId == "TITA" || p.LicenseTypeId == "TCA" || p.LicenseTypeId.Contains("TPBA"))
                 select new
                 {
                     p.ApplicationId,
                     p.Status,
                     p.RRReference,
                     pay = p.TransactionID.ToString(),
                     servicecharge = p.ServiceCharge.ToString(),
                     processingfee = p.ProcessingFee.ToString(),
                     amt = p.TxnAmount.ToString(),
                     transdate = p.TransactionDate.ToString(),
                     transDATE = p.TransactionDate,
                     category = (from a in dbCtxt.LicenseTypes where a.LicenseTypeId == p.LicenseTypeId select a.ShortName).FirstOrDefault(),
                     companyname = (from a in dbCtxt.ApplicationRequests where a.ApplicantUserId == p.ApplicantUserId select a.ApplicantName).FirstOrDefault(),
                     companyemail = p.ApplicantUserId

                 }).GroupBy(x => x.RRReference).Select(x => x.LastOrDefault());



            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (searchTxt == "All Payment")
            {
                staff = staff.Where(s => s.ApplicationId == s.ApplicationId);
            }
            else
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.ApplicationId.Contains(searchTxt)
               || a.Status.Contains(searchTxt) || a.RRReference.Contains(searchTxt) || a.companyemail.Contains(searchTxt) || a.companyname.Contains(searchTxt) || a.pay.Contains(searchTxt) || a.amt.Contains(searchTxt)
               || a.transdate.Contains(searchTxt));
            }


            string firstdate = Request.Form.Get("mymin1");
            string lastdate = Request.Form.Get("mymax1");
            if ((!string.IsNullOrEmpty(firstdate) && (!string.IsNullOrEmpty(lastdate))))
            {
                var mindate = Convert.ToDateTime(firstdate).Date;
                var maxdate = Convert.ToDateTime(lastdate).Date;
                staff = staff.Where(a => a.ApplicationId == a.ApplicationId && DbFunctions.TruncateTime(a.transDATE) >= mindate && DbFunctions.TruncateTime(a.transDATE) <= maxdate);
            }

            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }




        public ActionResult EditIGRRemita(FormCollection coll)
        {
            var rrr = coll.Get("rrr");
            var refnum = coll.Get("Appid");
            var amount = coll.Get("amount");

            var editPaymentlog = (from a in dbCtxt.PaymentLogs where a.ApplicationId == refnum select a).FirstOrDefault();
            var editExtrapayment = (from a in dbCtxt.ExtraPayments where a.ExtraPaymentAppRef == refnum select a).FirstOrDefault();

            if (editPaymentlog != null)
            {
                editPaymentlog.RRReference = rrr;
                editPaymentlog.TxnAmount = Convert.ToDecimal(amount);

            }
            if (editExtrapayment != null)
            {
                editExtrapayment.RRReference = rrr;
                editExtrapayment.TxnAmount = Convert.ToDecimal(amount);

            }
            dbCtxt.SaveChanges();

            return RedirectToAction("IGRPaymentList");
        }








        //public ActionResult PaymentList()
        //{
        //    var mypayment = userMaster.UserLocation == "2" ? (from s in dbCtxt.PaymentLogs
        //                                                      where s.Status == "AUTH"
        //                                                      select s).ToList().GroupBy(x => x.ApplicationId).Select(x => x.LastOrDefault()).ToList()
        //                                                    :
        //                                                    (from s in dbCtxt.PaymentLogs
        //                                                     join r in dbCtxt.ApplicationRequests on s.ApplicationId equals r.ApplicationId
        //                                                     where s.Status == "AUTH" && r.FieldLocation.StateLocated.Contains(r.StateCode) && r.FieldLocation.FieldLocationID == userMaster.UserLocation
        //                                                     select s).ToList().GroupBy(x => x.ApplicationId).Select(x => x.LastOrDefault()).ToList();

        //    var totalpayment = mypayment.Sum(s => s.TxnAmount);
        //    decimal Sumtotalpayment = Convert.ToDecimal(totalpayment);
        //    TempData["totalpaymentcount"] = mypayment.ToList().Count();
        //    TempData["Sumtotalpayment"] = Sumtotalpayment.ToString("N") + " (" + commonHelper.NumberToWords(Convert.ToInt64(Sumtotalpayment)) + ")";
        //    var SSA = (from s in dbCtxt.PaymentLogs where s.Status == "AUTH" && s.LicenseTypeId == "SSA" select s).ToList().GroupBy(x => x.ApplicationId).Select(x => x.LastOrDefault()).ToList();
        //    var PTE = (from s in dbCtxt.PaymentLogs where s.Status == "AUTH" && s.LicenseTypeId == "PTE" select s).ToList().GroupBy(x => x.ApplicationId).Select(x => x.LastOrDefault()).ToList();
        //    var ATC = (from s in dbCtxt.PaymentLogs where s.Status == "AUTH" && s.LicenseTypeId == "ATC" select s).ToList().GroupBy(x => x.ApplicationId).Select(x => x.LastOrDefault()).ToList();
        //    var LTO = (from s in dbCtxt.PaymentLogs where s.Status == "AUTH" && s.LicenseTypeId == "LTO" select s).ToList().GroupBy(x => x.ApplicationId).Select(x => x.LastOrDefault()).ToList();
        //    var ATM = (from s in dbCtxt.PaymentLogs where s.Status == "AUTH" && s.LicenseTypeId == "ATM" select s).ToList().GroupBy(x => x.ApplicationId).Select(x => x.LastOrDefault()).ToList();
        //    var ATO = (from s in dbCtxt.PaymentLogs where s.Status == "AUTH" && s.LicenseTypeId == "ATO" select s).ToList().GroupBy(x => x.ApplicationId).Select(x => x.LastOrDefault()).ToList();
        //    var ATCLFP = (from s in dbCtxt.PaymentLogs where s.Status == "AUTH" && s.LicenseTypeId == "ATCLFP" select s).ToList().GroupBy(x => x.ApplicationId).Select(x => x.LastOrDefault()).ToList();
        //    var LTOLFP = (from s in dbCtxt.PaymentLogs where s.Status == "AUTH" && s.LicenseTypeId == "LTOLFP" select s).ToList().GroupBy(x => x.ApplicationId).Select(x => x.LastOrDefault()).ToList();
        //    var TCA = (from s in dbCtxt.PaymentLogs where s.Status == "AUTH" && s.LicenseTypeId == "TCA" select s).ToList().GroupBy(x => x.ApplicationId).Select(x => x.LastOrDefault()).ToList();
        //    var TITA = (from s in dbCtxt.PaymentLogs where s.Status == "AUTH" && s.LicenseTypeId == "TITA" select s).ToList().GroupBy(x => x.ApplicationId).Select(x => x.LastOrDefault()).ToList();
        //    var TPBAPLW = (from s in dbCtxt.PaymentLogs where s.Status == "AUTH" && s.LicenseTypeId == "TPBA-PLW" select s).ToList().GroupBy(x => x.ApplicationId).Select(x => x.LastOrDefault()).ToList();
        //    var TPBAPRW = (from s in dbCtxt.PaymentLogs where s.Status == "AUTH" && s.LicenseTypeId == "TPBA-PRW" select s).ToList().GroupBy(x => x.ApplicationId).Select(x => x.LastOrDefault()).ToList();

        //    //FG Only
        //    ViewBag.PTETotal = Convert.ToDecimal(PTE.Sum(s => s.TxnAmount)).ToString("N");
        //    ViewBag.ATCTotal = Convert.ToDecimal(ATC.Sum(s => s.TxnAmount)).ToString("N");
        //    ViewBag.LTOTotal = Convert.ToDecimal(LTO.Sum(s => s.TxnAmount)).ToString("N");

        //    //IGR and BrandOne
        //    ViewBag.SSATotal = Convert.ToDecimal(SSA.Sum(s => s.TxnAmount)).ToString("N");
        //    ViewBag.SSABrandOne = Math.Round(Convert.ToDecimal(ViewBag.SSATotal) * Convert.ToDecimal(0.1), 2).ToString("N");
        //    ViewBag.SSAIGR = Math.Round(Convert.ToDecimal(ViewBag.SSATotal) - Convert.ToDecimal(ViewBag.SSABrandOne), 2).ToString("N");

        //    ViewBag.ATMTotal = Convert.ToDecimal(ATM.Sum(s => s.TxnAmount)).ToString("N");
        //    ViewBag.ATMBrandOne = Math.Round(Convert.ToDecimal(ViewBag.ATMTotal) * Convert.ToDecimal(0.1), 2).ToString("N");
        //    ViewBag.ATMIGR = Math.Round(Convert.ToDecimal(ViewBag.ATMTotal) - Convert.ToDecimal(ViewBag.ATMBrandOne), 2).ToString("N");

        //    ViewBag.ATOTotal = Convert.ToDecimal(ATO.Sum(s => s.TxnAmount)).ToString("N");
        //    ViewBag.ATOBrandOne = Math.Round(Convert.ToDecimal(ViewBag.ATOTotal) * Convert.ToDecimal(0.1), 2).ToString("N");
        //    ViewBag.ATOIGR = Math.Round(Convert.ToDecimal(ViewBag.ATOTotal) - Convert.ToDecimal(ViewBag.ATOBrandOne), 2).ToString("N");

        //    ViewBag.ATCLFPTotal = Convert.ToDecimal(ATCLFP.Sum(s => s.TxnAmount)).ToString("N");
        //    ViewBag.ATCLFPBrandOne = Math.Round(Convert.ToDecimal(ViewBag.ATCLFPTotal) * Convert.ToDecimal(0.1), 2).ToString("N");
        //    ViewBag.ATCLFPIGR = Math.Round(Convert.ToDecimal(ViewBag.ATCLFPTotal) - Convert.ToDecimal(ViewBag.ATCLFPBrandOne), 2).ToString("N");

        //    ViewBag.LTOLFPTotal = Convert.ToDecimal(LTOLFP.Sum(s => s.TxnAmount)).ToString("N");
        //    ViewBag.LTOLFPBrandOne = Math.Round(Convert.ToDecimal(ViewBag.LTOLFPTotal) * Convert.ToDecimal(0.1), 2).ToString("N");
        //    ViewBag.LTOLFPIGR = Math.Round(Convert.ToDecimal(ViewBag.LTOLFPTotal) - Convert.ToDecimal(ViewBag.LTOLFPBrandOne), 2).ToString("N");

        //    ViewBag.TCATotal = Convert.ToDecimal(TCA.Sum(s => s.TxnAmount)).ToString("N");
        //    ViewBag.TCABrandOne = Math.Round(Convert.ToDecimal(ViewBag.TCATotal) * Convert.ToDecimal(0.1), 2).ToString("N");
        //    ViewBag.TCAIGR = Math.Round(Convert.ToDecimal(ViewBag.TCATotal) - Convert.ToDecimal(ViewBag.TCABrandOne), 2).ToString("N");

        //    ViewBag.TITATotal = Convert.ToDecimal(TITA.Sum(s => s.TxnAmount)).ToString("N");
        //    ViewBag.TITABrandOne = Math.Round(Convert.ToDecimal(ViewBag.TITATotal) * Convert.ToDecimal(0.1), 2).ToString("N");
        //    ViewBag.TITAIGR = Math.Round(Convert.ToDecimal(ViewBag.TITATotal) - Convert.ToDecimal(ViewBag.TITABrandOne), 2).ToString("N");

        //    ViewBag.TPBAPLWTotal = Convert.ToDecimal(TPBAPLW.Sum(s => s.TxnAmount)).ToString("N");
        //    ViewBag.TPBAPLWBrandOne = Math.Round(Convert.ToDecimal(ViewBag.TPBAPLWTotal) * Convert.ToDecimal(0.1), 2).ToString("N");
        //    ViewBag.TPBAPLWIGR = Math.Round(Convert.ToDecimal(ViewBag.TPBAPLWTotal) - Convert.ToDecimal(ViewBag.TPBAPLWBrandOne), 2).ToString("N");

        //    ViewBag.TPBAPRWTotal = Convert.ToDecimal(TPBAPRW.Sum(s => s.TxnAmount)).ToString("N");
        //    ViewBag.TPBAPRWBrandOne = Math.Round(Convert.ToDecimal(ViewBag.TPBAPRWTotal) * Convert.ToDecimal(0.1), 2).ToString("N");
        //    ViewBag.TPBAPRWIGR = Math.Round(Convert.ToDecimal(ViewBag.TPBAPRWTotal) - Convert.ToDecimal(ViewBag.TPBAPRWBrandOne), 2).ToString("N");

        //    ViewBag.GrandTotalIGR = Math.Round(Convert.ToDecimal(ViewBag.SSAIGR) + Convert.ToDecimal(ViewBag.ATMIGR) + Convert.ToDecimal(ViewBag.ATOIGR) + Convert.ToDecimal(ViewBag.ATCLFPIGR) + Convert.ToDecimal(ViewBag.LTOLFPIGR) + Convert.ToDecimal(ViewBag.TCAIGR) + Convert.ToDecimal(ViewBag.TITAIGR) + Convert.ToDecimal(ViewBag.TPBAPLWIGR) + Convert.ToDecimal(ViewBag.TPBAPRWIGR), 2).ToString("N");
        //    ViewBag.GrandTotalBrandOne = Math.Round(Convert.ToDecimal(ViewBag.SSABrandOne) + Convert.ToDecimal(ViewBag.ATMBrandOne) + Convert.ToDecimal(ViewBag.ATOBrandOne) + Convert.ToDecimal(ViewBag.ATCLFPBrandOne) + Convert.ToDecimal(ViewBag.LTOLFPBrandOne) + Convert.ToDecimal(ViewBag.TCABrandOne) + Convert.ToDecimal(ViewBag.TITABrandOne) + Convert.ToDecimal(ViewBag.TPBAPLWBrandOne) + Convert.ToDecimal(ViewBag.TPBAPRWBrandOne), 2).ToString("N");
        //    ViewBag.GrandTotalFG = Math.Round(Convert.ToDecimal(ViewBag.PTETotal) + Convert.ToDecimal(ViewBag.ATCTotal) + Convert.ToDecimal(ViewBag.LTOTotal), 2).ToString("N");
        //    ViewBag.GrandTotal = Math.Round(Convert.ToDecimal(ViewBag.PTETotal) + Convert.ToDecimal(ViewBag.ATCTotal) + Convert.ToDecimal(ViewBag.LTOTotal) + Convert.ToDecimal(ViewBag.TPBAPRWTotal) + Convert.ToDecimal(ViewBag.TPBAPLWTotal) + Convert.ToDecimal(ViewBag.TITATotal) + Convert.ToDecimal(ViewBag.TCATotal) + Convert.ToDecimal(ViewBag.LTOLFPTotal) + Convert.ToDecimal(ViewBag.ATCLFPTotal) + Convert.ToDecimal(ViewBag.ATOTotal) + Convert.ToDecimal(ViewBag.ATMTotal) + Convert.ToDecimal(ViewBag.SSATotal), 2).ToString("N");

        //    return View();
        //}




        public ActionResult PaymentList()
        {

            var payment = JsonConvert.SerializeObject(FilterPayments());
            var data = JsonConvert.DeserializeObject<List<PaymentReportViewModel>>(payment);
            PaymentRevenue();
            var field = dbCtxt.FieldLocations.FirstOrDefault(x => x.FieldLocationID == userMaster.UserLocation);

            if (field.FieldType.Equals("FD"))
            {
                var state = (from r in dbCtxt.RequestFieldMappings
                             join f in dbCtxt.FieldLocations on r.FieldLocationID equals f.FieldLocationID
                             where r.FieldLocationID.Equals(userMaster.UserLocation)
                             select new { f.FieldLocationID, f.StateLocated }).ToList();
                data = data.Where(x => state.Any(y => y.StateLocated.ToLower().Equals(x.StateCode.ToLower()))).ToList();
            }
            else if (field.FieldType.Equals("ZN"))
            {
                var fieldzones = dbCtxt.ZoneFieldMappings.Where(x => x.ZoneFieldID == userMaster.UserLocation).Select(y => y.FieldLocationID).ToList();
                var statecodess = dbCtxt.RequestFieldMappings.Where(x => fieldzones.Contains(x.FieldLocationID)).Select(x => x.StateCode.ToLower()).ToList();
                var state = dbCtxt.StateMasterLists.Where(x => statecodess.Contains(x.StateCode.ToLower())).Select(x => x.StateName).ToList();
                data = data.Where(x => state.Contains(x.StateName)).ToList();
            }

            var totalpaymentcompleted = data.Where(t => t.Status == "AUTH").ToList();
            //var totalpaymentcompleted = data.Sum(s => decimal.Parse(s.amt));
            decimal Sumtotalpaymentcompleted = totalpaymentcompleted.Sum(s => decimal.Parse(s.amt));
            ViewBag.totalpaymentcompletedcount = totalpaymentcompleted.Count();
            ViewBag.Sumtotalpaymentcompleted = Sumtotalpaymentcompleted.ToString("N") + " (" + commonHelper.NumberToWords(Convert.ToInt64(Sumtotalpaymentcompleted)) + ")";

            var totalpendingpayment = data.Where(t => t.Status == "INIT").ToList();
            //var totalpendingpayment = data.Sum(s => decimal.Parse(s.amt));
            decimal Sumtotalpendingpayment = totalpendingpayment.Sum(s => decimal.Parse(s.amt));
            ViewBag.totalpendingpaymentcount = totalpendingpayment.Count();
            ViewBag.Sumtotalpendingpayment = Sumtotalpendingpayment.ToString("N") + " (" + commonHelper.NumberToWords(Convert.ToInt64(Sumtotalpendingpayment)) + ")";


            return View(data);
        }


        [AllowAnonymous]
        [HttpPost]
        public ActionResult PaymentList(string min, string maxdate, List<string> status, List<string> cat, List<string> apptype, List<string> office)
        {
            var payments = JsonConvert.SerializeObject(FilterPayments());
            var data = JsonConvert.DeserializeObject<List<PaymentReportViewModel>>(payments);
            var field = dbCtxt.FieldLocations.FirstOrDefault(x => x.FieldLocationID == userMaster.UserLocation);

            if (field.FieldType.Equals("FD"))
            {
                var state = (from r in dbCtxt.RequestFieldMappings
                             join f in dbCtxt.FieldLocations on r.FieldLocationID equals f.FieldLocationID
                             where r.FieldLocationID.Equals(userMaster.UserLocation)
                             select new { f.FieldLocationID, f.StateLocated }).ToList();
                data = data.Where(x => state.Any(y => y.StateLocated.ToLower().Equals(x.StateCode.ToLower()))).ToList();
            }
            else if (field.FieldType.Equals("ZN"))
            {
                var fieldzones = dbCtxt.ZoneFieldMappings.Where(x => x.ZoneFieldID == userMaster.UserLocation).Select(y => y.FieldLocationID).ToList();
                var statecodess = dbCtxt.RequestFieldMappings.Where(x => fieldzones.Contains(x.FieldLocationID)).Select(x => x.StateCode.ToLower()).ToList();
                var state = dbCtxt.StateMasterLists.Where(x => statecodess.Contains(x.StateCode.ToLower())).Select(x => x.StateName).ToList();
                data = data.Where(x => state.Contains(x.StateName)).ToList();
            }

            if ((!string.IsNullOrEmpty(min) && (!string.IsNullOrEmpty(maxdate))))
            {
                var startdate = Convert.ToDateTime(min).Date;
                var enddate = Convert.ToDateTime(maxdate).Date;
                data = data.Where(a => a.transDATE >= new DateTime(startdate.Year, startdate.Month, startdate.Day, 00, 00, 00) && a.transDATE <= new DateTime(enddate.Year, enddate.Month, enddate.Day, 23, 59, 59)).ToList();
            }
            if (office?.Count() > 0)
                data = data.Where(x => office.Contains(x.StateCode)).ToList();

            if (status?.Count() > 0)
                data = data.Where(x => status.Contains(x.Status)).ToList();

            if (cat?.Count() > 0)
                data = data.Where(x => cat.Contains(x.Category)).ToList();

            if (apptype?.Count() > 0)
                data = data.Where(x => apptype.Contains(x.ApplicationType)).ToList();

            var totalpaymentcompleted = data.Where(t => t.Status == "AUTH").ToList();
            decimal Sumtotalpaymentcompleted = totalpaymentcompleted.Sum(s => decimal.Parse(s.amt));
            ViewBag.totalpaymentcompletedcount = totalpaymentcompleted.Count();
            ViewBag.Sumtotalpaymentcompleted = Sumtotalpaymentcompleted.ToString("N") + " (" + commonHelper.NumberToWords(Convert.ToInt64(Sumtotalpaymentcompleted)) + ")";

            var totalpendingpayment = data.Where(t => t.Status == "INIT").ToList();
            decimal Sumtotalpendingpayment = totalpendingpayment.Sum(s => decimal.Parse(s.amt));
            ViewBag.totalpendingpaymentcount = totalpendingpayment.Count();
            ViewBag.Sumtotalpendingpayment = Sumtotalpendingpayment.ToString("N") + " (" + commonHelper.NumberToWords(Convert.ToInt64(Sumtotalpendingpayment)) + ")";

            ViewData["startdate"] = min;
            ViewData["maxdate"] = maxdate;
            PaymentRevenue();

            return View(data);

        }

        public void PaymentRevenue()
        {


            appDocHelper.GetApplicationRevenue(out SSARevenue, out ATCRevenue, out LTORevenue, out ATORevenue, out PTERevenue, out TITARevenue,
                out ATMRevenue, out ATCLFPRevenue, out LTOLFPRevenue, out TPBAPLWRevenue, out TPBAPRWRevenue, out RevenueGrandTotal, out TCARevenue, out SSABrandOne, out SSAIGR,
                out ATOBrandOne, out ATOIGR, out ATMBrandOne, out ATMIGR, out ATCLFPBrandOne, out ATCLFPIGR, out LTOLFPBrandOne, out LTOLFPIGR, out TPBAPLWBrandOne, out TPBAPLWIGR, out TPBAPRWBrandOne,
                out TPBAPRWIGR, out TITABrandOne, out TITAIGR, out TCABrandOne, out TCAIGR, out GrandTotalIGR, out  GrandTotalBrandOne, out  GrandTotalFG);

            ViewBag.SSATotal = SSARevenue.ToString("N"); ViewBag.ATCTotal = ATCRevenue.ToString("N"); ViewBag.LTOTotal = LTORevenue.ToString("N"); ViewBag.ATOTotal = ATORevenue.ToString("N"); ViewBag.PTETotal = PTERevenue.ToString("N");
            ViewBag.TITATotal = TITARevenue.ToString("N"); ViewBag.ATMTotal = ATMRevenue.ToString("N"); ViewBag.ATCLFPTotal = ATCLFPRevenue.ToString("N"); ViewBag.LTOLFPTotal = LTOLFPRevenue.ToString("N"); ViewBag.GrandTotal = RevenueGrandTotal.ToString("N");
            ViewBag.TPBAPLWTotal = TPBAPLWRevenue.ToString("N"); ViewBag.TPBAPRWTotal = TPBAPRWRevenue.ToString("N"); ViewBag.TCATotal = TCARevenue.ToString("N"); 
            
            ViewBag.SSABrandOne = SSABrandOne; ViewBag.SSAIGR = SSAIGR; ViewBag.ATOBrandOne = ATOBrandOne;
            ViewBag.ATOIGR = ATOIGR; ViewBag.ATMBrandOne = ATMBrandOne; ViewBag.ATMIGR = ATMIGR; ViewBag.ATCLFPBrandOne = ATCLFPBrandOne; ViewBag.ATCLFPIGR = ATCLFPIGR; ViewBag.LTOLFPBrandOne = LTOLFPBrandOne; 
            ViewBag.LTOLFPIGR = LTOLFPIGR; ViewBag.TPBAPLWBrandOne = TPBAPLWBrandOne; ViewBag.TPBAPLWIGR = TPBAPLWIGR; ViewBag.TPBAPRWBrandOne = TPBAPRWBrandOne; ViewBag.TPBAPRWIGR = TPBAPRWIGR; 
            ViewBag.TITABrandOne = TITABrandOne; ViewBag.TITAIGR = TITAIGR; ViewBag.TCABrandOne = TCABrandOne; ViewBag.TCAIGR = TCAIGR;
            ViewBag.GrandTotalIGR = GrandTotalIGR; ViewBag.GrandTotalBrandOne = GrandTotalBrandOne; ViewBag.GrandTotalFG = GrandTotalFG;

        }

        private object FilterPayments()
        {
            return (from p in dbCtxt.PaymentLogs
                    join r in dbCtxt.ApplicationRequests on p.ApplicationId equals r.ApplicationId
                    select new
                    {
                        p.ApplicationId,
                        p.Status,
                        p.RRReference,
                        pay = p.PaymentId.ToString(),
                        amt = p.TxnAmount.ToString(),
                        transdate = p.TransactionDate.ToString(),
                        transDATE = p.TransactionDate,
                        StateName = (from s in dbCtxt.StateMasterLists where s.StateCode == r.StateCode select s.StateName).FirstOrDefault(),
                        StateCode = r.StateCode,
                        ApplicationType = r.ApplicationTypeId,
                        Category = r.LicenseTypeId
                    });
           
        }


        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetPaymentReport()
        {

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var searchTxt = Request.Form.GetValues("search[value]")[0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;


            var staff = userMaster.UserLocation.Equals(2.ToString()) ?
               (from p in dbCtxt.PaymentLogs
                join r in dbCtxt.ApplicationRequests on p.ApplicationId equals r.ApplicationId
                where p.Status == "AUTH"
                select new
                {
                    p.ApplicationId,
                    p.Status,
                    p.RRReference,
                    pay = p.PaymentId.ToString(),
                    amt = p.TxnAmount.ToString(),
                    servicecharge = p.ServiceCharge.ToString(),
                    processingfee = p.ProcessingFee.ToString(),
                    statutoryfee = p.StatutoryFee.ToString(),
                    transdate = p.TransactionDate.ToString(),
                    transDATE = p.TransactionDate,
                    category = r.ApplicationTypeId + " , " + r.LicenseTypeId,
                    companyname = r.ApplicantName,
                    companyemail = r.ApplicantUserId,
                    StateName = (from s in dbCtxt.StateMasterLists where s.StateCode == r.StateCode select s.StateName).FirstOrDefault()

                })
                : (from p in dbCtxt.PaymentLogs
                   join r in dbCtxt.ApplicationRequests on p.ApplicationId equals r.ApplicationId
                   join l in dbCtxt.RequestFieldMappings on r.StateCode equals l.StateCode
                   join t in dbCtxt.UserMasters on l.FieldLocationID equals t.UserLocation
                   where p.Status == "AUTH"
                   && t.UserId == userMaster.UserId
                   select new
                   {
                       p.ApplicationId,
                       p.Status,
                       p.RRReference,
                       pay = p.PaymentId.ToString(),
                       amt = p.TxnAmount.ToString(),
                       servicecharge = p.ServiceCharge.ToString(),
                       processingfee = p.ProcessingFee.ToString(),
                       statutoryfee = p.StatutoryFee.ToString(),
                       transdate = p.TransactionDate.ToString(),
                       transDATE = p.TransactionDate,
                       category = r.ApplicationTypeId + " , " + r.LicenseTypeId,
                       companyname = r.ApplicantName,
                       companyemail = r.ApplicantUserId,
                       StateName = (from s in dbCtxt.StateMasterLists where s.StateCode == l.StateCode select s.StateName).FirstOrDefault()
                   });






            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (searchTxt == "All Payment")
            {
                staff = staff.Where(s => s.ApplicationId == s.ApplicationId);
            }
            else
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.ApplicationId.Contains(searchTxt) || a.category.Contains(searchTxt)
               || a.Status.Contains(searchTxt) || a.RRReference.Contains(searchTxt) || a.companyemail.Contains(searchTxt) || a.companyname.Contains(searchTxt) || a.pay.Contains(searchTxt) || a.amt.Contains(searchTxt)
               || a.transdate.Contains(searchTxt) || a.statutoryfee.Contains(searchTxt) || a.processingfee.Contains(searchTxt) || a.servicecharge.Contains(searchTxt));
            }


            string firstdate = Request.Form.Get("mymin");
            string lastdate = Request.Form.Get("mymax");
            if ((!string.IsNullOrEmpty(firstdate) && (!string.IsNullOrEmpty(lastdate))))
            {
                var mindate = Convert.ToDateTime(firstdate).Date;
                var maxdate = Convert.ToDateTime(lastdate).Date;
                staff = staff.Where(a => a.ApplicationId == a.ApplicationId && DbFunctions.TruncateTime(a.transDATE) >= mindate && DbFunctions.TruncateTime(a.transDATE) <= maxdate);
            }

            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }












        [HttpGet]
        public JsonResult GetCompanyNameAutoSearch(string term = "")
        {

            var CompanyName = (from a in dbCtxt.ApplicationRequests
                               where a.ApplicantName.Contains(term)
                               group new
                               {
                                   a.ApplicantName
                               }
                                by new
                                {
                                    a.ApplicantName
                                }
                                        into g
                               orderby g.Key.ApplicantName

                               select new
                               {
                                   textvalue = g.Key.ApplicantName,
                               }).ToList();
            return Json(CompanyName, JsonRequestBehavior.AllowGet);

        }








        [HttpGet]
        public JsonResult AutoSearchCompanyAppId(string term = "")
        {
            var CompanyAppId = (from a in dbCtxt.PaymentLogs
                                where a.ApplicationId.Contains(term)
                                select new
                                {
                                    textvalue = a.ApplicationId
                                }).ToList();
            return Json(CompanyAppId, JsonRequestBehavior.AllowGet);
        }









        [HttpGet]
        public ActionResult GetApplicationChart(ApplicationRatio Appobj)
        {
            int totalapplication = (from a in dbCtxt.ApplicationRequests select a).ToList().Count();
            int CompleteApplication = (from a in dbCtxt.ApplicationRequests where a.LicenseReference != null select a).ToList().Count();
            int UnCompleteApplication = (from a in dbCtxt.ApplicationRequests where a.LicenseReference == null select a).ToList().Count();
            Appobj.totalapplication = totalapplication;
            Appobj.CompleteApplication = CompleteApplication;
            Appobj.UnCompleteApplication = UnCompleteApplication;
            return Json(Appobj, JsonRequestBehavior.AllowGet);
        }











        [HttpGet]
        public ActionResult GetStaffChart(StaffRatio staffobj)
        {
            int totalstaff = (from s in dbCtxt.UserMasters select s).ToList().Count();
            int ADOPERATIONRBP = (from s in dbCtxt.UserMasters
                                  where s.UserRoles == "AD RBP"
                                  select s).ToList().Count();
            int FIELDADMIN = (from s in dbCtxt.UserMasters
                              where s.UserRoles == "FIELDADMIN"
                              select s).ToList().Count();
            int HEADADMIN = (from s in dbCtxt.UserMasters
                             where s.UserRoles == "HEADADMIN"
                             select s).ToList().Count();
            int HEADGAS = (from s in dbCtxt.UserMasters
                           where s.UserRoles == "HOD"
                           select s).ToList().Count();
            int OFFICER = (from s in dbCtxt.UserMasters
                           where s.UserRoles == "HOOD"
                           select s).ToList().Count();
            int SUPERADMIN = (from s in dbCtxt.UserMasters
                              where s.UserRoles == "SUPERADMIN"
                              select s).ToList().Count();
            int SUPERVISOR = (from s in dbCtxt.UserMasters
                              where s.UserRoles == "SUPERVISOR"
                              select s).ToList().Count();
            int REVIWER = (from s in dbCtxt.UserMasters
                           where s.UserRoles == "REVIWER"
                           select s).ToList().Count();
            int ZONALADMIN = (from s in dbCtxt.UserMasters
                              where s.UserRoles == "ZONALADMIN"
                              select s).ToList().Count();
            int ZOPSCON = (from s in dbCtxt.UserMasters
                           where s.UserRoles == "ZOPSCON"
                           select s).ToList().Count();
            int OPSCON = (from s in dbCtxt.UserMasters
                          where s.UserRoles == "OPSCON"
                          select s).ToList().Count();
            staffobj.totalstaff = totalstaff;
            staffobj.ADOPERATIONRBP = ADOPERATIONRBP;
            staffobj.FIELDADMIN = FIELDADMIN;
            staffobj.HEADADMIN = HEADADMIN;
            staffobj.HEADGAS = HEADGAS;
            staffobj.OFFICER = OFFICER;
            staffobj.SUPERADMIN = SUPERADMIN;
            staffobj.SUPERVISOR = SUPERVISOR;
            staffobj.ZONALADMIN = ZONALADMIN;
            staffobj.ZOPSCON = ZOPSCON;
            return Json(staffobj, JsonRequestBehavior.AllowGet);
        }





        [HttpGet]
        public ActionResult StaffReport()
        {
            int totalstaff = (from s in dbCtxt.UserMasters where s.UserType != "COMPANY" select s).ToList().Count();
            TempData["totalstaff"] = totalstaff;

            return View();
        }


        [HttpGet]
        public ActionResult StaffReport_bk()
        {
            logger.Info("Calling StaffReport to Analyse Report");
            List<UserMaster> userList = new List<UserMaster>();
            try
            {
                ViewBag.ErrorMessage = "SUCCESS";
                userList = dbCtxt.UserMasters.Where(u => u.UserType != "COMPANY").ToList<UserMaster>();
                ViewBag.AllUserList = userList;
                ViewBag.totalstaff = userList.Capacity;
                logger.Info("Staff Count  =>" + userList.Capacity);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                ViewBag.ErrorMessage = "Error When Getting Staff List, Please Try again Later";
            }

            return View();
        }


        [HttpPost]
        public ActionResult GetStaffReport()
        {
            var role = TempData["role"];
            var field = TempData["field"];
            var zone = TempData["zone"];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var searchTxt = Request.Form.GetValues("search[value]")[0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var staff = userMaster.UserLocation == "2" ? (from u in dbCtxt.UserMasters
                                                          where u.UserType != "COMPANY"

                                                          select new
                                                          {
                                                              u.UserId,
                                                              Fullname = u.FirstName + " " + u.LastName,
                                                              u.UserRoles,
                                                              u.Status,
                                                              u.FieldLocation.Description,
                                                              u.UserLocation

                                                          }) : (from u in dbCtxt.UserMasters
                                                                where u.UserType != "COMPANY" && u.UserLocation == userMaster.UserLocation

                                                                select new
                                                                {
                                                                    u.UserId,
                                                                    Fullname = u.FirstName + " " + u.LastName,
                                                                    u.UserRoles,
                                                                    u.Status,
                                                                    u.FieldLocation.Description,
                                                                    u.UserLocation

                                                                });
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(u => u.UserId + " " + sortColumnDir);
            }

            if (!string.IsNullOrEmpty(searchTxt))
            {
                if (searchTxt == "ALLRoles")
                {
                    staff = staff.Where(s => s.UserRoles == s.UserRoles);
                }

                else if (searchTxt == "ALLFields")
                {
                    staff = staff.Where(s => s.UserLocation != 2.ToString() && s.UserLocation != 3.ToString() && s.UserLocation != 4.ToString() && s.UserLocation != 5.ToString() && s.UserLocation != 6.ToString()
                    && s.UserLocation != 7.ToString() && s.UserLocation != 8.ToString() && s.UserLocation != 9.ToString());
                }
                else if (searchTxt == "ALLZones")
                {
                    staff = staff.Where(s => s.UserLocation == 3.ToString() || s.UserLocation == 4.ToString() || s.UserLocation == 5.ToString() || s.UserLocation == 6.ToString()
                    || s.UserLocation == 7.ToString() || s.UserLocation == 8.ToString() || s.UserLocation == 9.ToString());
                }
                else
                {
                    staff = staff.Where(u => u.UserId.Contains(searchTxt) || u.Fullname.Contains(searchTxt)
                   || u.UserRoles.Contains(searchTxt) || u.Status.Contains(searchTxt) || u.Description.Contains(searchTxt) || u.UserLocation == searchTxt);

                }
            }

            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }

        //     [HttpGet]
        //     public ActionResult StaffDesk()
        //     {
        //         string ErrorMessage = "SUCCESS";
        //         StaffDeskModel model = new StaffDeskModel();
        //         List<StaffDesk> staffDeskList = new List<StaffDesk>();
        //         List<string> exemptUserRoles = new List<string> {
        // "COMPANY",
        // "HEADADMIN",
        // "SUPERADMIN",
        // "ZONALADMIN",
        // "FIELDADMIN",
        // "BRANDMIN",
        // "SUPPORT"
        //};

        //         try
        //         {
        //             ViewBag.ErrorMessage = "SUCCESS";

        //             foreach (UserMaster up in dbCtxt.UserMasters.Where(u => !exemptUserRoles.Contains(u.UserRoles)).ToList())
        //             {

        //                 staffDeskList.Add(new StaffDesk()
        //                 {
        //                     Role = up.UserRoles,
        //                     BranchName = dbCtxt.FieldLocations.Where(f => f.FieldLocationID == up.UserLocation).FirstOrDefault().Description,
        //                     StaffEmail = up.UserId,
        //                     StaffName = up.FirstName,
        //                     status = up.Status,
        //                     OnDesk = userMasterHelper.GetAssignedApplication(up, out ErrorMessage).Count
        //                 });

        //             }
        //             model.StaffDeskList = staffDeskList;
        //         }
        //         catch (Exception ex)
        //         {
        //             logger.Error(ex.StackTrace);
        //             ViewBag.ErrorMessage = "Error Occured On Staff Desk, Please Try again Later";
        //         }

        //         return View(model);
        //     }






        public ActionResult Configurations()
        {
            var config = (from c in dbCtxt.Configurations select c).ToList();
            return View(config);


        }

        public JsonResult AddConfig(string Paramid, string Paramvalue, string ParamStatus)
        {
            var status = "";
            var configcheck = (from c in dbCtxt.Configurations where c.ParamID == Paramid select c).FirstOrDefault();
            if (configcheck == null)
            {
                var config = new Configuration();
                try
                {
                    config.ParamID = Paramid;
                    config.ParamValue = Paramvalue;
                    config.Status = ParamStatus;
                    dbCtxt.Configurations.Add(config);
                    dbCtxt.SaveChanges();
                    status = "success";
                }
                catch (Exception ex)
                {
                    status = "failed " + ex.Message;
                }
            }
            else
            {
                status = "Record already exist";
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }



        public JsonResult UpdateConfigurationRecord(string Paramid, string Paramvalue, string ParamStatus)
        {
            var status = "";
            var config = (from c in dbCtxt.Configurations where c.ParamID == Paramid select c).FirstOrDefault();
            try
            {
                config.ParamID = Paramid;
                config.ParamValue = Paramvalue;
                config.Status = ParamStatus;
                dbCtxt.SaveChanges();
                status = "success";
            }
            catch (Exception ex)
            {
                status = "failed " + ex.Message;
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }





        public ActionResult PenaltyConfiguration()
        {
            var penaltylist = (from p in dbCtxt.Penalties select p).ToList();
            return View(penaltylist);
        }

        public JsonResult AddPenalty(string penaltytype, decimal penaltyamount, int penaltycode)
        {
            string status = string.Empty;
            string message = string.Empty;
            try
            {
                var penalty = new Penalty();
                penalty.PenaltyType = penaltytype;
                penalty.PenaltyAmount = penaltyamount;
                penalty.PenaltyCode = penaltycode;
                dbCtxt.Penalties.Add(penalty);
                dbCtxt.SaveChanges();
                status = "success";
                message = "Record was successfully added";
            }
            catch (Exception ex)
            {
                status = "failed";
                message = "Unable to save record. Please try again later " + ex.Message;
            }
            return Json(new { Status = status, Message = message }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeletePenalty(int penaltyid)
        {
            string status = string.Empty;
            string message = string.Empty;
            try
            {
                var delpenalty = (from p in dbCtxt.Penalties where p.PenaltyId == penaltyid select p).FirstOrDefault();
                dbCtxt.Penalties.Remove(delpenalty);
                dbCtxt.SaveChanges();
                status = "success";
                message = "Record was successfully deleted";
            }
            catch (Exception ex)
            {
                status = "failed";
                message = "Unable to delete record. Please try again later " + ex.Message;
            }
            return Json(new { Status = status, Message = message }, JsonRequestBehavior.AllowGet);
        }






        [HttpGet]
        public ActionResult GetStaffDesk(string userid)
        {
            string ErrorMessage = "";
            List<ApplicationOnStaffDesk> staffDeskList = new List<ApplicationOnStaffDesk>();

            try
            {
                logger.Info("About To GetApplication on Staff Desk =>" + userid);
                UserMaster up = dbCtxt.UserMasters.Where(u => u.UserId == userid).FirstOrDefault();
                logger.Info("StaffName =>" + up.FirstName);

                foreach (ApplicationRequest appRequest in userMasterHelper.GetAssignedApplication(up, out ErrorMessage))
                {
                    logger.Info("Retrieved Status =>" + ErrorMessage + ", With ReferenceID =>" + appRequest.ApplicationId);

                    ApplicationOnStaffDesk appDesk = new ApplicationOnStaffDesk();
                    appDesk.ReferenceId = appRequest.ApplicationId;
                    appDesk.CompanyName = appRequest.ApplicantName;
                    appDesk.DateAdded = appRequest.AddedDate.Value.ToString("dd-MMM-yyyy HH:mm");
                    appDesk.Stage = appRequest.WorkFlowState.StateName;
                    appDesk.LicenseType = appRequest.LicenseType.Description;
                    staffDeskList.Add(appDesk);
                }

                logger.Info("Request Count =>" + staffDeskList.Count);

            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
                ErrorMessage = "Error Occured On Staff Desk, Please Try again Later";
            }

            return Json(staffDeskList, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult ScheduleHistory()
        {
            List<Appointment> appointmentsList = null;


            appointmentsList = (from a in dbCtxt.Appointments select a).ToList();

            List<eventData> eventDataList = new List<eventData>();
            foreach (Appointment ap in appointmentsList)
            {
                eventDataList.Add(new eventData()
                {
                    start = ap.AppointmentDate.Value.ToString("yyyy-MM-dd"),
                    title = ap.TypeOfAppoinment,
                    venue = ap.AppointmentVenue
                });
            }

            string result = JsonConvert.SerializeObject(eventDataList);
            ViewBag.EventList = result;

            return View(appointmentsList);
        }


        [HttpGet]
        public JsonResult getEvents()
        {
            List<eventData> eventDataList = new List<eventData>();
            logger.Info("Coming to Get Events");
            try
            {
                logger.Info("About to Set EventData");
                foreach (Appointment ap in dbCtxt.Appointments.OrderByDescending(a => a.AppointmentDate).ToList())
                {
                    eventDataList.Add(new eventData()
                    {
                        start = ap.AppointmentDate.Value.ToString("yyyy-MM-dd HH:mm"),
                        title = ap.TypeOfAppoinment,
                        venue = ap.AppointmentVenue
                    });
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.InnerException);
            }
            logger.Info("About to Return");

            return Json(eventDataList, JsonRequestBehavior.AllowGet);
        }




        [HttpGet]
        public ActionResult ViewApplication(string applicationId)
        {
            string errorMessage = string.Empty;

            decimal processFeeAmt = 0,
             statutoryFeeAmt = 0,
             Arrears = 0;
            UserMaster applicantMaster = null;
            ApplicationRequest appRequest = null;
            PaymentLog paymentLog = null;
            ActionHistory actionHistory = null;
            UserMaster currentDeskUser = null;
            ViewBag.isLegacy = false;
            ViewBag.PaymentStatus = "Payment Pending";

            logger.Info("Coming into ViewApplication with Id =>" + applicationId);

            appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
            logger.Info("Application Retrieved");
            applicantMaster = dbCtxt.UserMasters.Where(c => c.UserId.Trim() == appRequest.ApplicantUserId).FirstOrDefault();
            logger.Info("Application Applicant Retrieved");
            paymentLog = dbCtxt.PaymentLogs.Where(p => p.ApplicationId == appRequest.ApplicationId).FirstOrDefault();
            logger.Info("PaymentLog Retrieved");
            //actionHistory = dbCtxt.ActionHistories.Where(a => a.ApplicationId == appRequest.ApplicationId && (a.NextStateID == appRequest.CurrentStageID)).OrderByDescending(a => a.ActionDate).ToList().First();
            actionHistory = dbCtxt.ActionHistories.Where(a => a.ApplicationId == appRequest.ApplicationId).OrderByDescending(a => a.ActionDate).ToList().First();

            logger.Info("ActionHistory Retrieved");
            currentDeskUser = dbCtxt.UserMasters.Where(u => u.UserId == appRequest.CurrentAssignedUser).FirstOrDefault();

            if (appRequest.IsLegacy == "YES")
            {
                ViewBag.isLegacy = true;
            }


            logger.Info("About to Get Application Fees");
            //Arrears = PaymentHelper.CalculateArreas(ApplicationId, dbCtxt);                    
            errorMessage = commonHelper.GetApplicationFees(appRequest, out processFeeAmt, out statutoryFeeAmt);
            logger.Info("Response Message =>" + errorMessage);
            // carryOverAmount = commonHelper.GetCarryOverAmount(appRequest);
            // logger.Info("GetNetValueAmount Amount " + carryOverAmount);

            if (errorMessage == "SUCCESS")
            {

                if (ViewBag.isLegacy == false && paymentLog == default(PaymentLog))
                {
                    ViewBag.Arrears = Arrears;
                    ViewBag.ProcessingFee = "₦" + processFeeAmt.ToString("N");
                    ViewBag.TotalAmount = "₦" + (processFeeAmt + Arrears + statutoryFeeAmt).ToString("N");
                    ViewBag.LicenseFee = "₦" + statutoryFeeAmt.ToString("N");
                    ViewBag.ServiceCharge = "₦" + ((processFeeAmt + Arrears + statutoryFeeAmt) * (Convert.ToDecimal(0.05))).ToString("N");

                    if (commonHelper.isPaymentMade(applicationId, out errorMessage))
                    {
                        ViewBag.PaymentStatus = "Payment Received";
                        ViewBag.PaymentDate = paymentLog.LastRetryDate.Value.ToString("dd-MMM-yyyy HH:mm");
                        ViewBag.Remita = paymentLog.RRReference;
                        ViewBag.RemitaDate = paymentLog.TransactionDate.Value.ToString("dd-MMM-yyyy HH:mm");

                    }
                    if (appRequest.LicenseTypeId == "ATO")
                    {
                        var ATOapprovalfess = dbCtxt.Configurations.Where(c => c.ParamID == "ATO_APPROVAL_FEES").FirstOrDefault().ParamValue;
                        ViewBag.Approvalfee = "₦" + Convert.ToDecimal(ATOapprovalfess).ToString("N");
                    }

                }

                else if (ViewBag.isLegacy == false && paymentLog != default(PaymentLog))
                {
                    ViewBag.Arrears = Arrears;
                    if (appRequest.LicenseTypeId == "LTO" || appRequest.LicenseTypeId == "PTE" || appRequest.LicenseTypeId == "ATC" || appRequest.LicenseTypeId == "LTOLFP")
                    {
                        if (appRequest.LicenseTypeId == "LTO" || appRequest.LicenseTypeId == "LTOLFP")
                        {
                            ViewBag.ProcessingFee = "₦" + processFeeAmt.ToString("N");
                            ViewBag.TotalAmount = "₦" + (processFeeAmt + Arrears + statutoryFeeAmt).ToString("N");
                            ViewBag.LicenseFee = "₦" + statutoryFeeAmt.ToString("N");//(paymentLog.TxnAmount.Value - processFeeAmt).ToString("N");
                        }
                        else
                        {
                            ViewBag.ProcessingFee = "₦" + processFeeAmt.ToString("N");
                            ViewBag.TotalAmount = "₦" + (processFeeAmt + Arrears).ToString("N");
                        }
                    }
                    else if (appRequest.LicenseTypeId == "ATO")
                    {
                        var ATOapprovalfess= Convert.ToDecimal((dbCtxt.Configurations.Where(c => c.ParamID == "ATO_APPROVAL_FEES")).FirstOrDefault().ParamValue);
                        ViewBag.Approvalfee = "₦" + Convert.ToDecimal(ATOapprovalfess).ToString("N");
                        ViewBag.ServiceCharge = "₦" + ((processFeeAmt + Arrears + statutoryFeeAmt) * (Convert.ToDecimal(0.05))).ToString("N");
                        ViewBag.TotalAmount = "₦" + (Convert.ToDecimal(processFeeAmt + Arrears + ATOapprovalfess) * Convert.ToDecimal(1.05)).ToString("N");
                    }
                    else if (appRequest.LicenseTypeId.Contains("TPBA"))//Approval To Takeover 
                    {
                        ViewBag.LicenseFee = "₦" + statutoryFeeAmt.ToString("N");
                        ViewBag.ServiceCharge = "₦" + (Convert.ToDecimal(processFeeAmt + statutoryFeeAmt + Arrears) * Convert.ToDecimal(0.05)).ToString("N");
                        ViewBag.TotalAmount = "₦" + (Convert.ToDecimal(processFeeAmt + statutoryFeeAmt + Arrears) * Convert.ToDecimal(1.05)).ToString("N");
                        ViewBag.ProcessingFee = "₦" + (processFeeAmt).ToString("N");
                    }
                    else
                    {
                        ViewBag.ProcessingFee = "₦" + (processFeeAmt).ToString("N");
                        ViewBag.ServiceCharge = "₦" + (Convert.ToDecimal(processFeeAmt + Arrears) * Convert.ToDecimal(0.05)).ToString("N");
                        ViewBag.TotalAmount = "₦" + (Convert.ToDecimal(processFeeAmt + Arrears) * Convert.ToDecimal(1.05)).ToString("N");
                    }


                    if (commonHelper.isPaymentMade(applicationId, out errorMessage))
                    {
                        ViewBag.PaymentStatus = "Payment Received";
                        ViewBag.PaymentDate = paymentLog.LastRetryDate.Value.ToString("dd-MMM-yyyy HH:mm");
                        ViewBag.Remita = paymentLog.RRReference;
                        ViewBag.RemitaDate = paymentLog.TransactionDate.Value.ToString("dd-MMM-yyyy HH:mm");

                    }


                }


                if (ViewBag.isLegacy == true)
                {
                    ViewBag.Arrears = "0.00";
                    ViewBag.ProcessingFee = "₦" + "0.00";
                    ViewBag.TotalAmount = "₦" + "0.00";
                    ViewBag.LicenseFee = "₦" + "0.00";
                }




                ViewBag.ShowPart = false;
                ViewBag.ChecksignatureImage = (from u in dbCtxt.UserMasters where u.UserId == userMaster.UserId select u.SignatureImage).FirstOrDefault();
                ViewBag.ChecksignatureRole = (from u in dbCtxt.UserMasters where u.UserId == userMaster.UserId select u.UserRoles).FirstOrDefault();
                ViewBag.AssignedZN = (from a in dbCtxt.ActionHistories where a.ApplicationId == applicationId && a.Action == "Acceptzn" && a.CurrentStageID == 9 select a).FirstOrDefault();
                ViewBag.AssignedOpscon = (from a in dbCtxt.ActionHistories where a.ApplicationId == applicationId && a.Action == "Accept" && a.CurrentStageID == 9 select a).FirstOrDefault();
                ViewBag.CheckInspectionExist = (from a in dbCtxt.Appointments join p in dbCtxt.AppointmentReports on a.AppointmentId equals p.AppointmentId where a.ApplicationId == applicationId select a).FirstOrDefault();
                ViewBag.Status = dbCtxt.WorkFlowStates.Where(w => w.StateID == appRequest.CurrentStageID).FirstOrDefault().StateName;
                ViewBag.Description = dbCtxt.LicenseTypes.Where(l => l.LicenseTypeId == appRequest.LicenseTypeId).FirstOrDefault().Description;
                var counthist = dbCtxt.ActionHistories.Where(a => a.ApplicationId == applicationId).OrderByDescending(a => a.ActionDate).ToList().Count();
                ViewBag.ActionHistoryList = counthist >= 5 ? dbCtxt.ActionHistories.Where(a => a.ApplicationId == applicationId).OrderByDescending(a => a.ActionDate).Take(5) : dbCtxt.ActionHistories.Where(a => a.ApplicationId == applicationId).OrderByDescending(a => a.ActionDate).Take(1);
                // ViewBag.ActionHistoryList = dbCtxt.ActionHistories.Where(a => a.ApplicationId == applicationId).OrderByDescending(a => a.ActionDate).Take(5);
                ViewBag.Applicationstatus = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == applicationId select a.Status).FirstOrDefault();
                ViewBag.ElpsId = applicantMaster.ElpsId;
                ViewBag.UserID = applicantMaster.UserId;
                ViewBag.AppRequest = appRequest;


                logger.Info("About to get GetCompanyDocumentListById => " + userMaster.ElpsId);
                ElpsResponse elpsResponse = serviceIntegrator.GetCompanyDocumentListById(applicantMaster.ElpsId);
                if (elpsResponse.message != "SUCCESS")
                {
                    logger.Error(elpsResponse.message);
                    ViewBag.ErrorMessage = elpsResponse.message;
                    return View(appRequest);
                }



                logger.Info("About to Cast DocumentList");
                List<Document> ElpsDocumentList = (List<Document>)elpsResponse.value;
                logger.Info("ElpsDocument Size =>" + ElpsDocumentList.Count);






                var appcode = userMasterHelper.AppLicenseCodeType(applicationId);

                List<RequiredLicenseDocument> RequiredDocumentList = dbCtxt.RequiredLicenseDocuments.Where(c => c.ApplicationTypeId.Trim() == appRequest.ApplicationTypeId.Trim() && c.LicenseTypeId == appcode && c.Status.Equals("ACTIVE")).ToList();
                List<LegacyDocument> LegacyDocList = null;
                switch (appRequest.LicenseTypeId)
                {
                    case "ATC": LegacyDocList = (from l in dbCtxt.LegacyDocuments where l.LicenseTypeCode == "ATC" select l).ToList(); break;
                    case "LTO": LegacyDocList = (from l in dbCtxt.LegacyDocuments where l.LicenseTypeCode == "LTO" select l).ToList(); break;
                    case "PTE": LegacyDocList = (from l in dbCtxt.LegacyDocuments where l.LicenseTypeCode == "PTE" select l).ToList(); break;
                    case "SSA": LegacyDocList = (from l in dbCtxt.LegacyDocuments where l.LicenseTypeCode == "SSA" select l).ToList(); break;

                }
                //var LegacyDocList = (from l in dbCtxt.LegacyDocuments select l).ToList();
                logger.Info("DocumentApplicationType Size =>" + RequiredDocumentList.Count);

                var checksubmittedDocs = (from s in dbCtxt.SubmittedDocuments where s.ApplicationID == applicationId select s).ToList();
                List<DocumentModel> documentModelList = new List<DocumentModel>();

                if (checksubmittedDocs.Count > 0)
                {
                    foreach (var item in checksubmittedDocs)
                    {
                        documentModelList.Add(new DocumentModel()
                        {
                            Source = item.DocSource,
                            DocumentName = item.DocName
                        });
                    }


                    ViewBag.DocumentList = documentModelList;
                }
                else if (appRequest.IsLegacy == "YES")
                {
                    ViewBag.DocumentList = appDocHelper.GetLegacyDocumentsView(LegacyDocList, ElpsDocumentList, applicationId, GlobalModel.elpsUrl);
                }
                else
                {
                    ViewBag.DocumentList = appDocHelper.GetDocumentsPendingView(RequiredDocumentList, ElpsDocumentList, applicationId, GlobalModel.elpsUrl);
                }

                ViewBag.ApplicationId = appRequest.ApplicationId;
                ViewBag.LicenseDefnID = appRequest.LicenseTypeId;
                ViewBag.ApplicationHash = commonHelper.GenerateHashText(GlobalModel.appEmail + GlobalModel.appKey);
                ViewBag.Email = GlobalModel.appEmail;
                ViewBag.ElpsUrl = GlobalModel.elpsUrl;






                var LinkedLicenseId = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == applicationId where (a.LicenseTypeId == "ATM" || a.LicenseTypeId == "ATO") select a.LinkedReference).FirstOrDefault();
                string[] linkedid = LinkedLicenseId == null ? null : LinkedLicenseId.Split('-');
                ViewBag.LinkedLicenseId = linkedid == null ? null : linkedid[0];
                ViewBag.Capacity = Convert.ToDecimal(appRequest.AnnualCumuBaseOilRequirementCapacity).ToString("N") + "Ltrs";
                ViewBag.Projection = appRequest.AnnualProductionProjectionCapacity + "%";
                ViewBag.TriggeredBy = actionHistory.TriggeredBy; // + " (" + actionHistory.TriggeredByRole + ")";
                ViewBag.TriggeredDate = actionHistory.ActionDate.Value.ToString("dd-MMM-yyyy HH:mm");
                ViewBag.TriggeredField = dbCtxt.FieldLocations.Where(f => f.FieldLocationID == actionHistory.CurrentFieldLocation).FirstOrDefault().Description;
                ViewBag.CurrentDesk = appRequest.CurrentAssignedUser + " (" + currentDeskUser.FirstName + ")";
                ViewBag.LastMessage = actionHistory.MESSAGE;
                logger.Info("userMaster.UserLocation  =>" + userMaster.UserLocation);
                ViewBag.UserLocationFieldType = dbCtxt.FieldLocations.Where(f => f.FieldLocationID == userMaster.UserLocation).FirstOrDefault().FieldType;
                ViewBag.LoggedinUserRole = userMaster.UserRoles;
                ViewBag.ErrorMessage = "SUCCESS";
                logger.Info("Location FieldType  =>" + ViewBag.UserLocationFieldType);

                ViewBag.InspectorSchedule = (from a in dbCtxt.ActionHistories where a.Action == "ScheduleInspection" && (a.TriggeredByRole == "REVIEWER" || a.TriggeredByRole == "SUPERVISOR") && a.ApplicationId == applicationId select a).ToList().LastOrDefault();
                ViewBag.ApplicationDelegationadops = (from a in dbCtxt.ActionHistories where a.Action == "Delegate" && a.TriggeredByRole == "AD RBP" && a.ApplicationId == applicationId select a).ToList().LastOrDefault();
                ViewBag.ApplicationDelegationopscon = (from a in dbCtxt.ActionHistories where a.Action == "Delegate" && a.TriggeredByRole == "OPSCON" && a.ApplicationId == applicationId select a).ToList().LastOrDefault();
                ViewBag.ZopsconOpsconApplication = (from a in dbCtxt.ActionHistories where (a.TriggeredByRole == "OPSCON" || a.TriggeredByRole == "ZOPSCON") && a.ApplicationId == applicationId select a.TriggeredByRole).ToList().LastOrDefault();


                List<ExtraPayment> penaltyInfo = new List<ExtraPayment>();

                penaltyInfo = (from p in dbCtxt.ExtraPayments.AsEnumerable()
                               where p.ApplicationID == applicationId
                               select new ExtraPayment
                               {
                                   ApplicationID = p.ApplicationID,
                                   SanctionType = p.SanctionType,
                                   TxnAmount = p.TxnAmount,
                                   RRReference = p.RRReference,
                                   Status = p.Status,
                                   Description = p.Description

                               }).ToList();

                ViewBag.ExtraPayment = penaltyInfo;


                var extrapaycount = (from p in dbCtxt.ExtraPayments where p.ApplicationID == applicationId && p.Status == "Pending" select p).ToList().Count();


                ViewBag.ExtraPaymentCount = extrapaycount;


            }

            return View(appRequest);
        }







        [HttpGet]
        public JsonResult GetPenaltyAmount(string Penalty)
        {


            var datas = (from a in dbCtxt.Penalties

                         where a.PenaltyType == Penalty
                         select new
                         {
                             PenaltyFee = a.PenaltyAmount
                         });

            return Json(datas.ToList(), JsonRequestBehavior.AllowGet);

        }






        public ActionResult GetPenalty()
        {
            var Penalty = (from p in dbCtxt.Penalties select new { p.PenaltyType }).ToList();
            return Json(Penalty, JsonRequestBehavior.AllowGet);
        }







        [HttpGet]
        public ActionResult CompanyReports()
        {
            List<UserMaster> userMasterList = new List<UserMaster>();

            foreach (UserMaster m in dbCtxt.UserMasters.Where(u => u.UserType == "COMPANY").ToList())
            {
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(u => u.ApplicantUserId == m.UserId).FirstOrDefault();
                if (appRequest != default(ApplicationRequest))
                {
                    m.UserLocation = appRequest.SiteLocationAddress;
                    Appointment appointment = dbCtxt.Appointments.Where(a => a.ApplicationId == appRequest.ApplicationId).FirstOrDefault();
                    if (appointment != default(Appointment))
                    {
                        m.LastName = appointment.ContactPerson;
                    }
                    userMasterList.Add(m);
                }

            }

            return View(userMasterList);
        }



        [HttpGet]
        public ActionResult TransitionHistory(string applicationId)
        {

            List<ActionHistory> NotificationList = new List<ActionHistory>();
            try
            {
                //NotificationList = dbCtxt.ActionHistories.Where(a => a.ApplicationId == applicationId).ToList().OrderByDescending(a => a.ActionDate).ToList();
                NotificationList = dbCtxt.ActionHistories.Where(a => a.ApplicationId == applicationId).ToList();
                ViewBag.ApplicationID = applicationId;
                ViewBag.ErrorMessage = "SUCCESS";
            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error Occured when calling Transition History, Please try again Later";
            }

            return View(NotificationList);
        }


        [HttpGet]
        public ActionResult ApplicationDetails(string applicationId)
        {
            ApplicationRequest br = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId).FirstOrDefault();
            return View(br);
        }


        /*
              [HttpGet]
              public JsonResult ApplicationAcceptDecline(string applicationId, string useraction, string comment) {

               string response = string.Empty;
               ResponseWrapper responseWrapper;
               logger.Info("UserAction => " + useraction);
               logger.Info("Applications => " + applicationId);
               logger.Info("UserComment => " + comment);


               try {
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                if (appRequest ==
                 default (ApplicationRequest)) {
                 return Json(new {
                   status = "failure",
                    Message = "Application ID with Reference " + applicationId + " Cannot be retrievd from the Database"
                  },
                  JsonRequestBehavior.AllowGet);
                }

                logger.Info("Continuing with the Approval to Process Application");

                responseWrapper = workflowHelper.processAction(appRequest.ApplicationId, useraction, userMaster.UserId, (string.IsNullOrEmpty(comment)) ? "Application Documents Accepted" : comment);
                if (!responseWrapper.status) {
                 response = responseWrapper.value;
                 logger.Error(response);
                 return Json(new {
                   status = "failure",
                    Message = response
                  },
                  JsonRequestBehavior.AllowGet);
                }


                if (responseWrapper.nextStateType == "COMPLETE") {
                 GenerateDocument(appRequest.ApplicationId);
                }


               } catch (Exception ex) {
                logger.Error(ex.Message);
                return Json(new {
                  status = "failure",
                   Message = "An Exception occur during Transaction, Please try again Later"
                 },
                 JsonRequestBehavior.AllowGet);

               }

               return Json(new {
                 status = "success",
                  Message = responseWrapper.value
                },
                JsonRequestBehavior.AllowGet);

              }
            */




        [HttpGet]
        public ActionResult ApplicationMapping()
        {
            return View(dbCtxt.ApplicationRequests.Where(a => a.LicenseReference == null && a.WorkFlowState.StateType == "PROGRESS").ToList());
        }



        //[HttpGet]
        //public JsonResult TaskDelegationUsers(string ApplicationId)
        //{

        //    ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(a => a.ApplicationId == ApplicationId).FirstOrDefault();
        //    string searchRole = dbCtxt.WorkFlowNavigations.Where(w => w.CurrentStageID == appRequest.CurrentStageID).FirstOrDefault().ActionRole;


        //    var userRole = (from r in dbCtxt.UserMasters
        //                    where r.UserRoles.Contains(searchRole) && r.UserLocation == appRequest.CurrentOfficeLocation
        //                    select new
        //                    {
        //                        role = r.UserId + " (" + r.FirstName + " " + r.LastName + ")"
        //                    }).ToList();

        //    return Json(userRole, JsonRequestBehavior.AllowGet);

        //}


        [HttpGet]
        public JsonResult GetUsersToAssign(string ApplicationId)
        {

            logger.Info("About to GetUsers To Assign with ID =>" + ApplicationId);
            ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(a => a.ApplicationId == ApplicationId).FirstOrDefault();
            //string searchRole = dbCtxt.WorkFlowNavigations.Where(w => w.CurrentStageID == appRequest.CurrentStageID).FirstOrDefault().ActionRole;
            List<UserMaster> userList = userMasterHelper.GetMaintainedStaffList(userMaster);
            logger.Info("Returned Staff List =>" + userList.Count);

            var userRole = (from r in userList
                            select new
                            {
                                id = r.UserId,
                                description = r.UserId + " (" + r.FirstName + " " + r.UserRoles + ")"
                            }).ToList();

            return Json(userRole, JsonRequestBehavior.AllowGet);

        }




        public ActionResult UpdateFacilityInfo()
        {

            List<FacilityInfo> facilityInfo = new List<FacilityInfo>();

            var Updateinfo = (from a in dbCtxt.ApplicationRequests.AsEnumerable()
                              join f in dbCtxt.Facilities on a.SiteLocationAddress equals f.LocationAddress
                              where a.SiteLocationAddress == f.LocationAddress
                              select new FacilityInfo
                              {
                                  ApplicationRequestId = a.ApplicationRequestId,
                                  CompanyEmail = a.ApplicantUserId,
                                  CompanyName = a.ApplicantName,
                                  ApplicationID = a.ApplicationId,
                                  FacilityId = a.FacilityId,
                                  ElpsFacilityId = f.ElpsFacilityId,
                                  FacilityName = f.FalicityName,
                                  Description = (from l in dbCtxt.LicenseTypes where l.LicenseTypeId == a.LicenseTypeId select l.ShortName).FirstOrDefault(),//item.FirstOrDefault().a.LinkedReference,
                                  Category = a.ApplicationTypeId + ", " + a.LicenseTypeId,
                                  AnnualStorageCapacity = a.AnnualCumuBaseOilRequirementCapacity,
                                  StorageCapacity = a.StorageCapacity,
                                  AplicationCodeType = a.LicenseTypeId,
                                  AppliedDate = a.AddedDate.ToString(),
                                  LocationAddress = a.SiteLocationAddress,
                                  GPS = a.GPSCordinates,
                                  CurrentDeskEmail = a.CurrentAssignedUser,
                                  CurrentDeskId = Convert.ToInt32(a.CurrentStageID),
                                  LicenseIssuedDate = a.LicenseIssuedDate,
                                  LicenseExpiryDate = a.LicenseExpiryDate,
                                  LicenseReference = a.LicenseReference,
                                  StateCode = a.StateCode,
                                  ApplicationCategory = a.ApplicationCategory,
                                  LGACode = a.LgaCode,
                                  CurrentOfficeLocationId = a.CurrentOfficeLocation,
                                  Action = a.Status,
                                  Quarter = a.Quarter


                              }).ToList();//.AsEnumerable().GroupBy(a =>a.ApplicationID).ToList();

            facilityInfo = Updateinfo.GroupBy(x => x.ApplicationID).Select(x => x.LastOrDefault()).ToList();







            ViewBag.AllFacInformation = facilityInfo;

            ViewBag.UserLoginEmail = userMaster.UserRoles;

            return View(facilityInfo);

        }



        [HttpPost]
        public ActionResult UpdateFacilityRecord(string ApplicationId, string companyemail, string companyname, string facilityname, int Facilityid, string FacAddress, string Gps, string AnnualStoragecapacity, string issuancedate, string expirydate, int currentstageid, string currentdesk, string LicenseReference, string LGACode, string StateCode, string Appcategory, string officelocationid, int applicationrequestId, string action, int quarter, string capacitystorage, FormCollection coll)
        {
            ElpsResponse wrapper = null;
            var result = "";
            try
            {
                Facility facility = new Facility();
                var appinfo = (from a in dbCtxt.ApplicationRequests where a.ApplicationRequestId == applicationrequestId select a).FirstOrDefault();

                var checkfacility = (from f in dbCtxt.Facilities where f.FacilityId == appinfo.FacilityId select f.ElpsFacilityId).FirstOrDefault();
                var checkfacilityexist = (from f in dbCtxt.Facilities where f.FacilityId == appinfo.FacilityId select f).FirstOrDefault();

                var statename = (from s in dbCtxt.StateMasterLists where s.StateCode == appinfo.StateCode select s.StateName).FirstOrDefault();
                var ElpsId = (from u in dbCtxt.UserMasters where u.UserId == appinfo.ApplicantUserId select u.ElpsId).FirstOrDefault();


                var Fname = coll.Get("FacilityName");

                if (appinfo != null)
                {

                    checkfacilityexist.CompanyUserId = appinfo.ApplicantUserId;
                    checkfacilityexist.FalicityName = facilityname;
                    appinfo.SiteLocationAddress = FacAddress;
                    appinfo.GPSCordinates = Gps;
                    appinfo.AnnualCumuBaseOilRequirementCapacity = AnnualStoragecapacity;//total processing capacity
                    appinfo.StorageCapacity = capacitystorage;
                    appinfo.CurrentAssignedUser = currentdesk;
                    appinfo.CurrentStageID = Convert.ToInt16(currentstageid);
                    appinfo.LicenseReference = LicenseReference == null || LicenseReference ==""? null: LicenseReference;
                    appinfo.LgaCode = LGACode;
                    appinfo.StateCode = StateCode;
                    appinfo.ApplicationCategory = Appcategory;
                    appinfo.CurrentOfficeLocation = officelocationid;
                    appinfo.ApplicantUserId = companyemail;
                    appinfo.Status = action;
                    appinfo.Quarter = quarter;
                    if (expirydate != null && expirydate != "")
                    {
                        appinfo.LicenseExpiryDate = Convert.ToDateTime(expirydate).Date;
                    }
                    if (!string.IsNullOrEmpty(issuancedate))
                    {
                        appinfo.LicenseIssuedDate = Convert.ToDateTime(issuancedate).Date;
                    }


                    if (facilityname == "")
                    {
                        checkfacilityexist.FalicityName = appinfo.ApplicantName + " Facility";
                        checkfacilityexist.LocationAddress = FacAddress;
                    }
                    else
                    {
                        checkfacilityexist.FalicityName = Fname;
                        checkfacilityexist.LocationAddress = FacAddress;
                    }
                    if (checkfacilityexist == null)
                    {
                        facility.CompanyUserId = appinfo.ApplicantUserId;
                        facility.FalicityName = coll.Get("FacilityName");

                        dbCtxt.Facilities.Add(facility);
                    }

                    Facilities _facilities1 = new Facilities()//Update Elps Facility
                    {
                        Name = Fname == "" || Fname == null ? appinfo.ApplicantName + " (Facility)" : Fname,
                        CompanyId = Convert.ToInt32(ElpsId),
                        StreetAddress = FacAddress,
                        City = statename,
                        Id = Convert.ToInt32(checkfacilityexist.ElpsFacilityId),
                        FacilityType = "Lube Oil Blending Plant",
                        StateId = 1,
                        DateAdded = DateTime.Now,
                    };

                    wrapper = elpServiceHelper.UpdateFacility(_facilities1);


                    dbCtxt.SaveChanges();



                    var facilityupdate = (from f in dbCtxt.Facilities where f.FacilityId == Facilityid select f).ToList().LastOrDefault();

                    appinfo.FacilityId = facilityupdate.FacilityId;


                    if (checkfacility == null)
                    {
                        Facilities _facilities = new Facilities()
                        {
                            Name = Fname == "" || Fname == null ? appinfo.ApplicantName + " (Facility)" : Fname,
                            CompanyId = Convert.ToInt32(ElpsId),
                            StreetAddress = appinfo.SiteLocationAddress,
                            City = statename,
                            FacilityType = "Lube Oil Blending Plant",
                            StateId = 1,
                            DateAdded = DateTime.Now,
                        };

                        wrapper = serviceIntegrator.PostFacility(_facilities);

                        if (wrapper.message == "SUCCESS")
                        {
                            Facilities facilityDetail = (Facilities)wrapper.value;

                            var updateFacility = dbCtxt.Facilities.Where(x => x.FacilityId == facilityupdate.FacilityId).FirstOrDefault();
                            updateFacility.ElpsFacilityId = facilityDetail.Id;
                        }
                    }
                    dbCtxt.SaveChanges();
                    result = "success";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }








        [HttpGet]
        public ActionResult ChangePassword()
        {

            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(PasswordModel model)
        {
            string responseMessage = null;

            try
            {
                ElpsResponse elpsResponse = serviceIntegrator.ChangePassword(userMaster.UserId, model.OldPassword, model.NewPassword);
                logger.Info("Response from Elps =>" + elpsResponse.message);
                if (elpsResponse.message.Trim() != "SUCCESS")
                {
                    responseMessage = "An Error Message occured during Service Call to Elps Server, Please try again Later";
                }
                else
                {
                    if (((bool)elpsResponse.value) == true)
                    {
                        {
                            responseMessage = "success";
                            TempData["success"] = userMaster.UserId + " password was successfully changed";
                        }
                    }
                    else
                    {
                        responseMessage = "Password Cannot Change, Kindly ensure your Old Password is correct and try again";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
                responseMessage = "A General Error occured during Change Password";
            }

            return Json(new
            {
                Message = responseMessage
            },
             JsonRequestBehavior.AllowGet);
        }





        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult LogOff()
        {
            Session.Clear();
            var elpsLogOffUrl = GlobalModel.elpsUrl + "/Account/RemoteLogOff";
            var returnUrl = Request.Url.GetLeftPart(UriPartial.Authority) + Request.ApplicationPath;
            var frm = "<form action='" + elpsLogOffUrl + "' id='frmTest' method='post'>" + "<input type='hidden' name='returnUrl' value='" + returnUrl + "' />" + "<input type='hidden' name='appId' value='" + GlobalModel.appKey + "' />" + "</form>" + "<script>document.getElementById('frmTest').submit();</script>";
            return Content(frm, "text/html");
        }





        [HttpGet]
        public ActionResult ScheduleAppointment(string applicationId, string proposedDate, string proposedVenue, string principalOfficer, string appointmentType, string appointedofficers, string inspectiontype, string DelegatedStaff)
        {

            string comment = null;
            string response = string.Empty;
            ResponseWrapper responseWrapper = null;
            int done = 0;
            logger.Info("ApplicationId =>" + applicationId);
            logger.Info("ProposedDate =>" + proposedDate);
            logger.Info("ProposedVenue =>" + proposedVenue);
            logger.Info("AppointmentType =>" + appointmentType);
            logger.Info("principalOfficer =>" + principalOfficer);

            try
            {
                Appointment appointment = new Appointment();
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                Appointment appoitmt = dbCtxt.Appointments.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                appointment = appoitmt == null ? appointment : appoitmt;
                var locationid = (from a in dbCtxt.RequestFieldMappings where a.StateCode.Contains(appRequest.StateCode) select a.FieldLocationID).FirstOrDefault();
                var locationznid = (from z in dbCtxt.ZoneFieldMappings where z.FieldLocationID == locationid select z.ZoneFieldID).FirstOrDefault();

                if (appRequest ==
                 default(ApplicationRequest))
                {
                    logger.Error("Application Reference " + applicationId + " Cannot be retrievd from the System");
                    return Json(new
                    {
                        status = "failure",
                        Message = "Application Reference " + applicationId + " Cannot be retrievd from the System"
                    },
                     JsonRequestBehavior.AllowGet);
                }

                var inspectionstages = (appRequest.CurrentStageID == 12 || appRequest.CurrentStageID == 11) ? "HOI" : "FOI";

                inspectiontype = inspectiontype == null ? inspectionstages : inspectiontype;

                if ((userMaster.UserRoles != "OPSCON" && userMaster.UserRoles != "ZOPSCON" && userMaster.UserRoles != "AD RBP"))
                {
                    appointment.ApplicationId = applicationId.Trim();
                    appointment.LicenseTypeId = appRequest.LicenseTypeId;
                    appointment.ScheduledBy = userMaster.UserId;
                    appointment.ScheduledDate = DateTime.Now;
                    appointment.AppointmentVenue = proposedVenue;
                    appointment.AppointmentDate = Convert.ToDateTime(proposedDate); //Convert.ToDateTime(proposedDate, ukCulture);
                    appointment.PrincipalOfficer = principalOfficer;
                    appointment.TypeOfAppoinment = appointmentType;
                    appointment.InspectionTypeId = inspectiontype;

                    appointment.SchduleExpiryDate = Convert.ToDateTime(proposedDate).AddDays(3);
                    appointment.Status = "INIT";

                    if (appointmentType == "INSPECTION")
                    {
                        comment = "An Inspection have been Schedule on " + appointment.AppointmentDate + " at Site Location";
                    }
                    else
                    {
                        comment = "Meeting have been Schedule on " + appointment.AppointmentDate + " At " + proposedVenue;
                    }


                    if (inspectiontype == "FOI" && userMaster.UserRoles == "REVIEWER" && appRequest.CurrentStageID == 26)
                    {
                        responseWrapper = workflowHelper.processAction(dbCtxt, appRequest.ApplicationId, "ScheduleInspectionfdr", userMaster.UserId, comment, userMaster.UserLocation, "");

                    }
                    else if (inspectiontype == "FOI" && userMaster.UserRoles == "SUPERVISOR" && appRequest.CurrentStageID == 25)
                    {
                        responseWrapper = workflowHelper.processAction(dbCtxt, appRequest.ApplicationId, "ScheduleInspectionfds", userMaster.UserId, comment, userMaster.UserLocation, "");

                    }
                    else if ((inspectiontype == "HOI" || inspectiontype == "JOI") && (userMaster.UserRoles == "REVIEWER" && appRequest.CurrentStageID == 12))
                    {
                        responseWrapper = workflowHelper.processAction(dbCtxt, appRequest.ApplicationId, "ScheduleInspectionhqr", userMaster.UserId, comment, userMaster.UserLocation, "");

                    }
                    else if ((inspectiontype == "HOI" || inspectiontype == "JOI") && (userMaster.UserRoles == "SUPERVISOR" && appRequest.CurrentStageID == 11))
                    {
                        responseWrapper = workflowHelper.processAction(dbCtxt, appRequest.ApplicationId, "ScheduleInspectionhqs", userMaster.UserId, comment, userMaster.UserLocation, "");

                    }

                    if (appRequest.StateCode.Contains("ABU") || appRequest.StateCode.Contains("IMO") || appRequest.StateCode.Contains("DEL") || appRequest.StateCode.Contains("RIV") || appRequest.StateCode.Contains("BOR") || appRequest.StateCode.Contains("KAD") || appRequest.StateCode.Contains("LAG"))
                    {
                        if (inspectiontype == "FOI" && userMaster.UserRoles == "REVIEWER" && appRequest.CurrentStageID == 38)
                        {
                            responseWrapper = workflowHelper.processAction(dbCtxt, appRequest.ApplicationId, "ScheduleInspectionznr", userMaster.UserId, comment, userMaster.UserLocation, "");

                        }
                        else if (inspectiontype == "FOI" && userMaster.UserRoles == "SUPERVISOR" && appRequest.CurrentStageID == 37)
                        {
                            responseWrapper = workflowHelper.processAction(dbCtxt, appRequest.ApplicationId, "ScheduleInspectionzns", userMaster.UserId, comment, userMaster.UserLocation, "");

                        }
                    }


                    if (!responseWrapper.status)
                    {
                        response = responseWrapper.value;
                        logger.Error(response);
                        return Json(new
                        {
                            status = "failure",
                            Message = response
                        },
                         JsonRequestBehavior.AllowGet);
                    }
                    if (appoitmt == null)
                    {
                        dbCtxt.Appointments.Add(appointment);
                    }

                    done = dbCtxt.SaveChanges();

                    if (done > 0)
                    {


                        var comment1 = "Inspection have been Schedule on " + Convert.ToDateTime(proposedDate) + " and will expiry after 72 hours from the date scheduled. Please kindly login with the link below to confirm inspection.";
                        var sendemail = userMasterHelper.SendStaffEmailMessage(appRequest.ApplicantUserId, "Inspection Schedule", comment1);

                        var Scheduledoffice = (from u in dbCtxt.UserMasters join f in dbCtxt.FieldLocations on u.UserLocation equals f.FieldLocationID where u.UserLocation == userMaster.UserLocation select f.Description).FirstOrDefault();

                        var staffcomment = "You have Been Nominated by " + userMaster.UserId + " as one of the Inspection Officers to inspect " + appRequest.ApplicantName + " at " + Scheduledoffice + ". Inspection Date =>" + Convert.ToDateTime(proposedDate) + ". Inspection Venue=> " + proposedVenue;

                        string[] StaffEmail = appointedofficers.Split(',');


                        if (appointedofficers != null && appointedofficers != "")


                        {

                            foreach (var item in StaffEmail)
                            {

                                var sendemail1 = userMasterHelper.SendStaffEmailMessage(item.ToString(), "Nominated Staff for Inspection", staffcomment);

                            }

                        }
                    }

                }
                else
                {


                    if ((appRequest.StateCode.Contains("ABU") || appRequest.StateCode.Contains("IMO") || appRequest.StateCode.Contains("DEL") || appRequest.StateCode.Contains("RIV") || appRequest.StateCode.Contains("BOR") || appRequest.StateCode.Contains("KAD") || appRequest.StateCode.Contains("LAG")) && (inspectiontype == "FOI"))
                    {
                        comment = "inspection schedule have been passed to Zopscon";

                        responseWrapper = workflowHelper.processAction(dbCtxt, appRequest.ApplicationId, "Acceptzn", userMaster.UserId, comment, locationznid, "");

                    }
                    else if (inspectiontype != "FOI")
                    {
                        comment = "inspection schedule have been delegated to " + DelegatedStaff;
                        responseWrapper = workflowHelper.processAction(dbCtxt, appRequest.ApplicationId, "Delegate", userMaster.UserId, comment, userMaster.UserLocation, DelegatedStaff);
                    }
                    else
                    {
                        comment = "inspection schedule have been passed to Opscon";

                        responseWrapper = workflowHelper.processAction(dbCtxt, appRequest.ApplicationId, "Accept", userMaster.UserId, comment, locationid, "");
                    }

                    comment = responseWrapper.value;
                }



            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return Json(new
                {
                    status = "failure",
                    Message = "An Exception occur during Transaction, Please try again Later"
                },
                 JsonRequestBehavior.AllowGet);

            }

            return Json(new
            {
                status = "success",
                Message = comment
            },
             JsonRequestBehavior.AllowGet);





        }



        [HttpGet]
        public ActionResult MoveApplication(string applicationId, string assigneduser, string reason)
        {

            string comment = null;
            string response = string.Empty;
            string action = "MoveTo";

            logger.Info("ApplicationId =>" + applicationId);
            logger.Info("New Assigned User =>" + assigneduser);

            try
            {

                logger.Info("About to retrieve Application Object to Move Application =>" + applicationId);
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    logger.Error("Application Reference " + applicationId + " Cannot be retrievd from the System");
                    return Json(new
                    {
                        status = "failure",
                        Message = "Application Reference " + applicationId + " Cannot be retrievd from the System"
                    },
                     JsonRequestBehavior.AllowGet);
                }

                logger.Info("About to Work on UserActivity");
                UserActivity useractivity = dbCtxt.UserActivities.Where(a => a.UserId == assigneduser && a.CurrentStageID == appRequest.CurrentStageID && a.ValueDate == DateTime.Today.Date).FirstOrDefault();
                if (useractivity ==
                 default(UserActivity))
                {
                    useractivity = new UserActivity();
                    useractivity.UserId = assigneduser;
                    useractivity.TxnCount = 1;
                    useractivity.ValueDate = DateTime.Today.Date;
                    useractivity.CurrentStageID = appRequest.CurrentStageID;
                    dbCtxt.UserActivities.Add(useractivity);
                    dbCtxt.SaveChanges();
                    useractivity = dbCtxt.UserActivities.Where(a => a.UserId == assigneduser && a.CurrentStageID == appRequest.CurrentStageID && a.ValueDate == DateTime.Today.Date).FirstOrDefault();

                }
                else
                {
                    useractivity.TxnCount = useractivity.TxnCount + 1;
                }

                logger.Info("About to Work on UserActivityHist");
                UserActivityHist userActivityHist = new UserActivityHist();
                userActivityHist.ActivityId = useractivity.ActivityId;
                userActivityHist.UserId = assigneduser;
                userActivityHist.ApplicationId = appRequest.ApplicationId;
                userActivityHist.LicenseTypeId = appRequest.LicenseTypeId;
                userActivityHist.ActivityOn = DateTime.Today;
                userActivityHist.CurrentStageID = appRequest.CurrentStageID;
                dbCtxt.UserActivityHists.Add(userActivityHist);
                dbCtxt.SaveChanges();

                logger.Info("About to Work on ActionHistory");
                ActionHistory actionHistory = new ActionHistory();
                actionHistory.CurrentFieldLocation = appRequest.CurrentOfficeLocation;
                actionHistory.LicenseTypeId = appRequest.LicenseTypeId;
                actionHistory.ApplicationId = appRequest.ApplicationId;
                actionHistory.CurrentStageID = (short)appRequest.CurrentStageID;
                actionHistory.Action = action;
                actionHistory.ActionDate = DateTime.UtcNow;
                actionHistory.MESSAGE = "Application Moved To " + assigneduser + " By " + userMaster.UserId + " because " + reason;
                actionHistory.TriggeredBy = userMaster.UserId;
                actionHistory.TriggeredByRole = userMaster.UserRoles;
                actionHistory.TargetedTo = assigneduser;
                actionHistory.TargetedToRole = dbCtxt.UserMasters.Where(u => u.UserId == userMaster.UserId).FirstOrDefault().UserRoles;
                actionHistory.NextStateID = (short)appRequest.CurrentStageID;
                actionHistory.NextFieldLocation = appRequest.CurrentOfficeLocation;
                actionHistory.StatusMode = "INIT";
                actionHistory.ActionMode = "NORMAL";
                dbCtxt.ActionHistories.Add(actionHistory);


                logger.Info("About to update CurrentAssigned User");
                appRequest.CurrentAssignedUser = assigneduser;
                dbCtxt.SaveChanges();

                logger.Info("Application Moved Successfully to " + assigneduser);

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return Json(new
                {
                    status = "failure",
                    Message = "An Exception occur during Transaction, Please try again Later"
                },
                 JsonRequestBehavior.AllowGet);

            }

            return Json(new
            {
                status = "success",
                Message = comment
            },
             JsonRequestBehavior.AllowGet);

        }



        [HttpGet]
        public ActionResult DelegateRevalidation(string applicationId, string delegatedOfficer, string comment)
        {
            string response = string.Empty;
            ResponseWrapper responseWrapper;

            logger.Info("ApplicationId =>" + applicationId);
            logger.Info("DelegatedOfficer =>" + delegatedOfficer);

            try
            {

                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    logger.Error("Application Reference " + applicationId + " Cannot be retrievd from the System");
                    return Json(new
                    {
                        status = "failure",
                        Message = "Application Reference " + applicationId + " Cannot be retrievd from the System"
                    },
                     JsonRequestBehavior.AllowGet);
                }
                var fieldlocation = userMaster.UserLocation;
                responseWrapper = workflowHelper.processAction(dbCtxt, appRequest.ApplicationId, "Delegate", userMaster.UserId, comment, fieldlocation, delegatedOfficer);

                if (!responseWrapper.status)
                {
                    response = responseWrapper.value;
                    logger.Error(response);
                    return Json(new
                    {
                        status = "failure",
                        Message = response
                    },
                     JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return Json(new
                {
                    status = "failure",
                    Message = "An Exception occur during Delegation, Please try again Later"
                },
                 JsonRequestBehavior.AllowGet);

            }

            return Json(new
            {
                status = "success",
                Message = responseWrapper.value
            },
             JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult DelegateVerification(string applicationId, string delegatedOfficer)
        {
            string comment = "Verification Task have been Assigned To " + delegatedOfficer + " by " + userMaster.FirstName;
            string response = string.Empty;
            ResponseWrapper responseWrapper;

            logger.Info("ApplicationId =>" + applicationId);
            logger.Info("DelegatedOfficer =>" + delegatedOfficer);

            try
            {

                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    logger.Error("Application Reference " + applicationId + " Cannot be retrievd from the System");
                    return Json(new
                    {
                        status = "failure",
                        Message = "Application Reference " + applicationId + " Cannot be retrievd from the System"
                    },
                     JsonRequestBehavior.AllowGet);
                }
                var fieldlocation = userMaster.UserLocation;
                responseWrapper = workflowHelper.processAction(dbCtxt, appRequest.ApplicationId, "Accept", userMaster.UserId, comment, fieldlocation, delegatedOfficer);

                if (!responseWrapper.status)
                {
                    response = responseWrapper.value;
                    logger.Error(response);
                    return Json(new
                    {
                        status = "failure",
                        Message = response
                    },
                     JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return Json(new
                {
                    status = "failure",
                    Message = "An Exception occur during Delegation, Please try again Later"
                },
                 JsonRequestBehavior.AllowGet);

            }

            return Json(new
            {
                status = "success",
                Message = responseWrapper.value
            },
             JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetRelievedStaffOutofOffice()
        {
            ViewBag.ReliverStaffOutofOfficeList = commonHelper.GetReliverStaffOutofOffice(dbCtxt, userMaster.UserId);
            return View();
        }

        public ActionResult AllStaffOutofOffice()
        {
            ViewBag.AllStaffOutofOfficeList = commonHelper.GetAllOutofOffice(dbCtxt);
            return View();
        }

        [HttpPost]
        public ActionResult GetStaffEndOutofOffice()
        {
            string status = "";
            try
            {
                using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
                {
                    var today = DateTime.Now.Date;
                    List<OutofOffice> office = (from o in db.OutofOffices where DbFunctions.TruncateTime(o.EndDate) <= today select o).ToList();
                    foreach (var item in office)
                    {
                        item.Status = "Finished";
                    }
                    db.SaveChanges();
                    status = "done";
                }
            }
            catch (Exception ex)
            {
                status = "failed";
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetStaffStartOutofOffice()
        {
            string status = "";
            try
            {
                using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
                {
                    var today = DateTime.Now.Date;
                    List<OutofOffice> office = (from o in db.OutofOffices where DbFunctions.TruncateTime(o.StartDate) == today select o).ToList();
                    foreach (var item in office)
                    {
                        item.Status = "Started";
                    }
                    db.SaveChanges();
                    status = "done";
                }
            }
            catch (Exception ex)
            {
                status = "failed";
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EndLeave(OutofOffice office)
        {
            try
            {
                using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
                {

                    var Outoffice = (from u in db.OutofOffices where u.Relieved == office.Relieved select u).FirstOrDefault();
                    Outoffice.Status = "Finished";

                    db.SaveChanges();
                    TempData["success"] = office.Relieved + " Successfully Ended Leave";
                }
            }

            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }
            return RedirectToAction("OutOfOffice", "Admin", "");
        }

        [HttpPost]
        public ActionResult EditOutofOffice(OutofOffice usr)
        {
            try
            {
                using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
                {
                    var outofoffice = (from u in db.OutofOffices where u.Relieved == usr.Relieved select u).FirstOrDefault();
                    outofoffice.Reliever = usr.Reliever;
                    outofoffice.Relieved = usr.Relieved;
                    outofoffice.EndDate = usr.EndDate;
                    outofoffice.StartDate = usr.StartDate;
                    outofoffice.Comment = usr.Comment;
                    db.SaveChanges();
                    TempData["success"] = "Update was Successful";
                }
            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }

            return RedirectToAction("OutOfOffice", "Admin", "");
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetOutofOfficeStaff()
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var searchTxt = Request.Form.GetValues("search[value]")[0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;
            var staff = (from u in dbCtxt.OutofOffices
                         where u.Relieved == userMaster.UserId
                         select new
                         {
                             u.Reliever,
                             u.Relieved,
                             StartDate = u.StartDate.ToString(),
                             EndDate = u.EndDate.ToString(),
                             u.Comment,
                             u.Status
                         });

            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(u => u.Reliever.Contains(searchTxt) || u.Relieved.Contains(searchTxt)
               || u.EndDate.Contains(searchTxt) || u.StartDate.Contains(searchTxt) || u.Comment.Contains(searchTxt) || u.Status.Contains(searchTxt));
            }
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.Reliever + " " + sortColumnDir);
            }


            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult DeleteOutofOffice(OutofOffice office)
        {
            try
            {
                var useremail = (from u in dbCtxt.OutofOffices where u.Relieved == office.Relieved && u.Status == "Starting" select u).ToList();
                if (useremail.Count > 0)
                {
                    dbCtxt.OutofOffices.Remove(useremail.FirstOrDefault());
                    dbCtxt.SaveChanges();
                }

                TempData["success"] = "Out of office was successfully deleted";
            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message + " Unable to delete user " + office.Relieved;
            }
            return RedirectToAction("OutOfOffice");
        }

        [HttpPost]
        public ActionResult AddOutofOffice(OutofOffice usr)
        {
            try
            {
                using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
                {
                    var today = DateTime.Now;
                    var outofoffice = new OutofOffice()
                    {
                        Reliever = usr.Reliever,
                        Relieved = usr.Relieved,
                        EndDate = usr.EndDate,
                        StartDate = usr.StartDate,
                        Comment = usr.Comment
                    };
                    outofoffice.Status = usr.StartDate < today ? "Started" : "Starting";
                    db.OutofOffices.Add(outofoffice);
                    db.SaveChanges();

                    TempData["success"] = "Update was Successful";
                }
            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }

            return RedirectToAction("OutOfOffice", "Admin", "");
        }

        public ActionResult OutOfOffice()
        {
            ElpsResponse elpsResponse = serviceIntegrator.GetAllStaff();
            logger.Info("Response from Elps =>" + elpsResponse.message);
            ViewBag.UserID = userMaster.UserId;
            if (elpsResponse.message == "SUCCESS")
            {
                AllStaffs = (List<Staff>)elpsResponse.value;
            }



            return View();
        }

        //[HttpPost]
        //public ActionResult AutoConfirmPayment()
        //{
        //    string status = "";
        //    ResponseWrapper responseWrapper = null;
        //    try
        //    {
        //        List<ApplicationRequest> changecurrentstage = null;
        //        using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
        //        {
        //            List<ExtraPayment> extrapayment = (from e in db.ExtraPayments where e.Status == "Pending" && e.RRReference != null select e).ToList();
        //            List<PaymentLog> paymentlog = (from p in db.PaymentLogs where p.Status == "INIT" && p.RRReference != null select p).ToList();

        //            if (extrapayment.Count > 0)
        //            {
        //                foreach (var item in extrapayment)
        //                {

        //                    var res = serviceIntegrator.CheckRRR(item.RRReference);
        //                    var resp = JsonConvert.DeserializeObject<JObject>(res.value.ToString());
        //                    if (resp.GetValue("message").ToString() == "Approved" || resp.GetValue("status").ToString() == "01")
        //                    {
        //                        item.Status = "AUTH";
        //                    }
        //                    db.SaveChanges();
        //                    status = "done";
        //                }
        //            }
        //            if (paymentlog.Count > 0)
        //            {
        //                foreach (var item1 in paymentlog)
        //                {

        //                    var res = serviceIntegrator.CheckRRR(item1.RRReference);
        //                    var resp = JsonConvert.DeserializeObject<JObject>(res.value.ToString());
        //                    changecurrentstage = (from a in db.ApplicationRequests where a.ApplicationId == item1.ApplicationId select a).ToList();

        //                    if (resp.GetValue("message").ToString() == "Approved" || resp.GetValue("status").ToString() == "01")
        //                    {


        //                        if (changecurrentstage != null)
        //                        {
        //                            if (changecurrentstage.FirstOrDefault().Status != "Rejected" && changecurrentstage.FirstOrDefault().CurrentStageID < 5)
        //                            {
        //                                responseWrapper = workflowHelper.processAction(dbCtxt, item1.ApplicationId, "Proceed", changecurrentstage.FirstOrDefault().ApplicantUserId, "Document Submitted", changecurrentstage.FirstOrDefault().CurrentOfficeLocation, "");

        //                                responseWrapper = workflowHelper.processAction(dbCtxt, item1.ApplicationId, "GenerateRRR", changecurrentstage.FirstOrDefault().ApplicantUserId, "Remita Retrieval Reference Generated", changecurrentstage.FirstOrDefault().CurrentOfficeLocation, "");

        //                                responseWrapper = workflowHelper.processAction(dbCtxt, item1.ApplicationId, "Submit", changecurrentstage.FirstOrDefault().ApplicantUserId, "Application Reference " + item1.ApplicationId + " have been Submitted to DPR", changecurrentstage.FirstOrDefault().CurrentOfficeLocation, "");
        //                                //changecurrentstage.FirstOrDefault().Status = "Processing";
        //                                //changecurrentstage.FirstOrDefault().CurrentStageID = 5;
        //                                //userMasterHelper.AutoAssignApplication(item1.ApplicationId, "AD RBP");
        //                            }

        //                        }

        //                        item1.TxnMessage = "01 - Approved";
        //                        item1.Status = "AUTH";

        //                    }
        //                    db.SaveChanges();
        //                    status = "done";
        //                }
        //            }



        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        status = "failed";
        //    }
        //    return Json(status, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public ActionResult GiveValue(string Appid)
        {
            decimal processFeeAmt = 0, statutoryFeeAmt = 0;
            string errorMessage = "";
            string status = "";
            var appRequest = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == Appid select a).FirstOrDefault();

            /// decimal Arrears = commonHelper.CalculateArrears(Appid, userMaster.UserId, dbCtxt);
            try
            {
                errorMessage = commonHelper.GetApplicationFees(appRequest, out processFeeAmt, out statutoryFeeAmt);
                logger.Info("Response Message =>" + errorMessage);

                if (errorMessage == "SUCCESS")
                {
                    var paylog = (from l in dbCtxt.PaymentLogs where l.ApplicationId == Appid select l).FirstOrDefault();
                    var despt = dbCtxt.LicenseTypes.Where(l => l.LicenseTypeId == appRequest.LicenseTypeId).FirstOrDefault().Description;

                    if (paylog == null)
                    {
                        PaymentLog paymentLog = new PaymentLog();
                        paymentLog.ApplicationId = appRequest.ApplicationId;
                        paymentLog.TransactionDate = DateTime.UtcNow;
                        paymentLog.LastRetryDate = DateTime.UtcNow;
                        paymentLog.TransactionID = "Value Given";
                        paymentLog.LicenseTypeId = appRequest.LicenseTypeId;
                        paymentLog.ApplicantUserId = appRequest.ApplicantUserId;
                        paymentLog.TxnMessage = "Given";
                        paymentLog.Description = despt;
                        paymentLog.RRReference = "Value Given";
                        paymentLog.AppReceiptID = "Value Given";
                        paymentLog.TxnAmount = processFeeAmt + statutoryFeeAmt;
                        paymentLog.Arrears = 0;
                        paymentLog.Account = dbCtxt.Configurations.Where(c => c.ParamID == "ACCOUNT").FirstOrDefault().ParamValue.ToString();
                        paymentLog.BankCode = dbCtxt.Configurations.Where(c => c.ParamID == "BANKCODE").FirstOrDefault().ParamValue.ToString();
                        paymentLog.RetryCount = 0;
                        paymentLog.CreatedBy = userMaster.UserId;
                        paymentLog.Status = "AUTH";
                        logger.Info("About to Add Payment Log");
                        dbCtxt.PaymentLogs.Add(paymentLog);
                        dbCtxt.SaveChanges();
                        logger.Info("Added Payment Log to Table");


                        status = "success";

                        logger.Info("Saved it Successfully");
                    }
                    else
                    {
                        paylog.Status = "AUTH";
                        dbCtxt.SaveChanges();
                    }

                    if (appRequest != null)
                    {
                        appRequest.CurrentStageID = 5;
                        appRequest.Status = "Processing";
                        dbCtxt.SaveChanges();

                        var Elps = (from a in dbCtxt.ApplicationRequests
                                    join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                                    where a.ApplicationId == Appid && a.ApplicantUserId == u.UserId
                                    select new { u.ElpsId, a.Status }).FirstOrDefault();

                        serviceIntegrator.GetElpAppUpdateStatus(Appid, Elps.ElpsId, Elps.Status);

                        userMasterHelper.AutoAssignApplication(Appid, "AD RBP");
                    }

                }
            }
            catch (Exception ex) { ViewBag.message = ex.Message; }
            return Json(new { Status = status, JsonRequestBehavior.AllowGet });
        }

        [HttpGet]
        public ActionResult GiveValueList()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetGiveValue()
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var searchTxt = Request.Form.GetValues("search[value]")[0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;
            var staff = (from p in dbCtxt.ApplicationRequests
                         where p.CurrentStageID == 2 || p.CurrentStageID == 3 || p.CurrentStageID == 4
                         select new
                         {
                             p.ApplicationId,
                             p.ApplicantName,
                             p.LicenseTypeId,
                             p.ApplicationTypeId,
                             p.ApplicantUserId
                         });
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.ApplicantName.Contains(searchTxt)
               || a.LicenseTypeId.Contains(searchTxt) || a.ApplicationTypeId.Contains(searchTxt) || a.ApplicantUserId.Contains(searchTxt));
            }
            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            switch (sortColumn)
            {
                case "0":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationId).ToList() : data.OrderBy(p => p.ApplicationId).ToList();
                    break;
                case "1":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicantName).ToList() : data.OrderBy(p => p.ApplicantName).ToList();
                    break;
                case "2":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.LicenseTypeId).ToList() : data.OrderBy(p => p.LicenseTypeId).ToList();
                    break;
                case "3":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationTypeId).ToList() : data.OrderBy(p => p.ApplicationTypeId).ToList();
                    break;
                case "4":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicantUserId).ToList() : data.OrderBy(p => p.ApplicantUserId).ToList();
                    break;
            }
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult ALLPermits()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetAllPermits()
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var searchTxt = Request.Form.GetValues("search[value]")[0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;
            var location = (from l in dbCtxt.UserMasters where l.UserId == userMaster.UserId select l.UserLocation).FirstOrDefault();

            var staff = (from p in dbCtxt.ApplicationRequests
                         where p.LicenseReference != null && p.LicenseReference != "" && p.IsLegacy != "YES"
                         select new
                         {
                             p.ApplicationId,
                             p.LicenseReference,
                             p.ApplicantName,
                             p.LicenseTypeId,
                             p.LinkedReference,
                             p.ApplicationTypeId,
                             p.ApplicantUserId,
                             IssuedDate = p.LicenseIssuedDate.ToString(),
                             ExpiryDate = p.LicenseExpiryDate.ToString(),
                             AppliedDate = p.AddedDate.ToString(),
                             description = (from t in dbCtxt.LicenseTypes where t.LicenseTypeId == p.LicenseTypeId select t.ShortName).FirstOrDefault(),
                             takeoverappid = (from t in dbCtxt.TakeoverApps where t.LicenseReference == p.LicenseReference select p.LicenseTypeId).FirstOrDefault()
                         });

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }

            if (!string.IsNullOrEmpty(searchTxt))
            {
                staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.ApplicantName.Contains(searchTxt) || a.AppliedDate.Contains(searchTxt) || a.description.Contains(searchTxt)
               || a.LicenseTypeId.Contains(searchTxt) || a.ApplicationTypeId.Contains(searchTxt) || a.ApplicantUserId.Contains(searchTxt) || a.IssuedDate.Contains(searchTxt) || a.ExpiryDate.Contains(searchTxt));
            }
            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            switch (sortColumn)
            {
                case "0":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationId).ToList() : data.OrderBy(p => p.ApplicationId).ToList();
                    break;
                case "1":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicantName).ToList() : data.OrderBy(p => p.ApplicantName).ToList();
                    break;
                case "2":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.LicenseTypeId).ToList() : data.OrderBy(p => p.LicenseTypeId).ToList();
                    break;
                case "3":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationTypeId).ToList() : data.OrderBy(p => p.ApplicationTypeId).ToList();
                    break;
                case "4":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicantUserId).ToList() : data.OrderBy(p => p.ApplicantUserId).ToList();
                    break;
                case "5":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.LicenseReference).ToList() : data.OrderBy(p => p.LicenseReference).ToList();
                    break;
            }
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult ApplicationReport()
        {
            int totalapplication = (from a in dbCtxt.ApplicationRequests select a).ToList().Count();
            TempData["totalapplication"] = totalapplication;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetApplicationReport()
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var searchTxt = Request.Form.GetValues("search[value]")[0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;
            var today = DateTime.Now.Date;
            var staff = (from p in dbCtxt.ApplicationRequests

                         select new
                         {
                             p.ApplicationId,
                             p.Status,
                             category = p.ApplicationTypeId + " , " + p.LicenseTypeId,
                             p.ApplicantUserId,
                             p.ApplicantName,
                             capacity = p.StorageCapacity.ToString(),
                             issueddate = p.LicenseIssuedDate.ToString(),
                             expirydate = p.LicenseExpiryDate.ToString(),
                             expiryDATE = p.LicenseExpiryDate,
                             issuedDATE = p.LicenseIssuedDate
                         });

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchTxt))
            {
                if (searchTxt == "All Company")
                {
                    staff = staff.Where(s => s.ApplicationId == s.ApplicationId);
                }
                else
                {
                    staff = staff.Where(a => a.ApplicationId.Contains(searchTxt) || a.ApplicantUserId.Contains(searchTxt)
                   || a.Status.Contains(searchTxt) || a.capacity.Contains(searchTxt) || a.category.Contains(searchTxt) || a.ApplicantName.Contains(searchTxt)
                   || a.issueddate.Contains(searchTxt) || a.expirydate.Contains(searchTxt));
                }
            }
            string firstdate = Request.Form.Get("mymin");
            string lastdate = Request.Form.Get("mymax");
            if ((!string.IsNullOrEmpty(firstdate) && (!string.IsNullOrEmpty(lastdate))))
            {
                var mindate = Convert.ToDateTime(firstdate).Date;
                var maxdate = Convert.ToDateTime(lastdate).Date;
                staff = staff.Where(a => a.ApplicationId == a.ApplicationId && DbFunctions.TruncateTime(a.issuedDATE) >= mindate && DbFunctions.TruncateTime(a.issuedDATE) <= maxdate);
            }

            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult AllExtraPayment()
        {
            List<PaymentModel> extrapayment = new List<PaymentModel>();
            var paymt = (from p in dbCtxt.ExtraPayments
                         join a in dbCtxt.ApplicationRequests on p.ApplicationID equals a.ApplicationId
                         select new
                         {
                             p.RRReference,
                             p.Status,
                             p.TransactionDate,
                             p.TxnAmount,
                             a.ApplicantName,
                             p.ApplicationID,
                             a.ApplicantUserId,
                             p.TransactionID,
                             p.ExtraPaymentAppRef
                         }).ToList();

            foreach (var item in paymt)
            {
                extrapayment.Add(new PaymentModel()
                {

                    RRReference = item.RRReference,
                    Status = item.Status,
                    TransactionDate = item.TransactionDate,
                    TxnAmount = item.TxnAmount,
                    ApplicantName = item.ApplicantName,
                    ApplicationID = item.ApplicationID,
                    CompanyUserId = item.ApplicantUserId,
                    TransactionID = item.TransactionID,
                    ExtraApplicationID = item.ExtraPaymentAppRef
                });
            }
            ViewBag.ActiveLoginUser = userMaster.UserRoles;
            ViewBag.ExtraPaymentList = extrapayment;
            return View();
        }

        public ActionResult EditExtraPayment(string Appid, string Extrapaymentappid)
        {
            var ChangeAppRef = (from e in dbCtxt.ExtraPayments where e.ExtraPaymentAppRef == Extrapaymentappid select e).ToList();

            if (ChangeAppRef.Count() > 0)
            {
                ChangeAppRef.FirstOrDefault().ExtraPaymentAppRef = commonHelper.GenerateApplicationNo();
                dbCtxt.SaveChanges();
            }

            return RedirectToAction("AllExtraPayment");

        }

        public ActionResult DeleteExtraPayment(string Appid, string Extrapaymentappid)
        {
            var ChangeAppRef = (from e in dbCtxt.ExtraPayments where e.ExtraPaymentAppRef == Extrapaymentappid select e).FirstOrDefault();

            if (ChangeAppRef != null)
            {
                dbCtxt.ExtraPayments.Remove(ChangeAppRef);
                dbCtxt.SaveChanges();
            }

            return RedirectToAction("AllExtraPayment");

        }

        [HttpGet]
        public ActionResult AllLegacy(string location)
        {
            var locationnum = userMaster.UserLocation;

            List<ApplicationRequest> legacyapps = null;
            legacyapps = (from a in dbCtxt.ApplicationRequests where a.IsLegacy == "YES" select a).ToList();
            if (location == "Stafflocation")
            {
                legacyapps = (from a in dbCtxt.ApplicationRequests where a.IsLegacy == "YES" && a.CurrentOfficeLocation == locationnum select a).ToList();
            }
            else if (location == "Allstafflocation")
            {
                legacyapps = (from a in dbCtxt.ApplicationRequests where a.IsLegacy == "YES" select a).ToList();

            }
            return View(legacyapps);
        }

        public ActionResult AllPayment()
        {
            List<PaymentModel> payment = new List<PaymentModel>();
            var paymt = (from p in dbCtxt.PaymentLogs
                         join a in dbCtxt.ApplicationRequests on p.ApplicationId equals a.ApplicationId
                         select new
                         {
                             p.RRReference,
                             p.Status,
                             p.TransactionDate,
                             p.TxnAmount,
                             a.ApplicantName,
                             p.ApplicationId,
                             a.ApplicantUserId,
                             p.Arrears,

                             p.TransactionID
                         }).ToList();

            foreach (var item in paymt)
            {
                payment.Add(new PaymentModel()
                {

                    RRReference = item.RRReference,
                    Status = item.Status,
                    TransactionDate = item.TransactionDate,
                    TxnAmount = item.TxnAmount,
                    ApplicantName = item.ApplicantName,
                    ApplicationID = item.ApplicationId,
                    CompanyUserId = item.ApplicantUserId,
                    Arrears = item.Arrears,
                    TransactionID = item.TransactionID
                });
            }
            ViewBag.Loggedinuserrole = userMaster.UserRoles;
            ViewBag.PaymentList = payment;
            return View();
        }

        public ActionResult PenaltyList()
        {
            List<PaymentModel> payment = new List<PaymentModel>();
            var paymt = (from p in dbCtxt.ExtraPayments
                         select new
                         {
                             p.RRReference,
                             p.LicenseTypeCode,
                             p.ApplicantID,
                             p.Status,
                             p.TransactionDate,
                             p.TxnAmount,
                             p.SanctionType,
                             p.ApplicationID,
                             p.ExtraPaymentAppRef,
                             p.PenaltyCode,
                             p.ExtraPaymentID,
                             p.TransactionID
                         }).ToList();

            foreach (var item in paymt)
            {
                payment.Add(new PaymentModel()
                {

                    RRReference = item.RRReference,
                    ApplicationID = item.ApplicationID,
                    LicenseTypeCode = item.LicenseTypeCode,
                    PenaltyCode = item.PenaltyCode,
                    SanctionType = item.SanctionType,
                    ExtraPaymentID = item.ExtraPaymentID,
                    ExtraPaymentAppRef = item.ExtraPaymentAppRef,
                    ApplicantId = item.ApplicantID,
                    Status = item.Status,
                    TransactionDate = item.TransactionDate,
                    TxnAmount = item.TxnAmount,
                    TransactionID = item.TransactionID
                });
            }
            ViewBag.Loggedinuserrole = userMaster.UserRoles;
            ViewBag.PenaltyList = payment;
            return View();
        }

        [HttpPost]
        public JsonResult UpdatePenaltyUniqueCode(int Penaltycode, string Extrapaymentid)
        {
            string status = string.Empty;
            string message = string.Empty;
            try
            {
                var Record = (from e in dbCtxt.ExtraPayments where e.ExtraPaymentAppRef == Extrapaymentid select e).FirstOrDefault();
                Record.PenaltyCode = Penaltycode;
                dbCtxt.SaveChanges();

                status = "success";
                message = "Update was successful";

            }
            catch (Exception ex)
            {
                status = "failed";
                message = "Unable to update record " + ex.Message;
            }

            return Json(new { Status = status, Message = message });
        }

        [HttpGet]
        public ActionResult DetermineInspectionType(string applicationId, string delegatedOfficer, string Comment, string actionType)
        {

            string comment = "Inspection Activities to be Done => " + Comment;
            string response = string.Empty;
            ResponseWrapper responseWrapper;

            logger.Info("ApplicationId =>" + applicationId);
            logger.Info("DelegatedOfficer =>" + delegatedOfficer);
            logger.Info("actionType =>" + actionType);
            logger.Info("Comment =>" + Comment);

            try
            {

                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    logger.Error("Application Reference " + applicationId + " Cannot be retrievd from the System");
                    return Json(new
                    {
                        status = "failure",
                        Message = "Application Reference " + applicationId + " Cannot be retrievd from the System"
                    },
                     JsonRequestBehavior.AllowGet);
                }
                var fieldlocation = userMaster.UserLocation;
                responseWrapper = workflowHelper.processAction(dbCtxt, appRequest.ApplicationId, actionType, userMaster.UserId, comment, fieldlocation, delegatedOfficer);

                if (!responseWrapper.status)
                {
                    response = responseWrapper.value;
                    logger.Error(response);
                    return Json(new
                    {
                        status = "failure",
                        Message = response
                    },
                     JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return Json(new
                {
                    status = "failure",
                    Message = "An Exception occur during Delegation, Please try again Later"
                },
                 JsonRequestBehavior.AllowGet);

            }

            return Json(new
            {
                status = "success",
                Message = responseWrapper.value
            },
             JsonRequestBehavior.AllowGet);

        }

        public ActionResult DeleteDocument(string fileId, string appid, string apptype)
        {
            if (appid != null)
            {
                ViewBag.Appid = appid;
            }
            ViewBag.fileId = fileId;
            ViewBag.AppType = apptype;
            return PartialView("_DeleteDocument");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDocument(string ApplicationID, string ApplicationDocType, FormCollection coll)
        {
            var appreq = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == ApplicationID select a).FirstOrDefault();

            try
            {

                ElpsResponse elpsResponse = null;
                if (ApplicationDocType == "B")
                {
                    elpsResponse = serviceIntegrator.DeleteDocument(coll["toDelId"]);
                }
                else
                {
                    elpsResponse = serviceIntegrator.DeleteFacDocument(coll["toDelId"]);

                }

                if (elpsResponse.message != "SUCCESS")
                {
                    throw new Exception("Could not delete document...");
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
                throw new Exception("Error when Deleting Document...");
            }
            if (appreq.LicenseTypeId == "SSA" || appreq.LicenseTypeId == "PTE" || appreq.LicenseTypeId == "ATM" || appreq.LicenseTypeId == "ATO" || appreq.LicenseTypeId == "RA" || appreq.LicenseTypeId == "ATC")

            {
                return RedirectToAction("PresentationMaintenance", "Admin", new { applicationId = ApplicationID });

            }
            else if (appreq.LicenseTypeId == "LTO")
            {
                return RedirectToAction("LTOInspectionMaintenance", "Admin", new { applicationId = ApplicationID });
            }
            else { return null; }

        }

        [HttpGet]
        public ActionResult DetermineInspectionResource(string applicationId, string delegatedOfficer, string Comment)
        {

            string comment = "Inspection Activities to be Done => " + Comment;
            string response = string.Empty;
            ResponseWrapper responseWrapper;

            logger.Info("ApplicationId =>" + applicationId);
            logger.Info("DelegatedResource =>" + delegatedOfficer);
            logger.Info("Comment =>" + Comment);

            try
            {

                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    logger.Error("Application Reference " + applicationId + " Cannot be retrievd from the System");
                    return Json(new
                    {
                        status = "failure",
                        Message = "Application Reference " + applicationId + " Cannot be retrievd from the System"
                    },
                     JsonRequestBehavior.AllowGet);
                }
                var fieldlocation = userMaster.UserLocation;
                responseWrapper = workflowHelper.processAction(dbCtxt, appRequest.ApplicationId, "Accept", userMaster.UserId, comment, fieldlocation, delegatedOfficer);

                if (!responseWrapper.status)
                {
                    response = responseWrapper.value;
                    logger.Error(response);
                    return Json(new
                    {
                        status = "failure",
                        Message = response
                    },
                     JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return Json(new
                {
                    status = "failure",
                    Message = "An Exception occur during Delegation, Please try again Later"
                },
                 JsonRequestBehavior.AllowGet);

            }

            return Json(new
            {
                status = "success",
                Message = responseWrapper.value
            },
             JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult InspectionFormReport()
        {
            var Insp = commonHelper.GetInspectionReoprt(dbCtxt);
            ViewBag.InspectionInfo = Insp;
            return View();
        }

        public ActionResult RouteToInspectionForm(string ApplicationId)
        {
            string status = null;
            string message = null;

            logger.Info("About to RouteToInspectionForm with Id =>" + ApplicationId);

            ApplicationRequest applicationRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();
            if (applicationRequest ==
             default(ApplicationRequest))
            {
                status = "failure";
                message = "Application  Reference " + ApplicationId + " Cannot be retrievd from the System";
                return Json(new
                {
                    Status = status,
                    Message = message
                }, JsonRequestBehavior.AllowGet);
            }

            logger.Info("Retrieved LicenseTypeId =>" + applicationRequest.LicenseTypeId);

            if (applicationRequest.LicenseTypeId == "PTE")
            {
                return RedirectToAction("PermitToEstablish", new
                {
                    LicenseTypeId = applicationRequest.LicenseTypeId,
                    ApplicationId = ApplicationId
                });
            }
            else if (applicationRequest.LicenseTypeId == "ATC")
            {
                return RedirectToAction("ApprovalToConstruct", new
                {
                    LicenseTypeId = applicationRequest.LicenseTypeId,
                    ApplicationId = ApplicationId
                });
            }
            else if (applicationRequest.LicenseTypeId == "LTO")
            {
                return RedirectToAction("LicenseToOperate", new
                {
                    LicenseTypeId = applicationRequest.LicenseTypeId,
                    ApplicationId = ApplicationId
                });
            }

            return View();
        }

        [HttpGet]
        public ActionResult SiteSuitabilityMaintenance(string applicationId)
        {
            ViewBag.ErrorMessage = "SUCCESS";
            AppointmentReportModel apptRptModel = new AppointmentReportModel();
            List<SelectListItem> TopologyList = new List<SelectListItem>();
            List<SelectListItem> NatureofAreaList = new List<SelectListItem>();

            try
            {
                logger.Info("About to Get Site Suitability Form with Id =>" + applicationId);

                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    ViewBag.ErrorMessage = "Application Reference " + applicationId + " Cannot be retrievd from the System";
                    return View(apptRptModel);
                }

                Appointment appointment = dbCtxt.Appointments.Where(a => a.ApplicationId == applicationId).ToList().LastOrDefault();
                if (appointment ==
                 default(Appointment))
                {
                    ViewBag.ErrorMessage = "Appointment Reference " + applicationId + " Cannot be retrievd from the System";
                    return View(apptRptModel);
                }

                logger.Info("Appointment with Id =>" + appointment.AppointmentId);

                AppointmentReport appointmentReport = appointment.AppointmentReports.FirstOrDefault();
                if (appointmentReport !=
                 default(AppointmentReport))
                {
                    apptRptModel.ApplicationId = applicationId;
                    apptRptModel.LandSize = (appointmentReport.LandSize != null) ? appointmentReport.LandSize : appRequest.LandSize;
                    apptRptModel.BeaconLocations = (appointmentReport.BeaconLocations != null) ? appointmentReport.BeaconLocations : appRequest.BeaconLocations;

                    logger.Info("LandSize =>" + apptRptModel.LandSize);

                    foreach (LandTopologyLookUp r in dbCtxt.LandTopologyLookUps.ToList())
                    {
                        if (r.Code == apptRptModel.LandTopology)
                        {
                            TopologyList.Add(new SelectListItem
                            {
                                Text = r.TopologyName,
                                Value = r.Code,
                                Selected = true
                            });
                        }
                        else
                            TopologyList.Add(new SelectListItem
                            {
                                Text = r.TopologyName,
                                Value = r.Code
                            });
                    }
                    ViewBag.LandTopology = TopologyList;


                    foreach (NatureOfAreaLookUp r in dbCtxt.NatureOfAreaLookUps.ToList())
                    {
                        if (r.AreaCode == apptRptModel.NatureOfArea)
                        {
                            NatureofAreaList.Add(new SelectListItem
                            {
                                Text = r.AreaName,
                                Value = r.AreaCode,
                                Selected = true
                            });
                        }
                        else
                            NatureofAreaList.Add(new SelectListItem
                            {
                                Text = r.AreaName,
                                Value = r.AreaCode
                            });
                    }
                    ViewBag.NatureOfArea = NatureofAreaList;

                    apptRptModel.AdjoiningProperties = (appointmentReport.AdjoiningProperties != null) ? appointmentReport.AdjoiningProperties : appRequest.AdjoiningProperties;
                    apptRptModel.AccessRoadSite = (appointmentReport.AccessRoadToFromSite != null) ? appointmentReport.AccessRoadToFromSite : appRequest.AccessRoadToFromSite;
                    apptRptModel.ProposedPlantCapacity = (appointmentReport.ProposedPlantCapacity != null) ? appointmentReport.ProposedPlantCapacity : appRequest.ProdBaseOilRequirement;
                    apptRptModel.EquipmentOnSite = (appointmentReport.EquipmentOnSite != null) ? appointmentReport.EquipmentOnSite : appRequest.AnyEquipmentOnSite;
                    apptRptModel.RelationshipWithPipelineRow = (appointmentReport.PipelineRightOfWay != null) ? appointmentReport.PipelineRightOfWay : appRequest.RelationshipWithPipelineRightOfWay;
                    apptRptModel.RelationshipWithStreams = (appointmentReport.RelationshipWithStreams != null) ? appointmentReport.RelationshipWithStreams : appRequest.RelationshipWithStreams;
                    apptRptModel.RelationshipWithPhcn = (appointmentReport.RelationshipWithPHCNTensionLines != null) ? appointmentReport.RelationshipWithPHCNTensionLines : appRequest.RelationshipWithPHCNTensionLines;
                    apptRptModel.RelationshipWithRailwayLine = (appointmentReport.RelationshipWithRailwayLine != null) ? appointmentReport.RelationshipWithRailwayLine : appRequest.RelationshipWithRailwayLine;
                    apptRptModel.RelationshipWithSensitiveIns = (appointmentReport.RelationshipWithSensitiveInstitutions != null) ? appointmentReport.RelationshipWithSensitiveInstitutions : appRequest.RelationshipWithSensitiveInstitutions;
                    apptRptModel.MemberOfTeams = appointmentReport.MemberNames;
                    apptRptModel.OfficerFieldObservations = appointmentReport.OfficerObservation;
                    apptRptModel.OfficerFieldRecommendation = appointmentReport.OfficerRecomm;
                    apptRptModel.UploadedImage = appointmentReport.UploadedImage;

                }
                else
                {

                    ViewBag.NatureOfArea = new SelectList(dbCtxt.NatureOfAreaLookUps.Where(c => c.Status == "ACTIVE"), "AreaCode", "AreaName");
                    ViewBag.LandTopology = new SelectList(dbCtxt.LandTopologyLookUps.Where(c => c.Status == "ACTIVE"), "Code", "TopologyName");

                    apptRptModel.LandSize = appRequest.LandSize;
                    apptRptModel.BeaconLocations = appRequest.BeaconLocations;
                    apptRptModel.AdjoiningProperties = appRequest.AdjoiningProperties;
                    apptRptModel.AccessRoadSite = appRequest.AccessRoadToFromSite;
                    apptRptModel.ProposedPlantCapacity = appRequest.AnnualCumuBaseOilRequirementCapacity;
                    apptRptModel.EquipmentOnSite = appRequest.AnyEquipmentOnSite;
                    apptRptModel.RelationshipWithPipelineRow = appRequest.RelationshipWithPipelineRightOfWay;
                    apptRptModel.RelationshipWithStreams = appRequest.RelationshipWithStreams;
                    apptRptModel.RelationshipWithPhcn = appRequest.RelationshipWithPHCNTensionLines;
                    apptRptModel.RelationshipWithRailwayLine = appRequest.RelationshipWithRailwayLine;
                    apptRptModel.RelationshipWithSensitiveIns = appRequest.RelationshipWithSensitiveInstitutions;

                }



                ViewBag.BaseRequest = appRequest;
                logger.Info("About to get GetCompanyDocumentListById => " + userMaster.ElpsId);
                UserMaster applicantMaster = dbCtxt.UserMasters.Where(c => c.UserId.Trim() == appRequest.ApplicantUserId).FirstOrDefault();
                ElpsResponse elpsResponse = serviceIntegrator.GetCompanyDocumentListById(applicantMaster.ElpsId);
                if (elpsResponse.message != "SUCCESS")
                {
                    logger.Error(elpsResponse.message);
                    ViewBag.ErrorMessage = elpsResponse.message;
                    return View(appRequest);
                }

                logger.Info("About to Cast DocumentList");
                List<Document> ElpsDocumentList = (List<Document>)elpsResponse.value;
                logger.Info("ElpsDocument Size =>" + ElpsDocumentList.Count);

                ViewBag.ApplicationId = applicationId;
                ViewBag.ElpsId = applicantMaster.ElpsId;
                ViewBag.UserID = applicantMaster.UserId;
                ViewBag.UploadDocumentUrl = GlobalModel.elpsUrl + "/api/UploadCompanyDoc/{0}/{1}/{2}/{3}?docName={4}&uniqueid={5}";
                ViewBag.ApplicationHash = commonHelper.GenerateHashText(GlobalModel.appEmail + GlobalModel.appKey);
                ViewBag.Email = GlobalModel.appEmail;
                ViewBag.ElpsUrl = GlobalModel.elpsUrl;
                ViewBag.DocumentList = appDocHelper.GetAppointmentDocuments("INSPECTION", ElpsDocumentList, applicationId, GlobalModel.elpsUrl);



            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error in Loading Inspection Form, Please Try Again Later";
            }

            logger.Info("ErrorMessage =>" + ViewBag.ErrorMessage);

            return View(apptRptModel);
        }

        [HttpGet]
        public ActionResult LTOInspectionMaintenance(string applicationId)
        {
            ViewBag.ErrorMessage = "SUCCESS";
            AppointmentReportModel apptRptModel = new AppointmentReportModel();

            try
            {
                logger.Info("About to Get LTO Inspection Form with Id =>" + applicationId);

                ViewBag.insptby = (from i in dbCtxt.AppointmentReports join p in dbCtxt.Appointments on i.AppointmentId equals p.AppointmentId where p.ApplicationId == applicationId select i.AddedBy).FirstOrDefault();
                ViewBag.LoginEmail = userMaster.UserId;
                var checkinspectionimageupload = (from i in dbCtxt.AppointmentReports join p in dbCtxt.Appointments on i.AppointmentId equals p.AppointmentId where p.ApplicationId == applicationId select i.UploadedImage).FirstOrDefault();

                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    ViewBag.ErrorMessage = "Application Reference " + applicationId + " Cannot be retrievd from the System";
                    return View(apptRptModel);
                }

                Appointment appointment = dbCtxt.Appointments.Where(a => a.ApplicationId == applicationId).ToList().LastOrDefault();
                if (appointment ==
                 default(Appointment))
                {
                    ViewBag.ErrorMessage = "Appointment Reference " + applicationId + " Cannot be retrievd from the System";
                    return View(apptRptModel);
                }
                else
                {
                    apptRptModel.ApplicationId = applicationId;
                    apptRptModel.CompanyReps = appointment.ContactPerson;
                    apptRptModel.AppointmentDateTime = appointment.AppointmentDate.GetValueOrDefault().ToString("dd/MM/yyyy hh:mm", ukCulture);
                    ViewBag.InspectionType = appRequest.LicenseTypeId;
                }
                logger.Info("Appointment with Id =>" + appointment.AppointmentId);


                AppointmentReport appointmentReport = appointment.AppointmentReports.FirstOrDefault();
                if (appointmentReport !=
                 default(AppointmentReport))
                {
                    apptRptModel.MemberOfTeams = appointmentReport.MemberNames;
                    apptRptModel.OfficerFieldObservations = appointmentReport.OfficerObservation;
                    apptRptModel.OfficerFieldRecommendation = appointmentReport.OfficerRecomm;
                    apptRptModel.UploadedImage = appointmentReport.UploadedImage;
                }


                ViewBag.BaseRequest = appRequest;
                logger.Info("About to get GetCompanyDocumentListById => " + userMaster.ElpsId);
                UserMaster applicantMaster = dbCtxt.UserMasters.Where(c => c.UserId.Trim() == appRequest.ApplicantUserId).FirstOrDefault();
                ElpsResponse elpsResponse = serviceIntegrator.GetCompanyDocumentListById(applicantMaster.ElpsId);

                var facilityelpsid = (from f in dbCtxt.Facilities
                                      join a in dbCtxt.ApplicationRequests on f.LocationAddress equals a.SiteLocationAddress
                                      where a.ApplicationId == applicationId && a.FacilityId == f.FacilityId
                                      select f.ElpsFacilityId).FirstOrDefault();

                ViewBag.Elpsfacid = facilityelpsid;

                if (elpsResponse.message != "SUCCESS")
                {
                    logger.Error(elpsResponse.message);
                    ViewBag.ErrorMessage = elpsResponse.message;
                    return View(appRequest);
                }

                logger.Info("About to Cast DocumentList");
                List<Document> ElpsDocumentList = (List<Document>)elpsResponse.value;
                logger.Info("ElpsDocument Size =>" + ElpsDocumentList.Count);

                List<RequiredLicenseDocument> DocumentList = dbCtxt.RequiredLicenseDocuments.Where(c => c.LicenseTypeId == "INS").ToList();


                ViewBag.ApplicationId = applicationId;
                ViewBag.ElpsId = applicantMaster.ElpsId;
                ViewBag.UserID = applicantMaster.UserId;
                ViewBag.ApplicationHash = commonHelper.GenerateHashText(GlobalModel.appEmail + GlobalModel.appKey);
                ViewBag.Email = GlobalModel.appEmail;
                ViewBag.ElpsUrl = GlobalModel.elpsUrl;
                ElpsResponse facilityelpsResponse = serviceIntegrator.GetFacilityDocumentListById(facilityelpsid.ToString());
                List<FacilityDocument> ElpsFacDocumenList = (List<FacilityDocument>)facilityelpsResponse.value;
                if (checkinspectionimageupload == null)
                {
                    ViewBag.FacilityDocumentList = appDocHelper.GetAppointmentDocuments("INSPECTION", ElpsDocumentList, applicationId, GlobalModel.elpsUrl);
                }
                else
                {
                    ViewBag.FacilityDocumentList = appDocHelper.GetFacilityDocumentsPending(DocumentList, ElpsFacDocumenList, applicationId, GlobalModel.elpsUrl);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error in Loading Inspection Form, Please Try Again Later";
            }

            logger.Info("ErrorMessage =>" + ViewBag.ErrorMessage);

            return View(apptRptModel);

        }

        [HttpPost]
        public ActionResult SiteSuitabilityMaintenance(AppointmentReportModel apptRptModel, FormCollection coll)
        {
            ResponseWrapper responseWrapper = null;

            try
            {
                logger.Info("About to SAVE Site Suitability Information On Id =>" + apptRptModel.ApplicationId);
                string comment = apptRptModel.OfficerFieldRecommendation;
                Appointment appointment = dbCtxt.Appointments.Where(a => a.ApplicationId == apptRptModel.ApplicationId).ToList().LastOrDefault();
                AppointmentReport appointmentReport = appointment.AppointmentReports.FirstOrDefault();
                var source = coll.Get("txtsource");
                logger.Info("AppointmentReport =>" + appointmentReport);
                if (appointmentReport ==
                 default(AppointmentReport))
                {
                    logger.Info("About to Create New AppointmentReport");
                    appointmentReport = new AppointmentReport();
                    appointmentReport.AppointmentId = appointment.AppointmentId;
                    appointmentReport.LandSize = apptRptModel.LandSize;
                    appointmentReport.BeaconLocations = apptRptModel.BeaconLocations;
                    appointmentReport.LandTopology = apptRptModel.LandTopology;
                    appointmentReport.AdjoiningProperties = apptRptModel.AdjoiningProperties;
                    appointmentReport.AccessRoadToFromSite = apptRptModel.AccessRoadSite;
                    appointmentReport.ProposedPlantCapacity = apptRptModel.ProposedPlantCapacity;
                    appointmentReport.EquipmentOnSite = apptRptModel.EquipmentOnSite;
                    appointmentReport.PipelineRightOfWay = apptRptModel.RelationshipWithPipelineRow;
                    appointmentReport.RelationshipWithStreams = apptRptModel.RelationshipWithStreams;
                    appointmentReport.RelationshipWithPHCNTensionLines = apptRptModel.RelationshipWithPhcn;
                    appointmentReport.RelationshipWithRailwayLine = apptRptModel.RelationshipWithRailwayLine;
                    appointmentReport.RelationshipWithSensitiveInstitutions = apptRptModel.RelationshipWithSensitiveIns;
                    appointmentReport.MemberNames = apptRptModel.MemberOfTeams;
                    appointmentReport.OfficerObservation = apptRptModel.OfficerFieldObservations;
                    appointmentReport.OfficerRecomm = apptRptModel.OfficerFieldRecommendation;
                    appointmentReport.UploadedImage = source;
                    appointmentReport.AddedBy = userMaster.UserId;
                    appointmentReport.AddedDateStamp = DateTime.Now;

                    dbCtxt.AppointmentReports.Add(appointmentReport);
                    logger.Info("Done with New AppointmentReport");
                }
                else
                {
                    logger.Info("About to Update AppointmentReport");
                    appointmentReport.LandSize = apptRptModel.LandSize;
                    appointmentReport.BeaconLocations = apptRptModel.BeaconLocations;
                    appointmentReport.LandTopology = apptRptModel.LandTopology;
                    appointmentReport.AdjoiningProperties = apptRptModel.AdjoiningProperties;
                    appointmentReport.AccessRoadToFromSite = apptRptModel.AccessRoadSite;
                    appointmentReport.ProposedPlantCapacity = apptRptModel.ProposedPlantCapacity;
                    appointmentReport.EquipmentOnSite = apptRptModel.EquipmentOnSite;
                    appointmentReport.PipelineRightOfWay = apptRptModel.RelationshipWithPipelineRow;
                    appointmentReport.RelationshipWithStreams = apptRptModel.RelationshipWithStreams;
                    appointmentReport.RelationshipWithPHCNTensionLines = apptRptModel.RelationshipWithPhcn;
                    appointmentReport.RelationshipWithRailwayLine = apptRptModel.RelationshipWithRailwayLine;
                    appointmentReport.RelationshipWithSensitiveInstitutions = apptRptModel.RelationshipWithSensitiveIns;
                    appointmentReport.MemberNames = apptRptModel.MemberOfTeams;
                    appointmentReport.OfficerObservation = apptRptModel.OfficerFieldObservations;
                    appointmentReport.OfficerFieldRecomm = apptRptModel.OfficerFieldRecommendation;
                    appointmentReport.AddedBy = userMaster.UserId;
                    appointmentReport.UploadedImage = source;
                    logger.Info("Done with Update AppointmentReport");
                }
                var fieldlocation = userMaster.UserLocation;
                responseWrapper = workflowHelper.processAction(dbCtxt, appointment.ApplicationId, "Accept", userMaster.UserId, comment, fieldlocation, "");


                if (!responseWrapper.status)
                {
                    logger.Error(responseWrapper.value);
                    return Json(new
                    {
                        status = "failure",
                        Message = responseWrapper.value
                    },
                     JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                logger.Error(ex.InnerException);
                return Json(new
                {
                    status = "failure",
                    Message = "An Exception occur during Site Suitability Maintenance, Please Try Again Later"
                },
                 JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                status = "success",
                Message = responseWrapper.value
            },
             JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult PresentationMaintenance(string applicationId)
        {
            ViewBag.ErrorMessage = "SUCCESS";
            AppointmentReportModel apptRptModel = new AppointmentReportModel();

            try
            {
                logger.Info("About to Get Presentation Form with Id =>" + applicationId);

                ViewBag.insptby = (from i in dbCtxt.AppointmentReports join p in dbCtxt.Appointments on i.AppointmentId equals p.AppointmentId where p.ApplicationId == applicationId select i.AddedBy).FirstOrDefault();
                ViewBag.LoginEmail = userMaster.UserId;
                var checkinspectionimageupload = (from i in dbCtxt.AppointmentReports join p in dbCtxt.Appointments on i.AppointmentId equals p.AppointmentId where p.ApplicationId == applicationId select i.UploadedImage).FirstOrDefault();


                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    ViewBag.ErrorMessage = "Application Reference " + applicationId + " Cannot be retrievd from the System";
                    return View(apptRptModel);
                }

                Appointment appointment = dbCtxt.Appointments.Where(a => a.ApplicationId == applicationId).ToList().LastOrDefault();
                if (appointment ==
                 default(Appointment))
                {
                    ViewBag.ErrorMessage = "Appointment Reference " + applicationId + " Cannot be retrievd from the System";
                    return View(apptRptModel);
                }
                else
                {
                    apptRptModel.ApplicationId = applicationId;
                    apptRptModel.CompanyReps = appointment.ContactPerson;
                    apptRptModel.AppointmentDateTime = appointment.AppointmentDate.GetValueOrDefault().ToString("dd/MM/yyyy hh:mm", ukCulture);
                }
                logger.Info("Appointment with Id =>" + appointment.AppointmentId);


                AppointmentReport appointmentReport = appointment.AppointmentReports.FirstOrDefault();
                if (appointmentReport !=
                 default(AppointmentReport))
                {
                    apptRptModel.MemberOfTeams = appointmentReport.MemberNames;
                    apptRptModel.OfficerFieldObservations = appointmentReport.OfficerObservation;
                    apptRptModel.OfficerFieldRecommendation = appointmentReport.OfficerRecomm;
                    apptRptModel.UploadedImage = appointmentReport.UploadedImage;
                    apptRptModel.Cali_Int_TestDate = appointmentReport.Cali_Int_TestDate != null ? Convert.ToDateTime(appointmentReport.Cali_Int_TestDate).Date : appointmentReport.Cali_Int_TestDate;
                }


                ViewBag.BaseRequest = appRequest;
                logger.Info("About to get GetCompanyDocumentListById => " + userMaster.ElpsId);
                UserMaster applicantMaster = dbCtxt.UserMasters.Where(c => c.UserId.Trim() == appRequest.ApplicantUserId).FirstOrDefault();
                ElpsResponse elpsResponse = serviceIntegrator.GetCompanyDocumentListById(applicantMaster.ElpsId);
                if (elpsResponse.message != "SUCCESS")
                {
                    logger.Error(elpsResponse.message);
                    ViewBag.ErrorMessage = elpsResponse.message;
                    return View(appRequest);
                }

                logger.Info("About to Cast DocumentList");
                List<Document> ElpsDocumentList = (List<Document>)elpsResponse.value;
                logger.Info("ElpsDocument Size =>" + ElpsDocumentList.Count);



                ViewBag.BaseRequest = appRequest;
                logger.Info("About to get GetCompanyDocumentListById => " + userMaster.ElpsId);


                var facilityelpsid = (from f in dbCtxt.Facilities
                                      join a in dbCtxt.ApplicationRequests on f.LocationAddress equals a.SiteLocationAddress
                                      where a.ApplicationId == applicationId && a.FacilityId == f.FacilityId
                                      select f.ElpsFacilityId).FirstOrDefault();

                ViewBag.Elpsfacid = facilityelpsid;

                if (elpsResponse.message != "SUCCESS")
                {
                    logger.Error(elpsResponse.message);
                    ViewBag.ErrorMessage = elpsResponse.message;
                    return View(appRequest);
                }

                logger.Info("About to Cast DocumentList");
                logger.Info("ElpsDocument Size =>" + ElpsDocumentList.Count);

                List<RequiredLicenseDocument> DocumentList = dbCtxt.RequiredLicenseDocuments.Where(c => c.LicenseTypeId == "INS").ToList();


                ViewBag.ApplicationId = applicationId;
                ViewBag.ElpsId = applicantMaster.ElpsId;
                ViewBag.UserID = applicantMaster.UserId;
                //ViewBag.UploadDocumentUrl = GlobalModel.elpsUrl + "/api/UploadCompanyDoc/{0}/{1}/{2}/{3}?docName={4}&uniqueid={5}";
                ViewBag.ApplicationHash = commonHelper.GenerateHashText(GlobalModel.appEmail + GlobalModel.appKey);
                ViewBag.Email = GlobalModel.appEmail;
                ViewBag.ElpsUrl = GlobalModel.elpsUrl;
                //ViewBag.DocumentList = appDocHelper.GetAppointmentDocuments("INSPECTION", ElpsDocumentList, applicationId, GlobalModel.elpsUrl);

                ElpsResponse facilityelpsResponse = serviceIntegrator.GetFacilityDocumentListById(facilityelpsid.ToString());
                List<FacilityDocument> ElpsFacDocumenList = (List<FacilityDocument>)facilityelpsResponse.value;
                if (checkinspectionimageupload == null)
                {
                    ViewBag.FacilityDocumentList = appDocHelper.GetAppointmentDocuments("MEETING", ElpsDocumentList, applicationId, GlobalModel.elpsUrl);//appDocHelper.GetAppointmentDocuments("INSPECTION", ElpsDocumentList, applicationId, GlobalModel.elpsUrl);
                }
                else
                {
                    ViewBag.FacilityDocumentList = appDocHelper.GetFacilityDocumentsPending(DocumentList, ElpsFacDocumenList, applicationId, GlobalModel.elpsUrl);
                }


                //ViewBag.ApplicationId = applicationId;
                //ViewBag.ElpsId = applicantMaster.ElpsId;
                //ViewBag.UserID = applicantMaster.UserId;
                //ViewBag.UploadDocumentUrl = GlobalModel.elpsUrl + "/api/UploadCompanyDoc/{0}/{1}/{2}/{3}?docName={4}&uniqueid={5}";
                //ViewBag.ApplicationHash = commonHelper.GenerateHashText(GlobalModel.appEmail + GlobalModel.appKey);
                //ViewBag.Email = GlobalModel.appEmail;
                //ViewBag.ElpsUrl = GlobalModel.elpsUrl;
                //ViewBag.DocumentList = appDocHelper.GetAppointmentDocuments("MEETING", ElpsDocumentList, applicationId, GlobalModel.elpsUrl);


            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error in Loading Presentation Form, Please Try Again Later";
            }

            logger.Info("ErrorMessage =>" + ViewBag.ErrorMessage);

            return View(apptRptModel);

        }

        [HttpPost]
        public ActionResult LTOInspectionMaintenance(AppointmentReportModel apptRptModel, FormCollection coll)
        {
            ResponseWrapper responseWrapper = null;

            try
            {
                logger.Info("About to SAVE LTO Inspection  On Id =>" + apptRptModel.ApplicationId);
                string comment = apptRptModel.OfficerFieldRecommendation;
                var source = coll.Get("txtsource");
                logger.Info("Officer Recommendation =>" + comment);

                Appointment appointment = dbCtxt.Appointments.Where(a => a.ApplicationId == apptRptModel.ApplicationId).ToList().LastOrDefault();
                AppointmentReport appointmentReport = appointment.AppointmentReports.FirstOrDefault();
                var appointmentdetails = (from a in dbCtxt.Appointments join p in dbCtxt.AppointmentReports on a.AppointmentId equals p.AppointmentId where a.ApplicationId == apptRptModel.ApplicationId select a).FirstOrDefault();

                logger.Info("AppointmentReport =>" + appointmentReport);
                if (appointmentReport ==
                 default(AppointmentReport))
                {
                    logger.Info("About to Create New AppointmentReport");
                    appointmentReport = new AppointmentReport();
                    appointmentReport.AppointmentId = appointment.AppointmentId;
                    appointmentReport.MemberNames = apptRptModel.MemberOfTeams;
                    appointmentReport.OfficerObservation = apptRptModel.OfficerFieldObservations;
                    appointmentReport.OfficerFieldRecomm = apptRptModel.OfficerFieldRecommendation;
                    appointmentReport.UploadedImage = source;
                    appointmentReport.AddedBy = userMaster.UserId;
                    appointmentReport.AddedDateStamp = DateTime.Now;

                    dbCtxt.AppointmentReports.Add(appointmentReport);
                    logger.Info("Done with New AppointmentReport");
                }
                else
                {
                    logger.Info("About to Update AppointmentReport");
                    appointmentReport.MemberNames = apptRptModel.MemberOfTeams;
                    appointmentReport.OfficerObservation = apptRptModel.OfficerFieldObservations;
                    appointmentReport.OfficerFieldRecomm = apptRptModel.OfficerFieldRecommendation;
                    appointmentReport.UploadedImage = source;
                    appointmentReport.AddedBy = userMaster.UserId;
                    logger.Info("Done with Update AppointmentReport");
                }

                dbCtxt.SaveChanges();
                var fieldlocation = userMaster.UserLocation;
                var delegatedstaff = (from h in dbCtxt.ActionHistories where h.ApplicationId == appointment.ApplicationId select h.Action).ToList().LastOrDefault();

                if (appointmentdetails == null)
                {
                    responseWrapper = workflowHelper.processAction(dbCtxt, appointment.ApplicationId, "Accept", userMaster.UserId, comment, fieldlocation, "");

                    if (!responseWrapper.status)
                    {
                        logger.Error(responseWrapper.value);
                        return Json(new
                        {
                            Status = "failure",
                            Message = responseWrapper.value
                        },
                         JsonRequestBehavior.AllowGet);
                    }
                }

                else if (delegatedstaff == "Delegate")
                {
                    responseWrapper = workflowHelper.processAction(dbCtxt, appointment.ApplicationId, "Accept", userMaster.UserId, comment, fieldlocation, "");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.InnerException);
                return Json(new
                {
                    Status = "failure",
                    Message = "An Exception occur during Inspection Maintenance, Please Try Again Later"
                },
                 JsonRequestBehavior.AllowGet);
            }

            logger.Info("About to return Success");

            return Json(new
            {
                Status = "success",
                Message = "Inspection report was saved successfully"
            },
             JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PresentationMaintenance(AppointmentReportModel apptRptModel, FormCollection coll)
        {
            ResponseWrapper responseWrapper = null;
            try
            {
                logger.Info("About to SAVE Presentation On Id =>" + apptRptModel.ApplicationId);
                string comment = "Inpection Report Filled by " + userMaster.UserId;
                var source = coll.Get("txtsource");
                var testdate = coll.Get("Cali_Int_TestDate");
                int month = 0, day = 0, Yr = 0;
                if (testdate == null)
                {
                    month = Convert.ToDateTime(testdate).Month;
                    day = Convert.ToDateTime(testdate).Day;
                    Yr = Convert.ToDateTime(testdate).Year;
                }
                logger.Info("Officer Recommendation =>" + comment);

                Appointment appointment = dbCtxt.Appointments.Where(a => a.ApplicationId == apptRptModel.ApplicationId).ToList().LastOrDefault();
                AppointmentReport appointmentReport = appointment.AppointmentReports.FirstOrDefault();
                var appointmentdetails = (from a in dbCtxt.Appointments join p in dbCtxt.AppointmentReports on a.AppointmentId equals p.AppointmentId where a.ApplicationId == apptRptModel.ApplicationId select a).FirstOrDefault();
                //date = DateTime.ParseExact(testdate, "MM/dd/yyyy", CultureInfo.InvariantCulture);

                logger.Info("AppointmentReport =>" + appointmentReport);
                if (appointmentReport ==
                 default(AppointmentReport))
                {
                    logger.Info("About to Create New AppointmentReport");
                    appointmentReport = new AppointmentReport();
                    appointmentReport.AppointmentId = appointment.AppointmentId;
                    appointmentReport.MemberNames = apptRptModel.MemberOfTeams;
                    appointmentReport.OfficerObservation = apptRptModel.OfficerFieldObservations;

                    appointmentReport.Cali_Int_TestDate = testdate == null ? apptRptModel.Cali_Int_TestDate : DateTime.ParseExact(testdate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    appointmentReport.UploadedImage = source;

                    appointmentReport.AddedBy = userMaster.UserId;
                    appointmentReport.AddedDateStamp = DateTime.Now;

                    dbCtxt.AppointmentReports.Add(appointmentReport);
                    logger.Info("Done with New AppointmentReport");
                }
                else
                {
                    logger.Info("About to Update AppointmentReport");

                    appointmentReport.Cali_Int_TestDate = testdate == null ? apptRptModel.Cali_Int_TestDate : DateTime.ParseExact(testdate, testdate.Length == 22 ? "dd/MM/yyyy hh:mm:ss tt" : "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    appointmentReport.UploadedImage = source;
                    appointmentReport.AddedBy = userMaster.UserId;
                    logger.Info("Done with Update AppointmentReport");
                }

                dbCtxt.SaveChanges();
                var fieldlocation = userMaster.UserLocation;
                var delegatedstaff = (from h in dbCtxt.ActionHistories where h.ApplicationId == appointment.ApplicationId select h.Action).ToList().LastOrDefault();

                if (appointmentdetails == null)
                {
                    responseWrapper = workflowHelper.processAction(dbCtxt, appointment.ApplicationId, "Accept", userMaster.UserId, comment, fieldlocation, "");

                    if (!responseWrapper.status)
                    {
                        logger.Error(responseWrapper.value);
                        return Json(new
                        {
                            Status = "failure",
                            Message = responseWrapper.value
                        },
                         JsonRequestBehavior.AllowGet);
                    }
                }

                else if (delegatedstaff == "Delegate")
                {
                    responseWrapper = workflowHelper.processAction(dbCtxt, appointment.ApplicationId, "Accept", userMaster.UserId, comment, fieldlocation, "");
                }


            }
            catch (Exception ex)
            {
                logger.Error(ex.InnerException);
                return Json(new
                {
                    Status = "failure",
                    Message = "An Exception occur during Inspection Maintenance, Please Try Again Later"
                },
                 JsonRequestBehavior.AllowGet);
            }

            logger.Info("About to return Success");

            return Json(new
            {
                Status = "success",
                Message = "Inspection report was saved successfully"
            },
             JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult StaffDesk()
        {
            string ErrorMessage = "";
            //int onDeskCount = 0;
            StaffDeskModel model = new StaffDeskModel();
            List<StaffDesk> staffDeskList = new List<StaffDesk>();
            var userlocationid = (from u in dbCtxt.UserMasters where u.UserId == userMaster.UserId select u.UserLocation).FirstOrDefault();
            var desk = userMaster.UserRoles == "SUPPORT" || userMaster.UserRoles == "SUPERADMIN" || userMaster.UserRoles == "ICT-ADMIN" ? dbCtxt.UserMasters.Where(a => a.UserRoles != "COMPANY" && a.UserRoles != "HEADADMIN" && a.UserRoles != "SUPERADMIN" && a.UserRoles != "ZONALADMIN" && a.UserRoles != "SECRETORY HEADGAS").ToList() : dbCtxt.UserMasters.Where(a => a.UserRoles != "COMPANY" && a.UserRoles != "HEADADMIN" && a.UserRoles != "SUPERADMIN" && a.UserRoles != "ZONALADMIN" && a.UserRoles != "SECRETORY HEADGAS" && a.UserLocation == userlocationid).ToList();
            foreach (UserMaster up in desk)
            {
                //onDeskCount = userMasterHelper.GetApprovalRequest(dbCtxt, up, out ErrorMessage).Count;
                staffDeskList.Add(new StaffDesk()
                {
                    Role = up.UserRoles,
                    StaffEmail = up.UserId,
                    StaffName = up.FirstName,
                    Location = (from u in dbCtxt.FieldLocations where u.FieldLocationID == up.UserLocation select u.Description).FirstOrDefault(),
                    status = up.Status,
                    OnDesk = (from a in dbCtxt.ApplicationRequests where a.CurrentAssignedUser == up.UserId select a).ToList().Count()
                });

            }
            model.StaffDeskList = staffDeskList;
            ViewBag.ErrorMessage = ErrorMessage;
            ViewBag.UserRole = userMaster.UserRoles;
            return View(model);
        }

        public JsonResult GetAppointment()
        {

            var events = (from a in dbCtxt.Appointments
                          join ap in dbCtxt.ApplicationRequests on a.ApplicationId equals ap.ApplicationId
                          where a.ScheduledBy == userMaster.UserId || userMaster.UserRoles == "HOD" || userMaster.UserRoles == "HOOD" || userMaster.UserRoles == "ZOPSCON"
                          select new
                          {
                              appid = a.AppointmentId,
                              companyname = ap.ApplicantName,
                              contact = a.ContactPerson,
                              appdate = a.AppointmentDate
                          });
            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public JsonResult ProcessUserAction(string applicationId, string useraction, string processtype, string comment, string DocId)
        {
            Appointment appointment = null;
            string response = string.Empty;
            ResponseWrapper responseWrapper;
            AppointmentReport appointmentReport = null;

            logger.Info("Application => " + applicationId);
            logger.Info("UserAction => " + useraction);
            logger.Info("ProcessType => " + processtype);
            logger.Info("UserComment => " + comment);

            try
            {

                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();

                var missingdocexist = (from m in dbCtxt.MissingDocuments where m.ApplicationID == appRequest.ApplicationId select m).ToList();


                string[] DocID = DocId.Split(',');

                if (DocId != null && DocId != "")
                {
                    if (missingdocexist.Count() == 0)
                    {
                        foreach (var item in DocID)
                        {
                            MissingDocument md = new MissingDocument();
                            md.ApplicationID = appRequest.ApplicationId;
                            md.DocId = Convert.ToInt32(item);
                            dbCtxt.MissingDocuments.Add(md);
                        }
                    }

                    else
                    {
                        foreach (var item2 in missingdocexist)
                        {
                            dbCtxt.MissingDocuments.Remove(item2);
                        }
                        dbCtxt.SaveChanges();

                        foreach (var item3 in DocID)
                        {
                            MissingDocument md = new MissingDocument();
                            md.ApplicationID = appRequest.ApplicationId;
                            md.DocId = Convert.ToInt32(item3);
                            dbCtxt.MissingDocuments.Add(md);
                        }
                    }
                    dbCtxt.SaveChanges();//save missing documents

                }


                if (appRequest ==
                 default(ApplicationRequest))
                {
                    return Json(new
                    {
                        status = "failure",
                        Message = "Application Reference " + applicationId + " Cannot be retrieved from the System"
                    },
                     JsonRequestBehavior.AllowGet);
                }

                if (processtype.Contains("RECOMM"))
                {
                    appointment = dbCtxt.Appointments.Where(a => a.ApplicationId == applicationId).ToList().LastOrDefault();
                    appointmentReport = appointment.AppointmentReports.FirstOrDefault();
                    if (appointmentReport ==
                     default(AppointmentReport))
                    {
                        return Json(new
                        {
                            status = "failure",
                            Message = "Appointment Schedule Reports Cannot be retrievd from the System"
                        },
                         JsonRequestBehavior.AllowGet);
                    }
                }

                logger.Info("Continuing with the Approval to Process Application");

                if (string.IsNullOrEmpty(comment))
                {
                    comment = userMaster.FirstName + " (" + userMaster.UserId + ") Passed Application For the Next Stage";
                }

                var locationfield = (from f in dbCtxt.FieldLocations where f.FieldLocationID == userMaster.UserLocation select f.Description).FirstOrDefault();

                var approcom = comment == "" ? "" : "Comment => " + comment + ";";
                var approvecomment = appRequest.IsLegacy == "YES" ? approcom + " Legacy License Approved by " + userMaster.UserId + " (" + userMaster.UserRoles + ") at " + locationfield : "Application Documents Accepted by " + userMaster.UserId + " (" + userMaster.UserRoles + ") at " + locationfield;
                logger.Info("Continuing with the Approval to Process Application");


                responseWrapper = workflowHelper.processAction(dbCtxt, applicationId, useraction, userMaster.UserId, (comment == "=> " || comment == "") ? approvecomment : comment, userMaster.UserLocation, "");

                if (!responseWrapper.status)
                {
                    response = responseWrapper.value;
                    logger.Error(response);
                    return Json(new
                    {
                        status = "failure",
                        Message = response
                    },
                     JsonRequestBehavior.AllowGet);
                }


                logger.Info("About to Determine if Approval will Take  Place NextStateType =>," + responseWrapper.nextStateType + ", Legacy =>" + appRequest.IsLegacy);

                if (useraction == "Accept" && appRequest.CurrentStageID == 20 && appRequest.IsLegacy == "NO")
                {
                    logger.Info("Coming to Approve");
                    GenerateLicenseDocument(appRequest.ApplicationId);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return Json(new
                {
                    status = "failure",
                    Message = "An Exception occur during Transaction, Please try again Later"
                },
                 JsonRequestBehavior.AllowGet);

            }

            return Json(new
            {
                status = "success",
                Message = responseWrapper.value
            },
             JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult GetRecommendationDetail(string applicationId)
        {
            Appointment appointment;
            ApplicationRequest appRequest;
            AppointmentReport appointmentReport;
            UserMaster inspectorMaster = null;
            RecommendationModel recommendModel = new RecommendationModel();

            logger.Info("ApplicationId =>" + applicationId);

            try
            {
                appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    return Json(new
                    {
                        status = "failure",
                        Message = "Application Reference " + applicationId + " Cannot be retrieved from the System"
                    },
                     JsonRequestBehavior.AllowGet);
                }

                appointment = dbCtxt.Appointments.Where(a => a.ApplicationId == applicationId).ToList().LastOrDefault();
                if (appointment ==
                 default(Appointment))
                {
                    return Json(new
                    {
                        status = "failure",
                        Message = "Appointment Reference " + applicationId + " Cannot be retrieved from the System"
                    },
                     JsonRequestBehavior.AllowGet);
                }

                logger.Info("Appointment with Id =>" + appointment.AppointmentId);
                logger.Info("License Type ID =>" + appRequest.LicenseTypeId);


                appointmentReport = appointment.AppointmentReports.FirstOrDefault();
                if (appointmentReport !=
                 default(AppointmentReport))
                {
                    logger.Info("About To Get Appointment Details");

                    inspectorMaster = dbCtxt.UserMasters.Where(c => c.UserId.Trim() == appointment.PrincipalOfficer).FirstOrDefault();

                    recommendModel.PlantName = appRequest.ApplicantName;
                    recommendModel.PlantLocation = appRequest.SiteLocationAddress;
                    recommendModel.DprMemberTeams = string.IsNullOrEmpty(appointmentReport.MemberNames) ? " " : appointmentReport.MemberNames;
                    recommendModel.OfficerObservation = string.IsNullOrEmpty(appointmentReport.OfficerObservation) ? " " : appointmentReport.OfficerObservation;
                    recommendModel.PrincipalOfficer = inspectorMaster.FirstName + " (" + appointment.PrincipalOfficer + ")";
                    recommendModel.AppointmentDate = appointment.AppointmentDate.Value.ToString("dd-MMM-yyyy HH:mm");


                    logger.Info("Recommend1");
                    if (appRequest.LicenseTypeId.Contains("PTE") || appRequest.LicenseTypeId.Contains("LTO"))
                    {
                        recommendModel.OfficerFieldRecommendation = string.IsNullOrEmpty(appointmentReport.OfficerFieldRecomm) ? " " : appointmentReport.OfficerFieldRecomm;
                        recommendModel.SuperviorFieldRecommendation = string.IsNullOrEmpty(appointmentReport.SupervisorFieldRecomm) ? " " : appointmentReport.SupervisorFieldRecomm;
                        recommendModel.HodFieldRecommendation = string.IsNullOrEmpty(appointmentReport.HODFieldRecomm) ? " " : appointmentReport.HODFieldRecomm;
                        recommendModel.AdOpsFieldRecommendation = string.IsNullOrEmpty(appointmentReport.AdOPFieldRecomm) ? " " : appointmentReport.AdOPFieldRecomm;
                        recommendModel.ZopsconFieldRecommendation = string.IsNullOrEmpty(appointmentReport.ZOpsconRecomm) ? " " : appointmentReport.ZOpsconRecomm;
                    }

                    logger.Info("Recommend2");
                    if (appRequest.LicenseTypeId.Contains("ATC"))
                    {
                        recommendModel.OfficerHORecommendation = string.IsNullOrEmpty(appointmentReport.OfficerRecomm) ? " " : appointmentReport.OfficerRecomm;
                        recommendModel.SupervisorHORecommendation = string.IsNullOrEmpty(appointmentReport.SupervisorRecomm) ? "" : appointmentReport.SupervisorRecomm;
                        recommendModel.AdOpsHORecommendation = string.IsNullOrEmpty(appointmentReport.ADOpsRecomm) ? "" : appointmentReport.ADOpsRecomm;
                        recommendModel.HodHORecommendation = string.IsNullOrEmpty(appointmentReport.HODRecomm) ? "" : appointmentReport.HODRecomm;
                    }

                    logger.Info("Recommend3");
                    if (appRequest.LicenseTypeId.Contains("LTO"))
                    {
                        recommendModel.SupervisorHORecommendation = string.IsNullOrEmpty(appointmentReport.SupervisorRecomm) ? "" : appointmentReport.SupervisorRecomm;
                        recommendModel.AdOpsHORecommendation = string.IsNullOrEmpty(appointmentReport.ADOpsRecomm) ? "" : appointmentReport.ADOpsRecomm;
                        recommendModel.HodHORecommendation = string.IsNullOrEmpty(appointmentReport.HODRecomm) ? "" : appointmentReport.HODRecomm;
                    }
                }

                //appointmentReport.OfficerFieldRecomm

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return Json(new
                {
                    status = "failure",
                    Message = "An Exception occur Getting Recommendation Details, Please Try Again Later"
                },
                 JsonRequestBehavior.AllowGet);
            }

            logger.Info("Abou to return Json");
            return Json(new
            {
                status = "success",
                recommendModel
            }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult GetPTEApplicationAsJson(string applicationId)
        {
            Appointment appointment;
            ApplicationRequest appRequest;
            AppointmentReport appointmentReport;
            UserMaster inspectorMaster = null;
            AppointmentReportModel recommendModel = new AppointmentReportModel();

            logger.Info("ApplicationId =>" + applicationId);

            try
            {
                appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    return Json(new
                    {
                        status = "failure",
                        Message = "Application Reference " + applicationId + " Cannot be retrieved from the System"
                    },
                     JsonRequestBehavior.AllowGet);
                }

                appointment = dbCtxt.Appointments.Where(a => a.ApplicationId == applicationId).ToList().LastOrDefault();
                if (appointment ==
                 default(Appointment))
                {
                    return Json(new
                    {
                        status = "failure",
                        Message = "Appointment Reference " + applicationId + " Cannot be retrieved from the System"
                    },
                     JsonRequestBehavior.AllowGet);
                }

                logger.Info("Appointment with Id =>" + appointment.AppointmentId);
                logger.Info("License Type ID =>" + appRequest.LicenseTypeId);


                appointmentReport = appointment.AppointmentReports.FirstOrDefault();
                if (appointmentReport !=
                 default(AppointmentReport))
                {
                    logger.Info("About To Get Appointment Details");

                    inspectorMaster = dbCtxt.UserMasters.Where(c => c.UserId.Trim() == appointment.PrincipalOfficer).FirstOrDefault();

                    recommendModel.ApplicationId = applicationId;
                    recommendModel.LandSize = appRequest.LandSize;
                    recommendModel.ProposedPlantCapacity = appRequest.StorageCapacity;
                    recommendModel.BeaconLocations = appRequest.BeaconLocations;
                    recommendModel.AccessRoadSite = appRequest.AccessRoadToFromSite;
                    recommendModel.NatureOfArea = appRequest.NatureOfArea;
                    recommendModel.LandTopology = appRequest.LandTopology;
                    recommendModel.AdjoiningProperties = appRequest.AdjoiningProperties;
                    recommendModel.EquipmentOnSite = appRequest.AnyEquipmentOnSite;
                    recommendModel.RelationshipWithPipelineRow = appRequest.RelationshipWithPipelineRightOfWay;
                    recommendModel.RelationshipWithPhcn = appRequest.RelationshipWithPHCNTensionLines;
                    recommendModel.RelationshipWithRailwayLine = appRequest.RelationshipWithRailwayLine;
                    recommendModel.RelationshipWithStreams = appRequest.RelationshipWithStreams;
                    recommendModel.RelationshipWithSensitiveIns = appRequest.RelationshipWithSensitiveInstitutions;

                    recommendModel.MemberOfTeams = string.IsNullOrEmpty(appointmentReport.MemberNames) ? " " : appointmentReport.MemberNames;
                    recommendModel.OfficerFieldRecommendation = string.IsNullOrEmpty(appointmentReport.OfficerFieldRecomm) ? " " : appointmentReport.OfficerFieldRecomm;
                    recommendModel.OfficerFieldObservations = string.IsNullOrEmpty(appointmentReport.OfficerObservation) ? " " : appointmentReport.OfficerObservation;
                    recommendModel.SuperviorFieldRecommendation = string.IsNullOrEmpty(appointmentReport.SupervisorFieldRecomm) ? " " : appointmentReport.SupervisorFieldRecomm;
                    recommendModel.HodFieldRecommendation = string.IsNullOrEmpty(appointmentReport.HODFieldRecomm) ? " " : appointmentReport.HODFieldRecomm;
                    recommendModel.AdOpsFieldRecommendation = string.IsNullOrEmpty(appointmentReport.AdOPFieldRecomm) ? " " : appointmentReport.AdOPFieldRecomm;
                    recommendModel.ZopsconFieldRecommendation = string.IsNullOrEmpty(appointmentReport.ZOpsconRecomm) ? " " : appointmentReport.ZOpsconRecomm;

                    logger.Info("GetPTEApplicationAsJson done");
                }

                //appointmentReport.OfficerFieldRecomm

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return Json(new
                {
                    status = "failure",
                    Message = "An Exception occur Getting GetPTEApplicationAsJson Details, Please Try Again Later"
                },
                 JsonRequestBehavior.AllowGet);
            }

            logger.Info("About to return Json");
            return Json(new
            {
                status = "success",
                recommendModel
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult LicenseTypeConfig()
        {
            var licensetypelist = (from l in dbCtxt.LicenseTypes select l).ToList();
            return View(licensetypelist);

        }

        public JsonResult DeleteLicenseTypeConfig(string licenseid)
        {
            string res = string.Empty;
            try
            {
                var delete = (from l in dbCtxt.LicenseTypes where l.LicenseTypeId == licenseid select l).FirstOrDefault();
                dbCtxt.LicenseTypes.Remove(delete);
                dbCtxt.SaveChanges();
                res = "success";
            }
            catch (Exception ex)
            {
                res = ex.Message;
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddLicenseTypeConfig(string licenseid, string description, string shortname)
        {
            string res = string.Empty;
            LicenseType licenseType = new LicenseType();
            var checkexit = (from l in dbCtxt.LicenseTypes where l.LicenseTypeId == licenseid select l).FirstOrDefault();

            try
            {
                if (checkexit != null)
                {
                    res = "Record already exist";
                }
                else
                {
                    licenseType.LicenseTypeId = licenseid;
                    licenseType.Description = description;
                    licenseType.ShortName = shortname;
                    licenseType.LicenseSerial = 1;
                    dbCtxt.LicenseTypes.Add(licenseType);
                    dbCtxt.SaveChanges();
                    res = "success";
                }
            }
            catch (Exception ex)
            {
                res = ex.Message;
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PaymentChartReport()
        {
            StateChart statechart = new StateChart();
            statechart = userMasterHelper.StateChart(statechart);
            return Json(statechart, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult UpdateLicense(string applicationId)
        {
            logger.Info("About To Update License => " + applicationId);
            GenerateLicenseDocument(applicationId);

            //ApprovePermit(applicationId);
            return Json(new
            {
                status = "success"
            }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult DeleteCompany(string Email)
        {
            string status = "success";
            try
            {
                var query = (from u in dbCtxt.UserMasters where u.UserId == Email select u).FirstOrDefault();
                dbCtxt.UserMasters.Remove(query);
                dbCtxt.SaveChanges();
            }
            catch (Exception ex)
            {
                status = "failed " + ex.Message;
            }
            return Json(new { Status = status }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dbCtxt.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Helper Methods

        private List<SelectListItem> GetUsersBasedOnRoles(string userId, string branch, string roleToSearchFor)
        {
            List<SelectListItem> userListItem = new List<SelectListItem>();
            List<string> role = new List<string> { "REVIEWER", "SUPERVISOR"};
            var allusers = roleToSearchFor == "SUPERVISOR" ? (from u in dbCtxt.UserMasters join f in dbCtxt.FieldLocations on u.UserLocation equals f.FieldLocationID where role.Contains(u.UserRoles) && u.UserLocation == branch && u.Status == "ACTIVE" select new { u, f }).ToList() : 
                                                             (from u in dbCtxt.UserMasters
                                                              join f in dbCtxt.FieldLocations on u.UserLocation equals f.FieldLocationID
                                                              where u.UserRoles == roleToSearchFor && u.UserLocation == branch && u.Status == "ACTIVE"
                                                              select new { u, f }).ToList();

            //(from u in dbCtxt.UserMasters join f in dbCtxt.FieldLocations on u.UserLocation equals f.FieldLocationID where u.UserRoles == roleToSearchFor && u.UserLocation == branch && u.Status == "ACTIVE" select new { u, f }).ToList();

            logger.Info("UserID =>" + userId);
            logger.Info("Input Branch =>" + branch);
            logger.Info("SearchRole =>" + roleToSearchFor);

            foreach (var item in allusers)
            {

                string branchName = string.Empty;
                //UserMaster uu = dbCtxt.UserMasters.Where(c => c.UserId.Trim() == u.Trim() && c.Status == "ACTIVE").FirstOrDefault();


                if (allusers.Count > 0)
                {
                    logger.Info("Checking For Retrieved User =>" + item.u.UserId);

                    string dwvalue = item.u.UserId;
                    branchName = item.u.UserId + " ( " + item.f.Description + " )";
                    userListItem.Add(new SelectListItem()
                    {
                        Text = branchName,
                        Value = dwvalue
                    });


                }
                //break;
            }

            return userListItem;
        }

        private void GenerateLicenseDocument(string applicationId)
        {
            try
            {
                logger.Info("About to generate License");
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId.Trim()).FirstOrDefault();
                logger.Info("LicenseTypeCode => " + appRequest.LicenseTypeId);

                if (appRequest.LicenseTypeId == "ATC")
                {
                    licenseHelper.GenerateATCLetter(applicationId, userMaster.UserId);
                }
                else if (appRequest.LicenseTypeId == "ATCLFP")
                {
                    licenseHelper.GenerateATCLFPLetter(applicationId, userMaster.UserId);
                }
                else if (appRequest.LicenseTypeId == "LTO")
                {
                    licenseHelper.GenerateLicense(applicationId, userMaster.UserId);
                }
                else if (appRequest.LicenseTypeId == "LTOLFP")
                {
                    licenseHelper.GenerateLTOLFP(applicationId, userMaster.UserId);
                }
                else if (appRequest.LicenseTypeId == "SSA")
                {
                    licenseHelper.GenerateSuitabilityLetter(applicationId, userMaster.UserId);
                }
                else if (appRequest.LicenseTypeId == "PTE")
                {
                    licenseHelper.GeneratePTELetter(applicationId, userMaster.UserId);
                }
                else if (appRequest.LicenseTypeId == "ATO")
                {
                    licenseHelper.GenerateTakeOverRefNo(applicationId, userMaster.UserId);
                }
                else if (appRequest.LicenseTypeId == "ATM")
                {
                    licenseHelper.GenerateModificationRefNo(applicationId, userMaster.UserId);
                }
                else if (appRequest.LicenseTypeId == "TITA" || appRequest.LicenseTypeId == "TCA")
                {
                    licenseHelper.GenerateTITA_TCALetter(applicationId, userMaster.UserId);
                }
                else if (appRequest.LicenseTypeId.Contains("TPBA"))
                {
                    licenseHelper.GenerateTPBALetter(applicationId, userMaster.UserId);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.InnerException);
            }

        }

        //public ActionResult SubmittedDocuments() => View();

        //[HttpPost]
        public ActionResult SubmittedDocuments(string id)
        {
            var docs = new List<SubmittedDocument>();
            if(!string.IsNullOrEmpty(id))
                docs = dbCtxt.SubmittedDocuments.Where(x => x.ApplicationID.Equals(id)).ToList();

            return View(docs);
        }

        public ActionResult DeleteSubDocs(int id) 
        {
            var doc = dbCtxt.SubmittedDocuments.FirstOrDefault(x => x.SubmitedDocId == id);
            if(doc != null)
            {
                dbCtxt.SubmittedDocuments.Remove(doc);
                dbCtxt.SaveChanges();
            }
            var retUrl = doc != null ? $"SubmittedDocuments/{doc.ApplicationID}" : "SubmittedDocuments" ;
            return RedirectToAction(retUrl);
        }

        public static double RoundUp(double input)
        {
            return Math.Ceiling(input * 100) / 100;
            //double multiplier = Math.Pow(10, Convert.ToDouble(places));
            //return Math.Ceiling(input * multiplier) / multiplier;
        }

        #endregion



    }
}