using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Telerik.Web.Mvc;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Models.Services;
using Aspose.Excel;
using System.IO;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Director of Allocation,Admin,Support")]
    public class RingFenceController : AppController
    {
        #region Fields

        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        // NOTE: Both caselot name and size are stored in the same varchar db column, if value is more than 3 digits, we know it is a caselot....
        public static int _CASELOT_SIZE_INDICATOR_VALUE_LENGTH = 3;

        #endregion

        //
        // GET: /RingFence/

        #region Non-Public Methods

        private List<RingFenceDetail> GetFutureAvailable(RingFence ringFence)
        {
            List<RingFenceDetail> list = new List<RingFenceDetail>();
            RingFenceDAO dao = new RingFenceDAO();
            list = dao.GetFuturePOs(ringFence);
            list.AddRange(dao.GetTransloadPOs(ringFence));
            return list;
        }

        private List<RingFenceDetail> GetRingFenceDetails(Int64 ringFenceID)
        {
            // Get ring fence data...
            // HACK: Should really be relational, and pulled in on a single query from EF
            var ringFenceItemName = db.RingFences.AsNoTracking().Single(rf => rf.ID == ringFenceID).Sku;
            var ringFenceItemID = db.ItemMasters.Single(i => i.MerchantSku == ringFenceItemName).ID;
            var ringFenceDetails = db.RingFenceDetails.AsNoTracking().Where(d => d.RingFenceID == ringFenceID && 
                                                                                 d.ActiveInd == "1").ToList();
            var dcs = (from a in db.DistributionCenters select a).ToList();
            foreach (var det in ringFenceDetails)
            {
                // Determine if ring fence detail record is for caselot or bin
                if (det.Size.Length > _CASELOT_SIZE_INDICATOR_VALUE_LENGTH)
                {
                    // Load sizes of caselot/pack
                    try
                    {
                        var itemPack = db.ItemPacks.Include("Details").Single(p => p.ItemID == ringFenceItemID && p.Name == det.Size);
                        det.PackDetails = itemPack.Details.ToList();
                    }
                    catch 
                    {
                        det.PackDetails = new List<ItemPackDetail>();
                    }
                }

                // Load warehouse
                det.Warehouse = (from a in dcs where a.ID == det.DCID select a).First().Name;
            }

            return ringFenceDetails;
        }

        private List<RingFenceDetail> GetWarehouseAvailable(RingFence ringFence)
        {
            RingFenceDAO dao = new RingFenceDAO();
            return dao.GetWarehouseAvailable(ringFence);
        }

        private string ValidateStorePick(RingFencePickModel rf)
        {
            if ((rf.Store == null) || (rf.Store == ""))
            {
                return "Store is required for Pick.";
            }

            foreach (RingFenceDetail det in rf.Details)
            {
                if (det.AssignedQty > det.Qty)
                {
                    return "Qty must be less than ring fence Qty.";
                }
            }
            return "";
        }

        #endregion

        public ActionResult Index(string message)
        {            
            List<RingFenceModel> model = new List<RingFenceModel>();

            RingFenceDAO rfDAO = new RingFenceDAO();
            List<RingFence> list = rfDAO.GetValidRingFences(Divisions());            

            ViewData["message"] = message;
            return View(list);
        }

        public ActionResult IndexSummary(string message)
        {
            ViewData["message"] = message;
            return View();
        }

        public ActionResult IndexByStore(string message)
        {
            ViewData["message"] = message;
            return View();
        }

        [GridAction]
        public ActionResult _RingFenceSummary()
        {
            RingFenceDAO rfDAO = new RingFenceDAO();
            List<RingFence> list = rfDAO.GetValidRingFences(Divisions());

            var rfGroups = from rf in list
            group rf by new
            {
                Sku = rf.Sku,
                itemid = rf.ItemID,
            } into g
            select new RingFenceSummary()
            {
                Sku = g.Key.Sku,
                Size = "",
                DC = "",
                PO = "",
                Qty = g.Sum(r => r.Qty)
            };

            //List<RingFenceSummary> list2 = rfGroups.ToList();
            //return PartialView(new GridModel(list2));
            return PartialView(new GridModel(rfGroups.ToList()));
        }

        [GridAction]
        public ActionResult _RingFenceStores()
        {
            List<Division> divs = Divisions();
            List<StoreLookup> list = (from a in db.RingFences join b in db.StoreLookups on new { a.Division, a.Store } equals new { b.Division, b.Store } where a.Qty > 0 select b).ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    select a).Distinct().ToList();

            return PartialView(new GridModel(list));
        }

        [GridAction]
        public ActionResult _RingFenceFOB(string div, string store)
        {
            div = div.Trim();
            store = store.Trim();

            List<Division> divs = Divisions();
            List<FOB> list = (from a in db.RingFences join b in db.StoreLookups on new { a.Division, a.Store } equals new { b.Division, b.Store } 
                              join c in db.ItemMasters on a.ItemID equals c.ID
                              join d in db.FOBDepts on c.Dept equals d.Department
                              join e in db.FOBs on d.FOBID equals e.ID
                              where ((a.Qty > 0)&&(a.Store == store)&&(a.Division == div) && (e.Division == div)) select e).Distinct().ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    select a).Distinct().ToList();

            return PartialView(new GridModel(list));
        }

        [GridAction]
        public ActionResult _RingFences(string sku)
        {
            List<Division> divs = Divisions();
            List<RingFence> list = (from a in db.RingFences where ((a.Qty > 0)&&(a.Sku == sku)) select a).ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    select new RingFence{ 
                        ID = a.ID,
                        Sku = a.Sku, 
                        Size = a.Size, 
                        Division = a.Division,
                        Store = a.Store,
                        //ItemMaster = a.ItemMaster,
                        Qty = a.Qty,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate,
                        Type = a.Type,
                        Comments = a.Comments,
                        CreatedBy = a.CreatedBy,
                        CreateDate = a.CreateDate,
                        RingFenceTypeDescription = a.RingFenceTypeDescription,
                        ItemDescription = a.ItemDescription
                    }).OrderByDescending(x => x.CreateDate).ToList();



            //eturn new JsonResult { Data = list.ToList() };
            return PartialView(new GridModel(list));
        }

        [GridAction]
        public ActionResult _RingFencesForStore(string div, string store)
        {
            List<RingFence> list;
            if ((Session["rfStore"] != null) &&
                ((String)Session["rfStore"] == div + store))
            {
                list = (List<RingFence>)Session["rfStoreList"];
            }
            else
            {
                List<Division> divs = Divisions();
                list = (from a in db.RingFences where ((a.Qty > 0) && (a.Division == div)&& (a.Store == store)) select a).ToList();
                list = (from a in list
                        join d in divs on a.Division equals d.DivCode
                        select new RingFence
                        {
                            ID = a.ID,
                            Sku = a.Sku,
                            Size = a.Size,
                            Division = a.Division,
                            Store = a.Store,
                            //ItemMaster = a.ItemMaster,
                            Qty = a.Qty,
                            StartDate = a.StartDate,
                            EndDate = a.EndDate,
                            Type = a.Type,
                            Comments = a.Comments,
                            CreatedBy = a.CreatedBy,
                            CreateDate = a.CreateDate,
                            RingFenceTypeDescription = a.RingFenceTypeDescription,
                            ItemDescription = a.ItemDescription
                        }).OrderByDescending(x => x.CreateDate).ToList();

            }
            Session["rfStore"] = div + store;
            Session["rfStoreList"] = list;

            //eturn new JsonResult { Data = list.ToList() };
            return PartialView(new GridModel(list));
        }

        [GridAction]
        public ActionResult _RingFencesForFOB(string div, string store, string fob)
        {
            div = div.Trim();
            store = store.Trim();

            List<RingFence> list;
            if ((Session["rfFOBStore"] != null) &&
                ((String)Session["rfFOBStore"] == div + store + fob))
            {
                list = (List<RingFence>)Session["rfFOBStoreList"];
            }
            else
            {
                List<Division> divs = Divisions();
                list = (from a in db.RingFences 
                        join i in db.ItemMasters on a.ItemID equals i.ID
                        join b in db.FOBDepts on i.Dept equals b.Department
                        join c in db.FOBs on b.FOBID equals c.ID
                        where ((a.Qty > 0) && (a.Division == div) && (a.Store == store) && (c.Name == fob) && (c.Division == div))
                        select a).ToList();
                list = (from a in list
                        join d in divs on a.Division equals d.DivCode
                        select new RingFence
                        {
                            ID = a.ID,
                            Sku = a.Sku,
                            Size = a.Size,
                            Division = a.Division,
                            Store = a.Store,
                            //ItemMaster = a.ItemMaster,
                            Qty = a.Qty,
                            StartDate = a.StartDate,
                            EndDate = a.EndDate,
                            Type = a.Type,
                            Comments = a.Comments,
                            CreatedBy = a.CreatedBy,
                            CreateDate = a.CreateDate,
                            RingFenceTypeDescription = a.RingFenceTypeDescription,
                            ItemDescription = a.ItemDescription
                        }).OrderByDescending(x => x.CreateDate).ToList();

            }
            Session["rfFOBStore"] = div + store + fob;
            Session["rfFOBStoreList"] = list;

            //eturn new JsonResult { Data = list.ToList() };
            return PartialView(new GridModel(list));
        }


        public ActionResult Create()
        {
            RingFenceModel model = new RingFenceModel();
            model.RingFence = new RingFence();
            //model.RingFence.StartDate = DateTime.Now;
            model.Divisions = this.Divisions();
            return View(model);
        }

        private void createEcommRingFences(RingFence ringFence)
        {
            //Europe ecomm for all countries, need to create on for each country
            List<EcommWarehouse> list = (from a in db.EcommWarehouses
                                         where a.Store != "00800"
                                         select a).ToList();
            Boolean add;
            foreach (EcommWarehouse w in list)
            {
                add = false;

                RingFence rf = (from a in db.RingFences
                                where ((a.Division == w.Division) &&
                                       (a.Store == w.Store) &&
                                       (a.ItemID == ringFence.ItemID))
                                select a).FirstOrDefault();
                if (rf == null)
                {
                    add = true;
                    rf = new RingFence();
                    rf.Division = w.Division;
                    rf.Store = w.Store;
                    rf.Sku = ringFence.Sku;
                    rf.ItemID = ringFence.ItemID;
                }
                rf.Comments = ringFence.Comments;
                rf.StartDate = ringFence.StartDate;
                rf.EndDate = ringFence.EndDate;
                rf.DCID = ringFence.DCID;
                rf.Type = 2;
                rf.LastModifiedDate = DateTime.Now;
                rf.LastModifiedUser = User.Identity.Name;

                if (add)
                {
                    db.RingFences.Add(rf);
                }
            }
            db.SaveChanges(UserName);
        }

        public void SetUpRingFenceHeader(RingFence rf)
        {
            rf.ItemID = (from a in db.ItemMasters
                         where (a.MerchantSku == rf.Sku)
                         select a).First().ID;

            rf.StartDate = (from a in db.ControlDates
                                 join b in db.InstanceDivisions
                                     on a.InstanceID equals b.InstanceID
                            where b.Division == rf.Division
                            select a.RunDate).First().AddDays(1);

            rf.CreateDate = DateTime.Now;
            rf.CreatedBy = User.Identity.Name;
            rf.LastModifiedDate = DateTime.Now;
            rf.LastModifiedUser = User.Identity.Name;

            //set the type for ringfence, normal/ecomm/alshaya
            rf.Type = 1;//default to normal
            var ecomm = (from a in db.EcommWarehouses
                         where ((a.Division == rf.Division) &&
                                (a.Store == rf.Store))
                         select a);
            if (ecomm.Count() > 0)
            {
                rf.Type = 2;
            }
            rf.Qty = 0;
        }

        [HttpPost]
        public ActionResult Create(RingFenceModel model)
        {
            model.Divisions = this.Divisions();
            RingFenceDAO rfDAO = new RingFenceDAO();
            string errorMessage;

            if (!rfDAO.isValidRingFence(model.RingFence, UserName, out errorMessage))
            {
                ViewData["message"] = errorMessage;
                return View(model);
            }
            else
            {
                SetUpRingFenceHeader(model.RingFence);
                
                db.RingFences.Add(model.RingFence);
                db.SaveChanges(User.Identity.Name);

                //if (model.RingFence.Store == "00800")
                //{
                //    createEcommRingFences(model.RingFence);
                //}

                return AssignInventory(model);
            }
        }

        public ActionResult AssignInventory(RingFenceModel model)
        {
            ViewData["ringFenceID"] = model.RingFence.ID;

            model.Divisions = this.Divisions();

            model.WarehouseAvailable = GetWarehouseAvailable(model.RingFence);
            model.FutureAvailable = GetFutureAvailable(model.RingFence);

            return View("AssignInventory", model);
        }

        //[HttpPost]
        //public ActionResult SaveAssignInventory(RingFenceModel model)
        //{
        //    //TODO:  Save the assignments.
        //    var warehouses = (from a in model.WarehouseAvailable where a.Qty > 0 select a);
        //    var futures = (from a in model.FutureAvailable where a.Qty > 0 select a);

        //    string message = "";
        //    Boolean save = true;
        //    foreach (RingFenceDetail det in warehouses)
        //    {
        //        det.PO = "";
        //        det.ActiveInd = "1";
        //        det.ringFenceStatusCode = "4";

        //        if ((det.Qty > det.AvailableQty)&&(det.Qty > 0))
        //        {
        //            message = message + "Max Qty for " + det.Warehouse + " " + det.PO + " is " + det.AvailableQty;
        //            save = false;
        //        } 
        //        else if (det.Qty < 0)
        //        {
        //            message = message + "Cannot have qty < 0";
        //            save = false;
        //        }
        //    }
        //    foreach (RingFenceDetail det in futures)
        //    {
        //        det.ActiveInd = "1";
        //        det.ringFenceStatusCode = "1";

        //        if ((det.Qty > det.AvailableQty)&&(det.Qty > 0))
        //        {
        //            message = message + "Max Qty for " + det.Warehouse + " " + det.PO + " is " + det.AvailableQty + "<br>";
        //            save = false;
        //        }
        //        else if (det.Qty < 0)
        //        {
        //            message = message + "Cannot have qty < 0";
        //            save = false;
        //        }
        //        List<ExistingPO> poList = (new ExistingPODAO()).GetExistingPO(model.RingFence.Division, det.PO);

        //        foreach (ExistingPO po in poList)
        //        {
        //            if (po.ExpectedDeliveryDate < DateTime.Now)
        //            {
        //                message = message + "This PO is expected to delivery today.  If it does, this ringfence will NOT be enforced (it will be deleted).<br>";
        //            }
        //        }
        //    }

        //    if (save)
        //    {
        //        if (model.RingFence.Store != "00800")
        //        {
        //            foreach (RingFenceDetail det in warehouses)
        //            {
        //                if (det.Qty > 0)
        //                {
        //                    det.Size = det.Size.Trim();
        //                    db.RingFenceDetails.Add(det);
        //                    db.SaveChanges(User.Identity.Name);
        //                }
        //            }
        //            foreach (RingFenceDetail det in futures)
        //            {
        //                if (det.Qty > 0)
        //                {
        //                    db.RingFenceDetails.Add(det);
        //                    db.SaveChanges(User.Identity.Name);
        //                }
        //            }

        //            //RingFence rf = (from a in db.RingFences where a.ID == model.RingFence.ID select a).First();
        //            //rf.Qty = (from a in futures select a.Qty).Sum();
        //            //rf.Qty = rf.Qty + (from a in warehouses select a.Qty).Sum();
        //            //db.SaveChanges(User.Identity.Name);

        //            return RedirectToAction("Index");
        //        }
        //        else
        //        { 
        //            //ecomm store for all countries, break it out

        //            RingFenceDetail newDet = new RingFenceDetail();
        //            List<EcommWeight> weights = (new EcommWeightDAO()).GetEcommWeightList(model.RingFence.Department);

        //            foreach (EcommWeight w in weights)
        //            {
        //                RingFence rf = (from a in db.RingFences
        //                                where ((a.Sku == model.RingFence.Sku)&&(a.Store == w.Store))
        //                                select a).First();

        //                foreach (RingFenceDetail det in warehouses)
        //                {
        //                    newDet = new RingFenceDetail();
        //                    newDet.DCID = det.DCID;
        //                    newDet.PO = det.PO;
        //                    newDet.Qty = Convert.ToInt32(det.Qty*w.Weight);
        //                    newDet.Size = det.Size.Trim();

        //                    newDet.RingFenceID = rf.ID;
        //                    db.RingFenceDetails.Add(newDet);
        //                    //db.SaveChanges(User.Identity.Name);
        //                }
        //                foreach (RingFenceDetail det in futures)
        //                {
        //                    newDet = new RingFenceDetail();
        //                    newDet.DCID = det.DCID;
        //                    newDet.PO = det.PO;
        //                    newDet.Qty = Convert.ToInt32(det.Qty * w.Weight);
        //                    newDet.Size = det.Size.Trim();
        //                    newDet.RingFenceID = rf.ID;
        //                    db.RingFenceDetails.Add(newDet);
        //                    //db.SaveChanges(User.Identity.Name);
        //                }

        //                rf.Qty = (from a in futures select a.Qty).Sum();
        //                rf.Qty = rf.Qty + (from a in warehouses select a.Qty).Sum();
        //                db.SaveChanges(User.Identity.Name);
        //            }

        //            return RedirectToAction("Index");
        //        }
        //    }
        //    else
        //    {
        //        model.RingFence = (from a in db.RingFences where a.ID == model.RingFence.ID select a).First();
        //        ViewData["message"] = message;
        //        return View("AssignInventory", model);
        //    }
        //}

        public ActionResult Details(int ID)
        {
            RingFenceModel model = new RingFenceModel();
            model.RingFence = (from a in db.RingFences where a.ID==ID select a).First();
            model.FutureAvailable = (from a in db.RingFenceDetails
                                     where a.RingFenceID == ID && 
                                           a.ActiveInd == "1"
                                     select a).ToList();

            List<DistributionCenter> dcs = (from a in db.DistributionCenters
                                            select a).ToList();

            foreach (RingFenceDetail det in model.FutureAvailable)
            {
                det.Warehouse = (from a in dcs
                                 where a.ID == det.DCID
                                 select a).First().Name;
            }

            model.Divisions = this.Divisions();
            return View(model);
        }

        [GridAction]
        public ActionResult _Audit(int ID)
        {
            List<DistributionCenter> dcs = (from a in db.DistributionCenters select a).ToList();
            List<RingFenceHistory> model = (from a in db.RingFenceHistory where a.RingFenceID == ID select a).ToList();

            foreach (RingFenceHistory h in model)
            {
                h.Warehouse = (from a in dcs where a.ID == h.DCID select a.Name).FirstOrDefault();
            }
            return View(new GridModel(model));
        }

        public ActionResult Audit(int ID)
        {
            RingFenceModel model = new RingFenceModel();
            model.RingFence = (from a in db.RingFences where a.ID == ID select a).First();
            return View(model);
        }

        public ActionResult SizeSummary(int ID)
        {
            List<RingFenceSizeSummary> sizes = new List<RingFenceSizeSummary>();

            List<RingFenceDetail> details = (from a in db.RingFenceDetails
                                             where ((a.RingFenceID == ID) &&
                                                    (a.ActiveInd == "1") && 
                                                    (a.Size.Length == _CASELOT_SIZE_INDICATOR_VALUE_LENGTH))
                                             select a).ToList();
            List<RingFenceDetail> caselotDetails = (from a in db.RingFenceDetails
                                                    where ((a.RingFenceID == ID) &&
                                                           (a.ActiveInd == "1") &&
                                                           (a.Size.Length > _CASELOT_SIZE_INDICATOR_VALUE_LENGTH))
                                                    select a).ToList();

            RingFenceSizeSummary currentSize = new RingFenceSizeSummary();

            foreach (RingFenceDetail det in details)
            { 
                var query = (from a in sizes
                             where a.Size == det.Size
                             select a);
                if (query.Count() > 0)
                {
                    currentSize = query.First();
                }
                else
                {
                    currentSize = new RingFenceSizeSummary();
                    currentSize.Size = det.Size;
                    sizes.Add(currentSize);
                }
                if ((det.PO != null)&&(det.PO != ""))
                {
                    currentSize.FutureQty += det.Qty;
                }
                currentSize.BinQty += det.Qty;
                currentSize.TotalQty += det.Qty;
            }

            long ringFenceItemID = (from a in db.RingFences
                                    where a.ID == ID
                                    select a.ItemID).First();

            foreach (RingFenceDetail det in caselotDetails)
            {
                try
                {
                    var itemPack = db.ItemPacks.Include("Details").Single(p => p.ItemID == ringFenceItemID && p.Name == det.Size);
                    det.PackDetails = itemPack.Details.ToList();

                    foreach (ItemPackDetail pd in det.PackDetails)
                    {
                        var query = (from a in sizes where a.Size == pd.Size select a);
                        if (query.Count() > 0)
                        {
                            currentSize = query.First();
                        }
                        else
                        {
                            currentSize = new RingFenceSizeSummary();
                            currentSize.Size = pd.Size;
                            sizes.Add(currentSize);
                        }
                        if ((det.PO != null) && (det.PO != ""))
                        {
                            currentSize.FutureQty += (pd.Quantity * det.Qty);
                        }
                        currentSize.CaselotQty += (pd.Quantity * det.Qty);
                        currentSize.TotalQty += (pd.Quantity * det.Qty);
                    }
                }
                catch (Exception ex)
                {
                }
            }

            RingFenceSizeModel model = new RingFenceSizeModel();
            model.RingFence = (from a in db.RingFences
                               where a.ID == ID
                               select a).First();
            model.Details = sizes;

            model.Divisions = this.Divisions();
            return View(model);
        }

        [GridAction]
        public ActionResult _SelectSizeDetail(int ID, string size)
        {
            List<RingFenceDetail> list = (from a in db.RingFenceDetails
                                          where ((a.RingFenceID == ID) && 
                                                 (a.ActiveInd == "1") &&
                                                 ((a.Size == size) || 
                                                  (a.Size.Length > _CASELOT_SIZE_INDICATOR_VALUE_LENGTH)))
                                          select a).ToList();

            List<RingFenceDetail> model = new List<RingFenceDetail>();
            List<DistributionCenter> dcs = (from a in db.DistributionCenters select a).ToList();
            Boolean includeDetail = false;
            long ringFenceItemID = (from a in db.RingFences
                                    where a.ID == ID
                                    select a.ItemID).First();

            foreach (RingFenceDetail det in list)
            {
                includeDetail = false;
                if (det.Size.Length > _CASELOT_SIZE_INDICATOR_VALUE_LENGTH)
                {
                    try
                    {
                        var itemPack = db.ItemPacks.Include("Details").Single(p => p.ItemID == ringFenceItemID && p.Name == det.Size);
                        det.PackDetails = itemPack.Details.ToList();

                        foreach (ItemPackDetail pd in det.PackDetails)
                        {
                            if (pd.Size == size)
                            {
                                det.Units += det.Qty * pd.Quantity;
                                includeDetail = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        includeDetail = false;
                    }
                }
                else
                {
                    includeDetail = true;
                    det.Units = det.Qty;
                }

                det.Warehouse = (from a in dcs where a.ID == det.DCID select a).First().Name;

                if (includeDetail)
                {
                    model.Add(det);
                }
            }

            return View(new GridModel(model));
        }

        public ActionResult SizeDetail(int ID, string size)
        {
            RingFenceModel model = new RingFenceModel();
            model.RingFence = (from a in db.RingFences
                               where a.ID == ID
                               select a).First();
            model.FutureAvailable = new List<RingFenceDetail>();
            //List<RingFenceDetail> list = (from a in db.RingFenceDetails where ((a.RingFenceID == ID) && ((a.Size == size) || (a.Size.Length > _CASELOT_SIZE_INDICATOR_VALUE_LENGTH))) select a).ToList();

            //List<DistributionCenter> dcs = (from a in db.DistributionCenters select a).ToList();
            //Boolean includeDetail = false;
            //long ringFenceItemID = model.RingFence.ItemID;
            //foreach (RingFenceDetail det in list)
            //{
            //    includeDetail = false;
            //    if (det.Size.Length > _CASELOT_SIZE_INDICATOR_VALUE_LENGTH)
            //    {
            //        try
            //        {
            //            var itemPack = db.ItemPacks.Include("Details").Single(p => p.ItemID == ringFenceItemID && p.Name == det.Size);
            //            det.PackDetails = itemPack.Details.ToList();

            //            foreach (ItemPackDetail pd in det.PackDetails)
            //            {
            //                if (pd.Size == size)
            //                {
            //                    includeDetail = true;
            //                }
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            includeDetail = false;
            //        }
            //    }
            //    else
            //    {
            //        includeDetail = true;
            //    }

            //    det.Warehouse = (from a in dcs where a.ID == det.DCID select a).First().Name;

            //    model.FutureAvailable.Add(det);
            //}

            model.Divisions = this.Divisions();
            ViewData["Size"] = size;
            return View(model);
        }

        public ActionResult Edit(int ID)
        {
            ViewData["ringFenceID"] = ID;
            string errorMessage;
            RingFenceDAO rfDAO = new RingFenceDAO();

            // Build up a RingFence view model
            RingFenceModel model = new RingFenceModel();
            var ringfenceQuery = (from a in db.RingFences where a.ID == ID select a);
            if (ringfenceQuery.Count() == 0)
            {
                return RedirectToAction("Index", new { message = "Ring fence no longer exists.  " });
            }
            model.RingFence = ringfenceQuery.First();

            if (!rfDAO.canUserUpdateRingFence(model.RingFence, UserName, out errorMessage))
            {
                return RedirectToAction("Index", new { message = errorMessage });
            }

            model.Divisions = this.Divisions();

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(RingFenceModel model)
        {
            string errorMessage;
            RingFenceDAO rfDAO = new RingFenceDAO();

            try
            {
                model.Divisions = this.Divisions();
                ViewData["ringFenceID"] = model.RingFence.ID;
                if (!rfDAO.canUserUpdateRingFence(model.RingFence, UserName, out errorMessage))
                {
                    ViewData["message"] = errorMessage;
                    return View(model);
                }

            model.RingFence.CreatedBy = User.Identity.Name;
            model.RingFence.CreateDate = DateTime.Now;

            db.Entry(model.RingFence).State = System.Data.EntityState.Modified;

            //if (model.RingFence.Type == 2)
            //{ 
            //    //ecomm warehouse ringfence, need to update warehouse inventory
            //    var ecommQuery = (from a in db.EcommInventory where ((a.Division == model.RingFence.Division)&&(a.Store == model.RingFence.Store)&&(a.Size == model.RingFence.Size)&&(a.ItemID == model.RingFence.ItemID)) select a);
            //    EcommInventory inventory;
            //    if (ecommQuery.Count() >0)
            //    {
            //        inventory = ecommQuery.First();
            //        inventory.Qty = model.RingFence.Qty;
            //        db.Entry(inventory).State = System.Data.EntityState.Modified;
            //        inventory.UpdateDate = DateTime.Now;
            //        inventory.UpdatedBy = UserName;
            //        db.SaveChanges();
            //    }
            //    else
            //    {
            //        inventory = new EcommInventory();
            //        inventory.Qty = model.RingFence.Qty;
            //        inventory.Store = model.RingFence.Store;
            //        inventory.Division = model.RingFence.Division;
            //        inventory.ItemID = model.RingFence.ItemID;
            //        inventory.Size = model.RingFence.Size;
            //        inventory.UpdateDate = DateTime.Now;
            //        inventory.UpdatedBy = UserName;
            //        db.EcommInventory.Add(inventory);
            //        db.SaveChanges();
            //    }

            //}


            db.SaveChanges(User.Identity.Name);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                return View(model);
            }
        }

        public ActionResult Delete(int ID)
        {
            string errorMessage;
            RingFenceDAO rfDAO = new RingFenceDAO();

            var rfQuery = (from a in db.RingFences where a.ID == ID select a);
            if (rfQuery.Count() == 0)
            {
                return RedirectToAction("Index", new { message = "Ringfence no longer exists." });
            }
            RingFence rf = rfQuery.First();

            if (!rfDAO.canUserUpdateRingFence(rf, UserName, out errorMessage))
            {
                return RedirectToAction("Index", new { message = errorMessage });
            }

            List<RingFenceDetail> details = (from a in db.RingFenceDetails
                                             where a.RingFenceID == ID
                                             select a).ToList();

            foreach (RingFenceDetail det in details)
            {
                db.RingFenceDetails.Remove(det);
                db.SaveChanges(User.Identity.Name);

                //if (rf.Type == 2)
                //{ 
                //    //ecomm warehouse, need to update inventory
                //    var ecomm = (from a in db.EcommInventory where ((a.Division == rf.Division)&&(a.Store == rf.Store)&&(a.Size== det.Size)) select a);
                //    if (ecomm.Count() > 0)
                //    {
                //        EcommInventory inv = ecomm.First();
                //        inv.Qty -= det.Qty;
                //        db.Entry(inv).State = System.Data.EntityState.Modified;
                //        db.SaveChanges(User.Identity.Name);
                //    }

                //}

            }

            db.RingFences.Remove(rf);
            db.SaveChanges(User.Identity.Name);

            return RedirectToAction("Index");
        }

        public ActionResult MassDeleteRingfence(string sku)
        {
            List<RingFence> ringfences = (from a in db.RingFences where a.Sku == sku select a).ToList();

            string message = BulkDeleteRingFences(sku, ringfences);

            return RedirectToAction("IndexSummary", new { message = message });
        }

        public ActionResult MassDeleteRingfenceStore(string div, string store)
        {
            List<RingFence> ringfences = (from a in db.RingFences where ((a.Division==div)&&(a.Store == store)) select a).ToList();

            string message = BulkDeleteRingFences(div + "-" + store, ringfences);

            return RedirectToAction("IndexByStore", new { message = message });
        }

        public ActionResult MassDeleteRingfenceFOB(string div, string store, string fob)
        {
            List<RingFence> rfList = (from a in db.RingFences
                                      join b in db.ItemMasters on a.ItemID equals b.ID
                                      join c in db.FOBDepts on b.Dept equals c.Department
                                      join d in db.FOBs on c.FOBID equals d.ID
                                      where ((a.Division == div) && (a.Store == store) && (d.Name == fob) &&(d.Division == div))
                                      select a).ToList();

            string message = BulkDeleteRingFences(div + "-" + store + " " + fob, rfList);

            return RedirectToAction("IndexByStore", new { message = message });
        }


        private string BulkDeleteRingFences(string key, List<RingFence> ringfences)
        {
            int count = 0;
            int permissionCount = 0;
            foreach (RingFence rf in ringfences)
            {
                if (!(WebSecurityService.UserHasDivision(UserName, "Allocation", rf.Division)))
                {
                    permissionCount++;
                }
                else if (!(WebSecurityService.UserHasDepartment(UserName, "Allocation", rf.Division, rf.Department)))
                {
                    permissionCount++;
                }
                else
                {
                    List<RingFenceDetail> details = (from a in db.RingFenceDetails
                                                     where a.RingFenceID == rf.ID
                                                     select a).ToList();

                    foreach (RingFenceDetail det in details)
                    {
                        db.RingFenceDetails.Remove(det);
                        db.SaveChanges(User.Identity.Name);
                    }

                    db.RingFences.Remove(rf);
                    count++;
                }
            }
            if (count>0)
            {
                db.SaveChanges(User.Identity.Name);
            }

            string message = "Deleted " + count + " ringfences for " + key + ".  ";
            if (permissionCount >0)
            {
                message = message + permissionCount + " you do not have permission.  ";
            }
            return message;
        }

        public ActionResult SelectStorePick(RingFence rf)
        {
            //FLOW:  Pick a division/store
            //show rdq's for everything on there, with a qty that they can input
            //they click save, it removes that amount
            //when all details have no qty, then delete the rdq

            RingFencePickModel model = new RingFencePickModel();
            model.RingFence = rf;
            model.Details = GetRingFenceDetails(rf.ID);
            model.Divisions = this.Divisions();
            return View(model);
        }

        public ActionResult PickStorePick(RingFencePickModel rf)
        {
            Boolean optionalPick = false;
            // Validate
            if (!ModelState.IsValid) { return View("SelectStorePick", rf); }

            // Apply pick database changes and update model object directly with any necessary validation messages
            //FLOW:  Pick a division/store
            //show rdq's for everything on there, with a qty that they can input
            //they click save, it removes that amount
            //when all details have no qty, then delete the rdq
            int pickedQty = 0;
            rf.Message = ValidateStorePick(rf);

            if (rf.RingFence.StartDate > (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == rf.Division select a.RunDate).First())
            {
                //startdate is the next control date, so make sure no details have POs if they do, warn them.
                optionalPick = true;
            }

            if (rf.Message.Length == 0)
            {
                //create RDQ's
                RingFenceHistory history = new RingFenceHistory();
                history.RingFenceID = rf.RingFence.ID;
                history.Action = "Picked";
                history.CreateDate = DateTime.Now;
                history.CreatedBy = User.Identity.Name;
                db.RingFenceHistory.Add(history);
                //db.SaveChanges(User.Identity.Name);

                List<RDQ> rdqsToCheck = new List<RDQ>();

                List<RingFenceDetail> deleteList = new List<RingFenceDetail>();
                foreach (RingFenceDetail det in rf.Details.Where(d => d.AssignedQty > 0))
                {
                    //TODO:  Create RDQ for each detail
                    RDQ rdq = new RDQ();
                    rdq.Sku = rf.RingFence.Sku;
                    rdq.Size = det.Size;
                    if (det.AssignedQty > det.Qty)
                    {
                        //don't let them take too much
                        rdq.Qty = det.Qty;
                    }
                    else
                    {
                        rdq.Qty = det.AssignedQty;
                    }
                    rdq.Store = rf.Store;
                    rdq.PO = det.PO;
                    rdq.Division = rf.Division;
                    rdq.DCID = det.DCID;
                    ItemMaster item = (from a in db.ItemMasters where a.MerchantSku == rdq.Sku select a).FirstOrDefault();
                    rdq.ItemID = item != null ? (long?)item.ID : null;
                    rdq.CreatedBy = User.Identity.Name;
                    rdq.CreateDate = DateTime.Now;
                    SetRDQDefaults(det, rdq);
                    if ((rdq.PO != null) && (rdq.PO != "") && optionalPick)
                    {
                        rdq.Type = "user_opt";
                        rf.Message = "Since this was created today, it will NOT be honored if the PO is delivered today.";
                    }

                    rdqsToCheck.Add(rdq);

                    history = new RingFenceHistory();
                    history.RingFenceID = det.RingFenceID;
                    history.DCID = det.DCID;
                    history.PO = det.PO;
                    history.Qty = det.AssignedQty;
                    //reduce ringfence detail or delete
                    if (det.AssignedQty >= det.Qty)
                    {
                        history.Action = "Picked Det";
                        if (det.PO == null)
                        {
                            det.PO = "";
                        }
                        RingFenceDetail delete = (from a in db.RingFenceDetails
                                                  where ((a.DCID == det.DCID) && 
                                                         (a.RingFenceID == det.RingFenceID) && 
                                                         (a.Size == det.Size) && 
                                                         (a.PO == det.PO))
                                                  select a).First();
                        pickedQty += det.Qty;
                        db.RingFenceDetails.Remove(delete);
                        //det.Qty -= det.AssignedQty;
                        //det.AssignedQty = 0;
                        deleteList.Add(det);
                        //db.SaveChanges(User.Identity.Name);
                    }
                    else if (det.AssignedQty > 0)
                    {
                        history.Action = "Partial Picked Det";

                        // Get current detail record (so EF doesnt geek out about about attaching an entity that hasnt been loaded yet)
                        var currDetail = db.RingFenceDetails.First(d => d.RingFenceID == det.RingFenceID && d.Size == det.Size && (d.DCID == det.DCID));

                        // Decrement quantity from detail record that was picked from
                        currDetail.Qty = currDetail.Qty - det.AssignedQty;
                        currDetail.PO = det.PO ?? String.Empty;
                        //db.SaveChanges(User.Identity.Name);

                        pickedQty += det.AssignedQty;
                    }

                    history.CreateDate = DateTime.Now;
                    history.CreatedBy = User.Identity.Name;
                    db.RingFenceHistory.Add(history);
                    //db.SaveChanges(User.Identity.Name);

                }
                ////reduce qty on header
                //RingFence header = (from a in db.RingFences where a.ID == rf.RingFence.ID select a).First();
                //header.Qty = header.Qty - pickedQty;
                //db.SaveChanges(User.Identity.Name);

                int holdCount = CheckHolds(rf.Division, rdqsToCheck);
                if (holdCount > 0)
                {
                    rf.Message = holdCount + " on hold.  Please see ReleaseHeldRDQs for held RDQs.";
                }

                int cancelHoldCount = CheckCancelHolds(rf.Division, rdqsToCheck);
                if (cancelHoldCount > 0)
                {
                    rf.Message = cancelHoldCount + " rejected because of cancel inventory hold.";
                }

                foreach (RDQ rdq in rdqsToCheck)
                {
                    db.RDQs.Add(rdq);
                }

                foreach (RingFenceDetail det in deleteList)
                {
                    rf.Details.Remove(det);
                }
                //remove ringfence if no details left
                if (rf.Details.Count() == 0)
                {
                    RingFence deleteRF = (from a in db.RingFences where a.ID == history.RingFenceID select a).First();
                    db.RingFences.Remove(deleteRF);
                    //db.SaveChanges(User.Identity.Name);
                    rf.Message += " Ring fence picked, no quantity remaining.";
                }
                else
                {
                    rf.Message += " Ring fence picked, quantity remaining.";
                }
                db.SaveChanges(User.Identity.Name);
            }

            return View(rf);
        }

        private int CheckHolds(string division, List<RDQ> list)
        {
            int instance = (from a in db.InstanceDivisions where a.Division == division select a.InstanceID).First();
            return (new RDQDAO()).ApplyHolds(list, instance);
        }

        private int CheckHolds(string division, RDQ rdq)
        {
            List<RDQ> list = new List<RDQ>();
            list.Add(rdq);
            int instance = (from a in db.InstanceDivisions where a.Division == division select a.InstanceID).First();
            return (new RDQDAO()).ApplyHolds(list, instance);
        }

        private int CheckCancelHolds(string division, List<RDQ> list)
        {
            int instance = (from a in db.InstanceDivisions where a.Division == division select a.InstanceID).First();
            return (new RDQDAO()).ApplyCancelHolds(list);
        }

        private int CheckCancelHolds(string division, RDQ rdq)
        {
            List<RDQ> list = new List<RDQ>();
            list.Add(rdq);
            int instance = (from a in db.InstanceDivisions where a.Division == division select a.InstanceID).First();
            return (new RDQDAO()).ApplyCancelHolds(list);
        }

        private static void SetRDQDefaults(RingFenceDetail det, RDQ rdq)
        {
            rdq.Type = "user";
            rdq.Status = "WEB PICK";
            if ((det.PO != null) && (det.PO != "") && (det.PO != "N/A"))
            {
                rdq.DestinationType = "CROSSDOCK";
                rdq.Status = "HOLD-XDC";
            }
            else
            {
                rdq.DestinationType = "WAREHOUSE";
            }
        }

        public ActionResult Pick(int ID)
        {
            Boolean optionalPick = false;
            string message=null;
            string errorMessage;
            RingFenceDAO rfDAO = new RingFenceDAO();

            var ringfenceQuery = (from a in db.RingFences where a.ID == ID select a);
            if (ringfenceQuery.Count() == 0)
            {
                return RedirectToAction("Index", new { message = "Ringfence no longer exists.  Please verify.  " });
            }
            RingFence rf = ringfenceQuery.First();

            if (!rfDAO.canUserUpdateRingFence(rf, UserName, out errorMessage))
            {
                return RedirectToAction("Index", new { message = errorMessage });
            }

            if (rf.Type == 2)
            { 
                //this is an ecomm ringfence, you can't pick it
                return RedirectToAction("Index", new { message = "Sorry, you cannot pick for an Ecomm warehouse.  Do you mean Delete?" });
            }
            if (rf.StartDate > (from a in db.ControlDates
                                      join b in db.InstanceDivisions 
                                          on a.InstanceID equals b.InstanceID
                                where b.Division == rf.Division
                                select a.RunDate).First())
            { 
                //startdate is the next control date, so make sure no details have POs if they do, warn them.
                optionalPick = true;
            }
            if ((rf.Store == null)||(rf.Store == ""))
            {
                //FLOW:  Pick a division/store
                //show rdq's for everything on there, with a qty that they can input
                //they click save, it removes that amount
                //when all details have no qty, then delete the rdq
                return RedirectToAction("SelectStorePick", rf);
            }
            List<RingFenceDetail> details = (from a in db.RingFenceDetails
                                             where a.RingFenceID == ID &&
                                                   a.ActiveInd == "1"
                                             select a).ToList();

            RingFenceHistory history = new RingFenceHistory();
            history.RingFenceID = ID;
            history.Action = "Picked";
            history.CreateDate = DateTime.Now;
            history.CreatedBy = User.Identity.Name;
            db.RingFenceHistory.Add(history);
            db.SaveChanges(User.Identity.Name);


            List<RDQ> rdqsToCheck = new List<RDQ>();
            foreach (RingFenceDetail det in details)
            {
                //TODO:  Create RDQ for each detail
                RDQ rdq = new RDQ();
                rdq.Sku = rf.Sku;
                rdq.Size = det.Size;
                rdq.Qty = det.Qty;
                rdq.Store = rf.Store;
                rdq.PO = det.PO;
                rdq.Division = rf.Division;
                rdq.DCID = det.DCID;
                rdq.ItemID = (from a in db.ItemMasters where a.MerchantSku == rf.Sku select a).FirstOrDefault().ID;
                rdq.CreatedBy = User.Identity.Name;
                rdq.CreateDate = DateTime.Now;
                SetRDQDefaults(det, rdq);
                if ((rdq.PO != null) && (rdq.PO != "") && optionalPick)
                { 
                    rdq.Type = "user_opt";
                    message = "Since this was created today, it will NOT be honored if the PO is delivered today.";
                }
                rdqsToCheck.Add(rdq);

                db.RingFenceDetails.Remove(det);
                //db.SaveChanges(User.Identity.Name);

                history = new RingFenceHistory();
                history.RingFenceID = det.RingFenceID;
                history.DCID = det.DCID;
                history.PO = det.PO;
                history.Qty = det.Qty;
                history.Action = "Picked Det";
                history.CreateDate = DateTime.Now;
                history.CreatedBy = User.Identity.Name;
                db.RingFenceHistory.Add(history);
                db.SaveChanges(User.Identity.Name);
            }

            int holdCount = CheckHolds(rf.Division, rdqsToCheck);
            if (holdCount > 0)
            {
                message = holdCount + " on hold.  Please see ReleaseHeldRDQs for held RDQs.  ";
            }

            int cancelHoldCount = CheckCancelHolds(rf.Division, rdqsToCheck);
            if (cancelHoldCount > 0)
            {
                message = cancelHoldCount + " rejected because of cancel inventory hold.";
            }
            foreach (RDQ rdq in rdqsToCheck)
            {
                db.RDQs.Add(rdq);
//                db.SaveChanges(User.Identity.Name);
            }

            db.RingFences.Remove(rf);
            db.SaveChanges(User.Identity.Name);

            return RedirectToAction("Index", new { message = message });
        }

        public ActionResult MassPickRingFence(string sku)
        {
            List<RingFence> rfList = (from a in db.RingFences where a.Sku == sku select a).ToList();
            string message = BulkPickRingFence(sku, rfList, true);

            return RedirectToAction("IndexSummary", new { message = message });
        }

        public ActionResult MassPickRingFenceNoFuture(string sku)
        {
            List<RingFence> rfList = (from a in db.RingFences where a.Sku == sku select a).ToList();
            string message = BulkPickRingFence(sku, rfList, false);

            return RedirectToAction("IndexSummary", new { message = message });
        }

        private string BulkPickRingFence(string key, List<RingFence> rfList, Boolean pickPOs)
        {
            int noStoreCount = 0;
            int ecommCount = 0;
            int count = 0;
            int countWarehouseOnly = 0;
            int errorCount = 0;
            int permissionCount = 0;
            int deliveredToday = 0;
            Boolean optionalPick = false;
            List<RDQ> rdqs = new List<RDQ>();
            Boolean canDelete = true;

            foreach (RingFence rf in rfList)
            {
                if (!(WebSecurityService.UserHasDivision(UserName, "Allocation", rf.Division)))
                {
                    permissionCount++;
                }
                else if (!(WebSecurityService.UserHasDepartment(UserName, "Allocation", rf.Division, rf.Department)))
                {
                    permissionCount++;
                }
                else if (rf.Type == 2)
                {
                    //this is an ecomm ringfence, skip it
                    ecommCount++;
                }
                else if ((rf.Store == null) || (rf.Store == ""))
                {
                    //FLOW:  Pick a division/store
                    //show rdq's for everything on there, with a qty that they can input
                    //they click save, it removes that amount
                    //when all details have no qty, then delete the rdq
                    noStoreCount++;
                    //return RedirectToAction("SelectStorePick", rf);
                }
                else
                {
                    if (rf.StartDate > (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == rf.Division select a.RunDate).First())
                    {
                        //startdate is the next control date, so make sure no details have POs if they do, warn them.
                        optionalPick = true;
                    }
                    try
                    {
                        List<RingFenceDetail> details = (from a in db.RingFenceDetails
                                                         where a.RingFenceID == rf.ID
                                                         select a).ToList();
                        int detCount = details.Count();
                        if (!pickPOs)
                        {
                            details = (from a in details where a.PO == "" select a).ToList();
                            canDelete = (detCount == details.Count());
                            if (details.Count() > 0)
                            {
                                countWarehouseOnly++;
                            }
                        }

                        RingFenceHistory history = new RingFenceHistory();
                        history.RingFenceID = rf.ID;
                        history.Action = "Picked";
                        history.CreateDate = DateTime.Now;
                        history.CreatedBy = User.Identity.Name;
                        db.RingFenceHistory.Add(history);
                        //db.SaveChanges(User.Identity.Name);



                        foreach (RingFenceDetail det in details)
                        {
                            //TODO:  Create RDQ for each detail
                            RDQ rdq = new RDQ();
                            rdq.Sku = rf.Sku;
                            rdq.Size = det.Size;
                            rdq.Qty = det.Qty;
                            rdq.Store = rf.Store;
                            rdq.PO = det.PO;
                            rdq.Division = rf.Division;
                            rdq.DCID = det.DCID;
                            rdq.ItemID = (from a in db.ItemMasters where a.MerchantSku == rf.Sku select a).FirstOrDefault().ID;
                            rdq.CreatedBy = User.Identity.Name;
                            rdq.CreateDate = DateTime.Now;
                            SetRDQDefaults(det, rdq);
                            if ((rdq.PO != null) && (rdq.PO != "") && optionalPick)
                            {
                                rdq.Type = "user_opt";
                                deliveredToday++;
                            }

                            db.RDQs.Add(rdq);
                            //db.SaveChanges(User.Identity.Name);
                            rdqs.Add(rdq);

                            db.RingFenceDetails.Remove(det);
                            //db.SaveChanges(User.Identity.Name);

                            history = new RingFenceHistory();
                            history.RingFenceID = det.RingFenceID;
                            history.DCID = det.DCID;
                            history.PO = det.PO;
                            history.Qty = det.Qty;
                            history.Action = "Picked Det";
                            history.CreateDate = DateTime.Now;
                            history.CreatedBy = User.Identity.Name;
                            db.RingFenceHistory.Add(history);
                            //db.SaveChanges(User.Identity.Name);

                        }

                        if (canDelete)
                        {
                            db.RingFences.Remove(rf);
                        }
                        db.SaveChanges(User.Identity.Name);
                        count++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                    }
                }
            }

            int holdCount = 0;
            int cancelholdcount = 0;
            if (rdqs.Count() > 0)
            {
                string div = rfList[0].Division;
                int instance = (from a in db.InstanceDivisions where a.Division == div select a.InstanceID).First();
                RDQDAO rdqDAO = new RDQDAO();
                holdCount = rdqDAO.ApplyHolds(rdqs, instance);
                cancelholdcount = rdqDAO.ApplyCancelHolds(rdqs);
            }

            string message = "Picked " + count + " Ringfences for " + key + ".  ";
            if (!pickPOs)
            {
                message = message + countWarehouseOnly + " had warehouse inventory.  ";
            }
            if (errorCount > 0)
            {
                message = message + errorCount + " Errors.  ";
            }
            if (ecommCount > 0)
            {
                message = message + ecommCount + " Ecomm Ringfences (excluded).  ";
            }
            if (noStoreCount > 0)
            {
                message = message + noStoreCount + " did not have store (excluded).  ";
            }
            if (permissionCount > 0)
            {
                message = message + permissionCount + " you do not have permissions.  ";
            }
            if (deliveredToday > 0)
            {
                message = message + deliveredToday + " created today, will NOT be honored if the PO is delivered today.  ";
            }
            if (holdCount > 0)
            {
                message = message + holdCount + " on hold.  See ReleaseHeldRDQs to view held RDQs.  ";
            }
            if (cancelholdcount > 0)
            {
                message = message + cancelholdcount + " rejected by cancel inventory hold.  ";
            }
            return message;
        }

        public ActionResult MassPickRingFenceStore(string div, string store)
        {
            div = div.Trim();
            store = store.Trim();

            List<RingFence> rfList = (from a in db.RingFences where ((a.Division==div)&&(a.Store==store)) select a).ToList();
            string message = BulkPickRingFence(div + "-" + store, rfList, true);

            return RedirectToAction("IndexByStore", new { message = message });
        }

        public ActionResult MassPickRingFenceStoreNoFuture(string div, string store)
        {
            div = div.Trim();
            store = store.Trim();

            List<RingFence> rfList = (from a in db.RingFences where ((a.Division == div) && (a.Store == store)) select a).ToList();
            string message = BulkPickRingFence(div + "-" + store, rfList, false);

            return RedirectToAction("IndexByStore", new { message = message });
        }

        public ActionResult MassPickRingFenceFOB(string div, string store, string fob)
        {
            div = div.Trim();
            store = store.Trim();
            fob = fob.Trim();

            List<RingFence> rfList = (from a in db.RingFences 
                                      join b in db.ItemMasters on a.ItemID equals b.ID
                                      join c in db.FOBDepts on b.Dept equals c.Department
                                      join d in db.FOBs on c.FOBID equals d.ID
                                      where ((a.Division == div) && (a.Store == store) && (d.Name == fob && (d.Division == div)))
                                      select a).ToList();
            string message = BulkPickRingFence(div + "-" + store + " " + fob, rfList, true);

            return RedirectToAction("IndexByStore", new { message = message });
        }

        public ActionResult MassPickRingFenceFOBNoFuture(string div, string store, string fob)
        {
            div = div.Trim();
            store = store.Trim();
            fob = fob.Trim();

            List<RingFence> rfList = (from a in db.RingFences
                                      join b in db.ItemMasters on a.ItemID equals b.ID
                                      join c in db.FOBDepts on b.Dept equals c.Department
                                      join d in db.FOBs on c.FOBID equals d.ID
                                      where ((a.Division == div) && (a.Store == store) && (d.Name == fob && (d.Division == div)))
                                      select a).ToList();
            string message = BulkPickRingFence(div + "-" + store + " " + fob, rfList, false);

            return RedirectToAction("IndexByStore", new { message = message });
        }

        public ActionResult MassEditRingFence(string sku)
        {
            MassEditRingFenceModel model = new MassEditRingFenceModel();
            model.Sku = sku;
            return View(model);
        }

        [HttpPost]
        public ActionResult MassEditRingFence(MassEditRingFenceModel model)
        {
            List<RingFence> rfList = (from a in db.RingFences where (a.Sku == model.Sku) select a).ToList();

            string message;
            message = SaveMassEditRingFence(model, rfList);

            return RedirectToAction("IndexSummary", new { message = message });
        }

        private string SaveMassEditRingFence(MassEditRingFenceModel model, List<RingFence> rfList)
        {
            string message;
            int count = 0;
            int ecommCount = 0;
            int permissionCount = 0;
            string errorMessage;
            RingFenceDAO rfDAO = new RingFenceDAO();

            foreach (RingFence rf in rfList)
            {
                if (!rfDAO.canUserUpdateRingFence(rf, UserName, out errorMessage))
                {
                    permissionCount++;
                }
                else if (rf.Type == 2)
                {
                    //this is an ecomm ringfence, skip it
                    ecommCount++;
                }
                else
                {
                    rf.CreatedBy = User.Identity.Name;
                    rf.CreateDate = DateTime.Now;

                    if (model.EndDate != null)
                    {
                        rf.EndDate = model.EndDate;
                    }
                    if ((model.Comment != null)&&(model.Comment != ""))
                    {
                        rf.Comments = model.Comment;
                    }
                    db.Entry(rf).State = System.Data.EntityState.Modified;
                    count++;
                }
            }
            if (count > 0)
            {
                db.SaveChanges(UserName);
                Session["rfStore"] = null;
                Session["rfFOBStore"] = null;
            }

            message = "Updated " + count + " ringfences for ";

            if (model.Sku != null)
            {
                message = message + model.Sku + ".  ";
            }
            else if (model.FOB != null)
            {
                message = message + model.Div + "-" + model.Store + " " + model.FOB + ".  ";            
            }
            else
            {
                message = message + model.Div + "-" + model.Store + ".  ";
            }

            if (ecommCount > 0)
            {
                message = message + ecommCount + " ecomm stores (excluded).  ";
            }
            if (permissionCount > 0)
            {
                message = message + permissionCount + " permission denied.  ";
            }
            return message;
        }

        public ActionResult MassEditRingFenceStore(string div, string store, string fob)
        {
            MassEditRingFenceModel model = new MassEditRingFenceModel();
            model.Div = div;
            model.Store = store;
            model.FOB = fob;
            return View(model);
        }

        [HttpPost]
        public ActionResult MassEditRingFenceStore(MassEditRingFenceModel model)
        {
            List<RingFence> rfList;



            if ((model.FOB != null) && (model.FOB != ""))
            {
                rfList = (from a in db.RingFences 
                          join b in db.ItemMasters on a.ItemID equals b.ID
                          join c in db.FOBDepts on b.Dept equals c.Department
                          join d in db.FOBs on c.FOBID equals d.ID
                          where ((a.Division == model.Div) && (a.Store == model.Store) && (d.Name == model.FOB) && (d.Division == model.Div)) select a).ToList();
            }
            else
            {
                rfList = (from a in db.RingFences where ((a.Division == model.Div) && (a.Store == model.Store)) select a).ToList();
            }
            string message;
            message = SaveMassEditRingFence(model, rfList);
            return RedirectToAction("IndexByStore", new { message = message });
        }

        [GridAction]
        public ActionResult Ajax_GetPackDetails(Int64 ringFenceID, string packName)
        {
            var packDetails = new List<ItemPackDetail>();

            // Get pack's item
            var item = db.RingFences.Single(rf => rf.ID == ringFenceID).Sku;
            var itemID = db.ItemMasters.Single(i => i.MerchantSku == item).ID;

            // Get pack by item/pack name
            var pack = db.ItemPacks.Include("Details").FirstOrDefault(p => p.ItemID == itemID && p.Name == packName);
            if(pack != null)
            {
                packDetails = pack.Details.OrderBy(p=> p.Size).ToList();
            }

            return View(new GridModel(packDetails));
        }

        [GridAction]
        public ActionResult _SelectBatchEditing(Int64 ringFenceID)
        {
            // Get Ring Fence data
            var details = GetRingFenceDetails(ringFenceID);
            RingFence rf = (from a in db.RingFences
                            where a.ID == ringFenceID
                            select a).First();
            RingFenceDAO dao = new RingFenceDAO();
            List<RingFenceDetail> stillAvailable = dao.GetWarehouseAvailable(rf);
            stillAvailable.AddRange(dao.GetFuturePOs(rf));

            RingFenceDetail existing;
            foreach (RingFenceDetail det in stillAvailable)
            {
                var query = (from a in details
                             where ((a.Size == det.Size) && 
                                    (a.PO == det.PO) && 
                                    (a.Warehouse == det.Warehouse))
                             select a);
                if (query.Count() > 0)
                {
                    existing = query.First();
                    det.Qty = existing.Qty;
                    //det.AvailableQty += existing.Qty;
                    det.RingFenceID = existing.RingFenceID;
                }
            }

            return View(new GridModel(stillAvailable));
        }

        [GridAction]
        public ActionResult _SelectWarehouses(Int64 ringFenceID)
        {
            RingFence ringFence = (from a in db.RingFences where a.ID == ringFenceID select a).First();
            return View(new GridModel(GetWarehouseAvailable(ringFence)));
        }

        [GridAction]
        public ActionResult _SelectPOs(Int64 ringFenceID)
        {
            RingFence ringFence = (from a in db.RingFences where a.ID == ringFenceID select a).First();
            return View(new GridModel(GetFutureAvailable(ringFence)));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _SaveBatchEditing([Bind(Prefix = "updated")]IEnumerable<RingFenceDetail> updated)
        {
            string errorMessage;
            RingFenceDAO rfDAO = new RingFenceDAO();

            long ringFenceID = (from a in updated select a.RingFenceID).First();
            RingFence ringFence = null;
            List<RingFenceDetail> available = null;

            ringFence = (from a in db.RingFences
                         where a.ID == ringFenceID
                         select a).First();


            available = GetWarehouseAvailable(ringFence);
            available.AddRange(GetFutureAvailable(ringFence));
            
            if (updated != null)
            {                
                foreach (RingFenceDetail det in updated)
                {
                    det.Message = "";

                    if (!rfDAO.canUserUpdateRingFence(ringFence, UserName, out errorMessage))
                    {                        
                        det.Message += errorMessage;
                    }

                    det.AvailableQty = (from a in available
                                        where ((a.PO == det.PO) &&
                                               (a.Warehouse == det.Warehouse) &&
                                               (a.Size == det.Size))
                                        select a.AvailableQty).FirstOrDefault();

                    if (isRingFenceDetailValid(det))
                    {
                        if (det.Qty > 0)
                        {
                            det.ActiveInd = "1";

                            if (string.IsNullOrEmpty(det.PO))
                                det.ringFenceStatusCode = "4";
                            else
                                det.ringFenceStatusCode = "1";

                            if ((from a in db.RingFenceDetails
                                 where ((a.RingFenceID == det.RingFenceID) &&
                                        (a.DCID == det.DCID) &&
                                        (a.Size == det.Size) &&
                                        (a.PO == det.PO))
                                 select a).Count() > 0)
                            {
                                db.Entry(det).State = System.Data.EntityState.Modified;
                            }
                            else
                            {
                                db.Entry(det).State = System.Data.EntityState.Added;
                            }

                            det.LastModifiedDate = DateTime.Now;
                            det.LastModifiedUser = User.Identity.Name;

                            db.SaveChanges(User.Identity.Name);
                        }
                    }
                }
            }

            // Build up viewmodel to be returned, (add errors if necessary)
            List<RingFenceDetail> details = (from a in db.RingFenceDetails
                                             where a.RingFenceID == ringFenceID &&
                                                   a.ActiveInd == "1"
                                             select a).ToList();

            List<RingFenceDetail> final = new List<RingFenceDetail>();

            foreach (RingFenceDetail det in available)
            {
                var updateRecord = (from a in updated
                                    where a.DCID == det.DCID &&
                                          a.Size == det.Size &&
                                          a.PO == det.PO
                                    select a).FirstOrDefault();

                var detailRecord = (from a in details
                                    where a.DCID == det.DCID &&
                                          a.Size == det.Size &&
                                          a.PO == det.PO
                                    select a).FirstOrDefault();

                if (updateRecord != null)
                    final.Add(updateRecord);
                else if (detailRecord != null)
                {
                    det.Qty = detailRecord.Qty;
                    final.Add(det);
                }
                else
                    final.Add(det);
            }

            return View(new GridModel(final));
        }

        bool isRingFenceDetailValid(RingFenceDetail rfDetail)
        {
            if (rfDetail.Qty > rfDetail.AvailableQty)
            {
                if (string.IsNullOrEmpty(rfDetail.PO))
                    rfDetail.Message = "Max Quantity for " + rfDetail.Warehouse + " is " + rfDetail.AvailableQty;
                else
                    rfDetail.Message = "Max Quantity for PO  " + rfDetail.PO + " is " + rfDetail.AvailableQty;

                return false;
            }

            if (rfDetail.Qty < 0)
            {
                rfDetail.Message = "You cannot ring fence a negative number";
                return false;
            }

            return true;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _SaveBatchInsert([Bind(Prefix = "updated")]IEnumerable<RingFenceDetail> updated)
        {
            List<RingFenceDetail> available = null;
            RingFenceDAO rfDAO = new RingFenceDAO();
            string PO;

            if (updated != null)
            {
                string message = "";
                RingFence ringFence = null;

                int availableQty;
                long ringFenceID = updated.First().RingFenceID;
                PO = updated.First().PO;

                ringFence = (from a in db.RingFences
                             where a.ID == ringFenceID
                             select a).First();

                if (string.IsNullOrEmpty(PO))
                {
                    available = GetWarehouseAvailable(ringFence);
                }
                else
                {
                    available = GetFutureAvailable(ringFence);                    
                }

                bool ecommwarehouse = rfDAO.isEcommWarehouse(ringFence.Division, ringFence.Store);

                if (!ecommwarehouse)
                {
                    foreach (RingFenceDetail det in updated)
                    {
                        if (isRingFenceDetailValid(det))
                        {
                            if (det.Qty > 0)
                            {
                                var exists = (from a in db.RingFenceDetails
                                              where ((a.RingFenceID == det.RingFenceID) &&
                                                     (a.DCID == det.DCID) &&
                                                     (a.Size == det.Size) &&
                                                     (a.PO == det.PO))
                                              select a);

                                if (exists.Count() == 0)
                                {
                                    det.Message = "";
                                    det.ActiveInd = "1";

                                    if (det.PO == "")
                                        det.ringFenceStatusCode = "4";
                                    else
                                        det.ringFenceStatusCode = "1";

                                    db.RingFenceDetails.Add(det);
                                }
                                else
                                {
                                    RingFenceDetail existingDet = exists.First();
                                    existingDet.Qty = det.Qty;
                                    existingDet.ActiveInd = "1";
                                    db.Entry(existingDet).State = EntityState.Modified;
                                }

                                if (det.PO != "")
                                {
                                    List<ExistingPO> poList = (new ExistingPODAO()).GetExistingPO(ringFence.Division, det.PO);

                                    foreach (ExistingPO po in poList)
                                    {
                                        if (po.ExpectedDeliveryDate < DateTime.Now)
                                        {
                                            det.Message = "This PO is expected for delivery today. If it does, this ringfence will NOT be enforced (it will be deleted).";
                                        }
                                    }
                                    //available.Insert(0, det);
                                }

                                det.LastModifiedDate = DateTime.Now;
                                det.LastModifiedUser = User.Identity.Name;

                                db.SaveChanges(User.Identity.Name);
                            }
                        }
                    }

                    List<RingFenceDetail> details = (from a in db.RingFenceDetails
                                                     where a.RingFenceID == ringFenceID &&
                                                           a.ActiveInd == "1"
                                                     select a).ToList();

                    List<RingFenceDetail> final = new List<RingFenceDetail>();

                    foreach (RingFenceDetail det in available)
                    {
                        var updateRecord = (from a in updated
                                            where a.DCID == det.DCID &&
                                                  a.Size == det.Size &&
                                                  a.PO == det.PO
                                            select a).FirstOrDefault();

                        var detailRecord = (from a in details
                                            where a.DCID == det.DCID &&
                                                  a.Size == det.Size &&
                                                  a.PO == det.PO
                                            select a).FirstOrDefault();

                        if (updateRecord != null)
                            final.Add(updateRecord);
                        else if (detailRecord != null)
                        {
                            det.Qty = detailRecord.Qty;
                            final.Add(det);
                        }                            
                        else
                            final.Add(det);
                    }
                    return View(new GridModel(final));
                }
                else
                {
                    //Ecomm RingFence we need to create ecomm inventory
                    List<RingFenceDetail> futures = GetFutureAvailable(ringFence);
                    List<RingFenceDetail> warehouse = GetWarehouseAvailable(ringFence);

                    if (ringFence.Store == "00800")
                    {
                        List<RingFenceUploadModel> list = new List<RingFenceUploadModel>();
                        List<RingFenceUploadModel> errorlist = new List<RingFenceUploadModel>();
                        List<RingFenceDetail> outputList = new List<RingFenceDetail>();
                        RingFenceUploadModel model = new RingFenceUploadModel();
                        foreach (RingFenceDetail det in updated)
                        {
                            if (isRingFenceDetailValid(det))
                            {
                                if (det.Qty > 0)
                                {
                                    model = new RingFenceUploadModel();
                                    model.SKU = ringFence.Sku;
                                    model.Size = det.Size;
                                    model.PO = det.PO;
                                    model.Comments = ringFence.Comments;
                                    model.Qty = Convert.ToString(det.Qty);

                                    model.Store = ringFence.Store;
                                    model.Warehouse = det.Warehouse;
                                    model.Division = ringFence.Division;
                                    model.EndDate = Convert.ToString(ringFence.EndDate);

                                    list.Add(model);
                                }
                            }
                        }

                        if (list.Count() > 0)
                        {
                            CreateOrUpdateRingFence(ringFence.Division, ringFence.Store, ringFence.Sku, list, errorlist, warehouse, futures);
                            List<string> errors = (from a in errorlist
                                          where (!(a.ErrorMessage.StartsWith("Warning")))
                                          select a.ErrorMessage).ToList();

                            if (errors.Count() == 0)
                            {
                                RingFenceDetail finishMsg = new RingFenceDetail();

                                finishMsg.Qty = ringFence.Qty;  
                                finishMsg.Message = "This data has been processed.";
                                outputList.Add(finishMsg);
                            }
                            else
                            {
                                RingFenceDetail finishMsg = new RingFenceDetail();
                                
                                finishMsg.Message = errors.ToString();
                                outputList.Add(finishMsg);
                            }
                        }

                        return View(new GridModel(outputList));
                    }
                    else
                    {
                        //ecomm all countries store
                        //EcommInventory ecommInv = new EcommInventory();
                        RingFenceDetail newDet = new RingFenceDetail();
                        List<EcommWeight> weights;

                        weights = new List<EcommWeight>();
                        EcommWeight weight = new EcommWeight();
                        weight.Division = ringFence.Division;
                        weight.Store = ringFence.Store;
                        weight.Weight = 1;
                        weights.Add(weight);
                        Boolean addDetail;
                        foreach (EcommWeight w in weights)
                        {
                            RingFence rf = (from a in db.RingFences
                                            where ((a.Sku == ringFence.Sku) && (a.Store == w.Store))
                                            select a).First();
                            foreach (RingFenceDetail det in updated)
                            {
                                availableQty = 0;
                                try
                                {
                                    availableQty += (from a in futures
                                                     where ((a.PO == det.PO) && 
                                                            (a.Size == det.Size) && 
                                                            (a.DCID == det.DCID))
                                                     select a.AvailableQty).Sum();
                                }
                                catch { }
                                try
                                {
                                    availableQty += (from a in warehouse
                                                     where ((a.Size == det.Size) && (a.DCID == det.DCID))
                                                     select a.AvailableQty).Sum();
                                }
                                catch { }

                                if ((det.Qty > availableQty) && (det.Qty > 0))
                                {
                                    message = message + "Max Qty for " + det.Warehouse + " " + det.PO + " is " + (det.AvailableQty);
                                    det.Message = message;
                                }
                                else
                                {
                                    addDetail = false;
                                    det.Size = det.Size.Trim();
                                    newDet = (from a in db.RingFenceDetails
                                              where ((a.Size == det.Size) && 
                                                     (a.RingFenceID == rf.ID) && 
                                                     (a.ActiveInd == "1"))                                              
                                              select a).FirstOrDefault();
                                    if (newDet == null)
                                    {
                                        newDet = new RingFenceDetail();
                                        addDetail = true;
                                        newDet.Size = det.Size;
                                    }
                                    newDet.DCID = det.DCID;
                                    newDet.ActiveInd = "1";
                                    newDet.LastModifiedDate = DateTime.Now;
                                    newDet.LastModifiedUser = User.Identity.Name;
                                    if (det.PO == null)
                                    {
                                        newDet.PO = "";
                                        newDet.ringFenceStatusCode = "4";
                                    }
                                    else
                                    {
                                        newDet.PO = det.PO;
                                        newDet.ringFenceStatusCode = "1";
                                    }
                                    newDet.Qty = Convert.ToInt32(det.Qty * w.Weight);

                                    newDet.RingFenceID = rf.ID;
                                    if (addDetail)
                                    {
                                        db.RingFenceDetails.Add(newDet);
                                    }

                                    //save individually so the logic to automatically calculate the total works
                                    db.SaveChanges(User.Identity.Name);
                                }
                            }
                        }
                        return View(new GridModel(updated));
                    }
                }
            }
            return View(new GridModel(updated));
        }

        public ActionResult Upload()
        {
            return View();
        }

        private RingFenceUploadModel createModelFromSpreadsheet(Cells spreadsheet, int row)
        {
            RingFenceUploadModel model = new RingFenceUploadModel();

            model.Store = Convert.ToString(spreadsheet[row, 1].Value);
            model.Division = Convert.ToString(spreadsheet[row, 0].Value);
            model.Comments = Convert.ToString(spreadsheet[row, 9].Value);
            model.EndDate = Convert.ToString(spreadsheet[row, 3].Value);
            model.PO = Convert.ToString(spreadsheet[row, 4].Value);
            model.Qty = Convert.ToString(spreadsheet[row, 8].Value);

            if (Convert.ToString(spreadsheet[row, 6].Value) != "")
            {
                model.Size = Convert.ToString(spreadsheet[row, 6].Value).PadLeft(3, '0');
            }
            else
            {
                model.Size = Convert.ToString(spreadsheet[row, 7].Value).PadLeft(5, '0');
            }

            model.SKU = Convert.ToString(spreadsheet[row, 2].Value);
            model.Warehouse = Convert.ToString(spreadsheet[row, 5].Value).PadLeft(2, '0');
            model.PO = Convert.ToString(spreadsheet[row, 4].Value);

            return model;
        }

        private bool validateUploadModel(RingFenceUploadModel model)
        {
            if (!(Footlocker.Common.WebSecurityService.UserHasDivision(UserName, "allocation", model.Division)))
            {
                model.ErrorMessage = "You are not authorized to update division " + model.Division;
                return false;
            }
            else if (model.SKU.Substring(0, 2) != model.Division)
            {
                model.ErrorMessage = "Division doesn't match";
                return false;
            }
            else if (model.Store.Length == 0)
            {
                model.ErrorMessage = "Store is required";
                return false;
            }
            else if (model.SKU.Length == 0)
            {
                model.ErrorMessage = "Sku is required";
                return false;
            }
            else if (model.Size.Length == 0)
            {
                model.ErrorMessage = "Size is required";
                return false;
            }
            else if (model.Warehouse.Length == 0)
            {
                model.ErrorMessage = "Warehouse is required";
                return false;
            }

            return true;
        }

        public ActionResult SaveRingFences(IEnumerable<HttpPostedFileBase> attachments)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            string Division = "";
            RingFenceDAO rfDAO = new RingFenceDAO();
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

                int row = 1;
                if ((Convert.ToString(mySheet.Cells[0, 0].Value).Contains("Div")) && (Convert.ToString(mySheet.Cells[0, 1].Value).Contains("Store")) &&
                    (Convert.ToString(mySheet.Cells[0, 2].Value).Contains("SKU")) && (Convert.ToString(mySheet.Cells[0, 3].Value).Contains("EndDate")) &&
                    (Convert.ToString(mySheet.Cells[0, 4].Value).Contains("PO")) && (Convert.ToString(mySheet.Cells[0, 5].Value).Contains("Warehouse")) &&
                    (Convert.ToString(mySheet.Cells[0, 6].Value).Contains("Size")) && (Convert.ToString(mySheet.Cells[0, 7].Value).Contains("Caselot")) &&
                    (Convert.ToString(mySheet.Cells[0, 8].Value).Contains("Qty")) && (Convert.ToString(mySheet.Cells[0, 9].Value).Contains("Comments")) 
                    )
                {
                    Division = Convert.ToString(mySheet.Cells[row, 0].Value).PadLeft(2, '0');

                    string sku,prevsku;
                    prevsku = "FIRST";
                    RingFenceDAO dao = new RingFenceDAO();
                    List<RingFenceDetail> Available=null;
                    List<RingFenceDetail> FuturePOs=null;

                    List<RingFenceUploadModel> ProcessList = new List<RingFenceUploadModel>();
                    List<RingFenceUploadModel> Errors = new List<RingFenceUploadModel>();
                    RingFenceUploadModel model;
                    prevsku = Convert.ToString(mySheet.Cells[row, 2].Value);                 

                    while (mySheet.Cells[row, 0].Value != null)
                    {
                        sku = Convert.ToString(mySheet.Cells[row, 2].Value);
                        //process per sku
                        while ((sku == prevsku) && (mySheet.Cells[row, 0].Value != null))
                        {
                            model = createModelFromSpreadsheet(mySheet.Cells, row);

                            string status = (from a in db.StoreLookups
                                             where a.Store == model.Store
                                             select a.status).FirstOrDefault();

                            bool ecommwarehouse = rfDAO.isEcommWarehouse(model.Division, model.Store);

                            if (!validateUploadModel(model))
                            {
                                Errors.Add(model);
                            }
                            else //if ((status == "N") || (status == "T") || (status == "A") || ecommwarehouse)
                            {
                                ProcessList.Add(model);
                            }
                            //Note:  Originally we weren't supposed to be ringfencing much, so we limited to Ecomm and new stores.
                            //  Because of all-star game stores, they wanted more
                            //  opening it up for now.  Probably should restrict to certain stores based on a new store extention attribute.
                            //else
                            //{
                            //    model.ErrorMessage = "You can only upload new stores or Ecomm warehouse ringfences";
                            //    Errors.Add(model);
                            //}
                            row++;
                            //for next loop, set sku to next value
                            sku = Convert.ToString(mySheet.Cells[row, 2].Value);
                        }
                        prevsku = sku;

                        //now we have a list to process
                        if (ProcessList.Count > 0)
                        {
                            RingFence tempRingFence = new RingFence();
                            tempRingFence.Sku = ProcessList[0].SKU;
                            tempRingFence.Division = ProcessList[0].Division;
                            tempRingFence.Store = ProcessList[0].Store;

                            SetUpRingFenceHeader(tempRingFence);

                            Available = dao.GetWarehouseAvailable(tempRingFence);
                            FuturePOs = GetFutureAvailable(tempRingFence);

                            CreateOrUpdateRingFence(tempRingFence.Division, tempRingFence.Store, tempRingFence.Sku, ProcessList, Errors, Available, FuturePOs);

                            ProcessList.Clear();
                        }
                    }

                    if (Errors.Count() > 0)
                    {
                        Session["errorList"] = Errors;
                        int count = (from a in Errors
                                     where (!(a.ErrorMessage.StartsWith("Warning")))
                                     select a).Count();

                        string msg=(row - count - 1) + " successfully uploaded";
                        if (count > 0)
                        {
                            msg += ", " + count + " Errors";
                        }
                        if (Errors.Count() > count)
                        {
                            //have warnings
                            msg += ", " + (Errors.Count() - count) + " Warnings";
                        }
                        return Content(msg);
                    }
                }
                else
                {
                    // Inform of missing/bad header row
                    return Content("Incorrectly formatted or missing header row. Please correct and re-process.");
                }
            }

            return Content("");
        }


        public void CreateOrUpdateRingFence(string division, string store, string sku, List<RingFenceUploadModel> ProcessList, List<RingFenceUploadModel> Errors, List<RingFenceDetail> Available, List<RingFenceDetail> FuturePOs)
        {
            List<EcommRingFence> EcommAllStoresList = new List<EcommRingFence>();

            ItemMaster item = (from a in db.ItemMasters
                               where a.MerchantSku == sku
                               select a).FirstOrDefault();

            foreach (RingFenceUploadModel model in ProcessList)
            {
                List<EcommWeight> weights = new List<EcommWeight>();

                if (model.Store == "00800")
                {
                    //Europe ecomm store that needs to be broken out
                    EcommRingFence ecomm = new EcommRingFence();
                    ecomm.Sku = model.SKU;
                    ecomm.Size = model.Size;
                    ecomm.PO = model.PO;
                    ecomm.Comments = model.Comments;

                    try
                    {
                        ecomm.Qty = Convert.ToInt32(model.Qty);
                        if (ecomm.Qty < 0)
                            throw new Exception("Qty < 0");

                        EcommAllStoresList.Add(ecomm);
                    }
                    catch (Exception ex)
                    {
                        model.ErrorMessage = ex.Message;
                        Errors.Add(model);
                    }
                }
                else
                {
                    EcommWeight tw = new EcommWeight();
                    tw.Store = store;
                    tw.Division = division;
                    if (model.Store != "")
                    {
                        tw.Store = model.Store;
                        tw.Division = model.Division;
                    }
                    tw.Weight = 1;
                    weights.Add(tw);

                    try
                    {
                        foreach (EcommWeight weight in weights)
                        {
                            RingFence ringFence = (from a in db.RingFences
                                                   where ((a.Division == division) && 
                                                          (a.Store == weight.Store) && 
                                                          (a.Sku == sku))
                                                   select a).FirstOrDefault();
                            Boolean newRingFence = false;
                            if (ringFence == null)
                            {
                                ringFence = new RingFence();
                                ringFence.Sku = sku;
                                newRingFence = true;
                            }
                            ringFence.Comments = model.Comments;
                            ringFence.Division = division;
                            ringFence.Store = weight.Store;

                            SetUpRingFenceHeader(ringFence);

                            if (newRingFence)
                            {
                                db.RingFences.Add(ringFence);
                                db.SaveChanges(UserName);
                            }

                            RingFenceDetail detail;
                            if ((model.PO == null) || (model.PO == ""))
                            {
                                detail = (from a in db.RingFenceDetails join 
                                               b in db.DistributionCenters 
                                                   on a.DCID equals b.ID
                                          where ((a.RingFenceID == ringFence.ID) && 
                                                 (a.Size == model.Size) && 
                                                 (b.MFCode == model.Warehouse) && 
                                                 ((a.PO == null) || (a.PO == "")))
                                          select a).FirstOrDefault();
                            }
                            else
                            {
                                detail = (from a in db.RingFenceDetails join 
                                               b in db.DistributionCenters 
                                                   on a.DCID equals b.ID
                                          where ((a.RingFenceID == ringFence.ID) && 
                                                 (a.Size == model.Size) && 
                                                 (b.MFCode == model.Warehouse) && 
                                                 (a.PO == model.PO))
                                          select a).FirstOrDefault();
                            }
                            Boolean newDetail = false;

                            if (detail == null)
                            {
                                detail = new RingFenceDetail();
                                detail.RingFenceID = ringFence.ID;
                                detail.Size = model.Size.PadLeft(3, '0');

                                detail.Warehouse = model.Warehouse.PadLeft(2, '0');
                                try
                                {
                                    detail.DCID = (from a in db.DistributionCenters
                                                   where a.MFCode == model.Warehouse
                                                   select a.ID).First();
                                }
                                catch
                                {
                                    throw new Exception("Invalid Warehouse");
                                }
                                newDetail = true;
                            }

                            detail.ActiveInd = "1";
                            detail.PO = model.PO;
                            detail.LastModifiedDate = DateTime.Now;
                            detail.LastModifiedUser = User.Identity.Name;
                            detail.Qty = Convert.ToInt32(Convert.ToInt32(model.Qty) * weight.Weight);
                            int availableQty;
                            if (detail.PO != "")
                            {
                                detail.ringFenceStatusCode = "1";
                                var query = (from a in FuturePOs
                                             where ((a.PO == detail.PO) && 
                                                    (a.Size == detail.Size) && 
                                                    (a.DCID == detail.DCID))
                                             select a.AvailableQty);
                                availableQty = 0;

                                if (query.Count() > 0)
                                {
                                    availableQty = query.Sum();
                                    try
                                    {
                                        availableQty += (from a in db.RingFences join 
                                                              b in db.RingFenceDetails 
                                                                  on a.ID equals b.RingFenceID
                                                         where ((a.Sku == model.SKU) && 
                                                                (b.Size == model.Size) &&
                                                                (b.ActiveInd == "1"))
                                                         select b.Qty).Sum();
                                    }
                                    catch { }
                                }
                                if (detail.Qty > availableQty)
                                {
                                    model.ErrorMessage = "Only " + availableQty + " available";
                                    Errors.Add(model);
                                }
                                else if (detail.Qty < 0)
                                {
                                    model.ErrorMessage = "Qty < 0";
                                    Errors.Add(model);
                                }
                            }
                            else
                            {
                                detail.ringFenceStatusCode = "4";
                                var query = (from a in Available
                                             where ((a.Size == detail.Size) && 
                                                    (a.DCID == detail.DCID))
                                             select a.AvailableQty);
                                availableQty = 0;

                                if (query.Count() > 0)
                                {
                                    availableQty = query.Sum();
                                    try
                                    {
                                        availableQty += (from a in db.RingFences join 
                                                              b in db.RingFenceDetails 
                                                                  on a.ID equals b.RingFenceID
                                                         where ((a.Sku == model.SKU) && 
                                                                (b.Size == model.Size) &&
                                                                (b.ActiveInd == "1"))
                                                         select b.Qty).Sum();
                                    }
                                    catch { }
                                }
                                if (detail.Qty > availableQty)
                                {
                                    model.ErrorMessage = "Only " + availableQty + " available";
                                    Errors.Add(model);
                                }
                            }

                            if ((model.ErrorMessage == null) || (model.ErrorMessage == ""))
                            {
                                if ((newDetail)&&(detail.Qty > 0))
                                {
                                    db.RingFenceDetails.Add(detail);
                                }
                                else if (detail.Qty > 0)
                                {
                                    model.ErrorMessage = "Warning:  Already existed, updated to new value";
                                    Errors.Add(model);
                                }
                                db.SaveChanges(UserName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        model.ErrorMessage = ex.Message;
                        Errors.Add(model);
                    }
                }
            }

            if (EcommAllStoresList.Count > 0)
            {
                ConvertRangeDAO crDAO = new ConvertRangeDAO();
                crDAO.SaveEcommRingFences(EcommAllStoresList, User.Identity.Name);
            }
        }

        public ActionResult ExcelTemplate()
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            excelDocument.Worksheets[0].Cells[0, 0].PutValue("Div (##)");
            excelDocument.Worksheets[0].Cells[0, 1].PutValue("Store (#####)");
            excelDocument.Worksheets[0].Cells[0, 2].PutValue("SKU (##-##-#####-##)");
            excelDocument.Worksheets[0].Cells[0, 3].PutValue("EndDate (DD/MM/YYYY)");
            excelDocument.Worksheets[0].Cells[0, 4].PutValue("PO (######)");
            excelDocument.Worksheets[0].Cells[0, 5].PutValue("Warehouse (##)");
            excelDocument.Worksheets[0].Cells[0, 6].PutValue("Size (###)");
            excelDocument.Worksheets[0].Cells[0, 7].PutValue("Caselot (#####)");
            excelDocument.Worksheets[0].Cells[0, 8].PutValue("Qty");
            excelDocument.Worksheets[0].Cells[0, 9].PutValue("Comments");

            excelDocument.Save("RingFenceUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

        public ActionResult DownloadErrors()
        {
            List<RingFenceUploadModel> errors = (List<RingFenceUploadModel>)Session["errorList"];

            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            excelDocument.Worksheets[0].Cells[0, 0].PutValue("Div (##)");
            excelDocument.Worksheets[0].Cells[0, 1].PutValue("Store (#####)");
            excelDocument.Worksheets[0].Cells[0, 2].PutValue("SKU (##-##-#####-##)");
            excelDocument.Worksheets[0].Cells[0, 3].PutValue("EndDate (DD/MM/YYYY)");
            excelDocument.Worksheets[0].Cells[0, 4].PutValue("PO (######)");
            excelDocument.Worksheets[0].Cells[0, 5].PutValue("Warehouse (##)");
            excelDocument.Worksheets[0].Cells[0, 6].PutValue("Size/Caselot (### or #####)");
            excelDocument.Worksheets[0].Cells[0, 7].PutValue("Qty");
            excelDocument.Worksheets[0].Cells[0, 8].PutValue("Comments");
            excelDocument.Worksheets[0].Cells[0, 9].PutValue("Error");
            int row = 1;

            foreach (RingFenceUploadModel model in errors)
            {
                excelDocument.Worksheets[0].Cells[row, 0].PutValue(model.Division);
                excelDocument.Worksheets[0].Cells[row, 1].PutValue(model.Store);
                excelDocument.Worksheets[0].Cells[row, 2].PutValue(model.SKU);
                excelDocument.Worksheets[0].Cells[row, 3].PutValue(model.EndDate);
                excelDocument.Worksheets[0].Cells[row, 4].PutValue(model.PO);
                excelDocument.Worksheets[0].Cells[row, 5].PutValue(model.Warehouse);
                excelDocument.Worksheets[0].Cells[row, 6].PutValue(model.Size);
                excelDocument.Worksheets[0].Cells[row, 7].PutValue(model.Qty);
                excelDocument.Worksheets[0].Cells[row, 8].PutValue(model.Comments);
                excelDocument.Worksheets[0].Cells[row, 9].PutValue(model.ErrorMessage);

                row++;                
            }

            excelDocument.Save("RingFenceUploadErrors.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

        public ActionResult PickFOB(string div, string store)
        {
            StoreLookup model = (from a in db.StoreLookups where ((a.Division == div) && (a.Store == store)) select a).First();
            return View(model);
        }
    }
}
