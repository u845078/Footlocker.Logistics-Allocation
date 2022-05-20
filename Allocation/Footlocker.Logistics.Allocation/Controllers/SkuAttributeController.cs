using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Common;
using Telerik.Web.Mvc;
using Aspose.Excel;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class SkuAttributeController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        public ActionResult Index(bool? showMessage)
        {
            ViewData["hasEditRole"] = HasEditRole();

            if (showMessage != null)
            {
                ViewData["message"] = "You are not authorized for this division/department";
            }
            return View();
        }

        [GridAction]
        public ActionResult _Index(GridCommand settings)
        {
            List<SkuAttributeHeader> headers = (from a in db.SkuAttributeHeaders.AsEnumerable()
                                                join d in currentUser.GetUserDivisions(AppName)
                                                    on new { Division = a.Division } equals
                                                       new { Division = d.DivCode }
                                                orderby a.Division, a.Dept, a.Category
                                                select a).ToList();
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
            model.Divisions = currentUser.GetUserDivisions(AppName);
            if (string.IsNullOrEmpty(model.Division))
            {
                //default to first one in the list
                if (model.Divisions.Any())
                {
                    model.Division = model.Divisions.First().DivCode;
                }
                else
                {
                    model.Message = "You are not authorized for any division";
                }
            }
        }

        private void InitializeDepartments(SkuAttributeModel model, bool reset)
        {
            model.Departments = currentUser.GetUserDepartments(AppName).Where(d => d.DivCode == model.Division).ToList();

            if (reset || string.IsNullOrEmpty(model.Department))
            {
                //default to first one in the list
                if (model.Departments.Any())
                {
                    model.Department = model.Departments.First().DeptNumber;
                }
                else
                {
                    model.Message = "You are not authorized for any departments within division " + model.Division;
                }
            }
        }

        private void InitializeCategories(SkuAttributeModel model)
        {
            model.Categories = db.Categories.Where(c => c.divisionCode == model.Division && c.departmentCode == model.Department).ToList();
        }

        private void InitializeBrands(SkuAttributeModel model)
        {
            model.Brands = db.BrandIDs.Where(b => b.divisionCode == model.Division && b.departmentCode == model.Department).ToList();
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
        public ActionResult Create(string div)
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
        public ActionResult Create(SkuAttributeModel model)
        {
            InitializeDivisions(model);
            InitializeDepartments(model, false);

            if (string.IsNullOrEmpty(model.Message))
            {
                var existing = from a in db.SkuAttributeHeaders
                                where a.Division == model.Division &&
                                      a.Dept == model.Department &&
                                      (
                                            a.Category == model.Category ||
                                            (a.Category == null && model.Category == null)
                                      ) &&
                                      (
                                           a.Brand == model.BrandID ||
                                           (a.Brand == null && model.BrandID == null)
                                      )                                    
                                select a;

                if (existing.Any())
                {
                    model.Message = "This Department/Category/BrandID is already setup, please use go Back to List and use Edit.";
                }

                if (!string.IsNullOrEmpty(model.BrandID) && string.IsNullOrEmpty(model.Category) && string.IsNullOrEmpty(model.Message))
                {
                    model.Message = "Category is required when a BrandID is selected";
                }

                if (!string.IsNullOrEmpty(model.BrandID) && string.IsNullOrEmpty(model.Message))
                {
                    var skus = from a in db.ItemMasters
                               where a.Div == model.Division &&
                                     a.Dept == model.Department &&
                                     a.Category == model.Category &&
                                     a.Brand == model.BrandID                                        
                               select a;

                    if (!skus.Any())
                    {
                        model.Message = "This Department/Category/BrandID selection doesn't match any skus.";
                    }
                }

                if (string.IsNullOrEmpty(model.Message))
                {
                    int total = model.Attributes.Sum(m => m.WeightInt);
                    if (total != 0 && total != 100)
                    {
                        model.Message = "Total must equal 100, it was " + total;
                    }
                }
            }

            if (!string.IsNullOrEmpty(model.Message))
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
                    CreatedBy = User.Identity.Name,
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

        public ActionResult Edit(int ID)
        {
            SkuAttributeModel model = new SkuAttributeModel();
            SkuAttributeHeader header = db.SkuAttributeHeaders.Where(s => s.ID == ID).First();

            model.HeaderID = ID;
            model.Division = header.Division; 
            model.Department = header.Dept;
            model.Category = header.Category;
            model.BrandID = header.Brand;
            model.WeightActive = header.WeightActiveInt;
            model.Attributes = db.SkuAttributeDetails.Where(s => s.HeaderID == header.ID).ToList();
            model.Attributes = (from a in model.Attributes 
                                orderby a.SortOrder, a.AttributeType ascending 
                                select a).ToList();
            model.Divisions = DivisionService.ListDivisions();
            model.Departments = DepartmentService.ListDepartments(model.Division);

            InitializeCategories(model);
            InitializeBrands(model);

            if (currentUser.HasDivDept(AppName, model.Division, model.Department))
            {
                //check edit role
                ViewData["hasEditRole"] = HasEditRole();
            }
            else
            {
                //no authorization to division/department
                ViewData["hasEditRole"] = false;
                model.Message = "You are not authorized for this division/department";
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(SkuAttributeModel model)
        {
            ViewData["hasEditRole"] = true;

            if (currentUser.HasDivDept(AppName, model.Division, model.Department))
            {
                int total = model.Attributes.Sum(a => a.WeightInt);

                if ((total == 100) || (total == 0))
                {
                    foreach (SkuAttributeDetail det in model.Attributes)
                    {
                        db.Entry(det).State = System.Data.EntityState.Modified;
                        db.SaveChanges();
                    }

                    SkuAttributeHeader header = db.SkuAttributeHeaders.Where(s => s.ID == model.HeaderID).First();

                    header.WeightActiveInt = model.WeightActive;
                    header.CreatedBy = User.Identity.Name;
                    header.CreateDate = DateTime.Now;
                    db.SaveChanges();
                }
                else
                {
                    model.Message = "Total must equal 100, it was " + total;
                }
            }
            else
            {
                model.Message = "You are not authorized for this division/department";
                ViewData["hasEditRole"] = false;
            }

            if (string.IsNullOrEmpty(model.Message))
            {
                return RedirectToAction("Index");
            }
            else
            {
                model.Divisions = DivisionService.ListDivisions();
                model.Departments = DepartmentService.ListDepartments(model.Division);

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
                db.SaveChanges();
                var details = db.SkuAttributeDetails.Where(d => d.HeaderID == ID).ToList();
                foreach (SkuAttributeDetail det in details)
                {
                    db.SkuAttributeDetails.Remove(det);
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index", new {showMessage = true});
            }
        }

        #region Importing/Exporting

        public ActionResult Upload()
        {
            return View();
        }

        public ActionResult GetSkuAttributeTemplate()
        {
            GetSkuAttributeExcelFile().Save("SkuAttributeUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View("Upload");
        }

        public ActionResult UploadSkuAttributes(IEnumerable<HttpPostedFileBase> attachments)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            // set license
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            string message = string.Empty;
            int successCount = 0;
            List<SkuAttributeHeader> list = new List<SkuAttributeHeader>();

            foreach (HttpPostedFileBase file in attachments)
            {
                // instantiate a workbook object taht represents an excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];

                // determine if the spreadsheet contains a valid header row
                var hasValidHeaderRow = 
                (
                    (Convert.ToString(mySheet.Cells[0, 5].Value).Contains("Attribute Weighting")) &&
                    (Convert.ToString(mySheet.Cells[1, 0].Value).Contains("Division")) &&
                    (Convert.ToString(mySheet.Cells[1, 1].Value).Contains("Department")) &&
                    (Convert.ToString(mySheet.Cells[1, 2].Value).Contains("Category")) &&
                    (Convert.ToString(mySheet.Cells[1, 3].Value).Contains("BrandID")) &&
                    (Convert.ToString(mySheet.Cells[1, 4].Value).Contains("Update Date")) &&
                    (Convert.ToString(mySheet.Cells[1, 5].Value).Contains("Active")) &&
                    (Convert.ToString(mySheet.Cells[1, 6].Value).Contains("Department")) &&
                    (Convert.ToString(mySheet.Cells[1, 7].Value).Contains("Category")) &&
                    (Convert.ToString(mySheet.Cells[1, 8].Value).Contains("VendorNumber")) &&
                    (Convert.ToString(mySheet.Cells[1, 9].Value).Contains("BrandID")) &&
                    (Convert.ToString(mySheet.Cells[1, 10].Value).Contains("Size")) &&
                    (Convert.ToString(mySheet.Cells[1, 11].Value).Contains("SizeRange")) &&
                    (Convert.ToString(mySheet.Cells[1, 12].Value).Contains("Color1")) &&
                    (Convert.ToString(mySheet.Cells[1, 13].Value).Contains("Color2")) &&
                    (Convert.ToString(mySheet.Cells[1, 14].Value).Contains("Color3")) &&
                    (Convert.ToString(mySheet.Cells[1, 15].Value).Contains("Gender")) &&
                    (Convert.ToString(mySheet.Cells[1, 16].Value).Contains("LifeOfSku")) &&
                    (Convert.ToString(mySheet.Cells[1, 17].Value).Contains("Material")) &&
                    (Convert.ToString(mySheet.Cells[1, 18].Value).Contains("PlayerID")) &&
                    (Convert.ToString(mySheet.Cells[1, 19].Value).Contains("SkuID1")) &&
                    (Convert.ToString(mySheet.Cells[1, 20].Value).Contains("SkuID2")) &&
                    (Convert.ToString(mySheet.Cells[1, 21].Value).Contains("SkuID3")) &&
                    (Convert.ToString(mySheet.Cells[1, 22].Value).Contains("SkuID4")) &&
                    (Convert.ToString(mySheet.Cells[1, 23].Value).Contains("SkuID5")) &&
                    (Convert.ToString(mySheet.Cells[1, 24].Value).Contains("Team Code"))
                );

                if (!hasValidHeaderRow)
                {
                    message = "Upload failed: Incorrect header - please use template.";
                    return Content(message);
                }
                else
                {
                    int row = 2;
                    try
                    {
                        while (HasDataOnRow(mySheet, row))
                        {
                            SkuAttributeHeader header = new SkuAttributeHeader();
                            header.Division = Convert.ToString(mySheet.Cells[row, 0].Value).Trim();
                            header.Dept = Convert.ToString(mySheet.Cells[row, 1].Value).Trim();
                            header.Category = Convert.ToString(mySheet.Cells[row, 2].Value).Trim();
                            header.Brand = Convert.ToString(mySheet.Cells[row, 3].Value).Trim();
                            header.CreateDate = Convert.ToDateTime(mySheet.Cells[row, 4].Value);
                            header.WeightActiveInt = Convert.ToInt32(mySheet.Cells[row, 5].Value);

                            message += CreateDetailRecord(header, "Department", Convert.ToString(mySheet.Cells[row, 6].Value).Trim(), row);
                            message += CreateDetailRecord(header, "Category", Convert.ToString(mySheet.Cells[row, 7].Value).Trim(), row);
                            message += CreateDetailRecord(header, "VendorNumber", Convert.ToString(mySheet.Cells[row, 8].Value).Trim(), row);
                            message += CreateDetailRecord(header, "BrandID", Convert.ToString(mySheet.Cells[row, 9].Value).Trim(), row);
                            message += CreateDetailRecord(header, "Size", Convert.ToString(mySheet.Cells[row, 10].Value).Trim(), row);
                            message += CreateDetailRecord(header, "SizeRange", Convert.ToString(mySheet.Cells[row, 11].Value).Trim(), row);
                            message += CreateDetailRecord(header, "Color1", Convert.ToString(mySheet.Cells[row, 12].Value).Trim(), row);
                            message += CreateDetailRecord(header, "Color2", Convert.ToString(mySheet.Cells[row, 13].Value).Trim(), row);
                            message += CreateDetailRecord(header, "Color3", Convert.ToString(mySheet.Cells[row, 14].Value).Trim(), row);
                            message += CreateDetailRecord(header, "Gender", Convert.ToString(mySheet.Cells[row, 15].Value).Trim(), row);
                            message += CreateDetailRecord(header, "LifeOfSku", Convert.ToString(mySheet.Cells[row, 16].Value).Trim(), row);
                            message += CreateDetailRecord(header, "Material", Convert.ToString(mySheet.Cells[row, 17].Value).Trim(), row);
                            message += CreateDetailRecord(header, "PlayerID", Convert.ToString(mySheet.Cells[row, 18].Value).Trim(), row);
                            message += CreateDetailRecord(header, "SkuID1", Convert.ToString(mySheet.Cells[row, 19].Value).Trim(), row);
                            message += CreateDetailRecord(header, "SkuID2", Convert.ToString(mySheet.Cells[row, 20].Value).Trim(), row);
                            message += CreateDetailRecord(header, "SkuID3", Convert.ToString(mySheet.Cells[row, 21].Value).Trim(), row);
                            message += CreateDetailRecord(header, "SkuID4", Convert.ToString(mySheet.Cells[row, 22].Value).Trim(), row);
                            message += CreateDetailRecord(header, "SkuID5", Convert.ToString(mySheet.Cells[row, 23].Value).Trim(), row);
                            message += CreateDetailRecord(header, "TeamCode", Convert.ToString(mySheet.Cells[row, 24].Value).Trim(), row);

                            if (message != "")
                            {
                                return Content(message);
                            }

                            // validate header
                            if ((message = ValidateUploadValues(header)) != "")
                            {
                                message = string.Format("Row #{0}:<br /> {1}", (row + 1), message);
                                return Content(message);
                            }

                            // determine if the header already exists
                            SkuAttributeHeader existentHeader = (from a in db.SkuAttributeHeaders
                                                where a.Division == header.Division &&
                                                      a.Dept == header.Dept &&
                                                      (a.Category == null ? header.Category == null : a.Category == header.Category) &&
                                                      (a.Brand == null ? header.Brand == null : a.Brand == header.Brand)
                                                select a).SingleOrDefault();


                            if (existentHeader != null)
                            {
                                existentHeader.WeightActiveInt = header.WeightActiveInt;
                                existentHeader.CreateDate = DateTime.Now;
                                existentHeader.CreatedBy = User.Identity.Name;
                                header.ID = existentHeader.ID;
                                // delete existing detail records.
                                List<SkuAttributeDetail> deleteDetailRecords = (from a in db.SkuAttributeDetails
                                                                                where a.HeaderID == existentHeader.ID
                                                                                select a).ToList();
                                foreach (var detail in deleteDetailRecords)
                                {
                                    db.SkuAttributeDetails.Remove(detail);
                                }
                                db.Entry(existentHeader).State = System.Data.EntityState.Modified;

                                // populate detail records with header ID and add record
                                foreach (var detail in header.SkuAttributeDetails)
                                {
                                    detail.HeaderID = header.ID;
                                    db.SkuAttributeDetails.Add(detail);
                                }

                                db.SaveChanges();
                            }
                            else
                            {
                                // add header to get its ID
                                header.CreatedBy = User.Identity.Name;
                                header.CreateDate = DateTime.Now;
                                db.SkuAttributeHeaders.Add(header);
                                db.SaveChanges();
                            }

                            db.SaveChanges();
                            row++;
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        message = "Upload failed: One or more columns has missing or invalid data.";
                        return Content(message);
                    }
                }
            }
            return Json(new { message = string.Format("{0} Sku Attribute(s) Created/Modified", successCount) }, "application/json");
        }

        private string CreateDetailRecord(SkuAttributeHeader header, string attributeType, string value, int row)
        {
            string errorsFound = "";
            SkuAttributeDetail detail = new SkuAttributeDetail();
            detail.AttributeType = attributeType;
            if (value.ToLower().Equals("m"))
            {
                detail.Mandatory = true;
            }
            else if (string.IsNullOrEmpty(value))
            {
                detail.WeightInt = 0;
            }
            else
            {
                try
                {
                    detail.WeightInt = Convert.ToInt32(value);
                }
                catch (FormatException)
                {
                    string message = "The attribute type, " + attributeType + " has an invalid supplied value.";
                    errorsFound = string.Format("Row #{0}: {1}\n", row, message);
                    return errorsFound;
                }                
            }

            header.SkuAttributeDetails.Add(detail);

            return errorsFound;
        }

        private string ValidateUploadValues(SkuAttributeHeader header)
        {
            string errorsFound = "";

            // take out display for category and brand
            if (header.Category.ToLower().Equals("default") || header.Category.Equals(""))
            {
                header.Category = null;
            }
            if (header.Brand.ToLower().Equals("default") || header.Brand.Equals(""))
            {
                header.Brand = null;
            }

            bool divisionExists = !string.IsNullOrEmpty(header.Division);
            bool deptExists = !string.IsNullOrEmpty(header.Dept);
            bool categoryExists = !string.IsNullOrEmpty(header.Category);
            bool brandExists = !string.IsNullOrEmpty(header.Brand);

            if (!divisionExists)
            {
                errorsFound += "Division is required. <br />";
            }

            if (!deptExists)
            {
                errorsFound += "Department is required. <br />";
            }

            if(!categoryExists && brandExists)
            {
                errorsFound += "Category is required if a Brand is supplied. <br />";
            }

            // division/department/category/brand combination must have skus.
            bool comboExists = true;
            if (!categoryExists && !brandExists)
            {
                comboExists = ( from a in db.ItemMasters
                               where a.Div == header.Division &&
                                     a.Dept == header.Dept
                              select a).Any();

                if (!comboExists)
                {
                    errorsFound += "The Division/Department combination is not associated with any Sku. <br />";
                }
            }
            else if (categoryExists && !brandExists)
            {
                comboExists = ( from a in db.ItemMasters
                               where a.Div == header.Division &&
                                     a.Dept == header.Dept &&
                                     a.Category == header.Category
                              select a).Any();               

                if (!comboExists)
                {
                    errorsFound += "The Division/Department/Category combination is not associated with any Sku.  <br />";
                }
            }
            else if (brandExists)
            {
                comboExists = ( from a in db.ItemMasters
                               where a.Div == header.Division &&
                                     a.Dept == header.Dept &&
                                     a.Category == header.Category &&
                                     a.Brand == header.Brand
                              select a).Any();

                if (!comboExists)
                {
                    errorsFound += "The Division/Department/Category/Brand combination is not associated with any Sku  <br />";
                }
            }

            // department MUST be mandatory.
            var departmentAttributeDetail = header.SkuAttributeDetails.Where(sad => sad.AttributeType.ToLower().Equals("department")).SingleOrDefault();
            if (!departmentAttributeDetail.Mandatory)
            {
                errorsFound += "The department attribute must be mandatory. <br />";
            }

            // if category or brand is supplied then the attributes for category and brand MUST be mandatory.
            var categoryAttributeDetail = header.SkuAttributeDetails.Where(sad => sad.AttributeType.ToLower().Equals("category")).SingleOrDefault();
            if (categoryExists && !categoryAttributeDetail.Mandatory)
            {
                errorsFound += "The category attribute must be mandatory. <br />";
            }
            var brandAttributeDetail = header.SkuAttributeDetails.Where(sad => sad.AttributeType.ToLower().Equals("brandid")).SingleOrDefault();
            if (brandExists && !brandAttributeDetail.Mandatory)
            {
                errorsFound += "The brand attribute must be mandatory. <br />";
            }

            // all attributes must add up to 100
            if (header.SkuAttributeDetails.Sum(sad => sad.WeightInt) != 100)
            {
                errorsFound += "The weight must add up to 100. <br />";
            }

            return errorsFound;
        }

        private bool HasDataOnRow(Worksheet mySheet, int row)
        {
            return mySheet.Cells[row, 0].Value != null ||
                   mySheet.Cells[row, 1].Value != null ||
                   mySheet.Cells[row, 2].Value != null ||
                   mySheet.Cells[row, 3].Value != null ||
                   mySheet.Cells[row, 4].Value != null ||
                   mySheet.Cells[row, 5].Value != null ||
                   mySheet.Cells[row, 6].Value != null ||
                   mySheet.Cells[row, 7].Value != null ||
                   mySheet.Cells[row, 8].Value != null ||
                   mySheet.Cells[row, 9].Value != null ||
                   mySheet.Cells[row, 10].Value != null ||
                   mySheet.Cells[row, 11].Value != null ||
                   mySheet.Cells[row, 12].Value != null ||
                   mySheet.Cells[row, 13].Value != null ||
                   mySheet.Cells[row, 14].Value != null ||
                   mySheet.Cells[row, 15].Value != null ||
                   mySheet.Cells[row, 16].Value != null ||
                   mySheet.Cells[row, 17].Value != null ||
                   mySheet.Cells[row, 18].Value != null ||
                   mySheet.Cells[row, 19].Value != null ||
                   mySheet.Cells[row, 20].Value != null ||
                   mySheet.Cells[row, 21].Value != null ||
                   mySheet.Cells[row, 22].Value != null ||
                   mySheet.Cells[row, 23].Value != null ||
                   mySheet.Cells[row, 24].Value != null;   
        }

        [GridAction]
        public ActionResult ExportGrid(GridCommand settings)
        {
            IQueryable<SkuAttributeHeader> headers = (from a in db.SkuAttributeHeaders.Include("SkuAttributeDetails").AsEnumerable()
                                                join d in currentUser.GetUserDivisions(AppName)
                                                    on new { Division = a.Division } equals
                                                       new { Division = d.DivCode }
                                                orderby a.Division, a.Dept, a.Category
                                                select a).AsQueryable();

            if (settings.FilterDescriptors.Any())
            {
                headers = headers.ApplyFilters(settings.FilterDescriptors);
            }            
            Aspose.Excel.Excel excelDocument = CreateSkuAttributeExport(headers.ToList());
            excelDocument.Save("SkuAttributes.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return RedirectToAction("Index");
        }

        public ActionResult Export(int ID)
        {
            // retrieve data (return list even though only 1 should be returned in order to use general method)
            List<SkuAttributeHeader> header = (from a in db.SkuAttributeHeaders.Include("SkuAttributeDetails") where a.ID == ID select a).ToList();
            Excel excelDocument = CreateSkuAttributeExport(header);
            SkuAttributeHeader sah = header.FirstOrDefault();
            string excelFileName = sah.Division + "-" + sah.Dept;
            if (sah != null)
            {
                if (sah.Category != null)
                {
                    excelFileName += "-" + sah.Category;
                }
                if (sah.Brand != null)
                {
                    excelFileName += "-" + sah.Brand;
                }
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
                mySheet.Cells[row, 4].PutValue(header.CreateDate);
                mySheet.Cells[row, 4].Style.Number = 14;
                mySheet.Cells[row, 5].PutValue(header.WeightActiveInt);
                mySheet.Cells[row, 5].Style.HorizontalAlignment = TextAlignmentType.Right;
                AddBorder(row, 5, mySheet);

                // attribute weighting
                PopulateRowValue(row, 6, header, mySheet, "department");
                PopulateRowValue(row, 7, header, mySheet, "category");
                PopulateRowValue(row, 8, header, mySheet, "vendornumber");
                PopulateRowValue(row, 9, header, mySheet, "brandid");
                PopulateRowValue(row, 10, header, mySheet, "size");
                PopulateRowValue(row, 11, header, mySheet, "sizerange");
                PopulateRowValue(row, 12, header, mySheet, "color1");
                PopulateRowValue(row, 13, header, mySheet, "color2");
                PopulateRowValue(row, 14, header, mySheet, "color3");
                PopulateRowValue(row, 15, header, mySheet, "gender");
                PopulateRowValue(row, 16, header, mySheet, "lifeofsku");
                PopulateRowValue(row, 17, header, mySheet, "material");
                PopulateRowValue(row, 18, header, mySheet, "playerid");
                PopulateRowValue(row, 19, header, mySheet, "skuid1");
                PopulateRowValue(row, 20, header, mySheet, "skuid2");
                PopulateRowValue(row, 21, header, mySheet, "skuid3");
                PopulateRowValue(row, 22, header, mySheet, "skuid4");
                PopulateRowValue(row, 23, header, mySheet, "skuid5");
                PopulateRowValue(row, 24, header, mySheet, "teamcode");
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

            Aspose.Excel.Range range = mySheet.Cells.CreateRange("F1", "Y1");
            range.Merge();
            mySheet.Cells[0, 5].PutValue("Attribute Weighting");
            mySheet.Cells[0, 5].Style.HorizontalAlignment = TextAlignmentType.Center;
            mySheet.Cells[0, 5].Style.Font.Size = 12;
            mySheet.Cells[0, 5].Style.Font.IsBold = true;
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
            mySheet.Cells[1, 4].PutValue("Update Date");
            mySheet.Cells[1, 5].PutValue("Active");
            mySheet.Cells[1, 6].PutValue("Department");
            PutComment(mySheet, "G2", "Department must have a mandatory value (M).");
            mySheet.Cells[1, 7].PutValue("Category");
            PutComment(mySheet, "H2", "If a Category was supplied, then this field must be mandatory (M).");
            mySheet.Cells[1, 8].PutValue("VendorNumber");
            mySheet.Cells[1, 9].PutValue("BrandID");
            PutComment(mySheet, "J2", "If a BrandID was supplied, then this field must be mandatory (M).");
            mySheet.Cells[1, 10].PutValue("Size");
            mySheet.Cells[1, 11].PutValue("SizeRange");
            mySheet.Cells[1, 12].PutValue("Color1");
            mySheet.Cells[1, 13].PutValue("Color2");
            mySheet.Cells[1, 14].PutValue("Color3");
            mySheet.Cells[1, 15].PutValue("Gender");
            mySheet.Cells[1, 16].PutValue("LifeOfSku");
            mySheet.Cells[1, 17].PutValue("Material");
            mySheet.Cells[1, 18].PutValue("PlayerID");
            mySheet.Cells[1, 19].PutValue("SkuID1");
            mySheet.Cells[1, 20].PutValue("SkuID2");
            mySheet.Cells[1, 21].PutValue("SkuID3");
            mySheet.Cells[1, 22].PutValue("SkuID4");
            mySheet.Cells[1, 23].PutValue("SkuID5");
            mySheet.Cells[1, 24].PutValue("Team Code");

            for (int i = 0; i < 25; i++)
            {
                mySheet.Cells[1, i].Style.Font.IsBold = true;
                if (i > 4)
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

        private bool HasEditRole()
        {
            string checkroles = "Director of Allocation,Admin,Support,Advanced Merchandiser Processes";
            string[] roles = checkroles.Split(new char[] { ',' });
            

            bool ok = WebSecurityService.UserHasRole(UserName, "Allocation", roles);
            return ok;
        }
    }
}
