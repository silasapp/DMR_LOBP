using LOBP.DbEntities;
using LOBP.Helper;
using LOBP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using AllowAnonymousAttribute = System.Web.Mvc.AllowAnonymousAttribute;
using HttpGetAttribute = System.Web.Mvc.HttpGetAttribute;
using HttpPostAttribute = System.Web.Mvc.HttpPostAttribute;

namespace CNGIndustrialLicense.Controllers
{
    public class InspectionController : Controller
    {
        private LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities();
        private UserHelper commonHelper = new UserHelper();
        private WorkFlowHelper workflowManager = new WorkFlowHelper();
        private ElpServiceHelper serviceIntegrator = new ElpServiceHelper();
        // GET: Inspection
        [HttpGet]
        [AllowAnonymous]
        public JsonResult GetSingleInspection(string email, string applicationid)
        {

            var Inspection = (from a in dbCtxt.ApplicationRequests
                              join appmt in dbCtxt.Appointments on a.ApplicationId equals appmt.ApplicationId
                              join p in dbCtxt.PaymentLogs on a.ApplicationId equals p.ApplicationId
                              where a.CurrentAssignedUser == email && (a.CurrentStageID == 27 || a.CurrentStageID == 28 || a.CurrentStageID == 14 || a.CurrentStageID == 15 || a.CurrentStageID == 39 || a.CurrentStageID == 40) && a.CurrentAssignedUser == appmt.ScheduledBy && a.ApplicationId == applicationid
                              select new { ApplicationId = a.ApplicationId, FacilityLocation = a.SiteLocationAddress, RegisteredAddress = a.RegisteredAddress, CompanyName = a.ApplicantName, ApplicationType = a.ApplicationTypeId, StateName = a.StateMasterList.StateName, LGA = a.LgaMasterList.LgaName}).ToList();

            if (Inspection.Count() > 0)
            {
                return Json(new { data = Inspection, message = "Success", TotalRecord = Inspection.Count() }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = Inspection, message = "No Record Found", TotalRecord = Inspection.Count() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult GetAllInspections(string email, string formtype)
        {
            var Inspection = (from a in dbCtxt.ApplicationRequests
                              join appmt in dbCtxt.Appointments on a.ApplicationId equals appmt.ApplicationId
                              join p in dbCtxt.PaymentLogs on a.ApplicationId equals p.ApplicationId
                              where (a.CurrentStageID == 27 || a.CurrentStageID == 28 || a.CurrentStageID == 14 || a.CurrentStageID == 15 || a.CurrentStageID == 39 || a.CurrentStageID == 40) && (a.LicenseTypeId == formtype && appmt.ScheduledBy == email)
                              select new { ApplicationId = a.ApplicationId, FacilityLocation = a.SiteLocationAddress, RegisteredAddress = a.RegisteredAddress, CompanyName = a.ApplicantName, ApplicationType = a.ApplicationTypeId, StateName = a.StateMasterList.StateName, LGA = a.LgaMasterList.LgaName }).ToList();


            if (Inspection.Count() > 0)
            {
                return Json(new { data = Inspection, message = "Success", TotalRecord = Inspection.Count() }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = Inspection, message = "No Record Found", TotalRecord = Inspection.Count() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult PostInspection([FromBody] AppointmentReportModel inspectionModel)
        {
            //AppointmentReportModel apptRptModel = new AppointmentReportModel();
            try
            {
                var User = (from a in dbCtxt.ApplicationRequests
                            join appmt in dbCtxt.Appointments on a.ApplicationId equals appmt.ApplicationId
                            join u in dbCtxt.UserMasters on a.CurrentAssignedUser equals u.UserId
                            where (a.CurrentStageID == 27 || a.CurrentStageID == 28 || a.CurrentStageID == 14 || a.CurrentStageID == 15 || a.CurrentStageID == 39 || a.CurrentStageID == 40) && (appmt.ScheduledBy == a.CurrentAssignedUser) && (a.LicenseTypeId == inspectionModel.LicenseType) && (a.ApplicationId == inspectionModel.ApplicationId)
                            select new { u.UserId, u.UserLocation, u.UserRoles, a.ApplicationId}).ToList();

                string fieldlocation = ""; string userRole = ""; string useremail = ""; string appid = "";
                var checkexitingrecord = (from a in dbCtxt.AppointmentReports join p in dbCtxt.Appointments on a.AppointmentId equals p.AppointmentId where p.ApplicationId == inspectionModel.ApplicationId select a).ToList();
                if (User.Count > 0)
                {
                    fieldlocation = User.FirstOrDefault().UserLocation;
                    userRole = User.FirstOrDefault().UserRoles;
                    useremail = User.FirstOrDefault().UserId;
                    appid = User.FirstOrDefault().ApplicationId;
                }
                else if(checkexitingrecord.Count > 0)
                {
                    return Json(new {message = "Record Already Exist"}, JsonRequestBehavior.AllowGet);
                }
                string licensecode = commonHelper.AppLicenseCodeType(appid);

                string comment = "";
                var apptype = (from a in dbCtxt.ApplicationRequests where a.ApplicationId== appid select a.ApplicationTypeId).FirstOrDefault();
                AppointmentReport inspectionform = (from a in dbCtxt.AppointmentReports join p in dbCtxt.Appointments on a.AppointmentId equals p.AppointmentId where p.ApplicationId == appid select a).FirstOrDefault();
                Appointment appointment = dbCtxt.Appointments.Where(a => a.ApplicationId == appid).ToList().LastOrDefault();

                if (inspectionform == null)
                {
                    if (licensecode == "LTO")
                    {
                        var appointmentRep = new AppointmentReport();

                        appointmentRep.AppointmentId = appointment.AppointmentId;
                        appointmentRep.MemberNames = inspectionModel.MemberOfTeams;
                        appointmentRep.OfficerObservation = inspectionModel.OfficerFieldObservations;
                        appointmentRep.OfficerFieldRecomm = inspectionModel.OfficerFieldRecommendation;
                        appointmentRep.AddedDateStamp = DateTime.Now;
                        appointmentRep.Latitude = inspectionModel.Latitude.Trim();
                        appointmentRep.Longitude = inspectionModel.Longitude.Trim();
                        appointmentRep.UploadedImage = inspectionModel.UploadedImage.Trim();
                        appointmentRep.AddedBy = useremail;

                        dbCtxt.AppointmentReports.Add(appointmentRep);
                    }
                    else
                    {
                        var appointmentRep = new AppointmentReport();
                        appointmentRep.AppointmentId = appointment.AppointmentId;
                        appointmentRep.AddedDateStamp = DateTime.Now;
                        appointmentRep.Latitude = inspectionModel.Latitude.Trim();
                        appointmentRep.Longitude = inspectionModel.Longitude.Trim();
                        appointmentRep.UploadedImage = inspectionModel.UploadedImage.Trim();
                        appointmentRep.AddedBy = useremail;
                    }

                    dbCtxt.SaveChanges();
                    ResponseWrapper responseWrapper = workflowManager.processAction(dbCtxt, appointment.ApplicationId, "Accept", useremail, comment, fieldlocation, "");

                }
                else
                {
                    return Json(new { message = "Record Already Exist" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { message = "Insertion was successful" }, JsonRequestBehavior.AllowGet);
        }





        [HttpGet]
        [AllowAnonymous]
        public ActionResult ChangeStateName()
        {
            // FieldLocation statename = new FieldLocation();
            try
            {
                //var states = (from a in dbCtxt.FieldLocations where a.StateLocated == "CROSSS RIVER" select a).ToList();
                //var paymentlog = (from a in dbCtxt.PaymentLogs where a.ApplicantUserId == "Asharami_Terminal@asharamisynergy.com" select a).ToList();
                //states.FirstOrDefault().StateLocated = "CROSS RIVER";

                var paymentlog = (from a in dbCtxt.PaymentLogs where a.RRReference == "Asharami_Terminal@asharamisynergy.com" select a).ToList();
                paymentlog.FirstOrDefault().ApplicantUserId = "mirabel.akpaka@asharamisynergy.com";


                dbCtxt.SaveChanges();
            }catch(Exception ex)
            {
                Content(ex.Message);
            }
            return Content("Update Successful");
        }






        [HttpGet]
        [AllowAnonymous]
        public async Task <JsonResult> PostToIgr()
        {
            //var values = new JObject();

            JObject elspResponse = new JObject();
            List<string> list = new List<string>();
            string status = "success";
            try
            {
                using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
                {
                    var extrapayment = (from e in db.ExtraPayments.AsEnumerable() where (e.Status=="AUTH" && e.RRReference != null && e.PenaltyCode != null && e.RRReference != "Value Given") && (e.LicenseTypeCode != "LTO" && e.LicenseTypeCode != "PTE" && e.LicenseTypeCode != "ATC") select new { e }).GroupBy(c => c.e.RRReference).Select(x => x.LastOrDefault()).ToList();

                    var extraids = extrapayment.Select(c => c.e.RRReference).ToList(); //!= null ? extrapayment.FirstOrDefault().e.RRReference : "";

                    var appreq = (from e in db.ApplicationRequests.AsEnumerable()
                                  join a in db.PaymentLogs on e.ApplicationId equals a.ApplicationId
                                  where ((e.LicenseTypeId == "SSA" || e.LicenseTypeId == "TITA" || e.LicenseTypeId == "TCA" || e.LicenseTypeId.Contains("TPBA") || e.LicenseTypeId == "ATO" || e.LicenseTypeId == "ATM") && (a.Status == "AUTH" && a.RRReference != null && a.RRReference != "Value Given" && (!extraids.Contains(a.RRReference))))
                                  select new { e, a }).GroupBy(c => c.a.RRReference).Select(x => x.LastOrDefault()).ToList();




                    if (appreq.Count > 0)
                    {
                        foreach (var item in appreq)
                        {
                            elspResponse = await serviceIntegrator.IGRPaymentPost(item.a.RRReference, item.e.ApplicationId);
                            list.Add(elspResponse.ToString() + "--xx--");

                        }
                    }






                    if (extrapayment.Count > 0)
                    {
                        foreach (var item in extrapayment)
                        {
                            elspResponse = await serviceIntegrator.IGRPaymentPost(item.e.RRReference, item.e.ExtraPaymentAppRef);
                            list.Add(elspResponse.ToString() + "--xx--");

                        }
                        
                    }


                }
            }
            catch (Exception ex)
            {
                //elspResponse. = elspResponse.message + ex.Message;
            }

            return Json(new { success = true, message = string.Join("<br/>,", list) }, JsonRequestBehavior.AllowGet);
        }

        }
    }
