using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Aspose.Cells;
using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Spreadsheets;

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

                List<string> uniqueNames = (from a in model.List
                                            where !string.IsNullOrEmpty(a.CreatedBy)
                                            select a.CreatedBy).Distinct().ToList();

                Dictionary<string, string> fullNamePairs = LoadUserNames(uniqueNames);

                foreach (var item in fullNamePairs)
                {
                    model.List.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
                }
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
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StoreSeasonality model)
        {
            StoreSeasonality group = db.StoreSeasonality.Where(ss => ss.ID == model.ID).FirstOrDefault();

            group.CreateDate = DateTime.Now;
            group.CreatedBy = currentUser.NetworkID;
            group.Name = model.Name; 
            
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

        #region Upload spreadsheet
        public ActionResult SeasonalityTemplate()
        {
            SeasonalitySpreadsheet seasonalitySpreadsheet = new SeasonalitySpreadsheet(appConfig, configService);
            Workbook excelDocument;

            excelDocument = seasonalitySpreadsheet.GetTemplate();

            excelDocument.Save(System.Web.HttpContext.Current.Response, "SeasonalityTemplate.xlsx", ContentDisposition.Attachment, seasonalitySpreadsheet.SaveOptions);
            return View();
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public ActionResult Save(IEnumerable<HttpPostedFileBase> attachments, int groupID)
        {
            SeasonalitySpreadsheet seasonalitySpreadsheet = new SeasonalitySpreadsheet(appConfig, configService);

            int successCount = 0;
            string message;

            foreach (HttpPostedFileBase file in attachments)
            {
                seasonalitySpreadsheet.Save(file, groupID);

                if (!string.IsNullOrEmpty(seasonalitySpreadsheet.message))
                    return Content(seasonalitySpreadsheet.message);
                else
                {
                    if (seasonalitySpreadsheet.errorList.Count() > 0)
                    {
                        Session["errorList"] = seasonalitySpreadsheet.errorList;

                        message = string.Format("{0} successfully uploaded, {1} Errors", seasonalitySpreadsheet.validData.Count.ToString(),
                            seasonalitySpreadsheet.errorList.Count.ToString());

                        return Content(message);
                    }
                }

                successCount += seasonalitySpreadsheet.validData.Count();
            }

            return Json(new { message = string.Format("Upload complete. Added {0} store(s)", successCount) }, "application/json");
        }

        public ActionResult SeasonalityErrors()
        {
            List<StoreSeasonalityDetail> errors = (List<StoreSeasonalityDetail>)Session["errorList"];
            Workbook excelDocument;
            SeasonalitySpreadsheet seasonalitySpreadsheet = new SeasonalitySpreadsheet(appConfig, configService);

            if (errors != null)
            {
                excelDocument = seasonalitySpreadsheet.GetErrors(errors);
                excelDocument.Save(System.Web.HttpContext.Current.Response, "SeasonalityUploadErrors.xlsx", ContentDisposition.Attachment, seasonalitySpreadsheet.SaveOptions);
            }
            return View();
        }
        #endregion

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

            Dictionary<string, string> fullNamePairs = new Dictionary<string, string>();
            var uniqueNames = (from a in ssGroupsOfDiv
                               where !string.IsNullOrEmpty(a.CreatedBy)
                               select a.CreatedBy).Distinct();

            List<ApplicationUser> allUserNames = GetAllUserNamesFromDatabase();

            foreach (var item in uniqueNames)
            {
                if (!item.Contains(" ") && !string.IsNullOrEmpty(item))
                {
                    string userLookup = item.Replace('\\', '/');
                    userLookup = userLookup.Replace("CORP/", "");

                    if (userLookup.Substring(0, 1) == "u")
                        fullNamePairs.Add(item, allUserNames.Where(aun => aun.UserName == userLookup).Select(aun => aun.FullName).FirstOrDefault());
                    else
                        fullNamePairs.Add(item, item);
                }
                else
                    fullNamePairs.Add(item, item);
            }

            foreach (var item in fullNamePairs.Where(fnp => !string.IsNullOrEmpty(fnp.Value)))
            {
                ssGroupsOfDiv.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
            }

            return View(new GridModel(ssGroupsOfDiv));
        }
    }
}
