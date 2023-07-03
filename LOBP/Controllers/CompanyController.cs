using log4net;
using LOBP.DbEntities;
using LOBP.Helper;
using LOBP.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LOBP.Controllers
{

    [LOBP.Filters.SessionExpireTracker]
    public class CompanyController : Controller
    {
        private UserMaster userMaster;
        private UserHelper userMasterHelper;
        private UtilityHelper commonHelper;
        private WorkFlowHelper workflowHelper = new WorkFlowHelper();
        private ElpServiceHelper serviceIntegrator;
        private ApplicationDocHelper appDocHelper;
        private CultureInfo ukCulture = new CultureInfo("en-GB");
        private LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities();
        private ILog log = log4net.LogManager.GetLogger(typeof(CompanyController));
        private LicenseHelper licenseHelper = new LicenseHelper();


        public CompanyController() { }
        protected override void Initialize(RequestContext requestContext)
        {
            try
            {
                base.Initialize(requestContext);
                userMaster = (UserMaster)Session["UserID"];
                userMasterHelper = new UserHelper(dbCtxt);
                commonHelper = new UtilityHelper(dbCtxt);
                // workflowHelper = new WorkFlowHelper(dbCtxt);
                appDocHelper = new ApplicationDocHelper(dbCtxt);
                serviceIntegrator = new ElpServiceHelper(GlobalModel.elpsUrl, GlobalModel.appEmail, GlobalModel.appKey);
            }
            catch (Exception ex)
            {
                log.Error(ex.InnerException);
            }
        }


        // GET: /Company/
        public ActionResult Index()
        {
            string responseMessage = null;
            Dictionary<string, int> appStatistics;
            Dictionary<string, int> appStageReference = null;
            List<ApplicationRequest> AppRequest = null;
            try
            {
                log.Info("About To Generate User DashBoard Information");
                ViewBag.TotalPermitCount = 0;
                ViewBag.ApplicationCount = 0;
                ViewBag.PermitExpiringCount = 0;
                ViewBag.ProcessedApplicationCount = 0;
                ViewBag.CompanyName = userMaster.FirstName;
                var mistdoexpirydate = DateTime.Now;
                ViewBag.Mistdocheck = (from m in dbCtxt.MistdoDatas where m.ElpsId == userMaster.ElpsId && m.certificateExpiry > mistdoexpirydate select m).ToList().Count();




                var Rejectcomment = (from u in dbCtxt.UserMasters
                                     where u.UserId == userMaster.UserId
                                     join a in dbCtxt.ApplicationRequests on u.UserId equals a.ApplicantUserId
                                     join ah in dbCtxt.ActionHistories on a.ApplicationId equals ah.ApplicationId
                                     orderby ah.ActionId descending
                                     select new { ah.MESSAGE, ah.Action }).FirstOrDefault();

                

                if (Rejectcomment != null)
                {
                    TempData["Rejectcomment"] = Rejectcomment.MESSAGE;
                    TempData["Acceptcomment"] = Rejectcomment.Action;
                }



                var extrapay = (from a in dbCtxt.ApplicationRequests
                                join e in dbCtxt.ExtraPayments on a.ApplicationId equals e.ApplicationID
                                where a.ApplicationId == e.ApplicationID && e.Status != "AUTH"
                                select new { e.TxnAmount, a.ApplicationId, a.ApplicantUserId }).ToList().LastOrDefault();
                if (extrapay != null)
                {
                    ViewBag.ExtraPaymentAmount = extrapay.TxnAmount;
                    ViewBag.ExtraPaymentAPPID = extrapay.ApplicationId;
                    ViewBag.ExtraPay = extrapay;
                    ViewBag.LoggedInUser = userMaster.UserId;
                    ViewBag.ExtraPaymentEmail = extrapay.ApplicantUserId;
                }












                log.Info("About To Get Applications and Company Notification Messages");

                ViewBag.AllMessages = appDocHelper.GetCompanyMessages(dbCtxt, userMaster);
                ViewBag.AllUserApplication = appDocHelper.GetApplications(userMaster.UserId, "ALL", out responseMessage);

                log.Info("GetApplications ResponseMessage => " + responseMessage);

                if (responseMessage == "SUCCESS")
                {

                    ViewBag.AllMessages = appDocHelper.GetCompanyMessages(userMaster);
                    ViewBag.Allcomments = appDocHelper.AllHistoryComment(userMaster.UserId);

                    appStatistics = appDocHelper.GetApplicationStatistics(userMaster.UserId, out responseMessage, out appStageReference);
                    ViewBag.TotalPermitCount = appStatistics["PEM"];
                    ViewBag.ApplicationCount = appStatistics["ALL"];
                    ViewBag.PermitExpiringCount = appStatistics["EXP"];
                    ViewBag.ProcessedApplicationCount = appStatistics["PROC"];

                    ViewBag.AllUserApplication = appDocHelper.GetApplications(userMaster.UserId, "ALL", out responseMessage);
                    ViewBag.AllUserApplication = appDocHelper.GetUnissuedLicense(userMaster.UserId, "ALL", out responseMessage);
                    ViewBag.CheckEmptyRecord = (from a in dbCtxt.ApplicationRequests where a.ApplicantUserId == userMaster.UserId select a).ToList().Count();
                    ViewBag.Unissuedcount = (from a in dbCtxt.ApplicationRequests where a.ApplicantUserId == userMaster.UserId && a.LicenseReference == null select a).ToList().Count();
                    ViewBag.ApprovedApplicationAlert = (from a in dbCtxt.ApplicationRequests where a.ApplicantUserId == userMaster.UserId orderby a.ApplicantUserId descending select a.CurrentStageID == 19).FirstOrDefault();
                    var legacycheck = (from l in dbCtxt.ApplicationRequests where l.ApplicantUserId == userMaster.UserId && l.IsLegacy == "YES" select new { l.IsLegacy, l.CurrentStageID, l.ApplicationId, l.LicenseTypeId }).ToList().LastOrDefault();
                    var nonlegacycheck = (from l in dbCtxt.ApplicationRequests where l.ApplicantUserId == userMaster.UserId && l.IsLegacy == "NO" select l).ToList().Count();
                    if (legacycheck != null)
                    {

                        ViewBag.islegacy = legacycheck.IsLegacy;
                        ViewBag.legacystage = legacycheck.CurrentStageID;
                        ViewBag.legacyappid = legacycheck.ApplicationId;
                        ViewBag.LicenseCodes = legacycheck.LicenseTypeId;

                    }


                    AppRequest = appDocHelper.GetApplicationDetails(userMaster.UserId, out AppRequest);

                    ViewBag.AllApplicationStageDetails = AppRequest;


                   


                    List<InspectionModel> AllSchedules = new List<InspectionModel>();
                    var appschedule = (from s in dbCtxt.Appointments
                                       join a in dbCtxt.ApplicationRequests
                                       on s.ApplicationId equals a.ApplicationId
                                       where a.CurrentStageID == 10 && a.ApplicantUserId == userMaster.UserId
                                       select new { s.SchduleExpiryDate, s.ScheduledDate, s.ApplicationId, a.CurrentStageID}).GroupBy(x => x.ApplicationId).Select(grp => grp.OrderByDescending(x => x.SchduleExpiryDate).FirstOrDefault()).ToList();

                    if (appschedule != null)
                    {
                        foreach (var item in appschedule)
                        {
                            AllSchedules.Add(new InspectionModel()
                            {
                                ApplicationId = item.ApplicationId,
                                CurrentStageID = item.CurrentStageID,
                                SchduleExpiryDate = item.SchduleExpiryDate
                            });
                        }
                    }
                    ViewBag.ScheduleReference = AllSchedules;





                    log.Info("GetApplicationStatistics ResponseMessage=> " + responseMessage);
                }

                log.Info("GetApplicationStatistics Count => " + appStageReference.Count);
                ViewBag.StageReferenceList = appStageReference;
                ViewBag.ErrorMessage = responseMessage;

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace + Environment.NewLine + "InnerException =>" + ex.InnerException);
                ViewBag.ErrorMessage = "Error Occured Getting the Company DashBoard, Please Try again Later";
            }

            return View();
        }







        [HttpPost]
        public ActionResult EditApplication(FormCollection coll)
        {
            try
            {
                var appid = coll.Get("ApplicationID");
                var legacy = coll.Get("IsLegacy");
                var TypeCode = coll.Get("LicenseTypeCode");
                var capacity = coll.Get("StorageCapacity");
                var appdetails = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == appid select a).FirstOrDefault();
                //appdetails.ApplicantName = coll.Get("ApplicantName");
                appdetails.ModifiedDate = DateTime.Now;
                appdetails.RegisteredAddress = coll.Get("RegisteredAddress");
                appdetails.SiteLocationAddress = coll.Get("LocationAddress");
                appdetails.StateCode = coll.Get("mystatecode");
                appdetails.LgaCode = coll.Get("mylgacode");
                if (TypeCode == "LTO")
                {
                    appdetails.StorageCapacity = capacity;
                }
                else if (legacy == "YES")
                {
                    appdetails.ApplicationTypeId = coll.Get("ApplicationType");
                    appdetails.LicenseExpiryDate = Convert.ToDateTime(coll.Get("ExpiryDate"));
                    appdetails.LicenseIssuedDate = Convert.ToDateTime(coll.Get("IssuedDate"));
                    appdetails.LicenseReference = coll.Get("LicenseReference");
                    appdetails.LicenseTypeId = TypeCode;
                }
                dbCtxt.SaveChanges();
                TempData["DeleteMessage"] = "Application with the reference number " + appid + " was successfully updated";
            }
            catch (Exception ex) { TempData["DeleteErrorMessage"] = ex.Message; }

            return RedirectToAction("MyApplications");
        }









        [HttpPost]
        public JsonResult DeleteApplication(string AppID)
        {
            string message = "";
            try
            {
                var deleteapp = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == AppID select a).FirstOrDefault();


                if (deleteapp != null)
                {
                    var countfacid = (from a in dbCtxt.ApplicationRequests where a.FacilityId == deleteapp.FacilityId select a).ToList().Count();

                    var deletefac = (from f in dbCtxt.Facilities where f.FacilityId == deleteapp.FacilityId select f).FirstOrDefault();
                    if (countfacid == 1)
                    {
                        dbCtxt.Facilities.Remove(deletefac);

                    }
                    dbCtxt.ApplicationRequests.Remove(deleteapp);

                }

                message = "success";
                dbCtxt.SaveChanges();
            }
            catch (Exception ex) { message = ex.Message; }

            return Json(message, JsonRequestBehavior.AllowGet);
        }










        [HttpGet]
        public ActionResult CompanyProfile()
        {
            CompanyDetail compDto = null;

            //List<SelectListItem> StateList = new List<SelectListItem>();
            List<SelectListItem> Nationality = new List<SelectListItem>();
            List<SelectListItem> StateAddress = new List<SelectListItem>();
            List<SelectListItem> CountryAddress = new List<SelectListItem>();
            CompanyInformationModel compDetailsModel = new CompanyInformationModel();
            List<DocumentModel> companyDocuments = new List<DocumentModel>();

            try
            {
                ViewBag.ApplicantName = userMaster.FirstName;
                ElpsResponse elpsResponse = serviceIntegrator.GetCompanyDetailByEmail(userMaster.UserId);
                log.Info("Result From CompanyDetail =>" + elpsResponse.message);
                var firsttimer = (from u in dbCtxt.UserMasters where u.UserId == userMaster.UserId && u.LoginCount == 1 select u).FirstOrDefault();
                var emptyregaddress = (from u in dbCtxt.ApplicationRequests where u.ApplicantUserId == userMaster.UserId select u.RegisteredAddress).FirstOrDefault();

                ViewBag.FirstTimeLogin = firsttimer;
                ViewBag.Emptyregaddress = emptyregaddress;
                if (elpsResponse.message == "SUCCESS")
                {
                    companyDocuments = appDocHelper.GetAllCompanyDocuments(userMaster.ElpsId, GlobalModel.elpsUrl, serviceIntegrator);
                    ViewBag.ResponseMessage = "SUCCESS";
                    ViewBag.AllDocument = companyDocuments;

                    compDto = (CompanyDetail)elpsResponse.value;
                    if (compDto != null)
                    {
                        compDetailsModel.company = compDto;
                    }
                }

                log.Info("Company =>" + JsonConvert.SerializeObject(compDto));

                if (compDto.registered_Address_Id != null)
                {
                    log.Info("compDto.registered_Address_Id");
                    elpsResponse = serviceIntegrator.GetAddressByID(Convert.ToString(compDto.registered_Address_Id));
                    log.Info("Result for GetAddressByID =>" + elpsResponse.message);
                    if (elpsResponse.message == "SUCCESS")
                    {

                        compDetailsModel.registeredAddress = (CompanyAddressDTO)elpsResponse.value;
                        log.Info("CompanyAddressDTO Address =>" + compDetailsModel.registeredAddress.address_1);
                    }
                }

                if (compDetailsModel.registeredAddress == null)
                {
                    compDetailsModel.registeredAddress = new CompanyAddressDTO();
                }

                if (!string.IsNullOrEmpty(compDto.operational_Address_Id))
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
                log.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error Occured Getting CompanyProfile From Elps, Pls Try again Later";
            }

            return View(compDetailsModel);
        }







        public ActionResult UpdateCompanyRecord(CompanyInformationModel compDetailsModel, FormCollection formCollection)
        {
            string status = "success";
            string jsonRequest = null;
            ElpsResponse wrapper = null;
            CompanyInformationModel companyInfoModel = new CompanyInformationModel();
            CompanyChangeModel companyChangeEmail = new CompanyChangeModel();

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
                log.Info("JsonRequest =>" + jsonRequest);

                wrapper = serviceIntegrator.maintainCompanyInformation(actionType, companyId, jsonRequest, companyChangeEmail);
                log.Info("Response From Elps =>" + wrapper.message);

                if (wrapper.message != "SUCCESS")
                {
                    status = wrapper.message;
                    log.Error(status);
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
                        log.Info("JsonRequest =>" + jsonRequest);

                        wrapper = serviceIntegrator.maintainCompanyInformation("ADD_ADDRESS", companyId, jsonRequest, companyChangeEmail);
                        if (wrapper.message == "SUCCESS")
                        {
                            log.Info("Successfully Created Address");
                            List<CompanyAddressDTO> compaddressList = (List<CompanyAddressDTO>)wrapper.value;
                            CompanyAddressDTO compaddress = compaddressList.FirstOrDefault();

                            log.Info("Address Response  =>" + JsonConvert.SerializeObject(compaddressList));

                            compDetail.registered_Address_Id = Convert.ToString(compaddress.id);
                            companyInfoModel = new CompanyInformationModel();
                            companyInfoModel.company = compDetail;
                            log.Info("About to Link Created Address");

                            log.Info("Company To Link  =>" + JsonConvert.SerializeObject(companyInfoModel));
                            wrapper = serviceIntegrator.maintainCompanyInformation("UPDATE_PROFILE", companyId, JsonConvert.SerializeObject(companyInfoModel), companyChangeEmail);
                            if (wrapper.message != "SUCCESS")
                            {
                                status = wrapper.message;
                            }
                            else
                            {
                                log.Info("Company Link Is SUCESS  =>" + JsonConvert.SerializeObject((CompanyDetail)wrapper.value));
                                log.Info("Address is Linked");
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
                        log.Info("JsonRequest =>" + jsonRequest);

                        wrapper = serviceIntegrator.maintainCompanyInformation("UPDATE_ADDRESS", companyId, jsonRequest, companyChangeEmail);
                        status = wrapper.message;
                    }

                }
                else
                {
                    status = wrapper.message;
                    log.Error(status);
                }

            }

            return Json(new
            {
                Status = status
            },
             JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(PasswordModel model)
        {
            string errorMessage = null;

            try
            {
                ElpsResponse elpsResponse = serviceIntegrator.ChangePassword(userMaster.UserId, model.OldPassword, model.NewPassword);
                log.Info("Response from Elps =>" + elpsResponse.message);

                if (elpsResponse.message != "SUCCESS")
                {
                    errorMessage = "Error Occured Contacting Elps, Please Try Again Later";
                }
                else
                {
                    if (((bool)elpsResponse.value) == true)
                    {
                        {
                            errorMessage = "success";
                        }
                    }
                    else
                    {
                        errorMessage = "Password Cannot be  Changed,KIndly Ensure your Original Password is Correct";
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                errorMessage = "General Error occured during Change Password Routine,Please Try Again Later";
            }

            return Json(new
            {
                Message = errorMessage
            },
             JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult MyLicenses()
        {
            string errorMessage = null;
            List<ApplicationRequest> applicationRequestList = new List<ApplicationRequest>();
            try
            {
                applicationRequestList = appDocHelper.GetApplications((userMaster).UserId, "PEM", out errorMessage);
                ViewBag.ErrorMessage = errorMessage;
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error Occured Getting  Approvals and Licenses, Please Try again Later";
            }

            return View(applicationRequestList);
        }


        [HttpGet]
        public ActionResult MyPayments()
        {
            List<PaymentLog> paymentLogList = new List<PaymentLog>();
            try
            {
                paymentLogList = dbCtxt.PaymentLogs.Where(p => p.ApplicantUserId == userMaster.UserId).ToList();
                ViewBag.ErrorMessage = "SUCCESS";
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error Occured Getting Payment List, Please Try Again Later";
            }

            return View(paymentLogList);
        }


        [HttpGet]
        public ActionResult MyDocuments()
        {
            List<DocumentModel> companyDocuments = new List<DocumentModel>();
            try
            {
                ViewBag.ApplicantName = userMaster.FirstName;
                companyDocuments = appDocHelper.GetAllCompanyDocuments(userMaster.ElpsId, GlobalModel.elpsUrl, serviceIntegrator);
                ViewBag.ResponseMessage = "SUCCESS";
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                ViewBag.ResponseMessage = "An Error Occured Getting Company Document From Elps, Pls Try again Later";
            }

            return View(companyDocuments);
        }

        [HttpGet]
        public ActionResult UpdateDocument(int fileId, int docId, string documentName)
        {
            //'@ViewBag.ElpsUrl' + '/api/CompanyDocument/UpdateFile/' + docId + '/@ViewBag.ElpsId' + '/company?docid=' + docId;
            ViewBag.ElpsId = userMaster.ElpsId;
            ViewBag.UpdateFileUrl = GlobalModel.elpsUrl + "/api/CompanyDocument/UpdateFile/{0}/{1}/company?docid={2}";
            ViewBag.ApplicationHash = commonHelper.GenerateHashText(GlobalModel.appEmail + GlobalModel.appKey);
            ViewBag.Email = GlobalModel.appEmail;
            ViewBag.ElpsUrl = GlobalModel.elpsUrl;
            ViewBag.DocId = docId;
            ViewBag.FileId = fileId;
            ViewBag.DocumentName = documentName;

            return View();
        }










        [HttpGet]
        public ActionResult MyApplications()
        {
            //String errorMessage = null;
            List<SelectListItem> StateList = new List<SelectListItem>();
            List<SelectListItem> LgaList = new List<SelectListItem>();
            try
            {
                var appdetails = new List<ApplicationRequest>();
                var wksbackapp = (from p in dbCtxt.ApplicationRequests
                                  where p.ApplicantUserId == userMaster.UserId && p.IsLegacy == "NO"
                                  select new
                                  {
                                      p.ApplicationId,
                                      p.ApplicantName,
                                      p.LicenseTypeId,
                                      p.ApplicationTypeId,
                                      p.RegisteredAddress,
                                      p.SiteLocationAddress,
                                      p.StorageCapacity,
                                      p.LicenseReference,
                                      p.LicenseIssuedDate,
                                      p.LicenseExpiryDate,
                                      p.IsLegacy,
                                      p.CurrentStageID,
                                      p.AddedDate,
                                      p.Status,
                                      p.GPSCordinates

                                  }).ToList();
                foreach (var item in wksbackapp)
                {
                    appdetails.Add(new ApplicationRequest()
                    {
                        ApplicationId = item.ApplicationId,
                        ApplicantName = item.ApplicantName,
                        LicenseTypeId = item.LicenseTypeId,
                        ApplicationTypeId = item.ApplicationTypeId,
                        RegisteredAddress = item.RegisteredAddress,
                        SiteLocationAddress = item.SiteLocationAddress,
                        StorageCapacity = item.StorageCapacity,
                        LicenseReference = item.LicenseReference,
                        LicenseIssuedDate = item.LicenseIssuedDate,
                        LicenseExpiryDate = item.LicenseExpiryDate,
                        IsLegacy = item.IsLegacy,
                        CurrentStageID = item.CurrentStageID,
                        AddedDate = item.AddedDate,
                        Status = item.Status,
                        GPSCordinates = item.GPSCordinates
                    });
                }
                ViewBag.ApplicationDetails = appdetails;

                ViewBag.State = new SelectList(GlobalModel.AllStates.OrderBy(c => c.StateName), "StateCode", "StateName");

                ViewBag.LGA = LgaList;

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                ViewBag.ResponseMessage = "Error Occured Getting  Application List, Please Try Again Later";
            }
            return View();
        }









        [HttpGet]
        public ActionResult RouteApplication(string ApplicationId = null)
        {
            string actionName = "";
            string applicationForm = "";

            ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();






            switch (appRequest.CurrentStageID)
            {
                case 1:
                    actionName = "DocumentUpload";
                    break;
                case 2:
                    actionName = "DocumentUpload";
                    break;
                case 3:
                    actionName = "ChargeSummary";
                    break;
                case 4:
                    actionName = "ChargeSummary";
                    break;
                default:
                    actionName = "DocumentUpload";
                    break;
            }

            return RedirectToAction(actionName, new
            {
                ApplicationId = appRequest.ApplicationId
            });

        }

        [HttpGet]
        public ActionResult ApplyForLicense()
        {
            List<LicenseType> licenseTypeList = new List<LicenseType>();
            try
            {
                licenseTypeList = dbCtxt.LicenseTypes.Where(l => l.Status == "ACTIVE").OrderBy(l => l.SequencNo).ToList();
                ViewBag.ErrorMessage = "SUCCESS";
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error Occured Getting Permit List, Please Try Again Later";
            }
            return View(licenseTypeList);
        }


        [HttpGet]
        public ActionResult PermitApplications()
        {
            string errorMessage = string.Empty;
            List<ApplicationRequest> applicationTypeList = new List<ApplicationRequest>();
            try
            {
                applicationTypeList = appDocHelper.GetPTEATCLTOApplications(userMaster.UserId, "ALL", out errorMessage);
                ViewBag.ErrorMessage = errorMessage;
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                ViewBag.ErrorMessage = "Error Occured Getting Permit List, Please Try Again Later";
            }
            return View(applicationTypeList);
        }

        public ActionResult ApplicationForm(string ApplicationId)
        {
            string status = null;
            string message = null;

            log.Info("Coming Into the ApplicationForm");

            ApplicationRequest applicationRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();
            if (applicationRequest ==
             default(ApplicationRequest))
            {
                status = "error";
                message = "Application ID with Reference " + ApplicationId + " Cannot be retrievd from the Database";
                return Json(new
                {
                    Status = status,
                    Message = message
                }, JsonRequestBehavior.AllowGet);
            }

            log.Info("Retrieved LicenseTypeId =>" + applicationRequest.LicenseTypeId);
            log.Info("IsLegacy =>" + applicationRequest.IsLegacy);

            if (applicationRequest.IsLegacy == "YES")
            {
                return RedirectToAction("LegacyApplication", new
                {
                    LicenseTypeId = applicationRequest.LicenseTypeId,
                    ReferenceId = ApplicationId
                });
            }
            else
            {

                if (applicationRequest.LicenseTypeId == "PTE" || applicationRequest.LicenseTypeId == "SSA" || applicationRequest.LicenseTypeId == "TITA" || applicationRequest.LicenseTypeId == "TCA" || applicationRequest.LicenseTypeId.Contains("TPBA"))
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
                        ApplicationId = ApplicationId,
                        PTEReference = applicationRequest.LinkedReference

                    });
                }
                else if (applicationRequest.LicenseTypeId == "LTO")
                {
                    return RedirectToAction("LicenseToOperate", new
                    {
                        ApplicationTypeId = applicationRequest.ApplicationTypeId,
                        LicenseTypeId = applicationRequest.LicenseTypeId,
                        ApplicationId = ApplicationId,
                        LinkedReference = applicationRequest.LinkedReference
                    });
                }

                else if (applicationRequest.LicenseTypeId == "ATM" || applicationRequest.LicenseTypeId == "ATO")
                {

                    return RedirectToAction("LicenseToOperate");


                }
            }



            return View();
        }

        public ActionResult TOATMRedirect(string LicenseNumber, string ApplicationRef)
        {

            var applicationRequest = (from a in dbCtxt.ApplicationRequests where a.LicenseReference == LicenseNumber select a).FirstOrDefault();


            if (applicationRequest != null)
            {
                if (applicationRequest.LicenseTypeId == "LTO")
                {
                    return RedirectToAction("LicenseToOperate", new
                    {
                        ApplicationTypeId = applicationRequest.ApplicationTypeId,
                        LicenseTypeId = applicationRequest.LicenseTypeId,
                        ApplicationId = applicationRequest.ApplicationId,
                        LinkedReference = applicationRequest.LinkedReference,
                        appref = ApplicationRef
                    });
                }
                else if (applicationRequest.LicenseTypeId == "SSA" || applicationRequest.LicenseTypeId == "PTE" || applicationRequest.LicenseTypeId == "TITA" || applicationRequest.LicenseTypeId == "TCA" || applicationRequest.LicenseTypeId == "TPBA")
                {
                    return RedirectToAction("PermitToEstablish", new
                    {
                        LicenseTypeId = applicationRequest.LicenseTypeId,
                        ApplicationId = applicationRequest.ApplicationId,
                        appref = ApplicationRef
                    });

                }
                else if (applicationRequest.LicenseTypeId == "ATC")
                {
                    return RedirectToAction("ApprovalToConstruct", new
                    {
                        LicenseTypeId = applicationRequest.LicenseTypeId,
                        ApplicationId = applicationRequest.ApplicationId,
                        PTEReference = applicationRequest.LinkedReference,
                        appref = ApplicationRef
                    });
                }
                else
                {
                    return null;
                }

            }
            else { return null; }


        }

        public static string SentenceCase(string input)
        {
            if (input.Length < 1)
                return input;

            string sentence = input.ToLower();
            return sentence[0].ToString().ToUpper() +
               sentence.Substring(1);
        }

        [HttpGet]
        public ActionResult DocumentUpload(string ApplicationId)
        {
            DocumentUploadModel model = new DocumentUploadModel();
            ElpsResponse wrapper = new ElpsResponse();

            try
            {



                using (var dbCtxt = new LubeBlendingDBEntities())
                {
                    ApplicationRequest applicationRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();


                    if (applicationRequest ==
                     default(ApplicationRequest))
                    {
                        log.Error("Application ID with Reference " + ApplicationId + " Cannot be retrievd from the Database");
                        ViewBag.ResponseMessage = "Application ID with Reference " + ApplicationId + " Cannot be retrievd from the Database";
                        return View(model);
                    }
                    else
                    {
                        ViewBag.LicenseDescription = (from a in dbCtxt.LicenseTypes where a.LicenseTypeId == applicationRequest.LicenseTypeId select a.ShortName).FirstOrDefault();

                        log.Info("About to Proceed with LicenseDefnID => ");
                        ViewBag.ApplicationId = applicationRequest.ApplicationId;
                        ViewBag.ElpsId = userMaster.ElpsId;
                        ViewBag.UserID = userMaster.UserId;

                        log.Info("About to get GetCompanyDocumentListById => " + userMaster.ElpsId);



                        var facilityelpsid = (from f in dbCtxt.Facilities
                                              join a in dbCtxt.ApplicationRequests on f.LocationAddress equals a.SiteLocationAddress
                                              where a.ApplicationId == ApplicationId && a.FacilityId == f.FacilityId
                                              select f.ElpsFacilityId).FirstOrDefault();

                        ViewBag.Elpsfacid = facilityelpsid;

                        ElpsResponse elpsResponse = serviceIntegrator.GetCompanyDocumentListById(userMaster.ElpsId);

                        ElpsResponse facilityelpsResponse = serviceIntegrator.GetFacilityDocumentListById(facilityelpsid.ToString());

                        ElpsResponse allfacilityelpsResponse = serviceIntegrator.GetAllFacilityDocumentListById();

                        ElpsResponse AllDocselpsResponse = serviceIntegrator.GetAllDocumentType();


                        if (elpsResponse.message != "SUCCESS")
                        {
                            log.Error(elpsResponse.message);
                            ViewBag.ResponseMessage = elpsResponse.message;
                            return View(model);
                        }
                        var doctype = (from l in dbCtxt.LicenseTypes where l.LicenseTypeId == applicationRequest.LicenseTypeId select l.ShortName).FirstOrDefault();
                        ViewBag.LicenseDescription = doctype;
                        log.Info("About to Cast DocumentList");
                        List<Document> ElpsDocumenList = (List<Document>)elpsResponse.value;
                        List<AllDocumentTypes> AllDocElpsDocumenList = (List<AllDocumentTypes>)AllDocselpsResponse.value;
                        List<FacilityDocument> ElpsFacDocumenList = (List<FacilityDocument>)facilityelpsResponse.value;
                        List<FacilityDocument> AllElpsFacDocumenList = (List<FacilityDocument>)allfacilityelpsResponse.value;// use if facility app id equals null

                        log.Info("ElpsDocument Size =>" + ElpsDocumenList.Count);
                        List<MissingDocument> missindoc = (from m in dbCtxt.MissingDocuments where m.ApplicationID == ApplicationId select m).ToList();
                        List<RequiredLicenseDocument> AllCompanyDocumentList = dbCtxt.RequiredLicenseDocuments.Where(c => c.ApplicationTypeId.Trim() == applicationRequest.ApplicationTypeId.Trim() && c.LicenseTypeId == applicationRequest.LicenseTypeId && c.IsBaseTran == "B" && c.Status.Equals("ACTIVE")).ToList(); /*applicationRequest.LicenseTypeCode*/
                        List<RequiredLicenseDocument> AllFacilityDocList = dbCtxt.RequiredLicenseDocuments.Where(c => c.ApplicationTypeId.Trim() == applicationRequest.ApplicationTypeId.Trim() && c.LicenseTypeId == applicationRequest.LicenseTypeId && c.IsBaseTran == "T" && c.Status.Equals("ACTIVE")).ToList();
                        List<RequiredLicenseDocument> CompareDocList = dbCtxt.RequiredLicenseDocuments.Where(c => c.ApplicationTypeId.Trim() == applicationRequest.ApplicationTypeId.Trim() && c.LicenseTypeId == applicationRequest.LicenseTypeId && c.Status.Equals("ACTIVE")).ToList();


                        log.Info("DocumentApplicationType Size =>" + AllCompanyDocumentList.Count);
                        model.ApplicationId = applicationRequest.ApplicationId;
                        model.AdditionalDocumentList = appDocHelper.MissingDocuments(missindoc, AllDocElpsDocumenList, CompareDocList, applicationRequest.ApplicationId, GlobalModel.elpsUrl);
                        model.CompanyDocumentList = appDocHelper.GetDocumentsPending(AllCompanyDocumentList, ElpsDocumenList, applicationRequest.ApplicationId, GlobalModel.elpsUrl);
                        model.CompanyFacilityMissingDocumentList = appDocHelper.CompanyFacilityMissingDocuments(AllDocElpsDocumenList, AllElpsFacDocumenList, AllCompanyDocumentList, AllFacilityDocList, CompareDocList);



                        if (facilityelpsid != null)
                        {
                            model.FacilityDocumentList = appDocHelper.GetFacilityDocumentsPending(AllFacilityDocList, ElpsFacDocumenList, applicationRequest.ApplicationId, GlobalModel.elpsUrl);
                        }
                        else
                        {
                            model.FacilityDocumentList = appDocHelper.GetFacilityDocumentsPending(AllFacilityDocList, AllElpsFacDocumenList, applicationRequest.ApplicationId, GlobalModel.elpsUrl);
                        }
                        model.ElpsId = userMaster.ElpsId;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        model.ApplicationHash = userMasterHelper.GenerateSHA512(GlobalModel.appEmail + GlobalModel.appKey);
                        model.Email = GlobalModel.appEmail;
                        model.ElpsUrl = GlobalModel.elpsUrl;
                        ViewBag.AppDescription = dbCtxt.LicenseTypes.Where(l => l.LicenseTypeId == applicationRequest.LicenseTypeId).FirstOrDefault().Description;
                        //ViewBag.AppDescription = commonHelper.GetApplicationDescription(applicationRequest.ApplicationType);
                        ViewBag.ResponseMessage = "SUCCESS";
                    }

                }

            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                ViewBag.ResponseMessage = "An Error Occured Getting the Required Document to Upload, Please Try again Later";
                log.Error(ex.StackTrace);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult DocumentElpServerUpload(string ApplicationId = null, List<SubmittedDocument> MyApplicationDocs = null)
        {
            string message = null;
            string status = "success";
            ResponseWrapper responseWrapper = null;
            try
            {
                log.Info("About to get WorkFlow Navigation for DocumentsUpload");
                if (ApplicationId !=
                 default(string))
                {
                    using (var dbCtxt = new LubeBlendingDBEntities())
                    {


                        var check_doc1 = dbCtxt.SubmittedDocuments.Where(x => x.ApplicationID == ApplicationId).ToList();

                        if (MyApplicationDocs != null)
                        {
                            foreach (var item in MyApplicationDocs)
                            {
                                var check_doc = dbCtxt.SubmittedDocuments.Where(x => x.ApplicationID == ApplicationId && x.DocId == item.DocId).ToList();

                                if (check_doc1.Count() == 0 || check_doc.Count() == 0)
                                {
                                    SubmittedDocument submitDocs = new SubmittedDocument()
                                    {
                                        ApplicationID = item.ApplicationID,
                                        DocId = item.DocId,
                                        BaseorTrans = item.BaseorTrans,
                                        DocName = item.DocName,
                                        FileId = item.FileId,
                                        DocSource = item.DocSource
                                    };
                                    dbCtxt.SubmittedDocuments.Add(submitDocs);
                                }

                                else if (check_doc1.Count() > 0 && check_doc1.Count() == MyApplicationDocs.Count())
                                {
                                    check_doc.FirstOrDefault().ApplicationID = item.ApplicationID;
                                    check_doc.FirstOrDefault().DocId = item.DocId;
                                    check_doc.FirstOrDefault().BaseorTrans = item.BaseorTrans;
                                    check_doc.FirstOrDefault().DocName = item.DocName;
                                    check_doc.FirstOrDefault().FileId = item.FileId;
                                    check_doc.FirstOrDefault().DocSource = item.DocSource;
                                }
                                else
                                {
                                    foreach (var item1 in check_doc1)
                                    {
                                        dbCtxt.SubmittedDocuments.Remove(item1);
                                    }
                                    dbCtxt.SaveChanges();


                                    foreach (var item2 in MyApplicationDocs)
                                    {

                                        SubmittedDocument submitDocs = new SubmittedDocument()
                                        {
                                            ApplicationID = item2.ApplicationID,
                                            DocId = item2.DocId,
                                            BaseorTrans = item2.BaseorTrans,
                                            DocName = item2.DocName,
                                            FileId = item2.FileId,
                                            DocSource = item2.DocSource
                                        };
                                        dbCtxt.SubmittedDocuments.Add(submitDocs);
                                    }
                                    break;
                                }
                            }
                        }

                        dbCtxt.SaveChanges();




                        log.Info("About to get WorkFlow Navigation for DocumentsUpload");
                        //var fieldlocation = userMaster.UserLocation;
                        PaymentLog paymentLog = dbCtxt.PaymentLogs.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();
                        var appstatus = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == ApplicationId select new { a }).FirstOrDefault();
                       // var histry = (from a in dbCtxt.ActionHistories where a.ApplicationId == ApplicationId && a.Action == "Reject" select new { a.TriggeredBy, a.CurrentStageID }).ToList().LastOrDefault();


                        if (appstatus != null)
                        {
                            var comment = appstatus.a.Status == "Rejected" ? "Application with Reference " + ApplicationId + " Resubmited after Fix by Company" : "Document Uploads";
                            var action = appstatus.a.Status == "Rejected" ? "ReSubmit" : "Proceed";

                            // responseWrapper = workflowManager.processAction(dbCtxt, ApplicationId, action, userMaster.UserId, comment, fieldlocation, "");
                            responseWrapper = workflowHelper.processAction(dbCtxt, ApplicationId, action, userMaster.UserId, comment, appstatus.a.CurrentOfficeLocation, "");

                            log.Info("Done with WorkFlow Navigation");
                            if (!responseWrapper.status)
                            {
                                status = "failure";
                                message = "Error Occur during the WorkFlow Navigation, Please try again Later";
                            }

                        }




                    }

                }
                else
                {
                    status = "failure";
                    message = "Application Reference " + ApplicationId + " is Invalid, Please try again Later";
                }

                log.Info("Done with Documents Upload");
            }
            catch (Exception ex) { status = "failure"; message = "System Error Occured During Documents Upload Navigation, Please try again Later"; }
            return Json(new { Status = status, Message = message, applicationId = ApplicationId }, JsonRequestBehavior.AllowGet);
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
            var staff = (from p in dbCtxt.ApplicationRequests
                         where (p.LicenseReference != null && p.LicenseReference != "") && (p.IsLegacy != "YES" && p.ApplicantUserId == userMaster.UserId)
                         select new
                         {
                             p.ApplicationId,
                             p.LicenseReference,
                             p.ApplicantName,
                             p.LicenseTypeId,
                             p.LinkedReference,
                             p.ApplicationTypeId,
                             p.ApplicantUserId,
                             description = (from t in dbCtxt.LicenseTypes where t.LicenseTypeId == p.LinkedReference select t.Description).FirstOrDefault(),
                             takeoverappid = (from t in dbCtxt.TakeoverApps where t.LicenseReference == p.LicenseReference select p.LicenseTypeId).FirstOrDefault()

                         });

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationId + " " + sortColumnDir);
            }
            else
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
                case "5":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.LicenseReference).ToList() : data.OrderBy(p => p.LicenseReference).ToList();
                    break;
            }
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }

        //[HttpGet]
        //public ActionResult DocumentElpServerUpload(string ApplicationId)
        //{
        //    string message = null;
        //    string status = "success";
        //    string errorMessage = null;
        //    ResponseWrapper responseWrapper = null;

        //    try
        //    {
        //        log.Info("About to get WorkFlow Navigation for DocumentsUpload");
        //        if (ApplicationId !=
        //         default(string))
        //        {

        //            if (!commonHelper.isPaymentMade(ApplicationId, out errorMessage))
        //            {
        //                log.Info("About to get WorkFlow Navigation for DocumentsUpload");
        //                responseWrapper = workflowHelper.processAction(ApplicationId, "Proceed", userMaster.UserId, null,"Document Uploaded For Application");
        //                message = responseWrapper.value;
        //                log.Info("Done with WorkFlow Navigation");
        //                if (!responseWrapper.status)
        //                {
        //                    status = "failure";
        //                }
        //            }

        //        }
        //        else
        //        {
        //            status = "failure";
        //            message = "Application Reference " + ApplicationId + " is Invalid, Please try again Later";
        //        }

        //        log.Info("Done with Documents Upload");
        //        log.Info("Message =>" + message);

        //    }
        //    catch (Exception ex)
        //    {
        //        status = "failure";
        //        message = "System Error Occured During Documents Upload Navigation, Please try again Later";
        //        log.Error(ex.InnerException);
        //    }

        //    return Json(new
        //    {
        //        Status = status,
        //        Message = message,
        //        applicationId = ApplicationId
        //    },
        //     JsonRequestBehavior.AllowGet);
        //}

        public ActionResult DeleteLegacyDocument(string fileId, string appid, string apptype, string Licenseid)
        {
            if (appid != null)
            {
                ViewBag.Appid = appid;
            }
            ViewBag.fileId = fileId;
            ViewBag.AppType = apptype;
            ViewBag.LicenseID = Licenseid;
            return PartialView("_DeleteLegacyDocument");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteLegacyDocument(string ApplicationID, string ApplicationDocType, string LicenseID, FormCollection coll)
        {

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
                log.Error(ex.StackTrace);
                throw new Exception("Error when Deleting Document...");
            }
            return RedirectToAction("LegacyApplication", "Company", new { LicenseTypeId = LicenseID, ReferenceId = ApplicationID });
        }

        public ActionResult DeleteDocument(string fileId, string Appid, string apptype)
        {
            var appid = (from a in dbCtxt.ApplicationRequests where a.ApplicantUserId == userMaster.UserId && a.ApplicationId == Appid && a.LicenseReference == null select a).FirstOrDefault();
            if (appid != null)
            {
                ViewBag.Appid = appid.ApplicationId;
            }
            ViewBag.fileId = fileId;
            ViewBag.AppType = apptype;

            return PartialView("_DeleteDocument");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public void DeleteDocument(string ApplicationID, string ApplicationDocType, FormCollection coll)
        {

            var licensetype = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == ApplicationID select a.LicenseTypeId).FirstOrDefault();

            try
            {
                ElpsResponse elpsResponse = null; //serviceIntegrator.DeleteDocument(coll["toDelId"]);
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
                log.Error(ex.StackTrace);
                throw new Exception("Error when Deleting Document...");
            }
            Response.Redirect(Request.UrlReferrer.ToString());
        }

        [HttpGet]
        public ActionResult ChargeSummary(string ApplicationId)
        {
            string errorMessage = null;
            string totalamount = "";
            ApplicationRequest appRequest = null;
            decimal processFeeAmt = 0,
             statutoryFeeAmt = 0,
             lateRenewalAmt = 0,
             NonRenewalAmt = 0,
             Arrears = 0,
             total =0,
             carryOverAmount = 0;


            try
            {
                log.Info("ApplicationID = >" + ApplicationId);
                ViewBag.Paid = false;
                appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    log.Error("Application ID with Reference " + ApplicationId + " Cannot be retrieved from the Database");
                    ViewBag.ResponseMessage = "Application ID with Reference " + ApplicationId + " Cannot be retrievd from the Database";
                    return View(appRequest);
                }

                log.Info("About to Get Application Fees");
                errorMessage = commonHelper.GetApplicationCharges(appRequest, out processFeeAmt, out statutoryFeeAmt, out Arrears, out lateRenewalAmt, out NonRenewalAmt);
                log.Info("Response Message =>" + errorMessage);
                log.Info("GetNetValueAmount Amount " + carryOverAmount);


                if (errorMessage == "SUCCESS")
                {
                    ViewBag.ProcessFee = "₦" + processFeeAmt.ToString("N");
                    ViewBag.LicenseFee = "₦" + statutoryFeeAmt.ToString("N");
                    ViewBag.Arrears = "₦" + Arrears.ToString("N");
                    if(appRequest.LicenseTypeId == "ATO" || appRequest.LicenseTypeId == "ATM" || appRequest.LicenseTypeId == "SSA" || appRequest.LicenseTypeId == "TPBA" || appRequest.LicenseTypeId == "TITA" || appRequest.LicenseTypeId == "TCA" || appRequest.LicenseTypeId == "ATCLFP")
                    {
                        totalamount = "₦" + ((processFeeAmt + Arrears + statutoryFeeAmt) * (Convert.ToDecimal(1.05))).ToString("N");
                    }
                    
                    else
                    {
                        totalamount = "₦" + (processFeeAmt + Arrears + statutoryFeeAmt).ToString("N");

                    }
                    ViewBag.Servicecharge = "₦" + ((processFeeAmt + Arrears + statutoryFeeAmt)* Convert.ToDecimal(0.05)).ToString("N");
                    ViewBag.ApprovalFeeLicenseTypeId = appRequest.LicenseTypeId;
                    
                    if (commonHelper.isPaymentMade(ApplicationId, out errorMessage))
                    {
                        ViewBag.Paid = true;
                    }
                    if (appRequest.LicenseTypeId == "ATO")
                    {
                        var ATOapprovalfess = dbCtxt.Configurations.Where(c => c.ParamID == "ATO_APPROVAL_FEES").FirstOrDefault().ParamValue;
                        ViewBag.Approvalfee = "₦" + Convert.ToDecimal(ATOapprovalfess).ToString("N");
                        totalamount = "₦" + ((processFeeAmt + Arrears + statutoryFeeAmt + Convert.ToDecimal(ATOapprovalfess)) * (Convert.ToDecimal(1.05))).ToString("N");

                    }
                    ViewBag.ElpsURL = GlobalModel.elpsUrl;
                    ViewBag.UserID = userMaster.UserId;
                    ViewBag.TotalAmount = totalamount;
                    ViewBag.ResponseMessage = "SUCCESS";
                }
                else
                {
                    ViewBag.ResponseMessage = errorMessage;
                }
            }
            catch (Exception ex)
            {
                ViewBag.ResponseMessage = "Error Occured during Application Summary Generation, Pls Try again Later";
                log.Info(ex.Message);
            }

            return View(appRequest);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult GeneratePaymentRef(string ApplicationId)
        {
            String NewUrl = null;
            string message = "success";
            string status = "success";
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority) + Request.ApplicationPath;

            try
            {
                log.Info("ApplicationID =>" + ApplicationId);
                log.Info("BaseURL =>" + baseUrl);

                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    status = "failure";
                    message = "Application ID with Reference " + ApplicationId + " Cannot be retrievd from the Database";
                    log.Error(message);
                }
                else
                {
                    String result = commonHelper.GeneratePaymentReference(serviceIntegrator, ApplicationId, userMaster, baseUrl);
                    log.Info("Response from GeneratePaymentRRR =>" + result);
                    var rrrCheck = GlobalModel.elpsUrl + "/Payment/checkifpaid?id=r" + result;
                    status = result.Contains("InternalServerError") ? "InternalServerError" : "success";
                    var res = serviceIntegrator.CheckRRR(result);

                    var resp = JsonConvert.DeserializeObject<JObject>(res.value.ToString());

                    if (resp.GetValue("message").ToString() == "Approved" || resp.GetValue("status").ToString() == "01")
                    {
                        NewUrl = "/Company/PaymentSuccess?orderId=" + resp.GetValue("orderId").ToString();
                    }
                    else
                    {
                        NewUrl = GlobalModel.elpsUrl + "/Payment/Pay?rrr=" + result;
                    }

                }
            }
            catch (Exception ex)
            {
                status = "failure";
                message = "System Error Occured, Please try again Later";
                log.Error(ex.StackTrace, ex);
            }

            return Json(new
            {
                Status = status,
                Message = message,
                NewUrl = NewUrl
            },
             JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult ByPassPayment(string ApplicationId)
        {

            string errorMessage = "SUCCESS";
            decimal processFeeAmt = 0, statutoryFeeAmt = 0, Arrears = 0, lateRenewalAmt = 0, NonRenewalAmt = 0;

            try
            {
                log.Info("ApplicationID =>" + ApplicationId);
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();

                errorMessage = commonHelper.GetApplicationCharges(appRequest, out processFeeAmt, out statutoryFeeAmt, out Arrears, out lateRenewalAmt, out NonRenewalAmt);
                PaymentLog paymentLog = new PaymentLog();
                paymentLog.ApplicationId = appRequest.ApplicationId;
                paymentLog.TransactionDate = DateTime.UtcNow;
                paymentLog.LastRetryDate = DateTime.UtcNow;
                paymentLog.TransactionID = "TXNID";
                paymentLog.LicenseTypeId = appRequest.LicenseTypeId;
                paymentLog.ApplicantUserId = userMaster.UserId;
                paymentLog.RRReference = "RRR";
                paymentLog.AppReceiptID = "APPID";
                paymentLog.TxnAmount = processFeeAmt + statutoryFeeAmt + Arrears;
                // paymentLog.TxnAmount = processFeeAmt + statutoryFeeAmt + (Arrears + lateRenewalAmt + NonRenewalAmt);
                paymentLog.Arrears = Arrears;
                paymentLog.LateRenewalPenalty = lateRenewalAmt;
                paymentLog.NonRenewalPenalty = NonRenewalAmt;
                paymentLog.Account = dbCtxt.Configurations.Where(c => c.ParamID == "ACCOUNT").FirstOrDefault().ParamValue;
                paymentLog.BankCode = dbCtxt.Configurations.Where(c => c.ParamID == "BANKCODE").FirstOrDefault().ParamValue;
                paymentLog.RetryCount = 0;
                paymentLog.Status = "AUTH";

                log.Info("About to Add Payment Log");
                dbCtxt.PaymentLogs.Add(paymentLog);

                log.Info("Added Payment Log to Table");
                dbCtxt.SaveChanges();
                log.Info("Saved it Successfully");





                ResponseWrapper responseWrapper = workflowHelper.processAction(dbCtxt, ApplicationId, "Proceed", userMaster.UserId, "Document Submitted", appRequest.CurrentOfficeLocation, "");

                responseWrapper = workflowHelper.processAction(dbCtxt, ApplicationId, "GenerateRRR", userMaster.UserId, "Remita Retrieval Reference Generated", appRequest.CurrentOfficeLocation, "");

                responseWrapper = workflowHelper.processAction(dbCtxt, paymentLog.ApplicationId.Trim(), "Submit", paymentLog.ApplicantUserId, "Application Reference " + paymentLog.ApplicationId + " have been Submitted to DPR", appRequest.CurrentOfficeLocation, "");

                if (responseWrapper.status)
                {
                    userMasterHelper.AutoAssignApplication(paymentLog.ApplicationId, "AD RBP");
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace, ex);
            }

            return Json(new
            {
                Status = "success",
                Message = "/Company/PaymentReceipt?ApplicationId=" + ApplicationId
            },
             JsonRequestBehavior.AllowGet);
        }

        public ActionResult ALLCompanyPermits()
        {
            return View();
        }

        public ActionResult CompanyViewSUI(string id)
        {
            return licenseHelper.ViewSUI(id);
        }

        public ActionResult CompanyViewPTE(string id)
        {
            return licenseHelper.ViewPTE(id);
        }

        public ActionResult CompanyViewATC(string id)
        {

            return licenseHelper.ViewATC(id);
        }

        public ActionResult CompanyViewLTO(string id)
        {
            return licenseHelper.ViewLTO(id);
        }
        public ActionResult CompanyViewLTOLFP(string id)
        {
            return licenseHelper.ViewLTOLFP(id);
        }
        public ActionResult CompanyViewATCLFP(string id)
        {
            return licenseHelper.ViewATCLFP(id);
        }
        public ActionResult CompanyViewATCMOD(string id)
        {
            return licenseHelper.ViewATCMOD(id);
        }
        public ActionResult CompanyViewTPBA(string id)
        {
            return licenseHelper.ViewTPBA(id);
        }
        public ActionResult CompanyViewTITA(string id)
        {
            return licenseHelper.ViewTITA(id);
        }
        public ActionResult CompanyViewTCA(string id)
        {
            return licenseHelper.ViewTCA(id);
        }


        public ActionResult CompanyViewTO(string id)
        {
            return licenseHelper.ViewTO(id);
        }



        [HttpPost]
        public ActionResult Resubmit(string ApplicationId)
        {
            string message = null;
            string status = "success";
            string NextProcessor = null;

            try
            {
                log.Info("ApplicationID =>" + ApplicationId);
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    status = "failure";
                    message = "Application ID with Reference " + ApplicationId + " Cannot be retrievd from the Database";
                    log.Error(message);
                    return Json(new
                    {
                        Status = status,
                        Message = message
                    },
                     JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //ActionHistory actionHistory = dbCtxt.ActionHistories.Where(a => a.ApplicationId == ApplicationId && (a.Action == "Reject")).OrderByDescending(a => a.ActionDate).ToList().First();
                    //NextProcessor = actionHistory.TriggeredBy;
                    //log.Info("Triggered by :" + NextProcessor);

                    //var fieldlocation = userMaster.UserLocation;

                    ResponseWrapper responseWrap = workflowHelper.processAction(dbCtxt, ApplicationId, "ReSubmit", userMaster.UserId, "Application Resubmited For Reprocessing By Company", appRequest.CurrentOfficeLocation, "");

                    message = responseWrap.value;

                    log.Info("Done with WorkFlow Navigation");
                    if (!responseWrap.status)
                    {
                        status = "failure";
                    }
                }
            }
            catch (Exception ex)
            {
                status = "failure";
                message = "System Error Occured, Please try again Later";
                log.Error(ex.StackTrace, ex);
            }

            return Json(new
            {
                Status = status,
                Message = message
            },
             JsonRequestBehavior.AllowGet);
        }


        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult Remita(string orderId = null, string rrr = null, string status = null)
        {
            log.Info("Coming into Remita with OrderID => " + orderId + " And RRR => " + rrr);

            try
            {
                PaymentLog paymentLog = dbCtxt.PaymentLogs.Where(c => c.ApplicationId.Trim() == orderId.Trim()).FirstOrDefault();
                ApplicationRequest AppRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == orderId.Trim()).FirstOrDefault();

                if (paymentLog ==
                 default(PaymentLog))
                {
                    return Content("Payment Transaction Log Not Found on the System");
                }

                log.Info("Coming to Retrieve the Transaction Status from Remita");
                ElpsResponse result = serviceIntegrator.GetTransactionStatus(orderId);
                log.Info("Transaction Call Response =>" + result.message);

                if (result.message == "SUCCESS")
                {
                    TransactionStatus txnstatus = (TransactionStatus)result.value;
                    log.Info("Remita Status: " + txnstatus.status);
                    log.Info("Remita Message: " + txnstatus.statusmessage);

                    paymentLog.TxnMessage = txnstatus.status + " - " + txnstatus.statusmessage;
                    paymentLog.TransactionDate = DateTime.Now;

                    log.Info("Transaction Message: " + paymentLog.TxnMessage);

                    if (txnstatus.status == "00" || txnstatus.status == "01")
                    {
                        paymentLog.Status = "AUTH";
                    }
                    else
                    {
                        paymentLog.Status = "FAIL";
                    }
                }
                else
                {
                    paymentLog.Status = "FAIL";
                    paymentLog.TxnMessage = result.message;
                    paymentLog.TransactionDate = DateTime.Now;
                }

                log.Info("About to Update Table with Status =>" + paymentLog.Status);
                dbCtxt.SaveChanges();

                /*
                if (paymentLog.Status == "AUTH")
                {
                    ResponseWrapper responseWrapper = workflowHelper.processAction(paymentLog.ApplicationId.Trim(), "Submit", paymentLog.ApplicantUserId,null, "Application Reference " + paymentLog.ApplicationId + " have been Submitted to DPR");
                    if (responseWrapper.status)
                    {
                        userMasterHelper.AutoAssignApplication(paymentLog.ApplicationId, "OFFICER");
                    }
                }*/


                ResponseWrapper responseWrapper = null;
                if (paymentLog.Status == "AUTH" && paymentLog.PaymentType == "MAIN")
                {
                    UserMaster adrbpUser = dbCtxt.UserMasters.Where(u => u.UserRoles.Contains("AD RBP") && u.Status == "ACTIVE").FirstOrDefault();
                    if (adrbpUser != default(UserMaster))
                    {

                        responseWrapper = workflowHelper.processAction(dbCtxt, paymentLog.ApplicationId.Trim(), "Submit", paymentLog.ApplicantUserId, "Application Reference " + paymentLog.ApplicationId + " have been Submitted to DPR", AppRequest.CurrentOfficeLocation, "");

                    }
                    else
                    {
                        log.Info("No Active AD RBP Found on the LOBP Platform");
                    }
                }



            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

            return RedirectToAction("PaymentReceipt", new
            {
                ApplicationId = orderId
            });
        }



        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult RemitaPayment(string orderId = null)
        {

            log.Info("Coming into RemitaPayment with OrderID => " + orderId);

            try
            {
                PaymentLog paymentLog = dbCtxt.PaymentLogs.Where(c => c.ApplicationId.Trim() == orderId.Trim()).FirstOrDefault();
                ApplicationRequest AppRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == orderId.Trim()).FirstOrDefault();
                if (paymentLog ==
                 default(PaymentLog))
                {
                    return Content("Payment Transaction Log Not Found on the System");
                }

                paymentLog.Status = "AUTH";
                paymentLog.TxnMessage = "SUCCESS";
                paymentLog.TransactionDate = DateTime.Now;


                log.Info("About to Update Table with Status =>" + paymentLog.Status);
                dbCtxt.SaveChanges();


                ResponseWrapper responseWrapper = null;
                if (paymentLog.Status == "AUTH" && paymentLog.PaymentType == "MAIN")
                {
                    UserMaster adrbpUser = dbCtxt.UserMasters.Where(u => u.UserRoles.Contains("AD RBP") && u.Status == "ACTIVE").FirstOrDefault();
                    if (adrbpUser != default(UserMaster))
                    {

                        responseWrapper = workflowHelper.processAction(dbCtxt, paymentLog.ApplicationId.Trim(), "Submit", paymentLog.ApplicantUserId, "Application Reference " + paymentLog.ApplicationId + " have been Submitted to DPR", AppRequest.CurrentOfficeLocation, "");




                    }
                    else
                    {
                        log.Info("No Active AD RBP Found on the LOBP Platform");
                        return Content("No Active ADRBP Found on the LOBP Platform, Kindly reach out to DPR Support Office");
                    }
                }



            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

            return RedirectToAction("PaymentReceipt", new
            {
                ApplicationId = orderId
            });
        }








        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult PaymentSuccess(string orderId)//string orderId
        {
            //log.Info("Coming into Remita with OrderID => " + orderId);
            ElpsResponse result = null;
            PaymentLog paymentLog = null;
            ApplicationRequest aprq = null;
            ResponseWrapper responseWrapper = null;
            var checkpaymentlogAppId = (from p in dbCtxt.ExtraPayments where p.ExtraPaymentAppRef == orderId && p.Status == "Pending" select p.ApplicationID).FirstOrDefault();
            paymentLog = dbCtxt.PaymentLogs.Where(c => c.ApplicationId.Trim() == orderId.Trim()).FirstOrDefault();
            ExtraPayment extrapayment = dbCtxt.ExtraPayments.Where(e => e.ExtraPaymentAppRef == orderId.Trim()).FirstOrDefault();
            ApplicationRequest apprequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == orderId.Trim()).FirstOrDefault();

            if (extrapayment != null) { 
                aprq = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == extrapayment.ApplicationID select a).FirstOrDefault();
                }

            try
            {
                //using (var dbCtxt = DbManager.getConnectionEntities())
                //{
                log.Info("Coming to Retrieve the Transaction Status from Remita");
                if (extrapayment != null) { result = serviceIntegrator.GetTransactionStatus(extrapayment.ExtraPaymentAppRef); }
                else { result = serviceIntegrator.GetTransactionStatus(orderId); }
                log.Info("Transaction Call Response =>" + result.message);

                if (result.message == "SUCCESS")
                {
                    TransactionStatus txnstatus = (TransactionStatus)result.value;
                    log.Info("Remita Status: " + txnstatus.status);
                    log.Info("Remita Message: " + txnstatus.statusmessage);
                    if (extrapayment != null)
                    {
                        extrapayment.TxnMessage = txnstatus.status + " - " + txnstatus.statusmessage;
                        extrapayment.TransactionDate = DateTime.Now;
                    }
                    else
                    {
                        paymentLog.TxnMessage = txnstatus.status + " - " + txnstatus.statusmessage;
                        paymentLog.TransactionDate = DateTime.Now;
                        log.Info("Transaction Message: " + paymentLog.TxnMessage);
                    }


                    if (txnstatus.status == "00" || txnstatus.status == "01")
                    {

                        if (extrapayment != null)
                        {
                            var paymet = new PaymentLog();
                            paymet.ApplicationId = extrapayment.ExtraPaymentAppRef;
                            paymet.TransactionDate = DateTime.UtcNow;
                            paymentLog.TxnMessage = "ExtraPaymt";
                            paymet.TransactionID = extrapayment.TransactionID;
                            paymet.LicenseTypeId = extrapayment.LicenseTypeCode;
                            paymet.ApplicantUserId = userMaster.UserId;
                            paymet.Description = extrapayment.Description;
                            paymet.RRReference = extrapayment.RRReference;
                            paymet.AppReceiptID = extrapayment.AppReceiptID;
                            paymet.TxnAmount = extrapayment.TxnAmount;
                            paymet.Arrears = extrapayment.Arrears;
                            paymet.Account = extrapayment.Account;
                            paymet.BankCode = extrapayment.BankCode;
                            paymet.RetryCount = extrapayment.RetryCount;
                            paymentLog.Status = "AUTH";
                            dbCtxt.PaymentLogs.Add(paymet);
                            int done = dbCtxt.SaveChanges();
                            if (done > 0)
                            {

                                if (aprq != null)
                                {
                                    if (aprq.Status == "Rejected")
                                    {
                                        ResponseWrapper responseWrap = workflowHelper.processAction(dbCtxt, extrapayment.ApplicationID, "ReSubmit", userMaster.UserId, "Application Resubmited For Reprocessing By Company", aprq.CurrentOfficeLocation, "");

                                    }
                                    else if (aprq.CurrentStageID < 5)
                                    {


                                        responseWrapper = workflowHelper.processAction(dbCtxt, orderId, "Proceed", userMaster.UserId, "Document Submitted", aprq.CurrentOfficeLocation, "");

                                        responseWrapper = workflowHelper.processAction(dbCtxt, orderId, "GenerateRRR", userMaster.UserId, "Remita Retrieval Reference Generated", aprq.CurrentOfficeLocation, "");

                                        responseWrapper = workflowHelper.processAction(dbCtxt, paymentLog.ApplicationId.Trim(), "Submit", paymentLog.ApplicantUserId, "Application Reference " + paymentLog.ApplicationId + " have been Submitted to DPR", aprq.CurrentOfficeLocation, "");

                                    }
                                }

                            }
                        }
                        if (extrapayment != null)
                        {
                            extrapayment.Status = "AUTH";
                        }
                        if (paymentLog != null)
                        {
                            paymentLog.Status = "AUTH";
                        }
                        apprequest.Status = "Processing";

                        if (apprequest != null)
                        {
                            var Elps = (from a in dbCtxt.ApplicationRequests
                                        join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                                        where a.ApplicationId == apprequest.ApplicationId && a.ApplicantUserId == u.UserId
                                        select new { u.ElpsId, a.Status }).FirstOrDefault();

                            serviceIntegrator.GetElpAppUpdateStatus(apprequest.ApplicationId, Elps.ElpsId, Elps.Status);
                        }
                    }
                    else
                    {
                        if (extrapayment != null) { extrapayment.Status = "FAIL"; }
                        if (paymentLog != null) { paymentLog.Status = "FAIL"; }
                        apprequest.Status = "Payment Failed";
                    }
                }
                else
                {
                    paymentLog.Status = "FAIL";
                    paymentLog.TxnMessage = result.message;
                    paymentLog.TransactionDate = DateTime.Now;
                }
                dbCtxt.SaveChanges();
                if (extrapayment == null)
                {
                    if (paymentLog.Status == "AUTH")
                    {
                        responseWrapper = workflowHelper.processAction(dbCtxt, orderId, "Proceed", userMaster.UserId, "Document Submitted", aprq.CurrentOfficeLocation, "");

                        responseWrapper = workflowHelper.processAction(dbCtxt, orderId, "GenerateRRR", userMaster.UserId, "Remita Retrieval Reference Generated", aprq.CurrentOfficeLocation, "");

                        responseWrapper = workflowHelper.processAction(dbCtxt, paymentLog.ApplicationId.Trim(), "Submit", paymentLog.ApplicantUserId, "Application Reference " + paymentLog.ApplicationId + " have been Submitted to DPR", aprq.CurrentOfficeLocation, "");

                    }

                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

            return RedirectToAction("PaymentReceipt", new { ApplicationId = extrapayment != null ? extrapayment.ExtraPaymentAppRef.ToString() : orderId.ToString() });

        }









        public JsonResult ConfirmPayment(string id)
        {
            string message = "";
            try
            {


                var rrr = (from p in dbCtxt.PaymentLogs where p.ApplicationId == id select p.RRReference).FirstOrDefault();
                if (rrr != null && id != null)
                {
                    var changestatus = (from p in dbCtxt.PaymentLogs where p.ApplicationId == id select p).ToList().LastOrDefault();
                    var changecurrentstage = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == id select a).FirstOrDefault();
                    var rrrCheck = GlobalModel.elpsUrl + "/Payment/checkifpaid?id=r" + rrr;
                    var res = serviceIntegrator.CheckRRR(rrr);
                    var resp = JsonConvert.DeserializeObject<JObject>(res.value.ToString());
                    if (resp.GetValue("message").ToString() == "Approved" || resp.GetValue("status").ToString() == "01")
                    {
                        message = "success";
                        changecurrentstage.Status = "Processing";
                        changecurrentstage.CurrentStageID = 5;
                        changestatus.Status = "AUTH";
                        changestatus.TxnMessage = "Confirmed";
                        dbCtxt.SaveChanges();

                        userMasterHelper.AutoAssignApplication(id, "AD RBP");

                    }
                    else { message = "no payment"; }
                }
                else { message = "no rrr"; }
            }
            catch (Exception ex)
            {
                message = "exception";
                log.Info(ex.Message);
            }
            return Json(message, JsonRequestBehavior.AllowGet);
        }








        public ActionResult GetCalibrationFee()
        {
            var Penalty = (from p in dbCtxt.Penalties where (p.PenaltyCode == 759 || p.PenaltyType.Contains("THIRD PARTY")) select new { p.PenaltyType }).ToList();
            return Json(Penalty, JsonRequestBehavior.AllowGet);
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





        [HttpGet]
        public ActionResult AddExtraPayment()
        {

            return View();
        }






        [HttpPost]
        public ActionResult AddExtraPayment(FormCollection collection)
        {
            //var appid = collection.Get("myappid");
            var status = collection.Get("status");
            var sanctype = collection.Get("penaltytype");
            var descrip = collection.Get("penaltydescription");
            var qty = collection.Get("Qty");
            int done = 0;
            string genApplicationId = commonHelper.GenerateApplicationNo();

            try
            {
                Penalty appPenalty = dbCtxt.Penalties.Where(c => c.PenaltyType.Trim() == sanctype).FirstOrDefault();

               

                var amount = Convert.ToDecimal(collection.Get("penaltyamount"));
                var ExtraAmount = new ExtraPayment()
                {
                    ApplicationID = genApplicationId,
                    Description = descrip,
                    TxnAmount = amount,
                    Qty =  Convert.ToInt32(qty),
                    ExtraPaymentAppRef = genApplicationId,
                    Arrears = 0,
                    LastRetryDate = DateTime.Now,
                    RetryCount = 1,
                    Status = status,
                    SanctionType = sanctype,
                    ApplicantID = userMaster.UserId,
                    LicenseTypeCode = appPenalty.PenaltyCode == 759? "CALIBRATION":"GENERAL",
                    PenaltyCode = appPenalty.PenaltyCode,
                    ExtraPaymentBy = userMaster.UserId
                };
                dbCtxt.ExtraPayments.Add(ExtraAmount);
                 done = dbCtxt.SaveChanges();
                
            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }
            if (done > 0)
            {
                //TempData["GeneSuccess"] = "Extra payment was successfully generated for application with the reference number " + appid;
                var extrapaymentdetails = (from e in dbCtxt.ExtraPayments where e.ApplicationID == genApplicationId select e).FirstOrDefault();
                if (extrapaymentdetails != null)
                {

                    return RedirectToAction("CalibrationPayment", "Company", new { ApplicationId = genApplicationId, code = "Cali" });

                }
                else
                {
                    return RedirectToAction("AddExtraPayment", "Company", new { ApplicationId = genApplicationId });
                }
            }
            else
            {
                return RedirectToAction("AddExtraPayment", "Company", new { ApplicationId = genApplicationId });
            }
        }















        //[AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        //public async Task<ActionResult> CalibrationPayment(string ApplicationId, string code)
        //{
        //    String newUrl = null;
        //    string message = "success";
        //    string status = "";
        //    string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority) + Request.ApplicationPath;


        //    try
        //    {
        //        log.Info("ApplicationID =>" + ApplicationId);
        //        log.Info("BaseURL =>" + baseUrl);

        //        using (var dbCtxt = new LubeBlendingDBEntities())
        //        {
        //            ExtraPayment extraappid = dbCtxt.ExtraPayments.Where(c => c.ExtraPaymentAppRef.Trim() == ApplicationId.Trim()).FirstOrDefault();


        //            if (ApplicationId == null || ApplicationId == "")
        //            {
        //                status = "failure";
        //                message = "Application ID with Reference " + ApplicationId + " Cannot be retrievd from the Database";
        //                log.Error(message);
        //            }
        //            else
        //            {
        //                String result = commonHelper.GenerateCalibrationPaymentReference(serviceIntegrator, ApplicationId, userMaster, baseUrl);
        //                log.Info("Response from GeneratePaymentRRR =>" + result);
        //                var rrrCheck = GlobalModel.elpsUrl + "/Payment/checkifpaid?id=r" + result;
        //                status = result.Contains("InternalServerError") ? "InternalServerError" : "success";
        //                var res = serviceIntegrator.CheckRRR(result);
        //                var resp = JsonConvert.DeserializeObject<JObject>(res.value.ToString());
        //                var expaystatus = extraappid == null ? "empty" : extraappid.Status;


        //                if (extraappid != null && (expaystatus == "Pending" || expaystatus == "FAIL"))
        //                {
                           
        //                        var awaitresult = await IGRPost(result, ApplicationId);
        //                        log.Info("IGR_PARTNER Payment");

        //                }
                       
        //                if (resp.GetValue("message").ToString() == "Approved" || resp.GetValue("status").ToString() == "01")
        //                {
        //                    return RedirectToAction("PaymentSuccess", "Company", new { orderId = resp.GetValue("orderId").ToString() });
        //                }
        //                else if(code == "Cali")
        //                {
        //                    return Redirect(GlobalModel.elpsUrl + "/Payment/Pay?rrr=" + result);
        //                }
        //                else
        //                {
        //                    newUrl = GlobalModel.elpsUrl + "/Payment/Pay?rrr=" + result;
        //                }

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        status = "failure";
        //        message = "System Error Occured, Please try again Later";
        //        log.Error(ex.StackTrace, ex);
        //    }
        //    return Json(new
        //    {
        //        Status = status,
        //        Message = message,
        //        NewUrl = newUrl
        //    },
        //     JsonRequestBehavior.AllowGet);
        //}






            [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> Payment(string ApplicationId)
        {
            String newUrl = null;
            string message = "success";
            string status = "";
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority) + Request.ApplicationPath;

            try
            {
                log.Info("ApplicationID =>" + ApplicationId);
                log.Info("BaseURL =>" + baseUrl);

                using (var dbCtxt = new LubeBlendingDBEntities())
                {
                    ExtraPayment extraappid = dbCtxt.ExtraPayments.Where(c => c.ExtraPaymentAppRef.Trim() == ApplicationId.Trim()).FirstOrDefault();
                    var Appreqid = (from e in dbCtxt.ExtraPayments where e.ExtraPaymentAppRef == ApplicationId select e.ApplicationID).FirstOrDefault();

                    ApplicationRequest appreq = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault() == null ? dbCtxt.ApplicationRequests.Where(c => c.ApplicationId == Appreqid).FirstOrDefault() : dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();

                   
                    if (ApplicationId == null || ApplicationId == "")
                    {
                        status = "failure";
                        message = "Application ID with Reference " + ApplicationId + " Cannot be retrievd from the Database";
                        log.Error(message);
                    }
                    else
                    {
                        String result = commonHelper.GeneratePaymentReference(serviceIntegrator, ApplicationId, userMaster, baseUrl);
                        log.Info("Response from GeneratePaymentRRR =>" + result);
                        var rrrCheck = GlobalModel.elpsUrl + "/Payment/checkifpaid?id=r" + result;
                        status = result.Contains("InternalServerError") ? "InternalServerError" : "success";
                        var res = serviceIntegrator.CheckRRR(result);

                        var resp = JsonConvert.DeserializeObject<JObject>(res.value.ToString());
                        var expaystatus = extraappid == null ? "empty" : extraappid.Status;
                        var appLicenseTypeId = appreq == null ? "empty" : appreq.LicenseTypeId;

                        if (extraappid != null && (expaystatus == "Pending" || expaystatus == "FAIL"))
                        {
                            if ((appLicenseTypeId == "LTO" || appLicenseTypeId == "PTE" || appLicenseTypeId == "ATC") && (extraappid.SanctionType == "INCOMPLETE FEE"))
                            {
                                log.Info("Remita Payment");
                            }
                            else
                            {
                                var awaitresult = await serviceIntegrator.IGRPaymentPost(result, ApplicationId);
                                log.Info("IGR_PARTNER Payment");

                            }

                        }
                        else
                        {
                            if (appreq.LicenseTypeId == "ATO" || appreq.LicenseTypeId == "ATM" || appreq.LicenseTypeId == "SSA" || appreq.LicenseTypeId == "TITA" || appreq.LicenseTypeId == "TCA" || appreq.LicenseTypeId.Contains("TPBA"))
                            {
                                var awaitresult = await serviceIntegrator.IGRPaymentPost(result, ApplicationId);
                            }

                        }

                        if (resp.GetValue("message").ToString() == "Approved" || resp.GetValue("message").ToString() == "Successful" || resp.GetValue("status").ToString() == "01")
                        {
                            return RedirectToAction("PaymentSuccess", "Company", new { orderId = resp.GetValue("orderId").ToString() });
                        }
                        else
                        {
                            newUrl = GlobalModel.elpsUrl + "/Payment/Pay?rrr=" + result;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                status = "failure";
                message = "System Error Occured, Please try again Later";
                log.Error(ex.StackTrace, ex);
            }

            return Json(new
            {
                Status = status,
                Message = message,
                NewUrl = newUrl
            },
             JsonRequestBehavior.AllowGet);
        }




        //public async Task<ActionResult> IGRPost(string rrr, string appid)
        //{



        //    int uniqueid = 0; decimal amount = 0;
            
        //    CompanyAddressDTO compDetailsModel = new CompanyAddressDTO();

            
        //    ElpsResponse elpsResponse = serviceIntegrator.GetCompanyDetailByEmail(userMaster.UserId);
        //    CompanyDetail companyDetail = (CompanyDetail)elpsResponse.value;
        //    ElpsResponse elpsResponseaddress = serviceIntegrator.GetAddressByID(Convert.ToString(companyDetail.registered_Address_Id));
        //    compDetailsModel = (CompanyAddressDTO)elpsResponseaddress.value;
        //    var location = compDetailsModel.city != null ? compDetailsModel.city : "Abia";
        //    var Address = compDetailsModel.address_1 != null ? compDetailsModel.address_1 : "Default Address";
        //    IGRResponse igrresponse = null;
        //    ExtraPayment extraappid = dbCtxt.ExtraPayments.Where(c => c.ExtraPaymentAppRef.Trim() == appid.Trim()).FirstOrDefault();
        //    var expaystatus = extraappid == null ? "empty" : extraappid.Status;

        //    if (extraappid != null && (expaystatus == "Pending" || expaystatus == "FAIL"))
        //    {
        //        uniqueid = Convert.ToInt32(extraappid.PenaltyCode);
        //        amount = Convert.ToDecimal(extraappid.TxnAmount) * Convert.ToDecimal(1.05);
        //    }



        //    var values = new JObject();

        //    var revenueItems = new List<RevenueItemViewModel>
        //    {
        //        new RevenueItemViewModel { RevenueItemId = uniqueid, Amount = amount, Quantity = Convert.ToInt32(extraappid.Qty) },
        //    };
        //    var revItems = JsonConvert.SerializeObject(revenueItems);
        //    Debug.WriteLine(revItems);

        //    values.Add("RevenueItems", revItems);
        //    values.Add("Quantity", 1);
        //    values.Add("RRR", rrr);
        //    values.Add("ExternalPaymentReference", appid);
        //    values.Add("State", location);
        //    values.Add("Address", Address);
        //    values.Add("CompanyName", companyDetail.name);
        //    values.Add("Phone", companyDetail.contact_Phone);
        //    values.Add("CompanyEmail", userMaster.UserId);
        //    string jsonResponse = await ElpServiceHelper.postExternalTest("addpayments/", values);

        //    igrresponse = JsonConvert.DeserializeObject<IGRResponse>(jsonResponse);

        //    return Json(igrresponse.Status);

        //}




            public ActionResult PaymentReceipt(string ApplicationId)
        {
            log.Info("Coming into PaymentReceipt with ApplicationId => " + ApplicationId);

            using (var dbCtxt = new LubeBlendingDBEntities())
            {
                PaymentLog paymentLog = dbCtxt.PaymentLogs.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();
                ExtraPayment extrapayment = dbCtxt.ExtraPayments.Where(e => e.ExtraPaymentAppRef == ApplicationId).FirstOrDefault();
                if (paymentLog != null)

                {
                    decimal totalamt = Convert.ToDecimal(paymentLog.TxnAmount);
                    ViewBag.TotalAmount = "₦" + totalamt.ToString("N");
                }
                if (extrapayment != null)

                {
                    decimal totalamt = Convert.ToDecimal(extrapayment.TxnAmount);
                    ViewBag.TotalAmount = "₦" + totalamt.ToString("N");
                }
                ViewBag.PaymentLog = paymentLog;
                ViewBag.Extrapayment = extrapayment;


            }

            return View();
        }

        public JsonResult GetAppointmentDetail(string ApplicationId)
        {
            log.Info("Coming into GetAppointmentDetail");
            string LicenseDesc = null, AppointDate = null, AppointVenue = null, ContactDetails = " ", AppointType = null, InspectionType;

            try
            {
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    return Json(new
                    {
                        Status = "failure",
                        Message = "Application ID with Reference " + ApplicationId + " Cannot be retrievd from the Database"
                    }, JsonRequestBehavior.AllowGet);
                }

                LicenseDesc = appRequest.LicenseType.Description + " (" + ApplicationId + ")";

                log.Info("About To get Appointment");
                Appointment appointment = dbCtxt.Appointments.Where(a => a.ApplicationId == ApplicationId.Trim()).ToList().Last();
                AppointDate = appointment.AppointmentDate.GetValueOrDefault().ToString("yyyy-MMMM-dd HH:mm");
                AppointVenue = appointment.AppointmentVenue;
                AppointType = appointment.TypeOfAppoinment;
                InspectionType = appointment.InspectionTypeId;
                ElpsResponse elpsResponse = serviceIntegrator.GetCompanyDetailByEmail(userMaster.UserId);
                if (elpsResponse.message != "SUCCESS")
                {
                    return Json(new
                    {
                        Status = "success",
                        LicenseDesc = LicenseDesc,
                        AppointDate = AppointDate,
                        AppointVenue = AppointVenue,
                        ContactDetails = ContactDetails,
                        AppointType = AppointType,
                        InspectionType = InspectionType
                    },
                     JsonRequestBehavior.AllowGet);
                }


                CompanyDetail companyDetail = (CompanyDetail)elpsResponse.value;
                if (companyDetail ==
                 default(CompanyDetail))
                {
                    return Json(new
                    {
                        Status = "success",
                        LicenseDesc = LicenseDesc,
                        AppointDate = AppointDate,
                        AppointVenue = AppointVenue,
                        ContactDetails = ContactDetails,
                        AppointType = AppointType,
                        InspectionType = InspectionType
                    },
                     JsonRequestBehavior.AllowGet);
                }

                if (!string.IsNullOrEmpty(companyDetail.contact_LastName))
                {
                    ContactDetails = ContactDetails + companyDetail.contact_LastName + " ";
                }

                if (!string.IsNullOrEmpty(companyDetail.contact_FirstName))
                {
                    ContactDetails = ContactDetails + companyDetail.contact_FirstName + " ";
                }

                if (!string.IsNullOrEmpty(companyDetail.contact_Phone))
                {
                    ContactDetails = ContactDetails + ",PHONE: " + companyDetail.contact_Phone + " ";
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return Json(new
                {
                    Status = "failure",
                    Message = "Error occured getting Appointment Details"
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                Status = "success",
                LicenseDesc = LicenseDesc,
                AppointDate = AppointDate,
                AppointVenue = AppointVenue,
                ContactDetails = ContactDetails,
                AppointType = AppointType,
                InspectionType = InspectionType
            },
             JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult ConfirmAppointment(string ApplicationId, string AppointmentType, string UserAction, string ContactDetail, string Comment)
        {
            string response = string.Empty;
            string autoComment = string.Empty;

            log.Info("About to confirm Appointment with Details => " + ApplicationId + "," + AppointmentType + "," + UserAction + "," + ContactDetail);

            try
            {

                var ActionSchedule = (from a in dbCtxt.ActionHistories where (a.Action == "ScheduleInspectionfdr" || a.Action == "ScheduleInspectionfds" || a.Action == "ScheduleInspectionhqr" || a.Action == "ScheduleInspectionhqs" || a.Action == "ScheduleInspectionznr" || a.Action == "ScheduleInspectionzns") && (a.ApplicationId == ApplicationId) select a).ToList().LastOrDefault();




                Appointment appointment = dbCtxt.Appointments.Where(a => a.ApplicationId == ApplicationId.Trim()).ToList().Last();
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(a => a.ApplicationId == ApplicationId.Trim()).FirstOrDefault();


                if (AppointmentType == "INSPECTION" && ActionSchedule != null)
                {
                    if (UserAction == "Accept")
                    {
                        if (ActionSchedule.Action == "ScheduleInspectionfdr")
                        {
                            UserAction = "AcceptInspectionfdr";
                        }
                        else if (ActionSchedule.Action == "ScheduleInspectionfds")
                        {
                            UserAction = "AcceptInspectionfds";

                        }
                        else if (ActionSchedule.Action == "ScheduleInspectionhqr")
                        {
                            UserAction = "AcceptInspectionhqr";

                        }
                        else if (ActionSchedule.Action == "ScheduleInspectionhqs")
                        {
                            UserAction = "AcceptInspectionhqs";

                        }
                        else if (ActionSchedule.Action == "ScheduleInspectionznr")
                        {
                            UserAction = "AcceptInspectionznr";

                        }
                        else if (ActionSchedule.Action == "ScheduleInspectionzns")
                        {
                            UserAction = "AcceptInspectionzns";

                        }

                        autoComment = "Inspection Appointment For " + appointment.ScheduledDate + " Have been Confirmed by The Company (" + Comment + ")";
                    }
                    else
                    {
                        if (ActionSchedule.Action == "ScheduleInspectionfdr")
                        {
                            UserAction = "RejectInspectionfdr";
                        }
                        else if (ActionSchedule.Action == "ScheduleInspectionfds")
                        {
                            UserAction = "RejectInspectionfds";

                        }
                        else if (ActionSchedule.Action == "ScheduleInspectionhqr")
                        {
                            UserAction = "RejectInspectionhqr";

                        }
                        else if (ActionSchedule.Action == "ScheduleInspectionhqs")
                        {
                            UserAction = "RejectInspectionhqs";

                        }
                        else if (ActionSchedule.Action == "ScheduleInspectionznr")
                        {
                            UserAction = "AcceptInspectionznr";

                        }
                        else if (ActionSchedule.Action == "ScheduleInspectionzns")
                        {
                            UserAction = "AcceptInspectionzns";

                        }


                        autoComment = "Inspection Appointment For " + appointment.ScheduledDate + " Rejected By Company (" + Comment + ")";

                    }
                }
                else
                {
                    if (UserAction == "Accept")
                    {

                        if (UserAction == "Accept")
                        {
                            if (ActionSchedule.Action == "ScheduleInspectionfdr")
                            {
                                UserAction = "AcceptInspectionfdr";
                            }
                            else if (ActionSchedule.Action == "ScheduleInspectionfds")
                            {
                                UserAction = "AcceptInspectionfds";

                            }
                            else if (ActionSchedule.Action == "ScheduleInspectionhqr")
                            {
                                UserAction = "AcceptInspectionhqr";

                            }
                            else if (ActionSchedule.Action == "ScheduleInspectionhqs")
                            {
                                UserAction = "AcceptInspectionhqs";

                            }
                            else if (ActionSchedule.Action == "ScheduleInspectionznr")
                            {
                                UserAction = "AcceptInspectionznr";

                            }
                            else if (ActionSchedule.Action == "ScheduleInspectionzns")
                            {
                                UserAction = "AcceptInspectionzns";

                            }


                            autoComment = "Meeting Appointment For " + appointment.ScheduledDate + " Have been Confirmed by The Company (" + Comment + ")";
                        }
                        else
                        {


                            if (ActionSchedule.Action == "ScheduleInspectionfdr")
                            {
                                UserAction = "RejectInspectionfdr";
                            }
                            else if (ActionSchedule.Action == "ScheduleInspectionfds")
                            {
                                UserAction = "RejectInspectionfds";

                            }
                            else if (ActionSchedule.Action == "ScheduleInspectionhqr")
                            {
                                UserAction = "RejectInspectionhqr";

                            }
                            else if (ActionSchedule.Action == "ScheduleInspectionhqs")
                            {
                                UserAction = "RejectInspectionhqs";

                            }
                            else if (ActionSchedule.Action == "ScheduleInspectionznr")
                            {
                                UserAction = "AcceptInspectionznr";

                            }
                            else if (ActionSchedule.Action == "ScheduleInspectionzns")
                            {
                                UserAction = "AcceptInspectionzns";

                            }


                            autoComment = "Meeting Appointment For " + appointment.ScheduledDate + " Rejected By Company (" + Comment + ")";
                        }
                    }
                }

                ResponseWrapper responseWrapper = workflowHelper.processAction(dbCtxt, ApplicationId, UserAction, userMaster.UserId, autoComment, appRequest.CurrentOfficeLocation, "");


                if (!responseWrapper.status)
                {
                    response = responseWrapper.value;
                    log.Error(response);
                    return Json(new
                    {
                        status = "failure",
                        Message = response
                    },
                     JsonRequestBehavior.AllowGet);
                }
                else
                {
                    appointment.LastApprovedCustDate = DateTime.Now;
                    appointment.LastCustComment = Comment;
                    appointment.ContactPerson = ContactDetail;
                    appointment.Status = (UserAction == "AcceptAppointment") ? "AUTH" : "REJECT";
                    dbCtxt.SaveChanges();
                }




            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                return Json(new
                {
                    status = "failure",
                    Message = "An Exception occur during Confirmation of Appoinment, Please try again Later"
                },
                 JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                status = "success",
                Message = autoComment
            },
             JsonRequestBehavior.AllowGet);
        }




        // GET: /Company/GetLga
        [HttpGet]
        public JsonResult GetLga(string stateCode)
        {
            List<LgaData> dataList = new List<LgaData>();
            try
            {
                foreach (LgaMasterList lm in dbCtxt.LgaMasterLists.Where(c => c.StateCode == stateCode.Trim()).ToList())
                {
                    StateMasterList statemasterlist = dbCtxt.StateMasterLists.Where(s => s.StateCode.Trim() == stateCode.Trim()).FirstOrDefault();

                    if (statemasterlist !=
                     default(StateMasterList))
                    {
                        dataList.Add(new LgaData()
                        {
                            lga_code = lm.LgaCode,
                            lga_name = lm.LgaName,
                            latitude = statemasterlist.Latitude,
                            longitude = statemasterlist.Longitude
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

            return Json(dataList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMessageDetail(string ApplicationId = null, string MessageId = null)
        {
            string LicenseDesc = null, Message = null, MessageDate = null;
            log.Info("ApplicationId =>" + ApplicationId);
            log.Info("MessageId =>" + MessageId);
            try
            {

                if (!string.IsNullOrEmpty(ApplicationId) && !string.IsNullOrEmpty(MessageId))
                {
                    long id = Convert.ToInt64(MessageId);
                    ActionHistory actionHistory = dbCtxt.ActionHistories.Where(n => n.ApplicationId == ApplicationId.Trim() && n.ActionId == id).FirstOrDefault();

                    LicenseDesc = dbCtxt.LicenseTypes.Where(l => l.LicenseTypeId == actionHistory.LicenseTypeId).FirstOrDefault().Description;
                    Message = actionHistory.MESSAGE;
                    MessageDate = actionHistory.ActionDate.Value.ToString();
                    log.Info("Notification Message retrived");
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return Json(new
                {
                    Status = "failure",
                    Message = "Error occured trying to get Notification details"
                }, JsonRequestBehavior.AllowGet);
            }

            log.Info("About to return Notification Message retrived");
            return Json(new
            {
                Status = "success",
                LicenseDesc = LicenseDesc + " (" + ApplicationId + ")",
                MessageDate = MessageDate,
                ApplicationId = ApplicationId,
                Message = Message
            },
             JsonRequestBehavior.AllowGet);
        }




        // GET: /Company/GetLicenseToRenew
        [HttpGet]
        public JsonResult GetLicenseToRenew(string LicenseType)
        {
            List<string> expiringLicenseList = new List<string>();
            try
            {
                log.Info("License Type =>" + LicenseType);
                foreach (ApplicationRequest appRequest in dbCtxt.ApplicationRequests.Where(c => c.ApplicantUserId == userMaster.UserId && c.LicenseTypeId == LicenseType && c.LicenseReference != null && c.LinkedReference == null).ToList())
                {

                    string stateType = dbCtxt.WorkFlowStates.Where(w => w.StateID == appRequest.CurrentStageID).FirstOrDefault().StateType;
                    if (stateType !=
                     default(string) && stateType == "COMPLETE")
                    {
                        expiringLicenseList.Add(appRequest.LicenseReference);
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
            return Json(expiringLicenseList, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public JsonResult KeepSessionAlive()
        {
            log.Info("ping");
            return new JsonResult
            {
                Data = "Success"
            };
        }

        [HttpGet]
        public JsonResult CheckForUnsubmittedApplication(string LicenseTypeCode)
        {
            int appcount = 0;
            string status = "success";
            string message = string.Empty;
            string appId = string.Empty;
            List<ApplicationRequest> checkexpiry = null;

            try
            {
                log.Info("LicenseType =>" + LicenseTypeCode + ", UserID => " + userMaster.UserId);

                foreach (ApplicationRequest b in dbCtxt.ApplicationRequests.Where(c => (c.ApplicantUserId.Trim() == userMaster.UserId.Trim()) && c.CurrentStageID < 4 && c.LicenseTypeId.Trim() == LicenseTypeCode.Trim() && string.IsNullOrEmpty(c.LicenseReference)).ToList())
                {
                    if (b.WorkFlowState.StateType == "CONTINUE")
                    {
                        appId = b.ApplicationId;
                        appcount = appcount + 1;
                        log.Info("ApplicationID =>" + appId + ", AppCount =>" + appcount);
                        break;
                    }
                }

                if (LicenseTypeCode == "ATC")
                {
                    checkexpiry = (from a in dbCtxt.ApplicationRequests where a.LicenseTypeId == "PTE" && a.LicenseExpiryDate != null && a.ApplicantUserId == userMaster.UserId select a).ToList();
                    if (checkexpiry.Count > 0)
                    {
                        foreach (var item in checkexpiry)
                        {
                            if (item.LicenseExpiryDate < DateTime.Now)
                            {
                                status = "expiredpte";
                            }
                            else
                            {
                                status = "success";
                                break;
                            }
                        }
                    }

                }
                else if (LicenseTypeCode == "PTE")
                {
                    checkexpiry = (from a in dbCtxt.ApplicationRequests where a.LicenseTypeId == "SSA" && a.LicenseExpiryDate != null && a.ApplicantUserId == userMaster.UserId select a).ToList();

                    if (checkexpiry.Count > 0)
                    {
                        foreach (var item in checkexpiry)
                        {
                            if (item.LicenseExpiryDate < DateTime.Now)
                            {
                                status = "expiredssa";
                            }
                            else
                            {
                                status = "success";
                                break;
                            }
                        }
                    }

                    
                }
                else if (LicenseTypeCode == "LTO")
                {
                    checkexpiry = (from a in dbCtxt.ApplicationRequests where a.LicenseTypeId == "ATC" && a.LicenseExpiryDate != null && a.ApplicantUserId == userMaster.UserId select a).ToList();
                    if (checkexpiry.Count > 0)
                    {
                        foreach (var item in checkexpiry)
                        {
                            if (item.LicenseExpiryDate < DateTime.Now)
                            {
                                status = "expiredatc";
                            }
                            else
                            {
                                status = "success";
                                break;
                            }
                        }
                    }

                    
                }

                else if (LicenseTypeCode == "LTOLFP")
                {
                    checkexpiry = (from a in dbCtxt.ApplicationRequests where a.LicenseTypeId == "ATCLFP" && a.LicenseExpiryDate != null && a.ApplicantUserId == userMaster.UserId select a).ToList();

                    if (checkexpiry.Count > 0)
                    {
                        foreach (var item in checkexpiry)
                        {
                            if (item.LicenseExpiryDate < DateTime.Now)
                            {
                                status = "expiredatclfp";
                            }
                            else
                            {
                                status = "success";
                                break;
                            }
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                status = "failure";
                message = "Error Occured When Trying To Check If UnSubmited Applications";
            }

            return Json(new
            {
                Status = status,
                AppCount = appcount,
                Message = message
            },
             JsonRequestBehavior.AllowGet);
        }




        [HttpGet]
        public ActionResult LegacyApplication(string LicenseTypeId = null, string ReferenceId = null)
        {
            String selectedStateCode = null;
            LubePlantModel lubePlantModel = new LubePlantModel();
            List<SelectListItem> StateList = new List<SelectListItem>();
            List<SelectListItem> LgaList = new List<SelectListItem>();
            //LubePlantModel model = new LubePlantModel();
            //DocumentUploadModel model1 = new DocumentUploadModel();
            try
            {

                ViewBag.ResponseMessage = "SUCCESS";
                lubePlantModel.LicenseTypeId = LicenseTypeId;
                lubePlantModel.ApplicationId = ReferenceId;
                ViewBag.LegacyName = dbCtxt.LicenseTypes.Where(l => l.LicenseTypeId == LicenseTypeId).FirstOrDefault().Description;

                log.Info("About to get GetCompanyDocumentListById => " + userMaster.ElpsId);
                ElpsResponse elpsResponse = serviceIntegrator.GetCompanyDocumentListById(userMaster.ElpsId);
                if (elpsResponse.message != "SUCCESS")
                {
                    log.Error(elpsResponse.message);
                    ViewBag.ResponseMessage = elpsResponse.message;
                    return View(lubePlantModel);
                }

                log.Info("About to Cast DocumentList Fetched From Elps");
                List<Document> ElpsDocumenList = (List<Document>)elpsResponse.value;
                log.Info("ElpsDocument Size =>" + ElpsDocumenList.Count);


                var facilityelpsid = (from f in dbCtxt.Facilities
                                      join a in dbCtxt.ApplicationRequests on f.LocationAddress equals a.SiteLocationAddress
                                      where a.ApplicationId == ReferenceId && a.FacilityId == f.FacilityId
                                      select f.ElpsFacilityId).FirstOrDefault();

                ViewBag.Elpsfacid = facilityelpsid;

                ElpsResponse facilityelpsResponse = serviceIntegrator.GetFacilityDocumentListById(facilityelpsid.ToString());
                List<FacilityDocument> ElpsFacDocumenList = (List<FacilityDocument>)facilityelpsResponse.value;


                ElpsResponse allfacilityelpsResponse = serviceIntegrator.GetAllFacilityDocumentListById();
                List<FacilityDocument> AllElpsFacDocumenList = (List<FacilityDocument>)allfacilityelpsResponse.value;// use if facility app id equals null




                List<LegacyDocument> LegacyDocList = null;
                switch (LicenseTypeId)
                {
                    case "ATC": LegacyDocList = (from l in dbCtxt.LegacyDocuments where l.LicenseTypeCode == "ATC" select l).ToList(); break;
                    case "LTO": LegacyDocList = (from l in dbCtxt.LegacyDocuments where l.LicenseTypeCode == "LTO" select l).ToList(); break;
                    case "SSA": LegacyDocList = (from l in dbCtxt.LegacyDocuments where l.LicenseTypeCode == "SSA" select l).ToList(); break;
                    case "PTE": LegacyDocList = (from l in dbCtxt.LegacyDocuments where l.LicenseTypeCode == "PTE" select l).ToList(); break;
                    case "ATCLFP": LegacyDocList = (from l in dbCtxt.LegacyDocuments where l.LicenseTypeCode == "ATCLFP" select l).ToList(); break;

                }
                log.Info("AdditionalDocumentApplicationType Size =>" + LegacyDocList.Count);
                lubePlantModel.ApplicationId = ReferenceId;
                ViewBag.ApplicationId = ReferenceId;
                ElpsResponse AllDocselpsResponse = serviceIntegrator.GetAllDocumentType();
                List<AllDocumentTypes> AllDocElpsDocumenList = (List<AllDocumentTypes>)AllDocselpsResponse.value;
                List<RequiredLicenseDocument> AllCompanyDocumentList = dbCtxt.RequiredLicenseDocuments.Where(c => c.ApplicationTypeId.Trim() == "NEW" && c.LicenseTypeId == LicenseTypeId && c.IsBaseTran == "B" && c.Status.Equals("ACTIVE")).ToList(); /*applicationRequest.LicenseTypeCode*/
                List<RequiredLicenseDocument> AllFacilityDocList = dbCtxt.RequiredLicenseDocuments.Where(c => c.ApplicationTypeId.Trim() == "NEW" && c.LicenseTypeId == LicenseTypeId && c.IsBaseTran == "T" && c.Status.Equals("ACTIVE")).ToList();
                List<RequiredLicenseDocument> CompareDocList = dbCtxt.RequiredLicenseDocuments.Where(c => c.ApplicationTypeId.Trim() == "NEW" && c.LicenseTypeId == LicenseTypeId && c.Status.Equals("ACTIVE")).ToList();
                List<MissingDocument> missindoc = (from m in dbCtxt.MissingDocuments where m.ApplicationID == ReferenceId select m).ToList();


                lubePlantModel.CompanyFacilityMissingDocumentList1 = appDocHelper.CompanyFacilityMissingDocuments(AllDocElpsDocumenList, AllElpsFacDocumenList, AllCompanyDocumentList, AllFacilityDocList, CompareDocList);

                lubePlantModel.AdditionalDocumentList1 = appDocHelper.MissingDocuments(missindoc, AllDocElpsDocumenList, CompareDocList, ReferenceId, GlobalModel.elpsUrl);


                if (facilityelpsid != null)
                {
                    lubePlantModel.FacilityDocumentList1 = appDocHelper.GetLegacyDocuments(LegacyDocList, ElpsFacDocumenList, ReferenceId, GlobalModel.elpsUrl);
                }
                else
                {
                    lubePlantModel.FacilityDocumentList1 = appDocHelper.GetLegacyDocuments(LegacyDocList, AllElpsFacDocumenList, ReferenceId, GlobalModel.elpsUrl);
                }

                lubePlantModel.ElpsId = userMaster.ElpsId;
                lubePlantModel.ApplicationHash = userMasterHelper.GenerateSHA512(GlobalModel.appEmail + GlobalModel.appKey);
                lubePlantModel.Email = GlobalModel.appEmail;
                lubePlantModel.ElpsUrl = GlobalModel.elpsUrl;





                ElpsResponse addresselpsResponse = serviceIntegrator.GetCompanyAddressList(userMaster.ElpsId);
                if (addresselpsResponse.message != "SUCCESS")
                {
                    log.Warn("GetCompanyAddressList => " + addresselpsResponse.message);
                }
                else
                {
                    List<CompanyAddressDTO> addressList = (List<CompanyAddressDTO>)addresselpsResponse.value;
                    log.Info("Address List Count => " + addressList.Count);
                    if (addressList.Count > 0)
                    {
                        log.Info("Address Name => " + addressList[0].address_1 + " " + addressList[0].address_2);
                        lubePlantModel.RegisteredAddress = addressList[0].address_1 + " " + addressList[0].address_2;
                    }
                }

                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ReferenceId).FirstOrDefault();
                if (appRequest !=
                 default(ApplicationRequest))
                {

                    foreach (StateMasterList r in dbCtxt.StateMasterLists.ToList().OrderBy(s => s.StateName))
                    {
                        if (r.StateCode == appRequest.StateCode)
                        {
                            selectedStateCode = r.StateCode;
                            StateList.Add(new SelectListItem
                            {
                                Text = r.StateName,
                                Value = r.StateCode,
                                Selected = true
                            });
                        }
                        else
                            StateList.Add(new SelectListItem
                            {
                                Text = r.StateName,
                                Value = r.StateCode
                            });
                    }
                    ViewBag.State = StateList;


                    foreach (LgaMasterList r in dbCtxt.LgaMasterLists.Where(l => l.StateCode == selectedStateCode).ToList().OrderBy(l => l.LgaName))
                    {
                        if (r.LgaCode == appRequest.LgaCode)
                        {
                            LgaList.Add(new SelectListItem
                            {
                                Text = r.LgaName,
                                Value = r.LgaCode,
                                Selected = true
                            });
                        }
                        else
                            LgaList.Add(new SelectListItem
                            {
                                Text = r.LgaName,
                                Value = r.LgaCode
                            });
                    }
                    ViewBag.LGA = LgaList;


                    lubePlantModel.LocationAddress = appRequest.SiteLocationAddress;
                    lubePlantModel.AnnualCumulativeBaseOilRequirementsInLitres = appRequest.AnnualCumuBaseOilRequirementCapacity;
                    //lubePlantModel.AnnualProductsProductionProjections = appRequest.AnnualProductionProjectionCapacity;
                    lubePlantModel.LandSize = appRequest.LandSize;
                    lubePlantModel.IssuedDate = appRequest.LicenseIssuedDate.Value.ToString("dd/MM/yyyy");
                    lubePlantModel.ExpiryDate = appRequest.LicenseExpiryDate.Value.ToString("dd/MM/yyyy");
                    lubePlantModel.ExpiredReference = appRequest.LicenseReference;
                    lubePlantModel.GPS = appRequest.GPSCordinates;
                    lubePlantModel.StorageCapacity = appRequest.StorageCapacity;
                    lubePlantModel.AdditionalInformation = appRequest.AdditionalInfo;
                    lubePlantModel.ApplicationCategory = appRequest.ApplicationCategory;
                    lubePlantModel.FacilityName = (from f in dbCtxt.Facilities where f.FacilityId == appRequest.FacilityId select f.FalicityName).FirstOrDefault();
                   
                    if (!string.IsNullOrEmpty(appRequest.GPSCordinates))
                    {
                        string[] gps = appRequest.GPSCordinates.Split(',');
                        ViewBag.Latitude = gps[0];
                        ViewBag.Longitude = gps[1];
                    }


                }
                else
                {
                    ViewBag.State = new SelectList(dbCtxt.StateMasterLists.OrderBy(c => c.StateName), "StateCode", "StateName");
                    foreach (LgaMasterList r in dbCtxt.LgaMasterLists.Where(l => l.StateCode == "ABI").ToList().OrderBy(l => l.LgaName))
                    {
                        LgaList.Add(new SelectListItem
                        {
                            Text = r.LgaName,
                            Value = r.LgaCode
                        });
                    }
                    ViewBag.LGA = LgaList;

                    StateMasterList abia = dbCtxt.StateMasterLists.Where(s => s.StateCode == "ABI").FirstOrDefault();
                    ViewBag.Latitude = abia.Latitude;
                    ViewBag.Longitude = abia.Longitude;

                }


            }
            catch (Exception ex)
            {
                ViewBag.ResponseMessage = "An Error Occured Getting the Legacy Form, Please Try Again Later";
                log.Error(ex.Message);
            }

            return View(lubePlantModel);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult LegacyApplication(LubePlantModel model, FormCollection collect, List<SubmittedDocument> MyApplicationDocs)
        {

            String status = "success";
            String message = string.Empty;
            String comment = string.Empty;
            ElpsResponse wrapper = null;
            ResponseWrapper responseWrapper = null;
            ApplicationRequest appRequest;
            bool isNew = false;
            String LicenseTypeId = model.LicenseTypeId;
            String genApplicationId = model.ApplicationId;
            Facility facility = new Facility();
            //var facilityName = collect.Get("facilityname");
            var appcategory = collect.Get("appcategory");
            try
            {
                log.Info("Coming into The Function To Maintain Data with ID =>" + model.ApplicationId);
                appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == model.ApplicationId.Trim()).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    isNew = true;
                    appRequest = new ApplicationRequest();
                }

                log.Info("About to Set Application Master Fields");
                var appcat = model.ApplicationCategory == null ? appcategory : model.ApplicationCategory;
                appRequest.ApplicationId = genApplicationId;
                appRequest.LicenseTypeId = LicenseTypeId;
                appRequest.ApplicationTypeId = "NEW";
                appRequest.ApplicantUserId = userMaster.UserId;
                appRequest.CurrentAssignedUser = userMaster.UserId;
                appRequest.ApplicantName = userMaster.FirstName;
                appRequest.SiteLocationAddress = model.LocationAddress;
                appRequest.BeaconLocations = model.LocationAddress;
                appRequest.RegisteredAddress = model.RegisteredAddress;
                appRequest.StateCode = model.State;
                appRequest.LgaCode = model.LGA;
                appRequest.GPSCordinates = model.GPS;
                appRequest.LandSize = model.LandSize;
                appRequest.LicenseReference = model.ExpiredReference.Replace("/", "-");
                appRequest.AnnualCumuBaseOilRequirementCapacity = model.AnnualCumulativeBaseOilRequirementsInLitres.Replace(",", "");
                appRequest.StorageCapacity = model.AnnualCumulativeBaseOilRequirementsInLitres.Replace(",", "");
                appRequest.AdditionalInfo = model.AdditionalInformation;
                appRequest.Status = appRequest.Status == "Rejected" ? "Rejected" : "ACTIVE";
                appRequest.AddedDate = DateTime.UtcNow;
                appRequest.IsLegacy = "YES";
                appRequest.LicenseReference = model.ExpiredReference;
                appRequest.LicenseIssuedDate = Convert.ToDateTime(model.IssuedDate);
                appRequest.LicenseExpiryDate = Convert.ToDateTime(model.ExpiryDate);
                appRequest.CurrentStageID = 46;
                appRequest.CurrentOfficeLocation = dbCtxt.FieldLocations.Where(f => f.FieldType == "HQ").FirstOrDefault().FieldLocationID;
                if (LicenseTypeId != "ATCLFP")
                {
                    appRequest.ApplicationCategory = LicenseTypeId == "SSA" ? "Lube Oil Blending Plant" : appcat;

                }
                else
                {
                    appRequest.ApplicationCategory = "Lubricant Filling Plant";

                }
                var check_doc1 = dbCtxt.SubmittedDocuments.Where(x => x.ApplicationID == genApplicationId).ToList();

                if (MyApplicationDocs != null)
                {
                    foreach (var item in MyApplicationDocs)
                    {
                        var check_doc = dbCtxt.SubmittedDocuments.Where(x => x.ApplicationID == genApplicationId && x.DocId == item.DocId).ToList();

                        if (check_doc1.Count() == 0 || check_doc.Count() == 0)
                        {
                            SubmittedDocument submitDocs = new SubmittedDocument()
                            {
                                ApplicationID = item.ApplicationID,
                                DocId = item.DocId,
                                BaseorTrans = item.BaseorTrans,
                                DocName = item.DocName,
                                FileId = item.FileId,
                                DocSource = item.DocSource
                            };
                            dbCtxt.SubmittedDocuments.Add(submitDocs);
                        }

                        else if (check_doc1.Count() > 0 && check_doc1.Count() == MyApplicationDocs.Count())
                        {
                            check_doc.FirstOrDefault().ApplicationID = item.ApplicationID;
                            check_doc.FirstOrDefault().DocId = item.DocId;
                            check_doc.FirstOrDefault().BaseorTrans = item.BaseorTrans;
                            check_doc.FirstOrDefault().DocName = item.DocName;
                            check_doc.FirstOrDefault().FileId = item.FileId;
                            check_doc.FirstOrDefault().DocSource = item.DocSource;
                        }
                        else
                        {
                            foreach (var item1 in check_doc1)
                            {
                                dbCtxt.SubmittedDocuments.Remove(item1);
                            }
                            dbCtxt.SaveChanges();


                            foreach (var item2 in MyApplicationDocs)
                            {

                                SubmittedDocument submitDocs = new SubmittedDocument()
                                {
                                    ApplicationID = item2.ApplicationID,
                                    DocId = item2.DocId,
                                    BaseorTrans = item2.BaseorTrans,
                                    DocName = item2.DocName,
                                    FileId = item2.FileId,
                                    DocSource = item2.DocSource
                                };
                                dbCtxt.SubmittedDocuments.Add(submitDocs);
                            }
                            break;
                        }
                    }
                }



                var facility_count = dbCtxt.Facilities.Where(x => x.FalicityName == model.FacilityName && x.LocationAddress == model.LocationAddress && x.CompanyUserId == userMaster.UserId).ToList();

                if (facility_count.Count != 0)
                {
                    appRequest.FacilityId = facility_count.FirstOrDefault().FacilityId;
                }

                if (facility_count.Count() == 0)
                {

                    facility.CompanyUserId = userMaster.UserId;
                    facility.FalicityName = model.FacilityName;
                    facility.LocationAddress = model.LocationAddress;
                    dbCtxt.Facilities.Add(facility);
                    int done = dbCtxt.SaveChanges();


                    int facilityID = facility.FacilityId;
                    var statename = (from s in dbCtxt.StateMasterLists where s.StateCode == model.State select s.StateName).FirstOrDefault();
                    Facilities _facilities = new Facilities()
                    {
                        Name = model.FacilityName,
                        CompanyId = Convert.ToInt32(userMaster.ElpsId),
                        StreetAddress = model.LocationAddress,
                        City = statename,
                        FacilityType = "Lube Oil Blending Plant",
                        StateId = 1,//Convert.ToInt32(wrapper.value),
                        DateAdded = DateTime.Now,
                    };

                    wrapper = serviceIntegrator.PostFacility(_facilities);
                    if (wrapper.message == "SUCCESS")
                    {
                        Facilities facilityDetail = (Facilities)wrapper.value;

                        var updateFacility = dbCtxt.Facilities.Where(x => x.FacilityId == facilityID).FirstOrDefault();
                        updateFacility.ElpsFacilityId = facilityDetail.Id;
                        dbCtxt.SaveChanges();
                    }

                }

                var fid = (from f in dbCtxt.Facilities where f.CompanyUserId == userMaster.UserId select f.FacilityId).ToList().LastOrDefault();
                appRequest.FacilityId = fid;




                log.Info("About to Save ApplicationMaster");
                if (isNew == true)
                {
                    dbCtxt.ApplicationRequests.Add(appRequest);
                }
                dbCtxt.SaveChanges();

                comment = "ApplicationForm Completed";
                log.Info("About to get WorkFlow Navigation");

                var fieldlocation = dbCtxt.FieldLocations.Where(f => f.FieldType == "HQ").FirstOrDefault().FieldLocationID;

                if (appRequest != null)
                {
                    if (appRequest.Status == "Rejected")
                    {
                        responseWrapper = workflowHelper.processAction(dbCtxt, genApplicationId, "ReSubmit", userMaster.UserId, "Legacy Application Resubmitted for Reprocessing After Fix by Marketer", fieldlocation, "");

                    }
                    else
                    {
                        responseWrapper = workflowHelper.processAction(dbCtxt, genApplicationId, "LCompany", userMaster.UserId, "Legacy Document Uploads", fieldlocation, "");

                    }
                }
                else
                {
                    responseWrapper = workflowHelper.processAction(dbCtxt, genApplicationId, "LCompany", userMaster.UserId, "Legacy Document Uploads", fieldlocation, "");

                }


                log.Info("Done with WorkFlow Navigation");
                message = "Your application was successfully submitted.";
                if (!responseWrapper.status)
                {
                    message = responseWrapper.value;
                    log.Error(message);
                }


            }
            
            catch (Exception ex)
            {
                status = "failure";
                message = "System Error Occured When Maintaining Data, Please Try Again Later";
                log.Error(ex.StackTrace, ex);
            }

            return Json(new
            {
                Status = status,
                Message = message,
                applicationId = genApplicationId
            }, JsonRequestBehavior.AllowGet);

        }



        [HttpGet]
        public ActionResult PermitToEstablish(string LicenseTypeId = null, string ApplicationId = null, string appref = null)
        {
            String selectedStateCode = null;
            List<SelectListItem> StateList = new List<SelectListItem>();
            List<SelectListItem> LgaList = new List<SelectListItem>();
            List<SelectListItem> TopologyList = new List<SelectListItem>();
            List<SelectListItem> NatureofAreaList = new List<SelectListItem>();
            PermitToEstbalishModel permitToEstablishModel = new PermitToEstbalishModel();
            permitToEstablishModel.AppRef = appref;
            try
            {
                if (LicenseTypeId == "ATM") { ViewBag.ATMTOdescription = "Approval To Modify"; }
                else if (LicenseTypeId == "ATO") { ViewBag.ATMTOdescription = "Takeover"; }
                else if (LicenseTypeId.Contains("TPBA")) { ViewBag.TPBAdescription = "Third Party Blending Approval"; }
                else if (LicenseTypeId == "TITA") { ViewBag.TITAdescription = "Tank Integrity Test Approval"; }
                else if (LicenseTypeId == "TCA") { ViewBag.TCAdescription = "Tank Calibration Approval"; }
                else if (LicenseTypeId == "ATCLFP") { ViewBag.ATCLFPdescription = "Approval To Construct Lubricant Filling Plant"; }
                permitToEstablishModel.LicenseTypeId = LicenseTypeId;
                permitToEstablishModel.ApplicationId = ApplicationId;

                ElpsResponse addresselpsResponse = serviceIntegrator.GetCompanyAddressList(userMaster.ElpsId);
                if (addresselpsResponse.message != "SUCCESS")
                {
                    log.Warn("GetCompanyAddressList => " + addresselpsResponse.message);
                }
                else
                {
                    List<CompanyAddressDTO> addressList = (List<CompanyAddressDTO>)addresselpsResponse.value;
                    log.Info("Address List Count => " + addressList.Count);
                    if (addressList.Count > 0)
                    {
                        log.Info("Address Name => " + addressList[0].address_1 + " " + addressList[0].address_2);
                        permitToEstablishModel.RegisteredAddress = addressList[0].address_1 + " " + addressList[0].address_2;
                    }
                }

                if (!string.IsNullOrEmpty(ApplicationId))
                {

                    ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == ApplicationId.Trim()).FirstOrDefault();
                    if (appRequest !=
                     default(ApplicationRequest))
                    {

                        foreach (StateMasterList r in dbCtxt.StateMasterLists.ToList().OrderBy(s => s.StateName))
                        {
                            if (r.StateCode == appRequest.StateCode)
                            {
                                selectedStateCode = r.StateCode;
                                StateList.Add(new SelectListItem
                                {
                                    Text = r.StateName,
                                    Value = r.StateCode,
                                    Selected = true
                                });
                            }
                            else
                                StateList.Add(new SelectListItem
                                {
                                    Text = r.StateName,
                                    Value = r.StateCode
                                });
                        }
                        ViewBag.State = StateList;


                        foreach (LgaMasterList r in dbCtxt.LgaMasterLists.Where(l => l.StateCode == selectedStateCode).ToList().OrderBy(l => l.LgaName))
                        {
                            if (r.LgaCode == appRequest.LgaCode)
                            {
                                LgaList.Add(new SelectListItem
                                {
                                    Text = r.LgaName,
                                    Value = r.LgaCode,
                                    Selected = true
                                });
                            }
                            else
                                LgaList.Add(new SelectListItem
                                {
                                    Text = r.LgaName,
                                    Value = r.LgaCode
                                });
                        }
                        ViewBag.LGA = LgaList;


                        foreach (LandTopologyLookUp r in dbCtxt.LandTopologyLookUps.ToList())
                        {
                            if (r.Code == appRequest.LandTopology)
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
                            if (r.AreaCode == appRequest.NatureOfArea)
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

                        permitToEstablishModel.Status = appRequest.Status;
                        permitToEstablishModel.ApplicationId = ApplicationId;
                        permitToEstablishModel.SiteLocation = appRequest.SiteLocationAddress;
                        permitToEstablishModel.RegisteredAddress = appRequest.RegisteredAddress;
                        permitToEstablishModel.FacilityName = (from f in dbCtxt.Facilities where f.FacilityId == appRequest.FacilityId select f.FalicityName).FirstOrDefault();
                        permitToEstablishModel.GeneralFeatures = new GeneralFeatures();
                        permitToEstablishModel.GeneralFeatures.LandSize = appRequest.LandSize;
                        permitToEstablishModel.GeneralFeatures.BeaconLocations = appRequest.BeaconLocations;
                        permitToEstablishModel.GeneralFeatures.LandTopology = commonHelper.GetLandTopologyNames(appRequest.LandTopology);
                        permitToEstablishModel.GeneralFeatures.NatureOfArea = commonHelper.GetNatureOfAreaNames(appRequest.NatureOfArea);
                        permitToEstablishModel.GeneralFeatures.AdjoiningProperties = appRequest.AdjoiningProperties;
                        permitToEstablishModel.GeneralFeatures.AccessRoad = appRequest.AccessRoadToFromSite;
                        permitToEstablishModel.GeneralFeatures.ProposedPlantCapacity = appRequest.AnnualCumuBaseOilRequirementCapacity;
                        permitToEstablishModel.GeneralFeatures.PropAnnProd = appRequest.AnnualProductionProjectionCapacity;
                        permitToEstablishModel.GeneralFeatures.AdditionalInfo = appRequest.AdditionalInfo;
                        permitToEstablishModel.GeneralFeatures.GPS = appRequest.GPSCordinates;
                        permitToEstablishModel.ExistingFacilities = new ExistingFacilities();
                        permitToEstablishModel.ExistingFacilities.StructuresName = appRequest.AnyEquipmentOnSite;
                        permitToEstablishModel.ExistingFacilities.ROWName = appRequest.RelationshipWithPipelineRightOfWay;
                        permitToEstablishModel.ExistingFacilities.StreamName = appRequest.RelationshipWithStreams;
                        permitToEstablishModel.ExistingFacilities.PhcnName = appRequest.RelationshipWithPHCNTensionLines;
                        permitToEstablishModel.ExistingFacilities.RailwayName = appRequest.RelationshipWithRailwayLine;
                        permitToEstablishModel.ExistingFacilities.SensitiveInfo = appRequest.RelationshipWithSensitiveInstitutions;
                        permitToEstablishModel.ApplicationCategory = appRequest.ApplicationCategory;
                        permitToEstablishModel.ApprovalCategory = LicenseTypeId == null ? appRequest.LicenseTypeId : LicenseTypeId;
                        permitToEstablishModel.Quarter = appRequest.Quarter;
                        permitToEstablishModel.PLW_PRW_Name = appRequest.PLW_PRW_Name;
                        permitToEstablishModel.PLW_PRW_Address = appRequest.PLW_PRW_Address;
                        permitToEstablishModel.NumberOfTanks = (from a in dbCtxt.ApplicationRequests join t in dbCtxt.Tanks on a.ApplicationId equals t.ApplicationId select t.NbrOfTanks).FirstOrDefault();
                    }
                }
                else
                {

                    ViewBag.State = new SelectList(dbCtxt.StateMasterLists.OrderBy(c => c.StateName), "StateCode", "StateName");
                    ViewBag.NatureOfArea = new SelectList(dbCtxt.NatureOfAreaLookUps.Where(c => c.Status == "ACTIVE"), "AreaCode", "AreaName");
                    ViewBag.LandTopology = new SelectList(dbCtxt.LandTopologyLookUps.Where(c => c.Status == "ACTIVE"), "Code", "TopologyName");

                    foreach (LgaMasterList r in dbCtxt.LgaMasterLists.Where(l => l.StateCode == "ABI").ToList().OrderBy(l => l.LgaName))
                    {
                        LgaList.Add(new SelectListItem
                        {
                            Text = r.LgaName,
                            Value = r.LgaCode
                        });
                    }
                    ViewBag.LGA = LgaList;

                    StateMasterList abia = dbCtxt.StateMasterLists.Where(s => s.StateCode == "ABI").FirstOrDefault();
                    ViewBag.Latitude = abia.Latitude;
                    ViewBag.Longitude = abia.Longitude;
                }

                ViewBag.ResponseMessage = "SUCCESS";
            }
            catch (Exception ex)
            {
                ViewBag.ResponseMessage = "An Error Occured Getting the Application Form, Please Try Again Later";
                log.Error(ex.Message);
            }

            return View(permitToEstablishModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult PermitToEstablish(PermitToEstbalishModel model, FormCollection coll)
        {

            String status = "success";
            String message = string.Empty;
            String comment = string.Empty;
            string errorMessage = null;
            bool isNew = false;
            ApplicationRequest appRequest;
            String LicenseTypeId = model.LicenseTypeId;
            String genApplicationId = model.ApplicationId;
            ElpsResponse wrapper = null;
            Facility facility = new Facility();

            var takeoverlicensenumber = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == model.ApplicationId select a.LicenseReference).FirstOrDefault();

            if ((model.LicenseTypeId == "ATM" || model.LicenseTypeId == "ATO") && (model.Status != "Rejected"))
            {
                model.ApplicationId = null;
            }

            string appstatus = "";
            appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == model.ApplicationId.Trim()).FirstOrDefault();

            if (appRequest != null)
            {
                if (appRequest.Status == "Rejected")
                {
                    model.ApplicationId = appRequest.ApplicationId;
                    LicenseTypeId = appRequest.LicenseTypeId;
                    model.AppRef = appRequest.LicenseTypeId;
                    appstatus = "Rejected";
                }
                else
                {
                    //model.ApplicationId = commonHelper.GenerateApplicationNo();

                    appstatus = "ACTIVE";
                }
            }
            else
            {
                //model.ApplicationId = commonHelper.GenerateApplicationNo();
                appstatus = "ACTIVE";
            }


            try
            {
                log.Info("Coming into The Function To Maintain Data with ID =>" + model.ApplicationId);

                //if (!string.IsNullOrEmpty(model.ApplicationId))
                if (appRequest != null)

                {
                    appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == model.ApplicationId.Trim()).FirstOrDefault();
                }
                else
                {
                    isNew = true;
                    appRequest = new ApplicationRequest();
                    genApplicationId = commonHelper.GenerateApplicationNo();
                }

                log.Info("About to Set Application Master Fields");

                var Approvaltype = coll.Get("approvalcat");
                var Applicationtype = coll.Get("appcategory");
                var quarter = coll.Get("allquarter");
                var appcat = model.ApplicationCategory == null ? Applicationtype : model.ApplicationCategory;
                appRequest.ApplicationId = genApplicationId;
                appRequest.LinkedReference = takeoverlicensenumber;
                var licensecodeid = model.AppRef == null ? Approvaltype : model.AppRef;

                appRequest.LicenseTypeId = licensecodeid == null ? model.LicenseTypeId : licensecodeid;
                if (appRequest.LicenseTypeId == "SSA")
                {
                    appRequest.ApplicationCategory = "Lube Oil Blending Plant";
                }else if(appRequest.LicenseTypeId == "PTE")
                {
                    appRequest.ApplicationCategory = appcat;
                }
                else if (appRequest.LicenseTypeId.Contains("TPBA"))
                {
                    appRequest.ApplicationCategory = "Application For Third Party Blending";
                    appRequest.Quarter = Convert.ToInt32(quarter);
                    appRequest.PLW_PRW_Address = model.PLW_PRW_Address;
                    appRequest.PLW_PRW_Name = model.PLW_PRW_Name;
                }
               
                else if (appRequest.LicenseTypeId == "TITA")
                {
                    appRequest.ApplicationCategory = "Application For Tank Integrity Test Approval";
                }
                else if (appRequest.LicenseTypeId == "TCA")
                {
                    appRequest.ApplicationCategory = "Application For Tank Calibration Approval";
                }
                else if (appRequest.LicenseTypeId == "ATCLFP")
                {
                    appRequest.ApplicationCategory = "Lubricant Filling Plant";
                }
                
                appRequest.ApplicationTypeId = "NEW";
                appRequest.ApplicantUserId = userMaster.UserId;
                appRequest.StorageCapacity = model.GeneralFeatures.ProposedPlantCapacity;
                appRequest.CurrentAssignedUser = userMaster.UserId;
                appRequest.ApplicantName = userMaster.FirstName;
                appRequest.SiteLocationAddress = model.SiteLocation;
                appRequest.RegisteredAddress = model.RegisteredAddress;
                appRequest.StateCode = model.State;
                appRequest.LgaCode = model.LGA;
                appRequest.GPSCordinates = model.GeneralFeatures.GPS;
                appRequest.AnnualCumuBaseOilRequirementCapacity = model.GeneralFeatures.ProposedPlantCapacity;
                appRequest.AdditionalInfo = model.GeneralFeatures.AdditionalInfo;
                if (!model.LicenseTypeId.Contains("TPBA") && model.LicenseTypeId != "TITA" && model.LicenseTypeId != "TCA" && model.LicenseTypeId != "ATCLFP")
                {
                    appRequest.LandSize = model.GeneralFeatures.LandSize;
                    appRequest.BeaconLocations = model.GeneralFeatures.BeaconLocations;
                    appRequest.AccessRoadToFromSite = model.GeneralFeatures.AccessRoad;
                    appRequest.AnyEquipmentOnSite = model.ExistingFacilities.EquipmentOnSite;
                    appRequest.LandTopology = model.GeneralFeatures.LandTopology;
                    appRequest.NatureOfArea = model.GeneralFeatures.NatureOfArea;
                    appRequest.AnnualProductionProjectionCapacity = model.GeneralFeatures.PropAnnProd;
                    appRequest.AdjoiningProperties = model.GeneralFeatures.AdjoiningProperties;
                    appRequest.AnyEquipmentOnSite = model.ExistingFacilities.StructuresName;
                    appRequest.RelationshipWithPipelineRightOfWay = model.ExistingFacilities.ROWName;
                    appRequest.RelationshipWithSensitiveInstitutions = model.ExistingFacilities.SensitiveInfo;
                    appRequest.RelationshipWithStreams = model.ExistingFacilities.StreamName;
                    appRequest.RelationshipWithRailwayLine = model.ExistingFacilities.RailwayName;
                    appRequest.RelationshipWithPHCNTensionLines = model.ExistingFacilities.PhcnName;
                }
                appRequest.Status = appstatus;
                appRequest.IsLegacy = "NO";
                appRequest.PrintedStatus = "Not Printed";
                appRequest.AddedDate = DateTime.UtcNow;
                appRequest.CurrentOfficeLocation = dbCtxt.FieldLocations.Where(f => f.FieldType == "HQ").FirstOrDefault().FieldLocationID;



                var facility_count = model.LicenseTypeId == "ATO" ? dbCtxt.Facilities.Where(x => x.FalicityName == model.FacilityName && x.LocationAddress == model.SiteLocation).ToList() : dbCtxt.Facilities.Where(x => x.FalicityName == model.FacilityName && x.LocationAddress == model.SiteLocation && x.CompanyUserId == userMaster.UserId).ToList();
                int facilityid = 0;

                if (facility_count.Count != 0)
                {
                    appRequest.FacilityId = facility_count.FirstOrDefault().FacilityId;
                }

                if (facility_count.Count == 0)
                {

                    facility.CompanyUserId = userMaster.UserId;
                    facility.FalicityName = model.FacilityName;
                    facility.LocationAddress = model.SiteLocation;
                    dbCtxt.Facilities.Add(facility);
                    dbCtxt.SaveChanges();


                    facilityid = dbCtxt.Facilities.Where(x => x.FalicityName == model.FacilityName && x.LocationAddress == model.SiteLocation).FirstOrDefault().FacilityId;
                    var statename = (from s in dbCtxt.StateMasterLists where s.StateCode == model.State select s.StateName).FirstOrDefault();

                    Facilities _facilities = new Facilities()
                    {
                        Name = model.FacilityName,
                        CompanyId = Convert.ToInt32(userMaster.ElpsId),
                        StreetAddress = model.SiteLocation,
                        City = statename,
                        FacilityType = "Lube Oil Blending Plant",
                        StateId = 1,
                        DateAdded = DateTime.Now,
                    };

                    wrapper = serviceIntegrator.PostFacility(_facilities);
                    if (wrapper.message == "SUCCESS")
                    {
                        Facilities facilityDetail = (Facilities)wrapper.value;

                        var updateFacility = dbCtxt.Facilities.Where(x => x.FacilityId == facilityid).FirstOrDefault();
                        updateFacility.ElpsFacilityId = facilityDetail.Id;
                        dbCtxt.SaveChanges();
                    }
                    var fid = (from f in dbCtxt.Facilities where f.CompanyUserId == userMaster.UserId select f.FacilityId).ToList().LastOrDefault();
                    appRequest.FacilityId = fid;
                }







                comment = "ApplicationForm Completed";
                if (!commonHelper.isPaymentMade(genApplicationId, out errorMessage))
                {
                    appRequest.CurrentStageID = 1;
                }
                else
                {
                    comment = "ApplicationForm Modified";
                }

                log.Info("About to Save ApplicationMaster");
                if (isNew == true)
                {
                    dbCtxt.ApplicationRequests.Add(appRequest);
                }


                var checkATOExist = (from a in dbCtxt.ApplicationRequests join t in dbCtxt.TakeoverApps on a.ApplicationId equals t.ApplicationID where t.ApplicationID == model.ApplicationId select t).FirstOrDefault();
                var checkTankExist = (from a in dbCtxt.ApplicationRequests join t in dbCtxt.Tanks on a.ApplicationId equals t.ApplicationId where t.ApplicationId == model.ApplicationId select t).FirstOrDefault();
                TakeoverApp takeover = new TakeoverApp();
                Tank tnks = new Tank();

                if (model.LicenseTypeId == "ATO" && checkATOExist == null)
                {
                    takeover.ApplicationID = genApplicationId;
                    takeover.LicenseReference = takeoverlicensenumber;                   
                    dbCtxt.TakeoverApps.Add(takeover);
                }
                else if(model.LicenseTypeId == "ATO" && checkATOExist != null)
                {
                    takeover.ApplicationID = genApplicationId;
                    takeover.LicenseReference = takeoverlicensenumber;
                }



                if ((model.LicenseTypeId == "TITA" || model.LicenseTypeId == "TCA") && checkTankExist == null)
                {

                    tnks.ApplicationId = genApplicationId;
                    tnks.NbrOfTanks = model.NumberOfTanks;

                    dbCtxt.Tanks.Add(tnks);
                }
                else if ((model.LicenseTypeId == "TITA" || model.LicenseTypeId == "TCA") && checkTankExist != null)
                {
                    tnks.ApplicationId = genApplicationId;
                    tnks.NbrOfTanks = model.NumberOfTanks;
                }



                dbCtxt.SaveChanges();


                log.Info("About to get WorkFlow Navigation");
                if (!commonHelper.isPaymentMade(genApplicationId, out errorMessage))
                {
                    log.Info("About to get WorkFlow Navigation");

                    var fieldlocation = dbCtxt.FieldLocations.Where(f => f.FieldType == "HQ").FirstOrDefault().FieldLocationID;
                    ResponseWrapper responseWrapper = workflowHelper.processAction(dbCtxt, genApplicationId, "Proceed", userMaster.UserId, comment, fieldlocation, "");

                    log.Info("Done with WorkFlow Navigation");
                    message = responseWrapper.value;
                    if (!responseWrapper.status)
                    {
                        message = responseWrapper.value;
                        log.Error(message);
                    }
                }


                log.Info("Done with Permit to ESTABLISH");

            }
            catch (DbEntityValidationException ex)
            {
                string l = string.Empty;
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        l += String.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }

                status = "failure";
                message = "Database Validation Error, Please Try Again Later";
                log.Error(l);
            }
            catch (Exception ex)
            {
                status = "failure";
                message = "System Error Occured When Maintaining Data, Please Try Again Later";
                log.Error(ex.StackTrace, ex);
            }

            return Json(new
            {
                Status = status,
                Message = message,
                applicationId = genApplicationId
            }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult ApprovalToConstruct(string LicenseTypeId = null, string ApplicationId = null, string PTEReference = null, string appref = null)
        {
            String message = string.Empty;
            ApplicationRequest PTEAppRequest = null;
            LubePlantModel plantModel = new LubePlantModel();
            // DocumentUploadModel model = new DocumentUploadModel();
            plantModel.AppRef = appref;



            try
            {
                if (LicenseTypeId == "ATM") { ViewBag.ATMTOdescription = "Approval To Modify"; }
                else if (LicenseTypeId == "ATO") { ViewBag.ATMTOdescription = "Takeover"; }
                if (!string.IsNullOrEmpty(PTEReference))
                {


                    PTEAppRequest = dbCtxt.ApplicationRequests.Where(c => c.LicenseReference.Trim() == PTEReference.Trim()).FirstOrDefault();
                    if (PTEAppRequest ==
                     default(ApplicationRequest))
                    {
                        message = "The Application with PTE/SSA Permit NO " + PTEReference + " Cannot be retrievd from the Database";
                        log.Error(message);
                        ViewBag.ResponseMessage = message;
                        return View(plantModel);
                    }
                    else
                    {
                        plantModel.CompanyName = PTEAppRequest.ApplicantName;
                        plantModel.StorageCapacity = PTEAppRequest.StorageCapacity;
                        plantModel.LocationAddress = PTEAppRequest.SiteLocationAddress;
                        plantModel.RegisteredAddress = PTEAppRequest.RegisteredAddress;
                        plantModel.Status = PTEAppRequest.Status;
                        plantModel.ApplicationCategory = PTEAppRequest.ApplicationCategory;
                        plantModel.FacilityName = (from f in dbCtxt.Facilities where f.FacilityId == PTEAppRequest.FacilityId select f.FalicityName).FirstOrDefault();
                    }
                }




                log.Info("About to get GetCompanyDocumentListById => " + userMaster.ElpsId);
                ElpsResponse elpsResponse = serviceIntegrator.GetCompanyDocumentListById(userMaster.ElpsId);
                if (elpsResponse.message != "SUCCESS")
                {
                    log.Error(elpsResponse.message);
                    ViewBag.ResponseMessage = elpsResponse.message;
                    return View(plantModel);
                }




                ElpsResponse addresselpsResponse = serviceIntegrator.GetCompanyAddressList(userMaster.ElpsId);
                if (addresselpsResponse.message != "SUCCESS")
                {
                    log.Warn("GetCompanyAddressList => " + addresselpsResponse.message);
                }
                else
                {
                    List<CompanyAddressDTO> addressList = (List<CompanyAddressDTO>)addresselpsResponse.value;
                    log.Info("Address List Count => " + addressList.Count);
                    if (addressList.Count > 0)
                    {
                        log.Info("Address Name => " + addressList[0].address_1 + " " + addressList[0].address_2);
                        plantModel.RegisteredAddress = addressList[0].address_1 + " " + addressList[0].address_2;
                    }
                }

                log.Info("About to Cast DocumentList Fetched From Elps");



                var facilityelpsid = PTEAppRequest == null ? (from f in dbCtxt.Facilities
                                                              join a in dbCtxt.ApplicationRequests on f.LocationAddress equals a.SiteLocationAddress
                                                              where a.ApplicationId == ApplicationId && a.FacilityId == f.FacilityId
                                                              select f.ElpsFacilityId).FirstOrDefault()
                                      : (from f in dbCtxt.Facilities
                                         join a in dbCtxt.ApplicationRequests on f.LocationAddress equals a.SiteLocationAddress
                                         where a.ApplicationId == PTEAppRequest.ApplicationId && a.FacilityId == f.FacilityId
                                         select f.ElpsFacilityId).FirstOrDefault();

                ViewBag.Elpsfacid = facilityelpsid;


                ElpsResponse facilityelpsResponse = serviceIntegrator.GetFacilityDocumentListById(facilityelpsid.ToString());

                ElpsResponse allfacilityelpsResponse = serviceIntegrator.GetAllFacilityDocumentListById();

                ElpsResponse AllDocselpsResponse = serviceIntegrator.GetAllDocumentType();



                List<Document> ElpsDocumenList = (List<Document>)elpsResponse.value;
                log.Info("ElpsDocument Size =>" + ElpsDocumenList.Count);
                List<AllDocumentTypes> AllDocElpsDocumenList = (List<AllDocumentTypes>)AllDocselpsResponse.value;
                List<FacilityDocument> ElpsFacDocumenList = (List<FacilityDocument>)facilityelpsResponse.value;
                List<FacilityDocument> AllElpsFacDocumenList = (List<FacilityDocument>)allfacilityelpsResponse.value;// use if facility app id equals null

                log.Info("ElpsDocument Size =>" + ElpsDocumenList.Count);
                List<MissingDocument> missindoc = (from m in dbCtxt.MissingDocuments where m.ApplicationID == ApplicationId select m).ToList();
                List<RequiredLicenseDocument> AllCompanyDocumentList = dbCtxt.RequiredLicenseDocuments.Where(c => c.ApplicationTypeId.Trim() == "NEW" && c.LicenseTypeId == LicenseTypeId && c.IsBaseTran == "B" && c.Status.Equals("ACTIVE")).ToList();
                List<RequiredLicenseDocument> AllFacilityDocList = dbCtxt.RequiredLicenseDocuments.Where(c => c.ApplicationTypeId.Trim() == "NEW" && c.LicenseTypeId == LicenseTypeId && c.IsBaseTran == "T" && c.Status.Equals("ACTIVE")).ToList();

                List<RequiredLicenseDocument> CompareDocList = dbCtxt.RequiredLicenseDocuments.Where(c => c.ApplicationTypeId.Trim() == "NEW" && c.LicenseTypeId == LicenseTypeId && c.Status.Equals("ACTIVE")).ToList();


                plantModel.AdditionalDocumentList1 = appDocHelper.MissingDocuments(missindoc, AllDocElpsDocumenList, CompareDocList, ApplicationId, GlobalModel.elpsUrl);
                plantModel.CompanyDocumentList1 = appDocHelper.GetDocumentsPending(AllCompanyDocumentList, ElpsDocumenList, ApplicationId, GlobalModel.elpsUrl);
                plantModel.CompanyFacilityMissingDocumentList1 = appDocHelper.CompanyFacilityMissingDocuments(AllDocElpsDocumenList, AllElpsFacDocumenList, AllCompanyDocumentList, AllFacilityDocList, CompareDocList);



                if (facilityelpsid != null)
                {
                    plantModel.FacilityDocumentList1 = appDocHelper.GetFacilityDocumentsPending(AllFacilityDocList, ElpsFacDocumenList, ApplicationId, GlobalModel.elpsUrl);
                }
                else
                {
                    plantModel.FacilityDocumentList1 = appDocHelper.GetFacilityDocumentsPending(AllFacilityDocList, AllElpsFacDocumenList, ApplicationId, GlobalModel.elpsUrl);
                }


                //ViewBag.DocumentList = appDocHelper.GetDocumentsPending(RequiredDocumentList, ElpsDocumenList, ApplicationId, GlobalModel.elpsUrl);

                plantModel.ElpsId = userMaster.ElpsId;
                //ViewBag.UploadDocumentUrl = GlobalModel.elpsUrl + "/api/UploadCompanyDoc/{0}/{1}/{2}/{3}?docName={4}&uniqueid={5}";
                plantModel.ApplicationHash = commonHelper.GenerateHashText(GlobalModel.appEmail + GlobalModel.appKey);
                plantModel.Email = GlobalModel.appEmail;
                plantModel.ElpsUrl = GlobalModel.elpsUrl;

                ViewBag.ResponseMessage = "SUCCESS";
                log.Info("Return with Success");

            }
            catch (Exception ex)
            {
                log.Error(ex.InnerException);
                ViewBag.ResponseMessage = "An Error Occured When trying to Get Values for the UserInterface, Please try again Later";
            }

            return View(plantModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApprovalToConstruct(LubePlantModel model, FormCollection coll, List<SubmittedDocument> MyApplicationDocs)
        {
            bool isNew = false;
            String status = "success";
            String message = string.Empty;
            String comment = string.Empty;
            string errorMessage = string.Empty;
            ApplicationRequest appRequest = null;
            ApplicationRequest PTEAppRequest = null;
            string PTEReference = model.PTEReference;
            string LicenseTypeId = model.LicenseTypeId;
            // string genApplicationId = model.ApplicationId;
            ElpsResponse wrapper = null;
            Facility facility = new Facility();
            // var Facid = coll.Get("facId");
            var TakeoverRef = coll.Get("TakeoverRefNo");
            var Applicationtypecat = coll.Get("appcategory");
            string appstatus = "";



            if ((model.LicenseTypeId == "ATM" || model.LicenseTypeId == "ATO") && (model.Status != "Rejected"))
            {
                model.ApplicationId = null;
            }


            appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == model.ApplicationId.Trim()).FirstOrDefault();

            if (appRequest != null)
            {
                if (appRequest.Status == "Rejected")
                {
                    model.ApplicationId = appRequest.ApplicationId;
                    appstatus = "Rejected";
                }
                else
                {
                    model.ApplicationId = commonHelper.GenerateApplicationNo();
                    appstatus = "ACTIVE";
                }
            }
            else
            {
                model.ApplicationId = commonHelper.GenerateApplicationNo();
                appstatus = "ACTIVE";
            }





            try
            {

                log.Info("Coming into The Function To Maintain Data with ID =>" + model.ApplicationId);

                appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == model.ApplicationId.Trim()).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    isNew = true;
                    appRequest = new ApplicationRequest();
                }



                if (!string.IsNullOrEmpty(PTEReference))
                {
                    PTEAppRequest = dbCtxt.ApplicationRequests.Where(c => c.LicenseReference.Trim() == PTEReference.Trim()).FirstOrDefault();
                    if (appRequest ==
                     default(ApplicationRequest))
                    {
                        status = "failure";
                        message = "The Application with PTE Permit NO " + PTEReference + " Cannot be retrievd from the Database";
                        log.Error(message);

                        return Json(new
                        {
                            Status = status,
                            Message = message
                        }, JsonRequestBehavior.AllowGet);
                    }
                }



                log.Info("LicenseTypeId => " + LicenseTypeId);
                log.Info("PTEReference => " + PTEReference);
                log.Info("ApplicationId =>" + model.ApplicationId);

                appRequest.ApplicationId = model.ApplicationId;
                appRequest.LicenseTypeId = LicenseTypeId;
                appRequest.ApplicationTypeId = "NEW";
                appRequest.ApplicationCategory = PTEAppRequest.ApplicationCategory == null ? Applicationtypecat : PTEAppRequest.ApplicationCategory;
                appRequest.ApplicantUserId = userMaster.UserId;
                appRequest.CurrentAssignedUser = userMaster.UserId;
                appRequest.ApplicantName = userMaster.FirstName;
                appRequest.SiteLocationAddress = PTEAppRequest.SiteLocationAddress;
                appRequest.RegisteredAddress = PTEAppRequest.RegisteredAddress;
                appRequest.LinkedReference = PTEReference;
                appRequest.StateCode = PTEAppRequest.StateCode;
                appRequest.LgaCode = PTEAppRequest.LgaCode;
                appRequest.LandSize = PTEAppRequest.LandSize;
                appRequest.GPSCordinates = PTEAppRequest.GPSCordinates;
                appRequest.BeaconLocations = PTEAppRequest.BeaconLocations;
                appRequest.AccessRoadToFromSite = PTEAppRequest.AccessRoadToFromSite;
                appRequest.AnyEquipmentOnSite = PTEAppRequest.AnyEquipmentOnSite;
                appRequest.LandTopology = PTEAppRequest.LandTopology;
                appRequest.NatureOfArea = PTEAppRequest.NatureOfArea;
                appRequest.AnnualCumuBaseOilRequirementCapacity = PTEAppRequest.AnnualCumuBaseOilRequirementCapacity;
                appRequest.AnnualProductionProjectionCapacity = PTEAppRequest.AnnualProductionProjectionCapacity;
                appRequest.AdjoiningProperties = PTEAppRequest.AdjoiningProperties;
                appRequest.AnyEquipmentOnSite = PTEAppRequest.AnyEquipmentOnSite;
                appRequest.RelationshipWithPipelineRightOfWay = PTEAppRequest.RelationshipWithPipelineRightOfWay;
                appRequest.RelationshipWithSensitiveInstitutions = PTEAppRequest.RelationshipWithSensitiveInstitutions;
                appRequest.RelationshipWithStreams = PTEAppRequest.RelationshipWithStreams;
                appRequest.RelationshipWithRailwayLine = PTEAppRequest.RelationshipWithRailwayLine;
                appRequest.RelationshipWithPHCNTensionLines = PTEAppRequest.RelationshipWithPHCNTensionLines;
                appRequest.AdditionalInfo = PTEAppRequest.AdditionalInfo;
                appRequest.StorageCapacity = PTEAppRequest.StorageCapacity;
                appRequest.Status = appstatus;
                appRequest.AddedDate = DateTime.UtcNow;
                appRequest.PrintedStatus = "Not Printed";
                appRequest.IsLegacy = "NO";
                appRequest.CurrentOfficeLocation = dbCtxt.FieldLocations.Where(f => f.FieldType == "HQ").FirstOrDefault().FieldLocationID;



                comment = "ApplicationForm Completed";


                var facility_count = model.LicenseTypeId == "ATO" ? dbCtxt.Facilities.Where(x => x.FalicityName == model.FacilityName && x.LocationAddress == model.LocationAddress).ToList() : dbCtxt.Facilities.Where(x => x.FalicityName == model.FacilityName && x.LocationAddress == model.LocationAddress && x.CompanyUserId == userMaster.UserId).ToList();
                int facilityid = 0;

                //if (model.State == "LAG") { appRequest.CurrentOfficeLocation = "3"; }
                //else
                //{
                //    appRequest.CurrentOfficeLocation = dbCtxt.RequestFieldMappings.Where(r => r.StateCode == PTEAppRequest.StateCode).FirstOrDefault().FieldLocationID;
                //}

                if (facility_count.Count != 0)
                {
                    appRequest.FacilityId = facility_count.FirstOrDefault().FacilityId;
                }

                if (facility_count.Count == 0)
                {

                    facility.CompanyUserId = userMaster.UserId;
                    facility.FalicityName = model.FacilityName;
                    facility.LocationAddress = model.LocationAddress;
                    dbCtxt.Facilities.Add(facility);
                    dbCtxt.SaveChanges();


                    facilityid = dbCtxt.Facilities.Where(x => x.FalicityName == model.FacilityName && x.LocationAddress == model.LocationAddress).FirstOrDefault().FacilityId;
                    var statename = (from s in dbCtxt.StateMasterLists where s.StateCode == model.State select s.StateName).FirstOrDefault();

                    Facilities _facilities = new Facilities()
                    {
                        Name = model.FacilityName,
                        CompanyId = Convert.ToInt32(userMaster.ElpsId),
                        StreetAddress = model.LocationAddress,
                        City = statename,
                        FacilityType = "Lube Oil Blending Plant",
                        StateId = 1,
                        DateAdded = DateTime.Now,
                    };

                    wrapper = serviceIntegrator.PostFacility(_facilities);
                    if (wrapper.message == "SUCCESS")
                    {
                        Facilities facilityDetail = (Facilities)wrapper.value;

                        var updateFacility = dbCtxt.Facilities.Where(x => x.FacilityId == facilityid).FirstOrDefault();
                        updateFacility.ElpsFacilityId = facilityDetail.Id;
                        dbCtxt.SaveChanges();
                    }
                    var fid = (from f in dbCtxt.Facilities where f.CompanyUserId == userMaster.UserId select f.FacilityId).ToList().LastOrDefault();
                    appRequest.FacilityId = fid;
                }







                if (!commonHelper.isPaymentMade(model.ApplicationId, out errorMessage))
                {
                    appRequest.CurrentStageID = 1;
                }
                else
                {
                    comment = "ApplicationForm Modified";
                }


                if (model.LicenseTypeId == "ATM" || model.LicenseTypeId == "ATO")
                {
                    facilityid = facility_count.FirstOrDefault().FacilityId; //Convert.ToInt32(Facid);
                }
                else
                {
                    facilityid = dbCtxt.Facilities.Where(x => x.FalicityName == model.FacilityName && x.LocationAddress == model.LocationAddress && x.CompanyUserId == userMaster.UserId).FirstOrDefault().FacilityId;

                }






                var check_doc1 = dbCtxt.SubmittedDocuments.Where(x => x.ApplicationID == model.ApplicationId).ToList();

                if (MyApplicationDocs != null)
                {
                    foreach (var item in MyApplicationDocs)
                    {
                        var check_doc = dbCtxt.SubmittedDocuments.Where(x => x.ApplicationID == model.ApplicationId && x.DocId == item.DocId).ToList();

                        if (check_doc1.Count() == 0 || check_doc.Count() == 0)
                        {
                            SubmittedDocument submitDocs = new SubmittedDocument()
                            {
                                ApplicationID = model.ApplicationId,
                                DocId = item.DocId,
                                BaseorTrans = item.BaseorTrans,
                                DocName = item.DocName,
                                FileId = item.FileId,
                                DocSource = item.DocSource
                            };
                            dbCtxt.SubmittedDocuments.Add(submitDocs);
                        }

                        else if (check_doc1.Count() > 0 && check_doc1.Count() == MyApplicationDocs.Count())
                        {
                            check_doc.FirstOrDefault().ApplicationID = model.ApplicationId;
                            check_doc.FirstOrDefault().DocId = item.DocId;
                            check_doc.FirstOrDefault().BaseorTrans = item.BaseorTrans;
                            check_doc.FirstOrDefault().DocName = item.DocName;
                            check_doc.FirstOrDefault().FileId = item.FileId;
                            check_doc.FirstOrDefault().DocSource = item.DocSource;
                        }
                        else
                        {
                            foreach (var item1 in check_doc1)
                            {
                                dbCtxt.SubmittedDocuments.Remove(item1);
                            }
                            dbCtxt.SaveChanges();


                            foreach (var item2 in MyApplicationDocs)
                            {

                                SubmittedDocument submitDocs = new SubmittedDocument()
                                {
                                    ApplicationID = model.ApplicationId,
                                    DocId = item2.DocId,
                                    BaseorTrans = item2.BaseorTrans,
                                    DocName = item2.DocName,
                                    FileId = item2.FileId,
                                    DocSource = item2.DocSource
                                };
                                dbCtxt.SubmittedDocuments.Add(submitDocs);
                            }
                            break;
                        }
                    }
                }

                //}

                if (model.LicenseTypeId == "ATO")
                {
                    TakeoverApp takeover = new TakeoverApp()
                    {
                        ApplicationID = model.ApplicationId,
                        LicenseReference = PTEReference

                    };
                    dbCtxt.TakeoverApps.Add(takeover);
                }













                log.Info("About to Save ApplicationMaster");
                if (isNew == true)
                {
                    dbCtxt.ApplicationRequests.Add(appRequest);
                }
                dbCtxt.SaveChanges();

                comment = "ApplicationForm Completed";
                log.Info("About to get WorkFlow Navigation");

                message = "Application Details Saved Successfully, Proceed";

                if (!commonHelper.isPaymentMade(model.ApplicationId, out errorMessage))
                {
                    log.Info("About to get WorkFlow Navigation");

                    var fieldlocation = dbCtxt.FieldLocations.Where(f => f.FieldType == "HQ").FirstOrDefault().FieldLocationID;
                    ResponseWrapper responseWrapper = workflowHelper.processAction(dbCtxt, model.ApplicationId, "Proceed", userMaster.UserId, comment, fieldlocation, "");

                    log.Info("Done with WorkFlow Navigation");
                    message = responseWrapper.value;
                    if (!responseWrapper.status)
                    {
                        status = "failure";
                        log.Error(message);
                    }
                }

                log.Info("Status =>" + status + ", Message =>" + message);




            }
            catch (DbEntityValidationException ex)
            {
                string l = string.Empty;
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        l += String.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }

                status = "failure";
                message = "Database Validation Error, Please Try Again Later";
                log.Error(l);
            }
            catch (Exception ex)
            {
                status = "failure";
                message = "System Error Occured When Maintaining Data, Please Try Again Later";
                log.Error(ex.StackTrace, ex);
            }


            return Json(new
            {
                Status = status,
                Message = message,
                applicationId = model.ApplicationId
            }, JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public ActionResult LicenseToOperate(string ApplicationTypeId, string LicenseTypeId = null, string ApplicationId = null, string LinkedReference = null, string appref = null)
        {

            LubePlantModel lubePlantModel = new LubePlantModel();
            ApplicationRequest appRequest =
             default(ApplicationRequest);
            List<SelectListItem> StateList = new List<SelectListItem>();
            List<SelectListItem> LgaList = new List<SelectListItem>();
            List<SelectListItem> PlantTypeList = new List<SelectListItem>();

            try
            {



                log.Info("Coming into Function With ApplicationTypeId,LicenseTypeId,ApplicationId,LinkedReference => " + ApplicationTypeId + "," + LicenseTypeId + "," + ApplicationId + "," + LinkedReference);
                lubePlantModel.CompanyName = userMaster.FirstName;
                lubePlantModel.LicenseTypeId = LicenseTypeId;
                lubePlantModel.ApplicantTypeId = ApplicationTypeId;
                lubePlantModel.ApplicationId = ApplicationId;
                lubePlantModel.LinkedReference = LinkedReference;
                if (LicenseTypeId == "ATM") { ViewBag.ATMTOdescription = "Approval To Modify"; }
                else if (LicenseTypeId == "ATO") { ViewBag.ATMTOdescription = "Takeover"; }
                else if (LicenseTypeId == "LTOLFP") { ViewBag.LTOLFPdescription = "License To Operate Lubricant Filling Plant"; }


                ElpsResponse addresselpsResponse = serviceIntegrator.GetCompanyAddressList(userMaster.ElpsId);
                if (addresselpsResponse.message != "SUCCESS")
                {
                    log.Warn("GetCompanyAddressList => " + addresselpsResponse.message);
                }
                else
                {
                    List<CompanyAddressDTO> addressList = (List<CompanyAddressDTO>)addresselpsResponse.value;
                    log.Info("Address List Count => " + addressList.Count);
                    if (addressList.Count > 0)
                    {
                        log.Info("Address Name => " + addressList[0].address_1 + " " + addressList[0].address_2);
                        lubePlantModel.RegisteredAddress = addressList[0].address_1 + " " + addressList[0].address_2;
                    }
                }

               

                appRequest = dbCtxt.ApplicationRequests.Where(a => a.ApplicationId == ApplicationId).FirstOrDefault() == null ? dbCtxt.ApplicationRequests.Where(a => a.LicenseReference == LinkedReference && a.ApplicantUserId == userMaster.UserId).FirstOrDefault()
                    : dbCtxt.ApplicationRequests.Where(a => a.ApplicationId == ApplicationId).FirstOrDefault();

                if (appRequest ==
                 default(ApplicationRequest))
                {
                    if (ApplicationTypeId == "RENEW" && !string.IsNullOrEmpty(LinkedReference))
                    {
                        appRequest = dbCtxt.ApplicationRequests.Where(a => a.LicenseReference == LinkedReference && a.ApplicantUserId == userMaster.UserId).FirstOrDefault();
                    }

                    if (LicenseTypeId == "ATO")
                    {
                        appRequest = dbCtxt.ApplicationRequests.Where(a => a.LicenseReference == LinkedReference).FirstOrDefault();

                    }
                }

                if (appRequest !=
                 default(ApplicationRequest))
                {


                    String selectedStateCode = null;
                    foreach (StateMasterList r in dbCtxt.StateMasterLists.ToList().OrderBy(s => s.StateName))
                    {
                        if (r.StateCode == appRequest.StateCode)
                        {
                            selectedStateCode = r.StateCode;
                            StateList.Add(new SelectListItem
                            {
                                Text = r.StateName,
                                Value = r.StateCode,
                                Selected = true
                            });
                        }
                        else
                            StateList.Add(new SelectListItem
                            {
                                Text = r.StateName,
                                Value = r.StateCode
                            });
                    }
                    ViewBag.State = StateList;


                    foreach (LgaMasterList r in dbCtxt.LgaMasterLists.Where(l => l.StateCode == selectedStateCode).ToList().OrderBy(l => l.LgaName))
                    {
                        if (r.LgaCode == appRequest.LgaCode)
                        {
                            LgaList.Add(new SelectListItem
                            {
                                Text = r.LgaName,
                                Value = r.LgaCode,
                                Selected = true
                            });
                        }
                        else
                            LgaList.Add(new SelectListItem
                            {
                                Text = r.LgaName,
                                Value = r.LgaCode
                            });
                    }
                    ViewBag.LGA = LgaList;
                    lubePlantModel.CompanyName = appRequest.ApplicantName;
                    lubePlantModel.Status = appRequest.Status;
                    lubePlantModel.LocationAddress = appRequest.SiteLocationAddress;
                    lubePlantModel.RegisteredAddress = appRequest.RegisteredAddress;
                    lubePlantModel.PostalAddress = appRequest.RegisteredAddress;
                    lubePlantModel.State = appRequest.StateMasterList.StateCode;
                    lubePlantModel.LGA = appRequest.LgaMasterList.LgaCode;
                    lubePlantModel.AnnualCumulativeBaseOilRequirementsInLitres = appRequest.AnnualCumuBaseOilRequirementCapacity;
                    //lubePlantModel.AnnualProductsProductionProjections = appRequest.AnnualProductionProjectionCapacity;
                    lubePlantModel.BaseOilRequirements = appRequest.ProdBaseOilRequirement;
                    lubePlantModel.LubricantsProducedByCompany = appRequest.LubricantProdByCompany;
                    lubePlantModel.TrainingNMaintenanceProgramme = appRequest.ListOfPropTrainingforStaff;
                    lubePlantModel.PreventiveAnnualMaintenance = appRequest.PreventiveAnnualMaintPrgmPlant;
                    lubePlantModel.StorageCapacity = appRequest.StorageCapacity;
                    lubePlantModel.ApplicationCategory = appRequest.ApplicationCategory;
                    lubePlantModel.GPS = appRequest.GPSCordinates;
                    lubePlantModel.UTM = "";
                    lubePlantModel.AdditionalInformation = appRequest.AdditionalInfo;
                    lubePlantModel.FacilityName = (from f in dbCtxt.Facilities where f.FacilityId == appRequest.FacilityId select f.FalicityName).FirstOrDefault();
                }
                else
                {
                    ViewBag.State = new SelectList(dbCtxt.StateMasterLists.OrderBy(c => c.StateName), "StateCode", "StateName");
                    foreach (LgaMasterList r in dbCtxt.LgaMasterLists.Where(l => l.StateCode == "ABI").ToList().OrderBy(l => l.LgaName))
                    {
                        LgaList.Add(new SelectListItem
                        {
                            Text = r.LgaName,
                            Value = r.LgaCode
                        });
                    }
                    ViewBag.LGA = LgaList;

                    StateMasterList abia = dbCtxt.StateMasterLists.Where(s => s.StateCode == "ABI").FirstOrDefault();
                    ViewBag.Latitude = abia.Latitude;
                    ViewBag.Longitude = abia.Longitude;
                }


                log.Info("About to get GetCompanyDocumentListById => " + userMaster.ElpsId);
                ElpsResponse elpsResponse = serviceIntegrator.GetCompanyDocumentListById(userMaster.ElpsId);
                if (elpsResponse.message != "SUCCESS")
                {
                    log.Error(elpsResponse.message);
                    ViewBag.ResponseMessage = elpsResponse.message;
                    return View(lubePlantModel);
                }

                log.Info("About to Cast DocumentList Fetched From Elps");

                var facilityelpsid = appRequest == null ? (from f in dbCtxt.Facilities
                                                           join a in dbCtxt.ApplicationRequests on f.LocationAddress equals a.SiteLocationAddress
                                                           where a.ApplicationId == ApplicationId && a.FacilityId == f.FacilityId
                                                           select f.ElpsFacilityId).FirstOrDefault()
                                      : (from f in dbCtxt.Facilities
                                         join a in dbCtxt.ApplicationRequests on f.LocationAddress equals a.SiteLocationAddress
                                         where a.ApplicationId == appRequest.ApplicationId && a.FacilityId == f.FacilityId
                                         select f.ElpsFacilityId).FirstOrDefault();

                ViewBag.Elpsfacid = facilityelpsid;


                ElpsResponse facilityelpsResponse = serviceIntegrator.GetFacilityDocumentListById(facilityelpsid.ToString());

                ElpsResponse allfacilityelpsResponse = serviceIntegrator.GetAllFacilityDocumentListById();

                ElpsResponse AllDocselpsResponse = serviceIntegrator.GetAllDocumentType();



                List<Document> ElpsDocumenList = (List<Document>)elpsResponse.value;
                log.Info("ElpsDocument Size =>" + ElpsDocumenList.Count);
                List<AllDocumentTypes> AllDocElpsDocumenList = (List<AllDocumentTypes>)AllDocselpsResponse.value;
                List<FacilityDocument> ElpsFacDocumenList = (List<FacilityDocument>)facilityelpsResponse.value;
                List<FacilityDocument> AllElpsFacDocumenList = (List<FacilityDocument>)allfacilityelpsResponse.value;// use if facility app id equals null

                log.Info("ElpsDocument Size =>" + ElpsDocumenList.Count);
                List<MissingDocument> missindoc = (from m in dbCtxt.MissingDocuments where m.ApplicationID == ApplicationId select m).ToList();
                List<RequiredLicenseDocument> AllCompanyDocumentList = dbCtxt.RequiredLicenseDocuments.Where(c => c.ApplicationTypeId.Trim() == ApplicationTypeId && c.LicenseTypeId == LicenseTypeId && c.IsBaseTran == "B" && c.Status.Equals("ACTIVE")).ToList();
                List<RequiredLicenseDocument> AllFacilityDocList = dbCtxt.RequiredLicenseDocuments.Where(c => c.ApplicationTypeId.Trim() == ApplicationTypeId && c.LicenseTypeId == LicenseTypeId && c.IsBaseTran == "T" && c.Status.Equals("ACTIVE")).ToList();

                List<RequiredLicenseDocument> CompareDocList = dbCtxt.RequiredLicenseDocuments.Where(c => c.ApplicationTypeId.Trim() == ApplicationTypeId && c.LicenseTypeId == LicenseTypeId && c.Status.Equals("ACTIVE")).ToList();


                lubePlantModel.AdditionalDocumentList1 = appDocHelper.MissingDocuments(missindoc, AllDocElpsDocumenList, CompareDocList, ApplicationId, GlobalModel.elpsUrl);
                lubePlantModel.CompanyDocumentList1 = appDocHelper.GetDocumentsPending(AllCompanyDocumentList, ElpsDocumenList, ApplicationId, GlobalModel.elpsUrl);
                lubePlantModel.CompanyFacilityMissingDocumentList1 = appDocHelper.CompanyFacilityMissingDocuments(AllDocElpsDocumenList, AllElpsFacDocumenList, AllCompanyDocumentList, AllFacilityDocList, CompareDocList);



                if (facilityelpsid != null)
                {
                    lubePlantModel.FacilityDocumentList1 = appDocHelper.GetFacilityDocumentsPending(AllFacilityDocList, ElpsFacDocumenList, ApplicationId, GlobalModel.elpsUrl);
                }
                else
                {
                    lubePlantModel.FacilityDocumentList1 = appDocHelper.GetFacilityDocumentsPending(AllFacilityDocList, AllElpsFacDocumenList, ApplicationId, GlobalModel.elpsUrl);
                }


                //ViewBag.DocumentList = appDocHelper.GetDocumentsPending(RequiredDocumentList, ElpsDocumenList, ApplicationId, GlobalModel.elpsUrl);

                lubePlantModel.ElpsId = userMaster.ElpsId;
                //ViewBag.UploadDocumentUrl = GlobalModel.elpsUrl + "/api/UploadCompanyDoc/{0}/{1}/{2}/{3}?docName={4}&uniqueid={5}";
                lubePlantModel.ApplicationHash = commonHelper.GenerateHashText(GlobalModel.appEmail + GlobalModel.appKey);
                lubePlantModel.Email = GlobalModel.appEmail;
                lubePlantModel.ElpsUrl = GlobalModel.elpsUrl;

                ViewBag.ResponseMessage = "SUCCESS";
                log.Info("Continue With the Function");

            }
            catch (Exception ex)
            {
                log.Error(ex.InnerException);
                ViewBag.ResponseMessage = "An Error Occured When trying to Get Values for the UserInterface, Please try again Later";
            }

            return View(lubePlantModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult LicenseToOperate(LubePlantModel model, FormCollection coll, List<SubmittedDocument> MyApplicationDocs)
        {

            bool isNew = false;
            string message = null;
            string status = "success";
            string comment = string.Empty;
            string errorMessage = string.Empty;
            ApplicationRequest appRequest = null;
            ApplicationRequest linkedappRequest = null;
            string LicenseTypeId = model.LicenseTypeId;
            //string genApplicationId = model.ApplicationId;
            ElpsResponse wrapper = null;
            string LinkedReference = model.LinkedReference;
            Facility facility = new Facility();
            //var Facid = coll.Get("facId");
            var TakeoverRef = coll.Get("TakeoverRefNo");
            var Applicationtypecat = coll.Get("appcategory");
            string appstatus = "";



            if ((model.LicenseTypeId == "ATM" || model.LicenseTypeId == "ATO") && (model.Status != "Rejected"))
            {
                model.ApplicationId = null;
            }



            appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == model.ApplicationId.Trim()).FirstOrDefault();

            if (appRequest != null)
            {
                if (appRequest.Status == "Rejected")
                {
                    model.ApplicationId = appRequest.ApplicationId;
                    LicenseTypeId = appRequest.LicenseTypeId;
                    model.AppRef = appRequest.LicenseTypeId;
                    appstatus = "Rejected";
                }
                else
                {
                    model.ApplicationId = commonHelper.GenerateApplicationNo();
                    appstatus = "ACTIVE";
                }
            }
            else
            {
                model.ApplicationId = commonHelper.GenerateApplicationNo();
                appstatus = "ACTIVE";
            }


            try
            {

                log.Info("Coming into The Function To Maintain Data with ID =>" + model.ApplicationId + " with Linked Reference =>" + model.LinkedReference);

                appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == model.ApplicationId.Trim()).FirstOrDefault();
                if (appRequest ==
                 default(ApplicationRequest))
                {
                    isNew = true;
                    appRequest = new ApplicationRequest();
                    log.Info("Application ID " + model.ApplicationId + " IS a NEW Applicaton");
                }

                if (!string.IsNullOrEmpty(LinkedReference))
                {
                    linkedappRequest = dbCtxt.ApplicationRequests.Where(c => c.LicenseReference.Trim() == LinkedReference.Trim()).FirstOrDefault();
                    if (appRequest ==
                     default(ApplicationRequest))
                    {
                        status = "failure";
                        message = "The Application with Reference " + LinkedReference + " Cannot be retrievd from the Database";
                        log.Error(message);
                        return Json(new
                        {
                            Status = status,
                            Message = message
                        }, JsonRequestBehavior.AllowGet);
                    }
                }

                log.Info("About to Set Application Master Fields");
                if(model.LicenseTypeId == "LTOLFP")
                {
                    appRequest.ApplicationCategory = "Lubricant Filling Plant";
                }
                else
                {
                    appRequest.ApplicationCategory = model.ApplicationCategory == null ? Applicationtypecat : model.ApplicationCategory;

                }
                appRequest.ApplicationId = model.ApplicationId;
                appRequest.LicenseTypeId = LicenseTypeId;
                log.Info("1");
                appRequest.ApplicationTypeId = model.ApplicantTypeId;
                appRequest.ApplicantUserId = userMaster.UserId;
                appRequest.CurrentAssignedUser = userMaster.UserId;
                appRequest.ApplicantName = userMaster.FirstName;
                appRequest.SiteLocationAddress = model.LocationAddress;
                appRequest.RegisteredAddress = model.RegisteredAddress;
                appRequest.StateCode = model.State;
                appRequest.LgaCode = model.LGA;
                log.Info("2");
                appRequest.LinkedReference = LinkedReference;
                appRequest.GPSCordinates = model.GPS;
                appRequest.ProdBaseOilRequirement = model.BaseOilRequirements;
                appRequest.LubricantProdByCompany = model.LubricantsProducedByCompany;
                log.Info("3");
                appRequest.AnnualCumuBaseOilRequirementCapacity = model.AnnualCumulativeBaseOilRequirementsInLitres;
                //appRequest.AnnualProductionProjectionCapacity = model.AnnualProductsProductionProjections;
                appRequest.ListOfPropTrainingforStaff = model.TrainingNMaintenanceProgramme;
                appRequest.PreventiveAnnualMaintPrgmPlant = model.PreventiveAnnualMaintenance;
                appRequest.AddedDate = DateTime.UtcNow;
                appRequest.Status = appstatus;
                log.Info("4");
                appRequest.StorageCapacity = model.StorageCapacity;
                appRequest.AdditionalInfo = model.AdditionalInformation;
                log.Info("5");
                appRequest.IsLegacy = "NO";
                appRequest.PrintedStatus = "Not Printed";
                appRequest.CurrentOfficeLocation = dbCtxt.FieldLocations.Where(f => f.FieldType == "HQ").FirstOrDefault().FieldLocationID;

                var facility_count = model.LicenseTypeId == "ATO" ? dbCtxt.Facilities.Where(x => x.FalicityName == model.FacilityName && x.LocationAddress == model.LocationAddress).ToList() : dbCtxt.Facilities.Where(x => x.FalicityName == model.FacilityName && x.LocationAddress == model.LocationAddress && x.CompanyUserId == userMaster.UserId).ToList();
                int facilityid = 0;

                if (facility_count.Count != 0)
                {
                    appRequest.FacilityId = facility_count.FirstOrDefault().FacilityId;
                }

                if (facility_count.Count == 0)
                {

                    facility.CompanyUserId = userMaster.UserId;
                    facility.FalicityName = model.FacilityName;
                    facility.LocationAddress = model.LocationAddress;
                    dbCtxt.Facilities.Add(facility);
                    dbCtxt.SaveChanges();


                    facilityid = dbCtxt.Facilities.Where(x => x.FalicityName == model.FacilityName && x.LocationAddress == model.LocationAddress).FirstOrDefault().FacilityId;
                    var statename = (from s in dbCtxt.StateMasterLists where s.StateCode == model.State select s.StateName).FirstOrDefault();

                    Facilities _facilities = new Facilities()
                    {
                        Name = model.FacilityName,
                        CompanyId = Convert.ToInt32(userMaster.ElpsId),
                        StreetAddress = model.LocationAddress,
                        City = statename,
                        FacilityType = "Lube Oil Blending Plant",
                        StateId = 1,
                        DateAdded = DateTime.Now,
                    };

                    wrapper = serviceIntegrator.PostFacility(_facilities);
                    if (wrapper.message == "SUCCESS")
                    {
                        Facilities facilityDetail = (Facilities)wrapper.value;

                        var updateFacility = dbCtxt.Facilities.Where(x => x.FacilityId == facilityid).FirstOrDefault();
                        updateFacility.ElpsFacilityId = facilityDetail.Id;
                        dbCtxt.SaveChanges();
                    }
                    var fid = (from f in dbCtxt.Facilities where f.CompanyUserId == userMaster.UserId select f.FacilityId).ToList().LastOrDefault();
                    appRequest.FacilityId = fid;
                }
                log.Info("8");

                comment = "ApplicationForm Completed";
                if (!commonHelper.isPaymentMade(model.ApplicationId, out errorMessage))
                {
                    log.Info("About to Set First Stage ID");
                    appRequest.CurrentStageID = 1;
                    //linkedappRequest.Status = "LINKED";
                }
                else
                {
                    comment = "ApplicationForm Modified";
                }





                if (model.LicenseTypeId == "ATM" || model.LicenseTypeId == "ATO")
                {
                    facilityid = facility_count.FirstOrDefault().FacilityId;
                }
                else
                {
                    facilityid = dbCtxt.Facilities.Where(x => x.FalicityName == model.FacilityName && x.LocationAddress == model.LocationAddress && x.CompanyUserId == userMaster.UserId).FirstOrDefault().FacilityId;

                }






                var check_doc1 = dbCtxt.SubmittedDocuments.Where(x => x.ApplicationID == model.ApplicationId).ToList();

                if (MyApplicationDocs != null)
                {
                    foreach (var item in MyApplicationDocs)
                    {
                        var check_doc = dbCtxt.SubmittedDocuments.Where(x => x.ApplicationID == model.ApplicationId && x.DocId == item.DocId).ToList();

                        if (check_doc1.Count() == 0 || check_doc.Count() == 0)
                        {
                            SubmittedDocument submitDocs = new SubmittedDocument()
                            {
                                ApplicationID = model.ApplicationId,
                                DocId = item.DocId,
                                BaseorTrans = item.BaseorTrans,
                                DocName = item.DocName,
                                FileId = item.FileId,
                                DocSource = item.DocSource
                            };
                            dbCtxt.SubmittedDocuments.Add(submitDocs);
                        }

                        else if (check_doc1.Count() > 0 && check_doc1.Count() == MyApplicationDocs.Count())
                        {
                            check_doc.FirstOrDefault().ApplicationID = model.ApplicationId;
                            check_doc.FirstOrDefault().DocId = item.DocId;
                            check_doc.FirstOrDefault().BaseorTrans = item.BaseorTrans;
                            check_doc.FirstOrDefault().DocName = item.DocName;
                            check_doc.FirstOrDefault().FileId = item.FileId;
                            check_doc.FirstOrDefault().DocSource = item.DocSource;
                        }
                        else
                        {
                            foreach (var item1 in check_doc1)
                            {
                                dbCtxt.SubmittedDocuments.Remove(item1);
                            }
                            dbCtxt.SaveChanges();


                            foreach (var item2 in MyApplicationDocs)
                            {

                                SubmittedDocument submitDocs = new SubmittedDocument()
                                {
                                    ApplicationID = model.ApplicationId,
                                    DocId = item2.DocId,
                                    BaseorTrans = item2.BaseorTrans,
                                    DocName = item2.DocName,
                                    FileId = item2.FileId,
                                    DocSource = item2.DocSource
                                };
                                dbCtxt.SubmittedDocuments.Add(submitDocs);
                            }
                            break;
                        }
                    }
                }


                if (model.LicenseTypeId == "ATO")
                {
                    TakeoverApp takeover = new TakeoverApp()
                    {
                        ApplicationID = model.ApplicationId,
                        LicenseReference = LinkedReference

                    };
                    dbCtxt.TakeoverApps.Add(takeover);
                }






                log.Info("About to Save ApplicationMaster1");
                if (isNew == true)
                {
                    dbCtxt.ApplicationRequests.Add(appRequest);
                }
                dbCtxt.SaveChanges();

                comment = "ApplicationForm Completed";




                log.Info("About to get WorkFlow Navigation");
                if (!commonHelper.isPaymentMade(model.ApplicationId, out errorMessage))
                {
                    log.Info("About to get WorkFlow Navigation");

                    var fieldlocation = dbCtxt.FieldLocations.Where(f => f.FieldType == "HQ").FirstOrDefault().FieldLocationID;
                    ResponseWrapper responseWrapper = workflowHelper.processAction(dbCtxt, model.ApplicationId, "Proceed", userMaster.UserId, comment, fieldlocation, "");

                    log.Info("Done with WorkFlow Navigation");
                    message = responseWrapper.value;
                    if (!responseWrapper.status)
                    {
                        status = "failure";
                        log.Error(message);
                    }
                }
                else
                {
                    message = "Application Modification was Successfull";
                }


                log.Info("Message =>" + message);
                log.Info("Done with License to Establish");

            }
            catch (DbEntityValidationException ex)
            {
                string l = string.Empty;
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        l += String.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }

                status = "failure";
                message = "Database Validation Error, Please Try Again Later";
                log.Info(l);
            }
            catch (Exception ex)
            {
                status = "failure";
                message = "System Error Occured When Maintaining Data, Please Try Again Later";
                log.Info(ex.InnerException);
            }

            return Json(new
            {
                Status = status,
                Message = message,
                applicationId = model.ApplicationId
            }, JsonRequestBehavior.AllowGet);
        }









        [HttpGet]
        public ActionResult Mistdo(string ApplicationId)
        {
            List<MistdoData> mistdo = new List<MistdoData>();
            var mistdolist = (from m in dbCtxt.MistdoDatas where m.ElpsId == userMaster.ElpsId && m.HasExpired == "NO" select m).ToList();
            ViewBag.Appref = ApplicationId;
            if (mistdolist.Count > 0)
            {
                foreach (var item in mistdolist)
                {
                    mistdo.Add(new MistdoData()
                    {
                        certificateExpiry = item.certificateExpiry,
                        certificateIssue = item.certificateIssue,
                        certificateNo = item.certificateNo,
                        email = item.email,
                        fullName = item.fullName,
                        phoneNumber = item.phoneNumber,
                        mistdoId = item.mistdoId

                    });

                    if (item.certificateExpiry < DateTime.Now)
                    {
                        item.HasExpired = "YES";
                        dbCtxt.SaveChanges();
                    }
                }
            }
            var mistdolist1 = (from m in dbCtxt.MistdoDatas where m.ElpsId == userMaster.ElpsId && m.HasExpired == "NO" select m).ToList();

            ViewBag.Mistdolist = mistdolist1.Count();

            return View(mistdo);
        }

        [HttpPost]
        public async Task<JsonResult> MistdoPost(string certificatid)
        {
            string success = string.Empty;
            string message = string.Empty;
            try
            {


                ElpsResponse response = await serviceIntegrator.MisdtdoPostAPI(certificatid);
                List<MistdoModel> mistdoResponse = (List<MistdoModel>)response.value;
                MistdoData mistdodata = new MistdoData();
                if (mistdoResponse != null)
                {
                    var expirydate = Convert.ToDateTime(mistdoResponse.FirstOrDefault().certificateExpiry);
                    var certificatenum = mistdoResponse.FirstOrDefault().certificateNo;

                    if (mistdoResponse.FirstOrDefault().success == true && expirydate > DateTime.Now)
                    {
                        var checkexistingrecord = (from m in dbCtxt.MistdoDatas where m.certificateNo == certificatenum select m).FirstOrDefault();

                        if (checkexistingrecord != null)
                        {
                            success = "failed";
                            message = "Mistdo certificate number already exist";
                        }
                        else
                        {
                            mistdodata.certificateExpiry = expirydate;
                            mistdodata.certificateIssue = Convert.ToDateTime(mistdoResponse.FirstOrDefault().certificateIssue);
                            mistdodata.certificateNo = mistdoResponse.FirstOrDefault().certificateNo;
                            mistdodata.email = mistdoResponse.FirstOrDefault().email;
                            mistdodata.fullName = mistdoResponse.FirstOrDefault().fullName;
                            mistdodata.phoneNumber = mistdoResponse.FirstOrDefault().phoneNumber;
                            mistdodata.mistdoId = mistdoResponse.FirstOrDefault().mistdoId;
                            mistdodata.ElpsId = userMaster.ElpsId;
                            mistdodata.AddedDate = DateTime.Now;
                            mistdodata.HasExpired = "NO";

                            dbCtxt.MistdoDatas.Add(mistdodata);

                            success = "success";
                            message = "Certificate Valid";
                        }
                    }
                    else if (mistdoResponse.FirstOrDefault().success == true && expirydate < DateTime.Now)
                    {
                        mistdodata.HasExpired = "YES";
                        success = "failed";
                        message = "Certificate has expired";
                    }
                    else
                    {
                        success = "failed";
                        message = "Invalid Certificate";
                    }
                    dbCtxt.SaveChanges();
                    var mistdolist = (from m in dbCtxt.MistdoDatas where m.ElpsId == userMaster.ElpsId && m.HasExpired == "NO" select m).ToList();
                    ViewBag.MistdolistPost = mistdolist.Count();
                }
                else
                {
                    success = "failed";
                    message = "Invalid Certificate";
                }
            }
            catch (Exception ex)
            {
                success = "failed";
                message = "An error occurred please try again later " + ex.Message;
            }

            return Json(new { Success = success, Message = message }, JsonRequestBehavior.AllowGet);
        }





        [HttpPost]
        public JsonResult DeleteMisdo(string certificatenbr)
        {
            string success = string.Empty;
            string message = string.Empty;
            try
            {
                var delete = (from m in dbCtxt.MistdoDatas where m.certificateNo == certificatenbr select m).FirstOrDefault();
                dbCtxt.MistdoDatas.Remove(delete);
                dbCtxt.SaveChanges();
                success = "success";
                message = "Record with the certificate number " + certificatenbr + " was successfully deleted";
            }
            catch (Exception ex)
            {
                success = "failed";
                message = "Unable to delete record " + ex.Message + " " + certificatenbr;
            }
            return Json(new { Success = success, Message = message }, JsonRequestBehavior.AllowGet);

        }












        [HttpGet]
        public ActionResult MyApplicationForm(string LicenseType)
        {
            string description = "";

            if (LicenseType == "ATM")
            {
                description = "Application For Approval To Modify";
            }
            else
            {
                description = "Application For Takeover";

            }

            ViewBag.AppType = LicenseType;
            ViewBag.Description = description;
            ViewBag.LoginElpsId = userMaster.ElpsId;
            ViewBag.Todaydate = DateTime.Now;
            return View();

        }




        public ActionResult GetSuitabilityRef()
        {
            var SuiRef = (from a in dbCtxt.ApplicationRequests where a.ApplicantUserId == userMaster.UserId && a.LicenseTypeId == "SSA" && a.LicenseReference != null && a.Status == "Approved" select new { a.LicenseReference }).ToList();
            return Json(SuiRef, JsonRequestBehavior.AllowGet);
        }










        [HttpGet]
        public JsonResult ModTODetails(string LicenseName, string CodeType)
        {
            var mydata = (from l in dbCtxt.ApplicationRequests where l.LicenseReference == LicenseName select l).FirstOrDefault();
            var facilityname = mydata==null?"":(from f in dbCtxt.Facilities where f.FacilityId == mydata.FacilityId select f.FalicityName).FirstOrDefault();

            var datas = (from a in dbCtxt.ApplicationRequests
                         join l in dbCtxt.LgaMasterLists on a.LgaCode equals l.LgaCode
                         join s in dbCtxt.StateMasterLists on a.StateCode equals s.StateCode
                         where a.LicenseReference == LicenseName && a.LicenseTypeId == CodeType
                         select new
                         {
                             LocationAddress = a.SiteLocationAddress,
                             StateCode = a.StateCode,
                             LGACode = a.LgaCode,
                             FacilityId = a.FacilityId,
                             Application = a.ApplicationId,
                             FacilityName = facilityname,
                             Capacity = a.StorageCapacity,
                             GPS = a.GPSCordinates,
                             LGA = l.LgaName,
                             State = s.StateName,
                             AdditionalInfo = a.AdditionalInfo,
                             ElpsID = dbCtxt.UserMasters.Where(x => x.UserId == a.ApplicantUserId).Select(x => x.ElpsId).FirstOrDefault(),
                             RegisteredAddress = a.RegisteredAddress,
                             ExpiryDate = mydata.LicenseExpiryDate.ToString(),
                         });

            return Json(datas.ToList(), JsonRequestBehavior.AllowGet);

        }





        [HttpGet]
        public ActionResult MyLegacyApplications()
        {
            //String errorMessage = null;
            List<SelectListItem> StateList = new List<SelectListItem>();
            List<SelectListItem> LgaList = new List<SelectListItem>();
            try
            {



                var appdetails = new List<ApplicationRequest>();
                var wksbackapp = (from p in dbCtxt.ApplicationRequests
                                  where p.ApplicantUserId == userMaster.UserId && p.IsLegacy == "YES"
                                  select new
                                  {
                                      p.ApplicationId,
                                      p.ApplicantName,
                                      p.LicenseTypeId,
                                      p.ApplicationTypeId,
                                      p.RegisteredAddress,
                                      p.SiteLocationAddress,
                                      p.StorageCapacity,
                                      p.LicenseReference,
                                      p.LicenseIssuedDate,
                                      p.LicenseExpiryDate,
                                      p.IsLegacy,
                                      p.CurrentStageID,
                                      p.AddedDate,
                                      p.Status,
                                      p.GPSCordinates

                                  }).ToList();
                foreach (var item in wksbackapp)
                {
                    appdetails.Add(new ApplicationRequest()
                    {
                        ApplicationId = item.ApplicationId,
                        ApplicantName = item.ApplicantName,
                        LicenseTypeId = item.LicenseTypeId,
                        ApplicationTypeId = item.ApplicationTypeId,
                        RegisteredAddress = item.RegisteredAddress,
                        SiteLocationAddress = item.SiteLocationAddress,
                        StorageCapacity = item.StorageCapacity,
                        LicenseReference = item.LicenseReference,
                        LicenseIssuedDate = item.LicenseIssuedDate,
                        LicenseExpiryDate = item.LicenseExpiryDate,
                        IsLegacy = item.IsLegacy,
                        CurrentStageID = item.CurrentStageID,
                        AddedDate = item.AddedDate,
                        Status = item.Status,
                        GPSCordinates = item.GPSCordinates
                    });
                }
                ViewBag.ApplicationDetails = appdetails;
                ViewBag.State = new SelectList(GlobalModel.AllStates.OrderBy(c => c.StateName), "StateCode", "StateName");

                ViewBag.LGA = LgaList;

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                ViewBag.ResponseMessage = "Error Occured Getting  Application List, Please Try Again Later";
            }
            return View();
        }





        [HttpPost]
        public ActionResult GetExtraPayment()
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
                         //join e in dbCtxt.ApplicationRequests on p.ApplicationID equals e.ApplicationId
                         where (p.Status == "Pending" || p.Status == "ExtraPaymt") && p.ApplicantID == userMaster.UserId
                         
                         select new
                         {
                             p.ApplicationID,
                             p.ExtraPaymentAppRef,
                             p.Status,
                             p.SanctionType,
                             p.Description,
                             p.PenaltyCode,
                             amt = p.TxnAmount.ToString(),
                             servicecharge= Math.Round(((decimal)(p.TxnAmount) * (decimal)(0.05)), 2),
                             total = Math.Round((decimal)(p.TxnAmount) * (decimal)(0.05) + (decimal)p.TxnAmount,2)
                         });
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                staff = staff.OrderBy(s => s.ApplicationID + " " + sortColumnDir);
            }
            else
            {
                staff = staff.Where(a => a.ApplicationID.Contains(searchTxt) || a.ApplicationID.Contains(searchTxt)
               || a.Status.Contains(searchTxt) || a.amt.Contains(searchTxt));
            }
            totalRecords = staff.Count();
            var data = staff.Skip(skip).Take(pageSize).ToList();
            switch (sortColumn)
            {
                case "0":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ApplicationID).ToList() : data.OrderBy(p => p.ApplicationID).ToList();
                    break;
                case "1":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ExtraPaymentAppRef).ToList() : data.OrderBy(p => p.ExtraPaymentAppRef).ToList();
                    break;
                case "2":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.amt).ToList() : data.OrderBy(p => p.amt).ToList();
                    break;
                case "3":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.Status).ToList() : data.OrderBy(p => p.Status).ToList();
                    break;
                case "4":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.SanctionType).ToList() : data.OrderBy(p => p.SanctionType).ToList();
                    break;
                case "5":
                    data = sortColumnDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.Description).ToList() : data.OrderBy(p => p.Description).ToList();
                    break;

            }
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);

        }









        [HttpGet]
        public ActionResult ExtraPaymentsReport()
        {
            List<ExtraPayment> ExtraPaymentList = new List<ExtraPayment>();
            try
            {
                ExtraPaymentList = dbCtxt.ExtraPayments.Where(p => p.ApplicantID == userMaster.UserId).ToList();
                ViewBag.ResponseMessage = "SUCCESS";
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                ViewBag.ResponseMessage = "Error Occured Getting Payment List, Please Try Again Later";
            }

            return View(ExtraPaymentList);
        }






        [HttpGet]
        public ActionResult MakeExtraPayment()
        {
            return View();
        }






        [HttpGet]
        public JsonResult GetRenewLicenseDetails(string LicenseName)
        {

            var mydata = (from l in dbCtxt.ApplicationRequests where l.LicenseReference == LicenseName select l).FirstOrDefault();
            //var sum = (from t in dbCtxt.Tanks where t.FacilityId == mydata.FacilityId select t).Sum(t => t.Capacity);
            var facilityname = (from f in dbCtxt.Facilities where f.FacilityId == mydata.FacilityId select f.FalicityName).FirstOrDefault();

            var datas = (from a in dbCtxt.ApplicationRequests
                         join l in dbCtxt.LgaMasterLists on a.LgaCode equals l.LgaCode
                         join s in dbCtxt.StateMasterLists on a.StateCode equals s.StateCode
                         where a.LicenseReference == LicenseName && a.ApplicantUserId == userMaster.UserId
                         select new
                         {


                             SiteLocationAddress = a.SiteLocationAddress,
                             RegisteredAddress = a.RegisteredAddress,
                             StateCode = a.StateCode,
                             LgaCode = a.LgaCode,
                             FacilityId = a.FacilityId,
                             FacilityName = facilityname,
                             LandSize = a.LandSize,
                             LGA = l.LgaName,
                             State = s.StateName,
                             GPSCordinates = a.GPSCordinates,
                             BeaconLocations = a.BeaconLocations,
                             AccessRoadToFromSite = a.AccessRoadToFromSite,
                             AnyEquipmentOnSite = a.AnyEquipmentOnSite,
                             LandTopology = a.LandTopology,
                             NatureOfArea = a.NatureOfArea,
                             AnnualCumuBaseOilRequirementCapacity = a.AnnualCumuBaseOilRequirementCapacity,
                             AnnualProductionProjectionCapacity = a.AnnualProductionProjectionCapacity,
                             AdjoiningProperties = a.AdjoiningProperties,
                             RelationshipWithPipelineRightOfWay = a.RelationshipWithPipelineRightOfWay,
                             RelationshipWithSensitiveInstitutions = a.RelationshipWithSensitiveInstitutions,
                             RelationshipWithStreams = a.RelationshipWithStreams,
                             RelationshipWithRailwayLine = a.RelationshipWithRailwayLine,
                             RelationshipWithPHCNTensionLines = a.RelationshipWithPHCNTensionLines,
                             AdditionalInfo = a.AdditionalInfo
                         });

            return Json(datas.ToList(), JsonRequestBehavior.AllowGet);

        }




        [HttpGet]
        public JsonResult GetApprovedLicense(string LicenseTypeId)
        {
            var approvedLicense = (from r in dbCtxt.ApplicationRequests
                                   where r.LicenseTypeId == LicenseTypeId && r.ApplicantUserId == userMaster.UserId && !string.IsNullOrEmpty(r.LicenseReference) && r.Status == "ACTIVE" &&
                                       string.IsNullOrEmpty(r.LinkedReference)
                                   select new
                                   {
                                       Id = r.LicenseReference,
                                       Description = r.ApplicationId + " (" + r.LicenseTypeId + ")"
                                   }).ToList();
            return Json(approvedLicense, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public JsonResult CheckForLegacyApplication()
        {
            var appTypes = (from r in dbCtxt.ApplicationRequests
                            where (r.ApplicantUserId == userMaster.UserId && r.Status == "ACTIVE" && r.IsLegacy == "YES" && r.WorkFlowState.StateType == "CONTINUE")
                            select new
                            {
                                Description = r.LicenseType.Description,
                                Id = r.ApplicationId
                            }).ToList();
            return Json(appTypes, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GenerateLegacyApplicationNo()
        {
            string status = "success";
            string legacyId = "";
            string message = "";

            try
            {
                legacyId = userMaster.ElpsId + "LCY" + commonHelper.GenerateApplicationNo();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                status = "failure";
                message = "Error occured Generating Legacy Application Reference";
            }

            return Json(new
            {
                Status = status,
                Message = message,
                applicationId = legacyId
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GenerateNormalApplicationNo()
        {
            string message = "";
            string status = "success";
            string applicationId = "";

            try
            {
                applicationId = commonHelper.GenerateApplicationNo();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                status = "failure";
                message = "Error occured Generating Application Reference";
            }

            return Json(new
            {
                Status = status,
                Message = message,
                applicationId = applicationId
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult GetMyApplicationChart(MyApplicationData myData)
        {

            Appointment appointment = null;
            string responseMessage = null;
            Dictionary<string, int> appStatistics;
            Dictionary<string, int> appStageReference = null;
            myData.TotalCount = 0;
            myData.PermitCount = 0;
            myData.ProcessingCount = 0;
            myData.YetToSubmitCount = 0;
            myData.ExpiringCount = 0;
            myData.AppointmentCount = 0;

            try
            {
                appStatistics = appDocHelper.GetApplicationStatistics(userMaster.UserId, out responseMessage, out appStageReference);
                myData.TotalCount = appStatistics["ALL"];
                myData.PermitCount = appStatistics["PEM"];
                myData.ProcessingCount = appStatistics["PROC"];
                myData.YetToSubmitCount = appStatistics["YTST"];
                myData.ExpiringCount = appStatistics["EXP"];

            }
            catch (Exception ex) { }

            return Json(myData, JsonRequestBehavior.AllowGet);
        }

    }
}