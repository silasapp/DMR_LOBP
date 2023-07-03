using log4net;
using LOBP.DbEntities;
using LOBP.Helper;
using LOBP.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace LOBP.Jobs {
 public class NotificationService: IJob {
  private ElpServiceHelper serviceIntegrator;
  private UserHelper userMasterHelper;
  private WorkFlowHelper workflowManager;
  private ILog log = log4net.LogManager.GetLogger(typeof(NotificationService));
  public void Execute(IJobExecutionContext context) {

   try {
    using(var dbCtxt = new LubeBlendingDBEntities()) {

     log.Info("Notification Service Triggered");
     userMasterHelper = new UserHelper(dbCtxt);
    // workflowManager = new WorkFlowHelper(dbCtxt);
     serviceIntegrator = new ElpServiceHelper(GlobalModel.elpsUrl, GlobalModel.appEmail, GlobalModel.appKey);
     DateTime last5date = DateTime.Today.Date.AddDays(-5);
     log.Info("Last Interval date =>"+last5date);
     //DbFunctions.TruncateTime(p.ActionDate) >= DateTime.Today.Date.AddDays(-5)

     foreach(ActionHistory actionHistory in dbCtxt.ActionHistories.Where(p => p.StatusMode != "NORMAL" && p.ActionDate >= last5date ).ToList()) {}

    }
   } catch (Exception ex) {
    log.Error(ex.StackTrace + Environment.NewLine + "InnerException =>" + ex.InnerException);
   }

  }

 }
}