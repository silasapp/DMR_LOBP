using LOBP.DbEntities;
using LOBP.Jobs;
using LOBP.Models;
using log4net;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace LOBP
{
    public class MvcApplication : System.Web.HttpApplication
    {

        private IScheduler scheduler;
        private LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities();
        private ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
        private ILog logger = log4net.LogManager.GetLogger(typeof(MvcApplication));

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            // WebApiConfig.Register(GlobalConfiguration.Configuration);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            log4net.Config.XmlConfigurator.Configure();
            initServices();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        }

        private void initServices()
        {
            try
            {
                GlobalModel.AllStates = dbCtxt.StateMasterLists.ToList();
                GlobalModel.AllLgas = dbCtxt.LgaMasterLists.ToList();
                GlobalModel.AllWorkFlowNavigation = dbCtxt.WorkFlowNavigations.ToList();
              
                logger.Info("About to initialise Global Data Models");
                GlobalModel.appId = dbCtxt.Configurations.Where(c => c.ParamID == "APP_ID").FirstOrDefault().ParamValue;
                logger.Info("Global AppID =>" + GlobalModel.appId);
                GlobalModel.appKey = dbCtxt.Configurations.Where(c => c.ParamID == "APP_KEY").FirstOrDefault().ParamValue;
                logger.Info("Global AppKey =>" + GlobalModel.appKey);
                GlobalModel.elpsUrl = dbCtxt.Configurations.Where(c => c.ParamID == "ELPS_URL").FirstOrDefault().ParamValue;
                logger.Info("Global ElpsURL =>" + GlobalModel.elpsUrl);
                GlobalModel.appName = dbCtxt.Configurations.Where(c => c.ParamID == "APP_NAME").FirstOrDefault().ParamValue;
                logger.Info("Global AppName =>" + GlobalModel.appName);
                GlobalModel.appEmail = dbCtxt.Configurations.Where(c => c.ParamID == "APP_EMAIL").FirstOrDefault().ParamValue;
                logger.Info("Global AppEmail =>" + GlobalModel.appEmail);
                logger.Info("Done With Global Model initialisation");

                string paymentVerifyTimer = dbCtxt.Configurations.Where(c => c.ParamID == "PMT_VERIFY_TIMER").FirstOrDefault().ParamValue;
                logger.Info("PaymentCheckerService Timer =>" + paymentVerifyTimer);
                string emailNotifTimer = dbCtxt.Configurations.Where(c => c.ParamID == "EMAIL_NOTI_TIMER").FirstOrDefault().ParamValue;
                logger.Info("Notification Timer =>" + emailNotifTimer);
                string adhocLicenseTimer = dbCtxt.Configurations.Where(c => c.ParamID == "ADHOC_LICENSE_TIMER").FirstOrDefault().ParamValue;
                logger.Info("AdHocLicense Timer =>" + adhocLicenseTimer);

                scheduler = schedulerFactory.GetScheduler();

                IJobDetail paymentCheckerJob = JobBuilder.Create<PaymentCheckerService>().WithIdentity("PaymentCheckerService", "JobGroup1").Build();
                var paymentCheckerTrigger = TriggerBuilder.Create().WithIdentity("PaymentCheckerServiceTrigger", "TriggerGroup1").WithCronSchedule(paymentVerifyTimer).ForJob(paymentCheckerJob.Key).Build();
                logger.Info("PaymentCheckerService JobDetailTrigger Created");

                IJobDetail notificationJob = JobBuilder.Create<NotificationService>().WithIdentity("NotificationService", "JobGroup1").Build();
                var notificationTrigger = TriggerBuilder.Create().WithIdentity("NotificationServiceTrigger", "TriggerGroup1").WithCronSchedule(emailNotifTimer).ForJob(notificationJob.Key).Build();
                logger.Info("NotificationService JobDetailTrigger Created");

                IJobDetail adhocLicenseJob = JobBuilder.Create<AdHocLicenseService>().WithIdentity("AdHocLicenseService", "JobGroup1").Build();
                var adhocLicenseTrigger = TriggerBuilder.Create().WithIdentity("AdHocLicenseServiceTrigger", "TriggerGroup1").WithCronSchedule(adhocLicenseTimer).ForJob(adhocLicenseJob.Key).Build();
                logger.Info("AdHocService JobDetailTrigger Created");

                scheduler.ScheduleJob(paymentCheckerJob, paymentCheckerTrigger);
                scheduler.ScheduleJob(notificationJob, notificationTrigger);
                scheduler.ScheduleJob(adhocLicenseJob, adhocLicenseTrigger);

                logger.Info("All Jobs Scheduled");
                scheduler.Start();
                logger.Info("All Jobs Started");

                logger.Info("Services Was Initialise SUCCESSFULLY");
            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
                logger.Error("Services FAILED to Initialize Well");
            }
        }

        protected void Application_End()
        {
            if (scheduler !=
             default(IScheduler))
            {
                if (scheduler.IsShutdown == false)
                {
                    scheduler.Shutdown();
                }
            }

            if (dbCtxt !=
             default(LubeBlendingDBEntities))
            {
                dbCtxt.Dispose();
            }
        }



    }
}
