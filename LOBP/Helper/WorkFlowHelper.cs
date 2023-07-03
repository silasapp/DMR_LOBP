using LOBP.DbEntities;
using log4net;
using LOBP.Helper;
using LOBP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Validation;

namespace LOBP.Helper
{
    public class WorkFlowHelper
    {

        //private CommonHelper commonhelper = new CommonHelper();
        private static ILog logger = log4net.LogManager.GetLogger(typeof(WorkFlowHelper));
        private UserHelper userHelper = new UserHelper();
        private ElpServiceHelper elpServiceManager = new ElpServiceHelper();
        public WorkFlowHelper() { }

        public ResponseWrapper processAction(LubeBlendingDBEntities dbCtxt1, string ApplicationId, string Action, string FromUserId, string Comment, string fieldlocation, string Delegateduser)
        {

            UserMaster nextUser = null;
            string NextProcessor = null;
            ResponseWrapper responsewrapper = new ResponseWrapper();
            List<string> applicationTypeList = new List<string>();
            WorkFlowNavigation mainWkflowNavigation = default(WorkFlowNavigation);
            try
            {
                using (LubeBlendingDBEntities dbCtxt = new LubeBlendingDBEntities())
                {
                    var statecode = (from s in dbCtxt.ApplicationRequests where s.ApplicationId == ApplicationId select s.StateCode).FirstOrDefault();
                    var Appcode = userHelper.AppLicenseCodeType(ApplicationId);
                    logger.Info("UserID =>" + FromUserId + ", Action => " + Action + ", ApplicationID =>" + ApplicationId);

                    applicationTypeList.Add("ALL");
                    UserMaster userMaster = dbCtxt.UserMasters.Where(c => c.UserId.Trim() == FromUserId.Trim() && c.Status == "ACTIVE").FirstOrDefault();
                    var fldlocation = (from u in dbCtxt.ApplicationRequests
                                       join w in dbCtxt.WorkFlowNavigations on u.CurrentStageID equals w.CurrentStageID
                                       where w.ActionRole == userMaster.UserRoles && u.CurrentOfficeLocation == userMaster.UserLocation && u.ApplicationId == ApplicationId && u.CurrentStageID == w.CurrentStageID
                                       select new { w.FieldLocationApply, u.CurrentOfficeLocation }).FirstOrDefault();

                    if (userMaster ==
                     default(UserMaster))
                    {
                        responsewrapper.status = false;
                        responsewrapper.value = "USER RECORD WITH ID " + FromUserId + " CANNOT BE FOUND ON THE SYSTEM";
                        return responsewrapper;
                    }

                    logger.Info("Done with UserMaster Record");
                    ApplicationRequest appmaster = dbCtxt.ApplicationRequests.Where(c => c.ApplicationId == ApplicationId).FirstOrDefault();
                    var location = (from l in dbCtxt.FieldLocations where l.FieldLocationID == appmaster.CurrentOfficeLocation select l.FieldLocationID).FirstOrDefault();//l.FieldType == mainWkflowNavigation.FieldLocationApply && 

                   
                    if (appmaster == default(ApplicationRequest))

                    {
                        responsewrapper.status = false;
                        responsewrapper.value = "APPLICATION REFERENCE " + ApplicationId + " CANNOT BE FOUND ON THE SYSTEM";
                        return responsewrapper;
                    }

                    logger.Info("Done with Application Record");
                    applicationTypeList.Add(appmaster.ApplicationTypeId.Trim());
                    nextUser = dbCtxt.UserMasters.Where(u => u.Status == "ACTIVE" && u.UserLocation == fieldlocation && u.UserId == Delegateduser).FirstOrDefault();                     


                    var wkflowNavigationList = Action != "Delegate" ? (from w in dbCtxt.WorkFlowNavigations
                                                                       join a in dbCtxt.ApplicationRequests on w.CurrentStageID
                                                                       equals a.CurrentStageID
                                                                       where a.ApplicationId == ApplicationId && a.CurrentAssignedUser == FromUserId && w.Action == Action
                                                                       select w).ToList()
                                                : (from w in dbCtxt.WorkFlowNavigations
                                                   join a in dbCtxt.ApplicationRequests on w.CurrentStageID
                                                   equals a.CurrentStageID
                                                   where a.ApplicationId == ApplicationId && w.TargetRole == nextUser.UserRoles && a.CurrentAssignedUser == FromUserId && w.Action == Action
                                                   select w).ToList();
                    if (wkflowNavigationList.Count == 0)
                    {
                        responsewrapper.status = false;
                        responsewrapper.value = "WORKFLOW NAVIGATION PARAMETER => " + appmaster.ApplicationTypeId + "," + appmaster.CurrentStageID + "," + Action + " CANNOT BE RETRIEVED, CONTACT ADMINISTRATOR";
                        return responsewrapper;
                    }
                    else
                    {
                        foreach (var wkflowNavigation in wkflowNavigationList)
                        {
                            //EXAMPLE  2  =  2
                            if (wkflowNavigation.ActionRole == "COMPANY")
                            {
                                mainWkflowNavigation = wkflowNavigation;
                                break;
                            }

                            //EXAMPLE  2  !=  4
                            else if (wkflowNavigation.FieldLocationApply != appmaster.CurrentOfficeLocation && userHelper.IsNumeric(wkflowNavigation.FieldLocationApply))
                            {
                                continue;
                            }
                            else
                            {
                                if ( wkflowNavigation.FieldLocationApply.Contains("ZN"))
                                {
                                    mainWkflowNavigation = wkflowNavigation;
                                    break;
                                }

                                else if (wkflowNavigation.FieldLocationApply.Contains("FD"))
                                {
                                    mainWkflowNavigation = wkflowNavigation;
                                    break;
                                }
                                else if (wkflowNavigation.FieldLocationApply.Contains("HQ"))
                                {
                                    mainWkflowNavigation = wkflowNavigation;
                                    break;
                                }
                                
                            }
                        }
                    }
                    responsewrapper.receivedLocation = "";
                    logger.Info("Done with WorkFlow Navigation Record");
                    logger.Info("Current User Role =>" + userMaster.UserRoles);
                    logger.Info("Company UserId =>" + appmaster.ApplicantUserId);
                    logger.Info("LastOffice Location =>" + appmaster.CurrentOfficeLocation);
                   
                    if (mainWkflowNavigation.FieldLocationApply != null)
                    {
                        if ((mainWkflowNavigation.FieldLocationApply == "HQ" || mainWkflowNavigation.TargetRole =="AD RBP"))
                        {

                            fieldlocation = dbCtxt.FieldLocations.Where(f => f.FieldType == "HQ").FirstOrDefault().FieldLocationID;


                        }

                        
                    }
                    
                    if (mainWkflowNavigation.TargetRole == "COMPANY")
                    {
                        NextProcessor = appmaster.ApplicantUserId;
                        nextUser = dbCtxt.UserMasters.Where(u => u.UserId == appmaster.ApplicantUserId).FirstOrDefault();
                        appmaster.CurrentStageID = mainWkflowNavigation.NextStateID;
                        fieldlocation = appmaster.CurrentOfficeLocation;

                        if ((Action == "Accept" || Action == "LRAccept") && (mainWkflowNavigation.NextStateID == 21 || mainWkflowNavigation.NextStateID == 49))
                        {
                            //completed app
                            var content = "";
                            appmaster.Status = "Approved";
                            var subject = "Application Approved";

                            content = Action == "Accept" ? "Application with the reference number " + ApplicationId + " has been approved successfully." : "Your Legacy application with the reference number " + ApplicationId + " has been approved successfully.";
                            var sendemail = userHelper.SendStaffEmailMessage(NextProcessor, subject, content);

                            var Supervisoremail = (from a in dbCtxt.ActionHistories where a.ApplicationId == ApplicationId && a.TriggeredByRole == "SUPERVISOR" select a.TriggeredBy).FirstOrDefault();
                            content = Action == "Accept" ? "Application with the reference number " + ApplicationId + " and company name " + appmaster.ApplicantName + " has been approved successfully." : "Legacy application with the reference number " + ApplicationId + " and company name " + appmaster.ApplicantName + " has been approved successfully.";

                            if (Supervisoremail != null)
                            {
                                var sendemai = userHelper.SendStaffEmailMessage(Supervisoremail, subject, content);
                            }


                        }


                        //else if ((Action == "Reject" || Action == "LSReject" || Action == "Rejectmk") && (mainWkflowNavigation.CurrentStageID == 6 || mainWkflowNavigation.CurrentStageID == 7 || mainWkflowNavigation.CurrentStageID == 14 || mainWkflowNavigation.CurrentStageID == 15 || mainWkflowNavigation.CurrentStageID == 16 || mainWkflowNavigation.CurrentStageID == 17
                        //    || mainWkflowNavigation.CurrentStageID == 27 || mainWkflowNavigation.CurrentStageID == 28 || mainWkflowNavigation.CurrentStageID == 29 || mainWkflowNavigation.CurrentStageID == 30 || mainWkflowNavigation.CurrentStageID == 11 || mainWkflowNavigation.CurrentStageID == 12
                        //    || mainWkflowNavigation.CurrentStageID == 39 || mainWkflowNavigation.CurrentStageID == 40 || mainWkflowNavigation.CurrentStageID == 41 || mainWkflowNavigation.CurrentStageID == 42 || mainWkflowNavigation.CurrentStageID == 47))
                        else if ((Action == "Reject" || Action == "LSReject" || Action == "Rejectmk"))
                        {
                            //rejected app
                            appmaster.Status = "Rejected";


                            var subject = "Application Rejected";
                            var content = "Application with the reference number " + ApplicationId + " was rejected, please login to the portal and see details on your dashboard.";
                            var sendmail = userHelper.SendStaffEmailMessage(NextProcessor, subject, content);

                        }
                    }
                    else
                    {
                        //WorkFlowNavigation wkflowNavCurrentStageID = dbCtxt.WorkFlowNavigations.Where(c => c.CurrentStageID == appmaster.CurrentStageID && c.Action.Trim() == Action.Trim() && applicationTypeList.Contains(c.ApplicationType.Trim())).FirstOrDefault();

                        var targettorole = wkflowNavigationList.FirstOrDefault().TargetRole;

                        var applocations = (from a in dbCtxt.ActionHistories where a.ApplicationId == ApplicationId && a.TargetedToRole == targettorole select a.CurrentFieldLocation).ToList().LastOrDefault();
                        var nxtusers = (from a in dbCtxt.ActionHistories where a.ApplicationId == ApplicationId && a.TriggeredByRole == targettorole select new { a.TriggeredBy, a.CurrentFieldLocation }).ToList().LastOrDefault();
                        var ActionSchedule = (from a in dbCtxt.ActionHistories where (a.Action == "ScheduleInspectionfdr" || a.Action == "ScheduleInspectionfds" || a.Action == "ScheduleInspectionhqr" || a.Action == "ScheduleInspectionhqs" || a.Action == "ScheduleInspectionznr" || a.Action == "ScheduleInspectionzns") && (a.ApplicationId == ApplicationId) select a).ToList().LastOrDefault();


                        if (appmaster.CurrentStageID == wkflowNavigationList.FirstOrDefault().CurrentStageID)//14
                        {
                            appmaster.Status = "Processing";
                           // var histry = (from a in dbCtxt.ActionHistories where (a.ApplicationId == ApplicationId) && (a.Action == "Reject" || a.Action == "Rejectmk") select new { a.TriggeredBy, a.CurrentStageID }).ToList().LastOrDefault();
                            
                            var histry = (from n in dbCtxt.ActionHistories
                                     where (n.ApplicationId == ApplicationId) && (n.Action == "Reject" || n.Action == "Rejectmk")
                                     group n by n.ActionId into g
                                    select new { AccountId = g.Key, TriggeredBy = g.FirstOrDefault().TriggeredBy, CurrentStageID = g.FirstOrDefault().CurrentStageID, Date = g.Max(t => t.ActionDate) }).ToList().LastOrDefault();

                            if (mainWkflowNavigation.ActionRole == "COMPANY" && Action == "ReSubmit" && histry != null)
                            {
                                appmaster.Status = "Processing";
                                appmaster.CurrentStageID = histry.CurrentStageID;
                                NextProcessor = histry.TriggeredBy;
                                fieldlocation = appmaster.CurrentOfficeLocation;
                            }



                            else if (Action == "Delegate")
                            {
                                var Checkexistingschedule = (from a in dbCtxt.ActionHistories where (a.ApplicationId == ApplicationId) && (a.Action == "AcceptInspectionfdr" || a.Action == "AcceptInspectionfds" || a.Action == "AcceptInspectionznr" || a.Action == "AcceptInspectionzns") && (mainWkflowNavigation.CurrentStageID == 24 || mainWkflowNavigation.CurrentStageID == 36) select new { a.NextStateID, a.TargetedTo }).ToList().LastOrDefault();
                                if(Checkexistingschedule != null)
                                {
                                    appmaster.CurrentStageID = Checkexistingschedule.NextStateID;
                                    NextProcessor = Checkexistingschedule.TargetedTo;
                                }
                                else
                                {
                                    appmaster.CurrentStageID = mainWkflowNavigation.NextStateID;
                                    NextProcessor = Delegateduser;
                                }
                                
                            }

                            else if (Action == "Reject" || Action == "Rejectopscon"|| Action == "Rejectzn")
                            {
                                var nxtuser = (from a in dbCtxt.ActionHistories where a.ApplicationId == ApplicationId && a.TriggeredByRole == targettorole select new { a.TriggeredBy, a.CurrentFieldLocation }).FirstOrDefault();

                                if (wkflowNavigationList.FirstOrDefault().FieldLocationApply == "FD")
                                {
                                    var applocation = (from a in dbCtxt.ActionHistories where a.ApplicationId == ApplicationId && a.TriggeredByRole == "COMPANY" select a.CurrentFieldLocation).FirstOrDefault();

                                    //fieldlocation = applocation;
                                    if (nxtuser != null) 
                                    {


                                        var role = (from u in dbCtxt.UserMasters where u.UserId == nxtuser.TriggeredBy select new { u.UserRoles, u.UserLocation }).FirstOrDefault();
                                        NextProcessor = (targettorole == role.UserRoles && nxtuser.CurrentFieldLocation == role.UserLocation) ? nxtuser.TriggeredBy : userHelper.GetNextProcessingStaff(dbCtxt, appmaster, mainWkflowNavigation.TargetRole, fieldlocation, Action, mainWkflowNavigation.ActionRole);
                                        fieldlocation = nxtuser.CurrentFieldLocation; 
                                    }

                                    appmaster.CurrentStageID = wkflowNavigationList.FirstOrDefault().NextStateID;
                                }
                                else
                                {
                                    

                                    fieldlocation = appmaster.CurrentOfficeLocation;
                                    if (nxtusers != null) 
                                    {
                                        var role = (from u in dbCtxt.UserMasters where u.UserId == nxtusers.TriggeredBy select new { u.UserRoles, u.UserLocation }).FirstOrDefault();
                                        NextProcessor = (targettorole == role.UserRoles && nxtusers.CurrentFieldLocation == role.UserLocation) ? nxtusers.TriggeredBy : userHelper.GetNextProcessingStaff(dbCtxt, appmaster, mainWkflowNavigation.TargetRole, fieldlocation, Action, mainWkflowNavigation.ActionRole);
                                        if (Action == "Rejectopscon" || Action == "Rejectzn")
                                        {
                                            fieldlocation = nxtuser.CurrentFieldLocation;
                                        }
                                         
                                    }
                                    appmaster.CurrentStageID = wkflowNavigationList.FirstOrDefault().NextStateID;//currentstageid;
                                }

                            }
                            else if ((appmaster.CurrentStageID == 14 || appmaster.CurrentStageID == 15 || appmaster.CurrentStageID == 27 || appmaster.CurrentStageID == 28 || appmaster.CurrentStageID == 39 || appmaster.CurrentStageID == 40) && Action == "Accept")//after filling checklist maimtain same user
                            {
                                NextProcessor = appmaster.CurrentAssignedUser;
                                appmaster.CurrentStageID = mainWkflowNavigation.NextStateID;
                            }
                            //else if ((wkflowNavigationList.FirstOrDefault().ActionRole == "REVIEWER" || wkflowNavigationList.FirstOrDefault().ActionRole == "SUPERVISOR") && (appmaster.IsLegacy == "NO") && (Action != "ReSubmit") && (nxtusers != null) && appmaster.CurrentStageID != 30 && appmaster.CurrentStageID != 42)
                            //{
                            //    fieldlocation = appmaster.CurrentOfficeLocation;
                            //    if (nxtusers != null) { NextProcessor = nxtusers.TriggeredBy; }
                            //    appmaster.CurrentStageID = wkflowNavigationList.FirstOrDefault().NextStateID;

                            //}
                            else if(appmaster.CurrentStageID == 10 && ActionSchedule != null)//Accept or Reject Inspection Schedule by Marketer
                            {
                                fieldlocation = appmaster.CurrentOfficeLocation;
                                NextProcessor = ActionSchedule.TriggeredBy;
                                appmaster.CurrentStageID = mainWkflowNavigation.NextStateID;
                            }

                            else
                            {
                                nextUser = dbCtxt.UserMasters.Where(u => u.UserRoles.Contains(mainWkflowNavigation.TargetRole) && u.Status == "ACTIVE" && u.UserLocation == fieldlocation).FirstOrDefault();
                                NextProcessor = userHelper.GetNextProcessingStaff(dbCtxt, appmaster, mainWkflowNavigation.TargetRole, fieldlocation, Action, mainWkflowNavigation.ActionRole);
                                appmaster.CurrentStageID = mainWkflowNavigation.NextStateID;
                            }

                        }
                    }



                    if (!string.IsNullOrEmpty(NextProcessor))
                    {
                        nextUser = dbCtxt.UserMasters.Where(u => u.UserId == NextProcessor).FirstOrDefault();
                    }

                    else
                    {
                        responsewrapper.status = false;
                        responsewrapper.value = "No User was maintaned for the Next WorkFlow on the System, Kindly Liase with Admin and try again";
                        return responsewrapper;
                    }

                    appmaster.CurrentAssignedUser = NextProcessor;

                    appmaster.CurrentOfficeLocation = fieldlocation;
                    appmaster.ModifiedDate = DateTime.Now;
                    logger.Info("About to Start Notification");
                    if (mainWkflowNavigation.NotifyAction != null && mainWkflowNavigation.TargetRole != null && mainWkflowNavigation.NotifyAction.Contains("NORMAL"))
                    {
                        //foreach (string targetRole in mainWkflowNavigation.TargetRole.Split(','))
                        //{
                            insertIntoHistory(dbCtxt, appmaster, userMaster, nextUser, Action, Comment, mainWkflowNavigation.CurrentStageID, mainWkflowNavigation.NextStateID);
                        //}
                    }

                    responsewrapper.status = true;

                    string stateType = null;
                    String nextStageName = null;
                    WorkFlowState wkflowstate = dbCtxt.WorkFlowStates.Where(w => w.StateID == mainWkflowNavigation.NextStateID).FirstOrDefault();
                    stateType = wkflowstate.StateType;
                    nextStageName = wkflowstate.StateName;
                    responsewrapper.receivedBy = appmaster.CurrentAssignedUser;
                    responsewrapper.nextStageId = Convert.ToString(mainWkflowNavigation.NextStateID);
                    responsewrapper.receivedLocation = "";
                    logger.Info("Done With Notification");
                    dbCtxt.SaveChanges();
                    if (mainWkflowNavigation.TargetRole != "COMPANY")
                    {

                        var subject = "Application waiting for Approval";
                        var content = "Application with the reference number " + ApplicationId + " is currently on your desk waiting for approval.";

                        var outofofficestaff = (from a in dbCtxt.OutofOffices where a.Relieved == appmaster.CurrentAssignedUser && a.Status == "Started" select a).FirstOrDefault();
                        if (outofofficestaff != null)
                        {
                            var subject1 = "Relieved Application waiting for Approval";
                            var content1 = "Application is currently on " + outofofficestaff.Relieved + " desk waiting for your approval. NOTE: You relieved " + outofofficestaff.Relieved;

                            var sendemail = userHelper.SendStaffEmailMessage(outofofficestaff.Reliever, subject1, content1);
                        }

                        var sendmail = userHelper.SendStaffEmailMessage(NextProcessor, subject, content);

                        var Elps = (from a in dbCtxt.ApplicationRequests
                                    join u in dbCtxt.UserMasters on a.ApplicantUserId equals u.UserId
                                    where a.ApplicationId == ApplicationId && a.ApplicantUserId == u.UserId
                                    select new { u.ElpsId, a.Status }).FirstOrDefault();

                        elpServiceManager.GetElpAppUpdateStatus(ApplicationId, Elps.ElpsId, Elps.Status);
                    }




                    var received = (from w in dbCtxt.ActionHistories where w.ApplicationId == appmaster.ApplicationId select w).ToList().LastOrDefault();
                    responsewrapper.receivedByRole = received.TargetedToRole;
                    var allfieldlocation = Convert.ToString(appmaster.CurrentOfficeLocation);
                    FieldLocation fd = dbCtxt.FieldLocations.Where(f => f.FieldLocationID == allfieldlocation).FirstOrDefault();

                    if (fd !=
                     default(FieldLocation))
                    {
                        responsewrapper.receivedLocation = fd.Description;
                    }

                    logger.Info("Current State Type =>" + stateType);

                    if (stateType == "PROGRESS")
                    {
                        responsewrapper.value = "Application has been moved To " + responsewrapper.receivedBy + "(" + responsewrapper.receivedByRole + ") at " + responsewrapper.receivedLocation;//responsewrapper.receivedBy 
                    }
                    else if (stateType == "COMPLETE")
                    {
                        responsewrapper.value = "Application Has been Approved and License/Approval Generated ";
                    }
                    else if (stateType == "LCOMPLETE")
                    {
                        responsewrapper.value = "Legacy License Approved";
                    }
                    else if (stateType == "REJECTED")
                    {
                        responsewrapper.value = "Approval Has been Denied and License/Approval Rejected ";
                    }
                    else if (stateType == "CONTINUE" || stateType == "START")
                    {
                        responsewrapper.value = "Application Has been Moved To " + appmaster.ApplicantName + " (" + nextStageName.ToUpper() + ") Stage";
                    }

                }
            }


            catch (DbEntityValidationException ex)
            {
                string l = string.Empty;
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        l += String.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }

                responsewrapper.status = false;
                responsewrapper.value = "General Error occured during Workflow Navigation, Please Contact Support";
                logger.Error(l);
            }


