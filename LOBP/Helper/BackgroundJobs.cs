using LOBP.DbEntities;
using LOBP.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace LOBP.Helper
{

    public class BackgroundJobs
    {

    }

   

    public class BackgroundPaymentCheck : IDisposable
    {
        private CancellationTokenSource m_cancel;
        private Task m_task;
        private TimeSpan m_interval;
        private bool m_running;
        private ElpServiceHelper serviceIntegrator = new ElpServiceHelper();
        private WorkFlowHelper workflowHelper = new WorkFlowHelper();
        private LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities();
        private UserHelper userHelper = new UserHelper();







        public BackgroundPaymentCheck(TimeSpan interval)
        {
            m_interval = interval;

            m_running = true;
            m_cancel = new CancellationTokenSource();
             m_task = Task.Run(() =>PaymentCheck(), m_cancel.Token);

        }

        private async void PaymentCheck()
        {


            while (m_running)
            {
                serviceIntegrator = new ElpServiceHelper(GlobalModel.elpsUrl, GlobalModel.appEmail, GlobalModel.appKey);


                ResponseWrapper responseWrapper;
                try
                {
                    List<ApplicationRequest> changecurrentstage = null;
                    using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
                    {
                         List <ExtraPayment> extrapayment = (from e in db.ExtraPayments where e.Status == "Pending" && e.RRReference != null select e).ToList();
                        List<PaymentLog>  paymentlog =  ( from p in db.PaymentLogs where (p.Status == "INIT" || (p.Status == "AUTH" || p.Status == "FAIL") && (p.ApplicationRequest.CurrentStageID == 2 || p.ApplicationRequest.CurrentStageID == 3 || p.ApplicationRequest.CurrentStageID == 4)) && p.RRReference != null select p).ToList();




                        if (extrapayment.Count() > 0)
                        {
                             foreach (var item in extrapayment)
                            {

                                var res = serviceIntegrator.CheckRRR(item.RRReference);
                                var resp = JsonConvert.DeserializeObject<JObject>(res.value.ToString());
                                if (resp.GetValue("message").ToString() == "Approved" || resp.GetValue("message").ToString() == "Successful" || resp.GetValue("status").ToString() == "01")
                                {
                                    item.Status = "AUTH";
                                }
                                await Task.Delay(db.SaveChanges());
                            }
                        }



                        if (paymentlog.Count > 0)
                        {
                             foreach (var item1 in paymentlog)
                            {

                                var res = serviceIntegrator.CheckRRR(item1.RRReference);
                                var resp = JsonConvert.DeserializeObject<JObject>(res.value.ToString());
                                changecurrentstage = (from a in db.ApplicationRequests where a.ApplicationId == item1.ApplicationId select a).ToList();

                                if (resp.GetValue("message").ToString() == "Approved" || resp.GetValue("message").ToString() == "Successful" || resp.GetValue("status").ToString() == "01")
                                {


                                    if (changecurrentstage != null)
                                    {
                                        if (changecurrentstage.FirstOrDefault().Status != "Rejected" && changecurrentstage.FirstOrDefault().CurrentStageID < 5)
                                        {
                                             responseWrapper = workflowHelper.processAction(dbCtxt, item1.ApplicationId, "Proceed", item1.ApplicantUserId, "Document Submitted", changecurrentstage.FirstOrDefault().CurrentOfficeLocation, "");

                                            responseWrapper =  workflowHelper.processAction(dbCtxt, item1.ApplicationId, "GenerateRRR", item1.ApplicantUserId, "Remita Retrieval Reference Generated", changecurrentstage.FirstOrDefault().CurrentOfficeLocation, "");

                                            responseWrapper = workflowHelper.processAction(dbCtxt, item1.ApplicationId, "Submit", item1.ApplicantUserId, "Application Reference " + item1.ApplicationId + " have been Submitted to DPR", changecurrentstage.FirstOrDefault().CurrentOfficeLocation, "");


                                        }

                                    }

                                    item1.TxnMessage = "01 - Approved";
                                    item1.Status = "AUTH";

                                }
                                await Task.Delay(db.SaveChanges());
                            }

                             Thread.Sleep(m_interval);
                        }



                    }
                }
                catch (Exception ex)
                {
                }


            }
        }



        public void Dispose()
        {
            m_running = false;

            if (m_cancel != null)
            {
                try
                {
                    m_cancel.Cancel();
                    m_cancel.Dispose();
                }
                catch
                {
                }
                finally
                {
                    m_cancel = null;
                }
            }
        }
    }







    public class PermitExpiryReminder : IDisposable
    {
        private CancellationTokenSource ex_cancel;
        private Task ex_task;
        private TimeSpan ex_interval;
        private bool ex_running;
        private UserHelper userHelper = new UserHelper();
        public PermitExpiryReminder(TimeSpan interval)
        {
            ex_interval = interval;
            ex_running = true;
            ex_cancel = new CancellationTokenSource();
            ex_task = Task.Run(() => ExpiryReminder(), ex_cancel.Token);
        }


        private void ExpiryReminder()
        {
            while (ex_running)
            {
                string status = "";
                try
                {
                    using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
                    {
                        var dt = DateTime.UtcNow;

                        List<ApplicationRequest> appreq = (from e in db.ApplicationRequests where e.LicenseExpiryDate != null && e.LicenseReference != null && e.IsLegacy == "NO" select e).ToList();

                        if (appreq.Count > 0)
                        {
                            foreach (var item in appreq)
                            {
                                if (item.LicenseExpiryDate.Value.Subtract(dt).Days <= 14 && item.LicenseExpiryDate.Value.Date >= dt.Date) {
                                    var subject = "REMINDER:-License/Approval Expiry Reminder";
                                    var content = "Application with the reference number " + item.ApplicationId + " will expire in less than 14 days. Please kindly renew.";
                                 var sendemail = userHelper.SendStaffEmailMessage(item.ApplicantUserId, subject, content);
                                    status = "done"; 
                                }

                            }
                        }



                    }
                }
                catch (Exception ex)
                {
                    status = "failed";
                }


                Thread.Sleep(ex_interval);


            }
        }




        public void Dispose()
        {
            ex_running = false;

            if (ex_cancel != null)
            {
                try
                {
                    ex_cancel.Cancel();
                    ex_cancel.Dispose();
                }
                catch
                {
                }
                finally
                {
                    ex_cancel = null;
                }
            }
        }
    }







    public class IGRPaymentPostCheck : IDisposable
    {
        private CancellationTokenSource ex_cancel;
        private Task ex_task;
        private TimeSpan ex_interval;
        private bool ex_running;
        private ElpServiceHelper serviceIntegrator = new ElpServiceHelper();

        public IGRPaymentPostCheck(TimeSpan interval)
        {
            ex_interval = interval;
            ex_running = true;
            ex_cancel = new CancellationTokenSource();
            ex_task = Task.Run(() => IGRPost(), ex_cancel.Token);
        }



        private async void IGRPost()
        {

            while (ex_running)
            {
                string status = "";
                try
                {
                    using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
                    {
                        var dt = DateTime.UtcNow;

                        var appreq = (from e in db.ApplicationRequests
                                      join a in db.PaymentLogs on e.ApplicationId equals a.ApplicationId
                                      where (e.LicenseTypeId == "SSA" || e.LicenseTypeId == "TITA" || e.LicenseTypeId == "TCA" || e.LicenseTypeId.Contains("TPBA") || e.LicenseTypeId == "ATO" || e.LicenseTypeId == "ATM" || e.LicenseTypeId == "ATCLFP" || e.LicenseTypeId == "LTOLFP") && (e.Status =="AUTH")
                                      select new { e, a }).ToList();


                        var extrapayment = (from e in db.ExtraPayments
                                      where (e.LicenseTypeCode == "SSA" || e.LicenseTypeCode == "TITA" || e.LicenseTypeCode == "TCA" || e.LicenseTypeCode.Contains("TPBA") || e.LicenseTypeCode == "ATO" || e.LicenseTypeCode == "ATM" || e.LicenseTypeCode == "ATCLFP" || e.LicenseTypeCode == "LTOLFP") && (e.Status == "AUTH")
                                            select new { e}).ToList();

                        if (appreq.Count > 0)
                        {
                            foreach (var item in appreq)
                            {
                                await serviceIntegrator.IGRPaymentPost(item.a.RRReference, item.e.ApplicationId);

                            }
                        }


                        if (extrapayment.Count > 0)
                        {
                            foreach (var item in extrapayment)
                            {
                                await serviceIntegrator.IGRPaymentPost(item.e.RRReference, item.e.ExtraPaymentAppRef);

                            }
                        }


                    }
                }
                catch (Exception ex)
                {
                    status = "failed";
                }


                Thread.Sleep(ex_interval);


            }
        }




        public void Dispose()
        {
            ex_running = false;

            if (ex_cancel != null)
            {
                try
                {
                    ex_cancel.Cancel();
                    ex_cancel.Dispose();
                }
                catch
                {
                }
                finally
                {
                    ex_cancel = null;
                }
            }
        }
    }









    public class EmailReminderMonitorJob : IDisposable
    {
        private CancellationTokenSource e_cancel;
        private Task e_task;
        private TimeSpan e_interval;
        private bool e_running;
        private ElpServiceHelper serviceIntegrator = new ElpServiceHelper();
        private WorkFlowHelper workflowHelper = new WorkFlowHelper();
        private LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities();
        private UserHelper userHelper = new UserHelper();



        public EmailReminderMonitorJob(TimeSpan interval)
        {
            e_interval = interval;
            e_running = true;
            e_cancel = new CancellationTokenSource();
            e_task = Task.Run(() => EmailReminder(), e_cancel.Token);

        }


        private void EmailReminder()
        {
            while (e_running)
            {
                string status = "";
                try
                {
                    using (LubeBlendingDBEntities db = new LubeBlendingDBEntities())
                    {
                        List<ApplicationRequest> appreq = (from e in db.ApplicationRequests
                                                           join u in db.UserMasters on e.CurrentAssignedUser equals u.UserId
                                                            where e.Status == "Processing" && u.UserType == "ADMIN" select e).ToList();
                        if (appreq.Count > 0)
                        {
                            foreach (var item in appreq)
                            {
                                var subject = "REMINDER:-Application waiting for Approval";
                                var content = "Application with the reference number " + item.ApplicationId + " is currently on your desk waiting for approval.";
                                var sendemail =  userHelper.SendStaffEmailMessage(item.CurrentAssignedUser, subject, content);
                                status = "done";
                            }
                        }



                    }
                }
                catch (Exception ex)
                {
                    status = "failed";
                }


                Thread.Sleep(e_interval);


            }
        }




        public void Dispose()
        {
            e_running = false;

            if (e_cancel != null)
            {
                try
                {
                    e_cancel.Cancel();
                    e_cancel.Dispose();
                }
                catch
                {
                }
                finally
                {
                    e_cancel = null;
                }
            }
        }
    }





}