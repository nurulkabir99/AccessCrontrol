using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using RBACModel;
using MvcHRMS.ActionFilters;
using MvcHRMS.Models;

namespace MvcHRMS.Controllers
{
    [RBAC]
    public class AccessController : Controller
    {
        private RBAC_Model database = new RBAC_Model();

        public ActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        public ActionResult Dashboard()
        {
            if (Session["Role"] != null)
            {
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }


        #region COMPANY

        public ActionResult AddViewCompanyInfo()
        {
            if (Session["Role"] != null)
            {
                ViewBag.ListCompanies = CompanyInfo.Instance.GetAllCompanies();
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");

            //return View(database.USERS.Where(r => r.Status == true && r.ID != 0).OrderBy(r => r.ID).ToList());
        }

        [HttpPost]
        public ActionResult AddViewCompanyInfo(CompanyInfo company)
        {
            if (Session["Role"] != null)
            {
                if (company.CompanyName == null)
                {
                    ModelState.AddModelError("Company Name", "Company Name must be entered");
                }
                if (company.CompanyAddress == null)
                {
                    ModelState.AddModelError("Company Address", "Company Address must be entered");
                }

                if (ModelState.IsValid)
                {
                    string action = Request["action"].ToString();
                    switch (action)
                    {
                        case "Insert":
                            company.EntryBy = Session["EmpID"].ToString();
                            var str = company.AddCompanyInfo(company);

                            if (str == "Data Inserted Successfully")
                            {
                                Session["success_div"] = "true";
                                Session["success_msg"] = "New Company added successfully.";
                                return RedirectToAction("AddViewCompanyInfo");
                            }
                            Session["warning_div"] = "true";
                            Session["warning_msg"] = "Company Info Not Saved";
                            return RedirectToAction("AddViewCompanyInfo");
                        case "Update":
                            var str2 = company.UpdateCompanyInfo(company);

                            if (str2 == "Data Updated Successfully")
                            {
                                Session["success_div"] = "true";
                                Session["success_msg"] = "Company Updated Successfully.";
                                return RedirectToAction("AddViewCompanyInfo");
                            }
                            Session["warning_div"] = "true";
                            Session["warning_msg"] = "Company Info Not Updated";
                            return RedirectToAction("AddViewCompanyInfo");
                    }
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Company Info Not Saved";
                return RedirectToAction("AddViewCompanyInfo");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        public ActionResult CompanyDetails(int id)
        {
            if (Session["Role"] != null)
            {
                CompanyInfo com = CompanyInfo.Instance.GetCompanyById(id);
                if (com.CompanySL > 0)
                {
                    return View(com);
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Dear User, we could not find the Company Info you were looking for!";
                return RedirectToAction("AddViewCompanyInfo");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        public ActionResult DeleteCompany(int id)
        {
            if (Session["Role"] != null)
            {
                if (CompanyInfo.Instance.DeleteCompanyById(id) == "Data Deleted Successfully")
                {
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Company Deleted Successfully.";
                    return RedirectToAction("AddViewCompanyInfo");
                }
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Dear User, we could not find the Company Info you were looking for!";
                return RedirectToAction("AddViewCompanyInfo");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        #endregion

        #region FEATURES

        public ActionResult ViewAllFeatures()
        {
            if (Session["Role"] != null)
            {
                return View(database.FEATURES.ToList());
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");

            //return View(database.USERS.Where(r => r.Status == true && r.ID != 0).OrderBy(r => r.ID).ToList());
        }

        public ActionResult AddNewFeature()
        {
            if (Session["Role"] != null)
            {
                //ViewBag.List_boolNullYesNo = this.List_boolNullYesNo();
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        [HttpPost]
        public ActionResult AddNewFeature(FEATURE _feature)
        {
            if (Session["Role"] != null)
            {
                if (_feature.FeatureName == null)
                {
                    ModelState.AddModelError("Feature Name", "Feature Name must be entered");
                }
                if (_feature.ModuleName == null)
                {
                    ModelState.AddModelError("Model Name", "Model Name must be entered");
                }

                //USER user = database.USERS.Where(r => r.Email == User.Identity.Name).FirstOrDefault();
                if (ModelState.IsValid)
                {
                    database.FEATURES.Add(_feature);
                    database.SaveChanges();
                    Session["success_div"] = "true";
                    Session["success_msg"] = "New Feature added successfully.";
                    return RedirectToAction("ViewAllFeatures");
                }
                //ViewBag.List_boolNullYesNo = this.List_boolNullYesNo();
                return View(_feature);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        public ActionResult FeatureDetails(int id)
        {
            if (Session["Role"] != null)
            {
                FEATURE feat = database.FEATURES.Where(r => r.Feature_Id == id)
                       .Include(a => a.PERMISSIONS)
                       .FirstOrDefault();

                // Rights combo
                ViewBag.PermissionId = new SelectList(database.PERMISSIONS.Where(u => u.Status == true).OrderBy(a => a.PermissionName), "Permission_Id", "PermissionName");
                //ViewBag.List_boolNullYesNo = this.List_boolNullYesNo();

                return View(feat);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        public ActionResult EditFeature(int id)
        {
            if (Session["Role"] != null)
            {
                FEATURE feat = database.FEATURES.Where(r => r.Feature_Id == id)
                       .Include(a => a.PERMISSIONS)
                       .FirstOrDefault();

                SelectListItem itm;
                List<SelectListItem> listItms = new List<SelectListItem>();
                itm = new SelectListItem { Text = "", Value = "" };
                listItms.Add(itm);
                if (feat != null)
                {
                    foreach (var perm in database.PERMISSIONS.Where(p => p.IsPage == true).ToList())
                    {
                        if (!feat.PERMISSIONS.Contains(perm))
                        {
                            itm = new SelectListItem { Text = "Name: " + perm.LinkText + "; Link: " + perm.PermissionName, Value = perm.Permission_Id.ToString() };
                            listItms.Add(itm);
                        }
                    }
                }

                ViewBag.RoleId = id;

                // Rights combo
                ViewBag.PermissionId = new SelectList(listItms, "Value", "Text"); //new SelectList(database.PERMISSIONS.Where(u => u.Status == true).OrderBy(a => a.PermissionName), "Permission_Id", "PermissionName");
                //ViewBag.List_boolNullYesNo = this.List_boolNullYesNo();

                return View(feat);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        [HttpPost]
        public ActionResult EditFeature(FEATURE _feature)
        {
            if (Session["Role"] != null)
            {
                if (string.IsNullOrEmpty(_feature.FeatureName))
                {
                    ModelState.AddModelError("Feature Name", "Feature Name must be entered");
                }

                //EntityState state = database.Entry(_role).State;
                //USER user = database.USERS.Where(r => r.Email == User.Identity.Name).FirstOrDefault();
                if (ModelState.IsValid)
                {
                    database.Entry(_feature).State = EntityState.Modified;
                    database.SaveChanges();
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Role updated successfully.";
                    return RedirectToAction("FeatureDetails", new RouteValueDictionary(new { id = _feature.Feature_Id }));
                }
                SelectListItem itm;
                List<SelectListItem> listItms = new List<SelectListItem>();
                itm = new SelectListItem { Text = "", Value = "" };
                listItms.Add(itm);
                foreach (var perm in database.PERMISSIONS.Where(p => p.IsPage == true).ToList())
                {
                    if (!_feature.PERMISSIONS.Contains(perm))
                    {
                        itm = new SelectListItem { Text = "Name: " + perm.LinkText + "; Link: " + perm.PermissionName, Value = perm.Permission_Id.ToString() };
                        listItms.Add(itm);
                    }
                }
                // Rights combo
                ViewBag.PermissionId = new SelectList(listItms, "Value", "Text");//new SelectList(database.PERMISSIONS.Where(u => u.Status == true).OrderBy(a => a.PermissionName), "Permission_Id", "PermissionName");
                //ViewBag.List_boolNullYesNo = this.List_boolNullYesNo();
                return View(_feature);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        public ActionResult DeletePermissionFromFeature(int featId, int permissionId)
        {
            if (Session["Role"] != null)
            {
                FEATURE _feature = database.FEATURES.Find(featId);
                PERMISSION _permission = database.PERMISSIONS.Find(permissionId);
                if (_feature.PERMISSIONS.Contains(_permission))
                {
                    _feature.PERMISSIONS.Remove(_permission);
                    database.SaveChanges();
                    Session["success_div"] = "true";
                    Session["success_msg"] = "You are successfully remove Permission from this Feature";
                    return RedirectToAction("FeatureDetails", "Access", new { id = featId });
                }
                return RedirectToAction("ViewAllFeatures", "Access");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        [HttpPost]
        public ActionResult AddPermissionToFeature(int featId, int permissionId)
        {
            if (Session["Role"] != null)
            {
                FEATURE feat = database.FEATURES.Find(featId);
                PERMISSION _permission = database.PERMISSIONS.Find(permissionId);

                if (!feat.PERMISSIONS.Contains(_permission))
                {
                    feat.PERMISSIONS.Add(_permission);
                    database.SaveChanges();
                    database.SaveChanges();
                    Session["success_div"] = "true";
                    Session["success_msg"] = "You are successfully add Permission to this Feature";
                    return RedirectToAction("EditFeature", "Access", new { id = featId });
                }
                return RedirectToAction("EditFeature", "Access", new { id = featId });
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }


        public ActionResult DeleteFeature(int id)
        {
            return View();
        }

        #endregion

        #region USERS
        // GET: Access
        
        public ActionResult ViewAllAccessUsers()
        {
            if (Session["Role"] != null)
            {
                return View(database.USERS.Where(r => r.SrlNo != 0).ToList());
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");

            //return View(database.USERS.Where(r => r.Status == true && r.ID != 0).OrderBy(r => r.ID).ToList());
        }

        public ActionResult AddNewUser()
        {
            if (Session["Role"] != null)
            {
                //Employee emp = new Employee();
                ViewBag.EmployeeIdList = new SelectList(Employee.Instance.GetEmplyeeIdListForUserCreate(false), "Value", "Text");

                ViewBag.CountryList = new SelectList(Employee.Instance.GetCountrylist(), "Value", "Text");

                //Get Departmentlist
                ViewBag.DepartmentList = new SelectList(Employee.Instance.GetDepartmentlist(), "Value", "Text");
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        [HttpPost]
        public ActionResult AddNewUser(Models.AccessControl ac, HttpPostedFileBase imagefile)
        {
            if (Session["Role"] != null)
            {
                ac.CreatedBy = Session["EmpID"].ToString();
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
                        //Employee emp = new Employee();
                        ViewBag.CountryList = new SelectList(Employee.Instance.GetCountrylist(), "Value", "Text");
                        ViewBag.EmployeeIdList = new SelectList(Employee.Instance.GetEmplyeeIdListForUserCreate(false), "Value", "Text");
                        //Get Departmentlist
                        ViewBag.DepartmentList = new SelectList(Employee.Instance.GetDepartmentlist(), "Value", "Text");
                        Session["warning_div"] = "true";
                        Session["warning_msg"] = "Please upload only Image type file!";
                        return View(ac);
                    }
                }
                ac.Picture = pic;
                //([Employee_ID],[UserName],[Password],[Name],[Department],[Email_ID],[Mobile_No],[Operator],[Groups],[Country],[Status]

                //([Employee_ID],[UserName],[Password],[Name],[Department],[Email_ID],[Mobile_No],[Operator],[Groups],[Country],[Status]
                if (ac.EmployeeId == "New User")
                {
                    ac.EmployeeId = ac.GetNextUserId();
                }
                
                //ac.Name = emp.Name;
                //ac.Department = emp.Department;
                //ac.EmailId = emp.EmailId;
                //ac.MobileNo = emp.MobileNo;
                //ac.Group = emp.Department;
                //ac.Country = emp.Country;
                //ac.Picture = emp.Photo;
                ac.JoiningDate = DateTime.Today.ToShortDateString();
                var pwd = "123456";//ac.CreateRandomPassword(6);
                var hpwd = FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, "SHA1");
                ac.Password = hpwd;
                ac.UserName = ac.EmailId.Substring(0, ac.EmailId.IndexOf("@", StringComparison.Ordinal));
                var acid = "";
                if (ac.InsertAccessUsers(ac, out acid) == "Data Inserted Successfully")
                {
                    //Email.SendEmail(ac.EmailId, "Employee Add to HRM System.",
                    //  "Dear Employee your information successfully added to HRM symtem. your user name is " + ac.UserName + ". and your Employee ID is " +
                    //  ac.EmployeeId + ". Now you access to the HRM system using your username and your password is " + pwd + ". this is an auto generated password. so please change your password for your security purpose.");

                    Session["success_div"] = "true";
                    Session["success_msg"] = "User Created Successfully.";
                    return RedirectToAction("AccessDetails", "Access", new RouteValueDictionary(new { id = acid }));
                }
                //Employee emp = new Employee();
                ViewBag.CountryList = new SelectList(Employee.Instance.GetCountrylist(), "Value", "Text");
                ViewBag.EmployeeIdList = new SelectList(Employee.Instance.GetEmplyeeIdListForUserCreate(false), "Value", "Text");
                //Get Departmentlist
                ViewBag.DepartmentList = new SelectList(Employee.Instance.GetDepartmentlist(), "Value", "Text");
                Session["warning_div"] = "true";
                Session["warning_msg"] = "Error Happned, New User Not Created.";
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        //
        // GET: /Access/BlockAccess

        public
        ActionResult BlockAccess(string id)
        {
            /**/
            if (Session["Role"] != null)// && Session["Role"].ToString() == "Admin")
            {
                try
                {
                    var query = "update AccessUsers set [Status]=0 where [SrlNo]=" + id;
                    //TODO: Add Block Access logic here
                    var str = SqlDataAccess.SQL_ExecuteCommand(query);
                    if (str == "Command Executed Successfully")
                    {
                        Session["success_div"] = "true";
                        Session["success_msg"] = "User Blocked Successfully.";
                        return RedirectToAction("ViewAllAccessUsers");
                    }
                    Session["error_div"] = "true";
                    Session["error_msg"] = "User Blocking failed. Error Occurred";//Message Is: " + str;
                    return RedirectToAction("ViewAllAccessUsers");
                }
                catch (Exception ex)
                {
                    Session["error_div"] = "true";
                    Session["error_msg"] = "User Blocking failed. Error Occurred";//Message Is: " + ex.Message;
                    return RedirectToAction("ViewAllAccessUsers");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to block a user!";
            return RedirectToAction("Index", "Account");

        }

        //
        // GET: /Access/UnblockAccess

        public ActionResult UnblockAccess(string id)
        {
            /**/
            if (Session["Role"] != null)// && Session["Role"].ToString() == "Admin")
            {
                try
                {
                    var query = "update AccessUsers set [Status]=1 where [SrlNo]=" + id;
                    //TODO: Add Unblock Access logic here
                    var str = SqlDataAccess.SQL_ExecuteCommand(query);
                    if (str == "Command Executed Successfully")
                    {
                        Session["success_div"] = "true";
                        Session["success_msg"] = "User Unblocked Successfully.";
                        return RedirectToAction("ViewAllAccessUsers");
                    }
                    Session["error_div"] = "true";
                    Session["error_msg"] = "User Unblocking failed. Error Occurred";// Message Is: " + str;
                    return RedirectToAction("ViewAllAccessUsers");
                }
                catch (Exception ex)
                {
                    Session["error_div"] = "true";
                    Session["error_msg"] = "User Unblocking failed. Error Occurred";//Message Is: " + ex.Message;
                    return RedirectToAction("ViewAllAccessUsers");
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear Admin, you must log in to unblock a user!";
            return RedirectToAction("Index", "Access");
        }

        public ActionResult AccessDetails(int id)
        {
            if (Session["Role"] != null)
            {
                USER user;
                //EmployeeInfo emp;
                if (id == 0)
                {
                    if (string.Equals(Session["AccessId"].ToString(), id.ToString()))
                    {
                        user = database.USERS.Find(id);
                        //emp = new EmployeeInfo { EmployeeInfoId = -1 };
                        //ViewBag.EmpInfo = emp;//.GetEmployeeInfoById(id.ToString());
                        SetViewBagData(id);
                        return View(user);
                    }
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear User, we could not find the access info you were looking for!";
                    return RedirectToAction("ViewAllAccessUsers");
                }
                user = database.USERS.Find(id);
                //emp = new EmployeeInfo{EmployeeInfoId = -1};
                //ViewBag.EmpInfo = emp;//.GetEmployeeInfoById(id.ToString());
                SetViewBagData(id);
                return View(user);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        /*[HttpPost]
        public ActionResult UserCreate(USER user)
        {
            if (user.Email == "" || user.Email == null)
            {
                ModelState.AddModelError(string.Empty, "Email cannot be blank");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    List<string> results = database.Database.SqlQuery<String>(string.Format("SELECT Email FROM AccessUsers WHERE Email = '{0}'", user.Email)).ToList();
                    bool _userExistsInTable = (results.Count > 0);

                    USER _user = null;
                    if (_userExistsInTable)
                    {
                        _user = database.USERS.Where(p => p.Email == user.Email).FirstOrDefault();
                        if (_user != null)
                        {
                            if (_user.Status == false)
                            {
                                ModelState.AddModelError(string.Empty, "USER already exists!");
                            }
                            else
                            {
                                database.Entry(_user).Entity.Status = true;
                                database.Entry(_user).Entity.Update_By = Session["UserEmail"].ToString();
                                database.Entry(_user).Entity.Update_Date = System.DateTime.Now;
                                database.Entry(_user).State = EntityState.Modified;
                                database.SaveChanges();
                                return RedirectToAction("Index");
                            }
                        }
                    }
                    else
                    {
                        _user = new USER();
                        _user.Email = user.Email;
                        _user.Lastname = user.Lastname;
                        _user.Login_Name = user.Login_Name;
                        _user.Title = user.Title;
                        _user.Initial = user.Initial;
                        _user.EMail = user.EMail;

                        if (ModelState.IsValid)
                        {
                            _user.Status = false;
                            _user.Update_Date = System.DateTime.Now;

                            database.USERS.Add(_user);
                            database.SaveChanges();
                            return RedirectToAction("Index");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                //return base.ShowError(ex);
            }

            return View(user);
        }*/

        /*
         
         
         
         
         
         if (Session["Role"] != null)
            {
                
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
         
         
         
         */


        [HttpGet]
        public ActionResult EditAccess(int id)
        {
            if (Session["Role"] != null)
            {
                USER user;
                if (id == 0)
                {
                    if (string.Equals(Session["AccessId"].ToString(), id.ToString()))
                    {
                        user = database.USERS.Find(id);
                        SetViewBagData(id);
                        return View(user);
                    }
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear User, we could not find the access info you were looking for!";
                    return RedirectToAction("ViewAllAccessUsers");
                }
                user = database.USERS.Find(id);
                SetViewBagData(id);
                return View(user);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        /*[HttpPost]
        public ActionResult EditAccess(USER user)
        {
            if (Session["Role"] != null)
            {
                USER _user = database.USERS.Where(p => p.ID == user.ID).FirstOrDefault();
                if (_user != null)
                {
                    try
                    {
                        database.Entry(_user).CurrentValues.SetValues(user);
                        database.Entry(_user).Entity.Update_Date = System.DateTime.Now;
                        database.SaveChanges();
                    }
                    catch (Exception)
                    {

                    }
                }
                return RedirectToAction("AccessDetails", new RouteValueDictionary(new { id = user.ID }));
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        [HttpPost]
        public ActionResult AccessDetails(USER user)
        {
            if (Session["Role"] != null)
            {
                if (user.Email == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid USER Name");
                }
                if (ModelState.IsValid)
                {
                    database.Entry(user).Entity.Status = user.Status;
                    database.Entry(user).Entity.Update_Date = System.DateTime.Now;
                    database.Entry(user).State = EntityState.Modified;
                    database.SaveChanges();
                }
                return View(user);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }*/

        [HttpPost]
        public ActionResult AddUserRole(FormCollection frm)//Add
        {
            if (Session["Role"] != null)
            {
                var rid = frm["roleId"];
                var uid = frm["roleId"];
                if (string.IsNullOrEmpty(rid))
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear User, please select a role to add to User.";
                    return RedirectToAction("EditAccess", "Access", new { id = uid });
                }
                int roleId = Convert.ToInt32(rid);
                int userId = Convert.ToInt32(uid);
                ROLE role;// = database.ROLES.Find(roleId);
                USER user;// = database.USERS.Find(userId);
                if (userId == 0)
                {
                    if (string.Equals(Session["AccessId"].ToString(), userId.ToString()))
                    {
                        role = database.ROLES.Find(roleId);
                        user = database.USERS.Find(userId);
                        if (!role.USERS.Contains(user))
                        {
                            role.USERS.Add(user);
                            database.SaveChanges();
                        }
                        Session["success_div"] = "true";
                        Session["success_msg"] = "Role added To user successfully.";
                        return RedirectToAction("EditAccess", "Access", new { id = userId });
                    }
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear User, please select a user to add role.";
                    return RedirectToAction("ViewAllAccessUsers");
                }
                role = database.ROLES.Find(roleId);
                user = database.USERS.Find(userId);
                if (!role.USERS.Contains(user))
                {
                    role.USERS.Add(user);
                    database.SaveChanges();
                }
                return RedirectToAction("AccessDetails", "Access", new { id = userId });
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        [HttpGet]
        public ActionResult DeleteUserRole(int roleId, int userId)
        {
            if (Session["Role"] != null)
            {
                //if (string.Equals(Session["AccessId"].ToString(),userId.ToString()))
                //{
                //    Session["warning_div"] = "true";
                //    Session["warning_msg"] = "Dear User, you can\'t delete Role from yourself.";
                //    return RedirectToAction("AccessDetails", "Access", new { id = userId });
                //}
                if (userId == 0)
                {
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Dear User, please select a user to delete role.";
                    return RedirectToAction("ViewAllAccessUsers");
                }
                ROLE role = database.ROLES.Find(roleId);
                USER user = database.USERS.Find(userId);

                if (role.USERS.Contains(user))
                {
                    role.USERS.Remove(user);
                    database.SaveChanges();
                }
                return RedirectToAction("AccessDetails", "Access", new { id = userId });
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        //[HttpGet]
        //public PartialViewResult filter4Users(string _surname)
        //{
        //    return PartialView("_ListUserTable", GetFilteredUserList(_surname));
        //}

        /*[HttpGet]
        public PartialViewResult filterReset()
        {
            return PartialView("_ListUserTable", database.USERS.Where(r => r.Status == true).ToList());
        }

        [HttpGet]
        public PartialViewResult DeleteUserReturnPartialView(int userId)
        {
            try
            {
                USER user = database.USERS.Find(userId);
                if (user != null)
                {
                    database.Entry(user).Entity.Status = true;
                    database.Entry(user).Entity.ID = user.ID;
                    database.Entry(user).Entity.Update_Date = System.DateTime.Now;
                    database.Entry(user).State = EntityState.Modified;
                    database.SaveChanges();
                }
            }
            catch
            {
            }
            return this.filterReset();
        }

        private IEnumerable<USER> GetFilteredUserList(string _surname)
        {
            IEnumerable<USER> _ret = null;
            try
            {
                if (string.IsNullOrEmpty(_surname))
                {
                    _ret = database.USERS.Where(r => r.Status == true).ToList();
                }
                else
                {
                    _ret = database.USERS.Where(p => p.Lastname == _surname).ToList();
                }
            }
            catch
            {
            }
            return _ret;
        }

       
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public PartialViewResult DeleteUserRoleReturnPartialView(int id, int userId)
        {
            ROLE role = database.ROLES.Find(id);
            USER user = database.USERS.Find(userId);

            if (role.USERS.Contains(user))
            {
                role.USERS.Remove(user);
                database.SaveChanges();
            }
            SetViewBagData(userId);
            return PartialView("_ListUserRoleTable", database.USERS.Find(userId));
        }

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public PartialViewResult AddUserRoleReturnPartialView(int id, int userId)
        {
            ROLE role = database.ROLES.Find(id);
            USER user = database.USERS.Find(userId);

            if (!role.USERS.Contains(user))
            {
                role.USERS.Add(user);
                database.SaveChanges();
            }
            SetViewBagData(userId);
            return PartialView("_ListUserRoleTable", database.USERS.Find(userId));
        }*/

        private void SetViewBagData(int _userId)
        {
            ViewBag.UserId = _userId;
            ViewBag.List_boolNullYesNo = this.List_boolNullYesNo();
            ViewBag.RoleId = new SelectList(database.ROLES.OrderBy(p => p.RoleName), "Role_Id", "RoleName");
        }

        public List<SelectListItem> List_boolNullYesNo()
        {
            var _retVal = new List<SelectListItem>();
            try
            {
                _retVal.Add(new SelectListItem { Text = "Not Set", Value = null });
                _retVal.Add(new SelectListItem { Text = "Yes", Value = bool.TrueString });
                _retVal.Add(new SelectListItem { Text = "No", Value = bool.FalseString });
            }
            catch { }
            return _retVal;
        }
        #endregion

        #region ROLES
        public ActionResult ViewAllRoles()
        {
            if (Session["Role"] != null)
            {
                return View(database.ROLES.OrderBy(r => r.RoleDescription).ToList());
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");

        }

        public ActionResult RoleDetails(int id)
        {
            if (Session["Role"] != null)
            {
                USER user = database.USERS.Where(r => r.UserName == User.Identity.Name).FirstOrDefault();
                USER usr = database.USERS.Where(r => r.SrlNo == 0).FirstOrDefault();
                ROLE role = database.ROLES.Where(r => r.Role_Id == id)
                       .Include(a => a.PERMISSIONS)
                       .Include(a => a.USERS)
                       .FirstOrDefault();

                if (role != null && !object.Equals(user, usr))
                {
                    var usrs = role.USERS.Where(r => r.SrlNo != 0).ToList();
                    role.USERS = usrs;
                }
                //USERS combo
                var selectList = new SelectList(database.USERS.Where(r => r.Status == true && r.SrlNo != 0), "ID", "Email");
                ViewBag.UserId = selectList;
                ViewBag.RoleId = id;

                // Rights combo
                ViewBag.PermissionId = new SelectList(database.PERMISSIONS.Where(u => u.Status == true).OrderBy(a => a.PermissionName), "Permission_Id", "PermissionName");
                ViewBag.List_boolNullYesNo = this.List_boolNullYesNo();

                return View(role);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        public ActionResult AddNewRole()
        {
            USER user = database.USERS.Where(r => r.UserName == User.Identity.Name).FirstOrDefault();
            ViewBag.List_boolNullYesNo = this.List_boolNullYesNo();
            return View();
        }

        [HttpPost]
        public ActionResult AddNewRole(ROLE _role)
        {
            if (Session["Role"] != null)
            {
                if (_role.RoleDescription == null)
                {
                    ModelState.AddModelError("Role Description", "Role Description must be entered");
                }
                if (_role.IsSysAdmin == false)
                {
                    _role.IsSysAdmin = false;
                }
                USER user = database.USERS.Where(r => r.UserName == User.Identity.Name).FirstOrDefault();
                if (ModelState.IsValid)
                {
                    database.ROLES.Add(_role);
                    database.SaveChanges();
                    return RedirectToAction("ViewAllRoles");
                }
                ViewBag.List_boolNullYesNo = this.List_boolNullYesNo();
                return View(_role);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }


        public ActionResult EditRole(int id)
        {
            if (Session["Role"] != null)
            {
                USER user = database.USERS.Where(r => r.UserName == User.Identity.Name).FirstOrDefault();
                USER usr = database.USERS.Where(r => r.SrlNo == 0).FirstOrDefault();
                ROLE role = database.ROLES.Where(r => r.Role_Id == id)
                       .Include(a => a.PERMISSIONS)
                       .Include(a => a.USERS)
                       .FirstOrDefault();

                if (role != null && !object.Equals(user, usr))
                {
                    var usrs = role.USERS.Where(r => r.SrlNo != 0).ToList();
                    role.USERS = usrs;
                }
                //USERS combo
                //ViewBag.UserId = new SelectList(database.USERS.Where(r => r.Status == true && r.ID != 0), "ID", "Email");
                SelectListItem itm;
                List<SelectListItem> listItms = new List<SelectListItem>();
                itm = new SelectListItem { Text = "", Value = "" };
                listItms.Add(itm);
                foreach (var us in database.USERS.Where(r => r.Status == true && r.SrlNo != 0))
                {
                    if (!role.USERS.Contains(us))
                    {
                        itm = new SelectListItem { Text = "Name: " + us.Name + "; Email: " + us.UserName, Value = us.SrlNo.ToString() };
                        listItms.Add(itm);
                    }
                }
                ViewBag.UserId = new SelectList(listItms, "Value", "Text");

                ViewBag.RoleId = id;

                // Rights combo
                ViewBag.PermissionId = new SelectList(database.PERMISSIONS.Where(u => u.Status == true).OrderBy(a => a.PermissionName), "Permission_Id", "PermissionName");
                ViewBag.List_boolNullYesNo = this.List_boolNullYesNo();

                return View(role);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");

        }

        [HttpPost]
        public ActionResult EditRole(ROLE _role)
        {
            if (Session["Role"] != null)
            {
                if (string.IsNullOrEmpty(_role.RoleDescription))
                {
                    ModelState.AddModelError("Role Description", "Role Description must be entered");
                }

                //EntityState state = database.Entry(_role).State;
                USER user = database.USERS.Where(r => r.UserName == User.Identity.Name).FirstOrDefault();
                if (ModelState.IsValid)
                {
                    database.Entry(_role).State = EntityState.Modified;
                    database.SaveChanges();
                    Session["success_div"] = "true";
                    Session["success_msg"] = "Role updated successfully.";
                    return RedirectToAction("RoleDetails", new RouteValueDictionary(new { id = _role.Role_Id }));
                }
                // USERS combo
                //ViewBag.UserId = new SelectList(database.USERS.Where(r => r.Status == true && r.ID != 0), "ID", "Email");
                SelectListItem itm;
                List<SelectListItem> listItms = new List<SelectListItem>();
                itm = new SelectListItem { Text = "", Value = "" };
                listItms.Add(itm);
                foreach (var usr in database.USERS.Where(r => r.Status == true && r.SrlNo != 0))
                {
                    if (!_role.USERS.Contains(usr))
                    {
                        itm = new SelectListItem { Text = "Name: " + usr.Name + "; Email: " + usr.UserName, Value = usr.SrlNo.ToString() };
                        listItms.Add(itm);
                    }
                }
                ViewBag.UserId = new SelectList(listItms, "Value", "Text");

                // Rights combo
                ViewBag.PermissionId = new SelectList(database.PERMISSIONS.Where(u => u.Status == true).OrderBy(a => a.PermissionName), "Permission_Id", "PermissionName");
                ViewBag.List_boolNullYesNo = this.List_boolNullYesNo();
                return View(_role);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }


        public ActionResult RoleDelete(int id)
        {
            if (Session["Role"] != null)
            {
                ROLE _role = database.ROLES.Find(id);
                if (_role != null)
                {
                    _role.USERS.Clear();
                    _role.PERMISSIONS.Clear();

                    database.Entry(_role).State = EntityState.Deleted;
                    database.SaveChanges();
                }
                return RedirectToAction("ViewAllRoles");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        /*[HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public PartialViewResult DeleteUserFromRoleReturnPartialView(int id, int userId)
        {
            ROLE role = database.ROLES.Find(id);
            USER user = database.USERS.Find(userId);

            if (role.USERS.Contains(user))
            {
                role.USERS.Remove(user);
                database.SaveChanges();
            }
            return PartialView("_ListUsersTable4Role", role);
        }*/

        [HttpGet]
        public ActionResult DeleteUserFromRole(int roleId, int userId)
        {
            if (Session["Role"] != null)
            {
                ROLE role = database.ROLES.Find(roleId);
                USER user = database.USERS.Find(userId);

                if (role.USERS.Contains(user))
                {
                    role.USERS.Remove(user);
                    database.SaveChanges();
                    Session["success_div"] = "true";
                    Session["success_msg"] = "You are successfully remove User from this Role";
                    return RedirectToAction("EditRole", "Access", new { id = roleId });
                }
                return RedirectToAction("EditRole", "Access", new { id = roleId });
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        [HttpPost]
        public ActionResult AddUserToRole(int roleId, int userId)
        {
            if (Session["Role"] != null)
            {
                ROLE role = database.ROLES.Find(roleId);
                USER user = database.USERS.Find(userId);

                if (!role.USERS.Contains(user))
                {
                    role.USERS.Add(user);
                    database.SaveChanges();
                    Session["success_div"] = "true";
                    Session["success_msg"] = "You are successfully add User to this Role";
                    return RedirectToAction("EditRole", "Access", new { id = roleId });
                }
                return RedirectToAction("EditRole", "Access", new { id = roleId });
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        /*[HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public PartialViewResult AddUser2RoleReturnPartialView(int id, int userId)
        {
            ROLE role = database.ROLES.Find(id);
            USER user = database.USERS.Find(userId);

            if (!role.USERS.Contains(user))
            {
                role.USERS.Add(user);
                database.SaveChanges();
            }
            return PartialView("_ListUsersTable4Role", role);
        }*/

        #endregion

        #region PERMISSIONS

        public ActionResult ViewAllPermissions()
        {
            if (Session["Role"] != null)
            {
                List<PERMISSION> permissions = database.PERMISSIONS
                                              .Where(wn => wn.Status == true)
                                              .OrderBy(wn => wn.PermissionDescription)
                                              .Include(a => a.ROLES)
                                              .ToList();
                return View(permissions);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        public ActionResult PermissionDetails(int id)
        {
            if (Session["Role"] != null)
            {
                PERMISSION _permission = database.PERMISSIONS.Find(id);
                return View(_permission);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        public ActionResult AddNewPermission()
        {
            if (Session["Role"] != null)
            {
                var controllerList = new List<DropDownData> { new DropDownData("", "") };
                var clist = MvcHelper.Instance.GetControllerNames();
                if (clist.Any())
                {
                    foreach (var ctrl in clist)
                    {
                        controllerList.Add(new DropDownData(ctrl, ctrl));
                    }
                }
                ViewBag.ListOfController = new SelectList(controllerList, "Value", "Text");
                return View();
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");

        }

        [HttpPost]
        public ActionResult AddNewPermission(PERMISSION _permission)
        {
            if (Session["Role"] != null)
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if ((_permission.IsPage != null && _permission.IsPage.Value) && string.IsNullOrEmpty(_permission.LinkText))
                        {
                            _permission.LinkText = _permission.Action;
                        }
                        database.PERMISSIONS.Add(_permission);
                        database.SaveChanges();
                        Session["success_div"] = "true";
                        Session["success_msg"] = "You are successfully add Permission.";
                        return RedirectToAction("ViewAllPermissions");
                    }
                    var controllerList = new List<DropDownData> { new DropDownData("", "") };
                    var clist = MvcHelper.Instance.GetControllerNames();
                    if (clist.Any())
                    {
                        foreach (var ctrl in clist)
                        {
                            controllerList.Add(new DropDownData(ctrl, ctrl));
                        }
                    }
                    ViewBag.ListOfController = new SelectList(controllerList, "Value", "Text");
                    Session["warning_div"] = "true";
                    Session["warning_msg"] = "Permission Not Saved, Please Give all data correctly.";
                    return View(_permission);
                }
                catch (Exception ex)
                {
                    Exception innEx = ex.InnerException;
                    Exception innInnEx = innEx.InnerException;
                    var controllerList = new List<DropDownData> { new DropDownData("", "") };
                    var clist = MvcHelper.Instance.GetControllerNames();
                    if (clist.Any())
                    {
                        foreach (var ctrl in clist)
                        {
                            controllerList.Add(new DropDownData(ctrl, ctrl));
                        }
                    }
                    ViewBag.ListOfController = new SelectList(controllerList, "Value", "Text");
                    if (innInnEx.Message.Contains("Violation of UNIQUE KEY constraint 'Ux_PermissionDescription'"))
                    {
                        Session["warning_div"] = "true";
                        Session["warning_msg"] = "Dear Admin, Controller: '" + _permission.Controller + "' and Action: '" + _permission.Action + "' This Permission Is Already Exists!";
                    }
                    else
                    {
                        Session["error_div"] = "true";
                        Session["error_msg"] = "Error happend! Permission NOT Saved";
                            //string.Format("Error happend! 1st Error Message is: {0}, 2nd Error Message is: {1}, 3rd Error Message is: {2}", ex.Message, innEx.Message, innInnEx.Message);
                    }
                    return View(_permission);
                }
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        public ActionResult EditPermission(int id)
        {
            if (Session["Role"] != null)
            {
                PERMISSION _permission = database.PERMISSIONS.Find(id);
                ViewBag.RoleId = new SelectList(database.ROLES.OrderBy(p => p.RoleName), "Role_Id", "RoleName");
                return View(_permission);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");

        }

        [HttpPost]
        public ActionResult EditPermission(PERMISSION _permission)
        {
            if (Session["Role"] != null)
            {
                if (ModelState.IsValid)
                {
                    database.Entry(_permission).State = EntityState.Modified;
                    database.SaveChanges();
                    return RedirectToAction("PermissionDetails", new RouteValueDictionary(new { id = _permission.Permission_Id }));
                }
                ViewBag.RoleId = new SelectList(database.ROLES.OrderBy(p => p.RoleName), "Role_Id", "RoleName");
                return View(_permission);
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        [HttpPost]
        public ActionResult AddPermissionToRole(int roleId, int permissionId)
        {
            if (Session["Role"] != null)
            {
                ROLE role = database.ROLES.Find(roleId);
                PERMISSION _permission = database.PERMISSIONS.Find(permissionId);

                if (!role.PERMISSIONS.Contains(_permission))
                {
                    role.PERMISSIONS.Add(_permission);
                    database.SaveChanges();
                    database.SaveChanges();
                    Session["success_div"] = "true";
                    Session["success_msg"] = "You are successfully add Permission to this Role";
                    return RedirectToAction("EditRole", "Access", new { id = roleId });
                }
                return RedirectToAction("EditRole", "Access", new { id = roleId });
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        public ActionResult DeletePermissionFromRole(int roleId, int permissionId)
        {
            if (Session["Role"] != null)
            {
                ROLE _role = database.ROLES.Find(roleId);
                PERMISSION _permission = database.PERMISSIONS.Find(permissionId);
                if (_role.PERMISSIONS.Contains(_permission))
                {
                    _role.PERMISSIONS.Remove(_permission);
                    database.SaveChanges();
                    Session["success_div"] = "true";
                    Session["success_msg"] = "You are successfully remove Permission from this Role";
                    return RedirectToAction("RoleDetails", "Access", new { id = roleId });
                }
                return RedirectToAction("ViewAllRoles", "Access");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        public ActionResult DeleteRoleFromPermission(int permissionId, int roleId)
        {
            if (Session["Role"] != null)
            {
                ROLE _role = database.ROLES.Find(roleId);
                PERMISSION _permission = database.PERMISSIONS.Find(permissionId);
                if (_role.PERMISSIONS.Contains(_permission))
                {
                    _role.PERMISSIONS.Remove(_permission);
                    database.SaveChanges();
                    Session["success_div"] = "true";
                    Session["success_msg"] = "You are successfully remove Role from this Permission";
                    return RedirectToAction("PermissionDetails", "Access", new { id = permissionId });
                }
                return RedirectToAction("ViewAllPermissions", "Access");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        [HttpPost]
        public ActionResult AddRoleToPermission(int permissionId, int roleId)
        {
            if (Session["Role"] != null)
            {
                ROLE role = database.ROLES.Find(roleId);
                PERMISSION permission = database.PERMISSIONS.Find(permissionId);

                if (!role.PERMISSIONS.Contains(permission))
                {
                    role.PERMISSIONS.Add(permission);
                    database.SaveChanges();
                    Session["success_div"] = "true";
                    Session["success_msg"] = "You are successfully Add Role to this Permission";
                    return RedirectToAction("PermissionDetails", "Access", new { id = permissionId });
                }
                return RedirectToAction("ViewAllPermissions", "Access");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        //GetActionNameByControllerJson

        public JsonResult GetActionNameByControllerJson(string controllerName)
        {
            if (Session["Role"] != null)//&& Session["Role"].ToString() == "Admin")
            {
                var lstList = new List<DropDownData>();
                Assembly asm = Assembly.GetAssembly(typeof(MvcHRMS.MvcApplication));
                var controlleractionlist = asm.GetTypes()
                        .Where(type => typeof(System.Web.Mvc.Controller).IsAssignableFrom(type))
                        .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                        .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
                        .Select(x => new { Controller = x.DeclaringType.Name, Action = x.Name})//, ReturnType = x.ReturnType.Name, Attributes = String.Join(",", x.GetCustomAttributes().Select(a => a.GetType().Name.Replace("Attribute", ""))) })
                        .OrderBy(x => x.Controller).ThenBy(x => x.Action).ToList();
                
                foreach (var r in controlleractionlist)
                {
                    string _controllerName = r.Controller;
                    if (_controllerName.EndsWith("Controller"))
                    {
                        _controllerName = _controllerName.Substring(0, _controllerName.LastIndexOf("Controller"));
                    }
                    if (controllerName == _controllerName)
                    {
                        var dd = new DropDownData(r.Action, r.Action);
                        bool containsItem = lstList.Any(item => item.Value == dd.Value);
                        if (!containsItem)
                        {
                            lstList.Add(dd);
                        }
                    }
                }
                var retval = Json(new { lstList }, JsonRequestBehavior.AllowGet);
                return retval;
            }
            return null;
        }

        /*[HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult PermissionDelete(int id)
        {
            if (Session["Role"] != null)
            {
                PERMISSION permission = database.PERMISSIONS.Find(id);
                if (permission.ROLES.Count > 0)
                    permission.ROLES.Clear();

                database.Entry(permission).State = EntityState.Deleted;
                database.SaveChanges();
                return RedirectToAction("PermissionIndex");
            }
            Session["warning_div"] = "true";
            Session["warning_msg"] = "Dear User, you must log in to continue the request.";
            return RedirectToAction("Index", "Account");
        }

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public PartialViewResult AddPermission2RoleReturnPartialView(int id, int permissionId)
        {
            ROLE role = database.ROLES.Find(id);
            PERMISSION _permission = database.PERMISSIONS.Find(permissionId);

            if (!role.PERMISSIONS.Contains(_permission))
            {
                role.PERMISSIONS.Add(_permission);
                database.SaveChanges();
            }
            return PartialView("_ListPermissions", role);
        }

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public PartialViewResult AddAllPermissions2RoleReturnPartialView(int id)
        {
            ROLE _role = database.ROLES.Where(p => p.Role_Id == id).FirstOrDefault();
            List<PERMISSION> _permissions = database.PERMISSIONS.ToList();
            foreach (PERMISSION _permission in _permissions)
            {
                if (!_role.PERMISSIONS.Contains(_permission))
                {
                    _role.PERMISSIONS.Add(_permission);

                }
            }
            database.SaveChanges();
            return PartialView("_ListPermissions", _role);
        }

       [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public PartialViewResult DeletePermissionFromRoleReturnPartialView(int id, int permissionId)
        {
            ROLE _role = database.ROLES.Find(id);
            PERMISSION _permission = database.PERMISSIONS.Find(permissionId);

            if (_role.PERMISSIONS.Contains(_permission))
            {
                _role.PERMISSIONS.Remove(_permission);
                database.SaveChanges();
            }
            return PartialView("_ListPermissions", _role);
        }

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public PartialViewResult DeleteRoleFromPermissionReturnPartialView(int id, int permissionId)
        {
            ROLE role = database.ROLES.Find(id);
            PERMISSION permission = database.PERMISSIONS.Find(permissionId);

            if (role.PERMISSIONS.Contains(permission))
            {
                role.PERMISSIONS.Remove(permission);
                database.SaveChanges();
            }
            return PartialView("_ListRolesTable4Permission", permission);
        }

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public PartialViewResult AddRole2PermissionReturnPartialView(int permissionId, int roleId)
        {
            ROLE role = database.ROLES.Find(roleId);
            PERMISSION _permission = database.PERMISSIONS.Find(permissionId);

            if (!role.PERMISSIONS.Contains(_permission))
            {
                role.PERMISSIONS.Add(_permission);
                database.SaveChanges();
            }
            return PartialView("_ListRolesTable4Permission", _permission);
        }*/


        /*public ActionResult PermissionsImport()
         {
             //SmartLivingSolution.Web.MvcApplication
             if (Session["Role"] != null)
             {
                 Assembly asm = Assembly.GetAssembly(typeof(MvcHRMS.MvcApplication));
                 var controlleractionlist = asm.GetTypes()
                         .Where(type => typeof(System.Web.Mvc.Controller).IsAssignableFrom(type))
                         .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                         .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
                         .Select(x => new { Controller = x.DeclaringType.Name, Action = x.Name, ReturnType = x.ReturnType.Name, Attributes = String.Join(",", x.GetCustomAttributes().Select(a => a.GetType().Name.Replace("Attribute", ""))) })
                         .OrderBy(x => x.Controller).ThenBy(x => x.Action).ToList();
                 foreach (var r in controlleractionlist)
                 {
                     string _controllerName = r.Controller;
                     if (_controllerName.EndsWith("Controller"))
                     {
                         _controllerName = _controllerName.Substring(0, _controllerName.LastIndexOf("Controller"));
                     }
                     string _controllerActionName = r.Action;
                     string _permissionDescription = string.Format("{0}-{1}", _controllerName.ToLower(), _controllerActionName.ToLower());
                     PERMISSION _permission = database.PERMISSIONS.Where(p => p.PermissionDescription == _permissionDescription).FirstOrDefault();
                     if (_permission == null)
                     {
                         if (ModelState.IsValid)
                         {
                             PERMISSION _perm = new PERMISSION();
                             _perm.PermissionDescription = _permissionDescription;
                             _perm.PermissionName = _permissionDescription;
                             _perm.Controller = _controllerName;//r.Controller;
                             _perm.Action = r.Action;
                             _perm.ReturnType = r.ReturnType;
                             _perm.Attributes = r.Attributes;
                             database.PERMISSIONS.Add(_perm);
                             database.SaveChanges();
                         }
                     }
                 }

                  *var _controllerTypes = AppDomain.CurrentDomain.GetAssemblies()
                 .SelectMany(a => a.GetTypes())
                 .Where(t => t != null
                     && t.IsPublic
                     && t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
                     && !t.IsAbstract
                     && typeof(IController).IsAssignableFrom(t));

                 var _controllerMethods = _controllerTypes.ToDictionary(controllerType => controllerType,
                         controllerType => controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                         .Where(m => typeof(ActionResult).IsAssignableFrom(m.ReturnType)));

                 foreach (var _controller in _controllerMethods)
                 {
                     string _controllerName = _controller.Key.Name;
                     foreach (var _controllerAction in _controller.Value)
                     {
                         string _controllerActionName = _controllerAction.Name;
                         if (_controllerName.EndsWith("Controller"))
                         {
                             _controllerName = _controllerName.Substring(0, _controllerName.LastIndexOf("Controller"));
                         }

                         string _permissionDescription = string.Format("{0}-{1}", _controllerName, _controllerActionName);
                         PERMISSION _permission = database.PERMISSIONS.Where(p => p.PermissionDescription == _permissionDescription).FirstOrDefault();
                         if (_permission == null)
                         {
                             if (ModelState.IsValid)
                             {
                                 PERMISSION _perm = new PERMISSION();
                                 _perm.PermissionDescription = _permissionDescription;

                                 database.PERMISSIONS.Add(_perm);
                                 database.SaveChanges();
                             }
                         }
                     }
                 }*
                 return RedirectToAction("ViewAllPermissions");
             }
             Session["warning_div"] = "true";
             Session["warning_msg"] = "Dear User, you must log in to continue the request.";
             return RedirectToAction("Index", "Account");
         }*/

        protected override void Dispose(bool disposing)
        {
            database.Dispose();
            base.Dispose(disposing);
        }
        #endregion
    }
}
