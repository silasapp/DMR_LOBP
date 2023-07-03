using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LOBP.Filters {
 public class SessionExpireTracker: ActionFilterAttribute {

  private ILog logger = log4net.LogManager.GetLogger(typeof(SessionExpireTracker));
  public override void OnActionExecuting(ActionExecutingContext filterContext) {

   try {
    if (filterContext.HttpContext.Session["UserID"] == null) {
     filterContext.Result = new RedirectResult("~/Account/LogOff");
     return;
    }
    base.OnActionExecuting(filterContext);
   } catch (Exception ex) {
    logger.Info("Exception => " + ex.Message);
    filterContext.Result = new RedirectResult("~/Account/LogOff");
    return;
   }

  }
 }
}