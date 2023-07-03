using log4net;
using LOBP.DbEntities;
using LOBP.Helper;
using LOBP.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace LOBP.Controllers
{
    public class ExternalController : Controller
    {
         private LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities();
  private WorkFlowHelper workflowHelper;
  private CultureInfo ukCulture = new CultureInfo("en-GB");
  private ILog logger = log4net.LogManager.GetLogger(typeof(ExternalController));


        public ExternalController() {}
  protected override void Initialize(RequestContext requestContext) {
   try {
    base.Initialize(requestContext);
    //workflowHelper = new WorkFlowHelper(dbCtxt);
   } catch (Exception ex) {
    logger.Error(ex.InnerException);
   }
  }


        // GET: /External/GetInspectionSchedulesByEmail
  [AllowAnonymous]
  [HttpGet]
  public JsonResult GetInspectionSchedulesByEmail(string Email) {

   AllInspection allInspection = new AllInspection();
   List < InspectionSchedule > InspectionScheduleList = new List < InspectionSchedule > ();

   logger.Info("Coming to Get InspectionSchedules For Email => " + Email);


   try {

    allInspection.Status = true;
    allInspection.Message = "success";

    if (string.IsNullOrEmpty(Email)) {
     allInspection.Status = false;
     allInspection.Message = "Inspector Email  Must Be Set";
     allInspection.InspectionSchedules = InspectionScheduleList;

     return Json(allInspection, JsonRequestBehavior.AllowGet);
    } else {


     logger.Info("About to Fetch InspectionSchedules");
     foreach(Appointment ap in dbCtxt.Appointments.Where(a => a.PrincipalOfficer == Email && a.Status == "AUTH").OrderByDescending(a => a.AppointmentDate).ToList()) {

      InspectionSchedule inspectionSchedule = new InspectionSchedule();
      inspectionSchedule.ApplicationId = ap.ApplicationId;
      inspectionSchedule.AppointmentId = ap.AppointmentId.ToString();
      inspectionSchedule.Type = ap.ApplicationRequest.ApplicationTypeId;
      inspectionSchedule.LicenseTypeId = ap.LicenseTypeId;
      inspectionSchedule.Category = ap.ApplicationRequest.LicenseType.Description;

      if (ap.LicenseTypeId == "LTO") { 
      inspectionSchedule.Capacity = ap.ApplicationRequest.StorageCapacity.ToString();
      }
      else
      {
       inspectionSchedule.Capacity = ap.ApplicationRequest.AnnualCumuBaseOilRequirementCapacity.ToString();
      }


      inspectionSchedule.CompanyName = ap.ApplicationRequest.ApplicantName;
      inspectionSchedule.Location = ap.AppointmentVenue;
      inspectionSchedule.State = ap.ApplicationRequest.StateMasterList.StateName;
      inspectionSchedule.Lga = ap.ApplicationRequest.LgaMasterList.LgaName;
      inspectionSchedule.CompanyRepresentative = ap.ContactPerson;
      inspectionSchedule.SheduledDateTime = ap.AppointmentDate.Value.ToString("yyyy-MM-dd HH:mm");
      inspectionSchedule.InspectorEmail = ap.PrincipalOfficer;

      InspectionScheduleList.Add(inspectionSchedule);
     }

     logger.Info("Done with Inspection Search");

     if (InspectionScheduleList.Count == 0) {
      allInspection.Message = "No Schedules Found for User " + Email;
      logger.Info(allInspection.Message);

     }

    }

   } catch (Exception ex) {
    logger.Error(ex.InnerException);
    allInspection.Status = false;
    allInspection.Message = ex.Message;
   }



   logger.Info("About to Return from GetInspectionSchedules");
   allInspection.InspectionSchedules = InspectionScheduleList;

   return Json(allInspection, JsonRequestBehavior.AllowGet);
  }



/*
// GET: /External/GetInspectionSchedulesByEmail
  [AllowAnonymous]
  [HttpGet]
  public JsonResult GetPTEInspectionSchedulesByEmail(string Email) {

   AllInspection allInspection = new AllInspection();
   List < InspectionSchedule > InspectionScheduleList = new List < InspectionSchedule > ();

   logger.Info("Coming to Get InspectionSchedules For Email => " + Email);


   try {

    allInspection.Status = true;
    allInspection.Message = "success";

    if (string.IsNullOrEmpty(Email)) {
     allInspection.Status = false;
     allInspection.Message = "Inspector Email  Must Be Set";
     allInspection.InspectionSchedules = InspectionScheduleList;

     return Json(allInspection, JsonRequestBehavior.AllowGet);
    } else {


     logger.Info("About to Fetch InspectionSchedules");
     foreach(Appointment ap in dbCtxt.Appointments.Where(a => a.PrincipalOfficer == Email && a.Status == "AUTH" && a.LicenseTypeId == "PTE").OrderByDescending(a => a.AppointmentDate).ToList()) {

      InspectionSchedule inspectionSchedule = new InspectionSchedule();
      inspectionSchedule.ApplicationId = ap.ApplicationId;
      inspectionSchedule.AppointmentId = ap.AppointmentId.ToString();
      inspectionSchedule.Type = ap.ApplicationRequest.ApplicationTypeId;
      inspectionSchedule.Category = ap.ApplicationRequest.LicenseType.Description;
      inspectionSchedule.Capacity = ap.ApplicationRequest.StorageCapacity.ToString();
      inspectionSchedule.CompanyName = ap.ApplicationRequest.ApplicantName;
      inspectionSchedule.Location = ap.AppointmentVenue;
      inspectionSchedule.State = ap.ApplicationRequest.StateMasterList.StateName;
      inspectionSchedule.Lga = ap.ApplicationRequest.LgaMasterList.LgaName;
      inspectionSchedule.CompanyRepresentative = ap.ContactPerson;
      inspectionSchedule.SheduledDateTime = ap.AppointmentDate.Value.ToString("yyyy-MM-dd HH:mm");
      inspectionSchedule.InspectorEmail = ap.PrincipalOfficer;

      InspectionScheduleList.Add(inspectionSchedule);
     }

     logger.Info("Done with Inspection Search");

     if (InspectionScheduleList.Count == 0) {
      allInspection.Message = "No Schedules Found for User " + Email;
      logger.Info(allInspection.Message);

     }

    }

   } catch (Exception ex) {
    logger.Error(ex.InnerException);
    allInspection.Status = false;
    allInspection.Message = ex.Message;
   }



   logger.Info("About to Return from GetInspectionSchedules");
   allInspection.InspectionSchedules = InspectionScheduleList;

   return Json(allInspection, JsonRequestBehavior.AllowGet);
  }






// GET: /External/GetInspectionSchedulesByEmail
  [AllowAnonymous]
  [HttpGet]
  public JsonResult GetLTOInspectionSchedulesByEmail(string Email) {

   AllInspection allInspection = new AllInspection();
   List < InspectionSchedule > InspectionScheduleList = new List < InspectionSchedule > ();

   logger.Info("Coming to Get InspectionSchedules For Email => " + Email);


   try {

    allInspection.Status = true;
    allInspection.Message = "success";

    if (string.IsNullOrEmpty(Email)) {
     allInspection.Status = false;
     allInspection.Message = "Inspector Email  Must Be Set";
     allInspection.InspectionSchedules = InspectionScheduleList;

     return Json(allInspection, JsonRequestBehavior.AllowGet);
    } else {


     logger.Info("About to Fetch InspectionSchedules");
     foreach(Appointment ap in dbCtxt.Appointments.Where(a => a.PrincipalOfficer == Email && a.Status == "AUTH" && a.LicenseTypeId == "LTO").OrderByDescending(a => a.AppointmentDate).ToList()) {

      InspectionSchedule inspectionSchedule = new InspectionSchedule();
      inspectionSchedule.ApplicationId = ap.ApplicationId;
      inspectionSchedule.AppointmentId = ap.AppointmentId.ToString();
      inspectionSchedule.Type = ap.ApplicationRequest.ApplicationTypeId;
      inspectionSchedule.Category = ap.ApplicationRequest.LicenseType.Description;
      inspectionSchedule.Capacity = ap.ApplicationRequest.StorageCapacity.ToString();
      inspectionSchedule.CompanyName = ap.ApplicationRequest.ApplicantName;
      inspectionSchedule.Location = ap.AppointmentVenue;
      inspectionSchedule.State = ap.ApplicationRequest.StateMasterList.StateName;
      inspectionSchedule.Lga = ap.ApplicationRequest.LgaMasterList.LgaName;
      inspectionSchedule.CompanyRepresentative = ap.ContactPerson;
      inspectionSchedule.SheduledDateTime = ap.AppointmentDate.Value.ToString("yyyy-MM-dd HH:mm");
      inspectionSchedule.InspectorEmail = ap.PrincipalOfficer;

      InspectionScheduleList.Add(inspectionSchedule);
     }

     logger.Info("Done with Inspection Search");

     if (InspectionScheduleList.Count == 0) {
      allInspection.Message = "No Schedules Found for User " + Email;
      logger.Info(allInspection.Message);

     }

    }

   } catch (Exception ex) {
    logger.Error(ex.InnerException);
    allInspection.Status = false;
    allInspection.Message = ex.Message;
   }



   logger.Info("About to Return from GetInspectionSchedules");
   allInspection.InspectionSchedules = InspectionScheduleList;

   return Json(allInspection, JsonRequestBehavior.AllowGet);
  }

*/




  // GET: /External/GetInspectionForm
  [HttpGet]
  public JsonResult GetPTEInspectionDetail(string applicationId) {

   ApplicationRequest appRequest =
    default (ApplicationRequest);
   PTEInspectionDetail inspectionformdetail = new PTEInspectionDetail();

   PTEInspectionForm inspectionForm = new PTEInspectionForm();
   inspectionForm.Status = true;
   inspectionForm.Message = "success";

   logger.Info("About to Get LTO Inspection Detail with ApplicationId =>" + applicationId);

   try {


    if (string.IsNullOrEmpty(applicationId)) {
     inspectionForm.Status = false;
     inspectionForm.Message = "Application Reference Must Be Set";
     inspectionForm.inspectionDetail = inspectionformdetail;

     return Json(inspectionForm, JsonRequestBehavior.AllowGet);
    } else {


     appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId).FirstOrDefault();
     if (appRequest ==
      default (ApplicationRequest)) {
      inspectionForm.Status = false;
      inspectionForm.inspectionDetail = inspectionformdetail;
      inspectionForm.Message = "Application Reference " + applicationId + " Cannot be retrievd from the System";
      return Json(inspectionForm, JsonRequestBehavior.AllowGet);
     }


     Appointment appointment = dbCtxt.Appointments.Where(a => a.ApplicationId == applicationId).ToList().LastOrDefault();
     if (appointment ==
      default (Appointment)) {
      inspectionForm.Status = false;
      inspectionForm.inspectionDetail = inspectionformdetail;
      inspectionForm.Message = "Appointment Reference " + applicationId + " Cannot be retrievd from the System";
      return Json(inspectionForm, JsonRequestBehavior.AllowGet);
     } else {
      inspectionformdetail.ApplicationId = applicationId;
      inspectionformdetail.CompanyRepresentative = appointment.ContactPerson;
      inspectionformdetail.AppointmentDateTime = appointment.AppointmentDate.GetValueOrDefault().ToString("dd/MM/yyyy hh:mm", ukCulture);
     }

     logger.Info("Appointment with Id =>" + appointment.AppointmentId);

    
     AppointmentReport appointmentReport = appointment.AppointmentReports.LastOrDefault();
     if (appointmentReport !=
      default (AppointmentReport)) {
      inspectionformdetail.ParticipatingOfficers = appointmentReport.MemberNames;
      inspectionformdetail.Recommendation = appointmentReport.OfficerRecomm;
     }

     inspectionForm.inspectionDetail = inspectionformdetail;

    }

   } catch (Exception ex) {
    inspectionForm.Status = false;
    inspectionForm.Message = ex.Message;
    inspectionForm.inspectionDetail = inspectionformdetail;
    logger.Error(ex.InnerException);
   }
   logger.Info("About to Return from GetPTEInspectionDetail");

   return Json(inspectionForm, JsonRequestBehavior.AllowGet);
  }




 // GET: /External/GetInspectionForm
  [HttpGet]
  public JsonResult GetLTOInspectionDetail(string applicationId) {

   ApplicationRequest appRequest =
    default (ApplicationRequest);
   LTOInspectionDetail inspectionformdetail = new LTOInspectionDetail();

   LTOInspectionForm inspectionForm = new LTOInspectionForm();
   inspectionForm.Status = true;
   inspectionForm.Message = "success";

   logger.Info("About to Get LTO Inspection Detail with ApplicationId =>" + applicationId);

   try {


    if (string.IsNullOrEmpty(applicationId)) {
     inspectionForm.Status = false;
     inspectionForm.Message = "Application Reference Must Be Set";
     inspectionForm.inspectionDetail = inspectionformdetail;

     return Json(inspectionForm, JsonRequestBehavior.AllowGet);
    } else {


     appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == applicationId).FirstOrDefault();
     if (appRequest ==
      default (ApplicationRequest)) {
      inspectionForm.Status = false;
      inspectionForm.inspectionDetail = inspectionformdetail;
      inspectionForm.Message = "Application Reference " + applicationId + " Cannot be retrievd from the System";
      return Json(inspectionForm, JsonRequestBehavior.AllowGet);
     }


     Appointment appointment = dbCtxt.Appointments.Where(a => a.ApplicationId == applicationId).ToList().LastOrDefault();
     if (appointment ==
      default (Appointment)) {
      inspectionForm.Status = false;
      inspectionForm.inspectionDetail = inspectionformdetail;
      inspectionForm.Message = "Appointment Reference " + applicationId + " Cannot be retrievd from the System";
      return Json(inspectionForm, JsonRequestBehavior.AllowGet);
     } else {
      inspectionformdetail.ApplicationId = applicationId;
      inspectionformdetail.CompanyRepresentative = appointment.ContactPerson;
      inspectionformdetail.AppointmentDateTime = appointment.AppointmentDate.GetValueOrDefault().ToString("dd/MM/yyyy hh:mm", ukCulture);
     }

     logger.Info("Appointment with Id =>" + appointment.AppointmentId);

    
     AppointmentReport appointmentReport = appointment.AppointmentReports.LastOrDefault();
     if (appointmentReport !=
      default (AppointmentReport)) {
      inspectionformdetail.ParticipatingOfficers = appointmentReport.MemberNames;
      inspectionformdetail.Recommendation = appointmentReport.OfficerRecomm;
     }

     inspectionForm.inspectionDetail = inspectionformdetail;

    }

   } catch (Exception ex) {
    inspectionForm.Status = false;
    inspectionForm.Message = ex.Message;
    inspectionForm.inspectionDetail = inspectionformdetail;
    logger.Error(ex.InnerException);
   }
   logger.Info("About to Return from GetPTEInspectionDetail");

   return Json(inspectionForm, JsonRequestBehavior.AllowGet);
  }









    }
}
