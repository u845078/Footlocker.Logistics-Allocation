using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Aspose.Cells;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Models.Services;
using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Spreadsheets;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class ItemController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
        readonly ConfigService configService = new ConfigService();

        [CheckPermission(Roles = "Support,IT")]
        public ActionResult Lookup()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPermission(Roles = "Support,IT")]
        public ActionResult Lookup(ItemLookupModel model)
        {
            string item;
            string[] tokens;
            if (!string.IsNullOrEmpty(model.QItem))
            {
                tokens = model.QItem.Split('-');
                item = tokens[0];
                long itemid = Convert.ToInt64(item);

                model.noSizeItems = db.ItemMasters.Where(im => im.ID == itemid).ToList();
            }
            else 
                if (model.MerchantSku != null)            
                    model.noSizeItems = db.ItemMasters.Where(im => im.MerchantSku == model.MerchantSku).ToList();
            
            return View(model);
        }
        
        private bool HasEditRole()
        {
            string checkroles = "Support,Buyer Planner,Director of Allocation,Div Logistics,Head Merchandiser,IT,Logistics,Merchandiser,Space Planning";
            List<string> roles = checkroles.Split(new char[] { ',' }).ToList();
            return currentUser.HasUserRole(AppName, roles);
        }
        
        [CheckPermission(Roles = "Support,Buyer Planner,Director of Allocation,Div Logistics,Head Merchandiser,IT,Logistics,Merchandiser,Space Planning,TroubleShootingReadOnly")]        
        public ActionResult Troubleshoot(string sku)
        {
            TroubleshootModel model = new TroubleshootModel()
            {
                Warehouse = -1
            };
            
            SetDCs(model);

            ViewBag.HasEditRole = HasEditRole();

            if (sku != null)
            {
                model.Sku = sku;
                UpdateTroubleShootModel(model);
            }            

            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Troubleshoot(TroubleshootModel model)
        {
            ViewBag.HasEditRole = HasEditRole();

            UpdateTroubleShootModel(model);
            SetDCs(model);
            return View(model);
        }

        private void UpdateTroubleShootModel(TroubleshootModel model)
        {
            int instanceID;
            string div = "";

            if (model.Size == null)            
                model.Size = "";
            
            if (model.Store == null)            
                model.Store = "";

            if (!string.IsNullOrEmpty(model.Sku))
                if (model.Sku.Length >= 2)
                    div = model.Sku.Substring(0, 2);

            try
            {                               
                model.RangePlans = (from a in db.RangePlans
                                    join b in db.RangePlanDetails 
                                    on a.Id equals b.ID
                                    where a.Sku == model.Sku && 
                                    ((b.Store == model.Store) || (model.Store == ""))
                                    select a).Distinct().ToList();

                if (model.RangePlans.Count > 0)
                {
                    model.RangePlans.ForEach(rp =>
                    {
                        if (rp.UpdatedBy.Contains("CORP"))
                            rp.UpdatedBy = getFullUserNameFromDatabase(rp.UpdatedBy.Replace('\\', '/'));

                        var reInitStatus = (from a in db.ReInitializeSKUs
                                            where a.ItemID == rp.ItemID
                                            orderby a.CreateDate descending
                                            select a).FirstOrDefault();

                        if (reInitStatus != null)                        
                            rp.ReInitializeStatus = (reInitStatus.SkuExtracted) ? "SKU Extracted on " + reInitStatus.LastModifiedDate.ToShortDateString() : "Pending to be Extracted";                        
                    });
                }
            }
            catch
            {
                model.ValidItem = false;
                ViewData["message"] = "invalid item";
            }

            try
            {
                ItemMaster item = db.ItemMasters.Where(im => im.MerchantSku == model.Sku).FirstOrDefault();

                model.ItemMaster = item;
                model.Department = db.Departments.Where(d => d.instanceID == item.InstanceID &&
                                                             d.divisionCode == item.Div &&
                                                             d.departmentCode == item.Dept).FirstOrDefault();

                model.Category = db.Categories.Where(c => c.instanceID == item.InstanceID &&
                                                          c.divisionCode == item.Div &&
                                                          c.departmentCode == item.Dept &&
                                                          c.categoryCode == item.Category).FirstOrDefault();

                model.Vendor = db.Vendors.Where(v => v.InstanceID == item.InstanceID &&
                                                     v.VendorCode == item.Vendor).FirstOrDefault();

                model.BrandID = db.BrandIDs.Where(b => b.instanceID == item.InstanceID && 
                                                       b.divisionCode == item.Div && 
                                                       b.departmentCode == item.Dept && 
                                                       b.brandIDCode == item.Brand).FirstOrDefault();

                if (item.TeamCode != "000")
                {
                    model.TeamCode = db.TeamCodes.Where(t => t.InstanceID == item.InstanceID &&
                                                             t.DivisionCode == item.Div &&
                                                             t.TeamCode == item.TeamCode).FirstOrDefault();
                }

                AllocationDriverDAO dao = new AllocationDriverDAO(appConfig.EuropeDivisions, appConfig.DB2PrefixDriver);
                model.AllocationDriver = dao.GetAllocationDriverList(item.Div).Where(adl => adl.Department == item.Dept).FirstOrDefault();

                instanceID = configService.GetInstance(div);
                model.ControlDate = configService.GetControlDate(div);                

                model.CPID = configService.GetCPID(model.ItemMaster.MerchantSku);
                model.RetailPriceCurrency = configService.GetDivisionalCurrencyCode(div);
                ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);
                model.RetailPrice = itemDAO.GetLocalPrice(model.Sku);

                model.ValidItem = true;

                if (item.Category.StartsWith("99") || item.Category.Equals("098") || (Convert.ToInt32(item.ServiceCode) > 2))                
                    model.ValidItem = false;                
            }
            catch 
            {
                ViewData["message"] = "invalid item";
            }

            model.Sizes = db.Sizes.Where(s => s.Sku == model.Sku).ToList();
        }

        [CheckPermission(Roles = "Support,Buyer Planner,Director of Allocation,Div Logistics,Head Merchandiser,IT,Logistics,Merchandiser,Space Planning")]
        public ActionResult TroubleshootStore()
        {
            TroubleshootStoreModel model = new TroubleshootStoreModel()
            {
                Divisions = currentUser.GetUserDivisions(AppName)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TroubleshootStore(TroubleshootStoreModel model)
        {
            UpdateTroubleShootStoreModel(model);
            return View(model);
        }

        private void UpdateTroubleShootStoreModel(TroubleshootStoreModel model)
        {
            if (model.Division == null)            
                model.Division = "";
            
            if (model.Store == null)            
                model.Store = "";

            DateTime today = configService.GetControlDate(model.Division);            

            model.Divisions = currentUser.GetUserDivisions(AppName);

            var validStoreCount = db.vValidStores.Where(vs => vs.Division == model.Division && vs.Store == model.Store).Count();
            model.isValid = validStoreCount > 0;

            model.StoreLookup = db.StoreLookups.Where(sl => sl.Division == model.Division && sl.Store == model.Store).FirstOrDefault();

            if (model.StoreLookup == null)            
                model.Message = "Store not found";            

            model.StoreExtension = (from a in db.StoreExtensions.Include("ConceptType").Include("StrategyType").Include("CustomerType") 
                                    where a.Division == model.Division && 
                                          a.Store == model.Store
                                    select a).FirstOrDefault();

            model.StoreSeasonality = (from a in db.StoreSeasonality 
                                      join b in db.StoreSeasonalityDetails 
                                      on a.ID equals b.GroupID 
                                      where b.Division == model.Division && 
                                            b.Store == model.Store
                                      select a).FirstOrDefault();

            model.Zone = (from a in db.NetworkZones 
                          join b in db.NetworkZoneStores 
                          on a.ID equals b.ZoneID 
                          where b.Division == model.Division && 
                                b.Store == model.Store
                          select a).FirstOrDefault();
        }

        #region GridAction Methods
        [GridAction]
        public ActionResult _POOverrides(string sku)
        {
            List<ExpeditePO> model = (from a in db.ExpeditePOs
                                      where a.Sku == sku
                                      select a).ToList();

            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _POs(string sku)
        {
            string division = sku.Split('-')[0];
            ExistingPODAO dao = new ExistingPODAO(appConfig.EuropeDivisions);
            List<ExistingPO> model = dao.GetExistingPOsForSku(division, sku, false);

            List<string> overridePOsForSku = (from a in db.ExpeditePOs
                                              where a.Sku == sku
                                              select a.PO).ToList();

            foreach (string overridePO in overridePOsForSku)
            {
                bool isPresent = model.Any(po => po.PO == overridePO);
                if (!isPresent)                
                    model.AddRange(dao.GetExistingPO(division, overridePO));                
            }

            List<POStatus> poStatusCodes = db.POStatusCodes.ToList();

            foreach (ExistingPO epo in model)
            {
                List<ExpeditePO> overridePOs = db.ExpeditePOs.Where(ep => ep.Division == division && ep.PO == epo.PO).ToList();

                if (overridePOs.Count > 0)
                    epo.OverrideDate = overridePOs[0].OverrideDate;

                var codeDesc = (from d in poStatusCodes
                                where d.Code == epo.POStatusCode
                                select d.Description).FirstOrDefault();

                if (epo.POStatusCode == " ")
                    epo.POStatus = codeDesc;
                else
                    epo.POStatus = epo.POStatusCode + " - " + codeDesc;

                var blanketVal = (from p in db.POs
                                  where p.PO == epo.PO
                                  select p.BlanketPOInd).FirstOrDefault();
                epo.BlanketPOInd = blanketVal;
            }

            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _StoreInventoryBySize(string sku, string store)
        {
            StoreInventoryDAO dao = new StoreInventoryDAO(appConfig.EuropeDivisions);
            List<StoreInventory> model = dao.GetStoreInventoryBySize(sku, store);

            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _StoreInventoryForSize(string sku, string store, string packName)
        {

            StoreInventoryDAO dao = new StoreInventoryDAO(appConfig.EuropeDivisions);
            List<StoreInventory> model = dao.GetStoreInventoryForSize(sku, store, packName);

            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _WarehouseInventory(string sku, int warehouseNum)
        {
            string warehouseID;
            if (warehouseNum == -1)
                warehouseID = "-1";
            else
            {
                warehouseID = (from w in db.DistributionCenters
                               where w.ID == warehouseNum
                               select w.MFCode).FirstOrDefault().ToString();
            }

            WarehouseInventoryDAO dao = new WarehouseInventoryDAO(sku, warehouseID, appConfig.EuropeDivisions);

            List<WarehouseInventory> warehouseInventoryList = dao.GetWarehouseInventory(WarehouseInventoryDAO.InventoryListType.ListOnlyAvailableSizes);            

            return View(new GridModel(warehouseInventoryList));
        }

        [GridAction]
        public ActionResult Ajax_GetPackDetails(long itemID, string packName, int totalQuantity)
        {
            var packDetails = new List<ItemPackDetail>();

            // Get pack by item/pack name
            var pack = db.ItemPacks.Include("Details").FirstOrDefault(p => p.ItemID == itemID && p.Name == packName);
            if (pack != null)
            {
                packDetails = pack.Details.OrderBy(p => p.Size).ToList();
                foreach (ItemPackDetail det in packDetails)
                {
                    det.packAmount = totalQuantity;
                }
            }

            return View(new GridModel(packDetails));
        }

        [GridAction]
        public ActionResult _BTSGroups(string div, string store)
        {
            List<StoreBTS> model = (from a in db.StoreBTS 
                                    join b in db.StoreBTSDetails 
                                    on a.ID equals b.GroupID 
                                    where (b.Store == store) && (b.Division == div) 
                                    select a).ToList();

            return View(new GridModel(model));

        }

        [GridAction]
        public ActionResult _ItemPacks(string sku, string store)
        {
            long itemid = db.ItemMasters.Where(im => im.MerchantSku == sku)
                                        .Select(im => im.ID)
                                        .FirstOrDefault();

            List<ItemPack> model = db.ItemPacks.Where(ip => ip.ItemID == itemid).ToList();

            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _PackDetails(long packID)
        {
            List<ItemPackDetail> model = db.ItemPackDetails.Where(ipd => ipd.PackID == packID).ToList();
            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _LikeStores(string div, string store)
        {
            List<StoreAttribute> model = (from a in db.StoreAttributes where ((a.Division == div) && (a.Store == store)) select a).ToList();
            return View(new GridModel(model));

        }

        [GridAction]
        public ActionResult _StoreLeadTimes(string div, string store)
        {
            List<StoreLeadTime> model = (from a in db.StoreLeadTimes where ((a.Division == div) && (a.Store == store) && a.Active == true) select a).ToList();
            foreach (StoreLeadTime lt in model)
            {
                lt.Warehouse = (from a in db.DistributionCenters where a.ID == lt.DCID select a.Name).FirstOrDefault();
            }
            return View(new GridModel(model));

        }

        [GridAction]
        public ActionResult _StoreRingfences(string div, string store)
        {
            //lazy loading was causing invalid circular references
            db.Configuration.ProxyCreationEnabled = false;
            DateTime today = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == div select a.RunDate).FirstOrDefault();

            List<RingFence> model = (from a in db.RingFenceDetails
                                     join b in db.RingFences 
                                        on a.RingFenceID equals b.ID
                                     where ((b.Division == div) && (b.Store == store)) &&
                                           a.ActiveInd == "1"
                                     select b).Distinct().ToList();

            model = (from a in model
                     where ((a.StartDate <= today) && ((a.EndDate >= today) || (a.EndDate == null)))
                     select a).ToList();

            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _ItemRingfences(string sku, string store, string mode)
        {
            string div = sku.Substring(0, 2);
            DateTime today = (from a in db.ControlDates
                              join b in db.InstanceDivisions 
                              on a.InstanceID equals b.InstanceID
                              where b.Division == div
                              select a.RunDate).FirstOrDefault();

            //lazy loading was causing invalid circular references
            db.Configuration.ProxyCreationEnabled = false;

            List<RingFenceStatusCodes> rfStatusCodes = db.RingFenceStatusCodes.ToList();

            List<RingFenceSummary> ringFences = new List<RingFenceSummary>();

            var newRingFences = (from rf in db.RingFences
                                     join rfd in db.RingFenceDetails
                                      on rf.ID equals rfd.RingFenceID
                                     where rf.Sku == sku &&
                                           rfd.ActiveInd == "1" &&
                                           ((rf.Store == store) || (store == null) || (rf.Store == null))
                                     select new { RingFence = rf, RingFenceDetail = rfd });

            if (mode == "Current")
            {
                newRingFences = (from rf in db.RingFences
                                 join rfd in db.RingFenceDetails
                                  on rf.ID equals rfd.RingFenceID
                                 where rf.Sku == sku &&
                                       rfd.ActiveInd == "1" &&
                                    ((rf.StartDate <= today) && ((rf.EndDate >= today) || (rf.EndDate == null))) &&
                                       ((rf.Store == store) || (store == null) || (rf.Store == null))
                                 select new { RingFence = rf, RingFenceDetail = rfd });
            }

            var distinctUsers = newRingFences.GroupBy(h => h.RingFence.CreatedBy)
                .Select(group => group.FirstOrDefault()).ToList();

            foreach (var rec in distinctUsers)
            {
                string fullName = "";
                if (rec.RingFence.CreatedBy.Contains("CORP"))
                    fullName = getFullUserNameFromDatabase(rec.RingFence.CreatedBy.Replace('\\', '/'));
                else
                    fullName = getFullUserNameFromDatabase(rec.RingFence.CreatedBy);

                newRingFences.Where(r => r.RingFence.CreatedBy == rec.RingFence.CreatedBy).ToList().ForEach(x => x.RingFence.CreatedBy = fullName);
            }

            foreach (var rf in newRingFences)
            {
                RingFenceSummary newRF = new RingFenceSummary
                {
                    RingFenceID = rf.RingFence.ID,
                    Division = rf.RingFence.Division,
                    Store = rf.RingFence.Store,
                    Sku = rf.RingFence.Sku,
                    PickQuantity = rf.RingFenceDetail.Qty,
                    ItemID = rf.RingFence.ItemID,
                    Qty = rf.RingFenceDetail.Qty,
                    StartDate = rf.RingFence.StartDate,
                    EndDate = rf.RingFence.EndDate,
                    Size = rf.RingFenceDetail.Size,
                    PO = rf.RingFenceDetail.PO,
                    CreatedBy = rf.RingFence.CreatedBy,
                    CreateDate = rf.RingFence.CreateDate.Value,
                    RingFenceStatus = rfStatusCodes.Find(s => s.ringFenceStatusCode == rf.RingFenceDetail.ringFenceStatusCode)
                };
            
                ringFences.Add(newRF);
            }

            // Note: I'm doing this here to avoid having multiple active result sets in LINQ
            foreach (RingFenceSummary rf in ringFences.Where(r => r.Size.Length == 5))
            {
                int packQty = (from ip in db.ItemPacks
                                where ip.Name == rf.Size &&
                                      ip.ItemID == rf.ItemID
                                select ip.TotalQty).FirstOrDefault();
                rf.Qty *= packQty;
            }

            return View(new GridModel(ringFences));
        }

        [GridAction]
        public ActionResult _StoreRDQs(string div, string store)
        {
            //lazy loading was causing invalid circular references
            db.Configuration.ProxyCreationEnabled = false;
			List<RDQ> model = new List<RDQ>();

			var templist = (from a in db.RDQs
							join d in db.DistributionCenters on a.DCID equals d.ID
							join p in db.ItemPacks on new { ItemID = (long)a.ItemID, Size = a.Size } equals new { ItemID = p.ItemID, Size = p.Name } into itempacks
							from p in itempacks.DefaultIfEmpty()
							where
							((a.Division == div) && ((a.Store == store)) || (a.Store == null))
							select new { rdq = a, DistCenter = d.Name, UnitQty = p == null ? a.Qty : p.TotalQty * a.Qty }).ToList();

			//update unitqty
			foreach (var item in templist)
			{
				item.rdq.UnitQty = item.UnitQty;
				item.rdq.WarehouseName = item.DistCenter;
				model.Add(item.rdq);
			}

            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _ItemRDQs(string sku, string store, string orderBy)
        {
            //lazy loading was causing invalid circular references
            db.Configuration.ProxyCreationEnabled = false;

			var templist = (from a in db.RDQs
							join d in db.DistributionCenters on a.DCID equals d.ID
							join p in db.ItemPacks on new { ItemID = (long)a.ItemID, Size = a.Size } equals new { ItemID = p.ItemID, Size = p.Name } into itempacks
							from p in itempacks.DefaultIfEmpty()
							where a.Sku == sku
							select new { rdq = a, DistCenter = d.Name, UnitQty = p == null ? a.Qty : p.TotalQty * a.Qty }).ToList();

			//update unitqty
			foreach (var item in templist)
			{
				item.rdq.UnitQty = item.UnitQty;
				item.rdq.WarehouseName = item.DistCenter;
			}

			List<RDQ> model = templist.Select(m => m.rdq)
				.OrderBy(x => x.Store).ThenBy(x => x.Size).ThenBy(x => x.PO).ToList();

            if ((store != null) && (store != ""))
            {
                model = (from a in model
                         where a.Store == store
                         select a).ToList();
            }

            var distinctUsers = model.GroupBy(h => h.CreatedBy)
                 .Select(group => group.FirstOrDefault()).ToList();

            foreach (var rec in distinctUsers)
            {
                string fullName = "";
                if (rec.CreatedBy.Contains("CORP"))
                    fullName = getFullUserNameFromDatabase(rec.CreatedBy.Replace('\\', '/'));
                else
                    fullName = rec.CreatedBy;

                model.Where(r => r.CreatedBy == rec.CreatedBy).ToList().ForEach(x => x.CreatedBy = fullName);
            }

            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _StoreHolds(string div, string store)
        {
            DateTime today = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == div select a.RunDate).FirstOrDefault();

            List<Hold> model = (from a in db.Holds where ((a.Division == div) && ((a.Store == store) || (a.Store == null))) select a).ToList();
            model = (from a in model where ((a.StartDate <= today) && ((a.EndDate >= today) || (a.EndDate == null))) select a).ToList();

            return View(new GridModel(model));

        }

        [GridAction]
        public ActionResult _ItemHolds(string sku, string store)
        {
            string div = sku.Substring(0, 2);
            DateTime today = (from a in db.ControlDates
                              join b in db.InstanceDivisions 
                                on a.InstanceID equals b.InstanceID
                              where b.Division == div
                              select a.RunDate).FirstOrDefault().AddDays(2);
            
            List<Hold> holds = db.Holds.ToList();

            ItemMaster item = db.ItemMasters.Where(im => im.MerchantSku == sku).FirstOrDefault();
            AllocationDriverDAO dao = new AllocationDriverDAO(appConfig.EuropeDivisions, appConfig.DB2PrefixDriver);

            holds = (from a in holds
                     where ((a.Division == item.Div) &&
                            ((a.Store == store) || (a.Store == null) || (store == "") || (store == null)) &&
                             ((a.Level == "All") ||
                              (a.Level == "Store") ||
                              ((a.Level == "Dept") && (a.Value == item.Dept)) ||
                              ((a.Level == "Category") && (a.Value == (item.Dept + "-" + item.Category))) ||
                              ((a.Level == "VendorDept") && (a.Value == (item.Vendor + "-" + item.Dept))) ||
                              ((a.Level == "VendorDeptCategory") && 
                               (a.Value == (item.Vendor + "-" + item.Dept + "-" + item.Category))) ||
                             ((a.Level == "DivDeptBrand") && 
                              (a.Value == (item.Div + "-" + item.Dept + "-" + item.Brand))) ||
                             ((a.Level == "Sku") && (a.Value == sku)) ||
                             ((a.Level == "DeptCatBrand") && (a.Value == (item.Dept + "-" + item.Category + "-" + item.Brand))) ||
                             ((a.Level == "DeptTeam") && (a.Value == item.Dept + "-" + item.TeamCode))))
                     select a).ToList();

            IEnumerable<Hold> distinctUsers = holds
                .GroupBy(h => h.CreatedBy)
                .Select(group => group.First());

            foreach (Hold hold in distinctUsers)
            {
                string fullName = "";
                if (hold.CreatedBy.Contains("CORP"))
                    fullName = getFullUserNameFromDatabase(hold.CreatedBy.Replace('\\', '/'));
                else
                    fullName = hold.CreatedBy;

                holds.Where(h => h.CreatedBy == hold.CreatedBy).ToList().ForEach(x => x.CreatedBy = fullName);
            }

            return View(new GridModel(holds));
        }

        private void SetDCs(TroubleshootModel model)
        {
            model.AllDCs = db.DistributionCenters.ToList();
            DistributionCenter dc = new DistributionCenter
            {
                ID = -1,
                Name = "Any"
            };
            model.AllDCs.Insert(0, dc);
        }
        #endregion

        #region TroubleShoot Inventory
        [CheckPermission(Roles = "Support,Buyer Planner,Director of Allocation,Div Logistics,Head Merchandiser,IT,Logistics,Merchandiser,Space Planning")]
        public ActionResult TroubleshootInventory()
        {
            TroubleshootInventory model = new TroubleshootInventory();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TroubleshootInventory(TroubleshootInventory model)
        {
            return View(model);
        }

        [GridAction]
        public ActionResult _MainframeLinks(string sku)
        {
            MainframeDAO dao = new MainframeDAO(appConfig.EuropeDivisions);
            List<MainframeLink> model;
            model = dao.GetMainframeLinks(sku);

            return View(new GridModel(model));
        }
        
        [GridAction]
        public ActionResult _LegacyInventory(string sku)
        {
            List<LegacyInventory> model;

            if (sku != null)            
                model = db.LegacyInventory.Where(li => li.Sku == sku && li.LocationTypeCode == "W").ToList();            
            else            
                model = new List<LegacyInventory>();
            
            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _LegacyFutureInventory(string sku)
        {
            List<LegacyFutureInventory> model;
            if (sku != null)            
                model = db.LegacyFutureInventory.Where(lfi => lfi.Sku == sku).ToList();            
            else            
                model = new List<LegacyFutureInventory>();
            
            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _LegacyInventoryFinal(string sku)
        {
            List<LegacyInventory> model;
            if (sku != null)            
                model = (new LegacyInventoryDAO()).GetLegacyInventoryForSku(sku);            
            else            
                model = new List<LegacyInventory>();
            
            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _LegacyFutureInventoryFinal(string sku)
        {
            List<LegacyFutureInventory> model;
            if (sku != null)            
                model = (new LegacyFutureInventoryDAO()).GetLegacyFutureInventoryForSku(sku);            
            else            
                model = new List<LegacyFutureInventory>();            

            return View(new GridModel(model));
        }

        #endregion

        #region TroubleShoot RDQs
        [CheckPermission(Roles = "Support,Buyer Planner,Director of Allocation,Div Logistics,Head Merchandiser,IT,Logistics,Merchandiser,Space Planning")]
        public ActionResult TroubleshootRDQ(string instanceID)
        {
            TroubleshootRDQModel model = new TroubleshootRDQModel()
            {
                AvailableInstances = db.Instances.ToList()
            };

            Instance inst = new Instance()
            {
                Name = ""
            };
            
            model.AvailableInstances.Insert(0, inst);
            model.InstanceID = Convert.ToInt32(instanceID);
            model.ControlDate = configService.GetControlDate(model.InstanceID);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TroubleshootRDQ(TroubleshootRDQModel model)
        {
            model.AvailableInstances = db.Instances.ToList();
            Instance inst = new Instance()
            {
                Name = ""
            };
            
            model.AvailableInstances.Insert(0, inst);
            return View(model);
        }

        [GridAction]
        public ActionResult _TroubleshootRDQToMF(int instance, DateTime controldate)
        {
            List<RDQ> model;

            RDQDAO dao = new RDQDAO();
            if (instance > 0)
            {
                model = GetRDQExtractForDate(instance, controldate);
            }
            else
            {
                model = new List<RDQ>();
            }
            return View(new GridModel(model));
        }

        private List<RDQ> GetRDQExtractForDate(int instance, DateTime controldate)
        {
            List<RDQ> model;
            if ((Session["rdqExtract"] != null) &&
                ((string)Session["rdqdate"] == (instance + "-" + controldate.ToShortDateString())))
            {
                model = (List<RDQ>)Session["rdqExtract"];
            }
            else
            {
                RDQDAO dao = new RDQDAO();

                model = dao.GetRDQExtractForDate(instance, controldate);

                Session["rdqdate"] = instance + "-" + controldate.ToShortDateString();
                Session["rdqExtract"] = model;
            }
            return model;
        }
        #endregion  

        #region TroubleShoot RDQs for Sku
        [CheckPermission(Roles = "Support,Buyer Planner,Director of Allocation,Div Logistics,Head Merchandiser,IT,Logistics,Merchandiser,Space Planning")]
        public ActionResult TroubleshootRDQForSku(string sku)
        {
            TroubleshootRDQForSkuModel model = new TroubleshootRDQForSkuModel()
            {
                Sku = sku
            };
            
            if (!string.IsNullOrEmpty(sku))
            {
                string div = sku.Substring(0, 2);
                model.ControlDate = configService.GetControlDate(div);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TroubleshootRDQForSku(TroubleshootRDQForSkuModel model)
        {
            if (model.ControlDate == null)
            {
                string div = model.Sku.Substring(0, 2);
                model.ControlDate = configService.GetControlDate(div);
            }
            return View(model);
        }

        [GridAction]
        public ActionResult _TroubleshootRDQForSkuToMF(string sku, DateTime? controldate)
        {
            List<RDQ> model;
            RDQDAO rdqDAO = new RDQDAO();

            if (!string.IsNullOrEmpty(sku))            
                model = rdqDAO.GetRDQExtractForSkuDate(sku, controldate.Value);            
            else            
                model = new List<RDQ>();
            
            return View(new GridModel(model));
        }

		public ActionResult ExportTroubleshootRDQ(string sku, DateTime? controldate)
		{
            RDQDAO rdqDAO = new RDQDAO();
            TroubleshootRDQExport exportRDQSheet = new TroubleshootRDQExport(appConfig, rdqDAO);

            exportRDQSheet.WriteData(sku, controldate.Value);
            
            try
			{				
                exportRDQSheet.excelDocument.Save(System.Web.HttpContext.Current.Response, "RDQsToMF.xlsx", ContentDisposition.Attachment, exportRDQSheet.SaveOptions);
				return RedirectToAction("TroubleshootRDQForSku");
			}
			catch (Exception ex)
			{
				return Content(ex.Message);
			}
		}
        #endregion  

        #region Quantum Data Download
        [CheckPermission(Roles = "Support,IT, Advanced Merchandiser Processes, Head Merchandiser")]
        public ActionResult QuantumDataDownload()
        {
            QuantumDataDownloadModel model = new QuantumDataDownloadModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuantumDataDownload(QuantumDataDownloadModel model, string submitAction)
        {
            QuantumDAO dao = new QuantumDAO();

            if (submitAction == "LostSales")
            {
                LostSalesExtract lostSalesExtract = new LostSalesExtract(appConfig, dao);
                lostSalesExtract.WriteData(model.Sku);

                if (!string.IsNullOrEmpty(lostSalesExtract.errorMessage))
                {
                    ViewBag.NoDataFound = lostSalesExtract.errorMessage;
                    return View(model);
                }

                lostSalesExtract.excelDocument.Save(System.Web.HttpContext.Current.Response, model.Sku + "-LostSales.xlsx", ContentDisposition.Attachment, lostSalesExtract.SaveOptions);
            } 
            else if (submitAction == "WSMextract")
            {
                WSMExtract wsmExtract = new WSMExtract(appConfig, dao);
                wsmExtract.WriteData(model.Sku, model.includeinvalidrecords);

                if (!string.IsNullOrEmpty(wsmExtract.errorMessage))
                {
                    ViewBag.NoDataFound = wsmExtract.errorMessage;
                    return View(model);
                }

                wsmExtract.excelDocument.Save(System.Web.HttpContext.Current.Response, model.Sku + "-WSMextract.xlsx", ContentDisposition.Attachment, wsmExtract.SaveOptions);
            }
            return View(model);
        }
        #endregion
    }
}
