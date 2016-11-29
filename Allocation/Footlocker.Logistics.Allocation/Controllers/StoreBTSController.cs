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

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Director of Allocation,VP of Allocation,Admin,Support")]
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

            StoreBTSModel model = new StoreBTSModel();
            model.Divisions = Divisions();
            if (model.Divisions.Count() > 0)
            {
                
                if ((div==null)||(div == ""))
                {
                    div = model.Divisions[0].DivCode;
                }
                model.CurrentDivision = div;
                model.List = (from a in db.StoreBTS where a.Division == div select a).ToList();
                var query = (from a in db.StoreBTSControls where a.Division == div select a);
                if (query.Count() > 0)
                {
                    model.Control = query.First();
                }
                else
                {
                    model.Control = new StoreBTSControl();
                    model.Control.Division = div;
                    model.Control.TY = DateTime.Now.Year;
                    model.Control.LY = DateTime.Now.Year-1;

                }
            }
            model.Years = new List<int>();
            model.Years.Add((System.DateTime.Now.Year + 1));
            model.Years.Add(System.DateTime.Now.Year);
            model.Years.Add((System.DateTime.Now.Year-1));
            model.Years.Add((System.DateTime.Now.Year - 2));
            model.Years.Add((System.DateTime.Now.Year - 3));
            model.Years.Add((System.DateTime.Now.Year - 4));
            model.Years.Add((System.DateTime.Now.Year - 5));

            if (year != null)
            {
                model.CurrentYear = (int)year;
            }
            else
            {
                model.CurrentYear = System.DateTime.Now.Year;
            }
            return View(model);
        }

        public ActionResult Create(string div, string name, int year)
        {
            string message="";
            if ((from a in db.StoreBTS where ((a.Division == div) && (a.Name == name) && (a.Year == year)) select a).Count() > 0)
            {
                message = "The group " + name + " was already created for " + year + ".";
            }
            else
            {
                if ((from a in db.StoreClusters where ((a.Division == div) && (a.Name == name)) select a).Count() == 1)
                {
                    try
                    {
                        StoreBTS group = new StoreBTS();
                        group.Name = name;
                        group.Year = year;
                        group.Division = div;
                        group.CreatedBy = User.Identity.Name;
                        group.CreateDate = DateTime.Now;

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
            return RedirectToAction("Index", new { message = message, div = div });
        }

        public ActionResult Edit(int ID)
        {
            StoreBTS group = (from a in db.StoreBTS where a.ID == ID select a).First();

            return View(group);
        }


        [HttpPost]
        public ActionResult SetYear(StoreBTSModel model)
        {
            StoreBTSControl control = (from a in db.StoreBTSControls where a.Division == model.Control.Division select a).FirstOrDefault();
            if (control == null)
            {
                control = new StoreBTSControl();

                control.Division = model.Control.Division;
                control.TY = model.Control.TY;
                control.LY = model.Control.LY;

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

        [HttpPost]
        public ActionResult Edit(StoreBTS model)
        {
            //no longer can edit name of plan
            //model.CreateDate = DateTime.Now;
            //model.CreatedBy = User.Identity.Name;

            //db.Entry(model).State = System.Data.EntityState.Modified;
            //db.SaveChanges();
            return RedirectToAction("Index");

        }

        public ActionResult Search(string store, int year, string div)
        {
            var list = (from a in db.StoreBTSDetails where (a.Store.Contains(store) && (a.Year == year) && (a.Division == div)) select a);

            if (list.Count() == 1)
            {
                StoreBTSDetail det = list.First();
                StoreBTS group = (from a in db.StoreBTS where a.ID == det.GroupID select a).First();

                return View("ShowSearchResult", group);
            }
            else if (list.Count() == 0)
            {
                return RedirectToAction("Index", new {message = "Store " + div + "-" + store + " is not in any group for year " + year, div = div});
            }
            else
            {
                return View(list);
            }

        }

        public ActionResult ShowSearchResult(string store, string div, int year)
        {
            var list = (from a in db.StoreBTSDetails where ((a.Store ==store)&&(a.Division == div)&&(a.Year == year)) select a);
            StoreBTSDetail det = list.First();
            StoreBTS group = (from a in db.StoreBTS where a.ID == det.GroupID select a).First();

            return View(group);
        }

        public ActionResult Delete(int ID)
        {
            var details = (from a in db.StoreBTSDetails where a.GroupID == ID select a);
            foreach (StoreBTSDetail det in details)
            {
                db.StoreBTSDetails.Remove(det);
            }
            db.SaveChanges();
            StoreBTS group = (from a in db.StoreBTS where a.ID == ID select a).First();

            db.StoreBTS.Remove(group);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Details(int ID, string message)
        {
            StoreBTSDetailModel model = new StoreBTSDetailModel();
            ViewData["message"] = message;

            model.details = (from a in db.StoreBTSDetails join b in db.StoreLookups on new {a.Division, a.Store} equals new {b.Division, b.Store} where a.GroupID == ID select b).ToList();
            model.header = (from a in db.StoreBTS where a.ID == ID select a).FirstOrDefault();
            model.divisions = Footlocker.Common.DivisionService.ListDivisions();
            ViewData["GroupID"] = ID;
            string div = (from a in db.StoreBTS where a.ID == ID select a.Division).First();
            model.CopyFrom = (from a in db.StoreBTS where ((a.Year == System.DateTime.Now.Year - 1)&&(a.Division == div)) select a).ToList();
            model.division = (from a in db.StoreBTS where a.ID == ID select a.Division).First();
            int year = (from a in db.StoreBTS where a.ID == ID select a.Year).First();
            
            model.UnassignedStores = (from a in db.StoreLookups
                                      join b in (from c in db.StoreBTSDetails where c.Year == year select c) on new { a.Division, a.Store } equals new { b.Division, b.Store } into subset
                                      from sc in subset.DefaultIfEmpty()
                                      where ((sc == null) && (a.Division == model.division))
                                      select a).ToList();

            return View(model);
        }

        public ActionResult DownloadUnassignedStores(string div, int year)
        {
            List<StoreLookup> stores = (from a in db.StoreLookups.Include("StoreExtension")
                                        join b in
                                            (from c in db.StoreBTSDetails where c.Year == year select c) on new { a.Division, a.Store } equals new { b.Division, b.Store } into subset
                                        from sc in subset.DefaultIfEmpty()
                                        where ((sc == null) && (a.Division == div))
                                        select a).ToList();
            List<StoreExtension> excludedstores = (from a in db.StoreExtensions where (a.ExcludeStore == true) select a).ToList();

            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();

            Worksheet workSheet = excelDocument.Worksheets[0];

            int row = 0;
            workSheet.Cells[0, 0].PutValue("Division");
            workSheet.Cells[0, 0].Style.Font.IsBold = true;
            workSheet.Cells[0, 1].PutValue("Region");
            workSheet.Cells[0, 1].Style.Font.IsBold = true;
            workSheet.Cells[0, 2].PutValue("League");
            workSheet.Cells[0, 2].Style.Font.IsBold = true;
            workSheet.Cells[0, 3].PutValue("Store");
            workSheet.Cells[0, 3].Style.Font.IsBold = true;
            workSheet.Cells[0, 4].PutValue("Mall");
            workSheet.Cells[0, 4].Style.Font.IsBold = true;
            workSheet.Cells[0, 5].PutValue("State");
            workSheet.Cells[0, 5].Style.Font.IsBold = true;
            workSheet.Cells[0, 6].PutValue("City");
            workSheet.Cells[0, 6].Style.Font.IsBold = true;
            workSheet.Cells[0, 7].PutValue("DBA");
            workSheet.Cells[0, 7].Style.Font.IsBold = true;
            workSheet.Cells[0, 8].PutValue("Status");
            workSheet.Cells[0, 8].Style.Font.IsBold = true;
            workSheet.Cells[0, 9].PutValue("Closed Date");
            workSheet.Cells[0, 9].Style.Font.IsBold = true;

            List<MainframeStore> ClosingDates = new List<MainframeStore>();
            List<string> ClosedStores = new List<string>();
            MainframeStoreDAO dao = new MainframeStoreDAO();

            foreach (StoreLookup s in stores)
            {
                if (s.status == "C")
                {
                    ClosedStores.Add(s.Store);
                    if (ClosedStores.Count == 15)
                    {
                        ClosingDates.AddRange(dao.GetClosingDates(ClosedStores, div));
                        ClosedStores.Clear();
                    }
                }
            }
            if (ClosedStores.Count > 0)
            {
                ClosingDates.AddRange(dao.GetClosingDates(ClosedStores, div));
                ClosedStores.Clear();
            }

            foreach (StoreLookup s in stores)
            {
                if ((from a in excludedstores where ((a.Store == s.Store) && (a.Division == s.Division)) select a).Count() == 0)
                {
                    row++;
                    workSheet.Cells[row, 0].PutValue(s.Division);
                    workSheet.Cells[row, 1].PutValue(s.Region);
                    workSheet.Cells[row, 2].PutValue(s.League);
                    workSheet.Cells[row, 3].PutValue(s.Store);
                    workSheet.Cells[row, 4].PutValue(s.Mall);
                    workSheet.Cells[row, 5].PutValue(s.State);
                    workSheet.Cells[row, 6].PutValue(s.City);
                    workSheet.Cells[row, 7].PutValue(s.DBA);
                    workSheet.Cells[row, 8].PutValue(s.status);
                    if (s.status == "C")
                    {
                        var query = (from a in ClosingDates where a.Store == s.Store select a.ClosedDate);
                        if (query.Count() > 0)
                        {
                            workSheet.Cells[row, 9].PutValue(query.First());
                        }
                    }
                }
            }

            excelDocument.Save("BTS_Unassigned.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }


        [GridAction]
        public ActionResult _RefreshGrid(int groupID)
        {
            List<StoreLookup> list = (from a in db.StoreBTSDetails join b in db.StoreLookups on new { a.Division, a.Store } equals new { b.Division, b.Store } where a.GroupID == groupID select b).ToList();
            return View(new GridModel(list)); 
        }

        public ActionResult DeleteDetail(int ID, string div, string store)
        {
            StoreBTSDetail det = (from a in db.StoreBTSDetails where ((a.GroupID == ID) && (a.Division == div)&&(a.Store==store)) select a).First();
            db.StoreBTSDetails.Remove(det);
            db.SaveChanges();

            StoreBTS group = (from a in db.StoreBTS where a.ID == ID select a).First();
            group.Count = group.Count - 1;
            db.SaveChanges();

            return RedirectToAction("Details", new { ID = ID});
        }

        public ActionResult CopyStoreBTS(int ID, int CopyFrom)
        {
            int year = (from a in db.StoreBTS where a.ID == ID select a.Year).First();
            var copyFromList = (from a in db.StoreBTSDetails where a.GroupID == CopyFrom select a);

            StoreBTSDetail newDet;
            List<StoreBTSDetail> list = new List<StoreBTSDetail>();
            foreach (StoreBTSDetail det in copyFromList)
            {
                newDet = new StoreBTSDetail();
                newDet.GroupID = ID;
                newDet.Year = year;
                newDet.Store = det.Store;
                newDet.Division = det.Division;
                newDet.CreatedBy = User.Identity.Name;
                newDet.CreateDate = DateTime.Now;
                list.Add(newDet);
            }
            int errors = 0;
            int count = 0;
            foreach (StoreBTSDetail det in list)
            {
                if ((from a in db.StoreBTSDetails where ((a.Store == det.Store) && (a.Division == det.Division) && (a.Year == det.Year)) select a).Count() == 0)
                {
                    db.StoreBTSDetails.Add(det);
                    db.SaveChanges();
                    count++;
                }
                else
                {
                    errors++;
                }
            }
            StoreBTS group = (from a in db.StoreBTS where a.ID == ID select a).First();
            group.Count += count;
            db.Entry(group).State = System.Data.EntityState.Modified;
            db.SaveChanges();


            return RedirectToAction("Details", new { ID = ID, message = "Copied group " + count + " stores, " + errors + " errors." });            
        }

        public ActionResult AddDetail(int ID, string store)
        {
            string message = "";
            StoreBTS storebts =(from a in db.StoreBTS where a.ID == ID select a).First();
            string division = storebts.Division;
            store = store.PadLeft(5, '0');
            int year = storebts.Year;

            if (Footlocker.Common.WebSecurityService.UserHasDivision(UserName,"Allocation",division))
            {
                var storequery = (from a in db.StoreLookups where ((a.Division == division)&&(a.Store==store)) select a);
                if (storequery.Count() > 0)
                {
                    var existing = (from a in db.StoreBTSDetails where ((a.Division == division) && (a.Store == store)&&(a.Year == year)) select a);
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
                        //RedirectToAction("ConfirmMove", new { ID = ID, vendorNumber = vendorNumber });
                    }
                    else
                    {
                        try
                        {
                            StoreBTSDetail det = new StoreBTSDetail();
                            det.GroupID = ID;
                            det.Division = division;
                            det.Store = store;
                            det.Year = year;
                            det.CreateDate = DateTime.Now;
                            det.CreatedBy = User.Identity.Name;
                            db.StoreBTSDetails.Add(det);
                            db.SaveChanges();

                            StoreBTS group = (from a in db.StoreBTS where a.ID == ID select a).First();
                            group.Count = group.Count + 1;
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
                {
                    message = "Invalid division/store.";
                }
            }
            else
            {
                message = "You are not autorized for this division.";
            }
            return RedirectToAction("Details", new { ID = ID, message = message });
        }


        public ActionResult ShowDetail(int ID, string div, string store)
        {
            List<StoreBTSDetail> model = (from a in db.StoreBTSDetails where ((a.Division == div) && (a.Store == store) && (a.GroupID == ID)) select a).ToList();
            return View(model);
        }

        public ActionResult ConfirmMove(int ID, string div, string store, int year)
        {
            StoreBTSDetail det = (from a in db.StoreBTSDetails where ((a.Division == div)&&(a.Store == store)&&(a.Year == year)) select a).First();
            StoreBTS group = (from a in db.StoreBTS where a.ID == det.GroupID select a).First();
            group.Count = group.Count - 1;
            db.SaveChanges();

            
            det.GroupID = ID;
            det.CreateDate = DateTime.Now;
            det.CreatedBy = User.Identity.Name;
            db.SaveChanges();

            group = (from a in db.StoreBTS where a.ID == ID select a).First();
            group.Count = group.Count + 1;
            db.SaveChanges();

            return RedirectToAction("Details", new { ID = ID });
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
                    //if (!(Footlocker.Common.Services.WebSecurityService.UserHasDivision(User.Identity.Name.Split('\\')[1], "Allocation", Division)))
                    //{
                    //    return Content("You do not have permission to update this division.");
                    //}
                    while (mySheet.Cells[row, 0].Value != null)
                    {
                        Division = Convert.ToString(mySheet.Cells[row, 0].Value).PadLeft(2,'0');
                        //if (!(Division.Equals(mainDivision)))
                        //{
                        //    return Content("Spreadsheet must be for one division only.");
                        //}

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

                    group.Count = group.Count + addedCount;
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
            //FileStream file = new FileStream(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["ProductTypeTemplate"]), FileMode.Open, System.IO.FileAccess.Read);
            //Byte[] data1 = new Byte[file.Length];
            //file.Read(data1, 0, data1.Length);
            //file.Close();
            //MemoryStream memoryStream1 = new MemoryStream(data1);
            //excelDocument.Open(memoryStream1);
            Worksheet mySheet = excelDocument.Worksheets[0];
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

            excelDocument.Save("BTSUploadErrors.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
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
            excelDocument.Save("StoreBTSDetails.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }
    }
}
