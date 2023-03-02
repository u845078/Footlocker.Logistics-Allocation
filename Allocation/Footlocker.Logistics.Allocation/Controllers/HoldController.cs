using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using Telerik.Web.Mvc;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.DAO;
using System.IO;
using Aspose.Excel;
using System.Globalization;
using Aspose.Cells;
using System.Data;
using System.Web.ApplicationServices;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Director of Allocation,Admin,Support,Space Planning")]
    public class HoldController : AppController
    {
        #region Fields

        private readonly int _BIN_SIZE_VALUE_LENGTH = 3;
        AllocationContext db = new AllocationContext();
        ConfigService configService = new ConfigService();
        HoldService holdService;

        #endregion

        public ActionResult Index(string duration, string message)
        {
            if (string.IsNullOrEmpty(duration))
            {
                duration = "All";
            }

            List<Division> divs = currentUser.GetUserDivisions(AppName);
            List<Hold> list = db.Holds.ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode 
                    where a.Duration == duration || duration == "All" 
                    select a).ToList();

            //TODO:  Do we want dept level security on holds???
            ViewData["message"] = message;
            return View(list);
        }

        public ActionResult IndexByProduct(string duration, string message)
        {
            if (string.IsNullOrEmpty(duration))
            {
                duration = "All";
            }
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
            {
                duration = "All";
            }
            List<Division> divs = currentUser.GetUserDivisions(AppName);
            List<Hold> list = db.Holds.ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    where ((a.Duration == duration) || (duration == "All"))
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
            {
                duration = "All";
            }
            List<Division> divs = currentUser.GetUserDivisions(AppName);
            List<Hold> list = db.Holds.ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    where ((a.Duration == duration) || (duration == "All"))
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
            {
                duration = "All";
            }
            List<Division> divs = currentUser.GetUserDivisions(AppName);
            List<Hold> list = db.Holds.ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    where ((a.Duration == duration) || (duration == "All"))
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
            {
                duration = "All";
            }
            List<Division> divs = currentUser.GetUserDivisions(AppName);
            List<Hold> list = db.Holds.ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    where (((a.Duration == duration) || (duration == "All"))
                    && (a.Division == div)
                    && (a.Level == level)
                    && (a.Value == value)
                    && (a.HoldType == holdType)
                    )
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
            {
                duration = "All";
            }
            List<Division> divs = currentUser.GetUserDivisions(AppName);
            List<Hold> list = db.Holds.ToList();
            list = (from a in list
                    join d in divs 
                    on a.Division equals d.DivCode
                    where (a.Duration == duration || duration == "All") && 
                          (a.Division == div) &&
                          (a.HoldType == holdType) &&
                          (a.Store == store || string.IsNullOrEmpty(a.Store))                 
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
            if (string.IsNullOrEmpty(duration))
            {
                duration = "All";
            }
            List<Division> divs = currentUser.GetUserDivisions(AppName);
            List<Hold> list = db.Holds.ToList();
            IQueryable<Hold> holds = (from a in list
                                      join d in divs on a.Division equals d.DivCode
                                      where ((a.Duration == duration) || (duration == "All"))
                                      select a).AsQueryable();

            if (settings.FilterDescriptors.Any())
            {
                holds = holds.ApplyFilters(settings.FilterDescriptors);
            }
            Workbook excelDocument = CreateHoldsExport(holds.ToList());

            OoxmlSaveOptions save = new OoxmlSaveOptions(SaveFormat.Xlsx);
            excelDocument.Save(System.Web.HttpContext.Current.Response, "Holds.xlsx", ContentDisposition.Attachment, save);
            return RedirectToAction("Index");
        }

        private Workbook CreateHoldsExport(List<Hold> holds)
        {
            Workbook excelDocument = RetrieveHoldsExcelFile(false);
            int row = 1;
            Aspose.Cells.Worksheet workSheet = excelDocument.Worksheets[0];
            foreach (var rr in holds)
            {
                Aspose.Cells.Style align = excelDocument.CreateStyle();
                align.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Right;

                Aspose.Cells.Style date = excelDocument.CreateStyle();
                date.Number = 14;

                workSheet.Cells[row, 0].PutValue(rr.Division);
                workSheet.Cells[row, 0].SetStyle(align);
                workSheet.Cells[row, 1].PutValue(rr.Store);
                workSheet.Cells[row, 1].SetStyle(align);
                workSheet.Cells[row, 2].PutValue(rr.Level);
                workSheet.Cells[row, 2].SetStyle(align);
                workSheet.Cells[row, 3].PutValue(rr.Value);
                workSheet.Cells[row, 3].SetStyle(align);
                workSheet.Cells[row, 4].PutValue(rr.StartDate);
                workSheet.Cells[row, 4].SetStyle(date);
                workSheet.Cells[row, 5].PutValue(rr.EndDate);
                workSheet.Cells[row, 5].SetStyle(date);
                workSheet.Cells[row, 6].PutValue(rr.Duration);
                workSheet.Cells[row, 6].SetStyle(align);
                workSheet.Cells[row, 7].PutValue(rr.HoldType);
                workSheet.Cells[row, 7].SetStyle(align);
                workSheet.Cells[row, 8].PutValue(rr.Comments);
                workSheet.Cells[row, 8].SetStyle(align);
                row++;
            }

            for (int i = 0; i < 9; i++)
            {
                workSheet.AutoFitColumn(i);
            }
            return excelDocument;
        }

        private Workbook RetrieveHoldsExcelFile(bool errorFile)
        {
            int row = 0;
            int col = 0;

            Aspose.Cells.License license = new Aspose.Cells.License();
            license.SetLicense("C:\\Aspose\\Aspose.Cells.lic");

            Workbook excelDocument = new Workbook();
            Aspose.Cells.Worksheet workSheet = excelDocument.Worksheets[0];

            Aspose.Cells.Style style = excelDocument.CreateStyle();
            style.Font.IsBold = true;

            workSheet.Cells[row, col].PutValue("Division");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("Store");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("Level");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("Value");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("Start Date");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("End Date");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("Duration");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("Hold Type");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("Comments");
            workSheet.Cells[row, col].SetStyle(style);
            if (errorFile)
            {
                col++;
                workSheet.Cells[row, col].PutValue("Message");
                workSheet.Cells[row, col].SetStyle(style);
            }
            return excelDocument;
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
        public ActionResult Create(HoldModel model)
        {
            ViewData["ruleSetID"] = model.RuleSetID;
            ViewData["ruleType"] = "hold";

            holdService = new HoldService(currentUser, configService)
            {
                Hold = model.Hold
            };
            
            string validationMessage = holdService.ValidateHold(model.RuleSetID > 0, false, false);
            //string validationMessage = ValidateHold(model.Hold, (model.RuleSetID > 0), false);
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
                //TODO:  Do we want dept level security on holds???
                if (currentUser.GetUserDivisions(AppName).Exists(d => d.DivCode == model.Hold.Division))
                {
                    if ((model.Hold.Level == "Sku") && (model.Hold.Division != model.Hold.Value.Substring(0, 2)))
                    {
                        ViewData["message"] = "Invalid Sku, division does not match selection.";
                        model.Divisions = currentUser.GetUserDivisions(AppName);
                        return View(model);
                    }
                    else
                    {
                        if (model.RuleSetID == 0)
                        {
                            db.Holds.Add(model.Hold);
                            db.SaveChanges();
                            ApplyHoldsToExistingWebPicks(model.Hold);
                            return RedirectToAction("Index", new { duration = model.Hold.Duration });
                        }
                        else
                        {
                            //create hold for each store
                            RuleDAO dao = new RuleDAO();
                            Hold h;
                            foreach (StoreLookup s in dao.GetStoresInRuleSet(model.RuleSetID))
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

                                db.Holds.Add(h);
                                db.SaveChanges();

                                ApplyHoldsToExistingWebPicks(h);
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
                if (!(h.ReserveInventoryBool))
                {
                    needsave = true;
                    rdq.Status = "REJECTED";
                    db.Entry(rdq).State = System.Data.EntityState.Modified;
                }
                else if (rdq.Status.Contains("WEB PICK"))
                {
                    needsave = true;
                    rdq.Status = "HOLD-NEW";
                    db.Entry(rdq).State = System.Data.EntityState.Modified;
                }
            }
            if (needsave)
            {
                db.SaveChanges(UserName);
            }
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
        public ActionResult Edit(HoldModel model)
        {
            holdService = new HoldService(currentUser, configService)
            {
                Hold = model.Hold
            };
            
            string validationMessage = holdService.ValidateHold(false, true, false);
            //string validationMessage = ValidateHold(model.Hold, false, true);
            if (model.OriginalStartDate > model.Hold.StartDate)
            {
                validationMessage = "Start date must be after original start date of hold";
            }
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
                model.Hold = (from a in db.Holds 
                              where ((a.Division == div) && (a.Level == level) && (a.Value == value) && (a.ReserveInventory == 1)) 
                              select a).First();
            }
            else
            {
                model.Hold = (from a in db.Holds
                              where ((a.Division == div) && (a.Level == level) && (a.Value == value) && (a.ReserveInventory == 0)) 
                              select a).First();
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
            {
                model.Hold = (from a in db.Holds where ((a.Division == div) && (a.Store == store) && (a.ReserveInventory == 1)) select a).First();
            }
            else
            {
                model.Hold = (from a in db.Holds where ((a.Division == div) && (a.Store == store) && (a.ReserveInventory == 0)) select a).First();
            }
            model.Hold.Comments = "";
            model.Divisions = currentUser.GetUserDivisions(AppName);
            model.OriginalStartDate = model.Hold.StartDate;

            return View(model);
        }

        [HttpPost]
        public ActionResult MassEdit(HoldModel model)
        {
            List<Hold> holds;
            if (model.Hold.HoldType.Contains("Reserve"))
            {
                holds = (from a in db.Holds 
                         where ((a.Division == model.Hold.Division) && (a.Level == model.Hold.Level) && (a.Value == model.Hold.Value) && (a.ReserveInventory == 1)) 
                         select a).ToList();
            }
            else
            {
                holds = (from a in db.Holds 
                         where ((a.Division == model.Hold.Division) && (a.Level == model.Hold.Level) && (a.Value == model.Hold.Value) && (a.ReserveInventory == 0)) 
                         select a).ToList();
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
                {
                    h.Comments = model.Hold.Comments;
                }

                holdService.Hold = h;                
                
                string tempvalidationMessage = holdService.ValidateHold(false, true, false);
                //string tempvalidationMessage = ValidateHold(h, false, true);
                if (startdate > model.Hold.StartDate)
                {
                    tempvalidationMessage = "Start date must be after original start date of hold for store " + h.Store + "<br>";
                }

                if (tempvalidationMessage == "")
                {
                    updatedList.Add(h);
                }

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
                    uh.CreatedBy = User.Identity.Name;
                    db.Entry(uh).State = System.Data.EntityState.Modified;
                }
                db.SaveChanges();
                ViewData["message"] = "Successfully updated group of holds";
                return RedirectToAction("IndexByProduct");
            }
        }

        [HttpPost]
        public ActionResult MassEditStore(HoldModel model)
        {
            List<Hold> holds;
            if (model.Hold.HoldType.Contains("Reserve"))
            {
                holds = (from a in db.Holds where ((a.Division == model.Hold.Division) && (a.Store == model.Hold.Store) && (a.ReserveInventory == 1)) select a).ToList();
            }
            else
            {
                holds = (from a in db.Holds where ((a.Division == model.Hold.Division) && (a.Store == model.Hold.Store) && (a.ReserveInventory == 0)) select a).ToList();
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
                {
                    h.Comments = model.Hold.Comments;
                }

                holdService.Hold = h;
                string tempvalidationMessage = holdService.ValidateHold(false, true, false);
                //string tempvalidationMessage = ValidateHold(h, false, true);
                if (startdate > model.Hold.StartDate)
                {
                    tempvalidationMessage = "Start date must be after original start date of hold for store " + h.Store + "<br>";
                }

                if (tempvalidationMessage == "")
                {
                    updatedList.Add(h);
                }

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
                    uh.CreatedBy = User.Identity.Name;
                    db.Entry(uh).State = System.Data.EntityState.Modified;
                }
                db.SaveChanges();
                ViewData["message"] = "Successfully updated group of holds";
                return RedirectToAction("IndexByStore");
            }
        }

        public ActionResult ReleaseRDQs(int ID)
        {
            Hold model = (from a in db.Holds where a.ID == ID select a).First();
            DeleteHoldModel dh = new DeleteHoldModel();
            dh.Hold = model;
            ViewData["holdID"] = ID;
            RDQDAO dao = new RDQDAO();

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
                holds = (from a in db.Holds where ((a.Division == div) && (a.Level == level) && (a.Value == value) && (a.ReserveInventory == 1)) select a).ToList();
            }
            else
            {
                holds = (from a in db.Holds where ((a.Division == div) && (a.Level == level) && (a.Value == value) && (a.ReserveInventory == 0)) select a).ToList();
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
                holds = (from a in db.Holds where ((a.Division == div) && (a.Store == store) && (a.ReserveInventory == 1)) select a).ToList();
            }
            else
            {
                holds = (from a in db.Holds where ((a.Division == div) && (a.Store == store) && (a.ReserveInventory == 0)) select a).ToList();
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
                holds = (from a in db.Holds where ((a.Division == div) && (a.Level == level) && (a.Value == value) && (a.ReserveInventory == 1)) select a).ToList();
            }
            else
            {
                holds = (from a in db.Holds where ((a.Division == div) && (a.Level == level) && (a.Value == value) && (a.ReserveInventory == 0)) select a).ToList();
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
                var dao = new RDQDAO();

                var rdqs = GetRDQsInSession();

                foreach (RDQ rdq in rdqs)
                {
                    // Load rdq (so able to be removed from context)
                    var contextRDQ = db.RDQs.Find(rdq.ID);

                    //delete rdq
                    db.RDQs.Remove(contextRDQ);
                }

                // Persist changes
                db.SaveChanges(User.Identity.Name);
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
                db.SaveChanges(User.Identity.Name);
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

                    if ((rdq.PO != null) && (rdq.PO != "") && (rdq.PO != "N/A") && (rdq.Size.Length == 5))
                    {
                        rdq.DestinationType = "CROSSDOCK";
                    }
                    else
                    {
                        rdq.DestinationType = "WAREHOUSE";
                    }
                    db.Entry(contextRDQ).State = System.Data.EntityState.Modified;
                }

                // Persist changes
                db.SaveChanges(User.Identity.Name);
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
                var dao = new RDQDAO();

                var rdqs = GetRDQsInSession();

                foreach (RDQ rdq in rdqs)
                {
                    // Load rdq (so able to be removed from context)
                    var contextRDQ = db.RDQs.Find(rdq.ID);

                    contextRDQ.Status = "HOLD-REL";

                    if ((rdq.PO != null) && (rdq.PO != "") && (rdq.PO != "N/A") && (rdq.Size.Length == 5))
                    {
                        rdq.DestinationType = "CROSSDOCK";
                    }
                    else
                    {
                        rdq.DestinationType = "WAREHOUSE";
                    }
                    db.Entry(contextRDQ).State = System.Data.EntityState.Modified;
                }

                // Persist changes
                db.SaveChanges(User.Identity.Name);
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
            var groupRDQs = holdRDQs.Where(rdq =>
                rdq.Division == rdqGroup.Division
                && rdq.Store == rdqGroup.Store
                && rdq.WarehouseName == rdqGroup.WarehouseName
                && rdq.Category == rdqGroup.Category
                && rdq.Sku == rdqGroup.Sku)
                .ToList();

            //performance was really bad via entity framework, we'll just run a quick stored proc and update records in memory
            dao.DeleteRDQs(groupRDQs, UserName);

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
            var groupRDQs = holdRDQs.Where(rdq =>
                rdq.Division == rdqGroup.Division
                && rdq.Store == rdqGroup.Store
                && rdq.WarehouseName == rdqGroup.WarehouseName
                && rdq.Category == rdqGroup.Category
                && rdq.Sku == rdqGroup.Sku)
                .ToList();

            //performance was really bad via entity framework, we'll just run a quick stored proc and update records in memory
            RDQDAO dao = new RDQDAO();
            dao.ReleaseRDQs(groupRDQs, UserName);

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
            var groupRDQs = holdRDQs.Where(rdq =>
                rdq.Division == rdqGroup.Division
                && rdq.Store == rdqGroup.Store
                && rdq.WarehouseName == rdqGroup.WarehouseName
                && rdq.Category == rdqGroup.Category
                && rdq.Sku == rdqGroup.Sku)
                .ToList();

            //performance was really bad via entity framework, we'll just run a quick stored proc and update records in memory
            dao.DeleteRDQs(groupRDQs, UserName);

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
            var groupRDQs = holdRDQs.Where(rdq =>
                rdq.Division == rdqGroup.Division
                && rdq.Store == rdqGroup.Store
                && rdq.WarehouseName == rdqGroup.WarehouseName
                && rdq.Category == rdqGroup.Category
                && rdq.Sku == rdqGroup.Sku)
                .ToList();

            //performance was really bad via entity framework, we'll just run a quick stored proc and update records in memory
            RDQDAO dao = new RDQDAO();
            dao.ReleaseRDQs(groupRDQs, UserName);

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
            {
                return (List<RDQ>)Session["rdqgrouplist"];
            }
            else
            {
                return new List<RDQ>();
            }
        }

        private List<RDQ> GetRDQsForSession(string div, string level, string value)
        {
            List<RDQ> model;
            if ((Session["rdqgrouplist"] != null) &&
                ((String)Session["holdgrouprdq"] == div + "|" + level + "|" + value))
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
                    {
                        r.UnitQty = r.Qty;
                    }
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

                Session["holdgrouprdq"] = div + "|" + level + "|" + value;
                Session["rdqgrouplist"] = model;
            }
            return model;

        }

        private List<RDQ> GetRDQsForSession(string div, string store)
        {
            List<RDQ> model;
            if ((Session["rdqgrouplist"] != null) &&
                ((String)Session["holdgrouprdq"] == div + "|" + store))
            {
                model = (List<RDQ>)Session["rdqgrouplist"];
            }
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
                    {
                        r.UnitQty = r.Qty;
                    }
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
            if ((Session["holdrdq"] != null) &&
                ((Int64)Session["holdrdq"] == holdID))
            {
                model = (List<RDQ>)Session["holdrdqlist"];
            }
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
                    {
                        r.UnitQty = r.Qty;
                    }
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

                Session["holdrdq"] = holdID;
                Session["holdrdqlist"] = model;
            }
            return model;
        }


        [GridAction]
        public ActionResult _RDQs(Int32 holdID)
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

        public ActionResult UploadHoldsUpdates()
        {
            return View();
        }

        public ActionResult ExcelDeleteTemplate()
        {
            Aspose.Cells.License license = new Aspose.Cells.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Cells.lic");
                        
            string templateFilename = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["HoldsUpdates"]);
            Workbook excelDocument = new Workbook(System.Web.HttpContext.Current.Server.MapPath(templateFilename));            

            OoxmlSaveOptions save = new OoxmlSaveOptions(SaveFormat.Xlsx);
            excelDocument.Save(System.Web.HttpContext.Current.Response, "HoldsUpdates.xlsx", ContentDisposition.Attachment, save);            
            return View();
        }

        #region HoldsUpload spreadsheet
        public ActionResult ExcelHoldsUploadTemplate()
        {
            HoldsUploadSpreadsheet holdsUploadSpreadsheet = new HoldsUploadSpreadsheet(appConfig, configService, holdService);
            Excel excelDocument;

            excelDocument = holdsUploadSpreadsheet.GetTemplate();

            excelDocument.Save("HoldsUpload.xls", Aspose.Excel.SaveType.OpenInExcel, Aspose.Excel.FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View("HoldsUpload");
        }

        public ActionResult UploadHolds(IEnumerable<HttpPostedFileBase> attachments)
        {
            holdService = new HoldService(currentUser, configService);            

            HoldsUploadSpreadsheet holdsUploadSpreadsheet = new HoldsUploadSpreadsheet(appConfig, configService, holdService);

            string message = string.Empty;
            int successCount = 0;

            foreach (HttpPostedFileBase file in attachments)
            {
                holdsUploadSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(holdsUploadSpreadsheet.message))
                    return Content(message);
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
            Excel excelDocument;
            HoldsUploadSpreadsheet holdsUploadSpreadsheet = new HoldsUploadSpreadsheet(appConfig, configService, holdService);

            if (errors != null)
            {
                excelDocument = holdsUploadSpreadsheet.GetErrors(errors);
                excelDocument.Save("HoldsUploadErrors.xls", Aspose.Excel.SaveType.OpenInExcel, Aspose.Excel.FileFormatType.Default, System.Web.HttpContext.Current.Response);
            }
            return View();
        }
        #endregion

        public ActionResult MassDeleteHolds(IEnumerable<HttpPostedFileBase> attachments)
        {
            Aspose.Cells.License license = new Aspose.Cells.License();
            license.SetLicense("C:\\Aspose\\Aspose.Cells.lic");
            
            foreach (HttpPostedFileBase file in attachments)
            {
                Workbook workbook = new Workbook(file.InputStream);
                Aspose.Cells.Worksheet worksheet = workbook.Worksheets[0];

                int rows = worksheet.Cells.MaxDataRow;
                int columns = worksheet.Cells.MaxDataColumn;

                DataTable excelData = worksheet.Cells.ExportDataTable(0, 0, rows + 1, columns + 1, new ExportTableOptions() { ExportAsString = true, ExportColumnName = true});

                if (!(excelData.Columns[0].ColumnName == "Division" && excelData.Columns[1].ColumnName == "Store" && excelData.Columns[2].ColumnName == "Level" &&
                        excelData.Columns[3].ColumnName == "Value" && excelData.Columns[4].ColumnName == "Start Date" && excelData.Columns[5].ColumnName == "End Date" &&
                        excelData.Columns[6].ColumnName == "Duration" && excelData.Columns[7].ColumnName == "Hold Type" && excelData.Columns[8].ColumnName == "Comments"))
                {
                    return Content("Incorrectly formatted or missing header row. Please correct and re-process.");
                }

                List<DataRow> errorData = excelData.AsEnumerable().Where(x => x[7].ToString().Contains("Reserve")).ToList();                    

                List<HoldsUploadDeleteModel> errorList = new List<HoldsUploadDeleteModel>();

                foreach (DataRow reserveData in errorData)
                {
                    HoldsUploadDeleteModel error = new HoldsUploadDeleteModel();

                    string division = reserveData["Division"].ToString().Trim();    
                    string store = String.IsNullOrEmpty(reserveData["Store"].ToString().Trim()) ? null : reserveData["Store"].ToString().Trim();
                    string level = reserveData["Level"].ToString().Trim();
                    string value = reserveData["Value"].ToString().Trim();
                    DateTime StartDate = DateTime.Parse(reserveData["Start Date"].ToString().Trim());

                    Hold hold = (from a in db.Holds
                                 where (a.Division == division) && (a.Level == level) && (a.Value == value) && (a.ReserveInventory == 1) && 
                                        (a.StartDate == StartDate) && (a.Store == store)
                                  select a).FirstOrDefault();

                    if (hold != null && hold.ReserveInventoryBool)
                    {
                        RDQDAO dao = new RDQDAO();
                        if (dao.GetUniqueRDQsForHold(hold.ID).Count > 0)
                        {
                            DateTime? dt = null;
                            error.Division = division;
                            error.Store = store;
                            error.Level = level;
                            error.Value = value;
                            error.StartDate = StartDate;
                            error.EndDate = String.IsNullOrEmpty(reserveData["End Date"].ToString().Trim()) ? dt : DateTime.Parse(reserveData["End Date"].ToString().Trim());
                            error.Duration = reserveData["Duration"].ToString().Trim();
                            error.HoldType = reserveData["Hold Type"].ToString().Trim();
                            error.Comments = reserveData["Comments"].ToString().Trim();
                            error.ErrorMessage = "You must release all RDQs before you can delete this hold.";

                            errorList.Add(error);
                            excelData.Rows.Remove(reserveData);
                        }
                    }                    
                }
                
                foreach (DataRow validRows in excelData.Rows)
                {

                    string division = validRows["Division"].ToString().Trim();
                    string store = String.IsNullOrEmpty(validRows["Store"].ToString().Trim()) ? null : validRows["Store"].ToString().Trim();
                    string level = validRows["Level"].ToString().Trim();
                    string value = validRows["Value"].ToString().Trim();
                    DateTime StartDate = DateTime.Parse(validRows["Start Date"].ToString().Trim());

                    Hold hold = (from a in db.Holds
                                 where (a.Division == division) && (a.Level == level) && (a.Value == value) &&
                                        (a.StartDate == StartDate) && (a.Store == store)
                                 select a).FirstOrDefault();

                    DateTime? dt = null;
                    hold.EndDate = String.IsNullOrEmpty(validRows["End Date"].ToString().Trim()) ? dt : DateTime.Parse(validRows["End Date"].ToString().Trim());
                    hold.Duration = validRows["Duration"].ToString().Trim();
                    hold.HoldType = validRows["Hold Type"].ToString().Trim();
                    hold.Comments = validRows["Comments"].ToString().Trim();
                    hold.CreateDate = DateTime.Now;
                    hold.CreatedBy = User.Identity.Name;

                    db.Entry(hold).State = System.Data.EntityState.Modified;                    
                }
                db.SaveChanges();

                if (errorList.Count() > 0)
                {
                    Session["errorList"] = errorList;

                    string msg = excelData.Rows.Count + " Successfully uploaded";
                    // and " + errorList.Count() + " error records downloaded to excel";

                    //Workbook excelDocument = CreateErrorListExcel(errorList);

                    //OoxmlSaveOptions save = new OoxmlSaveOptions(SaveFormat.Xlsx);
                    //excelDocument.Save(System.Web.HttpContext.Current.Response, "HoldsErrorList.xlsx", ContentDisposition.Attachment, save);
                    //excelDocument.Save("HoldsErrorList.xlsx", save);

                    return Content(msg);
                }
            }

            return Content("");
        }

        public ActionResult DownloadDeleteErrors()
        {
            List<HoldsUploadDeleteModel> errorList = (List<HoldsUploadDeleteModel>)Session["errorList"];

            Aspose.Cells.License license = new Aspose.Cells.License();
            license.SetLicense("C:\\Aspose\\Aspose.Cells.lic");

            Workbook excelDocument = RetrieveHoldsExcelFile(true);
            int row = 1;
            Aspose.Cells.Worksheet workSheet = excelDocument.Worksheets[0];
            foreach (HoldsUploadDeleteModel rr in errorList)
            {
                Aspose.Cells.Style align = excelDocument.CreateStyle();
                align.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Right;

                Aspose.Cells.Style date = excelDocument.CreateStyle();
                date.Number = 14;

                workSheet.Cells[row, 0].PutValue(rr.Division);
                workSheet.Cells[row, 0].SetStyle(align);
                workSheet.Cells[row, 1].PutValue(rr.Store);
                workSheet.Cells[row, 1].SetStyle(align);
                workSheet.Cells[row, 2].PutValue(rr.Level);
                workSheet.Cells[row, 2].SetStyle(align);
                workSheet.Cells[row, 3].PutValue(rr.Value);
                workSheet.Cells[row, 3].SetStyle(align);
                workSheet.Cells[row, 4].PutValue(rr.StartDate);
                workSheet.Cells[row, 4].SetStyle(date);
                workSheet.Cells[row, 5].PutValue(rr.EndDate);
                workSheet.Cells[row, 5].SetStyle(date);
                workSheet.Cells[row, 6].PutValue(rr.Duration);
                workSheet.Cells[row, 6].SetStyle(align);
                workSheet.Cells[row, 7].PutValue(rr.HoldType);
                workSheet.Cells[row, 7].SetStyle(align);
                workSheet.Cells[row, 8].PutValue(rr.Comments);
                workSheet.Cells[row, 8].SetStyle(align);
                workSheet.Cells[row, 9].PutValue(rr.ErrorMessage);
                workSheet.Cells[row, 9].SetStyle(align);
                row++;
            }

            for (int i = 0; i < 10; i++)
            {
                workSheet.AutoFitColumn(i);
            }

            OoxmlSaveOptions save = new OoxmlSaveOptions(SaveFormat.Xlsx);
            excelDocument.Save(System.Web.HttpContext.Current.Response, "HoldsErrorList.xlsx", ContentDisposition.Attachment, save);

            return View();
        }


        /// <summary>
        /// Will check to ensure the data on the next row has data
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private bool HasDataOnRow(Aspose.Excel.Worksheet sheet, int row)
        {
            return sheet.Cells[row, 0].Value != null ||
                   sheet.Cells[row, 1].Value != null ||
                   sheet.Cells[row, 2].Value != null ||
                   sheet.Cells[row, 3].Value != null ||
                   sheet.Cells[row, 4].Value != null ||
                   sheet.Cells[row, 5].Value != null ||
                   sheet.Cells[row, 6].Value != null ||
                   sheet.Cells[row, 7].Value != null ||
                   sheet.Cells[row, 8].Value != null ||
                   sheet.Cells[row, 9].Value != null ||
                   sheet.Cells[row, 10].Value != null ||
                   sheet.Cells[row, 11].Value != null ||
                   sheet.Cells[row, 12].Value != null;
        }
        #endregion
    }
}
