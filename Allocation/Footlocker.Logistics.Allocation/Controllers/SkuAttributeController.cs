using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Spreadsheets;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.DAO;
using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Services;
using Aspose.Cells;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class SkuAttributeController : AppController
    {
        AllocationContext db = new AllocationContext();
        ConfigService configService = new ConfigService();

        readonly string editRoles = "Director of Allocation,Admin,Support,Advanced Merchandiser Processes";

        public ActionResult Index()
        {
            ViewBag.hasEditRole = currentUser.HasUserRole(editRoles.Split(',').ToList());

            ModelStateDictionary previousModelState = TempData["ModelState"] as ModelStateDictionary;

            if (previousModelState != null)
            {
                foreach (KeyValuePair<string, ModelState> kvp in previousModelState)
                    if (!ModelState.ContainsKey(kvp.Key))
                        ModelState.Add(kvp.Key, kvp.Value);
            }

            return View();
        }

        [GridAction]
        public ActionResult _Index()
        {
            List<SkuAttributeHeader> headers = (from a in db.SkuAttributeHeaders.AsEnumerable()
                                                join d in currentUser.GetUserDivisions()
                                                    on new { a.Division } equals
                                                       new { Division = d.DivCode }
                                                orderby a.Division, a.Dept, a.Category
                                                select a).ToList();
            
            List<string> users = (from a in headers
                                  select a.CreatedBy).Distinct().ToList();

            Dictionary<string, string> names = LoadUserNames(users);

            foreach (var item in headers)
            {
                item.CreatedBy = names[item.CreatedBy];
            }

            return View(new GridModel(headers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DivisionSelected(SkuAttributeModel model)
        {
            InitializeDivisions(model);
            InitializeDepartments(model, true);
            InitializeCategories(model);
            InitializeBrands(model);
            InitializeAttributes(model);

            return View("Create", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DepartmentSelected(SkuAttributeModel model)
        {
            InitializeDivisions(model);
            InitializeDepartments(model, false);
            InitializeCategories(model);
            InitializeBrands(model);
            InitializeAttributes(model);
            
            return View("Create", model);
        }

        private void InitializeDivisions(SkuAttributeModel model)
        {
            var userDivisions = currentUser.GetUserDivisions()
                .OrderBy(d => d.DivCode)
                .Select(d => new SelectListItem 
                            { 
                                Text = d.DisplayName, 
                                Value = d.DivCode,
                                Selected = d.DivCode == model.Division
                            });

            model.DivisionList = new SelectList(userDivisions, "Value", "Text");
            if (string.IsNullOrEmpty(model.Division))
            {
                if (userDivisions.Any())
                    model.Division = userDivisions.First().Value;
                else
                    ModelState.AddModelError("Division", "You are not authorized for any division");
            }
        }

        private void InitializeDepartments(SkuAttributeModel model, bool reset)
        {
            var userDepts = currentUser.GetUserDepartments().Where(d => d.DivCode == model.Division)
                                                            .Select(d => new SelectListItem
                                                            {
                                                                Text = d.DisplayName,
                                                                Value = d.DeptNumber,
                                                                Selected = d.DeptNumber == model.Department
                                                            });
            model.DepartmentList = new SelectList(userDepts, "Value", "Text");

            if (reset || string.IsNullOrEmpty(model.Department))
            {
                //default to first one in the list
                if (userDepts.Any())
                    model.Department = userDepts.First().Value;
                else
                    ModelState.AddModelError("Department", string.Format("You are not authorized for any departments within division {0}", model.Division));
            }
        }

        private void InitializeCategories(SkuAttributeModel model)
        {
            model.CategoryList = new SelectList(db.Categories.Where(c => c.divisionCode == model.Division &&
                                                                         c.departmentCode == model.Department), "categoryCode", "CategoryDisplay");
        }

        private void InitializeBrands(SkuAttributeModel model)
        {
            model.BrandList = new SelectList(db.BrandIDs.Where(b => b.divisionCode == model.Division &&
                                                                    b.departmentCode == model.Department), "brandIDCode", "brandIDDisplay");
        }

        private void InitializeAttributes(SkuAttributeModel model)
        {
            model.WeightActive = 100;
            model.Attributes = new List<SkuAttributeDetail>
            {
                new SkuAttributeDetail("BrandID", false, 0),
                new SkuAttributeDetail("Category", false, 0),
                new SkuAttributeDetail("color1", false, 0),
                new SkuAttributeDetail("color2", false, 0),
                new SkuAttributeDetail("color3", false, 0),
                new SkuAttributeDetail("Department", true, 0),
                new SkuAttributeDetail("Gender", false, 0),
                new SkuAttributeDetail("LifeOfSku", false, 0),
                new SkuAttributeDetail("Material", false, 0),
                new SkuAttributeDetail("Size", true, 0),
                new SkuAttributeDetail("SizeRange", false, 0),
                new SkuAttributeDetail("Skuid1", false, 0),
                new SkuAttributeDetail("Skuid2", false, 0),
                new SkuAttributeDetail("Skuid3", false, 0),
                new SkuAttributeDetail("Skuid4", false, 0),
                new SkuAttributeDetail("Skuid5", false, 0),
                new SkuAttributeDetail("TeamCode", false, 0),
                new SkuAttributeDetail("VendorNumber", false, 0),
                new SkuAttributeDetail("PlayerID", false, 0)
            };

            model.Attributes = (from a in model.Attributes 
                                orderby a.SortOrder, a.AttributeType ascending 
                                select a).ToList();
        }

        [CheckPermission(Roles = "Director of Allocation,Admin,Support,Advanced Merchandiser Processes")]
        public ActionResult Create()
        {
            SkuAttributeModel model = new SkuAttributeModel();

            InitializeDivisions(model);
            InitializeDepartments(model, true);
            InitializeCategories(model);
            InitializeBrands(model);
            InitializeAttributes(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SkuAttributeModel model, string submitButton)
        {
            if (submitButton == "Reinitialize")
            {
                ReInitializeSKU(model);

                InitializeDivisions(model);
                InitializeDepartments(model, false);
                InitializeCategories(model);
                InitializeBrands(model);

                if (ModelState.IsValid)
                    model.Message = "The SKU has been scheduled to reinitialize.";

                return View(model);
            }                
            else
            {
                InitializeDivisions(model);
                InitializeDepartments(model, false);

                if (ModelState.IsValid)
                {
                    bool existing = db.SkuAttributeHeaders.Where(sah => sah.Division == model.Division &&
                                                                        sah.Dept == model.Department &&
                                                                        (sah.Category == model.Category ||
                                                                         (sah.Category == null && model.Category == null)) &&
                                                                        (sah.Brand == model.BrandID ||
                                                                         (sah.Brand == null && model.BrandID == null)) &&
                                                                        (sah.SKU == model.SKU ||
                                                                         (sah.SKU == null && model.SKU == null))).Any();

                    if (existing)
                        ModelState.AddModelError("", "This Department/Category/BrandID/SKU is already setup, please use go Back to List and use Edit.");

                    if (!string.IsNullOrEmpty(model.BrandID) && string.IsNullOrEmpty(model.Category))
                        ModelState.AddModelError("Category", "Category is required when a BrandID is selected");

                    if (!string.IsNullOrEmpty(model.BrandID))
                    {
                        int skuCount = db.ItemMasters.Where(im => im.Div == model.Division &&
                                                              im.Dept == model.Department &&
                                                              im.Category == model.Category &&
                                                              im.Brand == model.BrandID).Count();

                        if (skuCount == 0)
                            ModelState.AddModelError("", "This Department/Category/BrandID selection doesn't match any skus.");
                    }

                    if (!string.IsNullOrEmpty(model.SKU))
                    {
                        string skuDivision = model.SKU.Substring(0, 2);
                        string skuDepartment = model.SKU.Substring(3, 2);

                        if (skuDivision != model.Division || skuDepartment != model.Department)
                            ModelState.AddModelError("SKU", "The Division and Department must match the SKU's division and department");

                        ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);
                        long itemID = itemDAO.GetItemID(model.SKU);
                        if (itemID == 0)
                            ModelState.AddModelError("SKU", "This SKU is not found in the database");

                        if (!string.IsNullOrEmpty(model.Category) || !string.IsNullOrEmpty(model.BrandID))
                            ModelState.AddModelError("SKU", "You can't provide a Category or Brand ID when providing a SKU.");
                    }

                    int total = model.Attributes.Sum(m => m.WeightInt);

                    if (total != 0 && total != 100)
                        ModelState.AddModelError("", string.Format("Total must equal 100, it was {0}", total));
                }

                if (!ModelState.IsValid)
                {
                    //has errors
                    InitializeCategories(model);
                    InitializeBrands(model);

                    return View(model);
                }
                else
                {
                    //process create
                    SkuAttributeHeader header = new SkuAttributeHeader
                    {
                        Division = model.Division,
                        Dept = model.Department,
                        Category = model.Category,
                        Brand = model.BrandID,
                        SKU = model.SKU,
                        CreatedBy = currentUser.NetworkID,
                        CreateDate = DateTime.Now,
                        WeightActiveInt = model.WeightActive
                    };

                    db.SkuAttributeHeaders.Add(header);
                    db.SaveChanges();

                    foreach (SkuAttributeDetail det in model.Attributes)
                    {
                        det.HeaderID = header.ID;
                        db.SkuAttributeDetails.Add(det);
                        db.SaveChanges();
                    }

                    return RedirectToAction("Index");
                }
            }
        }

        public void ReInitializeSKU(SkuAttributeModel model)
        {
            string errorMessage;

            if (string.IsNullOrEmpty(model.SKU))
                ModelState.AddModelError("SKU", "You must supply a SKU to reinitialize");
            else
            {
                SkuRangeController skuRangeController = new SkuRangeController();
                errorMessage = skuRangeController.AddReinitializedSKU(model.SKU, currentUser);

                if (!string.IsNullOrEmpty(errorMessage))
                    ModelState.AddModelError("SKU", errorMessage);
            }
        }

        public ActionResult Edit(int ID)
        {
            SkuAttributeHeader header = db.SkuAttributeHeaders.Where(s => s.ID == ID).First();
            var divisions = DivisionService.ListDivisions()
                .OrderBy(d => d.DivCode)
                .Select(d => new SelectListItem
                {
                    Text = d.DisplayName,
                    Value = d.DivCode,
                    Selected = d.DivCode == header.Division
                });

            var depts = DepartmentService.ListDepartments(header.Division).Select(d => new SelectListItem
                                                                                       {
                                                                                           Text = d.DisplayName,
                                                                                           Value = d.DeptNumber,
                                                                                           Selected = d.DeptNumber == header.Dept
                                                                                       });

            SkuAttributeModel model = new SkuAttributeModel()
            {
                HeaderID = ID,
                Division = header.Division,
                Department = header.Dept,
                Category = header.Category,
                BrandID = header.Brand,
                SKU = header.SKU,
                WeightActive = header.WeightActiveInt,
                DivisionList = new SelectList(divisions, "Value", "Text"),
                DepartmentList = new SelectList(depts, "Value", "Text"),
                Attributes = db.SkuAttributeDetails.Where(s => s.HeaderID == header.ID).ToList()
            };

            model.Attributes = (from a in model.Attributes 
                                orderby a.SortOrder, a.AttributeType ascending 
                                select a).ToList();

            InitializeCategories(model);
            InitializeBrands(model);
            
            if (currentUser.HasDivDept(model.Division, model.Department))
            {
                //check edit role    
                ViewBag.hasEditRole = currentUser.HasUserRole(editRoles.Split(',').ToList());
            }
            else
            {
                //no authorization to division/department
                ViewBag.hasEditRole = false;
                ModelState.AddModelError("", "You are not authorized for this division/department");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SkuAttributeModel model, string submitButton)
        {
            ViewBag.hasEditRole = true;

            if (currentUser.HasDivDept(model.Division, model.Department))
            {
                if (submitButton == "Save")
                {
                    int total = model.Attributes.Sum(a => a.WeightInt);

                    if ((total == 100) || (total == 0))
                    {
                        foreach (SkuAttributeDetail det in model.Attributes)
                            db.Entry(det).State = System.Data.EntityState.Modified;

                        SkuAttributeHeader header = db.SkuAttributeHeaders.Where(s => s.ID == model.HeaderID).First();

                        header.WeightActiveInt = model.WeightActive;
                        header.CreatedBy = currentUser.NetworkID;
                        header.CreateDate = DateTime.Now;
                        db.SaveChanges();
                    }
                    else
                        ModelState.AddModelError("", string.Format("Total must equal 100, it was {0}", total));
                }
                else                
                    ReInitializeSKU(model);                
            }
            else
            {
                ModelState.AddModelError("", "You are not authorized for this division/department");
                ViewBag.hasEditRole = false;
            }

            if (ModelState.IsValid && submitButton == "Save")
                return RedirectToAction("Index");
            else
            {
                if (ModelState.IsValid)
                    model.Message = "The SKU has been scheduled to reinitialize.";

                model.DivisionList = new SelectList(DivisionService.ListDivisions(), "divCode", "DisplayName");
                model.DepartmentList = new SelectList(DepartmentService.ListDepartments(model.Division), "DeptNumber", "DisplayName");

                InitializeCategories(model);
                InitializeBrands(model);

                return View(model);
            }
        }

        [CheckPermission(Roles = "Director of Allocation,Admin,Support,Advanced Merchandiser Processes")]        
        public ActionResult Delete(int ID)
        {
            SkuAttributeHeader header = db.SkuAttributeHeaders.Where(s => s.ID == ID).First();

            if (currentUser.HasDivDept(header.Division, header.Dept))
            {
                db.SkuAttributeHeaders.Remove(header);
                //db.SaveChanges();
                var details = db.SkuAttributeDetails.Where(d => d.HeaderID == ID).ToList();
                foreach (SkuAttributeDetail det in details)                
                    db.SkuAttributeDetails.Remove(det);                    
                
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "You are not authorized for this division/department");

                TempData["ModelState"] = ModelState;
                return RedirectToAction("Index");
            }
        }
        
        #region Upload
        public ActionResult Upload()
        {
            return View();
        }

        public ActionResult GetSkuAttributeTemplate()
        {
            ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);
            SkuAttributeSpreadsheet skuAttributeSpreadsheet = new SkuAttributeSpreadsheet(appConfig, configService, itemDAO);
            Workbook excelDocument;

            excelDocument = skuAttributeSpreadsheet.GetTemplate();

            excelDocument.Save(System.Web.HttpContext.Current.Response, "SkuAttributeUpload.xlsx", ContentDisposition.Attachment, skuAttributeSpreadsheet.SaveOptions);
            return View("Upload");
        }

        public ActionResult UploadSkuAttributes(IEnumerable<HttpPostedFileBase> attachments)
        {
            ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);
            SkuAttributeSpreadsheet skuAttributeSpreadsheet = new SkuAttributeSpreadsheet(appConfig, configService, itemDAO);

            int successCount = 0;

            foreach (HttpPostedFileBase file in attachments)
            {
                skuAttributeSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(skuAttributeSpreadsheet.message))
                    return Content(skuAttributeSpreadsheet.message);
                else
                {
                    if (skuAttributeSpreadsheet.errorList.Count > 0)
                    {
                        Session["errorList"] = skuAttributeSpreadsheet.errorList;
                        return Content(string.Format("There were {0} errors", skuAttributeSpreadsheet.errorList.Count.ToString()));
                    }
                }

                successCount += skuAttributeSpreadsheet.validSKUAttributes.Count();
            }

            return Json(new { message = string.Format("{0} Sku Attribute(s) Created/Modified", successCount) }, "application/json");
        }

        public ActionResult DownloadErrors()
        {
            ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);
            SkuAttributeSpreadsheet skuAttributeSpreadsheet = new SkuAttributeSpreadsheet(appConfig, configService, itemDAO);
            Workbook excelDocument;

            List<SkuAttributeHeader> errorList = new List<SkuAttributeHeader>();

            if (Session["errorList"] != null)
                errorList = (List<SkuAttributeHeader>)Session["errorList"];

            excelDocument = skuAttributeSpreadsheet.GetErrors(errorList);
            excelDocument.Save(System.Web.HttpContext.Current.Response, "SkuAttributeErrors.xlsx", ContentDisposition.Attachment, skuAttributeSpreadsheet.SaveOptions);

            return View();
        }
        #endregion

        #region Export
        [GridAction]
        public ActionResult ExportGrid(GridCommand settings)
        {
            SkuAttributeExport skuAttributeExport = new SkuAttributeExport(appConfig);

            skuAttributeExport.ExtractGrid(settings.FilterDescriptors);

            skuAttributeExport.excelDocument.Save(System.Web.HttpContext.Current.Response, "SkuAttributes.xlsx", ContentDisposition.Attachment, skuAttributeExport.SaveOptions);
            return RedirectToAction("Index");
        }

        public ActionResult Export(int ID)
        {
            SkuAttributeExport skuAttributeExport = new SkuAttributeExport(appConfig);

            skuAttributeExport.ExtractHeader(ID);

            if (string.IsNullOrEmpty(skuAttributeExport.errorMessage))            
                skuAttributeExport.excelDocument.Save(System.Web.HttpContext.Current.Response, skuAttributeExport.headerFileName, ContentDisposition.Attachment, skuAttributeExport.SaveOptions);
            else
                throw new Exception(skuAttributeExport.errorMessage);
            
            return RedirectToAction("Index");
        }
        #endregion
    }
}
