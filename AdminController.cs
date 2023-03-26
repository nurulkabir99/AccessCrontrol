using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using MvcHRMS.ActionFilters;
using MvcHRMS.Models;

namespace MvcHRMS.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                RedirectToAction("Dashboard", "Admin");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to view your dashboard!";
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Admin/Dashboard

        public ActionResult Dashboard()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                return View();
            }
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Admin/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Admin/AddEmployee
        [RBAC]
        [HttpGet]
        public ActionResult AddEmployee()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                //Get Country List
                Employee emp = new Employee();
                ViewBag.CountryList = new SelectList(emp.GetCountrylist(), "Value","Text");

                //Get Departmentlist
                ViewBag.DepartmentList = new SelectList(emp.GetDepartmentlist(), "Value", "Text");

                //Get Designation list
                ViewBag.DesignationList = new SelectList(emp.GetDesignationlist(), "Value", "Text");

                //Get Joining list
                ViewBag.JoiningStation = new SelectList(emp.GetStationlist(), "Value", "Text");
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add employee";
            return RedirectToAction("Login", "Account");
        }

        //
        // POST: /Admin/AddEmployee
        //[RBAC]
        [HttpPost]
        public ActionResult AddEmployee(Models.Employee emp, HttpPostedFileBase imagefile)
        {
            //Employee emp = new Employee();
            ViewBag.CountryList = new SelectList(emp.GetCountrylist(), "Value", "Text");

            //Get Departmentlist
            ViewBag.DepartmentList = new SelectList(emp.GetDepartmentlist(), "Value", "Text");

            //Get Designation list
            ViewBag.DesignationList = new SelectList(emp.GetDesignationlist(), "Value", "Text");

            //Get Joining list
            ViewBag.JoiningStation = new SelectList(emp.GetStationlist(), "Value", "Text");

            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                emp.CreatedBy = Session["EmpID"].ToString();
                string pic = "nopicture.png";
                // TODO: Add insert logic here
                if (imagefile != null && imagefile.ContentLength > 0)
                {
                    string fileExtension = Path.GetExtension(imagefile.FileName).ToLower();
                    if (fileExtension == ".jpg" || fileExtension == ".png" || fileExtension == ".gif")
                    {
                        var npic = Guid.NewGuid().ToString("N");//imagefile.FileName;
                        pic = npic + fileExtension;
                        string fileLocation = Server.MapPath("~/Assets/EmpImages/") + pic;
                        if (System.IO.File.Exists(fileLocation))
                        {
                            System.IO.File.Delete(fileLocation);
                        }
                        imagefile.SaveAs(fileLocation);
                    }
                    else
                    {
                        Session["warning_div"] = "true";
                        Session["warning_msg"] = "Please upload only Image type file!";
                        return View(emp);
                    }
                }
                emp.Photo = pic;
                if (emp.InsertEmployee(emp) == "Data Inserted Successfully")
                {
                    //([Employee_ID],[UserName],[Password],[Name],[Department],[Email_ID],[Mobile_No],[Operator],[Groups],[Country],[Status]
                    if (emp.CreateAccount)
                    {
                        AccessControl ac = new AccessControl();
                        ac.EmployeeId = emp.EmployeeId;
                        ac.Name = emp.Name;
                        ac.Department = emp.Department;
                        ac.EmailId = emp.EmailId;
                        ac.MobileNo = emp.MobileNo;
                        ac.Group = emp.Department;
                        ac.Country = emp.Country;
                        ac.Picture = emp.Photo;
                        ac.JoiningDate = emp.JoiningDate;
                        var pwd = ac.CreateRandomPassword(6);
                        var hpwd = FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, "SHA1");
                        ac.Password = hpwd;
                        ac.UserName = emp.EmailId.Substring(0, emp.EmailId.IndexOf("@", StringComparison.Ordinal));
                        ac.InsertAccessUsers(ac);
                        Email.SendEmail(emp.EmailId, "Employee Add to HRM System.",
                          "Dear Employee your information successfully added to HRM symtem. your user name is " + ac.UserName + ". and your Employee ID is " +
                          emp.EmployeeId + ". Now you access to the HRM system using your username and your password is " + pwd + ". this is an auto generated password. so please change your password for your security purpose.");
                    }
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Employee Data Inserted Successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = emp.EmployeeId}));
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Employee Information not submitted.";
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add employee";
            return RedirectToAction("Login", "Account");
        }


        

        //[HttpGet]
        //public ActionResult AddSpouseInfo()
        //{
        //    if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
        //    {
        //    }
        //    return View();
        //}


        [HttpPost]
        public ActionResult AddSpouseInfo(Models.Spouse spouse)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (spouse.InsertSpouse(spouse) == "Data Inserted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Spouse added successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = spouse.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add employee spouse info";
            return RedirectToAction("Login", "Account");
        }

        // POST: /Admin/Update Spouse Information/

        [HttpPost]
        public ActionResult UpdateSpouseInfo(Models.Spouse spouse)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (spouse.UpdateSpouseInfo(spouse) == "Data Updated Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Spouse updated successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = spouse.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to update employee spouse info";
            return RedirectToAction("Login", "Account");
        }

        // POST: /Admin/Add Child Information/

        [HttpPost]
        public ActionResult AddChildInfo(Models.Child child)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (child.InsertChild(child) == "Data Inserted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Child added successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = child.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add employee child info";
            return RedirectToAction("Login", "Account");
        }


       //  POST: /Admin/Update Child Information/

        [HttpPost]
        public ActionResult UpdateChildInfo(Models.Child child)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (child.UpdateChildInfo(child) == "Data Updated Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Child info updated successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = child.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to update employee child info";
            return RedirectToAction("Login", "Account");
        }

        // POST: /Admin/Add Academic Information/

        [HttpPost]
        public ActionResult AddAcademicInfo(Models.AcademicQualification academic)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (academic.InsertEducationalQualification(academic) == "Data Inserted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Academic qualification added successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = academic.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add employee academic qualification";
            return RedirectToAction("Login", "Account");
        }


        //  POST: /Admin/Update Academic Information/

        [HttpPost]
        public ActionResult UpdateAcademicInfo(Models.AcademicQualification academic)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (academic.UpdateAcademicInfo(academic) == "Data Updated Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Academic info updated successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = academic.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to update employee academic info";
            return RedirectToAction("Login", "Account");
        }


        // POST: /Admin/Add Training Information/

        [HttpPost]
        public ActionResult AddTrainingInfo(Models.Training training)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (training.InsertTraining(training) == "Data Inserted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Training info added successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = training.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add employee training info";
            return RedirectToAction("Login", "Account");
        }

        //  POST: /Admin/Update Training Information/

        [HttpPost]
        public ActionResult UpdateTrainingInfo(Models.Training training)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (training.UpdateTrainingInfo(training) == "Data Updated Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Training info updated successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = training.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to update employee training info";
            return RedirectToAction("Login", "Account");
        }

        // POST: /Admin/Add Experience Information/

        [HttpPost]
        public ActionResult AddExperienceInfo(Models.Experience experience)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (experience.InsertExperience(experience) == "Data Inserted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Experience info added successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = experience.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add employee experience info";
            return RedirectToAction("Login", "Account");
        }


        //  POST: /Admin/Update Experience Information/

        [HttpPost]
        public ActionResult UpdateExperienceInfo(Models.Experience experience)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (experience.UpdateExperienceInfo(experience) == "Data Updated Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Experience info updated successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = experience.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to update employee experience info";
            return RedirectToAction("Login", "Account");
        }

        // POST: /Admin/Add Skill Information/

        [HttpPost]
        public ActionResult AddSkillInfo(Models.Skill skill)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (skill.InsertSkill(skill) == "Data Inserted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Skill info added successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = skill.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add employee skill info";
            return RedirectToAction("Login", "Account");
        }

        //  POST: /Admin/Update Skill Information/

        [HttpPost]
        public ActionResult UpdateSkillInfo(Models.Skill skill)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (skill.UpdateSkillInfo(skill) == "Data Updated Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Skill info updated successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = skill.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to update employee skill info";
            return RedirectToAction("Login", "Account");
        }


        // POST: /Admin/Add Reference Information/

        [HttpPost]
        public ActionResult AddReferenceInfo(Models.Reference reference)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (reference.InsertReference(reference) == "Data Inserted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Reference added successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = reference.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add employee reference info";
            return RedirectToAction("Login", "Account");
        }

        //  POST: /Admin/Update Reference Information/

        [HttpPost]
        public ActionResult UpdateReferenceInfo(Models.Reference reference)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (reference.UpdateReferenceInfo(reference) == "Data Updated Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Reference info updated successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = reference.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to update employee reference info";
            return RedirectToAction("Login", "Account");
        }

        // POST: /Admin/Add Salary Information/
        [RBAC]
        [HttpPost]
        public ActionResult AddSalaryInfo(Models.Salary salary)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (salary.InsertSalary(salary) == "Data Inserted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Salary added successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = salary.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add employee salary info";
            return RedirectToAction("Login", "Account");
        }

        //  POST: /Admin/Update Salary Information/

        [RBAC]
        [HttpPost]
        public ActionResult UpdateSalaryInfo(Models.Salary salary)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (salary.UpdateSalaryInfo(salary) == "Data Updated Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Salary info updated successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = salary.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to update employee salary info";
            return RedirectToAction("Login", "Account");
        }

        // POST: /Admin/Add facility Information/
        [RBAC]
        [HttpPost]
        public ActionResult AddFacilityInfo(Models.Benefit benefit)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (benefit.InsertBenefit(benefit) == "Data Inserted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Facility information added successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = benefit.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add employee facility information";
            return RedirectToAction("Login", "Account");
        }


        //  POST: /Admin/Update Facility Information/
        [RBAC]
        [HttpPost]
        public ActionResult UpdatefacilityInfo(Models.Benefit benefit)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (benefit.UpdateBenefitInfo(benefit) == "Data Updated Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Benefit info updated successfully.";
                    return RedirectToAction("EmployeeInfoDetails", "Admin", new RouteValueDictionary(new { id = benefit.EmployeeId }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to update employee benefit info";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Admin/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Admin/Edit/5

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
        // GET: /Admin/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Admin/Delete/5

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

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        [RBAC]
        public ActionResult EmployeeList()
        {
            if (Session["Role"] != null)// && Session["Role"].ToString() == "Admin")
            {
                try
                {
                    //Employee emp=new Employee();
                    //var list = emp.GetAllEmpData();
                    return View();
                }
                catch(Exception ex)
                {
                    Session["error_div"] = "true";
                    Session["error_msg"] = "Error happend! Error Message is: "+ex.Message;
                    return RedirectToAction("Dashboard", "Admin");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }



        public JsonResult GetNextEmployeeIdJson(string empType)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Models.Employee empmodel = new Employee();
                var nextId = empmodel.GetNextIdforAllEmployee(empType);
                
                var retval = Json(new { nextId }, JsonRequestBehavior.AllowGet);
                return retval;
            }
            return null;
        }

        // The back-end then will determine if the employee email is available or not,
        // and finally returns a JSON { "valid": true } or { "valid": false }
        public JsonResult CheckEmpEmail(string emailId)
        {
            Employee emp = new Employee();
            var isavailble = emp.CheckEmailIsAvailable(emailId);
            var retval = Json(new { valid = isavailble.ToString().ToLower() }, JsonRequestBehavior.AllowGet);
            return retval;
        }
        public JsonResult CheckEmpEmailForNewUserCreate(string emailId)
        {
            Employee emp = new Employee();
            var isavailble = emp.CheckEmailIsAvailableForNewUserCreate(emailId);
            var retval = Json(new { valid = isavailble.ToString().ToLower() }, JsonRequestBehavior.AllowGet);
            return retval;
        }
        public JsonResult CheckEmpIdForNewUserCreate(string empId)
        {
            Employee emp = new Employee();
            var isavailble = emp.CheckEmpIdIsAvailableForNewUserCreate(empId);
            var retval = Json(new { valid = isavailble.ToString().ToLower() }, JsonRequestBehavior.AllowGet);
            return retval;
        }
        public JsonResult CheckEmpNId(string emailId)
        {
            Employee emp = new Employee();
            var isavailble = emp.CheckNIdIsAvailable(emailId);
            var retval = Json(new { valid = isavailble.ToString().ToLower() }, JsonRequestBehavior.AllowGet);
            return retval;
        }

        public JsonResult CheckEmpId(string employeeId)
        {
            Employee emp = new Employee();
            var isavailble = emp.CheckEmpIdIsAvailable(employeeId);
            var retval = Json(new { valid = isavailble.ToString().ToLower() }, JsonRequestBehavior.AllowGet);
            return retval;
        }

        public JsonResult GetEmployeeListJson(string joiningStation)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Models.Employee emp = new Employee();
                var list = emp.GetAllEmpDataByJoiningStation(joiningStation);
                var retval = Json(new { list }, JsonRequestBehavior.AllowGet);
                return retval;
            }
            return null;
        }
        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }
        public JsonResult GetEmpolyeeIForDeptJson(string departmentName)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Models.Employee empmodel = new Employee();
                var lstList = empmodel.GetEmployeeId(departmentName);
                var retval = Json(new { lstList }, JsonRequestBehavior.AllowGet);
                return retval;
            }
            return null;
        }

        public JsonResult GetApprisalInfoJson(string employeeId)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Models.Appraisal appmodel = new Appraisal();
                var lstList = appmodel.GetEmployeeDataforAppraisal(employeeId);
                var retval = Json(new { lstList }, JsonRequestBehavior.AllowGet);
                return retval;
            }
            return null;
        }

        public JsonResult GetTrainingDurationJson(string startDate, string endDate)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                var lstList = new List<string>();
                if (string.IsNullOrEmpty(startDate) || string.IsNullOrWhiteSpace(endDate))
                {
                    lstList.Add("Please select training start date first!");
                    return Json(new { lstList }, JsonRequestBehavior.AllowGet);
                }
                var r = DateDiff.GetDuration(startDate, endDate);
                if (string.IsNullOrEmpty(r))
                {
                    lstList.Add("End date must be greater than start date!");
                    return Json(new {lstList}, JsonRequestBehavior.AllowGet);
                }
                lstList.Add(r);
                return Json(new { lstList }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }


        public JsonResult GetExperienceDurationJson(string startDate, string endDate)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                var lstList = new List<string>();
                if (string.IsNullOrEmpty(startDate) || string.IsNullOrWhiteSpace(endDate))
                {
                    lstList.Add("Please select start date first!");
                    return Json(new { lstList }, JsonRequestBehavior.AllowGet);
                }
                var r = DateDiff.GetDuration(startDate, endDate);
                if (string.IsNullOrEmpty(r))
                {
                    lstList.Add("End date must be greater than start date!");
                    return Json(new { lstList }, JsonRequestBehavior.AllowGet);
                }
                lstList.Add(r);
                return Json(new { lstList }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        // GET: /Admin/Edit Employee info
        [RBAC]
        [HttpGet]
        public ActionResult EditEmployeeLineManagerAdminDeptHead(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Employee emp = new Employee();
                ViewBag.DepartmentList = new SelectList(emp.GetDepartmentlist(), "Value", "Text");
                var Info = emp.GetEmployeeLineManagerAdminDeptHead(id);
                if (string.IsNullOrEmpty(Info.EmployeeId))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Admin, we could not find the Employee info you were looking for!";
                    return RedirectToAction("Dashboard");
                }
                return View(Info);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Admin/Edit Employee info
        [RBAC]
        [HttpPost]
        public ActionResult EditEmployeeLineManagerAdminDeptHead(Models.Employee emp)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                try
                {
                    if (emp.UpdateEmployeeLineManagerDeptHeadAdmin(emp) == "Data Updated Successfully")
                    {
                        Session["success_div"] = "true";
                        Session["success_msg"] = "Employee Updated Successfully.";
                        return RedirectToAction("EmployeeInfoDetails");
                    }
                }
                catch (Exception ex)
                {
                    Session["error_div"] = "true";
                    Session["error_msg"] = "Error happend! Error Message is: " + ex.Message;
                    return RedirectToAction("Dashboard", "Admin");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }


        // GET: /Admin/Edit Employee info
        [HttpGet]
        public ActionResult EditEmployeeImage(string eid)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Employee emp = new Employee();
                var Info = emp.GetEmployeePhoto(eid);
                if (string.IsNullOrEmpty(Info.EmployeeId))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Admin, we could not find the Employee info you were looking for!";
                    return RedirectToAction("Dashboard");
                }
                return View(Info);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        // POST: /Admin/Edit Employee general Information
        [HttpPost]
        public ActionResult EditEmployeeImage(Models.Employee emp, HttpPostedFileBase imagefile)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                try
                {
                    string pic = emp.Photo;
                    // TODO: Add insert logic here
                    if (imagefile != null && imagefile.ContentLength > 0)
                    {
                        string fileExtension = Path.GetExtension(imagefile.FileName).ToLower();
                        if (fileExtension == ".jpg" || fileExtension == ".png" || fileExtension == ".gif")
                        {
                            var npic = Guid.NewGuid().ToString("N");//imagefile.FileName;
                            pic = npic + fileExtension;
                            string fileLocation = Server.MapPath("~/Assets/EmpImages/") + pic;
                            if (System.IO.File.Exists(fileLocation))
                            {
                                System.IO.File.Delete(fileLocation);
                            }
                            imagefile.SaveAs(fileLocation);
                        }
                        else
                        {
                            Session["warning_div"] = "true";
                            Session["warning_msg"] = "Please upload only Image type file!";
                        }
                    }
                    emp.Photo = pic;

                    if (emp.UpdateEmployeePhoto(emp) == "Data Updated Successfully")
                    {
                        Session["success_div"] = "true";
                        Session["success_msg"] = "Employee Photo Updated Successfully.";
                        return RedirectToAction("EmployeeInfoDetails");
                    }
                }
                catch (Exception ex)
                {
                    Session["error_div"] = "true";
                    Session["error_msg"] = "Error happend! Error Message is: " + ex.Message;
                    return RedirectToAction("Dashboard", "Admin");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Admin/Edit Employee info
        [HttpGet]
        public ActionResult EditEmployeeInfo(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                //Get Country List
                Employee emp = new Employee();
                ViewBag.CountryList = new SelectList(emp.GetCountrylist(), "Value", "Text");

                //Get Departmentlist
                ViewBag.DepartmentList = new SelectList(emp.GetDepartmentlist(), "Value", "Text");

                //Get Designation list
                ViewBag.DesignationList = new SelectList(emp.GetDesignationlist(), "Value", "Text");

                //Get Joining list
                ViewBag.JoiningStation = new SelectList(emp.GetStationlist(), "Value", "Text");

                var Info = emp.GetEmployeeData(id);
                if (string.IsNullOrEmpty(Info.EmployeeId))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Admin, we could not find the Employee info you were looking for!";
                    return RedirectToAction("EmployeeList");
                }
                return View(Info);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        
        // POST: /Admin/Edit Employee general Information
        [HttpPost]
        public ActionResult EditEmployeeInfo(Models.Employee emp)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                try
                {
                    if (emp.UpdateEmployeeInfo(emp) == "Data Updated Successfully")
                    {
                        Session["success_div"] = "true";
                        Session["success_msg"] = "Employee Updated Successfully.";
                        return RedirectToAction("EmployeeList");
                    }
                }
                catch (Exception ex)
                {
                    Session["error_div"] = "true";
                    Session["error_msg"] = "Error happend! Error Message is: " + ex.Message;
                    return RedirectToAction("Dashboard", "Admin");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

          //
        // GET: /Admin/EmployeeInfoDetails
        [RBAC]
        [HttpGet]
        public ActionResult EmployeeInfoDetails(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                try
                {
                    if (string.IsNullOrEmpty(id))
                    {
                        id = Session["EmpID"].ToString();
                    }
                    if (ControllerContext.Controller.HasPermission("admin-employeeinfodetails") ||
                        ControllerContext.Controller.IsSysAdmin())
                    {
                        if (ControllerContext.Controller.HasRole("Standard User"))
                        {
                            if (id != Session["EmpID"].ToString())
                            {
                                return RedirectToAction("Index", "Unauthorised");
                            }
                        }
                        Employee emp = new Employee();
                        var Info = emp.GetEmployeeData(id);

                        Spouse spouse = new Spouse();
                        var listSpInfo = spouse.GetSpouseData(id);
                        ViewBag.ListSpouseInfo = listSpInfo;

                        Child child = new Child();
                        var listChInfo = child.GetChildData(id);
                        ViewBag.ListChildInfo = listChInfo;

                        AcademicQualification academicqualification = new AcademicQualification();
                        var listacademicInfo = academicqualification.GetAcademicData(id);
                        ViewBag.ListAcademicInfo = listacademicInfo;

                        Training trainig = new Training();
                        var listtraininginfo = trainig.GetTrainingData(id);
                        ViewBag.ListTrainingInfo = listtraininginfo;

                        Experience experience = new Experience();
                        var listexperienceInfo = experience.GetExperienceData(id);
                        ViewBag.ListExperienceInfo = listexperienceInfo;

                        Skill skill = new Skill();
                        var listskillinfo = skill.GetSkillData(id);
                        ViewBag.ListSkillinfo = listskillinfo;

                        Reference reference = new Reference();
                        var listRefInfo = reference.GetReferenceData(id);
                        ViewBag.ListReferenceInfo = listRefInfo;

                        Salary salInfo = new Salary();
                        var listSalaryinfo = salInfo.GetSalaryData(id);
                        ViewBag.ListSalaryInfo = listSalaryinfo;


                        Benefit beinfo = new Benefit();
                        var listBenefitInfo = beinfo.GetBenefitData(id);
                        ViewBag.ListBenefitInfo = listBenefitInfo;

                        return View(Info);
                    }
                }
                catch (Exception ex)
                {
                    Session["error_div"] = "true";
                    Session["error_msg"] = "Error happend! Error Message is: " + ex.Message;
                    return RedirectToAction("Dashboard", "Admin");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        // Method : GET : To Add and/or View Appraisal Info 
        [HttpGet]
        public ActionResult GetAppraisalInfo()
        {
            if (Session["Role"] != null)// && Session["Role"].ToString() == "User")
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

        [RBAC]
        [HttpGet]
        public ActionResult AddCountryInfo()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Country con=new Country();
                ViewBag.CountryListall = new SelectList(con.GetCountrylistAll(), "Value", "Text");
                ViewBag.ListCountryInfo = con.GetAllCountryData();
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add Country";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult DeleteCountryInfo(string cid)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Country con=new Country();
                if (con.DeleteCountryById(cid) == "Data Deleted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Country Deleted Successfully.";
                    return RedirectToAction("AddCountryInfo");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to delete country info";
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public ActionResult AddCountryInfo(Models.Country country)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Country con = new Country();
                var slist = new List<string>();
                var countrylist = con.GetAllCountryName(out slist);
                if (slist.Contains(country.CountryName))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Admin, this country is already exist";
                    ViewBag.CountryListall = new SelectList(con.GetCountrylistAll(), "Value", "Text");
                    ViewBag.ListCountryInfo = countrylist;
                    return View();
                }
                if (country.InsertCountry(country) == "Data Inserted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Country added successfully.";
                    ViewBag.CountryListall = new SelectList(con.GetCountrylistAll(), "Value", "Text");
                    ViewBag.ListCountryInfo = con.GetAllCountryData();
                    return View();
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add country info";
            return RedirectToAction("Login", "Account");
        }

        [RBAC]
        [HttpGet]
        public ActionResult AddDepartmentInfo()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                //Get Country List
                Employee emp = new Employee();
                ViewBag.CountryList = new SelectList(emp.GetCountrylist(), "Value", "Text");
                Department department = new Department();
                ViewBag.ListDepartmentInfo = department.GetDepartmentData();
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to any info";
            return RedirectToAction("Login", "Account");
        }

        // POST: /Admin/Add Department Information/
        [RBAC]
        [HttpPost]
        public ActionResult AddDepartmentInfo(Models.Department department)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Department con = new Department();
                var slist = new List<string>();
                var Deptlist = con.GetAllDeptName(out slist);
                if (slist.Contains(department.DepartmentName.ToLowerInvariant()))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Admin, this department name is already exist";
                    return RedirectToAction("AddDepartmentInfo");
                }
                else
                {
                    string action = Request["action"].ToString();
                    switch (action)
                    {
                        case "Insert":
                            if (department.InsertDepartment(department) == "Data Inserted Successfully")
                            {
                                Session["success_div"] = "true";
                                Session["success_msg"] = "Department information added successfully.";
                                //Employee emp = new Employee();
                                //ViewBag.CountryList = new SelectList(emp.GetCountrylist(), "Value", "Text");
                                //ViewBag.ListDepartmentInfo = department.GetDepartmentData();
                                //return View();
                                return RedirectToAction("AddDepartmentInfo");
                            }
                            break;
                        case "Update":
                            if (department.UpdateDepartment(department) == "Data Updated Successfully")
                            {
                                Session["success_div"] = "true";
                                Session["success_msg"] = "Department information updated successfully.";
                                //Employee emp = new Employee();
                                //ViewBag.CountryList = new SelectList(emp.GetCountrylist(), "Value", "Text");
                                //ViewBag.ListDepartmentInfo = department.GetDepartmentData();
                                //return View();
                                return RedirectToAction("AddDepartmentInfo");
                            }
                            break;
                    }
                } 
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to department information";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult DeleteDepartmentInfo(string cid)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Department con = new Department();
                if (con.DeleteDepartmentById(cid) == "Data Deleted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Department Deleted Successfully.";
                    return RedirectToAction("AddDepartmentInfo");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to delete info";
            return RedirectToAction("Login", "Account");
        }


        [RBAC]
        [HttpGet]
        public ActionResult AddDesignationInfo()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Designation designation = new Designation();
                ViewBag.ListDesignationInfo = designation.GetDesingationData();
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to view any info";
            return RedirectToAction("Login", "Account");
        }

        [RBAC]
        [HttpPost]
        public ActionResult AddDesignationInfo(Models.Designation designation)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Designation con = new Designation();
                var slist = new List<string>();
                var designationlist = con.GetAllDesingationName(out slist);
                if (slist.Contains(designation.DesignationName.ToLowerInvariant()))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Admin, this designation is already exist";
                    ViewBag.ListDesignationInfo = designation.GetDesingationData();
                    return View();
                }
                else
                {
                     string action = Request["action"].ToString();
                    switch (action)
                    {
                        case "Insert":
                            if (designation.InsertDesignation(designation) == "Data Inserted Successfully")
                            {
                                Session["success_div"] = "true";
                                Session["success_msg"] = "Designation added successfully.";
                              // ViewBag.ListDesignationInfo = designation.GetDesingationData();
                                return RedirectToAction("AddDesignationInfo");
                            }
                            break;
                        case "Update":
                            if (designation.UpdateDesignation(designation) == "Data Updated Successfully")
                            {
                                Session["success_div"] = "true";
                                Session["success_msg"] = "Designation updated successfully.";
                               // ViewBag.ListDesignationInfo = designation.GetDesingationData();
                                return RedirectToAction("AddDesignationInfo");
                            }
                            break;
                    }
                }
               
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add designation info";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult DeleteDesignationInfo(string cid)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Designation con = new Designation();
                if (con.DeleteDesignationById(cid) == "Data Deleted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Designation Deleted Successfully.";
                    return RedirectToAction("AddDesignationInfo");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to delete info";
            return RedirectToAction("Login", "Account");
        }

        // Method : GET : To Add Joining Staion
        [RBAC]
        [HttpGet]
        public ActionResult AddJoiningStaion()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                JoiningStation JoiningStation = new JoiningStation();
                ViewBag.ListJoiningStationInfo = JoiningStation.GetJoiningStationData();
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to view any info";
            return RedirectToAction("Login", "Account");
        }

        [RBAC]
        [HttpPost]
        public ActionResult AddJoiningStaion(Models.JoiningStation joiningstation)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                JoiningStation station = new JoiningStation();
                var slist = new List<string>();
                var joiningstationlist = station.GetAllJoiningStationName(out slist);
                if (slist.Contains(joiningstation.StationName.ToLowerInvariant()))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Admin, this joining station is already exist";
                    ViewBag.ListJoiningStationInfo = joiningstation.GetJoiningStationData();
                    return View();
                }
                else
                {
                    string action = Request["action"].ToString();
                    switch (action)
                    {
                        case "Insert":
                            if (joiningstation.InsertJoiningStation(joiningstation) == "Data Inserted Successfully")
                            {
                                Session["success_div"] = "true";
                                Session["success_msg"] = "Joining Station added successfully.";
                                return RedirectToAction("AddJoiningStaion");
                            }
                            break;
                        case "Update":
                            if (joiningstation.UpdateJoiningStation(joiningstation) == "Data Updated Successfully")
                            {
                                Session["success_div"] = "true";
                                Session["success_msg"] = "Joining station updated successfully.";
                                // ViewBag.ListDesignationInfo = designation.GetDesingationData();
                                return RedirectToAction("AddJoiningStaion");
                            }
                            break;
                    }
                }

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add joining station info";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult DeleteJoiningStationInfo(string cid)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                JoiningStation con = new JoiningStation();
                if (con.DeleteJoiningStationById(cid) == "Data Deleted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Joining Station Deleted Successfully.";
                    return RedirectToAction("AddJoiningStaion");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to delete info";
            return RedirectToAction("Login", "Account");
        }


        // Method : GET : To Add Manage Roll 
        [RBAC]
        [HttpGet]
        public ActionResult AddManageRoll()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                ManageRoll manageRoll = new ManageRoll();
                ViewBag.ListManageRollInfo = manageRoll.GetManagerRollData();
                //Get Departmentlist
                Employee emp = new Employee();
                ViewBag.DepartmentList = new SelectList(emp.GetDepartmentlist(), "Value", "Text");
                ViewBag.EmployeeIDList = new SelectList(emp.GetEmplyeeIDlist(), "Value", "Text");
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to view any info";
            return RedirectToAction("Login", "Account");
        }


        // Method : POST : To Add Manage Roll 
        [RBAC]
        [HttpPost]
        public ActionResult AddManageRoll(Models.ManageRoll manageRoll)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                //ManageRoll manageroll= new ManageRoll();
                //var slist = new List<string>();
                //var joiningstationlist = manageroll.GetAllManagerollName(out slist);
                //if (slist.Contains(manageroll.RollType))
                //{
                //    Session["warning_div"] = "true";
                //    Session["warning_msg"] = "Dear Admin, this the employee is already exist for this roll";
                //    ViewBag.ListManageRollInfo = manageRoll.GetManagerRollData();
                //    return View();
                //}
                string action = Request["action"].ToString();
                switch (action)
                {
                    case "Insert":
                        //var resp = manageRoll.InsertManageRoll(manageRoll);
                        if (manageRoll.InsertManageRoll(manageRoll) == "Data Inserted Successfully")
                        {
                            Session["success_div"] = "true";
                            Session["success_msg"] = "Roll added successfully.";
                            //return RedirectToAction("AddManageRoll");
                        }
                        else
                        {
                            Session["warning_div"] = "true";
                            Session["warning_msg"] = "Dear Admin, this the employee is already exist for this roll";
                            //return RedirectToAction("AddManageRoll");
                        }
                        break;
                    case "Update":
                        if (manageRoll.UpdateManageroll(manageRoll) == "Data Updated Successfully")
                        {
                            Session["success_div"] = "true";
                            Session["success_msg"] = "Roll updated successfully.";
                            //return RedirectToAction("AddManageRoll");
                        }
                        else
                        {
                            Session["warning_div"] = "true";
                            Session["warning_msg"] = "Dear Admin, this the employee is already exist for this roll";
                            //return RedirectToAction("AddManageRoll");
                        }
                        break;
                }
                return RedirectToAction("AddManageRoll");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add roll info";
            return RedirectToAction("Login", "Account");
        }

        // Method : GET : To Add and/or View Appraisal Info 
        [RBAC]
        [HttpGet]
        public ActionResult AddAppraisalInfo()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Employee emp = new Employee();
                ViewBag.EmployeeIDList = new SelectList(emp.GetEmplyeeIDlist(), "Value", "Text");
                Appraisal appraisal = new Appraisal();
                var appraisalEmployeeList = appraisal.GetAppraisalData();
                return View(appraisalEmployeeList);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to view any info";
            return RedirectToAction("Login", "Account");
        }

        // Method : POST : To Add  Appraisal Info 
        [RBAC]
        [HttpPost]
        public ActionResult AddAppraisalInfo(Models.Appraisal appraisal)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (appraisal.InsertAppraislInfo(appraisal) == "Data Inserted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Employee Appraisal successfully.";
                    //ViewBag.AppraisalEmployeeList = appraisal.GetAppraisalData();
                    //Employee emp = new Employee();
                    //ViewBag.EmployeeIDList = new SelectList(emp.GetEmplyeeIDlist(), "Value", "Text");
                    return RedirectToAction("AddAppraisalInfo");
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Dear Admin, Employee Appraisal not  success.";

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to view any info";
            return RedirectToAction("Login", "Account");
        }


        // Method : GET : To Add Log Info 
        [RBAC]
        [HttpGet]
        public ActionResult AddLogInfo()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                try
                {
                    AccessControl accessControl = new AccessControl();
                    var list = accessControl.GetAlllogData();
                    return View(list);
                }
                catch (Exception ex)
                {
                    Session["error_div"] = "true";
                    Session["error_msg"] = "Error happend! Error Message is: " + ex.Message;
                    return RedirectToAction("Dashboard", "Admin");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
           
        } // Method : GET : Get Access User Info 

        [HttpPost]
        public JsonResult TeamMemberattendanceUpdate(string eid, string dt, string cs1, string cs2)
        {
            List<string> retvalSuccess = new List<string>();
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                var lt = 30.ToString();
                Attendance att = new Attendance();
                if (att.UpdateEmpAttendancebyTeamleader(eid, dt, cs1, cs2, lt) == "Data Updated Successfully")
                {
                    retvalSuccess.Add("Attendance updated successfully.");
                    retvalSuccess.Add("Success");
                    return Json(new {retvalSuccess}, JsonRequestBehavior.AllowGet);
                }
                retvalSuccess.Add("Attendance NOT updated successfully.");
                retvalSuccess.Add("Error");
                return Json(new { retvalSuccess }, JsonRequestBehavior.AllowGet);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            retvalSuccess.Add("Logout");
            return Json(new {retvalSuccess}, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult MarkAsActive(string eid, string st)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                AccessControl accessControl = new AccessControl();
                if (accessControl.UpdateEmpStatus(eid,st) == "Data Updated Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Status updated successfully.";
                    return RedirectToAction("GetAccessUserInfo");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        // Method : GET : Get Access User Info 
        [HttpGet]
        public ActionResult GetAccessUserInfo()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                try
                {
                    AccessControl accessControl = new AccessControl();
                    var list = accessControl.GetAcessUserData();
                    return View(list);
                }
                catch (Exception ex)
                {
                    Session["error_div"] = "true";
                    Session["error_msg"] = "Error happend! Error Message is: " + ex.Message;
                    return RedirectToAction("Dashboard", "Admin");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }


        [HttpGet]
        public ActionResult GetAccessUserInfoforUsertype()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                try
                {
                    AccessControl accessControl = new AccessControl();
                    var list = accessControl.GetAcessUserData();
                    return View(list);
                }
                catch (Exception ex)
                {
                    Session["error_div"] = "true";
                    Session["error_msg"] = "Error happend! Error Message is: " + ex.Message;
                    return RedirectToAction("Dashboard", "Admin");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult MarkAsAdminUser(string eid, string st)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                AccessControl accessControl = new AccessControl();
                if (accessControl.UpdateUserTypeEmpStatus(eid, st) == "Data Updated Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "User type updated successfully.";
                    return RedirectToAction("GetAccessUserInfoforUsertype");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        // Method : GET : Get Access User Info
        
        [HttpGet]
        public ActionResult GetEmployeeInfoForClearence()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Clearence clearenceinfo = new Clearence();
                var Info = clearenceinfo.GetEmployeeDataforExitClearence(Session["EmpID"].ToString());
                if (string.IsNullOrEmpty(Info.EmployeeId))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Admin, we could not find the Employee info you were looking for!";
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
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
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
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Clearence clearenceinfo = new Clearence();
                var clearenceInfoLm = clearenceinfo.GetExitClearenceinfoForLineManager(Session["EmpID"].ToString());
                var clearenceInfoDeptHead = clearenceinfo.GetExitClearenceinfoForDeptHead(Session["EmpID"].ToString());
                ViewBag.ClearenceInfoDeptHead = clearenceInfoDeptHead;
                var clearenceInfoAdmin=clearenceinfo.GetExitClearenceinfoForAdmin(Session["EmpID"].ToString());
                ViewBag.ClearenceInfoAdmin = clearenceInfoAdmin;
                if (clearenceInfoLm.Any() || clearenceInfoDeptHead.Any() || clearenceInfoAdmin.Any())
                {
                    return View(clearenceInfoLm);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Data not found";
                return RedirectToAction("Dashboard", "Admin");
            } 
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Admin/Update Clearence Info by Line Manager
        [RBAC]
        [HttpGet]
        public ActionResult UpdateClearenceInfobyLm(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Clearence clinfo=new Clearence();
                var clearencInfo = clinfo.GetClearenceDataforLm(Session["EmpID"].ToString(), id);
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
        [RBAC]
        [HttpPost]
        public ActionResult UpdateClearenceInfobyLm(Models.Clearence clearence)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
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
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Clearence clearenceinfo = new Clearence();
                var list = clearenceinfo.GetExitClearenceinfoForSpecificEmployee(Session["EmpID"].ToString());
                if (list.Count > 0)
                {
                    return View(list);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Data not found";
                return RedirectToAction("Dashboard", "Admin");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");

        }

        // GET: /Admin/Get Clearence Info for Specific User Details
        [HttpGet]
        public ActionResult GetSpecificClearenceInfoDetails(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Clearence clinfo = new Clearence();
                var clearencInfo = clinfo.GetExitClearenceinfoForSpecificEmployeeDetails(id,Session["EmpID"].ToString());
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
        [RBAC]
        [HttpGet]
        public ActionResult UpdateClearenceInfobyDeptHead(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Clearence clinfo = new Clearence();
                var clearencInfo = clinfo.GetSpecificClearenceinfoForDeptHead(Session["EmpID"].ToString(), id);
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
        [RBAC]
        [HttpPost]
        public ActionResult UpdateClearenceInfobyDeptHead(Models.Clearence clearence)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (clearence.UpdateClearenceInfobyDeptHead(clearence) == "Data Updated Successfully")
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

        // GET: /Admin/Update Clearence Info by Admin
        [RBAC]
        [HttpGet]
        public ActionResult UpdateClearenceInfobyAdmin(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Clearence clinfo = new Clearence();
                var clearencInfo = clinfo.GetClearenceDataforAdmin(Session["EmpID"].ToString(), id);
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
        [RBAC]
        [HttpPost]
        public ActionResult UpdateClearenceInfobyAdmin(Models.Clearence clearence)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (clearence.UpdateClearenceInfobyAdmin(clearence) == "Data Updated Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Clearence info updated successfully.";
                    return RedirectToAction("GetClearenceInfo", "Admin", new RouteValueDictionary(new { eid = clearence.SrlNo }));
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }


        // Method : GET : Get Clearence Info 
        [RBAC]
        [HttpGet]
        public ActionResult GetAllClearenceInfoforAdmin()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Clearence clearenceinfo = new Clearence();
                var list = clearenceinfo.GetAllPendingClearenceInfo();
                ViewBag.ClearenceAcceptedandRejectedClearenceInfo = clearenceinfo.GetAllAcceptedandRejectedClearenceInfo();
                if (list.Count > 0)
                {
                    return View(list);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Data not found";
                return RedirectToAction("Dashboard", "Admin");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");

        }

        // GET: /Admin/Get Clearence Specific User info for Admin
        [RBAC]
        [HttpGet]
        public ActionResult GetSpecificClearenceInfoDetailsforAdmin(string slid)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Clearence clinfo = new Clearence();
                var clearencInfo = clinfo.GetSpecificClearenceinfoForAdmin(slid);
                if (string.IsNullOrEmpty(clearencInfo.SrlNo))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Admin, we could not find data you were looking for!";
                    return RedirectToAction("GetAllClearenceInfoforAdmin");
                }
                return View(clearencInfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }


        // Method : GET : Get Clearence Info
        [RBAC]
        [HttpGet]
        public ActionResult GetAllContractualEmployeeInfoforAdmin()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                EmployeeConfirmation cempinfo = new EmployeeConfirmation();
                var list = cempinfo.GetAllContractualEmployeeInfo();
                if (list.Count > 0)
                {
                    return View(list);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Data not found";
                return RedirectToAction("Dashboard", "Admin");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");

        }

        // GET: /Admin/Get Specific Cotractual Employee info for Admin
        [RBAC]
        [HttpGet]
        public ActionResult GetSpecificContractualEmployeeInfoforAdmin(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Employee emp = new Employee();
                //Get Designation list
                ViewBag.DesignationList = new SelectList(emp.GetDesignationlist(), "Value", "Text");

                //Get Joining list
                ViewBag.JoiningStation = new SelectList(emp.GetStationlist(), "Value", "Text");


                EmployeeConfirmation empinfo = new EmployeeConfirmation();
                empinfo = empinfo.GetSpecificContractualEmployeeInfo(id);
                //ViewBag.Employeesalaryinfo = empinfo.GetSpecificEmployeeSalaryInfo(empsl);

                if (string.IsNullOrEmpty(empinfo.EmpSl))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Admin, we could not find employee that you were looking for!";
                    return RedirectToAction("GetAllContractualEmployeeInfoforAdmin");
                }
                return View(empinfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }


        //GetSpecificContractualEmployeeInfo

         // GET: /Admin/Get Submit Specific Cotractual Employee info for Admin
        [RBAC]
        [HttpPost]
        public ActionResult EmployeeconfirmationbyAdmin(Models.EmployeeConfirmation employeeConfirmation)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                employeeConfirmation.Confirmby = Session["EmpID"].ToString();
                if (employeeConfirmation.InsertUpdateEmployeeConfirmatoin(employeeConfirmation) == "Data Inserted Successfully")
                {
                    NotificationClass notification = new NotificationClass();
                    notification.SenderId = Session["EmpID"].ToString();
                    notification.ReceiverId = employeeConfirmation.PresentEmployeeId;
                    notification.SenderType = "Admin";
                    notification.Notificationtype = "Normal";
                    notification.Subject = "Employee Confirmation";
                    notification.Description = "Dear employee congrates. Now you are a " +
                                               employeeConfirmation.EmployeeType + " employee of this company.";

                    notification.InsertNotification(notification);
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Employee Confirmation successfully.";
                    return RedirectToAction("GetAllContractualEmployeeInfoforAdmin","Admin");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult NotificationDetails(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                NotificationClass nc = new NotificationClass();
                var noInfo = nc.GetNotificationById(id);
                if (noInfo == null)
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Admin, we could not find data you were looking for!";
                    return RedirectToAction("Dashboard");
                }
                return View(noInfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to view any notification info";
            return RedirectToAction("Login", "Account");
        }


        [HttpGet]
        public ActionResult ViewAllNotifications()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                NotificationClass nc = new NotificationClass();
                nc.ReceiverId = Session["EmpID"].ToString();
                var noInfo = nc.GetSpecificEmployeeNotificationAll(nc);
                if (noInfo.Any())
                {
                    return View(noInfo);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Dear Admin, we could not find data you were looking for!";
                return RedirectToAction("Dashboard");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to view any notification info";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult UnreadNotification(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                NotificationClass nc = new NotificationClass();
                var noInfo = nc.MarkNotificationAsUnread(id);
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
            Session["warning_msg"] = "Dear Admin, you must log in to mark notification as unread";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult ReadNotification(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                NotificationClass nc = new NotificationClass();
                var noInfo = nc.MarkNotificationAsRead(id);
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
            Session["warning_msg"] = "Dear Admin, you must log in to mark notification as read";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult DeleteNotification(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                NotificationClass nc = new NotificationClass();
                var noInfo = nc.DeleteNotificationById(id);
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
            Session["warning_msg"] = "Dear Admin, you must log in to delete notification info";
            return RedirectToAction("Login", "Account");
        }

        [RBAC]
        [HttpGet]
        public ActionResult ViewAllConfirmedEmployeelist()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                EmployeeConfirmation ec = new EmployeeConfirmation();
                var ecInfo = ec.GetAllConfirmedEmployeeInfo();
                if (ecInfo.Any())
                {
                    return View(ecInfo);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Dear Admin, Data Not Found!";
                return RedirectToAction("Dashboard");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to view confirmation info";
            return RedirectToAction("Login", "Account");
        }

        [RBAC]
        [HttpGet]
        public ActionResult LeaveApplicationForAll()
        {
            if (Session["Role"] != null)
            {
                Employee emp = new Employee();
                ViewBag.EmployeeList = new SelectList(emp.GetEmplyeeIDlist(), "Value", "Text");
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to add employee";
            return RedirectToAction("Login", "Account");
        }

        [RBAC]
        [HttpPost]
        public ActionResult LeaveApplicationForAll(Models.Employee emp)
        {
            if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
            {
                emp.CreatedBy = Session["EmpID"].ToString();
                Session["EmpIDLeave"] = emp.EmployeeId;
                if (emp.EmployeeId != null)
                {
                   return RedirectToAction("GetLeaveUserInfoAll");
                }
               
                
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Data Not Found";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public ActionResult GetLeaveUserInfoAll()
        {
            if (Session["Role"] != null)
            {
                LeaveClass lc = new LeaveClass();
                var info = lc.GetLeaveUserInfoAll(Session["EmpIDLeave"].ToString());
                ViewBag.HtmLeaveInfo = new HtmlString(lc.GetRemainingLeaveInfoAll(Session["EmpIDLeave"].ToString()));
                ViewBag.LeaveCategoryList = new SelectList(lc.GetLeaveCategorylist(), "Value", "Text");
                if (string.IsNullOrEmpty(info.EmployeeId))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Data Not Found";
                    return RedirectToAction("Dashboard");
                }
                return View(info);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue submit leave application!";
            return RedirectToAction("Login", "Account");
        }

        [RBAC]
        [HttpPost]
        public ActionResult GetLeaveUserInfoAll(Models.LeaveClass leaveClass)
        {
            if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
            {
                leaveClass.Apply_Date = DateTime.Now.ToShortDateString();
                if (leaveClass.InsertLeaveApplication(leaveClass) == "Data Inserted Successfully")
                {

                    if (leaveClass.UpdateLeaveInstanly(leaveClass) == "Data Updated Successfully")
                    {
                        return RedirectToAction("LeaveApplicationForAll", "Admin"); 
                    }

                    //Email.SendEmail(leaveClass.LmEmail, "Leave Application",
                    //    "Dear Line Manager you have got a Leave Application from " + leaveClass.Name + ". His/Her Employee ID is " +
                    //    leaveClass.EmployeeId + " and the leave type is " + leaveClass.LeaveCategory);


                    NotificationClass notification = new NotificationClass();
                    notification.SenderId = Session["EmpID"].ToString();
                    notification.ReceiverId = leaveClass.LineManagerId;
                    notification.SenderType = "Admin";
                    notification.Notificationtype = "Normal";
                    notification.Subject = "Leave Application";
                    notification.Description = "Dear Employee you have got a leave application from " +
                                               leaveClass.Name + " as a line manager . His/her employee id is " + leaveClass.EmployeeId;

                    notification.InsertNotification(notification);
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Leave application submitted successfully.";
                    return RedirectToAction("Dashboard", "Admin");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue submit leave application!";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult GetLeaveUserInfo()
        {
            if (Session["Role"] != null )
            {
                LeaveClass lc = new LeaveClass();
                var info = lc.GetLeaveUserInfo(Session["EmpID"].ToString());
                ViewBag.HtmLeaveInfo = new HtmlString(lc.GetRemainingLeaveInfo(Session["EmpID"].ToString()));
                ViewBag.LeaveCategoryList = new SelectList(lc.GetLeaveCategorylist(), "Value", "Text");
                if (string.IsNullOrEmpty(info.EmployeeId))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Data Not Found";
                    return RedirectToAction("Dashboard");
                }
                return View(info);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue submit leave application!";
            return RedirectToAction("Login", "Account");
        }

        [RBAC]
        [HttpPost]
        public ActionResult GetLeaveUserInfo(Models.LeaveClass leaveClass)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
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
                                               leaveClass.Name + " as a line manager . His/her employee id is " + leaveClass.EmployeeId;

                    notification.InsertNotification(notification);
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Leave application submitted successfully.";
                    return RedirectToAction("Dashboard", "Admin");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue submit leave application!";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult GetSpecificUserLeaveAll()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
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

        [RBAC]
        [HttpGet]
        public ActionResult GetSpecificLeaveInfo(string id)
        {
            if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
            {
                LeaveClass lc = new LeaveClass();
                var leaveinfo = lc.GetSpecificLeaveInfo(id, "User");
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
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
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
        [RBAC]
        [HttpGet]
        public ActionResult UpdateLeaveApprovalbyLm(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                LeaveClass leaveInfo = new LeaveClass();
                var lvInfo = leaveInfo.GetSpecificLeaveInfo(id, "LineManager");
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
        [RBAC]
        [HttpPost]
        public ActionResult UpdateLeaveApprovalbyLm(Models.LeaveClass leaveClass)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
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
                        Session["success_msg"] = "Leave application accepted successfully.";
                        return RedirectToAction("GetGetAllPendingProcessingLeaveInfo", "Admin");

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
                        Session["success_msg"] = "Leave application Rejected successfully.";
                        return RedirectToAction("GetGetAllPendingProcessingLeaveInfo", "Admin");
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
        [RBAC]
        [HttpGet]
        public ActionResult UpdateLeaveApprovalbyDh(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                LeaveClass leaveInfo = new LeaveClass();
                var lvInfo = leaveInfo.GetSpecificLeaveInfo(id, "DepartmentHead");
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
        [RBAC]
        [HttpPost]
        public ActionResult UpdateLeaveApprovalbyDh(Models.LeaveClass leaveClass)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
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
                        Session["success_msg"] = "Leave application Accepted successfully.";
                        return RedirectToAction("GetGetAllPendingProcessingLeaveInfo", "Admin");

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
                        return RedirectToAction("GetGetAllPendingProcessingLeaveInfo", "Admin");
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
        [RBAC]
        [HttpGet]
        public ActionResult UpdateLeaveApprovalbyAdmin(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                LeaveClass leaveInfo = new LeaveClass();
                var lvInfo = leaveInfo.GetSpecificLeaveInfo(id, "Admin");
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
        [RBAC]
        [HttpPost]
        public ActionResult UpdateLeaveApprovalbyAdmin(Models.LeaveClass leaveClass)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
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
                        Session["success_msg"] = "Leave application approved successfully.";
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

        [RBAC]
        [HttpGet]
        public ActionResult GetGetAllLeaveInforReport()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                LeaveClass lc = new LeaveClass();
                var leaveinfoAccesspted = lc.GetAllApprovedLeaveForAdminforReport(Session["EmpID"].ToString());
                var leaveinfoNotAccesspted = lc.GetAllPendingProcessingRejectedLeaveForAdminforReport(Session["EmpID"].ToString());
                ViewBag.AdminNotApprovedLeaveInfo = leaveinfoNotAccesspted;
                if (leaveinfoAccesspted.Any() || leaveinfoNotAccesspted.Any())
                {
                    return View(leaveinfoAccesspted);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Data Not Found";
                return RedirectToAction("Dashboard");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to view leave information!";
            return RedirectToAction("Login", "Account");
        }

        [RBAC]
        [HttpGet]
        public ActionResult GetSpecificLeaveInfoforAdmin(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                LeaveClass lc = new LeaveClass();
                var leaveinfo = lc.GetSpecificLeaveInfoForReportAdmin(id);
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
        public ActionResult GetResigneeInfo()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
               Resignation rs=new Resignation();
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
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
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
                            "Dear Admin you have got a RESIGN APPLICATION from "+resignation.Name+". His/Her Employee ID is "+resignation.EmployeeId+".";
                        notification.InsertNotification(notificationA);
                    }
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Resign application submitted successfully.";
                    return RedirectToAction("Dashboard", "Admin");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue submit resign application!";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult DailyWorkProgress()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                WorkProgress wp = new WorkProgress();
                ViewBag.ProjectNameList = new SelectList(wp.GetProjectlist(), "Value", "Text");
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue submit your work progress!";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult DailyWorkProgressReportSpecific()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                WorkProgress wp = new WorkProgress();
                var list = wp.GetSpecificWorkReportData(Session["EmpID"].ToString());
                if (list.Any())
                {
                    return View(list);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "No Data Found!";
                Response.Redirect("Dashboard");
                
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue..!";
            return RedirectToAction("Login", "Account");
        }
        [RBAC]
        [HttpGet]
        public ActionResult DailyWorkProgressReportAll()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                WorkProgress wp = new WorkProgress();
                var list = wp.GetAllWorkReportData();
                if (list.Any())
                {
                    return View(list);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "No Data Found!";
                Response.Redirect("Dashboard");

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue..!";
            return RedirectToAction("Login", "Account");
        }
        
        [HttpPost]
        public ActionResult DailyWorkProgress(List<WorkProgress> list)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                WorkProgress wp = new WorkProgress();
                if (wp.InsertWorkProgress(list) == "Data Inserted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Daily work progress submitted successfully.";
                    return RedirectToAction("DailyWorkProgressReportSpecific");
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

        [HttpGet]
        public ActionResult ViewAllConfirmedEmployeelistSpecific()
        {
            if (Session["Role"] != null)// && Session["Role"].ToString() == "User")
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
        public ActionResult GetAllResigneeInfoSpecific()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
               Resignation rs=new Resignation();
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
        public ActionResult GetResignInfoDetails(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Resignation resignation = new Resignation();
                var resignInfo = resignation.GetSpecificResignInfoDetails(id);
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
        //
        [RBAC]
        [HttpGet]
        public ActionResult GetAllPendingresignInfo()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Resignation rs = new Resignation();
                var listInfo = rs.GetAllPendingresigneInfo();
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

        [RBAC]
        [HttpGet]
        public ActionResult UpdateResignInfobyAdmin(string id, string adminId)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Resignation resignation = new Resignation();

                var resignInfo = resignation.GetSpecificResignInfoDetailsForAdmin(id,adminId);
                if (string.IsNullOrEmpty(resignInfo.SrlNo))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                    return RedirectToAction("GetAllPendingresignInfo", "Admin");
                }
                return View(resignInfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue...";
            return RedirectToAction("Login", "Account");
        }
        // GET: /Admin/Update Leave approval by Admin
        [RBAC]
        [HttpPost]
        public ActionResult UpdateResignInfobyAdmin(Models.Resignation resignation)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (resignation.UpdateResignbyAdmin(resignation) == "Data Updated Successfully")
                {
                    NotificationClass notification = new NotificationClass();
                    if (resignation.AdminStatus == "Accept")
                    {
                        using (var notificationA = new NotificationClass())
                        {
                            notificationA.SenderId = Session["EmpID"].ToString();
                            notificationA.ReceiverId = resignation.EmployeeId;
                            notificationA.SenderType = "Admin";
                            notificationA.Notificationtype = "Normal";
                            notificationA.Subject = "Resign Information.";
                            notificationA.Description =
                                "Dear Employee your resign application seen by admin. Now your resign application is accepted";
                            notification.InsertNotification(notificationA);
                            Email.SendEmail(resignation.EmailId, "Resign Information",
                                "Dear Employee your resign application seen by admin. Now your resign application is accepted");
                        }
                        Session["success_div"] = "true";
                        Session["success_msg"] = "resign application accepted successfully.";
                        return RedirectToAction("GetAllPendingresignInfo", "Admin");
                    }
                    else
                    {
                        using (var notificationA = new NotificationClass())
                        {
                            notificationA.SenderId = Session["EmpID"].ToString();
                            notificationA.ReceiverId = resignation.EmployeeId;
                            notificationA.SenderType = "Admin";
                            notificationA.Notificationtype = "Normal";
                            notificationA.Subject = "Resign Information.";
                            notificationA.Description =
                                "Dear Employee your resign application seen by admin. Now your resign application is rejected";
                            notification.InsertNotification(notificationA);
                            Email.SendEmail(resignation.EmailId, "Resign Information",
                                "Dear Employee your resign application seen by admin. Now your resign application is rejected");
                        }
                        Session["success_div"] = "true";
                        Session["success_msg"] = "resign application rejected.";
                        return RedirectToAction("GetAllPendingresignInfo", "Admin");
                    }
                   
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "resign application Submit Fail";
                return RedirectToAction("UpdateResignInfobyAdmin");

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        [RBAC]
        [HttpGet]
        public ActionResult GetAllResignInfoforAdmin()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Resignation resignation = new Resignation();
                var resignInfo = resignation.GetAllresigneInfoforAdmin();
                if (resignInfo.Any())
                {
                    return View(resignInfo);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                return RedirectToAction("GetAllPendingresignInfo", "Admin");
               
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue...";
            return RedirectToAction("Login", "Account");
        }

        [RBAC]
        [HttpGet]
        public ActionResult GetResignInfoDetailsforRepot(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Resignation resignation = new Resignation();
                var resignInfo = resignation.GetSpecificResignInfoDetails(id);
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
        
        //================= UPL Start=========================
        [HttpGet]
        public ActionResult GetUplemployeeInfo()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
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
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
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
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
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
        public ActionResult GetUplInfoDetails(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Upl upl = new Upl();
                var upllist = upl.GetSpecificUplInfoDetails(id);
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

        [RBAC]
        [HttpGet]
        public ActionResult GetAllPendinguplinfo()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Upl upl =new Upl();
                var listInfo = upl.GetAllPendingUplInfo(Session["EmpID"].ToString());
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

        [RBAC]
        [HttpGet]
        public ActionResult UpdateUplInfobyAdmin(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Upl upl =new Upl();
                var uplInfo = upl.GetSpecificUplInfoDetailsForAdmin(id);
                if (string.IsNullOrEmpty(uplInfo.SrlNo))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                    return RedirectToAction("GetAllPendinguplinfo", "Admin");
                }
                return View(uplInfo);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue...";
            return RedirectToAction("Login", "Account");
        }
        // GET: /Admin/Update Leave approval by Admin
        [RBAC]
        [HttpPost]
        public ActionResult UpdateUplInfobyAdmin(Models.Upl upl)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (upl.UpdateUPLbyAdmin(upl) == "Data Updated Successfully")
                {
                    NotificationClass notification = new NotificationClass();
                    if (upl.UplStatus == "Accept")
                    {
                        using (var notificationA = new NotificationClass())
                        {
                            notificationA.SenderId = Session["EmpID"].ToString();
                            notificationA.ReceiverId = upl.EmployeeId;
                            notificationA.SenderType = "Admin";
                            notificationA.Notificationtype = "Normal";
                            notificationA.Subject = "Unpaid Leave Information.";
                            notificationA.Description =
                                "Dear Employee your Unpaid Leave application seen by admin. Now your Unpaid Leave application is accepted";
                            notification.InsertNotification(notificationA);
                            Email.SendEmail(upl.EmailId, "Unpaid Leave Information",
                                "Dear Employee your unpaid leave application seen by admin. Now your resign application is accepted");
                        }
                        Session["success_div"] = "true";
                        Session["success_msg"] = "Unpaid leave application submitted successfully.";
                        return RedirectToAction("GetAllPendinguplinfo", "Admin");
                    }
                    else
                    {
                        using (var notificationA = new NotificationClass())
                        {
                            notificationA.SenderId = Session["EmpID"].ToString();
                            notificationA.ReceiverId = upl.EmployeeId;
                            notificationA.SenderType = "Admin";
                            notificationA.Notificationtype = "Normal";
                            notificationA.Subject = "Unpaid Leave Information.";
                            notificationA.Description =
                                "Dear Employee your Unpaid Leave application seen by admin. Now your Unpaid Leave application is rejected";
                            notification.InsertNotification(notificationA);
                            Email.SendEmail(upl.EmailId, "Unpaid Leave Information",
                                "Dear Employee your unpaid leave application seen by admin. Now your resign application is rejected");
                        }
                        Session["success_div"] = "true";
                        Session["success_msg"] = "Unpaid leave application submitted successfully.";
                        return RedirectToAction("GetAllPendinguplinfo", "Admin");
                    }

                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Unpaid leave application Submit Fail";
                return RedirectToAction("UpdateUplInfobyAdmin");

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue!";
            return RedirectToAction("Login", "Account");
        }

        [RBAC]
        [HttpGet]
        public ActionResult GetAllUplInfoforAdmin()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Upl upl = new Upl();
                var uplInfo = upl.GetAllUplInfo();
                if (uplInfo.Any())
                {
                    return View(uplInfo);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                return RedirectToAction("Dashboard", "Admin");

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Please Login to continue...";
            return RedirectToAction("Login", "Account");
        }

        [RBAC]
        [HttpGet]
        public ActionResult GetUplInfoDetailsforRepot(string id)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Upl upl = new Upl();
                var uplInfo = upl.GetSpecificUplInfoDetailsForAdmin(id);
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


        // Method : GET : To Add and/or View Appraisal Info 
        [RBAC]
        [HttpGet]
        public ActionResult Byforceseparation()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Employee emp = new Employee();
                ViewBag.EmployeeIDList = new SelectList(emp.GetEmplyeeIDlist(), "Value", "Text");
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to view any info";
            return RedirectToAction("Login", "Account");
        }

        public JsonResult GetEmpInfoJsonforUpl(string employeeId)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Models.Upl uplmodel = new Upl();
                var lstList = uplmodel.GetEmployeeDataforUpl(employeeId);
                var retval = Json(new { lstList }, JsonRequestBehavior.AllowGet);
                return retval;
            }
            return null;
        }

        // Method : POST : To Add  Appraisal Info 
        [RBAC]
        [HttpPost]
        public ActionResult Byforceseparation(Models.Upl upl)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (upl.InsertTerminationbyadmin(upl) == "Data Inserted Successfully")
                {
                    NotificationClass notification= new NotificationClass();
                    using (var notificationA = new NotificationClass())
                    {
                        notificationA.SenderId = Session["EmpID"].ToString();
                        notificationA.ReceiverId = upl.EmployeeId;
                        notificationA.SenderType = "Admin";
                        notificationA.Notificationtype = "High Priority";
                        notificationA.Subject = "Termination Information";
                        notificationA.Description =
                            "Dear Employee you have been terminated following this reason." + upl.Penalty + ".";
                        notification.InsertNotification(notificationA);
                        Email.SendEmail(upl.EmailId, "Termination Information",
                            "Dear Employee you have been terminated following this reason." + upl.Penalty + ".");
                    }
                    using (var notificationB = new NotificationClass())
                    {
                        notificationB.SenderId = Session["EmpID"].ToString();
                        notificationB.ReceiverId = upl.DepartmentHeadId;
                        notificationB.SenderType = "Admin";
                        notificationB.Notificationtype = "High Priority";
                        notificationB.Subject = "Termination Information";
                        notificationB.Description =
                            "Dear department head let you know that MR " + upl.Name +
                            " has been terminated. His employee Id is " + upl.EmployeeId + ". Termination affect from " +
                            upl.Affectdate + ".";
                        notification.InsertNotification(notificationB);
                        Email.SendEmail(upl.DeptHeadEmail, "Termination Information",
                            "Dear department head let you know that MR " + upl.Name +
                            " has been terminated. His employee Id is" + upl.EmployeeId + ". Termination affect from " +
                            upl.Affectdate + ".");
                    }
                    using (var notificationC = new NotificationClass())
                    {
                        notificationC.SenderId = Session["EmpID"].ToString();
                        notificationC.ReceiverId = upl.LineManagerId;
                        notificationC.SenderType = "Admin";
                        notificationC.Notificationtype = "High Priority";
                        notificationC.Subject = "Termination Information";
                        notificationC.Description =
                            "Dear Line Manager let you know that MR " + upl.Name +
                            " has been terminated. His employee Id is " + upl.EmployeeId + ". Termination affect from " +
                            upl.Affectdate + ".";
                        notification.InsertNotification(notificationC);
                        Email.SendEmail(upl.LineManagerEmail, "Termination Information",
                            "Dear Line Manager let you know that MR " + upl.Name +
                            " has been terminated. His employee Id is" + upl.EmployeeId + ". Termination affect from " +
                            upl.Affectdate + ".");
                    }
                    using (var notificationD = new NotificationClass())
                    {
                        notificationD.SenderId = Session["EmpID"].ToString();
                        notificationD.ReceiverId = upl.AdminId;
                        notificationD.SenderType = "Admin";
                        notificationD.Notificationtype = "High Priority";
                        notificationD.Subject = "Termination Information";
                        notificationD.Description =
                            "Dear Admin let you know that MR " + upl.Name +
                            " has been terminated. His employee Id is " + upl.EmployeeId + ". Termination affect from " +
                            upl.Affectdate + ".";
                        notification.InsertNotification(notificationD);
                        Email.SendEmail(upl.AdminEmail, "Termination Information",
                            "Dear Line Manager let you know that MR " + upl.Name +
                            " has been terminated. His employee Id is" + upl.EmployeeId + ". Termination affect from " +
                            upl.Affectdate + ".");
                    }
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Employee Terminated Successfully.";
                    return RedirectToAction("Dashboard");
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Dear Admin, Employee Terminated not  success.";

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to continue.";
            return RedirectToAction("Login", "Account");
        }

        // Method : GET : To Add and/or View Appraisal Info 
        [RBAC]
        [HttpGet]
        public ActionResult ByforceUnpaidleave()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Employee emp = new Employee();
                ViewBag.EmployeeIDList = new SelectList(emp.GetEmplyeeIDlist(), "Value", "Text");
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to view any info";
            return RedirectToAction("Login", "Account");
        }
        public JsonResult GetEmpInfoJsonforUplforce(string employeeId)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Models.Upl uplmodel = new Upl();
                var lstList = uplmodel.GetEmployeeDataforUplforce(employeeId);
                var retval = Json(new { lstList }, JsonRequestBehavior.AllowGet);
                return retval;
            }
            return null;
        }

        // Method : POST : To Add  Appraisal Info 
        [RBAC]
        [HttpPost]
        public ActionResult ByforceUnpaidleave(Models.Upl upl)
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                if (upl.InsertUplbyAdminforceLeave(upl) == "Data Inserted Successfully")
                {
                    NotificationClass notification = new NotificationClass();
                    using (var notificationA = new NotificationClass())
                    {
                        notificationA.SenderId = Session["EmpID"].ToString();
                        notificationA.ReceiverId = upl.EmployeeId;
                        notificationA.SenderType = "Admin";
                        notificationA.Notificationtype = "High Priority";
                        notificationA.Subject = "Unpaid Leave Information";
                        notificationA.Description =
                            "Dear Employee you have been got UNPAID LEAVE for following this reason." + upl.Penalty + ".";
                        notification.InsertNotification(notificationA);
                        Email.SendEmail(upl.EmailId, "Unpaid Leave Information",
                            "Dear Employee you have been got UNPAID LEAVE for some reasons. So please check your detail in your HRMS system.");
                    }
                    using (var notificationB = new NotificationClass())
                    {
                        notificationB.SenderId = Session["EmpID"].ToString();
                        notificationB.ReceiverId = upl.DepartmentHeadId;
                        notificationB.SenderType = "Admin";
                        notificationB.Notificationtype = "High Priority";
                        notificationB.Subject = "Unpaid Leave Information";
                        notificationB.Description =
                            "Dear department head let you know that MR " + upl.Name +
                            " will be Unpaid Leave . His employee Id is " + upl.EmployeeId + ". Unpaid Leave  affect from the date of " +
                            upl.Affectdate + ".";
                        notification.InsertNotification(notificationB);
                        Email.SendEmail(upl.DeptHeadEmail, "Unpaid Leave Information",
                            "Dear department head let you know that MR " + upl.Name +
                            " will be Unpaid Leave . His employee Id is " + upl.EmployeeId + ". Unpaid Leave  affect from the date of " +
                            upl.Affectdate + ".");
                    }
                    using (var notificationC = new NotificationClass())
                    {
                        notificationC.SenderId = Session["EmpID"].ToString();
                        notificationC.ReceiverId = upl.LineManagerId;
                        notificationC.SenderType = "Admin";
                        notificationC.Notificationtype = "High Priority";
                        notificationC.Subject = "Unpaid Leave Information";
                        notificationC.Description =
                            "Dear Line Manager let you know thatMR " + upl.Name +
                            " will be Unpaid Leave . His employee Id is " + upl.EmployeeId + ". Unpaid Leave  affect from the date of " +
                            upl.Affectdate + ".";
                        notification.InsertNotification(notificationC);
                        Email.SendEmail(upl.LineManagerEmail, "Unpaid Leave Information",
                            "Dear Line Manager let you know that MR " + upl.Name +
                            " will be Unpaid Leave . His employee Id is " + upl.EmployeeId + ". Unpaid Leave  affect from the date of " +
                            upl.Affectdate + ".");
                    }
                    using (var notificationD = new NotificationClass())
                    {
                        notificationD.SenderId = Session["EmpID"].ToString();
                        notificationD.ReceiverId = upl.AdminId;
                        notificationD.SenderType = "Admin";
                        notificationD.Notificationtype = "High Priority";
                        notificationD.Subject = "Unpaid Leave Information";
                        notificationD.Description =
                            "Dear Admin let you know that MR " + upl.Name +
                            " will be Unpaid Leave . His employee Id is " + upl.EmployeeId + ". Unpaid Leave  affect from the date of " +
                            upl.Affectdate + ".";
                        notification.InsertNotification(notificationD);
                        Email.SendEmail(upl.AdminEmail, "Unpaid Leave Information",
                            "Dear Line Manager let you know that MR " + upl.Name +
                            " will be Unpaid Leave . His employee Id is " + upl.EmployeeId + ". Unpaid Leave  affect from the date of " +
                            upl.Affectdate + ".");
                    }
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Unpaid Leave Submit Successfully.";
                    return RedirectToAction("Dashboard");
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Dear Admin, Employee Unpaid Leave not  success.";

            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to continue.";
            return RedirectToAction("Login", "Account");
        }

         [HttpGet]
        public ActionResult Separtionreport()
        {
            if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
            {
                Upl upl =new Upl();
                var listInfo = new List<Upl>();
                if (ControllerContext.Controller.IsSysAdmin())
                {
                    listInfo = upl.GetAllSepartionInfo();
                }
                else if (ControllerContext.Controller.HasRole("Standard User"))
                {
                    listInfo = upl.GetAllSepartionInfoSpecific(Session["EmpID"].ToString());
                }
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
         public ActionResult SepartionreportDetails(string id)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 Upl upl = new Upl();
                 var uplInfo = upl.GetAllSepartionInfoDetailsForAdmin(id);
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
 /*======================UPL END*/
         [HttpGet]
         public ActionResult AddEmpTrainingInfo()
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 TrainingManagement tm = new TrainingManagement();
                 var getTrainingList = tm.GetTrainingManagementData();
                 return View(getTrainingList);
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }
         public JsonResult GetTrainingInfoJson(string trType)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 Models.TrainingManagement tmngt=new TrainingManagement();
                 var lstList = tmngt.GetTrainingId(trType);
                 var retval = Json(new { lstList }, JsonRequestBehavior.AllowGet);
                 return retval;
             }
             return null;
         }
         [HttpPost]
         public ActionResult AddEmpTrainingInfo(Models.TrainingManagement tm)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 if (tm.InsertTraining(tm) == "Data Inserted Successfully")
                 {
                     Session["success_div"] = "true";
                     Session["success_msg"] = "Training Added Successfully.";
                     var getTrainingList = tm.GetTrainingManagementData();
                     return View(getTrainingList);
                 }
                 Session["warning_div"] = "true";
                 Session["warning_msg"] = "Something went wrong!";
                 return View();
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }

         [HttpGet]
         public ActionResult AddEmpTrainerInfo()
         {
             var emp = new Employee();
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 ViewBag.EmployeeIDList = new SelectList(emp.GetEmplyeeIDlist(), "Value", "Text");
                 return View();
                 //TrainingManagement tm = new TrainingManagement();
                 //var getTrainingList = tm.GetTrainingManagementData();
                 //return View(getTrainingList);
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }
         
         [RBAC]
         [HttpGet]
         public ActionResult AddEmpAttendance()
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 List<MvcHRMS.Models.Attendance> list=new List<Attendance>();
                 return View(list);
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }
         
         [RBAC]
         [HttpPost]
         public ActionResult AddEmpAttendance(Models.Attendance att)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 var late = 30;
                 List<MvcHRMS.Models.Attendance> list = att.GetAllEmpAttData(att.Stdate.ToShortDateString(), att.Endate.ToShortDateString(), late.ToString());
                // list = att.GetAllEmpAttData(att.Stdate.ToShortDateString(), att.Enddate.ToShortDateString(),late.ToString());
                 if (list.Count > 0)
                 {
                     return View(list);
                 }

                 Session["warning_div"] = "true";
                 Session["warning_msg"] = "No Data Found";
                 return RedirectToAction("AddEmpAttendance");
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }

         [RBAC]
         [HttpGet]
         public ActionResult AddandViewHoliday()
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 LeaveClass list=new LeaveClass();
                 var holidayList = list.GetAllHoliday();
                 ViewBag.GetDepartmentList = new SelectList(list.GetAlldepartmentforHolidayAssign(), "Value", "Text");
                 return View(holidayList);
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }

         [RBAC]
         [HttpPost]
         public ActionResult AddandViewHoliday(Models.LeaveClass lc)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 switch (lc.Action)
                 {
                     case "Save":
                         if (lc.InsertHoliday(lc) == "Data Inserted Successfully")
                         {
                             Session["success_div"] = "true";
                             Session["success_msg"] = "Holiday Inserted Successfully.";
                             return RedirectToAction("AddandViewHoliday", "Admin");
                         }
                         break;
                     case "Update":
                         if (lc.UpdateHoliday(lc) == "Data Updated Successfully")
                         {
                             Session["success_div"] = "true";
                             Session["success_msg"] = "Holiday Updated Successfully.";
                             return RedirectToAction("AddandViewHoliday", "Admin");
                         }
                         break;
                 }
                 Session["warning_div"] = "true";
                 Session["warning_msg"] = "Something went wrong!";
                 return View();
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }

         [RBAC]
         [HttpGet]
         public ActionResult AddandViewYearlyLeave()
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 LeaveClass list = new LeaveClass();
                 var leaveList = list.GettypewiseLeaveAmount();
                 ViewBag.GetLeaveTypeList = new SelectList(list.GetAllLeaveTypeforLeaveAssign(), "Value", "Text");
                 ViewBag.GetYearlist = new SelectList(list.GetAssignLeaveYear(), "Value", "Text");
                 return View(leaveList);
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }

         [RBAC]
         [HttpPost]
         public ActionResult AddandViewYearlyLeave(Models.LeaveClass lc)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 switch (lc.Action)
                 {
                     case "Save":
                         if (lc.InsertYearlyLeaveAssign(lc) == "Data Inserted Successfully")
                         {
                             Session["success_div"] = "true";
                             Session["success_msg"] = "Yearly Leave Assign Successfully.";
                             return RedirectToAction("AddandViewYearlyLeave", "Admin");
                         }
                         break;
                     case "Update":
                         if (lc.UpdateYearlyLeaveAssign(lc) == "Data Updated Successfully")
                         {
                             Session["success_div"] = "true";
                             Session["success_msg"] = "Yearly Leave Assign Updated Successfully.";
                             return RedirectToAction("AddandViewYearlyLeave", "Admin");
                         }
                         break;
                 }
                 Session["warning_div"] = "true";
                 Session["warning_msg"] = "Something went wrong!";
                 return View();
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }
         
         [RBAC]
         [HttpGet]
         public ActionResult AddHolidayException()
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 LeaveClass list = new LeaveClass();
                 var leaveList = list.GetExceptionHolidayList();
                 ViewBag.GetHolidayList = new SelectList(list.GetAllHolidayList(), "Value", "Text");
               //  ViewBag.GetYearlist = new SelectList(list.GetAssignLeaveYear(), "Value", "Text");
                 return View(leaveList);
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }

         [RBAC]
         [HttpPost]
         public ActionResult AddHolidayException(Models.LeaveClass lc)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 switch (lc.Action)
                 {
                     case "Save":
                         if (lc.InsertHolidayException(lc) == "Data Inserted Successfully")
                         {
                             Session["success_div"] = "true";
                             Session["success_msg"] = "Holiday Exception Assigned Successfully.";
                             return RedirectToAction("AddHolidayException", "Admin");
                         }
                         break;
                     case "Update":
                         if (lc.UpdateHolidayException(lc) == "Data Updated Successfully")
                         {
                             Session["success_div"] = "true";
                             Session["success_msg"] = "Holiday Exception Updated Successfully.";
                             return RedirectToAction("AddHolidayException", "Admin");
                         }
                         break;
                 }
                
                 Session["warning_div"] = "true";
                 Session["warning_msg"] = "Something went wrong!";
                 return View();
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }
         public JsonResult GetHolidayemplistJson(string date)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 LeaveClass leaveClass= new LeaveClass();
                 var holidayemplist = leaveClass.GetAllHolidayEmployeeList(date);
                 var retval = Json(new { holidayemplist }, JsonRequestBehavior.AllowGet);
                 return retval;
             }
             return null;
         }
         public JsonResult GetHolidayDescriptionJson(string date)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 LeaveClass leaveClass = new LeaveClass();
                 var holidaydescription = leaveClass.GetHolidayDescription(date);
                 var retval = Json(new { holidaydescription }, JsonRequestBehavior.AllowGet);
                 return retval;
             }
             return null;
         }
         
         [RBAC]
         [HttpGet]
         public ActionResult AddandViewYearlyLeavetoEmp()
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 LeaveClass list = new LeaveClass();
                // var leaveList = list.GettypewiseLeaveAmount();
                 var assignlist = list.GetAssingLeaveAmount();
                 ViewBag.GetLeaveTypeList = new SelectList(list.GetAllLeaveTypeforLeaveAssign(), "Value", "Text");
                 ViewBag.GetYearlist = new SelectList(list.GetAssignLeaveYear(), "Value", "Text");
                 return View(assignlist);
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }

         public JsonResult GetCategoryLeaveAmountJsonSpecific(string leaveType, string leaveYear, string cofirmationDuration)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 Models.LeaveClass leavemodel = new LeaveClass();
                 var lstList = leavemodel.GetSpecificCategorywiseLeaveAmount(leaveYear, leaveType, cofirmationDuration);
                 var retval = Json(new { lstList }, JsonRequestBehavior.AllowGet);
                 return retval;
             }
             return null;
         }

         public JsonResult GetCategoryLeaveAmountJson(string leaveType, string leaveYear)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 Models.LeaveClass leavemodel = new LeaveClass();
                 var lstList = leavemodel.GetCategorywiseLeaveAmount(leaveYear, leaveType);
                 var retval = Json(new { lstList }, JsonRequestBehavior.AllowGet);
                 return retval;
             }
             return null;
         }

         [RBAC]
         [HttpPost]
         public ActionResult AddandViewYearlyLeavetoEmp(Models.LeaveClass lc)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 if (lc.ApplyOn == "All")
                 {
                     if (lc.InsertLeaveAllemployee(lc) == "Data Inserted Successfully")
                     {
                         Session["success_div"] = "true";
                         Session["success_msg"] = "All Employee Leave Assigned Successfully.";
                         return RedirectToAction("AddandViewYearlyLeavetoEmp", "Admin");
                     }
                     Session["warning_div"] = "true";
                     Session["warning_msg"] = "Something went wrong!";
                     return RedirectToAction("AddandViewYearlyLeavetoEmp", "Admin");
                 }
                 if (lc.ApplyOn == "Permanent")
                 {
                     if (lc.InsertLeavePermanentemployee(lc) == "Data Inserted Successfully")
                     {
                         Session["success_div"] = "true";
                         Session["success_msg"] = "Permanent Employee Leave Assigned Successfully.";
                         return RedirectToAction("AddandViewYearlyLeavetoEmp", "Admin");
                     }
                     Session["warning_div"] = "true";
                     Session["warning_msg"] = "Something went wrong!";
                     return RedirectToAction("AddandViewYearlyLeavetoEmp", "Admin");
                 }
                 if (lc.ApplyOn == "Contractual")
                 {
                     if (lc.InsertLeaveContractualemployee(lc) == "Data Inserted Successfully")
                     {
                         Session["success_div"] = "true";
                         Session["success_msg"] = "Contrarctual Employee Leave Assigned Successfully.";
                         return RedirectToAction("AddandViewYearlyLeavetoEmp", "Admin");
                     }
                     Session["warning_div"] = "true";
                     Session["warning_msg"] = "Something went wrong!";
                     return RedirectToAction("AddandViewYearlyLeavetoEmp", "Admin");
                 }
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }

         [RBAC]
         [HttpGet]
         public ActionResult SpecificUserLeaveInfoUpdate(string leaveId)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 LeaveClass lc = new LeaveClass();
                 var leaveInfo = lc.GetSpecificLeaveUserInfo(leaveId);
                 if (string.IsNullOrEmpty(leaveInfo.LeaveId))
                 {
                     Session["warning_div"] = "true";
                     Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                     return RedirectToAction("AddandViewYearlyLeavetoEmp");
                 }
                 return View(leaveInfo);
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue!";
             return RedirectToAction("Login", "Account");
         }

         [RBAC]
         [HttpPost]
         public ActionResult SpecificUserLeaveInfoUpdate(Models.LeaveClass lc)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 if (lc.UpdateSpecificLeaveUser(lc) == "Data Updated Successfully")
                 {
                     Session["success_div"] = "true";
                     Session["success_msg"] = "Leave Assigned Updated Successfully.";
                     return RedirectToAction("AddandViewYearlyLeavetoEmp", "Admin");
                 }
                 Session["warning_div"] = "true";
                 Session["warning_msg"] = "Something went wrong!";
                 return RedirectToAction("SpecificUserLeaveInfoUpdate", "Admin");
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue!";
             return RedirectToAction("Login", "Account");
         }

         [RBAC]
         [HttpGet]
         public ActionResult SpecificemployeeLeaveAssign()
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 LeaveClass lc = new LeaveClass();
                 ViewBag.EmployeeID = new SelectList(lc.GetSpecificEmployeeListforLeaveassign(), "Value", "Text");
                 ViewBag.GetLeaveTypeList = new SelectList(lc.GetAllLeaveTypeforLeaveAssign(), "Value", "Text");
                 return View();
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue!";
             return RedirectToAction("Login", "Account");
         }

         public JsonResult GetSpecifcEmpInfoforLeaveAssignJson(string employeeId)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 Models.LeaveClass leaveempdata = new LeaveClass();
                 var lstList = leaveempdata.GetEmployeeDataforleaveAssign(employeeId);
                 var retval = Json(new { lstList }, JsonRequestBehavior.AllowGet);
                 return retval;
             }
             return null;
         }

        [RBAC]
        [HttpGet]
         public ActionResult AddAttendanceInfo()
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                Attendance  list = new Attendance();
                var attassigntimelist = list.GetAllEmpAssignTimeData();
                ViewBag.EmployeeId = new SelectList(list.GetAllEmployeelistforAttendancetimeAssign(), "Value", "Text");
                return View(attassigntimelist);
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }

         public JsonResult GetEmpInfoJsonfortimeAssign(string employeeId)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 Models.Attendance uplmodel = new Attendance();
                 var lstList = uplmodel.GetEmployeeinfoforOfficetimeAssign(employeeId);
                 var retval = Json(new { lstList }, JsonRequestBehavior.AllowGet);
                 return retval;
             }
             return null;
         }
         
         [RBAC]
         [HttpPost]
         public ActionResult AddAttendanceInfo(Models.Attendance att)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 if (att.Affectdate != null && !string.IsNullOrEmpty(att.EmployeeId))
                 {
                     var lstList = att.GetEmployeeinfoFortimeassignandvalidation(att.Affectdate.ToString(), att.EmployeeId);
                     if (lstList == 1 || lstList==0)
                     {
                         if (att.InsertEmployeeOfficeTime(att) == "Data Inserted Successfully")
                         {
                             Session["success_div"] = "true";
                             Session["success_msg"] = "Employee Office Time Assigned Successfully.";
                             return RedirectToAction("AddAttendanceInfo", "Admin");
                         }
                         Session["warning_div"] = "true";
                         Session["warning_msg"] = "Something went wrong!";
                         return RedirectToAction("AddAttendanceInfo", "Admin");
                     }
                     Session["warning_div"] = "true";
                     Session["warning_msg"] = "This Employee Office Time Already Assigned!";
                     return RedirectToAction("AddAttendanceInfo", "Admin");
                 }
                 Session["warning_div"] = "true";
                 Session["warning_msg"] = "Employee ID and Affect date Not Found!";
                 return RedirectToAction("AddAttendanceInfo", "Admin");
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }
         //public JsonResult GetEmpInfoJsonfortimeAssignValidation(string datefrom, string employeeId)
         //{
         //    if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
         //    {
         //        Models.Attendance uplmodel = new Attendance();
         //        var lstList = uplmodel.GetEmployeeinfoFortimeassignandvalidation(datefrom, employeeId);
         //        var retval = Json(new { lstList }, JsonRequestBehavior.AllowGet);
         //        return retval;
         //    }
         //    return null;
         //}
         [RBAC]
         [HttpGet]
         public ActionResult GetAttendancetimeInfoforUpdate(string lid)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 Attendance att = new Attendance();
                 var attInfo = att.GetAttenanceTImeInfoforUpdate(lid);
                 if (string.IsNullOrEmpty(attInfo.Sl))
                 {
                     Session["warning_div"] = "true";
                     Session["warning_msg"] = "Dear Employee, we could not find data you were looking for!";
                     return RedirectToAction("Dashboard");
                 }
                 return View(attInfo);
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue....";
             return RedirectToAction("Login", "Account");
         }

         [RBAC]
         [HttpPost]
         public ActionResult GetAttendancetimeInfoforUpdate(Models.Attendance att)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 if (att.UpdateEmployeeOfficeTime(att) == "Data Updated Successfully")
                 {
                     NotificationClass notification = new NotificationClass();
                     using (var notificationA = new NotificationClass())
                     {
                         notificationA.SenderId = Session["EmpID"].ToString();
                         notificationA.ReceiverId = att.EmployeeId;
                         notificationA.SenderType = "Admin";
                         notificationA.Notificationtype = "High Priority";
                         notificationA.Subject = "Office Time Updated Information";
                         notificationA.Description =
                             "Dear Employee your office time have been changed. Your office time start at " + att.Intime +
                             "  to " + att.OutTime + " from the date of " + att.Datefrom + " to " + att.Dateto + " .";
                         notification.InsertNotification(notificationA);
                         Email.SendEmail(att.EmailId, "Office Time Updated Information",
                             "Dear Employee your office time have been changed. Your office time start at " + att.Intime +
                             "  to " + att.OutTime + " from the date of " + att.Datefrom + " to " + att.Dateto + " .");
                     }
                     Session["success_div"] = "true";
                     Session["success_msg"] = "Office Time Updated Successfully.";
                     return RedirectToAction("AddAttendanceInfo", "Admin");
                 }
                 Session["warning_div"] = "true";
                 Session["warning_msg"] = "Something went wrong!";
                 return RedirectToAction("GetAttendancetimeInfoforUpdate", "Admin");
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue....";
             return RedirectToAction("Login", "Account");
         }

        
         [HttpGet]
         public ActionResult GetAttendanceInfoforLineManager()
         {

             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
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
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
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

         [HttpGet]
         public ActionResult GetAttendanceInfo()
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 List<MvcHRMS.Models.Attendance> list = new List<Attendance>();
                 return View(list);
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }

         [HttpPost]
         public ActionResult GetAttendanceInfo(Models.Attendance att)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 var late = 0;
                 List<MvcHRMS.Models.Attendance> list = att.GetSpecificEmpAttData(Session["EmpID"].ToString(), att.Stdate.ToShortDateString(), att.Endate.ToShortDateString(), late.ToString());
                 // list = att.GetAllEmpAttData(att.Stdate.ToShortDateString(), att.Enddate.ToShortDateString(),late.ToString());
                 if(list.Count>0)
                 {
                    return View(list);
                 }
                 Session["warning_div"] = "true";
                 Session["warning_msg"] = "No Data Found";
                 return RedirectToAction("GetAttendanceInfo");

             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }

         [RBAC]
         [HttpGet]
         public ActionResult AssignLeavePolicy()
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 LeaveClass list = new LeaveClass();
                // var leaveList = list.GettypewiseLeaveAmount();
                // ViewBag.GetLeaveTypeList = new SelectList(list.GetAllLeaveTypeforLeaveAssign(), "Value", "Text");
                 ViewBag.LeaveCategoryList = new SelectList(list.GetLeaveCategorylist(), "Value", "Text");
                 ViewBag.GetYearlist = new SelectList(list.GetAssignLeaveYear(), "Value", "Text");
                 return View(list);
                 //List<MvcHRMS.Models.LeaveClass> list = new List<LeaveClass>();
                 //return View(list);
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }
         
         [RBAC]
         [HttpPost]
         public ActionResult AssignLeavePolicy(Models.LeaveClass lc)
         {
             if (Session["Role"] != null )//&& Session["Role"].ToString() == "Admin")
             {
                 if (lc.InsertLeavePolicy(lc) == "Data Inserted Successfully")
                 {
                     Session["success_div"] = "true";
                     Session["success_msg"] = "Leave Policy Assign Successfully.";
                     return RedirectToAction("AssignLeavePolicy", "Admin");
                 }
                 Session["warning_div"] = "true";
                 Session["warning_msg"] = "Something went wrong!";
                 return RedirectToAction("AssignLeavePolicy", "Admin");
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }

         [HttpGet]
         public ActionResult AddLeaveCatInfo()
         {
             if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
             {
                 LeaveClass leave = new LeaveClass();
               leave.LeaveId  = leave.GetLeaveID();
                
                   if (string.IsNullOrEmpty(leave.LeaveId))
                   {
                       Session["warning_div"] = "true";
                       Session["warning_msg"] = "Leave Serial No. Not Found";
                       return RedirectToAction("AddLeaveCatInfo");
                   }
               return View(leave);
                
             }
             return View();
         }


         [HttpPost]
         public ActionResult AddLeaveCatInfo(Models.LeaveClass leave)
         {
             if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
             {
                 if (leave.InsertLeaveType(leave) == "Data Inserted Successfully")
                 {
                     Session["success_div"] = "true";
                     Session["success_msg"] = "Spouse added successfully.";
                     return RedirectToAction("AddLeaveCatInfo", "Admin");
                 }
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Dear Admin, you must log in to add employee spouse info";
             return RedirectToAction("Login", "Account");
         }
         [RBAC]
         [HttpGet]
         public ActionResult EmpRecuitmentInfo()
         {
             if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
             {
                 //Get Country List
                 var slist = new List<string>();
                 Department con = new Department();
                 Employee emp = new Employee();
                 ViewBag.DeptList = new SelectList(emp.GetDepartmentlist(), "Value", "Text");
               
                 //Department department = new Department();
                 ViewBag.ListDepartmentInfo = emp.GetRecuitmentData();
                 ViewBag.ListRecInfo = emp.GetRecuitmentDataPass();
                 ViewBag.ListRecInfoSucess = emp.GetRecuitmentDataSuccess();
                 return View();
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Dear Admin, you must log in to any info";
             return RedirectToAction("Login", "Account");
         }

         // POST: /Admin/Add Department Information/
         [RBAC]
         [HttpPost]
         public ActionResult EmpRecuitmentInfo(Models.Employee dept)
         {
             if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
             {
                 Employee con = new Employee();
                 var slist = new List<string>();
                 var Deptlist = "";//con.GetAllDeptName(out slist);
                 //if (slist.Contains(dept.Department.ToLowerInvariant()))
                 //{
                 //    Session["warning_div"] = "true";
                 //    Session["warning_msg"] = "Dear Admin, this department name is already exist";
                 //    return RedirectToAction("EmpRecuitmentInfo");
                 //}
                 //else
                 //{
                     string action = Request["action"].ToString();
                     switch (action)
                     {
                         case "Insert":
                             if (dept.InsertApplicant(dept) == "Data Inserted Successfully")
                             {
                                 Session["success_div"] = "true";
                                 Session["success_msg"] = "Department information added successfully.";
                                 //Employee emp = new Employee();
                                 //ViewBag.CountryList = new SelectList(emp.GetCountrylist(), "Value", "Text");
                                 //ViewBag.ListDepartmentInfo = department.GetDepartmentData();
                                 //return View();
                                 return RedirectToAction("EmpRecuitmentInfo");
                             }
                             break;
                         case "Update":
                             if (dept.UpdateDepartment(dept) == "Data Updated Successfully")
                             {
                                 Session["success_div"] = "true";
                                 Session["success_msg"] = "Department information updated successfully.";
                                 //Employee emp = new Employee();
                                 //ViewBag.CountryList = new SelectList(emp.GetCountrylist(), "Value", "Text");
                                 //ViewBag.ListDepartmentInfo = department.GetDepartmentData();
                                 //return View();
                                 return RedirectToAction("EmpRecuitmentInfo");
                             }
                             break;
                         case "Updated":
                             if (dept.UpdateDept(dept) == "Data Updated Successfully")
                             {
                                 Session["success_div"] = "true";
                                 Session["success_msg"] = "Department information updated successfully.";
                                 //Employee emp = new Employee();
                                 //ViewBag.CountryList = new SelectList(emp.GetCountrylist(), "Value", "Text");
                                 //ViewBag.ListDepartmentInfo = department.GetDepartmentData();
                                 //return View();
                                 return RedirectToAction("EmpRecuitmentInfo");
                             }
                             break;
                     }
                 //}
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Dear Admin, you must log in to department information";
             return RedirectToAction("Login", "Account");
         }

         [RBAC]
         [HttpGet]
         public ActionResult EmpRecuitmentInfoPass()
         {
             if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
             {
                 //Get Country List
                 var slist = new List<string>();
                 Department con = new Department();
                 Employee emp = new Employee();
                 ViewBag.DeptList = new SelectList(emp.GetDepartmentlist(), "Value", "Text");

                 //Department department = new Department();
                 ViewBag.ListDepartmentInfo = emp.GetRecuitmentDataPass();
                 return RedirectToAction("EmpRecuitmentInfo", "Admin");
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Dear Admin, you must log in to any info";
             return RedirectToAction("Login", "Account");
         }

         [HttpGet]
         public ActionResult DeleteApplicantInfo(string cid)
         {
             if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
             {
                 Employee con = new Employee();
                 if (con.DeleteApplicantById(cid) == "Data Deleted Successfully")
                 {
                     Session["success_div"] = "true";
                     Session["success_msg"] = "Department Deleted Successfully.";
                     return RedirectToAction("EmpRecuitmentInfo");
                 }
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Dear Admin, you must log in to delete info";
             return RedirectToAction("Login", "Account");
         }
         [RBAC]
         [HttpGet]
         public ActionResult GetattendenceForAll()
         {
             if (Session["Role"] != null)
             {
                 Employee emp = new Employee();
                 ViewBag.EmployeeList = new SelectList(emp.GetEmplyeeIDlist(), "Value", "Text");
                 return View();
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Dear Admin, you must log in to add employee";
             return RedirectToAction("Login", "Account");
         }

         [RBAC]
         [HttpPost]
         public ActionResult GetattendenceForAll(Models.Employee emp)
         {
             if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
             {
                 emp.CreatedBy = Session["EmpID"].ToString();
                 Session["EmpIDLeave"] = emp.EmployeeId;
                 if (emp.EmployeeId != null)
                 {
                     return RedirectToAction("GetAttendanceInfoSp");
                 }


             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Data Not Found";
             return RedirectToAction("Dashboard");
         }
         [HttpGet]
         public ActionResult GetAttendanceInfoSp()
         {
             if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
             {
                 List<MvcHRMS.Models.Attendance> list = new List<Attendance>();
                 return View(list);
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }

         [HttpPost]
         public ActionResult GetAttendanceInfoSp(Models.Attendance att)
         {
             if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
             {
                 var late = 0;
                 List<MvcHRMS.Models.Attendance> list = att.GetSpecificEmpAttData(Session["EmpIDLeave"].ToString(), att.Stdate.ToShortDateString(), att.Endate.ToShortDateString(), late.ToString());
                 // list = att.GetAllEmpAttData(att.Stdate.ToShortDateString(), att.Enddate.ToShortDateString(),late.ToString());
                 if (list.Count > 0)
                 {
                     return View(list);
                 }
                 Session["warning_div"] = "true";
                 Session["warning_msg"] = "No Data Found";
                 return RedirectToAction("GetAttendanceInfo");

             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }

         [HttpGet]
         public ActionResult EditAttendance()
         {
             if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
             {
                 Employee empInfo = new Employee();
                 ViewBag.ListOfCode = new SelectList(empInfo.GetEmplyeeIDlist(), "Value", "Text");

                 

                 return View();
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Dear Admin, you must log in to continue";
             return RedirectToAction("Login", "Account");
         }

         //
         // POST: /Store/ItemRequisition
         //[RBAC]
         [HttpPost]
         public ActionResult EditAttendance(Models.Employee empInfo)
         {
          
           if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
             {

                 ViewBag.ListOfCode = new SelectList(empInfo.GetEmplyeeIDlist(), "Value", "Text");
                 empInfo.CreatedBy = Session["EmpID"].ToString();
                 empInfo.Createddate = DateTime.Now;
                 if (empInfo.UpdateAttendanceInfo(empInfo) == "Data Updated Successfully")
                 {
                     Session["success_div"] = "true";
                     Session["success_msg"] = "Successfully Complete.";
                     return RedirectToAction("EditAttendance");
                 }
                 Session["warning_div"] = "true";
                 Session["warning_msg"] = "Information not submitted.";
                 return View();
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Dear Admin, you must log in to continue";
             return RedirectToAction("Login", "Account");
         }
         public JsonResult GetEmpinfoattendaance(string subCat, string FrDate)
         {
             if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
             {
                 Models.Employee strModel = new Employee();
                 var lstList = strModel.GetattendanceEmpInfo(subCat, FrDate);
                 var retval = Json(new { lstList }, JsonRequestBehavior.AllowGet);
                 return retval;
             }
             return null;
         }

         public JsonResult GetEmpinfoattendaanceAddition(string subCat)
         {
             if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
             {
                 Models.Employee strModel = new Employee();
                 var lstList = strModel.GetattendanceEmpInfo(subCat);
                 var retval = Json(new { lstList }, JsonRequestBehavior.AllowGet);
                 return retval;
             }
             return null;
         }
         [HttpGet]
         public ActionResult GetAttendanceInfoCategory()
         {
             if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
             {
                List<MvcHRMS.Models.Attendance> list = new List<Attendance>();
                 return View(list);
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }

         [HttpPost]
         public ActionResult GetAttendanceInfoCategory(Models.Attendance att)
         {
             if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
             {
                 var late = 0;
                 List<MvcHRMS.Models.Attendance> list = att.GetSpecificEmpAttDataCategory(att.JoiningStations, att.Stdate.ToShortDateString(), att.Endate.ToShortDateString(), late.ToString());
                 // list = att.GetAllEmpAttData(att.Stdate.ToShortDateString(), att.Enddate.ToShortDateString(),late.ToString());
                 if (list.Count > 0)
                 {
                     return View(list);
                 }
                 Session["warning_div"] = "true";
                 Session["warning_msg"] = "No Data Found";
                 return RedirectToAction("GetAttendanceInfo");

             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Please Login to continue..!";
             return RedirectToAction("Login", "Account");
         }
    }
}
