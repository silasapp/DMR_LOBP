
using LOBP.DbEntities;
using LOBP.Helper;
using LOBP.Models;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LOBP.Controllers
{
    public class VerifyController : Controller
    {
        private UtilityHelper commonHelper = new UtilityHelper();
        private LicenseHelper licenseHelper = new LicenseHelper();
        List<PermitModels> permitmodel = new List<PermitModels>();
        List<Permitmodel> permits = new List<Permitmodel>();
        private UtilityHelper commonhelper = new UtilityHelper();


        // GET: Verify
        [HttpGet]
        [AllowAnonymous]
        public ActionResult VerifyPermitQrCode(string id)
        {
            Permitmodel permit = new Permitmodel();

            if (string.IsNullOrWhiteSpace(id))
            {
                ViewBag.Message = "Something went wrong. Permit not found or not in correct format. Kindly contact support";
            }
            else
            {
                using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
                {
                    var details = (from a in dbCtxt.ApplicationRequests
                                   join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                                   join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                                   where a.ApplicationId == id && a.IsLegacy == "NO"
                                   select new { f.TxnAmount, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.StorageCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate }).FirstOrDefault();

                    //permits.Add(new Permitmodel()
                    //{
                    permit.CompanyName = details.ApplicantName;
                    permit.CompanyIdentity = details.ElpsId;
                    permit.LicenseNumber = details.LicenseReference;
                    permit.RegisteredAddress = details.RegisteredAddress;
                    permit.LocationAddress = details.SiteLocationAddress;
                    permit.Expiry = Convert.ToDateTime(details.LicenseExpiryDate);
                    permit.CapacityToWord = commonHelper.NumberToWords(Convert.ToInt64(details.StorageCapacity));
                    permit.AmountToWord = commonHelper.NumberToWords(Convert.ToInt64(details.TxnAmount));
                    permit.DateIssued = Convert.ToDateTime(details.LicenseIssuedDate);
                    permit.Capacity = Convert.ToInt64(details.StorageCapacity);
                    permit.ApprefNo = details.ApplicationId;

                    //});
                }
            }
            return View(permit);

        }


        public ActionResult VerifyPermit(int id)
        {
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                var permit = (from a in dbCtxt.ApplicationRequests where a.PermitElpsId == id select new { a.LicenseTypeId, a.LinkedReference, a.ApplicationId}).ToList();

                if(permit.Count > 0)
                {
                    if (permit.FirstOrDefault().LicenseTypeId == "LTO")
                    {
                        return RedirectToAction("ViewLTO", "Admin", new { id = permit.FirstOrDefault().ApplicationId });
                    }
                   else if (permit.FirstOrDefault().LicenseTypeId == "ATC")
                    {
                        return RedirectToAction("ViewATC", "Admin", new { id = permit.FirstOrDefault().ApplicationId });
                    }
                    else if (permit.FirstOrDefault().LicenseTypeId == "SSA")
                    {
                        return RedirectToAction("ViewSUI", "Admin", new { id = permit.FirstOrDefault().ApplicationId });
                    }
                    else if (permit.FirstOrDefault().LicenseTypeId == "ATM")
                    {
                        return RedirectToAction("ViewATCMOD", "Admin", new { id = permit.FirstOrDefault().ApplicationId });
                    }
                    else if (permit.FirstOrDefault().LicenseTypeId == "ATO")
                    {
                        return RedirectToAction("ViewTO", "Admin", new { id = permit.FirstOrDefault().ApplicationId });
                    }
                    else if (permit.FirstOrDefault().LicenseTypeId == "PTE")
                    {
                        return RedirectToAction("ViewPTE", "Admin", new { id = permit.FirstOrDefault().ApplicationId });
                    }
                    else
                    {
                        return Content("NO RECORD FOUND!!!");
                    }
                }
                else { return Content("NO RECORD FOUND!!!"); }
               
            }
            //return null;
        }







        public ActionResult ViewPTE(string id)
        {
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == id.Trim()).FirstOrDefault();

                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();


                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.StorageCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();



                string Lga = null, statename = null, address = null;
                var fieldofficeaddress = commonhelper.FieldLocation(dbCtxt, id, out Lga, out statename, out address);
                var TelePhoneNumber = (from c in dbCtxt.Configurations where c.ParamID == "TelePhone" select c.ParamValue).FirstOrDefault();

                var PMB = (from c in dbCtxt.Configurations where c.ParamID == "P_M_B" select c.ParamValue).FirstOrDefault();

                var compdetails = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == id && a.IsLegacy == "NO" select new { a.RegisteredAddress, a.ApplicantName, a.SiteLocationAddress, a.LicenseIssuedDate, a.LicenseReference, a.LicenseTypeId, a.ApplicationCategory }).ToList();
                var Host = Request.Url.Authority;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                if (compdetails.Count > 0)
                {
                    var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                    Pstatus.PrintedStatus = "Printed";
                    dbCtxt.SaveChanges();
                    permits.Add(new Permitmodel()
                    {
                        Address = address,
                        LGA = Lga,
                        State = statename,
                        PMB = PMB,
                        Signature = signature,
                        IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                        IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                        IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                        TelePhoneNumber = TelePhoneNumber,
                        CompanyName = compdetails.FirstOrDefault().ApplicantName,
                        RegisteredAddress = compdetails.FirstOrDefault().RegisteredAddress,
                        LocationAddress = compdetails.FirstOrDefault().SiteLocationAddress,
                        IssuedDate = Convert.ToDateTime(compdetails.FirstOrDefault().LicenseIssuedDate).ToString("MMMM dd, yyyy"),
                        PermitNumber = compdetails.FirstOrDefault().LicenseReference,
                        ApplicationCategory = compdetails.FirstOrDefault().ApplicationCategory,
                        QrCode = QrCode
                    });

                    permitmodel.Add(new PermitModels()
                    {
                        permitmodels = permits.ToList()
                    });
                }
            }
            return View(permitmodel.ToList());

        }






        public ActionResult ViewATCMOD(string id)
        {
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == id.Trim()).FirstOrDefault();

                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();


                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.StorageCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();



                string Lga = null, statename = null, address = null;
                var fieldofficeaddress = commonhelper.FieldLocation(dbCtxt, id, out Lga, out statename, out address);
                var TelePhoneNumber = (from c in dbCtxt.Configurations where c.ParamID == "TelePhone" select c.ParamValue).FirstOrDefault();

                var PMB = (from c in dbCtxt.Configurations where c.ParamID == "P_M_B" select c.ParamValue).FirstOrDefault();

                var compdetails = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == id && a.IsLegacy == "NO" select new { a.RegisteredAddress, a.ApplicantName, a.SiteLocationAddress, a.LicenseIssuedDate, a.LicenseReference, a.LicenseTypeId, a.ApplicationCategory }).ToList();
                var Host = Request.Url.Authority;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                if (compdetails.Count > 0)
                {
                    var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                    Pstatus.PrintedStatus = "Printed";
                    dbCtxt.SaveChanges();
                    permits.Add(new Permitmodel()
                    {
                        Address = address,
                        LGA = Lga,
                        State = statename,
                        PMB = PMB,
                        IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                        IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                        IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                        Signature = signature,
                        TelePhoneNumber = TelePhoneNumber,
                        CompanyName = compdetails.FirstOrDefault().ApplicantName,
                        RegisteredAddress = compdetails.FirstOrDefault().RegisteredAddress,
                        LocationAddress = compdetails.FirstOrDefault().SiteLocationAddress,
                        IssuedDate = Convert.ToDateTime(compdetails.FirstOrDefault().LicenseIssuedDate).ToString("MMMM dd, yyyy"),
                        PermitNumber = compdetails.FirstOrDefault().LicenseReference,
                        ApplicationCategory = compdetails.FirstOrDefault().ApplicationCategory,
                        QrCode = QrCode
                    });

                    permitmodel.Add(new PermitModels()
                    {
                        permitmodels = permits.ToList()
                    });

                }
            }
            return View(permitmodel.ToList());

        }







        public ActionResult ViewSUI(string id)
        {
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();
                string Lga = null, statename = null, address = null;
                var fieldofficeaddress = commonhelper.FieldLocation(dbCtxt, id, out Lga, out statename, out address);
                var TelePhoneNumber = (from c in dbCtxt.Configurations where c.ParamID == "TelePhone" select c.ParamValue).FirstOrDefault();

                var PMB = (from c in dbCtxt.Configurations where c.ParamID == "P_M_B" select c.ParamValue).FirstOrDefault();



                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.StorageCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();



                var compdetails = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == id && a.IsLegacy == "NO" select new { a.RegisteredAddress, a.ApplicantName, a.SiteLocationAddress, a.LicenseIssuedDate, a.LicenseReference, a.LicenseTypeId }).ToList();
                var Host = Request.Url.Authority; ;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                if (compdetails.Count > 0)
                {
                    var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                    Pstatus.PrintedStatus = "Printed";
                    dbCtxt.SaveChanges();

                    permits.Add(new Permitmodel()
                    {
                        Address = address,
                        LGA = Lga,
                        State = statename,
                        PMB = PMB,
                        Signature = signature,
                        IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                        IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                        IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                        TelePhoneNumber = TelePhoneNumber,
                        CompanyName = compdetails.FirstOrDefault().ApplicantName,
                        RegisteredAddress = compdetails.FirstOrDefault().RegisteredAddress,
                        LocationAddress = compdetails.FirstOrDefault().SiteLocationAddress,
                        IssuedDate = Convert.ToDateTime(compdetails.FirstOrDefault().LicenseIssuedDate).ToString("MMMM dd, yyyy"),
                        PermitNumber = compdetails.FirstOrDefault().LicenseReference,
                        QrCode = QrCode
                    });

                    permitmodel.Add(new PermitModels()
                    {
                        permitmodels = permits.ToList()
                    });

                }
            }
            return View(permitmodel.ToList());

        }










        public ActionResult ViewTO(string id)
        {
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == id.Trim()).FirstOrDefault();

                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();


                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.StorageCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();



                string Lga = null, statename = null, address = null;
                var fieldofficeaddress = commonhelper.FieldLocation(dbCtxt, id, out Lga, out statename, out address);
                var TelePhoneNumber = (from c in dbCtxt.Configurations where c.ParamID == "TelePhone" select c.ParamValue).FirstOrDefault();

                var PMB = (from c in dbCtxt.Configurations where c.ParamID == "P_M_B" select c.ParamValue).FirstOrDefault();

                var compdetails = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == id && a.IsLegacy == "NO" select new { a.RegisteredAddress, a.ApplicantName, a.SiteLocationAddress, a.LicenseIssuedDate, a.LicenseReference, a.LicenseTypeId, a.ApplicationCategory }).ToList();
                var Host = Request.Url.Authority;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                if (compdetails.Count > 0)
                {
                    var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                    Pstatus.PrintedStatus = "Printed";
                    dbCtxt.SaveChanges();
                    permits.Add(new Permitmodel()
                    {
                        Address = address,
                        LGA = Lga,
                        State = statename,
                        PMB = PMB,
                        IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                        IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                        IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                        Signature = signature,
                        TelePhoneNumber = TelePhoneNumber,
                        CompanyName = compdetails.FirstOrDefault().ApplicantName,
                        RegisteredAddress = compdetails.FirstOrDefault().RegisteredAddress,
                        LocationAddress = compdetails.FirstOrDefault().SiteLocationAddress,
                        IssuedDate = Convert.ToDateTime(compdetails.FirstOrDefault().LicenseIssuedDate).ToString("MMMM dd, yyyy"),
                        PermitNumber = compdetails.FirstOrDefault().LicenseReference,
                        ApplicationCategory = compdetails.FirstOrDefault().ApplicationCategory,
                        QrCode = QrCode
                    });

                    permitmodel.Add(new PermitModels()
                    {
                        permitmodels = permits.ToList()
                    });

                }
            }
            return View(permitmodel.ToList());

        }











        public ActionResult ViewATC(string id)
        {
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == id.Trim()).FirstOrDefault();

                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();


                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.StorageCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();



                string Lga = null, statename = null, address = null;
                var fieldofficeaddress = commonhelper.FieldLocation(dbCtxt, id, out Lga, out statename, out address);
                var TelePhoneNumber = (from c in dbCtxt.Configurations where c.ParamID == "TelePhone" select c.ParamValue).FirstOrDefault();

                var PMB = (from c in dbCtxt.Configurations where c.ParamID == "P_M_B" select c.ParamValue).FirstOrDefault();

                var compdetails = (from a in dbCtxt.ApplicationRequests where a.ApplicationId == id && a.IsLegacy == "NO" select new { a.RegisteredAddress, a.ApplicantName, a.SiteLocationAddress, a.LicenseIssuedDate, a.LicenseReference, a.LicenseTypeId, a.ApplicationCategory }).ToList();
                var Host = Request.Url.Authority;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                if (compdetails.Count > 0)
                {
                    var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                    Pstatus.PrintedStatus = "Printed";
                    dbCtxt.SaveChanges();
                    permits.Add(new Permitmodel()
                    {
                        Address = address,
                        LGA = Lga,
                        State = statename,
                        PMB = PMB,
                        Signature = signature,
                        IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                        IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                        IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                        TelePhoneNumber = TelePhoneNumber,
                        CompanyName = compdetails.FirstOrDefault().ApplicantName,
                        RegisteredAddress = compdetails.FirstOrDefault().RegisteredAddress,
                        LocationAddress = compdetails.FirstOrDefault().SiteLocationAddress,
                        IssuedDate = Convert.ToDateTime(compdetails.FirstOrDefault().LicenseIssuedDate).ToString("MMMM dd, yyyy"),
                        PermitNumber = compdetails.FirstOrDefault().LicenseReference,
                        ApplicationCategory = compdetails.FirstOrDefault().ApplicationCategory,
                        QrCode = QrCode
                    });

                    permitmodel.Add(new PermitModels()
                    {
                        permitmodels = permits.ToList()
                    });
                }
            }
            return View(permitmodel.ToList());

        }














        public ActionResult ViewLTO(string id)
        {
            DateTime currentDateTime = DateTime.UtcNow.AddYears(1);
            DateTime currentDate = DateTime.UtcNow;
            string expiryDate = currentDateTime.ToString("yyyy") + "-12-31";
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                var signature = (from a in dbCtxt.ApplicationRequests
                                 join u in dbCtxt.UserMasters on a.SignatureID equals u.SignatureID
                                 where a.ApplicationId == id
                                 select u.SignatureImage).FirstOrDefault();
                ApplicationRequest appRequest = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == id.Trim()).FirstOrDefault();
                var Pstatus = (from p in dbCtxt.ApplicationRequests where p.ApplicationId == id select p).FirstOrDefault();
                Pstatus.PrintedStatus = "Printed";

                var Host = Request.Url.Authority;
                var absolutUrl = Host + "/Verify/VerifyPermitQrCode/" + id;
                var QrCode = commonhelper.GenerateQR(absolutUrl);
                dbCtxt.SaveChanges();
                var details = (from a in dbCtxt.ApplicationRequests
                               join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                               join f in dbCtxt.PaymentLogs on a.ApplicationId equals f.ApplicationId
                               where a.ApplicationId == id && a.IsLegacy == "NO"
                               select new { f.TxnAmount, f.Arrears, a.ApplicantName, u.ElpsId, a.ApplicationId, a.LicenseReference, a.SiteLocationAddress, a.RegisteredAddress, a.StorageCapacity, a.LicenseExpiryDate, a.LicenseIssuedDate, a.ApplicationCategory }).FirstOrDefault();

                permits.Add(new Permitmodel()
                {
                    Amountpaid = Convert.ToDecimal(details.TxnAmount),
                    Arrear = Convert.ToDecimal(details.Arrears),
                    Signature = signature,
                    CompanyName = details.ApplicantName,
                    CompanyIdentity = details.ElpsId,
                    LicenseNumber = details.LicenseReference,
                    RegisteredAddress = details.RegisteredAddress,
                    LocationAddress = details.SiteLocationAddress,
                    ExpiryYear = Convert.ToDateTime(details.LicenseExpiryDate).ToString("yyyy"),
                    CapacityToWord = commonhelper.NumberToWords(Convert.ToInt64(details.StorageCapacity)),
                    AmountToWord = commonhelper.NumberToWords(Convert.ToInt64(details.TxnAmount)),
                    IssuedDay = Convert.ToDateTime(details.LicenseIssuedDate).ToString("dd"),
                    IssuedMonth = Convert.ToDateTime(details.LicenseIssuedDate).ToString("MMMM"),
                    IssuedYear = Convert.ToDateTime(details.LicenseIssuedDate).ToString("yyyy"),
                    Capacity = Convert.ToInt64(details.StorageCapacity),
                    ApprefNo = details.ApplicationId,
                    ApplicationCategory = details.ApplicationCategory,
                    QrCode = QrCode

                });
                permitmodel.Add(new PermitModels()
                {
                    permitmodels = permits.ToList()
                });
            }
            return View(permitmodel.ToList());

        }

    }
}