using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Common;
using Telerik.Web.Mvc;
using System.Data.Objects;
using Footlocker.Logistics.Allocation.Services;
using System.Data.Entity;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Director of Allocation,VP of Allocation,Admin,Support")]
    public class ProductHierarchyOverrideController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();        
        AllocationLibraryContext context = new Services.AllocationLibraryContext();

        #region ActionResult handlers
        //
        // GET: /ProductHierarchyOverride/

        public ActionResult Index(string message)
        {
            ViewData["message"] = message;

            List<Division> userDivList = currentUser.GetUserDivisions(AppName);

            List<ProductHierarchyOverrides> model = (from p in context.ProductOverrides.Include(p => p.productOverrideType)
                        select p).ToList();

            var filteredModel = from a in model
                                join u in userDivList
                                on a.overrideDivision equals u.DivCode
                                select a;
            
            List<string> users = (from a in filteredModel
                                  select a.lastModifiedUser).Distinct().ToList();

            Dictionary<string, string> names = LoadUserNames(users);

            foreach (var item in filteredModel)
            {
                item.lastModifiedUserName = names[item.lastModifiedUser];
            }

            return View(filteredModel);
        }

        //
        // GET: /ProductHierarchyOverride/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /ProductHierarchyOverride/Create

        public ActionResult Create()
        {
            ProductHierarchyOverrideModel model = new ProductHierarchyOverrideModel();
            model.prodHierarchyOverride.effectiveFromDt = getCRCDate();

            model = FillModelLists(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductHierarchyOverrideModel model, string submitAction)
        {
            string errorMessage = "";
            try
            {
                if (submitAction.Equals("lookup"))
                {
                    model = Lookup(model);
                    model = FillModelLists(model);
                    return View(model);
                }
                else
                {
                    List<SelectListItem> seasonalityLevels = GetSeasonalityLevels();
                    string level = seasonalityLevels.Where(sl => sl.Value == model.selectedInstance).FirstOrDefault().Text;
                    if (level == "Category")
                    {
                        model.prodHierarchyOverride.newBrandID = null;
                    }

                    if (model.prodHierarchyOverride.productOverrideTypeCode == "SKU")
                        model = Lookup(model);

                    model.prodHierarchyOverride = PopulateFields(model.prodHierarchyOverride);

                    if (!ValidateOverride(model.prodHierarchyOverride, level, out errorMessage))
                    {
                        ViewData["message"] = errorMessage;
                        model = FillModelLists(model);
                        return View(model);
                    }

                    ProductHierarchyOverrides newRec = PopulateFields(model.prodHierarchyOverride);

                    db.ProductHierarchyOverrides.Add(newRec);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            catch
            {
                ViewData["message"] = "An unexpected error has occured.";
                model = FillModelLists(model);
                return View(model);
            }
        }

        public ActionResult Edit(int id)
        {
            ProductHierarchyOverrideModel model = new ProductHierarchyOverrideModel();
            model.prodHierarchyOverride = LoadTransaction(id);
            model.selectedInstance = (from eid in db.InstanceDivisions
                                      where eid.Division == model.prodHierarchyOverride.newDivision
                                      select eid.InstanceID).FirstOrDefault().ToString();
            string errorMessage = "";

            List<SelectListItem> seasonalityLevels = GetSeasonalityLevels();
            string level = seasonalityLevels.Where(sl => sl.Value == model.selectedInstance).FirstOrDefault().Text;
            {
                model.prodHierarchyOverride.newBrandID = null;
            }

            if (!ValidateExistingOverride(model, out errorMessage))
            {
                //display error message 
                ViewData["message"] = errorMessage;
                //populate fields
                model = FillModelLists(model);
                // populate the SKU labels if the SKU is valid
                if (model.prodHierarchyOverride.productOverrideTypeCode == "SKU")
                    model = Lookup(model);

                return View(model);
            }

            if (model.prodHierarchyOverride.productOverrideTypeCode == "SKU")
                model = Lookup(model);

            model = FillModelLists(model);
            return View(model);
        }

        //
        // POST: /ProductHierarchyOverride/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductHierarchyOverrideModel model, int id, string submitAction)
        {
            string errorMessage = "";
            List<SelectListItem> seasonalityLevels = GetSeasonalityLevels();
            model.selectedInstance = (from eid in db.InstanceDivisions
                                      where eid.Division == model.prodHierarchyOverride.overrideDivision
                                      select eid.InstanceID).FirstOrDefault().ToString();
            string level = seasonalityLevels.Where(sl => sl.Value == model.selectedInstance).FirstOrDefault().Text;

            if (level == "Category")
            {
                model.prodHierarchyOverride.newBrandID = null;
            }

            if (submitAction.Equals("lookup"))
            {
                model = Lookup(model);
                model = FillModelLists(model);
                
                if (!ValidateOverride(model.prodHierarchyOverride, level, out errorMessage))
                {
                    ViewData["message"] = errorMessage;  
                }
                return View(model);
            }
            else
            {
                if (model.prodHierarchyOverride.productOverrideTypeCode == "SKU")
                    model = Lookup(model);

                ProductHierarchyOverrides editedRec = PopulateFields(model.prodHierarchyOverride);
                editedRec.productHierarchyOverrideID = id;

                if (!ValidateOverride(model.prodHierarchyOverride, level, out errorMessage))
                {
                    ViewData["message"] = errorMessage;
                    model = FillModelLists(model);
                    return View(model);
                }

                db.ProductHierarchyOverrides.Attach(editedRec);
                db.Entry(editedRec).State = System.Data.EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        //
        // GET: /ProductHierarchyOverride/Delete/5

        public ActionResult Delete(int id)
        {
            var deleteRec = new ProductHierarchyOverrides { productHierarchyOverrideID = id };
            db.ProductHierarchyOverrides.Attach(deleteRec);
            db.ProductHierarchyOverrides.Remove(deleteRec);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        #endregion

        #region SelectListItem routines
        public List<SelectListItem> GetOverrideTypes()
        {
            List<SelectListItem> overrideTypeList = new List<SelectListItem>();
            var pot = db.ProductOverrideTypes.ToList();
            var sortedTypes = pot.OrderBy(p => p.sortValue).ToList();

            if (sortedTypes.Count() > 0)
            {
                foreach (var rec in sortedTypes)
                {
                    overrideTypeList.Add(new SelectListItem { Text = rec.productOverrideTypeDesc, Value = rec.productOverrideTypeCode });
                }
            }

            return overrideTypeList;
        }

        public List<SelectListItem> GetDivisionList(string instanceIDFilter)
        {
            List<SelectListItem> divisionList = new List<SelectListItem>();
            List<string> instanceDivisions = new List<string>();

            if (!string.IsNullOrEmpty(instanceIDFilter))
            {
                int instanceID = Convert.ToInt32(instanceIDFilter);
                instanceDivisions = (from id in db.InstanceDivisions
                                     where id.InstanceID == instanceID
                                     select id.Division).ToList();
            }

            List<Division> userDivList = currentUser.GetUserDivisions(AppName);
            var divsWithDepts = (from a in db.Departments 
                                 select a.divisionCode).Distinct();

            if (userDivList.Count() > 0)
            {
                foreach (var rec in userDivList)
                {
                    if (string.IsNullOrEmpty(instanceIDFilter))
                    {
                        if (divsWithDepts.Contains(rec.DivCode))
                            divisionList.Add(new SelectListItem { Text = rec.DisplayName, Value = rec.DivCode });
                    }
                    else
                    {
                        if (divsWithDepts.Contains(rec.DivCode) && instanceDivisions.Contains(rec.DivCode))
                            divisionList.Add(new SelectListItem { Text = rec.DisplayName, Value = rec.DivCode });
                    }
                }
            }

            return divisionList;
        }

        public List<SelectListItem> GetDepartmentList(string divisionFilter)
        {
            List<SelectListItem> departmentList = new List<SelectListItem>();
            List<Departments> departments = new List<Departments>();

            departments = GetValidDepartments(divisionFilter);

            if (departments.Count() > 0)
            {
                foreach (var rec in departments)
                {
                    departmentList.Add(new SelectListItem { Text = rec.departmentDisplay, Value = rec.departmentCode });
                }
            }

            return departmentList.OrderBy(o => o.Text).ToList();
        }

        public List<SelectListItem> GetCategoryList(string divisionFilter, string departmentFilter)
        {
            List<SelectListItem> categoryList = new List<SelectListItem>();
            List<Categories> categories = new List<Categories>();

            // retrieve distinct categories from ItemMaster in order to populate active categoryids
            categories = GetValidCategories(divisionFilter, departmentFilter);

            if (categories.Count() > 0)
            {
                foreach (var rec in categories)
                {
                    categoryList.Add(new SelectListItem { Text = rec.CategoryDisplay, Value = rec.categoryCode });
                }
            }

            return categoryList.OrderBy(o => o.Text).ToList();
        }

        public List<SelectListItem> GetBrandIDList(string divisionFilter, string departmentFilter, string categoryFilter)
        {
            List<SelectListItem> brandIDList = new List<SelectListItem>();
            List<BrandIDs> brands;

            // retrieve distinct brands from ItemMaster in order to populate in use brandids
            brands = GetValidBrands(divisionFilter, departmentFilter, categoryFilter);

            if (brands.Count() > 0)
            {
                foreach (var rec in brands)
                {
                    brandIDList.Add(new SelectListItem { Text = rec.brandIDDisplay, Value = rec.brandIDCode });
                }
            }

            return brandIDList;
        }

        public List<SelectListItem> GetInstances(List<Division> divisionFilter)
        {
            List<SelectListItem> instances = new List<SelectListItem>();
            var instanceList = (from i in db.Instances
                                join id in db.InstanceDivisions
                                 on i.ID equals id.InstanceID
                                select new { i.ID, i.Name, id.Division}).ToList();

            var joinedList = (from id1 in instanceList
                              join df in divisionFilter
                               on id1.Division equals df.DivCode
                              select new { id1.ID, id1.Name }).ToList().Distinct();

            foreach (var instance in joinedList)
            {
                instances.Add(new SelectListItem
                {
                    Text = instance.Name,
                    Value = instance.ID.ToString()
                });
            }

            return instances;
        }

        public List<SelectListItem> GetSeasonalityLevels()
        {
            List<SelectListItem> seasonalityLevels = new List<SelectListItem>();

            var results = (from cp in db.ConfigParams
                           join c in db.Configs
                             on cp.ParamID equals c.ParamID
                           where cp.Name == "SEASONALITY_OVERRIDE_LEVEL"
                           select c).ToList();
            
            foreach (var result in results)
            {
                seasonalityLevels.Add(new SelectListItem { Text = result.Value, Value = result.InstanceID.ToString() });
            }

            return seasonalityLevels;
        }
        #endregion

        #region JSON Result routines
        public JsonResult GetNewDivsJson(string Id)
        {
            List<SelectListItem> newDivList = GetDivisionList(Id);
            return Json(new SelectList(newDivList.ToArray(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNewDeptsJson(string Id)
        {
            List<SelectListItem> newDeptList = GetDepartmentList(Id);
            return Json(new SelectList(newDeptList.ToArray(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNewCatJson(string div, string dept)
        {
            List<SelectListItem> newCategoryList = GetCategoryList(div, dept);
            return Json(new SelectList(newCategoryList.ToArray(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNewBrandIDJson(string div, string dept, string cat)
        {
            List<SelectListItem> newBrandIDList = GetBrandIDList(div, dept, cat);
            return Json(new SelectList(newBrandIDList.ToArray(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Miscellaneous helper routines
        public DateTime getCRCDate()
        {
            DateTime crcDate = db.ControlDates.Min(d => d.RunDate);
            return crcDate;
        }

        public ProductHierarchyOverrideModel Lookup(ProductHierarchyOverrideModel model)
        {
            string lookupSKU = model.prodHierarchyOverride.overrideSKU;

            var itemRec = validSku(lookupSKU);

            if (itemRec == null)
            {
                model.prodHierarchyOverride.overrideCategory = "";
                model.prodHierarchyOverride.overrideDepartment = "";
                model.prodHierarchyOverride.overrideDivision = "";
                model.prodHierarchyOverride.overrideBrandID = "";
                model.prodHierarchyOverride.overrideItemID = null;
                model.overrideSKUDescription = "The SKU was not found";
            }
            else
            {
                model.overrideDivisionLabel = (from a in currentUser.GetUserDivisions(AppName)
                                               where a.DivCode == itemRec.Div
                                               select a.DisplayName).FirstOrDefault();

                model.overrideDepartmentLabel = ((from a in db.Departments
                                                  where a.departmentCode == itemRec.Dept &&
                                                        a.divisionCode == itemRec.Div
                                                  select a).FirstOrDefault()).departmentDisplay;

                model.overrideCategoryLabel = ((from a in db.Categories
                                                where a.categoryCode == itemRec.Category &&
                                                      a.divisionCode == itemRec.Div &&
                                                      a.departmentCode == itemRec.Dept
                                                select a).FirstOrDefault()).CategoryDisplay;

                model.overrideBrandIDLabel = ((from a in db.BrandIDs
                                               where a.divisionCode == itemRec.Div &&
                                                     a.departmentCode == itemRec.Dept &&
                                                     a.brandIDCode == itemRec.Brand
                                               select a).FirstOrDefault()).brandIDDisplay;

                model.prodHierarchyOverride.overrideCategory = itemRec.Category;
                model.prodHierarchyOverride.overrideDepartment = itemRec.Dept;
                model.prodHierarchyOverride.overrideDivision = itemRec.Div;
                model.prodHierarchyOverride.newDivision = itemRec.Div;
                model.prodHierarchyOverride.overrideBrandID = itemRec.Brand;
                model.prodHierarchyOverride.overrideItemID = itemRec.ID;
                model.overrideSKUDescription = itemRec.Description;
            }

            return model;
        }

        private ProductHierarchyOverrideModel FillModelLists(ProductHierarchyOverrideModel model)
        {
            ProductHierarchyOverrides pho = model.prodHierarchyOverride;
            List<Division> userDivisions = currentUser.GetUserDivisions(AppName); 

            model.Instances = GetInstances(userDivisions);
            if (string.IsNullOrEmpty(model.selectedInstance))
            {
                model.selectedInstance = model.Instances[0].Value;
            }

            model.overrideDivisionList = GetDivisionList(model.selectedInstance);
            var existentOverrideDivision = model.overrideDivisionList.Where(div => div.Value == pho.overrideDivision).Count();
            if (existentOverrideDivision == 0 && model.overrideDivisionList.Count() > 0)
            {
                pho.overrideDivision = model.overrideDivisionList[0].Value;
            }
            
            model.seasonalityLevels = GetSeasonalityLevels();
            if (string.IsNullOrEmpty(model.selectedSeasonalityLevel))
            {
                model.selectedSeasonalityLevel = model.seasonalityLevels
                    .Where(sl => sl.Value == model.selectedInstance).FirstOrDefault().Text;
            }

            model.overrideTypes = GetOverrideTypes();
            var existentOverrideTypes = model.overrideTypes.Where(o => o.Value == pho.productOverrideTypeCode).Count();
            if (existentOverrideTypes == 0 && model.overrideTypes.Count() > 0)
            {
                pho.productOverrideTypeCode = model.overrideTypes[0].Value;
            }

            model.overrideDepartmentList = GetDepartmentList(pho.overrideDivision);
            var existentOverrideDepartment = model.overrideDepartmentList.Where(dept => dept.Value == pho.overrideDepartment).Count();
            if (existentOverrideDepartment == 0 && model.overrideDepartmentList.Count() > 0)
            {
                pho.overrideDepartment = model.overrideDepartmentList[0].Value;
            }

            model.overrideCategoryList = GetCategoryList(pho.overrideDivision, pho.overrideDepartment);
            var existentOverrideCategory = model.overrideCategoryList.Where(cat => cat.Value == pho.overrideCategory).Count();
            if (existentOverrideCategory == 0 && model.overrideCategoryList.Count() > 0)
            {
                pho.overrideCategory = model.overrideCategoryList[0].Value;
            }

            model.overrideBrandIDList = GetBrandIDList(pho.overrideDivision, pho.overrideDepartment, pho.overrideCategory);
            var existentOverrideBrand = model.overrideBrandIDList.Where(brand => brand.Value == pho.overrideBrandID).Count();
            if (existentOverrideBrand == 0 && model.overrideBrandIDList.Count() > 0)
            {
                pho.overrideBrandID = model.overrideBrandIDList[0].Value;
            }

            // used to populate the newDivisionList but we mimic the overrideDivision to limit the user from creating
            // cross division overrides
            pho.newDivision = pho.overrideDivision;

            model.newDepartmentList = GetDepartmentList(pho.newDivision);
            var existentNewDepartment = model.newDepartmentList.Where(dept => dept.Value == pho.newDepartment).Count();
            if (existentNewDepartment == 0 && model.newDepartmentList.Count() > 0)
            {
                pho.newDepartment = model.newDepartmentList[0].Value;
            }

            model.newCategoryList = GetCategoryList(pho.newDivision, pho.newDepartment);
            var existentNewCategory = model.newCategoryList.Where(cat => cat.Value == pho.newCategory).Count();
            if (existentNewCategory == 0 && model.newCategoryList.Count() > 0)
            {
                pho.newCategory = model.newCategoryList[0].Value;
            }

            model.newBrandIDList = GetBrandIDList(pho.newDivision, pho.newDepartment, pho.newCategory);
            var existentNewBrandID = model.newBrandIDList.Where(brand => brand.Value == pho.newBrandID).Count();
            if (existentNewBrandID == 0 && model.newBrandIDList.Count() > 0)
            {
                pho.newBrandID = model.newBrandIDList[0].Value;
            }

            return model;
        }

        private ProductHierarchyOverrides PopulateFields(ProductHierarchyOverrides record)
        {
            // set the new division equal to the override division
            record.newDivision = record.overrideDivision;
            record.displayNewValue = record.newDivision + ":" + record.newDepartment + ":" + record.newCategory;
            
            if (!string.IsNullOrEmpty(record.newBrandID))
                record.displayNewValue += ":" + record.newBrandID;

            switch (record.productOverrideTypeCode)
            {
                case "DEPT":
                    record.displayOverrideValue = record.overrideDivision + ":" + record.overrideDepartment;
                    record.overrideBrandID = null;
                    record.overrideCategory = null;
                    record.overrideItemID = null;
                    record.overrideSKU = null;

                    break;

                case "CAT":
                    record.displayOverrideValue = record.overrideDivision + ":" + record.overrideDepartment + ":" +
                        record.overrideCategory;

                    record.overrideBrandID = null;
                    record.overrideItemID = null;
                    record.overrideSKU = null;
                    break;

                case "LC_BRANDID":
                    record.displayOverrideValue = record.overrideDivision + ":" + record.overrideDepartment + ":" +
                        record.overrideCategory + ":" + record.overrideBrandID;
                    record.overrideItemID = null;
                    record.overrideSKU = null;
                    break;

                case "SKU":
                    record.overrideItemID = (from a in db.ItemMasters
                                             where a.MerchantSku == record.overrideSKU
                                             select a.ID).FirstOrDefault();
                    record.displayOverrideValue = record.overrideSKU;
                    break;

                default:
                    break;
            }

            record.lastModifiedDate = DateTime.Now;
            record.lastModifiedUser = currentUser.NetworkID;

            return record;
        }

        private ProductHierarchyOverrides LoadTransaction(int id)
        {
            ProductHierarchyOverrides trans = (from a in db.ProductHierarchyOverrides
                                               where a.productHierarchyOverrideID == id
                                               select a).FirstOrDefault();
            return trans;
        }

        /// <summary>
        /// Will validate the created producthierarchyoverride
        /// </summary>
        /// <param name="pho">ProductHierarchyOverride</param>
        /// <param name="errorMessage">Specific error message</param>
        /// <returns>boolean dependent on if the producthierarchyoverride is valid</returns>
        private bool ValidateOverride(ProductHierarchyOverrides pho, string categoryLevel, out string errorMessage)
        {
            bool result = true;
            // line break for formatting
            string lineBreak = @"<br />", message = "";
            // error message for override combinations that are already existent within the system
            string existingMessage = "The override combination '" + pho.displayOverrideValue + "' is already existent and active within the system.  Please modify the original override or change this combination.";
            errorMessage = null;
            
            switch (pho.productOverrideTypeCode)
            {
                case "DEPT":
                    if (pho.overrideDivision != null && pho.overrideDepartment != null)
                    {
                        // ensure the override combination does not already exist
                        var query = (from p in db.ProductHierarchyOverrides
                                     where p.overrideDivision == pho.overrideDivision &&
                                           p.overrideDepartment == pho.overrideDepartment &&
                                           p.productOverrideTypeCode == pho.productOverrideTypeCode &&
                                           (p.effectiveToDt >= EntityFunctions.TruncateTime(DateTime.Now) || p.effectiveToDt == null) &&
                                           p.productHierarchyOverrideID != pho.productHierarchyOverrideID
                                     select p).SingleOrDefault();
                        if (query != null)
                        {
                            result = false;
                            message = existingMessage;
                            errorMessage += (errorMessage == null) ? message : lineBreak + message;
                        }
                    }
                    else
                    {
                        result = false;
                        errorMessage = "The override division and department must all have values";

                    }
                    break;
                case "CAT":
                    if (pho.overrideDivision != null &&
                        pho.overrideDepartment != null &&
                        pho.overrideCategory != null)
                    {
                        // ensure the override combination does not already exist (and is active)
                        var query = (from p in db.ProductHierarchyOverrides
                                     where p.overrideDivision == pho.overrideDivision &&
                                           p.overrideDepartment == pho.overrideDepartment &&
                                           p.overrideCategory == pho.overrideCategory &&
                                           p.productOverrideTypeCode == pho.productOverrideTypeCode &&
                                           (p.effectiveToDt >= EntityFunctions.TruncateTime(DateTime.Now) || p.effectiveToDt == null) &&
                                           p.productHierarchyOverrideID != pho.productHierarchyOverrideID
                                     select p).SingleOrDefault();
                        if (query != null)
                        {
                            result = false;
                            message = existingMessage;
                            errorMessage = (errorMessage == null) ? message : lineBreak + message;
                        }
                    }
                    else
                    {
                        result = false;
                        message = "The override division, department, and category must all have values";
                        errorMessage += (errorMessage == null) ? message : lineBreak + message;
                    }
                    break;
                case "LC_BRANDID":
                    // ensure the override combination does not already exist
                    if (pho.overrideDivision != null &&
                        pho.overrideDepartment != null &&
                        pho.overrideCategory != null &&
                        pho.overrideBrandID != null)
                    {
                        var query = (from p in db.ProductHierarchyOverrides
                                     where p.overrideDivision == pho.overrideDivision &&
                                           p.overrideDepartment == pho.overrideDepartment &&
                                           p.overrideCategory == pho.overrideCategory &&
                                           p.overrideBrandID == pho.overrideBrandID &&
                                           p.productOverrideTypeCode == pho.productOverrideTypeCode &&
                                           (p.effectiveToDt >= EntityFunctions.TruncateTime(DateTime.Now) || p.effectiveToDt == null) &&
                                           p.productHierarchyOverrideID != pho.productHierarchyOverrideID
                                     select p).SingleOrDefault();
                        if (query != null)
                        {
                            result = false;
                            message = existingMessage;
                            errorMessage = (errorMessage == null) ? message : lineBreak + message;
                        }
                    }
                    else
                    {
                        result = false;
                        message = "The override division, department, category and brand must all have values";
                        errorMessage += (errorMessage == null) ? message : lineBreak + message;
                    }
                    break;
                case "SKU":
                    if (validSku(pho.overrideSKU) != null)
                    {
                        var query = (from p in db.ProductHierarchyOverrides
                                     where p.overrideSKU == pho.overrideSKU &&
                                           p.productOverrideTypeCode == pho.productOverrideTypeCode &&
                                           (p.effectiveToDt >= EntityFunctions.TruncateTime(DateTime.Now) || p.effectiveToDt == null) &&
                                           p.productHierarchyOverrideID != pho.productHierarchyOverrideID
                                     select p).SingleOrDefault();
                        if (query != null)
                        {
                            result = false;
                            message = existingMessage;
                            errorMessage = (errorMessage == null) ? message : lineBreak + message;
                        }
                    }
                    else
                    {
                        result = false;
                        message = "Invalid override SKU";
                        errorMessage += (errorMessage == null) ? message : lineBreak + message;
                    }
                    break;
            }

            if (categoryLevel == "BrandID")
            {
                // validate new division, department, category, and brand list
                if (pho.newDivision == null ||
                    pho.newDepartment == null ||
                    pho.newCategory == null ||
                    pho.newBrandID == null)
                {
                    result = false;
                    message = "The new division, department, category and brand must all have values";
                    errorMessage += (errorMessage == null) ? message : lineBreak + message;
                }
            }
            else  // we'll assume it's Category
            {
                // validate new division, department, category, and brand list
                if (pho.newDivision == null ||
                    pho.newDepartment == null ||
                    pho.newCategory == null)
                {
                    result = false;
                    message = "The new division, department, and category must all have values";
                    errorMessage += (errorMessage == null) ? message : lineBreak + message;
                }
            }

            return result;
        }

        /// <summary>
        /// Will validate the existing override
        /// </summary>
        /// <param name="pho">ProductHierarchyOverride</param>
        /// <returns></returns>
        private bool ValidateExistingOverride(ProductHierarchyOverrideModel model, out string errorMessage)
        {
            bool result = true;
            ProductHierarchyOverrides pho = model.prodHierarchyOverride;
            // line break for view
            string lineBreak = @"<br />";
            // override sku error message
            string skuErrMessage = "Override SKU '" + pho.overrideSKU + "' no longer exists.";
            // override combination error message
            string oErrMessage = "The override division, department, category, and brand combination '" + pho.displayOverrideValue +"' no longer exists.";
            // new combination error message
            string nErrMessage = "The new division, department, category, and brand combination '" + pho.displayNewValue + "' no longer exists.";
            string n2ErrMessage = "The new division, department, and category combination '" + pho.displayNewValue + "' no longer exists.";

            // common appending statement for invalid combinations
            string commonMessage = "  Please choose a valid combination from the lists below.";
            errorMessage = null;

            // 'o' denotes override
            var oDepartmentExists = GetValidDepartments(pho.overrideDivision).Select(dept => dept.departmentCode).Contains(pho.overrideDepartment);
            var oCategoryExists = GetValidCategories(pho.overrideDivision, pho.overrideDepartment).Select(cat => cat.categoryCode).Contains(pho.overrideCategory);
            var oBrandExists = GetValidBrands(pho.overrideDivision, pho.overrideDepartment, pho.overrideCategory).Select(b => b.brandIDCode).Contains(pho.overrideBrandID);

            // validate override list dependent on override level
            switch (pho.productOverrideTypeCode)
            {
                case "DEPT":
                    if (!oDepartmentExists)
                    {
                        errorMessage = oErrMessage + commonMessage;
                        result = false;
                    }
                    break;
                case "CAT":
                    if (!oDepartmentExists || !oCategoryExists)
                    {
                        errorMessage = oErrMessage + commonMessage;
                        result = false;
                    }
                    break;
                case "LC_BRANDID":
                    if (!oDepartmentExists || !oCategoryExists || !oBrandExists)
                    {
                        errorMessage = oErrMessage + commonMessage;
                        result = false;
                    }
                    break;
                case "SKU":
                    if (validSku(pho.overrideSKU) == null)
                    {
                        errorMessage = skuErrMessage;
                        model.overrideSKUDescription = "The Sku was not found";
                        result = false;
                    }
                    break;
            }

            // validate new list
            var nDepartmentExists = GetValidDepartments(pho.newDivision).Select(dept => dept.departmentCode).Contains(pho.newDepartment);
            var nCategoryExists = GetValidCategories(pho.newDivision, pho.newDepartment).Select(cat => cat.categoryCode).Contains(pho.newCategory);

            if (!string.IsNullOrEmpty(pho.newBrandID))
            {
                var nBrandExists = GetValidBrands(pho.newDivision, pho.newDepartment, pho.newCategory).Select(b => b.brandIDCode).Contains(pho.newBrandID);
                if (!nDepartmentExists || !nCategoryExists || !nBrandExists)
                {
                    errorMessage += (errorMessage == null) ? nErrMessage + commonMessage : lineBreak + nErrMessage + commonMessage;
                    result = false;
                }
            }
            else
            {
                if (!nDepartmentExists || !nCategoryExists)
                {
                    errorMessage += (errorMessage == null) ? n2ErrMessage + commonMessage : lineBreak + n2ErrMessage + commonMessage;
                    result = false;
                }
            }
              
            return result;
        }

        /// <summary>
        /// Retrieve all valid departments from the ItemMaster table given the specified division
        /// </summary>
        /// <param name="divisionFilter">specified division</param>
        /// <returns>List of valid departments</returns>
        private List<Departments> GetValidDepartments(string divisionFilter)
        {
            List<Departments> departments = new List<Departments>();

            departments = (from im in db.ItemMasters
                           join d in db.Departments
                             on new { Division = im.Div, Department = im.Dept } equals
                                new { Division = d.divisionCode, Department = d.departmentCode }
                          where im.Div == divisionFilter &&
                                 // ensure that the brandid associated with ItemMaster record exists in the Brands table.
                                 (from b in db.BrandIDs
                                 where b.brandIDCode == im.Brand &&
                                       b.divisionCode == im.Div &&
                                       b.departmentCode == im.Dept
                                select b.brandIDCode).Distinct().Contains(im.Brand)
                         select d).Distinct().ToList();

            return departments;
        }

        /// <summary>
        /// Retrieve all valid categories from the ItemMaster table given the specified division
        /// and department
        /// </summary>
        /// <param name="divisionFilter">specified division</param>
        /// <param name="departmentFilter">specified department</param>
        /// <returns>List of valid categories</returns>
        private List<Categories> GetValidCategories(string divisionFilter, string departmentFilter)
        {
            List<Categories> categories = new List<Categories>();

            categories = (from im in db.ItemMasters
                          join c in db.Categories
                            on new { Category = im.Category, Division = im.Div, Department = im.Dept } equals
                               new { Category = c.categoryCode, Division = c.divisionCode, Department = c.departmentCode }
                         where im.Div == divisionFilter &&
                               im.Dept == departmentFilter &&
                                // ensure that the brandid associated with ItemMaster record exists in the Brands table.
                                (from b in db.BrandIDs
                                where b.brandIDCode == im.Brand &&
                                      b.divisionCode == im.Div &&
                                      b.departmentCode == im.Dept
                               select b.brandIDCode).Distinct().Contains(im.Brand)
                        select c).Distinct().ToList();

            return categories;
        }

        /// <summary>
        /// Retrieve all valid brands from the ItemMaster table given the specified division,
        /// department, and category
        /// </summary>
        /// <param name="divisionFilter">specified division</param>
        /// <param name="departmentFilter">specified department</param>
        /// <param name="categoryFilter">specified category</param>
        /// <returns>List of valid brands</returns>
        private List<BrandIDs> GetValidBrands(string divisionFilter, string departmentFilter, string categoryFilter)
        {
            List<BrandIDs> brands = new List<BrandIDs>();

            brands = (from im in db.ItemMasters
                      join b in db.BrandIDs
                        on new { Brand = im.Brand, Division = im.Div, Department = im.Dept } equals
                           new { Brand = b.brandIDCode, Division = b.divisionCode, Department = b.departmentCode }
                     where im.Div == divisionFilter &&
                           im.Dept == departmentFilter &&
                           im.Category == categoryFilter
                    select b).Distinct().ToList();

            return brands;
        }

        /// <summary>
        /// Will determine if the sku specified is valid within the ItemMaster table.
        /// </summary>
        /// <param name="sku">Override SKU</param>
        /// <returns>The item if valid, null if it does not exist</returns>
        private ItemMaster validSku(string sku)
        {
            var result = (from im in db.ItemMasters
                         where im.MerchantSku == sku
                        select im).FirstOrDefault();

            return result;
        }

        #endregion
    }
}
