using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LOBP.Models;
using LOBP.Helper;
using System.Web.Routing;
using log4net;
using LOBP.DbEntities;

namespace LOBP.Controllers
{

    public class AccountController : Controller
    {

        private UserHelper userHelper;
        public static string roleid;
        public static string elpsid;
        private ElpServiceHelper serviceIntegrator;
        private UtilityHelper utilityHelper = new UtilityHelper();
        private LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities();
        private ILog logger = log4net.LogManager.GetLogger(typeof(AccountController));
        CompanyDetail compDto = null;
        CompanyInformationModel compDetailsModel = new CompanyInformationModel();

        protected override void Initialize(RequestContext requestContext)
        {
            try
            {
                base.Initialize(requestContext);
                userHelper = new UserHelper(dbCtxt);
                serviceIntegrator = new ElpServiceHelper(GlobalModel.elpsUrl, GlobalModel.appEmail, GlobalModel.appKey);
            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
            }
        }


        [AllowAnonymous]
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        //[HttpPost]
        //[HttpGet]
        public ActionResult Login(string email, string code)
        {
            string responseMessage = string.Empty;
            try
            {
                
                var issuccess = userHelper.CodeCheck(email, code);
                if (issuccess == true || issuccess == false)
                {

                    logger.Info("Coming To Login User with Email =>" + email);
                    string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(ipAddress))
                    {
                        ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                    }

                    logger.Info("Client IpAddress =>" + ipAddress);
                    string validateResult = validateUser(email, ipAddress, System.Web.HttpContext.Current.Request.Browser);
                    logger.Info("Validate User Result =>" + validateResult);

                    if (validateResult == "SUCCESS")
                    {
                        

                        Session["UserEmail"] = email;
                        roleid = ((UserMaster)Session["UserID"]).UserRoles;
                        elpsid = ((UserMaster)Session["UserID"]).ElpsId;
                        if (!roleid.Contains("COMPANY"))
                        {
                            userHelper.UpdateNewEmailDomain(email);
                        }

                        if (roleid.Contains("COMPANY"))
                        {
                            ElpsResponse elpsResponse = serviceIntegrator.GetCompanyDetailByEmail(elpsid);
                            compDto = (CompanyDetail)elpsResponse.value;
                            elpsResponse = serviceIntegrator.GetAddressByID(Convert.ToString(compDto.registered_Address_Id));
                            compDetailsModel.registeredAddress = (CompanyAddressDTO)elpsResponse.value;
                            var firsttimercount = (from u in dbCtxt.UserMasters where u.UserId == email select u.LoginCount).FirstOrDefault();
                            var emptyregaddress = compDetailsModel.registeredAddress == null ? null : compDetailsModel.registeredAddress.address_1;

                            if (firsttimercount == 1 || emptyregaddress == null)
                            {
                                return RedirectToAction("CompanyProfile", "Company");

                            }
                            else
                            {
                                return RedirectToAction("Index", "Company");
                            }
                            //}
                        }
                        else
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                    }
                    else
                    {
                        return Content("<html><head><script>alert(\"" + validateResult + "\");window.location.replace('LogOff')</script></head></html>");
                    }

                }
                else
                {
                    return Content("An Error Occurred while Logging in to portal.");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
                return Content("<html><head><script>alert(\"" + ex.Message + "\");window.location.replace('LogOff')</script></head></html>");
            }
        }









        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult LogOff()
        {
            try
            {
                logger.Info("About to LogOff User");
                Session.Clear();
                var elpsLogOffUrl = GlobalModel.elpsUrl + "/Account/RemoteLogOff";
                var returnUrl = Request.Url.GetLeftPart(UriPartial.Authority) + Request.ApplicationPath + "Home";
                logger.Info(returnUrl);
                var frm = "<form action='" + elpsLogOffUrl + "' id='frmTest' method='post'>" + "<input type='hidden' name='returnUrl' value='" + returnUrl + "' />" + "<input type='hidden' name='appId' value='" + GlobalModel.appKey + "' />" + "</form>" + "<script>document.getElementById('frmTest').submit();</script>";
                return Content(frm, "text/html");
            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace + Environment.NewLine + "InnerException =>" + ex.InnerException);
                return RedirectToAction("Index", "Home");
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dbCtxt.Dispose();
            }
            base.Dispose(disposing);
        }


        #region Helpers
        private string validateUser(string Email, string ipAddress, HttpBrowserCapabilities browser)
        {
            string responseMessage = string.Empty;
            logger.Info("Coming To validateUser User =>" + Email);
            UserMaster userMaster = null;
            try
            {
                if (string.IsNullOrEmpty(Email))
                {
                    return "UserId should Not be Empty";
                }
                Session.Clear();
                Session.RemoveAll();

                logger.Info("About To Retrieve UserMaster Details from the System");
                var email = Email.Split('@');
                string firstsplit = email[0];
                userMaster = dbCtxt.UserMasters.Where(c => c.UserId.Contains(firstsplit)).FirstOrDefault();

                if (userMaster?.UserType == "ADMIN")
                {
                    userMaster = dbCtxt.UserMasters.Where(c => c.UserId.Contains(firstsplit)).FirstOrDefault();
                }
                else
                {
                    userMaster = dbCtxt.UserMasters.Where(c => c.UserId.Trim() == Email.Trim()).FirstOrDefault();
                }
                logger.Info("User Details => " + userMaster);

                if (userMaster ==
                 default(UserMaster))
                {
                    userMaster = userHelper.AddCompanyUser(serviceIntegrator, Email, out responseMessage);
                }
                else
                {
                    userMaster.LastLogin = DateTime.UtcNow;
                    userMaster.LoginCount = userMaster.LoginCount + 1;
                    if (userMaster.UserRoles.Contains("COMPANY"))
                    {
                        userHelper.UpdateCompanyUser(dbCtxt, serviceIntegrator, Email, userMaster.UserRoles);
                    }
                }

                if (userMaster !=
                 default(UserMaster))
                {
                    logger.Info("User Master Status => " + userMaster.Status);

                    UserLogin userLogin = new UserLogin();
                    userLogin.UserId = userMaster.UserId;
                    userLogin.UserType = userMaster.UserType;
                    userLogin.Browser = "Browser Type = " + browser.Type + "," + "Platform = " + browser.Platform + "," + "Screen Resolution = " + browser.ScreenPixelsWidth + "px X " + browser.ScreenPixelsHeight + "px";
                    userLogin.Client = ipAddress;
                    userLogin.LoginMessage = (userMaster.Status == "ACTIVE") ? "LOGGED" : userMaster.FirstName + "(" + Email + ") is not Active on the platform";
                    userLogin.LoginTime = DateTime.Now;
                    userLogin.Status = userMaster.Status;
                    dbCtxt.UserLogins.Add(userLogin);
                    dbCtxt.SaveChanges();

                    logger.Info("About To Maintain User Information on Session");
                    Session["UserID"] = userMaster;
                    Session["Email"] = userMaster.UserId;
                    Session["UserName"] = userMaster.FirstName;
                    logger.Info("Done With User Session Maintenance");

                    return "SUCCESS";
                }
                else
                {
                    return responseMessage;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
                return "An Error Occured Validating User,Please try again Later";
            }
        }

        #endregion

    }
}