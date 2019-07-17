using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.DAO;
using Telerik.Web.Mvc;
using Aspose.Excel;
using System.IO;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Admin,IT,Support")]
    public class RDQRestrictionController : AppController
    {

        #region Private Members

        private AllocationContext db { get; set; }

        #endregion

        public RDQRestrictionController()
            : base()
        {
            this.db = new AllocationContext();
        }

        public ActionResult Index()
        {
            RDQRestrictionModel model = RetrieveModel();
            return View(model);
        }

        private RDQRestrictionModel RetrieveModel()
        {
            RDQRestrictionModel model = new RDQRestrictionModel();
            var permissions = WebSecurityService.ListUserRoles(UserName, "Allocation");
            model.CanEdit = permissions.Contains("IT");
            return model;
        }

        public ActionResult IndexByProduct(string message)
        {
            ViewData["message"] = message;
            RDQRestrictionModel model = RetrieveModel();
            return View(model);
        }

        public ActionResult IndexByStore(string message)
        {
            ViewData["message"] = message;
            RDQRestrictionModel model = RetrieveModel();
            return View(model);
        }

        [GridAction]
        public ActionResult _Index()
        {
            List<RDQRestriction> list = db.RDQRestrictions.ToList();
            return View(new GridModel(list));
        }

        public ActionResult Create()
        {
            RDQRestrictionModel model = new RDQRestrictionModel();
            model = FillModelLists(model);
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(RDQRestrictionModel model)
        {
            string message = "";

            if (!ValidateRDQRestriction(model, out message))
            {
                model = FillModelLists(model);
                ViewData["errorMessage"] = message;
                return View(model);
            }

            model.RDQRestriction.LastModifiedDate = DateTime.Now;
            model.RDQRestriction.LastModifiedUser = User.Identity.Name;
            db.RDQRestrictions.Add(model.RDQRestriction);
            db.SaveChanges();

            ViewData["message"] = "Successfully created an RDQ Restriction";
            model = FillModelLists(model);

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            RDQRestrictionModel model = null;

            RDQRestriction rr
                = db.RDQRestrictions
                    .Where(r => r.RDQRestrictionID.Equals(id))
                    .FirstOrDefault();

            if (rr != null)
            {
                model = new RDQRestrictionModel(rr);
            }
            else
            {
                // err
            }

            model = FillModelLists(model);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(RDQRestrictionModel model)
        {
            string message = "";

            if (!ValidateRDQRestriction(model, out message))
            {
                model = FillModelLists(model);
                ViewData["errorMessage"] = message;
                return View(model);
            }

            db.RDQRestrictions.Attach(model.RDQRestriction);
            db.Entry(model.RDQRestriction).State = System.Data.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [GridAction]
        public ActionResult _RDQRestrictionStores()
        {
            List<StoreLookup> returnValue = new List<StoreLookup>();

            List<Division> divs = this.Divisions();
            var list = (from rr in db.RDQRestrictions
                           join sl in db.StoreLookups
                             on new { Division = rr.Division, Store = rr.ToStore } equals
                                new { Division = sl.Division, Store = sl.Store }
                           select sl).Distinct().ToList();

            returnValue = (from a in list
                           join d in divs
                             on a.Division equals d.DivCode
                         select a).ToList();


            return PartialView(new GridModel(returnValue));
        }

        [GridAction]
        public ActionResult _RDQRestrictionsForStore(string div, string store)
        {
            List<RDQRestriction> returnValue = new List<RDQRestriction>();
            returnValue = db.RDQRestrictions.Where(rr => rr.Division.Equals(div) && rr.ToStore.Equals(store)).ToList();
            return PartialView(new GridModel(returnValue));
        }

        [GridAction]
        public ActionResult _RDQRestrictionProducts()
        {
            List<RDQRestriction> returnValue = new List<RDQRestriction>();

            List<string> divisions 
                = this.Divisions()
                    .Select(d => d.DivCode)
                    .Distinct()
                    .ToList();

            var list
                = db.RDQRestrictions
                    .Where(rr => divisions.Contains(rr.Division))
                    .Select(rr => new { rr.Division, rr.Department, rr.Category, rr.Brand })
                    .Distinct()
                    .ToList();

            returnValue
                = list
                    .Select(rr => new RDQRestriction(rr.Division, rr.Department, rr.Category, rr.Brand))
                    .OrderBy(rr => rr.Division)
                    .ThenBy(rr => rr.Department)
                    .ThenBy(rr => rr.Category)
                    .ThenBy(rr => rr.Brand)
                    .ToList();

            return PartialView(new GridModel(returnValue));
        }

        [GridAction]
        public ActionResult _RDQRestrictionsForProduct(string div, string dept, string cat, string brand)
        {
            List<RDQRestriction> returnValue = new List<RDQRestriction>();

            returnValue
                = db.RDQRestrictions
                    .Where(rr => rr.Division.Equals(div) &&
                                 (string.IsNullOrEmpty(dept) || rr.Department.Equals(dept)) &&
                                 (string.IsNullOrEmpty(cat) || rr.Category.Equals(cat)) &&
                                 (string.IsNullOrEmpty(brand) || rr.Brand.Equals(brand))).ToList();

            return PartialView(new GridModel(returnValue));
        }

        private void RevertDefaultValues(RDQRestriction rr)
        {
            const string defaultValue = "N/A";

            rr.Department = rr.Department.Equals(defaultValue) || string.IsNullOrEmpty(rr.Department) ? null : rr.Department;
            rr.Category = rr.Category.Equals(defaultValue) || string.IsNullOrEmpty(rr.Category) ? null : rr.Category;
            rr.Brand = rr.Brand.Equals(defaultValue) || string.IsNullOrEmpty(rr.Brand) ? null : rr.Brand;
            rr.FromDCCode = rr.FromDCCode.Equals(defaultValue) || string.IsNullOrEmpty(rr.FromDCCode) ? null : rr.FromDCCode;
            rr.ToDCCode = rr.ToDCCode.Equals(defaultValue) || string.IsNullOrEmpty(rr.ToDCCode) ? null : rr.ToDCCode;
            rr.RDQType = rr.RDQType.Equals(defaultValue) || string.IsNullOrEmpty(rr.RDQType) ? null : rr.RDQType;
            rr.Vendor = string.IsNullOrEmpty(rr.Vendor) ? null : rr.Vendor;
            rr.ToLeague = string.IsNullOrEmpty(rr.ToLeague) ? null : rr.ToLeague;
            rr.ToRegion = string.IsNullOrEmpty(rr.ToRegion) ? null : rr.ToRegion;

        }

        private bool ValidateRDQRestriction(RDQRestrictionModel model, out string errorMessage)
        {
            bool result = true;
            errorMessage = null;
            RDQRestriction rr = model.RDQRestriction;
            const string lineBreak = @"<br />";

            // revert any values that have the default value (N/A) to null
            this.RevertDefaultValues(rr);

            // check for duplicate
            bool duplicate
                = db.RDQRestrictions
                    .Any(r => r.Division.Equals(rr.Division) &&
                              ((r.Department == null && rr.Department == null) || r.Department.Equals(rr.Department)) &&
                              ((r.Category == null && rr.Category == null) || r.Category.Equals(rr.Category)) &&
                              ((r.Brand == null && rr.Category == null) || r.Brand.Equals(rr.Brand)) &&
                              ((r.RDQType == null && rr.RDQType == null) || r.RDQType.Equals(rr.RDQType)) &&
                              ((r.Vendor == null && rr.Vendor == null) || r.Vendor.Equals(rr.Vendor)) &&
                              ((r.FromDCCode == null && rr.FromDCCode == null) || r.FromDCCode.Equals(rr.FromDCCode)) &&
                              ((r.ToDCCode == null && rr.ToDCCode == null) || r.ToDCCode.Equals(rr.ToDCCode)) &&
                              ((r.ToLeague == null && rr.ToLeague == null) || r.ToLeague.Equals(rr.ToLeague)) &&
                              ((r.ToRegion == null && rr.ToRegion == null) || r.ToRegion.Equals(rr.ToRegion)) &&
                              ((r.ToStore == null && rr.ToStore == null) || r.ToStore.Equals(rr.ToStore)) &&
                              r.RDQRestrictionID != rr.RDQRestrictionID);

            if (duplicate)
            {
                // err
                result = false;
                // err message
                errorMessage = "There is already an existing record for the criteria populated.";
                return result;
            }

            bool hasDepartmentSelected = !string.IsNullOrEmpty(rr.Department);
            bool hasCategorySelected = !string.IsNullOrEmpty(rr.Category);
            bool hasBrandSelected = !string.IsNullOrEmpty(rr.Brand);
            bool hasRDQTypeSelected = !string.IsNullOrEmpty(rr.RDQType);
            bool hasFromDCSelected = !string.IsNullOrEmpty(rr.FromDCCode);
            bool hasToDCSelected = !string.IsNullOrEmpty(rr.ToDCCode);

            if (hasDepartmentSelected && !hasCategorySelected && !hasBrandSelected)
            {
                // validation for division/department combination
                var validCombination
                    = db.ItemMasters.Any(im => im.Div.Equals(rr.Division) &&
                                               im.Dept.Equals(rr.Department));

                if (!validCombination)
                {
                    // err
                    result = false;
                    string message = "The Division and Department combination is not valid.";
                    errorMessage = (errorMessage == null) ? message : lineBreak + message;
                }
            }
            else if (hasDepartmentSelected && hasCategorySelected && !hasBrandSelected)
            {
                // validation for division/department/category combination
                var validCombination
                    = db.ItemMasters.Any(im => im.Div.Equals(rr.Division) &&
                                               im.Dept.Equals(rr.Department) &&
                                               im.Category.Equals(rr.Category));

                if (!validCombination)
                {
                    // err
                    result = false;
                    string message = "The Division, Department, and Category combination is not valid.";
                    errorMessage = (errorMessage == null) ? message : lineBreak + message;
                }
            }
            else if (hasDepartmentSelected && hasCategorySelected && hasBrandSelected)
            {
                // validation for division/department/category/brand combination
                var validCombination
                    = db.ItemMasters.Any(im => im.Div.Equals(rr.Division) &&
                                               im.Dept.Equals(rr.Department) &&
                                               im.Category.Equals(rr.Category) &&
                                               im.Brand.Equals(rr.Brand));

                if (!validCombination)
                {
                    // err
                    result = false;
                    string message = "The Division, Department, Category, and Brand combination is not valid.";
                    errorMessage = (errorMessage == null) ? message : lineBreak + message;
                }
            }

            // validate vendor
            if (!string.IsNullOrEmpty(rr.Vendor))
            {
                var validVendor = (from v in db.Vendors
                                   join id in db.InstanceDivisions
                                     on v.InstanceID equals id.InstanceID
                                   where id.Division.Equals(rr.Division) &&
                                         v.VendorName.Equals(rr.Vendor)
                                   select v).Any();

                if (!validVendor)
                {
                    // err
                    result = false;
                    string message = string.Format("The division/vendor combination is not valid. {0}, {1}", rr.Division, rr.Vendor);
                    errorMessage += (errorMessage == null) ? message : lineBreak + message;
                }
            }


            // validate store
            if (!string.IsNullOrEmpty(rr.ToStore))
            {
                var validStore
                    = db.StoreLookups.Any(sl => sl.Division.Equals(rr.Division) &&
                                                sl.Store.Equals(rr.ToStore));

                if (!validStore)
                {
                    // err
                    result = false;
                    string message = string.Format("The division/store combination is not valid. {0}-{1}", rr.Division, rr.ToStore);
                    errorMessage += (errorMessage == null) ? message : lineBreak + message;
                }
            }

            // validate region
            if (!string.IsNullOrEmpty(rr.ToRegion))
            {
                var validRegion
                    = db.StoreLookups.Any(sl => sl.Division.Equals(rr.Division) &&
                                                sl.Region.Equals(rr.ToRegion));

                if (!validRegion)
                {
                    // err
                    result = false;
                    string message = string.Format("The division/region combination is not valid. {0}, {1}", rr.Division, rr.ToRegion);
                    errorMessage += (errorMessage == null) ? message : lineBreak + message;
                }
            }

            // validate league
            if (!string.IsNullOrEmpty(rr.ToLeague))
            {
                var validLeague
                    = db.StoreLookups.Any(sl => sl.Division.Equals(rr.Division) &&
                                                sl.League.Equals(rr.ToLeague));

                if (!validLeague)
                {
                    // err
                    result = false;
                    string message = string.Format("The division/league combination is not valid. {0}, {1}", rr.Division, rr.ToLeague);
                    errorMessage += (errorMessage == null) ? message : lineBreak + message;
                }
            }

            return result;
        }

        private RDQRestrictionModel FillModelLists(RDQRestrictionModel model)
        {
            RDQRestriction rr = model.RDQRestriction;

            // populate division
            model.Divisions = GetDivisionsList();
            bool existentDivision = model.Divisions.Any(d => d.Value.Equals(rr.Division));
            if (!existentDivision && model.Divisions.Count() > 0)
            {
                rr.Division = model.Divisions.First().Value;
            }

            // populate department
            model.Departments = GetDepartmentsList(rr.Division);
            bool existentDepartment = model.Departments.Any(d => d.Value.Equals(rr.Department));
            if (!existentDepartment && model.Departments.Count() > 0)
            {
                rr.Department = model.Departments.First().Value;
            }

            // populate category
            model.Categories = GetCategoriesList(rr.Division, rr.Department);
            bool existentCategory = model.Categories.Any(c => c.Value.Equals(rr.Category));
            if (!existentCategory && model.Categories.Count() > 0)
            {
                rr.Category = model.Categories.First().Value;
            }

            // populate brand
            model.Brands = GetBrandsList(rr.Division, rr.Department, rr.Category);
            bool existentBrand = model.Brands.Any(b => b.Value.Equals(rr.Brand));
            if (!existentBrand && model.Brands.Count() > 0)
            {
                rr.Brand = model.Brands.First().Value;
            }

            // populate from distribution centers
            model.DistributionCenters = GetDistributionCentersList();
            bool existentFromDC = model.DistributionCenters.Any(dc => dc.Value.Equals(rr.FromDCCode));
            if (!existentFromDC && model.DistributionCenters.Count() > 0)
            {
                rr.FromDCCode = model.DistributionCenters.First().Value;
            }

            bool existentToDC = model.DistributionCenters.Any(dc => dc.Value.Equals(rr.ToDCCode));
            if (!existentToDC && model.DistributionCenters.Count() > 0)
            {
                rr.ToDCCode = model.DistributionCenters.First().Value;
            }

            // populate rdq types
            model.RDQTypes = GetRDQTypes();
            bool existentRDQType = model.RDQTypes.Any(r => r.Value.Equals(rr.RDQType));
            if (!existentRDQType && model.DistributionCenters.Count() > 0)
            {
                rr.RDQType = model.RDQTypes.First().Value;
            }

            return model;
        }

        public ActionResult DeleteRDQRestriction(int id)
        {
            RDQRestriction rr
                = db.RDQRestrictions
                    .Where(r => r.RDQRestrictionID.Equals(id))
                    .FirstOrDefault();

            if (rr != null)
            {
                db.RDQRestrictions.Remove(rr);
                db.SaveChanges();
            }
            else
            {
                return RedirectToAction("Index", new { message = "The RDQ Restriction no longer exists." });
            }

            return RedirectToAction("Index");
        }

        public ActionResult Upload(string message)
        {
            ViewData["errorMessage"] = message;
            return View();
        }

        public ActionResult SaveRDQRestrictions(IEnumerable<HttpPostedFileBase> attachments)
        {
            string message = string.Empty, errorMessage = string.Empty;
            List<RDQRestriction> parsedRRs = new List<RDQRestriction>(), validRRs = new List<RDQRestriction>();
            List<Tuple<RDQRestriction, string>> errorList = new List<Tuple<RDQRestriction, string>>();
            int successfulCount = 0;
            List<Tuple<RDQRestriction, string>> warnings, errors;

            foreach (HttpPostedFileBase file in attachments)
            {
                Worksheet workSheet = RetrieveWorkSheet(file);

                // validate header of uploaded file
                if (!this.HasValidHeaderRow(workSheet))
                {
                    message = "Upload failed: Incorrect header - please use template.";
                    return Content(message);
                }
                else
                {
                    int row = 1;
                    try
                    {
                        // create a local list of type RDQRestriction to store the values from the upload
                        while (this.HasDataOnRow(workSheet, row))
                        {
                            parsedRRs.Add(this.ParseUploadRow(workSheet, row));
                            row++;
                        }

                        //continue processing if there is at least 1 record to upload
                        if (parsedRRs.Count() > 0)
                        {
                            // file validation - duplicates, permission for unique divisions, etc
                            if (!this.ValidateFile(parsedRRs, errorList))
                            {
                                Session["errorList"] = errorList;
                                successfulCount = validRRs.Count;
                                warnings = errorList.Where(el => el.Item2.StartsWith("Warning")).ToList();
                                errors = errorList.Except(warnings).ToList();
                                errorMessage = string.Format(
                                    "{0} lines were processed successfully. {1} warnings and {2} errors were found."
                                    , successfulCount
                                    , warnings.Count
                                    , errors.Count);
                                return Content(errorMessage);
                            }

                            // validate the parsed rdq restrictions.  all valid rdq restrictions will be added to validRRs
                            this.ValidateParsedRRs(parsedRRs, validRRs, errorList);

                            if (validRRs.Count() > 0)
                            {
                                foreach (RDQRestriction rr in validRRs)
                                {
                                    this.RevertDefaultValues(rr);
                                    rr.LastModifiedDate = DateTime.Now;
                                    rr.LastModifiedUser = User.Identity.Name;
                                    db.RDQRestrictions.Add(rr);
                                }
                                db.SaveChanges();
                            }

                            successfulCount = validRRs.Count;
                            warnings = errorList.Where(el => el.Item2.StartsWith("Warning")).ToList();
                            errors = errorList.Except(warnings).ToList();

                            // if errors occured, allow user to download them
                            if (errorList.Count > 0)
                            {
                                errorMessage = string.Format(
                                    "{0} lines were processed successfully. {1} warnings and {2} errors were found."
                                    , successfulCount
                                    , warnings.Count
                                    , errors.Count);
                                Session["errorList"] = errorList;
                                return Content(errorMessage);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        FLLogger logger = new FLLogger("C:\\Log\\allocation");
                        logger.Log(string.Format("{0}: {1}", ex.Message, ex.StackTrace), FLLogger.eLogMessageType.eError);
                        message = "Upload failed: One or more columns has unexpected missing or invalid data.";
                        Session["errorList"] = null;
                        return Content(message);
                    }
                }
            }

            message = string.Format("Success! {0} lines were processed.", successfulCount);
            return Json(new { message = message }, "application/json");
        }

        public ActionResult ExcelTemplate()
        {
            License license = new License();
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            string templateFileName = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["RDQRestrictionsTemplate"]);
            FileStream file = new FileStream(Server.MapPath("~/") + templateFileName, FileMode.Open, FileAccess.Read);

            byte[] data = new byte[file.Length];
            file.Read(data, 0, data.Length);
            file.Close();
            MemoryStream memoryStream = new MemoryStream(data);
            excelDocument.Open(memoryStream);
            excelDocument.Save("RDQRestrictionUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

        public ActionResult DownloadErrors()
        {
            int row = 0;
            int col = 0;
            var errorList = (List<Tuple<RDQRestriction, string>>)Session["errorList"];
            if (errorList != null)
            {
                License license = new License();
                license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

                Excel excelDocument = new Excel();
                Worksheet workSheet = excelDocument.Worksheets[0];
                
                workSheet.Cells[row, col].PutValue("Division");
                workSheet.Cells[row, col].Style.Font.IsBold = true;
                col++;
                workSheet.Cells[row, col].PutValue("Department");
                workSheet.Cells[row, col].Style.Font.IsBold = true;
                col++;
                workSheet.Cells[row, col].PutValue("Category");
                workSheet.Cells[row, col].Style.Font.IsBold = true;
                col++;
                workSheet.Cells[row, col].PutValue("Brand");
                workSheet.Cells[row, col].Style.Font.IsBold = true;
                col++;
                workSheet.Cells[row, col].PutValue("Vendor");
                workSheet.Cells[row, col].Style.Font.IsBold = true;
                col++;
                workSheet.Cells[row, col].PutValue("RDQ Type");
                workSheet.Cells[row, col].Style.Font.IsBold = true;
                col++;
                workSheet.Cells[row, col].PutValue("From Date");
                workSheet.Cells[row, col].Style.Font.IsBold = true;
                col++;
                workSheet.Cells[row, col].PutValue("To Date");
                workSheet.Cells[row, col].Style.Font.IsBold = true;
                col++;
                workSheet.Cells[row, col].PutValue("From DC Code");
                workSheet.Cells[row, col].Style.Font.IsBold = true;
                col++;
                workSheet.Cells[row, col].PutValue("To League");
                workSheet.Cells[row, col].Style.Font.IsBold = true;
                col++;
                workSheet.Cells[row, col].PutValue("To Region");
                workSheet.Cells[row, col].Style.Font.IsBold = true;
                col++;
                workSheet.Cells[row, col].PutValue("To Store");
                workSheet.Cells[row, col].Style.Font.IsBold = true;
                col++;
                workSheet.Cells[row, col].PutValue("To DC Code");
                workSheet.Cells[row, col].Style.Font.IsBold = true;
                col++;
                workSheet.Cells[row, col].PutValue("Message");
                workSheet.Cells[row, col].Style.Font.IsBold = true;
                col++;

                row = 1;
                if (errorList != null && errorList.Count() > 0)
                {
                    foreach (var error in errorList)
                    {
                        col = 0;
                        workSheet.Cells[row, col++].PutValue(error.Item1.Division);
                        workSheet.Cells[row, col++].PutValue(error.Item1.Department);
                        workSheet.Cells[row, col++].PutValue(error.Item1.Category);
                        workSheet.Cells[row, col++].PutValue(error.Item1.Brand);
                        workSheet.Cells[row, col++].PutValue(error.Item1.Vendor);
                        workSheet.Cells[row, col++].PutValue(error.Item1.RDQType);
                        if (error.Item1.FromDate.Equals(DateTime.MinValue) || error.Item1.FromDate == null)
                        {
                            workSheet.Cells[row, col++].PutValue(string.Empty);
                        }
                        else
                        {
                            workSheet.Cells[row, col++].PutValue(error.Item1.FromDate);
                        }
                        if (error.Item1.ToDate.Equals(DateTime.MinValue) || error.Item1.ToDate == null)
                        {
                            workSheet.Cells[row, col++].PutValue(string.Empty);
                        }
                        else
                        {
                            workSheet.Cells[row, col++].PutValue(error.Item1.ToDate);
                        }
                        workSheet.Cells[row, col++].PutValue(error.Item1.FromDCCode);
                        workSheet.Cells[row, col++].PutValue(error.Item1.ToLeague);
                        workSheet.Cells[row, col++].PutValue(error.Item1.ToRegion);
                        workSheet.Cells[row, col++].PutValue(error.Item1.ToStore);
                        workSheet.Cells[row, col++].PutValue(error.Item1.ToDCCode);
                        workSheet.Cells[row++, col].PutValue(error.Item2);
                    }

                    for (int i = 0; i < 14; i++)
                    {
                        workSheet.AutoFitColumn(i);
                    }
                }

                excelDocument.Save("RDQRestrictionErrors.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            }
            else
            {
                // if this message is hit that means there was an exception while processing that was not accounted for
                // check the log to see what the exception was
                var message = "An unexpected error has occured.  Please try again or contact an administrator.";
                return RedirectToAction("Upload", new { message = message });
            }

            return View();
        }

        #region Helper Methods (Validation, Population)

        #region Upload Helper Methods

        private Worksheet RetrieveWorkSheet(HttpPostedFileBase file)
        {
            License license = new License();
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            Excel workbook = new Excel();
            byte[] data = new byte[file.InputStream.Length];
            file.InputStream.Read(data, 0, data.Length);
            file.InputStream.Close();
            MemoryStream memoryStream = new MemoryStream(data);
            workbook.Open(memoryStream);
            return workbook.Worksheets[0];
        }

        private bool HasValidHeaderRow(Worksheet workSheet)
        {
            return
                (Convert.ToString(workSheet.Cells[0, 0].Value).Contains("Division")) &&
                (Convert.ToString(workSheet.Cells[0, 1].Value).Contains("Department")) &&
                (Convert.ToString(workSheet.Cells[0, 2].Value).Contains("Category")) &&
                (Convert.ToString(workSheet.Cells[0, 3].Value).Contains("Brand")) &&
                (Convert.ToString(workSheet.Cells[0, 4].Value).Contains("Vendor")) &&
                (Convert.ToString(workSheet.Cells[0, 5].Value).Contains("RDQ Type")) &&
                (Convert.ToString(workSheet.Cells[0, 6].Value).Contains("From Date")) &&
                (Convert.ToString(workSheet.Cells[0, 7].Value).Contains("To Date")) &&
                (Convert.ToString(workSheet.Cells[0, 8].Value).Contains("From DC Code")) &&
                (Convert.ToString(workSheet.Cells[0, 9].Value).Contains("To League")) &&
                (Convert.ToString(workSheet.Cells[0, 10].Value).Contains("To Region")) &&
                (Convert.ToString(workSheet.Cells[0, 11].Value).Contains("To Store")) &&
                (Convert.ToString(workSheet.Cells[0, 12].Value).Contains("To DC Code"));
        }

        private bool HasDataOnRow(Worksheet workSheet, int row)
        {
            return workSheet.Cells[row, 0].Value != null ||
                   workSheet.Cells[row, 1].Value != null ||
                   workSheet.Cells[row, 2].Value != null ||
                   workSheet.Cells[row, 3].Value != null ||
                   workSheet.Cells[row, 4].Value != null ||
                   workSheet.Cells[row, 5].Value != null ||
                   workSheet.Cells[row, 6].Value != null ||
                   workSheet.Cells[row, 7].Value != null ||
                   workSheet.Cells[row, 8].Value != null ||
                   workSheet.Cells[row, 9].Value != null ||
                   workSheet.Cells[row, 10].Value != null ||
                   workSheet.Cells[row, 11].Value != null ||
                   workSheet.Cells[row, 12].Value != null;
        }

        private RDQRestriction ParseUploadRow(Worksheet workSheet, int row)
        {
            RDQRestriction returnValue = null;

            string division = Convert.ToString(workSheet.Cells[row, 0].Value);
            string department = Convert.ToString(workSheet.Cells[row, 1].Value);
            string category = Convert.ToString(workSheet.Cells[row, 2].Value);
            string brand = Convert.ToString(workSheet.Cells[row, 3].Value);
            string vendor = Convert.ToString(workSheet.Cells[row, 4].Value);
            string rdqType = Convert.ToString(workSheet.Cells[row, 5].Value);
            DateTime fromDate = Convert.ToDateTime(workSheet.Cells[row, 6].Value);
            DateTime toDate = Convert.ToDateTime(workSheet.Cells[row, 7].Value);
            string fromDCCode = Convert.ToString(workSheet.Cells[row, 8].Value);
            string toLeague = Convert.ToString(workSheet.Cells[row, 9].Value);
            string toRegion = Convert.ToString(workSheet.Cells[row, 10].Value);
            string toStore = Convert.ToString(workSheet.Cells[row, 11].Value);
            string toDCCode = Convert.ToString(workSheet.Cells[row, 12].Value);

            returnValue = new RDQRestriction(division, department, category, brand, vendor, rdqType,
                                fromDate, toDate, fromDCCode, toLeague, toRegion, toStore, toDCCode);

            return returnValue;
        }

        private bool ValidateFile(List<RDQRestriction> parsedRRs, List<Tuple<RDQRestriction, string>> errorList)
        {
            // remove all records that have a null or empty division... we check to see if users have access
            // to the specified division and cannot do this validation without removing this data scenario
            parsedRRs
                .Where(pr => string.IsNullOrEmpty(pr.Division))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, "Division must be provided.");
                    parsedRRs.Remove(rr);
                });

            // check to see if the user has permission for each division, if not remove them
            string userName = User.Identity.Name.Split('\\')[1];

            List<string> uniqueDivisions
                = parsedRRs
                    .Select(pr => pr.Division)
                    .Distinct()
                    .ToList();

            List<string> invalidDivisions
                = uniqueDivisions
                    .Where(div => !WebSecurityService.UserHasDivision(userName, "Allocation", div))
                    .ToList();

            parsedRRs
                .Where(pr => invalidDivisions.Contains(pr.Division))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, string.Format("You do not have permission for Division {0}", rr.Division));
                    parsedRRs.Remove(rr);
                });

            // check to see if there are any duplicates and remove them
            parsedRRs
                .GroupBy(pr => new { pr.Division, pr.Department, pr.Category, pr.Brand, pr.Vendor, pr.RDQType, pr.FromDCCode, pr.ToLeague, pr.ToRegion, pr.ToStore, pr.ToDCCode })
                .Where(pr => pr.Count() > 1)
                .Select(pr => new { DuplicatesRRs = pr.ToList(), Counter = pr.Count() })
                .ToList()
                .ForEach(rr =>
                {
                    // set error message for first duplicate and the amount of times it was found in the file
                    SetErrorMessage(errorList, rr.DuplicatesRRs.FirstOrDefault(), string.Format(
                        "The following row of data was duplicated in the spreadsheet {0} times.  Please provide unique rows of data.", rr.Counter));
                    // delete all instances of the duplications from the parsedRRs list
                    rr.DuplicatesRRs.ForEach(drr => parsedRRs.Remove(drr));
                });


            if (parsedRRs.Count() == 0)
            {
                return false;
            }

            return true;
        }

        private void SetErrorMessage(List<Tuple<RDQRestriction, string>> errorList, RDQRestriction errorRR, string newErrorMessage)
        {
            int tupleIndex = errorList.FindIndex(err => err.Item1.Equals(errorRR));
            if (tupleIndex > -1)
            {
                errorList[tupleIndex] = Tuple.Create(errorRR, string.Format("{0} {1}", errorList[tupleIndex].Item2, newErrorMessage));
            }
            else
            {
                errorList.Add(Tuple.Create(errorRR, newErrorMessage));
            }
        }

        private void ValidateParsedRRs(List<RDQRestriction> parsedRRs, List<RDQRestriction> validRRs, List<Tuple<RDQRestriction, string>> errorList)
        {
            // check for duplicates in db
            parsedRRs
                .Where(pr => db.RDQRestrictions.Any(r => r.Division.Equals(pr.Division) &&
                            ((r.Department == null && string.IsNullOrEmpty(pr.Department) || r.Department.Equals(pr.Department)) &&
                            ((r.Category == null && string.IsNullOrEmpty(pr.Category)) || r.Category.Equals(pr.Category)) &&
                            ((r.Brand == null && string.IsNullOrEmpty(pr.Brand)) || r.Brand.Equals(pr.Brand)) &&
                            ((r.RDQType == null && string.IsNullOrEmpty(pr.RDQType)) || r.RDQType.Equals(pr.RDQType)) &&
                            ((r.Vendor == null && string.IsNullOrEmpty(pr.Vendor)) || r.Vendor.Equals(pr.Vendor)) &&
                            ((r.FromDCCode == null && string.IsNullOrEmpty(pr.FromDCCode)) || r.FromDCCode.Equals(pr.FromDCCode)) &&
                            ((r.ToDCCode == null && string.IsNullOrEmpty(pr.ToDCCode)) || r.ToDCCode.Equals(pr.ToDCCode)) &&
                            ((r.ToLeague == null && string.IsNullOrEmpty(pr.ToLeague)) || r.ToLeague.Equals(pr.ToLeague)) &&
                            ((r.ToRegion == null && string.IsNullOrEmpty(pr.ToRegion)) || r.ToRegion.Equals(pr.ToRegion)) &&
                            ((r.ToStore == null && string.IsNullOrEmpty(pr.ToStore)) || r.ToStore.Equals(pr.ToStore)))))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, "The combination provided already exists within the system.");
                });



            // required dates are populated
            parsedRRs
                .Where(pr => pr.FromDate == null || pr.FromDate.Equals(DateTime.MinValue))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, "The From Date is required.");
                });

            parsedRRs
                .Where(pr => pr.ToDate == null || pr.ToDate.Equals(DateTime.MinValue))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, "The To Date is required.");
                });
            

            // has valid combination ( div / department / category / brand )
            var uniqueCombos
                = parsedRRs
                    .Select(pr => new { pr.Division, pr.Department, pr.Category, pr.Brand })
                    .Distinct()
                    .ToList();

            // division / department
            var divDeptCombos
                = uniqueCombos
                    .Where(uc => !string.IsNullOrEmpty(uc.Division) &&
                                 !string.IsNullOrEmpty(uc.Department) &&
                                 string.IsNullOrEmpty(uc.Category) &&
                                 string.IsNullOrEmpty(uc.Brand)).ToList();

            var invalidCombos
                = divDeptCombos
                    .Where(uc => !db.ItemMasters.Any(im => im.Div.Equals(uc.Division) &&
                                                           im.Dept.Equals(uc.Department))).ToList();

            parsedRRs
                .Where(pr => invalidCombos.Contains(new { pr.Division, pr.Department, pr.Category, pr.Brand }))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, "The division / department combination does not exist.");
                });

            // division / department / category
            var divDeptCatCombos
                = uniqueCombos
                    .Where(uc => !string.IsNullOrEmpty(uc.Division) &&
                                 !string.IsNullOrEmpty(uc.Department) &&
                                 !string.IsNullOrEmpty(uc.Category) &&
                                 string.IsNullOrEmpty(uc.Brand)).ToList();

            invalidCombos
                = divDeptCatCombos
                    .Where(uc => !db.ItemMasters.Any(im => im.Div.Equals(uc.Division) &&
                                                           im.Dept.Equals(uc.Department) &&
                                                           im.Category.Equals(uc.Category))).ToList();

            parsedRRs
                .Where(pr => invalidCombos.Contains(new { pr.Division, pr.Department, pr.Category, pr.Brand }))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, "The division / department / category combination does not exist.");
                });

            // division / deparment / category / brand
            var divDeptCatBrandCombos
                = uniqueCombos
                    .Where(uc => !string.IsNullOrEmpty(uc.Division) &&
                                 !string.IsNullOrEmpty(uc.Department) &&
                                 !string.IsNullOrEmpty(uc.Category) &&
                                 !string.IsNullOrEmpty(uc.Brand)).ToList();

            invalidCombos
                = divDeptCatBrandCombos
                    .Where(uc => !db.ItemMasters.Any(im => im.Div.Equals(uc.Division) &&
                                                           im.Dept.Equals(uc.Department) &&
                                                           im.Category.Equals(uc.Category) &&
                                                           im.Brand.Equals(uc.Brand))).ToList();

            parsedRRs
                .Where(pr => invalidCombos.Contains(new { pr.Division, pr.Department, pr.Category, pr.Brand }))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, "The division / department / category / brand combination does not exist.");
                });

            // division / brand combination exists
            var divBrandCombos
                = uniqueCombos
                    .Where(uc => !string.IsNullOrEmpty(uc.Division) &&
                                 string.IsNullOrEmpty(uc.Department) &&
                                 string.IsNullOrEmpty(uc.Category) &&
                                 !string.IsNullOrEmpty(uc.Brand)).ToList();

            invalidCombos
                = divBrandCombos
                    .Where(uc => !db.ItemMasters.Any(im => im.Div.Equals(uc.Division) &&
                                                           im.Brand.Equals(uc.Brand))).ToList();

            parsedRRs
                .Where(pr => invalidCombos.Contains(new { pr.Division, pr.Department, pr.Category, pr.Brand }))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, "The division / brand combination does not exist.");
                });

            // division / category combinations
            var divCatCombos
                = uniqueCombos
                    .Where(uc => !string.IsNullOrEmpty(uc.Division) &&
                                 string.IsNullOrEmpty(uc.Department) &&
                                 !string.IsNullOrEmpty(uc.Category) &&
                                 string.IsNullOrEmpty(uc.Brand)).ToList();

            invalidCombos
                = divCatCombos
                    .Where(uc => !db.ItemMasters.Any(im => im.Div.Equals(uc.Division) &&
                                                           im.Category.Equals(uc.Category))).ToList();

            parsedRRs
                .Where(pr => invalidCombos.Contains(new { pr.Division, pr.Department, pr.Category, pr.Brand }))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, "The division / category combination does not exist.");
                });

            // division / vendor combination exists
            var divVendorCombos
                = parsedRRs
                    .Where(pr => !string.IsNullOrEmpty(pr.Vendor))
                    .Select(pr => new { pr.Division, pr.Vendor })
                    .Distinct();

            var invalidVendors
                = divVendorCombos
                    .Where(vc => !db.ItemMasters.Any(im => im.Div.Equals(vc.Division) &&
                                                           im.Vendor.Equals(vc.Vendor)));

            parsedRRs
                .Where(pr => invalidVendors.Contains(new { pr.Division, pr.Vendor }))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, "The division / vendor combination does not exist.");
                });

            // rdq type exists
            var uniqueRDQTypes
                = parsedRRs
                    .Where(pr => !string.IsNullOrEmpty(pr.RDQType))
                    .Select(pr => pr.RDQType)
                    .Distinct()
                    .ToList();

            var invalidRDQTypes
                = uniqueRDQTypes
                    .Where(urt => !db.RDQTypes.Any(rt => rt.RDQTypeName.Equals(urt)))
                    .ToList();

            parsedRRs
                .Where(pr => invalidRDQTypes.Contains(pr.RDQType))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, "The RDQType does not exist.");
                });

            // from dc code exists
            var uniqueFromDCCodes
                = parsedRRs
                    .Where(pr => !string.IsNullOrEmpty(pr.FromDCCode))
                    .Select(pr => pr.FromDCCode)
                    .Distinct()
                    .ToList();

            var invalidFromDCCodes
                = uniqueFromDCCodes
                    .Where(udc => !db.DistributionCenters.Any(dc => dc.MFCode.Equals(udc)))
                    .ToList();

            parsedRRs
                .Where(pr => invalidFromDCCodes.Contains(pr.FromDCCode))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, "The From DC Code is invalid.");
                });

            // division / to league combination exists
            var uniqueToLeagueCombos
                = parsedRRs
                    .Where(pr => !string.IsNullOrEmpty(pr.ToLeague))
                    .Select(pr => new { pr.Division, pr.ToLeague })
                    .Distinct()
                    .ToList();

            var invalidToLeagueCombos
                = uniqueToLeagueCombos
                    .Where(utl => !db.StoreLookups.Any(sl => sl.Division.Equals(utl.Division) &&
                                                             sl.League.Equals(utl.ToLeague))).ToList();

            parsedRRs
                .Where(pr => invalidToLeagueCombos.Contains(new { pr.Division, pr.ToLeague }))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, "The division / to league combination does not exist.");
                });

            // division / to region combination exists
            var uniqueToRegionCombos
                = parsedRRs
                    .Where(pr => !string.IsNullOrEmpty(pr.ToRegion))
                    .Select(pr => new { pr.Division, pr.ToRegion })
                    .Distinct()
                    .ToList();

            var invalidToRegionCombos
                = uniqueToRegionCombos
                    .Where(utr => !db.StoreLookups.Any(sl => sl.Division.Equals(utr.Division) &&
                                                             sl.Region.Equals(utr.ToRegion))).ToList();

            parsedRRs
                .Where(pr => invalidToRegionCombos.Contains(new { pr.Division, pr.ToRegion }))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, "The division / to region does not exist.");
                });

            // division / to store combination exists
            var uniqueToStoreCombos
                = parsedRRs
                    .Where(pr => !string.IsNullOrEmpty(pr.ToStore))
                    .Select(pr => new { pr.Division, pr.ToStore })
                    .Distinct()
                    .ToList();

            var invalidToStoreCombos
                = uniqueToStoreCombos
                    .Where(uts => !db.StoreLookups.Any(sl => sl.Division.Equals(uts.Division) &&
                                                             sl.Store.Equals(uts.ToStore))).ToList();

            parsedRRs
                .Where(pr => invalidToStoreCombos.Contains(new { pr.Division, pr.ToStore }))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, "The division / to store combination does not exist.");
                });

            // to dc code exists
            var uniqueToDCCodes
                = parsedRRs
                    .Where(pr => !string.IsNullOrEmpty(pr.ToDCCode))
                    .Select(pr => pr.ToDCCode)
                    .Distinct()
                    .ToList();

            var invalidDCCodes
                = uniqueToDCCodes
                    .Where(udc => !db.DistributionCenters.Any(dc => dc.MFCode.Equals(udc)))
                    .ToList();

            parsedRRs
                .Where(pr => invalidDCCodes.Contains(pr.ToDCCode))
                .ToList()
                .ForEach(rr =>
                {
                    SetErrorMessage(errorList, rr, "The To DC Code does not exist.");
                });

            // remove all parsedRRs that were found invalid in the code above
            parsedRRs
                .Where(pr => errorList.Any(er => er.Item1.Equals(pr)))
                .ToList()
                .ForEach(rr => parsedRRs.Remove(rr));

            validRRs.AddRange(parsedRRs);
        }

        #endregion

        #region JSON result routines

        public JsonResult GetNewDepartments(string division)
        {
            List<SelectListItem> newDeptList = GetDepartmentsList(division);
            return Json(new SelectList(newDeptList.ToArray(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNewCategories(string division, string department)
        {
            List<SelectListItem> newCategoriesList = GetCategoriesList(division, department);
            return Json(new SelectList(newCategoriesList.ToArray(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNewBrands(string division, string department, string category)
        {
            List<SelectListItem> newBrandsList = GetBrandsList(division, department, category);
            return Json(new SelectList(newBrandsList.ToArray(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNewDistributionCenters(string division)
        {
            List<SelectListItem> newDistributionCenters = GetDistributionCentersList();
            return Json(new SelectList(newDistributionCenters.ToArray(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region UI dropdown lists

        private List<SelectListItem> GetDivisionsList()
        {
            List<SelectListItem> divisionList = new List<SelectListItem>();
            List<Division> userDivList = WebSecurityService.ListUserDivisions(UserName, "Allocation");
            var divsWithDepts = (from a in db.Departments select a.divisionCode).Distinct();


            if (userDivList.Count() > 0)
            {
                foreach (var rec in userDivList)
                {
                    if (divsWithDepts.Contains(rec.DivCode))
                    {
                        divisionList.Add(new SelectListItem { Text = rec.DisplayName, Value = rec.DivCode });
                    }
                }
            }

            return divisionList;
        }

        private List<SelectListItem> GetDepartmentsList(string division)
        {
            List<SelectListItem> departmentList = new List<SelectListItem>();
            List<Departments> departments = new List<Departments>();

            departments = GetValidDepartments(division);

            if (departments.Count() > 0)
            {
                foreach (var rec in departments)
                {
                    departmentList.Add(new SelectListItem { Text = rec.departmentDisplay, Value = rec.departmentCode });
                }
            }
            departmentList = departmentList.OrderBy(d => d.Text).ToList();
            departmentList.Insert(0, new SelectListItem { Text = "Select a Department...", Value = "N/A" });

            return departmentList;
        }

        private List<SelectListItem> GetCategoriesList(string division, string department)
        {
            List<SelectListItem> categoryList = new List<SelectListItem>();
            List<Categories> categories = new List<Categories>();

            categories = GetValidCategories(division, department);

            if (categories.Count() > 0)
            {
                foreach (var rec in categories)
                {
                    categoryList.Add(new SelectListItem { Text = rec.CategoryDisplay, Value = rec.categoryCode });
                }
            }
            categoryList = categoryList.OrderBy(c => c.Text).ToList();
            categoryList.Insert(0, new SelectListItem { Text = "Select a Category...", Value = "N/A" });

            return categoryList;
        }

        private List<SelectListItem> GetBrandsList(string division, string department, string category)
        {
            List<SelectListItem> brandIDList = new List<SelectListItem>();
            List<BrandIDs> brands = new List<BrandIDs>();

            brands = GetValidBrands(division, department, category);

            if (brands.Count() > 0)
            {
                foreach (var rec in brands)
                {
                    brandIDList.Add(new SelectListItem { Text = rec.brandIDDisplay, Value = rec.brandIDCode });
                }
            }
            brandIDList = brandIDList.OrderBy(b => b.Text).ToList();
            brandIDList.Insert(0, new SelectListItem { Text = "Select a Brand...", Value = "N/A" });

            return brandIDList;
        }

        private List<SelectListItem> GetDistributionCentersList()
        {
            List<SelectListItem> distributionCentersList = new List<SelectListItem>();
            List<DistributionCenter> distributionCenters = new List<DistributionCenter>();

            distributionCenters = GetValidDistributionCenters();

            if (distributionCenters.Count() > 0)
            {
                foreach (var dc in distributionCenters)
                {
                    distributionCentersList.Add(new SelectListItem { Text = dc.displayValue, Value = dc.MFCode });
                }
            }
            distributionCentersList = distributionCentersList.OrderBy(d => d.Text).ToList();
            distributionCentersList.Insert(0, new SelectListItem { Text = "Select a Distribution Center...", Value = "N/A" });

            return distributionCentersList;
        }

        private List<SelectListItem> GetRDQTypes()
        {
            List<SelectListItem> rdqTypesList = new List<SelectListItem>();
            List<RDQType> rdqTypes = new List<RDQType>();

            rdqTypes = db.RDQTypes.ToList();

            if (rdqTypes.Count() > 0)
            {
                foreach (var rt in rdqTypes)
                {
                    rdqTypesList.Add(new SelectListItem { Text = rt.RDQTypeName, Value = rt.RDQTypeName });
                }
            }
            rdqTypesList = rdqTypesList.OrderBy(r => r.Text).ToList();
            rdqTypesList.Insert(0, new SelectListItem { Text = "Select an RDQ Type", Value = "N/A" });

            return rdqTypesList;
        }

        #endregion

        #region Valid Combinations

        /// <summary>
        /// Retrieve all valid departments from the ItemMaster given the specified division
        /// </summary>
        /// <param name="division">specified division</param>
        /// <returns>List of valid departments</returns>
        private List<Departments> GetValidDepartments(string division)
        {
            List<Departments> departments = new List<Departments>();

            departments = (from im in db.ItemMasters
                           join d in db.Departments
                             on new { Division = im.Div, Department = im.Dept } equals
                                new { Division = d.divisionCode, Department = d.departmentCode }
                           where im.Div == division &&
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
        /// Retrieve all valid categories from the ItemMaster table given the specified division and department
        /// </summary>
        /// <param name="division">The specified division</param>
        /// <param name="department">The specified department</param>
        /// <returns>List of valid categories</returns>
        private List<Categories> GetValidCategories(string division, string department)
        {
            List<Categories> categories = new List<Categories>();

            categories = (from im in db.ItemMasters
                          join c in db.Categories
                            on new { Category = im.Category, Division = im.Div, Department = im.Dept } equals
                               new { Category = c.categoryCode, Division = c.divisionCode, Department = c.departmentCode }
                          where im.Div == division &&
                                im.Dept == department &&
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
        /// <param name="division">The specified division</param>
        /// <param name="department">The specified department</param>
        /// <param name="category">The specified category</param>
        /// <returns>List of valid brands</returns>
        private List<BrandIDs> GetValidBrands(string division, string department, string category)
        {
            List<BrandIDs> brands = new List<BrandIDs>();

            brands = (from im in db.ItemMasters
                      join b in db.BrandIDs
                        on new { Brand = im.Brand, Division = im.Div, Department = im.Dept } equals
                           new { Brand = b.brandIDCode, Division = b.divisionCode, Department = b.departmentCode }
                      where im.Div == division &&
                            im.Dept == department &&
                            im.Category == category
                      select b).Distinct().ToList();

            return brands;
        }

        private List<DistributionCenter> GetValidDistributionCenters()
        {
            List<DistributionCenter> distributionCenters = new List<DistributionCenter>();

            distributionCenters = db.DistributionCenters.ToList();

            return distributionCenters;
        }

        #endregion

        #endregion
    }
}
