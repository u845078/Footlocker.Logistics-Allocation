using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.DAO;
using Telerik.Web.Mvc;
using Aspose.Excel;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Footlocker.Logistics.Allocation.Services;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class SkuAttributeController : AppController
    {
        AllocationContext db = new AllocationContext();
        ConfigService configService = new ConfigService();
        ItemDAO itemDAO = new ItemDAO();

        readonly string editRoles = "Director of Allocation,Admin,Support,Advanced Merchandiser Processes";

        public ActionResult Index()
        {
            ViewBag.hasEditRole = currentUser.HasUserRole(AppName, editRoles.Split(',').ToList());

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
                                                join d in currentUser.GetUserDivisions(AppName)
                                                    on new { a.Division } equals
                                                       new { Division = d.DivCode }
                                                orderby a.Division, a.Dept, a.Category
                                                select a).ToList();

            Dictionary<string, string> names = new Dictionary<string, string>();
            var users = (from a in headers
                         select a.CreatedBy).Distinct();
            foreach (string userID in users)
            {
                names.Add(userID, getFullUserNameFromDatabase(userID.Replace('\\', '/')));
            }
            foreach (var item in headers)
            {
                item.CreatedBy = names[item.CreatedBy];
            }

            return View(new GridModel(headers));
        }

        [HttpPost]
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
            var userDivisions = currentUser.GetUserDivisions(AppName)
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
            var userDepts = currentUser.GetUserDepartments(AppName).Where(d => d.DivCode == model.Division)
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
        public ActionResult Create(SkuAttributeModel model, string submitButton)
        {
            if (submitButton == "Reinitialize")
            {
                ReInitializeSKU(model);

                InitializeDivisions(model);
                InitializeDepartments(model, false);
                InitializeCategories(model);
                InitializeBrands(model);
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
            
            if (currentUser.HasDivDept(AppName, model.Division, model.Department))
            {
                //check edit role    
                ViewBag.hasEditRole = currentUser.HasUserRole(AppName, editRoles.Split(',').ToList());
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
        public ActionResult Edit(SkuAttributeModel model)
        {
            ViewBag.hasEditRole = true;

            if (currentUser.HasDivDept(AppName, model.Division, model.Department))
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
            {
                ModelState.AddModelError("", "You are not authorized for this division/department");
                ViewBag.hasEditRole = false;
            }

            if (ModelState.IsValid)            
                return RedirectToAction("Index");            
            else
            {
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

            if (currentUser.HasDivDept(AppName, header.Division, header.Dept))
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

        #region Importing/Exporting
        public ActionResult Upload()
        {
            return View();
        }

        public ActionResult GetSkuAttributeTemplate()
        {
            SkuAttributeSpreadsheet skuAttributeSpreadsheet = new SkuAttributeSpreadsheet(appConfig, configService, itemDAO);
            Excel excelDocument;

            excelDocument = skuAttributeSpreadsheet.GetTemplate();

            excelDocument.Save("SkuAttributeUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View("Upload");
        }

        public ActionResult UploadSkuAttributes(IEnumerable<HttpPostedFileBase> attachments)
        {
            SkuAttributeSpreadsheet skuAttributeSpreadsheet = new SkuAttributeSpreadsheet(appConfig, configService, itemDAO);

            string message = string.Empty;
            int successCount = 0;

            foreach (HttpPostedFileBase file in attachments)
            {
                skuAttributeSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(skuAttributeSpreadsheet.message))
                    return Content(message);

                successCount += skuAttributeSpreadsheet.validSKUAttributes.Count();
            }

            return Json(new { message = string.Format("{0} Sku Attribute(s) Created/Modified", successCount) }, "application/json");
        }

        [GridAction]
        public ActionResult ExportGrid(GridCommand settings)
        {
            IQueryable<SkuAttributeHeader> headers = (from a in db.SkuAttributeHeaders.Include("SkuAttributeDetails").AsEnumerable()
                                                join d in currentUser.GetUserDivisions(AppName)
                                                    on new { a.Division } equals new { Division = d.DivCode }
                                                orderby a.Division, a.Dept, a.Category, a.SKU
                                                select a).AsQueryable();

            if (settings.FilterDescriptors.Any())
                headers = headers.ApplyFilters(settings.FilterDescriptors);            

            Aspose.Excel.Excel excelDocument = CreateSkuAttributeExport(headers.ToList());
            excelDocument.Save("SkuAttributes.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return RedirectToAction("Index");
        }

        public ActionResult Export(int ID)
        {
            // retrieve data (return list even though only 1 should be returned in order to use general method)
            List<SkuAttributeHeader> header = db.SkuAttributeHeaders.Include("SkuAttributeDetails")
                                                                    .Where(sa => sa.ID == ID)
                                                                    .ToList();
            Excel excelDocument = CreateSkuAttributeExport(header);
            SkuAttributeHeader sah = header.FirstOrDefault();
            string excelFileName = string.Format("{0}-{1}", sah.Division, sah.Dept);
            if (sah != null)
            {
                if (sah.Category != null)                
                    excelFileName += "-" + sah.Category;
                
                if (sah.Brand != null)                
                    excelFileName += "-" + sah.Brand;
                
                if (!string.IsNullOrEmpty(sah.SKU))
                    excelFileName += "-" + sah.SKU;
            }

            excelFileName += "-SkuAttributes.xls";
            excelDocument.Save(excelFileName, SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return RedirectToAction("Index");
        }

        private Excel CreateSkuAttributeExport(List<SkuAttributeHeader> headers)
        {
            Excel excelDocument = GetSkuAttributeExcelFile();

            int row = 2;
            Worksheet mySheet = excelDocument.Worksheets[0];
            foreach (var header in headers)
            {
                // header values
                mySheet.Cells[row, 0].PutValue(header.Division);
                mySheet.Cells[row, 0].Style.HorizontalAlignment = TextAlignmentType.Right;
                mySheet.Cells[row, 1].PutValue(header.Dept);
                mySheet.Cells[row, 1].Style.HorizontalAlignment = TextAlignmentType.Right;
                mySheet.Cells[row, 2].PutValue(header.CategoryForDisplay);
                mySheet.Cells[row, 2].Style.HorizontalAlignment = TextAlignmentType.Right;
                mySheet.Cells[row, 3].PutValue(header.BrandForDisplay);
                mySheet.Cells[row, 3].Style.HorizontalAlignment = TextAlignmentType.Right;
                mySheet.Cells[row, 4].PutValue(header.SKU);
                mySheet.Cells[row, 4].Style.HorizontalAlignment = TextAlignmentType.Right;
                mySheet.Cells[row, 5].PutValue(header.CreateDate);
                mySheet.Cells[row, 5].Style.Number = 14;
                mySheet.Cells[row, 6].PutValue(header.WeightActiveInt);
                mySheet.Cells[row, 6].Style.HorizontalAlignment = TextAlignmentType.Right;
                AddBorder(row, 6, mySheet);

                // attribute weighting
                PopulateRowValue(row, 7, header, mySheet, "department");
                PopulateRowValue(row, 8, header, mySheet, "category");
                PopulateRowValue(row, 9, header, mySheet, "vendornumber");
                PopulateRowValue(row, 10, header, mySheet, "brandid");
                PopulateRowValue(row, 11, header, mySheet, "size");
                PopulateRowValue(row, 12, header, mySheet, "sizerange");
                PopulateRowValue(row, 13, header, mySheet, "color1");
                PopulateRowValue(row, 14, header, mySheet, "color2");
                PopulateRowValue(row, 15, header, mySheet, "color3");
                PopulateRowValue(row, 16, header, mySheet, "gender");
                PopulateRowValue(row, 17, header, mySheet, "lifeofsku");
                PopulateRowValue(row, 18, header, mySheet, "material");
                PopulateRowValue(row, 19, header, mySheet, "playerid");
                PopulateRowValue(row, 20, header, mySheet, "skuid1");
                PopulateRowValue(row, 21, header, mySheet, "skuid2");
                PopulateRowValue(row, 22, header, mySheet, "skuid3");
                PopulateRowValue(row, 23, header, mySheet, "skuid4");
                PopulateRowValue(row, 24, header, mySheet, "skuid5");
                PopulateRowValue(row, 25, header, mySheet, "teamcode");
                row++;
            }

            return excelDocument;
        }

        private void AddBorder(int row, int col, Worksheet mySheet)
        {
            mySheet.Cells[row, col].Style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
            mySheet.Cells[row, col].Style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
            mySheet.Cells[row, col].Style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
            mySheet.Cells[row, col].Style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;

            mySheet.Cells[row, col].Style.Borders[BorderType.LeftBorder].Color = System.Drawing.Color.Black;
            mySheet.Cells[row, col].Style.Borders[BorderType.RightBorder].Color = System.Drawing.Color.Black;
            mySheet.Cells[row, col].Style.Borders[BorderType.BottomBorder].Color = System.Drawing.Color.Black;
            mySheet.Cells[row, col].Style.Borders[BorderType.TopBorder].Color = System.Drawing.Color.Black;
        }

        private Excel GetSkuAttributeExcelFile()
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            // set license
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();

            Worksheet mySheet = excelDocument.Worksheets[0];

            Aspose.Excel.Range range = mySheet.Cells.CreateRange("G1", "Z1");
            range.Merge();
            mySheet.Cells[0, 6].PutValue("Attribute Weighting");
            mySheet.Cells[0, 6].Style.HorizontalAlignment = TextAlignmentType.Center;
            mySheet.Cells[0, 6].Style.Font.Size = 12;
            mySheet.Cells[0, 6].Style.Font.IsBold = true;
            range.SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Thin, System.Drawing.Color.Black);
            range.SetOutlineBorder(BorderType.TopBorder, CellBorderType.Thin, System.Drawing.Color.Black);
            range.SetOutlineBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
            range.SetOutlineBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);
            range.RowHeight = 25;

            mySheet.Cells[1, 0].PutValue("Division");
            PutComment(mySheet, "A2", "Division is mandatory.");
            mySheet.Cells[1, 1].PutValue("Department");
            PutComment(mySheet, "B2", "Department is mandatory.");
            mySheet.Cells[1, 2].PutValue("Category");
            mySheet.Cells[1, 3].PutValue("BrandID");
            mySheet.Cells[1, 4].PutValue("SKU");
            mySheet.Cells[1, 5].PutValue("Update Date");
            mySheet.Cells[1, 6].PutValue("Active");
            mySheet.Cells[1, 7].PutValue("Department");
            PutComment(mySheet, "H2", "Department must have a mandatory value (M).");
            mySheet.Cells[1, 8].PutValue("Category");
            PutComment(mySheet, "I2", "If a Category was supplied, then this field must be mandatory (M).");
            mySheet.Cells[1, 9].PutValue("VendorNumber");
            mySheet.Cells[1, 10].PutValue("BrandID");
            PutComment(mySheet, "K2", "If a BrandID was supplied, then this field must be mandatory (M).");
            mySheet.Cells[1, 11].PutValue("Size");
            mySheet.Cells[1, 12].PutValue("SizeRange");
            mySheet.Cells[1, 13].PutValue("Color1");
            mySheet.Cells[1, 14].PutValue("Color2");
            mySheet.Cells[1, 15].PutValue("Color3");
            mySheet.Cells[1, 16].PutValue("Gender");
            mySheet.Cells[1, 17].PutValue("LifeOfSku");
            mySheet.Cells[1, 18].PutValue("Material");
            mySheet.Cells[1, 19].PutValue("PlayerID");
            mySheet.Cells[1, 20].PutValue("SkuID1");
            mySheet.Cells[1, 21].PutValue("SkuID2");
            mySheet.Cells[1, 22].PutValue("SkuID3");
            mySheet.Cells[1, 23].PutValue("SkuID4");
            mySheet.Cells[1, 24].PutValue("SkuID5");
            mySheet.Cells[1, 25].PutValue("Team Code");

            for (int i = 0; i < 26; i++)
            {
                mySheet.Cells[1, i].Style.Font.IsBold = true;
                if (i > 5)
                {
                    AddBorder(1, i, mySheet);
                }
                mySheet.AutoFitColumn(i);
            }

            return excelDocument;
        }

        private void PutComment(Worksheet mySheet, string cellLocation, string comment)
        {
            int commentIndex = mySheet.Comments.Add(cellLocation);
            Comment c = mySheet.Comments[commentIndex];
            c.Note = comment;
        }

        /// <summary>
        /// This was made so I didn't have to create an if/else for every attribute type since the PutValue method does
        /// not allow you to use a turing operator to determine what value to put (if they are of two different types i.e. int/string)
        /// </summary>
        private void PopulateRowValue(int row, int col, SkuAttributeHeader header, Aspose.Excel.Worksheet mySheet, string attributeType)
        {
            SkuAttributeDetail attribute = header.SkuAttributeDetails.Where(sad => sad.AttributeType.ToLower().Equals(attributeType)).SingleOrDefault();
            if (attribute.Mandatory)
            {
                mySheet.Cells[row, col].PutValue("M");
            }
            // users don't want to have a 0 for the value. so if 0, return empty string.
            else if (attribute.WeightInt == 0)
            {
                mySheet.Cells[row, col].PutValue(string.Empty);
            }
            else
            {
                mySheet.Cells[row, col].PutValue(attribute.WeightInt);
            }

            mySheet.Cells[row, col].Style.HorizontalAlignment = TextAlignmentType.Right;
            AddBorder(row, col, mySheet);
        }

        #endregion
    }
}
