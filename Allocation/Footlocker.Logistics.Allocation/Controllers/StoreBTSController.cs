using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using System.IO;
using Aspose.Excel;
using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Common;
using Aspose.Cells;
using System.Web.UI;
using Footlocker.Common.Entities;
using Footlocker.Logistics.Allocation.Spreadsheets;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Director of Allocation,Admin,Support")]
    public class StoreBTSController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

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
                Divisions = currentUser.GetUserDivisions(AppName)
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
            List<StoreBTSGridExtract> storeBTS = (from a in db.StoreBTS
                                                  where a.Division == div && a.Year == year
                                                  select new StoreBTSGridExtract
                                                  {
                                                      Year = a.Year,
                                                      Name = a.Name,
                                                      ID = a.ID,
                                                      Division = a.Division,
                                                      CreateDate = a.CreateDate,
                                                      CreatedBy = a.CreatedBy
                                                  }
                                                  ).ToList();

            foreach (var storeBTSRec in storeBTS)
            {
                storeBTSRec.ClusterID = (from a in db.StoreClusters
                                         where a.Division == storeBTSRec.Division &&
                                               a.Name == storeBTSRec.Name
                                         select a.ID).FirstOrDefault();
            }

            IQueryable<StoreBTSGridExtract> list = storeBTS.AsQueryable();

            if (settings.FilterDescriptors.Any())            
                list = list.ApplyFilters(settings.FilterDescriptors);            

            foreach (var bts in list.ToList())
            {
                bts.StoreLookups = (from a in db.StoreBTSDetails 
                                    join b in db.StoreLookups 
                                    on new { a.Division, a.Store } equals new { b.Division, b.Store } 
                                    where a.GroupID == bts.ID 
                                    select b).ToList();
            }

            Workbook excelDocument = CreateSchoolBTSExport(list.ToList());

            OoxmlSaveOptions save = new OoxmlSaveOptions(SaveFormat.Xlsx);
            excelDocument.Save(System.Web.HttpContext.Current.Response, "BackToSchool.xlsx", ContentDisposition.Attachment, save);
            return RedirectToAction("Index");
        }

        [GridAction]
        public ActionResult ExportDetailsGrid(GridCommand settings, int ID)
        {
            List<StoreBTSGridExtract> storeBTS = (from a in db.StoreBTS
                                                  where a.ID == ID
                                                  select new StoreBTSGridExtract
                                                  {
                                                      Year = a.Year,
                                                      Name = a.Name,
                                                      ID = a.ID,
                                                      Division = a.Division,
                                                      CreateDate = a.CreateDate,
                                                      CreatedBy = a.CreatedBy
                                                  }).ToList();

            foreach (var storeBTSRec in storeBTS)
            {
                storeBTSRec.ClusterID = (from a in db.StoreClusters
                                         where a.Division == storeBTSRec.Division &&
                                               a.Name == storeBTSRec.Name
                                         select a.ID).FirstOrDefault();

                IQueryable<StoreLookup> listToFilter = (from a in db.StoreBTSDetails
                                                        join b in db.StoreLookups on new { a.Division, a.Store } equals new { b.Division, b.Store }
                                                        where a.GroupID == ID
                                                        select b).ToList().AsQueryable();

                if (settings.FilterDescriptors.Any())                
                    listToFilter = listToFilter.ApplyFilters(settings.FilterDescriptors);                

                storeBTSRec.StoreLookups = listToFilter.ToList();
            }

            Workbook excelDocument = CreateSchoolBTSExport(storeBTS);

            OoxmlSaveOptions save = new OoxmlSaveOptions(SaveFormat.Xlsx);
            excelDocument.Save(System.Web.HttpContext.Current.Response, "BackToSchoolDetails.xlsx", ContentDisposition.Attachment, save);

            return RedirectToAction("Details", new { ID });
        }

        private Workbook CreateSchoolBTSExport(List<StoreBTSGridExtract> storeBTS)
        {
            Workbook excelDocument = RetrieveStoreBTSExcelFile(false);
            int row = 1;
            Aspose.Cells.Worksheet workSheet = excelDocument.Worksheets[0];

            foreach (var rr in storeBTS)
            {
                Aspose.Cells.Style align = excelDocument.CreateStyle();
                align.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Right;

                Aspose.Cells.Style date = excelDocument.CreateStyle();
                date.Number = 14;

                if (rr.StoreLookups.Count() == 0)
                {
                    workSheet.Cells[row, 0].PutValue(rr.ClusterID);
                    workSheet.Cells[row, 0].SetStyle(align);
                    workSheet.Cells[row, 1].PutValue(rr.Year);
                    workSheet.Cells[row, 1].SetStyle(align);
                    workSheet.Cells[row, 2].PutValue(rr.Name);
                    workSheet.Cells[row, 2].SetStyle(align);
                    workSheet.Cells[row, 3].PutValue(rr.Division);
                    workSheet.Cells[row, 3].SetStyle(align);
                    workSheet.Cells[row, 10].PutValue(rr.CreatedBy);
                    workSheet.Cells[row, 10].SetStyle(align);
                    workSheet.Cells[row, 11].PutValue(rr.CreateDate);
                    workSheet.Cells[row, 11].SetStyle(date);
                    row++;
                }
                else
                {
                    foreach (var detail in rr.StoreLookups)
                    {
                        workSheet.Cells[row, 0].PutValue(rr.ClusterID);
                        workSheet.Cells[row, 0].SetStyle(align);
                        workSheet.Cells[row, 1].PutValue(rr.Year);
                        workSheet.Cells[row, 1].SetStyle(align);
                        workSheet.Cells[row, 2].PutValue(rr.Name);
                        workSheet.Cells[row, 2].SetStyle(align);
                        workSheet.Cells[row, 3].PutValue(rr.Division);
                        workSheet.Cells[row, 3].SetStyle(align);
                        workSheet.Cells[row, 4].PutValue(detail.League);
                        workSheet.Cells[row, 4].SetStyle(align);
                        workSheet.Cells[row, 5].PutValue(detail.Store);
                        workSheet.Cells[row, 5].SetStyle(align);
                        workSheet.Cells[row, 6].PutValue(detail.DBA);
                        workSheet.Cells[row, 6].SetStyle(align);
                        workSheet.Cells[row, 7].PutValue(detail.Mall);
                        workSheet.Cells[row, 7].SetStyle(align);
                        workSheet.Cells[row, 8].PutValue(detail.City);
                        workSheet.Cells[row, 8].SetStyle(align);
                        workSheet.Cells[row, 9].PutValue(detail.State);
                        workSheet.Cells[row, 9].SetStyle(align);
                        workSheet.Cells[row, 10].PutValue(rr.CreatedBy);
                        workSheet.Cells[row, 10].SetStyle(align);
                        workSheet.Cells[row, 11].PutValue(rr.CreateDate);
                        workSheet.Cells[row, 11].SetStyle(date);
                        row++;
                    }
                }
            }

            for (int i = 0; i < 12; i++)
            {
                workSheet.AutoFitColumn(i);
            }

            return excelDocument;
        }

        private Workbook RetrieveStoreBTSExcelFile(bool errorFile)
        {
            int row = 0;
            int col = 0;

            Aspose.Cells.License license = new Aspose.Cells.License();
            license.SetLicense("C:\\Aspose\\Aspose.Cells.lic");

            Workbook excelDocument = new Workbook();
            Aspose.Cells.Worksheet workSheet = excelDocument.Worksheets[0];

            Aspose.Cells.Style style = excelDocument.CreateStyle();
            style.Font.IsBold = true;

            workSheet.Cells[row, col].PutValue("Cluster ID");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("Year");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("Name");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("Division");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("League");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("Store");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("DBA");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("Mall");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("City");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("State");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("Created By");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("Create Date");
            workSheet.Cells[row, col].SetStyle(style);
            if (errorFile)
            {
                col++;
                workSheet.Cells[row, col].PutValue("Message");
                workSheet.Cells[row, col].SetStyle(style);
            }
            return excelDocument;
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
            
            exportBTS.excelDocument.Save("BTS_Unassigned.xls", Aspose.Excel.SaveType.OpenInExcel, Aspose.Excel.FileFormatType.Default, System.Web.HttpContext.Current.Response);
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

            StoreBTS group = db.StoreBTS.Where(sb => sb.ID == ID).First();
            group.Count--;
            db.SaveChanges();

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
            StoreBTS group = db.StoreBTS.Where(sb => sb.ID == ID).First();
            group.Count += count;
            db.Entry(group).State = System.Data.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Details", new { ID, message = string.Format("Copied group {0} stores, {1} errors.", count, errors) });            
        }

        public ActionResult AddDetail(int ID, string store)
        {
            string message = "";
            StoreBTS storebts = db.StoreBTS.Where(sb => sb.ID == ID).First();
            string division = storebts.Division;
            store = store.PadLeft(5, '0');
            int year = storebts.Year;

            if (currentUser.HasDivision(AppName, division))
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

                            StoreBTS group = db.StoreBTS.Where(sb => sb.ID == ID).First();
                            group.Count++;
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
                }
                else                 
                    message = "Invalid division/store.";                
            }
            else            
                message = "You are not autorized for this division.";
            
            return RedirectToAction("Details", new { ID = ID, message = message });
        }

        public ActionResult ShowDetail(int ID, string div, string store)
        {
            List<StoreBTSDetail> model = db.StoreBTSDetails.Where(sbd => sbd.Division == div && sbd.Store == store && sbd.GroupID == ID).ToList();
            return View(model);
        }

        public ActionResult ConfirmMove(int ID, string div, string store, int year)
        {
            StoreBTSDetail det = db.StoreBTSDetails.Where(sbd => sbd.Division == div && sbd.Store == store && sbd.Year == year).First();
            StoreBTS group = db.StoreBTS.Where(sb => sb.ID == det.GroupID).First();
            group.Count--;
            db.SaveChanges();
            
            det.GroupID = ID;
            det.CreateDate = DateTime.Now;
            det.CreatedBy = currentUser.NetworkID;
            db.SaveChanges();

            group = db.StoreBTS.Where(sb => sb.ID == ID).First();
            group.Count++;
            db.SaveChanges();

            return RedirectToAction("Details", new { ID });
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public ActionResult Save(IEnumerable<HttpPostedFileBase> attachments, int groupID)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            string Division = "";
            string Store = "";
            List<StoreBTSDetail> errorList = new List<StoreBTSDetail>();
            int errors = 0;
            int addedCount = 0;
            StoreBTS group = (from a in db.StoreBTS where a.ID == groupID select a).First();
            int year = group.Year;

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
                string mainDivision;
                int row = 1;
                if ((Convert.ToString(mySheet.Cells[0, 0].Value).Contains("Div")) &&
                    (Convert.ToString(mySheet.Cells[0, 1].Value).Contains("Store"))
                    )
                {
                    Division = Convert.ToString(mySheet.Cells[row, 0].Value).PadLeft(2, '0');
                    mainDivision = Division;

                    while (mySheet.Cells[row, 0].Value != null)
                    {
                        Division = Convert.ToString(mySheet.Cells[row, 0].Value).PadLeft(2,'0');

                        Store = Convert.ToString(mySheet.Cells[row, 1].Value).PadLeft(5, '0');
                        var foundStore = (from a in db.StoreLookups where ((a.Division == Division) && (a.Store == Store)) select a);
                        if (foundStore.Count() > 0)
                        {
                            StoreBTSDetail det = new StoreBTSDetail();
                            try
                            {
                                det.Division = Division;
                                det.Store = Store;
                                det.CreateDate = DateTime.Now;
                                det.CreatedBy = User.Identity.Name;
                                det.GroupID = groupID;
                                det.Year = year;
                                db.StoreBTSDetails.Add(det);
                                db.SaveChanges();
                                addedCount++;
                            }
                            catch (Exception ex)
                            {
                                db.StoreBTSDetails.Remove(det);
                                errors++;
                                //add to errors list
                                StoreBTSDetail errorDet = new StoreBTSDetail();
                                errorDet.Division = Division;
                                errorDet.Store = Store;
                                errorDet.Year = year;
                                errorDet.CreateDate = DateTime.Now;
                                errorDet.CreatedBy = User.Identity.Name;
                                errorDet.GroupID = groupID;
                                while (ex.Message.Contains("inner exception"))
                                {
                                    ex = ex.InnerException;
                                }
                                errorDet.errorMessage = ex.Message;

                                errorList.Add(errorDet);
                            }
                        }
                        row++;
                    }

                    group.Count += addedCount;
                    db.SaveChanges();

                }
                else
                {
                    return Content("Incorrect header, please use template.");
                }
            }
            if (errorList.Count > 0)
            {
                Session["errorList"] = errorList;
                return Content(errors + " Errors on spreadsheet (" + addedCount + " successfully uploaded)");

            }
            else
            {
                return Content("");
            }
        }

        public ActionResult BTSErrors()
        {
            List<StoreBTSDetail> errorList = new List<StoreBTSDetail>();
            if (Session["errorList"] != null)
            {
                errorList = (List<StoreBTSDetail>)Session["errorList"];
            }

            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();

            Aspose.Excel.Worksheet mySheet = excelDocument.Worksheets[0];
            int row = 1;
            mySheet.Cells[0, 0].PutValue("Div");
            mySheet.Cells[0, 1].PutValue("Store");
            foreach (StoreBTSDetail p in errorList)
            {
                mySheet.Cells[row, 0].PutValue(p.Division);
                mySheet.Cells[row, 1].PutValue(p.Store);
                mySheet.Cells[row, 2].PutValue(p.errorMessage);

                row++;
            }

            excelDocument.Save("BTSUploadErrors.xls", Aspose.Excel.SaveType.OpenInExcel, Aspose.Excel.FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();

        }

        public ActionResult ExcelTemplate()
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            FileStream file = new FileStream(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SeasonalityTemplate"]), FileMode.Open, System.IO.FileAccess.Read);
            Byte[] data1 = new Byte[file.Length];
            file.Read(data1, 0, data1.Length);
            file.Close();
            MemoryStream memoryStream1 = new MemoryStream(data1);
            excelDocument.Open(memoryStream1);
            excelDocument.Save("StoreBTSDetails.xls", Aspose.Excel.SaveType.OpenInExcel, Aspose.Excel.FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }
    }
}