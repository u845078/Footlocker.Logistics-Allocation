using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Common;
using Telerik.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Director of Allocation,VP of Allocation,Admin,Support")]
    public class ProductHierarchyOverrideController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        #region ActionResult handlers
        //
        // GET: /ProductHierarchyOverride/

        public ActionResult Index(string message)
        {
            ViewData["message"] = message;

            List<Division> userDivList = WebSecurityService.ListUserDivisions(UserName, "Allocation");
            List<ProductHierarchyOverrides> model = (from a in db.ProductHierarchyOverrides
                                                     select a).ToList();
            var filteredModel = from a in model
                                join u in userDivList
                on a.overrideDivision equals u.DivCode
                                select a;

            foreach (ProductHierarchyOverrides pho in filteredModel)
            {
                pho.lastModifiedUserName = getFullUserName(pho.lastModifiedUser.Replace('\\', '/'));
                pho.productOverrideType = (from a in db.ProductOverrideTypes where a.productOverrideTypeCode == pho.productOverrideTypeCode select a).FirstOrDefault();
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
                    if (model.prodHierarchyOverride.productOverrideTypeCode == "SKU")
                        model = Lookup(model);

                    ProductHierarchyOverrides newRec = PopulateFields(model.prodHierarchyOverride);

                    if (!ValidateOverride(model.prodHierarchyOverride, out errorMessage))
                    {
                        model = FillModelLists(model);
                        ViewData["message"] = errorMessage;
                        return View(model);
                    }

                    db.ProductHierarchyOverrides.Add(newRec);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            catch
            {
                model = FillModelLists(model);
                return View(model);
            }
        }

        public ActionResult Edit(int id)
        {
            ProductHierarchyOverrideModel model = new ProductHierarchyOverrideModel();
            model.prodHierarchyOverride = LoadTransaction(id);

            // will deny access to user from editing a non-existing (invalid) override.
            if (!ValidateExistingOverride(model.prodHierarchyOverride))
            {
                string errMessage = "The override you have selected is no longer valid within the system.  Please delete this override and create a new one.";
                return RedirectToAction("Index", new { message = errMessage });
            }

            if (model.prodHierarchyOverride.productOverrideTypeCode == "SKU")
                model = Lookup(model);

            model = FillModelLists(model);
            return View(model);
        }

        //
        // POST: /ProductHierarchyOverride/Edit/5
        [HttpPost]
        public ActionResult Edit(ProductHierarchyOverrideModel model, int id)
        {
            string errorMessage = "";
            try
            {
                if (model.prodHierarchyOverride.productOverrideTypeCode == "SKU")
                    model = Lookup(model);

                ProductHierarchyOverrides editedRec = PopulateFields(model.prodHierarchyOverride);
                editedRec.productHierarchyOverrideID = id;

                if (!ValidateOverride(model.prodHierarchyOverride, out errorMessage))
                {
                    model = FillModelLists(model);
                    ViewData["message"] = errorMessage;
                    return View(model);
                }

                db.ProductHierarchyOverrides.Attach(editedRec);
                db.Entry(editedRec).State = System.Data.EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                model = FillModelLists(model);
                return View(model);
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
            var query = (from a in db.ProductOverrideTypes select a).ToList();
            var sortedTypes = query.OrderBy(o => o.sortValue).ToList();

            if (sortedTypes.Count() > 0)
            {
                foreach (var rec in sortedTypes)
                {
                    overrideTypeList.Add(new SelectListItem { Text = rec.productOverrideTypeDesc, Value = rec.productOverrideTypeCode });
                }
            }

            return overrideTypeList;
        }

        public List<SelectListItem> GetDivisionList()
        {
            List<SelectListItem> divisionList = new List<SelectListItem>();

            List<Division> userDivList = WebSecurityService.ListUserDivisions(UserName, "Allocation");
            var divsWithDepts = (from a in db.Departments select a.divisionCode).Distinct();

            if (userDivList.Count() > 0)
            {
                foreach (var rec in userDivList)
                {
                    if (divsWithDepts.Contains(rec.DivCode))
                        divisionList.Add(new SelectListItem { Text = rec.DisplayName, Value = rec.DivCode });
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
                    categoryList.Add(new SelectListItem { Text = rec.categoryDisplay, Value = rec.categoryCode });
                }
            }

            return categoryList.OrderBy(o => o.Text).ToList();
        }

        public List<SelectListItem> GetBrandIDList(string divisionFilter, string departmentFilter, string categoryFilter)
        {
            List<SelectListItem> brandIDList = new List<SelectListItem>();
            List<BrandIDs> brands = new List<BrandIDs>();

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

        #endregion

        #region JSON Result routines
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

            var itemRec = (from a in db.ItemMasters
                           where a.MerchantSku == lookupSKU
                           select a).FirstOrDefault();
            if (itemRec == null)
            {
                model.prodHierarchyOverride.overrideCategory = "";
                model.prodHierarchyOverride.overrideDepartment = "";
                model.prodHierarchyOverride.overrideDivision = "";
                model.prodHierarchyOverride.overrideBrandID = "";
                model.overrideSKUDescription = "The SKU was not found";
            }
            else
            {
                model.overrideDivisionLabel = (from a in WebSecurityService.ListUserDivisions(UserName, "Allocation")
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
                                                select a).FirstOrDefault()).categoryDisplay;

                model.overrideBrandIDLabel = ((from a in db.BrandIDs
                                               where a.divisionCode == itemRec.Div &&
                                                     a.departmentCode == itemRec.Dept &&
                                                     a.brandIDCode == itemRec.Brand
                                               select a).FirstOrDefault()).brandIDDisplay;

                model.prodHierarchyOverride.overrideCategory = itemRec.Category;
                model.prodHierarchyOverride.overrideDepartment = itemRec.Dept;
                model.prodHierarchyOverride.overrideDivision = itemRec.Div;
                model.prodHierarchyOverride.overrideBrandID = itemRec.Brand;
                model.overrideSKUDescription = itemRec.Description;
            }

            return model;
        }

        private ProductHierarchyOverrideModel FillModelLists(ProductHierarchyOverrideModel model)
        {
            model.overrideTypes = GetOverrideTypes();
            ProductHierarchyOverrides pho = model.prodHierarchyOverride;

            if (string.IsNullOrEmpty(pho.productOverrideTypeCode))
                pho.productOverrideTypeCode = model.overrideTypes[0].Value;

            model.overrideDivisionList = GetDivisionList();

            if (string.IsNullOrEmpty(pho.overrideDivision))
                pho.overrideDivision = model.overrideDivisionList[0].Value;
            //else
            //    model.overrideDivisionList[model.overrideDivisionList.FindIndex(m => m.Value == pho.overrideDivision)].Selected = true;

            model.overrideDepartmentList = GetDepartmentList(pho.overrideDivision);

            if (string.IsNullOrEmpty(pho.overrideDepartment))
                pho.overrideDepartment = model.overrideDepartmentList[0].Value;

            model.overrideCategoryList = GetCategoryList(pho.overrideDivision, pho.overrideDepartment);

            if (string.IsNullOrEmpty(pho.overrideCategory))
                pho.overrideCategory = model.overrideCategoryList[0].Value;

            model.overrideBrandIDList = GetBrandIDList(pho.overrideDivision, pho.overrideDepartment, pho.overrideCategory);

            if (string.IsNullOrEmpty(pho.overrideBrandID))
                pho.overrideBrandID = model.overrideBrandIDList[0].Value;

            model.newDivisionList = GetDivisionList();

            if (string.IsNullOrEmpty(pho.newDivision))
                pho.newDivision = model.newDivisionList[0].Value;

            model.newDepartmentList = GetDepartmentList(pho.newDivision);

            if (string.IsNullOrEmpty(pho.newDepartment))
                pho.newDepartment = model.newDepartmentList[0].Value;

            model.newCategoryList = GetCategoryList(pho.newDivision, pho.newDepartment);

            if (string.IsNullOrEmpty(pho.newCategory))
                pho.newCategory = model.newCategoryList[0].Value;

            model.newBrandIDList = GetBrandIDList(pho.newDivision, pho.newDepartment, pho.newCategory);

            if (string.IsNullOrEmpty(pho.newBrandID))
                pho.newBrandID = model.newBrandIDList[0].Value;

            return model;
        }

        private ProductHierarchyOverrides PopulateFields(ProductHierarchyOverrides record)
        {
            record.displayNewValue = record.newDivision + ":" + record.newDepartment + ":" + record.newCategory +
                ":" + record.newBrandID;
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
            record.lastModifiedUser = User.Identity.Name;

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
        private bool ValidateOverride(ProductHierarchyOverrides pho, out string errorMessage)
        {
            bool result = true;
            errorMessage = "";

            if (pho.productOverrideTypeCode.Equals("DEPT"))
            {
                if (pho.overrideDivision == null || pho.overrideDepartment == null)
                {
                    result = false;
                    errorMessage += "The override division and department must all have values";
                }

            }

            if (pho.productOverrideTypeCode.Equals("CAT"))
            {
                if (pho.overrideDivision == null ||
                    pho.overrideDepartment == null ||
                    pho.overrideCategory == null)
                {
                    result = false;
                    errorMessage += "The override division, department, and category must all have values";
                }

            }

            if (pho.productOverrideTypeCode.Equals("LC_BRANDID"))
            {
                if (pho.overrideDivision == null ||
                    pho.overrideDepartment == null ||
                    pho.overrideCategory == null ||
                    pho.overrideBrandID == null)
                {
                    result = false;
                    errorMessage += "The override division, department, category and brand must all have values";
                }
            }

            if (pho.productOverrideTypeCode.Equals("SKU"))
            {
                if (string.IsNullOrEmpty(pho.overrideDivision) ||
                    string.IsNullOrEmpty(pho.overrideDepartment) ||
                    string.IsNullOrEmpty(pho.overrideCategory) ||
                    string.IsNullOrEmpty(pho.overrideBrandID))
                {
                    result = false;
                    errorMessage += "Invalid override SKU";
                }
            }

            // validate new division, department, category, and brand list
            if (pho.newDivision == null ||
                pho.newDepartment == null ||
                pho.newCategory == null ||
                pho.newBrandID == null)
            {
                result = false;
                errorMessage += "The new division, department, category and brand must all have values";
            }

            return result;
        }

        /// <summary>
        /// Will validate the existing override to ensure the user is not trying to edit an invalid product
        /// hierarchy combination.  This is problematic because the population of the dropdowns on the edit
        /// screen depend on a valid combination or else it will throw an exception that will not be
        /// handled.
        /// </summary>
        /// <param name="pho">ProductHierarchyOverride</param>
        /// <returns></returns>
        private bool ValidateExistingOverride(ProductHierarchyOverrides pho)
        {
            bool result = true;

            if (pho.productOverrideTypeCode.Equals("DEPT"))
            {
                var departments = GetValidDepartments(pho.overrideDivision);
                int departmentCount = departments.Where(d => d.departmentCode == pho.overrideDepartment).Count();
                if (departmentCount == 0)
                {
                    result = false;
                }
            }

            if (pho.productOverrideTypeCode.Equals("CAT"))
            {
                var categories = GetValidCategories(pho.overrideDivision, pho.overrideDepartment);
                int categoryCount = categories.Where(category => category.categoryCode.ToString() == pho.overrideCategory).Count();
                if (categoryCount == 0)
                {
                    result = false;
                }
            }

            if (pho.productOverrideTypeCode.Equals("LC_BRANDID"))
            {
                var brands = GetValidBrands(pho.overrideDivision, pho.overrideDepartment, pho.overrideCategory);
                int brandCount = brands.Where(brand => brand.brandIDCode == pho.overrideBrandID).Count();
                if (brandCount == 0)
                {
                    result = false;
                }
            }

            // validate new list
            var newBrands = GetValidBrands(pho.newDivision, pho.newDepartment, pho.newCategory);
            int newBrandCount = newBrands.Where(brand => brand.brandIDCode == pho.newBrandID).Count();
            if (newBrandCount == 0)
            {
                result = false;
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

        #endregion
    }
}
