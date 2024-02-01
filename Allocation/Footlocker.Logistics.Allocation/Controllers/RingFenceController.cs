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
using Footlocker.Logistics.Allocation.Spreadsheets;
using Aspose.Cells;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Factories;
using Telerik.Web.Mvc.Extensions;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Director of Allocation,Admin,Support")]
    public class RingFenceController : AppController
    {
        #region Fields

        DAO.AllocationContext db = new DAO.AllocationContext();
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
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

            list = dao.GetFuturePOs(ringFence);
            list.AddRange(dao.GetTransloadPOs(ringFence));
            return list;
        }

        private List<RingFenceDetail> GetWarehouseAvailable(RingFence ringFence)
        {
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);
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

            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);
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
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);
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
                names.Add(userID, GetFullUserNameFromDatabase(userID.Replace('\\', '/')));
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
                names.Add(userID, GetFullUserNameFromDatabase(userID.Replace('\\', '/')));
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
            ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);
            rf.ItemID = itemDAO.GetItemID(rf.Sku);

            rf.StartDate = configService.GetControlDate(rf.Division).AddDays(1);

            rf.CreateDate = DateTime.Now;
            rf.CreatedBy = currentUser.NetworkID;
            rf.LastModifiedDate = DateTime.Now;
            rf.LastModifiedUser = currentUser.NetworkID;

            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);
            if (dao.isEcommWarehouse(rf.Division, rf.Store))
                rf.Type = 2;
            else
                rf.Type = 1;

            rf.Qty = 0;

            rf.ringFenceDetails = new List<RingFenceDetail>();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RingFenceModel model)
        {
            model.Divisions = currentUser.GetUserDivisions(AppName);            
            string errorMessage;
            
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

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
                names.Add(userID, GetFullUserNameFromDatabase(userID.Replace('\\', '/')));
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

            List<RingFenceDetail> details = db.RingFenceDetails.Where(rfd => rfd.RingFenceID == ID && rfd.ActiveInd == "1").ToList();

            List<RingFenceDetail> binDetails = details.Where(rfd => rfd.Size.Length == _CASELOT_SIZE_INDICATOR_VALUE_LENGTH).ToList();
            
            RingFenceSizeSummary currentSize;

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
            
            List<RingFenceDetail> caselotDetails = details.Where(rfd => rfd.Size.Length > _CASELOT_SIZE_INDICATOR_VALUE_LENGTH).ToList();

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

                newDetail.Warehouse = dcs.Where(d => d.ID == newDetail.DCID).First().Name;

                if (includeDetail)                
                    model.Add(newDetail);                
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

            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);
            if (!dao.CanUserUpdateRingFence(model.RingFence, currentUser, AppName, out errorMessage))            
                return RedirectToAction("Index", new { message = errorMessage });                        

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RingFenceModel model)
        {
            string errorMessage;
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

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
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

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

        public ActionResult Release(string message)
        {
            RingFenceReleaseModel model = new RingFenceReleaseModel();

            InitializeDivisions(model);

            InitializeDepartments(model, false);
            ViewData["message"] = message;
            //model.SearchResult = false;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Release(RingFenceReleaseModel model)
        {
            ViewData["ruleSetID"] = model.RuleSetID;
            ViewData["ruleType"] = "ringFence";
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
                        Type = "ringFence",
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
            //model.SearchResult = false;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReleaseAll(RingFenceReleaseModel model)
        {
            string outputMessage;

            outputMessage = BulkPickRingFence(model.Division, model.Department, model.DistributionCenter, model.Store, model.RuleSetID,
                                              model.SKU, model.PO, Convert.ToInt32(model.RingFenceType), model.RingFenceStatus);

            if (!string.IsNullOrEmpty(outputMessage))
                ViewBag.message = string.Format("Filtered ring fence group(s) were picked with the exception of the following errors. \r\n\r\n{0}", outputMessage);
            else
                ViewBag.message = "Filtered ring fence group(s) were picked. ";

            InitializeDivisions(model);
            InitializeDepartments(model, false);
            return View("Release", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReleaseAllToWarehouse(RingFenceReleaseModel model)
        {
            string outputMessage;

            outputMessage = BulkDeleteRingFence(model.Division, model.Department, model.DistributionCenter, model.Store, model.RuleSetID,
                                                model.SKU, model.PO, Convert.ToInt32(model.RingFenceType), model.RingFenceStatus);

            if (!string.IsNullOrEmpty(outputMessage))
                ViewBag.message = string.Format("Ring fence group(s) were attempted to be removed with the exception of the following errors (No RDQs were created). \r\n\r\n{0}", outputMessage);
            else
                ViewBag.message = "Ring fence group(s) were removed. No RDQs were created. ";

            InitializeDivisions(model);
            InitializeDepartments(model, false);
            return View("Release", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RefreshDivisions(RingFenceReleaseModel model)
        {
            InitializeDivisions(model);
            InitializeDepartments(model, true);
            return View("Release", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RefreshDepartments(RingFenceReleaseModel model)
        {
            InitializeDivisions(model);
            InitializeDepartments(model, false);
            return View("Release", model);
        }

        private void InitializeDivisions(RingFenceReleaseModel model)
        {
            List<Footlocker.Common.Division> divs = currentUser.GetUserDivisions(AppName);

            var allInstances = (from a in db.Instances
                                join b in db.InstanceDivisions
                                on a.ID equals b.InstanceID
                                select new { instance = a, b.Division }).ToList();

            model.Instances = (from a in allInstances
                               join b in divs
                               on a.Division equals b.DivCode
                               select a.instance).Distinct().ToList();

            if (model.Instances.Any())
            {
                //if no selected instance, default to first one in the list
                if (model.Instance == 0)
                    model.Instance = model.Instances.First().ID;

                List<int> instanceDistributionCenters = db.InstanceDistributionCenters.Where(idc => idc.InstanceID == model.Instance).Select(idc => idc.DCID).ToList();
                model.DistributionCenterList = db.DistributionCenters.Where(dc => instanceDistributionCenters.Contains(dc.ID)).ToList();
                model.DistributionCenterList.Insert(0, new DistributionCenter() { ID = 0, MFCode = "XX", Name = "All Distribution Centers" });

                model.Divisions = (from a in allInstances
                                   join b in divs on a.Division equals b.DivCode
                                   where a.instance.ID == model.Instance
                                   select b).ToList();
            }
            else
            {
                model.Instances.Insert(0, new Instance() { ID = -1, Name = "No division permissions enabled" });
                model.DistributionCenterList = new List<DistributionCenter>();
            }                

            model.RingFenceTypeList = db.RingFenceTypes.OrderBy(rft => rft.ID).ToList();                                            
            model.RingFenceTypeList.Insert(0, new RingFenceType() { ID = 0, Description = "All" });

            model.RingFenceStatusList = db.RingFenceStatusCodes.OrderBy(rfs => rfs.ringFenceStatusCode).ToList();
            model.RingFenceStatusList.Insert(0, new RingFenceStatusCodes() { ringFenceStatusCode = "0", ringFenceStatusDesc = "All" });
        }

        private void InitializeDepartments(RingFenceReleaseModel model, bool resetDivision)
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

        //public ActionResult BulkAdmin(string message)
        //{
        //    RingFenceReleaseModel model = new RingFenceReleaseModel();

        //    InitializeDivisions(model);
        //    InitializeDepartments(model, false);
        //    ViewData["message"] = message;
        //    ViewData["ruleSetID"] = model.RuleSetID;
        //    ViewData["ruleType"] = "ringFence";
        //    //model.SearchResult = false;

        //    return View(model);
        //}

        //[HttpPost]
        //public ActionResult BulkAdmin(RingFenceReleaseModel model)
        //{
        //    ViewData["ruleSetID"] = model.RuleSetID;
        //    ViewData["ruleType"] = "ringFence";
        //    InitializeDivisions(model);
        //    InitializeDepartments(model, false);
        //    model.HaveResults = true;

        //    if (model.ShowStoreSelector == "yes")
        //    {
        //        if (model.RuleSetID < 1)
        //        {
        //            //get a new ruleset
        //            RuleSet rs = new RuleSet
        //            {
        //                Type = "ringFence",
        //                CreateDate = DateTime.Now,
        //                CreatedBy = currentUser.NetworkID
        //            };

        //            db.RuleSets.Add(rs);
        //            db.SaveChanges();

        //            model.RuleSetID = rs.RuleSetID;
        //        }

        //        ViewData["ruleSetID"] = model.RuleSetID;
        //        return View(model);
        //    }
        //    //model.SearchResult = false;

        //    return View(model);
        //}

        [HttpPost]
        public ActionResult ReleasePOGroupRF(GroupedPORingFence rfGroup)
        {
            string outputMessage;

            if (string.IsNullOrEmpty(rfGroup.PO))
                rfGroup.PO = "";

            RingFencePickModel pickData = new RingFencePickModel()
            {
                RingFence = db.RingFences.Where(rf => rf.ID == rfGroup.ID).FirstOrDefault(), 
                Details = db.RingFenceDetails.Where(rfd => rfd.RingFenceID == rfGroup.ID && rfd.PO == rfGroup.PO && rfd.DCID == rfGroup.DCID && rfd.ringFenceStatusCode == rfGroup.RingFenceStatusCode).ToList()
            };

            outputMessage = PickRingFence(pickData);

            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success, outputMessage) };
        }

        [HttpPost]
        public ActionResult DeletePOGroupRF(GroupedPORingFence rfGroup)
        {
            string errorMessage;
            string outputMessage;

            if (string.IsNullOrEmpty(rfGroup.PO))
                rfGroup.PO = "";

            RingFencePickModel pickData = new RingFencePickModel()
            {
                RingFence = db.RingFences.Where(rf => rf.ID == rfGroup.ID).FirstOrDefault(),
                Details = db.RingFenceDetails.Where(rfd => rfd.RingFenceID == rfGroup.ID && rfd.PO == rfGroup.PO && rfd.DCID == rfGroup.DCID && rfd.ringFenceStatusCode == rfGroup.RingFenceStatusCode).ToList()
            };            

            errorMessage = DeleteRingFence(pickData);

            if (!string.IsNullOrEmpty(errorMessage))
                outputMessage = string.Format("This ring fence group was attempted to be removed but had the following error. No RDQs were created. \r\n\r\n{0}", errorMessage);
            else
                outputMessage = "This ring fence group was removed. No RDQs were created. ";

            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success, outputMessage) };
        }

        [GridAction]
        public ActionResult _BulkRingFences(string div, string department, int dcid, string sku, int ringFenceType, string ringFenceStatus, string po, string store, long ruleset)
        {
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);
            List<GroupedPORingFence> ringFenceList = dao.GetPORingFenceGroups(div, department, dcid, store, ruleset, sku, po, ringFenceType, ringFenceStatus);

            //var rdqGroups = from rdq in rdqList
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
            //                    InstanceID = instanceID,
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

            //List<RDQGroup> rdqGroupList = rdqGroups.OrderBy(g => g.Division)
            //        .ThenBy(g => g.Store)
            //        .ThenBy(g => g.WarehouseName)
            //        .ThenBy(g => g.Category)
            //        .ThenBy(g => g.Sku)
            //        .ThenBy(g => g.Status)
            //        .ToList();

            return View(new GridModel(ringFenceList));
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
            string errorMessage;
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

            foreach (RingFence rf in ringfences)
            {
                if (!dao.CanUserUpdateRingFence(rf, currentUser, AppName, out errorMessage))
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
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

            RingFencePickModel model = new RingFencePickModel()
            {
                RingFence = rf,
                Details = dao.GetRingFenceDetails(rf.ID),
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

            DateTime controlDate = configService.GetControlDate(rf.Division);

            //startdate is the next control date, so make sure no details have POs if they do, warn them.
            if (rf.RingFence.StartDate > controlDate)
                optionalPick = true;            

            if (string.IsNullOrEmpty(rf.Message))
            {
                //create RDQs
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

                    ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);
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
                    rf.Message = string.Format("{0} on hold. Please see Release Held RDQs for held RDQs.", holdCount);                

                int cancelHoldCount = CheckCancelHolds(rdqsToCheck);

                if (cancelHoldCount > 0)                
                    rf.Message = string.Format("{0} rejected because of cancel inventory hold.", cancelHoldCount);                

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
            DateTime controlDate;
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

            var rf = db.RingFences.Where(r => r.ID == ID).FirstOrDefault();
            if (rf == null)            
                return RedirectToAction("Index", new { message = "Ring Fence no longer exists. Please verify." });            

            if (!dao.CanUserUpdateRingFence(rf, currentUser, AppName, out errorMessage))            
                return RedirectToAction("Index", new { message = errorMessage });

            //this is an ecomm ringfence, you can't pick it
            if (rf.Type == 2)                            
                return RedirectToAction("Index", new { message = "Sorry, you cannot pick for an Ecomm warehouse. Do you mean Delete?" });

            if (string.IsNullOrEmpty(rf.Store))
            {
                //FLOW:  Pick a division/store
                //show rdq's for everything on there, with a qty that they can input
                //they click save, it removes that amount
                //when all details have no qty, then delete the rdq
                return RedirectToAction("SelectStorePick", rf);
            }

            controlDate = configService.GetControlDate(rf.Division);

            //startdate is the next control date, so make sure no details have POs if they do, warn them.
            if (rf.StartDate > controlDate)                             
                optionalPick = true;
            
            List<RingFenceDetail> details = db.RingFenceDetails.Where(rfd => rfd.RingFenceID == ID &&
                                                                             rfd.ActiveInd == "1").ToList();

            ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);
            List<RDQ> rdqsToCheck = new List<RDQ>();
            RingFenceDataFactory rfDataFactory = new RingFenceDataFactory();
            foreach (RingFenceDetail det in details)
            {
                RDQ rdq = RDQFactory.CreateFromRingFence(rf, det, currentUser);

                if (!string.IsNullOrEmpty(rdq.PO) && optionalPick)
                { 
                    rdq.Type = "user_opt";
                    message = "Since this was created today, it will NOT be honored if the PO is delivered today.";
                }

                rdqsToCheck.Add(rdq);

                db.RingFenceDetails.Remove(det);

                RingFenceHistory history = rfDataFactory.CreateRingFenceHistory(rf, det, currentUser);
                history.Action = "Picked Det";    
                    
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
            List<RingFence> rfList = db.RingFences.Where(rf => rf.Sku == sku).ToList();
            List<RingFence> rfPickList = new List<RingFence>();

            foreach (RingFence rf in rfList)
            {
                if (rf.CanPick)
                    rfPickList.Add(rf);
            }

            string message = BulkPickRingFence(sku, rfPickList, true);

            return RedirectToAction("IndexSummary", new { message });
        }

        public ActionResult MassPickRingFenceNoFuture(string sku)
        {
            List<RingFence> rfList = db.RingFences.Where(rf => rf.Sku == sku).ToList();
            string message = BulkPickRingFence(sku, rfList, false);

            return RedirectToAction("IndexSummary", new { message });
        }

        private string BulkPickRingFence(string division, string department, int distributionCenterID, string store, long ruleSetID,
            string sku, string po, int ringFenceType, string ringFenceStatus)
        {
            string message = string.Empty;
            string errorMessage;
            int permissionsErrors = 0;
            int noStoreCount = 0;
            int ecommCount = 0;
            int holdCount = 0;
            int cancelholdcount = 0;
            List<RDQ> rdqList;
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

            List<GroupedPORingFence> ringFenceList = dao.GetPORingFenceGroups(division, department, distributionCenterID, store, ruleSetID, sku, po, ringFenceType, ringFenceStatus);

            foreach (GroupedPORingFence grf in ringFenceList)
            {
                RingFence rf = db.RingFences.Where(r => r.ID == grf.ID).FirstOrDefault();

                if (!dao.CanUserUpdateRingFence(rf, currentUser, AppName, out errorMessage))
                    permissionsErrors++;

                if (string.IsNullOrEmpty(grf.Store))
                    noStoreCount++;

                if (grf.RingFenceTypeCode == 2)
                    ecommCount++;
            }

            if (ecommCount > 0)
                message += string.Format("There were {0} Ecomm Ringfences excluded. ", ecommCount.ToString());

            if (noStoreCount > 0)
                message += noStoreCount + " did not have store (excluded). ";

            if (permissionsErrors > 0)
                return string.Format("You do not have permissions to change ring fences for {0} ring fence(s). Please filter the ring fences to match your permissions.", 
                    permissionsErrors.ToString());
            else
            {
                rdqList = dao.BulkPickRingFences(division, department, distributionCenterID, store, ruleSetID, sku, po, ringFenceType, ringFenceStatus, currentUser);
                
                if (rdqList.Count() > 0)
                {
                    int instance = configService.GetInstance(division);
                    RDQDAO rdqDAO = new RDQDAO();
                    holdCount = rdqDAO.ApplyHolds(rdqList, instance);
                    cancelholdcount = rdqDAO.ApplyCancelHolds(rdqList);

                    if (holdCount > 0)
                        message += holdCount + " on hold. See ReleaseHeldRDQs to view held RDQs. ";

                    if (cancelholdcount > 0)
                        message += cancelholdcount + " rejected by cancel inventory hold. ";
                }
            }

            return message;
        }

        /// <summary>
        /// This will pick the list of rf headers and details provided
        /// </summary>
        /// <param name="rfList"></param>
        private string PickRingFence(RingFencePickModel rfModel)
        {
            int noStoreCount = 0;
            int ecommCount = 0;
            int count = 0;
            int errorCount = 0;
            int deliveredToday = 0;
            bool optionalPick = false;
            List<RDQ> rdqs = new List<RDQ>();
            bool canDelete = true;
            DateTime controlDate;
            ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);
            RingFenceDataFactory rfDataFactory = new RingFenceDataFactory();
            RingFenceHistory history;
            string errorMessage;
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

            if (!dao.CanUserUpdateRingFence(rfModel.RingFence, currentUser, AppName, out errorMessage))
                return "You do not have permissions to pick the selected ring fence.";
                    
            if (rfModel.RingFence.Type == 2)
            {
                //this is an ecomm ringfence, skip it
                ecommCount++;
            }
            else if (string.IsNullOrEmpty(rfModel.RingFence.Store))
                noStoreCount++;
            else
            {
                //FLOW:  Pick a division/store
                //show rdq's for everything on there, with a qty that they can input
                //they click save, it removes that amount
                //when all details have no qty, then delete the rdq

                controlDate = configService.GetControlDate(rfModel.RingFence.Division);
                rfModel.RingFence.ItemID = itemDAO.GetItemID(rfModel.RingFence.Sku);

                if (rfModel.RingFence.StartDate > controlDate)
                {
                    //startdate is the next control date, so make sure no details have POs if they do, warn them.
                    optionalPick = true;
                }
                try
                {
                    int detailCount = db.RingFenceDetails.Where(rfd => rfd.RingFenceID == rfModel.RingFence.ID).Count();
                    canDelete = detailCount == rfModel.Details.Count();

                    foreach (RingFenceDetail det in rfModel.Details)
                    {
                        RDQ rdq = RDQFactory.CreateFromRingFence(rfModel.RingFence, det, currentUser);

                        if (!string.IsNullOrEmpty(rdq.PO) && optionalPick)
                        {
                            rdq.Type = "user_opt";
                            deliveredToday++;
                        }

                        db.RDQs.Add(rdq);
                        rdqs.Add(rdq);

                        db.RingFenceDetails.Remove(det);

                        history = rfDataFactory.CreateRingFenceHistory(rfModel.RingFence, det, currentUser);
                        history.Action = "Picked Det";

                        db.RingFenceHistory.Add(history);
                    }

                    if (canDelete)
                        db.RingFences.Remove(rfModel.RingFence);

                    db.SaveChanges(currentUser.NetworkID);

                    count++;
                }
                catch
                {
                    errorCount++;
                }
            }            

            int holdCount = 0;
            int cancelholdcount = 0;
            if (rdqs.Count() > 0)
            {
                string div = rfModel.RingFence.Division;
                int instance = configService.GetInstance(div);
                RDQDAO rdqDAO = new RDQDAO();
                holdCount = rdqDAO.ApplyHolds(rdqs, instance);
                cancelholdcount = rdqDAO.ApplyCancelHolds(rdqs);
            }

            string message = string.Empty;

            if (errorCount > 0)
                message += string.Format("There were {0} Errors. ", errorCount.ToString());

            if (ecommCount > 0)
                message += string.Format("There were {0} Ecomm Ringfences excluded. ", ecommCount.ToString());

            if (noStoreCount > 0)
                message += noStoreCount + " did not have store (excluded). ";

            if (deliveredToday > 0)
                message += deliveredToday + " created today, will NOT be honored if the PO is delivered today. ";

            if (holdCount > 0)
                message += holdCount + " on hold.  See ReleaseHeldRDQs to view held RDQs. ";

            if (cancelholdcount > 0)
                message += cancelholdcount + " rejected by cancel inventory hold. ";

            return message;
        }

        /// <summary>
        /// This will delete a bunch of ring fences
        /// </summary>
        /// <param name="rfList"></param>
        private string BulkDeleteRingFence(string division, string department, int distributionCenterID, string store, long ruleSetID,
            string sku, string po, int ringFenceType, string ringFenceStatus)
        {
            int permissionCount = 0;
            string errorMessage;
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

            List<GroupedPORingFence> ringFenceList = dao.GetPORingFenceGroups(division, department, distributionCenterID, store, ruleSetID, sku, po, ringFenceType, ringFenceStatus);

            foreach (GroupedPORingFence grf in ringFenceList)
            {
                RingFence rf = db.RingFences.Where(r => r.ID == grf.ID).FirstOrDefault();

                if (!dao.CanUserUpdateRingFence(rf, currentUser, AppName, out errorMessage))
                    permissionCount++;
            }

            if (permissionCount > 0)
                return string.Format("You do not have permissions to change ring fences for {0} ring fence(s). Please filter the ring fences to match your permissions.",
                    permissionCount.ToString());
            else
                dao.BulkDeleteRingFences(division, department, distributionCenterID, store, ruleSetID, sku, po, ringFenceType, ringFenceStatus, currentUser);
                            
            return string.Empty;
        }

        private string DeleteRingFence(RingFencePickModel rf)
        {            
            bool canDelete = true;
            RingFenceDataFactory rfDataFactory = new RingFenceDataFactory();
            RingFenceHistory history;
            string errorMessage;
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

            if (!dao.CanUserUpdateRingFence(rf.RingFence, currentUser, AppName, out errorMessage))
                return "You do not have permission to delete this ring fence";
            else
            {
                try
                {
                    int detailCount = db.RingFenceDetails.Where(rfd => rfd.RingFenceID == rf.RingFence.ID).Count();
                    canDelete = detailCount == rf.Details.Count();

                    foreach (RingFenceDetail det in rf.Details)
                    {
                        db.RingFenceDetails.Remove(det);                        

                        history = rfDataFactory.CreateRingFenceHistory(rf.RingFence, det, currentUser);
                        history.Action = "Delete Det";

                        db.RingFenceHistory.Add(history);
                    }

                    if (canDelete)
                        db.RingFences.Remove(rf.RingFence);                        

                    db.SaveChanges(currentUser.NetworkID);                    
                }
                catch
                {
                    return "There was an error processing this delete";
                }
            }            

            return string.Empty;
        }

        /// <summary>
        /// For the list of ring fence headers, it will pick all of the detail records under it, depending on how the pickPOs parameter is set
        /// </summary>
        /// <param name="key">This is only used in the display message, not in logic</param>
        /// <param name="rfList"></param>
        /// <param name="pickPOs">If you set this to false, it will remove ring fence detail recs with POs from the pick list</param>
        /// <returns>A big long error message</returns>
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
            DateTime controlDate;
            ItemDAO itemDAO = new ItemDAO(appConfig.EuropeDivisions);
            RingFenceDataFactory rfDataFactory = new RingFenceDataFactory();
            RingFenceHistory history;
            string errorMessage;
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

            foreach (RingFence rf in rfList)
            {
                if (!dao.CanUserUpdateRingFence(rf, currentUser, AppName, out errorMessage))
                    permissionCount++;                
                else if (rf.Type == 2)
                {
                    //this is an ecomm ringfence, skip it
                    ecommCount++;
                }
                else if (string.IsNullOrEmpty(rf.Store))                
                    noStoreCount++;                
                else
                {
                    //FLOW:  Pick a division/store
                    //show rdq's for everything on there, with a qty that they can input
                    //they click save, it removes that amount
                    //when all details have no qty, then delete the rdq

                    controlDate = configService.GetControlDate(rf.Division);

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

                        rf.ItemID = itemDAO.GetItemID(rf.Sku);
                        foreach (RingFenceDetail det in details)
                        {
                            RDQ rdq = RDQFactory.CreateFromRingFence(rf, det, currentUser);
                            
                            if (!string.IsNullOrEmpty(rdq.PO) && optionalPick)
                            {
                                rdq.Type = "user_opt";
                                deliveredToday++;
                            }

                            db.RDQs.Add(rdq);
                            rdqs.Add(rdq);

                            db.RingFenceDetails.Remove(det);

                            history = rfDataFactory.CreateRingFenceHistory(rf, det, currentUser);
                            history.Action = "Picked Det";

                            db.RingFenceHistory.Add(history);
                        }

                        if (canDelete)                        
                            db.RingFences.Remove(rf);
                        
                        db.SaveChanges(currentUser.NetworkID);
                        count++;
                    }
                    catch
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

            List<RingFence> rfList = db.RingFences.Where(rf => rf.Division == div && rf.Store == store).ToList();
            List<RingFence> rfPickList = new List<RingFence>();

            foreach (RingFence rf in rfList)
            {
                if (rf.CanPick)
                    rfPickList.Add(rf);
            }

            string message = BulkPickRingFence(div + "-" + store, rfPickList, true);

            return RedirectToAction("IndexByStore", new { message });
        }

        public ActionResult MassPickRingFenceStoreNoFuture(string div, string store)
        {
            div = div.Trim();
            store = store.Trim();

            List<RingFence> rfList = db.RingFences.Where(rf => rf.Division == div && rf.Store == store).ToList();
            string message = BulkPickRingFence(string.Format("{0}-{1}", div, store), rfList, false);

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
            string message = BulkPickRingFence(string.Format("{0}-{1} {2}", div, store, fob), rfList, true);

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
            string message = BulkPickRingFence(string.Format("{0}-{1} {2}", div, store, fob), rfList, false);

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
        [ValidateAntiForgeryToken]
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
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

            foreach (RingFence rf in rfList)
            {
                if (!dao.CanUserUpdateRingFence(rf, currentUser, AppName, out errorMessage))                
                    permissionCount++;                
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
                        rf.EndDate = model.EndDate;
                    
                    if ((model.Comment != null)&&(model.Comment != ""))                    
                        rf.Comments = model.Comment;
                    
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
                message += model.Sku + ". ";            
            else if (model.FOB != null)            
                message += string.Format("{0}-{1} {2}. ", model.Div, model.Store, model.FOB);            
            else            
                message += string.Format("{0}-{1}. ", model.Div, model.Store);            

            if (ecommCount > 0)            
                message += ecommCount + " ecomm stores (excluded).  ";
            
            if (permissionCount > 0)            
                message += permissionCount + " permission denied.  ";
            
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
        [ValidateAntiForgeryToken]
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
                rfList = db.RingFences.Where(rf => rf.Division == model.Div && rf.Store == model.Store).ToList();            

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
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);
            // Get Ring Fence data
            List<RingFenceDetail> details = dao.GetRingFenceDetails(ringFenceID);
            RingFence rf = db.RingFences.Where(r => r.ID == ringFenceID).First();
            
            List<RingFenceDetail> stillAvailable = GetWarehouseAvailable(rf);

            stillAvailable.AddRange(dao.GetFuturePOs(rf));            
            foreach (RingFenceDetail det in stillAvailable)
            {
                RingFenceDetail detailRec = details.Where(rfd => rfd.Size == det.Size &&
                                                                 rfd.PO == det.PO &&
                                                                 rfd.Warehouse == det.Warehouse).FirstOrDefault();
                if (detailRec != null)
                {                    
                    det.Qty = detailRec.Qty;
                    det.RingFenceID = detailRec.RingFenceID;
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
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _SaveBatchEditing([Bind(Prefix = "updated")]IEnumerable<RingFenceDetail> updated)
        {
            string errorMessage;
            RingFenceDAO rfDAO = new RingFenceDAO(appConfig.EuropeDivisions);

            long ringFenceID = updated.First().RingFenceID;
            RingFence ringFence = null;
            List<RingFenceDetail> available = null;

            ringFence = db.RingFences.Where(rf => rf.ID == ringFenceID).First();

            available = GetWarehouseAvailable(ringFence);
            available.AddRange(GetFutureAvailable(ringFence));
            
            if (updated != null)
            {                
                foreach (RingFenceDetail det in updated)
                {
                    det.Message = "";

                    if (!rfDAO.CanUserUpdateRingFence(ringFence, currentUser, AppName, out errorMessage))                                            
                        det.Message += errorMessage;                    

                    det.AvailableQty = available.Where(a => a.PO == det.PO &&
                                                            a.Warehouse == det.Warehouse &&
                                                            a.Size == det.Size)
                                                .Select(a => a.AvailableQty)
                                                .FirstOrDefault();

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
                                db.Entry(det).State = EntityState.Added;                            

                            det.LastModifiedDate = DateTime.Now;
                            det.LastModifiedUser = currentUser.NetworkID;                            
                        }
                        else if (det.Qty == 0)
                        {
                            if (detailRec != null)                            
                                db.Entry(detailRec).State = EntityState.Deleted;                            
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
            RingFenceDAO rfDAO = new RingFenceDAO(appConfig.EuropeDivisions);
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
                                    List<ExistingPO> poList = (new ExistingPODAO(appConfig.EuropeDivisions)).GetExistingPO(ringFence.Division, det.PO);

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
                            
                        //RingFence rf = db.RingFences.Where(r => r.Sku == ringFence.Sku && r.Store == ringFence.Store).First();
                        
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
                                                                          rfd.RingFenceID == ringFence.ID && 
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

                                newDet.RingFenceID = ringFence.ID;
                                if (addDetail)
                                    db.RingFenceDetails.Add(newDet);

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
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

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
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);
            RingFenceExport exportRF = new RingFenceExport(appConfig, dao);
            exportRF.WriteData(settings);
            exportRF.excelDocument.Save(System.Web.HttpContext.Current.Response, "RingFences.xlsx", ContentDisposition.Attachment, exportRF.SaveOptions);
            return RedirectToAction("Index");
        }

        public ActionResult DestExport(string div, string store)
        {
            RingFenceByDestExport exportRF = new RingFenceByDestExport(appConfig);
            exportRF.WriteData(div, store);
            exportRF.excelDocument.Save(System.Web.HttpContext.Current.Response, "RingFenceWebUpload.xlsx", ContentDisposition.Attachment, exportRF.SaveOptions);

            return View();
        }
        #endregion

        #region RingFence Delete mass load
        public ActionResult ExcelDeleteTemplate()
        {
            RingFenceDeleteSpreadsheet ringFenceDeleteSpreadsheet = new RingFenceDeleteSpreadsheet(appConfig, configService);
            Workbook excelDocument;

            excelDocument = ringFenceDeleteSpreadsheet.GetTemplate();

            excelDocument.Save(System.Web.HttpContext.Current.Response, "RingFenceDeleteUpload.xlsx", ContentDisposition.Attachment, ringFenceDeleteSpreadsheet.SaveOptions);
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
            Workbook excelDocument;
            RingFenceDeleteSpreadsheet ringFenceDeleteSpreadsheet = new RingFenceDeleteSpreadsheet(appConfig, configService);

            if (errors != null)
            {
                excelDocument = ringFenceDeleteSpreadsheet.GetErrors(errors); 
                excelDocument.Save(System.Web.HttpContext.Current.Response, "RingFenceDeleteErrors.xlsx", ContentDisposition.Attachment, ringFenceDeleteSpreadsheet.SaveOptions);
            }
            return View();
        }
        #endregion

        #region Ring Fence mass load
        public ActionResult ExcelTemplate()
        {
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

            RingFenceUploadSpreadsheet ringFenceUploadSpreadsheet = new RingFenceUploadSpreadsheet(appConfig, configService, dao, 
                                                                                                   new LegacyFutureInventoryDAO());
            Workbook excelDocument;

            excelDocument = ringFenceUploadSpreadsheet.GetTemplate();

            excelDocument.Save(System.Web.HttpContext.Current.Response, "RingFenceUpload.xlsx", ContentDisposition.Attachment, ringFenceUploadSpreadsheet.SaveOptions);
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
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

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

                        message = string.Format("{0} lines were processed successfully. {1} warnings and {2} errors were found.", ringFenceUploadSpreadsheet.successfulCount.ToString(),
                            ringFenceUploadSpreadsheet.warnings.Count.ToString(), ringFenceUploadSpreadsheet.errors.Count.ToString());

                        return Content(message);
                    }
                }
            }

            message = string.Format("{0} successfully uploaded", ringFenceUploadSpreadsheet.successfulCount.ToString());
            return Json(new { message }, "application/json");
        }

        public ActionResult DownloadErrors()
        {
            List<RingFenceUploadModel> errors = (List<RingFenceUploadModel>)Session["errorList"];
            Workbook excelDocument;
            RingFenceDAO dao = new RingFenceDAO(appConfig.EuropeDivisions);

            RingFenceUploadSpreadsheet ringFenceUploadSpreadsheet = new RingFenceUploadSpreadsheet(appConfig, configService, dao, 
                                                                                                   new LegacyFutureInventoryDAO());

            if (errors != null)
            {
                excelDocument = ringFenceUploadSpreadsheet.GetErrors(errors);
                excelDocument.Save(System.Web.HttpContext.Current.Response, "RingFenceUploadErrors.xlsx", ContentDisposition.Attachment, ringFenceUploadSpreadsheet.SaveOptions);
            }
            return View();
        }
        #endregion
    }
}
