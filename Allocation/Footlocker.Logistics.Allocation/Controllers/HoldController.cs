using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using Telerik.Web.Mvc;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.DAO;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Director of Allocation,Admin,Support,Space Planning")]
    public class HoldController : AppController
    {
        #region Fields

        private readonly int _BIN_SIZE_VALUE_LENGTH = 3;
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        #endregion

        private string CheckHoldPermission(Hold model)
        {                    //check permission
            if (model.Level == "Dept")
            {
                if (!CheckRole(User.Identity.Name, "Dept"))
                    return "You are not authorized to create Dept level holds.";
            }
            else if (model.Level == "All")
            {
                if (!CheckRole(User.Identity.Name, "Location"))
                    return "You are not authorized to create Store level holds.";
            }
            else if (!CheckRole(User.Identity.Name, "Sku"))
            {
                return "You are not authorized to create this hold.";
            }
            return "";
        }

        private Boolean CheckRole(string user, string holdType)
        {
            bool ok = false;
            string roles;
            switch (holdType)
            {
                case "Sku":
                    roles = "Merchandiser,Head Merchandiser,Director of Allocation,Admin,Support";
                    break;
                case "Dept":
                    roles = "Head Merchandiser,Space Planning,Director of Allocation,Admin,Support";
                    break;
                case "Location":
                    roles = "Space Planning,Director of Allocation,Admin,Support,Advanced Merchandiser Processes";
                    break;
                default:
                    roles = "Support";
                    break;
            }

            if (!string.IsNullOrEmpty(roles))
            {
                string username = System.Web.HttpContext.Current.User.Identity.Name.ToLower().Replace("corp\\", "");
                string[] rolelist = roles.Split(new char[] { ',' });
                ok = WebSecurityService.UserHasRole(username, "Allocation", rolelist);
            }

            return ok;

        }

        public ActionResult Index(string duration, string message)
        {
            if ((duration == null) || (duration == ""))
            {
                duration = "All";
            }
            List<Division> divs = Divisions();
            List<Hold> list = db.Holds.ToList();
            list = (from a in list
                               join d in divs on a.Division equals d.DivCode where ((a.Duration == duration )|| (duration=="All"))
                    select a).ToList();
            //TODO:  Do we want dept level security on holds???
            ViewData["message"] = message;
            return View(list);
        }

        public ActionResult IndexByProduct(string duration, string message)
        {
            if ((duration == null) || (duration == ""))
            {
                duration = "All";
            }
            //this is more for developer debugging
            //clear the cache when they go back to the index.
            Session["rdqgrouplist"] = null;
            ViewData["message"] = message;
            return View();
        }

        public ActionResult IndexByStore(string duration, string message)
        {
            if ((duration == null) || (duration == ""))
            {
                duration = "All";
            }
            //this is more for developer debugging
            //clear the cache when they go back to the index.
            Session["rdqgrouplist"] = null;
            ViewData["message"] = message;
            return View();
        }

        [GridAction]
        public ActionResult _Index(string duration)
        {
            if ((duration == null) || (duration == ""))
            {
                duration = "All";
            }
            List<Division> divs = Divisions();
            List<Hold> list = db.Holds.ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    where ((a.Duration == duration) || (duration == "All"))
                    select a).ToList();
            //TODO:  Do we want dept level security on holds???

            return View(new GridModel(list));
        }

        [GridAction]
        public ActionResult _IndexByProduct(string duration)
        {
            if ((duration == null) || (duration == ""))
            {
                duration = "All";
            }
            List<Division> divs = Divisions();
            List<Hold> list = db.Holds.ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    where ((a.Duration == duration) || (duration == "All"))
                    select a).ToList();
            //TODO:  Do we want dept level security on holds???

            List<HoldByProductModel> finalList = list.GroupBy(x => new {x.Division, x.Level, x.Value, x.HoldType})
                .Select( y => new HoldByProductModel(y.Key.Division, y.Key.Level, y.Key.Value, y.Key.HoldType)).ToList();
            return View(new GridModel(finalList));
        }

        [GridAction]
        public ActionResult _IndexByStore(string duration)
        {
            if ((duration == null) || (duration == ""))
            {
                duration = "All";
            }
            List<Division> divs = Divisions();
            List<Hold> list = db.Holds.ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    where ((a.Duration == duration) || (duration == "All"))
                    select a).ToList();
            //TODO:  Do we want dept level security on holds???

            List<HoldByStoreModel> finalList = list.GroupBy(x => new { x.Division, x.Store, x.HoldType })
                .Select(y => new HoldByStoreModel(y.Key.Division, y.Key.Store, y.Key.HoldType)).ToList();
            return View(new GridModel(finalList));
        }


        [GridAction]
        public ActionResult _HoldDetails(string div, string level, string value, string holdType, string duration)
        {
            if ((duration == null) || (duration == ""))
            {
                duration = "All";
            }
            List<Division> divs = Divisions();
            List<Hold> list = db.Holds.ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    where (((a.Duration == duration) || (duration == "All"))
                    && (a.Division == div)
                    && (a.Level == level)
                    && (a.Value == value)
                    && (a.HoldType == holdType)
                    )
                    select a).ToList();
            //TODO:  Do we want dept level security on holds???

            return View(new GridModel(list));
        }

        [GridAction]
        public ActionResult _HoldStoreDetails(string div, string store, string holdType, string duration)
        {
            if ((duration == null) || (duration == ""))
            {
                duration = "All";
            }
            List<Division> divs = Divisions();
            List<Hold> list = db.Holds.ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    where (((a.Duration == duration) || (duration == "All"))
                    && (a.Division == div)
                    && ((a.Store == store) || ((a.Store == null)&&(store == "")))
                    && (a.HoldType == holdType)
                    )
                    select a).ToList();

            return View(new GridModel(list));
        }


        public ActionResult Create()
        {
            HoldModel model = new HoldModel();
            model.Hold = new Hold();
            model.Hold.StartDate = DateTime.Now.AddDays(1);
            model.Divisions = this.Divisions();
            model.ShowStoreSelector = "no";
            model.RuleModel = new RuleModel();
            ViewData["ruleSetID"] = model.RuleSetID;
            ViewData["ruleType"] = "hold";
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(HoldModel model)
        {
            ViewData["ruleSetID"] = model.RuleSetID;
            ViewData["ruleType"] = "hold";
            string validationMessage = ValidateHold(model.Hold, (model.RuleSetID > 0), false);
            if (model.ShowStoreSelector == "yes")
            {
                if (model.RuleSetID < 1)
                {
                    //get a new ruleset
                    RuleSet rs = new RuleSet();
                    rs.Type = "hold";
                    rs.CreateDate = DateTime.Now;
                    rs.CreatedBy = UserName;
                    rs.Division = model.Hold.Division;
                    db.RuleSets.Add(rs);
                    db.SaveChanges();

                    model.RuleSetID = rs.RuleSetID;
                }
                
                ViewData["ruleSetID"] = model.RuleSetID;
                model.Divisions = this.Divisions();
                return View(model);
            }
            if (validationMessage != "")
            {
                ViewData["message"] = validationMessage;
                model.Divisions = this.Divisions();
                return View(model);
            }
            else
            {
                model.Hold.CreateDate = DateTime.Now;
                model.Hold.CreatedBy = User.Identity.Name;
                //TODO:  Do we want dept level security on holds???
                if ((Footlocker.Common.WebSecurityService.UserHasDivision(UserName, "Allocation", model.Hold.Division)))
                {
                    if ((model.Hold.Level == "Sku")&&(model.Hold.Division != model.Hold.Value.Substring(0, 2)))
                    {
                        ViewData["message"] = "Invalid Sku, division does not match selection.";
                        model.Divisions = this.Divisions();
                        return View(model);
                    }
                    else
                    {
                        if (model.RuleSetID == 0)
                        {
                            db.Holds.Add(model.Hold);
                            db.SaveChanges();
                            ApplyHoldsToExistingWebPicks(model.Hold);
                            return RedirectToAction("Index", new { duration = model.Hold.Duration });
                        }
                        else 
                        {
                            //create hold for each store
                            RuleDAO dao = new RuleDAO();
                            Hold h;
                            foreach (StoreLookup s in dao.GetStoresInRuleSet(model.RuleSetID))
                            {
                                h = new Hold();
                                h.Store = s.Store;
                                h.Division = s.Division;
                                h.Comments = model.Hold.Comments;
                                h.Duration = model.Hold.Duration;
                                h.EndDate = model.Hold.EndDate;
                                h.HoldType = model.Hold.HoldType;
                                h.Level = model.Hold.Level;
                                h.ReserveInventory = model.Hold.ReserveInventory;
                                h.StartDate = model.Hold.StartDate;
                                h.Value = model.Hold.Value;
                                h.CreateDate = model.Hold.CreateDate;
                                h.CreatedBy = model.Hold.CreatedBy;
                                db.Holds.Add(h);
                                db.SaveChanges();

                                ApplyHoldsToExistingWebPicks(h);
                            }
                            return RedirectToAction("Index", new { duration = model.Hold.Duration });
                        }
                    }
                }
                else
                {
                    ViewData["message"] = "You are not authorized to create holds for this division.";
                    model.Divisions = this.Divisions();
                    return View(model);
                }
            }
        }

        private void ApplyHoldsToExistingWebPicks(Hold h)
        {
            RDQDAO rdqDAO = new RDQDAO();
            List<RDQ> list = rdqDAO.GetRDQsForHold(h.ID);
            Boolean needsave = false;
            foreach (RDQ rdq in list)
            {
                if (!(h.ReserveInventoryBool))
                {
                    needsave = true;
                    rdq.Status = "REJECTED";
                    db.Entry(rdq).State = System.Data.EntityState.Modified;
                }
                else if (rdq.Status.Contains("WEB PICK"))
                {
                    needsave = true;
                    rdq.Status = "HOLD-NEW";
                    db.Entry(rdq).State = System.Data.EntityState.Modified;
                }
            }
            if (needsave)
            {
                db.SaveChanges(UserName);
            }
        }

        public ActionResult Edit(int ID)
        {
            HoldModel model = new HoldModel();
            model.Hold = (from a in db.Holds where a.ID == ID select a).First();
            model.Divisions = this.Divisions();
            model.OriginalStartDate = model.Hold.StartDate;

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(HoldModel model)
        {
            string validationMessage = ValidateHold(model.Hold,false, true);
            if (model.OriginalStartDate > model.Hold.StartDate)
            {
                validationMessage = "Start date must be after original start date of hold";
            }
            if (validationMessage != "")
            {
                ViewData["message"] = validationMessage;
                model.Divisions = this.Divisions();
                return View(model);
            }
            else
            {
                model.Hold.CreateDate = DateTime.Now;
                model.Hold.CreatedBy = User.Identity.Name;

                db.Entry(model.Hold).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { duration = model.Hold.Duration });
            }
        }

        public ActionResult MassEdit(string div, string level, string value, string holdType)
        {
            HoldModel model = new HoldModel();
            if (holdType.Contains("Reserve"))
            {
                model.Hold = (from a in db.Holds where ((a.Division == div) && (a.Level == level) && (a.Value == value) && (a.ReserveInventory == 1)) select a).First();
            }
            else
            {
                model.Hold = (from a in db.Holds where ((a.Division == div) && (a.Level == level) && (a.Value == value) && (a.ReserveInventory == 0)) select a).First();
            }
            model.Hold.Comments = "";
            model.Divisions = this.Divisions();
            model.OriginalStartDate = model.Hold.StartDate;

            return View(model);
        }

        public ActionResult MassEditStore(string div, string store, string holdType)
        {
            HoldModel model = new HoldModel();
            if (holdType.Contains("Reserve"))
            {
                model.Hold = (from a in db.Holds where ((a.Division == div) && (a.Store == store) && (a.ReserveInventory == 1)) select a).First();
            }
            else
            {
                model.Hold = (from a in db.Holds where ((a.Division == div) && (a.Store == store) && (a.ReserveInventory == 0)) select a).First();
            }
            model.Hold.Comments = "";
            model.Divisions = this.Divisions();
            model.OriginalStartDate = model.Hold.StartDate;

            return View(model);
        }

        [HttpPost]
        public ActionResult MassEdit(HoldModel model)
        {
            List<Hold> holds;
            if (model.Hold.HoldType.Contains("Reserve"))
            {
                holds = (from a in db.Holds where ((a.Division == model.Hold.Division) && (a.Level == model.Hold.Level) && (a.Value == model.Hold.Value) && (a.ReserveInventory == 1)) select a).ToList();
            }
            else
            {
                holds = (from a in db.Holds where ((a.Division == model.Hold.Division) && (a.Level == model.Hold.Level) && (a.Value == model.Hold.Value) && (a.ReserveInventory == 0)) select a).ToList();
            }

            string validationMessage = "";
            List<Hold> updatedList = new List<Hold>();
            DateTime startdate;
            foreach (Hold h in holds)
            {
                startdate = h.StartDate;

                h.StartDate = model.Hold.StartDate;
                h.EndDate = model.Hold.EndDate;
                if (model.Hold.Comments != "")
                {
                    h.Comments = model.Hold.Comments;
                }

                string tempvalidationMessage = ValidateHold(h, false, true);
                if (startdate > model.Hold.StartDate)
                {
                    tempvalidationMessage = "Start date must be after original start date of hold for store " + h.Store + "<br>";
                }
               
                if (tempvalidationMessage == "")
                {
                    updatedList.Add(h);
                }

                validationMessage += tempvalidationMessage;
            }
            if (validationMessage != "")
            {
                ViewData["message"] = validationMessage;
                model.Divisions = this.Divisions();
                return View(model);
            }
            else
            {
                foreach (Hold uh in updatedList)
                {
                    uh.CreateDate = DateTime.Now;
                    uh.CreatedBy = User.Identity.Name;
                    db.Entry(uh).State = System.Data.EntityState.Modified;
                }
                db.SaveChanges();
                ViewData["message"] = "Successfully updated group of holds";
                return RedirectToAction("IndexByProduct");
            }
        }

        [HttpPost]
        public ActionResult MassEditStore(HoldModel model)
        {
            List<Hold> holds;
            if (model.Hold.HoldType.Contains("Reserve"))
            {
                holds = (from a in db.Holds where ((a.Division == model.Hold.Division) && (a.Store == model.Hold.Store) && (a.ReserveInventory == 1)) select a).ToList();
            }
            else
            {
                holds = (from a in db.Holds where ((a.Division == model.Hold.Division) && (a.Store == model.Hold.Store) && (a.ReserveInventory == 0)) select a).ToList();
            }

            string validationMessage = "";
            List<Hold> updatedList = new List<Hold>();
            DateTime startdate;
            foreach (Hold h in holds)
            {
                startdate = h.StartDate;

                h.StartDate = model.Hold.StartDate;
                h.EndDate = model.Hold.EndDate;
                if (model.Hold.Comments != "")
                {
                    h.Comments = model.Hold.Comments;
                }

                string tempvalidationMessage = ValidateHold(h, false, true);
                if (startdate > model.Hold.StartDate)
                {
                    tempvalidationMessage = "Start date must be after original start date of hold for store " + h.Store + "<br>";
                }

                if (tempvalidationMessage == "")
                {
                    updatedList.Add(h);
                }

                validationMessage += tempvalidationMessage;
            }
            if (validationMessage != "")
            {
                ViewData["message"] = validationMessage;
                model.Divisions = this.Divisions();
                return View(model);
            }
            else
            {
                foreach (Hold uh in updatedList)
                {
                    uh.CreateDate = DateTime.Now;
                    uh.CreatedBy = User.Identity.Name;
                    db.Entry(uh).State = System.Data.EntityState.Modified;
                }
                db.SaveChanges();
                ViewData["message"] = "Successfully updated group of holds";
                return RedirectToAction("IndexByStore");
            }
        }


        private string ValidateHold(Hold hold, Boolean usesRuleSet, Boolean edit)
        {
            string returnMessage = "";
            string value = hold.Value + "";

            AllocationContext db = new AllocationContext();
            DateTime controlDate = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == hold.Division select a.RunDate).First();

            returnMessage = CheckHoldPermission(hold);

            if (string.IsNullOrEmpty(returnMessage) && !edit)
            {
                if (hold.StartDate <= controlDate.AddDays(2))
                {
                    returnMessage = "Start date must be after " + controlDate.AddDays(2).ToShortDateString();
                }
            }

            if (string.IsNullOrEmpty(returnMessage))
            {
                if (hold.Level == "All")
                {
                    if ((from a in db.Holds where ((a.Division == hold.Division)&&(a.Store == hold.Store) && (a.Level == hold.Level) && (a.ID != hold.ID)) select a).Count() > 0)
                    {
                        returnMessage = "There is already a hold for " + hold.Store;
                    }
                    else
                    {
                        int divDepts = DepartmentService.ListDepartments(hold.Division).Count();  //db.Departments.Where(m => m.divisionCode == hold.Division).Count();
                        int enabledDepts = Departments().Where(m => m.DivCode == hold.Division).Count();
                        if (divDepts != enabledDepts)
                        {
                            returnMessage ="You do not have authority to create this hold.  Store level holds must have dept level access for ALL departments in the division.";
                        }
                    }
                }
                else if ((from a in db.Holds
                          where ((a.ID != hold.ID)&&
                          (a.Level == hold.Level) && (a.Value == hold.Value)
                         && ((a.Store == hold.Store)||((a.Store == null)&&(hold.Store == null)))) select a).Count() > 0)
                {
                    returnMessage = "There is already a hold for " + hold.Store + " " + hold.Level + " " + hold.Value;
                }
            }

            if (string.IsNullOrEmpty(returnMessage) && hold.EndDate != null)
            {
                if (hold.EndDate < hold.StartDate)
                {
                    returnMessage = "End Date must be after Start date.";
                }
            }

            if (string.IsNullOrEmpty(returnMessage) && (hold.Store != null) && (hold.Store.Length > 0))
            {
                if (hold.Store.Length > 5)
                {
                    returnMessage = "Store must be in format #####";
                }
                else
                {
                    hold.Store = hold.Store.PadLeft(5, '0');

                    StoreLookup lookupStore = (from s in db.StoreLookups
                                               where s.Store == hold.Store &&
                                                     s.Division == hold.Division
                                               select s).FirstOrDefault();
                    if (lookupStore == null)
                    {
                        returnMessage = "Store ID is not valid or it is not under the selected division";
                    }
                }
            }

            if (string.IsNullOrEmpty(returnMessage))
            {
                if (hold.Level == "Sku")
                {
                    System.Text.RegularExpressions.Regex regexSku =
                        new System.Text.RegularExpressions.Regex(@"^\d{2}-\d{2}-\d{5}-\d{2}$");
                    if (!(regexSku.IsMatch(value)))
                    {
                        returnMessage = "Invalid Sku, format should be ##-##-#####-##";
                    }

                    else if (
                        !(Footlocker.Common.WebSecurityService.UserHasDivision(UserName, "Allocation",
                            hold.Value.Substring(0, 2))))
                    {
                        returnMessage =
                            "You do not have authority to create this hold.  You need division level access.";
                    }
                    else if (
                        !(Footlocker.Common.WebSecurityService.UserHasDepartment(UserName, "Allocation",
                            hold.Value.Substring(0, 2), hold.Value.Substring(3, 2))))
                    {
                        returnMessage =
                            "You do not have authority to create this hold.  You need dept level access.";
                    }
                    else if ((hold.Level == "Sku") && (hold.Division != hold.Value.Substring(0, 2)))
                    {
                        returnMessage = "Invalid Sku, division does not match selection.";
                    }
                }
                else if (hold.Level == "Dept")
                {
                    System.Text.RegularExpressions.Regex regexDept = new System.Text.RegularExpressions.Regex(@"^\d{2}$");
                    if (!(regexDept.IsMatch(value)))
                    {
                        returnMessage = "Invalid Department, format should be ##";
                    }
                    else if (
                        !(Footlocker.Common.WebSecurityService.UserHasDepartment(UserName, "Allocation",
                            hold.Division, hold.Value)))
                    {
                        returnMessage =
                            "You do not have authority to create this hold.  You need dept level access.";
                    }
                    else if ((hold.Store == null) || (hold.Store.Trim() == ""))
                    {
                        if (!(usesRuleSet))
                        {
                            returnMessage = "You must specify one or more stores.";
                        }
                    }
                }
                else if (hold.Level == "VendorDept")
                {
                    System.Text.RegularExpressions.Regex regexSku =
                        new System.Text.RegularExpressions.Regex(@"^\d{5}-\d{2}$");
                    if (!(regexSku.IsMatch(value)))
                    {
                        returnMessage = "Invalid Vendor-Dept, format should be #####-##";
                    }
                    else if (
                        !(Footlocker.Common.WebSecurityService.UserHasDepartment(UserName, "Allocation",
                            hold.Division, hold.Value.Substring(6, 2))))
                    {
                        returnMessage =
                            "You do not have authority to create this hold.  You need dept level access.";
                    }
                }
                else if (hold.Level == "VendorDeptCategory")
                {
                    System.Text.RegularExpressions.Regex regexSku =
                        new System.Text.RegularExpressions.Regex(@"^\d{5}-\d{2}-\d{3}$");
                    if (!(regexSku.IsMatch(value)))
                    {
                        returnMessage = "Invalid Vendor-Dept-Category, format should be #####-##-###";
                    }
                    else if (
                        !(Footlocker.Common.WebSecurityService.UserHasDepartment(UserName, "Allocation",
                            hold.Division, hold.Value.Substring(6, 2))))
                    {
                        returnMessage =
                            "You do not have authority to create this hold.  You need dept level access.";
                    }
                }
                else if (hold.Level == "DeptBrand")
                {
                    System.Text.RegularExpressions.Regex regexSku =
                        new System.Text.RegularExpressions.Regex(@"^\d{2}-\d{3}$");
                    if (!(regexSku.IsMatch(value)))
                    {
                        returnMessage = "Invalid Dept-Brand, format should be ##-###";
                    }
                    else if (
                        !(Footlocker.Common.WebSecurityService.UserHasDepartment(UserName, "Allocation",
                            hold.Division, hold.Value.Substring(0, 2))))
                    {
                        returnMessage =
                            "You do not have authority to create this hold.  You need dept level access.";
                    }
                    else if ((hold.Store == null) || (hold.Store.Trim() == ""))
                    {
                        if (!(usesRuleSet))
                        {
                            returnMessage = "You must specify one or more stores.";
                        }
                    }
                }
                else if (hold.Level == "Category") //category
                {
                    System.Text.RegularExpressions.Regex regex =
                        new System.Text.RegularExpressions.Regex(@"^\d{2}-\d{3}$");
                    if (!(regex.IsMatch(value)))
                    {
                        returnMessage = "Invalid Category, format should be ##-###";
                    }
                    else if (
                        !(Footlocker.Common.WebSecurityService.UserHasDepartment(UserName, "Allocation",
                            hold.Division, hold.Value.Substring(0, 2))))
                    {
                        returnMessage =
                            "You do not have authority to create this hold.  You need dept level access.";
                    }
                    else if ((hold.Store == null) || (hold.Store.Trim() == ""))
                    {
                        if (!(usesRuleSet))
                        {
                            returnMessage = "You must specify one or more stores.";
                        }
                    }

                }
                else if (String.Equals(hold.Level, "DeptTeam"))
                {
                    var regex = new System.Text.RegularExpressions.Regex(@"^\d{2}-\d{3}$");
                    if (!(regex.IsMatch(value)))
                    {
                        returnMessage = "Invalid Dept-Team, format should be ##-###";
                    }
                    else if (
                        !(Footlocker.Common.WebSecurityService.UserHasDepartment(UserName, "Allocation",
                            hold.Division, hold.Value.Substring(0, 2))))
                    {
                        returnMessage =
                            "You do not have authority to create this hold.  You need dept level access.";
                    }
                    else if ((hold.Store == null) || (hold.Store.Trim() == ""))
                    {
                        if (!(usesRuleSet))
                        {
                            returnMessage = "You must specify one or more stores.";
                        }
                    }
                }
                else if (String.Equals(hold.Level, "DeptCatTeam"))
                {
                    var regex = new System.Text.RegularExpressions.Regex(@"^\d{2}-\d{3}-\d{3}$");
                    if (!(regex.IsMatch(value)))
                    {
                        returnMessage = "Invalid Dept-Cat-Team, format should be ##-###-###";
                    }
                    else if (
                        !(Footlocker.Common.WebSecurityService.UserHasDepartment(UserName, "Allocation",
                            hold.Division, hold.Value.Substring(0, 2))))
                    {
                        returnMessage =
                            "You do not have authority to create this hold.  You need dept level access.";
                    }
                    else if ((hold.Store == null) || (hold.Store.Trim() == ""))
                    {
                        if (!(usesRuleSet))
                        {
                            returnMessage = "You must specify one or more stores.";
                        }
                    }
                }
                else if (String.Equals(hold.Level, "DeptCatBrand"))
                {
                    var regex = new System.Text.RegularExpressions.Regex(@"^\d{2}-\d{3}-\d{3}$");
                    if (!(regex.IsMatch(value)))
                    {
                        returnMessage = "Invalid Dept-Cat-Brand, format should be ##-###-###";
                    }
                    else if (
                        !(Footlocker.Common.WebSecurityService.UserHasDepartment(UserName, "Allocation",
                            hold.Division, hold.Value.Substring(0, 2))))
                    {
                        returnMessage =
                            "You do not have authority to create this hold.  You need dept level access.";
                    }
                    else if ((hold.Store == null) || (hold.Store.Trim() == ""))
                    {
                        if (!(usesRuleSet))
                        {
                            returnMessage = "You must specify one or more stores.";
                        }
                    }
                }
                else
                {
                    //store hold
                    if ((hold.Store == null) || (hold.Store.Trim() == ""))
                    {
                        if (!(usesRuleSet))
                        {
                            returnMessage = "For Store level, you must specify store.";
                        }
                    }
                    hold.Value = "N/A";
                }
            }

            if (string.IsNullOrEmpty(returnMessage) && hold.ReserveInventory == 0)
            {
                RDQDAO dao = new RDQDAO();
                if (dao.GetRDQsForHold(hold.ID).Count > 0)
                {
                    //if it was cancel inventory before, then let it be
                    var reserveinventory = (from a in db.Holds where a.ID == hold.ID select a.ReserveInventory).First();
                    if ((reserveinventory != null)&&(reserveinventory > 0))
                    {
                        returnMessage = "RDQs already assigned to this hold, you must release the RDQs before setting this to Cancel Inventory";
                    }
                }

            }

            return returnMessage;
        }


        public ActionResult ReleaseRDQs(int ID)
        {
            Hold model = (from a in db.Holds where a.ID == ID select a).First();
            DeleteHoldModel dh = new DeleteHoldModel();
            dh.Hold = model;
            ViewData["holdID"] = ID;
            RDQDAO dao = new RDQDAO();
            
            //probably want stored proc so we can join for category
            dh.CurrentRDQs = GetRDQsForSession(ID);

            HoldRelease hr = new HoldRelease();
            dh.HoldReleases = new List<HoldRelease>();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);

            return View(dh);
        }

        public ActionResult MassReleaseRDQs(string div, string level, string value, string holdType)
        {
            List<Hold> holds = new List<Hold>();
            if (holdType.Contains("Reserve"))
            {
                holds = (from a in db.Holds where ((a.Division == div) && (a.Level == level) && (a.Value == value) && (a.ReserveInventory == 1)) select a).ToList();
            }
            else
            {
                holds = (from a in db.Holds where ((a.Division == div) && (a.Level == level) && (a.Value == value) && (a.ReserveInventory == 0)) select a).ToList();
            }
            DeleteHoldModel dh = new DeleteHoldModel();
            dh.Hold = holds.First();
            RDQDAO dao = new RDQDAO();
            dh.CurrentRDQs = GetRDQsForSession(div, level, value);
            dh.NumberOfHolds = holds.Count();
            ViewData["holdID"] = dh.Hold.ID;

            HoldRelease hr = new HoldRelease();
            dh.HoldReleases = new List<HoldRelease>();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);

            return View(dh);
        }

        public ActionResult MassReleaseRDQsStore(string div, string store, string holdType)
        {
            List<Hold> holds = new List<Hold>();
            if (holdType.Contains("Reserve"))
            {
                holds = (from a in db.Holds where ((a.Division == div) && (a.Store == store) && (a.ReserveInventory == 1)) select a).ToList();
            }
            else
            {
                holds = (from a in db.Holds where ((a.Division == div) && (a.Store == store) && (a.ReserveInventory == 0)) select a).ToList();
            }
            DeleteHoldModel dh = new DeleteHoldModel();
            dh.Hold = holds.First();
            RDQDAO dao = new RDQDAO();
            dh.CurrentRDQs = GetRDQsForSession(div, store);
            dh.NumberOfHolds = holds.Count();
            ViewData["holdID"] = dh.Hold.ID;

            HoldRelease hr = new HoldRelease();
            dh.HoldReleases = new List<HoldRelease>();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);

            return View(dh);
        }

        public ActionResult ConfirmDelete(int ID)
        {
            Hold hold = (from a in db.Holds where a.ID == ID select a).First();
            if (hold.ReserveInventoryBool)
            {
                RDQDAO dao = new RDQDAO();
                if (dao.GetUniqueRDQsForHold(hold.ID).Count > 0)
                {
                    return RedirectToAction("Index", new { duration = hold.Duration, message = "You must release all RDQs before you can delete this hold." });
                }
            }

            db.Holds.Remove(hold);
            db.SaveChanges();
            return RedirectToAction("Index", new { duration = hold.Duration });
        }

        public ActionResult MassDelete(string div, string level, string value, string holdType)
        {
            List<Hold> holds = new List<Hold>();
            if (holdType.Contains("Reserve"))
            {
                holds = (from a in db.Holds where ((a.Division == div) && (a.Level == level) && (a.Value == value) && (a.ReserveInventory == 1)) select a).ToList();
            }
            else
            {
                holds = (from a in db.Holds where ((a.Division == div) && (a.Level == level) && (a.Value == value) && (a.ReserveInventory == 0)) select a).ToList();
            }
            foreach (Hold hold in holds)
            {
                if (hold.ReserveInventoryBool)
                {
                    RDQDAO dao = new RDQDAO();
                    if (dao.GetUniqueRDQsForHold(hold.ID).Count > 0)
                    {
                        return RedirectToAction("IndexByProduct", new { duration = hold.Duration, message = "You must release all RDQs before you can delete this group of holds." });
                    }
                }
            }
            foreach (Hold hold in holds)
            {
                db.Holds.Remove(hold);
            }
            db.SaveChanges();
            return RedirectToAction("IndexByProduct");
        }

        public ActionResult MassDeleteStore(string div, string store, string holdType)
        {
            List<Hold> holds = new List<Hold>();
            if (holdType.Contains("Reserve"))
            {
                holds = (from a in db.Holds where ((a.Division == div) && (a.Store == store) && (a.ReserveInventory == 1)) select a).ToList();
            }
            else
            {
                holds = (from a in db.Holds where ((a.Division == div) && (a.Store == store) && (a.ReserveInventory == 0)) select a).ToList();
            }
            foreach (Hold hold in holds)
            {
                if (hold.ReserveInventoryBool)
                {
                    RDQDAO dao = new RDQDAO();
                    if (dao.GetUniqueRDQsForHold(hold.ID).Count > 0)
                    {
                        return RedirectToAction("IndexByStore", new { duration = hold.Duration, message = "You must release all RDQs before you can delete this group of holds." });
                    }
                }
            }
            foreach (Hold hold in holds)
            {
                db.Holds.Remove(hold);
            }
            db.SaveChanges();
            return RedirectToAction("IndexByStore");
        }


        [HttpPost]
        public ActionResult MassReleaseAllRDQsToWarehouse()
        {
            using (db)
            {
                // Get all rdqs for specified hold
                // TODO: Should probably relate RDQs to the Hold in EF mapping, and load all via context, rather than sproc....
                var dao = new RDQDAO();

                var rdqs = GetRDQsInSession();

                foreach (RDQ rdq in rdqs)
                {
                    // Load rdq (so able to be removed from context)
                    var contextRDQ = db.RDQs.Find(rdq.ID);

                    //delete rdq
                    db.RDQs.Remove(contextRDQ);
                }

                // Persist changes
                db.SaveChanges(User.Identity.Name);
            }

            Session["rdqgrouplist"] = null;
            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult ReleaseAllRDQsToWarehouse(int ID)
        {
            using (db)
            {
                // Get all rdqs for specified hold
                // TODO: Should probably relate RDQs to the Hold in EF mapping, and load all via context, rather than sproc....
                var dao = new RDQDAO();
                var rdqs = dao.GetUniqueRDQsForHold(ID);

                foreach (RDQ rdq in rdqs)
                {
                    // Load rdq (so able to be removed from context)
                    var contextRDQ = db.RDQs.Find(rdq.ID);

                    //delete rdq
                    db.RDQs.Remove(contextRDQ);
                }

                // Persist changes
                db.SaveChanges(User.Identity.Name);
            }

            Session["holdrdq"] = -1;
            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult ReleaseAllRDQs(int ID)
        {
            using (db)
            {
                // Get all rdqs for specified hold
                // TODO: Should probably relate RDQs to the Hold in EF mapping, and load all via context, rather than sproc....
                var dao = new RDQDAO();
                var rdqs = dao.GetUniqueRDQsForHold(ID);

                foreach (RDQ rdq in rdqs)
                {
                    // Load rdq (so we can update the status)
                    var contextRDQ = db.RDQs.Find(rdq.ID);
                    contextRDQ.Status = "HOLD-REL";

                    if ((rdq.PO != null) && (rdq.PO != "") && (rdq.PO != "N/A") && (rdq.Size.Length == 5))
                    {
                        rdq.DestinationType = "CROSSDOCK";
                    }
                    else
                    {
                        rdq.DestinationType = "WAREHOUSE";
                    }
                    db.Entry(contextRDQ).State = System.Data.EntityState.Modified;
                }

                // Persist changes
                db.SaveChanges(User.Identity.Name);
            }

            Session["holdrdq"] = -1;
            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult MassReleaseAllRDQs()
        {
            using (db)
            {
                // Get all rdqs for specified hold
                // TODO: Should probably relate RDQs to the Hold in EF mapping, and load all via context, rather than sproc....
                var dao = new RDQDAO();

                var rdqs = GetRDQsInSession();

                foreach (RDQ rdq in rdqs)
                {
                    // Load rdq (so able to be removed from context)
                    var contextRDQ = db.RDQs.Find(rdq.ID);

                    contextRDQ.Status = "HOLD-REL";

                    if ((rdq.PO != null) && (rdq.PO != "") && (rdq.PO != "N/A") && (rdq.Size.Length == 5))
                    {
                        rdq.DestinationType = "CROSSDOCK";
                    }
                    else
                    {
                        rdq.DestinationType = "WAREHOUSE";
                    }
                    db.Entry(contextRDQ).State = System.Data.EntityState.Modified;
                }

                // Persist changes
                db.SaveChanges(User.Identity.Name);
            }

            Session["rdqgrouplist"] = null;
            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }


        [HttpPost]
        public ActionResult ReleaseRDQGroupToWarehouse(RDQGroup rdqGroup, int holdID)
        {
            // Get all RDQs of specified SKU for specified hold
            var dao = new RDQDAO();
            var holdRDQs = GetRDQsForSession(holdID);
            var groupRDQs = holdRDQs.Where(rdq =>
                rdq.Division == rdqGroup.Division
                && rdq.Store == rdqGroup.Store
                && rdq.WarehouseName == rdqGroup.WarehouseName
                && rdq.Category == rdqGroup.Category
                && rdq.Sku == rdqGroup.Sku)
                .ToList();

            //performance was really bad via entity framework, we'll just run a quick stored proc and update records in memory
            dao.DeleteRDQs(groupRDQs, UserName);

            groupRDQs.ForEach(rdq => holdRDQs.Remove(rdq));

            Session["holdrdqlist"] = holdRDQs;
            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult ReleaseRDQGroup(RDQGroup rdqGroup, int holdID)
        {
            // Get all RDQs of specified SKU for specified hold
            var holdRDQs = GetRDQsForSession(holdID);
            var groupRDQs = holdRDQs.Where(rdq =>
                rdq.Division == rdqGroup.Division
                && rdq.Store == rdqGroup.Store
                && rdq.WarehouseName == rdqGroup.WarehouseName
                && rdq.Category == rdqGroup.Category
                && rdq.Sku == rdqGroup.Sku)
                .ToList();

            //performance was really bad via entity framework, we'll just run a quick stored proc and update records in memory
            RDQDAO dao = new RDQDAO();
            dao.ReleaseRDQs(groupRDQs, UserName);

            groupRDQs.ForEach(rdq => holdRDQs.Remove(rdq));
            Session["holdrdqlist"] = holdRDQs;

            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult MassReleaseRDQGroupToWarehouse(RDQGroup rdqGroup)
        {
            // Get all RDQs of specified SKU for specified hold
            var dao = new RDQDAO();
            var holdRDQs = GetRDQsInSession();
            var groupRDQs = holdRDQs.Where(rdq =>
                rdq.Division == rdqGroup.Division
                && rdq.Store == rdqGroup.Store
                && rdq.WarehouseName == rdqGroup.WarehouseName
                && rdq.Category == rdqGroup.Category
                && rdq.Sku == rdqGroup.Sku)
                .ToList();

            //performance was really bad via entity framework, we'll just run a quick stored proc and update records in memory
            dao.DeleteRDQs(groupRDQs, UserName);

            groupRDQs.ForEach(rdq => holdRDQs.Remove(rdq));

            Session["rdqgrouplist"] = holdRDQs;
            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }


        [HttpPost]
        public ActionResult MassReleaseRDQGroup(RDQGroup rdqGroup)
        {
            // Get all RDQs of specified SKU for specified hold
            var holdRDQs = GetRDQsInSession();
            var groupRDQs = holdRDQs.Where(rdq =>
                rdq.Division == rdqGroup.Division
                && rdq.Store == rdqGroup.Store
                && rdq.WarehouseName == rdqGroup.WarehouseName
                && rdq.Category == rdqGroup.Category
                && rdq.Sku == rdqGroup.Sku)
                .ToList();

            //performance was really bad via entity framework, we'll just run a quick stored proc and update records in memory
            RDQDAO dao = new RDQDAO();
            dao.ReleaseRDQs(groupRDQs, UserName);

            groupRDQs.ForEach(rdq => holdRDQs.Remove(rdq));
            Session["rdqgrouplist"] = holdRDQs;

            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpGet]
        public ActionResult GetRDQTotalQtyForHold(int holdID)
        {
            // Get sum of qtys of rdqs for specified hold
            var dao = new RDQDAO();
            var rdqTotalQty = dao.GetRDQsForHold(holdID).ToList().Sum(r => r.Qty);

            // Return JSON representing Success
            return new JsonResult() { 
                Data = new JsonResultData(ActionResultCode.Success) { Data = rdqTotalQty }, 
                JsonRequestBehavior = JsonRequestBehavior.AllowGet 
            };
        }

        [HttpGet]
        public ActionResult GetMassRDQTotalQtyForHolds()
        {
            // Get sum of qtys of rdqs for specified hold
            var dao = new RDQDAO();
            var rdqTotalQty = GetRDQsInSession().ToList().Sum(r => r.Qty);

            // Return JSON representing Success
            return new JsonResult() { 
                Data = new JsonResultData(ActionResultCode.Success) { Data = rdqTotalQty }, 
                JsonRequestBehavior = JsonRequestBehavior.AllowGet 
            };
        }

        
        // ------------------------------------------------------------------------------------------------------------------
        // OLD: These actions were used to release RDQs at the RDQ level (item/size), not at the RDQGroup level (item) which is the current UI design...
        // NOTE: Leaving this code commented out as an artifact and point to come back to in case the user's change their mind on the UI design...
        // ------------------------------------------------------------------------------------------------------------------
        //[GridAction]
        //public ActionResult ReleaseRDQToWarehouse(int ID, int holdID)
        //{
        //    //to release back to the warehouse all we need to do is delete it
        //    //the result will be that we no longer decrease the inventory we send to Q, 
        //    //so it will see more available to allocate to whoever it would like

        //    RDQ rdq = (from a in db.RDQs where a.ID == ID select a).First();

        //    db.RDQs.Remove(rdq);
        //    db.SaveChanges();
        //    RDQDAO dao = new RDQDAO();
        //    return PartialView(new GridModel(dao.GetRDQsForHold(holdID)));
        //    //return RedirectToAction("Delete", new { id = holdID });
        //}

        //[GridAction]
        //public ActionResult ReleaseRDQ(int ID, int holdID)
        //{
        //    //they are releasing the RDQ, so we'll make it a user RDQ so that the hold won't apply and it will be picked the next pick day

        //    //first, find the RDQ being held.
        //    RDQ rdq = (from a in db.RDQs where a.ID == ID select a).First();

        //    //now update it to a user RDQ, so that it will go down with the next batch
        //    rdq.Type = "user";
        //    //set to WEB PICK so that it will pick on the stores next pick day
        //    rdq.Status = "WEB PICK";
        //    rdq.CreateDate = DateTime.Now;
        //    rdq.CreatedBy = User.Identity.Name;

        //    if ((rdq.PO != null) && (rdq.PO != "") && (rdq.PO != "N/A") && (rdq.Size.Length == 5))
        //    {
        //        rdq.DestinationType = "CROSSDOCK";
        //    }
        //    else
        //    {
        //        rdq.DestinationType = "WAREHOUSE";
        //    }

        //    db.SaveChanges();

        //    RDQDAO dao = new RDQDAO();
        //    return PartialView(new GridModel(dao.GetRDQsForHold(holdID)));
        //}
        // ---------------------------------------------------------------------------------------------------------------------------------------------------

        private List<RDQ> GetRDQsInSession()
        {
            if (Session["rdqgrouplist"] != null)
            {
                return (List<RDQ>)Session["rdqgrouplist"];
            }
            else
            {
                return new List<RDQ>();
            }
        }

        private List<RDQ> GetRDQsForSession(string div, string level, string value)
        {
            List<RDQ> model;
            if ((Session["rdqgrouplist"] != null) &&
                ((String)Session["holdgrouprdq"] == div + "|" + level + "|" + value))
            {
                model = (List<RDQ>)Session["rdqgrouplist"];
            }
            else
            {
                RDQDAO dao = new RDQDAO();

                model = dao.GetRDQsForHolds(div, level, value);

                List<RDQ> updateList = new List<RDQ>();
                List<string> caselots = new List<string>();
                foreach (RDQ r in model)
                {
                    if (r.Size.Length > 3)
                    {
                        updateList.Add(r);
                        caselots.Add(r.Size);
                    }
                    else
                    {
                        r.UnitQty = r.Qty;
                    }
                }

                List<ItemPack> qtyPerCase = db.ItemPacks.Where(p => caselots.Contains(p.Name)).ToList();
                foreach (RDQ r in updateList)
                {
                    try
                    {
                        r.UnitQty = (from a in qtyPerCase where a.Name == r.Size select a.TotalQty).First() * r.Qty;
                    }
                    catch
                    {
                        //    //don't know per qty, so we'll just leave blank
                    }
                }

                //model = list.Select(x => new RDQ(x)).ToList();

                Session["holdgrouprdq"] = div + "|" + level + "|" + value;
                Session["rdqgrouplist"] = model;
            }
            return model;

        }

        private List<RDQ> GetRDQsForSession(string div, string store)
        {
            List<RDQ> model;
            if ((Session["rdqgrouplist"] != null) &&
                ((String)Session["holdgrouprdq"] == div + "|" + store))
            {
                model = (List<RDQ>)Session["rdqgrouplist"];
            }
            else
            {
                RDQDAO dao = new RDQDAO();

                model = dao.GetRDQsForHolds(div, store);

                List<RDQ> updateList = new List<RDQ>();
                List<string> caselots = new List<string>();
                foreach (RDQ r in model)
                {
                    if (r.Size.Length > 3)
                    {
                        updateList.Add(r);
                        caselots.Add(r.Size);
                    }
                    else
                    {
                        r.UnitQty = r.Qty;
                    }
                }

                List<ItemPack> qtyPerCase = db.ItemPacks.Where(p => caselots.Contains(p.Name)).ToList();
                foreach (RDQ r in updateList)
                {
                    try
                    {
                        r.UnitQty = (from a in qtyPerCase where a.Name == r.Size select a.TotalQty).First() * r.Qty;
                    }
                    catch
                    {
                        //    //don't know per qty, so we'll just leave blank
                    }
                }

                //model = list.Select(x => new RDQ(x)).ToList();

                Session["holdgrouprdq"] = div + "|" + store;
                Session["rdqgrouplist"] = model;
            }
            return model;

        }


        private List<RDQ> GetRDQsForSession(long holdID)
        {
            List<RDQ> model;
            if ((Session["holdrdq"] != null) &&
                ((Int64)Session["holdrdq"] == holdID))
            {
                model = (List<RDQ>)Session["holdrdqlist"];
            }
            else
            {
                RDQDAO dao = new RDQDAO();

                model = dao.GetRDQsForHold(holdID);

                List<RDQ> updateList = new List<RDQ>();
                List<string> caselots = new List<string>();
                foreach (RDQ r in model)
                {
                    if (r.Size.Length > 3)
                    {
                        updateList.Add(r);
                        caselots.Add(r.Size);
                    }
                    else
                    {
                        r.UnitQty = r.Qty;
                    }
                }

                List<ItemPack> qtyPerCase = db.ItemPacks.Where(p => caselots.Contains(p.Name)).ToList();
                foreach (RDQ r in updateList)
                {
                    try
                    {
                        r.UnitQty = (from a in qtyPerCase where a.Name == r.Size select a.TotalQty).First() * r.Qty;
                    }
                    catch
                    {
                        //    //don't know per qty, so we'll just leave blank
                    }
                }

                //model = list.Select(x => new RDQ(x)).ToList();

                Session["holdrdq"] = holdID;
                Session["holdrdqlist"] = model;
            }
            return model;
        }


        [GridAction]
        public ActionResult _RDQs(Int32 holdID)
        {
            ViewData["holdID"] = holdID;
            RDQDAO dao = new RDQDAO();

            List<RDQ> list = GetRDQsForSession(holdID);

            // Hit db for RDQs for specified hold, aggregate in memory to level (defined by users)
            var rdqGroups =
                        from rdq in list
                        group rdq by new
                        {
                            Division = rdq.Division,
                            Store = rdq.Store,
                            WarehouseName = rdq.WarehouseName,
                            Category = rdq.Category,
                            ItemID = rdq.ItemID,
                            Sku = rdq.Sku
                        } into g
                        select new RDQGroup()
                        {
                            Division = g.Key.Division,
                            Store = g.Key.Store,
                            WarehouseName = g.Key.WarehouseName,
                            Category = g.Key.Category,
                            ItemID = Convert.ToInt64(g.Key.ItemID),
                            Sku = g.Key.Sku,
                            IsBin = g.Where(r => r.Size.Length > _BIN_SIZE_VALUE_LENGTH).Any() ? false : true,
                            Qty = g.Sum(r => r.Qty),
                            UnitQty = g.Sum(r => r.UnitQty)
                        };

            return PartialView(new GridModel(
                rdqGroups.OrderBy(g => g.Division)
                    .ThenBy(g => g.Store)
                    .ThenBy(g => g.WarehouseName)
                    .ThenBy(g => g.Category)
                    .ThenBy(g => g.Sku)));
        }

        [GridAction]
        public ActionResult _RDQsMass()
        {

            List<RDQ> list = GetRDQsInSession();

            // Hit db for RDQs for specified hold, aggregate in memory to level (defined by users)
            var rdqGroups =
                        from rdq in list
                        group rdq by new
                        {
                            Division = rdq.Division,
                            Store = rdq.Store,
                            WarehouseName = rdq.WarehouseName,
                            Category = rdq.Category,
                            ItemID = rdq.ItemID,
                            Sku = rdq.Sku
                        } into g
                        select new RDQGroup()
                        {
                            Division = g.Key.Division,
                            Store = g.Key.Store,
                            WarehouseName = g.Key.WarehouseName,
                            Category = g.Key.Category,
                            ItemID = Convert.ToInt64(g.Key.ItemID),
                            Sku = g.Key.Sku,
                            IsBin = g.Where(r => r.Size.Length > _BIN_SIZE_VALUE_LENGTH).Any() ? false : true,
                            Qty = g.Sum(r => r.Qty),
                            UnitQty = g.Sum(r => r.UnitQty)
                        };

            return PartialView(new GridModel(
                rdqGroups.OrderBy(g => g.Division)
                    .ThenBy(g => g.Store)
                    .ThenBy(g => g.WarehouseName)
                    .ThenBy(g => g.Category)
                    .ThenBy(g => g.Sku)));
        }


        [HttpPost]
        public ActionResult DeleteReleaseTo(DeleteHoldModel model)
        {
            //TODO:  Create E-Pick to each store

            db.Holds.Remove(model.Hold);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
