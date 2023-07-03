using log4net;
using LOBP.DbEntities;
using LOBP.Filters;
using LOBP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LOBP.Controllers {

 public class HomeController: Controller {

  private LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities();
  private ILog log = log4net.LogManager.GetLogger(typeof(HomeController));


  // GET: /Home/
  [AllowAnonymous]
  public ActionResult Index() {
   try {
    log.Info("Coming Into LOBP Home Index");
    ViewBag.ElpsURL = GlobalModel.elpsUrl;
    ViewBag.AppID = GlobalModel.appId;
    ViewBag.AppEmail = GlobalModel.appEmail;
    ViewBag.AppName = GlobalModel.appName;

    if (!(string.IsNullOrEmpty(GlobalModel.elpsUrl) && string.IsNullOrEmpty(GlobalModel.appKey) && string.IsNullOrEmpty(GlobalModel.appEmail) && string.IsNullOrEmpty(GlobalModel.appName))) {
     ViewBag.ErrorMessage = "SUCCESS";
    } else {
     ViewBag.ErrorMessage = "Lube Portal Model Fail to be Instantiated Successfully, Please Try Again Later";
    }


   } catch (Exception ex) {
    ViewBag.ErrorMessage = "Exception Occured Loading Lube Portal Home, Please Try Again Later";
    log.Error(ex.StackTrace);
   }

   return View();
  }


  // GET: /faq/
  [AllowAnonymous]
  public ActionResult faq() {
   return View();
  }


  protected override void Dispose(bool disposing) {
   if (disposing) {
    dbCtxt.Dispose();
   }
   base.Dispose(disposing);
  }


 }
}