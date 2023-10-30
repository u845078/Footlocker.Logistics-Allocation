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
    [CheckPermission(Roles = "Director of Allocation,Advanced Merchandiser Processes,Admin,Support")]
    public class StoreSeasonalityController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
        readonly ConfigService configService = new ConfigService();

        public ActionResult Index(string message, string div)
        {
            ViewData["message"] = message;
            StoreSeasonalityModel model = new StoreSeasonalityModel()
            {
                Divisions = currentUser.GetUserDivisions(AppName)
            };

            List<InstanceDivision> instDivs = (from a in db.InstanceDivisions
                                               join b in db.Configs on a.InstanceID equals b.InstanceID
                                               join c in db.ConfigParams on b.ParamID equals c.ParamID
                                               where c.Name == "BTS_SEASONALITY" && 
                                                     b.Value != "true"
                                               select a).ToList();
            model.Divisions = (from a in model.Divisions 
                               join b in instDivs 
                                 on a.DivCode equals b.Division 
                               select a).ToList();

            if (model.Divisions.Count() > 0)
            {

                if (string.IsNullOrEmpty(div))                
                    div = model.Divisions[0].DivCode;
                
                model.CurrentDivision = div;
                model.List = db.StoreSeasonality.Where(ss => ss.Division == div).ToList();
            }

            model.UnassignedStores = (from a in db.vValidStores
                                      join b in db.StoreSeasonalityDetails on new { a.Division, a.Store } equals new { b.Division, b.Store } into subset
                                      from sc in subset.DefaultIfEmpty()
                                      where sc == null && a.Division == div
                                      select a).ToList();
            
            return View(model);
        }

        public ActionResult Create(string div, string name)
        {
            string message = "";
            try
            {
                StoreSeasonality group = new StoreSeasonality()
                {
                    Name = name,
                    Division = div,
                    CreatedBy = currentUser.NetworkID,
                    CreateDate = DateTime.Now
                };

                db.StoreSeasonality.Add(group);
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

            return RedirectToAction("Index", new { message, div });
        }

        public ActionResult Edit(int ID)
        {
            StoreSeasonality group = db.StoreSeasonality.Where(ss => ss.ID == ID).First();

            return View(group);
        }

        [HttpPost]
        public ActionResult Edit(StoreSeasonality model)
        {
            model.CreateDate = DateTime.Now;
            model.CreatedBy = currentUser.NetworkID;

            db.Entry(model).State = System.Data.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index", new { message = "", div = model.Division});
        }

        public ActionResult Search(string store, string div)
        {
            var list = db.StoreSeasonalityDetails.Where(ssd => ssd.Store.Contains(store)).ToList();

            if (list.Count() == 1)
            {
                StoreSeasonalityDetail det = list.First();
                StoreSeasonality group = db.StoreSeasonality.Where(ss => ss.ID == det.GroupID).First();

                return View("ShowSearchResult", group);
            }
            else if (list.Count() == 0)
            {
                return RedirectToAction("Index", new { message = "Store " + store + " is not in any group.", div = div });
            }
            else            
                return View(list);            
        }

        public ActionResult ShowSearchResult(string store, string div)
        {
            StoreSeasonalityDetail det = db.StoreSeasonalityDetails.Where(ssd => ssd.Store == store && ssd.Division == div).First();
            StoreSeasonality group = db.StoreSeasonality.Where(ss => ss.ID == det.GroupID).First();

            return View(group);
        }

        public ActionResult Delete(int ID)
        {
            var details = db.StoreSeasonalityDetails.Where(ssd => ssd.GroupID == ID).ToList();
            foreach (StoreSeasonalityDetail det in details)            
                db.StoreSeasonalityDetails.Remove(det);
            
            db.SaveChanges();
            StoreSeasonality group = db.StoreSeasonality.Where(ss => ss.ID == ID).First();

            string div = group.Division;
            db.StoreSeasonality.Remove(group);
            db.SaveChanges();

            return RedirectToAction("Index", new {message = "Item deleted", div});
        }

        public ActionResult Details(int ID, string message)
        {
            StoreSeasonalityDetailModel model = new StoreSeasonalityDetailModel();
            ViewData["message"] = message;

            model.details = (from a in db.StoreSeasonalityDetails 
                             join b in db.vValidStores 
                             on new { a.Division, a.Store } equals new { b.Division, b.Store } 
                             where a.GroupID == ID 
                             select b).ToList();

            model.divisions = Footlocker.Common.DivisionService.ListDivisions();
            model.division = (from a in db.StoreSeasonality 
                              where a.ID == ID 
                              select a.Division).First();

            model.UnassignedStores = (from a in db.vValidStores
                                      join b in db.StoreSeasonalityDetails on new { a.Division, a.Store } equals new { b.Division, b.Store } into subset
                                      from sc in subset.DefaultIfEmpty()
                                      where sc == null && a.Division == model.division
                                      select a).ToList();

            ViewData["GroupID"] = ID;
            ViewData["GroupName"] = (from a in db.StoreSeasonality 
                                     where a.ID == ID 
                                     select a.Name).First();

            return View(model);
        }

        [GridAction]
        public ActionResult _RefreshGrid(int groupID)
        {
            List<ValidStoreLookup> list = (from a in db.StoreSeasonalityDetails 
                                           join b in db.vValidStores 
                                           on new { a.Division, a.Store } equals new { b.Division, b.Store } 
                                           where a.GroupID == groupID 
                                           select b).ToList();
            return View(new GridModel(list)); 
        }

        public ActionResult DeleteDetail(int ID, string div, string store)
        {
            StoreSeasonalityDetail det = db.StoreSeasonalityDetails.Where(ssd => ssd.GroupID == ID && ssd.Division == div && ssd.Store == store).First();
            db.StoreSeasonalityDetails.Remove(det);
            db.SaveChanges();

            return RedirectToAction("Details", new { ID});
        }

        public ActionResult AddDetail(int ID, string store, string div)
        {
            string message = "";
            store = store.PadLeft(5, '0');
            var existing = db.StoreSeasonalityDetails.Where(ssd => ssd.Division == div && ssd.Store == store);
            if (existing.Count() > 0)
            {
                int oldGroup = existing.First().GroupID;
                ViewData["OriginalGroup"] = db.StoreSeasonality.Where(ss => ss.ID == oldGroup).First().Name;
                ViewData["NewGroup"] = db.StoreSeasonality.Where(ss => ss.ID == ID).First().Name;
                //give them a confirmation screen about the move
                ViewData["GroupID"] = ID;
                ViewData["div"] = div;
                ViewData["store"] = store;
                return View();                
            }
            else
            {
                try
                {
                    // Validate entered store is valid store
                    if (!db.vValidStores.Any(vs => vs.Division == div && vs.Store == store))
                    {
                        throw new ArgumentException(string.Format("Store '{0}' is not a valid store in division '{1}'. Please enter a valid store.", store, div));
                    }

                    // Add store to store seasonality grouping
                    StoreSeasonalityDetail det = new StoreSeasonalityDetail()
                    {
                        GroupID = ID,
                        Division = div,
                        Store = store,
                        CreateDate = DateTime.Now,
                        CreatedBy = currentUser.NetworkID
                    };

                    db.StoreSeasonalityDetails.Add(det);
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

            return RedirectToAction("Details", new { ID, message });
        }

        public ActionResult ShowDetail(int ID, string div, string store)
        {
            List<StoreSeasonalityDetail> model = db.StoreSeasonalityDetails.Where(ssd => ssd.Division == div && ssd.Store == store && ssd.GroupID == ID).ToList();
            return View(model);
        }

        public ActionResult ConfirmMove(int ID, string div, string store)
        {
            StoreSeasonalityDetail det = db.StoreSeasonalityDetails.Where(ssd => ssd.Division == div && ssd.Store == store).First();
            det.GroupID = ID;
            det.CreateDate = DateTime.Now;
            det.CreatedBy = currentUser.NetworkID;
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
            List<StoreSeasonalityDetail> errorList = new List<StoreSeasonalityDetail>();
            int errors = 0;
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
                        var foundStore = (from a in db.vValidStores where ((a.Division == Division) && (a.Store == Store)) select a);
                        if (foundStore.Count() > 0)
                        {
                            StoreSeasonalityDetail det = new StoreSeasonalityDetail();
                            try
                            {
                                det.Division = Division;
                                det.Store = Store;
                                det.CreateDate = DateTime.Now;
                                det.CreatedBy = User.Identity.Name;
                                det.GroupID = groupID;
                                db.StoreSeasonalityDetails.Add(det);
                                db.SaveChanges();
                                addedCount++;
                            }
                            catch (Exception ex)
                            {
                                db.StoreSeasonalityDetails.Remove(det);
                                errors++;
                                //add to errors list
                                StoreSeasonalityDetail errorDet = new StoreSeasonalityDetail();
                                errorDet.Division = Division;
                                errorDet.Store = Store;
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
                        else
                        {
                            errors++;
                            //add to errors list
                            StoreSeasonalityDetail errorDet = new StoreSeasonalityDetail();
                            errorDet.Division = Division;
                            errorDet.Store = Store;
                            errorDet.CreateDate = DateTime.Now;
                            errorDet.CreatedBy = User.Identity.Name;
                            errorDet.GroupID = groupID;
                            errorDet.errorMessage = String.Format("Store '{0}' was not found to be a valid store. Please only enter existing, valid stores.", Store);
                            errorList.Add(errorDet);
                        }

                        row++;
                    }
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

        public ActionResult SeasonalityErrors()
        {
            List<StoreSeasonalityDetail> errorList = new List<StoreSeasonalityDetail>();
            if (Session["errorList"] != null)
            {
                errorList = (List<StoreSeasonalityDetail>)Session["errorList"];
            }


            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();

            Worksheet mySheet = excelDocument.Worksheets[0];
            int row = 1;
            mySheet.Cells[0, 0].PutValue("Div");
            mySheet.Cells[0, 1].PutValue("Store");
            foreach (StoreSeasonalityDetail p in errorList)
            {
                mySheet.Cells[row, 0].PutValue(p.Division);
                mySheet.Cells[row, 1].PutValue(p.Store);
                mySheet.Cells[row, 2].PutValue(p.errorMessage);

                row++;
            }

            excelDocument.Save("SeasonalityUploadErrors.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
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
            excelDocument.Save("StoreSeasonalityDetails.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

        [GridAction]
        public ActionResult Grid_GetGroups(string div)
        {
            int instanceID = configService.GetInstance(div);
            
            // Get div codes that user has access to in the Europe instance
            var userDivCodes = currentUser.GetUserDivList(AppName);
            var userInstanceDivCodes = db.InstanceDivisions.Where(id => id.InstanceID == instanceID && userDivCodes.Contains(id.Division))
                                                           .Select(id => id.Division);

            // Get all Store Seasonality Groups of these retrieved divs (and only detail counts of valid store detail records)
            var userInstanceSSGroups = db.StoreSeasonality
                .Include("Details")
                .Include("Details.ValidStore")
                .Where(ss => userInstanceDivCodes.Contains(ss.Division));
            var ssGroupsOfDiv = userInstanceSSGroups.Where(g => g.Division == div);

            return View(new GridModel(ssGroupsOfDiv));
        }
    }
}
