using log4net;
using LOBP.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using LOBP.DbEntities;
using System.Diagnostics;

namespace LOBP.Helper
{
    public class ElpServiceHelper
    {

        private string serviceURL;
        private string appEmail;
        private string appKey;
        private string apiHash;
        private UtilityHelper utilityHelper;
        private ILog logger = log4net.LogManager.GetLogger(typeof(ElpServiceHelper));
        private UserHelper userHelper = new UserHelper();
        public ElpServiceHelper() { }

        public ElpServiceHelper(string serviceURL, string appEmail, string appKey)
        {
            this.serviceURL = serviceURL;
            this.appEmail = appEmail;
            this.appKey = appKey;
            utilityHelper = new UtilityHelper();
            apiHash = utilityHelper.GenerateHashText(appEmail + appKey);
            /*
            ServicePointManager
        .ServerCertificateValidationCallback += 
        (sender, cert, chain, sslPolicyErrors) => true;
            */
        }



        public ElpsResponse authenticateUserOnElps(string email)
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info("About to Authenticate User On Elps with Email => " + email);
                var client = new RestClient(serviceURL);
                client.Timeout = 60 * 1000;
                //client.Authenticator = new HttpBasicAuthenticator(username, password);
                var request = new RestRequest("api/Accounts/Login/{companymail}/{apiEmail}/{apiHash}", Method.GET);
                request.AddUrlSegment("companymail", email);
                request.AddUrlSegment("apiEmail", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);

                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    elspResponse.value = (String.Compare((SimpleJson.DeserializeObject<LoginResponse>(response.Content)).code, "00") == 0) ? true : false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }








