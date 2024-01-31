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

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Div Logistics,Buyer Planner,Director of Allocation,Admin,Support")]
    public class OverridePOController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
        readonly ConfigService configService = new ConfigService();

        private List<ExpeditePO> getOverridePOsForDiv(string div)
        {
            List<ExpeditePO> results = db.ExpeditePOs.Where(ep => ep.Division == div).ToList(); 

            if (results != null)
            {
                var poData = (from a in db.POs
                              join r in db.ExpeditePOs
                               on a.PO equals r.PO
                              where r.Division == div
                              select a).ToList();

                foreach (ExpeditePO epo in results)
                {
                    var poDeliveryDate = poData.Where(pd => pd.PO == epo.PO)
                                               .Select(pd => pd.DeliveryDate)
                                               .FirstOrDefault();

                    if (poDeliveryDate != DateTime.MinValue)
                        epo.DeliveryDate = poDeliveryDate;
                }
            }

            return results;
        }

        private List<ExpeditePO> getOverridePOsForSku(string sku)
        {
            List<ExpeditePO> results = db.ExpeditePOs.Where(ep => ep.Sku == sku).ToList();

            if (results != null)
            {
                var poData = (from a in db.POs
                              join r in db.ExpeditePOs
                               on a.PO equals r.PO
                              where r.Sku == sku
                              select a).ToList();

                foreach (ExpeditePO epo in results)
                {
                    var poDeliveryDate = poData.Where(pd => pd.PO == epo.PO)
                                               .Select(pd => pd.DeliveryDate)
                                               .FirstOrDefault();

                    if (poDeliveryDate != DateTime.MinValue)
                        epo.DeliveryDate = poDeliveryDate;
                }
            }

            return results;
        }

        private List<ExpeditePO> getOverridePOsForPO(string div, string po)
        {
            List<ExpeditePO> results;
            results = db.ExpeditePOs.Where(ep => ep.Division == div && ep.PO == po).ToList();

            if (results != null)
            {
                var poData = (from a in db.POs
                              join r in db.ExpeditePOs
                               on a.PO equals r.PO
                              where r.PO == po
                              select a).ToList();

                foreach (ExpeditePO epo in results)
                {
                    var poDeliveryDate = poData.Where(pd => pd.PO == epo.PO)
                                               .Select(pd => pd.DeliveryDate)
                                               .FirstOrDefault();

                    if (poDeliveryDate != DateTime.MinValue)
                        epo.DeliveryDate = poDeliveryDate;
                }
            }

            return results;
        }

        public ActionResult Index(string div, string message)
        {
            if (message != null)            
                ViewData["message"] = message;            

            ExpeditePOModel model = new ExpeditePOModel()
            {
                Divisions = currentUser.GetUserDivisions(AppName)
            };
            
            if (string.IsNullOrEmpty(div))
            {
                if (model.Divisions.Count() > 0)                
                    div = model.Divisions[0].DivCode;                
            }
            model.CurrentDivision = div;
            model.POs = getOverridePOsForDiv(div);

            return View(model);
        }

        public ActionResult IndexByPO(string div, string message)
        {
            if (message != null)            
                ViewData["message"] = message;
            
            ExpeditePOModel model = new ExpeditePOModel()
            {
                Divisions = currentUser.GetUserDivisions(AppName)
            };

            if (string.IsNullOrEmpty(div))
            {
                if (model.Divisions.Count() > 0)                
                    div = model.Divisions[0].DivCode;                
            }
            model.CurrentDivision = div;
            model.POs = getOverridePOsForDiv(div);
            return View(model);
        }

        [GridAction]
        public ActionResult _Index(string div)
        {
            ExpeditePOModel model = new ExpeditePOModel()
            {
                Divisions = currentUser.GetUserDivisions(AppName)
            };

            if (string.IsNullOrEmpty(div))
            {
                if (model.Divisions.Count() > 0)
                    div = model.Divisions[0].DivCode;
            }
            model.CurrentDivision = div;
            model.POs = getOverridePOsForDiv(div);
            model.Headers = (from a in db.ExpeditePOs
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
            ExpeditePOModel model = new ExpeditePOModel()
            {
                Divisions = currentUser.GetUserDivisions(AppName)
            };

            if (string.IsNullOrEmpty(div))
            {
                if (model.Divisions.Count() > 0)
                    div = model.Divisions[0].DivCode;
            }
            model.CurrentDivision = div;
            model.POs = getOverridePOsForDiv(div);
            return View(new GridModel(model.POs));
        }

        [GridAction]
        public ActionResult _POListAjax(string sku)
        {
            var POs = getOverridePOsForSku(sku);

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
                Divisions = currentUser.GetUserDivisions(AppName)
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
                    if (!currentUser.HasDivDept(AppName, po.Division, dept))                    
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
                    model.NewPO.DeliveryDate = model.ExistingPOs[0].ExpectedDeliveryDate;                

                return View("Verify", model);
            }
            else
            {
                ViewData["message"] = "PO not found";
                model.Divisions = currentUser.GetUserDivisions(AppName);
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
                Divisions = currentUser.GetUserDivisions(AppName)
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
                string dept = po.Sku.Substring(3, 2);
                if (!currentUser.HasDivDept(AppName, po.Division, dept))
                {
                    model.Message += string.Format("<br>You do not have permission for dept {0}", dept);
                }
                if (!model.NewPO.Departments.Contains(dept))
                {
                    if (model.NewPO.Departments.Length > 0)
                    {
                        model.NewPO.Departments += ",";
                    }
                    model.NewPO.Departments += dept;
                }
                model.TotalRetail += po.Retail;
                model.TotalUnits += po.Units;
            }

            if (model.ExistingPOs.Count > 0)
            {
                model.NewPO.DeliveryDate = model.ExistingPOs[0].ExpectedDeliveryDate;
            }
            model.POs = new List<ExpeditePO>();
            foreach (ExistingPO po in model.ExistingPOs)
            {
                string message = "";
                if (po.DirectToStore)
                {
                    message = "Direct to store";
                }
                model.POs.Add(new ExpeditePO
                { 
                    PO = po.PO, 
                    Sku = po.Sku, 
                    OverrideDate = model.NewPO.OverrideDate, 
                    DeliveryDate = po.ExpectedDeliveryDate, 
                    CreateDate = DateTime.Now, 
                    CreatedBy = currentUser.NetworkID, 
                    TotalRetail = po.Retail, 
                    Division = po.Division, 
                    TotalUnits = po.Units, 
                    ErrorMessage = message 
                });
            }
            model.POs = model.POs.OrderBy(o => o.DeliveryDate).ToList();

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
                if (det.Sku == sku)
                {
                    if (!po.Departments.Contains(det.Sku.Substring(3, 2)))
                    {
                        if (po.Departments.Length > 0)
                        {
                            po.Departments += ",";
                        }
                        po.Departments += det.Sku.Substring(3, 2);
                    }
                    po.DeliveryDate = det.ExpectedDeliveryDate;
                    po.Sku = sku;
                    po.PO = det.PO;
                    po.Division = det.Division;
                    po.TotalRetail += det.Retail;
                    po.TotalUnits += det.Units;
                }
            }
            
            int alreadyExists = db.ExpeditePOs.Where(ep => ep.PO == po.PO && ep.Division == po.Division).Count();

            if (alreadyExists > 0)            
                db.Entry(po).State = System.Data.EntityState.Modified;            
            else            
                db.ExpeditePOs.Add(po);
            
            po.CreateDate = DateTime.Now;
            po.CreatedBy = currentUser.NetworkID;
            po.OverrideDate = overrideDate;

            db.SaveChanges();

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
                po.DeliveryDate = det.ExpectedDeliveryDate;
                po.Sku = det.Sku;
                po.PO = det.PO;
                po.Division = det.Division;
                po.TotalRetail += det.Retail;
                po.TotalUnits += det.Units;

            }
            ExpeditePO existingRec = db.ExpeditePOs.Where(ep => ep.PO == po.PO && ep.Division == po.Division).FirstOrDefault();
            if (existingRec != null)
            {
                db.ExpeditePOs.Remove(existingRec);
                db.SaveChanges();
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
                    DeliveryDate = det.ExpectedDeliveryDate, 
                    Sku = det.Sku, 
                    PO = det.PO, 
                    Departments = sku.Split('-')[1], 
                    Division = sku.Split('-')[0], 
                    TotalRetail = det.Retail, 
                    TotalUnits = det.Units,
                    OverrideDate = overrideDate,
                    CreateDate = DateTime.Now,
                    CreatedBy = currentUser.NetworkID
                };

                int alreadyExists = db.ExpeditePOs.Where(ep => ep.PO == po.PO && ep.Division == po.Division).Count();
                if (alreadyExists > 0)                
                    db.Entry(po).State = System.Data.EntityState.Modified;                
                else                
                    db.ExpeditePOs.Add(po);
                
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { div = sku.Split('-')[0], Sku = sku });
        }

        #region Ask Users about updating ranges after updating PO date.

        public ActionResult CheckForRanges(string div, string sku, string po)
        {
            List<RangePlan> plans =new List<RangePlan>();
            if ((po != "")&&(po != null))
            {
                ExistingPODAO dao = new ExistingPODAO(appConfig.EuropeDivisions);

                foreach (ExistingPO p in dao.GetExistingPO(div, po))
                {
                    plans.AddRange((from a in db.RangePlans where a.Sku == p.Sku select a).ToList());
                }
            }
            else
            {
                plans =(from a in db.RangePlans where a.Sku == sku select a).ToList();
            }

            if (plans.Count() == 0)
            {
                return RedirectToAction("Index", new { div = sku.Split('-')[0], Sku = sku });
            }

            return View(plans);

        }

        [GridAction]
        public ActionResult _DeliveryGroupsAjax(Int64 planid)
        {
            List<DeliveryGroup> groups = (from a in db.DeliveryGroups where (a.PlanID == planid) select a).ToList();

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
                    db.Entry(d).State = System.Data.EntityState.Modified;
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

                return RedirectToAction("Index", new { div = div, message="You must delete multi-sku by PO." });
            }
            else
            {
                div = sku.Split('-')[0];
            }
            List<ExpeditePO> list = db.ExpeditePOs.Where(ep => ep.OverrideDate == overrideDate && ep.Sku == sku).ToList();

            foreach (ExpeditePO det in list)
            {
                db.ExpeditePOs.Remove(det);
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { div = div });
        }

        public ActionResult EditSku(string sku, DateTime overrideDate)
        {
            string div;
            if (sku.Contains("multi"))
            {
                div = sku.Split('-')[2];

                return RedirectToAction("Index", new { div = div, message = "You must edit multi-sku by PO." });
            }
            else
            {
                div = sku.Split('-')[0];
            }

            ExpeditePO model = new ExpeditePO()
            {
                Division = div,
                Sku = sku,
                OverrideDate = overrideDate,
                //so we can update only the records we want we need to store the original overrideDate, kind of a hack
                DeliveryDate = overrideDate
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSku(ExpeditePO model)
        {
            //note that we're matching DeliveryDate because we used that field to store the old override date
            List<ExpeditePO> list = db.ExpeditePOs.Where(ep => ep.Division == model.Division && 
                                                               ep.Sku == model.Sku && 
                                                               ep.OverrideDate == model.DeliveryDate).ToList();

            foreach (ExpeditePO po in list)
            {
                po.OverrideDate = model.OverrideDate;
                po.CreateDate = DateTime.Now;
                po.CreatedBy = currentUser.NetworkID;
                db.Entry(po).State = System.Data.EntityState.Modified;
                db.SaveChanges();
            }
            
            return RedirectToAction("Index", new { div = model.Division });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAfterVerify(ExpeditePOModel model)
        {
            model.NewPO.CreateDate = DateTime.Now;
            model.NewPO.CreatedBy = currentUser.NetworkID;

            if (db.ExpeditePOs.Where(ep => ep.PO == model.NewPO.PO && ep.Division == model.NewPO.Division).Count() > 0)            
                db.Entry(model.NewPO).State = System.Data.EntityState.Modified;            
            else            
                db.ExpeditePOs.Add(model.NewPO);
            
            if (string.IsNullOrEmpty(model.NewPO.Sku))            
                model.NewPO.Sku = "unknown";            

            model.NewPO.PO = model.NewPO.PO.Trim();
            db.SaveChanges();
            
            return RedirectToAction("Index", new { div = model.NewPO.Division });
        }

        public ActionResult Delete(string div, string PO)
        {
            ExpeditePO model = db.ExpeditePOs.Where(ep => ep.Division == div && ep.PO == PO).First();

            db.ExpeditePOs.Remove(model);
            db.SaveChanges();

            return RedirectToAction("Index", new { div = div });
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

            model.CreateDate = DateTime.Now;
            model.CreatedBy = currentUser.NetworkID;
            model.PO = model.PO.Trim();

            db.Entry(model).State = System.Data.EntityState.Modified;
            db.SaveChanges();
            
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
