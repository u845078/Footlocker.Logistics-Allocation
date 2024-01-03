using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aspose.Cells;
using Footlocker.Logistics.Allocation.Spreadsheets;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class UploadController : AppController
    {
        #region Fields
        readonly private AllocationLibraryContext db;
        readonly private ConfigService configService = new ConfigService();
        #endregion

        public UploadController()
        {
            db = new AllocationLibraryContext();
        }

        #region Service Type
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Director of Allocation,Admin,Support")]
        public ActionResult SkuTypeUpload()
        {
            return View();
        }

        public ActionResult ExcelTemplate()
        {
            Workbook excelDocument;
            ServiceTypeSpreadsheet serviceTypeSpreadsheet = new ServiceTypeSpreadsheet(appConfig, configService, new MainframeDAO(appConfig.EuropeDivisions));

            excelDocument = serviceTypeSpreadsheet.GetTemplate();

            excelDocument.Save(System.Web.HttpContext.Current.Response, "ServiceTypeUpload.xlsx", ContentDisposition.Attachment, serviceTypeSpreadsheet.SaveOptions);
            return View();
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Director of Allocation,Admin,Support")]
        public ActionResult Save(IEnumerable<HttpPostedFileBase> attachments)
        {
            ServiceTypeSpreadsheet serviceTypeSpreadsheet = new ServiceTypeSpreadsheet(appConfig, configService, new MainframeDAO(appConfig.EuropeDivisions));

            foreach (HttpPostedFileBase file in attachments)
            {
                serviceTypeSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(serviceTypeSpreadsheet.message))
                    return Content(serviceTypeSpreadsheet.message);
            }

            return Content("");
        }
        #endregion

        #region AR SKU
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult ARSkusUpload()
        {
            return View();
        }

        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult ExcelARSkusUploadTemplate()
        {
            ARSKUSpreadsheet arSKUSpreadsheet = new ARSKUSpreadsheet(appConfig, configService);
            Workbook excelDocument;

            excelDocument = arSKUSpreadsheet.GetTemplate();
            excelDocument.Save(System.Web.HttpContext.Current.Response, "ARSkusUpload.xlsx", ContentDisposition.Attachment, arSKUSpreadsheet.SaveOptions);
            return View("ARSkusUpload");
        }

        [CheckPermission(Roles = "Merchandiser, Head Merchandiser, Buyer Planner, Director of Allocation, Admin, Support")]
        public ActionResult SaveARSkus(IEnumerable<HttpPostedFileBase> attachments)
        {
            ARSKUSpreadsheet arSKUSpreadsheet = new ARSKUSpreadsheet(appConfig, configService);
            string message = string.Empty;

            foreach (HttpPostedFileBase file in attachments)
            {
                arSKUSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(arSKUSpreadsheet.message))
                    message = string.Format("Upload failed: {0}", arSKUSpreadsheet.message);
            }

            return Content(message);
        }
        #endregion

        #region AR Constraints
        [CheckPermission(Roles = "Merchandiser, Head Merchandiser, Buyer Planner, Director of Allocation, Admin, Support")]
        public ActionResult ARConstraintsUpload()
        {
            return View();
        }

        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult ExcelARConstraintsUploadTemplate()
        {
            ARConstraintsSpreadsheet arConstraintsSpreadsheet = new ARConstraintsSpreadsheet(appConfig, configService);
            Workbook excelDocument;

            excelDocument = arConstraintsSpreadsheet.GetTemplate();
            excelDocument.Save(System.Web.HttpContext.Current.Response, "ARConstraintsUpload.xlsx", ContentDisposition.Attachment, arConstraintsSpreadsheet.SaveOptions);            
            return View("ARConstraintsUpload");
        }

        [CheckPermission(Roles = "Merchandiser, Head Merchandiser, Buyer Planner, Director of Allocation, Admin, Support")]
        public ActionResult SaveARConstraints(IEnumerable<HttpPostedFileBase> attachments)
        {
            ARConstraintsSpreadsheet arConstraintsSpreadsheet = new ARConstraintsSpreadsheet(appConfig, configService);
            string message = string.Empty;

            foreach (HttpPostedFileBase file in attachments)
            {
                arConstraintsSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(arConstraintsSpreadsheet.message))
                    message = string.Format("Upload failed: {0}", arConstraintsSpreadsheet.message);
            }

            return Content(message);
        }
        #endregion

        #region Life of SKU
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult ProductTypeUpload()
        {
            return View();
        }

        public ActionResult ExcelProductTemplate()
        {
            ProductTypeSpreadsheet productTypeSpreadsheet = new ProductTypeSpreadsheet(appConfig, configService, new ProductTypeDAO());
            Workbook excelDocument;

            excelDocument = productTypeSpreadsheet.GetTemplate();
            excelDocument.Save(System.Web.HttpContext.Current.Response, "LifeOfSkuTemplate.xlsx", ContentDisposition.Attachment, productTypeSpreadsheet.SaveOptions);
            return View();
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult SaveProductType(IEnumerable<HttpPostedFileBase> attachments)
        {
            ProductTypeSpreadsheet productTypeSpreadsheet = new ProductTypeSpreadsheet(appConfig, configService, new ProductTypeDAO());

            foreach (HttpPostedFileBase file in attachments)
            {
                productTypeSpreadsheet.Save(file);

                if (productTypeSpreadsheet.errorRows.Count() > 0)
                {
                    Session["errorList"] = productTypeSpreadsheet.errorRows;
                    return Content(string.Format("{0} errors on spreadsheet ({1} successfully uploaded)", productTypeSpreadsheet.errorRows.Count(), productTypeSpreadsheet.validRows.Count()));
                }
            }

            return Content("");
        }

        public ActionResult DownloadProductErrors()
        {
            ProductTypeSpreadsheet productTypeSpreadsheet = new ProductTypeSpreadsheet(appConfig, configService, new ProductTypeDAO());
            Workbook excelDocument;

            List<ProductType> errorList = new List<ProductType>();

            if (Session["errorList"] != null)
                errorList = (List<ProductType>)Session["errorList"];

            excelDocument = productTypeSpreadsheet.GetErrors(errorList);
            excelDocument.Save(System.Web.HttpContext.Current.Response, "ProductTypeUploadErrors.xlsx", ContentDisposition.Attachment, productTypeSpreadsheet.SaveOptions);

            return View();
        }
        #endregion

        #region Crossdock Link Upload
        public ActionResult CrossdockLinkUpload()
        {
            return View();
        }

        public ActionResult CrossdockLinkUploadTemplate()
        {
            CrossdockLinkSpreadsheet crossdockLinkSpreadsheet = new CrossdockLinkSpreadsheet(appConfig, configService);
            Workbook excelDocument;

            excelDocument = crossdockLinkSpreadsheet.GetTemplate();
            excelDocument.Save(System.Web.HttpContext.Current.Response, "CrossdockLinkUpload.xlsx", ContentDisposition.Attachment, crossdockLinkSpreadsheet.SaveOptions);
            return View("CrossdockLinkUpload");
        }

        public ActionResult SaveCrossdockLinks(IEnumerable<HttpPostedFileBase> attachments)
        {
            CrossdockLinkSpreadsheet crossdockLinkSpreadsheet = new CrossdockLinkSpreadsheet(appConfig, configService);
            string message = string.Empty;

            foreach (HttpPostedFileBase file in attachments)
            {
                crossdockLinkSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(crossdockLinkSpreadsheet.message))
                    message = string.Format("Upload failed: {0}", crossdockLinkSpreadsheet.message);
                else
                {
                    if (crossdockLinkSpreadsheet.errorList.Count() > 0)
                    {
                        Session["errorList"] = crossdockLinkSpreadsheet.errorList;

                        message = string.Format("{0} successfully uploaded, {1} Errors", crossdockLinkSpreadsheet.validPOCrossdocks.Count.ToString(),
                            crossdockLinkSpreadsheet.errorList.Count.ToString());
                    }
                }
            }

            return Content(message);
        }

        public ActionResult DownloadCrossdockLinkErrors()
        {
            List<POCrossdockData> errors = (List<POCrossdockData>)Session["errorList"];
            Workbook excelDocument;
            CrossdockLinkSpreadsheet crossdockLinkSpreadsheet = new CrossdockLinkSpreadsheet(appConfig, configService);

            if (errors != null)
            {
                excelDocument = crossdockLinkSpreadsheet.GetErrors(errors);
                excelDocument.Save(System.Web.HttpContext.Current.Response, "CrossdockLinkErrors.xlsx", ContentDisposition.Attachment, crossdockLinkSpreadsheet.SaveOptions);
            }
            return View();
        }
        #endregion

        #region SKU ID Upload
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult SkuIdUpload()
        {
            string authDivs;
            try
            {
                authDivs = configService.GetValue(1, "SKUID_UPLOAD_AUTHORIZED_DIVS");
            }
            catch (Exception ex)
            {
                return Redirect("~/Error/GenericallyDenied?message=" + ex.Message);
            }
            
            string[] divisions = authDivs.Split(',');
            bool canLoad = false;

            foreach (string div in divisions)
            {
                if (currentUser.HasDivision(AppName, div))
                {
                    canLoad = true;
                    break;
                }
            }

            if (!canLoad)
            {
                string message = string.Format("You need access to one of the following divisions to access this page: {0}", authDivs);
                return Redirect("~/Error/GenericallyDenied?message=" + message);
            }

            ViewBag.ValidDivisions = db.AllocationDivisions.Where(ad => divisions.Contains(ad.DivisionCode))
                                                            .OrderBy(ad => ad.DivisionCode)
                                                            .ToList();
            return View();
        }

        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult ExcelSkuIdUploadTemplate()
        {
            SKUIDSpreadsheet skuIDSpreadsheet = new SKUIDSpreadsheet(appConfig, configService);
            Workbook excelDocument;

            excelDocument = skuIDSpreadsheet.GetTemplate();
            excelDocument.Save(System.Web.HttpContext.Current.Response, "SkuIdUpload.xlsx", ContentDisposition.Attachment, skuIDSpreadsheet.SaveOptions);
            return RedirectToAction("SkuIdUpload");
        }

        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult SaveSkuId(IEnumerable<HttpPostedFileBase> attachments)
        {
            SKUIDSpreadsheet skuIDSpreadsheet = new SKUIDSpreadsheet(appConfig, configService);
            string message = string.Empty;

            foreach (HttpPostedFileBase file in attachments)
            {
                skuIDSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(skuIDSpreadsheet.message))
                    message = string.Format("Upload failed: {0}", skuIDSpreadsheet.message);
            }

            return Content(message);
        }
        #endregion
    }
}