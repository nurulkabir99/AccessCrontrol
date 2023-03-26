using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using MvcHRMS.Models;

namespace MvcHRMS.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/

        




        public ActionResult Index()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                return RedirectToAction("Dashboard", "User");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to view your dashboard!";
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /User/Dashboard

        public ActionResult Dashboard()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to view your dashboard!";
            return RedirectToAction("Login", "Account");
        }


        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /User/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /User/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult EmployeeList()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                try
                {
                    Employee emp = new Employee();
                    var list = emp.GetAllEmpData();
                    return View(list);
                }
                catch (Exception ex)
                {
                    Session["error_div"] = "true";
                    Session["error_msg"] = "Error happend! Error Message is: " + ex.Message;
                    return RedirectToAction("Dashboard", "User");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }


        // GET: /Admin/EmployeeInfoDetails

        public ActionResult EmployeeInfoDetails()
        {
           if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                var eid = Session["EmpID"].ToString();
                try
                {
                    Employee emp = new Employee();
                    var Info = emp.GetEmployeeData(eid);

                    Spouse spouse = new Spouse();
                    var listSpInfo = spouse.GetSpouseData(eid);
                    ViewBag.ListSpouseInfo = listSpInfo;

                    Child child = new Child();
                    var listChInfo = child.GetChildData(eid);
                    ViewBag.ListChildInfo = listChInfo;

                    AcademicQualification academicqualification = new AcademicQualification();
                    var listacademicInfo = academicqualification.GetAcademicData(eid);
                    ViewBag.ListAcademicInfo = listacademicInfo;

                    Training trainig = new Training();
                    var listtraininginfo = trainig.GetTrainingData(eid);
                    ViewBag.ListTrainingInfo = listtraininginfo;

                    Experience experience = new Experience();
                    var listexperienceInfo = experience.GetExperienceData(eid);
                    ViewBag.ListExperienceInfo = listexperienceInfo;

                    Skill skill = new Skill();
                    var listskillinfo = skill.GetSkillData(eid);
                    ViewBag.ListSkillinfo = listskillinfo;

                    Reference reference = new Reference();
                    var listRefInfo = reference.GetReferenceData(eid);
                    ViewBag.ListReferenceInfo = listRefInfo;

                    Salary salInfo = new Salary();
                    var listSalaryinfo = salInfo.GetSalaryData(eid);
                    ViewBag.ListSalaryInfo = listSalaryinfo;


                    Benefit beinfo = new Benefit();
                    var listBenefitInfo = beinfo.GetBenefitData(eid);
                    ViewBag.ListBenefitInfo = listBenefitInfo;

                    if (string.IsNullOrEmpty(Info.EmployeeId))
                    {
                        Session["warning_div"] = "true";
                        Session["warning_msg"] = "Dear Employee, Data not found!";
                        return RedirectToAction("Dashboard");
                    }
                    return View(Info);
                }
                catch (Exception ex)
                {
                    Session["error_div"] = "true";
                    Session["error_msg"] = "Error happend! Error Message is: " + ex.Message;
                    return RedirectToAction("Dashboard", "User");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /User/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /User/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /User/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /User/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult GetLeaveUserInfo()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                LeaveClass lc = new LeaveClass();
                var Info = lc.GetLeaveUserInfo(Session["EmpID"].ToString());
                ViewBag.HtmLeaveInfo = new HtmlString(lc.GetRemainingLeaveInfo(Session["EmpID"].ToString()));
                ViewBag.LeaveCategoryList = new SelectList(lc.GetLeaveCategorylist(), "Value", "Text");
                if (string.IsNullOrEmpty(Info.EmployeeId))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Data Not Found";
                    return RedirectToAction("Dashboard");
                }
                return View(Info);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue submit leave application!";
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public ActionResult GetLeaveUserInfo(Models.LeaveClass leaveClass)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                if (leaveClass.InsertLeaveApplication(leaveClass) == "Data Inserted Successfully")
                {
                    Email.SendEmail(leaveClass.LmEmail, "Leave Application",
                        "Dear Line Manager you have got a Leave Application from " + leaveClass.Name + ". His/Her Employee ID is " +
                        leaveClass.EmployeeId + " and the leave type is " + leaveClass.LeaveCategory);


                    NotificationClass notification = new NotificationClass();
                    notification.SenderId = Session["EmpID"].ToString();
                    notification.ReceiverId = leaveClass.LineManagerId;
                    notification.SenderType = "Admin";
                    notification.Notificationtype = "Normal";
                    notification.Subject = "Leave Application";
                    notification.Description = "Dear Employee you have got a leave application from " +
                                               leaveClass.Name + "as a line manager . His/her employee id is " + leaveClass.EmployeeId;

                    notification.InsertNotification(notification);
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Leave application submitted successfully.";
                    return RedirectToAction("Dashboard", "User");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue submit leave application!";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult GetSpecificUserLeaveAll()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                LeaveClass lc = new LeaveClass();
                var leaveinfo = lc.GetSpecificLeaveUserDataAll(Session["EmpID"].ToString());
                if (!leaveinfo.Any())
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Data Not Found";
                    return RedirectToAction("Dashboard");
                }
                return View(leaveinfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to view leave information!";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult GetSpecificLeaveInfo(string lid)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                LeaveClass lc = new LeaveClass();
                var leaveinfo = lc.GetSpecificLeaveInfo(lid, "User");
                if (string.IsNullOrEmpty(leaveinfo.LeaveId))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Data Not Found";
                    return RedirectToAction("Dashboard");
                }
                return View(leaveinfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to view details leave info!";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult GetGetAllPendingProcessingLeaveInfo()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                LeaveClass lc = new LeaveClass();
                var leaveinfoLm = lc.GetAllPendingLeaveForLm(Session["EmpID"].ToString());
                var leaveinfoDh = lc.GetAllProcessingLeaveForDeptHead(Session["EmpID"].ToString());
                var leaveinfoAd = lc.GetAllApprovedLeaveForAdmin(Session["EmpID"].ToString());
                ViewBag.DeptHeadApproval = leaveinfoDh;
                ViewBag.AdminApproval = leaveinfoAd;
                if (leaveinfoLm.Any() || leaveinfoDh.Any() || leaveinfoAd.Any())
                {
                    return View(leaveinfoLm);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Data Not Found";
                return RedirectToAction("Dashboard");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to view leave information!";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Admin/Update Leave approval by Line Manager
        [HttpGet]
        public ActionResult UpdateLeaveApprovalbyLm(string lid)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                LeaveClass leaveInfo = new LeaveClass();
                var lvInfo = leaveInfo.GetSpecificLeaveInfo(lid, "LineManager");
                if (string.IsNullOrEmpty(lvInfo.LeaveId))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                    return RedirectToAction("GetGetAllPendingProcessingLeaveInfo");
                }
                return View(lvInfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Admin/Update Leave approval by Line Manager
        [HttpPost]
        public ActionResult UpdateLeaveApprovalbyLm(Models.LeaveClass leaveClass)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                if (leaveClass.UpdateLeaveApprovalbyLm(leaveClass) == "Data Updated Successfully")
                {
                    NotificationClass notification = new NotificationClass();
                    if (leaveClass.LineManagerStatus == "Accept")
                    {
                        using (var notificationA = new NotificationClass())
                        {
                            notificationA.SenderId = Session["EmpID"].ToString();
                            notificationA.ReceiverId = leaveClass.EmployeeId;
                            notificationA.SenderType = "Line Manger";
                            notificationA.Notificationtype = "Normal";
                            notificationA.Subject = "Leave Application";
                            notificationA.Description =
                                "Dear Employee your leave application accepted by line manager. Now your application is processing.";
                            notification.InsertNotification(notificationA);
                        }
                        using (var notificationL = new NotificationClass())
                        {
                            notificationL.SenderId = leaveClass.EmployeeId;
                            notificationL.ReceiverId = leaveClass.DepartmentHeadId;
                            notificationL.SenderType = "Line Manger refer leave appliaction";
                            notificationL.Notificationtype = "Normal";
                            notificationL.Subject = "Leave Application";
                            notificationL.Description =
                                "Dear Employee you have got a leave application from " +
                                                   leaveClass.Name + "as a department head . His/her employee id is " + leaveClass.EmployeeId + ". Line Manager accepted leave application and refere to you.";
                            notification.InsertNotification(notificationL);
                        }
                        Session["success_div"] = "true";
                        Session["success_msg"] = "Leave application submitted successfully.";
                        return RedirectToAction("GetGetAllPendingProcessingLeaveInfo", "User");

                    }
                    else if (leaveClass.LineManagerStatus == "Reject")
                    {
                        using (var notificationA = new NotificationClass())
                        {
                            notificationA.SenderId = Session["EmpID"].ToString();
                            notificationA.ReceiverId = leaveClass.EmployeeId;
                            notificationA.SenderType = "Line Manger";
                            notificationA.Notificationtype = "Normal";
                            notificationA.Subject = "Leave Application";
                            notificationA.Description =
                                "Dear Employee your leave application rejected by line manager.";
                            notification.InsertNotification(notificationA);
                        }
                        Session["success_div"] = "true";
                        Session["success_msg"] = "Leave application submitted successfully.";
                        return RedirectToAction("GetGetAllPendingProcessingLeaveInfo", "User");
                    }
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Leave Application Submit Fail";
                return RedirectToAction("UpdateLeaveApprovalbyLm");

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Admin/Update Leave approval by Department Head
        [HttpGet]
        public ActionResult UpdateLeaveApprovalbyDh(string lid)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                LeaveClass leaveInfo = new LeaveClass();
                var lvInfo = leaveInfo.GetSpecificLeaveInfo(lid, "DepartmentHead");
                if (string.IsNullOrEmpty(lvInfo.LeaveId))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                    return RedirectToAction("GetGetAllPendingProcessingLeaveInfo");
                }
                return View(lvInfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Admin/Update Leave approval by Line Manager
        [HttpPost]
        public ActionResult UpdateLeaveApprovalbyDh(Models.LeaveClass leaveClass)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                if (leaveClass.UpdateLeaveApprovalbyDh(leaveClass) == "Data Updated Successfully")
                {
                    NotificationClass notification = new NotificationClass();
                    if (leaveClass.DeptHeadStatus == "Accept")
                    {
                        using (var notificationA = new NotificationClass())
                        {
                            notificationA.SenderId = Session["EmpID"].ToString();
                            notificationA.ReceiverId = leaveClass.EmployeeId;
                            notificationA.SenderType = "Department Head";
                            notificationA.Notificationtype = "Normal";
                            notificationA.Subject = "Leave Application";
                            notificationA.Description =
                                "Dear Employee your leave application accepted by Department head. Now your Leave Application is Approved.";
                            notification.InsertNotification(notificationA);
                        }
                        using (var notificationL = new NotificationClass())
                        {
                            notificationL.SenderId = leaveClass.EmployeeId;
                            notificationL.ReceiverId = leaveClass.DepartmentHeadId;
                            notificationL.SenderType = "Department Head Accepted leave appliaction";
                            notificationL.Notificationtype = "Normal";
                            notificationL.Subject = "Leave Application Accepted";
                            notificationL.Description =
                                "Dear Employee you have got a leave application from " +
                                                   leaveClass.Name + "as a Admin . His/her employee id is " + leaveClass.EmployeeId + ". Department Head accepted leave application and refere to you.";
                            notification.InsertNotification(notificationL);
                        }
                        Session["success_div"] = "true";
                        Session["success_msg"] = "Leave application submitted successfully.";
                        return RedirectToAction("GetGetAllPendingProcessingLeaveInfo", "User");

                    }
                    else if (leaveClass.LineManagerStatus == "Reject")
                    {
                        using (var notificationA = new NotificationClass())
                        {
                            notificationA.SenderId = Session["EmpID"].ToString();
                            notificationA.ReceiverId = leaveClass.EmployeeId;
                            notificationA.SenderType = "Department Head";
                            notificationA.Notificationtype = "Normal";
                            notificationA.Subject = "Leave Application Rejected";
                            notificationA.Description =
                                "Dear Employee your leave application rejected by department head.";
                            notification.InsertNotification(notificationA);
                        }
                        Session["success_div"] = "true";
                        Session["success_msg"] = "Leave application rejected successfully.";
                        return RedirectToAction("GetGetAllPendingProcessingLeaveInfo", "User");
                    }
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Leave Application Submit Fail";
                return RedirectToAction("UpdateLeaveApprovalbyLm");

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }


        // GET: /Admin/Update Leave approval by Admin
        [HttpGet]
        public ActionResult UpdateLeaveApprovalbyAdmin(string lid)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "Admin")
            {
                LeaveClass leaveInfo = new LeaveClass();
                var lvInfo = leaveInfo.GetSpecificLeaveInfo(lid, "Admin");
                if (string.IsNullOrEmpty(lvInfo.LeaveId))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                    return RedirectToAction("GetGetAllPendingProcessingLeaveInfo");
                }
                return View(lvInfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Admin/Update Leave approval by Admin
        [HttpPost]
        public ActionResult UpdateLeaveApprovalbyAdmin(Models.LeaveClass leaveClass)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "Admin")
            {
                if (leaveClass.UpdateLeaveApprovalbyAdmin(leaveClass) == "Data Updated Successfully")
                {
                    NotificationClass notification = new NotificationClass();

                    using (var notificationA = new NotificationClass())
                    {
                        notificationA.SenderId = Session["EmpID"].ToString();
                        notificationA.ReceiverId = leaveClass.EmployeeId;
                        notificationA.SenderType = "Admin";
                        notificationA.Notificationtype = "Normal";
                        notificationA.Subject = "Leave Application";
                        notificationA.Description =
                            "Dear Employee your leave application seen admin. Now your Leave Application is Approved.";
                        notification.InsertNotification(notificationA);
                    }

                    Session["success_div"] = "true";
                    Session["success_msg"] = "Leave application submitted successfully.";
                    return RedirectToAction("GetGetAllPendingProcessingLeaveInfo", "Admin");
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Leave Application Submit Fail";
                return RedirectToAction("UpdateLeaveApprovalbyLm");

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }


        // Method : GET : Get Access User Info 
        [HttpGet]
        public ActionResult GetEmployeeInfoForClearence()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                Clearence clearenceinfo = new Clearence();
                var Info = clearenceinfo.GetEmployeeDataforExitClearence(Session["EmpID"].ToString());
                if (string.IsNullOrEmpty(Info.EmployeeId))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Employee,information not found!";
                    return RedirectToAction("Dashboard");
                }
                return View(Info);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");

        }

        [HttpPost]
        public ActionResult GetEmployeeInfoForClearence(Models.Clearence clearence)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                var msg = clearence.InsertClerence(clearence);
                if (msg == "Data Inserted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Clearence submitted successfully.";
                    return RedirectToAction("GetEmployeeInfoForClearence");
                }
                else
                {
                    Session["error_div"] = "true";
                    Session["error_msg"] = msg;
                    return RedirectToAction("GetEmployeeInfoForClearence");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        // Method : GET : Get Clearence Info 
        [HttpGet]
        public ActionResult GetClearenceInfo()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                Clearence clearenceinfo = new Clearence();
                var clerencelistforLm = clearenceinfo.GetExitClearenceinfoForLineManager(Session["EmpID"].ToString());
                var clerencelistfordh  = clearenceinfo.GetExitClearenceinfoForDeptHead(Session["EmpID"].ToString());
                var clerencelistforad  = clearenceinfo.GetExitClearenceinfoForAdmin(Session["EmpID"].ToString());
                ViewBag.ClearenceInfoDeptHead = clerencelistfordh;
                ViewBag.ClearenceInfoAdmin = clerencelistforad;
                if (clerencelistforLm.Any() || clerencelistfordh.Any() || clerencelistforad.Any())
                {
                    return View(clerencelistforLm);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Data not found";
                return RedirectToAction("Dashboard", "User");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");

        }

        // GET: /Admin/Update Clearence Info by Line Manager
        [HttpGet]
        public ActionResult UpdateClearenceInfobyLm(string slid)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "Admin")
            {
                Clearence clinfo = new Clearence();
                var clearencInfo = clinfo.GetClearenceDataforLm(Session["EmpID"].ToString(), slid);
                if (string.IsNullOrEmpty(clearencInfo.SrlNo))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                    return RedirectToAction("GetClearenceInfo");
                }
                return View(clearencInfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }


        // POST: /Admin/Update Clearence Info by Line Manager
        [HttpPost]
        public ActionResult UpdateClearenceInfobyLm(Models.Clearence clearence)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "Admin")
            {
                if (clearence.UpdateClearenceInfobyLm(clearence) == "Data Updated Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Clearence info updated successfully.";
                    return RedirectToAction("GetClearenceInfo", "Admin", new RouteValueDictionary(new { eid = clearence.SrlNo }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Employee, you must log in to update clearence info";
            return RedirectToAction("Login", "Account");
        }


        // Method : GET : Get Clearence Info 
        [HttpGet]
        public ActionResult GetSpecificClearenceInfo()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                Clearence clearenceinfo = new Clearence();
                var list = clearenceinfo.GetExitClearenceinfoForSpecificEmployee(Session["EmpID"].ToString());
                if (list.Count > 0)
                {
                    return View(list);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Data not found";
                return RedirectToAction("Dashboard", "User");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");

        }

        // GET: /Admin/Get Clearence Info for Specific User Details
        public ActionResult GetSpecificClearenceInfoDetails(string spid)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                Clearence clinfo = new Clearence();
                var clearencInfo = clinfo.GetExitClearenceinfoForSpecificEmployeeDetails(spid, Session["EmpID"].ToString());
                if (string.IsNullOrEmpty(clearencInfo.SrlNo))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                    return RedirectToAction("GetSpecificClearenceInfo");
                }
                return View(clearencInfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }


        // GET: /Admin/Update Clearence Info by Department Head
        [HttpGet]
        public ActionResult UpdateClearenceInfobyDeptHead(string depsid)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                Clearence clinfo = new Clearence();
                var clearencInfo = clinfo.GetSpecificClearenceinfoForDeptHead(Session["EmpID"].ToString(), depsid);
                if (string.IsNullOrEmpty(clearencInfo.SrlNo))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                    return RedirectToAction("GetClearenceInfo");
                }
                return View(clearencInfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Admin/Update Clearence Info by Department Head
        [HttpPost]
        public ActionResult UpdateClearenceInfobyDeptHead(Models.Clearence clearence)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                if (clearence.UpdateClearenceInfobyDeptHead(clearence) == "Data Updated Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Clearence info updated successfully.";
                    return RedirectToAction("GetClearenceInfo", "User", new RouteValueDictionary(new { eid = clearence.SrlNo }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Employee, you must log in to update clearence info";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Admin/Update Clearence Info by Admin
        [HttpGet]
        public ActionResult UpdateClearenceInfobyAdmin(string adsid)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                Clearence clinfo = new Clearence();
                var clearencInfo = clinfo.GetClearenceDataforAdmin(Session["EmpID"].ToString(), adsid);
                if (string.IsNullOrEmpty(clearencInfo.SrlNo))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                    return RedirectToAction("GetClearenceInfo");
                }
                return View(clearencInfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Admin/Update Clearence Info by Admin
        [HttpPost]
        public ActionResult UpdateClearenceInfobyAdmin(Models.Clearence clearence)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                if (clearence.UpdateClearenceInfobyAdmin(clearence) == "Data Updated Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Clearence info updated successfully.";
                    return RedirectToAction("GetClearenceInfo", "User", new RouteValueDictionary(new { eid = clearence.SrlNo }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult DailyWorkProgress()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                WorkProgress wp = new WorkProgress();
                ViewBag.ProjectNameList = new SelectList(wp.GetProjectlist(), "Value", "Text");
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue submit your work progress!";
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public ActionResult DailyWorkProgress(List<WorkProgress> list)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                WorkProgress wp = new WorkProgress();
                if (wp.InsertWorkProgress(list) == "Data Inserted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Daily work progress submitted successfully.";
                    return RedirectToAction("Dashboard");
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Daily work progress NOT submitted!";
                ViewBag.ProjectNameList = new SelectList(wp.GetProjectlist(), "Value", "Text");
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue submit your work progress!";
            return RedirectToAction("Login", "Account");
        }

        // Method : GET : To Add and/or View Appraisal Info 
        [HttpGet]
        public ActionResult GetAppraisalInfo()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                Appraisal appraisal = new Appraisal();
                var appraisalEmployeeList = appraisal.GetAppraisalDataforEmployee(Session["EmpID"].ToString());
                if (appraisalEmployeeList.Any())
                {
                    return View(appraisalEmployeeList);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Dear Employee, you have not any appraisal info";
                return RedirectToAction("Dashboard");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Employee, you must log in to view any info";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult ViewAllConfirmedEmployeelistSpecific()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                EmployeeConfirmation ec = new EmployeeConfirmation();
                var ecInfo = ec.GetAllConfirmedEmployeeInfoSpecific(Session["EmpID"].ToString());
                if (ecInfo.Any())
                {
                    return View(ecInfo);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Dear Employee, Data Not Found!";
                return RedirectToAction("Dashboard");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Employee, you must log in to view confirmation info";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult GetResigneeInfo()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                Resignation rs = new Resignation();
                var Info = rs.GetresigneEmployeeInfo(Session["EmpID"].ToString());
                if (string.IsNullOrEmpty(Info.EmployeeId))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Data Not Found";
                    return RedirectToAction("Dashboard");
                }
                return View(Info);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue submit resign application!";
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public ActionResult GetResigneeInfo(Models.Resignation resignation)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                if (resignation.InsertResignation(resignation) == "Data Inserted Successfully")
                {
                    NotificationClass notification = new NotificationClass();
                    using (var notificationA = new NotificationClass())
                    {
                        notificationA.SenderId = Session["EmpID"].ToString();
                        notificationA.ReceiverId = resignation.AdminId;
                        notificationA.SenderType = "Employee";
                        notificationA.Notificationtype = "Normal";
                        notificationA.Subject = "Resign Application";
                        notificationA.Description =
                            "Dear Admin you have got a RESIGN APPLICATION from " + resignation.Name + ". His/Her Employee ID is " + resignation.EmployeeId + ".";
                        notification.InsertNotification(notificationA);
                    }
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Resign application submitted successfully.";
                    return RedirectToAction("Dashboard");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue submit resign application!";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult GetAllResigneeInfoSpecific()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                Resignation rs = new Resignation();
                var Info = rs.GetSpecificEmployeeAllresigneInfo(Session["EmpID"].ToString());
                if (Info.Any())
                {
                    return View(Info);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Data Not Found";
                return RedirectToAction("Dashboard");

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue to view info!";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult GetResignInfoDetails(string rid)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                Resignation resignation = new Resignation();
                var resignInfo = resignation.GetSpecificResignInfoDetails(rid);
                if (string.IsNullOrEmpty(resignInfo.SrlNo))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                    return RedirectToAction("Dashboard");
                }
                return View(resignInfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue view resign info";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult GetUplemployeeInfo()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                Upl upl = new Upl();
                var uplInfo = upl.GetUplEmployeeInfo(Session["EmpID"].ToString());
                if (string.IsNullOrEmpty(uplInfo.EmployeeId))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Data Not Found";
                    return RedirectToAction("Dashboard");
                }
                return View(uplInfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue submit UPL application!";
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public ActionResult GetUplemployeeInfo(Models.Upl upl)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                if (upl.InsertUpl(upl) == "Data Inserted Successfully")
                {
                    NotificationClass notification = new NotificationClass();
                    using (var notificationA = new NotificationClass())
                    {
                        notificationA.SenderId = Session["EmpID"].ToString();
                        notificationA.ReceiverId = upl.AdminId;
                        notificationA.SenderType = "Employee";
                        notificationA.Notificationtype = "Normal";
                        notificationA.Subject = "Unpaid Leave Application";
                        notificationA.Description =
                            "Dear Admin you have got a UNPAID LEAVE APPLICATION from " + upl.Name + ". His/Her Employee ID is " + upl.EmployeeId + ".";
                        notification.InsertNotification(notificationA);
                        Email.SendEmail(upl.AdminEmail, "Unpaid Leave Application",
                               "Dear Admin you have got an UNPAID LEAVE APPLICATION from from " + upl.Name + ". His/Her Employee ID is " + upl.EmployeeId + ".");
                    }
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Unpaid leave application submitted successfully.";
                    return RedirectToAction("Dashboard");
                }
                Session["warning_div"] = "true";
                Session["warning_div"] = "Unpaid leave application not submitted.";
                return RedirectToAction("Dashboard", "Admin");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue submit unpaid leave application!";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult GetAllUplInfoSpecific()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                Upl upl = new Upl();
                var Info = upl.GetSpecificEmployeeAllUplInfo(Session["EmpID"].ToString());
                if (Info.Any())
                {
                    return View(Info);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Data Not Found";
                return RedirectToAction("Dashboard");

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue to view info!";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult GetUplInfoDetails(string rid)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                Upl upl = new Upl();
                var upllist = upl.GetSpecificUplInfoDetails(rid);
                if (string.IsNullOrEmpty(upllist.SrlNo))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Data not found!";
                    return RedirectToAction("Dashboard");
                }
                return View(upllist);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue..";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult Separtionreport()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                Upl upl = new Upl();
                var listInfo = upl.GetAllSepartionInfoSpecific(Session["EmpID"].ToString());
                if (listInfo.Any())
                {
                    return View(listInfo);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Data Not Found";
                return RedirectToAction("Dashboard");

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue..!";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult SepartionreportDetails(string rid)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                Upl upl = new Upl();
                var uplInfo = upl.GetAllSepartionInfoDetailsForAdmin(rid);
                if (string.IsNullOrEmpty(uplInfo.SrlNo))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                    return RedirectToAction("Dashboard");
                }
                return View(uplInfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue view resign info";
            return RedirectToAction("Login", "Account");
        }


        [HttpGet]
        public ActionResult DailyWorkProgressReportSpecific()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                WorkProgress wp = new WorkProgress();
                var list = wp.GetSpecificWorkReportData(Session["EmpID"].ToString());
                if (list.Any())
                {
                    return View(list);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "No Data Found!";
                return RedirectToAction("Dashboard");

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue..!";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult NotificationDetails(string nid)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                NotificationClass nc = new NotificationClass();
                var noInfo = nc.GetNotificationById(nid);
                if (noInfo == null)
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                    return RedirectToAction("Dashboard");
                }
                return View(noInfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Employee, you must log in to view any notification info";
            return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        public ActionResult ViewAllNotifications()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                NotificationClass nc = new NotificationClass();
                nc.ReceiverId = Session["EmpID"].ToString();
                var noInfo = nc.GetSpecificEmployeeNotificationAll(nc);
                if (noInfo.Any())
                {
                    return View(noInfo);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                return RedirectToAction("Dashboard");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Employee, you must log in to view any notification info";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult UnreadNotification(string nid)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                NotificationClass nc = new NotificationClass();
                var noInfo = nc.MarkNotificationAsUnread(nid);
                if (noInfo)
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Notification Marked as Unread Successfully";
                    return RedirectToAction("ViewAllNotifications");
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Notification Marked as Unread Not Successful";
                return RedirectToAction("ViewAllNotifications");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Employee, you must log in to mark notification as unread";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult ReadNotification(string nid)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                NotificationClass nc = new NotificationClass();
                var noInfo = nc.MarkNotificationAsRead(nid);
                if (noInfo)
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Notification Marked as Read Successfully";
                    return RedirectToAction("ViewAllNotifications");
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Notification Marked as Read Not Successful";
                return RedirectToAction("ViewAllNotifications");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Employee, you must log in to mark notification as read";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult DeleteNotification(string nid)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                NotificationClass nc = new NotificationClass();
                var noInfo = nc.DeleteNotificationById(nid);
                if (noInfo)
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Notification Deleted Successfully";
                    return RedirectToAction("ViewAllNotifications");
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Notification Delete Not Successful";
                return RedirectToAction("ViewAllNotifications");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Employee, you must log in to delete notification info";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult GetPersonalAttendanceInfo()
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                List<MvcHRMS.Models.Attendance> list = new List<Attendance>();
                return View(list);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue..!";
            return RedirectToAction("Login", "Account");
        }
        [HttpPost]
        public ActionResult GetPersonalAttendanceInfo(Models.Attendance att)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                var late = 0;
                List<MvcHRMS.Models.Attendance> list = att.GetSpecificEmpAttData(Session["EmpID"].ToString(), att.Stdate.ToShortDateString(), att.Endate.ToShortDateString(), late.ToString());
                // list = att.GetAllEmpAttData(att.Stdate.ToShortDateString(), att.Enddate.ToShortDateString(),late.ToString());
                if (list.Count > 0)
                {
                    return View(list);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "No Data Found";
                return RedirectToAction("GetPersonalAttendanceInfo");

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue..!";
            return RedirectToAction("Login", "Account");
        }


        [HttpGet]
        public ActionResult GetAttendanceInfoforLineManager()
        {

            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                List<MvcHRMS.Models.Attendance> list = new List<Attendance>();
                return View(list);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue..!";
            return RedirectToAction("Login", "Account");
        }


        [HttpPost]
        public ActionResult GetAttendanceInfoforLineManager(Models.Attendance att)
        {
            if (Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                var late = 30;
                List<MvcHRMS.Models.Attendance> list = att.GetLineManagerEmpAttData(Session["EmpID"].ToString(), att.Stdate.ToShortDateString(), att.Endate.ToShortDateString(), late.ToString());
                // list = att.GetAllEmpAttData(att.Stdate.ToShortDateString(), att.Enddate.ToShortDateString(),late.ToString());
                if (list.Count > 0)
                {
                    return View(list);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "No Data Found";
                return RedirectToAction("GetAttendanceInfoforLineManager");

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue..!";
            return RedirectToAction("Login", "Account");
        }

    }
}
