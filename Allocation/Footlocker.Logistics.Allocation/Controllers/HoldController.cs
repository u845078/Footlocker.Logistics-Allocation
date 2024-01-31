using System;
using System.Collections.Generic;
using System.Linq;
using Footlocker.Logistics.Allocation.Spreadsheets;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using Telerik.Web.Mvc;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.DAO;
using Aspose.Cells;
using System.Data;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Director of Allocation,Admin,Support,Space Planning")]
    public class HoldController : AppController
    {
        #region Fields
        private readonly int _BIN_SIZE_VALUE_LENGTH = 3;
        readonly AllocationContext db = new AllocationContext();
        readonly ConfigService configService = new ConfigService();
        HoldService holdService;
        #endregion

        public ActionResult Index(string duration, string message)
        {
            if (string.IsNullOrEmpty(duration))            
                duration = "All";            

            List<Division> divs = currentUser.GetUserDivisions(AppName);
            List<Hold> list = db.Holds.Where(h => h.Duration == duration || duration == "All").ToList();
            list = (from a in list
                    join d in divs 
                    on a.Division equals d.DivCode                     
                    select a).ToList();

            if (list.Count > 0)
            {
                List<string> uniqueNames = (from l in list
                                            select l.CreatedBy).Distinct().ToList();
                Dictionary<string, string> fullNamePairs = new Dictionary<string, string>();

                foreach (var item in uniqueNames)
                {
                    fullNamePairs.Add(item, getFullUserNameFromDatabase(item.Replace('\\', '/')));
                }

                foreach (var item in fullNamePairs)
                {
                    list.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
                }
            }

            //TODO:  Do we want dept level security on holds???
            ViewData["message"] = message;
            return View(list);
        }

        public ActionResult IndexByProduct(string duration, string message)
        {           
            //this is more for developer debugging
            //clear the cache when they go back to the index.
            Session["rdqgrouplist"] = null;
            ViewData["message"] = message;
            return View();
        }

        public ActionResult IndexByStore(string duration, string message)
        {
            //this is more for developer debugging
            //clear the cache when they go back to the index.
            Session["rdqgrouplist"] = null;
            ViewData["message"] = message;
            return View();
        }

        [GridAction]
        public ActionResult _Index(string duration)
        {
            if (string.IsNullOrEmpty(duration))            
                duration = "All";
            
            List<Division> divs = currentUser.GetUserDivisions(AppName);
            List<Hold> list = db.Holds.Where(h => h.Duration == duration || duration == "All").ToList();
            list = (from a in list
                    join d in divs 
                    on a.Division equals d.DivCode                    
                    select a).ToList();

            if (list.Count > 0)
            {
                List<string> uniqueNames = (from l in list
                                            select l.CreatedBy).Distinct().ToList();
                Dictionary<string, string> fullNamePairs = new Dictionary<string, string>();

                foreach (var item in uniqueNames)
                {
                    fullNamePairs.Add(item, getFullUserNameFromDatabase(item.Replace('\\', '/')));
                }

                foreach (var item in fullNamePairs)
                {
                    list.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
                }
            }
            //TODO:  Do we want dept level security on holds???

            return View(new GridModel(list));
        }

        [GridAction]
        public ActionResult _IndexByProduct(string duration)
        {
            if (string.IsNullOrEmpty(duration))            
                duration = "All";
            
            List<Division> divs = currentUser.GetUserDivisions(AppName);
            List<Hold> list = db.Holds.Where(h => h.Duration == duration || duration == "All").ToList();

            list = (from a in list
                    join d in divs on a.Division equals d.DivCode            
                    select a).ToList();

            if (list.Count > 0)
            {
                List<string> uniqueNames = (from l in list
                                            select l.CreatedBy).Distinct().ToList();
                Dictionary<string, string> fullNamePairs = new Dictionary<string, string>();

                foreach (var item in uniqueNames)
                {
                    fullNamePairs.Add(item, getFullUserNameFromDatabase(item.Replace('\\', '/')));
                }

                foreach (var item in fullNamePairs)
                {
                    list.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
                }
            }

            //TODO:  Do we want dept level security on holds???

            List<HoldByProductModel> finalList = list.GroupBy(x => new { x.Division, x.Level, x.Value, x.HoldType })
                .Select(y => new HoldByProductModel(y.Key.Division, y.Key.Level, y.Key.Value, y.Key.HoldType)).ToList();
            return View(new GridModel(finalList));
        }

        [GridAction]
        public ActionResult _IndexByStore(string duration)
        {
            if (string.IsNullOrEmpty(duration))            
                duration = "All";
            
            List<Division> divs = currentUser.GetUserDivisions(AppName);
            List<Hold> list = db.Holds.Where(h => h.Duration == duration || duration == "All").ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    select a).ToList();

            //TODO:  Do we want dept level security on holds???

            List<HoldByStoreModel> finalList = list.GroupBy(x => new { x.Division, x.Store, x.HoldType })
                .Select(y => new HoldByStoreModel(y.Key.Division, y.Key.Store, y.Key.HoldType)).ToList();
            return View(new GridModel(finalList));
        }


        [GridAction]
        public ActionResult _HoldDetails(string div, string level, string value, string holdType, string duration)
        {
            if (string.IsNullOrEmpty(duration))            
                duration = "All";

            short holdTypeCode;
            if (holdType == "Cancel Inventory")
                holdTypeCode = 0;
            else
                holdTypeCode = 1;

            List<Division> divs = currentUser.GetUserDivisions(AppName);
            List<Hold> list = db.Holds.Where(h => (h.Duration == duration || duration == "All") &&
                                                  h.Division == div &&
                                                  h.Level == level &&
                                                  h.Value == value &&
                                                  h.ReserveInventory == holdTypeCode).ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    select a).ToList();
            //TODO:  Do we want dept level security on holds???

            if (list.Count > 0)
            {
                List<string> uniqueNames = (from l in list
                                            select l.CreatedBy).Distinct().ToList();
                Dictionary<string, string> fullNamePairs = new Dictionary<string, string>();

                foreach (var item in uniqueNames)
                {
                    fullNamePairs.Add(item, getFullUserNameFromDatabase(item.Replace('\\', '/')));
                }

                foreach (var item in fullNamePairs)
                {
                    list.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
                }
            }

            return View(new GridModel(list));
        }

        [GridAction]
        public ActionResult _HoldStoreDetails(string div, string store, string holdType, string duration)
        {
            if (string.IsNullOrEmpty(duration))            
                duration = "All";

            short holdTypeCode;
            if (holdType == "Cancel Inventory")
                holdTypeCode = 0;
            else
                holdTypeCode = 1;

            List<Division> divs = currentUser.GetUserDivisions(AppName);
            List<Hold> list = db.Holds.Where(h => (h.Duration == duration || duration == "All") && 
                                                  h.Division == div &&
                                                  h.ReserveInventory == holdTypeCode &&
                                                  (h.Store == store || string.IsNullOrEmpty(h.Store))).ToList();
            list = (from a in list
                    join d in divs 
                    on a.Division equals d.DivCode
                    select a).ToList();

            if (list.Count > 0)
            {
                List<string> uniqueNames = (from l in list
                                            select l.CreatedBy).Distinct().ToList();
                Dictionary<string, string> fullNamePairs = new Dictionary<string, string>();

                foreach (var item in uniqueNames)
                {
                    fullNamePairs.Add(item, getFullUserNameFromDatabase(item.Replace('\\', '/')));
                }

                foreach (var item in fullNamePairs)
                {
                    list.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
                }
            }

            return View(new GridModel(list));
        }

        [GridAction]
        public ActionResult ExportGrid(GridCommand settings, string duration)
        {
            HoldsExport holdsExport = new HoldsExport(appConfig);
            holdsExport.WriteData(settings, duration);
            holdsExport.excelDocument.Save(System.Web.HttpContext.Current.Response, "Holds.xlsx", ContentDisposition.Attachment, holdsExport.SaveOptions);
            return RedirectToAction("Index");
        }

        public ActionResult Create()
        {
            HoldModel model = new HoldModel()
            {
                Hold = new Hold() 
                { 
                    StartDate = DateTime.Now.AddDays(1) 
                },
                Divisions = currentUser.GetUserDivisions(AppName),
                ShowStoreSelector = "no",
                RuleModel = new RuleModel()
            };

            ViewData["ruleSetID"] = model.RuleSetID;
            ViewData["ruleType"] = "hold";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HoldModel model)
        {
            ViewData["ruleSetID"] = model.RuleSetID;
            ViewData["ruleType"] = "hold";

            holdService = new HoldService(currentUser, configService)
            {
                Hold = model.Hold
            };                        
            
            if (model.ShowStoreSelector == "yes")
            {
                if (model.RuleSetID < 1)
                {
                    //get a new ruleset
                    RuleSet rs = new RuleSet()
                    {
                        Type = "hold",
                        CreateDate = DateTime.Now,
                        CreatedBy = currentUser.NetworkID,
                        Division = model.Hold.Division
                    };

                    db.RuleSets.Add(rs);
                    db.SaveChanges();

                    model.RuleSetID = rs.RuleSetID;
                }

                ViewData["ruleSetID"] = model.RuleSetID;
                model.Divisions = currentUser.GetUserDivisions(AppName);
                return View(model);
            }
            else
            {
                model.Hold.CreateDate = DateTime.Now;
                model.Hold.CreatedBy = currentUser.NetworkID;

                if (currentUser.GetUserDivisions(AppName).Exists(d => d.DivCode == model.Hold.Division))
                {
                    if (model.Hold.Level == "Sku" && model.Hold.Division != model.Hold.Value.Substring(0, 2))
                    {
                        ViewData["message"] = "Invalid Sku, division does not match selection.";
                        model.Divisions = currentUser.GetUserDivisions(AppName);
                        return View(model);
                    }
                    else
                    {
                        if (model.RuleSetID == 0)
                        {
                            string validationMessage = holdService.ValidateHold(false, false);

                            if (!string.IsNullOrEmpty(validationMessage))
                            {
                                ViewData["message"] = validationMessage;
                                model.Divisions = currentUser.GetUserDivisions(AppName);
                                return View(model);
                            }
                            else
                            {
                                db.Holds.Add(model.Hold);
                                db.SaveChanges();
                                ApplyHoldsToExistingWebPicks(model.Hold);
                                return RedirectToAction("Index", new { duration = model.Hold.Duration });
                            }
                        }
                        else
                        {
                            //create hold for each store
                            RuleDAO dao = new RuleDAO();
                            Hold h;
                            foreach (StoreLookup s in dao.GetRuleSelectedStoresInRuleSet(model.RuleSetID))
                            {
                                h = new Hold()
                                {
                                    Store = s.Store,
                                    Division = s.Division,
                                    Comments = model.Hold.Comments,
                                    Duration = model.Hold.Duration,
                                    EndDate = model.Hold.EndDate,
                                    HoldType = model.Hold.HoldType,
                                    Level = model.Hold.Level,
                                    ReserveInventory = model.Hold.ReserveInventory,
                                    StartDate = model.Hold.StartDate,
                                    Value = model.Hold.Value,
                                    CreateDate = model.Hold.CreateDate,
                                    CreatedBy = model.Hold.CreatedBy
                                };

                                holdService.Hold = h;

                                string validationMessage = holdService.ValidateHold(false, false);

                                if (!string.IsNullOrEmpty(validationMessage))
                                {
                                    ViewData["message"] = validationMessage;
                                    model.Divisions = currentUser.GetUserDivisions(AppName);
                                    return View(model);
                                }
                                else
                                {
                                    db.Holds.Add(h);
                                    db.SaveChanges();
                                    ApplyHoldsToExistingWebPicks(h);
                                }
                            }

                            return RedirectToAction("Index", new { duration = model.Hold.Duration });
                        }
                    }
                }
                else
                {
                    ViewData["message"] = "You are not authorized to create holds for this division.";
                    model.Divisions = currentUser.GetUserDivisions(AppName);
                    return View(model);
                }
            }
        }

        private void ApplyHoldsToExistingWebPicks(Hold h)
        {
            RDQDAO rdqDAO = new RDQDAO();
            List<RDQ> list = rdqDAO.GetRDQsForHold(h.ID);
            bool needsave = false;
            foreach (RDQ rdq in list)
            {
                if (!h.ReserveInventoryBool)
                {
                    needsave = true;
                    rdq.Status = "REJECTED";
                    rdq.RDQRejectedReasonCode = 13;
                    db.Entry(rdq).State = System.Data.EntityState.Modified;
                }
                else if (rdq.Status.Contains("WEB PICK"))
                {
                    needsave = true;
                    rdq.Status = "HOLD-NEW";
                    db.Entry(rdq).State = System.Data.EntityState.Modified;
                }

                rdq.LastModifiedUser = currentUser.NetworkID;
            }
            if (needsave)
                db.SaveChanges(currentUser.NetworkID);
        }

        public ActionResult Edit(int ID)
        {
            HoldModel model = new HoldModel()
            {
                Hold = db.Holds.Where(h => h.ID == ID).FirstOrDefault(),
                Divisions = currentUser.GetUserDivisions(AppName)
            };

            model.OriginalStartDate = model.Hold.StartDate;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HoldModel model)
        {
            holdService = new HoldService(currentUser, configService)
            {
                Hold = model.Hold
            };
            
            string validationMessage = holdService.ValidateHold(true, false);
            
            if (model.OriginalStartDate > model.Hold.StartDate)            
                validationMessage = "Start date must be after original start date of hold";
            
            if (!string.IsNullOrEmpty(validationMessage))
            {
                ViewData["message"] = validationMessage;
                model.Divisions = currentUser.GetUserDivisions(AppName);
                return View(model);
            }
            else
            {
                model.Hold.CreateDate = DateTime.Now;
                model.Hold.CreatedBy = currentUser.NetworkID;

                db.Entry(model.Hold).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { duration = model.Hold.Duration });
            }
        }

        public ActionResult MassEdit(string div, string level, string value, string holdType)
        {
            HoldModel model = new HoldModel();
            if (holdType.Contains("Reserve"))
            {
                model.Hold = db.Holds.Where(h => h.Division == div && 
                                                 h.Level == level && 
                                                 h.Value == value && 
                                                 h.ReserveInventory == 1).First();
            }
            else
            {
                model.Hold = db.Holds.Where(h => h.Division == div && 
                                                 h.Level == level && 
                                                 h.Value == value && 
                                                 h.ReserveInventory == 0).First();
            }
            model.Hold.Comments = "";
            model.Divisions = currentUser.GetUserDivisions(AppName);
            model.OriginalStartDate = model.Hold.StartDate;

            return View(model);
        }

        public ActionResult MassEditStore(string div, string store, string holdType)
        {
            HoldModel model = new HoldModel();

            if (holdType.Contains("Reserve"))            
                model.Hold = db.Holds.Where(h => h.Division == div && h.Store == store && h.ReserveInventory == 1).First();            
            else            
                model.Hold = db.Holds.Where(h => h.Division == div && h.Store == store && h.ReserveInventory == 0).First();
            
            model.Hold.Comments = "";
            model.Divisions = currentUser.GetUserDivisions(AppName);
            model.OriginalStartDate = model.Hold.StartDate;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MassEdit(HoldModel model)
        {
            List<Hold> holds;
            if (model.Hold.HoldType.Contains("Reserve"))
            {
                holds = db.Holds.Where(h => h.Division == model.Hold.Division && 
                                            h.Level == model.Hold.Level && 
                                            h.Value == model.Hold.Value && 
                                            h.ReserveInventory == 1).ToList();
            }
            else
            {
                holds = db.Holds.Where(h => h.Division == model.Hold.Division && 
                                            h.Level == model.Hold.Level && 
                                            h.Value == model.Hold.Value && 
                                            h.ReserveInventory == 0).ToList();
            }

            holdService = new HoldService(currentUser, configService);
            string validationMessage = "";
            List<Hold> updatedList = new List<Hold>();
            DateTime startdate;
            foreach (Hold h in holds)
            {
                startdate = h.StartDate;

                h.StartDate = model.Hold.StartDate;
                h.EndDate = model.Hold.EndDate;
                if (model.Hold.Comments != "")                
                    h.Comments = model.Hold.Comments;                

                holdService.Hold = h;                
                
                string tempvalidationMessage = holdService.ValidateHold(true, false);
                
                if (startdate > model.Hold.StartDate)                
                    tempvalidationMessage = string.Format("Start date must be after original start date of hold for store {0}<br>", h.Store);                

                if (tempvalidationMessage == "")                
                    updatedList.Add(h);                

                validationMessage += tempvalidationMessage;
            }

            if (validationMessage != "")
            {
                ViewData["message"] = validationMessage;
                model.Divisions = currentUser.GetUserDivisions(AppName);
                return View(model);
            }
            else
            {
                foreach (Hold uh in updatedList)
                {
                    uh.CreateDate = DateTime.Now;
                    uh.CreatedBy = currentUser.NetworkID;
                    db.Entry(uh).State = System.Data.EntityState.Modified;
                }
                db.SaveChanges();
                ViewData["message"] = "Successfully updated group of holds";
                return RedirectToAction("IndexByProduct");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MassEditStore(HoldModel model)
        {
            List<Hold> holds;
            if (model.Hold.HoldType.Contains("Reserve"))
            {
                holds = db.Holds.Where(h => h.Division == model.Hold.Division && 
                                            h.Store == model.Hold.Store && 
                                            h.ReserveInventory == 1).ToList();
            }
            else
            {
                holds = db.Holds.Where(h => h.Division == model.Hold.Division && 
                                            h.Store == model.Hold.Store && 
                                            h.ReserveInventory == 0).ToList();
            }

            holdService = new HoldService(currentUser, configService);
            string validationMessage = "";
            List<Hold> updatedList = new List<Hold>();
            DateTime startdate;
            foreach (Hold h in holds)
            {
                startdate = h.StartDate;

                h.StartDate = model.Hold.StartDate;
                h.EndDate = model.Hold.EndDate;
                if (model.Hold.Comments != "")                
                    h.Comments = model.Hold.Comments;                

                holdService.Hold = h;
                string tempvalidationMessage = holdService.ValidateHold(true, false);
                
                if (startdate > model.Hold.StartDate)                
                    tempvalidationMessage = "Start date must be after original start date of hold for store " + h.Store + "<br>";                

                if (tempvalidationMessage == "")                
                    updatedList.Add(h);                

                validationMessage += tempvalidationMessage;
            }
            if (validationMessage != "")
            {
                ViewData["message"] = validationMessage;
                model.Divisions = currentUser.GetUserDivisions(AppName);
                return View(model);
            }
            else
            {
                foreach (Hold uh in updatedList)
                {
                    uh.CreateDate = DateTime.Now;
                    uh.CreatedBy = currentUser.NetworkID;
                    db.Entry(uh).State = EntityState.Modified;
                }
                db.SaveChanges();
                ViewData["message"] = "Successfully updated group of holds";
                return RedirectToAction("IndexByStore");
            }
        }

        public ActionResult ReleaseRDQs(int ID)
        {
            ViewData["holdID"] = ID;
            Hold model = db.Holds.Where(h => h.ID == ID).First();
            DeleteHoldModel dh = new DeleteHoldModel()
            {
                Hold = model
            };
            
            //probably want stored proc so we can join for category
            dh.CurrentRDQs = GetRDQsForSession(ID);

            HoldRelease hr = new HoldRelease();
            dh.HoldReleases = new List<HoldRelease>();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);

            return View(dh);
        }

        public ActionResult MassReleaseRDQs(string div, string level, string value, string holdType)
        {
            List<Hold> holds = new List<Hold>();
            if (holdType.Contains("Reserve"))
            {
                holds = (from a in db.Holds 
                         where a.Division == div && 
                               a.Level == level && 
                               a.Value == value && 
                               a.ReserveInventory == 1
                         select a).ToList();
            }
            else
            {
                holds = (from a in db.Holds 
                         where a.Division == div && 
                               a.Level == level && 
                               a.Value == value && 
                               a.ReserveInventory == 0
                         select a).ToList();
            }
            DeleteHoldModel dh = new DeleteHoldModel();
            dh.Hold = holds.First();
            RDQDAO dao = new RDQDAO();
            dh.CurrentRDQs = GetRDQsForSession(div, level, value);
            dh.NumberOfHolds = holds.Count();
            ViewData["holdID"] = dh.Hold.ID;

            HoldRelease hr = new HoldRelease();
            dh.HoldReleases = new List<HoldRelease>();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);

            return View(dh);
        }

        public ActionResult MassReleaseRDQsStore(string div, string store, string holdType)
        {
            List<Hold> holds = new List<Hold>();
            if (holdType.Contains("Reserve"))
            {
                holds = (from a in db.Holds 
                         where a.Division == div && 
                               a.Store == store && 
                               a.ReserveInventory == 1
                         select a).ToList();
            }
            else
            {
                holds = (from a in db.Holds 
                         where a.Division == div && 
                               a.Store == store && 
                               a.ReserveInventory == 0
                         select a).ToList();
            }
            DeleteHoldModel dh = new DeleteHoldModel();
            dh.Hold = holds.First();
            RDQDAO dao = new RDQDAO();
            dh.CurrentRDQs = GetRDQsForSession(div, store);
            dh.NumberOfHolds = holds.Count();
            ViewData["holdID"] = dh.Hold.ID;

            HoldRelease hr = new HoldRelease();
            dh.HoldReleases = new List<HoldRelease>();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);
            hr = new HoldRelease();
            dh.HoldReleases.Add(hr);

            return View(dh);
        }

        public ActionResult ConfirmDelete(int ID)
        {
            Hold hold = (from a in db.Holds where a.ID == ID select a).First();
            if (hold.ReserveInventoryBool)
            {
                RDQDAO dao = new RDQDAO();
                if (dao.GetUniqueRDQsForHold(hold.ID).Count > 0)
                {
                    return RedirectToAction("Index", new { duration = hold.Duration, message = "You must release all RDQs before you can delete this hold." });
                }
            }

            db.Holds.Remove(hold);
            db.SaveChanges();
            return RedirectToAction("Index", new { duration = hold.Duration });
        }

        public ActionResult MassDelete(string div, string level, string value, string holdType)
        {
            List<Hold> holds = new List<Hold>();
            if (holdType.Contains("Reserve"))
            {
                holds = (from a in db.Holds 
                         where a.Division == div && 
                               a.Level == level && 
                               a.Value == value && 
                               a.ReserveInventory == 1
                         select a).ToList();
            }
            else
            {
                holds = (from a in db.Holds 
                         where a.Division == div && 
                               a.Level == level && 
                               a.Value == value && 
                               a.ReserveInventory == 0
                         select a).ToList();
            }
            foreach (Hold hold in holds)
            {
                if (hold.ReserveInventoryBool)
                {
                    RDQDAO dao = new RDQDAO();
                    if (dao.GetUniqueRDQsForHold(hold.ID).Count > 0)
                    {
                        return RedirectToAction("IndexByProduct", new { duration = hold.Duration, message = "You must release all RDQs before you can delete this group of holds." });
                    }
                }
            }
            foreach (Hold hold in holds)
            {
                db.Holds.Remove(hold);
            }
            db.SaveChanges();
            return RedirectToAction("IndexByProduct");
        }

        public ActionResult MassDeleteStore(string div, string store, string holdType)
        {
            List<Hold> holds = new List<Hold>();
            if (holdType.Contains("Reserve"))
            {
                holds = (from a in db.Holds where ((a.Division == div) && (a.Store == store) && (a.ReserveInventory == 1)) select a).ToList();
            }
            else
            {
                holds = (from a in db.Holds where ((a.Division == div) && (a.Store == store) && (a.ReserveInventory == 0)) select a).ToList();
            }
            foreach (Hold hold in holds)
            {
                if (hold.ReserveInventoryBool)
                {
                    RDQDAO dao = new RDQDAO();
                    if (dao.GetUniqueRDQsForHold(hold.ID).Count > 0)
                    {
                        return RedirectToAction("IndexByStore", new { duration = hold.Duration, message = "You must release all RDQs before you can delete this group of holds." });
                    }
                }
            }
            foreach (Hold hold in holds)
            {
                db.Holds.Remove(hold);
            }
            db.SaveChanges();
            return RedirectToAction("IndexByStore");
        }


        [HttpPost]
        public ActionResult MassReleaseAllRDQsToWarehouse()
        {
            using (db)
            {
                // Get all rdqs for specified hold
                // TODO: Should probably relate RDQs to the Hold in EF mapping, and load all via context, rather than sproc....
                var rdqs = GetRDQsInSession();

                foreach (RDQ rdq in rdqs)
                {
                    // Load rdq (so able to be removed from context)
                    var contextRDQ = db.RDQs.Find(rdq.ID);

                    //delete rdq
                    db.RDQs.Remove(contextRDQ);
                }

                // Persist changes
                db.SaveChanges(currentUser.NetworkID);
            }

            Session["rdqgrouplist"] = null;
            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult ReleaseAllRDQsToWarehouse(int ID)
        {
            using (db)
            {
                // Get all rdqs for specified hold
                // TODO: Should probably relate RDQs to the Hold in EF mapping, and load all via context, rather than sproc....
                var dao = new RDQDAO();
                var rdqs = dao.GetUniqueRDQsForHold(ID);

                foreach (RDQ rdq in rdqs)
                {
                    // Load rdq (so able to be removed from context)
                    var contextRDQ = db.RDQs.Find(rdq.ID);

                    //delete rdq
                    db.RDQs.Remove(contextRDQ);
                }

                // Persist changes
                db.SaveChanges(currentUser.NetworkID);
            }

            Session["holdrdq"] = -1;
            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult ReleaseAllRDQs(int ID)
        {
            using (db)
            {
                // Get all rdqs for specified hold
                // TODO: Should probably relate RDQs to the Hold in EF mapping, and load all via context, rather than sproc....
                var dao = new RDQDAO();
                var rdqs = dao.GetUniqueRDQsForHold(ID);

                foreach (RDQ rdq in rdqs)
                {
                    // Load rdq (so we can update the status)
                    var contextRDQ = db.RDQs.Find(rdq.ID);
                    contextRDQ.Status = "HOLD-REL";

                    if (!string.IsNullOrEmpty(rdq.PO) && rdq.Size.Length == 5)                    
                        rdq.DestinationType = "CROSSDOCK";                    
                    else                    
                        rdq.DestinationType = "WAREHOUSE";
                    
                    db.Entry(contextRDQ).State = System.Data.EntityState.Modified;
                }

                // Persist changes
                db.SaveChanges(currentUser.NetworkID);
            }

            Session["holdrdq"] = -1;
            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult MassReleaseAllRDQs()
        {
            using (db)
            {
                // Get all rdqs for specified hold
                // TODO: Should probably relate RDQs to the Hold in EF mapping, and load all via context, rather than sproc....
                var rdqs = GetRDQsInSession();

                foreach (RDQ rdq in rdqs)
                {
                    // Load rdq (so able to be removed from context)
                    var contextRDQ = db.RDQs.Find(rdq.ID);

                    contextRDQ.Status = "HOLD-REL";

                    if (!string.IsNullOrEmpty(rdq.PO) && rdq.Size.Length == 5)                    
                        rdq.DestinationType = "CROSSDOCK";                    
                    else                    
                        rdq.DestinationType = "WAREHOUSE";
                    
                    db.Entry(contextRDQ).State = System.Data.EntityState.Modified;
                }

                // Persist changes
                db.SaveChanges(currentUser.NetworkID);
            }

            Session["rdqgrouplist"] = null;
            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult ReleaseRDQGroupToWarehouse(RDQGroup rdqGroup, int holdID)
        {
            // Get all RDQs of specified SKU for specified hold
            var dao = new RDQDAO();
            var holdRDQs = GetRDQsForSession(holdID);
            var groupRDQs = holdRDQs.Where(rdq => rdq.Division == rdqGroup.Division && 
                                           rdq.Store == rdqGroup.Store && 
                                           rdq.WarehouseName == rdqGroup.WarehouseName && 
                                           rdq.Category == rdqGroup.Category && 
                                           rdq.Sku == rdqGroup.Sku).ToList();

            //performance was really bad via entity framework, we'll just run a quick stored proc and update records in memory
            dao.DeleteRDQs(groupRDQs, currentUser.NetworkID);

            groupRDQs.ForEach(rdq => holdRDQs.Remove(rdq));

            Session["holdrdqlist"] = holdRDQs;
            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult ReleaseRDQGroup(RDQGroup rdqGroup, int holdID)
        {
            // Get all RDQs of specified SKU for specified hold
            var holdRDQs = GetRDQsForSession(holdID);
            var groupRDQs = holdRDQs.Where(rdq => rdq.Division == rdqGroup.Division && 
                                                  rdq.Store == rdqGroup.Store && 
                                                  rdq.WarehouseName == rdqGroup.WarehouseName && 
                                                  rdq.Category == rdqGroup.Category && 
                                                  rdq.Sku == rdqGroup.Sku).ToList();

            //performance was really bad via entity framework, we'll just run a quick stored proc and update records in memory
            RDQDAO dao = new RDQDAO();
            dao.ReleaseRDQs(groupRDQs, currentUser.NetworkID);

            groupRDQs.ForEach(rdq => holdRDQs.Remove(rdq));
            Session["holdrdqlist"] = holdRDQs;

            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult MassReleaseRDQGroupToWarehouse(RDQGroup rdqGroup)
        {
            // Get all RDQs of specified SKU for specified hold
            var dao = new RDQDAO();
            var holdRDQs = GetRDQsInSession();
            var groupRDQs = holdRDQs.Where(rdq => rdq.Division == rdqGroup.Division && 
                                                  rdq.Store == rdqGroup.Store && 
                                                  rdq.WarehouseName == rdqGroup.WarehouseName && 
                                                  rdq.Category == rdqGroup.Category && 
                                                  rdq.Sku == rdqGroup.Sku).ToList();

            //performance was really bad via entity framework, we'll just run a quick stored proc and update records in memory
            dao.DeleteRDQs(groupRDQs, currentUser.NetworkID);

            groupRDQs.ForEach(rdq => holdRDQs.Remove(rdq));

            Session["rdqgrouplist"] = holdRDQs;
            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult MassReleaseRDQGroup(RDQGroup rdqGroup)
        {
            // Get all RDQs of specified SKU for specified hold
            var holdRDQs = GetRDQsInSession();
            var groupRDQs = holdRDQs.Where(rdq => rdq.Division == rdqGroup.Division && 
                                                  rdq.Store == rdqGroup.Store && 
                                                  rdq.WarehouseName == rdqGroup.WarehouseName && 
                                                  rdq.Category == rdqGroup.Category && 
                                                  rdq.Sku == rdqGroup.Sku).ToList();

            //performance was really bad via entity framework, we'll just run a quick stored proc and update records in memory
            RDQDAO dao = new RDQDAO();
            dao.ReleaseRDQs(groupRDQs, currentUser.NetworkID);

            groupRDQs.ForEach(rdq => holdRDQs.Remove(rdq));
            Session["rdqgrouplist"] = holdRDQs;

            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpGet]
        public ActionResult GetRDQTotalQtyForHold(int holdID)
        {
            // Get sum of qtys of rdqs for specified hold
            var dao = new RDQDAO();
            var rdqTotalQty = dao.GetRDQsForHold(holdID).ToList().Sum(r => r.Qty);

            // Return JSON representing Success
            return new JsonResult() {
                Data = new JsonResultData(ActionResultCode.Success) { Data = rdqTotalQty },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        public ActionResult GetMassRDQTotalQtyForHolds()
        {
            // Get sum of qtys of rdqs for specified hold
            var dao = new RDQDAO();
            var rdqTotalQty = GetRDQsInSession().ToList().Sum(r => r.Qty);

            // Return JSON representing Success
            return new JsonResult() {
                Data = new JsonResultData(ActionResultCode.Success) { Data = rdqTotalQty },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        private List<RDQ> GetRDQsInSession()
        {
            if (Session["rdqgrouplist"] != null)            
                return (List<RDQ>)Session["rdqgrouplist"];            
            else            
                return new List<RDQ>();            
        }

        private List<RDQ> GetRDQsForSession(string div, string level, string value)
        {
            List<RDQ> model;
            if ((Session["rdqgrouplist"] != null) &&
                ((string)Session["holdgrouprdq"] == string.Format("{0}|{1}|{2}", div, level, value)))
            {
                model = (List<RDQ>)Session["rdqgrouplist"];
            }
            else
            {
                RDQDAO dao = new RDQDAO();

                model = dao.GetRDQsForHolds(div, level, value);

                List<RDQ> updateList = new List<RDQ>();
                List<string> caselots = new List<string>();
                foreach (RDQ r in model)
                {
                    if (r.Size.Length > 3)
                    {
                        updateList.Add(r);
                        caselots.Add(r.Size);
                    }
                    else                    
                        r.UnitQty = r.Qty;                    
                }

                List<ItemPack> qtyPerCase = db.ItemPacks.Where(p => caselots.Contains(p.Name)).ToList();
                foreach (RDQ r in updateList)
                {
                    try
                    {
                        r.UnitQty = (from a in qtyPerCase 
                                     where a.Name == r.Size 
                                     select a.TotalQty).First() * r.Qty;
                    }
                    catch
                    {
                        //    //don't know per qty, so we'll just leave blank
                    }
                }

                Session["holdgrouprdq"] = div + "|" + level + "|" + value;
                Session["rdqgrouplist"] = model;
            }

            return model;
        }

        private List<RDQ> GetRDQsForSession(string div, string store)
        {
            List<RDQ> model;
            if ((Session["rdqgrouplist"] != null) && ((string)Session["holdgrouprdq"] == string.Format("{0}|{1}", div, store)))            
                model = (List<RDQ>)Session["rdqgrouplist"];            
            else
            {
                RDQDAO dao = new RDQDAO();

                model = dao.GetRDQsForHolds(div, store);

                List<RDQ> updateList = new List<RDQ>();
                List<string> caselots = new List<string>();
                foreach (RDQ r in model)
                {
                    if (r.Size.Length > 3)
                    {
                        updateList.Add(r);
                        caselots.Add(r.Size);
                    }
                    else                    
                        r.UnitQty = r.Qty;                    
                }

                List<ItemPack> qtyPerCase = db.ItemPacks.Where(p => caselots.Contains(p.Name)).ToList();
                foreach (RDQ r in updateList)
                {
                    try
                    {
                        r.UnitQty = (from a in qtyPerCase where a.Name == r.Size select a.TotalQty).First() * r.Qty;
                    }
                    catch
                    {
                        //    //don't know per qty, so we'll just leave blank
                    }
                }

                Session["holdgrouprdq"] = div + "|" + store;
                Session["rdqgrouplist"] = model;
            }

            return model;
        }

        private List<RDQ> GetRDQsForSession(long holdID)
        {
            List<RDQ> model;
            if ((Session["holdrdq"] != null) && ((long)Session["holdrdq"] == holdID))            
                model = (List<RDQ>)Session["holdrdqlist"];            
            else
            {
                RDQDAO dao = new RDQDAO();

                model = dao.GetRDQsForHold(holdID);

                List<RDQ> updateList = new List<RDQ>();
                List<string> caselots = new List<string>();
                foreach (RDQ r in model)
                {
                    if (r.Size.Length > 3)
                    {
                        updateList.Add(r);
                        caselots.Add(r.Size);
                    }
                    else                    
                        r.UnitQty = r.Qty;                    
                }

                List<ItemPack> qtyPerCase = db.ItemPacks.Where(p => caselots.Contains(p.Name)).ToList();
                foreach (RDQ r in updateList)
                {
                    try
                    {
                        r.UnitQty = (from a in qtyPerCase 
                                     where a.Name == r.Size 
                                     select a.TotalQty).First() * r.Qty;
                    }
                    catch
                    {
                        //    //don't know per qty, so we'll just leave blank
                    }
                }

                Session["holdrdq"] = holdID;
                Session["holdrdqlist"] = model;
            }
            return model;
        }

        [GridAction]
        public ActionResult _RDQs(int holdID)
        {
            ViewData["holdID"] = holdID;
            RDQDAO dao = new RDQDAO();

            List<RDQ> list = GetRDQsForSession(holdID);

            // Hit db for RDQs for specified hold, aggregate in memory to level (defined by users)
            var rdqGroups =
                        from rdq in list
                        group rdq by new
                        {
                            Division = rdq.Division,
                            Store = rdq.Store,
                            WarehouseName = rdq.WarehouseName,
                            Category = rdq.Category,
                            ItemID = rdq.ItemID,
                            Sku = rdq.Sku
                        } into g
                        select new RDQGroup()
                        {
                            Division = g.Key.Division,
                            Store = g.Key.Store,
                            WarehouseName = g.Key.WarehouseName,
                            Category = g.Key.Category,
                            ItemID = Convert.ToInt64(g.Key.ItemID),
                            Sku = g.Key.Sku,
                            IsBin = g.Where(r => r.Size.Length > _BIN_SIZE_VALUE_LENGTH).Any() ? false : true,
                            Qty = g.Sum(r => r.Qty),
                            UnitQty = g.Sum(r => r.UnitQty)
                        };

            return PartialView(new GridModel(
                rdqGroups.OrderBy(g => g.Division)
                    .ThenBy(g => g.Store)
                    .ThenBy(g => g.WarehouseName)
                    .ThenBy(g => g.Category)
                    .ThenBy(g => g.Sku)));
        }

        [GridAction]
        public ActionResult _RDQsMass()
        {

            List<RDQ> list = GetRDQsInSession();

            // Hit db for RDQs for specified hold, aggregate in memory to level (defined by users)
            var rdqGroups =
                        from rdq in list
                        group rdq by new
                        {
                            Division = rdq.Division,
                            Store = rdq.Store,
                            WarehouseName = rdq.WarehouseName,
                            Category = rdq.Category,
                            ItemID = rdq.ItemID,
                            Sku = rdq.Sku
                        } into g
                        select new RDQGroup()
                        {
                            Division = g.Key.Division,
                            Store = g.Key.Store,
                            WarehouseName = g.Key.WarehouseName,
                            Category = g.Key.Category,
                            ItemID = Convert.ToInt64(g.Key.ItemID),
                            Sku = g.Key.Sku,
                            IsBin = g.Where(r => r.Size.Length > _BIN_SIZE_VALUE_LENGTH).Any() ? false : true,
                            Qty = g.Sum(r => r.Qty),
                            UnitQty = g.Sum(r => r.UnitQty)
                        };

            return PartialView(new GridModel(
                rdqGroups.OrderBy(g => g.Division)
                    .ThenBy(g => g.Store)
                    .ThenBy(g => g.WarehouseName)
                    .ThenBy(g => g.Category)
                    .ThenBy(g => g.Sku)));
        }

        [HttpPost]
        public ActionResult DeleteReleaseTo(DeleteHoldModel model)
        {
            //TODO:  Create E-Pick to each store

            db.Holds.Remove(model.Hold);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        #region Holds Upload
        public ActionResult Upload()
        {
            return View();
        }

        public ActionResult ExcelHoldsUploadTemplate()
        {
            HoldsUploadSpreadsheet holdsUploadSpreadsheet = new HoldsUploadSpreadsheet(appConfig, configService, holdService);
            Workbook excelDocument;

            excelDocument = holdsUploadSpreadsheet.GetTemplate();

            excelDocument.Save(System.Web.HttpContext.Current.Response, "HoldsUpload.xlsx", ContentDisposition.Attachment, holdsUploadSpreadsheet.SaveOptions);
            return View("HoldsUpload");
        }

        public ActionResult UploadHolds(IEnumerable<HttpPostedFileBase> attachments)
        {
            holdService = new HoldService(currentUser, configService);

            HoldsUploadSpreadsheet holdsUploadSpreadsheet = new HoldsUploadSpreadsheet(appConfig, configService, holdService);

            string message;
            int successCount = 0;

            foreach (HttpPostedFileBase file in attachments)
            {
                holdsUploadSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(holdsUploadSpreadsheet.message))
                    return Content(holdsUploadSpreadsheet.message);
                else
                {
                    if (holdsUploadSpreadsheet.errorList.Count() > 0)
                    {
                        Session["errorList"] = holdsUploadSpreadsheet.errorList;

                        message = string.Format("{0} successfully uploaded, {1} Errors", holdsUploadSpreadsheet.validHolds.Count.ToString(),
                            holdsUploadSpreadsheet.errorList.Count.ToString());

                        return Content(message);
                    }
                }

                successCount = holdsUploadSpreadsheet.validHolds.Count();
            }

            return Json(new { message = string.Format("{0} Hold(s) Uploaded", successCount) }, "application/json");
        }

        public ActionResult DownloadErrors()
        {
            List<Hold> errors = (List<Hold>)Session["errorList"];
            Workbook excelDocument;
            HoldsUploadSpreadsheet holdsUploadSpreadsheet = new HoldsUploadSpreadsheet(appConfig, configService, holdService);

            if (errors != null)
            {
                excelDocument = holdsUploadSpreadsheet.GetErrors(errors);
                excelDocument.Save(System.Web.HttpContext.Current.Response, "HoldsUploadErrors.xlsx", ContentDisposition.Attachment, holdsUploadSpreadsheet.SaveOptions);
            }
            return View();
        }
        #endregion

        #region Holds Updates Upload
        public ActionResult UploadHoldsUpdates()
        {
            return View();
        }

        public ActionResult ExcelUpdateTemplate()
        {
            HoldsUpdateSpreadsheet holdsUpdateSpreadsheet = new HoldsUpdateSpreadsheet(appConfig, configService);
            holdsUpdateSpreadsheet.GetTemplate().Save(System.Web.HttpContext.Current.Response, "HoldsUpdates.xlsx", ContentDisposition.Attachment, holdsUpdateSpreadsheet.SaveOptions);
            return View();
        }

        public ActionResult MassUpdateHolds(IEnumerable<HttpPostedFileBase> attachments)
        {
            HoldsUpdateSpreadsheet holdsUpdateSpreadsheet = new HoldsUpdateSpreadsheet(appConfig, configService);

            foreach (HttpPostedFileBase file in attachments)
            {
                holdsUpdateSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(holdsUpdateSpreadsheet.message))
                    return Content(holdsUpdateSpreadsheet.message);
                else
                {
                    if (holdsUpdateSpreadsheet.errorList.Count() > 0)
                    {
                        Session["errorList"] = holdsUpdateSpreadsheet.errorList;

                        string msg = string.Format("Successfully updated {0} rows, {1} errors.", holdsUpdateSpreadsheet.validHolds.Count, holdsUpdateSpreadsheet.errorList.Count);

                        return Content(msg);
                    }
                }
            }

            return Json(new { message = string.Format("{0} Hold(s) updated", holdsUpdateSpreadsheet.validHolds.Count) }, "application/json");
        }

        public ActionResult DownloadUpdateErrors()
        {
            List<HoldsUploadUpdateModel> errorList = (List<HoldsUploadUpdateModel>)Session["errorList"];
            HoldsUpdateSpreadsheet holdsUpdateSpreadsheet = new HoldsUpdateSpreadsheet(appConfig, configService);
            Workbook excelDocument = holdsUpdateSpreadsheet.GetErrors(errorList);

            excelDocument.Save(System.Web.HttpContext.Current.Response, "HoldsErrorList.xlsx", ContentDisposition.Attachment, holdsUpdateSpreadsheet.SaveOptions);

            return View();
        }
        #endregion
    }
}