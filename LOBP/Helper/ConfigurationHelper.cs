using LOBP.DbEntities;
using LOBP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace LOBP.Helper
{
    public class ConfigurationHelper
    {
        private ElpServiceHelper serviceIntegrator = new ElpServiceHelper(GlobalModel.elpsUrl, GlobalModel.appEmail, GlobalModel.appKey);

        public List<DocumentModel> GetAllElpsDoc()
        {
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                ApplicationDocHelper appDocHelper = new ApplicationDocHelper(dbCtxt);
                ElpsResponse elpsResponse = serviceIntegrator.GetAllDocumentType();
                ElpsResponse allfacilityelpsResponse = serviceIntegrator.GetAllFacilityDocumentListById();
                List<AllDocumentTypes> AllDocument = new List<AllDocumentTypes>();
                List<FacilityDocument> AllElpsFacDocumenList = new List<FacilityDocument>();
                List<DocumentModel> DocumentList = new List<DocumentModel>();

                if (elpsResponse.message == "SUCCESS")
                {
                    var result = elpsResponse.value;
                    AllDocument = (List<AllDocumentTypes>)elpsResponse.value;
                    AllElpsFacDocumenList = (List<FacilityDocument>)allfacilityelpsResponse.value;
                    DocumentList = appDocHelper.CompanyFacilityMissingDocumentsAdminRejectDoc(AllDocument, AllElpsFacDocumenList);


                }
                return DocumentList;
            }


        }

        public List<RequiredLicenseDocument> RequiredDocumentConfiguration()
        {
            List<RequiredLicenseDocument> doclist = new List<RequiredLicenseDocument>();
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                var RequiredDocList = (from d in dbCtxt.RequiredLicenseDocuments select d).ToList();
                foreach (var item in RequiredDocList)
                {
                    doclist.Add(new RequiredLicenseDocument()
                    {
                        ApplicationTypeId = item.ApplicationTypeId,
                        IsBaseTran = item.IsBaseTran,
                        IsLicenseDoc = item.IsLicenseDoc,
                        IsMandatory = item.IsMandatory,
                        LicenseTypeId = item.LicenseTypeId,
                        SeqNo = item.SeqNo,
                        Status = item.Status,
                        TypeID = item.TypeID,
                        Description = item.Description

                    });
                }
                return doclist;
            }
        }



        public List<LegacyDocument> LegacyDocumentConfiguration()
        {
            List<LegacyDocument> legacydoclist = new List<LegacyDocument>();
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                var LegacyDocList = (from l in dbCtxt.LegacyDocuments select l).ToList();
                foreach (var item1 in LegacyDocList)
                {
                    legacydoclist.Add(new LegacyDocument()
                    {
                        IsBaseTran = item1.IsBaseTran,
                        IsMandatory = item1.IsMandatory,
                        LicenseTypeCode = item1.LicenseTypeCode,
                        SeqNo = item1.SeqNo,
                        Status = item1.Status,
                        TypeID = item1.TypeID,
                        Description = item1.Description

                    });
                }
                return legacydoclist;
            }
        }


        public string AddDocumentOnline(string LicenseTypeId, string ApplicationType, int Docid, string docname, int Serialnum, string IsbaseTrans, string Ismandatory, string Islicensedoc, string status)
        {
            var res = "";
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                var docexist = (from d in dbCtxt.RequiredLicenseDocuments where d.LicenseTypeId == LicenseTypeId && d.ApplicationTypeId == ApplicationType && d.TypeID == Docid select d).FirstOrDefault();

                RequiredLicenseDocument doc = new RequiredLicenseDocument();
                try
                {
                    if (docexist == null)
                    {
                        doc.LicenseTypeId = LicenseTypeId;
                        doc.ApplicationTypeId = ApplicationType;
                        doc.TypeID = Docid;
                        doc.Description = docname;
                        doc.SeqNo = Convert.ToByte(Serialnum);
                        doc.IsBaseTran = IsbaseTrans;
                        doc.IsMandatory = Ismandatory;
                        doc.IsLicenseDoc = Islicensedoc;
                        doc.Status = status;
                        dbCtxt.RequiredLicenseDocuments.Add(doc);
                        dbCtxt.SaveChanges();
                        res = "success";
                    }
                    else
                    {
                        res = "existing";
                    }
                }
                catch (Exception ex)
                {
                    res = "Failed " + ex.Message;
                }

                return res;
            }
        }


        public string DeleteDocumentOnline(string LicenseTypeId, string ApplicationTypeId, int docID)
        {

            var res = "";
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                try
                {
                    var deletedoc = (from d in dbCtxt.RequiredLicenseDocuments where d.LicenseTypeId == LicenseTypeId && d.ApplicationTypeId == ApplicationTypeId && d.TypeID == docID select d).FirstOrDefault();
                    dbCtxt.RequiredLicenseDocuments.Remove(deletedoc);
                    dbCtxt.SaveChanges();
                    res = "success";
                }
                catch (Exception ex)
                {
                    res = "Failed " + ex.Message;
                }
                return res;
            }
        }




        public string AddDocumentLegacy(string LicenseTypeId, int Docid, string docname, int Serialnum, string IsbaseTrans, string Ismandatory, string status)
        {
            var res = "";

            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                var dexistingdoc = (from d in dbCtxt.LegacyDocuments where d.LicenseTypeCode == LicenseTypeId && d.TypeID == Docid select d).FirstOrDefault();

                LegacyDocument doc = new LegacyDocument();
                try
                {
                    if (dexistingdoc == null)
                    {
                        doc.LicenseTypeCode = LicenseTypeId;
                        doc.ApplicationType = "ALL";
                        doc.TypeID = Docid;
                        doc.Description = docname;
                        doc.SeqNo = Convert.ToByte(Serialnum);
                        doc.IsBaseTran = IsbaseTrans;
                        doc.IsMandatory = Ismandatory;
                        doc.Status = status;
                        dbCtxt.LegacyDocuments.Add(doc);
                        dbCtxt.SaveChanges();
                        res = "success";
                    }
                    else
                    {
                        res = "existing";
                    }
                }
                catch (Exception ex)
                {
                    res = "Failed " + ex.Message;
                }


                return res;
            }
        }


        public string DeleteDocumentLegacy(string LicenseTypeId, int docID)
        {

            var res = "";
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {
                try
                {
                    var deletedoc = (from d in dbCtxt.LegacyDocuments where d.LicenseTypeCode == LicenseTypeId && d.TypeID == docID select d).FirstOrDefault();
                    dbCtxt.LegacyDocuments.Remove(deletedoc);
                    dbCtxt.SaveChanges();
                    res = "success";
                }
                catch (Exception ex)
                {
                    res = "Failed " + ex.Message;
                }
                return res;
            }
        }
    }
}