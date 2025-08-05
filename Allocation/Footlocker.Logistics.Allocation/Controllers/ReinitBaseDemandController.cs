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
using Telerik.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Head Merchandiser,Director of Allocation,Admin,Support")]
    public class ReinitBaseDemandController : AppController
    {
        readonly ConfigService configService = new ConfigService();

        public ActionResult Index()
        {
            List<ReinitializeBaseDemandModel> model = new List<ReinitializeBaseDemandModel>();

            return View(model);
        }

        [GridAction]
        public ActionResult _Details(string sku)
        {
            ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);
            long itemID = itemDAO.GetItemID(sku);

            List<ReinitializeBaseDemand> reinitBaseDemand = new List<ReinitializeBaseDemand>();
            reinitBaseDemand = allocDB.ReinitializeBaseDemand.Where(rbd => rbd.ExtractedInd == false).ToList();

            if (reinitBaseDemand.Count > 0)
            {
                List<string> uniqueNames = (from l in reinitBaseDemand
                                            select l.CreateUser).Distinct().ToList();

                Dictionary<string, string> fullNamePairs = LoadUserNames(uniqueNames);

                foreach (var item in fullNamePairs)
                {
                    reinitBaseDemand.Where(x => x.CreateUser == item.Key).ToList().ForEach(y => y.CreateUser = item.Value);
                }
            }

            return View(new GridModel(reinitBaseDemand));
        }

        [GridAction]
        public ActionResult _ReinitBaseDemandIndex(bool allSKU)
        {
            List<ReinitializeBaseDemandModel> reinitBaseDemand = new List<ReinitializeBaseDemandModel>();
            ReinitializeBaseDemandModel model = new ReinitializeBaseDemandModel();

            if (allSKU)
            {
                reinitBaseDemand = (from a in allocDB.ReinitializeBaseDemand
                                    join b in allocDB.ItemMasters on a.ItemID equals b.ID
                                    orderby b.MerchantSku, a.LastModifiedDate descending
                                    select new ReinitializeBaseDemandModel
                                    {
                                        SKU = b.MerchantSku,
                                        SKUDescription = b.Description,
                                        ReinitializeBaseDemand = a
                                    }).Distinct().ToList();

                if (reinitBaseDemand.Count > 0)
                {
                    List<string> uniqueNames = (from l in reinitBaseDemand
                                                select l.ReinitializeBaseDemand.CreateUser).Distinct().ToList();

                    Dictionary<string, string> fullNamePairs = LoadUserNames(uniqueNames);

                    foreach (var item in fullNamePairs)
                    {
                        reinitBaseDemand.Where(x => x.ReinitializeBaseDemand.CreateUser == item.Key).ToList().ForEach(y => y.ReinitializeBaseDemand.CreateUser = item.Value);
                    }
                }
            }
            else
            {
                var baseDemandGroups = from rbd in allocDB.ReinitializeBaseDemand
                                       where rbd.ExtractedInd == false
                                       group rbd by new { rbd.ItemID } into grp
                                       select new
                                       {
                                           grp.Key.ItemID
                                       };

                reinitBaseDemand = (from a in baseDemandGroups
                                    join b in allocDB.ItemMasters on a.ItemID equals b.ID
                                    select new ReinitializeBaseDemandModel
                                    {
                                        SKU = b.MerchantSku,
                                        SKUDescription = b.Description
                                    }).Distinct().ToList();

                //reinitBaseDemand = (from a in allocDB.ReinitializeBaseDemand
                //                    join b in allocDB.ItemMasters on a.ItemID equals b.ID
                //                    where a.ExtractedInd == false
                //                    orderby b.MerchantSku, a.LastModifiedDate descending
                //                    select new ReinitializeBaseDemandModel
                //                    {
                //                        SKU = b.MerchantSku,
                //                        SKUDescription = b.Description,
                //                        ReinitializeBaseDemand = a
                //                    }).Distinct().ToList();
            }

            return View(new GridModel(reinitBaseDemand));
        }

        public ActionResult Create()
        {
            ReinitializeBaseDemandModel model = new ReinitializeBaseDemandModel()
            {
                ReinitializeBaseDemand = new ReinitializeBaseDemand()
            };            

            return View(model);
        }

        private void ValidateInput(ReinitializeBaseDemandModel model)
        {            
            ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);
            StoreDAO storeDAO = new StoreDAO();

            model.ReinitializeBaseDemand.ItemID = itemDAO.GetItemID(model.SKU);

            if (model.ReinitializeBaseDemand.ItemID == 0)            
                ModelState.AddModelError("SKU", "Invalid SKU: it was not found in the database");
            else            
                model.ReinitializeBaseDemand.Division = itemDAO.GetItem(model.ReinitializeBaseDemand.ItemID).Div;
            
            if (storeDAO.GetValidStore(model.ReinitializeBaseDemand.Division, model.ReinitializeBaseDemand.Store) == null)
                ModelState.AddModelError("ReinitializeBaseDemand.Store", "Invalid Store: it was not found in the database");

            if (!itemDAO.DoValidSizesExist(model.SKU, model.ReinitializeBaseDemand.Size))
                ModelState.AddModelError("ReinitializeBaseDemand.Size", "Invalid Size: it was not found in the database for this SKU");

            if (model.ReinitializeBaseDemand.BaseDemand <= 0)
                ModelState.AddModelError("ReinitializeBaseDemand.BaseDemand", "Invalid Base Demand: it must be greater than 0");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ReinitializeBaseDemandModel model)
        {
            ValidateInput(model);

            if (ModelState.IsValid)
            {
                // validate item to make sure it is okay and put the error in the right place                
                int recCount = allocDB.ReinitializeBaseDemand.Where(rbd => rbd.ItemID == model.ReinitializeBaseDemand.ItemID &&
                                                                           rbd.Size == model.ReinitializeBaseDemand.Size &&
                                                                           rbd.Store == model.ReinitializeBaseDemand.Store).Count();

                if (recCount > 0)
                {
                    ModelState.AddModelError("SKU", "There is a record for this SKU/Size/Store already.");
                    return View(model);
                }
                else
                {
                    model.ReinitializeBaseDemand.CreateDateTime = DateTime.Now;
                    model.ReinitializeBaseDemand.CreateUser = currentUser.NetworkID;
                    model.ReinitializeBaseDemand.LastModifiedDate = DateTime.Now;
                    model.ReinitializeBaseDemand.LastModifiedUser = currentUser.NetworkID;

                    allocDB.ReinitializeBaseDemand.Add(model.ReinitializeBaseDemand);
                    allocDB.SaveChanges();
                }
            }
            else
                return View(model);

            return RedirectToAction("Index");
        }
        
        public ActionResult Delete(string id)
        {
            long baseDemandID = long.Parse(id);
            ReinitializeBaseDemand record = allocDB.ReinitializeBaseDemand.Where(rbd => rbd.ReinitializeBaseDemandID == baseDemandID).FirstOrDefault();

            if (record == null)
                ViewData["message"] = string.Format("Could not find ID {0} to delete.", id);
            else
            {
                allocDB.ReinitializeBaseDemand.Remove(record);
                allocDB.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult DeleteSKU(string id)
        {
            ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);
            long itemID = itemDAO.GetItemID(id);

            if (itemID == 0)
                ViewData["message"] = string.Format("Could not find the SKU {0} to delete.", id);
            else
            {
                List<ReinitializeBaseDemand> recordList = allocDB.ReinitializeBaseDemand.Where(rbd => rbd.ItemID == itemID && rbd.ExtractedInd == false).ToList();
                
                foreach (ReinitializeBaseDemand record in recordList)
                {
                    allocDB.ReinitializeBaseDemand.Remove(record);
                }
                
                allocDB.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        #region Upload
        public ActionResult Upload()
        {
            return View();
        }

        public ActionResult ExcelTemplate()
        {
            BaseDemandSpreadsheet baseDemandSpreadsheet = new BaseDemandSpreadsheet(appConfig, configService, new ItemDAO(appConfig.EuropeDivisions), new StoreDAO());
            Workbook excelDocument;

            excelDocument = baseDemandSpreadsheet.GetTemplate();

            excelDocument.Save(System.Web.HttpContext.Current.Response, "BaseDemandUpload.xlsx", ContentDisposition.Attachment, baseDemandSpreadsheet.SaveOptions);
            return View();
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public ActionResult Save(IEnumerable<HttpPostedFileBase> attachments)
        {
            BaseDemandSpreadsheet baseDemandSpreadsheet = new BaseDemandSpreadsheet(appConfig, configService, new ItemDAO(appConfig.EuropeDivisions), new StoreDAO());

            string message;
            int successCount = 0;

            foreach (HttpPostedFileBase file in attachments)
            {
                baseDemandSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(baseDemandSpreadsheet.message))
                    return Content(baseDemandSpreadsheet.message);
                else
                {
                    if (baseDemandSpreadsheet.errorList.Count() > 0)
                    {
                        Session["errorList"] = baseDemandSpreadsheet.errorList;

                        message = string.Format("{0} successfully uploaded, {1} Errors", baseDemandSpreadsheet.validBaseDemand.Count.ToString(),
                            baseDemandSpreadsheet.errorList.Count.ToString());

                        return Content(message);
                    }
                }

                successCount = baseDemandSpreadsheet.validBaseDemand.Count();
            }

            return Json(new { message = string.Format("{0} Base Demand Override(s) Uploaded", successCount) }, "application/json");
        }

        public ActionResult DownloadErrors()
        {
            List<ReinitializeBaseDemand> errors = (List<ReinitializeBaseDemand>)Session["errorList"];
            Workbook excelDocument;
            BaseDemandSpreadsheet baseDemandSpreadsheet = new BaseDemandSpreadsheet(appConfig, configService, new ItemDAO(appConfig.EuropeDivisions), new StoreDAO());

            if (errors != null)
            {
                excelDocument = baseDemandSpreadsheet.GetErrors(errors);
                excelDocument.Save(System.Web.HttpContext.Current.Response, "BaseDemandOverrideErrors.xlsx", ContentDisposition.Attachment, baseDemandSpreadsheet.SaveOptions);
            }
            return View();
        }
        #endregion
    }
}
