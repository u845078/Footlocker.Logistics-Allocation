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
using Footlocker.Logistics.Allocation.Models.Factories;
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

            return dao.GetWarehouseAvailable(ringFence.Sku, ringFence.Size, ringFence.ID)
                .OrderBy(r => r.Size.Length)
                .ThenBy(rf => rf.Size).ToList();            
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
            List<StoreLookup> list = (from a in db.RingFences
                                      join rfd in db.RingFenceDetails
                                        on a.ID equals rfd.RingFenceID
                                      join b in db.StoreLookups 
                                        on new { a.Division, a.Store } equals new { b.Division, b.Store }
                                      where a.Qty > 0 &&
                                            rfd.ActiveInd == "1"
                                      select b).ToList();
            list = (from a in list
                    join d in divs 
                      on a.Division equals d.DivCode
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
                list = (from a in db.RingFences
                        where ((a.Qty > 0) && (a.Division == div) && (a.Store == store))
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

            rf.ringFenceDetails = new List<RingFenceDetail>();
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

                return AssignInventory(model);
            }
        }

        public ActionResult AssignInventory(RingFenceModel model)
        {
            ViewData["ringFenceID"] = model.RingFence.ID;

            model.Divisions = this.Divisions();

            return View("AssignInventory", model);
        }

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
                RingFenceDetail newDetail = new RingFenceDetail
                {
                    RingFenceID = det.RingFenceID,
                    Size = det.Size, 
                    PO = det.PO, 
                    Qty = det.Qty,
                    Units = det.Units,
                    DCID = det.DCID
                };
                includeDetail = false;
                if (newDetail.Size.Length > _CASELOT_SIZE_INDICATOR_VALUE_LENGTH)
                {
                    try
                    {
                        var itemPack = db.ItemPacks.Include("Details").Single(p => p.ItemID == ringFenceItemID && 
                                                                                    p.Name == newDetail.Size);
                        newDetail.PackDetails = itemPack.Details.ToList();

                        foreach (ItemPackDetail pd in newDetail.PackDetails)
                        {
                            if (pd.Size == size)
                            {
                                newDetail.Units += det.Qty * pd.Quantity;
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
                    newDetail.Units = newDetail.Qty;
                }

                newDetail.Warehouse = (from a in dcs
                                       where a.ID == newDetail.DCID
                                       select a).First().Name;

                if (includeDetail)
                {
                    model.Add(newDetail);
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
            var ringfenceQuery = (from a in db.RingFences
                                  where a.ID == ID select a);
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
                model.RingFence.LastModifiedDate = DateTime.Now;
                model.RingFence.LastModifiedUser = User.Identity.Name;

                db.Entry(model.RingFence).State = System.Data.EntityState.Modified;

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
                history.Division = rf.Division;
                history.Store = rf.Store;
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
                    history.Division = rf.Division;
                    history.Store = rf.Store;
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
            history.Division = rf.Division;
            history.Store = rf.Store;
            history.CreateDate = DateTime.Now;
            history.CreatedBy = User.Identity.Name;
            db.RingFenceHistory.Add(history);
            db.SaveChanges(User.Identity.Name);


            List<RDQ> rdqsToCheck = new List<RDQ>();
            foreach (RingFenceDetail det in details)
            {
                //TODO:  Create RDQ for each detail
                RDQ rdq = new RDQ
                {
                    Sku = rf.Sku,
                    Size = det.Size,
                    Qty = det.Qty,
                    Store = rf.Store,
                    PO = det.PO,
                    Division = rf.Division,
                    DCID = det.DCID,
                    ItemID = (from a in db.ItemMasters
                              where a.MerchantSku == rf.Sku
                              select a).FirstOrDefault().ID,
                    CreatedBy = User.Identity.Name,
                    CreateDate = DateTime.Now
                };
                SetRDQDefaults(det, rdq);
                if ((rdq.PO != null) && (rdq.PO != "") && optionalPick)
                { 
                    rdq.Type = "user_opt";
                    message = "Since this was created today, it will NOT be honored if the PO is delivered today.";
                }
                rdqsToCheck.Add(rdq);

                db.RingFenceDetails.Remove(det);
                //db.SaveChanges(User.Identity.Name);

                history = new RingFenceHistory
                {
                    RingFenceID = det.RingFenceID,
                    Division = rf.Division,
                    Store = rf.Store,
                    DCID = det.DCID,
                    PO = det.PO,
                    Qty = det.Qty,
                    Action = "Picked Det",
                    CreateDate = DateTime.Now,
                    CreatedBy = User.Identity.Name
                };
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
                        history.Division = rf.Division;
                        history.Store = rf.Store;
                        history.Action = "Picked";
                        history.CreateDate = DateTime.Now;
                        history.CreatedBy = User.Identity.Name;
                        db.RingFenceHistory.Add(history);
                        //db.SaveChanges(User.Identity.Name);

                        foreach (RingFenceDetail det in details)
                        {
                            //TODO:  Create RDQ for each detail
                            RDQ rdq = new RDQ
                            {
                                Sku = rf.Sku,
                                Size = det.Size,
                                Qty = det.Qty,
                                Store = rf.Store,
                                PO = det.PO,
                                Division = rf.Division,
                                DCID = det.DCID,
                                ItemID = (from a in db.ItemMasters
                                          where a.MerchantSku == rf.Sku
                                          select a).FirstOrDefault().ID,
                                CreatedBy = User.Identity.Name,
                                CreateDate = DateTime.Now
                            };
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

                            history = new RingFenceHistory
                            {
                                RingFenceID = det.RingFenceID,
                                Division = rf.Division,
                                Store = rf.Store,
                                DCID = det.DCID,
                                PO = det.PO,
                                Qty = det.Qty,
                                Action = "Picked Det",
                                CreateDate = DateTime.Now,
                                CreatedBy = User.Identity.Name
                            };
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
            
            List<RingFenceDetail> stillAvailable = GetWarehouseAvailable(rf);
            //List<RingFenceDetail> stillAvailable = dao.GetWarehouseAvailable(rf.Sku, rf.Size, rf.ID);
            //List<RingFenceDetail> stillAvailable = dao.GetWarehouseAvailableCommon(rf.Sku, rf.Size, "", rf.ID);

            RingFenceDAO dao = new RingFenceDAO();
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
            RingFence ringFence = (from a in db.RingFences
                                   where a.ID == ringFenceID
                                   select a).First();
            return View(new GridModel(GetWarehouseAvailable(ringFence)));
        }

        [GridAction]
        public ActionResult _SelectPOs(Int64 ringFenceID)
        {
            RingFence ringFence = (from a in db.RingFences where a.ID == ringFenceID select a).First();
            return View(new GridModel(GetFutureAvailable(ringFence)));
            //return View();
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

                    if (IsRingFenceDetailValid(det))
                    {
                        var detailRec = (from a in ringFence.ringFenceDetails
                                         where ((a.RingFenceID == det.RingFenceID) &&
                                                 (a.DCID == det.DCID) &&
                                                 (a.Size == det.Size) &&
                                                 (a.PO == det.PO))
                                         select a).FirstOrDefault();

                        if (det.Qty > 0)
                        {
                            det.ActiveInd = "1";

                            if (string.IsNullOrEmpty(det.PO))
                                det.ringFenceStatusCode = "4";
                            else
                                det.ringFenceStatusCode = "1";

                            if (detailRec != null)
                            {                                
                                detailRec.Qty = det.Qty;                                
                                db.Entry(detailRec).State = System.Data.EntityState.Modified;
                                //db.Entry(det).State = System.Data.EntityState.Modified;
                            }
                            else
                            {
                                db.Entry(det).State = System.Data.EntityState.Added;
                            }

                            det.LastModifiedDate = DateTime.Now;
                            det.LastModifiedUser = User.Identity.Name;                            
                        }
                        else if (det.Qty == 0)
                        {
                            if (detailRec != null)
                            {
                                db.Entry(detailRec).State = EntityState.Deleted;
                            }
                        }

                        db.SaveChanges(User.Identity.Name);
                    }
                }
            }

            // Build up viewmodel to be returned, (add errors if necessary)
            List<RingFenceDetail> details = (from a in ringFence.ringFenceDetails
                                             where a.ActiveInd == "1"
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
                {
                    //final.Add(updateRecord);
                    final.Add(new RingFenceDetail
                    {
                        Size = updateRecord.Size,
                        PackDetails = updateRecord.PackDetails,
                        RingFenceID = updateRecord.RingFenceID,
                        Warehouse = updateRecord.Warehouse,
                        PO = updateRecord.PO,
                        Qty = updateRecord.Qty,
                        AvailableQty = updateRecord.AvailableQty,
                        AssignedQty = updateRecord.AssignedQty,
                        Message = updateRecord.Message,
                        DCID = updateRecord.DCID,
                        ringFenceStatusCode = updateRecord.ringFenceStatusCode
                    });
                }
                else if (detailRecord != null)
                {
                    det.Qty = detailRecord.Qty;
                    //final.Add(det);
                    final.Add(new RingFenceDetail
                    {
                        Size = det.Size,
                        PackDetails = det.PackDetails,
                        RingFenceID = det.RingFenceID,
                        Warehouse = det.Warehouse,
                        PO = det.PO,
                        Qty = det.Qty,
                        AvailableQty = det.AvailableQty,
                        AssignedQty = det.AssignedQty,
                        Message = det.Message,
                        DCID = det.DCID,
                        ringFenceStatusCode = det.ringFenceStatusCode
                    });
                }
                else
                {
                    //final.Add(det);
                    final.Add(new RingFenceDetail
                    {
                        Size = det.Size,
                        PackDetails = det.PackDetails,
                        RingFenceID = det.RingFenceID,
                        Warehouse = det.Warehouse,
                        PO = det.PO,
                        Qty = det.Qty,
                        AvailableQty = det.AvailableQty,
                        AssignedQty = det.AssignedQty,
                        Message = det.Message,
                        DCID = det.DCID,
                        ringFenceStatusCode = det.ringFenceStatusCode
                    });
                }
            }

            return View(new GridModel(final));
        }

        bool IsRingFenceDetailValid(RingFenceDetail rfDetail)
        {
            if (rfDetail.Qty > 0)
            {
                if (rfDetail.Qty > rfDetail.AvailableQty)
                {
                    if (string.IsNullOrEmpty(rfDetail.PO))
                        rfDetail.Message = String.Format("Max Quantity for {0} is {1}", rfDetail.Warehouse, rfDetail.AvailableQty);
                    else
                        rfDetail.Message = String.Format("Max Quantity for PO {0} is {1}", rfDetail.PO, rfDetail.AvailableQty);

                    return false;
                }
            }

            if (rfDetail.Qty < 0)
            {
                rfDetail.Message = "You cannot ring fence a negative number";
                return false;
            }

            return true;
        }

        private RingFenceUploadModel CreateUploadModelFromDetail(RingFenceDetail rfd, RingFence rf)
        {
            RingFenceUploadModel model = new RingFenceUploadModel();

            model.SKU = rf.Sku;
            model.Size = rfd.Size;
            model.PO = rfd.PO;
            model.Comments = rf.Comments;
            model.Qty = Convert.ToString(rfd.Qty);

            model.Store = rf.Store;
            model.Warehouse = rfd.Warehouse;
            model.Division = rf.Division;
            model.EndDate = Convert.ToString(rf.EndDate);

            return model;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _SaveBatchInsert([Bind(Prefix = "updated")]IEnumerable<RingFenceDetail> updated)
        {
            List<RingFenceDetail> available = null;
            RingFenceDAO rfDAO = new RingFenceDAO();
            string PO;
            string message = "";
            RingFence ringFence = null;
            int availableQty;

            if (updated != null)
            {                
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
                        if (IsRingFenceDetailValid(det))
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
                        {
                            //final.Add(updateRecord);
                            final.Add(new RingFenceDetail
                            {
                                Size = updateRecord.Size,
                                PackDetails = updateRecord.PackDetails,
                                RingFenceID = updateRecord.RingFenceID,
                                Warehouse = updateRecord.Warehouse,
                                PO = updateRecord.PO,
                                Qty = updateRecord.Qty,
                                AvailableQty = updateRecord.AvailableQty,
                                AssignedQty = updateRecord.AssignedQty,
                                Message = updateRecord.Message,
                                DCID = updateRecord.DCID,
                                ringFenceStatusCode = updateRecord.ringFenceStatusCode
                            });
                        }
                        else if (detailRecord != null)
                        {
                            det.Qty = detailRecord.Qty;
                            //final.Add(det);
                            final.Add(new RingFenceDetail
                            {
                                Size = det.Size,
                                PackDetails = det.PackDetails,
                                RingFenceID = det.RingFenceID,
                                Warehouse = det.Warehouse,
                                PO = det.PO,
                                Qty = det.Qty,
                                AvailableQty = det.AvailableQty,
                                AssignedQty = det.AssignedQty,
                                Message = det.Message,
                                DCID = det.DCID,
                                ringFenceStatusCode = det.ringFenceStatusCode
                            });
                        }
                        else
                        {
                            //final.Add(det);
                            final.Add(new RingFenceDetail
                            {
                                Size = det.Size,
                                PackDetails = det.PackDetails,
                                RingFenceID = det.RingFenceID,
                                Warehouse = det.Warehouse,
                                PO = det.PO,
                                Qty = det.Qty,
                                AvailableQty = det.AvailableQty,
                                AssignedQty = det.AssignedQty,
                                Message = det.Message,
                                DCID = det.DCID,
                                ringFenceStatusCode = det.ringFenceStatusCode
                            });
                        }
                            
                    }
                    return View(new GridModel(final));
                }
                else
                {
                    if ((ringFence.Store == "00800") && (ringFence.Division == "31"))
                    {
                        List<RingFenceUploadModel> list = new List<RingFenceUploadModel>();
                        List<RingFenceUploadModel> errorlist = new List<RingFenceUploadModel>();
                        List<RingFenceDetail> outputList = new List<RingFenceDetail>();
                        RingFenceUploadModel model;

                        foreach (RingFenceDetail det in updated)
                        {
                            // took out the validation since we're allowing 31/00800 to ring fence against 0 qty
                            //if (isRingFenceDetailValid(det))
                            //{
                            if (det.Qty > 0)
                            {
                                model = CreateUploadModelFromDetail(det, ringFence);

                                list.Add(model);
                            }
                            //}
                        }

                        if (list.Count() > 0)
                        {
                            processEcommRingFences(list, errorlist);

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
                        //Ecomm RingFence we need to create ecomm inventory
                        List<RingFenceDetail> futures = GetFutureAvailable(ringFence);
                        List<RingFenceDetail> warehouse = GetWarehouseAvailable(ringFence);

                        //ecomm all countries store
                        //EcommInventory ecommInv = new EcommInventory();
                        RingFenceDetail newDet = new RingFenceDetail();

                        Boolean addDetail;
                        RingFence rf = (from a in db.RingFences
                                        where ((a.Sku == ringFence.Sku) && 
                                               (a.Store == ringFence.Store))
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
                                                    (a.PO == det.PO) &&
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
                                if (string.IsNullOrEmpty(det.PO))
                                {
                                    newDet.PO = "";
                                    newDet.ringFenceStatusCode = "4";
                                }
                                else
                                {
                                    newDet.PO = det.PO;
                                    newDet.ringFenceStatusCode = "1";
                                }
                                newDet.Qty += det.Qty;

                                newDet.RingFenceID = rf.ID;
                                if (addDetail)
                                {
                                    db.RingFenceDetails.Add(newDet);
                                }

                                //save individually so the logic to automatically calculate the total works
                                db.SaveChanges(User.Identity.Name);
                            }
                        }
                        return View(new GridModel(updated));
                    }
                }
            }
            return View(new GridModel(updated));
        }

        public ActionResult Upload(string message)
        {
            ViewData["errorMessage"] = message;
            return View();
        }

        public ActionResult UploadDeletes()
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

        private bool validateUploadSecurity(RingFenceUploadModel model)
        {
            if (!(Footlocker.Common.WebSecurityService.UserHasDivision(UserName, "allocation", model.Division)))
            {
                model.ErrorMessage = "You are not authorized to update division " + model.Division;
                return false;
            }

            return true;
        }

        private bool validateUploadModel(RingFenceUploadModel model)
        {            
            if (model.SKU.Substring(0, 2) != model.Division)
            {
                model.ErrorMessage = "Division doesn't match";
                return false;
            }
            //else if (model.Store.Length == 0)
            //{
            //    model.ErrorMessage = "Store is required";
            //    return false;
            //}
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

        private void processUploadedRingfences(List<RingFenceUploadModel> ProcessList, List<RingFenceUploadModel> Errors)
        {
            RingFenceDAO dao = new RingFenceDAO();
            List<RingFenceDetail> Available = null;
            List<RingFenceDetail> FuturePOs = null;

            RingFence tempRingFence = new RingFence();
            tempRingFence.Sku = ProcessList[0].SKU;
            tempRingFence.Division = ProcessList[0].Division;
            tempRingFence.Store = ProcessList[0].Store;

            SetUpRingFenceHeader(tempRingFence);

            foreach (RingFenceUploadModel rfum in ProcessList)
            {
                RingFenceDetail det = new RingFenceDetail();
                det.Size = rfum.Size;
                det.PO = rfum.PO;
                det.Warehouse = rfum.Warehouse;

                tempRingFence.ringFenceDetails.Add(det);
            }

            foreach (RingFenceUploadModel rfum in ProcessList)
            {
                if ((rfum.PO == "") && (Available == null))
                    Available = dao.GetWarehouseAvailable(tempRingFence.Sku, tempRingFence.Size, tempRingFence.ID);
                //Available = dao.GetWarehouseAvailableCommon(tempRingFence.Sku, tempRingFence.Size, "", tempRingFence.ID);

                if ((rfum.PO != "") && (FuturePOs == null))
                    FuturePOs = GetFutureAvailable(tempRingFence);
            }                        

            CreateOrUpdateRingFence(tempRingFence.Division, tempRingFence.Store, tempRingFence.Sku, ProcessList, 
                Errors, Available, FuturePOs);
        }

        public ActionResult DeleteRingFences(IEnumerable<HttpPostedFileBase> attachments)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            string division = "";
            string sku;
            string store;

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
                if ((Convert.ToString(mySheet.Cells[0, 0].Value).Contains("Store")) && (Convert.ToString(mySheet.Cells[0, 1].Value).Contains("SKU")))
                {                    
                    List<RingFenceUploadModel> ProcessList = new List<RingFenceUploadModel>();
                    List<RingFenceUploadModel> DeleteList = new List<RingFenceUploadModel>();
                    List<RingFenceUploadModel> Errors = new List<RingFenceUploadModel>();
                    RingFenceUploadModel model;

                    while (mySheet.Cells[row, 0].Value != null || mySheet.Cells[row, 1].Value != null)
                    {
                        sku = Convert.ToString(mySheet.Cells[row, 1].Value);
                        division = sku.Substring(0, 2);
                        store = Convert.ToString(mySheet.Cells[row, 0].Value);

                        model = new RingFenceUploadModel
                        {
                            Store = store,
                            Division = division,
                            SKU = sku
                        };

                        if (!validateUploadSecurity(model))
                        {
                            Errors.Add(model);
                        }
                        else
                        {
                            bool ringFenceExists = db.RingFences.Any(rf => rf.Sku.Equals(model.SKU) &&
                                                                           (rf.Store == model.Store ||
                                                                           (string.IsNullOrEmpty(model.Store) && string.IsNullOrEmpty(rf.Store))));

                            if (!ringFenceExists)
                            {
                                model.ErrorMessage = "There was no ring fence found for this store and SKU";
                                Errors.Add(model);
                            }
                            
                            if (string.IsNullOrEmpty(model.ErrorMessage))
                                ProcessList.Add(model);
                        }

                        row++;
                    }

                    var duplicates = (from p in ProcessList
                                      group p by new { f1 = p.SKU, f2 = p.Store } into g
                                      where g.Count() > 1
                                      select g).ToList();

                    foreach (var dup in duplicates)
                    {
                        foreach (var plDup in ProcessList.Where(x => x.SKU == dup.Key.f1 && x.Store == dup.Key.f2))
                        {
                            plDup.ErrorMessage = "Duplicate ring fence record";
                            Errors.Add(plDup);
                        }
                    }

                    ProcessList = ProcessList.Where(x => string.IsNullOrEmpty(x.ErrorMessage)).ToList();

                    if (ProcessList.Count > 0)
                    {
                        foreach (var rfDel in ProcessList)
                        {
                            var rfToDelete = (from r in db.RingFences
                                              where (r.Store == rfDel.Store ||
                                                     (string.IsNullOrEmpty(r.Store) && string.IsNullOrEmpty(rfDel.Store))) &&
                                                    r.Sku == rfDel.SKU &&
                                                    r.Division == rfDel.Division
                                              select r).ToList();
                            if (rfToDelete.Count == 0)
                            {
                                rfDel.ErrorMessage = "There was no ring fence found for this store and SKU";
                                Errors.Add(rfDel);
                            }
                            else
                            {
                                foreach (var rf in rfToDelete)
                                {
                                    var deleteRFD = rf.ringFenceDetails.ToList();
                                    foreach (var rfd in deleteRFD)
                                        db.RingFenceDetails.Remove(rfd);

                                    db.RingFences.Remove(rf);
                                }
                            }
                        }

                        // clear out session variables since something changed
                        Session["rfStore"] = null;
                        Session["rfStoreList"] = null;

                        db.SaveChanges(UserName);
                    }

                    if (Errors.Count() > 0)
                    {
                        Session["errorList"] = Errors;
                        int count = (from a in Errors
                                     where (!(a.ErrorMessage.StartsWith("Warning")))
                                     select a).Count();

                        string msg = (row - count - 1) + " successfully uploaded";
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

                    string sku, prevsku;
                    prevsku = "FIRST";                   

                    List<RingFenceUploadModel> ProcessList = new List<RingFenceUploadModel>();
                    List<RingFenceUploadModel> Errors = new List<RingFenceUploadModel>();
                    RingFenceUploadModel model;
                    bool skipSKU = false; 
                    string skipError = "";
                    //prevsku = Convert.ToString(mySheet.Cells[row, 2].Value);                 

                    while (mySheet.Cells[row, 0].Value != null)
                    {
                        model = createModelFromSpreadsheet(mySheet.Cells, row);

                        sku = Convert.ToString(mySheet.Cells[row, 2].Value);                        

                        if (sku != prevsku)
                        {
                            skipSKU = false;
                            skipError = "";
                            if (!validateUploadSecurity(model))
                            {
                                Errors.Add(model);
                                skipError = model.ErrorMessage;
                                skipSKU = true;
                            }
                            
                            prevsku = sku;
                        }
                        else
                        {
                            if (skipSKU)
                            {
                                model.ErrorMessage = skipError;
                                Errors.Add(model);
                            }
                        }

                        if (!skipSKU)
                        {
                            if (!validateUploadModel(model))
                            {
                                Errors.Add(model);
                            }
                            else
                            {
                                ProcessList.Add(model);
                            }
                        }

                        row++;
                    }

                    if (ProcessList.Count > 0)
                    {
                        //now we have a list to process
                        prevsku = "";
                        string prevStore = "";
                        List<RingFenceUploadModel> processListBySku = new List<RingFenceUploadModel>();

                        foreach (RingFenceUploadModel rfum in ProcessList)
                        {
                            if ((rfum.SKU != prevsku) || (rfum.Store != prevStore))
                            {
                                if (processListBySku.Count > 0)
                                {
                                    processUploadedRingfences(processListBySku, Errors);
                                    processListBySku.Clear();
                                }

                                processListBySku.Add(rfum);
                                prevsku = rfum.SKU;
                                prevStore = rfum.Store;
                            }
                            else
                                processListBySku.Add(rfum);
                        }
                        processUploadedRingfences(processListBySku, Errors);
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

        public void processEcommRingFences(List<RingFenceUploadModel> ProcessList, List<RingFenceUploadModel> Errors)
        {
            List<EcommRingFence> EcommAllStoresList = new List<EcommRingFence>();

            foreach (RingFenceUploadModel model in ProcessList)
            {
                //Europe ecomm store that needs to be broken out
                EcommRingFence ecomm = new EcommRingFence();
                ecomm.Sku = model.SKU;
                ecomm.Size = model.Size;
                ecomm.PO = model.PO;
                ecomm.Comments = model.Comments;

                try
                {
                    ecomm.Qty = Convert.ToInt32(Math.Round(Convert.ToDecimal(model.Qty)));
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

            if (EcommAllStoresList.Count() > 0)
            {
                ConvertRangeDAO crDAO = new ConvertRangeDAO();
                crDAO.SaveEcommRingFences(EcommAllStoresList, User.Identity.Name);
            }
        }

        public void CreateOrUpdateRingFence(string division, string store, string sku, List<RingFenceUploadModel> ProcessList, List<RingFenceUploadModel> Errors, List<RingFenceDetail> Available, List<RingFenceDetail> FuturePOs)
        {
            List<RingFenceUploadModel> EcommRingFenceList = new List<RingFenceUploadModel>();

            ItemMaster item = (from a in db.ItemMasters
                               where a.MerchantSku == sku
                               select a).FirstOrDefault();

            RingFence ringFence = null;
            Boolean newRingFence = false;

            foreach (RingFenceUploadModel model in ProcessList)
            {                
                if (model.Store == "00800")
                {
                    //Europe ecomm store that needs to be broken out
                    EcommRingFenceList.Add(model);                    
                }
                else
                {
                    try
                    {
                        RingFenceDetail detail = null;
                        int distributionCenterID;

                        try
                        {
                            distributionCenterID = (from a in db.DistributionCenters
                                                    where a.MFCode == model.Warehouse
                                                    select a.ID).First();
                        }
                        catch
                        {
                            throw new Exception("Invalid Warehouse");
                        }

                        if (ringFence == null)
                        {
                            ringFence = (from a in db.RingFences
                                         where ((a.Division == model.Division) &&
                                                 (a.Store == model.Store) &&
                                                 (a.Sku == model.SKU))
                                         select a).FirstOrDefault();

                            if (ringFence == null)
                            {
                                ringFence = new RingFence();
                                ringFence.Sku = model.SKU;
                                newRingFence = true;
                                ringFence.Division = model.Division;
                                ringFence.Store = model.Store;
                            }
                            ringFence.Comments = model.Comments;

                            SetUpRingFenceHeader(ringFence);
                        }

                        if (!newRingFence)
                        {
                            if (ringFence.ringFenceDetails.Count() == 0)
                            {
                                // if it is not a new ring fence, the detail might already exist
                                ringFence.ringFenceDetails = (from a in db.RingFenceDetails                                                              
                                                              where a.RingFenceID == ringFence.ID
                                                              select a).ToList();
                            }

                            detail = (from a in ringFence.ringFenceDetails
                                        where a.DCID == distributionCenterID &&
                                            a.Size == model.Size.PadLeft(3, '0') &&
                                            a.PO == model.PO
                                        select a).FirstOrDefault();

                            if (detail != null)
                            {
                                model.ErrorMessage = "Warning:  Already existed, updated to new value";
                                Errors.Add(model);
                            }
                        }

                        Boolean newDetail = false;

                        if ((detail == null) || newRingFence)
                        {
                            detail = new RingFenceDetail();
                            detail.Size = model.Size.PadLeft(3, '0');
                            detail.Warehouse = model.Warehouse.PadLeft(2, '0');
                            detail.DCID = distributionCenterID;
                            detail.PO = model.PO;

                            newDetail = true;
                        }

                        detail.ActiveInd = "1";                        
                        detail.LastModifiedDate = DateTime.Now;
                        detail.LastModifiedUser = User.Identity.Name;
                        detail.Qty = Convert.ToInt32(Math.Round(Convert.ToDecimal(model.Qty)));
                        int availableQty = 0;

                        if (detail.PO != "")
                        {
                            detail.ringFenceStatusCode = "1";
                            availableQty = (from a in FuturePOs
                                         where ((a.PO == detail.PO) &&
                                             (a.Size == detail.Size) &&
                                             (a.DCID == detail.DCID))
                                         select a.AvailableQty).Sum();
                        }
                        else
                        {
                            detail.ringFenceStatusCode = "4";
                            availableQty = (from a in Available
                                         where ((a.Size == detail.Size) &&
                                             (a.DCID == detail.DCID))
                                         select a.AvailableQty).Sum();
                        }

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

                        if ((model.ErrorMessage == null) || (model.ErrorMessage == ""))
                        {
                            if ((newDetail) && (detail.Qty > 0))
                            {
                                ringFence.ringFenceDetails.Add(detail);                                
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

            if (ringFence != null)
            {
                if (ringFence.ringFenceDetails.Count() > 0)
                {
                    ringFence.calculateTotalRingFenceQuantity();

                    if (newRingFence)
                        db.RingFences.Add(ringFence);
                }
            }

            db.SaveChanges(UserName);

            if (EcommRingFenceList.Count > 0)
            {
                processEcommRingFences(EcommRingFenceList, Errors);
            }
        }

        public ActionResult ExcelDeleteTemplate()
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            string templateFilename = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["RingFenceDeleteTemplate"]);
            FileStream file = new FileStream(Server.MapPath("~/") + templateFilename, FileMode.Open, System.IO.FileAccess.Read);

            Byte[] data1 = new Byte[file.Length];
            file.Read(data1, 0, data1.Length);
            file.Close();
            MemoryStream memoryStream1 = new MemoryStream(data1);
            excelDocument.Open(memoryStream1);
            excelDocument.Save("RingFenceDeleteUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

        private void GenerateHeader(Aspose.Excel.Worksheet mySheet, int col, string headerText)
        {
            mySheet.Cells[0, col].PutValue(headerText);
            mySheet.Cells[0, col].Style.HorizontalAlignment = TextAlignmentType.Center;
            mySheet.Cells[0, col].Style.Font.Size = 10;
            mySheet.Cells[0, col].Style.Font.IsBold = true;
            mySheet.AutoFitColumn(col);
        }

        public ActionResult ExcelTemplate()
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            Worksheet mySheet = excelDocument.Worksheets[0];
            GenerateHeader(mySheet, 0, "Div (##)");
            GenerateHeader(mySheet, 1, "Store (#####)");
            GenerateHeader(mySheet, 2, "SKU (##-##-#####-##)");
            GenerateHeader(mySheet, 3, "EndDate (DD/MM/YYYY)");
            GenerateHeader(mySheet, 4, "PO (######)");
            GenerateHeader(mySheet, 5, "Warehouse (##)");
            GenerateHeader(mySheet, 6, "Size (### or #####)");
            GenerateHeader(mySheet, 7, "Qty");
            GenerateHeader(mySheet, 8, "Comments");

            for (int i = 1; i < 10000; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (j != 7)
                    {
                        mySheet.Cells[i, j].Style.Number = 49;
                    }
                }
            }

            excelDocument.Save("RingFenceUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

        public ActionResult DownloadErrors()
        {
            List<RingFenceUploadModel> errors = (List<RingFenceUploadModel>)Session["errorList"];

            Aspose.Excel.License license = new Aspose.Excel.License();
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            Excel excelDocument = new Excel();

            if (errors != null)
            {
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
            }
            else
            {
                // if this message is hit that means there was an exception while processing that was not accounted for
                // check the log to see what the exception was
                var message = "An unexpected error has occured.  Please try again or contact an administrator.";
                return RedirectToAction("Upload", new { message = message });
            }
            
            return View();
        }

        public ActionResult DownloadErrorsNew()
        {
            var errorList = (List<Tuple<RingFenceUploadModelNew, string>>)Session["errorList"];
            if (errorList != null)
            {
                Aspose.Excel.License license = new Aspose.Excel.License();
                //Set the license
                license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

                Excel excelDocument = new Excel();
                Worksheet mySheet = excelDocument.Worksheets[0];
                int col = 0;
                mySheet.Cells[0, col].PutValue("Div (##)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Store (#####)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("SKU (##-##-#####-##)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("EndDate (DD/MM/YYYY)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("PO (######)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Warehouse (##)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Size/Caselot (### or #####)");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Qty");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Comments");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;
                mySheet.Cells[0, col].PutValue("Error");
                mySheet.Cells[0, col].Style.Font.IsBold = true;
                col++;

                int row = 1;
                if (errorList != null && errorList.Count > 0)
                {
                    foreach (var error in errorList)
                    {
                        col = 0;
                        mySheet.Cells[row, col].PutValue(error.Item1.Division);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.Store);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.Sku);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.EndDate);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.PO);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.DC);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.Size);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.Quantity);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item1.Comments);
                        col++;
                        mySheet.Cells[row, col].PutValue(error.Item2);
                        row++;
                    }

                    for (int i = 0; i < 10; i++)
                    {
                        mySheet.AutoFitColumn(i);
                    }
                }

                excelDocument.Save("RingFenceUploadErrors.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            }
            else
            {
                // if this message is hit that means there was an exception while processing that was not accounted for
                // check the log to see what the exception was
                var message = "An unexpected error has occured.  Please try again or contact an administrator.";
                return RedirectToAction("Upload", new { message = message });
            }

            return View();
        }

        public ActionResult DownloadDeleteErrors()
        {
            List<RingFenceUploadModel> errors = (List<RingFenceUploadModel>)Session["errorList"];

            if (errors != null)
            {
                Aspose.Excel.License license = new Aspose.Excel.License();
                //Set the license 
                license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

                Excel excelDocument = new Excel();
                excelDocument.Worksheets[0].Cells[0, 0].PutValue("Store (#####)");
                excelDocument.Worksheets[0].Cells[0, 1].PutValue("SKU (##-##-#####-##)");
                excelDocument.Worksheets[0].Cells[0, 2].PutValue("Error");
                int row = 1;

                foreach (RingFenceUploadModel model in errors)
                {
                    excelDocument.Worksheets[0].Cells[row, 0].PutValue(model.Store);
                    excelDocument.Worksheets[0].Cells[row, 1].PutValue(model.SKU);
                    excelDocument.Worksheets[0].Cells[row, 2].PutValue(model.ErrorMessage);

                    row++;
                }

                excelDocument.Save("RingFenceDeleteErrors.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            }
            return View();
        }

        public ActionResult PickFOB(string div, string store)
        {
            StoreLookup model = (from a in db.StoreLookups where ((a.Division == div) && (a.Store == store)) select a).First();
            return View(model);
        }

        public ActionResult DestExport(string div, string store)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            //excelDocument.Worksheets[0].Cells[0, 0].PutValue("Store");
            excelDocument.Worksheets[0].Cells[0, 0].PutValue("SKU");
            excelDocument.Worksheets[0].Cells[0, 1].PutValue("Size");
            excelDocument.Worksheets[0].Cells[0, 2].PutValue("Pick Quantity");
            excelDocument.Worksheets[0].Cells[0, 3].PutValue("");
            excelDocument.Worksheets[0].Cells[0, 4].PutValue("Ring Fence Store");
            excelDocument.Worksheets[0].Cells[0, 5].PutValue("Ring Fence Status");
            excelDocument.Worksheets[0].Cells[0, 6].PutValue("Quantity");
            excelDocument.Worksheets[0].Cells[0, 7].PutValue("Start Date");
            excelDocument.Worksheets[0].Cells[0, 8].PutValue("End Date");
            excelDocument.Worksheets[0].Cells[0, 9].PutValue("PO");
            excelDocument.Worksheets[0].Cells[0, 10].PutValue("Created By");
            excelDocument.Worksheets[0].Cells[0, 11].PutValue("Create Date");
            excelDocument.Worksheets[0].Cells[0, 12].PutValue("Comments");
            int row = 1;


            var ringFenceStoreList = (from a in db.RingFences
                                      where ((a.Qty > 0) && (a.Division == div) && (a.Store == store))
                                      select a).ToList();
            foreach (var rfStore in ringFenceStoreList)
            {
                foreach (var rfStoreDetail in rfStore.ringFenceDetails.Where(d => d.ActiveInd == "1" && d.Qty > 0))
                {                    
                    excelDocument.Worksheets[0].Cells[row, 0].PutValue(rfStore.Sku);
                    excelDocument.Worksheets[0].Cells[row, 1].PutValue(rfStoreDetail.Size);
                    excelDocument.Worksheets[0].Cells[row, 2].PutValue(rfStoreDetail.Qty);

                    excelDocument.Worksheets[0].Cells[row, 4].PutValue(rfStore.Store);
                    excelDocument.Worksheets[0].Cells[row, 5].PutValue(rfStoreDetail.RingFenceStatus.ringFenceStatusDesc);

                    int totalQuantity = 0;
                    if (rfStoreDetail.Size.Length > _CASELOT_SIZE_INDICATOR_VALUE_LENGTH)
                    {                        
                        int itemPackQty = (from i in db.ItemPacks
                                           where //i.ItemID == rfStore.ItemID &&
                                                 i.Name == rfStoreDetail.Size
                                           select i.TotalQty).FirstOrDefault();
                        totalQuantity = itemPackQty * rfStoreDetail.Qty;
                    }
                    else
                        totalQuantity = rfStoreDetail.Qty;

                    excelDocument.Worksheets[0].Cells[row, 6].PutValue(totalQuantity);

                    excelDocument.Worksheets[0].Cells[row, 7].PutValue(rfStore.StartDate.ToShortDateString());

                    if (rfStore.EndDate.HasValue)
                        excelDocument.Worksheets[0].Cells[row, 8].PutValue(rfStore.EndDate.Value.ToShortDateString());

                    excelDocument.Worksheets[0].Cells[row, 9].PutValue(rfStoreDetail.PO);
                    excelDocument.Worksheets[0].Cells[row, 10].PutValue(rfStore.CreatedBy);
                    excelDocument.Worksheets[0].Cells[row, 11].PutValue(rfStore.CreateDate.Value.ToShortDateString() + ' ' +
                        rfStore.CreateDate.Value.ToLongTimeString());
                    excelDocument.Worksheets[0].Cells[row, 12].PutValue(rfStore.Comments);

                    row++;
                }
            }

            excelDocument.Save("RingFenceWebUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);

            return View();
        }

        #region RingFenceUpload Optimization

        public ActionResult SaveRingFencesWithAccumulatedQuantity(IEnumerable<HttpPostedFileBase> attachments)
        {
            return SaveRingFencesOptimized(attachments, true);
        }

        public ActionResult SaveRingFencesReplacingQuantity(IEnumerable<HttpPostedFileBase> attachments2)
        {
            return SaveRingFencesOptimized(attachments2, false);
        }

        public ActionResult SaveRingFencesOptimized(IEnumerable<HttpPostedFileBase> attachments, bool accumulateQuantity)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            string message = string.Empty, errorMessage = string.Empty;
            List<RingFenceUploadModelNew> parsedRFs = new List<RingFenceUploadModelNew>(), validRFs = new List<RingFenceUploadModelNew>();
            List<EcommRingFence> ecommRFs = new List<EcommRingFence>();
            List<Tuple<RingFenceUploadModelNew, string>> errorList = new List<Tuple<RingFenceUploadModelNew, string>>();
            int successfulCount = 0;
            List<Tuple<RingFenceUploadModelNew, string>> warnings, errors;

            foreach (var file in attachments)
            {               
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];

                if (!this.HasValidHeaderRow(mySheet))
                {
                    message = "Upload failed: Incorrect header - please use template.";
                    return Content(message);
                }
                else
                {
                    int row = 1;
                    try
                    {
                        // create a local list of type RingFenceUploadModel to store the values from the upload
                        while (this.HasDataOnRow(mySheet, row))
                        {
                            parsedRFs.Add(this.ParseUploadRow(mySheet, row));
                            row++;
                        }

                        // continue processing only if there is at least 1 record to upload
                        if (parsedRFs.Count > 0)
                        {
                            // file validation - duplicates, permission for unique divisions, etc
                            if (!this.ValidateFile(parsedRFs, errorList))
                            {
                                Session["errorList"] = errorList;
                                successfulCount = validRFs.Count + ecommRFs.Count;
                                warnings = errorList.Where(el => el.Item2.StartsWith("Warning")).ToList();
                                errors = errorList.Except(warnings).ToList();
                                errorMessage = string.Format(
                                    "{0} lines were processed successfully. {1} warnings and {2} errors were found."
                                    , successfulCount
                                    , warnings.Count
                                    , errors.Count);
                                return Content(errorMessage);
                            }

                            // validate the parsed ring fences.  all valid ring fences will be added to validRFs
                            this.ValidateParsedRFs(parsedRFs, validRFs, errorList, ecommRFs);


                            if (validRFs.Count > 0)
                            {
                                // creates or updates the valid ring fences
                                this.CreateOrUpdateRingFencesNew(validRFs, errorList, accumulateQuantity);
                            }

                            if (ecommRFs.Count > 0)
                            {
                                // process ecomm ringfences
                                ConvertRangeDAO crDAO = new ConvertRangeDAO();
                                crDAO.SaveEcommRingFences(ecommRFs, User.Identity.Name, accumulateQuantity);
                            }

                            successfulCount = validRFs.Count + ecommRFs.Count;
                            warnings = errorList.Where(el => el.Item2.StartsWith("Warning")).ToList();
                            errors = errorList.Except(warnings).ToList();

                            // if errors occured, allow user to download them
                            if (errorList.Count > 0)
                            {
                                errorMessage = string.Format(
                                    "{0} lines were processed successfully. {1} warnings and {2} errors were found."
                                    , successfulCount
                                    , warnings.Count
                                    , errors.Count);
                                Session["errorList"] = errorList;
                                return Content(errorMessage);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        FLLogger logger = new FLLogger("C:\\Log\\allocation");
                        logger.Log(ex.Message + ": " + ex.StackTrace, FLLogger.eLogMessageType.eError);
                        message = "Upload failed: One or more columns has unexpected missing or invalid data.";
                        // clear out error list
                        Session["errorList"] = null;
                        return Content(message);
                    }
                }
            }
            message = string.Format("Success! {0} lines were processed.", successfulCount);
            return Json(new { message = message }, "application/json");
        }

        private void CreateOrUpdateRingFencesNew(List<RingFenceUploadModelNew> validRFs, List<Tuple<RingFenceUploadModelNew, string>> errorList, bool accumulateQuantity)
        {           

            //List<RingFence> ringFences = new List<RingFence>();
            List<RingFenceDetail> ringFenceDetails = new List<RingFenceDetail>();
            // group ring fences by div, store, sku, and list of there details (which is just the upload model)
            var rfHeaders = validRFs.GroupBy(vr => new { Division = vr.Division, Store = vr.Store, Sku = vr.Sku })
                                     .Select(vr => new
                                     {
                                         Division = vr.Key.Division,
                                         Store = vr.Key.Store,
                                         Sku = vr.Key.Sku,
                                         Details = vr.ToList()
                                     })
                                     .ToList();

            /* this is a messy linq query so allow me to explain.
             * At first we are joining our grouped ring fences together with the ringfence table to
             * see if there is an existing ringfence for this upload.  If it is found, we can't
             * make the assumption that there is not two or more records that will be returned.
             * Therefore, after selecting the initial division, store, sku, and detail records, we
             * group by division, store, and sku and then select the first record in the list that
             * we find (this is how the old process worked and I believe this needs revisited)
             */
            var existingHeaders = (from gr in rfHeaders
                                      join rf in db.RingFences
                                        on new { Division = gr.Division, Store = gr.Store, Sku = gr.Sku } equals
                                           new { Division = rf.Division, Store = rf.Store, Sku = rf.Sku }
                                      where (rf.EndDate == null || rf.EndDate > DateTime.Now)
                                      select new { RingFenceID = rf.ID, Division = rf.Division, Store = rf.Store, Sku = rf.Sku, Details = gr.Details })
                                      .GroupBy(rf => new { Division = rf.Division, Store = rf.Store, Sku = rf.Sku })
                                      .Select(rf => new
                                      {
                                          RingFenceID = rf.FirstOrDefault().RingFenceID,
                                          Division = rf.Key.Division,
                                          Store = rf.Key.Store,
                                          Sku = rf.Key.Sku,
                                          Details = rf.FirstOrDefault().Details
                                      }).ToList();

            // reselect the existingHeaders to be in the format to use Except in the next statement (just excludes the ringfenceid we brought back)
            var reformattedHeaders = existingHeaders.Select(egr => new { Division = egr.Division, Store = egr.Store, Sku = egr.Sku, Details = egr.Details }).ToList();
            var nonExistingHeaders = rfHeaders.Except(reformattedHeaders).ToList();

            // this section is for populating data from the database before we build out the non-existing ring fences.
            // I populate these lists to reduce the number of database calls for each and every new ring fence.
            var uniqueDivisions = validRFs.Select(rf => rf.Division).Distinct().ToList();
            var divisionControlDateMapping = (from ud in uniqueDivisions
                                              join id in db.InstanceDivisions
                                                on ud equals id.Division
                                              join cd in db.ControlDates
                                                on id.InstanceID equals cd.InstanceID
                                            select new { Division = id.Division, RunDate = cd.RunDate }).ToList();

            var uniqueDCs = validRFs.Select(vr => vr.DC).Distinct().ToList();
            var dcidMapping = (from dcs in db.DistributionCenters.Where(dcs => uniqueDCs.Contains(dcs.MFCode))
                               select new { DCID = dcs.ID, MFCode = dcs.MFCode }).ToList();

            var uniqueSkus = validRFs.Select(vr => vr.Sku).Distinct().ToList();

            var skuItemIDMapping = (from im in db.ItemMasters.Where(im => uniqueSkus.Contains(im.MerchantSku))
                                    select new { Sku = im.MerchantSku, ItemID = im.ID }).ToList();

            List<Tuple<string, int>> uniqueCaselotNameQtys = new List<Tuple<string, int>>();
            List<string> uniqueCaselots = new List<string>();

            if (validRFs.Any(vr => vr.Size.Length.Equals(5)))
            {

                // unique caselot schedule names
                uniqueCaselots = validRFs.Where(rf => rf.Size.Length.Equals(5))
                                         .Select(rf => rf.Size)
                                         .Distinct()
                                         .ToList();

                var itempacks = db.ItemPacks.Where(ip => uniqueCaselots.Contains(ip.Name)).ToList();
                uniqueCaselotNameQtys = itempacks.Select(ip => Tuple.Create(ip.Name, ip.TotalQty))
                                                 .Distinct()
                                                 .ToList();
            }

            var ecommWhse = db.EcommWarehouses.ToList();

            db.Configuration.AutoDetectChangesEnabled = false;
            db.Configuration.LazyLoadingEnabled = false;
            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.ValidateOnSaveEnabled = false;
            // create ring fences and ring fence details for non-existing combinations
            foreach (var grf in nonExistingHeaders)
            {
                // populate header record
                RingFence rf = new RingFence();
                rf.Division = grf.Division;
                rf.Store = grf.Store;
                rf.Sku = grf.Sku;
                rf.ItemID = skuItemIDMapping.Where(r => r.Sku.Equals(rf.Sku)).Select(r => r.ItemID).FirstOrDefault();
                rf.StartDate = divisionControlDateMapping.Where(cd => cd.Division.Equals(rf.Division)).Select(cd => cd.RunDate).FirstOrDefault().AddDays(1);
                var endDate = grf.Details.Select(g => g.EndDate).FirstOrDefault();
                rf.EndDate = endDate;
                rf.Comments = (grf.Details.FirstOrDefault()) == null ? "" : grf.Details.FirstOrDefault().Comments;
                rf.CreateDate = DateTime.Now;
                rf.CreatedBy = User.Identity.Name;
                rf.LastModifiedDate = DateTime.Now;
                rf.LastModifiedUser = User.Identity.Name;
                rf.Type = (ecommWhse.Any(ew => ew.Division.Equals(rf.Division) && ew.Store.Equals(rf.Store))) ? 2 : 1;
                rf.ringFenceDetails = new List<RingFenceDetail>();
                // traverse and generate detail records
                foreach (var detail in grf.Details)
                {
                    RingFenceDetail rfd = new RingFenceDetail();
                    rfd.DCID = dcidMapping.Where(dc => detail.DC.Equals(dc.MFCode)).Select(dc => dc.DCID).FirstOrDefault();
                    rfd.PO = detail.PO;
                    rfd.Size = detail.Size;
                    rfd.Qty = detail.Quantity;
                    rfd.ringFenceStatusCode = (!string.IsNullOrEmpty(rfd.PO)) ? "1" : "4";
                    rfd.ActiveInd = "1";
                    rfd.LastModifiedDate = DateTime.Now;
                    rfd.LastModifiedUser = User.Identity.Name;
                    rf.ringFenceDetails.Add(rfd);
                }
                rf.Qty = this.CalculateHeaderQty(uniqueCaselotNameQtys, rf.ringFenceDetails);
                db.RingFences.Add(rf);
            }

            // retrieve existing ringfences all at once, and then locally map it
            List<long> uniqueRFIDs = existingHeaders.Select(egr => egr.RingFenceID).Distinct().ToList();

            List<RingFence> existingRingFences = db.RingFences
                                                    .Include("ringFenceDetails")
                                                    .Include("ringFenceDetails.DistributionCenter")
                                                    .Where(rf => uniqueRFIDs.Contains(rf.ID))
                                                    .ToList();

            foreach (var erf in existingRingFences)
            {
                // retrieve specific groupedRF
                var groupedRF = existingHeaders.Where(egr => egr.RingFenceID.Equals(erf.ID)).FirstOrDefault();
                if (groupedRF != null)
                {
                    // determine how many details are existent for the specified rf
                    var existingDetails = erf.ringFenceDetails.Where(rfd => groupedRF.Details.Any(d => rfd.Size.Equals(d.Size) &&
                                                                                                       rfd.PO.Equals(d.PO) &&
                                                                                                       rfd.DistributionCenter.MFCode.Equals(d.DC))).ToList();
                    // update each detail
                    if (existingDetails.Count > 0)
                    {
                        foreach (var detail in existingDetails)
                        {
                            // retrieve specific detail
                            var newDetail = groupedRF.Details.Where(d => d.Size.Equals(detail.Size) &&
                                                                         d.PO.Equals(detail.PO) &&
                                                                         d.DC.Equals(detail.DistributionCenter.MFCode)).FirstOrDefault();

                            if (detail.ActiveInd.Equals("1"))
                            {
                                if (accumulateQuantity)
                                {
                                    detail.Qty += newDetail.Quantity;
                                    SetErrorMessage(errorList, newDetail, "Warning: Already existed, accumulated to new value");
                                }
                                else
                                {
                                    detail.Qty = newDetail.Quantity;
                                    SetErrorMessage(errorList, newDetail, "Warning: Already existed, updated to upload value");
                                }
                            }
                            else
                            {
                                detail.Qty = newDetail.Quantity;
                            }
                            
                            // ensure detail is active now
                            detail.ActiveInd = "1";
                            detail.LastModifiedDate = DateTime.Now;
                            detail.LastModifiedUser = User.Identity.Name;                            
                            db.Entry(detail).State = System.Data.EntityState.Modified;
                        }
                    }

                    // determine how many details are non-existent for the specified rf
                    var nonExistingDetails = groupedRF.Details.Where(d => !erf.ringFenceDetails.Any(rfd => rfd.Size.Equals(d.Size) &&
                                                                                                           rfd.PO.Equals(d.PO) &&
                                                                                                           rfd.ActiveInd.Equals("1") &&
                                                                                                           rfd.DistributionCenter.MFCode.Equals(d.DC))).ToList();

                    // create each detail
                    if (nonExistingDetails.Count > 0)
                    {
                        foreach (var neDetail in nonExistingDetails)
                        {
                            RingFenceDetail rfd = new RingFenceDetail();
                            rfd.RingFenceID = erf.ID;
                            rfd.DCID = dcidMapping.Where(dc => neDetail.DC.Equals(dc.MFCode)).Select(dc => dc.DCID).FirstOrDefault();
                            rfd.PO = neDetail.PO;
                            rfd.Size = neDetail.Size;
                            rfd.Qty = neDetail.Quantity;
                            rfd.ringFenceStatusCode = (!string.IsNullOrEmpty(rfd.PO)) ? "1" : "4";
                            rfd.ActiveInd = "1";
                            rfd.LastModifiedDate = DateTime.Now;
                            rfd.LastModifiedUser = User.Identity.Name;
                            erf.ringFenceDetails.Add(rfd);
                            db.Entry(rfd).State = System.Data.EntityState.Added;
                        }
                    }

                }
                var endDate = groupedRF.Details.Select(d => d.EndDate).FirstOrDefault();
                erf.EndDate = endDate;
                db.Entry(erf).State = System.Data.EntityState.Modified;
                erf.Qty = this.CalculateHeaderQty(uniqueCaselotNameQtys, erf.ringFenceDetails);
                erf.Comments = groupedRF.Details.FirstOrDefault().Comments;
            }

            // save the changes for the already existing ringfences
            db.SaveChangesBulk(User.Identity.Name);
        }

        private int CalculateHeaderQty(List<Tuple<string, int>> uniqueCaselotNameQtys, List<RingFenceDetail> ringFenceDetails)
        {
            int totalQuantity = 0;
            foreach (var rfd in ringFenceDetails)
            {
                if (rfd.ActiveInd.Equals("1"))
                {
                    if (rfd.Size.Length.Equals(5))
                    {
                        // Item1 => the ItemPack's name (caselot size i.e. "00009")
                        var caselotDetail = uniqueCaselotNameQtys.Where(ucq => ucq.Item1.Equals(rfd.Size)).FirstOrDefault();
                        if (caselotDetail != null)
                        {
                            // Item2 => the ItemPack's TotalQty
                            totalQuantity += (rfd.Qty * caselotDetail.Item2);
                        }
                    }
                    else
                    {
                        totalQuantity += rfd.Qty;
                    }
                }
            }
            return totalQuantity;
        }

        private void ValidateParsedRFs(List<RingFenceUploadModelNew> parsedRFs, List<RingFenceUploadModelNew> validRFs, List<Tuple<RingFenceUploadModelNew, string>> errorList, List<EcommRingFence> ecommRFs)
        {
            // 1) Sku exists
            parsedRFs.Where(pr => string.IsNullOrEmpty(pr.Sku))
                     .ToList()
                     .ForEach(rf =>
                     {
                         SetErrorMessage(errorList, rf, "Sku must be provided.");
                         //parsedRFs.Remove(rf);
                     });

            // 2) Sku's division and ring fence's division match
            parsedRFs.Where(pr => !pr.Sku.Split('-')[0].Equals(pr.Division))
                     .ToList()
                     .ForEach(rf =>
                     {
                         SetErrorMessage(errorList, rf, "The division entered does not match the Sku's division.");
                         //parsedRFs.Remove(rf);
                     });

            // 3) Size exists
            parsedRFs.Where(pr => pr.Size.Equals(string.Empty))
                     .ToList()
                     .ForEach(rf =>
                     {
                         SetErrorMessage(errorList, rf, "Size or caselot must be provided.");
                         //parsedRFs.Remove(rf);
                     });

            // 4) Size is of the correct length
            var uniqueSizeList = parsedRFs.Select(pr => pr.Size).Distinct().ToList();
            var invalidSizeList = uniqueSizeList.Where(sl => !sl.Length.Equals(3) && !sl.Length.Equals(5)).ToList();
            parsedRFs.Where(pr => invalidSizeList.Contains(pr.Size))
                     .ToList()
                     .ForEach(rf =>
                     {
                         SetErrorMessage(errorList, rf, string.Format("The size {0} is non-existent or invalid.", rf.Size));
                     });


            // 5) Quantity is greater than zero
            parsedRFs.Where(pr => pr.Quantity <= 0)
                     .ToList()
                     .ForEach(rf =>
                     {
                         SetErrorMessage(errorList, rf, "The quantity provided cannot be less than or equal to zero.");
                         //parsedRFs.Remove(rf);
                     });

            // 6) DC is valid
            List<string> uniqueDCList = parsedRFs.Select(pr => pr.DC).Distinct().ToList();
            var invalidDCList = uniqueDCList.Where(dc => !db.DistributionCenters.Any(dcs => dcs.MFCode.Equals(dc))).ToList();
            parsedRFs.Where(pr => invalidDCList.Contains(pr.DC))
                     .ToList()
                     .ForEach(rf =>
                     {
                         SetErrorMessage(errorList, rf, "DC is invalid.");
                         //parsedRFs.Remove(rf);
                     });

            // 7) if PO is not empty, it has a length of 7 numbers
            parsedRFs.Where(pr => !string.IsNullOrEmpty(pr.PO) && !pr.PO.Length.Equals(7))
                     .ToList()
                     .ForEach(rf =>
                     {
                         SetErrorMessage(errorList, rf, "PO must be seven digits.");
                         //parsedRFs.Remove(rf);
                     });

            // 8) division / store combination is valid
            var uniqueDivStoreList = parsedRFs
                                        .Select(pr => new { pr.Division, pr.Store })
                                        .Where(pr => !string.IsNullOrEmpty(pr.Store) && !pr.Store.Equals("00800"))
                                        .Distinct()
                                        .ToList();

            var invalidDivStoreList = uniqueDivStoreList
                                        .Where(ds => !db.StoreLookups.Any(sl => sl.Store.Equals(ds.Store) &&
                                                                                sl.Division.Equals(ds.Division) &&
                                                                                (sl.status.Equals("M") ||
                                                                                 sl.status.Equals("O") ||
                                                                                 sl.status.Equals("T") ||
                                                                                 sl.status.Equals("N")))).ToList();

            parsedRFs.Where(pr => invalidDivStoreList.Contains(new { pr.Division, pr.Store }) && !string.IsNullOrEmpty(pr.Store))
                     .ToList()
                     .ForEach(rf =>
                     {
                         SetErrorMessage(errorList, rf
                             , string.Format("The division and store combination {0}-{1} is not an existing or valid combination.", rf.Division, rf.Store));
                         //parsedRFs.Remove(rf);
                     });

            // remove all parsedRFs that were found in the validation above
            parsedRFs.Where(pr => errorList.Any(er => er.Item1.Equals(pr)))
                     .ToList()
                     .ForEach(pr => parsedRFs.Remove(pr));

            // add ecomm rfs to separate list (users do not want ecomm explosion to have available inventory validated)
            parsedRFs.Where(pr => pr.Store.Equals("00800"))
                     .ToList()
                     .ForEach(r =>
                     {
                         ecommRFs.Add(new EcommRingFence(r.Sku, r.Size, r.PO, r.Quantity, r.Comments));
                         parsedRFs.Remove(r);
                     });

            // 9) validate inventory by unique div/sku/size
            this.ValidateAvailableInventoryForParsedRFs(parsedRFs, errorList, validRFs, ecommRFs);
        }

        private void ValidateAvailableInventoryForParsedRFs(List<RingFenceUploadModelNew> parsedRFs, List<Tuple<RingFenceUploadModelNew, string>> errorList, List<RingFenceUploadModelNew> validRFs, List<EcommRingFence> ecommRF)
        {
            List<Tuple<string, string, string>> uniqueCombos = new List<Tuple<string, string, string>>();
            RingFenceDAO rfDAO = new RingFenceDAO();

            // unique combos excluding ringfences with POs (available)
            uniqueCombos = parsedRFs.Where(pr => string.IsNullOrEmpty(pr.PO)).Select(pr => Tuple.Create(pr.Sku, pr.Size, pr.DC)).Distinct().ToList();
            List<WarehouseInventory> details = rfDAO.GetWarehouseAvailableNew(uniqueCombos);
            // reduce details
            details = rfDAO.ReduceRingFenceQuantities(details);

            // remove all parsedRFs that do not have an associated mainframe warehouse inventory record.
            parsedRFs.Where(pr => string.IsNullOrEmpty(pr.PO) && !details.Any(d => d.Sku.Equals(pr.Sku) && d.size.Equals(pr.Size) && d.DistributionCenterID.Equals(pr.DC)))
                     .ToList()
                     .ForEach(pr =>
                     {
                         SetErrorMessage(errorList, pr, string.Format("The Sku, Size, and DC combination could not be found within our system.", pr.Sku, pr.Size, pr.DC));
                         parsedRFs.Remove(pr);
                     });

            // unique non-future combos with summed quantity
            var uniqueRFsGrouped = parsedRFs.GroupBy(pr => new { Sku = pr.Sku, Size = pr.Size, DC = pr.DC })
                                             .Select(pr => new { Sku = pr.Key.Sku, Size = pr.Key.Size, DC = pr.Key.DC, Quantity = pr.Sum(rf => rf.Quantity) })
                                             .ToList();

            var invalidRingFenceQuantity = (from rf in uniqueRFsGrouped
                                            join d in details
                                              on new { Sku = rf.Sku, Size = rf.Size, DC = rf.DC } equals
                                                 new { Sku = d.Sku, Size = d.size, DC = d.DistributionCenterID }
                                            where rf.Quantity > d.availableQuantity
                                           select Tuple.Create(rf, d.availableQuantity)).ToList();

            foreach (var rf in invalidRingFenceQuantity)
            {
                var rfsToDelete = parsedRFs.Where(pr => pr.Sku.Equals(rf.Item1.Sku) &&
                                                        pr.Size.Equals(rf.Item1.Size) &&
                                                        pr.DC.Equals(rf.Item1.DC)).ToList();
                rfsToDelete.ForEach(drf =>
                {
                    SetErrorMessage(errorList, drf
                        , string.Format("Not enough inventory available for all sizes.  Available inventory: {0}", rf.Item2));
                    parsedRFs.Remove(drf);
                });
            }

            // unique Sku, Size, PO, WhseID combos for future pos
            var uniqueFutureCombos = parsedRFs.Where(pr => !string.IsNullOrEmpty(pr.PO))
                                              .Select(pr => Tuple.Create(pr.Sku, pr.Size, pr.PO, pr.DC))
                                              .Distinct().ToList();

            details = rfDAO.GetFuturePOsNew(uniqueFutureCombos);
            // reduce details
            details = rfDAO.ReduceRingFenceQuantities(details);

            // unique future combos with summed quantity
            var uniqueFutureCombosGrouped = parsedRFs.Where(pr => !string.IsNullOrEmpty(pr.PO))
                                                     .GroupBy(pr => new { Sku = pr.Sku, PO = pr.PO, Size = pr.Size, DC = pr.DC })
                                                     .Select(pr => new { Sku = pr.Key.Sku, PO = pr.Key.PO, Size = pr.Key.Size, DC = pr.Key.DC, Quantity = pr.Sum(rf => rf.Quantity) })
                                                     .ToList();

            var nonExistentCombo = uniqueFutureCombosGrouped.Where(ucg => !details.Any(d => d.Sku.Equals(ucg.Sku) &&
                                                                                            d.PO.Equals(ucg.PO) &&
                                                                                            d.size.Equals(ucg.Size) &&
                                                                                            d.DistributionCenterID.Equals(ucg.DC))).ToList();

            foreach (var nec in nonExistentCombo)
            {
                var futureRFsToDelete = parsedRFs.Where(pr => pr.Sku.Equals(nec.Sku) &&
                                                              pr.PO.Equals(nec.PO) &&
                                                              pr.Size.Equals(nec.Size) &&
                                                              pr.DC.Equals(nec.DC)).ToList();

                futureRFsToDelete.ForEach(r =>
                {
                    SetErrorMessage(errorList, r, "Could not find any quantity within the system");
                    parsedRFs.Remove(r);
                });
            }

            // summed quantity > mainframe quantity (details)
            var invalidFutureCombos = (from r in uniqueFutureCombosGrouped
                                       join d in details
                                         on new { Sku = r.Sku, PO = r.PO, Size = r.Size, DC = r.DC }
                                     equals new { Sku = d.Sku, PO = d.PO, Size = d.size, DC = d.DistributionCenterID }
                                      where r.Quantity > d.availableQuantity
                                     select Tuple.Create(r, d.availableQuantity)).ToList();

            foreach (var r in invalidFutureCombos)
            {
                var futureRFsToDelete = parsedRFs.Where(pr => pr.Sku.Equals(r.Item1.Sku) &&
                                                              pr.PO.Equals(r.Item1.PO) &&
                                                              pr.Size.Equals(r.Item1.Size) &&
                                                              pr.DC.Equals(r.Item1.DC)).ToList();

                futureRFsToDelete.ForEach(rftd =>
                {
                    SetErrorMessage(errorList, rftd
                        , string.Format("Not enough inventory on the PO for all sizes.  Available inventory {0}", r.Item2));
                    parsedRFs.Remove(rftd);
                });
            }

            validRFs.AddRange(parsedRFs);
        }

        private bool ValidateFile(List<RingFenceUploadModelNew> parsedRFs, List<Tuple<RingFenceUploadModelNew, string>> errorList)
        {
            // remove all records that have a null or empty division... we check to see if users have access
            // to the specified division and cannot do this validation without removing these
            parsedRFs.Where(pr => string.IsNullOrEmpty(pr.Division))
                     .ToList()
                     .ForEach(rf =>
                     {
                         SetErrorMessage(errorList, rf, "Division must be provided.");
                         parsedRFs.Remove(rf);
                     });

            // check to see if user has permission for each division, if not remove them.
            string userName = User.Identity.Name.Split('\\')[1];
            List<string> uniqueDivisions = parsedRFs.Select(pr => pr.Division).Distinct().ToList();
            List<string> invalidDivisions = uniqueDivisions.Where(div => !Footlocker.Common.WebSecurityService.UserHasDivision(userName, "Allocation", div)).ToList();
            parsedRFs.Where(pr => invalidDivisions.Contains(pr.Division))
                     .ToList()
                     .ForEach(rf =>
                     {
                         SetErrorMessage(errorList, rf, string.Format("You do not have permission for Division {0}.", rf.Division));
                         parsedRFs.Remove(rf);
                     });

            // check to see if there are any duplicates and remove them
            parsedRFs.GroupBy(pr => new { pr.Division, pr.Store, pr.Sku, pr.EndDate, pr.PO, pr.DC, pr.Size })
                     .Where(pr => pr.Count() > 1)
                     .Select(pr => new { DuplicateRFs = pr.ToList(), Counter = pr.Count() })
                     .ToList().ForEach(rf =>
                     {
                         // set error message for first duplicate and the amount of times it was found in the file
                         SetErrorMessage(errorList, rf.DuplicateRFs.FirstOrDefault(), string.Format(
                             "The following row of data was duplicated in the spreadsheet {0} times.  Please provide unique rows of data.", rf.Counter));
                         // delete all instances of the duplications from the parsedRFs list
                         rf.DuplicateRFs.ForEach(drf => parsedRFs.Remove(drf));
                     });

            if (parsedRFs.Count == 0)
            {
                return false;
            }

            return true;
        }

        private void SetErrorMessage(List<Tuple<RingFenceUploadModelNew, string>> errorList, RingFenceUploadModelNew errorRF, string newErrorMessage)
        {
            int tupleIndex = errorList.FindIndex(err => err.Item1.Equals(errorRF));
            if (tupleIndex > -1)
            {
                errorList[tupleIndex] = Tuple.Create(errorRF, string.Format("{0} {1}", errorList[tupleIndex].Item2, newErrorMessage));
            }
            else
            {
                errorList.Add(Tuple.Create(errorRF, newErrorMessage));
            }
        }

        private RingFenceUploadModelNew ParseUploadRow(Aspose.Excel.Worksheet mySheet, int row)
        {
            RingFenceUploadModelNew returnValue = new RingFenceUploadModelNew();

            returnValue.Division = Convert.ToString(mySheet.Cells[row, 0].Value);
            var store = Convert.ToString(mySheet.Cells[row, 1].Value);
            returnValue.Store = (string.IsNullOrEmpty(store)) ? "" : store.PadLeft(5, '0');
            returnValue.Sku = Convert.ToString(mySheet.Cells[row, 2].Value);
            var endDate = Convert.ToString(mySheet.Cells[row, 3].Value);
            if (!string.IsNullOrEmpty(endDate))
            {
                returnValue.EndDate = Convert.ToDateTime(endDate);
            }
            returnValue.PO = Convert.ToString(mySheet.Cells[row, 4].Value);
            returnValue.DC = Convert.ToString(mySheet.Cells[row, 5].Value).Trim().PadLeft(2, '0');
            returnValue.Size = Convert.ToString(mySheet.Cells[row, 6].Value);
            returnValue.Quantity = Convert.ToInt32(Math.Round(Convert.ToDecimal(mySheet.Cells[row, 7].Value)));
            returnValue.Comments = Convert.ToString(mySheet.Cells[row, 8].Value);

            return returnValue;
        }

        private bool HasDataOnRow(Aspose.Excel.Worksheet mySheet, int row)
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

        private bool HasValidHeaderRow(Aspose.Excel.Worksheet mySheet)
        {
            return
                (Convert.ToString(mySheet.Cells[0, 0].Value).Contains("Div")) &&
                (Convert.ToString(mySheet.Cells[0, 1].Value).Contains("Store")) &&
                (Convert.ToString(mySheet.Cells[0, 2].Value).Contains("SKU")) &&
                (Convert.ToString(mySheet.Cells[0, 3].Value).Contains("EndDate")) &&
                (Convert.ToString(mySheet.Cells[0, 4].Value).Contains("PO")) &&
                (Convert.ToString(mySheet.Cells[0, 5].Value).Contains("Warehouse")) &&
                (Convert.ToString(mySheet.Cells[0, 6].Value).Contains("Size")) &&
                (Convert.ToString(mySheet.Cells[0, 7].Value).Contains("Qty")) &&
                (Convert.ToString(mySheet.Cells[0, 8].Value).Contains("Comments"));
        }

        #endregion
    }
}