        public ElpsResponse DeleteFacDocument(string fileid)
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info("About to Delete Document On Elps with File ID => " + fileid);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var request = new RestRequest("api/FacilityDocument/Delete/{id}/{email}/{apiHash}", Method.DELETE);
                request.AddUrlSegment("id", fileid);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    elspResponse.value = response.Content;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }
            return elspResponse;
        }






        public ElpsResponse DeleteDocument(string fileid)
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info("About to Delete Document On Elps with File ID => " + fileid);

                var request = new RestRequest("api/CompanyDocument/Delete/{fileid}/{email}/{apiHash}", Method.DELETE);
                request.AddUrlSegment("fileid", fileid);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    elspResponse.value = response.Content;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }
            return elspResponse;
        }










        public ElpsResponse CheckRRR(string rrr)
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info("About to check Payment Details On Elps for Application => " + rrr);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var request = new RestRequest("/Payment/checkifpaid?id=r" + rrr, Method.GET);
                //request.AddParameter("application/json; charset=utf-8", null, ParameterType.RequestBody);
                request.RequestFormat = DataFormat.Json;

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }

                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }

                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    var JSONContent = response.Content;
                    //if (JSONContent.Contains("generated") || JSONContent.Contains("Exist"))
                    //{
                    elspResponse.message = "SUCCESS";
                    elspResponse.value = JsonConvert.DeserializeObject<JObject>(JSONContent);
                    //}
                }

            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }







        public ElpsResponse GetCompanyDetailByEmail(string email)
        {
            ElpsResponse elspResponse = new ElpsResponse();
            try
            {
                apiHash = userHelper.GenerateSHA512(GlobalModel.appEmail + GlobalModel.appKey);
                logger.Info("About to GetCompanyDetail On Elps with Email => " + email);
                var request = new RestRequest("api/company/{compemail}/{email}/{apiHash}", Method.GET);
                request.AddUrlSegment("compemail", email);
                request.AddUrlSegment("email", GlobalModel.appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(GlobalModel.elpsUrl);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    elspResponse.value = JsonConvert.DeserializeObject<CompanyDetail>(response.Content);
                }

            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }


        public ElpsResponse GetCompanyDetailByID(string companyid)
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info("About to GetCompanyDetail On Elps with CompanyId => " + companyid);
                var request = new RestRequest("api/company/{id}/{apiEmail}/{apiHash}", Method.GET);
                request.AddUrlSegment("id", companyid);
                request.AddUrlSegment("apiEmail", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    elspResponse.value = SimpleJson.DeserializeObject<CompanyDetail>(response.Content);
                }

            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }








       public async Task <JObject> IGRPaymentPost(string rrr, string appid)
        {
            int uniqueid = 0, qty = 0, qty1=0, tankdetails = 0; decimal amount = 0;
            var values = new JObject();

            //IGRResponse igrresponse = null;
            ElpsResponse elspResponse = new ElpsResponse();
            using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
            {

                ExtraPayment extraappid = dbCtxt.ExtraPayments.Where(c => c.ExtraPaymentAppRef.Trim() == appid.Trim()).FirstOrDefault();
                PaymentLog payment = dbCtxt.PaymentLogs.Where(c => c.ApplicationId.Trim() == appid.Trim()).FirstOrDefault();

                ApplicationRequest appreq = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == appid).FirstOrDefault() == null ? dbCtxt.ApplicationRequests.Where(c => c.ApplicantUserId == extraappid.ApplicantID).FirstOrDefault() : dbCtxt.ApplicationRequests.Where(c => c.ApplicationId.Trim() == appid.Trim()).FirstOrDefault();
                tankdetails = (from t in dbCtxt.Tanks join a in dbCtxt.ApplicationRequests on t.ApplicationId equals a.ApplicationId select t.NbrOfTanks).FirstOrDefault();
                int tankdetail = tankdetails == 0 ? 1 : tankdetails;

                var expaystatus = extraappid == null ? "empty" : extraappid.Status;

                var TotalAmount = extraappid != null ? extraappid.TxnAmount : payment.TxnAmount;

                if (extraappid != null && (expaystatus == "Pending" || expaystatus == "FAIL" || expaystatus == "AUTH"))
                {
                    uniqueid = Convert.ToInt32(extraappid.PenaltyCode);
                    amount = Convert.ToDecimal(extraappid.TxnAmount + extraappid.ServiceCharge) / Convert.ToDecimal(1.05);
                }
                else
                {
                    if (appreq != null)
                    {
                        if (appreq.LicenseTypeId == "ATO")
                        {
                            // var processingfee = dbCtxt.Configurations.Where(c => c.ParamID == "ATO_NEW_PROCESS_FEES").FirstOrDefault().ParamValue;
                            //var approvalfee = dbCtxt.Configurations.Where(c => c.ParamID == "ATO_APPROVAL_FEES").FirstOrDefault().ParamValue;
                            //var code = dbCtxt.Penalties.Where(c => c.PenaltyType == "TAKE-OVER PROCESSING FEE").FirstOrDefault().PenaltyCode;
                            //amount = Convert.ToDecimal(processingfee + approvalfee) * Convert.ToDecimal(1.05);
                            amount = Convert.ToDecimal(TotalAmount) / Convert.ToDecimal(1.05);
                            uniqueid = Convert.ToInt32(771);
                        }
                        else if (appreq.LicenseTypeId == "ATM")
                        {
                            //var processingfee = dbCtxt.Configurations.Where(c => c.ParamID == "ATM_NEW_PROCESS_FEES").FirstOrDefault().ParamValue;
                            // amount = Convert.ToDecimal(processingfee) * Convert.ToDecimal(1.05);
                            amount = Convert.ToDecimal(TotalAmount) / Convert.ToDecimal(1.05);
                            uniqueid = Convert.ToInt32(772);
                        }
                        else if (appreq.LicenseTypeId == "SSA")
                        {
                            //var processingfee = dbCtxt.Configurations.Where(c => c.ParamID == "SSA_NEW_PROCESS_FEES").FirstOrDefault().ParamValue;
                            //var code = dbCtxt.Penalties.Where(c => c.PenaltyType == "SITE SUITABILITY APPROVAL (INSPECTION) FEE").FirstOrDefault().PenaltyCode;
                            //amount = Convert.ToDecimal(processingfee) * Convert.ToDecimal(1.05);
                            amount = Convert.ToDecimal(TotalAmount) / Convert.ToDecimal(1.05);
                            uniqueid = Convert.ToInt32(774);
                        }
                        else if (appreq.LicenseTypeId == "TITA")
                        {
                            //var processingfee = dbCtxt.Configurations.Where(c => c.ParamID == "TITA_NEW_PROCESS_FEES").FirstOrDefault().ParamValue;
                            //var code = dbCtxt.Penalties.Where(c => c.PenaltyType == "CERTIFICATION OF STORAGE TANK TABLES(CALIBRATION/INTEGRITY TESTS(NDTS) PER TANK CHARGE").FirstOrDefault().PenaltyCode;
                            //amount = (Convert.ToDecimal(processingfee) * Convert.ToDecimal(1.05)) * Convert.ToDecimal(tankdetail);
                            amount = Convert.ToDecimal(TotalAmount) / Convert.ToDecimal(1.05);
                            uniqueid = Convert.ToInt32(759);
                        }
                        else if (appreq.LicenseTypeId == "TCA")
                        {
                            //var processingfee = dbCtxt.Configurations.Where(c => c.ParamID == "TCA_NEW_PROCESS_FEES").FirstOrDefault().ParamValue;
                            //var code = dbCtxt.Penalties.Where(c => c.PenaltyType == "CERTIFICATION OF STORAGE TANK TABLES(CALIBRATION/INTEGRITY TESTS(NDTS) PER TANK CHARGE").FirstOrDefault().PenaltyCode;
                            //amount = (Convert.ToDecimal(processingfee) * Convert.ToDecimal(1.05)) * Convert.ToDecimal(tankdetail);
                            amount = Convert.ToDecimal(TotalAmount) / Convert.ToDecimal(1.05);
                            uniqueid = Convert.ToInt32(759);
                        }
                        else if (appreq.LicenseTypeId == "TPBA-PLW")
                        {
                            // var processingfee = dbCtxt.Configurations.Where(c => c.ParamID == "TPBA-PLW_NEW_PROCESS_FEES").FirstOrDefault().ParamValue;
                            //var statutoryfee = dbCtxt.Configurations.Where(c => c.ParamID == "TPBA-PLW_STATUTORY_FEES").FirstOrDefault().ParamValue;
                            // var code = dbCtxt.Penalties.Where(c => c.PenaltyType == "THIRD PARTY APPROVAL FEE (PLANT OWNER)-75,000/PER 500,000 LITRES AND/OR PART THEREOF").FirstOrDefault().PenaltyCode;
                            //amount = ((Convert.ToDecimal(processingfee) * Convert.ToDecimal(appreq.Quarter)) + Convert.ToDecimal(statutoryfee)) * Convert.ToDecimal(1.05);
                            amount = Convert.ToDecimal(TotalAmount) / Convert.ToDecimal(1.05);
                            uniqueid = Convert.ToInt32(757);
                        }
                        else if (appreq.LicenseTypeId == "TPBA-PRW")
                        {
                            //var processingfee = dbCtxt.Configurations.Where(c => c.ParamID == "TPBA-PRW_NEW_PROCESS_FEES").FirstOrDefault().ParamValue;
                            //var statutoryfee = dbCtxt.Configurations.Where(c => c.ParamID == "TPBA-PRW_STATUTORY_FEES").FirstOrDefault().ParamValue;
                            //var code = dbCtxt.Penalties.Where(c => c.PenaltyType == "THIRD PARTY APPROVAL FEE (PRODUCT OWNER) -250,000/PER 500,000 LITRES AND/OR PART THEREOF").FirstOrDefault().PenaltyCode;
                            //amount = ((Convert.ToDecimal(processingfee) * Convert.ToDecimal(appreq.Quarter)) + Convert.ToDecimal(statutoryfee)) * Convert.ToDecimal(1.05);
                            amount = Convert.ToDecimal(TotalAmount) / Convert.ToDecimal(1.05);
                            uniqueid = Convert.ToInt32(758);
                        }

                        else if (appreq.LicenseTypeId == "ATCLFP")
                        {
                            amount = Convert.ToDecimal(TotalAmount) / Convert.ToDecimal(1.05);
                            uniqueid = Convert.ToInt32(11999);
                        }
                        else if (appreq.LicenseTypeId == "LTOLFP")
                        {
                            amount = Convert.ToDecimal(TotalAmount) / Convert.ToDecimal(1.05);
                            uniqueid = Convert.ToInt32(12000);
                        }
                    }
                }

                if (appreq != null)
                {
                    //qty = (appreq.LicenseTypeId == "TPBA-PLW" || appreq.LicenseTypeId == "TPBA-PRW") ? Convert.ToInt32(appreq?.Quarter) : 1;

                    qty1 = 1; //qty == 0 ? 1 : qty;
                }
                var revenueItems = new List<RevenueItemViewModel>
            {
                new RevenueItemViewModel { RevenueItemId = uniqueid, Amount = amount, Quantity = qty1 },
            };
                if (appreq != null)
                {
                    var revItems = JsonConvert.SerializeObject(revenueItems);
                    Debug.WriteLine(revItems);
                    elspResponse = GetCompanyDetailByEmail(appreq?.ApplicantUserId);
                    CompanyDetail companyDetail = (CompanyDetail)elspResponse.value;
                    values.Add("RevenueItems", revItems);
                    values.Add("Quantity", qty1);
                    values.Add("RRR", rrr);
                    values.Add("ExternalPaymentReference", extraappid == null? appreq.ApplicationId: extraappid.ExtraPaymentAppRef);
                    values.Add("State", (from a in dbCtxt.FieldLocations where a.StateLocated.Contains(appreq.StateCode) select a.StateLocated).FirstOrDefault());
                    values.Add("Address", appreq?.SiteLocationAddress);
                    values.Add("CompanyName", appreq?.ApplicantName);
                    values.Add("Phone", companyDetail?.contact_Phone);
                    values.Add("CompanyEmail", appreq?.ApplicantUserId);
                    string jsonResponse = await postExternalTest("addpayments/", values);
                    values.Add("IgrResponse", jsonResponse);
                    elspResponse.value = JsonConvert.DeserializeObject<ElpsResponse>(jsonResponse);
                    elspResponse.message = "SUCCESS";
                    logger.Info("igr payment post values");
                    logger.Info("IGR Payment added " + elspResponse.message);
                }
                else
                {
                    var revItems = JsonConvert.SerializeObject(revenueItems);
                    values.Add("DIDNOTENTER", rrr);
                    values.Add("RevenueItems", revItems);
                    values.Add("RRR", rrr);
                }
            }
            return values;

        }










        public ElpsResponse maintainCompanyInformation(string detailtype, string CompanyId, string jsonRequest, CompanyChangeModel emailupdate)
        {
            RestRequest request;
            RestRequest request1;
            IRestResponse response = null;
            ElpsResponse elspResponse2 = new ElpsResponse();

            try
            {
                var client = new RestClient(serviceURL);

                if (detailtype.Contains("UPDATE_ADDRESS"))
                {
                    request = new RestRequest("api/Address/{email}/{apiHash}", Method.PUT);
                    request.AddParameter("application/json; charset=utf-8", jsonRequest, ParameterType.RequestBody);
                    request.RequestFormat = DataFormat.Json;
                    request.AddUrlSegment("email", appEmail);
                    request.AddUrlSegment("apiHash", apiHash);
                    response = client.Execute(request);
                }
                else if (detailtype.Contains("UPDATE_PROFILE"))
                {
                    request = new RestRequest("api/company/Edit/{email}/{apiHash}", Method.PUT);
                    request.AddParameter("application/json; charset=utf-8", jsonRequest, ParameterType.RequestBody);
                    request.RequestFormat = DataFormat.Json;
                    request.AddUrlSegment("email", appEmail);
                    request.AddUrlSegment("apiHash", apiHash);
                    response = client.Execute(request);


                    request1 = new RestRequest("api/Accounts/ChangeEmail/{email}/{apiHash}", Method.POST);
                    request1.AddParameter("application/json; charset=utf-8", emailupdate, ParameterType.RequestBody);
                    request1.RequestFormat = DataFormat.Json;
                    request1.AddUrlSegment("email", appEmail);
                    request1.AddUrlSegment("apiHash", apiHash);
                    response = client.Execute(request1);

                }
                else if (detailtype.Contains("ADD_ADDRESS"))
                {
                    request = new RestRequest("api/Address/{CompId}/{email}/{apiHash}", Method.POST);
                    request.AddParameter("application/json; charset=utf-8", jsonRequest, ParameterType.RequestBody);
                    request.RequestFormat = DataFormat.Json;
                    request.AddUrlSegment("CompId", CompanyId);
                    request.AddUrlSegment("email", appEmail);
                    request.AddUrlSegment("apiHash", apiHash);
                    response = client.Execute(request);
                }
                else if (detailtype.Contains("UPDATE_DIRECTOR"))
                {
                    request = new RestRequest("api/Directors/{email}/{apiHash}", Method.PUT);
                    request.AddParameter("application/json; charset=utf-8", jsonRequest, ParameterType.RequestBody);
                    request.RequestFormat = DataFormat.Json;
                    request.AddUrlSegment("email", appEmail);
                    request.AddUrlSegment("apiHash", apiHash);
                    response = client.Execute(request);
                }
                else if (detailtype.Contains("ADD_DIRECTOR"))
                {
                    request = new RestRequest("api/Directors/{CompId}/{email}/{apiHash}", Method.PUT);
                    request.AddParameter("application/json; charset=utf-8", jsonRequest, ParameterType.RequestBody);
                    request.RequestFormat = DataFormat.Json;
                    request.AddUrlSegment("CompId", CompanyId);
                    request.AddUrlSegment("email", appEmail);
                    request.AddUrlSegment("apiHash", apiHash);
                    response = client.Execute(request);
                }


                if (response.ErrorException != null)
                {
                    elspResponse2.message = response.ErrorMessage;
                    return elspResponse2;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse2.message = response.ResponseStatus.ToString();
                    return elspResponse2;
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse2.message = response.StatusCode.ToString();
                    return elspResponse2;
                }
                else
                {
                    if (detailtype.Contains("ADD_ADDRESS"))
                    {
                        elspResponse2.value = JsonConvert.DeserializeObject<List<CompanyAddressDTO>>(response.Content);
                    }

                    /*
                               else if (detailtype.Contains("UPDATE_PROFILE"))
                               {
                                   elspResponse2.value = JsonConvert.DeserializeObject<CompanyDetail>(response.Content);
                               }*/


                    elspResponse2.message = "SUCCESS";
                    return elspResponse2;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                elspResponse2.message = ex.Message;
            }

            return elspResponse2;
        }











        public async Task<ElpsResponse> MisdtdoPostAPI(string CertificateId)
        {
            ElpsResponse elspResponse = new ElpsResponse();
            //List<MistdoModel> mistdolist = new List<MistdoModel>();List<MistdoModel>
            List<MistdoModel> mistdolist = new List<MistdoModel>();
            try
            {

                HttpClient client = new HttpClient();

                HttpResponseMessage response = await client.GetAsync("https://mistdo.dpr.gov.ng/home/verifymistdocertificate?certificateid=" + CertificateId);

                if (response.IsSuccessStatusCode)
                {

                    var rootObjects = await response.Content.ReadAsAsync<JObject>();

                    JObject misdolist = JsonConvert.DeserializeObject<JObject>(rootObjects.ToString());
                    var d = misdolist.SelectToken("data");

                    if (misdolist != null)
                    {

                        mistdolist.Add(new MistdoModel()
                        {
                            success = Convert.ToBoolean(misdolist.SelectToken("success")),
                            message = misdolist.SelectToken("message").ToString(),
                            fullName = d.SelectToken("fullName").ToString(),
                            phoneNumber = d.SelectToken("phoneNumber").ToString(),
                            email = d.SelectToken("email").ToString(),
                            certificateNo = d.SelectToken("certificateNo").ToString(),
                            certificateIssue = d.SelectToken("certificateIssue").ToString(),
                            certificateExpiry = d.SelectToken("certificateExpiry").ToString(),
                            mistdoId = d.SelectToken("mistdoId").ToString(),
                        });

                    }
                    elspResponse.value = mistdolist;

                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }












        public ElpsResponse GetFacilityDocumentListById(string faciltyElpsid)
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info("About to GetCompany Documents On Elps with CompanyId => " + faciltyElpsid);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var request = new RestRequest("/api/FacilityFiles/{id}/{email}/{apiHash}", Method.GET);
                request.AddUrlSegment("id", faciltyElpsid);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }

                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }

                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    elspResponse.value = JsonConvert.DeserializeObject<List<FacilityDocument>>(response.Content);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }












        public ElpsResponse UpdateFacility(Facilities facilityDetails)
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info("About to Post Permit On Elps with Company ID => ");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var request = new RestRequest("api/Facility/EditFacility/{email}/{code}", Method.PUT);
                request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(facilityDetails), ParameterType.RequestBody);
                var client = new RestClient(serviceURL);
                request.RequestFormat = DataFormat.Json;
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("code", apiHash);
                IRestResponse response = client.Execute(request);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }

                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }

                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    //elspResponse.value = JsonConvert.DeserializeObject<Facilities>(response.Content);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }



        public ElpsResponse PostFacility(Facilities facilityDetails)
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info("About to Post Permit On Elps with Company ID => ");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var request = new RestRequest("/api/Facility/Add/{email}/{apiHash}", Method.POST);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);
                request.RequestFormat = DataFormat.Json;
                request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(facilityDetails), ParameterType.RequestBody);

                var client = new RestClient(serviceURL);
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }

                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }

                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    elspResponse.value = JsonConvert.DeserializeObject<Facilities>(response.Content);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }






        public ElpsResponse GetAllFacilityDocumentListById()
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var request = new RestRequest("/api/Documents/Facility/{email}/{apiHash}/{Type}", Method.GET);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);
                request.AddUrlSegment("Type", "facility");

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }

                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }

                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    elspResponse.value = JsonConvert.DeserializeObject<List<FacilityDocument>>(response.Content);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }








        public ElpsResponse GetAllDocumentType()
        {
            ElpsResponse elspResponse = new ElpsResponse();
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var request = new RestRequest("api/Documents/Types/{email}/{apiHash}", Method.GET);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }

                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }

                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.value = JsonConvert.DeserializeObject<List<AllDocumentTypes>>(response.Content);
                    elspResponse.message = "SUCCESS";
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }







        public ElpsResponse GetElpAppUpdateStatus(string Appid, string ElpsID, string Status)
        {
            ElpsResponse elspResponse = new ElpsResponse();
            try
            {

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var request = new RestRequest("/api/Application/ByOrderId/{orderId}/{email}/{apiHash}", Method.GET);
                request.AddUrlSegment("orderId", Appid);
                request.AddUrlSegment("email", GlobalModel.appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(GlobalModel.elpsUrl);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);



                var resp = JsonConvert.DeserializeObject<JObject>(response.Content);

                var values = new JObject();
                values.Add("orderId", Appid);
                values.Add("company_Id", Convert.ToInt32(ElpsID));
                values.Add("status", Status);
                values.Add("date", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                values.Add("categoryName", (string)resp.SelectToken("categoryName"));
                values.Add("licenseName", (string)resp.SelectToken("licenseName"));
                values.Add("licenseId", (string)resp.SelectToken("licenseId"));
                values.Add("id", (string)resp.SelectToken("id"));



                request = new RestRequest("api/Application/{email}/{apiHash}", Method.PUT);
                request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(values), ParameterType.RequestBody);
                request.RequestFormat = DataFormat.Json;
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);
                response = client.Execute(request);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }

                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }

                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.value = JsonConvert.DeserializeObject<JObject>(response.Content);
                    elspResponse.message = "SUCCESS";
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }








        public ElpsResponse GetCompanyDocumentListById(string companyid)
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info("About to GetCompany Documents On Elps with CompanyId => " + companyid);

                var request = new RestRequest("api/CompanyDocuments/{companyid}/{apiEmail}/{apiHash}", Method.GET);
                request.AddUrlSegment("companyid", companyid);
                request.AddUrlSegment("apiEmail", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    elspResponse.value = SimpleJson.DeserializeObject<List<Document>>(response.Content);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }











        public ElpsResponse GetDocumentDetail(string docId)
        {
            ElpsResponse elspResponse = new ElpsResponse();
            try
            {
                logger.Info("About to Get Documents On Elps with DocumentID => " + docId);

                var request = new RestRequest("api/CompanyDocument/{id}/{email}/{apiHash}", Method.GET);
                request.AddUrlSegment("id", docId);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.value = SimpleJson.DeserializeObject<Document>(response.Content);
                    elspResponse.message = "SUCCESS";
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }


        public ElpsResponse GetTransactionStatus(string orderID)
        {
            ElpsResponse elspResponse = new ElpsResponse();
            try
            {
                logger.Info("About to GetTransaction Status On Elps with ReferenceID => " + orderID);

                var request = new RestRequest("api/Payments/Status/{email}/{apiHash}/{orderId}", Method.GET);
                request.AddUrlSegment("orderId", orderID);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.value = SimpleJson.DeserializeObject<TransactionStatus>(response.Content); //SimpleJson.DeserializeObject<TransactionStatus>(jsonResponse);
                    elspResponse.message = "SUCCESS";
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }

        public ElpsResponse GetLocationDetails(string locationId)
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info("About to Get Location Details On Elps with LocationID => " + locationId);

                var request = new RestRequest("api/Branch/{id}/{email}/{apiHash}", Method.GET);
                request.AddUrlSegment("id", locationId);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    elspResponse.value = SimpleJson.DeserializeObject<Branch>(response.Content); //SimpleJson.DeserializeObject<BranchesDTO>(response.Content);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }


        public ElpsResponse GetAllDPRLocations()
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info("About to Get All DPR Locations On Elps");

                var request = new RestRequest("api/Branch/All/{email}/{apiHash}", Method.GET);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    elspResponse.value = SimpleJson.DeserializeObject<List<Branch>>(response.Content);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }



        public ElpsResponse GetStaffDetails(string emailID)
        {
            ElpsResponse elspResponse = new ElpsResponse();
            try
            {
                logger.Info("About to Get Staff Details On Elps with StaffEmail => " + emailID);
                var request = new RestRequest("api/Accounts/Staff/{staffEmail}/{email}/{apiHash}", Method.GET);
                request.AddUrlSegment("staffEmail", emailID);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.value = SimpleJson.DeserializeObject<Staff>(response.Content);
                    elspResponse.message = "SUCCESS";
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }


        public ElpsResponse GetAllStaff()
        {
            ElpsResponse elspResponse = new ElpsResponse();
            try
            {
                var request = new RestRequest("api/Accounts/Staff/{email}/{apiHash}", Method.GET);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.value = SimpleJson.DeserializeObject<List<Staff>>(response.Content);
                    elspResponse.message = "SUCCESS";
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }





        





        public ElpsResponse ChangePassword(string useremail, string oldPassword, string newPassword)
        {
            ElpsResponse elspResponse = new ElpsResponse();
            ChangePassword changePwd = new ChangePassword();

            try
            {
                logger.Info("About to ChangePassword On Elps with Email User => " + useremail);

                changePwd.oldPassword = oldPassword;
                changePwd.newPassword = newPassword;
                changePwd.confirmPassword = newPassword;

                var request = new RestRequest("api/Accounts/ChangePassword/{useremail}/{email}/{apiHash}", Method.POST);
                request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(changePwd), ParameterType.RequestBody);
                request.RequestFormat = DataFormat.Json;
                request.AddUrlSegment("useremail", useremail);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    ChangePasswordResponse cpr = SimpleJson.DeserializeObject<ChangePasswordResponse>(response.Content);
                    elspResponse.value = (cpr.code == 1) ? true : false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }


            return elspResponse;
        }

        











        public ElpsResponse GetAllDPRZones()
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info("About to Get All Zonal Locations of DPR On Elps ");

                var request = new RestRequest("api/Branch/AllZones/{email}/{apiHash}", Method.GET);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    elspResponse.value = SimpleJson.DeserializeObject<List<Zones>>(response.Content);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }


        public ElpsResponse GetStatesInZone(string zoneId)
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info("About to Get States in Zones On Elps with ZoneID => " + zoneId);

                var request = new RestRequest("api/Branch/StatesInZone/{id}/{email}/{apiHash}", Method.GET);
                request.AddUrlSegment("id", zoneId);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    elspResponse.value = SimpleJson.DeserializeObject<List<StatesInZone>>(response.Content);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }


        public ElpsResponse GetAllZoneMapping()
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info("About to Get All Zone Mapping On Elps");

                var request = new RestRequest("api/Branch/ZoneMapping/{email}/{apiHash}", Method.GET);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    elspResponse.value = SimpleJson.DeserializeObject<List<ZoneMapping>>(response.Content);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }


        public ElpsResponse PostPermit(string ElpsCompID, JObject values)
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                apiHash = userHelper.GenerateSHA512(GlobalModel.appEmail + GlobalModel.appKey);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var request = new RestRequest("api/Permits/{CompId}/{email}/{apiHash}", Method.POST);
                request.AddUrlSegment("CompId", ElpsCompID);
                request.AddUrlSegment("email", GlobalModel.appEmail);
                request.AddUrlSegment("apiHash", apiHash);
                request.RequestFormat = DataFormat.Json;
                request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(values), ParameterType.RequestBody);

                var client = new RestClient(GlobalModel.elpsUrl);
                IRestResponse response = client.Execute(request);

                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }

                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }

                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";

                    JObject eplsPermit = JsonConvert.DeserializeObject<JObject>(response.Content);
                    elspResponse.value = (int)eplsPermit.SelectToken("id");
                }
            }
            catch (Exception ex)
            {
                //logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }


        public ElpsResponse GetPermits(string compId)
        {
            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info("About to Get Permit On Elps with Company ID => " + compId);

                var request = new RestRequest("api/Permits/{CompId}/{email}/{apiHash}", Method.GET);
                request.AddUrlSegment("CompId", compId);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse.message = "SUCCESS";
                    elspResponse.value = SimpleJson.DeserializeObject<List<PermitsDTO>>(response.Content);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }


        public ElpsResponse GetCompanyAddressList(string companyID)
        {
            ElpsResponse elspResponse2 = new ElpsResponse();

            try
            {
                logger.Info("About to GetCompany Addresses On Elps with Company ID => " + companyID);

                var request = new RestRequest("api/Address/{CompId}/{email}/{apiHash}", Method.GET);
                request.AddUrlSegment("CompId", companyID);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse2.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse2.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse2.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse2.message = "SUCCESS";
                    elspResponse2.value = SimpleJson.DeserializeObject<List<CompanyAddressDTO>>(response.Content);
                }

            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse2.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse2.message);
            }

            return elspResponse2;
        }



        public ElpsResponse GetAddressByID(string addressID)
        {
            ElpsResponse elspResponse2 = new ElpsResponse();

            try
            {
                logger.Info("About to Get Addresses Details On Elps with ID => " + addressID);

                var request = new RestRequest("api/Address/ById/{Id}/{email}/{apiHash}", Method.GET);
                request.AddUrlSegment("Id", addressID);
                request.AddUrlSegment("email", appEmail);
                request.AddUrlSegment("apiHash", apiHash);

                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse2.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse2.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse2.message = response.StatusCode.ToString();
                }
                else
                {
                    elspResponse2.message = "SUCCESS";
                    elspResponse2.value = SimpleJson.DeserializeObject<CompanyAddressDTO>(response.Content);
                }

            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse2.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse2.message);
            }

            return elspResponse2;
        }




         public ElpsResponse maintainCompanyInformationNew(string detailtype, string CompanyId, string jsonRequest, CompanyChangeModel emailupdate)
        {
            RestRequest request;
            RestRequest request1;
            IRestResponse response = null;
            ElpsResponse elspResponse2 = new ElpsResponse();

            try
            {
                var client = new RestClient(serviceURL);

                if (detailtype.Contains("UPDATE_ADDRESS"))
                {
                    request = new RestRequest("api/Address/{email}/{apiHash}", Method.PUT);
                    request.AddParameter("application/json; charset=utf-8", jsonRequest, ParameterType.RequestBody);
                    request.RequestFormat = DataFormat.Json;
                    request.AddUrlSegment("email", appEmail);
                    request.AddUrlSegment("apiHash", apiHash);
                    response = client.Execute(request);
                }
                else if (detailtype.Contains("UPDATE_PROFILE"))
                {
                    request = new RestRequest("api/company/Edit/{email}/{apiHash}", Method.PUT);
                    request.AddParameter("application/json; charset=utf-8", jsonRequest, ParameterType.RequestBody);
                    request.RequestFormat = DataFormat.Json;
                    request.AddUrlSegment("email", appEmail);
                    request.AddUrlSegment("apiHash", apiHash);
                    response = client.Execute(request);

                    string jsonRequest1 = JsonConvert.SerializeObject(emailupdate);
                    request1 = new RestRequest("api/Accounts/ChangeEmail/{email}/{apiHash}", Method.POST);
                    request1.AddParameter("application/json; charset=utf-8", jsonRequest1, ParameterType.RequestBody);
                    request1.RequestFormat = DataFormat.Json;
                    request1.AddUrlSegment("email", appEmail);
                    request1.AddUrlSegment("apiHash", apiHash);
                    response = client.Execute(request1);

                }
                else if (detailtype.Contains("ADD_ADDRESS"))
                {
                    request = new RestRequest("api/Address/{CompId}/{email}/{apiHash}", Method.POST);
                    request.AddParameter("application/json; charset=utf-8", jsonRequest, ParameterType.RequestBody);
                    request.RequestFormat = DataFormat.Json;
                    request.AddUrlSegment("CompId", CompanyId);
                    request.AddUrlSegment("email", appEmail);
                    request.AddUrlSegment("apiHash", apiHash);
                    response = client.Execute(request);
                }
                else if (detailtype.Contains("UPDATE_DIRECTOR"))
                {
                    request = new RestRequest("api/Directors/{email}/{apiHash}", Method.PUT);
                    request.AddParameter("application/json; charset=utf-8", jsonRequest, ParameterType.RequestBody);
                    request.RequestFormat = DataFormat.Json;
                    request.AddUrlSegment("email", appEmail);
                    request.AddUrlSegment("apiHash", apiHash);
                    response = client.Execute(request);
                }
                else if (detailtype.Contains("ADD_DIRECTOR"))
                {
                    request = new RestRequest("api/Directors/{CompId}/{email}/{apiHash}", Method.PUT);
                    request.AddParameter("application/json; charset=utf-8", jsonRequest, ParameterType.RequestBody);
                    request.RequestFormat = DataFormat.Json;
                    request.AddUrlSegment("CompId", CompanyId);
                    request.AddUrlSegment("email", appEmail);
                    request.AddUrlSegment("apiHash", apiHash);
                    response = client.Execute(request);
                }


                if (response.ErrorException != null)
                {
                    elspResponse2.message = response.ErrorMessage;
                    return elspResponse2;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse2.message = response.ResponseStatus.ToString();
                    return elspResponse2;
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse2.message = response.StatusCode.ToString();
                    return elspResponse2;
                }
                else
                {
                    if (detailtype.Contains("ADD_ADDRESS"))
                    {
                        elspResponse2.value = JsonConvert.DeserializeObject<List<CompanyAddressDTO>>(response.Content);
                    }

                    /*
                               else if (detailtype.Contains("UPDATE_PROFILE"))
                               {
                                   elspResponse2.value = JsonConvert.DeserializeObject<CompanyDetail>(response.Content);
                               }*/


                    elspResponse2.message = "SUCCESS";
                    return elspResponse2;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                elspResponse2.message = ex.Message;
            }

            return elspResponse2;
        }









        public static async Task<string> postExternalTest(string api, JObject values)
        {
            using (var client = new HttpClient())
            {
                var baseUri = getIGRBase() + "/api/";
                client.BaseAddress = new Uri(baseUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", getIGRBearer());

                string output = JsonConvert.SerializeObject(values);
                var response = await client.PostAsync(api,
                    new StringContent(output, Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();

                    return responseJson;
                }
                return null;

            }
        }




        public static string getIGRBase()
        {
            return "https://igr.dpr.gov.ng";
        }


        public static string getIGRBearer()
        {
            return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJwb3J0YWxfbmFtZSI6IkxPQlAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoibG9icEBkcHIuZ292Lm5nIiwianRpIjoiYzNlMmVmMzQtMzI3MS00NDRhLWJlZGMtZDJjOTM5NzU2ZGYxIiwiZXhwIjo0NzYzNzkxODUxfQ.5CD3x-auY_5YHQU-Tb1erVHv0A2taatRyRb0mXF8P0M";
        }









        public ElpsResponse GetPaymentDetails(object paymentRequest, string ElpsId)
        {

            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info(JsonConvert.SerializeObject(paymentRequest));

                var request = new RestRequest("api/Payments/{companyid}/{apiEmail}/{apiHash}", Method.POST);
                request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(paymentRequest), ParameterType.RequestBody);
                request.RequestFormat = DataFormat.Json;
                request.AddUrlSegment("companyid", ElpsId);
                request.AddUrlSegment("apiEmail", appEmail);
                request.AddUrlSegment("apiHash", apiHash);


                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));
                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {
                    var JSONContent = response.Content;
                    //if (JSONContent.Contains("generated") || JSONContent.Contains("Exist"))
                    //{
                        elspResponse.message = "SUCCESS";
                        elspResponse.value = JsonConvert.DeserializeObject<PaymentResponse>(JSONContent);

                    //}
                }

            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }









        //public ElpsResponse GetPaymentDetails(PaymentRequest paymentRequest, string ElpsId)
        //{

        //    ElpsResponse elspResponse = new ElpsResponse();

        //    try
        //    {
        //        logger.Info("About to Get Payment Details On Elps for Application => " + paymentRequest.orderId);

        //        //string paymentjsonFormatted = JValue.Parse(JsonConvert.SerializeObject(paymentRequest)).ToString(Formatting.Indented);
        //        //string paymentjsonFormatted = SimpleJson.SerializeObject(paymentRequest);
        //        string paymentjsonFormatted = JValue.Parse(SimpleJson.SerializeObject(paymentRequest)).ToString(Formatting.Indented);
        //        //logger.Info(paymentjsonFormatted);

        //        var request = new RestRequest("api/Payments/{companyid}/{apiEmail}/{apiHash}", Method.POST);
        //        //request.Parameters.Clear();
        //        //request.AddParameter("application/json", paymentjsonFormatted, ParameterType.RequestBody);
        //        //request.AddParameter("application/json; charset=utf-8", paymentjsonFormatted, ParameterType.RequestBody);
        //        request.RequestFormat = DataFormat.Json;
        //        //request.AddBody(paymentRequest);
        //        request.AddUrlSegment("companyid", ElpsId);
        //        request.AddUrlSegment("apiEmail", appEmail);
        //        request.AddUrlSegment("apiHash", apiHash);


        //        var client = new RestClient(serviceURL);
        //        //logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));

        //        //test Header


        //        var sb = new StringBuilder();
        //        foreach (var param in request.Parameters)
        //        {
        //            sb.AppendFormat("{0}: {1}\r\n", param.Name, param.Value);
        //        }
        //        logger.Info("About to Execute Elsp RestUrl => " + sb.ToString());




        //        IRestResponse response = client.Execute(request);
        //        logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


        //        if (response.ErrorException != null)
        //        {
        //            elspResponse.message = response.ErrorMessage;
        //        }
        //        else if (response.ResponseStatus != ResponseStatus.Completed)
        //        {
        //            elspResponse.message = response.ResponseStatus.ToString();
        //        }
        //        else if (response.StatusCode != HttpStatusCode.OK)
        //        {
        //            elspResponse.message = response.StatusCode.ToString();
        //        }
        //        else
        //        {
        //            var JSONContent = response.Content;
        //            if (JSONContent.Contains("generated") || JSONContent.Contains("Exist"))
        //            {
        //                elspResponse.message = "SUCCESS";
        //                elspResponse.value = SimpleJson.DeserializeObject<PaymentResponse>(JSONContent);

        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Last Exception =>" + ex.Message);
        //        elspResponse.message = ex.Message;
        //    }
        //    finally
        //    {
        //        logger.Info("About to Return with Message => " + elspResponse.message);
        //    }

        //    return elspResponse;
        //}



        public ElpsResponse GetPaymentDetailsNew(string ApplicationId, JObject values, string ElpsId)
        {

            ElpsResponse elspResponse = new ElpsResponse();

            try
            {
                logger.Info("About to Get Payment Details On Elps for Application => " + ApplicationId);
                string paymentjsonFormatted = JValue.Parse(JsonConvert.SerializeObject(values)).ToString(Formatting.Indented);
                logger.Info("paymentjsonFormatted => " + paymentjsonFormatted);

                var request = new RestRequest("api/Payments/{companyid}/{apiEmail}/{apiHash}", Method.POST);
                request.AddParameter("application/json; charset=utf-8", paymentjsonFormatted, ParameterType.RequestBody);
                request.RequestFormat = DataFormat.Json;
                request.AddUrlSegment("companyid", ElpsId);
                request.AddUrlSegment("apiEmail", appEmail);
                request.AddUrlSegment("apiHash", apiHash);


                var client = new RestClient(serviceURL);
                logger.Info("About to Execute Elsp RestUrl => " + UtilityHelper.BuildDebugUri(client, request));

                /*
                var sb = new StringBuilder();
                foreach (var param in request.Parameters)
                {
                    sb.AppendFormat("{0}: {1}\r\n", param.Name, param.Value);
                }
                logger.Info("About to Execute Elsp RestUrl => " + sb.ToString());
                 * */


                IRestResponse response = client.Execute(request);
                logger.Info("Response Exception =>" + response.ErrorException + "\r\nResponse Status =>" + response.ResponseStatus + "\r\nStatus Code =>" + response.StatusCode);


                if (response.ErrorException != null)
                {
                    elspResponse.message = response.ErrorMessage;
                }
                else if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    elspResponse.message = response.ResponseStatus.ToString();
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    elspResponse.message = response.StatusCode.ToString();
                }
                else
                {

                        elspResponse.message = "SUCCESS";
                        elspResponse.value = SimpleJson.DeserializeObject<PaymentResponse>(response.Content);

                    /*
                    var JSONContent = response.Content;
                    if (JSONContent.Contains("generated") || JSONContent.Contains("Exist") || JSONContent.Contains("Exist") )
                    {
                        elspResponse.message = "SUCCESS";
                        elspResponse.value = SimpleJson.DeserializeObject<PaymentResponse>(JSONContent);
                    }
                    */
                }
                
            }
            catch (Exception ex)
            {
                logger.Error("Last Exception =>" + ex.Message);
                elspResponse.message = ex.Message;
            }
            finally
            {
                logger.Info("About to Return with Message => " + elspResponse.message);
            }

            return elspResponse;
        }





    }
}