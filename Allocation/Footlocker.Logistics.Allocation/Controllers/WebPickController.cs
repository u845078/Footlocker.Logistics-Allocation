using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Models.Services;
using Footlocker.Logistics.Allocation.DAO;
using System.IO;
using Aspose.Excel;
using Telerik.Web.Mvc;
using System.Data;
using Footlocker.Logistics.Allocation.Common;
using System.Web.Script.Serialization;
using Footlocker.Common.Entities;
using Telerik.Web.Mvc.Infrastructure;

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
            {
                model.Instances.Insert(0, new Instance() { ID = -1, Name = "No division permissions enabled" });
            }

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
                {
                    model.Departments.Insert(0, new Department() { DeptNumber = "00", DepartmentName = "All departments" });
                }
                else
                {
                    model.Departments.Insert(0, new Department() { DeptNumber = "-1", DepartmentName = "No department permissions enabled" });
                }
            }
        }

        [HttpPost]
        public ActionResult ReleaseAll(BulkRDQModel model)
        {
            List<RDQ> rdqsToRelease = GetRDQsForSession(model.Instance, model.Division, model.Department, model.Category, model.Sku, model.Status, model.PO, model.Store, model.RuleSetID);
           
            rdqsToRelease = rdqsToRelease.Where(rtr => rtr.Status.StartsWith("HOLD") && rtr.Status != "HOLD-XDC").ToList(); 

            ////they are releasing the RDQ, so we'll make it a user RDQ so that the hold won't apply and it will be picked the next pick day
            ////first, find the RDQ being held.
            rdqsToRelease.ToList().ForEach(r =>
            {
                //now update it to a user RDQ, so that it will go down with the next batch
                //set to HOLD-REL so that it will pick on the stores next pick day
                RDQ rdq = db.RDQs.Where(x => x.ID == r.ID).FirstOrDefault();
                rdq.Status = "HOLD-REL";
                rdq.CreateDate = DateTime.Now;
                rdq.CreatedBy = currentUser.NetworkID;

                if (!string.IsNullOrEmpty(rdq.PO) && (rdq.Size.Length == 5))                
                    rdq.DestinationType = "CROSSDOCK";                
                else                
                    rdq.DestinationType = "WAREHOUSE";                
            });

            // Persist changes
            db.SaveChanges(currentUser.NetworkID);

            model.SearchResult = false;
            InitializeDivisions(model);
            InitializeDepartments(model, false);
            return View("BulkAdmin", model);
        }

        [HttpPost]
        public ActionResult ReleaseAllToWarehouse(BulkRDQModel model)
        {
            List<RDQ> rdqsToRelease = GetRDQsForSession(model.Instance, model.Division, model.Department, model.Category, model.Sku, model.Status, model.PO, model.Store, model.RuleSetID);
            rdqsToRelease.ToList().ForEach(r => 
                {
                    RDQ rdq = db.RDQs.Where(rr => rr.ID == r.ID).First();
                    db.RDQs.Remove(rdq);                    
                });

            // Persist changes
            db.SaveChanges(currentUser.NetworkID);

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
            RDQDAO rdqDAO = new RDQDAO();

            // if it's all departments
            if (department == "00")
            {
                // get the list of user departments
                //List<Department> depts = currentUser.GetUserDepartments(AppName).Where(d => d.DivCode == div).ToList();
                List<string> depts = currentUser.GetUserDivDept(AppName).Where(d => d.StartsWith(string.Format("{0}-", div))).ToList();

                rdqList = rdqDAO.GetHeldRDQs(instance, div, department, category, sku, po, store, status);

                foreach (RDQ rdq in rdqList)
                {
                    if (!currentUser.HasDivDept(AppName, div, rdq.Department))                        
                        rdqList.Remove(rdq);                                          
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
                List<StoreLookup> stores = dao.GetStoresInRuleSet(ruleset);
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
            {
                groupRDQs.ForEach(rdq => { rdq.Status = "HOLD-REL"; });
            }
            else
            {
                groupRDQs.ForEach(rdq => holdRDQs.Remove(rdq));
            }
           
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

            ItemDAO itemDAO = new ItemDAO();

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
                int instance = (from a in db.InstanceDivisions
                                where a.Division == model.RDQ.Division
                                select a.InstanceID).First();

                DateTime controlDate = (from cd in db.ControlDates
                                        where cd.InstanceID == instance
                                        select cd.RunDate).FirstOrDefault();

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
                {
                    message = string.Format("Division must be same for sku and store {0} {1}", rdq.Division, rdq.Sku.Substring(0, 2));
                }
                else
                {
                    if (rdq.Store.Length == 5)
                    {
                        if (db.vValidStores.Where(vs => vs.Division == rdq.Division && vs.Store == rdq.Store).Count() == 0)
                        {
                            message = string.Format("{0}-{1} is not a valid store.", rdq.Division, rdq.Store);
                        }
                    }
                    else if (rdq.Store.Length == 2)
                    {                       
                        if (db.DistributionCenters.Where(dc => dc.MFCode == rdq.Store).Count() == 0)
                        {
                            message = string.Format("{0} is not a valid warehouse code.", rdq.Store);
                        }
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
                                {
                                    message = string.Format("Not enough inventory. Amount available (for size) is {0}", futureInv.StockQty - inventoryReductionQty);
                                }
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
        //private string CreateRDQ(RDQ rdq, ref Boolean pickAnyway)
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
            WarehouseInventoryDAO dao = new WarehouseInventoryDAO(rdq.Sku, warehouse);
            whInventory = dao.GetWarehouseInventory(WarehouseInventoryDAO.InventoryListType.ListOnlyAvailableSizes);
            qtyAvailable = (from wi in whInventory
                            where wi.size == rdq.Size
                            select wi.availableQuantity).FirstOrDefault();

            return qtyAvailable;
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
            {
                message = string.Format("Division must be same for sku and store {0} {1}", rdq.Division, rdq.Sku.Substring(0, 2));
            }
            else
            {
                if (rdq.Store.Length == 5)
                {
                    if ((from a in db.vValidStores
                         where a.Division == rdq.Division &&
                               a.Store == rdq.Store
                         select a).Count() == 0)
                    {
                        message = string.Format("{0}-{1} is not a valid store.", rdq.Division, rdq.Store);
                    }
                }
                else if (rdq.Store.Length == 2)
                {
                    if ((from d in db.DistributionCenters
                         where d.MFCode == rdq.Store
                         select d).Count() == 0)
                    {
                        message = string.Format("{0} is not a valid warehouse code.", rdq.Store);
                    }
                }
                else
                {
                    message = string.Format("{0} is not a valid store or warehouse code.", rdq.Store);
                }
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
            return RedirectToAction("Index", new { message = message });
        }

        public ActionResult ExcelUpload(string message)
        {
            ViewData["errorMessage"] = message;
            return View();
        }

        private bool HasDataOnRow(Worksheet mySheet, int row)
        {
            return mySheet.Cells[row, 0].Value != null ||
                   mySheet.Cells[row, 1].Value != null ||
                   mySheet.Cells[row, 2].Value != null ||
                   mySheet.Cells[row, 3].Value != null ||
                   mySheet.Cells[row, 4].Value != null ||
                   mySheet.Cells[row, 5].Value != null ||
                   mySheet.Cells[row, 6].Value != null ||
                   mySheet.Cells[row, 7].Value != null ||
                   mySheet.Cells[row, 8].Value != null;
        }

        private void ValidateParsedRDQs(List<RDQ> parsedRDQs, List<RDQ> validRDQs, List<Tuple<RDQ, string>> errorList, 
                                        int instanceID, List<string> uniqueSkus, List<ItemMaster> uniqueItems)
        {
            // 1) division/store combination is valid
            var uniqueDivStoreList = parsedRDQs 
                .Where(pr => pr.Store.Length == 5)
                .Select(pr => new { pr.Division, pr.Store }).Distinct().ToList();

            var invalidDivStoreList = uniqueDivStoreList.Where(ds => !db.vValidStores.Any(vs => vs.Store.Equals(ds.Store) && 
                                                                                                vs.Division.Equals(ds.Division))).ToList();     

            var uniqueDestWarehouseList = parsedRDQs
                .Where(pr => pr.Store.Length == 2)
                .Select(pr => new { pr.Store }).Distinct().ToList();

            var invalidWarehouseList = uniqueDestWarehouseList.Where(dw => !db.DistributionCenters.Any(dc => dc.MFCode.Equals(dw.Store))).ToList();
            List<string> kafkaWarehouseList = (from dc in db.DistributionCenters
                                              where dc.TransmitRDQsToKafka
                                              select dc.MFCode).ToList();

            parsedRDQs.Where(pr => invalidDivStoreList.Contains(new { pr.Division, pr.Store }))
                      .ToList()
                      .ForEach(r => SetErrorMessageNew(errorList, r,
                          string.Format("The division and store combination {0}-{1} is not an existing or valid combination.", r.Division, r.Store)));

            parsedRDQs.Where(pr => invalidWarehouseList.Contains(new { pr.Store }))
                      .ToList()
                      .ForEach(r => SetErrorMessageNew(errorList, r, string.Format("The warehouse {0} does not exist.", r.Store)));

            // 2) quantity is greater than zero
            parsedRDQs.Where(pr => pr.Qty <= 0)
                      .ToList()
                      .ForEach(r =>
                      {
                          SetErrorMessageNew(errorList, r, "The quantity provided cannot be less than or equal to zero.");
                          parsedRDQs.Remove(r);
                      });

            // 3) sku provided is valid
            var invalidSkusList = uniqueSkus.Where(sku => !uniqueItems.Any(im => im.MerchantSku.Equals(sku) &&
                                                                                 im.InstanceID == instanceID)).ToList();
            parsedRDQs.Where(pr => invalidSkusList.Contains(pr.Sku))
                      .ToList()
                      .ForEach(r => SetErrorMessageNew(errorList, r, string.Format("Sku {0} is invalid.", r.Sku)));

            // 4) sku division is equal to store division in rdq (just use the sku's division instead of hitting db.itemmasters)
            var invalidSkuStoreDivisionList = parsedRDQs.Where(pr => !invalidSkusList.Any(isl => isl.Equals(pr)) && !pr.Division.Equals(pr.Sku.Split('-')[0])).ToList();
            foreach (var r in invalidSkuStoreDivisionList)
            {
                string invalidSkuStoreDivErrorMessage = string.Format(
                    "The division for both the sku and store must be the same.  Sku Division: {0}, Store Division: {1}."
                    , r.Division
                    , r.Sku.Split('-')[0]);
                SetErrorMessageNew(errorList, r, invalidSkuStoreDivErrorMessage);
            }

            // 5) size is the correct length (3 for size, 5 for caselot)
            var uniqueSizeList = parsedRDQs.Select(pr => pr.Size).Distinct().ToList();
            var invalidSizeList = uniqueSizeList.Where(sl => !sl.Length.Equals(3) && !sl.Length.Equals(5)).ToList();
            parsedRDQs.Where(pr => invalidSizeList.Contains(pr.Size))
                      .ToList()
                      .ForEach(r =>
                      {
                          SetErrorMessageNew(errorList, r
                          , string.Format("The size {0} has an incorrect format.", r.Size));
                          parsedRDQs.Remove(r);
                      });

            var uniqueBinSkuSizeList = parsedRDQs.Where(pr => pr.Size.Length.Equals(3)).Select(pr => new { Sku = pr.Sku, Size = pr.Size }).Distinct().ToList();
            var uniqueCaselotSizeList = parsedRDQs.Where(pr => pr.Size.Length.Equals(5)).Select(pr => new { Sku = pr.Sku, Size = pr.Size }).Distinct().ToList();
            // check for bin sizes
            var invalidBinSizes = uniqueBinSkuSizeList.Where(sl => !db.Sizes.Any(s => s.Sku.Equals(sl.Sku) && 
                                                                                      s.Size.Equals(sl.Size) &&
                                                                                      s.InstanceID == instanceID)).ToList();

            parsedRDQs.Where(pr => invalidBinSizes.Contains(new { Sku = pr.Sku, Size = pr.Size }))
                      .ToList()
                      .ForEach(r =>
                      {
                          SetErrorMessageNew(errorList, r, string.Format("The bin size {0} could not be found in our system.", r.Size));
                          parsedRDQs.Remove(r);
                      });
            
            // check for caselot schedules (need to rework)
            var uniqueItemIDs = uniqueItems.Select(ui => ui.ID).ToList();

            var itemCaseLots = (from ip in db.ItemPacks
                                where uniqueItemIDs.Contains(ip.ItemID)
                                select ip).ToList();

            foreach (var ucs in uniqueCaselotSizeList)
            {
                var isValid = (from ip in itemCaseLots
                               join im in uniqueItems
                                 on ip.ItemID equals im.ID
                               where ip.Name.Equals(ucs.Size) &&
                                     im.MerchantSku.Equals(ucs.Sku)
                              select ip).Any();
                if (!isValid)
                {
                    parsedRDQs.Where(pr => pr.Sku.Equals(ucs.Sku) && pr.Size.Equals(ucs.Size)).ToList().ForEach(r =>
                    {
                        SetErrorMessageNew(errorList, r, string.Format("The caselot schedule {0} could not be found in our system.", r.Size));
                        parsedRDQs.Remove(r);
                    });
                }
            }

            // 5) check to see if there was a supplied DC or RingFence
            // none supplied
            var invalidRDQNoneSuppliedList = parsedRDQs.Where(pr => (pr.DC.Length == 0 || pr.DC == null) && 
                                                                    (pr.RingFencePickStore.Length == 0 || pr.RingFencePickStore == null)).ToList();

            invalidRDQNoneSuppliedList.ForEach(r => SetErrorMessageNew(errorList, r, "You must supply either a DC or a Ring Fence Store to pick from."));

            // both supplied
            var invalidRDQBothSuppliedList = parsedRDQs.Where(pr => !(pr.DC.Length == 0 || pr.DC == null) && 
                                                                    !(pr.RingFencePickStore.Length == 0 || pr.RingFencePickStore == null)).ToList();

            invalidRDQBothSuppliedList.ForEach(r => SetErrorMessageNew(errorList, r, 
                "You can't supply both a DC and a RingFence Store to pick from.  It must be one or the other."));

            // Can't supply a ring fence store for E-pick
            var invalidRDQRFStoreEpickList = parsedRDQs.Where(pr => (pr.Status == "E-PICK") &&
                                                                    !(pr.RingFencePickStore.Length == 0 || pr.RingFencePickStore == null)).ToList();
            invalidRDQRFStoreEpickList.ForEach(r => SetErrorMessageNew(errorList, r,
                "You can't supply a ring fence store for an E-Pick. It can only go against the DC."));

            // Can only do E-picks for warehouses supporting Kafka feed
            var invalidRDQKafkaEpickList = parsedRDQs.Where(pr => (pr.Status == "E-PICK") &&
                                                                   !kafkaWarehouseList.Contains(pr.DC)).ToList();
            invalidRDQKafkaEpickList.ForEach(r => SetErrorMessageNew(errorList, r, "This DC does not yet support E-Picks from Allocation."));

            invalidRDQRFStoreEpickList.ForEach(r => SetErrorMessageNew(errorList, r, "You can't supply a ring fence store for an E-Pick. It can only go against the DC."));

            // retreive parsed rdqs that are not in any of the lists above
            var validSuppliedRDQs = parsedRDQs.Where(pr => !invalidRDQBothSuppliedList.Any(ir => ir.Equals(pr)) && 
                                                           !invalidRDQNoneSuppliedList.Any(ir => ir.Equals(pr)) &&
                                                           !invalidRDQRFStoreEpickList.Any(ir => ir.Equals(pr)) &&
                                                           !invalidRDQKafkaEpickList.Any(ir => ir.Equals(pr))).ToList();

            // validate distribution center id
            List<string> uniqueDCList = validSuppliedRDQs.Select(pr => pr.DC).Where(dc => !string.IsNullOrEmpty(dc)).Distinct().ToList();
            var invalidDCList = uniqueDCList.Where(dc => !db.DistributionCenters.Any(dist => dist.MFCode.Equals(dc))).ToList();
            validSuppliedRDQs.Where(pr => invalidDCList.Contains(pr.DC))
                .ToList()
                .ForEach(r => SetErrorMessageNew(errorList, r, "DC is invalid."));

            List<DistributionCenter> dcList = db.DistributionCenters.ToList();

            // make sure DC is okay for PO scenario
            var invalidDCRDQList = (from pr in validSuppliedRDQs
                                    join dc in dcList 
                                     on pr.DC equals dc.MFCode
                                   where pr.Size.Length == 3 &&
                                         dc.Type == "CROSSDOCK"
                                   select pr).ToList();

            validSuppliedRDQs.Where(pr => invalidDCRDQList.Any(ir => ir.Equals(pr)))
                .ToList()
                .ForEach(r => SetErrorMessageNew(errorList, r, "A crossdock-only DC was used for bin product"));

            errorList.ForEach(er => validSuppliedRDQs.Remove(er.Item1));

            // populate dcid and add all remaining dcRDQs to validRDQs
            List<string> uniqueDCs = validSuppliedRDQs.Select(r => r.DC).Distinct().ToList();
            var dcs = db.DistributionCenters.Where(dist => uniqueDCs.Contains(dist.MFCode)).ToList();
            foreach (var r in validSuppliedRDQs)
            {
                // retrieve specific dc
                var dc = dcs.Where(d => d.MFCode.Equals(r.DC)).FirstOrDefault();
                if (dc != null)
                {
                    r.DCID = dc.ID;
                }
            }

            // validate the inventory for dc rdqs.  This is the final validation for dc rdqs, 
            // so if it is valid, add the rdq to the validrdqs list for later processing
            var dcRDQs = validSuppliedRDQs.Where(sr => !invalidDCList.Contains(sr.DC) &&
                                                       !string.IsNullOrEmpty(sr.DC) &&
                                                       !invalidSkusList.Contains(sr.Sku) &&
                                                       string.IsNullOrEmpty(sr.PO)).ToList();

            var futureRDQs = validSuppliedRDQs.Where(sr => !invalidDCList.Contains(sr.DC) &&
                                                           !string.IsNullOrEmpty(sr.DC) &&
                                                           !invalidSkusList.Contains(sr.Sku) &&
                                                           !string.IsNullOrEmpty(sr.PO)).ToList();

            ValidateAvailableQuantityForDCRDQs(dcRDQs, validRDQs, errorList);
            ValidateFutureQuantityForDCRDQs(futureRDQs, validRDQs, errorList);

            // -------------------------------------------------------------------- Ringfence rdq validation --------------------------------------------------------------------

            // retrieve parsed rdqs that are not in any of the lists above and are ringfence picks
            var validSuppliedRingFenceList = validSuppliedRDQs.Where(pr => !string.IsNullOrEmpty(pr.RingFencePickStore)).ToList();

            // filter down to the unique rf rdqs by division, store, sku. (how we uniquely can identify a ringfence record)
            var uniqueRFCombos = validSuppliedRingFenceList.Select(rf => new { Division = rf.Division, Store = rf.RingFencePickStore, Sku = rf.Sku }).Distinct().ToList();

            // validate ringfence rdqs to ensure they have a respective ringfence
            var invalidRingFenceList = uniqueRFCombos.Where(rfc => !db.RingFences.Any(rf => rf.Division.Equals(rfc.Division) &&
                                                                                             rf.Store.Equals(rfc.Store) &&
                                                                                             rf.Sku.Equals(rfc.Sku) &&
                                                                                             (rf.EndDate == null || rf.EndDate >= DateTime.Now))).ToList();

            validSuppliedRingFenceList.Where(pr => invalidRingFenceList.Contains(new { Division = pr.Division, Store = pr.RingFencePickStore, Sku = pr.Sku }))
                .ToList()
                .ForEach(r => {
                    validSuppliedRingFenceList.Remove(r);
                    SetErrorMessageNew(errorList, r, "No ringfences for the sku and pick store were found.");
                });

            if (validSuppliedRingFenceList.Count > 0)
            {
                // check to see if there is a detail record for the rdq
                var uniqueRFBySizeCombos = validSuppliedRingFenceList.Select(rf => new { Division = rf.Division, Store = rf.RingFencePickStore, Sku = rf.Sku, Size = rf.Size }).Distinct().ToList();

                List<RingFenceDetail> ringFenceDetails = new List<RingFenceDetail>();
                foreach (var urf in uniqueRFBySizeCombos)
                {
                    var ringfenceAndDetail = (from rf in db.RingFences
                             join rfd in db.RingFenceDetails
                               on rf.ID equals rfd.RingFenceID
                             where urf.Division.Equals(rf.Division) &&
                                   urf.Sku.Equals(rf.Sku) &&
                                   urf.Store.Equals(rf.Store) &&
                                   (rf.EndDate == null || rf.EndDate >= DateTime.Now) &&
                                   urf.Size.Equals(rfd.Size) &&
                                   rfd.ringFenceStatusCode.Equals("4")  &&
                                   rfd.PO.Equals(string.Empty) 
                             select new { RingFence = rf, RingFenceDetail = rfd }).FirstOrDefault();
                    if (ringfenceAndDetail != null)
                    {
                        int sizeQuantity = validSuppliedRingFenceList.Where(vsr => vsr.Division.Equals(ringfenceAndDetail.RingFence.Division) &&
                                                                                   vsr.RingFencePickStore.Equals(ringfenceAndDetail.RingFence.Store) &&
                                                                                   vsr.Sku.Equals(ringfenceAndDetail.RingFence.Sku) &&
                                                                                   vsr.Size.Equals(ringfenceAndDetail.RingFenceDetail.Size)).Sum(vsr => vsr.Qty);

                        if (sizeQuantity > ringfenceAndDetail.RingFenceDetail.Qty)
                        {
                            validSuppliedRingFenceList.Where(vsr => vsr.Division.Equals(urf.Division) &&
                                                                vsr.RingFencePickStore.Equals(urf.Store) &&
                                                                vsr.Sku.Equals(urf.Sku) &&
                                                                vsr.Size.Equals(urf.Size))
                                                      .ToList()
                                                      .ForEach(r =>
                                                      {
                                                          validSuppliedRingFenceList.Remove(r);
                                                          SetErrorMessageNew(errorList, r, string.Format(
                                                              "The ring fence detail quantity found cannot satisfy the provided quantity. RingFence Available Quantity: {0}"
                                                              , ringfenceAndDetail.RingFenceDetail.Qty));
                                                      });
                        }
                    }
                    else
                    {
                        validSuppliedRingFenceList.Where(vsr => vsr.Division.Equals(urf.Division) &&
                                                                vsr.RingFencePickStore.Equals(urf.Store) &&
                                                                vsr.Sku.Equals(urf.Sku) &&
                                                                vsr.Size.Equals(urf.Size))
                                                  .ToList()
                                                  .ForEach(r =>
                                                  {
                                                      validSuppliedRingFenceList.Remove(r);
                                                      SetErrorMessageNew(errorList, r, "No ring fence detail record for the store and size provided was found.");
                                                  });
                    }

                }

                // add remaining ringfence rdqs to validrdqs
                validRDQs.AddRange(validSuppliedRingFenceList);
            }
        }

        private void SetErrorMessageNew(List<Tuple<RDQ, string>> errorList, RDQ errorRDQ, string newErrorMessage)
        {
            int tupleIndex = errorList.FindIndex(err => err.Item1.Equals(errorRDQ));
            if (tupleIndex > -1)
            {
                errorList[tupleIndex] = Tuple.Create(errorRDQ, string.Format("{0} {1}", errorList[tupleIndex].Item2, newErrorMessage));
            }
            else
            {
                errorList.Add(Tuple.Create(errorRDQ, newErrorMessage));
            }
        }

        private RDQ ParseUploadRow(Worksheet mySheet, int row)
        {
            RDQ returnValue = new RDQ
            {
                Sku = Convert.ToString(mySheet.Cells[row, 1].Value).Trim(),
                Size = Convert.ToString(mySheet.Cells[row, 2].Value).Trim(),
                PO = Convert.ToString(mySheet.Cells[row, 3].Value).Trim(),
                Qty = Convert.ToInt32(mySheet.Cells[row, 4].Value),
                DC = Convert.ToString(mySheet.Cells[row, 6].Value).Trim(),
                RingFencePickStore = Convert.ToString(mySheet.Cells[row, 7].Value).Trim()
            };

            returnValue.DC = (string.IsNullOrEmpty(returnValue.DC)) ? "" : returnValue.DC.PadLeft(2, '0');
            returnValue.RingFencePickStore = (string.IsNullOrEmpty(returnValue.RingFencePickStore)) ? "" : returnValue.RingFencePickStore.PadLeft(5, '0');

            if (!string.IsNullOrEmpty(returnValue.Sku))
            {
                returnValue.Division = returnValue.Sku.Substring(0, 2);
            }

            if (!string.IsNullOrEmpty(Convert.ToString(mySheet.Cells[row, 0].Value).Trim()))
                returnValue.Store = Convert.ToString(mySheet.Cells[row, 0].Value).Trim().PadLeft(5, '0');
            else
            {
                if (!string.IsNullOrEmpty(Convert.ToString(mySheet.Cells[row, 5].Value).Trim()))
                    returnValue.Store = Convert.ToString(mySheet.Cells[row, 5].Value).Trim().PadLeft(2, '0');
            }

            if (Convert.ToString(mySheet.Cells[row, 8].Value).Trim() == "Y")
                returnValue.Status = "E-PICK";
            else
                returnValue.Status = "WEB PICK";

            return returnValue;
        }

        private string SetErrorMessage(string existingErrorMessage, string newErrorMessage)
        {
            return (existingErrorMessage.Equals(string.Empty)) ? newErrorMessage : existingErrorMessage + @"<br />" + newErrorMessage;
        }

        private bool HasValidHeaderRow(Worksheet mySheet)
        {
            return
                (Convert.ToString(mySheet.Cells[0, 0].Value).Contains("Store")) &&
                (Convert.ToString(mySheet.Cells[0, 1].Value).Contains("SKU")) &&
                (Convert.ToString(mySheet.Cells[0, 2].Value).Contains("Size")) &&
                (Convert.ToString(mySheet.Cells[0, 3].Value).Contains("PO")) &&
                (Convert.ToString(mySheet.Cells[0, 4].Value).Contains("Quantity")) &&
                (Convert.ToString(mySheet.Cells[0, 5].Value).Contains("To Warehouse")) &&
                (Convert.ToString(mySheet.Cells[0, 6].Value).Contains("Pick from DC")) &&
                (Convert.ToString(mySheet.Cells[0, 7].Value).Contains("Pick from Ring Fence Store")) &&
                (Convert.ToString(mySheet.Cells[0, 8].Value).Contains("Pick Right Away?"));
        }

        public ActionResult SaveOptimized(IEnumerable<HttpPostedFileBase> attachments)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            string message = string.Empty;
            List<RDQ> parsedRDQs = new List<RDQ>(), validRDQs = new List<RDQ>(), ringFenceRDQs = new List<RDQ>();
            List<Tuple<RDQ, string>> errorList = new List<Tuple<RDQ, string>>();

            foreach (var file in attachments)
            {
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];

                if (!HasValidHeaderRow(mySheet))
                {
                    message = "Upload failed: Incorrect header - please use template.";
                    return Content(message);
                }
                else
                {
                    int row = 1;
                    try
                    {
                        // create a local list of type RDQ to store the values from the upload
                        while (HasDataOnRow(mySheet, row))
                        {
                            parsedRDQs.Add(ParseUploadRow(mySheet, row));
                            row++;
                        }

                        // continue processing only if there is at least 1 record to upload
                        if (parsedRDQs.Count > 0)
                        {
                            // file level validations - duplicates, only one division, permission for division
                            if (!ValidateFile(parsedRDQs, errorList, out message))
                            {
                                return Content(message);
                            }

                            if (parsedRDQs.Count > 0)
                            {
                                var divCode = parsedRDQs.First().Sku.Substring(0, 2);

                                var instanceID = (from id in db.InstanceDivisions
                                                  where id.Division.Equals(divCode)
                                                  select id.InstanceID).FirstOrDefault();

                                DateTime controlDate = (from cd in db.ControlDates
                                                        where cd.InstanceID == instanceID
                                                        select cd.RunDate).FirstOrDefault();

                                List<string> uniqueSkus = parsedRDQs.Select(pr => pr.Sku).Distinct().ToList();
                                List<ItemMaster> uniqueItems = (from im in db.ItemMasters
                                                                where im.InstanceID == instanceID &&
                                                                      uniqueSkus.Contains(im.MerchantSku)
                                                                select im).ToList();

                                ValidateParsedRDQs(parsedRDQs, validRDQs, errorList, instanceID, uniqueSkus, uniqueItems);

                                // reduce ring fence quantities for the ring fence rdqs
                                ringFenceRDQs = validRDQs.Where(vr => !string.IsNullOrEmpty(vr.RingFencePickStore)).ToList();
                                ReduceRingFenceQuantities(ringFenceRDQs, errorList);

                                // populate necessary properties of RDQs to save to db
                                PopulateRDQProps(validRDQs, controlDate);

                                RDQDAO rdqDAO = new RDQDAO();
                                rdqDAO.InsertRDQs(validRDQs, currentUser.NetworkID);

                                db.SaveChanges("");

                                // once the rdqs are saved to the db, apply holds and cancel holds
                                ApplyHoldAndCancelHolds(validRDQs, errorList, instanceID, divCode, uniqueItems, uniqueSkus);
                            }
                        }

                        // if errors occured, allow user to download them
                        if (errorList.Count > 0)
                        {
                            string errorMessage = string.Format(
                                "{0} errors were found and {1} lines were processed successfully. <br />You can review the quantity details on the Release Held RDQ page."
                                , errorList.Count
                                , validRDQs.Count);
                            Session["errorList"] = errorList;
                            return Content(errorMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        message = string.Format("Upload failed: One or more columns has unexpected missing or invalid data. <br /> System error message: {0}", ex.GetBaseException().Message);
                        FLLogger logger = new FLLogger("C:\\Log\\allocation");
                        logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
                        // clear out error list
                        Session["errorList"] = null;
                        return Content(message);
                    }
                }
            }
            message = string.Format("Success!  {0} lines were processed. <br />You can review the quantity details on the Release Held RDQ page.", validRDQs.Count.ToString());
            return Json(new { successMessage = message }, "application/json");
        }

        private bool ValidateFile(List<RDQ> parsedRDQs, List<Tuple<RDQ, string>> errorList, out string errorMessage)
        {
            errorMessage = "";

            // remove all records that have a null or empty sku... we grab the division from this field so there
            // cannot be any empty skus before validating the file for only one division!
            parsedRDQs.Where(pr => string.IsNullOrEmpty(pr.Sku))
                      .ToList()
                      .ForEach(r =>
                      {
                          SetErrorMessageNew(errorList, r, "Sku must be provided.");
                          parsedRDQs.Remove(r);
                      });

            // 1) check to see if only one division exists
            if (parsedRDQs.Select(pr => pr.Division).Distinct().Count() > 1)
            {
                errorMessage = "Process cancelled.  Spreadsheet may only contain one division.";
                return false;
            }
            else
            {
                bool canEPick = false;
                bool canWebPick = true;

                List<string> userRoles = currentUser.GetUserRoles(AppName);

                // if they don't have any web pick roles, take out web pick
                if (!userRoles.Intersect(webPickRoles).Any())
                    canWebPick = false;

                if (userRoles.Contains("EPick"))
                    canEPick = true;             

                string division = parsedRDQs.Select(pr => pr.Division).FirstOrDefault();
                // if division is null, then no sku was entered on the spreadsheet
                if (division != null)
                {
                    foreach (RDQ rdq in parsedRDQs)
                    {
                        if ((rdq.Status == "E-PICK") && !canEPick)
                        {
                            errorMessage = "You do not have permission to do E-Picks. Please remove the E-Pick request(s) and resubmit";
                            return false;
                        }

                        if ((rdq.Status == "WEB PICK") && !canWebPick)
                        {
                            errorMessage = "You do not have permission to do Web Picks. Please remove the Web Pick request(s) and resubmit";
                            return false;
                        }
                    }

                    // 2) check to see if user has permission for this division
                    if (!currentUser.HasDivision(AppName, division)) 
                    {
                        errorMessage = string.Format("You do not have permission for Division {0}.", division);
                        return false;
                    }
                    else
                    {
                        // 3) check to see if the parsedRDQs has any duplicates. I don't return false
                        //    for this case because I just remove duplicates and move on with processing
                        parsedRDQs.GroupBy(pr => new { pr.Store, pr.Sku, pr.Size, pr.PO, pr.Qty, pr.DC, pr.RingFencePickStore, pr.Status })
                                  .Where(pr => pr.Count() > 1)
                                  .Select(pr => new { DuplicateRDQs = pr.ToList(), Counter = pr.Count() })
                                  .ToList().ForEach(r =>
                                  {
                                      // set error message for first duplicate and the amount of times it was found in the file
                                      SetErrorMessageNew(errorList, r.DuplicateRDQs.FirstOrDefault(), 
                                          string.Format("The following row of data was duplicated in the spreadsheet {0} times.  Please provide unique rows of data.", r.Counter));
                                      // delete all instances of the duplications from the parsedRDQs list
                                      r.DuplicateRDQs.ForEach(dr => parsedRDQs.Remove(dr));
                                  });
                    }
                }
                else
                {
                    errorMessage = "Please provide a SKU in order to continue.";
                    return false;
                }
            }

            return true;
        }

        private void PopulateRDQProps(List<RDQ> validRDQs, DateTime controlDate)
        {
            List<LegacyFutureInventory> futureInventory = new List<LegacyFutureInventory>();

            //unique skus
            var uniqueSkus = validRDQs.Select(r => r.Sku).Distinct().ToList();
            var uniqueItemMaster = db.ItemMasters.Where(im => uniqueSkus.Contains(im.MerchantSku)).Select(im => new { ItemID = im.ID, Sku = im.MerchantSku }).ToList();
            var uniqueFutureCombo = validRDQs.Where(r => !string.IsNullOrEmpty(r.PO)).Select(fr => new { fr.Division, fr.Sku, fr.Size, fr.DC, fr.PO, PODiv = fr.PO + "-" + fr.Division })
                                           .Distinct()
                                           .ToList();

            foreach (var uc in uniqueFutureCombo)
            {
                futureInventory.AddRange(db.LegacyFutureInventory.Where(lfi => lfi.Division == uc.Division &&
                                                                               lfi.Sku == uc.Sku &&
                                                                               lfi.Size == uc.Size &&
                                                                               lfi.InventoryID == uc.PODiv &&
                                                                               lfi.LocNodeType == "WAREHOUSE")
                                                                 .ToList());
            }

            foreach (var r in validRDQs)
            {
                r.CreatedBy = currentUser.NetworkID;
                r.LastModifiedUser = currentUser.NetworkID;
                r.CreateDate = DateTime.Now;
                r.ItemID = uniqueItemMaster.Where(uim => uim.Sku.Equals(r.Sku)).Select(uim => uim.ItemID).FirstOrDefault();
                r.Type = "user";                

                if (r.Status == "E-PICK")
                {
                    r.TransmitControlDate = controlDate;
                    if (r.Size.Length == 3)
                        r.RecordType = "1";
                    else
                        r.RecordType = "4";
                }

                if (!string.IsNullOrEmpty(r.PO))
                {
                    LegacyFutureInventory futureInv = futureInventory.Where(fi => fi.PO == r.PO && fi.Sku == r.Sku && fi.Size == r.Size).FirstOrDefault();

                    if (futureInv.ProductNodeType == "PRODUCT_PACK")
                    {
                        r.DestinationType = "CROSSDOCK";
                        r.Status = "PICK-XDC";
                    }
                    else
                    {
                        r.DestinationType = "WAREHOUSE";
                    }                        
                }
                else
                {
                    r.PO = "";
                    r.DestinationType = "WAREHOUSE";
                }
            }
        }

        private void ApplyHoldAndCancelHolds(List<RDQ> validRDQs, List<Tuple<RDQ, string>> errorList, int instanceID, string division, 
                                             List<ItemMaster> uniqueItems, List<string> uniqueSkus)
        {
            RDQDAO rdqDAO;
            // grab first division of rdq since we already validated that only one division is being used
            if (validRDQs.Count > 0)
            {
                rdqDAO = new RDQDAO();
                        
                int holdCount = rdqDAO.ApplyHolds(validRDQs, instanceID);
                if (holdCount > 0)
                {
                    // insert blank rdq and error message
                    RDQ rdq = new RDQ();
                    errorList.Insert(0, Tuple.Create(rdq, string.Format("{0} on hold.  Please go to Release Held RDQs to view held RDQs.", holdCount)));
                }

                List<RDQ> rejectedRDQs = rdqDAO.ApplyCancelHoldsNew(validRDQs, division, uniqueItems, uniqueSkus, currentUser.NetworkID);
                if (rejectedRDQs.Count > 0)
                {
                    rejectedRDQs.ForEach(r =>
                    {
                        SetErrorMessageNew(errorList, r, "Rejected by cancel inventory hold.");
                        validRDQs.Remove(r);
                    });
                }                
            }         
        }

        public ActionResult GetErrorsNew()
        {
            var errorList = (List<Tuple<RDQ, string>>)Session["errorList"];
            if (errorList != null)
            {
                Aspose.Excel.License license = new Aspose.Excel.License();
                //Set the license 
                license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

                Excel excelDocument = new Excel();
                Worksheet mySheet = excelDocument.Worksheets[0];
                int col = 0;
                mySheet.Cells[0, col].PutValue("Store (#####)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Warehouse (##)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("SKU (##-##-#####-##)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Size Caselot (### or #####)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("PO (#)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Quantity (#)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Pick from DC (##)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Pick from Ring Fence Store (#####)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Pick Right Away? (Y/N)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Message");
                mySheet.Cells[0, col].Style.Font.IsBold = true;

                int row = 1;
                if (errorList != null && errorList.Count > 0)
                {
                    foreach (var error in errorList)
                    {
                        col = 0;
                        if (error.Item1.Store != null && error.Item1.Store.Length == 5)
                            mySheet.Cells[row, col].PutValue(error.Item1.Store);
                        else
                            mySheet.Cells[row, col].PutValue("");

                        col++;

                        if (error.Item1.Store != null && error.Item1.Store.Length == 2)
                            mySheet.Cells[row, col].PutValue(error.Item1.Store);
                        else
                            mySheet.Cells[row, col].PutValue("");

                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.Sku);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.Size);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.PO);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.Qty);
                        col++;
                        // if the DC is not populated and the Distribution Center object is populated and this is a DC RDQ
                        if (error.Item1.DC == null && error.Item1.DistributionCenter != null && string.IsNullOrEmpty(error.Item1.RingFencePickStore))
                        {
                            mySheet.Cells[row, col].PutValue(error.Item1.DistributionCenter.MFCode);
                        }
                        else
                        {
                            mySheet.Cells[row, col].PutValue(error.Item1.DC);
                        }
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.RingFencePickStore);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.Status == "WEB PICK" ? "N" : "Y");
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item2);
                        row++;
                    }

                    for (int i = 0; i < 10; i++)
                    {
                        mySheet.AutoFitColumn(i);
                    }
                }

                excelDocument.Save("WebPicks.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            }
            else
            {
                // if this message is hit that means there was an exception while processing that was not accounted for
                // check the log to see what the exception was
                var message = "An unexpected error has occured.  Please try again or contact an administrator.";
                return RedirectToAction("ExcelUpload", new { message });
            }

            return View();
        }

        private void ReduceRingFenceQuantities(List<RDQ> ringFenceRDQs, List<Tuple<RDQ, string>> errorList)
        {
            var uniqueRFBySizeCombos = ringFenceRDQs.GroupBy(rf => new { Division = rf.Division, Store = rf.RingFencePickStore, Sku = rf.Sku, Size = rf.Size })
                                                    .Select(rf => new { Division = rf.Key.Division, Store = rf.Key.Store, Sku = rf.Key.Sku, Size = rf.Key.Size, Quantity = rf.Sum(s => s.Qty) })
                                                    .Distinct().ToList();

            foreach (var urf in uniqueRFBySizeCombos)
            {
                var ringFenceDetail = (from rf in db.RingFences
                                       join rfd in db.RingFenceDetails
                                         on rf.ID equals rfd.RingFenceID
                                       where rf.Sku.Equals(urf.Sku) &&
                                             rf.Division.Equals(urf.Division) &&
                                             rf.Store.Equals(urf.Store) &&
                                             rfd.Size.Equals(urf.Size) &&
                                             rfd.ActiveInd.Equals("1") &&                                             
                                             rfd.ringFenceStatusCode.Equals("4") &&
                                             (rf.EndDate == null ||
                                              rf.EndDate >= DateTime.Now)
                                       select rfd).FirstOrDefault();

                if (ringFenceDetail != null)
                {
                    var ringFenceHeader = (from rf in db.RingFences
                                           where rf.ID == ringFenceDetail.RingFenceID
                                           select rf).FirstOrDefault();
                    
                    // this may not be the correct answer if the size is a caselot, but that's fine because it will be recalculated in the SaveChanges().
                    // this is primarily done to get the ring fence record in the update cache.
                    ringFenceHeader.Qty -= urf.Quantity;
                    ringFenceDetail.Qty -= urf.Quantity;
                    
                    if (ringFenceDetail.Qty.Equals(0))
                    {
                        db.RingFenceDetails.Remove(ringFenceDetail);
                    }

                    // populate dcid for ringfencerdqs
                    var rdqs = ringFenceRDQs.Where(rf => rf.Division.Equals(urf.Division) &&
                                                         rf.Sku.Equals(urf.Sku) &&
                                                         rf.RingFencePickStore.Equals(urf.Store) &&
                                                         rf.Size.Equals(urf.Size)).ToList();
                    foreach (var r in rdqs)
                    {
                        r.DCID = ringFenceDetail.DCID;
                    }
                }
            }
        }

        /// <summary>
        /// Validate the inventory for the DC rdqs.  If it is invalid,
        /// it will remove the rdq from dcRDQs and add it to the error list.
        /// otherwise it will add the rdq to the validRDQs list.
        /// </summary>
        /// <param name="dcRDQs">valid supplied dc rdqs that do not have an invalid sku</param>
        /// <param name="validRDQs">the list of valid rdqs to generate at the end of the process</param>
        /// <param name="errorList">error list to display problems for the uploaded rdqs</param>
        private void ValidateAvailableQuantityForDCRDQs(List<RDQ> dcRDQs, List<RDQ> validRDQs, List<Tuple<RDQ, string>> errorList)
        {
            // unique combinations to pass to mf call (Sku, Size, DC)
            List<Tuple<string, string, string>> uniqueCombos = new List<Tuple<string, string, string>>();

            // retrieve the unique combinations to be sent to mf call
            uniqueCombos = dcRDQs.Select(dr => Tuple.Create(dr.Sku, dr.Size, dr.DC)).Distinct().ToList();

            if (uniqueCombos.Count > 0)
            {
                WarehouseInventoryDAO warehouseInventoryDAO = new WarehouseInventoryDAO(null, null);
                List<WarehouseInventoryDAO.WarehouseInventoryLookup> warehouseInventoryLookups = new List<WarehouseInventoryDAO.WarehouseInventoryLookup>();
                foreach (var uc in uniqueCombos)
                {
                    warehouseInventoryLookups.Add(new WarehouseInventoryDAO.WarehouseInventoryLookup
                    {
                        SKU = uc.Item1,
                        Size = uc.Item2,
                        DCCode = uc.Item3,
                        OnHandQuantity = 0
                    });
                }
                List<WarehouseInventory> details = warehouseInventoryDAO.GetSQLWarehouseInventory(warehouseInventoryLookups);
            //    // retrieve all available quantity for the specified combinations in one mf call
                RingFenceDAO rfDAO = new RingFenceDAO();
                details = rfDAO.ReduceAvailableInventory(details);

                // rdqs that cannot be satisfied by current whse avail quantity
                var dcRDQsGroupedBySize = dcRDQs
                    .Where(r => r.Status == "WEB PICK")
                  .GroupBy(r => new { Division = r.Division, Sku = r.Sku, Size = r.Size, DC = r.DCID.Value.ToString() }).ToList()
                  .Select(r => new { Division = r.Key.Division, Sku = r.Key.Sku, Size = r.Key.Size, DC = r.Key.DC, Quantity = r.Sum(s => s.Qty) }).ToList();

                var invalidRDQsAndAvailableQty = (from r in dcRDQsGroupedBySize
                                                  join d in details on new { Sku = r.Sku, Size = r.Size, DC = r.DC }
                                                                 equals new { Sku = d.Sku, Size = d.size, DC = d.DistributionCenterID }
                                                  where r.Quantity > d.quantity
                                                  select Tuple.Create(r, d.quantity)).ToList();

                foreach (var r in invalidRDQsAndAvailableQty)
                {
                    var dcRDQsToDelete = dcRDQs.Where(ir => ir.Division.Equals(r.Item1.Division) &&
                                           ir.Sku.Equals(r.Item1.Sku) &&
                                           ir.Size.Equals(r.Item1.Size) &&
                                           ir.DCID.Value.ToString().Equals(r.Item1.DC) &&
                                           ir.Status.Equals("WEB PICK")).ToList();

                    dcRDQsToDelete.ForEach(rtd =>
                    {
                        SetErrorMessageNew(errorList, rtd
                            , string.Format("Not enough inventory available for all sizes.  Available inventory: {0}", r.Item2));
                        dcRDQs.Remove(rtd);
                    });
                }

                validRDQs.AddRange(dcRDQs);
            }
        }

        /// <summary>
        /// Validate the inventory for the future rdqs, or RDQs with a PO.  If it is invalid,
        /// it will remove the rdq from dcRDQs and add it to the error list.
        /// otherwise it will add the rdq to the validRDQs list.
        /// </summary>
        /// <param name="futureRDQs">valid PO rdqs that do not have an invalid sku</param>
        /// <param name="validRDQs">the list of valid rdqs to generate at the end of the process</param>
        /// <param name="errorList">error list to display problems for the uploaded rdqs</param>
        private void ValidateFutureQuantityForDCRDQs(List<RDQ> futureRDQs, List<RDQ> validRDQs, List<Tuple<RDQ, string>> errorList)
        {
            List<RDQ> invalidRDQs = new List<RDQ>();

            var uniqueCombos = futureRDQs.Select(fr => new { fr.Division, fr.Sku, fr.Size, fr.DC, fr.PO, PODiv = fr.PO + "-" + fr.Division })
                .Distinct()
                .ToList();                        

            if (uniqueCombos.Count > 0)
            {
                // Look for situations where the PO is not found
                var invalidPO = uniqueCombos.Where(uc => !db.LegacyFutureInventory.Any(lfi => lfi.InventoryID.Equals(uc.PODiv))).ToList();
                
                foreach (var ip in invalidPO)
                {
                    invalidRDQs.AddRange(futureRDQs.Where(fr => fr.Division == ip.Division &&
                                                          fr.Sku == ip.Sku &&
                                                          fr.Size == ip.Size &&
                                                          fr.DC == ip.DC &&
                                                          fr.PO == ip.PO).ToList());
                    uniqueCombos.Remove(ip);
                }

                invalidRDQs.ForEach(r => SetErrorMessageNew(errorList, r, "PO was not found in Future Inventory. If it was created today, try again tomorrow."));
                invalidRDQs.ForEach(r => futureRDQs.Remove(r));                
                invalidRDQs.Clear();
                invalidPO.Clear();

                invalidPO = uniqueCombos.Where(uc => !db.LegacyFutureInventory.Any(lfi => lfi.Division.Equals(uc.Division) &&
                                                                                          lfi.Sku.Equals(uc.Sku) &&
                                                                                          lfi.Size.Equals(uc.Size) &&
                                                                                          lfi.Store.Equals(uc.DC) &&
                                                                                          lfi.InventoryID.Equals(uc.PODiv))).ToList();

                foreach (var ip in invalidPO)
                {
                    invalidRDQs.AddRange(futureRDQs.Where(fr => fr.Division == ip.Division &&
                                                          fr.Sku == ip.Sku &&
                                                          fr.Size == ip.Size &&
                                                          fr.DC == ip.DC &&
                                                          fr.PO == ip.PO).ToList());
                    uniqueCombos.Remove(ip);
                }

                invalidRDQs.ForEach(r => SetErrorMessageNew(errorList, r, "Did not find any future inventory for this specific SKU/Size/DC"));
                invalidRDQs.ForEach(r => futureRDQs.Remove(r));
                invalidRDQs.Clear();
                invalidPO.Clear();

                // Look for situations where the exact key is not found
                List<LegacyFutureInventory> futureInventory = new List<LegacyFutureInventory>();

                foreach (var uc in uniqueCombos)
                {
                    futureInventory.AddRange(db.LegacyFutureInventory.AsNoTracking()
                                                                     .Where(lfi => lfi.Division == uc.Division &&
                                                                                   lfi.Sku == uc.Sku &&
                                                                                   lfi.Size == uc.Size &&
                                                                                   lfi.InventoryID == uc.PODiv &&
                                                                                   lfi.LocNodeType == "WAREHOUSE")
                                                                     .ToList());
                }

                foreach (LegacyFutureInventory fi in futureInventory)
                {
                    InventoryReductions inventoryReductions = db.InventoryReductions.Where(ir => ir.PO == fi.PO && ir.Sku == fi.Sku && ir.Size == fi.Size).FirstOrDefault();

                    if (inventoryReductions != null)
                        fi.StockQty -= inventoryReductions.Qty;
                }

                // Look for cases where RDQ quantity > reduced future inventory
                foreach (RDQ r in futureRDQs)
                {
                    int futureQty = 0;
                    LegacyFutureInventory futureInvRec = futureInventory.Where(fi => fi.PO == r.PO && fi.Sku == r.Sku && fi.Size == r.Size).FirstOrDefault();
                    
                    if (futureInvRec != null)
                        futureQty = futureInvRec.StockQty;
                    else
                        futureQty = 0;

                    if (r.Qty > futureQty)
                        invalidRDQs.Add(r);
                    else
                    {
                        // in this case, there is enough future inventory, but we are going to reduce future inventory to claim this RDQ
                        futureInvRec.StockQty -= r.Qty;
                    }
                }

                invalidRDQs.ForEach(r => SetErrorMessageNew(errorList, r, "Inventory requested is greater than remaining inventory"));
                invalidRDQs.ForEach(r => futureRDQs.Remove(r));
                invalidRDQs.Clear();

                validRDQs.AddRange(futureRDQs);
            }
        }

        public ActionResult ExcelTemplate()
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            string templateFilename = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["WebPickTemplate"]);
            FileStream file = new FileStream(Server.MapPath("~/") + templateFilename, FileMode.Open, System.IO.FileAccess.Read);

            Byte[] data1 = new Byte[file.Length];
            file.Read(data1, 0, data1.Length);
            file.Close();
            MemoryStream memoryStream1 = new MemoryStream(data1);
            excelDocument.Open(memoryStream1);
            excelDocument.Save("WebPickUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }
    }
}
