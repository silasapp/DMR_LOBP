using log4net;
using LOBP.DbEntities;
using LOBP.Helper;
using LOBP.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LOBP.Jobs
{
    public class PaymentCheckerService : IJob
    {

        private ElpServiceHelper serviceIntegrator;
        private UserHelper userMasterHelper;
        private WorkFlowHelper workflowManager;
        private ILog log = log4net.LogManager.GetLogger(typeof(PaymentCheckerService));


        public void Execute(IJobExecutionContext context)
        {

            try
            {
                using (var dbCtxt = new LubeBlendingDBEntities())
                {

                    userMasterHelper = new UserHelper(dbCtxt);
                    //workflowManager = new WorkFlowHelper(dbCtxt);
                    serviceIntegrator = new ElpServiceHelper(GlobalModel.elpsUrl, GlobalModel.appEmail, GlobalModel.appKey);


                    foreach (PaymentLog paymentLog in dbCtxt.PaymentLogs.Where(p => p.Status != "AUTH" && p.PaymentType == "MAIN").ToList())
                    {

                        log.Info("About To Verify Payment Status from Remita with Application Id =>" + paymentLog.ApplicationId);
                        ElpsResponse elpsResponse = serviceIntegrator.GetTransactionStatus(paymentLog.ApplicationId);
                        log.Info("Service Call Response =>" + elpsResponse.message);

                        if (elpsResponse.message.Contains("SUCCESS"))
                        {
                            TransactionStatus txnstatus = (TransactionStatus)elpsResponse.value;
                            log.Info("Payment Status =>" + txnstatus.status + ",Status Message =>" + txnstatus.statusmessage);
                            paymentLog.TxnMessage = txnstatus.status + " - " + txnstatus.statusmessage;

                            if (txnstatus.status == "00" || txnstatus.status == "01")
                            {
                                paymentLog.Status = "AUTH";
                            }
                        }
                        else
                        {
                            paymentLog.TxnMessage = elpsResponse.message;
                        }

                        paymentLog.LastRetryDate = DateTime.Now;
                        paymentLog.RetryCount = paymentLog.RetryCount.GetValueOrDefault() + 1;
                        dbCtxt.SaveChanges();

                        ResponseWrapper responseWrapper = null;
                        if (paymentLog.Status == "AUTH" && paymentLog.PaymentType == "MAIN")
                        {
                            UserMaster adrbpUser = dbCtxt.UserMasters.Where(u => u.UserRoles.Contains("ADOPERATIONRBP") && u.Status == "ACTIVE").FirstOrDefault();
                            if (adrbpUser != default(UserMaster))
                            {
                                var fieldlocation = adrbpUser.UserLocation;
                                responseWrapper = workflowManager.processAction(dbCtxt, paymentLog.ApplicationId.Trim(), "Submit", paymentLog.ApplicantUserId, "Application Reference " + paymentLog.ApplicationId + " have been Submitted to DPR", fieldlocation,"");
                            }
                            else
                            {
                                log.Info("No Active ADOPERATIONRBP Found on the LOBP Platform");
                            }
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace + Environment.NewLine + "InnerException =>" + ex.InnerException);
            }
        }
    }
}