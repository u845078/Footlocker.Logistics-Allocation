using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Spreadsheets;
using Footlocker.Logistics.Allocation.DAO;
using Aspose.Cells;
using Telerik.Web.Mvc;
using System.Data;
using Footlocker.Logistics.Allocation.Common;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Div Logistics,Director of Allocation,Support,Epick")]
    public class WebPickController : AppController
    {
        readonly List<string> webPickRoles = new List<string> { "Merchandiser", "Head Merchandiser", "Div Logistics",
                    "Director of Allocation", "Support" };
        //
        // GET: /WebPick/
        AllocationContext db = new AllocationContext();
        ConfigService configService = new ConfigService();

        #region WebPick screens
        public ActionResult Index(string message)
        {
            List<string> divs = currentUser.GetUserDivList(AppName);
            List<RDQ> list = db.RDQs.Where(r => r.Type == "user").ToList();

            list = (from a in list
                    join b in divs 
                      on a.Division equals b
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

            ViewBag.Message = message;            

            return View(list);
        }

        public ActionResult Create()
        {
            WebPickModel model = new WebPickModel();
            InitializeCreate(model);
            model.RDQ = new RDQ();

            return View(model);
        }

        private void InitializeCreate(WebPickModel model)
        {
            model.Divisions = currentUser.GetUserDivisions(AppName);
            model.DCs = (from a in db.DistributionCenters
                         select a).ToList();

            model.PickOptions = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Store's next pick day",
                    Value = "WEB PICK"
                }
            };

            if (currentUser.HasUserRole(AppName, "EPick"))
            {
                List<string> userRoles = currentUser.GetUserRoles(AppName);

                // if they don't have any web pick roles, take out web pick
                if (!userRoles.Intersect(webPickRoles).Any())
                    model.PickOptions.Clear();

                model.PickOptions.Add(new SelectListItem() { Text = "Pick right away", Value = "E-PICK" });
            }
        }

        [HttpPost]
        public ActionResult Create(WebPickModel model)
        {
            string message = "";

            ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);

            if (!itemDAO.DoValidSizesExist(model.RDQ.Sku, model.RDQ.Size))
                ModelState.AddModelError("RDQ.Size", "Size does not exist for this sku");

            if (model.RDQ.Qty <= 0)
                ModelState.AddModelError("RDQ.Qty", "Qty must be greater than zero.");

            if (model.RDQ.Status == "E-PICK")
            {
                if (db.DistributionCenters.Where(dc => dc.ID == model.RDQ.DCID && dc.TransmitRDQsToKafka).Count() == 0)
                    ModelState.AddModelError("RDQ.Status", "This DC is not accepting E-Picks from Allocation yet.");
            }

            if (!(model.RDQ.Store.Length == 5 || model.RDQ.Store.Length == 2))
                ModelState.AddModelError("RDQ.Store", string.Format("{0} is not a valid store or warehouse code.", model.RDQ.Store));

            if (string.IsNullOrEmpty(model.Message) && ModelState.IsValid)
            {                
                int instance = configService.GetInstance(model.RDQ.Division);
                DateTime controlDate = configService.GetControlDate(instance);

                if (!string.IsNullOrEmpty(model.RDQ.PO))
                {
                    if (model.RDQ.Status == "E-PICK")
                        ModelState.AddModelError("RDQ.Status", "You cannot do an E-Pick with a PO");
                    else
                    {
                        message = CreateFutureUserRDQ(model.RDQ);
                    }
                }
                else
                {
                    if (model.RDQ.Status == "E-PICK")
                    {
                        if (!string.IsNullOrEmpty(model.RDQ.PO))
                            ModelState.AddModelError("RDQ.Status", "You cannot do an E-Pick with a PO");
                        else
                        {
                            model.RDQ.TransmitControlDate = controlDate;

                            if (model.RDQ.Size.Length == 3)
                                model.RDQ.RecordType = "1";
                            else
                                model.RDQ.RecordType = "4";
                        }
                    }

                    message = CreateRDQ(model.RDQ, model.RDQ.Status == "WEB PICK");
                }

                if (string.IsNullOrEmpty(message) && ModelState.IsValid)
                {
                    //call to apply holds
                    List<RDQ> list = new List<RDQ>
                    {
                        model.RDQ
                    };

                    RDQDAO rdqDAO = new RDQDAO();
                    int holdcount = rdqDAO.ApplyHolds(list, instance);
                    int cancelholdcount = rdqDAO.ApplyCancelHolds(list);
                    message = "Web/emergency pick generated.  ";
                    if (cancelholdcount > 0)
                    {
                        message += string.Format("{0} rejected by cancel inventory hold. ", cancelholdcount);
                    }

                    if (holdcount > 0)
                    {
                        message += string.Format("{0} on hold. Please go to Release Held RDQs to see held RDQs. ", holdcount);
                    }
                    return RedirectToAction("Index", new { message });
                }

                model.Message = message;
            }

            InitializeCreate(model);
            return View(model);
        }

        private string CreateFutureUserRDQ(RDQ rdq)
        {
            string message = "";
            bool webPick = false;

            if (TryUpdateModel(rdq, "RDQ"))
            {
                if (rdq.Division != rdq.Sku.Substring(0, 2))
                    message = string.Format("Division must be same for sku and store {0} {1}", rdq.Division, rdq.Sku.Substring(0, 2));
                else
                {
                    if (rdq.Store.Length == 5)
                    {
                        if (db.vValidStores.Where(vs => vs.Division == rdq.Division && vs.Store == rdq.Store).Count() == 0)
                            message = string.Format("{0}-{1} is not a valid store.", rdq.Division, rdq.Store);
                    }
                    else if (rdq.Store.Length == 2)
                    {
                        if (db.DistributionCenters.Where(dc => dc.MFCode == rdq.Store).Count() == 0)
                            message = string.Format("{0} is not a valid warehouse code.", rdq.Store);
                    }

                    if (string.IsNullOrEmpty(message))
                    {
                        rdq.DistributionCenter = db.DistributionCenters.Where(d => d.ID == rdq.DCID).FirstOrDefault();

                        if (rdq.Size.Length == 3)
                        {
                            if (rdq.DistributionCenter.Type == "CROSSDOCK")
                                message = "You provided a Distribution Center that only does crossdock orders and you used a bin size";
                        }
                    }
                }

                if (message == "")
                {
                    List<LegacyFutureInventory> futureInventories = db.LegacyFutureInventory.Where(lfi => lfi.InventoryID == rdq.PO + "-" + rdq.Division).ToList();

                    if (futureInventories.Count > 0)
                    {
                        LegacyFutureInventory futureInv = futureInventories.Where(fi => fi.Sku == rdq.Sku && fi.Store == rdq.DistributionCenter.MFCode && fi.Size == rdq.Size).FirstOrDefault();

                        if (futureInv != null)
                        {
                            // if not product pack, just make it a future RDQ - no error
                            if (rdq.Size.Length == 5 && futureInv.ProductNodeType != "PRODUCT_PACK")
                                webPick = true;
                            else
                            {
                                int inventoryReductionQty = 0;

                                InventoryReductions reductions = db.InventoryReductions.Where(ir => ir.PO == rdq.PO && ir.Sku == rdq.Sku && ir.Size == rdq.Size).FirstOrDefault();
                                if (reductions != null)
                                    inventoryReductionQty = reductions.Qty;

                                if (rdq.Qty > futureInv.StockQty - inventoryReductionQty)
                                    message = string.Format("Not enough inventory. Amount available (for size) is {0}", futureInv.StockQty - inventoryReductionQty);
                            }
                        }
                        else
                            message = "Did not find any future inventory for this specific SKU/Size/DC";
                    }
                    else
                        message = "There are no Future Inventory records set up for this PO. Try again tomorrow if the PO was just created";
                }

                if (message == "")
                {
                    rdq.CreateDate = DateTime.Now;
                    rdq.CreatedBy = currentUser.NetworkID;
                    rdq.LastModifiedUser = currentUser.NetworkID;

                    if (rdq.Size.Length == 5 && !webPick)
                    {
                        rdq.Status = "PICK-XDC";
                        rdq.DestinationType = "CROSSDOCK";
                    }
                    else
                    {
                        rdq.Status = "WEB PICK";
                        rdq.DestinationType = "WAREHOUSE";
                    }

                    rdq.Type = "user";
                    try
                    {
                        rdq.ItemID = db.ItemMasters.Where(im => im.MerchantSku == rdq.Sku).First().ID;
                    }
                    catch
                    {
                        message = "invalid sku";
                    }

                    if (rdq.ItemID > 0)
                    {
                        try
                        {
                            db.RDQs.Add(rdq);
                            db.SaveChanges(currentUser.NetworkID);
                        }
                        catch (Exception e)
                        {
                            message = e.Message;
                        }
                    }
                }
            }

            return message;
        }

        /// <summary>
        /// Will create a single RDQ if possible
        /// If not enough inventory, will report message and whether or not inventory can go negative
        /// </summary>
        /// <param name="rdq">pick details</param>
        /// <param name="pickAnyway">returns true if inventory can go negative (no ringfences)</param>
        /// <param name="message">reason we can't create this pick</param>
        /// <returns></returns>
        private string CreateRDQ(RDQ rdq, bool validateInventory)
        {
            string message = "";
            if (TryUpdateModel(rdq, "RDQ"))
            {
                message = AdditionalValidations(rdq, validateInventory);

                if (message == "")
                {
                    rdq.CreateDate = DateTime.Now;
                    rdq.CreatedBy = currentUser.NetworkID;
                    rdq.LastModifiedUser = currentUser.NetworkID;

                    rdq.PO = "";
                    rdq.DestinationType = "WAREHOUSE";
                    rdq.Type = "user";
                    try
                    {
                        rdq.ItemID = (from a in db.ItemMasters
                                      where a.MerchantSku == rdq.Sku
                                      select a.ID).First();
                    }
                    catch
                    {
                        message = "invalid sku";
                    }

                    if (rdq.ItemID > 0)
                    {
                        try
                        {
                            db.RDQs.Add(rdq);
                            db.SaveChanges(currentUser.NetworkID);
                        }
                        catch (Exception e)
                        {
                            message = e.Message;
                        }
                    }
                }
            }

            return message;
        }

        /// <summary>
        /// Will validate if we have inventory to pick
        /// </summary>
        /// <param name="rdq">information for pick</param>
        /// <param name="pickAnyway">returns true if no RDQs (inventory is allowed to go negative), false means there are rdqs (inventory is not allowed to go negative)</param>
        /// <returns></returns>
        public string AdditionalValidations(RDQ rdq, bool validateInventory)
        {
            string message = "";

            if (rdq.Division != rdq.Sku.Substring(0, 2))            
                message = string.Format("Division must be same for sku and store {0} {1}", rdq.Division, rdq.Sku.Substring(0, 2));            
            else
            {
                if (rdq.Store.Length == 5)
                {
                    if (db.vValidStores.Where(vs => vs.Division == rdq.Division && vs.Store == rdq.Store).Count() == 0)                    
                        message = string.Format("{0}-{1} is not a valid store.", rdq.Division, rdq.Store);                    
                }
                else if (rdq.Store.Length == 2)
                {
                    if (db.DistributionCenters.Where(dc => dc.MFCode == rdq.Store).Count() == 0)
                        message = string.Format("{0} is not a valid warehouse code.", rdq.Store);                    
                }
                else                
                    message = string.Format("{0} is not a valid store or warehouse code.", rdq.Store);                
            }

            if (message == "")
            {
                if (validateInventory)
                {
                    int qtyAvailable = InventoryAvailableToPick(rdq);
                    if (qtyAvailable < rdq.Qty)
                    {
                        message = string.Format("Not enough inventory. Amount available (for size) is {0}", qtyAvailable);
                    }
                }
            }
            return message;
        }
        #endregion

        public ActionResult BulkAdmin(string message)
        {
            BulkRDQModel model = new BulkRDQModel();

            InitializeDivisions(model);
            InitializeDepartments(model, false);
            ViewData["message"] = message;
            ViewData["ruleSetID"] = model.RuleSetID;
            ViewData["ruleType"] = "rdq";
            model.SearchResult = false;

            return View(model);
        }

        [HttpPost]
        public ActionResult BulkAdmin(BulkRDQModel model)
        {
            ViewData["ruleSetID"] = model.RuleSetID;
            ViewData["ruleType"] = "rdq";
            InitializeDivisions(model);
            InitializeDepartments(model, false);
            model.HaveResults = true;

            if (model.ShowStoreSelector == "yes")
            {
                // the user wants to select multiple stores. See if there is a rdq ruleset established and create one if it doesn't exist.
                if (model.RuleSetID < 1)
                {
                    //get a new ruleset
                    RuleSet rs = new RuleSet
                    {
                        Type = "rdq",
                        CreateDate = DateTime.Now,
                        CreatedBy = currentUser.NetworkID
                    };

                    db.RuleSets.Add(rs);

                    // this will generate the new RuleSetID so we can send it back
                    db.SaveChanges();

                    model.RuleSetID = rs.RuleSetID;
                }

                ViewData["ruleSetID"] = model.RuleSetID;
                return View(model);
            }
            model.SearchResult = false;

            // This isn't working right now based on the old Telerik grids we use. Maybe eventual upgrades will make this better

            //model.RDQResults = GetRDQsForSession(model.Instance, model.Division, model.Department, model.Category, model.Sku, model.Status, 
            //    model.PO, model.Store, model.RuleSetID);

            //var rdqGroups = from rdq in model.RDQResults
            //                group rdq by new
            //                {
            //                    Division = rdq.Division,
            //                    Store = rdq.Store,
            //                    WarehouseName = rdq.WarehouseName,
            //                    Category = rdq.Category,
            //                    ItemID = rdq.ItemID,
            //                    Sku = rdq.Sku,
            //                    Status = rdq.Status
            //                } into g
            //                select new RDQGroup()
            //                {
            //                    Division = g.Key.Division,
            //                    Store = g.Key.Store,
            //                    WarehouseName = g.Key.WarehouseName,
            //                    Category = g.Key.Category,
            //                    ItemID = Convert.ToInt64(g.Key.ItemID),
            //                    Sku = g.Key.Sku,
            //                    IsBin = g.Where(r => r.Size.Length > 3).Any() ? false : true,
            //                    Qty = g.Sum(r => r.Qty),
            //                    UnitQty = g.Sum(r => r.UnitQty),
            //                    Status = g.Key.Status
            //                };

            //model.RDQGroups = rdqGroups.OrderBy(g => g.Division)
            //        .ThenBy(g => g.Store)
            //        .ThenBy(g => g.WarehouseName)
            //        .ThenBy(g => g.Category)
            //        .ThenBy(g => g.Sku)
            //        .ThenBy(g => g.Status)
            //        .ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult RefreshDivisions(BulkRDQModel model)
        {
            InitializeDivisions(model);
            InitializeDepartments(model, true);
            return View("BulkAdmin", model);
        }

        [HttpPost]
        public ActionResult RefreshDepartments(BulkRDQModel model)
        {
            InitializeDivisions(model);
            InitializeDepartments(model, false);
            return View("BulkAdmin", model);
        }

        private void InitializeDivisions(BulkRDQModel model)
        {
            List<Footlocker.Common.Division> divs = currentUser.GetUserDivisions(AppName);

            var allInstances = (from a in db.Instances 
                                join b in db.InstanceDivisions 
                                on a.ID equals b.InstanceID 
                                select new { instance = a, Division = b.Division }).ToList();

            model.Instances = (from a in allInstances 
                               join b in divs 
                               on a.Division equals b.DivCode 
                               select a.instance).Distinct().ToList();

            if (model.Instances.Any())
            {
                //if no selected instance, default to first one in the list
                if (model.Instance == 0)
                    model.Instance = model.Instances.First().ID;

                model.Divisions = (from a in allInstances
                                   join b in divs on a.Division equals b.DivCode
                                   where a.instance.ID == model.Instance
                                   select b).ToList();
            }
            else            
                model.Instances.Insert(0, new Instance() { ID = -1, Name = "No division permissions enabled" });            

            model.StatusList = (from a in db.RDQs 
                                select a.Status).Distinct().ToList();
            model.StatusList.Sort();
            model.StatusList.Insert(0, "All");
        }

        private void InitializeDepartments(BulkRDQModel model, bool resetDivision)
        {
            if (model.Divisions.Any())
            {
                if (resetDivision)
                {
                    //default to first one in the list
                    model.Division = model.Divisions.First().DivCode;
                }
                else
                {
                    //default to first one in the list if not selected
                    if (string.IsNullOrEmpty(model.Division))
                        model.Division = model.Divisions.First().DivCode;
                }

                model.Departments = currentUser.GetUserDepartments(AppName).Where(d => d.DivCode == model.Division).ToList();
                if (model.Departments.Any())                
                    model.Departments.Insert(0, new Department() { DeptNumber = "00", DepartmentName = "All departments" });                
                else                
                    model.Departments.Insert(0, new Department() { DeptNumber = "-1", DepartmentName = "No department permissions enabled" });                
            }
        }

        [HttpPost]
        public ActionResult ReleaseAll(BulkRDQModel model)
        {
            RDQDAO rdqDAO = new RDQDAO();
            List<RDQ> rdqsToRelease = GetRDQsForSession(model.Instance, model.Division, model.Department, model.Category, model.Sku, model.Status, model.PO, model.Store, model.RuleSetID);

            int beforeCheck = rdqsToRelease.Count();

            rdqsToRelease = rdqsToRelease.Where(rtr => rtr.Status.StartsWith("HOLD") && rtr.Status != "HOLD-XDC").ToList(); 

            int afterCheck = rdqsToRelease.Count();

            if (beforeCheck != afterCheck)
                ViewBag.message = "Releasing RDQs only works with HOLD-related statuses and HOLD-XDC RDQs can't be released to stores, so these records were skipped. Your results may be less than what you expected.";

            rdqDAO.ReleaseRDQs(rdqsToRelease, currentUser.NetworkID);
            
            model.SearchResult = false;
            InitializeDivisions(model);
            InitializeDepartments(model, false);
            return View("BulkAdmin", model);
        }

        [HttpPost]
        public ActionResult ReleaseAllToWarehouse(BulkRDQModel model)
        {
            RDQDAO rdqDAO = new RDQDAO();

            List<RDQ> rdqsToRelease = GetRDQsForSession(model.Instance, model.Division, model.Department, model.Category, model.Sku, model.Status, model.PO, model.Store, model.RuleSetID);

            rdqDAO.DeleteRDQs(rdqsToRelease, currentUser.NetworkID);

            model.SearchResult = false;
            InitializeDivisions(model);
            InitializeDepartments(model, false);
            return View("BulkAdmin", model);
        }

        [GridAction]
        public ActionResult _BulkRDQs(int instanceID, string div, string department, string category, string sku, string status, string po, string store, long ruleset)
        {
            List<RDQ> rdqList = GetRDQsForSession(instanceID, div, department, category, sku, status, po, store, ruleset);

            var rdqGroups = from rdq in rdqList
                            group rdq by new
                            {
                                Division = rdq.Division,
                                Store = rdq.Store,
                                WarehouseName = rdq.WarehouseName,
                                Category = rdq.Category,
                                ItemID = rdq.ItemID,
                                Sku = rdq.Sku,
                                Status = rdq.Status
                            } into g
                            select new RDQGroup()
                            {
                                InstanceID = instanceID,
                                Division = g.Key.Division,
                                Store = g.Key.Store,
                                WarehouseName = g.Key.WarehouseName,
                                Category = g.Key.Category,
                                ItemID = Convert.ToInt64(g.Key.ItemID),
                                Sku = g.Key.Sku,
                                IsBin = g.Where(r => r.Size.Length > 3).Any() ? false : true,
                                Qty = g.Sum(r => r.Qty),
                                UnitQty = g.Sum(r => r.UnitQty),
                                Status = g.Key.Status
                            };

            List<RDQGroup> rdqGroupList = rdqGroups.OrderBy(g => g.Division)
                    .ThenBy(g => g.Store)
                    .ThenBy(g => g.WarehouseName)
                    .ThenBy(g => g.Category)
                    .ThenBy(g => g.Sku)
                    .ThenBy(g => g.Status)
                    .ToList();

            return View(new GridModel(rdqGroupList));
        }

        [GridAction]
        public ActionResult _RDQDetails(int instanceID, string div, string store, string warehousename, string sku, string status)
        {
            RDQDAO rdqDAO = new RDQDAO();
            List<RDQ> rdqList = rdqDAO.GetHeldRDQs(instanceID, div, "00", "", sku, "", "", status);

            rdqList = rdqList.Where(rl => rl.Store == store && rl.WarehouseName == warehousename).ToList();

            List<string> uniqueNames = (from l in rdqList
                                        select l.CreatedBy).Distinct().ToList();
            Dictionary<string, string> fullNamePairs = new Dictionary<string, string>();

            foreach (var item in uniqueNames)
            {
                fullNamePairs.Add(item, getFullUserNameFromDatabase(item.Replace('\\', '/')));
            }

            foreach (var item in fullNamePairs)
            {
                rdqList.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
            }

            return View(new GridModel(rdqList));
        }

        private List<RDQ> GetRDQsForSession(int instance, string div, string department, string category, string sku, string status, string po, string store, long ruleset)
        {
            List<RDQ> rdqList = new List<RDQ>();
            List<RDQ> tempRDQList = new List<RDQ>();
            RDQDAO rdqDAO = new RDQDAO();

            // if it's all departments
            if (department == "00")
            {
                // get the list of user departments
                List<string> depts = currentUser.GetUserDivDept(AppName).Where(d => d.StartsWith(string.Format("{0}-", div))).ToList();

                tempRDQList = rdqDAO.GetHeldRDQs(instance, div, department, category, sku, po, store, status);

                foreach (RDQ rdq in tempRDQList)
                {
                    if (currentUser.HasDivDept(AppName, div, rdq.Department))                        
                        rdqList.Add(rdq);                                          
                }
			}
            else
            {
                // user supplied a department
                rdqList = rdqDAO.GetHeldRDQs(instance, div, department, category, sku, po, store, status);
			}

			RuleDAO dao = new RuleDAO();
            if (ruleset > 0)
            {
                List<StoreLookup> stores = dao.GetRuleSelectedStoresInRuleSet(ruleset);
                rdqList = (from a in rdqList 
                            join b in stores 
                                on new { a.Division, a.Store } equals new { b.Division, b.Store } 
                            select a).ToList();
            }

            return rdqList;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _BulkPick([Bind(Prefix = "updated")]IEnumerable<RDQ> updated)
        {
            RDQ updateRDQ;
            foreach (RDQ r in updated)
            {
                if (r.Release)
                {
                    //need to pick this
                    updateRDQ = db.RDQs.Where(a => a.ID == r.ID).FirstOrDefault();
                    updateRDQ.Status = "HOLD-REL";
                }
            }

            db.SaveChanges(currentUser.NetworkID);

            List<Footlocker.Common.Division> divs = currentUser.GetUserDivisions(AppName);
            List<RDQ> list = (from a in db.RDQs 
                              join b in divs 
                                on a.Division equals b.DivCode 
                              select a).ToList();

            return View(new GridModel(list));
        }

        [HttpPost]
        public ActionResult ReleaseRDQGroupToWarehouse(RDQGroup rdqGroup)
        {
            // Get all RDQs of specified SKU for specified hold
            var rdqDAO = new RDQDAO();
            List<RDQ> holdRDQs = rdqDAO.GetHeldRDQs(rdqGroup.InstanceID, rdqGroup.Division, "00", "", rdqGroup.Sku, "", rdqGroup.Store, rdqGroup.Status);

            //performance was really bad via entity framework, we'll just run a quick stored proc and update records in memory
            rdqDAO.DeleteRDQs(holdRDQs, currentUser.NetworkID);

            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult ReleaseRDQGroup(RDQGroup rdqGroup, string status)
        {
            var rdqDAO = new RDQDAO();
            List<RDQ> holdRDQs = rdqDAO.GetHeldRDQs(rdqGroup.InstanceID, rdqGroup.Division, "00", "", rdqGroup.Sku, "", rdqGroup.Store, rdqGroup.Status);

            var groupRDQs = holdRDQs.Where(rdq => rdq.WarehouseName == rdqGroup.WarehouseName).ToList();

            //performance was really bad via entity framework, we'll just run a quick stored proc and update records in memory
            rdqDAO.ReleaseRDQs(groupRDQs, currentUser.NetworkID);

            if (status == "All")            
                groupRDQs.ForEach(rdq => { rdq.Status = "HOLD-REL"; });            
            else            
                groupRDQs.ForEach(rdq => holdRDQs.Remove(rdq));            
           
            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult ReleaseRDQToWarehouse(long id)
        {
            // Get all RDQs of specified SKU for specified hold            
            RDQ deleteRDQ = db.RDQs.Where(r => r.ID == id).FirstOrDefault();
            db.RDQs.Remove(deleteRDQ);
            db.SaveChanges(currentUser.NetworkID);

            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult ReleaseRDQ(long id)
        {
            RDQ releaseRDQ = db.RDQs.Where(r => r.ID == id).FirstOrDefault();
            releaseRDQ.Status = "HOLD-REL";
            db.Entry(releaseRDQ).State = EntityState.Modified;  
            
            db.SaveChanges(currentUser.NetworkID);

            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        public ActionResult AuditIndex()
        {
            AuditRDQModel model = new AuditRDQModel
            {
                StartDate = DateTime.Now.AddDays(-7),
                EndDate = DateTime.Now
            };

            model.list = db.AuditRDQs.Where(ar => ar.PickDate >= model.StartDate && ar.PickDate <= model.EndDate).ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult AuditIndex(AuditRDQModel model)
        {
            model.list = (from a in db.AuditRDQs where ((a.PickDate >= model.StartDate) && (a.PickDate <= model.EndDate)) select a).ToList();
            return View(model);
        }

        [GridAction]
        public ActionResult _AuditIndex(string startdate, string enddate)
        {
            DateTime start = Convert.ToDateTime(startdate);
            DateTime end = Convert.ToDateTime(enddate);
            List<AuditRDQ> list = db.AuditRDQs.Where(ar => ar.PickDate >= start && ar.PickDate <= end).ToList();

            List<DistributionCenter> dcs = db.DistributionCenters.ToList();
            foreach (AuditRDQ a in list)
            {
                a.WarehouseName = dcs.Where(d => d.ID == a.DCID).FirstOrDefault().Name;
            }
            return View(new GridModel(list));
        }

        private int InventoryAvailableToPick(RDQ rdq)
        {
            List<WarehouseInventory> whInventory;
            int qtyAvailable;

            string warehouse = (from a in db.DistributionCenters
                                where a.ID == rdq.DCID
                                select a.MFCode).FirstOrDefault();
            if (warehouse == null)
                return 0;

            //find maximum qty
            WarehouseInventoryDAO dao = new WarehouseInventoryDAO(rdq.Sku, warehouse, appConfig.EuropeDivisions);
            whInventory = dao.GetWarehouseInventory(WarehouseInventoryDAO.InventoryListType.ListOnlyAvailableSizes);
            qtyAvailable = (from wi in whInventory
                            where wi.size == rdq.Size
                            select wi.availableQuantity).FirstOrDefault();

            return qtyAvailable;
        }

        public ActionResult Delete(long ID)
        {
            RDQ rdq = db.RDQs.Where(r => r.ID == ID).FirstOrDefault();

            string message = "";
            try
            {
                db.RDQs.Remove(rdq);
                db.SaveChanges(currentUser.NetworkID);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return RedirectToAction("Index", new { message });
        }

        public ActionResult ExcelUpload(string message)
        {
            ViewData["errorMessage"] = message;
            return View();
        }

        public ActionResult ExcelTemplate()
        {
            Workbook excelDocument;
            WebPickSpreadsheet webPickSpreadsheet = new WebPickSpreadsheet(appConfig, webPickRoles, configService);

            excelDocument = webPickSpreadsheet.GetTemplate();

            excelDocument.Save(System.Web.HttpContext.Current.Response, "WebPickUpload.xlsx", ContentDisposition.Attachment, webPickSpreadsheet.SaveOptions);
            return View();
        }

        public ActionResult Save(IEnumerable<HttpPostedFileBase> attachments)
        {
            WebPickSpreadsheet webPickSpreadsheet = new WebPickSpreadsheet(appConfig, webPickRoles, configService);

            foreach (var file in attachments)
            {
                webPickSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(webPickSpreadsheet.message))
                    return Content(webPickSpreadsheet.message);
                else
                {
                    if (webPickSpreadsheet.errorList.Count > 0)
                    {
                        string errorMessage = string.Format("{0} errors were found and {1} lines were processed successfully. <br />You can review the quantity details on the Release Held RDQ page.",
                                              webPickSpreadsheet.errorList.Count, webPickSpreadsheet.validRDQs.Count);
                        Session["errorList"] = webPickSpreadsheet.errorList;
                        return Content(errorMessage);
                    }
                }
            }

            string message = string.Format("Success!  {0} lines were processed. <br />You can review the quantity details on the Release Held RDQ page.", webPickSpreadsheet.validRDQs.Count.ToString());
            return Json(new { successMessage = message }, "application/json");
        }

        public ActionResult GetErrors()
        {
            Workbook excelDocument;
            WebPickSpreadsheet webPickSpreadsheet = new WebPickSpreadsheet(appConfig, webPickRoles, configService);
            
            var errorList = (List<Tuple<RDQ, string>>)Session["errorList"];
            excelDocument = webPickSpreadsheet.GetErrors(errorList);

            if (!string.IsNullOrEmpty(webPickSpreadsheet.message))
                return RedirectToAction("ExcelUpload", new { webPickSpreadsheet.message });

            excelDocument.Save(System.Web.HttpContext.Current.Response, "WebPicks.xlsx", ContentDisposition.Attachment, webPickSpreadsheet.SaveOptions);
            
            return View();
        }
    }
}
