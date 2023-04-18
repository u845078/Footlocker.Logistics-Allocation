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
using Footlocker.Logistics.Allocation.Common;
using System.Web.Services.Description;
using Footlocker.Common.Entities;
using Telerik.Web.Mvc.Infrastructure;
//using Aspose.Cells;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Director of Allocation,Admin,Support")]
    public class RingFenceController : AppController
    {
        #region Fields

        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
        readonly RingFenceDAO dao = new RingFenceDAO();
        readonly ConfigService configService = new ConfigService();

        // NOTE: Both caselot name and size are stored in the same varchar db column, if value is more than 3 digits, we know it is a caselot....
        public static int _CASELOT_SIZE_INDICATOR_VALUE_LENGTH = 3;

        #endregion

        //
        // GET: /RingFence/

        #region Non-Public Methods

        private List<RingFenceDetail> GetFutureAvailable(RingFence ringFence)
        {
            List<RingFenceDetail> list;
            
            list = dao.GetFuturePOs(ringFence);
            list.AddRange(dao.GetTransloadPOs(ringFence));
            return list;
        }

        private List<RingFenceDetail> GetRingFenceDetails(long ringFenceID)
        {
            // Get ring fence data...
            // HACK: Should really be relational, and pulled in on a single query from EF
            var ringFenceItemName = db.RingFences.AsNoTracking().Single(rf => rf.ID == ringFenceID).Sku;
            var ringFenceItemID = db.ItemMasters.Single(i => i.MerchantSku == ringFenceItemName).ID;
            var ringFenceDetails = db.RingFenceDetails.AsNoTracking().Where(d => d.RingFenceID == ringFenceID && 
                                                                                 d.ActiveInd == "1").ToList();
            var dcs = db.DistributionCenters.ToList();
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
                det.Warehouse = dcs.Where(d => d.ID == det.DCID).First().Name;
            }

            return ringFenceDetails;
        }

        private List<RingFenceDetail> GetWarehouseAvailable(RingFence ringFence)
        {
            return dao.GetWarehouseAvailable(ringFence.Sku, ringFence.Size, ringFence.ID)
                .OrderBy(r => r.Size.Length)
                .ThenBy(rf => rf.Size).ToList();            
        }

        private string ValidateStorePick(RingFencePickModel rf)
        {
            if (string.IsNullOrEmpty(rf.Store))            
                return "Store is required for Pick.";            

            foreach (RingFenceDetail det in rf.Details)
            {
                if (det.AssignedQty > det.Qty)
                    return "Qty must be less than ring fence Qty.";                
            }
            return "";
        }
        #endregion

        public ActionResult Index(string message)
        {            
            List<RingFenceModel> model = new List<RingFenceModel>();

            List<GroupedRingFence> list = dao.GetValidRingFenceGroups(currentUser.GetUserDivList(AppName));

            Dictionary<string, string> fullNamePairs = new Dictionary<string, string>();
            var uniqueNames = (from a in list
                               where !string.IsNullOrEmpty(a.LastModifiedUser)
                               select a.LastModifiedUser).Distinct();

            List<ApplicationUser> allUserNames = GetAllUserNamesFromDatabase();

            foreach (var item in uniqueNames)
            {
                if (!item.Contains(" ") && !string.IsNullOrEmpty(item))
                {
                    string userLookup = item.Replace('\\', '/');
                    userLookup = userLookup.Replace("CORP/", "");

                    if (userLookup.Substring(0, 1) == "u")
                    {
                        fullNamePairs.Add(item, allUserNames.Where(aun => aun.UserName == userLookup).Select(aun => aun.FullName).FirstOrDefault());
                    }
                    else
                        fullNamePairs.Add(item, item);
                }
                else
                    fullNamePairs.Add(item, item);
            }

            foreach (var item in fullNamePairs)
            {
                list.Where(x => x.LastModifiedUser == item.Key).ToList().ForEach(y => y.LastModifiedUser = item.Value);
            }

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
            List<RingFence> list = dao.GetRingFences(currentUser.GetUserDivList(AppName));

            var rfGroups = from rf in list
            group rf by new
            {
                rf.Sku,
                itemid = rf.ItemID,
            } into g
            select new RingFenceSummary()
            {
                Sku = g.Key.Sku,
                Size = "",
                DC = "",
                PO = "",
                Qty = g.Sum(r => r.Qty),
                CanPick = !g.Any(r => !r.CanPick)
            };

            return PartialView(new GridModel(rfGroups.ToList()));
        }

        [GridAction]
        public ActionResult _RingFenceStores()
        {
            List<Footlocker.Common.Division> divs = currentUser.GetUserDivisions(AppName);
            List<StoreLookup> list = (from a in db.RingFences
                                      join rfd in db.RingFenceDetails
                                        on a.ID equals rfd.RingFenceID
                                      join b in db.StoreLookups 
                                        on new { a.Division, a.Store } equals new { b.Division, b.Store }
                                      where a.Qty > 0 && rfd.ActiveInd == "1"
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

            List<Footlocker.Common.Division> divs = currentUser.GetUserDivisions(AppName);
            List<FOB> list = (from a in db.RingFences 
                              join b in db.StoreLookups 
                              on new { a.Division, a.Store } equals new { b.Division, b.Store } 
                              join c in db.ItemMasters 
                              on a.ItemID equals c.ID
                              join d in db.FOBDepts 
                              on c.Dept equals d.Department
                              join e in db.FOBs 
                              on d.FOBID equals e.ID
                              where a.Qty > 0 && 
                                    a.Store == store && 
                                    a.Division == div && 
                                    e.Division == div
                              select e).Distinct().ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    select a).Distinct().ToList();

            return PartialView(new GridModel(list));
        }

        [GridAction]
        public ActionResult _RingFences(string sku)
        {
            List<Footlocker.Common.Division> divs = currentUser.GetUserDivisions(AppName);
            List<RingFence> list = db.RingFences.Where(rf => rf.Qty > 0 && rf.Sku == sku).ToList();
            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    select new RingFence{ 
                        ID = a.ID,
                        Sku = a.Sku, 
                        Size = a.Size, 
                        Division = a.Division,
                        Store = a.Store,
                        Qty = a.Qty,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate,
                        Type = a.Type,
                        Comments = a.Comments,
                        CreatedBy = a.CreatedBy,
                        CreateDate = a.CreateDate,
                        RingFenceTypeDescription = a.RingFenceTypeDescription,
                        ItemDescription = a.ItemDescription,
                        LastModifiedDate = a.LastModifiedDate,
                        LastModifiedUser = a.LastModifiedUser
                    }).OrderByDescending(x => x.CreateDate).ToList();

            Dictionary<string, string> names = new Dictionary<string, string>();
            var users = (from a in list
                         select a.LastModifiedUser).Distinct();
            foreach (string userID in users)
            {
                names.Add(userID, getFullUserNameFromDatabase(userID.Replace('\\', '/')));
            }

            foreach (var item in list)
            {
                item.LastModifiedUserName = names[item.LastModifiedUser];
            }

            return PartialView(new GridModel(list));
        }

        [GridAction]
        public ActionResult _RingFencesForStore(string div, string store)
        {
            List<RingFence> list;
            List<Footlocker.Common.Division> divs = currentUser.GetUserDivisions(AppName);
            list = db.RingFences.Where(rf => rf.Qty > 0 && rf.Division == div && rf.Store == store).ToList();

            list = (from a in list
                    join d in divs on a.Division equals d.DivCode
                    select new RingFence
                    {
                        ID = a.ID,
                        Sku = a.Sku,
                        Size = a.Size,
                        Division = a.Division,
                        Store = a.Store,
                        Qty = a.Qty,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate,
                        Type = a.Type,
                        Comments = a.Comments,
                        CreatedBy = a.CreatedBy,
                        CreateDate = a.CreateDate,
                        LastModifiedDate = a.LastModifiedDate,
                        LastModifiedUser = a.LastModifiedUser,
                        RingFenceTypeDescription = a.RingFenceTypeDescription,
                        ItemDescription = a.ItemDescription
                    }).OrderByDescending(x => x.CreateDate).ToList();

            Dictionary<string, string> names = new Dictionary<string, string>();
            var users = (from a in list
                         select a.LastModifiedUser).Distinct();
            foreach (string userID in users)
            {
                names.Add(userID, getFullUserNameFromDatabase(userID.Replace('\\', '/')));
            }
            foreach (var item in list)
            {
                item.LastModifiedUserName = names[item.LastModifiedUser];
            }

            return PartialView(new GridModel(list));
        }

        [GridAction]
        public ActionResult _RingFencesForFOB(string div, string store, string fob)
        {
            div = div.Trim();
            store = store.Trim();

            List<RingFence> list;
            if ((Session["rfFOBStore"] != null) &&
                ((string)Session["rfFOBStore"] == div + store + fob))
            {
                list = (List<RingFence>)Session["rfFOBStoreList"];
            }
            else
            {
                List<Footlocker.Common.Division> divs = currentUser.GetUserDivisions(AppName);
                list = (from a in db.RingFences 
                        join i in db.ItemMasters on a.ItemID equals i.ID
                        join b in db.FOBDepts on i.Dept equals b.Department
                        join c in db.FOBs on b.FOBID equals c.ID
                        where a.Qty > 0 && a.Division == div && a.Store == store && c.Name == fob && c.Division == div
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

            return PartialView(new GridModel(list));
        }

        public ActionResult Create()
        {
            RingFenceModel model = new RingFenceModel()
            {
                RingFence = new RingFence(),
                Divisions = currentUser.GetUserDivisions(AppName)
            };

            return View(model);
        }

        public void SetUpRingFenceHeader(RingFence rf)
        {
            ItemDAO itemDAO = new ItemDAO();
            rf.ItemID = itemDAO.GetItemID(rf.Sku);

            int instanceID = configService.GetInstance(rf.Division);
            rf.StartDate = configService.GetControlDate(instanceID).AddDays(1);

            rf.CreateDate = DateTime.Now;
            rf.CreatedBy = currentUser.NetworkID;
            rf.LastModifiedDate = DateTime.Now;
            rf.LastModifiedUser = currentUser.NetworkID;
            
            if (dao.isEcommWarehouse(rf.Division, rf.Store))
                rf.Type = 2;
            else
                rf.Type = 1;

            rf.Qty = 0;

            rf.ringFenceDetails = new List<RingFenceDetail>();
        }

        [HttpPost]
        public ActionResult Create(RingFenceModel model)
        {
            model.Divisions = currentUser.GetUserDivisions(AppName);            
            string errorMessage;

            if (!dao.isValidRingFence(model.RingFence, currentUser, AppName, out errorMessage))
            {
                ViewData["message"] = errorMessage;
                return View(model);
            }
            else
            {
                SetUpRingFenceHeader(model.RingFence);
                
                db.RingFences.Add(model.RingFence);
                db.SaveChanges(currentUser.NetworkID);

                return AssignInventory(model);
            }
        }

        public ActionResult AssignInventory(RingFenceModel model)
        {
            ViewData["ringFenceID"] = model.RingFence.ID;

            model.Divisions = currentUser.GetUserDivisions(AppName);

            return View("AssignInventory", model);
        }

        public ActionResult Details(int ID)
        {
            RingFenceModel model = new RingFenceModel()
            {
                RingFence = db.RingFences.Where(rf => rf.ID == ID).First(),
                FutureAvailable = db.RingFenceDetails.Where(rfd => rfd.RingFenceID == ID && rfd.ActiveInd == "1").ToList(),
                Divisions = currentUser.GetUserDivisions(AppName)
            };

            List<DistributionCenter> dcs = db.DistributionCenters.ToList();

            foreach (RingFenceDetail det in model.FutureAvailable)
            {
                det.Warehouse = (from a in dcs
                                 where a.ID == det.DCID
                                 select a.Name).First();
            }
            
            return View(model);
        }

        [GridAction]
        public ActionResult _Audit(int ID)
        {
            List<DistributionCenter> dcs = db.DistributionCenters.ToList();
            List<RingFenceHistory> model = db.RingFenceHistory.Where(rfh => rfh.RingFenceID == ID).ToList();

            foreach (RingFenceHistory h in model)
            {
                h.Warehouse = (from a in dcs 
                               where a.ID == h.DCID 
                               select a.Name).FirstOrDefault();
            }

            Dictionary<string, string> names = new Dictionary<string, string>();
            var users = (from a in model
                         select a.CreatedBy).Distinct();
            foreach (string userID in users)
            {
                names.Add(userID, getFullUserNameFromDatabase(userID.Replace('\\', '/')));
            }
            foreach (var item in model)
            {
                item.CreatedByName = names[item.CreatedBy];
            }

            return View(new GridModel(model));
        }

        public ActionResult Audit(int ID)
        {
            RingFenceModel model = new RingFenceModel()
            {
                RingFence = db.RingFences.Where(rf => rf.ID == ID).FirstOrDefault()
            };            

            return View(model);
        }

        public ActionResult SizeSummary(int ID)
        {
            List<RingFenceSizeSummary> sizes = new List<RingFenceSizeSummary>();

            List<RingFenceDetail> details = (from a in db.RingFenceDetails
                                             where a.RingFenceID == ID &&
                                                   a.ActiveInd == "1" && 
                                                   a.Size.Length == _CASELOT_SIZE_INDICATOR_VALUE_LENGTH
                                             select a).ToList();
            List<RingFenceDetail> caselotDetails = (from a in db.RingFenceDetails
                                                    where a.RingFenceID == ID &&
                                                          a.ActiveInd == "1" &&
                                                          a.Size.Length > _CASELOT_SIZE_INDICATOR_VALUE_LENGTH
                                                    select a).ToList();

            RingFenceSizeSummary currentSize = new RingFenceSizeSummary();

            foreach (RingFenceDetail det in details)
            { 
                var query = sizes.Where(s => s.Size == det.Size);
                if (query.Count() > 0)                
                    currentSize = query.First();                
                else
                {
                    currentSize = new RingFenceSizeSummary()
                    { 
                        Size = det.Size 
                    };
                    
                    sizes.Add(currentSize);
                }
                if (!string.IsNullOrEmpty(det.PO))                
                    currentSize.FutureQty += det.Qty;
                
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
                        var query = sizes.Where(s => s.Size == pd.Size);
                        if (query.Count() > 0)                        
                            currentSize = query.First();                        
                        else
                        {
                            currentSize = new RingFenceSizeSummary()
                            {
                                Size = pd.Size
                            };

                            sizes.Add(currentSize);
                        }
                        if (!string.IsNullOrEmpty(det.PO))                        
                            currentSize.FutureQty += pd.Quantity * det.Qty;
                        
                        currentSize.CaselotQty += pd.Quantity * det.Qty;
                        currentSize.TotalQty += pd.Quantity * det.Qty;
                    }
                }
                catch
                {
                }
            }

            RingFenceSizeModel model = new RingFenceSizeModel()
            {
                RingFence = db.RingFences.Where(rf => rf.ID == ID).First(), 
                Details = sizes, 
                Divisions = currentUser.GetUserDivisions(AppName)
            };
            
            return View(model);
        }

        [GridAction]
        public ActionResult _SelectSizeDetail(int ID, string size)
        {
            List<RingFenceDetail> list = (from a in db.RingFenceDetails
                                          where a.RingFenceID == ID && 
                                                a.ActiveInd == "1" &&
                                                (a.Size == size || a.Size.Length > _CASELOT_SIZE_INDICATOR_VALUE_LENGTH)
                                          select a).ToList();

            List<RingFenceDetail> model = new List<RingFenceDetail>();
            List<DistributionCenter> dcs = db.DistributionCenters.ToList(); 
            bool includeDetail = false;
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
                    catch 
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
            RingFenceModel model = new RingFenceModel()
            {
                RingFence = db.RingFences.Where(rf => rf.ID == ID).First(),
                FutureAvailable = new List<RingFenceDetail>(),
                Divisions = currentUser.GetUserDivisions(AppName)
            };

            ViewData["Size"] = size;
            return View(model);
        }

        public ActionResult Edit(int ID)
        {
            ViewData["ringFenceID"] = ID;
            string errorMessage;

            // Build up a RingFence view model
            RingFenceModel model = new RingFenceModel()
            {
                Divisions = currentUser.GetUserDivisions(AppName)
            };

            RingFence ringfence = db.RingFences.Where(rf => rf.ID == ID).FirstOrDefault();

            if (ringfence == null)            
                return RedirectToAction("Index", new { message = "Ring fence no longer exists." });
            
            model.RingFence = ringfence;

            if (!dao.CanUserUpdateRingFence(model.RingFence, currentUser, AppName, out errorMessage))            
                return RedirectToAction("Index", new { message = errorMessage });                        

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(RingFenceModel model)
        {
            string errorMessage;

            try
            {
                model.Divisions = currentUser.GetUserDivisions(AppName);
                ViewData["ringFenceID"] = model.RingFence.ID;
                if (!dao.CanUserUpdateRingFence(model.RingFence, currentUser, AppName, out errorMessage))
                {
                    ViewData["message"] = errorMessage;
                    return View(model);
                }
                else
                {
                    // get original ringfence to see if enddate is being changed
                    DateTime? originalEndDate = db.RingFences.Where(rf => rf.ID == model.RingFence.ID)
                                                             .Select(rf => rf.EndDate)
                                                             .FirstOrDefault();

                    if (originalEndDate != model.RingFence.EndDate)
                    {
                        // if date is now null or greater than current date, then check details
                        if (model.RingFence.EndDate == null || model.RingFence.EndDate >= DateTime.Now)
                        {
                            List<RingFenceDetail> warehouseInv = GetWarehouseAvailable(model.RingFence);

                            foreach (var d in warehouseInv)
                            {
                                if (d.Qty > 0)
                                {
                                    int reduce = d.AvailableQty - d.Qty;
                                    if (reduce < 0)
                                    {
                                        errorMessage = string.Format("The ringfence cannot be reactivated.  The total available quantity ({0}) for size {1} is less than the entered quantity ({2})."
                                            , d.AvailableQty, d.Size, d.Qty);

                                        model.RingFence.EndDate = originalEndDate;
                                        ViewData["message"] = errorMessage;
                                        return View(model);
                                    }
                                }
                            }
                        }
                    }
                }

                if (db.EcommWarehouses.Any(ew => ew.Division == model.RingFence.Division && ew.Store == model.RingFence.Store))
                    model.RingFence.Type = 2;
                else
                    model.RingFence.Type = 1;

                model.RingFence.CreatedBy = currentUser.NetworkID;
                model.RingFence.CreateDate = DateTime.Now;
                model.RingFence.LastModifiedDate = DateTime.Now;
                model.RingFence.LastModifiedUser = currentUser.NetworkID;

                db.Entry(model.RingFence).State = System.Data.EntityState.Modified;

                db.SaveChanges(currentUser.NetworkID);

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

            RingFence rf = db.RingFences.Where(r => r.ID == ID).FirstOrDefault();

            if (rf == null)            
                return RedirectToAction("Index", new { message = "Ringfence no longer exists." });                       

            if (!dao.CanUserUpdateRingFence(rf, currentUser, AppName, out errorMessage))            
                return RedirectToAction("Index", new { message = errorMessage });            

            List<RingFenceDetail> details = db.RingFenceDetails.Where(rfd => rfd.RingFenceID == ID).ToList();

            foreach (RingFenceDetail det in details)
            {
                db.RingFenceDetails.Remove(det);
                db.SaveChanges(currentUser.NetworkID);
            }

            db.RingFences.Remove(rf);
            db.SaveChanges(currentUser.NetworkID);

            return RedirectToAction("Index");
        }

        public ActionResult MassDeleteRingfence(string sku)
        {
            List<RingFence> ringfences = db.RingFences.Where(rf => rf.Sku == sku).ToList();

            string message = BulkDeleteRingFences(sku, ringfences);

            return RedirectToAction("IndexSummary", new { message });
        }

        public ActionResult MassDeleteRingfenceStore(string div, string store)
        {
            List<RingFence> ringfences = db.RingFences.Where(rf => rf.Division == div && rf.Store == store).ToList();

            string message = BulkDeleteRingFences(div + "-" + store, ringfences);

            return RedirectToAction("IndexByStore", new { message });
        }

        public ActionResult MassDeleteRingfenceFOB(string div, string store, string fob)
        {
            List<RingFence> rfList = (from a in db.RingFences
                                      join b in db.ItemMasters on a.ItemID equals b.ID
                                      join c in db.FOBDepts on b.Dept equals c.Department
                                      join d in db.FOBs on c.FOBID equals d.ID
                                      where a.Division == div && a.Store == store && d.Name == fob && d.Division == div
                                      select a).ToList();

            string message = BulkDeleteRingFences(string.Format("{0}-{1} {2}", div, store, fob), rfList);

            return RedirectToAction("IndexByStore", new { message });
        }

        private string BulkDeleteRingFences(string key, List<RingFence> ringfences)
        {
            int count = 0;
            int permissionCount = 0;
            foreach (RingFence rf in ringfences)
            {
                if (!currentUser.HasDivision(AppName, rf.Division))                 
                    permissionCount++;                
                else if (!currentUser.HasDivDept(AppName, rf.Division, rf.Department))                
                    permissionCount++;                
                else
                {
                    List<RingFenceDetail> details = db.RingFenceDetails.Where(rfd => rfd.RingFenceID == rf.ID).ToList();

                    foreach (RingFenceDetail det in details)
                    {
                        db.RingFenceDetails.Remove(det);
                        db.SaveChanges(currentUser.NetworkID);
                    }

                    db.RingFences.Remove(rf);
                    count++;
                }
            }

            if (count > 0)            
                db.SaveChanges(currentUser.NetworkID);            

            string message = string.Format("Deleted {0} ringfences for {1}. ", count, key);
            
            if (permissionCount > 0)            
                message += permissionCount + " you do not have permission.  ";
            
            return message;
        }

        public ActionResult SelectStorePick(RingFence rf)
        {
            //FLOW:  Pick a division/store
            //show rdq's for everything on there, with a qty that they can input
            //they click save, it removes that amount
            //when all details have no qty, then delete the rdq

            RingFencePickModel model = new RingFencePickModel()
            {
                RingFence = rf,
                Details = GetRingFenceDetails(rf.ID),
                Divisions = currentUser.GetUserDivisions(AppName)
            };

            return View(model);
        }

        public ActionResult PickStorePick(RingFencePickModel rf)
        {
            bool optionalPick = false;
            // Validate
            if (!ModelState.IsValid)              
                return View("SelectStorePick", rf);             

            // Apply pick database changes and update model object directly with any necessary validation messages
            //FLOW:  Pick a division/store
            //show rdq's for everything on there, with a qty that they can input
            //they click save, it removes that amount
            //when all details have no qty, then delete the rdq
            int pickedQty = 0;
            rf.Message = ValidateStorePick(rf);

            int instanceID = configService.GetInstance(rf.Division);
            DateTime controlDate = configService.GetControlDate(instanceID);

            //startdate is the next control date, so make sure no details have POs if they do, warn them.
            if (rf.RingFence.StartDate > controlDate)
                optionalPick = true;            

            if (rf.Message.Length == 0)
            {
                //create RDQ's
                RingFenceHistory history = new RingFenceHistory()
                {
                    RingFenceID = rf.RingFence.ID,
                    Division = rf.Division,
                    Store = rf.Store,
                    Action = "Picked",
                    CreateDate = DateTime.Now,
                    CreatedBy = currentUser.NetworkID
                };

                db.RingFenceHistory.Add(history);

                List<RDQ> rdqsToCheck = new List<RDQ>();

                List<RingFenceDetail> deleteList = new List<RingFenceDetail>();
                foreach (RingFenceDetail det in rf.Details.Where(d => d.AssignedQty > 0))
                {
                    RDQ rdq = new RDQ()
                    {
                        Sku = rf.RingFence.Sku,
                        Size = det.Size,
                        Store = rf.Store,
                        PO = det.PO,
                        Division = rf.Division,
                        DCID = det.DCID,
                        CreatedBy = currentUser.NetworkID,
                        CreateDate = DateTime.Now
                    };

                    //don't let them take too much
                    if (det.AssignedQty > det.Qty)                                            
                        rdq.Qty = det.Qty;                    
                    else                    
                        rdq.Qty = det.AssignedQty;

                    ItemDAO itemDAO = new ItemDAO();
                    rdq.ItemID = itemDAO.GetItemID(rdq.Sku);
                    
                    SetRDQDefaults(det, rdq);

                    if (!string.IsNullOrEmpty(rdq.PO) && optionalPick)
                    {
                        rdq.Type = "user_opt";
                        rf.Message = "Since this was created today, it will NOT be honored if the PO is delivered today.";
                    }

                    rdqsToCheck.Add(rdq);

                    history = new RingFenceHistory()
                    {
                        RingFenceID = det.RingFenceID,
                        Division = rf.Division,
                        Store = rf.Store,
                        DCID = det.DCID,
                        PO = det.PO,
                        Qty = det.AssignedQty,
                        CreateDate = DateTime.Now,
                        CreatedBy = currentUser.NetworkID
                    };

                    //reduce ringfence detail or delete
                    if (det.AssignedQty >= det.Qty)
                    {
                        history.Action = "Picked Det";
                        if (det.PO == null)                        
                            det.PO = "";
                        
                        RingFenceDetail delete = db.RingFenceDetails.Where(rfd => rfd.DCID == det.DCID && 
                                                                                  rfd.RingFenceID == det.RingFenceID && 
                                                                                  rfd.Size == det.Size && 
                                                                                  rfd.PO == det.PO).First();
                        pickedQty += det.Qty;
                        db.RingFenceDetails.Remove(delete);

                        deleteList.Add(det);
                    }
                    else if (det.AssignedQty > 0)
                    {
                        history.Action = "Partial Picked Det";

                        // Get current detail record (so EF doesnt geek out about about attaching an entity that hasnt been loaded yet)
                        var currDetail = db.RingFenceDetails.First(d => d.RingFenceID == det.RingFenceID && d.Size == det.Size && (d.DCID == det.DCID));

                        // Decrement quantity from detail record that was picked from
                        currDetail.Qty -= det.AssignedQty;
                        currDetail.PO = det.PO ?? string.Empty;

                        pickedQty += det.AssignedQty;
                    }

                    db.RingFenceHistory.Add(history);
                }

                int holdCount = CheckHolds(rf.Division, rdqsToCheck);
                if (holdCount > 0)                
                    rf.Message = holdCount + " on hold.  Please see ReleaseHeldRDQs for held RDQs.";                

                int cancelHoldCount = CheckCancelHolds(rdqsToCheck);

                if (cancelHoldCount > 0)                
                    rf.Message = cancelHoldCount + " rejected because of cancel inventory hold.";                

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
                    RingFence deleteRF = db.RingFences.Where(drf => drf.ID == history.RingFenceID).First();
                    db.RingFences.Remove(deleteRF);
                    rf.Message += " Ring fence picked, no quantity remaining.";
                }
                else                
                    rf.Message += " Ring fence picked, quantity remaining.";                

                db.SaveChanges(currentUser.NetworkID);
            }

            return View(rf);
        }

        private int CheckHolds(string division, List<RDQ> list)
        {
            int instance = configService.GetInstance(division);

            return (new RDQDAO()).ApplyHolds(list, instance);
        }

        private int CheckCancelHolds(List<RDQ> list)
        {
            return (new RDQDAO()).ApplyCancelHolds(list);
        }

        private static void SetRDQDefaults(RingFenceDetail det, RDQ rdq)
        {
            rdq.Type = "user";
            rdq.Status = "WEB PICK";
            if (!string.IsNullOrEmpty(det.PO))
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
            bool optionalPick = false;
            string message = null;
            string errorMessage;
            int instanceID;
            DateTime controlDate;
            RingFenceDAO rfDAO = new RingFenceDAO();

            var rf = db.RingFences.Where(r => r.ID == ID).FirstOrDefault();
            if (rf == null)            
                return RedirectToAction("Index", new { message = "Ring Fence no longer exists. Please verify." });            

            if (!rfDAO.CanUserUpdateRingFence(rf, currentUser, AppName, out errorMessage))            
                return RedirectToAction("Index", new { message = errorMessage });

            //this is an ecomm ringfence, you can't pick it
            if (rf.Type == 2)                            
                return RedirectToAction("Index", new { message = "Sorry, you cannot pick for an Ecomm warehouse. Do you mean Delete?" });

            instanceID = configService.GetInstance(rf.Division);
            controlDate = configService.GetControlDate(instanceID);

            //startdate is the next control date, so make sure no details have POs if they do, warn them.
            if (rf.StartDate > controlDate)
            {                 
                optionalPick = true;
            }
            if (string.IsNullOrEmpty(rf.Store))
            {
                //FLOW:  Pick a division/store
                //show rdq's for everything on there, with a qty that they can input
                //they click save, it removes that amount
                //when all details have no qty, then delete the rdq
                return RedirectToAction("SelectStorePick", rf);
            }
            List<RingFenceDetail> details = db.RingFenceDetails.Where(rfd => rfd.RingFenceID == ID &&
                                                                             rfd.ActiveInd == "1").ToList();

            RingFenceHistory history = new RingFenceHistory()
            {
                RingFenceID = ID,
                Action = "Picked",
                Division = rf.Division,
                Store = rf.Store,
                CreateDate = DateTime.Now,
                CreatedBy = currentUser.NetworkID
            };

            db.RingFenceHistory.Add(history);
            db.SaveChanges(currentUser.NetworkID);

            ItemDAO itemDAO = new ItemDAO();
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
                    ItemID = itemDAO.GetItemID(rf.Sku),
                    CreatedBy = currentUser.NetworkID,
                    CreateDate = DateTime.Now,
                    LastModifiedUser = currentUser.NetworkID
                };

                SetRDQDefaults(det, rdq);
                if (!string.IsNullOrEmpty(rdq.PO) && optionalPick)
                { 
                    rdq.Type = "user_opt";
                    message = "Since this was created today, it will NOT be honored if the PO is delivered today.";
                }

                rdqsToCheck.Add(rdq);

                db.RingFenceDetails.Remove(det);

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
                    CreatedBy = currentUser.NetworkID
                };

                db.RingFenceHistory.Add(history);
                db.SaveChanges(currentUser.NetworkID);
            }

            int holdCount = CheckHolds(rf.Division, rdqsToCheck);
            if (holdCount > 0)            
                message = holdCount + " on hold.  Please see ReleaseHeldRDQs for held RDQs. ";            

            int cancelHoldCount = CheckCancelHolds(rdqsToCheck);
            if (cancelHoldCount > 0)            
                message = cancelHoldCount + " rejected because of cancel inventory hold.";            

            foreach (RDQ rdq in rdqsToCheck)
            {
                db.RDQs.Add(rdq);
            }

            db.RingFences.Remove(rf);
            db.SaveChanges(currentUser.NetworkID);

            return RedirectToAction("Index", new { message });
        }

        public ActionResult MassPickRingFence(string sku)
        {
            List<RingFence> rfList = db.RingFences.Where(rf => rf.Sku == sku && rf.CanPick).ToList();
            string message = BulkPickRingFence(sku, rfList, true);

            return RedirectToAction("IndexSummary", new { message });
        }

        public ActionResult MassPickRingFenceNoFuture(string sku)
        {
            List<RingFence> rfList = db.RingFences.Where(rf => rf.Sku == sku).ToList();
            string message = BulkPickRingFence(sku, rfList, false);

            return RedirectToAction("IndexSummary", new { message });
        }

        private string BulkPickRingFence(string key, List<RingFence> rfList, bool pickPOs)
        {
            int noStoreCount = 0;
            int ecommCount = 0;
            int count = 0;
            int countWarehouseOnly = 0;
            int errorCount = 0;
            int permissionCount = 0;
            int deliveredToday = 0;
            bool optionalPick = false;
            List<RDQ> rdqs = new List<RDQ>();
            bool canDelete = true;
            int instanceID;
            DateTime controlDate;

            foreach (RingFence rf in rfList)
            {
                if (!currentUser.HasDivision(AppName, rf.Division))                
                    permissionCount++;                
                else if (!currentUser.HasDivDept(AppName, rf.Division, rf.Department))                
                    permissionCount++;                
                else if (rf.Type == 2)
                {
                    //this is an ecomm ringfence, skip it
                    ecommCount++;
                }
                else if (string.IsNullOrEmpty(rf.Store))
                {
                    //FLOW:  Pick a division/store
                    //show rdq's for everything on there, with a qty that they can input
                    //they click save, it removes that amount
                    //when all details have no qty, then delete the rdq
                    noStoreCount++;
                }
                else
                {
                    instanceID = configService.GetInstance(rf.Division);
                    controlDate = configService.GetControlDate(instanceID);

                    if (rf.StartDate > controlDate)
                    {
                        //startdate is the next control date, so make sure no details have POs if they do, warn them.
                        optionalPick = true;
                    }
                    try
                    {
                        List<RingFenceDetail> details = db.RingFenceDetails.Where(rfd => rfd.RingFenceID == rf.ID).ToList();
                        int detCount = details.Count();
                        if (!pickPOs)
                        {
                            details = details.Where(d => d.PO == "").ToList();
                            canDelete = detCount == details.Count();
                            if (details.Count() > 0)                            
                                countWarehouseOnly++;                            
                        }

                        RingFenceHistory history = new RingFenceHistory()
                        {
                            RingFenceID = rf.ID,
                            Division = rf.Division,
                            Store = rf.Store,
                            Action = "Picked",
                            CreateDate = DateTime.Now,
                            CreatedBy = currentUser.NetworkID
                        };

                        db.RingFenceHistory.Add(history);

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
                                          select a.ID).FirstOrDefault(),
                                CreatedBy = currentUser.NetworkID,
                                CreateDate = DateTime.Now,
                                LastModifiedUser = currentUser.NetworkID
                            };
                            SetRDQDefaults(det, rdq);
                            if (!string.IsNullOrEmpty(rdq.PO) && optionalPick)
                            {
                                rdq.Type = "user_opt";
                                deliveredToday++;
                            }

                            db.RDQs.Add(rdq);
                            rdqs.Add(rdq);

                            db.RingFenceDetails.Remove(det);

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
                                CreatedBy = currentUser.NetworkID
                            };
                            db.RingFenceHistory.Add(history);
                        }

                        if (canDelete)                        
                            db.RingFences.Remove(rf);
                        
                        db.SaveChanges(currentUser.NetworkID);
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
                int instance = configService.GetInstance(div);
                RDQDAO rdqDAO = new RDQDAO();
                holdCount = rdqDAO.ApplyHolds(rdqs, instance);
                cancelholdcount = rdqDAO.ApplyCancelHolds(rdqs);
            }

            string message = string.Format("Picked {0} Ring Fences for {1}. ", count.ToString(), key);
            if (!pickPOs)            
                message += countWarehouseOnly + " had warehouse inventory. ";            

            if (errorCount > 0)            
                message += errorCount + " Errors. ";
            
            if (ecommCount > 0)            
                message += ecommCount + " Ecomm Ringfences (excluded). ";
            
            if (noStoreCount > 0)            
                message += noStoreCount + " did not have store (excluded).  ";
            
            if (permissionCount > 0)            
                message += permissionCount + " you do not have permissions.  ";
            
            if (deliveredToday > 0)            
                message += deliveredToday + " created today, will NOT be honored if the PO is delivered today.  ";
            
            if (holdCount > 0)            
                message += holdCount + " on hold.  See ReleaseHeldRDQs to view held RDQs.  ";
            
            if (cancelholdcount > 0)            
                message += cancelholdcount + " rejected by cancel inventory hold.  ";
            
            return message;
        }

        public ActionResult MassPickRingFenceStore(string div, string store)
        {
            div = div.Trim();
            store = store.Trim();

            List<RingFence> rfList = db.RingFences.Where(rf => rf.Division == div && rf.Store == store && rf.CanPick).ToList();
            string message = BulkPickRingFence(div + "-" + store, rfList, true);

            return RedirectToAction("IndexByStore", new { message });
        }

        public ActionResult MassPickRingFenceStoreNoFuture(string div, string store)
        {
            div = div.Trim();
            store = store.Trim();

            List<RingFence> rfList = db.RingFences.Where(rf => rf.Division == div && rf.Store == store).ToList();
            string message = BulkPickRingFence(div + "-" + store, rfList, false);

            return RedirectToAction("IndexByStore", new { message });
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
                                      where a.Division == div && a.Store == store && d.Name == fob && d.Division == div
                                      select a).ToList();
            string message = BulkPickRingFence(div + "-" + store + " " + fob, rfList, true);

            return RedirectToAction("IndexByStore", new { message });
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
                                      where a.Division == div && a.Store == store && d.Name == fob && d.Division == div
                                      select a).ToList();
            string message = BulkPickRingFence(div + "-" + store + " " + fob, rfList, false);

            return RedirectToAction("IndexByStore", new { message });
        }

        public ActionResult MassEditRingFence(string sku)
        {
            MassEditRingFenceModel model = new MassEditRingFenceModel()
            {
                Sku = sku
            };
            
            return View(model);
        }

        [HttpPost]
        public ActionResult MassEditRingFence(MassEditRingFenceModel model)
        {
            List<RingFence> rfList = db.RingFences.Where(rf => rf.Sku == model.Sku).ToList();

            string message;
            message = SaveMassEditRingFence(model, rfList);

            return RedirectToAction("IndexSummary", new { message });
        }

        private string SaveMassEditRingFence(MassEditRingFenceModel model, List<RingFence> rfList)
        {
            string message;
            int count = 0;
            int ecommCount = 0;
            int permissionCount = 0;
            string errorMessage;

            foreach (RingFence rf in rfList)
            {
                if (!dao.CanUserUpdateRingFence(rf, currentUser, AppName, out errorMessage))
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
                    rf.CreatedBy = currentUser.NetworkID;
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
                db.SaveChanges(currentUser.NetworkID);
                Session["rfStore"] = null;
                Session["rfFOBStore"] = null;
            }

            message = string.Format("Updated {0} ringfences for ", count);

            if (model.Sku != null)
            {
                message += model.Sku + ". ";
            }
            else if (model.FOB != null)
            {
                message += string.Format("{0}-{1} {2}. ", model.Div, model.Store, model.FOB);            
            }
            else
            {
                message += string.Format("{0}-{1}. ", model.Div, model.Store);
            }

            if (ecommCount > 0)
            {
                message += ecommCount + " ecomm stores (excluded).  ";
            }
            if (permissionCount > 0)
            {
                message += permissionCount + " permission denied.  ";
            }
            return message;
        }

        public ActionResult MassEditRingFenceStore(string div, string store, string fob)
        {
            MassEditRingFenceModel model = new MassEditRingFenceModel()
            {
                Div = div,
                Store = store,
                FOB = fob
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult MassEditRingFenceStore(MassEditRingFenceModel model)
        {
            List<RingFence> rfList;

            if (!string.IsNullOrEmpty(model.FOB))
            {
                rfList = (from a in db.RingFences 
                          join b in db.ItemMasters on a.ItemID equals b.ID
                          join c in db.FOBDepts on b.Dept equals c.Department
                          join d in db.FOBs on c.FOBID equals d.ID
                          where a.Division == model.Div && a.Store == model.Store && d.Name == model.FOB && d.Division == model.Div
                          select a).ToList();
            }
            else
            {
                rfList = db.RingFences.Where(rf => rf.Division == model.Div && rf.Store == model.Store).ToList();
            }

            string message;
            message = SaveMassEditRingFence(model, rfList);
            return RedirectToAction("IndexByStore", new { message });
        }

        [GridAction]
        public ActionResult Ajax_GetPackDetails(long ringFenceID, string packName)
        {
            var packDetails = new List<ItemPackDetail>();

            // Get pack's item
            var item = db.RingFences.Single(rf => rf.ID == ringFenceID).Sku;
            var itemID = db.ItemMasters.Single(i => i.MerchantSku == item).ID;

            // Get pack by item/pack name
            var pack = db.ItemPacks.Include("Details").FirstOrDefault(p => p.ItemID == itemID && p.Name == packName);
            if (pack != null)            
                packDetails = pack.Details.OrderBy(p=> p.Size).ToList();
            
            return View(new GridModel(packDetails));
        }

        [GridAction]
        public ActionResult _SelectBatchEditing(long ringFenceID)
        {
            // Get Ring Fence data
            var details = GetRingFenceDetails(ringFenceID);
            RingFence rf = db.RingFences.Where(r => r.ID == ringFenceID).First();
            
            List<RingFenceDetail> stillAvailable = GetWarehouseAvailable(rf);

            stillAvailable.AddRange(dao.GetFuturePOs(rf));
            RingFenceDetail existing;
            foreach (RingFenceDetail det in stillAvailable)
            {
                var query = (from a in details
                             where a.Size == det.Size && 
                                   a.PO == det.PO && 
                                   a.Warehouse == det.Warehouse
                             select a);
                if (query.Count() > 0)
                {
                    existing = query.First();
                    det.Qty = existing.Qty;
                    det.RingFenceID = existing.RingFenceID;
                }
            }
            return View(new GridModel(stillAvailable));
        }

        [GridAction]
        public ActionResult _SelectWarehouses(long ringFenceID)
        {
            RingFence ringFence = db.RingFences.Where(rf => rf.ID == ringFenceID).FirstOrDefault();
            return View(new GridModel(GetWarehouseAvailable(ringFence)));
        }

        [GridAction]
        public ActionResult _SelectPOs(long ringFenceID)
        {
            RingFence ringFence = db.RingFences.Where(rf => rf.ID == ringFenceID).FirstOrDefault();
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

                    if (!rfDAO.CanUserUpdateRingFence(ringFence, currentUser, AppName, out errorMessage))
                    {                        
                        det.Message += errorMessage;
                    }

                    det.AvailableQty = (from a in available
                                        where a.PO == det.PO &&
                                              a.Warehouse == det.Warehouse &&
                                              a.Size == det.Size
                                        select a.AvailableQty).FirstOrDefault();

                    if (IsRingFenceDetailValid(det))
                    {
                        var detailRec = ringFence.ringFenceDetails.Where(rfd => rfd.RingFenceID == det.RingFenceID &&
                                                                                rfd.DCID == det.DCID &&
                                                                                rfd.Size == det.Size &&
                                                                                rfd.PO == det.PO).FirstOrDefault();
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
                                db.Entry(detailRec).State = EntityState.Modified;
                            }
                            else
                            {
                                db.Entry(det).State = EntityState.Added;
                            }

                            det.LastModifiedDate = DateTime.Now;
                            det.LastModifiedUser = currentUser.NetworkID;                            
                        }
                        else if (det.Qty == 0)
                        {
                            if (detailRec != null)
                            {
                                db.Entry(detailRec).State = EntityState.Deleted;
                            }
                        }

                        db.SaveChanges(currentUser.NetworkID);
                    }
                }
            }

            // Build up viewmodel to be returned, (add errors if necessary)
            List<RingFenceDetail> details = ringFence.ringFenceDetails.Where(rfd => rfd.ActiveInd == "1").ToList();

            List<RingFenceDetail> final = new List<RingFenceDetail>();

            foreach (RingFenceDetail det in available)
            {
                var updateRecord = updated.Where(u => u.DCID == det.DCID &&
                                                      u.Size == det.Size &&
                                                      u.PO == det.PO).FirstOrDefault();

                var detailRecord = details.Where(d => d.DCID == det.DCID &&
                                                      d.Size == det.Size &&
                                                      d.PO == det.PO).FirstOrDefault();
                if (updateRecord != null)
                {
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
                        rfDetail.Message = string.Format("Max Quantity for {0} is {1}", rfDetail.Warehouse, rfDetail.AvailableQty);
                    else
                        rfDetail.Message = string.Format("Max Quantity for PO {0} is {1}", rfDetail.PO, rfDetail.AvailableQty);

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
            RingFenceUploadModel model = new RingFenceUploadModel()
            {
                SKU = rf.Sku,
                Size = rfd.Size,
                PO = rfd.PO,
                Comments = rf.Comments,
                QtyString = Convert.ToString(rfd.Qty),
                Store = rf.Store,
                Warehouse = rfd.Warehouse,
                Division = rf.Division,
                EndDate = Convert.ToString(rf.EndDate)
            };

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

                ringFence = db.RingFences.Where(rf => rf.ID == ringFenceID).First();

                if (string.IsNullOrEmpty(PO))                
                    available = GetWarehouseAvailable(ringFence);                
                else                
                    available = GetFutureAvailable(ringFence);                                    

                bool ecommwarehouse = rfDAO.isEcommWarehouse(ringFence.Division, ringFence.Store);

                if (!ecommwarehouse)
                {
                    foreach (RingFenceDetail det in updated)
                    {
                        if (IsRingFenceDetailValid(det))
                        {
                            if (det.Qty > 0)
                            {
                                List<RingFenceDetail> ringFenceDetails = db.RingFenceDetails.Where(rfd => rfd.RingFenceID == det.RingFenceID &&
                                                                                                          rfd.DCID == det.DCID &&
                                                                                                          rfd.Size == det.Size &&
                                                                                                          rfd.PO == det.PO).ToList();

                                if (ringFenceDetails.Count() == 0)
                                {
                                    det.Message = "";
                                    det.ActiveInd = "1";

                                    if (string.IsNullOrEmpty(det.PO))
                                        det.ringFenceStatusCode = "4";
                                    else
                                        det.ringFenceStatusCode = "1";

                                    db.RingFenceDetails.Add(det);                               
                                }
                                else
                                {
                                    RingFenceDetail existingDet = ringFenceDetails.First();
                                    existingDet.Qty = det.Qty;
                                    existingDet.ActiveInd = "1";
                                    db.Entry(existingDet).State = EntityState.Modified;
                                }

                                if (!string.IsNullOrEmpty(det.PO))
                                {
                                    List<ExistingPO> poList = (new ExistingPODAO()).GetExistingPO(ringFence.Division, det.PO);

                                    foreach (ExistingPO po in poList)
                                    {
                                        if (po.ExpectedDeliveryDate < DateTime.Now)
                                        {
                                            det.Message = "This PO is expected for delivery today. If it does, this ringfence will NOT be enforced (it will be deleted).";
                                        }
                                    }
                                }

                                det.LastModifiedDate = DateTime.Now;
                                det.LastModifiedUser = currentUser.NetworkID;
                            }
                        }
                    }

                    db.SaveChanges(currentUser.NetworkID);

                    List<RingFenceDetail> details = db.RingFenceDetails.Where(rfd => rfd.RingFenceID == ringFenceID && rfd.ActiveInd == "1").ToList();

                    List<RingFenceDetail> final = new List<RingFenceDetail>();

                    foreach (RingFenceDetail det in available)
                    {
                        var updateRecord = updated.Where(u => u.DCID == det.DCID &&
                                                              u.Size == det.Size &&
                                                              u.PO == det.PO).FirstOrDefault();

                        var detailRecord = details.Where(d => d.DCID == det.DCID &&
                                                              d.Size == det.Size &&
                                                              d.PO == det.PO).FirstOrDefault();
                        if (updateRecord != null)
                        {
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
                            if (det.Qty > 0)
                            {
                                model = CreateUploadModelFromDetail(det, ringFence);

                                list.Add(model);
                            }
                        }

                        if (list.Count() > 0)
                        {
                            processEcommRingFences(list, errorlist);

                            List<string> errors = (from a in errorlist
                                          where !a.ErrorMessage.StartsWith("Warning")
                                          select a.ErrorMessage).ToList();

                            RingFenceDetail finishMsg;

                            if (errors.Count() == 0)
                            {
                                finishMsg = new RingFenceDetail()
                                {
                                    Qty = ringFence.Qty,
                                    Message = "This data has been processed."
                                };
                            }
                            else
                            {
                                finishMsg = new RingFenceDetail()
                                {
                                    Message = errors.ToString()
                                };
                            }
                            outputList.Add(finishMsg);
                        }

                        return View(new GridModel(outputList));
                    }
                    else
                    {
                        //Ecomm RingFence we need to create ecomm inventory
                        List<RingFenceDetail> futures = GetFutureAvailable(ringFence);
                        List<RingFenceDetail> warehouse = GetWarehouseAvailable(ringFence);

                        //ecomm all countries store                        
                        RingFenceDetail newDet = new RingFenceDetail();

                        bool addDetail;
                        RingFence rf = db.RingFences.Where(r => r.Sku == ringFence.Sku && r.Store == ringFence.Store).First();
                        
                        foreach (RingFenceDetail det in updated)
                        {
                            availableQty = 0;
                            try
                            {
                                availableQty += (from a in futures
                                                 where a.PO == det.PO && 
                                                       a.Size == det.Size && 
                                                       a.DCID == det.DCID
                                                 select a.AvailableQty).Sum();
                            }
                            catch { }

                            try
                            {
                                availableQty += (from a in warehouse
                                                 where a.Size == det.Size && 
                                                       a.DCID == det.DCID
                                                 select a.AvailableQty).Sum();
                            }
                            catch { }
                             
                            // don't want to have availability check for Europe Ecom
                            if (det.Qty > availableQty && det.Qty > 0 && ringFence.Division != "31")
                            {
                                message += string.Format("Max Qty for {0} {1} is {2}", det.Warehouse, det.PO, det.AvailableQty);
                                det.Message = message;
                            }
                            else
                            {
                                addDetail = false;
                                det.Size = det.Size.Trim();
                                newDet = db.RingFenceDetails.Where(rfd => rfd.Size == det.Size && 
                                                                          rfd.RingFenceID == rf.ID && 
                                                                          rfd.PO == det.PO &&
                                                                          rfd.ActiveInd == "1" &&
                                                                          rfd.DCID == det.DCID).FirstOrDefault();
                                if (newDet == null)
                                {
                                    addDetail = true;
                                    newDet = new RingFenceDetail()
                                    {
                                        Size = det.Size
                                    };
                                }

                                newDet.DCID = det.DCID;
                                newDet.ActiveInd = "1";
                                newDet.LastModifiedDate = DateTime.Now;
                                newDet.LastModifiedUser = currentUser.NetworkID;
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
                                db.SaveChanges(currentUser.NetworkID);
                            }
                        }
                        return View(new GridModel(updated));
                    }
                }
            }
            return View(new GridModel(updated));
        }

        public void processEcommRingFences(List<RingFenceUploadModel> ProcessList, List<RingFenceUploadModel> Errors)
        {
            List<EcommRingFence> EcommAllStoresList = new List<EcommRingFence>();

            foreach (RingFenceUploadModel model in ProcessList)
            {
                //Europe ecomm store that needs to be broken out
                EcommRingFence ecomm = new EcommRingFence()
                {
                    Sku = model.SKU,
                    Size = model.Size,
                    PO = model.PO,
                    Comments = model.Comments,
                    EndDate = model.EndDate
                };

                try
                {
                    ecomm.Qty = Convert.ToInt32(Math.Round(Convert.ToDecimal(model.QtyString)));
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
                dao.SaveEcommRingFences(EcommAllStoresList, currentUser.NetworkID);            
        }

        public ActionResult PickFOB(string div, string store)
        {
            StoreLookup model = db.StoreLookups.Where(s => s.Division == div && s.Store == store).First();
            return View(model);
        }

        public ActionResult Upload(string message)
        {
            ViewData["errorMessage"] = message;
            return View();
        }

        #region Spreadsheet exports
        [GridAction]
        public ActionResult ExportGrid(GridCommand settings)
        {
            RingFenceExport exportRF = new RingFenceExport(appConfig, dao);
            exportRF.WriteData(settings);
            exportRF.excelDocument.Save("RingFences.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return RedirectToAction("Index");
        }

        public ActionResult DestExport(string div, string store)
        {
            RingFenceByDestExport exportRF = new RingFenceByDestExport(appConfig);
            exportRF.WriteData(div, store);
            exportRF.excelDocument.Save("RingFenceWebUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);

            return View();
        }
        #endregion

        #region RingFence Delete mass load
        public ActionResult ExcelDeleteTemplate()
        {
            RingFenceDeleteSpreadsheet ringFenceDeleteSpreadsheet = new RingFenceDeleteSpreadsheet(appConfig, configService);
            Excel excelDocument;

            excelDocument = ringFenceDeleteSpreadsheet.GetTemplate();

            excelDocument.Save("RingFenceDeleteUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

        public ActionResult UploadDeletes()
        {
            return View();
        }

        public ActionResult DeleteRingFences(IEnumerable<HttpPostedFileBase> attachments)
        {
            RingFenceDeleteSpreadsheet ringFenceDeleteSpreadsheet = new RingFenceDeleteSpreadsheet(appConfig, configService);
            string msg;

            foreach (HttpPostedFileBase file in attachments)
            {
                ringFenceDeleteSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(ringFenceDeleteSpreadsheet.message))
                    return Content(ringFenceDeleteSpreadsheet.message);
                else
                {
                    if (ringFenceDeleteSpreadsheet.errorList.Count() > 0)
                    {
                        Session["errorList"] = ringFenceDeleteSpreadsheet.errorList;

                        msg = string.Format("{0} successfully uploaded, {1} Errors", ringFenceDeleteSpreadsheet.validRingFenceDeletes.Count.ToString(),
                            ringFenceDeleteSpreadsheet.errorList.Count.ToString());

                        return Content(msg);
                    }
                }
            }

            msg = string.Format("{0} successfully uploaded", ringFenceDeleteSpreadsheet.validRingFenceDeletes.Count.ToString());
            return Json(new { message = msg }, "application/json");
        }

        public ActionResult DownloadDeleteErrors()
        {
            List<RingFenceUploadModel> errors = (List<RingFenceUploadModel>)Session["errorList"];
            Excel excelDocument;
            RingFenceDeleteSpreadsheet ringFenceDeleteSpreadsheet = new RingFenceDeleteSpreadsheet(appConfig, configService);

            if (errors != null)
            {
                excelDocument = ringFenceDeleteSpreadsheet.GetErrors(errors); 
                excelDocument.Save("RingFenceDeleteErrors.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            }
            return View();
        }
        #endregion

        #region Ring Fence mass load
        public ActionResult ExcelTemplate()
        {
            RingFenceUploadSpreadsheet ringFenceUploadSpreadsheet = new RingFenceUploadSpreadsheet(appConfig, configService, dao, 
                                                                                                   new LegacyFutureInventoryDAO());
            Excel excelDocument;

            excelDocument = ringFenceUploadSpreadsheet.GetTemplate();

            excelDocument.Save("RingFenceUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

        public ActionResult SaveRingFencesWithAccumulatedQuantity(IEnumerable<HttpPostedFileBase> attachments)
        {
            return SaveRingFences(attachments, true);
        }

        public ActionResult SaveRingFencesReplacingQuantity(IEnumerable<HttpPostedFileBase> attachments2)
        {
            return SaveRingFences(attachments2, false);
        }

        public ActionResult SaveRingFences(IEnumerable<HttpPostedFileBase> attachments, bool accumulateQuantity)
        {
            RingFenceUploadSpreadsheet ringFenceUploadSpreadsheet = new RingFenceUploadSpreadsheet(appConfig, configService, dao, 
                                                                                                   new LegacyFutureInventoryDAO());
            string message;

            foreach (HttpPostedFileBase file in attachments)
            {
                ringFenceUploadSpreadsheet.Save(file, accumulateQuantity);

                if (!string.IsNullOrEmpty(ringFenceUploadSpreadsheet.message))
                    return Content(ringFenceUploadSpreadsheet.message);
                else
                {
                    if (ringFenceUploadSpreadsheet.errorList.Count() > 0)
                    {
                        Session["errorList"] = ringFenceUploadSpreadsheet.errorList;

                        message = string.Format("{0} lines were processed successfully. {1} warnings and {2} errors were found.", ringFenceUploadSpreadsheet.validRingFences.Count.ToString(),
                            ringFenceUploadSpreadsheet.warnings.Count.ToString(), ringFenceUploadSpreadsheet.errors.Count.ToString());

                        return Content(message);
                    }
                }
            }

            message = string.Format("{0} successfully uploaded", ringFenceUploadSpreadsheet.validRingFences.Count.ToString());
            return Json(new { message }, "application/json");
        }

        public ActionResult DownloadErrors()
        {
            List<RingFenceUploadModel> errors = (List<RingFenceUploadModel>)Session["errorList"];
            Excel excelDocument;
            RingFenceUploadSpreadsheet ringFenceUploadSpreadsheet = new RingFenceUploadSpreadsheet(appConfig, configService, dao, 
                                                                                                   new LegacyFutureInventoryDAO());

            if (errors != null)
            {
                excelDocument = ringFenceUploadSpreadsheet.GetErrors(errors);
                excelDocument.Save("RingFenceUploadErrors.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            }
            return View();
        }
        #endregion
    }
}
