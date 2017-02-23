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

                    db.ProductHierarchyOverrides.Add(newRec);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            ProductHierarchyOverrideModel model = new ProductHierarchyOverrideModel();
            model.prodHierarchyOverride = LoadTransaction(id);

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
            try
            {
                if (model.prodHierarchyOverride.productOverrideTypeCode == "SKU")
                    model = Lookup(model);

                ProductHierarchyOverrides editedRec = PopulateFields(model.prodHierarchyOverride);
                editedRec.productHierarchyOverrideID = id;

                db.ProductHierarchyOverrides.Attach(editedRec);
                db.Entry(editedRec).State = System.Data.EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
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
            var query = (from a in db.Departments
                         where a.divisionCode == divisionFilter
                         select a).ToList();
            if (query.Count() > 0)
            {
                foreach (var rec in query)
                {
                    departmentList.Add(new SelectListItem { Text = rec.departmentDisplay, Value = rec.departmentCode });
                }
            }

            return departmentList;
        }

        public List<SelectListItem> GetCategoryList(string divisionFilter, string departmentFilter)
        {
            List<SelectListItem> categoryList = new List<SelectListItem>();
            var query = (from a in db.Categories
                         where a.divisionCode == divisionFilter &&
                               a.departmentCode == departmentFilter
                         select a).ToList();

            if (query.Count() > 0)
            {
                foreach (var rec in query)
                {
                    categoryList.Add(new SelectListItem { Text = rec.categoryDisplay, Value = rec.categoryCode });
                }
            }

            return categoryList;
        }

        public List<SelectListItem> GetBrandIDList(string divisionFilter, string departmentFilter)
        {
            List<SelectListItem> brandIDList = new List<SelectListItem>();
            var query = (from a in db.BrandIDs
                         where a.divisionCode == divisionFilter &&
                               a.departmentCode == departmentFilter
                         select a).ToList();

            if (query.Count() > 0)
            {
                foreach (var rec in query)
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

        public JsonResult GetNewBrandIDJson(string div, string dept)
        {
            List<SelectListItem> newBrandIDList = GetBrandIDList(div, dept);
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

            if (string.IsNullOrEmpty(model.prodHierarchyOverride.productOverrideTypeCode))
                model.prodHierarchyOverride.productOverrideTypeCode = model.overrideTypes[0].Value;

            model.overrideDivisionList = GetDivisionList();

            if (string.IsNullOrEmpty(model.prodHierarchyOverride.overrideDivision))
                model.prodHierarchyOverride.overrideDivision = model.overrideDivisionList[0].Value;
            //else
            //    model.overrideDivisionList[model.overrideDivisionList.FindIndex(m => m.Value == model.prodHierarchyOverride.overrideDivision)].Selected = true;

            model.overrideDepartmentList = GetDepartmentList(model.prodHierarchyOverride.overrideDivision);

            if (string.IsNullOrEmpty(model.prodHierarchyOverride.overrideDepartment))
                model.prodHierarchyOverride.overrideDepartment = model.overrideDepartmentList[0].Value;

            model.overrideCategoryList = GetCategoryList(model.prodHierarchyOverride.overrideDivision, model.prodHierarchyOverride.overrideDepartment);

            if (string.IsNullOrEmpty(model.prodHierarchyOverride.overrideCategory))
                model.prodHierarchyOverride.overrideCategory = model.overrideCategoryList[0].Value;

            model.overrideBrandIDList = GetBrandIDList(model.prodHierarchyOverride.overrideDivision, model.prodHierarchyOverride.overrideDepartment);

            if (string.IsNullOrEmpty(model.prodHierarchyOverride.overrideBrandID))
                model.prodHierarchyOverride.overrideBrandID = model.overrideBrandIDList[0].Value;

            model.newDivisionList = GetDivisionList();

            if (string.IsNullOrEmpty(model.prodHierarchyOverride.newDivision))
                model.prodHierarchyOverride.newDivision = model.newDivisionList[0].Value;

            model.newDepartmentList = GetDepartmentList(model.prodHierarchyOverride.newDivision);

            if (string.IsNullOrEmpty(model.prodHierarchyOverride.newDepartment))
                model.prodHierarchyOverride.newDepartment = model.newDepartmentList[0].Value;

            model.newCategoryList = GetCategoryList(model.prodHierarchyOverride.newDivision, model.prodHierarchyOverride.newDepartment);

            if (string.IsNullOrEmpty(model.prodHierarchyOverride.newCategory))
                model.prodHierarchyOverride.newCategory = model.newCategoryList[0].Value;

            model.newBrandIDList = GetBrandIDList(model.prodHierarchyOverride.newDivision, model.prodHierarchyOverride.newDepartment);

            if (string.IsNullOrEmpty(model.prodHierarchyOverride.newBrandID))
                model.prodHierarchyOverride.newBrandID = model.newBrandIDList[0].Value;

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
        #endregion
    }
}
