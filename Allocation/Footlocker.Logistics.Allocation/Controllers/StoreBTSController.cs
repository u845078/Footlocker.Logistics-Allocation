using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Services;
using Aspose.Cells;
using Footlocker.Logistics.Allocation.Spreadsheets;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Director of Allocation,Admin,Support")]
    public class StoreBTSController : AppController
    {
        //Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
        readonly AllocationLibraryContext db = new AllocationLibraryContext();

        [HttpPost]
        public ActionResult _AutoCompleteFilteringAjax(string text, string div)
        {
            IQueryable<StoreCluster> stores = db.StoreClusters.AsQueryable();
            stores = stores.Where((p) => p.Division.Equals(div));
            return new JsonResult { Data = stores.Select(p => p.Name) };
        }

        public ActionResult Index(string message, string div, int? year)
        {
            ViewData["message"] = message;

            StoreBTSModel model = new StoreBTSModel()
            {
                Divisions = currentUser.GetUserDivisions()
            };
            
            if (model.Divisions.Count() > 0)
            {                
                if (string.IsNullOrEmpty(div))                
                    div = model.Divisions[0].DivCode;
                
                model.CurrentDivision = div;
                model.List = db.StoreBTS.Where(s => s.Division == div).ToList();

                foreach (var storeBTSRec in model.List)
                {
                    storeBTSRec.ClusterID = db.StoreClusters.Where(sc => sc.Division == storeBTSRec.Division && sc.Name == storeBTSRec.Name)
                                                            .Select(sc => sc.ID)
                                                            .FirstOrDefault();
                }

                if (model.List.Count > 0)
                {
                    List<string> uniqueNames = (from l in model.List
                                                select l.CreatedBy).Distinct().ToList();

                    Dictionary<string, string> fullNamePairs = LoadUserNames(uniqueNames);

                    foreach (var item in fullNamePairs)
                    {
                        model.List.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
                    }
                }

                var query = db.StoreBTSControls.Where(sbc => sbc.Division == div);
                if (query.Count() > 0)
                {
                    model.Control = query.First();
                }
                else
                {
                    model.Control = new StoreBTSControl()
                    {
                        Division = div,
                        TY = DateTime.Now.Year,
                        LY = DateTime.Now.Year - 1
                    };
                }
            }
            model.Years = new List<int>
            {
                DateTime.Now.Year + 1,
                DateTime.Now.Year,
                DateTime.Now.Year - 1,
                DateTime.Now.Year - 2,
                DateTime.Now.Year - 3,
                DateTime.Now.Year - 4,
                DateTime.Now.Year - 5
            };

            if (year != null)            
                model.CurrentYear = (int)year;            
            else            
                model.CurrentYear = DateTime.Now.Year;            

            return View(model);
        }

        [GridAction]
        public ActionResult ExportGrid(GridCommand settings, string div, int? year)
        {
            BTSExport btsExport = new BTSExport(appConfig);
            btsExport.WriteData(settings, div, year);

            btsExport.excelDocument.Save(System.Web.HttpContext.Current.Response, "BackToSchool.xlsx", ContentDisposition.Attachment, btsExport.SaveOptions);
            return RedirectToAction("Index");
        }

        [GridAction]
        public ActionResult ExportDetailsGrid(GridCommand settings, int ID)
        {
            BTSDetailsExport btsDetailsExport = new BTSDetailsExport(appConfig);
            btsDetailsExport.WriteData(settings, ID);

            btsDetailsExport.excelDocument.Save(System.Web.HttpContext.Current.Response, "BackToSchoolDetails.xlsx", ContentDisposition.Attachment, btsDetailsExport.SaveOptions);
            return RedirectToAction("Details", new { ID });
        }

        public ActionResult Create(string div, string name, int year)
        {
            string message="";
            if (db.StoreBTS.Where(sb => sb.Division == div && sb.Name == name && sb.Year == year).Count() > 0)            
                message = string.Format("The group {0} was already created for {1}.", name, year);            
            else
            {
                if (db.StoreClusters.Where(sc => sc.Division == div && sc.Name == name).Count() == 1)
                {
                    try
                    {
                        StoreBTS group = new StoreBTS()
                        {
                            Name = name,
                            Year = year,
                            Division = div,
                            CreatedBy = currentUser.NetworkID,
                            CreateDate = DateTime.Now
                        };

                        db.StoreBTS.Add(group);
                        db.SaveChanges();
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
                    {
                        try
                        {
                            message = ex2.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage;
                        }
                        catch
                        {
                            message = ex2.Message;
                        }
                    }
                    catch (Exception ex)
                    {
                        message = ex.Message;
                    }
                }
                else
                {
                    message = "Not a valid BTS group name for your division.";
                }
            }
            return RedirectToAction("Index", new { message, div });
        }

        public ActionResult Edit(int ID)
        {
            StoreBTS group = db.StoreBTS.Where(sb => sb.ID == ID).First();

            return View(group);
        }


        [HttpPost]
        public ActionResult SetYear(StoreBTSModel model)
        {
            StoreBTSControl control = db.StoreBTSControls.Where(sbc => sbc.Division == model.Control.Division).FirstOrDefault();
            if (control == null)
            {
                control = new StoreBTSControl()
                {
                    Division = model.Control.Division,
                    TY = model.Control.TY,
                    LY = model.Control.LY
                };

                db.StoreBTSControls.Add(control);
                db.SaveChanges();
            }
            else
            {
                control.TY = model.Control.TY;
                control.LY = model.Control.LY;

                db.Entry(control).State = System.Data.EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index", new {div = model.Control.Division});
        }

        public ActionResult Search(string store, int year, string div)
        {
            var list = db.StoreBTSDetails.Where(sbd => sbd.Store.Contains(store) && sbd.Year == year && sbd.Division == div);

            if (list.Count() == 1)
            {
                StoreBTSDetail det = list.First();
                StoreBTS group = db.StoreBTS.Where(sb => sb.ID == det.GroupID).First();

                return View("ShowSearchResult", group);
            }
            else if (list.Count() == 0)            
                return RedirectToAction("Index", new {message = string.Format("Store {0}-{1} is not in any group for year {2}", div, store, year), div});            
            else            
                return View(list);            
        }

        public ActionResult ShowSearchResult(string store, string div, int year)
        {
            var list = db.StoreBTSDetails.Where(sbd => sbd.Store == store && sbd.Division == div && sbd.Year == year);
            StoreBTSDetail det = list.First();
            StoreBTS group = db.StoreBTS.Where(sb => sb.ID == det.GroupID).First();

            return View(group);
        }

        public ActionResult Delete(int ID)
        {
            var details = db.StoreBTSDetails.Where(sbd => sbd.GroupID == ID);
            foreach (StoreBTSDetail det in details)            
                db.StoreBTSDetails.Remove(det);
            
            db.SaveChanges();
            StoreBTS group = db.StoreBTS.Where(sb => sb.ID == ID).First();

            db.StoreBTS.Remove(group);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Details(int ID, string message)
        {
            StoreBTSDetailModel model = new StoreBTSDetailModel();
            ViewData["message"] = message;

            model.details = (from a in db.StoreBTSDetails 
                             join b in db.StoreLookups 
                             on new {a.Division, a.Store} equals new {b.Division, b.Store} 
                             where a.GroupID == ID 
                             select b).ToList();

            model.header = db.StoreBTS.Where(sb => sb.ID == ID).FirstOrDefault();
            model.divisions = Footlocker.Common.DivisionService.ListDivisions();
            ViewData["GroupID"] = ID;
            string div = (from a in db.StoreBTS 
                          where a.ID == ID 
                          select a.Division).First();

            model.CopyFrom = db.StoreBTS.Where(sb => sb.Year == DateTime.Now.Year - 1 && sb.Division == div).ToList();
            model.division = (from a in db.StoreBTS 
                              where a.ID == ID 
                              select a.Division).First();
            int year = (from a in db.StoreBTS 
                        where a.ID == ID 
                        select a.Year).First();
            
            model.UnassignedStores = (from a in db.StoreLookups
                                      join b in (from c in db.StoreBTSDetails where c.Year == year select c) on new { a.Division, a.Store } equals new { b.Division, b.Store } into subset
                                      from sc in subset.DefaultIfEmpty()
                                      where sc == null && a.Division == model.division
                                      select a).ToList();

            return View(model);
        }

        public ActionResult DownloadUnassignedStores(string div, int year)
        {
            BTSUnassignedExport exportBTS = new BTSUnassignedExport(appConfig, new MainframeStoreDAO(appConfig.EuropeDivisions));
            exportBTS.WriteData(div, year);
            
            exportBTS.excelDocument.Save(System.Web.HttpContext.Current.Response, "BTS_Unassigned.xlsx", ContentDisposition.Attachment, exportBTS.SaveOptions);
            return View();
        }

        [GridAction]
        public ActionResult _RefreshGrid(int groupID)
        {
            List<StoreLookup> list = (from a in db.StoreBTSDetails 
                                      join b in db.StoreLookups 
                                      on new { a.Division, a.Store } equals new { b.Division, b.Store } 
                                      where a.GroupID == groupID 
                                      select b).ToList();
            return View(new GridModel(list)); 
        }

        public ActionResult DeleteDetail(int ID, string div, string store)
        {
            StoreBTSDetail det = db.StoreBTSDetails.Where(sbd => sbd.GroupID == ID && sbd.Division == div && sbd.Store==store).First();
            db.StoreBTSDetails.Remove(det);
            db.SaveChanges();

            //StoreBTS group = db.StoreBTS.Where(sb => sb.ID == ID).First();
            //group.Count--;
            //db.SaveChanges();

            return RedirectToAction("Details", new { ID});
        }

        public ActionResult CopyStoreBTS(int ID, int CopyFrom)
        {
            int year = (from a in db.StoreBTS 
                        where a.ID == ID 
                        select a.Year).First();
            var copyFromList = db.StoreBTSDetails.Where(sbd => sbd.GroupID == CopyFrom);

            StoreBTSDetail newDet;
            List<StoreBTSDetail> list = new List<StoreBTSDetail>();
            foreach (StoreBTSDetail det in copyFromList)
            {
                newDet = new StoreBTSDetail()
                {
                    GroupID = ID,
                    Year = year,
                    Store = det.Store,
                    Division = det.Division,
                    CreatedBy = currentUser.NetworkID,
                    CreateDate = DateTime.Now
                };

                list.Add(newDet);
            }
            int errors = 0;
            int count = 0;
            foreach (StoreBTSDetail det in list)
            {
                if (db.StoreBTSDetails.Where(sbd => sbd.Store == det.Store && sbd.Division == det.Division && sbd.Year == det.Year).Count() == 0)
                {
                    db.StoreBTSDetails.Add(det);
                    db.SaveChanges();
                    count++;
                }
                else                
                    errors++;                
            }
            //StoreBTS group = db.StoreBTS.Where(sb => sb.ID == ID).First();
            //group.Count += count;
            //db.Entry(group).State = System.Data.EntityState.Modified;
            //db.SaveChanges();

            return RedirectToAction("Details", new { ID, message = string.Format("Copied group {0} stores, {1} errors.", count, errors) });            
        }

        public ActionResult AddDetail(int ID, string store)
        {
            string message = "";
            StoreBTS storebts = db.StoreBTS.Where(sb => sb.ID == ID).First();
            string division = storebts.Division;
            store = store.PadLeft(5, '0');
            int year = storebts.Year;

            if (currentUser.HasDivision(division))
            {
                var storequery = db.StoreLookups.Where(sl => sl.Division == division && sl.Store == store);

                if (storequery.Count() > 0)
                {
                    var existing = db.StoreBTSDetails.Where(sbd => sbd.Division == division && sbd.Store == store && sbd.Year == year);

                    if (existing.Count() > 0)
                    {
                        int oldGroup = existing.First().GroupID;
                        ViewData["OriginalGroup"] = (from a in db.StoreBTS where a.ID == oldGroup select a).First().Name;
                        ViewData["NewGroup"] = (from a in db.StoreBTS where a.ID == ID select a).First().Name;
                        //give them a confirmation screen about the move
                        ViewData["GroupID"] = ID;
                        ViewData["div"] = division;
                        ViewData["store"] = store;
                        ViewData["year"] = year;
                        return View();                        
                    }
                    else
                    {
                        try
                        {
                            StoreBTSDetail det = new StoreBTSDetail()
                            {
                                GroupID = ID,
                                Division = division,
                                Store = store,
                                Year = year,
                                CreateDate = DateTime.Now,
                                CreatedBy = currentUser.NetworkID
                            };

                            db.StoreBTSDetails.Add(det);
                            db.SaveChanges();

                            //StoreBTS group = db.StoreBTS.Where(sb => sb.ID == ID).First();
                            //group.Count++;
                            //db.SaveChanges();
                        }
                        catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
                        {
                            try
                            {
                                message = ex2.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage;
                            }
                            catch
                            {
                                message = ex2.Message;
                            }
                        }
                        catch (Exception ex)
                        {
                            message = ex.Message;
                        }
                    }
                }
                else                 
                    message = "Invalid division/store.";                
            }
            else            
                message = "You are not autorized for this division.";
            
            return RedirectToAction("Details", new { ID, message });
        }

        public ActionResult ShowDetail(int ID, string div, string store)
        {
            List<StoreBTSDetail> model = db.StoreBTSDetails.Where(sbd => sbd.Division == div && sbd.Store == store && sbd.GroupID == ID).ToList();
            return View(model);
        }

        public ActionResult ConfirmMove(int ID, string div, string store, int year)
        {
            StoreBTSDetail det = db.StoreBTSDetails.Where(sbd => sbd.Division == div && sbd.Store == store && sbd.Year == year).First();
            //StoreBTS group = db.StoreBTS.Where(sb => sb.ID == det.GroupID).First();
            //group.Count--;
            //db.SaveChanges();
            
            det.GroupID = ID;
            det.CreateDate = DateTime.Now;
            det.CreatedBy = currentUser.NetworkID;
            db.SaveChanges();

            //group = db.StoreBTS.Where(sb => sb.ID == ID).First();
            //group.Count++;
            //db.SaveChanges();

            return RedirectToAction("Details", new { ID });
        }

        #region Upload spreadsheet
        public ActionResult ExcelTemplate()
        {
            BTSSpreadsheet btsSpreadsheet = new BTSSpreadsheet(appConfig, new ConfigService());
            Workbook excelDocument;

            excelDocument = btsSpreadsheet.GetTemplate();

            excelDocument.Save(System.Web.HttpContext.Current.Response, "StoreBTSDetails.xlsx", ContentDisposition.Attachment, btsSpreadsheet.SaveOptions);
            return View();
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public ActionResult Save(IEnumerable<HttpPostedFileBase> attachments, int groupID)
        {
            BTSSpreadsheet btsSpreadsheet = new BTSSpreadsheet(appConfig, new ConfigService());

            int successCount = 0;
            string message;

            foreach (HttpPostedFileBase file in attachments)
            {
                btsSpreadsheet.Save(file, groupID);

                if (!string.IsNullOrEmpty(btsSpreadsheet.message))
                    return Content(btsSpreadsheet.message);
                else
                {
                    if (btsSpreadsheet.errorList.Count() > 0)
                    {
                        Session["errorList"] = btsSpreadsheet.errorList;

                        message = string.Format("{0} successfully uploaded, {1} Errors", btsSpreadsheet.validData.Count.ToString(),
                            btsSpreadsheet.errorList.Count.ToString());

                        return Content(message);
                    }
                }

                successCount += btsSpreadsheet.validData.Count();
            }

            return Json(new { message = string.Format("Upload complete. Added {0} store(s)", successCount) }, "application/json");
        }

        public ActionResult BTSErrors()
        {
            List<StoreBTSDetail> errors = (List<StoreBTSDetail>)Session["errorList"];
            Workbook excelDocument;
            BTSSpreadsheet btsSpreadsheet = new BTSSpreadsheet(appConfig, new ConfigService());

            if (errors != null)
            {
                excelDocument = btsSpreadsheet.GetErrors(errors);
                excelDocument.Save(System.Web.HttpContext.Current.Response, "BTSUploadErrors.xlsx", ContentDisposition.Attachment, btsSpreadsheet.SaveOptions);
            }
            return View();
        }
        #endregion
    }
}