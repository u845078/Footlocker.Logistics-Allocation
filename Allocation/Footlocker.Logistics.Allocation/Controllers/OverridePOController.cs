using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Models.Services;
using Telerik.Web.Mvc;
using System.Web.Helpers;
using Aspose.Excel;
using System.IO;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Div Logistics,Buyer Planner,Planner,Director of Planning,VP of Planning,Director of Allocation,VP of Allocation,Admin,Support")]
    public class OverridePOController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        public ActionResult Index(string div, string message)
        {
            if (message != null)
            {
                ViewData["message"] = message;
            }
            ExpeditePOModel model = new ExpeditePOModel();
            model.Divisions = Divisions();
            if ((div == null) || (div == ""))
            {
                if (model.Divisions.Count() > 0)
                {
                    div = model.Divisions[0].DivCode;
                }
            }
            model.CurrentDivision = div;
            model.POs = (from a in db.ExpeditePOs where a.Division == div select a).ToList();
//            model.Headers = (from a in db.ExpeditePOs where a.Division == div select new ExpeditePOHeader { Sku = a.Sku, Department = a.Departments, OverrideDate = a.OverrideDate }).Distinct().ToList();
            return View(model);
        }

        public ActionResult IndexByPO(string div, string message)
        {
            if (message != null)
            {
                ViewData["message"] = message;
            }
            ExpeditePOModel model = new ExpeditePOModel();
            model.Divisions = Divisions();
            if ((div == null) || (div == ""))
            {
                if (model.Divisions.Count() > 0)
                {
                    div = model.Divisions[0].DivCode;
                }
            }
            model.CurrentDivision = div;
            model.POs = (from a in db.ExpeditePOs where a.Division == div select a).ToList();
            //            model.Headers = (from a in db.ExpeditePOs where a.Division == div select new ExpeditePOHeader { Sku = a.Sku, Department = a.Departments, OverrideDate = a.OverrideDate }).Distinct().ToList();
            return View(model);
        }

        [GridAction]
        public ActionResult _Index(string div)
        {
            ExpeditePOModel model = new ExpeditePOModel();
            model.Divisions = Divisions();
            if ((div == null) || (div == ""))
            {
                if (model.Divisions.Count() > 0)
                {
                    div = model.Divisions[0].DivCode;
                }
            }
            model.CurrentDivision = div;
            model.POs = (from a in db.ExpeditePOs where a.Division == div select a).ToList();
            model.Headers = (from a in db.ExpeditePOs where a.Division == div select new ExpeditePOHeader { Sku = a.Sku, Department = a.Departments, OverrideDate = a.OverrideDate }).Distinct().OrderBy(o=>o.Sku).ToList();
            return View(new GridModel(model.Headers));
        }

        [GridAction]
        public ActionResult _IndexByPO(string div)
        {
            ExpeditePOModel model = new ExpeditePOModel();
            model.Divisions = Divisions();
            if ((div == null) || (div == ""))
            {
                if (model.Divisions.Count() > 0)
                {
                    div = model.Divisions[0].DivCode;
                }
            }
            model.CurrentDivision = div;
            model.POs = (from a in db.ExpeditePOs where a.Division == div select a).ToList();
            return View(new GridModel(model.POs));
        }


        [GridAction]
        public ActionResult _POListAjax(string sku)
        {
            var POs = (from a in db.ExpeditePOs where (a.Sku == sku) select a).ToList();

            return View(new GridModel(POs));
        }

        public ActionResult Create(string div)
        {
            ExpeditePOModel model = new ExpeditePOModel();
            model.NewPO = new ExpeditePO();
            model.Divisions = this.Divisions();
            model.NewPO.Division = div;
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(ExpeditePOModel model)
        {
            ExistingPODAO dao = new ExistingPODAO();
            model.ExistingPOs = dao.GetExistingPO(model.NewPO.Division, model.NewPO.PO);
            if (model.ExistingPOs.Count() > 0)
            {
                model.NewPO.Departments = "";
                model.NewPO.Sku = "";
                int skucount = 0;
                foreach (ExistingPO po in model.ExistingPOs)
                {
                    if (!(Footlocker.Common.WebSecurityService.UserHasDepartment(UserName, "Allocation", po.Division, po.Sku.Substring(3, 2))))
                    {
                        model.Message = model.Message + "<br>You do not have permission for dept " + po.Sku.Substring(3, 2);
                    }
                    if (!(model.NewPO.Departments.Contains(po.Sku.Substring(3, 2))))
                    {
                        if (model.NewPO.Departments.Length > 0)
                        {
                            model.NewPO.Departments += ",";
                        }
                        model.NewPO.Departments += po.Sku.Substring(3, 2);
                    }
                    model.NewPO.Sku = po.Sku;
                    skucount++;
                    if (skucount > 1)
                    {
                        model.NewPO.Sku = "multi-sku-" + model.NewPO.Sku;
                    }
                    model.TotalRetail += po.Retail;
                    model.TotalUnits += po.Units;
                }
                if (model.NewPO.Sku.Length > 50)
                {
                    model.NewPO.Sku = model.NewPO.Sku.Substring(0, 50);
                }
                if (model.ExistingPOs.Count > 0)
                {
                    model.NewPO.DeliveryDate = model.ExistingPOs[0].ExpectedDeliveryDate;
                }

                return View("Verify", model);
            }
            else
            {
                ViewData["message"] = "PO not found";
                model.Divisions = this.Divisions();
                return View(model);
            
            }
        }

        public ActionResult CreateAllPOsForSku(string div)
        {
            ExpeditePOModel model = new ExpeditePOModel();
            model.NewPO = new ExpeditePO();
            model.Divisions = this.Divisions();
            model.NewPO.Division = div;
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateAllPOsForSku(ExpeditePOModel model)
        {
            ExistingPODAO dao = new ExistingPODAO();
            model.ExistingPOs = dao.GetExistingPOsForSku(model.NewPO.Division, model.NewPO.Sku, true);
            model.NewPO.Departments = "";
            foreach (ExistingPO po in model.ExistingPOs)
            {
                if (!(Footlocker.Common.WebSecurityService.UserHasDepartment(UserName, "Allocation", po.Division, po.Sku.Substring(3, 2))))
                {
                    model.Message = model.Message + "<br>You do not have permission for dept " + po.Sku.Substring(3, 2);
                }
                if (!(model.NewPO.Departments.Contains(po.Sku.Substring(3, 2))))
                {
                    if (model.NewPO.Departments.Length > 0)
                    {
                        model.NewPO.Departments += ",";
                    }
                    model.NewPO.Departments += po.Sku.Substring(3, 2);
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
                model.POs.Add(new ExpeditePO { PO = po.PO, Sku = po.Sku, OverrideDate = model.NewPO.OverrideDate, DeliveryDate = po.ExpectedDeliveryDate, CreateDate = DateTime.Now, CreatedBy = User.Identity.Name, TotalRetail = po.Retail, Division = po.Division, TotalUnits = po.Units, ErrorMessage = message });
            }
            model.POs = model.POs.OrderBy(o => o.DeliveryDate).ToList();

            return View("VerifyList", model);
        }

        [HttpPost]
        public JsonResult AcceptPO(string div, string PO, string sku, DateTime overrideDate)
        {
            ExistingPODAO dao = new ExistingPODAO();
            List<ExistingPO> list = dao.GetExistingPO(div, PO);

            ExpeditePO po;
            po = new ExpeditePO();
            po.Departments = "";
            foreach (ExistingPO det in list)
            {
                if (det.Sku == sku)
                {
                    if (!(po.Departments.Contains(det.Sku.Substring(3, 2))))
                    {
                        if (po.Departments.Length > 0)
                        {
                            po.Departments += ",";
                        }
                        po.Departments += det.Sku.Substring(3, 2);
                    }
                    po.DeliveryDate = det.ExpectedDeliveryDate;
                    po.Sku = sku;//det.Sku;
                    po.PO = det.PO;
                    po.Division = det.Division;
                    po.TotalRetail += det.Retail;
                    po.TotalUnits += det.Units;
                }
            }
            var alreadyExists = (from a in db.ExpeditePOs where ((a.PO == po.PO) && (a.Division == po.Division)) select a);
            if (alreadyExists.Count() > 0)
            {
                db.Entry(po).State = System.Data.EntityState.Modified;
            }
            else
            {
                db.ExpeditePOs.Add(po);
            }
            po.CreateDate = DateTime.Now;
            po.CreatedBy = User.Identity.Name;
            po.OverrideDate = overrideDate;

            db.SaveChanges();

            return Json("Success");

        }

        [HttpPost]
        public JsonResult RemovePO(string div, string PO)
        {
            ExistingPODAO dao = new ExistingPODAO();
            List<ExistingPO> list = dao.GetExistingPO(div, PO);

            ExpeditePO po;
            po = new ExpeditePO();
            foreach (ExistingPO det in list)
            {
                if (!(po.Departments.Contains(po.Sku.Substring(3, 2))))
                {
                    if (po.Departments.Length > 0)
                    {
                        po.Departments += ",";
                    }
                    po.Departments += po.Sku.Substring(3, 2);
                }
                po.DeliveryDate = det.ExpectedDeliveryDate;
                po.Sku = det.Sku;
                po.PO = det.PO;
                po.Division = det.Division;
                po.TotalRetail += det.Retail;
                po.TotalUnits += det.Units;

            }
            var alreadyExists = (from a in db.ExpeditePOs where ((a.PO == po.PO) && (a.Division == po.Division)) select a);
            if (alreadyExists.Count() > 0)
            {
                db.ExpeditePOs.Remove(alreadyExists.First());
                db.SaveChanges();
            }

            return Json("Success");

        }


        public ActionResult AcceptAll(string sku, DateTime overrideDate)
        {
            ExistingPODAO dao = new ExistingPODAO();
            List<ExistingPO> list = dao.GetExistingPOsForSku(sku.Split('-')[0], sku, true);

            ExpeditePO po;
            foreach (ExistingPO det in list)
            {
                po = new ExpeditePO { DeliveryDate = det.ExpectedDeliveryDate, Sku = det.Sku, PO = det.PO, Departments = sku.Split('-')[1], Division = sku.Split('-')[0], TotalRetail = det.Retail, TotalUnits = det.Units };
                po.OverrideDate = overrideDate;
                po.CreateDate = DateTime.Now;
                po.CreatedBy = User.Identity.Name;

                var alreadyExists = (from a in db.ExpeditePOs where ((a.PO == po.PO) && (a.Division == po.Division)) select a);
                if (alreadyExists.Count() > 0)
                {
                    db.Entry(po).State = System.Data.EntityState.Modified;
                }
                else
                {
                    db.ExpeditePOs.Add(po);
                }
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
                ExistingPODAO dao = new ExistingPODAO();

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
                    //            var list = (from a in db.SizeAllocations join b in db.RuleSelectedStores on new { a.Division, a.Store } equals new { b.Division, b.Store } join c in db.MaxLeadTimes on new { a.Division, a.Store } equals new { c.Division, c.Store } where (a.PlanID == model.DeliveryGroup.PlanID) select new { sa = a, lt = c }).ToList();
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
            List<ExpeditePO> list = (from a in db.ExpeditePOs where ((a.OverrideDate == overrideDate) && (a.Sku == sku)) select a).ToList();

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
            ExpeditePO model = new ExpeditePO();
            model.Division = div;
            model.Sku = sku;
            model.OverrideDate = overrideDate;
            //so we can update only the records we want we need to store the original overrideDate, kind of a hack
            model.DeliveryDate = overrideDate;
            return View(model);

            return RedirectToAction("Index", new { div = div });

        }

        [HttpPost]
        public ActionResult EditSku(ExpeditePO model)
        {
            //note that we're matching DeliveryDate because we used that field to store the old override date
            List<ExpeditePO> list = (from a in db.ExpeditePOs where ((a.Division == model.Division) && (a.Sku == model.Sku) && (a.OverrideDate == model.DeliveryDate)) select a).ToList();

            foreach (ExpeditePO po in list)
            {
                po.OverrideDate = model.OverrideDate;
                po.CreateDate = DateTime.Now;
                po.CreatedBy = User.Identity.Name;
                db.Entry(po).State = System.Data.EntityState.Modified;
                db.SaveChanges();
            }
            
            //return RedirectToAction("CheckForRanges", new { div = model.Division, sku = model.Sku, po = model.PO });

            return RedirectToAction("Index", new { div = model.Division });

        }


        [HttpPost]
        public ActionResult CreateAfterVerify(ExpeditePOModel model)
        {
            model.NewPO.CreateDate = DateTime.Now;
            model.NewPO.CreatedBy = User.Identity.Name;

            var alreadyExists = (from a in db.ExpeditePOs where ((a.PO == model.NewPO.PO) &&(a.Division == model.NewPO.Division)) select a);
            if (alreadyExists.Count() > 0)
            {
                db.Entry(model.NewPO).State = System.Data.EntityState.Modified;
            }
            else
            {
                db.ExpeditePOs.Add(model.NewPO);
            }
            if ((model.NewPO.Sku == null) || (model.NewPO.Sku == ""))
            {
                model.NewPO.Sku = "unknown";
            }
            model.NewPO.PO = model.NewPO.PO.Trim();
            db.SaveChanges();
            //return RedirectToAction("CheckForRanges", new { div = model.NewPO.Division, sku = model.NewPO.Sku, po = model.NewPO.PO });
            return RedirectToAction("Index", new { div = model.NewPO.Division });
        }

        public ActionResult Delete(string div, string PO)
        {
            ExpeditePO model = (from a in db.ExpeditePOs where ((a.Division == div) && (a.PO==PO)) select a).First();

            db.ExpeditePOs.Remove(model);
            db.SaveChanges();

            return RedirectToAction("Index", new { div = div });
        }

        public ActionResult Edit(string div, string PO)
        {
            ExpeditePO model = (from a in db.ExpeditePOs where ((a.Division == div) && (a.PO == PO)) select a).First();

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(ExpeditePO model)
        {

            model.CreateDate = DateTime.Now;
            model.CreatedBy = User.Identity.Name;
            model.PO = model.PO.Trim();

            db.Entry(model).State = System.Data.EntityState.Modified;
            db.SaveChanges();
            //return RedirectToAction("CheckForRanges", new { div = model.Division, sku = model.Sku, po = model.PO });
            return RedirectToAction("Index", new { div = model.Division });
        }

        public ActionResult Upload()
        {
            return View();
        }

        public ActionResult ExcelTemplate()
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            Worksheet mySheet = excelDocument.Worksheets[0];

            int row = 1;
            mySheet.Cells[0, 0].PutValue("Division-PO");
            mySheet.Cells[0, 1].PutValue("OverrideDate(mm/dd/yyyy)");

            excelDocument.Save("POOverrideUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public ActionResult Save(IEnumerable<HttpPostedFileBase> attachments)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            List<ExpeditePO> errors = new List<ExpeditePO>();
            int errorCount = 0;
            int addedCount = 0;

            foreach (HttpPostedFileBase file in attachments)
            {
                //Instantiate a Workbook object that represents an Excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];
                int row = 1;
                ExistingPODAO dao = new ExistingPODAO();
                ExpeditePO overridePO=null;
                if ((Convert.ToString(mySheet.Cells[0, 0].Value).Contains("Division-PO"))&&(Convert.ToString(mySheet.Cells[0, 1].Value).Contains("OverrideDate")))
                {
                    while (mySheet.Cells[row, 0].Value != null)
                    {
                        overridePO = new ExpeditePO();
                        string temp = mySheet.Cells[row, 0].Value.ToString();
                        string[] tokens = temp.Split('-');
                        overridePO.Division = tokens[0];
                        overridePO.PO = tokens[1];
                        overridePO.OverrideDate = Convert.ToDateTime(mySheet.Cells[row, 1].Value);
                        List<ExistingPO> existingPOs = dao.GetExistingPO(overridePO.Division, overridePO.PO);
                        if (existingPOs.Count() > 0)
                        {
                            overridePO.Departments = "";
                            overridePO.Sku = "";
                            int skucount = 0;
                            foreach (ExistingPO po in existingPOs)
                            {
                                if (!(Footlocker.Common.WebSecurityService.UserHasDepartment(UserName, "Allocation", po.Division, po.Sku.Substring(3, 2))))
                                {
                                    errorCount++;
                                    overridePO.ErrorMessage = "Permission denied.";
                                    errors.Add(overridePO);
                                    break;
                                }
                                if (!(overridePO.Departments.Contains(po.Sku.Substring(3, 2))))
                                {
                                    if (overridePO.Departments.Length > 0)
                                    {
                                        overridePO.Departments += ",";
                                    }
                                    overridePO.Departments += po.Sku.Substring(3, 2);
                                }
                                overridePO.Sku = po.Sku;
                                skucount++;
                                if (skucount > 1)
                                {
                                    overridePO.Sku = "multi-sku-" + overridePO.Sku;
                                }
                            }
                            if (overridePO.Sku.Length > 50)
                            {
                                overridePO.Sku = overridePO.Sku.Substring(0, 50);
                            }
                            if (existingPOs.Count > 0)
                            {
                                overridePO.DeliveryDate = existingPOs[0].ExpectedDeliveryDate;
                            }

                            overridePO.CreateDate = DateTime.Now;
                            overridePO.CreatedBy = User.Identity.Name;

                            var alreadyExists = (from a in db.ExpeditePOs where ((a.PO == overridePO.PO) && (a.Division == overridePO.Division)) select a);
                            if (alreadyExists.Count() > 0)
                            {
                                db.Entry(overridePO).State = System.Data.EntityState.Modified;
                            }
                            else
                            {
                                db.ExpeditePOs.Add(overridePO);
                            }
                            if ((overridePO.Sku == null) || (overridePO.Sku == ""))
                            {
                                overridePO.Sku = "unknown";
                            }
                        }
                        else
                        {
                            errorCount++;
                            overridePO.ErrorMessage = "PO not found.";
                            errors.Add(overridePO);

                        }
                        row++;
                    }
                    db.SaveChanges();

                }
                else
                {
                    return Content("Incorrect header, columns must be Division-PO, OverrideDate.");
                }
            }
            if (errors.Count > 0)
            {
                Session["errorList"] = errors;
                return Content(errorCount + " Errors on spreadsheet (" + addedCount + " successfully uploaded)");

            }
            else
            {
                return Content("");
            }
        }

        public ActionResult DownloadErrors()
        {
            List<ExpeditePO> errorList = new List<ExpeditePO>();
            if (Session["errorList"] != null)
            {
                errorList = (List<ExpeditePO>)Session["errorList"];
            }


            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            Worksheet mySheet = excelDocument.Worksheets[0];
            int row = 1;
            mySheet.Cells[0, 0].PutValue("Division-PO");
            mySheet.Cells[0, 1].PutValue("OverrideDate");
            mySheet.Cells[0, 2].PutValue("ErrorMessage");
            foreach (ExpeditePO p in errorList)
            {
                mySheet.Cells[row, 0].PutValue(p.Division + "-" + p.PO);
                mySheet.Cells[row, 1].PutValue(p.OverrideDate);
                mySheet.Cells[row, 2].PutValue(p.ErrorMessage);

                row++;
            }

            excelDocument.Save("POOverrideErrors.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();

        }

    }
}
