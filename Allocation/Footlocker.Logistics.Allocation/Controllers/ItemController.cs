using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aspose.Excel;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Models.Services;
using Telerik.Web.Mvc;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class ItemController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        [CheckPermission(Roles = "Support,IT")]
        public ActionResult Lookup()
        {
            return View();
        }

        [HttpPost]
        [CheckPermission(Roles = "Support,IT")]
        public ActionResult Lookup(ItemLookupModel model)
        {
            string item;
            string[] tokens;
            if (model.QItem != null)
            {
                tokens = model.QItem.Split('-');
                item = tokens[0];
                Int64 itemid = Convert.ToInt64(item);
                model.noSizeItems = (from a in db.ItemMasters where (a.ID == itemid) select a).ToList();
                try
                {
                    item = item.Substring(0, item.Length - 3);
                    itemid = Convert.ToInt64(item);
                }
                catch { }
            }
            else if (model.MerchantSku != null)
            {
                item = model.MerchantSku;

                model.noSizeItems = (from a in db.ItemMasters where (a.MerchantSku == item) select a).ToList();
            }
            return View(model);
        }


        [CheckPermission(Roles = "Support,Buyer Planner,Director of Allocation,Div Logistics,Head Merchandiser,IT,Logistics,Merchandiser,Space Planning")]
        public ActionResult Troubleshoot(string sku)
        {
            TroubleshootModel model = new TroubleshootModel();
            SetDCs(model);
            if (sku != null)
            {
                model.Sku = sku;
                UpdateTroubleShootModel(model);
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Troubleshoot(TroubleshootModel model)
        {
            UpdateTroubleShootModel(model);

            SetDCs(model);
            return View(model);
        }

        private void UpdateTroubleShootModel(TroubleshootModel model)
        {
            if (model.Size == null)
            {
                model.Size = "";
            }
            if (model.Store == null)
            {
                model.Store = "";
            }
            try
            {
                string div = model.Sku.Substring(0, 2);

                DateTime today = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == div select a.RunDate).First();

                //List<RingFence> ringFences = (from a in db.RingFences where ((a.StartDate < today) && ((a.EndDate > today) || (a.EndDate == null))) select a).ToList();


                //model.RingFences = (from a in ringFences
                //                    join b in db.RingFenceDetails on a.ID equals b.RingFenceID
                //                    where ((a.Sku == model.Sku)
                //                        //&& ((a.Size == model.Size)||(model.Size == ""))
                //                        && ((a.Store == model.Store) || (model.Store == "") || (a.Store == null))
                //                        && ((b.DCID == model.Warehouse) || (model.Warehouse == -1)))
                //                    select a).Distinct().ToList();

                model.RangePlans = (from a in db.RangePlans
                                    join b in db.RangePlanDetails on a.Id equals b.ID
                                    where
                                    (a.Sku == model.Sku)
                                    && ((b.Store == model.Store) || (model.Store == ""))
                                    select a).Distinct().ToList();

                //TODO, multisku POs aren't in here
                //model.POOverrides = (from a in db.ExpeditePOs
                //                     where
                //                         a.Sku == model.Sku
                //                     select a).ToList();
            }
            catch
            {
                model.ValidItem = false;
                ViewData["message"] = "invalid item";
            }

            try
            {
                ItemMaster item = (from a in db.ItemMasters where a.MerchantSku == model.Sku select a).First();
                model.ItemMaster = item;
                AllocationDriverDAO dao = new AllocationDriverDAO();
                model.AllocationDriver = (from a in dao.GetAllocationDriverList(item.Div) where a.Department == item.Dept select a).FirstOrDefault();
                model.ControlDate = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == item.Div select a).FirstOrDefault();

                model.ValidItem = true;
                if (item.Category.StartsWith("99"))
                {
                    model.ValidItem = false;
                }
                if (item.Category.Equals("098"))
                {
                    model.ValidItem = false;
                }
                if (Convert.ToInt32(item.ServiceCode) > 2)
                {
                    model.ValidItem = false;
                }

                //model.ItemPacks = (from a in db.ItemPacks where a.ItemID == item.ID select a).ToList();
            }
            catch (Exception ex)
            {
                ViewData["message"] = "invalid item";
                //if (ex.Message.Contains("Sequence"))
                //{
                //    ViewData["message"] = "Error getting holds, item invalid";
                //}
                //else
                //{
                //    ViewData["message"] = "Error getting holds " + ex.Message;
                //}
            }

            model.Sizes = (from a in db.Sizes where a.Sku == model.Sku select a).ToList();

            //model.RDQs = (from a in db.RDQs where a.Sku == model.Sku select a).ToList();
        }

        [CheckPermission(Roles = "Support,Buyer Planner,Director of Allocation,Div Logistics,Head Merchandiser,IT,Logistics,Merchandiser,Space Planning")]
        public ActionResult TroubleshootStore()
        {
            TroubleshootStoreModel model = new TroubleshootStoreModel();
            model.Divisions = this.Divisions();
            return View(model);
        }

        [HttpPost]
        public ActionResult TroubleshootStore(TroubleshootStoreModel model)
        {
            UpdateTroubleShootStoreModel(model);
            return View(model);
        }


        private void UpdateTroubleShootStoreModel(TroubleshootStoreModel model)
        {
            if (model.Division == null)
            {
                model.Division = "";
            }
            if (model.Store == null)
            {
                model.Store = "";
            }
            DateTime today = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == model.Division select a.RunDate).First();

            model.Divisions = this.Divisions();

            //model.BTSGroups = (from a in db.StoreBTS join b in db.StoreBTSDetails on a.ID equals b.GroupID where ((b.Store == model.Store) && (b.Division == model.Division)) select a).ToList();

            var validStoreQuery = (from a in db.vValidStores where ((a.Division == model.Division) && (a.Store == model.Store)) select a);
            model.isValid = (validStoreQuery.Count() > 0);

            //model.LikeStores = (from a in db.StoreAttributes where ((a.Division == model.Division) && (a.Store == model.Store)) select a).ToList();

            model.StoreLookup = (from a in db.StoreLookups where ((a.Division == model.Division) && (a.Store == model.Store)) select a).FirstOrDefault();

            if (model.StoreLookup == null)
            {
                model.Message = "Store not found";
            }

            model.StoreExtension = (from a in db.StoreExtensions.Include("ConceptType").Include("StrategyType").Include("CustomerType") where ((a.Division == model.Division) && (a.Store == model.Store)) select a).FirstOrDefault();

            model.StoreSeasonality = (from a in db.StoreSeasonality join b in db.StoreSeasonalityDetails on a.ID equals b.GroupID where ((b.Division == model.Division) && (b.Store == model.Store)) select a).FirstOrDefault();

            model.Zone = (from a in db.NetworkZones join b in db.NetworkZoneStores on a.ID equals b.ZoneID where ((b.Division == model.Division) && (b.Store == model.Store)) select a).FirstOrDefault();

            //model.StoreLeadTimes = (from a in db.StoreLeadTimes where ((a.Division == model.Division) && (a.Store == model.Store)) select a).ToList();
            //foreach (StoreLeadTime lt in model.StoreLeadTimes)
            //{
            //    lt.Warehouse = (from a in db.DistributionCenters where a.ID == lt.DCID select a.Name).FirstOrDefault();
            //}

            //model.RingFences = (from a in db.RingFenceDetails join b in db.RingFences on a.RingFenceID equals b.ID where ((b.Division == model.Division) && (b.Store == model.Store)) select b).ToList();
            //model.RDQs = (from a in db.RDQs where ((a.Division == model.Division) && ((a.Store == model.Store))||(a.Store == null)) select a).ToList();
        }

        #region GridAction Methods
        [GridAction]
        public ActionResult _POOverrides(string sku)
        {
            List<ExpeditePO> model = (from a in db.ExpeditePOs
                                      where
                                          a.Sku == sku
                                      select a).ToList();

            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _POs(string sku)
        {
            string division = sku.Split('-')[0];
            ExistingPODAO dao = new ExistingPODAO();
            List<ExistingPO> model = dao.GetExistingPOsForSku(division, sku, false);

            List<string> overridePOsForSku = (from a in db.ExpeditePOs
                                              where a.Sku == sku
                                              select a.PO).ToList();

            foreach (string overridePO in overridePOsForSku)
            {
                bool isPresent = model.Any(po => po.PO == overridePO);
                if (!isPresent)
                {
                    model.AddRange(dao.GetExistingPO(division, overridePO));
                }
            }

            foreach (ExistingPO epo in model)
            {
                List<ExpeditePO> overridePOs = (from a in db.ExpeditePOs
                                                where (a.Division == division) &&
                                                      (a.PO == epo.PO)
                                                select a).ToList();
                if (overridePOs.Count > 0)
                    epo.OverrideDate = overridePOs[0].OverrideDate;
            }

            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _StoreInventoryBySize(string sku, string store)
        {
            StoreInventoryDAO dao = new StoreInventoryDAO();
            List<StoreInventory> model = dao.GetStoreInventoryBySize(sku, store);

            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _StoreInventoryForSize(string sku, string store, string packName)
        {

            StoreInventoryDAO dao = new StoreInventoryDAO();
            List<StoreInventory> model = dao.GetStoreInventoryForSize(sku, store, packName);

            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _WarehouseInventory(string sku, int warehouseNum)
        {
            long itemID = (from a in db.ItemMasters
                           where a.MerchantSku.Equals(sku)
                           select a.ID).FirstOrDefault();

            string warehouseID;
            if (warehouseNum == -1)
                warehouseID = "-1";
            else
                warehouseID = (from w in db.DistributionCenters
                               where w.ID == warehouseNum
                               select w.MFCode).First().ToString();

            WarehouseInventoryDAO dao = new WarehouseInventoryDAO();

            List<WarehouseInventory> warehouseInventoryList = dao.GetWarehouseInventory(sku, warehouseID);

            return View(new GridModel(warehouseInventoryList));
        }

        [GridAction]
        public ActionResult Ajax_GetPackDetails(long itemID, string packName, int totalQuantity)
        {
            var packDetails = new List<ItemPackDetail>();

            //// Get pack's item
            //var item = db.RingFences.Single(rf => rf.ID == ringFenceID).Sku;
            //var itemID = db.ItemMasters.Single(i => i.MerchantSku == item).ID;

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
            List<StoreBTS> model = (from a in db.StoreBTS join b in db.StoreBTSDetails on a.ID equals b.GroupID where ((b.Store == store) && (b.Division == div)) select a).ToList();

            return View(new GridModel(model));

        }

        [GridAction]
        public ActionResult _ItemPacks(string sku, string store)
        {
            long itemid = (from a in db.ItemMasters where a.MerchantSku == sku select a.ID).First();

            List<ItemPack> model = (from a in db.ItemPacks where a.ItemID == itemid select a).ToList();

            return View(new GridModel(model));

        }

        [GridAction]
        public ActionResult _PackDetails(long packID)
        {
            List<ItemPackDetail> model = (from a in db.ItemPackDetails where a.PackID == packID select a).ToList();

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
            List<StoreLeadTime> model = (from a in db.StoreLeadTimes where ((a.Division == div) && (a.Store == store)) select a).ToList();
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
            DateTime today = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == div select a.RunDate).First();

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
                              select a.RunDate).First();

            //lazy loading was causing invalid circular references
            db.Configuration.ProxyCreationEnabled = false;

            List<RingFenceStatusCodes> rfStatusCodes = (from rfs in db.RingFenceStatusCodes
                                                        select rfs).ToList();

            List<RingFenceSummary> ringFences = new List<RingFenceSummary>();

            //List<RingFence> ringFences = (from a in db.RingFences where ((a.StartDate <= today) && ((a.EndDate >= today) || (a.EndDate == null))) select a).ToList();

            //ringFences = (from a in ringFences
            //                    join b in db.RingFenceDetails on a.ID equals b.RingFenceID
            //                    where ((a.Sku == sku)
            //                        //&& ((a.Size == model.Size)||(model.Size == ""))
            //                        && ((a.Store == store) || (store == null) || (a.Store == null)))
            //                    select a).Distinct().ToList();

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

            foreach (var rf in newRingFences)
            {
                RingFenceSummary newRF = new RingFenceSummary();
                newRF.RingFenceID = rf.RingFence.ID;
                newRF.Division = rf.RingFence.Division;
                newRF.Store = rf.RingFence.Store;
                newRF.Sku = rf.RingFence.Sku;
                newRF.PickQuantity = rf.RingFenceDetail.Qty;
                newRF.ItemID = rf.RingFence.ItemID;                
                newRF.Qty = rf.RingFenceDetail.Qty;
                newRF.StartDate = rf.RingFence.StartDate;
                newRF.EndDate = rf.RingFence.EndDate;
                newRF.Size = rf.RingFenceDetail.Size;
                newRF.PO = rf.RingFenceDetail.PO;
                newRF.CreatedBy = rf.RingFence.CreatedBy;
                newRF.CreateDate = rf.RingFence.CreateDate.Value;
                newRF.RingFenceStatus = rfStatusCodes.Find(s => s.ringFenceStatusCode == rf.RingFenceDetail.ringFenceStatusCode);

                ringFences.Add(newRF);
            }

            // Note: I'm doing this here to avoid having multiple active result sets in LINQ
            foreach (RingFenceSummary rf in ringFences.Where(r => r.Size.Length == 5))
            {
                int packQty = (from ip in db.ItemPacks
                                where ip.Name == rf.Size &&
                                        ip.ItemID == rf.ItemID
                                select ip.TotalQty).FirstOrDefault();
                rf.Qty = rf.Qty * packQty;
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
                model = (from a in model where a.Store == store select a).ToList();
            }

            return View(new GridModel(model));

        }

        [GridAction]
        public ActionResult _StoreHolds(string div, string store)
        {
            DateTime today = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == div select a.RunDate).First();

            List<Hold> model = (from a in db.Holds where ((a.Division == div) && ((a.Store == store) || (a.Store == null))) select a).ToList();
            model = (from a in model where ((a.StartDate <= today) && ((a.EndDate >= today) || (a.EndDate == null))) select a).ToList();

            return View(new GridModel(model));

        }

        [GridAction]
        public ActionResult _ItemHolds(string sku, string store)
        {
            string div = sku.Substring(0, 2);
            DateTime today = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == div select a.RunDate).First().AddDays(2);
            //List<Hold> holds = (from a in db.Holds where ((a.StartDate <= today) && ((a.EndDate >= today) || (a.EndDate == null))) select a).ToList();
            List<Hold> holds = (from a in db.Holds select a).ToList();

            ItemMaster item = (from a in db.ItemMasters where a.MerchantSku == sku select a).First();
            AllocationDriverDAO dao = new AllocationDriverDAO();

            holds = (from a in holds
                     where (
                         (a.Division == item.Div) &&
                         ((a.Store == store) || (a.Store == null) || (store == "") || (store == null)) &&
                         (
                         (a.Level == "All") ||
                         (a.Level == "Store") ||
                         ((a.Level == "Dept") && (a.Value == item.Dept)) ||
                         ((a.Level == "Category") && (a.Value == (item.Dept + "-" + item.Category))) ||
                         ((a.Level == "VendorDept") && (a.Value == (item.Vendor + "-" + item.Dept))) ||
                         ((a.Level == "VendorDeptCategory") && (a.Value == (item.Vendor + "-" + item.Dept + "-" + item.Category))) ||
                         ((a.Level == "DivDeptBrand") && (a.Value == (item.Div + "-" + item.Dept + "-" + item.Brand))) ||
                         ((a.Level == "Sku") && (a.Value == sku))
                         )
                         )
                     select a).ToList();

            return View(new GridModel(holds));
        }

        private void SetDCs(TroubleshootModel model)
        {
            model.AllDCs = (from a in db.DistributionCenters select a).ToList();
            DistributionCenter dc = new DistributionCenter();
            dc.ID = -1;
            dc.Name = "Any";
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
        public ActionResult TroubleshootInventory(TroubleshootInventory model)
        {
            return View(model);
        }

        [GridAction]
        public ActionResult _MainframeLinks(string sku)
        {
            MainframeDAO dao = new MainframeDAO();
            List<MainframeLink> model;
            model = dao.GetMainframeLinks(sku);

            return View(new GridModel(model));
        }

        
        [GridAction]
        public ActionResult _LegacyInventory(string sku)
        {

            List<LegacyInventory> model;
            if (sku != null)
            {
                model = (from a in db.LegacyInventory
                         where
                             ((a.Sku == sku) &&
                             (a.LocationTypeCode == "W"))
                         select a).ToList();
            }
            else
            {
                model = new List<LegacyInventory>();
            }

            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _LegacyFutureInventory(string sku)
        {

            List<LegacyFutureInventory> model;
            if (sku != null)
            {
                model = (from a in db.LegacyFutureInventory
                         where
                             ((a.Sku == sku) )
                         select a).ToList();
            }
            else
            {
                model = new List<LegacyFutureInventory>();
            }

            return View(new GridModel(model));
        }


        [GridAction]
        public ActionResult _LegacyInventoryFinal(string sku)
        {

            List<LegacyInventory> model;
            if (sku != null)
            {
                model = (new LegacyInventoryDAO()).GetLegacyInventoryForSku(sku);
            }
            else
            {
                model = new List<LegacyInventory>();
            }

            return View(new GridModel(model));
        }

        [GridAction]
        public ActionResult _LegacyFutureInventoryFinal(string sku)
        {

            List<LegacyFutureInventory> model;
            if (sku != null)
            {
                model = (new LegacyFutureInventoryDAO()).GetLegacyFutureInventoryForSku(sku);
            }
            else
            {
                model = new List<LegacyFutureInventory>();
            }

            return View(new GridModel(model));
        }

        #endregion

        #region TroubleShoot RDQs
        [CheckPermission(Roles = "Support,Buyer Planner,Director of Allocation,Div Logistics,Head Merchandiser,IT,Logistics,Merchandiser,Space Planning")]
        public ActionResult TroubleshootRDQ(string instanceID)
        {
            TroubleshootRDQModel model = new TroubleshootRDQModel();
            model.AvailableInstances = (from a in db.Instances select a).ToList();
            Instance inst = new Instance();
            inst.Name = "";
            model.AvailableInstances.Insert(0, inst);
            model.InstanceID = Convert.ToInt32(instanceID);
            model.ControlDate = (from a in db.ControlDates where a.InstanceID == model.InstanceID select a.RunDate).FirstOrDefault();
            return View(model);
        }

        [HttpPost]
        public ActionResult TroubleshootRDQ(TroubleshootRDQModel model)
        {
            model.AvailableInstances = (from a in db.Instances select a).ToList();
            Instance inst = new Instance();
            inst.Name = "";
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
                ((String)Session["rdqdate"] == (instance + "-" + controldate.ToShortDateString())))
            {
                model = (List<RDQ>)Session["rdqExtract"];
            }
            else
            {
                RDQDAO dao = new RDQDAO();

                model = dao.GetRDQExtractForDate(instance, controldate);

                Session["rdqdate"] = (instance + "-" + controldate.ToShortDateString());
                Session["rdqExtract"] = model;
            }
            return model;
        }


        #endregion  

        #region TroubleShoot RDQs for Sku
        [CheckPermission(Roles = "Support,Buyer Planner,Director of Allocation,Div Logistics,Head Merchandiser,IT,Logistics,Merchandiser,Space Planning")]
        public ActionResult TroubleshootRDQForSku(string sku)
        {
            TroubleshootRDQForSkuModel model = new TroubleshootRDQForSkuModel();
            model.Sku = sku;
            if ((sku != null)&&(sku != ""))
            {
                string div = sku.Substring(0, 2);
                model.ControlDate = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == div select a.RunDate).FirstOrDefault();
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult TroubleshootRDQForSku(TroubleshootRDQForSkuModel model)
        {
            if (model.ControlDate == null)
            {
                string div = model.Sku.Substring(0, 2);
                model.ControlDate = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == div select a.RunDate).FirstOrDefault();
            }
            return View(model);
        }

        [GridAction]
        public ActionResult _TroubleshootRDQForSkuToMF(string sku, DateTime? controldate)
        {
            List<RDQ> model;

            RDQDAO dao = new RDQDAO();
            if ((sku != null)&&(sku != ""))
            {
                model = GetRDQExtractForSkuDate(sku, controldate);
            }
            else
            {
                model = new List<RDQ>();
            }
            return View(new GridModel(model));
        }

		public ActionResult ExportTroubleshootRDQ(string sku, DateTime? controldate)
		{
			try
			{
				// retrieve data
				List<RDQ> items = GetRDQExtractForSkuDate(sku, controldate);
				Excel excelDocument = CreateRDQExport(items);
				excelDocument.Save("TroubleshootRDQs.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
				return RedirectToAction("TroubleshootRDQForSku");
			}
			catch (Exception ex)
			{
				return Content(ex.Message);
			}

		}

		private Excel CreateRDQExport(List<RDQ> items)
		{
			Excel excelDocument = GetRDQExcelFile();

			int row = 4;
			Worksheet mySheet = excelDocument.Worksheets[0];
			foreach (var rdq in items)
			{
				// rdq values

				mySheet.Cells[row, 0].PutValue(rdq.Status);
				mySheet.Cells[row, 1].PutValue(rdq.Division);
				mySheet.Cells[row, 1].Style.HorizontalAlignment = TextAlignmentType.Center;
				mySheet.Cells[row, 2].PutValue(rdq.Store);
				mySheet.Cells[row, 3].PutValue(rdq.Sku);
				mySheet.Cells[row, 4].PutValue(rdq.Size);
				mySheet.Cells[row, 5].PutValue(rdq.Qty);
				mySheet.Cells[row, 5].Style.HorizontalAlignment = TextAlignmentType.Center;
				mySheet.Cells[row, 6].PutValue(rdq.UnitQty);
				mySheet.Cells[row, 6].Style.HorizontalAlignment = TextAlignmentType.Center;
				mySheet.Cells[row, 7].PutValue(rdq.DC);
				mySheet.Cells[row, 8].PutValue(rdq.PO);
				mySheet.Cells[row, 9].PutValue(rdq.RecordType);
				mySheet.Cells[row, 9].Style.HorizontalAlignment = TextAlignmentType.Center;

				row++;
			}

			for (int i = 0; i < 9; i++)
			{
				mySheet.AutoFitColumn(i);
			}

			return excelDocument;
		}

		private Excel GetRDQExcelFile()
		{
			Aspose.Excel.License license = new Aspose.Excel.License();
			// set license
			license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

			Excel excelDocument = new Excel();

			Worksheet mySheet = excelDocument.Worksheets[0];

			mySheet.Cells[1, 3].PutValue("RDQs to MF");
//			mySheet.Cells[1, 3].Style.HorizontalAlignment = TextAlignmentType.Center;
			mySheet.Cells[1, 3].Style.Font.Size = 12;
			mySheet.Cells[1, 3].Style.Font.IsBold = true;

			mySheet.Cells[3, 0].PutValue("Status");
			mySheet.Cells[3, 1].PutValue("Division");
			mySheet.Cells[3, 2].PutValue("Store");
			mySheet.Cells[3, 3].PutValue("Sku");
			mySheet.Cells[3, 4].PutValue("Size/Caselot");
			mySheet.Cells[3, 5].PutValue("PickQty");
			mySheet.Cells[3, 6].PutValue("UnitQty");
			mySheet.Cells[3, 7].PutValue("DC");
			mySheet.Cells[3, 8].PutValue("PO");
			mySheet.Cells[3, 9].PutValue("RecordType");

			return excelDocument;
		}

		private List<RDQ> GetRDQExtractForSkuDate(string sku, DateTime? controldate)
        {

            RDQDAO dao = new RDQDAO();

            List<RDQ> model = dao.GetRDQExtractForSkuDate(sku, Convert.ToDateTime(controldate));

            return model;
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
        public ActionResult QuantumDataDownload(QuantumDataDownloadModel model, string submitAction)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();

            QuantumDAO dao = new QuantumDAO();

            LostSalesRequest request = new LostSalesRequest();

            int row = 1;
            int page = 0;

            if (submitAction == "LostSales")
            {
                DateTime start = default(DateTime); //Day 1 of the 14 day span to initialize excel headings
                Double weeklySales = 0; //Variable to store prior week lost sales
                
                request = dao.GetLostSales(model.Sku);

                //If lost sales query returns no results then inform user
                if (!request.LostSales.Any()) 
                {
                    ViewBag.NoDataFound = "There was no data found for Sku " + model.Sku;
                    return View(model);
                }

                start = request.BeginDate;
                Worksheet mySheet = InitializeNewLostSalesSheet(excelDocument, page, start);

                foreach (LostSalesInstance ls in request.LostSales)
                {
                    //eliminate 'S' and 'division' from location id, then put in store location column
                    mySheet.Cells[row, 0].PutValue(ls.LocationId.Substring(3)); 

                    //add shoe size from product id to end of sku, then put in sku column
                    mySheet.Cells[row, 1].PutValue(model.Sku + ls.ProductId.Substring(7));

                    //sum weekly lost sales from daily lost sales array, then put in appropriate column
                    for (int i = 0; i < 7; i++ )
                    {
                        weeklySales += ls.DailySales[request.WeeklySalesEndIndex - i];
                    }
                    mySheet.Cells[row, 2].PutValue(weeklySales);
                    //reset for the next lostsalesinstance
                    weeklySales = 0;

                    //put daily lost sales in the appropriate day column
                    for (int i = 0; i < 14; i++)
                    {
                       mySheet.Cells[row, i + 3].PutValue(ls.DailySales[i]); 
                    }
                    row++;
                    if (row > 60000)
                    {
                        //new page
                        row = 1;
                        page++;

                        //auto fit columns of current sheet before adding a new sheet
                        for (int i = 0; i < 17; i++)
                        {
                            mySheet.AutoFitColumn(i);
                        }
                        mySheet = InitializeNewLostSalesSheet(excelDocument, page, start);
                    }
                }

                //auto fit columns of current sheet before creating the excel file
                for (int i = 0; i < 17; i++)
                {
                    mySheet.AutoFitColumn(i);
                }

                excelDocument.Save(model.Sku + "-LostSales.xls", SaveType.OpenInExcel, FileFormatType.Default,
                    System.Web.HttpContext.Current.Response);
            } else if (submitAction == "WSM")
            {
                List<WSM> wsmList = dao.GetWSM(model.Sku);

                //If wsm query returns no results then inform user
                if (!wsmList.Any())
                {
                    ViewBag.NoDataFound = "There was no data found for Sku " + model.Sku;
                    return View(model);
                }

                Worksheet mySheet = InitializeNewWSMSheet(excelDocument, page);

                foreach (WSM w in wsmList)
                {
                    mySheet.Cells[row, 0].PutValue(w.RunDate);
                    mySheet.Cells[row, 1].PutValue(w.TargetProduct);
                    mySheet.Cells[row, 2].PutValue(w.TargetProductId);
                    mySheet.Cells[row, 3].PutValue(w.TargetLocation);
                    mySheet.Cells[row, 4].PutValue(w.MatchProduct);
                    mySheet.Cells[row, 5].PutValue(w.MatchProductId);
                    mySheet.Cells[row, 6].PutValue(w.ProductWeight);
                    mySheet.Cells[row, 7].PutValue(w.MatchLocation);
                    mySheet.Cells[row, 8].PutValue(w.LocationWeight);
                    mySheet.Cells[row, 9].PutValue(w.FinalMatchWeight);
                    mySheet.Cells[row, 10].PutValue(w.FinalMatchDemand);
                    mySheet.Cells[row, 11].PutValue(w.LastCapturedDemand);
                    mySheet.Cells[row, 12].PutValue(w.StatusCode);
                    row++;
                    if (row > 60000)
                    {
                        //new page
                        row = 1;
                        page++;

                        //auto fit columns before adding a new sheet
                        for (int i = 0; i < 12; i++)
                        {
                            mySheet.AutoFitColumn(i);
                        }
                        mySheet = InitializeNewWSMSheet(excelDocument, page);
                    }
                }

                //auto fit columns 
                for (int i = 0; i < 12; i++)
                {
                    mySheet.AutoFitColumn(i);
                }

                excelDocument.Save(model.Sku + "-WSM.xls", SaveType.OpenInExcel, FileFormatType.Default,
                    System.Web.HttpContext.Current.Response);
            }
            return View(model);
        }

        //method to create a new excel worksheet for lost sales
        private Worksheet InitializeNewLostSalesSheet(Excel excelDocument, int page, DateTime start)
        {
            if (page > 0)
            {
                excelDocument.Worksheets.Add();
            }
            Worksheet mySheet = excelDocument.Worksheets[page];

            //assign header names
            for (int i = 0; i < 17; i++)
            {
                //make the headers of excel sheet bold
                mySheet.Cells[0, i].Style.Font.IsBold = true;

                if (i == 0) //column 1 header
                {
                    mySheet.Cells[0, 0].PutValue("Location Id");
                }
                else if (i == 1) //column 2 header
                {
                    mySheet.Cells[0, 1].PutValue("Sku");
                }
                else if (i == 2) //column 3 header
                {
                    mySheet.Cells[0, 2].PutValue("Prior Week Lost Sales");
                }
                else //all other column headers
                {
                    //format cells to datetime type
                    mySheet.Cells[0, i].Style.Number = 14;
                    mySheet.Cells[0, i].PutValue(start.AddDays(i - 3));
                }
            }
            return mySheet;
        }

        //method to create a new excel worksheet for wsm
        private Worksheet InitializeNewWSMSheet(Excel excelDocument, int page)
        {
            if (page > 0)
            {
                excelDocument.Worksheets.Add();
            }
            Worksheet mySheet = excelDocument.Worksheets[page];

            //make the headers of excel sheet bold
            for (int i = 0; i < 12; i++)
            {
                mySheet.Cells[0, i].Style.Font.IsBold = true;
            }

            //assign header names
            mySheet.Cells[0, 0].PutValue("RunDate");
            mySheet.Cells[0, 1].PutValue("TargetProduct");
            mySheet.Cells[0, 2].PutValue("TargetProductID");
            mySheet.Cells[0, 3].PutValue("TargetLocation");
            mySheet.Cells[0, 4].PutValue("MatchProduct");
            mySheet.Cells[0, 5].PutValue("MatchProductID");
            mySheet.Cells[0, 6].PutValue("ProductWeight");
            mySheet.Cells[0, 7].PutValue("MatchLocation");
            mySheet.Cells[0, 8].PutValue("LocationWeight");
            mySheet.Cells[0, 9].PutValue("FinalMatchWeight");
            mySheet.Cells[0, 10].PutValue("FinalMatchDemand");
            mySheet.Cells[0, 11].PutValue("LastCapturedDemand");
            mySheet.Cells[0, 12].PutValue("StatusCode");
            return mySheet;
        }

        #endregion
    }
}
