using log4net;
using LOBP.Helper;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LOBP.Jobs
{
    public class AdHocLicenseService : IJob
    {

        private ElpServiceHelper serviceIntegrator;
        private UserHelper userMasterHelper;
        private WorkFlowHelper workflowManager;
        private ILog log = log4net.LogManager.GetLogger(typeof(AdHocLicenseService));

        public void Execute(IJobExecutionContext context) { }


    }
}