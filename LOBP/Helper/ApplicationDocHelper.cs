using log4net;
using LOBP.DbEntities;
using LOBP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LOBP.Helper
{
    public class ApplicationDocHelper
    {

        private LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities();
        private UtilityHelper utilityHelper;
        private ILog log = log4net.LogManager.GetLogger(typeof(ApplicationDocHelper));

        public ApplicationDocHelper(LubeBlendingDBEntities dbCtxt)
        {
            this.dbCtxt = dbCtxt;
            utilityHelper = new UtilityHelper(dbCtxt);
        }




        public List<DocumentModel> MissingDocuments(List<MissingDocument> DocApplicationTypeList, List<AllDocumentTypes> docListFromElps, List<RequiredLicenseDocument> CompareDocApplicationList, string applicationId, string elpsUrl)
        {
            List<DocumentModel> documentModelList = new List<DocumentModel>();

            Dictionary<string,
             DocumentModel> dictionary = new Dictionary<string,
             DocumentModel>();

            try
            {

                if (docListFromElps == null)
                {
                    docListFromElps = new List<AllDocumentTypes>();
                }
                else
                {
                    var comparelist = (from s in CompareDocApplicationList select s.TypeID).ToList();

                    var missingTypeList1 = (from m in DocApplicationTypeList where !comparelist.Contains(m.DocId) select m).ToList();


                    if (docListFromElps != null && missingTypeList1 != null)
                    {
                        foreach (var item1 in docListFromElps)
                        {
                            foreach (var items in missingTypeList1)
                            {
                                if (item1.id == items.DocId)
                                {
                                    documentModelList.Add(new DocumentModel()
                                    {

                                        DocId = item1.id,
                                        FileId = item1.id,
                                        DocumentName = item1.name,
                                        DocumentTypeName = item1.type

                                    });
                                }
                            }
                        }
                    }

                    documentModelList = documentModelList.GroupBy(x => x.DocId).Select(x => x.LastOrDefault()).ToList();
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
            }

            return documentModelList;
        }



        public void GetApplicationRevenue(out long SSARevenue, out long ATCRevenue, out long LTORevenue, out long ATORevenue, out long PTERevenue, out long TITARevenue,
            out long ATMRevenue, out long ATCLFPRevenue, out long LTOLFPRevenue, out long TPBAPLWRevenue, out long TPBAPRWRevenue, out long RevenueGrandTotal, out long TCARevenue,
            out string SSABrandOne, out string SSAIGR, out string ATOBrandOne, out string ATOIGR, out string ATMBrandOne, out string ATMIGR, out string ATCLFPBrandOne, out string ATCLFPIGR,
            out string LTOLFPBrandOne, out string LTOLFPIGR, out string TPBAPLWBrandOne, out string TPBAPLWIGR, out string TPBAPRWBrandOne, out string TPBAPRWIGR, out string TITABrandOne, out string TITAIGR,
            out string TCABrandOne, out string TCAIGR, out string GrandTotalIGR, out string GrandTotalBrandOne, out string GrandTotalFG)
        {


            var ssa = from p in dbCtxt.PaymentLogs
                      join e in dbCtxt.ExtraPayments on p.ApplicationId equals e.ApplicationID into ps
                      from e in ps.DefaultIfEmpty()
                      where (p.Status == "AUTH" && p.LicenseTypeId == "SSA") || (e.Status == "AUTH" && e.LicenseTypeCode == "SSA")
                      select new { paymentlog = p.TxnAmount, Extrapayment = e.TxnAmount };

            var pte = from p in dbCtxt.PaymentLogs
                      join e in dbCtxt.ExtraPayments on p.ApplicationId equals e.ApplicationID into ps
                      from e in ps.DefaultIfEmpty()
                      where (p.Status == "AUTH" && p.LicenseTypeId == "PTE") || (e.Status == "AUTH" && e.LicenseTypeCode == "PTE")
                      select new { paymentlog = p.TxnAmount, Extrapayment = e.TxnAmount };

            var atc = from p in dbCtxt.PaymentLogs
                      join e in dbCtxt.ExtraPayments on p.ApplicationId equals e.ApplicationID into ps
                      from e in ps.DefaultIfEmpty()
                      where (p.Status == "AUTH" && p.LicenseTypeId == "ATC") || (e.Status == "AUTH" && e.LicenseTypeCode == "ATC")
                      select new { paymentlog = p.TxnAmount, Extrapayment = e.TxnAmount };

            var lto = from p in dbCtxt.PaymentLogs
                      join e in dbCtxt.ExtraPayments on p.ApplicationId equals e.ApplicationID into ps
                      from e in ps.DefaultIfEmpty()
                      where (p.Status == "AUTH" && p.LicenseTypeId == "LTO") || (e.Status == "AUTH" && e.LicenseTypeCode == "LTO")
                      select new { paymentlog = p.TxnAmount, Extrapayment = e.TxnAmount };

            var atm = from p in dbCtxt.PaymentLogs
                      join e in dbCtxt.ExtraPayments on p.ApplicationId equals e.ApplicationID into ps
                      from e in ps.DefaultIfEmpty()
                      where (p.Status == "AUTH" && p.LicenseTypeId == "ATM") || (e.Status == "AUTH" && e.LicenseTypeCode == "ATM")
                      select new { paymentlog = p.TxnAmount, Extrapayment = e.TxnAmount };

            var ato = from p in dbCtxt.PaymentLogs
                     join e in dbCtxt.ExtraPayments on p.ApplicationId equals e.ApplicationID into ps
                     from e in ps.DefaultIfEmpty()
                     where (p.Status == "AUTH" && p.LicenseTypeId == "ATO") || (e.Status == "AUTH" && e.LicenseTypeCode == "ATO")
                     select new { paymentlog = p.TxnAmount, Extrapayment = e.TxnAmount };

            var atclfp = from p in dbCtxt.PaymentLogs
                           join e in dbCtxt.ExtraPayments on p.ApplicationId equals e.ApplicationID into ps
                           from e in ps.DefaultIfEmpty()
                           where (p.Status == "AUTH" && p.LicenseTypeId == "ATCLFP") || (e.Status == "AUTH" && e.LicenseTypeCode == "ATCLFP")
                           select new { paymentlog = p.TxnAmount, Extrapayment = e.TxnAmount };

            var ltolfp = from p in dbCtxt.PaymentLogs
                         join e in dbCtxt.ExtraPayments on p.ApplicationId equals e.ApplicationID into ps
                         from e in ps.DefaultIfEmpty()
                         where (p.Status == "AUTH" && p.LicenseTypeId == "LTOLFP") || (e.Status == "AUTH" && e.LicenseTypeCode == "LTOLFP")
                         select new { paymentlog = p.TxnAmount, Extrapayment = e.TxnAmount };

            var tpbaplw = from p in dbCtxt.PaymentLogs
                         join e in dbCtxt.ExtraPayments on p.ApplicationId equals e.ApplicationID into ps
                         from e in ps.DefaultIfEmpty()
                         where (p.Status == "AUTH" && p.LicenseTypeId == "TPBA-PLW") || (e.Status == "AUTH" && e.LicenseTypeCode == "TPBA-PLW")
                         select new { paymentlog = p.TxnAmount, Extrapayment = e.TxnAmount };

            var tpbaprw = from p in dbCtxt.PaymentLogs
                         join e in dbCtxt.ExtraPayments on p.ApplicationId equals e.ApplicationID into ps
                         from e in ps.DefaultIfEmpty()
                         where (p.Status == "AUTH" && p.LicenseTypeId == "TPBA-PRW") || (e.Status == "AUTH" && e.LicenseTypeCode == "TPBA-PRW")
                         select new { paymentlog = p.TxnAmount, Extrapayment = e.TxnAmount };

            var tita = from p in dbCtxt.PaymentLogs
                          join e in dbCtxt.ExtraPayments on p.ApplicationId equals e.ApplicationID into ps
                          from e in ps.DefaultIfEmpty()
                          where (p.Status == "AUTH" && p.LicenseTypeId == "TITA") || (e.Status == "AUTH" && e.LicenseTypeCode == "TITA")
                          select new { paymentlog = p.TxnAmount, Extrapayment = e.TxnAmount };

            var tca = from p in dbCtxt.PaymentLogs
                          join e in dbCtxt.ExtraPayments on p.ApplicationId equals e.ApplicationID into ps
                          from e in ps.DefaultIfEmpty()
                          where (p.Status == "AUTH" && p.LicenseTypeId == "TCA") || (e.Status == "AUTH" && e.LicenseTypeCode == "TCA")
                          select new { paymentlog = p.TxnAmount, Extrapayment = e.TxnAmount };

            PTERevenue = Convert.ToInt64(pte.ToList().Sum(x => x.paymentlog)) + Convert.ToInt64(pte.ToList().Sum(x => x.Extrapayment));
            ATCRevenue = Convert.ToInt64(atc.ToList().Sum(x => x.paymentlog)) + Convert.ToInt64(atc.ToList().Sum(x => x.Extrapayment));
            LTORevenue = Convert.ToInt64(lto.ToList().Sum(x => x.paymentlog)) + Convert.ToInt64(lto.ToList().Sum(x => x.Extrapayment));

            SSARevenue = Convert.ToInt64(ssa.ToList().Sum(x => x.paymentlog)) + Convert.ToInt64(ssa.ToList().Sum(x => x.Extrapayment));
            SSABrandOne = Math.Round(Convert.ToDecimal(SSARevenue) * Convert.ToDecimal(0.1), 2).ToString("N");
            SSAIGR = Math.Round(Convert.ToDecimal(SSARevenue) - Convert.ToDecimal(SSABrandOne), 2).ToString("N");

            ATORevenue = Convert.ToInt64(ato.ToList().Sum(x => x.paymentlog)) + Convert.ToInt64(ato.ToList().Sum(x => x.Extrapayment));
            ATOBrandOne = Math.Round(Convert.ToDecimal(ATORevenue) * Convert.ToDecimal(0.1), 2).ToString("N");
            ATOIGR = Math.Round(Convert.ToDecimal(ATORevenue) - Convert.ToDecimal(ATOBrandOne), 2).ToString("N");

            ATMRevenue = Convert.ToInt64(atm.ToList().Sum(x => x.paymentlog)) + Convert.ToInt64(atm.ToList().Sum(x => x.Extrapayment));
            ATMBrandOne = Math.Round(Convert.ToDecimal(ATMRevenue) * Convert.ToDecimal(0.1), 2).ToString("N");
            ATMIGR = Math.Round(Convert.ToDecimal(ATMRevenue) - Convert.ToDecimal(ATMBrandOne), 2).ToString("N");

            ATCLFPRevenue = Convert.ToInt64(atclfp.ToList().Sum(x => x.paymentlog)) + Convert.ToInt64(atclfp.ToList().Sum(x => x.Extrapayment));
            ATCLFPBrandOne = Math.Round(Convert.ToDecimal(ATCLFPRevenue) * Convert.ToDecimal(0.1), 2).ToString("N");
            ATCLFPIGR = Math.Round(Convert.ToDecimal(ATCLFPRevenue) - Convert.ToDecimal(ATCLFPBrandOne), 2).ToString("N");

            LTOLFPRevenue = Convert.ToInt64(ltolfp.ToList().Sum(x => x.paymentlog)) + Convert.ToInt64(ltolfp.ToList().Sum(x => x.Extrapayment));
            LTOLFPBrandOne = Math.Round(Convert.ToDecimal(LTOLFPRevenue) * Convert.ToDecimal(0.1), 2).ToString("N");
            LTOLFPIGR = Math.Round(Convert.ToDecimal(LTOLFPRevenue) - Convert.ToDecimal(LTOLFPBrandOne), 2).ToString("N");

            TPBAPLWRevenue = Convert.ToInt64(tpbaplw.ToList().Sum(x => x.paymentlog)) + Convert.ToInt64(tpbaplw.ToList().Sum(x => x.Extrapayment));
            TPBAPLWBrandOne = Math.Round(Convert.ToDecimal(TPBAPLWRevenue) * Convert.ToDecimal(0.1), 2).ToString("N");
            TPBAPLWIGR = Math.Round(Convert.ToDecimal(TPBAPLWRevenue) - Convert.ToDecimal(TPBAPLWBrandOne), 2).ToString("N");

            TPBAPRWRevenue = Convert.ToInt64(tpbaprw.ToList().Sum(x => x.paymentlog)) + Convert.ToInt64(tpbaprw.ToList().Sum(x => x.Extrapayment));
            TPBAPRWBrandOne = Math.Round(Convert.ToDecimal(TPBAPRWRevenue) * Convert.ToDecimal(0.1), 2).ToString("N");
            TPBAPRWIGR = Math.Round(Convert.ToDecimal(TPBAPRWRevenue) - Convert.ToDecimal(TPBAPRWBrandOne), 2).ToString("N");

            TITARevenue = Convert.ToInt64(tita.ToList().Sum(x => x.paymentlog)) + Convert.ToInt64(tita.ToList().Sum(x => x.Extrapayment));
            TITABrandOne = Math.Round(Convert.ToDecimal(TITARevenue) * Convert.ToDecimal(0.1), 2).ToString("N");
            TITAIGR = Math.Round(Convert.ToDecimal(TITARevenue) - Convert.ToDecimal(TITABrandOne), 2).ToString("N");

            TCARevenue = Convert.ToInt64(tca.ToList().Sum(x => x.paymentlog)) + Convert.ToInt64(tca.ToList().Sum(x => x.Extrapayment));
            TCABrandOne = Math.Round(Convert.ToDecimal(TCARevenue) * Convert.ToDecimal(0.1), 2).ToString("N");
            TCAIGR = Math.Round(Convert.ToDecimal(TCARevenue) - Convert.ToDecimal(TCABrandOne), 2).ToString("N");
            GrandTotalIGR = Math.Round(Convert.ToDecimal(SSAIGR) + Convert.ToDecimal(ATMIGR) + Convert.ToDecimal(ATOIGR) + Convert.ToDecimal(ATCLFPIGR) + Convert.ToDecimal(LTOLFPIGR) + Convert.ToDecimal(TCAIGR) + Convert.ToDecimal(TITAIGR) + Convert.ToDecimal(TPBAPLWIGR) + Convert.ToDecimal(TPBAPRWIGR), 2).ToString("N");
            GrandTotalBrandOne = Math.Round(Convert.ToDecimal(SSABrandOne) + Convert.ToDecimal(ATMBrandOne) + Convert.ToDecimal(ATOBrandOne) + Convert.ToDecimal(ATCLFPBrandOne) + Convert.ToDecimal(LTOLFPBrandOne) + Convert.ToDecimal(TCABrandOne) + Convert.ToDecimal(TITABrandOne) + Convert.ToDecimal(TPBAPLWBrandOne) + Convert.ToDecimal(TPBAPRWBrandOne), 2).ToString("N");
            GrandTotalFG = Math.Round(Convert.ToDecimal(PTERevenue) + Convert.ToDecimal(ATCRevenue) + Convert.ToDecimal(LTORevenue), 2).ToString("N");
            RevenueGrandTotal = SSARevenue + ATCRevenue + LTORevenue + ATORevenue + PTERevenue + ATMRevenue + ATCLFPRevenue + LTOLFPRevenue + TPBAPLWRevenue + TPBAPRWRevenue + TITARevenue + TCARevenue;
        }







        public List<DocumentModel> CompanyFacilityMissingDocuments(List<AllDocumentTypes> companydocListFromElps, List<FacilityDocument> facilitydocListFromElps, List<RequiredLicenseDocument> Appcompany, List<RequiredLicenseDocument> Appfacility, List<RequiredLicenseDocument> appcompare)
        {
            List<DocumentModel> documentModelList = new List<DocumentModel>();

            Dictionary<string,
             DocumentModel> dictionary = new Dictionary<string,
             DocumentModel>();
            List<AllDocumentTypes> companyTypeList1 = null;
            List<FacilityDocument> facilityTypeList1 = null;


            try
            {

                if (companydocListFromElps == null && facilitydocListFromElps == null)
                {
                    companydocListFromElps = new List<AllDocumentTypes>();
                    facilitydocListFromElps = new List<FacilityDocument>();
                }
                else
                {

                    var Appcompanylist = (from c in Appcompany select c.TypeID).ToList();
                    var Appfacilitylist = (from f in Appfacility select f.TypeID).ToList();
                    var Appcomparelist = (from a in appcompare select a.TypeID).ToList();

                    if (Appcomparelist.Count == 0)
                    {
                        companyTypeList1 = (from m in companydocListFromElps where !Appcompanylist.Contains(m.id) && !Appfacilitylist.Contains(m.id) select m).ToList();
                        facilityTypeList1 = (from m in facilitydocListFromElps where !Appcompanylist.Contains(m.Id) && !Appfacilitylist.Contains(m.Id) select m).ToList();
                    }
                    else
                    {
                        companyTypeList1 = (from m in companydocListFromElps where !Appcompanylist.Contains(m.id) && !Appfacilitylist.Contains(m.id) && !Appcomparelist.Contains(m.id) select m).ToList();
                        facilityTypeList1 = (from m in facilitydocListFromElps where !Appcompanylist.Contains(m.Id) && !Appfacilitylist.Contains(m.Id) && !Appcomparelist.Contains(m.Id) select m).ToList();
                    }



                    foreach (var item in companyTypeList1)
                    {
                        documentModelList.Add(new DocumentModel()
                        {
                            DocId = item.id,
                            FileId = item.id,
                            DocumentName = item.name,
                            DocumentTypeName = item.type
                        });

                    }
                    foreach (var item1 in facilityTypeList1)
                    {
                        documentModelList.Add(new DocumentModel()
                        {
                            DocId = item1.Id,
                            FileId = item1.Id,
                            DocumentName = item1.Name,
                            DocumentTypeName = item1.Type
                        });

                    }

                    documentModelList = documentModelList.GroupBy(x => x.DocId).Select(x => x.LastOrDefault()).ToList();
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
            }

            return documentModelList;
        }











        public List<ApplicationRequest> GetUnissuedLicense(string userId, string type, out string errorMessage)
        {
            errorMessage = "SUCCESS";
            List<ApplicationRequest> AllBaseRequest = new List<ApplicationRequest>();


            try
            {

                foreach (ApplicationRequest b in dbCtxt.ApplicationRequests.Where(c => (c.ApplicantUserId.Trim() == userId.Trim() && c.IsLegacy != "YES" && c.LicenseReference == null)).ToList())
                {

                    if (type == "ALL")
                    {
                        AllBaseRequest.Add(b);
                    }
                    else if (type == "EXP")
                    {
                        if (b.LicenseExpiryDate !=
                         default(DateTime) && b.LicenseReference !=
                         default(string))
                        {
                            if (b.LicenseExpiryDate.Value.Subtract(DateTime.UtcNow).Days <= 30)
                            {
                                AllBaseRequest.Add(b);
                            }
                        }

                    }
                    else if (type == "PEM")
                    {
                        if (b.LicenseReference != default(string))
                        {
                            AllBaseRequest.Add(b);
                        }
                    }
                    else if (type == "PROC")
                    {
                        if (b.LicenseReference ==
                         default(string) && b.CurrentStageID != default(int))
                        {
                            string stateType = dbCtxt.WorkFlowStates.Where(w => w.StateID == b.CurrentStageID).FirstOrDefault().StateType;
                            if (stateType !=
                             default(string) && stateType == "PROGRESS")
                            {
                                AllBaseRequest.Add(b);
                            }

                        }
                    }
                }
                //}

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                errorMessage = "Error Occured when Generating List of Applications, Please Try again Later";
            }

            return AllBaseRequest;
        }








        public List<GeneralCommentModel> AllHistoryComment(string userId)
        {
            List<GeneralCommentModel> AllComment = new List<GeneralCommentModel>();

            try
            {

                var unapprovedapps = (from ah in dbCtxt.ActionHistories
                                      join a in dbCtxt.ApplicationRequests on ah.ApplicationId equals a.ApplicationId
                                      where a.ApplicantUserId == userId && a.Status == "Rejected" && ah.TargetedToRole == "COMPANY" && (ah.Action == "Reject" || ah.Action == "Rejectmk") && a.CurrentStageID == 1 && a.IsLegacy != "YES" && (a.LicenseReference == null || a.LicenseReference == "")
                                      select ah).AsEnumerable().OrderByDescending(d => Convert.ToDateTime(d.ActionDate)).GroupBy(d => d.ApplicationId).ToList();

                foreach (var item in unapprovedapps)
                {
                    AllComment.Add(new GeneralCommentModel()
                    {
                        Comment = item.FirstOrDefault().MESSAGE,
                        ApplicationID = item.FirstOrDefault().ApplicationId
                    });
                }



            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
            }

            return AllComment;
        }










        public List<CompanyMessage> GetCompanyMessages(UserMaster userMaster)
        {
            List<CompanyMessage> messageList = new List<CompanyMessage>();
            log.Info("About to Get Notification Messages for Company =>" + userMaster.UserId);

            try
            {
                //foreach(ActionHistory actionHistory in dbCtxt.Database.SqlQuery < ActionHistory > (string.Format("select top 15 * from dbo.ActionHistory where ApplicationId in (select ApplicationId from dbo.ApplicationRequest where ApplicantUserId = '{0}' )  and TargetedToRole ='{1}' order by ActionDate desc", userMaster.UserId.Trim(), "COMPANY")).ToList()) {
                foreach (ActionHistory actionHistory in dbCtxt.Database.SqlQuery<ActionHistory>(string.Format("select top 15 * from dbo.ActionHistory where ApplicationId in (select ApplicationId from dbo.ApplicationRequest where ApplicantUserId = '{0}' and CurrentStageID >= 4 )  order by ActionDate desc", userMaster.UserId.Trim())).ToList())
                {
                    CompanyMessage companyMessage = new CompanyMessage();
                    companyMessage.ApplicationId = actionHistory.ApplicationId;
                    companyMessage.Date = UtilityHelper.GetElapsedTime(actionHistory.ActionDate.Value);
                    companyMessage.MessageId = Convert.ToString(actionHistory.ActionId);
                    companyMessage.Message = actionHistory.MESSAGE;
                    companyMessage.MessageType = (actionHistory.NextStateID == 7) ? "Info" : "Info";

                    messageList.Add(companyMessage);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace + Environment.NewLine + "InnerException =>" + ex.InnerException);
            }

            log.Info("Message List Count =>" + messageList.Capacity);

            return messageList;
        }












        public List<DocumentModel> GetDocumentsPendingView(List<RequiredLicenseDocument> DocApplicationTypeList, List<Document> docListFromElps, string applicationId, string elpsUrl)
        {
            bool document_added = false;
            List<DocumentModel> documentModelList = new List<DocumentModel>();
            Dictionary<string,
             DocumentModel> dictionary = new Dictionary<string,
             DocumentModel>();

            try
            {

                if (docListFromElps == null)
                {
                    docListFromElps = new List<Document>();
                }

                foreach (RequiredLicenseDocument documentAppType in DocApplicationTypeList)
                {
                    string dockey = null;
                    if (documentAppType.IsBaseTran == "T")
                    {
                        dockey = applicationId + "_" + documentAppType.TypeID;
                    }
                    else
                    {
                        dockey = Convert.ToString(documentAppType.TypeID);
                    }

                    foreach (Document d in docListFromElps)
                    {
                        if (d.document_Name != null)
                        {
                            if (dockey.Equals(d.document_Name))
                            {
                                if (!dictionary.ContainsKey(dockey))
                                {
                                    string source = d.source.Contains("https") ? d.source : elpsUrl + d.source;
                                    dictionary.Add(dockey, new DocumentModel()
                                    {
                                        DocId = documentAppType.TypeID,
                                        DocumentName = documentAppType.Description,
                                        UniqueId = d.uniqueId,
                                        Source = source,
                                        FileId = d.id,
                                        DocumentTypeName = d.documentTypeName,
                                        IsMandatory = documentAppType.IsMandatory,
                                        BaseorTran = documentAppType.IsBaseTran
                                    });
                                    document_added = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (dockey.Equals(d.document_type_id))
                            {
                                if (!dictionary.ContainsKey(dockey))
                                {
                                    string source = d.source.Contains("https") ? d.source : elpsUrl + d.source;
                                    dictionary.Add(dockey, new DocumentModel()
                                    {
                                        DocId = documentAppType.TypeID,
                                        DocumentName = documentAppType.Description,
                                        UniqueId = d.uniqueId,
                                        Source = source,
                                        FileId = d.id,
                                        DocumentTypeName = d.documentTypeName,
                                        IsMandatory = documentAppType.IsMandatory,
                                        BaseorTran = documentAppType.IsBaseTran
                                    });
                                    document_added = true;
                                    break;
                                }
                            }
                        }

                    }

                    if (document_added == false)
                    {
                        documentModelList.Add(new DocumentModel()
                        {
                            DocId = documentAppType.TypeID,
                            DocumentName = documentAppType.Description,
                            UniqueId = new Random().Next(9999999).ToString(),
                            BaseorTran = documentAppType.IsBaseTran,
                        });
                    }

                    document_added = false;

                }

                foreach (KeyValuePair<string, DocumentModel> pair in dictionary)
                {
                    documentModelList.Add(pair.Value);
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
            }

            return documentModelList;
        }


        public List<DocumentModel> CompanyFacilityMissingDocumentsAdminRejectDoc(List<AllDocumentTypes> companydocListFromElps, List<FacilityDocument> facilitydocListFromElps)
        {
            List<DocumentModel> documentModelList = new List<DocumentModel>();

            Dictionary<string,
             DocumentModel> dictionary = new Dictionary<string,
             DocumentModel>();
            List<AllDocumentTypes> companyTypeList1 = null;
            List<FacilityDocument> facilityTypeList1 = null;

            try
            {

                if (companydocListFromElps == null && facilitydocListFromElps == null)
                {
                    companydocListFromElps = new List<AllDocumentTypes>();
                    facilitydocListFromElps = new List<FacilityDocument>();
                }
                else
                {

                    companyTypeList1 = (from m in companydocListFromElps select m).ToList();
                    facilityTypeList1 = (from m in facilitydocListFromElps select m).ToList();

                    foreach (var item in companyTypeList1)
                    {
                        documentModelList.Add(new DocumentModel()
                        {
                            DocId = item.id,
                            DocumentName = item.name,
                        });

                    }
                    foreach (var item1 in facilityTypeList1)
                    {
                        documentModelList.Add(new DocumentModel()
                        {
                            DocId = item1.Id,
                            DocumentName = item1.Name,
                        });

                    }

                    documentModelList = documentModelList.GroupBy(x => x.DocId).Select(x => x.LastOrDefault()).ToList();
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
            }

            return documentModelList;
        }
















        public List<DocumentModel> GetLegacyDocumentsView(List<LegacyDocument> DocApplicationTypeList, List<Document> docListFromElps, string applicationId, string elpsUrl)
        {
            bool document_added = false;
            List<DocumentModel> documentModelList = new List<DocumentModel>();
            Dictionary<string,
             DocumentModel> dictionary = new Dictionary<string,
             DocumentModel>();

            try
            {

                if (docListFromElps == null)
                {
                    docListFromElps = new List<Document>();
                }

                foreach (LegacyDocument documentAppType in DocApplicationTypeList)
                {
                    string dockey = null;
                    if (documentAppType.IsBaseTran == "T")
                    {
                        dockey = applicationId + "_" + documentAppType.TypeID;
                    }
                    else
                    {
                        dockey = Convert.ToString(documentAppType.TypeID);
                    }

                    foreach (Document d in docListFromElps)
                    {
                        if (d.document_Name != null)
                        {
                            if (dockey.Equals(d.document_Name))
                            {
                                if (!dictionary.ContainsKey(dockey))
                                {
                                    string source = d.source.Contains("https") ? d.source : elpsUrl + d.source;
                                    dictionary.Add(dockey, new DocumentModel()
                                    {
                                        DocId = documentAppType.TypeID,
                                        DocumentName = documentAppType.Description,
                                        UniqueId = d.uniqueId,
                                        Source = source,
                                        FileId = d.id,
                                        DocumentTypeName = d.documentTypeName,
                                        IsMandatory = documentAppType.IsMandatory,
                                        BaseorTran = documentAppType.IsBaseTran
                                    });
                                    document_added = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (dockey.Equals(d.document_type_id))
                            {
                                if (!dictionary.ContainsKey(dockey))
                                {
                                    string source = d.source.Contains("https") ? d.source : elpsUrl + d.source;
                                    dictionary.Add(dockey, new DocumentModel()
                                    {
                                        DocId = documentAppType.TypeID,
                                        DocumentName = documentAppType.Description,
                                        UniqueId = d.uniqueId,
                                        Source = source,
                                        FileId = d.id,
                                        DocumentTypeName = d.documentTypeName,
                                        IsMandatory = documentAppType.IsMandatory,
                                        BaseorTran = documentAppType.IsBaseTran
                                    });
                                    document_added = true;
                                    break;
                                }
                            }
                        }

                    }

                    if (document_added == false)
                    {
                        documentModelList.Add(new DocumentModel()
                        {
                            DocId = documentAppType.TypeID,
                            DocumentName = documentAppType.Description,
                            UniqueId = new Random().Next(9999999).ToString(),
                            BaseorTran = documentAppType.IsBaseTran,
                        });
                    }

                    document_added = false;

                }

                foreach (KeyValuePair<string, DocumentModel> pair in dictionary)
                {
                    documentModelList.Add(pair.Value);
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
            }

            return documentModelList;
        }






        public List<DocumentModel> GetLegacyDocuments(List<LegacyDocument> DocApplicationTypeList, List<FacilityDocument> docListFromElps, string applicationId, string elpsUrl)
        {
            bool document_added = false;
            List<DocumentModel> documentModelList = new List<DocumentModel>();
            Dictionary<string,
             DocumentModel> dictionary = new Dictionary<string,
             DocumentModel>();

            try
            {

                if (docListFromElps == null)
                {
                    docListFromElps = new List<FacilityDocument>();
                }
                else
                {
                    foreach (var item in DocApplicationTypeList)
                    {
                        foreach (var item1 in docListFromElps)
                        {
                            if (item1.Document_Type_Id == item.TypeID)
                            {
                                documentModelList.Add(new DocumentModel()
                                {
                                    DocId = Convert.ToInt32(item1.Document_Type_Id),
                                    DocumentName = item1.Document_Name,
                                    UniqueId = item1.UniqueId,
                                    Source = item1.Source,
                                    FileId = item1.Id,
                                    DocumentTypeName = item1.documentTypeName,
                                    IsMandatory = item.IsMandatory,
                                    BaseorTran = item.IsBaseTran
                                });
                            }
                        }
                    }

                    var missingdoc = DocApplicationTypeList.Where(s => !documentModelList.Any(x => x.DocId == s.TypeID));

                    foreach (var m in missingdoc.ToList())
                    {
                        documentModelList.Add(new DocumentModel()
                        {
                            DocId = m.TypeID,
                            DocumentTypeName = m.Description,
                            IsMandatory = m.IsMandatory,
                            BaseorTran = m.IsBaseTran


                        });
                    }
                    documentModelList = documentModelList.GroupBy(x => x.DocId).Select(x => x.LastOrDefault()).ToList();
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
            }

            return documentModelList;
        }












        public List<DocumentModel> GetFacilityDocumentsPending(List<RequiredLicenseDocument> DocApplicationTypeList, List<FacilityDocument> docListFromElps, string applicationId, string elpsUrl)
        {

            List<DocumentModel> documentModelList = new List<DocumentModel>();
            Dictionary<string,
             DocumentModel> dictionary = new Dictionary<string,
             DocumentModel>();

            try
            {

                if (docListFromElps == null)
                {
                    docListFromElps = new List<FacilityDocument>();
                }
                else
                {
                    foreach (var item in DocApplicationTypeList)
                    {
                        foreach (var item1 in docListFromElps)
                        {
                            if (item1.Document_Type_Id == item.TypeID)
                            {
                                documentModelList.Add(new DocumentModel()
                                {
                                    DocId = Convert.ToInt32(item1.Document_Type_Id),
                                    DocumentName = item1.Document_Name,
                                    UniqueId = item1.UniqueId,
                                    Source = item1.Source,
                                    FileId = item1.Id,
                                    DocumentTypeName = item1.documentTypeName,
                                    IsMandatory = item.IsMandatory,
                                    BaseorTran = item.IsBaseTran
                                });
                            }
                        }
                    }

                    var missingdoc = DocApplicationTypeList.Where(s => !documentModelList.Any(x => x.DocId == s.TypeID));

                    foreach (var m in missingdoc.ToList())
                    {
                        documentModelList.Add(new DocumentModel()
                        {
                            DocId = m.TypeID,
                            DocumentTypeName = m.Description,
                            IsMandatory = m.IsMandatory,
                            BaseorTran = m.IsBaseTran


                        });
                    }
                    documentModelList = documentModelList.GroupBy(x => x.DocId).Select(x => x.LastOrDefault()).ToList();
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
            }

            return documentModelList;
        }






































        public List<DocumentModel> GetDocumentsPending(List<RequiredLicenseDocument> ReqLicenseDocumentList, List<Document> docListFromElps, string applicationId, string elpsUrl)
        {
            bool document_added = false;
            List<DocumentModel> documentModelList = new List<DocumentModel>();
            Dictionary<string, DocumentModel> dictionary = new Dictionary<string, DocumentModel>();

            try
            {

                if (docListFromElps == null)
                {
                    docListFromElps = new List<Document>();
                }

                foreach (RequiredLicenseDocument reqLicenseDoc in ReqLicenseDocumentList)
                {
                    string dockey = null;
                    if (reqLicenseDoc.IsBaseTran == "T")
                    {
                        dockey = applicationId + "_" + reqLicenseDoc.TypeID;
                    }
                    else
                    {
                        dockey = Convert.ToString(reqLicenseDoc.TypeID);
                    }

                    foreach (Document d in docListFromElps)
                    {
                        if (d.document_Name != null)
                        {
                            if (dockey.Equals(d.document_Name))
                            {
                                if (!dictionary.ContainsKey(dockey))
                                {
                                    string source = d.source.Contains("https") ? d.source : elpsUrl + d.source;
                                    //log.Info(source);
                                    dictionary.Add(dockey, new DocumentModel()
                                    {
                                        DocId = reqLicenseDoc.TypeID,
                                        DocumentName = reqLicenseDoc.Description,
                                        UniqueId = d.uniqueId,
                                        Source = source,
                                        FileId = d.id,
                                        DocumentTypeName = d.documentTypeName,
                                        IsMandatory = reqLicenseDoc.IsMandatory,
                                        BaseorTran = reqLicenseDoc.IsBaseTran
                                    });
                                    document_added = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (dockey.Equals(d.document_type_id))
                            {
                                if (!dictionary.ContainsKey(dockey))
                                {
                                    string source = d.source.Contains("https") ? d.source : elpsUrl + d.source;
                                    //log.Info(source);
                                    dictionary.Add(dockey, new DocumentModel()
                                    {
                                        DocId = reqLicenseDoc.TypeID,
                                        DocumentName = reqLicenseDoc.Description,
                                        UniqueId = d.uniqueId,
                                        Source = source,
                                        FileId = d.id,
                                        DocumentTypeName = d.documentTypeName,
                                        IsMandatory = reqLicenseDoc.IsMandatory,
                                        BaseorTran = reqLicenseDoc.IsBaseTran
                                    });
                                    document_added = true;
                                    break;
                                }
                            }
                        }

                    }

                    if (document_added == false)
                    {
                        documentModelList.Add(new DocumentModel()
                        {
                            DocId = reqLicenseDoc.TypeID,
                            DocumentName = reqLicenseDoc.Description,
                            UniqueId = new Random().Next(9999999).ToString(),
                            IsMandatory = reqLicenseDoc.IsMandatory,
                            BaseorTran = reqLicenseDoc.IsBaseTran
                        });
                    }

                    document_added = false;

                }

                foreach (KeyValuePair<string, DocumentModel> pair in dictionary)
                {
                    documentModelList.Add(pair.Value);
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
            }

            return documentModelList;
        }



        public List<DocumentModel> GetLegacyDocumentsPending(List<LegacyRequiredDocument> ReqLicenseDocumentList, List<Document> docListFromElps, string applicationId, string elpsUrl)
        {
            bool document_added = false;
            List<DocumentModel> documentModelList = new List<DocumentModel>();
            Dictionary<string, DocumentModel> dictionary = new Dictionary<string, DocumentModel>();

            try
            {

                if (docListFromElps == null)
                {
                    docListFromElps = new List<Document>();
                }

                foreach (LegacyRequiredDocument reqLicenseDoc in ReqLicenseDocumentList)
                {
                    string dockey = null;

                    dockey = applicationId + "_" + reqLicenseDoc.TypeID;

                    foreach (Document d in docListFromElps)
                    {
                        if (d.document_Name != null)
                        {
                            if (dockey.Equals(d.document_Name))
                            {
                                if (!dictionary.ContainsKey(dockey))
                                {
                                    string source = d.source.Contains("https") ? d.source : elpsUrl + d.source;
                                    //log.Info(source);
                                    dictionary.Add(dockey, new DocumentModel()
                                    {
                                        DocId = reqLicenseDoc.TypeID,
                                        DocumentName = reqLicenseDoc.Description,
                                        UniqueId = d.uniqueId,
                                        Source = source,
                                        FileId = d.id,
                                        DocumentTypeName = d.documentTypeName
                                    });
                                    document_added = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (dockey.Equals(d.document_type_id))
                            {
                                if (!dictionary.ContainsKey(dockey))
                                {
                                    string source = d.source.Contains("https") ? d.source : elpsUrl + d.source;
                                    //log.Info(source);
                                    dictionary.Add(dockey, new DocumentModel()
                                    {
                                        DocId = reqLicenseDoc.TypeID,
                                        DocumentName = reqLicenseDoc.Description,
                                        UniqueId = d.uniqueId,
                                        Source = source,
                                        FileId = d.id,
                                        DocumentTypeName = d.documentTypeName,
                                    });
                                    document_added = true;
                                    break;
                                }
                            }
                        }

                    }

                    if (document_added == false)
                    {
                        documentModelList.Add(new DocumentModel()
                        {
                            DocId = reqLicenseDoc.TypeID,
                            DocumentName = reqLicenseDoc.Description,
                            UniqueId = new Random().Next(9999999).ToString(),
                        });
                    }

                    document_added = false;

                }

                foreach (KeyValuePair<string, DocumentModel> pair in dictionary)
                {
                    documentModelList.Add(pair.Value);
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
            }

            return documentModelList;
        }



        public List<DocumentModel> GetAppointmentDocuments(string docType, List<Document> docListFromElps, string applicationId, string elpsUrl)
        {
            bool document_added = false;
            List<DocumentModel> appDocsList = new List<DocumentModel>();
            Dictionary<string, DocumentModel> dictionary = new Dictionary<string, DocumentModel>();

            try
            {

                StringBuilder queryBuilder = new StringBuilder("select * from dbo.Configuration where ParamID in ");
                if (docType == "ALL")
                {
                    queryBuilder.Append("('INSPECTION_REPORT','MINUTES_MEETING')");
                }
                else if (docType == "INSPECTION")
                {
                    queryBuilder.Append("('INSPECTION_REPORT')");
                }
                else if (docType == "MEETING")
                {
                    queryBuilder.Append("('MINUTES_MEETING')");
                }

                log.Info("Doc Query: " + queryBuilder.ToString());

                foreach (Configuration config in dbCtxt.Database.SqlQuery<Configuration>(queryBuilder.ToString()).ToList())
                {

                    String dockey = applicationId + "_" + config.ParamValue;

                    foreach (Document d in docListFromElps)
                    {
                        if (d.document_Name != null)
                        {
                            if (d.document_Name == dockey)
                            {
                                if (!dictionary.ContainsKey(dockey))
                                {
                                    string source = d.source.Contains("https") ? d.source : elpsUrl + d.source;
                                    log.Info("document_Name: " + d.document_Name + "---" + "docKey: " + dockey);
                                    dictionary.Add(dockey, new DocumentModel()
                                    {
                                        DocId = Convert.ToInt32(config.ParamValue),
                                        DocumentName = config.ParamID.Replace("_", " "),
                                        UniqueId = d.uniqueId,
                                        Source = source,
                                        FileId = d.id,
                                        DocumentTypeName = d.documentTypeName
                                    });
                                    document_added = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (dockey.Equals(d.document_type_id))
                            {
                                if (!dictionary.ContainsKey(dockey))
                                {
                                    string source = d.source.Contains("https") ? d.source : elpsUrl + d.source;
                                    dictionary.Add(dockey, new DocumentModel()
                                    {
                                        DocId = Convert.ToInt32(config.ParamValue),
                                        DocumentName = config.ParamID.Replace("_", " "),
                                        UniqueId = d.uniqueId,
                                        Source = source,
                                        FileId = d.id,
                                        DocumentTypeName = d.documentTypeName,
                                    });
                                    document_added = true;
                                    break;
                                }
                            }

                        }

                    }

                    if (document_added == false)
                    {
                        appDocsList.Add(new DocumentModel()
                        {
                            DocId = Convert.ToInt32(config.ParamValue),
                            DocumentName = config.ParamID.Replace("_", " "),
                            UniqueId = new Random().Next(9999999).ToString(),
                        });
                    }

                    document_added = false;
                }

                foreach (KeyValuePair<string, DocumentModel> pair in dictionary)
                {
                    appDocsList.Add(pair.Value);
                }


            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
            }


            return appDocsList;
        }






        public List<DocumentModel> GetOtherDocuments(List<Document> docListFromElps, string applicationId, string elpsUrl)
        {
            List<DocumentModel> appDocsList = new List<DocumentModel>();

            foreach (Configuration config in dbCtxt.Configurations.Where(c => c.ParamID == "INSPECTION_REPORT" || c.ParamID == "MINUTES_MEETING" || c.ParamID == "SWIFT_ADVICE").ToList())
            {

                String docKey = applicationId + "_" + config.ParamValue;

                foreach (Document d in docListFromElps)
                {
                    if (d.document_Name != null)
                    {
                        if (d.document_Name == docKey)
                        {
                            string source = d.source.Contains("https") ? d.source : elpsUrl + d.source;
                            log.Info("document_Name: " + d.document_Name + "---" + "docKey: " + docKey);

                            appDocsList.Add(new DocumentModel()
                            {
                                DocumentName = config.ParamID.Replace("_", " "),
                                Source = source,
                            });


                            //break;
                        }
                    }

                }
            }

            return appDocsList;
        }






        public List<DocumentModel> GetAllCompanyDocuments(string compId, string elpsurl, ElpServiceHelper serviceIntegrator)
        {

            List<DocumentModel> companyDocuments = new List<DocumentModel>();

            ElpsResponse wrapper = serviceIntegrator.GetCompanyDocumentListById(compId);
            if (wrapper.message != "SUCCESS")
            {
                return companyDocuments;
            }

            List<Document> docList = (List<Document>)wrapper.value;
            foreach (Document doc in docList)
            {
                string source = "";
                //log.Info("Source =>"+doc.source+" "+doc.source.Contains("https"));
                //if (doc.source.Contains("https")) { source = doc.source; } else {source = elpsurl + doc.source; }

                //log.Info("Source =>"+source);
                source = (doc.source.Contains("https")) ? doc.source : elpsurl + doc.source;
                DocumentModel docModel = new DocumentModel();
                docModel.DocId = Convert.ToInt32(doc.document_type_id);
                docModel.DocumentName = doc.fileName;
                docModel.DateAdded = Convert.ToDateTime(doc.date_added);
                docModel.DateModified = Convert.ToDateTime(doc.date_modified);
                docModel.Source = source; //doc.source;
                //docModel.Source =   doc.source.Contains("https")? doc.source.Replace("https","https:"):  elpsUrl + doc.source,
                docModel.UniqueId = doc.uniqueId;
                docModel.FileId = doc.id;
                docModel.DocumentTypeName = doc.documentTypeName;
                docModel.UploadDocumentUrl = elpsurl;

                companyDocuments.Add(docModel);
                source = "";
            }

            return companyDocuments;
        }




        public List<ApplicationRequest> GetApplications(string userId, string type, out string errorMessage)
        {
            errorMessage = "SUCCESS";
            List<ApplicationRequest> AllBaseRequest = new List<ApplicationRequest>();
            try
            {
                foreach (ApplicationRequest b in dbCtxt.ApplicationRequests.Where(c => (c.ApplicantUserId.Trim() == userId.Trim() && c.LicenseReference != null && c.IsLegacy == "NO")).ToList())
                {

                    if (type == "ALL")
                    {
                        AllBaseRequest.Add(b);
                    }
                    else if (type == "EXP")
                    {
                        if (b.LicenseExpiryDate !=
                         default(DateTime) && b.LicenseReference !=
                         default(string))
                        {
                            if (b.LicenseExpiryDate.Value.Subtract(DateTime.UtcNow).Days <= 30)
                            {
                                AllBaseRequest.Add(b);
                            }
                        }

                    }
                    else if (type == "PEM")
                    {
                        if (b.LicenseReference !=
                         default(string))
                        {
                            AllBaseRequest.Add(b);
                        }
                    }
                    else if (type == "PROC")
                    {
                        if (b.LicenseReference ==
                         default(string) && b.CurrentStageID !=
                         default(int))
                        {
                            string stateType = dbCtxt.WorkFlowStates.Where(w => w.StateID == b.CurrentStageID).FirstOrDefault().StateType;
                            if (stateType !=
                             default(string) && stateType == "PROGRESS")
                            {
                                AllBaseRequest.Add(b);
                            }

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                errorMessage = "Error Occured when Generating List of Applications, Please Try again Later";
            }

            return AllBaseRequest;
        }










        public List<ApplicationRequest> GetPTEATCLTOApplications(string userId, string type, out string errorMessage)
        {
            errorMessage = "SUCCESS";
            List<ApplicationRequest> AllBaseRequest = new List<ApplicationRequest>();
            try
            {
                foreach (ApplicationRequest b in dbCtxt.ApplicationRequests.Where(c => (c.ApplicantUserId.Trim() == userId.Trim() && (c.LicenseTypeId == "ATC" || c.LicenseTypeId == "LTO" || c.LicenseTypeId == "PTE" || c.LicenseTypeId == "ATCLFP" || c.LicenseTypeId == "LTOLFP") && (c.Status == "Approved"))).ToList())
                {

                    if (type == "ALL")
                    {
                        AllBaseRequest.Add(b);
                    }
                    
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                errorMessage = "Error Occured when Generating List of Applications, Please Try again Later";
            }

            return AllBaseRequest;
        }




        public List<ApplicationRequest> GetApplicationDetails(string useremail,out List<ApplicationRequest> Apprequest)
        {

            Apprequest = new List<ApplicationRequest>();

            foreach (ApplicationRequest b in dbCtxt.ApplicationRequests.Where(c => (c.ApplicantUserId.Trim() == useremail.Trim())).ToList())
            {

                Apprequest.Add(new ApplicationRequest()
                {
                    ApplicationId = b.ApplicationId,
                    CurrentStageID = b.CurrentStageID,
                    LicenseTypeId = b.LicenseTypeId
                });


            }

            return Apprequest;
       }











        public Dictionary<string, int> GetApplicationStatistics(string userId, out string responseMessage, out Dictionary<string, int> applicationStageReference)
        {
            int totalAppCount = 0;
            int ProcAppCount = 0;
            int yetToSubmitCount = 0;
            int totalPermitCount = 0;
            int permitExpiringCount = 0;
            responseMessage = "SUCCESS";
            Dictionary<string,
             int> appStatisticsTable = new Dictionary<string,
             int>();
            List<int> workFlowStagesToWork = new List<int> { 1,2,3,4,5,10,46,25,27,47};
    
            applicationStageReference = new Dictionary<string, int>();


            try
            {
                foreach (ApplicationRequest b in dbCtxt.ApplicationRequests.Where(c => (c.ApplicantUserId.Trim() == userId.Trim())).ToList())
                {
                    //ALL
                    totalAppCount = totalAppCount + 1;

                    if (workFlowStagesToWork.Contains(b.CurrentStageID.Value))
                    {
                        applicationStageReference.Add(b.ApplicationId, b.CurrentStageID.Value);
                    }

                    //EXP
                    string stateType1 = dbCtxt.WorkFlowStates.Where(w => w.StateID == b.CurrentStageID).FirstOrDefault().StateType;
                    if (!string.IsNullOrEmpty(stateType1) && stateType1 == "COMPLETE")
                    {

                        if (b.LicenseExpiryDate.HasValue && !string.IsNullOrEmpty(b.LicenseReference))
                        {
                            //b.LicenseExpiryDate.Value.Subtract(DateTime.UtcNow).Days <= 0) ||
                            if ((b.LicenseExpiryDate.Value.Subtract(DateTime.UtcNow).Days <= 30))
                            {
                                permitExpiringCount = permitExpiringCount + 1;
                            }
                        }
                    }


                    //PEM
                    if (b.LicenseReference != default(string))
                    {
                        totalPermitCount = totalPermitCount + 1;
                    }

                    //PROC
                    if (b.LicenseReference ==
                     default(string) && b.CurrentStageID !=
                     default(int))
                    {
                        string stateType = dbCtxt.WorkFlowStates.Where(w => w.StateID == b.CurrentStageID).FirstOrDefault().StateType;

                        if (stateType !=
                         default(string) && (stateType == "PROGRESS" || stateType == "CONTINUE"))
                        {
                            ProcAppCount = ProcAppCount + 1;
                        }
                    }


                }

                appStatisticsTable.Add("ALL", totalAppCount);
                appStatisticsTable.Add("EXP", permitExpiringCount);
                appStatisticsTable.Add("PEM", totalPermitCount);
                appStatisticsTable.Add("PROC", ProcAppCount);
                appStatisticsTable.Add("YTST", yetToSubmitCount);

            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace + Environment.NewLine + "InnerException =>" + ex.InnerException);
                responseMessage = "Error Occured when Generating Statistics for the DashBoard, Please Try again Later";
            }

            return appStatisticsTable;
        }

        public List<CompanyMessage> GetCompanyMessages(LubeBlendingDBEntities dbCtxt, UserMaster userMaster)
        {
            List<CompanyMessage> messageList = new List<CompanyMessage>();
            log.Info("About to Get Notification Messages for Company =>" + userMaster.UserId);

            try
            {
                foreach (ActionHistory actionHistory in dbCtxt.Database.SqlQuery<ActionHistory>(string.Format("select top 15 * from dbo.ActionHistory where ApplicationId in (select ApplicationId from dbo.ApplicationRequest where ApplicantUserId = '{0}')  and NextStateID in (1,2,3,4,10,21,46)  order by ActionDate desc", userMaster.UserId.Trim())).ToList())
                {
                    CompanyMessage companyMessage = new CompanyMessage();
                    companyMessage.ApplicationId = actionHistory.ApplicationId;
                    companyMessage.Date = UtilityHelper.GetElapsedTime(actionHistory.ActionDate.Value);
                    companyMessage.MessageId = Convert.ToString(actionHistory.ActionId);
                    companyMessage.Message = actionHistory.MESSAGE;
                    if (actionHistory.NextStateID == 7 || actionHistory.NextStateID == 105 || actionHistory.NextStateID == 106)
                    {
                        companyMessage.MessageType = "Error";
                    }
                    else
                    {
                        companyMessage.MessageType = "Info";
                    }


                    messageList.Add(companyMessage);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace + Environment.NewLine + "InnerException =>" + ex.InnerException);
            }

            log.Info("Message List Count =>" + messageList.Capacity);

            return messageList;
        }


        public string GenerateLicenseNumber(LubeBlendingDBEntities dbCtxt, string LicenseTypeId)
        {
            LicenseType licenseType = dbCtxt.LicenseTypes.Where(l => l.LicenseTypeId == LicenseTypeId.Trim()).FirstOrDefault();
            var licenseserialnum = (from l in dbCtxt.LicenseTypes where l.LicenseTypeId == LicenseTypeId select l.LicenseSerial).FirstOrDefault();
            if(licenseserialnum == null || licenseserialnum == 0)
            {
                licenseType.LicenseSerial = 1;
                dbCtxt.SaveChanges();
            }
            int result = licenseType.LicenseSerial.Value;
            licenseType.LicenseSerial = result + 1;
            dbCtxt.SaveChanges();
            return LicenseTypeId + "-" + utilityHelper.zeroPadder(Convert.ToString(result), 3) + "-" + DateTime.Now.Year;
        }

    }
}