using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Models.Services;
using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Spreadsheets;
using Aspose.Cells;
using Telerik.Web.Mvc.Infrastructure;
using Footlocker.Common.Entities;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Div Logistics,Buyer Planner,Director of Allocation,Admin,Support")]
    public class OverridePOController : AppController
    {               
        readonly ConfigService configService = new ConfigService();

        private List<ExpeditePO> getOverridePOsForDiv(string div)
        {
            List<ExpeditePO> divExpeditePOs = allocDB.ExpeditePOs.Where(ep => ep.Division == div).ToList(); 

            if (divExpeditePOs != null)
            {                
                List<PurchaseOrder> POs = (from a in allocDB.PurchaseOrders
                                           join r in allocDB.ExpeditePOs
                                           on a.PO equals r.PO
                                           where r.Division == div
                                           select a).ToList();

                foreach (ExpeditePO epo in divExpeditePOs)
                {
                    epo.ExpectedDeliveryDate = POs.Where(po => po.PO == epo.PO)
                                                  .Select(po => po.DeliveryDate)
                                                  .FirstOrDefault();
                }
            }

            return divExpeditePOs;
        }

        private List<ExpeditePO> getOverridePOsForSku(string sku)
        {
            List<ExpeditePO> skuExpeditePOs = allocDB.ExpeditePOs.Where(ep => ep.Sku == sku).ToList();

            if (skuExpeditePOs != null)
            {                
                List<PurchaseOrder> POs = (from a in allocDB.PurchaseOrders
                                           join r in allocDB.ExpeditePOs
                                           on a.PO equals r.PO
                                           where r.Sku == sku
                                           select a).ToList();

                foreach (ExpeditePO epo in skuExpeditePOs)
                {
                    epo.ExpectedDeliveryDate = POs.Where(po => po.PO == epo.PO)
                                                  .Select(po => po.DeliveryDate)
                                                  .FirstOrDefault();
                }
            }

            return skuExpeditePOs;
        }

        private List<ExpeditePO> getOverridePOsForPO(string div, string po)
        {
            List<ExpeditePO> poExpeditePOs = allocDB.ExpeditePOs.Where(ep => ep.Division == div && ep.PO == po).ToList();            

            if (poExpeditePOs != null)
            {                
                List<PurchaseOrder> POs = (from a in allocDB.PurchaseOrders
                                           join r in allocDB.ExpeditePOs
                                           on a.PO equals r.PO
                                           where r.Division == div &&
                                                 r.PO == po
                                           select a).ToList();

                foreach (ExpeditePO epo in poExpeditePOs)
                {
                    epo.ExpectedDeliveryDate = POs.Where(sp => sp.PO == epo.PO)
                                                  .Select(sp => sp.DeliveryDate)
                                                  .FirstOrDefault();
                }
            }

            return poExpeditePOs;
        }

        private ExpeditePOModel GetOverridePOsForDiv(string div)
        {
            ExpeditePOModel model = new ExpeditePOModel()
            {
                Divisions = currentUser.GetUserDivisions()
            };

            if (string.IsNullOrEmpty(div))
            {
                if (model.Divisions.Count() > 0)
                    div = model.Divisions[0].DivCode;
            }

            model.CurrentDivision = div;
            model.POs = getOverridePOsForDiv(div);

            return model;
        }

        public ActionResult Index(string div, string message)
        {
            if (message != null)
                ViewData["message"] = message;

            ExpeditePOModel model = new ExpeditePOModel()
            {
                Divisions = currentUser.GetUserDivisions(),
                CurrentDivision = div
            };

            return View(model);
        }

        public ActionResult IndexByPO(string div, string message)
        {
            if (message != null)            
                ViewData["message"] = message;

            ExpeditePOModel model = GetOverridePOsForDiv(div);

            //if (model.POs.Count > 0)
            //{
            //    List<string> uniqueNames = (from l in model.POs
            //                                select l.CreatedBy.Trim()).Distinct().ToList();

            //    List<string> uniqueNames2 = (from l in model.POs
            //                                select l.LastModifiedUser.Trim()).Distinct().ToList();

            //    foreach (string uniqueName in uniqueNames2)
            //        if (!uniqueNames.Contains(uniqueName))
            //            uniqueNames.Add(uniqueName);

            //    Dictionary<string, string> fullNamePairs = LoadUserNames(uniqueNames);

            //    foreach (var item in fullNamePairs)
            //    {
            //        model.POs.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
            //        model.POs.Where(x => x.LastModifiedUser == item.Key).ToList().ForEach(y => y.LastModifiedUser = item.Value);
            //    }
            //}
            return View(model);
        }

        [GridAction]
        public ActionResult _Index(string div)
        {
            ExpeditePOModel model = GetOverridePOsForDiv(div);
            div = model.CurrentDivision;

            model.Headers = (from a in allocDB.ExpeditePOs
                             where a.Division == div
                             select new ExpeditePOHeader
                             {
                                 Sku = a.Sku,
                                 Department = a.Departments,
                                 OverrideDate = a.OverrideDate
                             }).Distinct().OrderBy(o => o.Sku).ToList();
            return View(new GridModel(model.Headers));
        }

        [GridAction]
        public ActionResult _IndexByPO(string div)
        {
            ExpeditePOModel model = GetOverridePOsForDiv(div);

            if (model.POs.Count > 0)
            {
                List<string> uniqueNames = (from l in model.POs
                                            select l.CreatedBy.Trim()).Distinct().ToList();

                List<string> uniqueNames2 = (from l in model.POs
                                             select l.LastModifiedUser.Trim()).Distinct().ToList();

                foreach (string uniqueName in uniqueNames2)
                    if (!uniqueNames.Contains(uniqueName))
                        uniqueNames.Add(uniqueName);

                Dictionary<string, string> fullNamePairs = LoadUserNames(uniqueNames);

                foreach (var item in fullNamePairs)
                {
                    model.POs.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
                    model.POs.Where(x => x.LastModifiedUser == item.Key).ToList().ForEach(y => y.LastModifiedUser = item.Value);
                }
            }

            return View(new GridModel(model.POs));
        }

        [GridAction]
        public ActionResult _POListAjax(string sku)
        {
            List<ExpeditePO> POs = getOverridePOsForSku(sku);

            if (POs.Count > 0)
            {
                List<string> uniqueNames = (from l in POs
                                            select l.CreatedBy.Trim()).Distinct().ToList();
                List<string> uniqueNames2 = (from l in POs
                                             select l.LastModifiedUser.Trim()).Distinct().ToList();

                foreach (string uniqueName in uniqueNames2)
                    if (!uniqueNames.Contains(uniqueName))
                        uniqueNames.Add(uniqueName);

                Dictionary<string, string> fullNamePairs = LoadUserNames(uniqueNames);

                foreach (var item in fullNamePairs)
                {
                    POs.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
                    POs.Where(x => x.LastModifiedUser == item.Key).ToList().ForEach(y => y.LastModifiedUser = item.Value);
                }
            }

            return View(new GridModel(POs));
        }

        public ActionResult Create(string div)
        {
            ExpeditePOModel model = new ExpeditePOModel()
            {
                NewPO = new ExpeditePO()
                {
                    Division = div
                },
                Divisions = currentUser.GetUserDivisions()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ExpeditePOModel model)
        {
            ExistingPODAO dao = new ExistingPODAO(appConfig.EuropeDivisions);
            // look in the mainframe for a PO
            model.ExistingPOs = dao.GetExistingPO(model.NewPO.Division, model.NewPO.PO);
            if (model.ExistingPOs.Count() > 0)
            {
                model.NewPO.Departments = "";
                model.NewPO.Sku = "";
                int skucount = 0;
                foreach (ExistingPO po in model.ExistingPOs)
                {
                    string dept = po.Sku.Substring(3, 2);
                    if (!currentUser.HasDivDept(po.Division, dept))                    
                        model.Message += string.Format("<br>You do not have permission for dept {0}", dept);                    

                    if (!model.NewPO.Departments.Contains(dept))
                    {
                        if (model.NewPO.Departments.Length > 0)                        
                            model.NewPO.Departments += ",";
                        
                        model.NewPO.Departments += dept;
                    }

                    model.NewPO.Sku = po.Sku;
                    skucount++;
                    if (skucount > 1)                    
                        model.NewPO.Sku = "multi-sku-" + model.NewPO.Sku;
                    
                    model.TotalRetail += po.Retail;
                    model.TotalUnits += po.Units;
                }

                if (model.NewPO.Sku.Length > 50)                
                    model.NewPO.Sku = model.NewPO.Sku.Substring(0, 50);
                
                if (model.ExistingPOs.Count > 0)                
                    model.NewPO.ExpectedDeliveryDate = model.ExistingPOs[0].ExpectedDeliveryDate;                

                return View("Verify", model);
            }
            else
            {
                ViewData["message"] = "PO not found";
                model.Divisions = currentUser.GetUserDivisions();
                return View(model);            
            }
        }

        public ActionResult CreateAllPOsForSku(string div)
        {
            ExpeditePOModel model = new ExpeditePOModel()
            {
                NewPO = new ExpeditePO()
                {
                    Division = div
                },
                Divisions = currentUser.GetUserDivisions()
            };
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAllPOsForSku(ExpeditePOModel model)
        {
            ExistingPODAO dao = new ExistingPODAO(appConfig.EuropeDivisions);
            model.ExistingPOs = dao.GetExistingPOsForSku(model.NewPO.Division, model.NewPO.Sku, true);
            model.NewPO.Departments = "";
            foreach (ExistingPO po in model.ExistingPOs)
            {
                SKU poSKU = new SKU(po.Sku);
                string dept = poSKU.Department;
                if (!currentUser.HasDivDept(po.Division, dept))                
                    model.Message += string.Format("<br>You do not have permission for dept {0}", dept);
                
                if (!model.NewPO.Departments.Contains(dept))
                {
                    if (model.NewPO.Departments.Length > 0)                    
                        model.NewPO.Departments += ",";
                    
                    model.NewPO.Departments += dept;
                }
                model.TotalRetail += po.Retail;
                model.TotalUnits += po.Units;
            }

            if (model.ExistingPOs.Count > 0)            
                model.NewPO.ExpectedDeliveryDate = model.ExistingPOs[0].ExpectedDeliveryDate;
            
            model.POs = new List<ExpeditePO>();
            foreach (ExistingPO po in model.ExistingPOs)
            {
                string message = "";
                if (po.DirectToStore)                
                    message = "Direct to store";
                
                model.POs.Add(new ExpeditePO
                { 
                    PO = po.PO, 
                    Sku = po.Sku, 
                    OverrideDate = model.NewPO.OverrideDate, 
                    ExpectedDeliveryDate = po.ExpectedDeliveryDate, 
                    CreateDate = DateTime.Now, 
                    CreatedBy = currentUser.NetworkID, 
                    LastModifiedDate = DateTime.Now,
                    LastModifiedUser = currentUser.NetworkID,
                    TotalRetail = po.Retail, 
                    Division = po.Division, 
                    TotalUnits = po.Units, 
                    ErrorMessage = message 
                });
            }
            model.POs = model.POs.OrderBy(o => o.ExpectedDeliveryDate).ToList();

            return View("VerifyList", model);
        }

        [HttpPost]
        public JsonResult AcceptPO(string div, string PO, string sku, DateTime overrideDate)
        {
            ExistingPODAO dao = new ExistingPODAO(appConfig.EuropeDivisions);
            List<ExistingPO> list = dao.GetExistingPO(div, PO);

            ExpeditePO po;
            po = new ExpeditePO()
            {
                Departments = ""
            };
            
            foreach (ExistingPO det in list)
            {
                SKU detSKU = new SKU(det.Sku);

                if (det.Sku == sku)
                {
                    if (!po.Departments.Contains(detSKU.Department))
                    {
                        if (po.Departments.Length > 0)                        
                            po.Departments += ",";
                        
                        po.Departments += detSKU.Department;
                    }
                    po.ExpectedDeliveryDate = det.ExpectedDeliveryDate;
                    po.Sku = sku;
                    po.PO = det.PO;
                    po.Division = det.Division;
                    po.TotalRetail += det.Retail;
                    po.TotalUnits += det.Units;
                }
            }

            po.OverrideDate = overrideDate;

            int alreadyExists = allocDB.ExpeditePOs.Where(ep => ep.PO == po.PO && ep.Division == po.Division).Count();

            if (alreadyExists > 0)
            {
                po.LastModifiedDate = DateTime.Now;
                po.LastModifiedUser = currentUser.NetworkID;

                allocDB.Entry(po).State = System.Data.EntityState.Modified;
            }                
            else
            {
                po.CreateDate = DateTime.Now;
                po.CreatedBy = currentUser.NetworkID;

                allocDB.ExpeditePOs.Add(po);
            }
                         
            allocDB.SaveChanges();

            return Json("Success");
        }

        [HttpPost]
        public JsonResult RemovePO(string div, string PO)
        {
            ExistingPODAO dao = new ExistingPODAO(appConfig.EuropeDivisions);
            List<ExistingPO> list = dao.GetExistingPO(div, PO);

            ExpeditePO po;
            po = new ExpeditePO();
            foreach (ExistingPO det in list)
            {
                if (!po.Departments.Contains(po.Sku.Substring(3, 2)))
                {
                    if (po.Departments.Length > 0)                    
                        po.Departments += ",";
                    
                    po.Departments += po.Sku.Substring(3, 2);
                }
             
                po.ExpectedDeliveryDate = det.ExpectedDeliveryDate;
                po.Sku = det.Sku;
                po.PO = det.PO;
                po.Division = det.Division;
                po.TotalRetail += det.Retail;
                po.TotalUnits += det.Units;
            }
            ExpeditePO existingRec = allocDB.ExpeditePOs.Where(ep => ep.PO == po.PO && ep.Division == po.Division).FirstOrDefault();
            if (existingRec != null)
            {
                allocDB.ExpeditePOs.Remove(existingRec);
                allocDB.SaveChanges();
            }

            return Json("Success");
        }

        public ActionResult AcceptAll(string sku, DateTime overrideDate)
        {
            ExistingPODAO dao = new ExistingPODAO(appConfig.EuropeDivisions);
            List<ExistingPO> list = dao.GetExistingPOsForSku(sku.Split('-')[0], sku, true);

            ExpeditePO po;
            foreach (ExistingPO det in list)
            {
                po = new ExpeditePO 
                { 
                    ExpectedDeliveryDate = det.ExpectedDeliveryDate, 
                    Sku = det.Sku, 
                    PO = det.PO, 
                    Departments = sku.Split('-')[1], 
                    Division = sku.Split('-')[0], 
                    TotalRetail = det.Retail, 
                    TotalUnits = det.Units,
                    OverrideDate = overrideDate,
                    LastModifiedDate = DateTime.Now,
                    LastModifiedUser = currentUser.NetworkID
                };

                int alreadyExists = allocDB.ExpeditePOs.Where(ep => ep.PO == po.PO && ep.Division == po.Division).Count();
                if (alreadyExists > 0)
                    allocDB.Entry(po).State = System.Data.EntityState.Modified;                
                else
                {
                    po.CreateDate = DateTime.Now;
                    po.CreatedBy = currentUser.NetworkID;

                    allocDB.ExpeditePOs.Add(po);
                }
                    

                allocDB.SaveChanges();
            }
            return RedirectToAction("Index", new { div = sku.Split('-')[0], Sku = sku });
        }

        #region Ask Users about updating ranges after updating PO date.

        public ActionResult CheckForRanges(string div, string sku, string po)
        {
            List<RangePlan> plans =new List<RangePlan>();
            if (!string.IsNullOrEmpty(po))
            {
                ExistingPODAO dao = new ExistingPODAO(appConfig.EuropeDivisions);

                foreach (ExistingPO p in dao.GetExistingPO(div, po))
                {
                    plans.AddRange(allocDB.RangePlans.Where(r => r.Sku == p.Sku).ToList());
                }
            }
            else           
                plans = allocDB.RangePlans.Where(rp => rp.Sku == sku).ToList();            

            if (plans.Count() == 0)            
                return RedirectToAction("Index", new { div = sku.Split('-')[0], Sku = sku });            

            return View(plans);
        }

        [GridAction]
        public ActionResult _DeliveryGroupsAjax(long planid)
        {
            List<DeliveryGroup> groups = allocDB.DeliveryGroups.Where(dg => dg.PlanID == planid).ToList();

            return View(new GridModel(groups));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _SaveBatchEditing([Bind(Prefix = "updated")]IEnumerable<DeliveryGroup> updated)
        {
            if (updated != null)
            {
                foreach (DeliveryGroup d in updated)
                {
                    allocDB.Entry(d).State = System.Data.EntityState.Modified;
                    if (d.EndDate != null)
                    {
                        if (((DateTime)d.EndDate).Year < 2000)
                        {
                            int centuries = ((DateTime.Now.Year - 1900) / 100) * 100;
                            d.EndDate = ((DateTime)d.EndDate).AddYears(centuries);
                        }
                    }

                    (new SkuRangeController()).UpdateDeliveryGroupDates(d);
                }
            }

            return View(new GridModel(updated));
        }
        #endregion

        public ActionResult DeleteSku(string sku, DateTime overrideDate)
        {
            string div;
            if (sku.Contains("multi"))
            {
                div = sku.Split('-')[2];

                return RedirectToAction("Index", new { div, message = "You must delete multi-sku by PO." });
            }
            else            
                div = sku.Split('-')[0];            

            List<ExpeditePO> list = allocDB.ExpeditePOs.Where(ep => ep.OverrideDate == overrideDate && ep.Sku == sku).ToList();

            foreach (ExpeditePO det in list)
            {
                allocDB.ExpeditePOs.Remove(det);
                allocDB.SaveChanges();
            }

            return RedirectToAction("Index", new { div });
        }

        public ActionResult EditSku(string sku, DateTime overrideDate)
        {
            string div;
            if (sku.Contains("multi"))
            {
                div = sku.Split('-')[2];

                return RedirectToAction("Index", new { div, message = "You must edit multi-sku by PO." });
            }
            else            
                div = sku.Split('-')[0];            

            ExpeditePO model = new ExpeditePO()
            {
                Division = div,
                Sku = sku,
                OverrideDate = overrideDate,
                //so we can update only the records we want we need to store the original overrideDate, kind of a hack
                ExpectedDeliveryDate = overrideDate
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSku(ExpeditePO model)
        {            
            List<ExpeditePO> list = allocDB.ExpeditePOs.Where(ep => ep.Division == model.Division && 
                                                                    ep.Sku == model.Sku).ToList();

            foreach (ExpeditePO po in list)
            {
                po.OverrideDate = model.OverrideDate;
                po.LastModifiedDate = DateTime.Now;
                po.LastModifiedUser = currentUser.NetworkID;                                
            }

            allocDB.SaveChanges();

            return RedirectToAction("Index", new { div = model.Division });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAfterVerify(ExpeditePOModel model)
        {
            model.NewPO.CreatedBy = currentUser.NetworkID;
            model.NewPO.CreateDate = DateTime.Now;
            model.NewPO.LastModifiedDate = DateTime.Now;
            model.NewPO.LastModifiedUser = currentUser.NetworkID;

            if (allocDB.ExpeditePOs.Where(ep => ep.PO == model.NewPO.PO && ep.Division == model.NewPO.Division).Count() > 0)
                allocDB.Entry(model.NewPO).State = System.Data.EntityState.Modified;            
            else
            {
                model.NewPO.CreateDate = DateTime.Now;
                model.NewPO.CreatedBy = currentUser.NetworkID;

                allocDB.ExpeditePOs.Add(model.NewPO);
            }                
            
            if (string.IsNullOrEmpty(model.NewPO.Sku))            
                model.NewPO.Sku = "unknown";            

            model.NewPO.PO = model.NewPO.PO.Trim();
            allocDB.SaveChanges();
            
            return RedirectToAction("Index", new { div = model.NewPO.Division });
        }

        public ActionResult Delete(string div, string PO)
        {
            ExpeditePO model = allocDB.ExpeditePOs.Where(ep => ep.Division == div && ep.PO == PO).First();

            allocDB.ExpeditePOs.Remove(model);
            allocDB.SaveChanges();

            return RedirectToAction("Index", new { div });
        }

        public ActionResult Edit(string div, string PO)
        {
            ExpeditePO model = getOverridePOsForPO(div, PO).First();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ExpeditePO model)
        {
            model.PO = model.PO.Trim();

            ExpeditePO edited = allocDB.ExpeditePOs.Where(e => e.PO == model.PO).FirstOrDefault();

            edited.OverrideDate = model.OverrideDate;
            edited.LastModifiedDate = DateTime.Now;
            edited.LastModifiedUser = currentUser.NetworkID;

            allocDB.SaveChanges();
            
            return RedirectToAction("Index", new { div = model.Division });
        }

        #region Upload
        public ActionResult Upload()
        {
            return View();
        }

        public ActionResult ExcelTemplate()
        {
            POOverrideSpreadsheet poOverrideSpreadsheet = new POOverrideSpreadsheet(appConfig, configService, new ExistingPODAO(appConfig.EuropeDivisions));
            Workbook excelDocument;

            excelDocument = poOverrideSpreadsheet.GetTemplate();

            excelDocument.Save(System.Web.HttpContext.Current.Response, "POOverrideUpload.xlsx", ContentDisposition.Attachment, poOverrideSpreadsheet.SaveOptions);
            return View();
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public ActionResult Save(IEnumerable<HttpPostedFileBase> attachments)
        {
            POOverrideSpreadsheet poOverrideSpreadsheet = new POOverrideSpreadsheet(appConfig, configService, new ExistingPODAO(appConfig.EuropeDivisions));

            string message;
            int successCount = 0;

            foreach (HttpPostedFileBase file in attachments)
            {
                poOverrideSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(poOverrideSpreadsheet.message))
                    return Content(poOverrideSpreadsheet.message);
                else
                {
                    if (poOverrideSpreadsheet.errorList.Count() > 0)
                    {
                        Session["errorList"] = poOverrideSpreadsheet.errorList;

                        message = string.Format("{0} successfully uploaded, {1} Errors", poOverrideSpreadsheet.validRecs.Count.ToString(),
                            poOverrideSpreadsheet.errorList.Count.ToString());

                        return Content(message);
                    }
                }

                successCount = poOverrideSpreadsheet.validRecs.Count();
            }

            return Json(new { message = string.Format("{0} PO Override(s) Uploaded", successCount) }, "application/json");
        }

        public ActionResult DownloadErrors()
        {
            List<ExpeditePO> errors = (List<ExpeditePO>)Session["errorList"];
            Workbook excelDocument;
            POOverrideSpreadsheet poOverrideSpreadsheet = new POOverrideSpreadsheet(appConfig, configService, new ExistingPODAO(appConfig.EuropeDivisions));

            if (errors != null)
            {
                excelDocument = poOverrideSpreadsheet.GetErrors(errors);
                excelDocument.Save(System.Web.HttpContext.Current.Response, "POOverrideErrors.xlsx", ContentDisposition.Attachment, poOverrideSpreadsheet.SaveOptions);
            }
            return View();
        }
        #endregion
    }
}
