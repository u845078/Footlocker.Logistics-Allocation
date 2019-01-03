using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Models.Services;
using System.IO;
using Aspose.Excel;
using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Common;
using System.Web.Script.Serialization;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Div Logistics,Director of Allocation,Admin,Support")]
    public class WebPickController : AppController
    {
        //
        // GET: /WebPick/
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        public ActionResult Index(string message)
        {
            List<Division> divs = Divisions();
            List<RDQ> list = (from a in db.RDQs
                              where a.Type == "user"
                              select a).ToList();
            list = (from a in list
                    join b in divs 
                      on a.Division equals b.DivCode
                    select a).ToList();

            if (list.Count > 0)
            {
                List<string> uniqueNames = (from l in list
                                            where l.CreatedBy.Contains("CORP")
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

            ViewData["message"] = message;

            return View(list);
        }

        public ActionResult BulkAdmin(string message)
        {
            BulkRDQModel model = new BulkRDQModel();

            InitializeDivisions(model);
            InitializeDepartmets(model, false);
            ViewData["message"] = message;
            ViewData["ruleSetID"] = model.RuleSetID;
            ViewData["ruleType"] = "rdq";
            Session["searchresult"] = -1;

            return View(model);
        }

        [HttpPost]
        public ActionResult BulkAdmin(BulkRDQModel model)
        {
            ViewData["ruleSetID"] = model.RuleSetID;
            ViewData["ruleType"] = "rdq";
            InitializeDivisions(model);
            InitializeDepartmets(model, false);
            model.HaveResults = true;

            if (model.ShowStoreSelector == "yes")
            {
                if (model.RuleSetID < 1)
                {
                    //get a new ruleset
                    RuleSet rs = new RuleSet();
                    rs.Type = "rdq";
                    rs.CreateDate = DateTime.Now;
                    rs.CreatedBy = UserName;
                    db.RuleSets.Add(rs);
                    db.SaveChanges();

                    model.RuleSetID = rs.RuleSetID;

                }

                ViewData["ruleSetID"] = model.RuleSetID;
                return View(model);
            }

            Session["searchresult"] = -1;

            return View(model);
        }

        [HttpPost]
        public ActionResult RefreshDivisions(BulkRDQModel model)
        {
            InitializeDivisions(model);
            InitializeDepartmets(model, true);
            return View("BulkAdmin", model);
        }

        [HttpPost]
        public ActionResult RefreshDepartments(BulkRDQModel model)
        {
            InitializeDivisions(model);
            InitializeDepartmets(model, false);
            return View("BulkAdmin", model);
        }

        private void InitializeDivisions(BulkRDQModel model)
        {
            //model.Instances = new List<Instance>();
            //model.Divisions = new List<Division>();
            //model.Departments = new List<Department>();
            //model.StatusList = new List<string>();

            List<Division> divs = Divisions();
            var allInstances = (from a in db.Instances join b in db.InstanceDivisions on a.ID equals b.InstanceID select new { instance = a, Division = b.Division }).ToList();
            model.Instances = (from a in allInstances join b in divs on a.Division equals b.DivCode select a.instance).Distinct().ToList();

            if (model.Instances.Any())
            {
                //if no selected instance, default to first one in the list
                if (model.Instance == 0)
                    model.Instance = model.Instances.First().ID;

                model.Divisions =
                   (from a in allInstances
                    join b in divs on a.Division equals b.DivCode
                    where a.instance.ID == model.Instance
                    select b).ToList();
            }
            else
            {
                Instance instance = new Instance();
                instance.ID = -1;
                instance.Name = "No division permissions enabled";
                model.Instances.Insert(0, instance);
            }

            model.StatusList = (from a in db.RDQs select a.Status).Distinct().ToList();
            model.StatusList.Sort();
            model.StatusList.Insert(0, "All");
        }

        private void InitializeDepartmets(BulkRDQModel model, bool resetDivision)
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

                model.Departments = WebSecurityService.ListUserDepartments(UserName, "Allocation", model.Division);
                if (model.Departments.Any())
                {
                    Department dept = new Department();
                    dept.DeptNumber = "00";
                    dept.DepartmentName = "All departments";
                    model.Departments.Insert(0, dept);
                }
                else
                {
                    Department dept = new Department();
                    dept.DeptNumber = "-1";
                    dept.DepartmentName = "No department permissions enabled";
                    model.Departments.Insert(0, dept);
                }
            }
        }

        [HttpPost]
        public ActionResult ReleaseAll(BulkRDQModel model)
        {
            List<RDQ> rdqsToRelease = GetRDQsForSession(model.Instance, model.Division, model.Department, model.Category, model.Sku, model.Status, model.PO, model.Store, model.RuleSetID);

            rdqsToRelease = (from a in rdqsToRelease where ((a.Status.StartsWith("HOLD")) && (a.Status != "HOLD-XDC")) select a).ToList();

            ////they are releasing the RDQ, so we'll make it a user RDQ so that the hold won't apply and it will be picked the next pick day
            ////first, find the RDQ being held.
            rdqsToRelease.ToList().ForEach(r =>
            {
                //now update it to a user RDQ, so that it will go down with the next batch
                //set to HOLD-REL so that it will pick on the stores next pick day
                RDQ rdq = (from a in db.RDQs where a.ID == r.ID select a).First();
                rdq.Status = "HOLD-REL";
                rdq.CreateDate = DateTime.Now;
                rdq.CreatedBy = User.Identity.Name;

                if ((rdq.PO != null) && (rdq.PO != "") && (rdq.PO != "N/A") && (rdq.Size.Length == 5))
                {
                    rdq.DestinationType = "CROSSDOCK";
                }
                else
                {
                    rdq.DestinationType = "WAREHOUSE";
                }
            });

            // Persist changes
            db.SaveChanges(User.Identity.Name);

            Session["searchresult"] = -1;
            InitializeDivisions(model);
            InitializeDepartmets(model, false);
            return View("BulkAdmin", model);
        }

        [HttpPost]
        public ActionResult ReleaseAllToWarehouse(BulkRDQModel model)
        {
            List<RDQ> rdqsToRelease = GetRDQsForSession(model.Instance, model.Division, model.Department, model.Category, model.Sku, model.Status, model.PO, model.Store, model.RuleSetID);
            rdqsToRelease.ToList().ForEach(r => 
                {
                    RDQ rdq = (from a in db.RDQs where a.ID == r.ID select a).First();
                    db.RDQs.Remove(rdq);                    
                });

            // Persist changes
            db.SaveChanges(User.Identity.Name);

            Session["searchresult"] = -1;
            InitializeDivisions(model);
            InitializeDepartmets(model, false);
            return View("BulkAdmin", model);
        }



        [GridAction]
        public ActionResult _BulkRDQs(int instance, string div, string department, string category, string sku, string status, string po, string store, Int64 ruleset)
        {
            List<RDQ> model = GetRDQsForSession(instance, div, department, category, sku, status, po, store, ruleset);

            var rdqGroups =
            from rdq in model
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
            return View(new GridModel(rdqGroups.OrderBy(g => g.Division)
                    .ThenBy(g => g.Store)
                    .ThenBy(g => g.WarehouseName)
                    .ThenBy(g => g.Category)
                    .ThenBy(g => g.Sku)
                    .ThenBy(g => g.Status)
                    .ToList()));
        }

        [GridAction]
        public ActionResult _RDQDetails(string div, string store, string warehousename, string sku, string status)
        {
            List<RDQ> model = (List<RDQ>)Session["searchresultlist"];

            model = (from a in model where ((a.Division == div)
                         &&(a.Store == store)
                         &&(a.WarehouseName == warehousename)
                         &&(a.Sku == sku)
                         &&(a.Status == status)
                         ) select a).ToList();
            return View(new GridModel(model.ToList()));
        }


        private List<RDQ> GetRDQsForSession(int instance, string div, string department, string category, string sku, string status, string po, string store, Int64 ruleset)
        {
            List<RDQ> model;

            if ((Session["searchresult"] != null) &&
                ((Int32)Session["searchresult"] > 0))
            {
                model = (List<RDQ>)Session["searchresultlist"];
            }
            else
            {
                List<RDQ> list = new List<RDQ>();

                if (department == "00")
                {
                    List<Department> depts = WebSecurityService.ListUserDepartments(UserName, "Allocation", div);

                    var templist = (from a in db.RDQs
                            join b in db.InstanceDivisions on a.Division equals b.Division
                            join c in db.ItemMasters on a.ItemID equals c.ID
							join p in db.ItemPacks on new { ItemID = (long)a.ItemID, Size = a.Size } equals new { ItemID = p.ItemID, Size = p.Name } into itempacks
							from p in itempacks.DefaultIfEmpty()
					where ((b.InstanceID == instance)
                            && (c.Div == div)
                            && ((c.Category == category) || (category == null))
                            && ((a.Sku == sku) || (sku == null))
                            && ((a.PO == po) || (po == null))
                            && ((a.Store == store) || (store == null))
                            && ((a.Status == status) || (status == "All"))
                            )
					select new { rdq = a, dept = c.Dept, UnitQty = p == null ? a.Qty : p.TotalQty * a.Qty } ).ToList();

					//update unitqty
					foreach (var item in templist)
					{
						item.rdq.UnitQty = item.UnitQty;
					}

					//filter by valid departments
					list = (from a in templist
							join d in depts on a.dept equals d.DeptNumber
							select a.rdq).ToList();
				}
                else
                {
					var templist = (from a in db.RDQs
                            join b in db.InstanceDivisions on a.Division equals b.Division
                            join c in db.ItemMasters on a.ItemID equals c.ID
							join p in db.ItemPacks on new { ItemID = (long)a.ItemID, Size = a.Size } equals new { ItemID = p.ItemID, Size = p.Name } into itempacks
							from p in itempacks.DefaultIfEmpty()
							where ((b.InstanceID == instance)
                            && (c.Div == div)
                            && (c.Dept == department)
                            && ((c.Category == category) || (category == null))
                            && ((a.Sku == sku) || (sku == null))
                            && ((a.PO == po) || (po == null))
                            && ((a.Store == store) || (store == null))
                            && ((a.Status == status) || (status == "All"))
                            )
							select new { rdq = a, UnitQty = p == null ? a.Qty : p.TotalQty * a.Qty }).ToList();

					//update unitqty
					foreach (var item in templist)
					{
						item.rdq.UnitQty = item.UnitQty;
					}

					list = templist.Select(m => m.rdq).ToList();
				}

				RuleDAO dao = new RuleDAO();
                if (ruleset > 0)
                {
                    List<StoreLookup> stores = dao.GetStoresInRuleSet(ruleset);
                    list = (from a in list join b in stores on new { a.Division, a.Store } equals new { b.Division, b.Store } select a).ToList();
                }

				model = list.Select(x => new RDQ(x)).ToList();

				Session["searchresult"] = 1;
                Session["searchresultlist"] = model;
            }
            return model;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _BulkPick([Bind(Prefix = "updated")]IEnumerable<RDQ> updated)
        {
            int instanceid = 0;
            Int64 itemid = 0;
            RDQ updateRDQ;
            foreach (RDQ r in updated)
            {
                if (r.Release)
                {
                    //need to pick this
                    updateRDQ = (from a in db.RDQs where a.ID == r.ID select a).First();
                    updateRDQ.Status = "HOLD-REL";
                }
            }
            db.SaveChanges(UserName);

            List<Division> divs = Divisions();
            List<RDQ> list = (from a in db.RDQs select a).ToList();
            list = (from a in list join b in divs on a.Division equals b.DivCode select a).ToList();
            return View(new GridModel(list));

        }

        [HttpPost]
        public ActionResult ReleaseRDQGroupToWarehouse(RDQGroup rdqGroup)
        {
            // Get all RDQs of specified SKU for specified hold
            var dao = new RDQDAO();
            if ((Session["searchresult"] != null) &&
                ((Int32)Session["searchresult"] > 0))
            {
                var holdRDQs = (List<RDQ>)Session["searchresultlist"];
                var groupRDQs = holdRDQs.Where(rdq =>
                    rdq.Division == rdqGroup.Division
                    && rdq.Store == rdqGroup.Store
                    && rdq.WarehouseName == rdqGroup.WarehouseName
                    && rdq.Category == rdqGroup.Category
                    && rdq.Sku == rdqGroup.Sku
                    && rdq.Status == rdqGroup.Status
                    )
                    .ToList();

                //performance was really bad via entity framework, we'll just run a quick stored proc and update records in memory
                dao.DeleteRDQs(groupRDQs, UserName);

                groupRDQs.ForEach(rdq => holdRDQs.Remove(rdq));

                /*                using (db)
                                {
                                    var rdqsToRelease = db.RDQs.ToList().Where(r => groupRDQs.Any(g => g.ID == r.ID));

                                    //to release back to the warehouse all we need to do is delete it
                                    //the result will be that we no longer decrease the inventory we send to Q, 
                                    //so it will see more available to allocate to whoever it would like
                                    rdqsToRelease.ToList().ForEach(rdq => 
                                        {
                                            db.RDQs.Remove(rdq);
                                        });

                                    groupRDQs.ForEach(rdq => holdRDQs.Remove(rdq));

                                    // Persist changes
                                    db.SaveChanges(User.Identity.Name);
                                }
                                */
                Session["searchresultlist"] = holdRDQs;
                // Return JSON representing Success
                return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
            }
            else
            {
                Session["searchresult"] = -1;
                return new JsonResult() { Data = new JsonResultData(ActionResultCode.SystemError) };
            }
        }

        [HttpPost]
        public ActionResult ReleaseRDQGroup(RDQGroup rdqGroup, string status)
        {
            //FLLogger log = new FLLogger("c:\\log\\alloc_timing.log");
            //log.Log("starting releaseRDQGroup", FLLogger.eLogMessageType.eInfo);
            if ((Session["searchresult"] != null) &&
                ((Int32)Session["searchresult"] > 0))
            {
                var holdRDQs = (List<RDQ>)Session["searchresultlist"];
                //log.Log("pulled list from session", FLLogger.eLogMessageType.eInfo);
                var groupRDQs = holdRDQs.Where(rdq =>
                    rdq.Division == rdqGroup.Division
                    && rdq.Store == rdqGroup.Store
                    && rdq.WarehouseName == rdqGroup.WarehouseName
                    && rdq.Category == rdqGroup.Category
                    && rdq.Sku == rdqGroup.Sku
                    && rdq.Status == rdqGroup.Status
                    )
                    .ToList();
                //log.Log("pulled list of RDQs in group", FLLogger.eLogMessageType.eInfo);

                //performance was really bad via entity framework, we'll just run a quick stored proc and update records in memory
                RDQDAO dao = new RDQDAO();
                dao.ReleaseRDQs(groupRDQs, UserName);
                //log.Log("updated list in db", FLLogger.eLogMessageType.eInfo);

                if (status == "All")
                {
                    groupRDQs.ForEach(rdq => { rdq.Status = "HOLD-REL"; });
                }
                else
                {
                    groupRDQs.ForEach(rdq => holdRDQs.Remove(rdq));
                }
                //log.Log("updated list in session", FLLogger.eLogMessageType.eInfo);

                Session["searchresultlist"] = holdRDQs;
                //log.Log("finished", FLLogger.eLogMessageType.eInfo);
                // Return JSON representing Success
                return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
            }
            else
            {
                Session["searchresult"] = -1;
                return new JsonResult() { Data = new JsonResultData(ActionResultCode.SystemError) };
            }

        }

        [HttpPost]
        public ActionResult ReleaseRDQToWarehouse(Int64 id)
        {
            // Get all RDQs of specified SKU for specified hold
            var dao = new RDQDAO();
            if ((Session["searchresult"] != null) &&
                ((Int32)Session["searchresult"] > 0))
            {
                var holdRDQs = (List<RDQ>)Session["searchresultlist"];
                RDQ del = (from a in holdRDQs where a.ID == id select a).First();
                holdRDQs.Remove(del);

                del = (from a in db.RDQs where a.ID == id select a).First();
                db.RDQs.Remove(del);
                db.SaveChanges(UserName);
                Session["searchresultlist"] = holdRDQs;
                // Return JSON representing Success
                return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
            }
            else
            {
                Session["searchresult"] = -1;
                return new JsonResult() { Data = new JsonResultData(ActionResultCode.SystemError) };
            }
        }

        [HttpPost]
        public ActionResult ReleaseRDQ(Int64 id)
        {
            //FLLogger log = new FLLogger("c:\\log\\alloc_timing.log");
            //log.Log("starting releaseRDQGroup", FLLogger.eLogMessageType.eInfo);
            if ((Session["searchresult"] != null) &&
                ((Int32)Session["searchresult"] > 0))
            {
                var holdRDQs = (List<RDQ>)Session["searchresultlist"];
                RDQ del = (from a in holdRDQs where a.ID == id select a).First();
                del.Status = "HOLD-REL";
                db.Entry(del).State = System.Data.EntityState.Modified;
                db.SaveChanges(UserName);

                Session["searchresultlist"] = holdRDQs;
                // Return JSON representing Success
                return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
            }
            else
            {
                Session["searchresult"] = -1;
                return new JsonResult() { Data = new JsonResultData(ActionResultCode.SystemError) };
            }

        }


        public ActionResult AuditIndex()
        {
            AuditRDQModel model = new AuditRDQModel();
            model.StartDate = DateTime.Now.AddDays(-7);
            model.EndDate = DateTime.Now;
            model.list = (from a in db.AuditRDQs where ((a.PickDate >= model.StartDate) && (a.PickDate <= model.EndDate)) select a).ToList();
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
            List<AuditRDQ> list = (from a in db.AuditRDQs where ((a.PickDate >= start) && (a.PickDate <= end)) select a).ToList();

            List<DistributionCenter> dcs = (from a in db.DistributionCenters select a).ToList();
            foreach (AuditRDQ a in list)
            {
                a.WarehouseName = (from d in dcs where d.ID == a.DCID select d.Name).FirstOrDefault();
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
            model.Divisions = Divisions();
            model.DCs = (from a in db.DistributionCenters
                         where (a.Type == "BOTH" || 
                                a.Type == "BIN")
                         select a).ToList();
        }

        [HttpPost]
        public ActionResult Create(WebPickModel model)
        {
            string message = "";

            int size = (from a in db.Sizes
                        where (a.Sku == model.RDQ.Sku && 
                               a.Size == model.RDQ.Size)
                        select a).Count();

            size += (from a in db.ItemPacks
                     join b in db.ItemMasters 
                     on a.ItemID equals b.ID
                     where ((b.MerchantSku == model.RDQ.Sku) && 
                            (a.Name == model.RDQ.Size))
                     select a).Count();

            if (size == 0)
            {
                model.Message = "Size does not exist for this sku";
            }
            else if (model.RDQ.Qty <= 0)
            {
                model.Message = "Qty must be greater than zero.";
            }
            else
            {
                message = CreateRDQ(model.RDQ, true);

                if (message == "")
                {
                    //call to apply holds
                    List<RDQ> list = new List<RDQ>();
                    list.Add(model.RDQ);
                    int instance = (from a in db.InstanceDivisions
                                    where a.Division == model.RDQ.Division
                                    select a.InstanceID).First();

                    RDQDAO rdqDAO = new RDQDAO();
                    int holdcount = rdqDAO.ApplyHolds(list, instance);
                    int cancelholdcount = rdqDAO.ApplyCancelHolds(list);
                    message = "Web pick generated.  ";
                    if (cancelholdcount > 0)
                    {
                        message = message + cancelholdcount + " rejected by cancel inventory hold.  ";
                    }
                    if (holdcount > 0)
                    {
                        message = message + holdcount + " on hold.  Please go to ReleaseHeldRDQs to see held RDQs.  ";
                    }
                    return RedirectToAction("Index", new { message = message });
                }
                model.Message = message;
            }

            InitializeCreate(model);
            return View(model);
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
                    rdq.CreatedBy = User.Identity.Name;
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
                            db.SaveChanges(User.Identity.Name);
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

        private Int32 InventoryAvailableToPick(RDQ rdq)
        {
            //find maximum qty
            RDQDAO dao = new RDQDAO();
            string warehouse = (from a in db.DistributionCenters
                                where a.ID == rdq.DCID
                                select a.MFCode).FirstOrDefault();
            if (warehouse == null)
                return 0;
            Int32 QtyAvailable = dao.GetWarehouseAvailable(rdq.Sku, rdq.Size, warehouse);

            return QtyAvailable;
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
                message = "Division must be same for sku and store " + rdq.Division + " " + rdq.Sku.Substring(0, 2);
            }
            else
            {
                if (rdq.Store.Length == 5)
                {
                    if ((from a in db.vValidStores
                         where ((a.Division == rdq.Division) &&
                                (a.Store == rdq.Store))
                         select a).Count() == 0)
                    {
                        message = rdq.Division + "-" + rdq.Store + " is not a valid store.";
                    }
                }
                else if (rdq.Store.Length == 2)
                {
                    if ((from d in db.DistributionCenters
                         where d.MFCode == rdq.Store
                         select d).Count() == 0)
                    {
                        message = rdq.Store + " is not a valid warehouse code.";
                    }
                }
                else
                {
                    message = rdq.Store + " is not a valid store or warehouse code.";
                }
            }

            if (message == "")
            {
                if (validateInventory)
                {
                    Int32 qtyAvailable = InventoryAvailableToPick(rdq);
                    if (qtyAvailable < rdq.Qty)
                    {
                        message = "Not enough inventory.  Amount available (for size) is " + qtyAvailable;
                    }
                }
            }
            return message;
        }

        public ActionResult Delete(Int64 ID)
        {
            RDQ rdq = (from a in db.RDQs where a.ID == ID select a).First();

            string message = "";
            try
            {
                db.RDQs.Remove(rdq);
                db.SaveChanges(User.Identity.Name);
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
                   mySheet.Cells[row, 6].Value != null;
        }

        private void ValidateParsedRDQs(List<RDQ> parsedRDQs, List<RDQ> validRDQs, List<Tuple<RDQ, string>> errorList)
        {
            // 1) division/store combination is valid
            var uniqueDivStoreList = parsedRDQs 
                .Where(pr => pr.Store.Length == 5)
                .Select(pr => new { pr.Division, pr.Store }).Distinct().ToList();
            var invalidDivStoreList = uniqueDivStoreList.Where(ds => !db.vValidStores.Any(vs => vs.Store.Equals(ds.Store) && vs.Division.Equals(ds.Division))).ToList();

            var uniqueDestWarehouseList = parsedRDQs
                .Where(pr => pr.Store.Length == 2)
                .Select(pr => new { pr.Store }).Distinct().ToList();

            var invalidWarehouseList = uniqueDestWarehouseList.Where(dw => !db.DistributionCenters.Any(dc => dc.MFCode.Equals(dw.Store))).ToList();

            //var invalidDivStoreCombo = parsedRDQs.Where(pr => !db.vValidStores.Any(vs => vs.Division.Equals(pr.Division) && vs.Store.Equals(pr.Store))).ToList();
            parsedRDQs.Where(pr => invalidDivStoreList.Contains(new { pr.Division, pr.Store }))
                      .ToList()
                      .ForEach(r => SetErrorMessageNew(errorList, r
                          , string.Format("The division and store combination {0}-{1} is not an existing or valid combination.", r.Division, r.Store)));

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
            List<string> uniqueSkus = parsedRDQs.Select(pr => pr.Sku).Distinct().ToList();
            var invalidSkusList = uniqueSkus.Where(sku => !db.ItemMasters.Any(im => im.MerchantSku.Equals(sku))).ToList();
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
            var invalidBinSizes = uniqueBinSkuSizeList.Where(sl => !db.Sizes.Any(s => s.Sku.Equals(sl.Sku) && s.Size.Equals(sl.Size))).ToList();

            parsedRDQs.Where(pr => invalidBinSizes.Contains(new { Sku = pr.Sku, Size = pr.Size }))
                      .ToList()
                      .ForEach(r =>
                      {
                          SetErrorMessageNew(errorList, r, string.Format("The bin size {0} could not be found in our system.", r.Size));
                          parsedRDQs.Remove(r);
                      });

            // check for caselot schedules (need to rework)
            foreach (var ucs in uniqueCaselotSizeList)
            {
                var isValid = (from ip in db.ItemPacks
                               join im in db.ItemMasters
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
            var invalidRDQNoneSuppliedList = parsedRDQs.Where(pr => (pr.DC.Equals(string.Empty) || pr.DC == null) && (pr.RingFencePickStore.Equals(string.Empty) || pr.RingFencePickStore == null)).ToList();
            invalidRDQNoneSuppliedList.ForEach(r => SetErrorMessageNew(errorList, r, "You must supply either a DC or a Ring Fence Store to pick from."));

            // both supplied
            var invalidRDQBothSuppliedList = parsedRDQs.Where(pr => !(pr.DC.Equals(string.Empty) || pr.DC == null) && !(pr.RingFencePickStore.Equals(string.Empty) || pr.RingFencePickStore == null)).ToList();
            invalidRDQBothSuppliedList.ForEach(r => SetErrorMessageNew(errorList, r, "You can't supply both a DC and a RingFence Store to pick from.  It must be one or the other."));

            // retreive parsed rdqs that are not in any of the lists above
            var validSuppliedRDQs = parsedRDQs.Where(pr => !invalidRDQBothSuppliedList.Any(ir => ir.Equals(pr)) && !invalidRDQNoneSuppliedList.Any(ir => ir.Equals(pr))).ToList();

            // validate distribution center id
            List<string> uniqueDCList = validSuppliedRDQs.Select(pr => pr.DC).Where(dc => !string.IsNullOrEmpty(dc)).Distinct().ToList();
            var invalidDCList = uniqueDCList.Where(dc => !db.DistributionCenters.Any(dcs => dcs.MFCode.Equals(dc) && !dcs.Type.Equals("CROSSDOCK"))).ToList();
            parsedRDQs.Where(pr => invalidDCList.Contains(pr.DC))
                .ToList()
                .ForEach(r => SetErrorMessageNew(errorList, r, "DC is invalid or only supports crossdocking."));

            // validate the inventory for dc rdqs.  This is the final validation for dc rdqs, 
            // so if it is valid, add the rdq to the validrdqs list for later processing
            var dcRDQs = validSuppliedRDQs.Where(sr => !invalidDCList.Contains(sr.DC) &&
                                                       !string.IsNullOrEmpty(sr.DC) &&
                                                       !invalidSkusList.Contains(sr.Sku)).ToList();

            this.ValidateAvailableQuantityForDCRDQs(dcRDQs, validRDQs, errorList);

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
            RDQ returnValue = new RDQ();

            returnValue.Sku = Convert.ToString(mySheet.Cells[row, 2].Value).Trim();

            if (!string.IsNullOrEmpty(returnValue.Sku))
            {
                returnValue.Division = returnValue.Sku.Substring(0, 2);
            }

            if (!string.IsNullOrEmpty(Convert.ToString(mySheet.Cells[row, 0].Value).Trim()))
                returnValue.Store = Convert.ToString(mySheet.Cells[row, 0].Value).Trim().PadLeft(5, '0');
            else
            {
                if (!string.IsNullOrEmpty(Convert.ToString(mySheet.Cells[row, 1].Value).Trim()))
                    returnValue.Store = Convert.ToString(mySheet.Cells[row, 1].Value).Trim().PadLeft(2, '0');
            }

            returnValue.Size = Convert.ToString(mySheet.Cells[row, 3].Value).Trim();
            returnValue.Qty = Convert.ToInt32(mySheet.Cells[row, 4].Value);
            returnValue.DC = Convert.ToString(mySheet.Cells[row, 5].Value).Trim();
            returnValue.DC = (string.IsNullOrEmpty(returnValue.DC)) ? "" : returnValue.DC.PadLeft(2, '0');
            returnValue.RingFencePickStore = Convert.ToString(mySheet.Cells[row, 6].Value).Trim();
            returnValue.RingFencePickStore = (string.IsNullOrEmpty(returnValue.RingFencePickStore)) ? "" : returnValue.RingFencePickStore.PadLeft(5, '0');

            returnValue.Status = "WEB PICK";

            return returnValue;
        }

        private string SetErrorMessage(string existingErrorMessage, string newErrorMessage)
        {
            return (existingErrorMessage.Equals(string.Empty)) ? newErrorMessage : existingErrorMessage + @"<br />" + newErrorMessage;
        }

        private string IsValidUploadedRDQ(RDQ rdq)
        {
            string errorMessage = string.Empty;

            // 1) division/store combination is valid
            var validDivStoreCombo = db.vValidStores.Any(vs => vs.Division.Equals(rdq.Division) && vs.Store.Equals(rdq.Store));
            if (!validDivStoreCombo)
            {
                string divStoreErrorMessage = string.Format(
                    "The division and store combination {0}-{1} is not an existing or valid combination."
                    , rdq.Division
                    , rdq.Store);
                errorMessage = SetErrorMessage(errorMessage, divStoreErrorMessage);
            }

            // 2) sku provided is valid
            ItemMaster validSku = db.ItemMasters.Where(im => im.MerchantSku.Equals(rdq.Sku)).FirstOrDefault();
            if (validSku != null)
            {
                // check to see if sku division is equal to store division
                if (!rdq.Division.Equals(validSku.Div))
                {
                    string divSkuStoreErrorMessage = string.Format(
                        "The division for both the sku and store must be the same.  Sku Division: {0}, Store Division: {1}"
                        , validSku.Div
                        , rdq.Division);
                    errorMessage = SetErrorMessage(errorMessage, divSkuStoreErrorMessage);
                }
            }
            else
            {
                errorMessage = SetErrorMessage(errorMessage, string.Format("Sku {0} is invalid", rdq.Sku));
            }

            // 3) check to see if there was a supplied DC or RingFence
            if ((rdq.DC.Equals(string.Empty) || rdq.DC == null) && (rdq.RingFencePickStore.Equals(string.Empty) || rdq.RingFencePickStore == null))
            {
                errorMessage = SetErrorMessage(errorMessage, "You must supply either a DC or a Ring Fence Store to pick from.");
            }
            else if (!(rdq.DC.Equals(string.Empty) || rdq.DC == null) && !(rdq.RingFencePickStore.Equals(string.Empty) || rdq.RingFencePickStore == null))
            {
                errorMessage = SetErrorMessage(errorMessage, "You can't supply both a DC and a RingFence Store to pick from.  It must be one or the other.");
            }
            else
            {
                // must be dc, validate dc
                if (!(rdq.DC.Equals(string.Empty) || rdq.DC == null))
                {
                    rdq.RingFencePickStore = string.Empty;
                    int? distributionCenterID = db.DistributionCenters.Where(dc => dc.MFCode.Equals(rdq.DC) && !dc.Type.Equals("CROSSDOCK")).Select(dc => dc.ID).FirstOrDefault();
                    if (distributionCenterID != null)
                    {
                        rdq.DCID = distributionCenterID;
                    }
                    else
                    {
                        errorMessage = SetErrorMessage(errorMessage, "DC is invalid or only supports crossdocking.");
                    }

                }
                // must be ringfence, validate ringfence
                else
                {
                    // validate ringfencestore
                    validDivStoreCombo = db.vValidStores.Any(vs => vs.Division.Equals(rdq.Division) && vs.Store.Equals(rdq.RingFencePickStore));
                    if (!validDivStoreCombo)
                    {
                        string divStoreErrorMessage = string.Format(
                            "The division and ring fence store combination {0}-{1} is not an existing or valid combination."
                            , rdq.Division
                            , rdq.RingFencePickStore);
                        errorMessage = SetErrorMessage(errorMessage, divStoreErrorMessage);
                    }

                    rdq.DC = string.Empty;
                    long? ringFenceID = db.RingFences.Where(rf =>
                                                        rf.Sku.Equals(rdq.Sku) &&
                                                        rf.Division.Equals(rdq.Division) &&
                                                        rf.Store.Equals(rdq.RingFencePickStore) &&
                                                        (rf.EndDate == null || rf.EndDate >= DateTime.Now))
                                                     .Select(rf => rf.ID).FirstOrDefault();

                    if (ringFenceID != null)
                    {
                        List<RingFenceDetail> ringFenceDetails = new List<RingFenceDetail>();
                        ringFenceDetails = db.RingFenceDetails.Where(rfd =>
                                                                rfd.RingFenceID.Equals(ringFenceID ?? 0) &&
                                                                rfd.Size.Equals(rdq.Size) &&
                                                                rfd.ActiveInd.Equals("1") &&
                                                                rfd.ringFenceStatusCode.Equals("4") &&
                                                                rfd.PO.Equals(string.Empty))
                                                              .OrderBy(rfd => rfd.RingFenceID)
                                                              .ToList();
                        if (ringFenceDetails.Any())
                        {
                            if (ringFenceDetails.Any(rfd => rfd.Qty >= rdq.Qty))
                            {
                                int maxAmount = (ringFenceDetails.Max(rfd => rfd.Qty));
                                string qtyErrorMessage = string.Format("The ring fenced quantity cannot satisfy the requested distribution.  Amount available for size is {0}", maxAmount);
                                errorMessage = SetErrorMessage(errorMessage, qtyErrorMessage);
                            }
                            else
                            {
                                rdq.DCID = ringFenceDetails.First().DCID;
                            }
                        }
                        else
                        {
                            errorMessage = SetErrorMessage(errorMessage, "No active warehouse ring fences were found for the requested size, sku, and store.");
                        }
                    }
                    else
                    {
                        errorMessage = SetErrorMessage(errorMessage, "No ring fences for the SKU and pick store were found");
                    }
                }
            }


            return errorMessage;
        }

        private bool HasValidHeaderRow(Worksheet mySheet)
        {
            return
                (Convert.ToString(mySheet.Cells[0, 0].Value).Contains("Store")) &&
                (Convert.ToString(mySheet.Cells[0, 1].Value).Contains("Warehouse")) &&
                (Convert.ToString(mySheet.Cells[0, 2].Value).Contains("SKU")) &&
                (Convert.ToString(mySheet.Cells[0, 3].Value).Contains("Size")) &&
                (Convert.ToString(mySheet.Cells[0, 4].Value).Contains("Quantity")) &&
                (Convert.ToString(mySheet.Cells[0, 5].Value).Contains("Pick from DC")) &&
                (Convert.ToString(mySheet.Cells[0, 6].Value).Contains("Ring Fence Store"));
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
                            if (!this.ValidateFile(parsedRDQs, errorList, out message))
                            {
                                return Content(message);
                            }

                            ValidateParsedRDQs(parsedRDQs, validRDQs, errorList);

                            // reduce ring fence quantities for the ring fence rdqs
                            ringFenceRDQs = validRDQs.Where(vr => !string.IsNullOrEmpty(vr.RingFencePickStore)).ToList();
                            this.ReduceRingFenceQuantities(ringFenceRDQs, errorList);

                            // populate necessary properties of RDQs to save to db
                            this.PopulateRDQProps(validRDQs);
                            validRDQs.ForEach(r => db.RDQs.Add(r));
                            db.SaveChanges("");

                            // once the rdqs are saved to the db, apply holds and cancel holds
                            this.ApplyHoldAndCancelHolds(validRDQs, errorList);

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
                    }
                    catch (Exception)
                    {
                        message = "Upload failed: One or more columns has unexpected missing or invalid data.";
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
                string division = parsedRDQs.Select(pr => pr.Division).FirstOrDefault();
                // if division is null, then no sku was entered on the spreadsheet
                if (division != null)
                {
                    string userName = User.Identity.Name.Split('\\')[1];
                    // 2) check to see if user has permission for this division
                    if (!Footlocker.Common.WebSecurityService.UserHasDivision(userName, "Allocation", division))
                    {
                        errorMessage = string.Format("You do not have permission for Division {0}.", division);
                        return false;
                    }
                    else
                    {
                        // 3) check to see if the parsedRDQs has any duplicates. I don't return false
                        //    for this case because I just remove duplicates and move on with processing
                        parsedRDQs.GroupBy(pr => new { pr.Store, pr.Sku, pr.Size, pr.Qty, pr.DC, pr.RingFencePickStore })
                                  .Where(pr => pr.Count() > 1)
                                  .Select(pr => new { DuplicateRDQs = pr.ToList(), Counter = pr.Count() })
                                  .ToList().ForEach(r =>
                                  {
                                      // set error message for first duplicate and the amount of times it was found in the file
                                      SetErrorMessageNew(errorList, r.DuplicateRDQs.FirstOrDefault(), string.Format(
                                          "The following row of data was duplicated in the spreadsheet {0} times.  Please provide unique rows of data.", r.Counter));
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

        private void PopulateRDQProps(List<RDQ> validRDQs)
        {
            //unique skus
            var uniqueSkus = validRDQs.Select(r => r.Sku).Distinct().ToList();
            var uniqueItemMaster = db.ItemMasters.Where(im => uniqueSkus.Contains(im.MerchantSku)).Select(im => new { ItemID = im.ID, Sku = im.MerchantSku }).ToList();
            foreach (var r in validRDQs)
            {
                r.CreatedBy = User.Identity.Name;
                r.CreateDate = DateTime.Now;
                r.PO = "";
                r.DestinationType = "WAREHOUSE";
                r.Type = "user";
                r.ItemID = uniqueItemMaster.Where(uim => uim.Sku.Equals(r.Sku)).Select(uim => uim.ItemID).FirstOrDefault();
            }
        }

        private void ApplyHoldAndCancelHolds(List<RDQ> validRDQs, List<Tuple<RDQ, string>> errorList)
        {
            RDQDAO rdqDAO;
            // grab first division of rdq since we already validated that only one division is being used
            if (validRDQs.Count > 0)
            {
                string division = validRDQs.Select(r => r.Division).FirstOrDefault();
                if (division != null)
                {
                    rdqDAO = new RDQDAO();
                    var instance = db.InstanceDivisions.Where(id => id.Division.Equals(division)).FirstOrDefault();
                    if (instance != null)
                    {
                        
                        int holdCount = rdqDAO.ApplyHolds(validRDQs, instance.InstanceID);
                        if (holdCount > 0)
                        {
                            // insert blank rdq and error message
                            RDQ rdq = new RDQ();
                            errorList.Insert(0, Tuple.Create(rdq, string.Format("{0} on hold.  Please go to Release Held RDQs to view held RDQs.", holdCount)));
                        }
                    }

                    List<RDQ> rejectedRDQs = rdqDAO.ApplyCancelHoldsNew(validRDQs);
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
                mySheet.Cells[0, col].PutValue("Quantity (#)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Pick from DC (##)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Pick from Ring Fence Store (#####)");
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
                        if (error.Item1.Store.Length == 5)
                            mySheet.Cells[row, col].PutValue(error.Item1.Store);
                        else
                            mySheet.Cells[row, col].PutValue("");

                        col++;

                        if (error.Item1.Store.Length == 2)
                            mySheet.Cells[row, col].PutValue(error.Item1.Store);
                        else
                            mySheet.Cells[row, col].PutValue("");

                        //mySheet.Cells[row, col].PutValue(error.Item1.Store);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.Sku);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.Size);
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
                        mySheet.Cells[row, col].PutValue(error.Item2);
                        row++;
                    }

                    for (int i = 0; i < 7; i++)
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
                return RedirectToAction("ExcelUpload", new { message = message });
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
                // retrieve all available quantity for the specified combinations in one mf call
                RingFenceDAO rfDAO = new RingFenceDAO();
                List<WarehouseInventory> details = rfDAO.GetWarehouseAvailableNew(uniqueCombos);
                details = rfDAO.ReduceAvailableInventory(details);

                // rdqs that cannot be satisfied by current whse avail quantity
                var dcRDQsGroupedBySize = dcRDQs
                  .GroupBy(r => new { Division = r.Division, Sku = r.Sku, Size = r.Size, DC = r.DC }).ToList()
                  .Select(r => new { Division = r.Key.Division, Sku = r.Key.Sku, Size = r.Key.Size, DC = r.Key.DC, Quantity = r.Sum(s => s.Qty) }).ToList();


                var invalidRDQsAndAvailableQty = (  from  r in dcRDQsGroupedBySize
                                                    join  d in details on new { Sku = r.Sku, Size = r.Size, DC = r.DC }
                                                                   equals new { Sku = d.Sku, Size = d.size, DC = d.DistributionCenterID }
                                                    where r.Quantity > d.quantity
                                                   select Tuple.Create(r, d.quantity)).ToList(); 

                foreach (var r in invalidRDQsAndAvailableQty)
                {
                    var dcRDQsToDelete = dcRDQs.Where(ir => ir.Division.Equals(r.Item1.Division) &&
                                           ir.Sku.Equals(r.Item1.Sku) &&
                                           ir.Size.Equals(r.Item1.Size) &&
                                           ir.DC.Equals(r.Item1.DC)).ToList();

                    dcRDQsToDelete.ForEach(rtd =>
                    {
                        SetErrorMessageNew(errorList, rtd
                            , string.Format("Not enough inventory available for all sizes.  Available inventory: {0}", r.Item2));
                        dcRDQs.Remove(rtd);
                    });

                }

                // populate dcid and add all remaining dcRDQs to validRDQs
                List<string> uniqueDCs = dcRDQs.Select(r => r.DC).Distinct().ToList();
                var dcs = db.DistributionCenters.Where(dc => uniqueDCs.Contains(dc.MFCode)).ToList();
                foreach (var r in dcRDQs)
                {
                    // retrieve specific dc
                    var dc = dcs.Where(d => d.MFCode.Equals(r.DC)).FirstOrDefault();
                    if (dc != null)
                    {
                        r.DCID = dc.ID;
                    }
                }
                validRDQs.AddRange(dcRDQs);
            }
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
            string Division = "";
            bool validateInventory = true;
            int row = 1;
            List<RDQ> rdqErrors = new List<RDQ>();
            List<RDQ> rdqGood = new List<RDQ>();
            List<string> errorMessages = new List<string>();

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
                string mainDivision = "";

                if ((Convert.ToString(mySheet.Cells[0, 0].Value).Contains("Store")) &&
                    (Convert.ToString(mySheet.Cells[0, 1].Value).Contains("SKU")) &&
                    (Convert.ToString(mySheet.Cells[0, 2].Value).Contains("Size")) &&
                    (Convert.ToString(mySheet.Cells[0, 3].Value).Contains("Quantity")) &&
                    (Convert.ToString(mySheet.Cells[0, 4].Value).Contains("Pick from DC")) &&
                    (Convert.ToString(mySheet.Cells[0, 5].Value).Contains("Ring Fence Store"))
                    )
                {
                    if (mySheet.Cells[row, 1].Value != null)
                    {
                        Division = (Convert.ToString(mySheet.Cells[row, 1].Value)).Substring(0, 2);
                        mainDivision = Division;

                        if (!(Footlocker.Common.WebSecurityService.UserHasDivision(User.Identity.Name.Split('\\')[1], "Allocation", Division)))
                        {
                            return Content("You do not have permission to update this division.");
                        }
                    }
                    // Validate records and create lists of OK and not OK RDQs
                    RDQ rdq;
                    while (mySheet.Cells[row, 0].Value != null)
                    {
                        Division = (Convert.ToString(mySheet.Cells[row, 1].Value)).Substring(0, 2);

                        if (!(Division.Equals(mainDivision)))
                        {
                            return Content("Spreadsheet must be for one division only.");
                        }

                        //create RDQ
                        rdq = new RDQ();
                        rdq.Division = Division;
                        rdq.Store = Convert.ToString(mySheet.Cells[row, 0].Value).PadLeft(5, '0');
                        rdq.Sku = Convert.ToString(mySheet.Cells[row, 1].Value);
                        rdq.Size = Convert.ToString(mySheet.Cells[row, 2].Value).PadLeft(3, '0');
                        rdq.Qty = Convert.ToInt32(mySheet.Cells[row, 3].Value);                        
                        rdq.Status = "WEB PICK";
                        string dc = Convert.ToString(mySheet.Cells[row, 4].Value).PadLeft(2, '0');
                        string ringFenceStore = Convert.ToString(mySheet.Cells[row, 5].Value).PadLeft(5, '0');

                        string message = "";

                        if ((dc == "00") && (ringFenceStore == "00000"))
                        {
                            message = "You must supply either a DC or a Ring Fence Store to pick from";
                        }
                        else if ((dc != "00") && (ringFenceStore != "00000"))
                        {
                            rdq.RingFencePickStore = ringFenceStore;
                            rdq.DC = dc;
                            message = "You can't supply both a DC and a Ring Fence Store to pick from. It must be one or the other.";
                        }
                        else
                        {
                            if (dc != "00")
                            {
                                rdq.DC = dc;
                                rdq.RingFencePickStore = string.Empty;
                                validateInventory = true;

                                var dcQuery = from a in db.DistributionCenters
                                              where a.MFCode == dc &&
                                                    a.Type != "CROSSDOCK"
                                              select a.ID;

                                if (dcQuery.Count() == 0)
                                {
                                    message = "DC is invalid or only supports crossdocking.  ";
                                }
                                else
                                {
                                    rdq.DCID = dcQuery.First();
                                }
                            }
                            else
                            {
                                rdq.DC = string.Empty;
                                rdq.RingFencePickStore = ringFenceStore;
                                validateInventory = false;

                                var ringFence = (from r in db.RingFences
                                                 where r.Sku == rdq.Sku &&
                                                       r.Division == rdq.Division &&
                                                       r.Store == ringFenceStore &&
                                                       (r.EndDate == null ||
                                                       r.EndDate >= DateTime.Now)
                                                 select r).ToList();

                                if (ringFence.Count() == 0)
                                {
                                    message = "No ring fences for the SKU and pick store were found. ";
                                }
                                else
                                {
                                    var ringFenceDetails = (from r in ringFence
                                                            from rfd in r.ringFenceDetails                                                                  
                                                            where rfd.Size == rdq.Size &&
                                                                  rfd.ActiveInd == "1" &&
                                                                  rfd.ringFenceStatusCode == "4" &&
                                                                  rfd.PO == ""
                                                            orderby rfd.RingFenceID
                                                            select rfd).ToList();
                                                                                    
                                    if (ringFenceDetails.Count() == 0)
                                    {
                                        message = "No active warehouse ring fences were found for the requested size, SKU, and store";
                                    }
                                    else
                                    {
                                        if (ringFenceDetails.Where(d => d.Qty >= rdq.Qty).Count() == 0)
                                        {

                                            int maxAmount = (from rfd in ringFenceDetails
                                                             select rfd.Qty).Max();

                                            message = "The ring fenced quantity cannot satisfy the requested distribution. Amount available for size is " + maxAmount.ToString();
                                        }
                                        else
                                        {
                                            rdq.DCID = ringFenceDetails[0].DCID;
                                        }
                                    }
                                }
                            }
                        }

                        //Boolean pickAnyway = false;
                        //if ((Convert.ToString(mySheet.Cells[0, 9].Value).Contains("AllowNegative")))
                        //{
                        //    pickAnyway = Convert.ToBoolean(mySheet.Cells[row, 9].Value);
                        //}

                        //message = CreateRDQ(rdq, ref pickAnyway);
                        if (message == "")
                        {
                            message = CreateRDQ(rdq, validateInventory);
                        }
                        //    .Replace("(noringfence)", "  If you would like to pick anyway, put TRUE in AllowNegative column and reupload")
                        //    .Replace("(ringfence)", "  Since this item is ringfenced, you cannot pick it.");

                        if (message != "")
                        {
                            rdqErrors.Add(rdq);
                            //message = message;
                            errorMessages.Add(message);
                        }
                        else
                        {
                            if (ringFenceStore != "00000")
                            {
                                var ringFenceQuery = from rf in db.RingFences
                                                     join rfd in db.RingFenceDetails
                                                      on rf.ID equals rfd.RingFenceID
                                                     where rf.Sku == rdq.Sku &&
                                                           rf.Division == rdq.Division &&
                                                           rf.Store == ringFenceStore &&
                                                           rfd.Size == rdq.Size &&
                                                           rfd.ActiveInd == "1" &&
                                                           rfd.Qty >= rdq.Qty &&
                                                           rfd.ringFenceStatusCode == "4" &&
                                                           (rf.EndDate == null ||
                                                           rf.EndDate >= DateTime.Now)                                                    
                                                     orderby rfd.RingFenceID
                                                     select rfd;
                                var ringFenceDetailRec = ringFenceQuery.FirstOrDefault();

                                ringFenceDetailRec.Qty = ringFenceDetailRec.Qty - rdq.Qty;

                                if (ringFenceDetailRec.Qty == 0)
                                {
                                    db.RingFenceDetails.Remove(ringFenceDetailRec);
                                }
                            }
                            rdqGood.Add(rdq);
                            db.SaveChanges(User.Identity.Name);
                        }

                        row++;
                    }
                }
                else
                {
                    return Content("Incorrect header, please use template.");
                }
            }

            if (rdqGood.Count > 0)
            {
                int instance = (from a in db.InstanceDivisions
                                where a.Division == Division
                                select a.InstanceID).First();
                RDQDAO rdqDAO = new RDQDAO();
                int holdcount = rdqDAO.ApplyHolds(rdqGood, instance);

                if (holdcount > 0)
                {
                    errorMessages.Add(holdcount + " on hold.  Please go to Release Held RDQs to view held RDQs.");
                }

                int cancelholdcount = rdqDAO.ApplyCancelHolds(rdqGood);

                if (cancelholdcount > 0)
                {
                    errorMessages.Add(cancelholdcount + " rejected by cancel inventory hold.  ");
                }
            }

            if (errorMessages.Count > 0)
            {
                Session["errorList"] = rdqErrors;
                Session["errorMessageList"] = errorMessages;
                return Content(rdqErrors.Count() + " Errors on spreadsheet (" + (row - rdqErrors.Count() - 1) + " successfully uploaded)");
            }
            else
            {
                return Content("");
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

        public ActionResult GetErrors()
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
            mySheet.Cells[0, col].PutValue("Quantity (#)");
            mySheet.Cells[0, col].Style.Font.IsBold = true;
            col++;
            mySheet.Cells[0, col].PutValue("Pick from DC (##)");
            mySheet.Cells[0, col].Style.Font.IsBold = true;
            col++;
            mySheet.Cells[0, col].PutValue("Pick from Ring Fence Store (#####)");
            mySheet.Cells[0, col].Style.Font.IsBold = true;
            col++;
            mySheet.Cells[0, col].PutValue("Message");
            mySheet.Cells[0, col].Style.Font.IsBold = true;
            //col++;
            //mySheet.Cells[0, col].PutValue("AllowNegative (TRUE/FALSE)");
            //mySheet.Cells[0, col].Style.Font.IsBold = true;

            List<RDQ> errors = (List<RDQ>)Session["errorList"];
            List<string> errorMessages = (List<string>)Session["errorMessageList"];
            int row = 1;           
            if (errors != null)
            {
                foreach (RDQ rdq in errors)
                {
                    col = 0;
                    if (rdq.Store.Length == 5)
                        mySheet.Cells[row, col].PutValue(rdq.Store);
                    else
                        mySheet.Cells[row, col].PutValue("");

                    if (rdq.Store.Length == 2)
                        mySheet.Cells[row, col].PutValue(rdq.Store);
                    else
                        mySheet.Cells[row, col].PutValue("");

                    col++;
                    mySheet.Cells[row, col].PutValue(rdq.Sku);
                    col++;
                    mySheet.Cells[row, col].PutValue(rdq.Size);
                    col++;
                    mySheet.Cells[row, col].PutValue(rdq.Qty);
                    col++;
                    mySheet.Cells[row, col].PutValue(rdq.DC);
                    col++;
                    mySheet.Cells[row, col].PutValue(rdq.RingFencePickStore);
                    col++;
                    mySheet.Cells[row, col].PutValue(errorMessages[row - 1]);

                    row++;
                }
            }
            else
            {
                col = 0;
                mySheet.Cells[row, col].PutValue("Session timed out");
            }

            excelDocument.Save("WebPicks.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

    }
}
