using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Common;
using Telerik.Web.Mvc;
using Aspose.Excel;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class SkuAttributeController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        public ActionResult Index(bool? showMessage)
        {
            ViewData["hasEditRole"] = HasEditRole();
            List<SkuAttributeHeader> headers = (from a in db.SkuAttributeHeaders where 1 == 1 orderby a.Division, a.Dept, a.Category select a).ToList();

            if (showMessage != null)
            {
                ViewData["message"] = "You are not authorized for this division/department";
            }
            return View(headers);
        }

        [GridAction]
        public ActionResult _Index()
        {
            List<SkuAttributeHeader> headers = (from a in db.SkuAttributeHeaders where 1 == 1 orderby a.Division, a.Dept, a.Category select a).ToList();
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
            model.Divisions = Divisions();
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
            model.Departments = WebSecurityService.ListUserDepartments(UserName, "Allocation", model.Division);

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
            var query = (from a in db.Categories
                         where a.divisionCode == model.Division &&
                               a.departmentCode == model.Department
                         select a).ToList();

            model.Categories = new List<Categories>();

            if (query.Any())
            {
                foreach (var rec in query)
                {
                    model.Categories.Add(new Categories { divisionCode = rec.divisionCode, departmentCode = rec.departmentCode, categoryCode = rec.categoryCode, categoryName = rec.categoryName });
                }
            }
        }

        private void InitializeBrands(SkuAttributeModel model)
        {
            var query = (from a in db.BrandIDs
                         where a.divisionCode == model.Division &&
                               a.departmentCode == model.Department
                         select a).ToList();

            model.Brands = new List<BrandIDs>();

            if (query.Any())
            {
                foreach (var rec in query)
                {
                    model.Brands.Add(new BrandIDs { divisionCode = rec.divisionCode, departmentCode = rec.departmentCode, brandIDCode = rec.brandIDCode, brandIDName = rec.brandIDName });
                }
            }
        }

        private void InitializeAttributes(SkuAttributeModel model)
        {
            model.WeightActive = 100;
            model.Attributes = new List<SkuAttributeDetail>();
            model.Attributes.Add(new SkuAttributeDetail("BrandID", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Category", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("color1", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("color2", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("color3", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Department", true, 0));
            model.Attributes.Add(new SkuAttributeDetail("Gender", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("LifeOfSku", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Material", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Size", true, 0));
            model.Attributes.Add(new SkuAttributeDetail("SizeRange", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Skuid1", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Skuid2", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Skuid3", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Skuid4", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("Skuid5", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("TeamCode", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("VendorNumber", false, 0));
            model.Attributes.Add(new SkuAttributeDetail("PlayerID", false, 0));

            model.Attributes = (from a in model.Attributes orderby a.SortOrder, a.AttributeType ascending select a).ToList();
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
                var existing = (from a in db.SkuAttributeHeaders
                                where (
                                        (a.Division == model.Division) &&
                                        (a.Dept == model.Department) &&
                                        (
                                            (a.Category == model.Category) ||
                                            ((a.Category == null) && (model.Category == null))
                                        ) &&
                                        (
                                            (a.Brand == model.BrandID) ||
                                            ((a.Brand == null) && (model.BrandID == null))
                                        )
                                    )
                                select a);

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
                    var skus = (from a in db.ItemMasters
                                    where (
                                            (a.Div == model.Division) &&
                                            (a.Dept == model.Department) &&
                                            (a.Category == model.Category) &&
                                            (a.Brand == model.BrandID)
                                        )
                                    select a);

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
                SkuAttributeHeader header = new SkuAttributeHeader();

                header.Division = model.Division;
                header.Dept = model.Department;
                header.Category = model.Category;
                header.Brand = model.BrandID;
                header.CreatedBy = User.Identity.Name;
                header.CreateDate = DateTime.Now;
                header.WeightActiveInt = model.WeightActive;

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
            SkuAttributeHeader header = (from a in db.SkuAttributeHeaders where a.ID == ID select a).First();

            model.HeaderID = ID;
            model.Division = header.Division; 
            model.Department = header.Dept;
            model.Category = header.Category;
            model.BrandID = header.Brand;
            model.WeightActive = header.WeightActiveInt;
            model.Attributes = (from a in db.SkuAttributeDetails where a.HeaderID == header.ID select a).ToList();
            model.Attributes = (from a in model.Attributes orderby a.SortOrder, a.AttributeType ascending select a).ToList();
            model.Divisions = DivisionService.ListDivisions();
            model.Departments = DepartmentService.ListDepartments(model.Division);

            InitializeCategories(model);
            InitializeBrands(model);

            if (WebSecurityService.UserHasDepartment(UserName, "Allocation", model.Division, model.Department))
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

            if (WebSecurityService.UserHasDepartment(UserName, "Allocation", model.Division, model.Department))
            {
                int total = model.Attributes.Sum(a => a.WeightInt);

                if ((total == 100) || (total == 0))
                {
                    foreach (SkuAttributeDetail det in model.Attributes)
                    {
                        db.Entry(det).State = System.Data.EntityState.Modified;
                        db.SaveChanges();
                    }

                    SkuAttributeHeader header = (from a in db.SkuAttributeHeaders where a.ID == model.HeaderID select a).First();
                    //SkuAttributeHeader header = (from a in db.SkuAttributeHeaders where ((a.Division == model.Division) && (a.Dept == model.Department) && ((a.Category == model.Category) || ((a.Category == null) && (model.Category == null)))) select a).First();
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
            SkuAttributeHeader header = (from a in db.SkuAttributeHeaders where a.ID == ID select a).First();

            if (WebSecurityService.UserHasDepartment(UserName, "Allocation", header.Division, header.Dept))
            {
                db.SkuAttributeHeaders.Remove(header);
                db.SaveChanges();
                var query = (from a in db.SkuAttributeDetails where a.HeaderID == ID select a);
                foreach (SkuAttributeDetail det in query.ToList())
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

        #region Exporting

        public ActionResult ExportAllAttributes()
        {
            try
            {
                // retrieve data
                List<SkuAttributeHeader> headers = (from a in db.SkuAttributeHeaders.Include("SkuAttributeDetails")
                                                    where Departments().Contains(new Department { DivCode = a.Division, DeptNumber = a.Dept })
                                                    orderby a.Division, a.Dept, a.Category
                                                    select a).ToList();
                Excel excelDocument = CreateSkuAttributeExport(headers);
                excelDocument.Save("SkuAttributes.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

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
                mySheet.Cells[row, 2].PutValue(header.CreateDate);
                mySheet.Cells[row, 2].Style.Number = 14;
                mySheet.Cells[row, 2].Style.HorizontalAlignment = TextAlignmentType.Right;
                mySheet.Cells[row, 3].PutValue(header.Category);
                mySheet.Cells[row, 3].Style.HorizontalAlignment = TextAlignmentType.Right;
                mySheet.Cells[row, 4].PutValue(header.WeightActiveInt);
                mySheet.Cells[row, 4].Style.HorizontalAlignment = TextAlignmentType.Right;
                AddBorder(row, 4, mySheet);

                // attribute weighting
                PopulateRowValue(row, 5, header, mySheet, "department");
                PopulateRowValue(row, 6, header, mySheet, "category");
                PopulateRowValue(row, 7, header, mySheet, "vendornumber");
                PopulateRowValue(row, 8, header, mySheet, "brandid");
                PopulateRowValue(row, 9, header, mySheet, "size");
                PopulateRowValue(row, 10, header, mySheet, "sizerange");
                PopulateRowValue(row, 11, header, mySheet, "color1");
                PopulateRowValue(row, 12, header, mySheet, "color2");
                PopulateRowValue(row, 13, header, mySheet, "color3");
                PopulateRowValue(row, 14, header, mySheet, "gender");
                PopulateRowValue(row, 15, header, mySheet, "lifeofsku");
                PopulateRowValue(row, 16, header, mySheet, "material");
                PopulateRowValue(row, 17, header, mySheet, "playerid");
                PopulateRowValue(row, 18, header, mySheet, "skuid1");
                PopulateRowValue(row, 19, header, mySheet, "skuid2");
                PopulateRowValue(row, 20, header, mySheet, "skuid3");
                PopulateRowValue(row, 21, header, mySheet, "skuid4");
                PopulateRowValue(row, 22, header, mySheet, "skuid5");
                PopulateRowValue(row, 23, header, mySheet, "teamcode");
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

            Aspose.Excel.Range range = mySheet.Cells.CreateRange("E1", "X1");
            range.Merge();
            mySheet.Cells[0, 4].PutValue("Attribute Weighting");
            mySheet.Cells[0, 4].Style.HorizontalAlignment = TextAlignmentType.Center;
            mySheet.Cells[0, 4].Style.Font.Size = 12;
            mySheet.Cells[0, 4].Style.Font.IsBold = true;
            range.SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Thin, System.Drawing.Color.Black);
            range.SetOutlineBorder(BorderType.TopBorder, CellBorderType.Thin, System.Drawing.Color.Black);
            range.SetOutlineBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
            range.SetOutlineBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);
            range.RowHeight = 25;

            mySheet.Cells[1, 0].PutValue("Division");
            mySheet.Cells[1, 1].PutValue("Department");
            mySheet.Cells[1, 2].PutValue("Update Date");
            mySheet.Cells[1, 3].PutValue("Category");
            mySheet.Cells[1, 4].PutValue("Active");
            mySheet.Cells[1, 5].PutValue("Department");
            mySheet.Cells[1, 6].PutValue("Category");
            mySheet.Cells[1, 7].PutValue("VendorNumber");
            mySheet.Cells[1, 8].PutValue("BrandID");
            mySheet.Cells[1, 9].PutValue("Size");
            mySheet.Cells[1, 10].PutValue("SizeRange");
            mySheet.Cells[1, 11].PutValue("Color1");
            mySheet.Cells[1, 12].PutValue("Color2");
            mySheet.Cells[1, 13].PutValue("Color3");
            mySheet.Cells[1, 14].PutValue("Gender");
            mySheet.Cells[1, 15].PutValue("LifeOfSku");
            mySheet.Cells[1, 16].PutValue("Material");
            mySheet.Cells[1, 17].PutValue("PlayerID");
            mySheet.Cells[1, 18].PutValue("SkuID1");
            mySheet.Cells[1, 19].PutValue("SkuID2");
            mySheet.Cells[1, 20].PutValue("SkuID3");
            mySheet.Cells[1, 21].PutValue("SkuID4");
            mySheet.Cells[1, 22].PutValue("SkuID5");
            mySheet.Cells[1, 23].PutValue("Team Code");

            for (int i = 0; i < 24; i++)
            {
                mySheet.Cells[1, i].Style.Font.IsBold = true;
                if (i > 3)
                {
                    AddBorder(1, i, mySheet);
                }
                mySheet.AutoFitColumn(i);
            }

            return excelDocument;
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
