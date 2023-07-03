using System;
using LOBP.Helper;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

[assembly: WebActivator.PostApplicationStartMethod(typeof(LOBP.App_Start.BackgroundServices), "Start")]
[assembly: WebActivator.ApplicationShutdownMethod(typeof(LOBP.App_Start.BackgroundServices), "Shutdown")]

namespace LOBP.App_Start
{
     public static class  BackgroundServices
    {

        private static  BackgroundPaymentCheck m_job;
        private static EmailReminderMonitorJob e_job;
        private static PermitExpiryReminder ex_job;
        //private static IGRPaymentPostCheck igr_job;

        public static void Start()
        {
           
            m_job  = new BackgroundPaymentCheck (TimeSpan.FromMinutes(15));
            e_job =  new EmailReminderMonitorJob(TimeSpan.FromDays(3));
            ex_job = new PermitExpiryReminder(TimeSpan.FromDays(2));
            //igr_job = new IGRPaymentPostCheck(TimeSpan.FromDays(14));

        }

        public static void Shutdown()
        {
            m_job.Dispose();
            e_job.Dispose();
            ex_job.Dispose();
            //igr_job.Dispose();
        }
    }

}