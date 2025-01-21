using Aspose.Cells;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Models.Services;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Spreadsheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Director of Allocation,Admin,Support")]
    public class EcomRFRestrictionsController : AppController
    {
        readonly ConfigService configService = new ConfigService();

        public ActionResult Index()
        {
            EcomRFRestrictionModel model = new EcomRFRestrictionModel();

            var restrictionList = (from rfRestr in allocDB.EcomRFRestictions
                                   join item in allocDB.ItemMasters on rfRestr.ItemID equals item.ID
                                   select new
                                   {
                                       SKU = item.MerchantSku,
                                       SKUDescription = item.Description,
                                       rfRestr.EcomRFRestrictionID,
                                       rfRestr.ItemID,
                                       rfRestr.StartDate,
                                       rfRestr.EndDate,
                                       rfRestr.LastModifiedDate,
                                       rfRestr.LastModifiedUser
                                   }).ToList();
            
            model.ecomRFRestrictionList = new List<EcomRFRestriction>();

            foreach (var restriction in restrictionList)
            {
                model.ecomRFRestrictionList.Add(new EcomRFRestriction()
                {
                    SKU = restriction.SKU,
                    SKUDescription = restriction.SKUDescription,
                    EcomRFRestrictionID = restriction.EcomRFRestrictionID,
                    ItemID = restriction.ItemID,
                    StartDate = restriction.StartDate,
                    EndDate = restriction.EndDate,
                    LastModifiedDate = restriction.LastModifiedDate,
                    LastModifiedUser = restriction.LastModifiedUser
                });   
            }                                          

            if (model.ecomRFRestrictionList.Count > 0)
            {
                List<string> uniqueNames = (from l in model.ecomRFRestrictionList
                                            select l.LastModifiedUser).Distinct().ToList();
                Dictionary<string, string> fullNamePairs = LoadUserNames(uniqueNames);

                foreach (var item in fullNamePairs)
                {
                    model.ecomRFRestrictionList.Where(x => x.LastModifiedUser == item.Key).ToList().ForEach(y => y.LastModifiedUser = item.Value);
                }
            }

            return View(model.ecomRFRestrictionList);
        }

        public ActionResult Create()
        {
            EcomRFRestrictionModel model = new EcomRFRestrictionModel()
            {
                ecomRFRestriction = new EcomRFRestriction()
            };

            return View(model.ecomRFRestriction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EcomRFRestriction model)
        {
            ViewData["message"] = ValidateInput(model);

            if (string.IsNullOrEmpty(ViewData["message"].ToString()))
            {
                // validate item to make sure it is okay and put the error in the right place

                ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);                
                model.ItemID = itemDAO.GetItemID(model.SKU);

                if (model.ItemID == 0)                
                    ModelState.AddModelError("SKU", "Invalid SKU: it was not found in the database");
                else
                {
                    int recCount = allocDB.EcomRFRestictions.Where(erfr => erfr.ItemID == model.ItemID).Count();

                    if (recCount > 0)
                    {
                        ModelState.AddModelError("SKU", "There is a record for this SKU already. Either edit the existing one or use a different SKU");
                        return View(model);
                    }                        
                    else
                    {
                        model.StartDate = model.StartDate;
                        model.EndDate = model.EndDate;
                        model.LastModifiedDate = DateTime.Now;
                        model.LastModifiedUser = currentUser.NetworkID;

                        allocDB.EcomRFRestictions.Add(model);
                        allocDB.SaveChanges();
                    }
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult Edit(long EcomRFRestrictionID)
        {
            ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);            

            EcomRFRestrictionModel model = new EcomRFRestrictionModel()
            {
                ecomRFRestriction = allocDB.EcomRFRestictions.Where(erfr => erfr.EcomRFRestrictionID == EcomRFRestrictionID).FirstOrDefault()
            };

            ItemMaster item = itemDAO.GetItem(model.ecomRFRestriction.ItemID);
            model.ecomRFRestriction.SKU = item.MerchantSku;
            model.ecomRFRestriction.SKUDescription = item.Description;

            return View(model.ecomRFRestriction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EcomRFRestriction model)
        {
            ViewData["message"] = ValidateInput(model);

            if (!string.IsNullOrEmpty(ViewData["message"].ToString()))
                return View(model);

            EcomRFRestriction record = allocDB.EcomRFRestictions.Where(erfr => erfr.EcomRFRestrictionID == model.EcomRFRestrictionID).FirstOrDefault();

            record.SKU = model.SKU;
            record.LastModifiedUser = currentUser.NetworkID;
            record.LastModifiedDate = DateTime.Now;
            record.StartDate = model.StartDate;
            record.EndDate = model.EndDate;

            allocDB.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Delete(long ecomRFRestrictionID)
        {
            EcomRFRestriction record = allocDB.EcomRFRestictions.Where(erfr => erfr.EcomRFRestrictionID == ecomRFRestrictionID).FirstOrDefault();

            if (record == null)            
                ViewData["message"] = string.Format("Could not find ID {0} to delete.", ecomRFRestrictionID.ToString());            
            else
            {
                allocDB.EcomRFRestictions.Remove(record);
                allocDB.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        private string ValidateInput(EcomRFRestriction model)
        {
            string returnValue = string.Empty;

            if (model.EndDate.HasValue)
            {
                if (model.EndDate < model.StartDate)                
                    returnValue = "Your End Date cannot be before your Start Date.";                
            }

            return returnValue;
        }

        #region Upload
        public ActionResult Upload()
        {
            return View();
        }

        public ActionResult ExcelTemplate()
        {
            EcomRFRestrictionSpreadsheet ecomRFRestrictionSpreadsheet = new EcomRFRestrictionSpreadsheet(appConfig, configService, new ItemDAO(appConfig.EuropeDivisions));
            Workbook excelDocument;

            excelDocument = ecomRFRestrictionSpreadsheet.GetTemplate();

            excelDocument.Save(System.Web.HttpContext.Current.Response, "EcomRFRestrictionUpload.xlsx", ContentDisposition.Attachment, ecomRFRestrictionSpreadsheet.SaveOptions);
            return View();
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public ActionResult Save(IEnumerable<HttpPostedFileBase> attachments)
        {
            EcomRFRestrictionSpreadsheet ecomRFRestrictionSpreadsheet = new EcomRFRestrictionSpreadsheet(appConfig, configService, new ItemDAO(appConfig.EuropeDivisions));

            string message;
            int successCount = 0;

            foreach (HttpPostedFileBase file in attachments)
            {
                ecomRFRestrictionSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(ecomRFRestrictionSpreadsheet.message))
                    return Content(ecomRFRestrictionSpreadsheet.message);
                else
                {
                    if (ecomRFRestrictionSpreadsheet.errorList.Count() > 0)
                    {
                        Session["errorList"] = ecomRFRestrictionSpreadsheet.errorList;

                        message = string.Format("{0} successfully uploaded, {1} Errors", ecomRFRestrictionSpreadsheet.validRecs.Count.ToString(),
                            ecomRFRestrictionSpreadsheet.errorList.Count.ToString());

                        return Content(message);
                    }
                }

                successCount = ecomRFRestrictionSpreadsheet.validRecs.Count();
            }

            return Json(new { message = string.Format("{0} Ecom Ring Fence Exception(s) Uploaded", successCount) }, "application/json");
        }

        public ActionResult DownloadErrors()
        {
            List<EcomRFRestriction> errors = (List<EcomRFRestriction>)Session["errorList"];
            Workbook excelDocument;
            EcomRFRestrictionSpreadsheet ecomRFRestrictionSpreadsheet = new EcomRFRestrictionSpreadsheet(appConfig, configService, new ItemDAO(appConfig.EuropeDivisions));

            if (errors != null)
            {
                excelDocument = ecomRFRestrictionSpreadsheet.GetErrors(errors);
                excelDocument.Save(System.Web.HttpContext.Current.Response, "EcomRFRestrictionErrors.xlsx", ContentDisposition.Attachment, ecomRFRestrictionSpreadsheet.SaveOptions);
            }
            return View();
        }
        #endregion
    }
}