            catch (Exception ex)
            {
                logger.Error(ex.InnerException);

                responsewrapper.status = false;
                responsewrapper.value = "General Error occured during Workflow Navigation, Please Contact Support";
                return responsewrapper;
            }

            return responsewrapper;
        }


        private void insertIntoHistory(LubeBlendingDBEntities dbCtxt, ApplicationRequest appRequest, UserMaster actionUserMaster, UserMaster targetUserMaster, string userAction, string comment, int currentStageId, int nextStageId)
        {
            ActionHistory actionHistory = new ActionHistory();
            actionHistory.CurrentFieldLocation = appRequest.CurrentOfficeLocation;
            actionHistory.LicenseTypeId = appRequest.LicenseTypeId;
            actionHistory.ApplicationId = appRequest.ApplicationId;
            actionHistory.CurrentStageID = (short)currentStageId;
            actionHistory.Action = userAction;
            actionHistory.ActionDate = DateTime.UtcNow;
            var locationfield = (from f in dbCtxt.FieldLocations where f.FieldLocationID == actionUserMaster.UserLocation select f.Description).FirstOrDefault();
            var comRejected = locationfield != null ? " => Rejected by " + actionUserMaster.UserId + " (" + actionUserMaster.UserRoles + ") at " + locationfield : " => Rejected by " + appRequest.ApplicantName + " (" + actionUserMaster.UserRoles + ")" + locationfield;
            if (targetUserMaster.UserRoles == "COMPANY" || userAction == "ReSubmit")
            {
                actionHistory.MESSAGE = (comment != null) ? comment : dbCtxt.WorkFlowStates.Where(w => w.StateID == appRequest.CurrentStageID).FirstOrDefault().StateName;
            }
            else if (userAction == "Reject")
            {
                actionHistory.MESSAGE = (comment != null) ? comment + " " + comRejected : dbCtxt.WorkFlowStates.Where(w => w.StateID == appRequest.CurrentStageID).FirstOrDefault().StateName;
            }
            else if (userAction == "ReSubmit")
            {
                actionHistory.MESSAGE = "Application was resubmitted by " + appRequest.ApplicantName + " after Currentions have been made.";
            }
            else
            {
                actionHistory.MESSAGE = (comment != null) ? comment : dbCtxt.WorkFlowStates.Where(w => w.StateID == appRequest.CurrentStageID).FirstOrDefault().StateName;
            }

            actionHistory.TriggeredBy = actionUserMaster.UserId;
            actionHistory.TriggeredByRole = actionUserMaster.UserRoles;

            if (targetUserMaster != default(UserMaster))
            {
                actionHistory.TargetedTo = targetUserMaster.UserId;
                actionHistory.TargetedToRole = targetUserMaster.UserRoles;
            }

            actionHistory.NextStateID = (short)nextStageId;
            actionHistory.StatusMode = "INIT";
            actionHistory.ActionMode = "";

            dbCtxt.ActionHistories.Add(actionHistory);
        }


    }
}