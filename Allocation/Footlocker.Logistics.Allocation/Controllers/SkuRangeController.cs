using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using System.Linq.Expressions;
using System.Reflection;
using Footlocker.Logistics.Allocation.Services;
using Telerik.Web.Mvc;
using System.IO;
using Aspose.Excel;
using Footlocker.Common;
using System.Text;
using System.Text.RegularExpressions;
using Footlocker.Logistics.Allocation.Common;
using System.Data.Entity.Infrastructure;
using System.Web.UI;
using System.Runtime;

namespace Footlocker.Logistics.Allocation.Controllers
{

    [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Director of Allocation,Admin,Support,Ecomm PreSale")]
    public class SkuRangeController : AppController
    {
        public const int _DEFAULT_MAX_LEADTIME = 5;
        //
        // GET: /SkuRange/
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
        readonly ConfigService configService = new ConfigService();
        readonly RangePlanDAO rangePlanDAO = new RangePlanDAO();
        readonly ItemDAO itemDAO = new ItemDAO();

        #region ActionResults

        #region "Sku Range Plan (list of skus)

        public ActionResult Index(string message)
        {
            ViewData["message"] = message;
            List<RangePlan> model = GetRangesForUser();

            if (model.Count > 0)
            {
                List<string> uniqueNames = (from l in model
                                            select l.UpdatedBy).Distinct().ToList();
                Dictionary<string, string> fullNamePairs = new Dictionary<string, string>();

                List<ApplicationUser> allUserNames = GetAllUserNamesFromDatabase();

                foreach (var item in uniqueNames)
                {
                    if (!item.Contains(" ") && !string.IsNullOrEmpty(item))
                    {
                        string userLookup = item.Replace('\\', '/');
                        userLookup = userLookup.Replace("CORP/", "");

                        if (userLookup.Substring(0, 1) == "u")                        
                            fullNamePairs.Add(item, allUserNames.Where(aun => aun.UserName == userLookup).Select(aun => aun.FullName).FirstOrDefault());       
                        else
                            fullNamePairs.Add(item, item);
                    }
                    else
                        fullNamePairs.Add(item, item);
                }

                foreach (var item in fullNamePairs)
                {
                    model.Where(x => x.UpdatedBy == item.Key).ToList().ForEach(y => y.UpdatedBy = item.Value);
                }
            }

            return View(model);
        }

        public ActionResult Refresh()
        {
            return RedirectToAction("Index");
        }

        private List<RangePlan> GetRangesForUser()
        {
            List<string> userDivDepts = currentUser.GetUserDivDept(AppName);
            List<string> divs = currentUser.GetUserDivList(AppName);

            var query = (from rp in db.RangePlans
                         join im in db.ItemMasters on rp.ItemID equals im.ID
                         join di in divs on im.Div equals di
                         select new { RangePlan = rp, Division = im.Div, Department = im.Dept }).ToList();

            List<RangePlan> model = query.Where(q => userDivDepts.Contains(q.Division + "-" + q.Department))
                                          .Select(q => q.RangePlan)
                                          .OrderBy(q => q.Sku)
                                          .ToList();

            return model;
        }

        public ActionResult Manage()
        {
            List<string> userDivDepts = currentUser.GetUserDivDept(AppName);

            List<RangePlan> model = db.RangePlans.Include("ItemMaster").Where(u => userDivDepts.Contains(u.Sku.Substring(0, 5))).ToList();

            return View(model);
        }

        public ActionResult Delete(long planID)
        {
            var rangePlanExists = db.RangePlans.Any(rp => rp.Id == planID);
            if (!rangePlanExists)            
                return RedirectToAction("Index", new { message = "Range no longer exists." });           

            rangePlanDAO.DeleteRangePlan(planID);
            return RedirectToAction("Index");
        }

        public ActionResult DeleteConfirm(long planID)
        {
            RangePlan range = db.RangePlans.Where(rp => rp.Id == planID).FirstOrDefault();
            
            if (range == null)
                return RedirectToAction("Index", new { message = "Range no longer exists." });

            range.CreatedBy = getFullUserNameFromDatabase(range.CreatedBy.Replace('\\', '/'));
            range.UpdatedBy = getFullUserNameFromDatabase(range.UpdatedBy.Replace('\\', '/'));

            return View(range);
        }

        public ActionResult CreateRangePlan()
        {
            RangePlanModel p = new RangePlanModel()
            {
                Range = new RangePlan(),
                OPRequest = new OrderPlanningRequest()
            };            
           
            return View(p);
        }

        [HttpPost]
        public ActionResult CreateRangePlan(RangePlanModel p)
        {
            p.Range.CreatedBy = currentUser.NetworkID;
            p.Range.CreateDate = DateTime.Now;
            p.Range.UpdatedBy = currentUser.NetworkID;
            p.Range.UpdateDate = DateTime.Now;

            string skuErrors = ValidateSKU(p.Range.Sku);

            if (!string.IsNullOrEmpty(skuErrors))
                ModelState.AddModelError("Sku", skuErrors);

            if (!ModelState.IsValid)
                return View(p);

            try
            {
                p.Range.ItemID = RetreiveOrCreateItemID(p.Range.Sku);
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                return View(p);
            }

            db.RangePlans.Add(p.Range);
            db.SaveChanges();
            //update ActiveARStatus since we added a new rangeplan
            
            itemDAO.UpdateActiveARStatus();

            if (p.OPRequest.StartSend.HasValue)
            {
                p.OPRequest.PlanID = p.Range.Id;
                db.OrderPlanningRequests.Add(p.OPRequest);
                db.SaveChanges();
            }

            return RedirectToAction("EditStores", new { planID = p.Range.Id });
        }

        private string ValidateSKU(string SKU)
        {
            Regex regexSku = new Regex(@"^\d{2}-\d{2}-\d{5}-\d{2}$");
            if (!regexSku.IsMatch(SKU))            
                return "Invalid Sku, format should be ##-##-#####-##";            

            if (db.RangePlans.Any(a => a.Sku == SKU))
                return "Range Plan Already Exists for this Sku";

            if (db.Renumbers.Any(a => a.OldSKU.Substring(0, 12) == SKU.Substring(0, 12)))
                return "This SKU has been renumbered and should not be used.";

            if (!currentUser.HasDivDept(AppName, SKU.Substring(0, 2), SKU.Substring(3, 2)))
                return "You do not have permission for this division/department.";

            List<Division> divs = currentUser.GetUserDivisions(AppName);

            if (!divs.Any(d => d.DivCode == SKU.Substring(0, 2)))            
                return "You do not have permission to create a range plan for this division";

            return "";
        }

        private long RetreiveOrCreateItemID(string SKU)
        {
            var itemlist = db.ItemMasters.Where(im => im.MerchantSku == SKU).ToList();

            if (itemlist.Count() > 0)            
                return itemlist.First().ID;            
            else
            {                
                string div = SKU.Substring(0, 2);

                int instance = configService.GetInstance(div);

                try
                {
                    itemDAO.CreateItemMaster(SKU, instance);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                return itemDAO.GetItemID(SKU);
            }
        }

        private Dictionary<string, string> ValidateCopyRangeRecord(CopyRangePlanModel copyRangeRec)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();
            string skuError;
           
            if (!string.IsNullOrEmpty(copyRangeRec.FromSku))
            {
                RangePlan range = rangePlanDAO.GetRangePlan(copyRangeRec.FromSku);

                if (range == null)                
                    errors.Add("FromSKU", "From Sku is not ranged.");
                
                copyRangeRec.FromRangePlan = range;
            }
            else if (!string.IsNullOrEmpty(copyRangeRec.FromDescription))     
                copyRangeRec.FromRangePlan = db.RangePlans.Where(rp => rp.Description == copyRangeRec.FromDescription).FirstOrDefault();            
            else           
                errors.Add("", "You must specify either a SKU or Store Range Description.");            

            if (errors.Count == 0)
            {
                if (copyRangeRec.FromSku.Substring(0, 2) != copyRangeRec.ToSku.Substring(0, 2))                
                    errors.Add("FromSKU", "You can only copy from a sku in the same division.");                

                //verify the sizes match on old/new sku
                List<string> ToSizes = (from a in db.Sizes
                                        where a.Sku == copyRangeRec.ToSku
                                        select a.Size).OrderBy(p => p).ToList();
                List<string> FromSizes = (from a in db.Sizes
                                          where a.Sku == copyRangeRec.FromRangePlan.Sku
                                          select a.Size).OrderBy(p => p).ToList();

                if (ToSizes.Count != FromSizes.Count)
                    errors.Add("", "These skus have different sizes, cannot copy");
                else
                {
                    for (int i = 0; i < ToSizes.Count; i++)
                    {
                        if (errors.Count == 0)
                            if (ToSizes[i] != FromSizes[i])                        
                                errors.Add("", "These skus have different sizes, cannot copy");                        
                    }
                }
            }

            if (string.IsNullOrEmpty(copyRangeRec.FromSku))
                copyRangeRec.FromSku = copyRangeRec.FromRangePlan.Sku;

            skuError = ValidateSKU(copyRangeRec.ToSku);

            if (!string.IsNullOrEmpty(skuError))
                errors.Add("ToSku", skuError);

            return errors;
        }

        public ActionResult CopyRangePlan()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CopyRangePlan(CopyRangePlanModel model)
        {
            Dictionary<string, string> errors;
            long itemID;

            errors = ValidateCopyRangeRecord(model);

            if (errors.Count() > 0)
            {
                foreach (KeyValuePair<string, string> error in errors)                
                    ModelState.AddModelError(error.Key, error.Value);

                return View(model);
            }

            try
            {
                itemID = RetreiveOrCreateItemID(model.ToSku);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            
            try
            {
                rangePlanDAO.CopyRangePlan(model.FromSku, model.ToSku, itemID, model.ToDescription, model.CopyOPRequest, currentUser.NetworkID);

                return RedirectToAction("Index", new { message = string.Format("Copied from {0} to {1}", model.FromSku, model.ToSku) });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", string.Format("There was a problem copying the SKU Range: {0}", ex.Message));
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult _AutoCompleteSku(string text)
        {
            IQueryable<string> results;
            results = (from a in db.RangePlans 
                       where a.Sku.StartsWith(text) 
                       select a.Sku).Distinct();
            return new JsonResult { Data = results.ToList() };
        }

        [HttpPost]
        public ActionResult _AutoCompleteDescription(string text)
        {
            IQueryable<string> results;
            results = (from a in db.RangePlans 
                       where a.Description.StartsWith(text) 
                       select (a.Description + " (" + a.Sku + ")")).Distinct();
            return new JsonResult { Data = results.ToList() };
        }

        #endregion

        #region "Presentation Qtys"
        [HttpPost]
        public ActionResult SaveSkuRange(SizeAllocationModel model)
        {
            RangePlan p = db.RangePlans.Where(rp => rp.Id == model.Plan.Id).First();

            p.StartDate = model.Plan.StartDate;
            p.EndDate = model.Plan.EndDate;
            p.UpdateDate = DateTime.Now;
            p.UpdatedBy = currentUser.NetworkID;

            if (ModelState.IsValid)            
                db.SaveChanges();
            
            return RedirectToAction("PresentationQuantities", "SkuRange", new { planID = model.Plan.Id });
        }

        public ActionResult SaveTotalSizeAllocation(List<SizeAllocationTotal> list)
        {
            long planID = 1;
            if (list.Count() > 0)            
                planID = list.First().PlanID;            

            SizeAllocationDAO dao = new SizeAllocationDAO();

            List<Rule> rules = db.GetRulesForPlan(planID, "SizeAlc");

            IEnumerable<StoreLookup> stores;

            try
            {
                RuleSet r = db.RuleSets.Where(rs => rs.PlanID == planID && rs.Type == "SizeAlc").First();
                //check spreadsheet upload
                stores = (new RuleDAO()).GetStoresInRuleSet(r.RuleSetID);
                if (stores.Count() == 0)                
                    stores = GetStoresForRules(rules, planID);                
            }
            catch
            {
                //invalid rules, so we pull all the stores in the plan
                stores = (from a in db.StoreLookups
                          join b in db.RangePlanDetails
                          on new { a.Division, a.Store } equals new { b.Division, b.Store }
                          where b.ID == planID
                          select a).ToList();
            }
            List<SizeAllocation> allocs = dao.GetSizeAllocationList(planID);
            //find stores in selected delivery groups
            List<DeliveryGroup> selected = ((List<DeliveryGroup>)Session["selectedDeliveryGroups"]);
            List<RuleSelectedStore> selectedStores = new List<RuleSelectedStore>();
            foreach (DeliveryGroup dg in selected)
            {
                if (dg.Selected)
                {
                    selectedStores.AddRange((from c in db.RuleSelectedStores
                                             join b in db.vValidStores on new { c.Division, c.Store } equals new { b.Division, b.Store }
                                             where c.RuleSetID == dg.RuleSetID
                                             select c).ToList());
                }
            }
            //reduce allocs to only selected stores.
            allocs = (from a in allocs
                      join b in selectedStores
                      on new { a.Division, a.Store } equals new { b.Division, b.Store }
                      join c in stores
                      on new { b.Division, b.Store } equals new { c.Division, c.Store }
                      select a).ToList();

            stores = (from a in stores
                      join b in allocs
                      on new { a.Division, a.Store } equals new { b.Division, b.Store }
                      select a).Distinct().ToList();
            int processedCount = 0;

            List<SizeAllocationTotal> totals = GetTotals(allocs);
            foreach (SizeAllocationTotal t in list)
            {
                planID = t.PlanID;
                int count = totals.Where(a => a.Size == t.Size && a.Min != t.Min).Count();

                if (count > 0)
                {
                    //we have a change on UI, write update to db
                    processedCount++;
                    dao.SaveMin((SizeAllocation)t, stores);
                }

                count = totals.Where(a => a.Size == t.Size && a.Max != t.Max).Count();

                if (count > 0)
                {
                    //we have a change on UI, write update to db
                    processedCount++;
                    dao.SaveMax((SizeAllocation)t, stores);
                }

                count = totals.Where(a => a.Size == t.Size && a.InitialDemand != t.InitialDemand).Count();

                if (count > 0)
                {
                    //we have a change on UI, write update to db
                    processedCount++;
                    dao.SaveInitialDemand((SizeAllocation)t, stores);
                }

                count = totals.Where(a => a.Size == t.Size && a.Range != t.Range).Count();

                if (count > 0)
                {
                    //we have a change on UI, write update to db
                    processedCount++;
                    dao.SaveRangeFlag((SizeAllocation)t, stores);
                }

                count = totals.Where(a => a.Size == t.Size && a.MinEndDays != t.MinEndDays).Count();

                if (count > 0)
                {
                    //we have a change on UI, write update to db
                    processedCount++;
                    dao.SaveMinEndDays((SizeAllocation)t, stores);
                }
            }

            if ((list[0].EndDate != null) || (list[0].RangeType != "N/A"))
            {
                processedCount++;
                return SaveTotalDates(list);
            }

            if (processedCount > 0)
                db.UpdateRangePlanDate(planID, currentUser.NetworkID);

            return RedirectToAction("PresentationQuantities", "SkuRange", new { planID });
        }

        public ActionResult SaveTotalDates(List<SizeAllocationTotal> list)
        {
            long planID = 1;
            if (list.Count() > 0)
                planID = list.First().PlanID;

            SizeAllocationDAO dao = new SizeAllocationDAO();
            List<Rule> rules = db.GetRulesForPlan(planID, "SizeAlc");

            IEnumerable<StoreLookup> stores;

            try
            {
                RuleSet r = db.RuleSets.Where(rs => rs.PlanID == planID && rs.Type == "SizeAlc").First();
                stores = (new RuleDAO()).GetStoresInRuleSet(r.RuleSetID);

                stores = (new RuleDAO()).GetStoresInRuleSet(r.RuleSetID);

                if ((stores == null) || (stores.Count() == 0))                
                    stores = GetStoresForRules(rules, planID);                
            }
            catch
            {
                //invalid rules, so we pull all the stores in the plan
                stores = (from a in db.StoreLookups 
                          join b in db.RangePlanDetails 
                          on new { a.Division, a.Store } equals new { b.Division, b.Store } 
                          where b.ID == planID 
                          select a).ToList();
            }

            //find stores in selected delivery groups
            List<DeliveryGroup> selected = (List<DeliveryGroup>)Session["selectedDeliveryGroups"];
            List<RuleSelectedStore> selectedStores = new List<RuleSelectedStore>();
            foreach (DeliveryGroup dg in selected)
            {
                if (dg.Selected)
                {
                    selectedStores.AddRange((from c in db.RuleSelectedStores
                                             join b in db.vValidStores on new { c.Division, c.Store } equals new { b.Division, b.Store }
                                             where c.RuleSetID == dg.RuleSetID
                                             select c).ToList());
                }
            }

            ////Now let's update the date of the rangeplan
            List<RangePlanDetail> detList = db.RangePlanDetails.Where(rpd => rpd.ID == planID).ToList();

            //filter it for the stores based on current rules
            if (stores != null)
            {
                //reduce stores to only stores for selected delivery groups.
                stores = (from a in stores
                          join b in selectedStores 
                          on new { a.Division, a.Store } equals new { b.Division, b.Store }
                          select a).ToList();

                detList = (from a in stores
                           from b in detList
                           where a.Division == b.Division && a.Store == b.Store
                           select b).ToList();
            }

            foreach (RangePlanDetail det in detList)
            {
                if (list.First().EndDate != null)                
                    det.EndDate = list.First().EndDate;
                
                if (list.First().RangeType != "N/A")                
                    det.RangeType = list.First().RangeType;                

                //always default to "Both"
                if (string.IsNullOrEmpty(det.RangeType))                
                    det.RangeType = "Both";                

                fixEndDate(det);
                db.Entry(det).State = System.Data.EntityState.Modified;
            }

            db.SaveChanges();

            if (list.Count() > 0)            
                rangePlanDAO.UpdateRangeHeader(planID, currentUser);            

            return RedirectToAction("PresentationQuantities", "SkuRange", new { planID });
        }

        public ActionResult SaveStoreSizeAllocation(IList<SizeAllocation> list)
        {
            long planID = 1;
            SizeAllocationDAO dao = new SizeAllocationDAO();

            List<StoreLookup> stores = new List<StoreLookup>();

            StoreLookup s = new StoreLookup();
            string currentStore = "FIRST";
            string currentDiv = "FIRST";
            foreach (SizeAllocation t in list)
            {
                s.Store = t.Store;
                s.Division = t.Division;
                stores.Add(s);

                planID = t.PlanID;
                dao.Save(t, (IEnumerable<StoreLookup>)stores);

                if (t.Store != currentStore || t.Division != currentDiv)
                {
                    //Now let's update the date of the rangeplan
                    RangePlanDetail det = db.RangePlanDetails.Where(rpd => rpd.ID == planID &&
                                                                           rpd.Division == t.Division &&
                                                                           rpd.Store == t.Store).First();
                    det.StartDate = t.StartDate;
                    det.EndDate = t.EndDate;

                    if (t.RangeType != "N\\A")                    
                        det.RangeType = t.RangeType;                    

                    //always default to "Both"
                    if (string.IsNullOrEmpty(det.RangeType))                    
                        det.RangeType = "Both";                    

                    fixEndDate(det);
                    db.Entry(det).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                }

                currentStore = t.Store;
                currentDiv = t.Division;
            }

            if (list.Count() > 0)            
                db.UpdateRangePlanDate(planID, currentUser.NetworkID);
            
            return RedirectToAction("PresentationQuantities", "SkuRange", new { planID });
        }

        private static void fixEndDate(RangePlanDetail det)
        {
            if (det.EndDate != null)
            {
                if (((DateTime)det.EndDate).Year < 2000)
                {
                    int centuries = ((DateTime.Now.Year - 1900) / 100) * 100;
                    det.EndDate = ((DateTime)det.EndDate).AddYears(centuries);
                }
            }
        }

        public ActionResult SaveStoreSizeAllocationAjax(IList<SizeAllocation> list)
        {
            long planID = 1;
            SizeAllocationDAO dao = new SizeAllocationDAO();

            List<StoreLookup> stores = new List<StoreLookup>();

            StoreLookup s = new StoreLookup();
            string currentStore = "FIRST";
            string currentDiv = "FIRST";
            foreach (SizeAllocation t in list)
            {
                if (t.Store != null)
                {
                    stores.Clear();
                    s.Store = t.Store;
                    s.Division = t.Division;
                    stores.Add(s);

                    planID = t.PlanID;
                    dao.Save(t, (IEnumerable<StoreLookup>)stores);

                    if (t.Store != currentStore || t.Division != currentDiv)
                    {
                        //Now let's update the date of the rangeplan
                        RangePlanDetail det = db.RangePlanDetails.Where(rpd => rpd.ID == planID &&
                                                                               rpd.Division == t.Division &&
                                                                               rpd.Store == t.Store).First();
                        det.StartDate = t.StartDate;
                        det.EndDate = t.EndDate;

                        det.RangeType = "Both";

                        fixEndDate(det);
                        db.Entry(det).State = System.Data.EntityState.Modified;
                        db.SaveChanges();
                    }
                    currentStore = t.Store;
                    currentDiv = t.Division;
                }
            }

            if (list.Count() > 0)                           
                db.UpdateRangePlanDate(planID, currentUser.NetworkID);            

            return Json("Success");
        }

        private List<SizeAllocationTotal> GetTotals(List<SizeAllocation> list)
        {
            list = list.OrderBy(o => o.Size).ToList();

            List<SizeAllocationTotal> totals = new List<SizeAllocationTotal>();

            SizeAllocationTotal currentTotal = null;

            string prevSize = "FIRSTSIZE";
            foreach (SizeAllocation sa in list)
            {
                if (prevSize != sa.Size)
                {
                    AddTotal(totals, currentTotal);
                    currentTotal = new SizeAllocationTotal()
                    {
                        Division = "NA",
                        Store = "NA",
                        PlanID = sa.PlanID,
                        Days = sa.Days,
                        Min = sa.Min,
                        Max = sa.Max,
                        ModifiedStore = false,
                        Size = sa.Size,
                        Range = sa.Range,
                        InitialDemand = sa.InitialDemand,
                        StartDate = sa.StartDate,
                        EndDate = sa.EndDate,
                        MinEndDays = sa.MinEndDays
                    };
                }

                if (currentTotal.Days != sa.Days ||
                    currentTotal.Min != sa.Min ||
                    currentTotal.Max != sa.Max ||
                    currentTotal.Range != sa.Range ||
                    currentTotal.InitialDemand != sa.InitialDemand ||
                    currentTotal.MinEndDays != sa.MinEndDays)
                {
                    currentTotal.ModifiedStore = true;
                }
                if (currentTotal.StartDate != sa.StartDate || currentTotal.EndDate != sa.EndDate)
                {
                    //TODO, show on screen that all dates aren't the same
                    currentTotal.ModifiedDates = true;
                }

                prevSize = sa.Size;
            }
            if (currentTotal != null)            
                AddTotal(totals, currentTotal);            

            //blank this out so UI can work
            if (totals.Count > 0)            
                totals[0].EndDate = null;
            
            return totals;
        }

        private static void AddTotal(List<SizeAllocationTotal> totals, SizeAllocationTotal currentTotal)
        {
            if (currentTotal != null)            
                totals.Add(currentTotal);            
        }

        public ActionResult ShowQFeed(long planID)
        {
            RangePlan rp = db.RangePlans.Where(r => r.Id == planID).First();
            RangeFileItemDAO dao = new RangeFileItemDAO();

            var info = (from a in db.RangePlans 
                        join b in db.InstanceDivisions 
                        on a.Sku.Substring(0, 2) equals b.Division 
                        where a.Id == planID 
                        select new { sku = a.Sku, instance = b.InstanceID }).First();

            dao.SetFirstReceiptDates(info.instance, info.sku);

            QFeedModel model = new QFeedModel()
            {
                VerificationMessages = new List<string>()
            };
            
            int storecount = (from a in db.RangePlanDetails 
                              join b in db.vValidStores 
                              on new { a.Division, a.Store } equals new { b.Division, b.Store } 
                              where a.ID == planID 
                              select a).Count();

            model.VerificationMessages.Add(storecount + " stores ranged.");
            model.VerificationMessages.Add("");

            ItemMaster item = db.ItemMasters.Where(i => i.ID == rp.ItemID).First();
            model.Sku = item.MerchantSku;
            model.RangePlan = rp;

            return View(model);
        }

        public ActionResult ReinitSku(string sku, long planID)
        {
            string errorMessage;

            errorMessage = AddReinitializedSKU(sku, currentUser);
            if (!string.IsNullOrEmpty(errorMessage))
                TempData["message"] = errorMessage;
            else
                TempData["message"] = "The SKU has been scheduled to reinitialize.";

            return RedirectToAction("PresentationQuantities", "SkuRange", new { planID });
        }

        [GridAction]
        public ActionResult _ShowQFeed(long planID)
        {
            List<RangeFileItem> model = null;
            if (planID > 0)
            {
                string SKU = (from a in db.RangePlans
                              where a.Id == planID
                              select a.Sku).First();

                RangeFileItemDAO dao = new RangeFileItemDAO();
                model = dao.GetRangeFileExtract(SKU);
            }
            return View(new GridModel(model));
        }

        public ActionResult ShowRDQIssues(long planID)
        {
            RangePlan rp = db.RangePlans.Where(r => r.Id == planID).First();

            RangeFileItemDAO dao = new RangeFileItemDAO();

            QFeedModel model = new QFeedModel()
            {
                VerificationMessages = new List<string>()
            };
            
            int storecount = (from a in db.RangePlanDetails 
                              join b in db.vValidStores 
                              on new { a.Division, a.Store } equals new { b.Division, b.Store } 
                              where a.ID == planID 
                              select a).Count();

            model.VerificationMessages.Add(storecount + " stores ranged.");
            model.VerificationMessages.Add("");

            //Unit cost and Unit retail on QR_product – the margin needs to be positive or it will not generate positive utility
            ItemMaster item = db.ItemMasters.Where(i => i.ID == rp.ItemID).First();
            model.Sku = item.MerchantSku;
            model.RangePlan = rp;

            int instanceid = configService.GetInstance(item.Div);
            model.Routes = db.Routes.Where(r => r.InstanceID == instanceid).ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult _CheckPrice(string sku)
        {
            ItemMaster item = db.ItemMasters.Where(im => im.MerchantSku == sku).FirstOrDefault();

            if (item != null)
            {
                Price price = db.Prices.Where(p => p.Division == item.Div && p.Stock == item.MerchantSku.Substring(0, 11)).FirstOrDefault();

                if (price != null)
                {
                    if (price.Stock != null)                    
                        return new JsonResult() { Data = price, JsonRequestBehavior = JsonRequestBehavior.AllowGet };                    
                    else                    
                        return Json("error");                    
                }
                else                
                    return Json("error");                
            }
            else            
                return Json("error");            
        }

        [GridAction]
        public ActionResult _CheckRoute(int routeid, int planID)
        {
            //verify all these zones are in routes
            var zoneids = (from a in db.RangePlanDetails
                           join b in db.NetworkZoneStores 
                           on new { a.Division, a.Store } equals new { b.Division, b.Store }
                           join c in db.NetworkZones 
                           on b.ZoneID equals c.ID
                           where a.ID == planID
                           select new { b.Division, b.Store, b.ZoneID, c.Name });

            Route route = db.Routes.Where(r => r.ID == routeid).First();

            //2.  See if all zoneids are in that route
            var routeZones = (from a in db.Routes 
                              join b in db.RouteDetails 
                              on a.ID equals b.RouteID 
                              where a.ID == routeid 
                              select new { b.RouteID, b.ZoneID });

            var missingZones = (from a in zoneids
                                where (!routeZones.Any(b => (b.ZoneID == a.ZoneID)))
                                select a);
            List<RangeIssue> issues = new List<RangeIssue>();
            RangeIssue issue;

            foreach (var det in missingZones)
            {
                issue = new RangeIssue();
                issue.Division = det.Division;
                issue.Store = det.Store;
                issue.Message = string.Format("Zone [{0}] not in route [{1}] for this product", det.Name, route.DisplayString);
                issues.Add(issue);
            }

            return View(new GridModel(issues));
        }


        [GridAction]
        public ActionResult _IssueGrid(long planID)
        {
            List<RangeIssue> issues = new List<RangeIssue>();
            RangeIssue issue;

            //On Range date has to be within lead time days of CRC 
            //  If not FRD needs to be set to CRC
            var dateCheck = (from a in db.RangePlanDetails
                             join b in db.MaxLeadTimes on new { a.Division, a.Store } equals new { b.Division, b.Store }
                             join c in db.InstanceDivisions on a.Division equals c.Division
                             join d in db.ControlDates on c.InstanceID equals d.InstanceID
                             where a.ID == planID
                             select new { a.Division, a.Store, a.StartDate, a.FirstReceipt, b.LeadTime, d.RunDate }
             );

            int goodCount = 0;
            int totalCount = dateCheck.Count();
            int futureStart = 0;
            foreach (var check in dateCheck)
            {
                if ((check.StartDate >= check.RunDate.AddDays(0 - check.LeadTime)) ||
                    (check.FirstReceipt <= check.RunDate))
                {
                    goodCount++;
                }
                else
                {
                    futureStart++;
                    issue = new RangeIssue();
                    issue.Division = check.Division;
                    issue.Store = check.Store;
                    if (check.StartDate != null)
                    {
                        issue.Message = "start date (" + ((DateTime)check.StartDate).ToShortDateString() + ") before window " + check.RunDate.AddDays(0 - check.LeadTime).ToShortDateString();
                    }
                    else
                    {
                        issue.Message = "start date is null";
                    }
                    issues.Add(issue);
                }
            }

            //On Range date has to be before off range date
            var startCheck = (from a in db.RangePlanDetails
                              where ((a.ID == planID) && (a.EndDate != null) && (a.StartDate >= a.EndDate))
                              select a);

            if (startCheck.Count() > 0)
            {
                foreach (var start in startCheck)
                {
                    issue = new RangeIssue();
                    issue.Division = start.Division;
                    issue.Store = start.Store;
                    issue.Message = "end date on or before start date";
                    issues.Add(issue);

                }
            }

            //Off range date needs to be far enough in the future to warrant distribution
            var endCheck = (from a in db.RangePlanDetails
                            join b in db.MaxLeadTimes on new { a.Division, a.Store } equals new { b.Division, b.Store }
                            join c in db.InstanceDivisions on a.Division equals c.Division
                            join d in db.ControlDates on c.InstanceID equals d.InstanceID
                            where a.ID == planID
                            select new { a.Division, a.Store, a.StartDate, a.FirstReceipt, b.LeadTime, d.RunDate, a.EndDate }
             );
            int endCount = 0;
            foreach (var end in endCheck)
            {
                if (end.RunDate.AddDays(end.LeadTime) > end.EndDate)
                {
                    endCount++;
                    issue = new RangeIssue();
                    issue.Division = end.Division;
                    issue.Store = end.Store;
                    issue.Message = "not enough time (lead time days) before end date";
                    issues.Add(issue);
                }
            }

            //verify all the stores are in zones
            var noZone = (from s in db.RangePlanDetails
                          where
                          (
                              (s.ID == planID) &&
                              (!db.NetworkZoneStores.Any(es => (es.Division == s.Division) && (es.Store == s.Store)))
                          )
                          select s);

            foreach (RangePlanDetail det in noZone)
            {
                issue = new RangeIssue();
                issue.Division = det.Division;
                issue.Store = det.Store;
                issue.Message = "Store not in any Zone";
                issues.Add(issue);
            }

            return View(new GridModel(issues));
        }

        public ActionResult ShowQFeedTextFast(long planID)
        {
            RangePlan rp = db.RangePlans.Where(r => r.Id == planID).First();

            int instance = configService.GetInstance(rp.Division);

            RangeFileItemDAO dao = new RangeFileItemDAO();
            System.Data.IDataReader reader = dao.GetRangeFileExtractDataReader(rp.Sku);

            RangeReformat reformat = new RangeReformat(instance);

            string results = "";
            results += reformat.GetHeader() + "\r\n";

            while (reader.Read())
            {
                if (reader[11] as int? == 1) //is it ranged                
                    results += reformat.Format(reader, instance) + "\r\n";                
                else                
                    results += reformat.Format(reader, "N", instance) + "\r\n";                
            }

            return File(Encoding.UTF8.GetBytes(results), "text/plain", string.Format("{0}.csv", rp.Sku));
        }

        public ActionResult ClearFilteredStores(long planID)
        {
            return RedirectToAction("PresentationQuantities", new { planID = planID });
        }

        public ActionResult PresentationQuantities(long planID, string message, string page, string show)
        {
            SkuSetupModel model = InitPresentationQtyModel(planID, message, page, show);
            GetPresentationQtyDeliveryGroups(model);

            model.OrderPlanningRequest = db.OrderPlanningRequests.Where(opr => opr.PlanID == planID).FirstOrDefault();
            GetPresentationQtyModelDetails(model, show);

            return View(model);
        }

        public ActionResult SelectDeliveryGroup(long DeliveryGroupID, long planID)
        {
            if (Session["selectedDeliveryGroups"] != null)
            {
                List<DeliveryGroup> groups = ((List<DeliveryGroup>)Session["selectedDeliveryGroups"]);
                DeliveryGroup updated = (from a in groups where a.ID == DeliveryGroupID select a).First();
                updated.Selected = !(updated.Selected);
                Session["selectedDeliveryGroups"] = groups;
            }
            return RedirectToAction("PresentationQuantities", new { planID = planID });
        }

        private void GetPresentationQtyModelDetails(SkuSetupModel model, string show)
        {
            string ruleType = "SizeAlc";
            #region ruleModel

            var existingRuleSet = (from a in db.RuleSets
                                   where (a.PlanID == model.RangePlan.Id) && (a.Type == ruleType)
                                   select a.RuleSetID);
            if (existingRuleSet.Count() > 0)
            {
                model.RuleSetID = existingRuleSet.First();
            }
            else
            {
                RuleSet rs = new RuleSet();
                rs.PlanID = model.RangePlan.Id;
                rs.Type = ruleType;
                rs.CreatedBy = User.Identity.Name;
                rs.CreateDate = DateTime.Now;
                db.RuleSets.Add(rs);
                db.SaveChanges();
                model.RuleSetID = rs.RuleSetID;
            }
            ViewData["ruleSetID"] = model.RuleSetID;
            ViewData["gridtype"] = ruleType;

            model.Rules = db.GetRulesForRuleSet(model.RuleSetID, ruleType);

            SizeAllocationDAO dao = new SizeAllocationDAO();

            List<SizeAllocation> allocs = dao.GetSizeAllocationList(model.RangePlan.Id);
            //find stores in selected delivery groups
            List<DeliveryGroup> selected = ((List<DeliveryGroup>)Session["selectedDeliveryGroups"]);
            List<RuleSelectedStore> selectedStores = new List<RuleSelectedStore>();
            foreach (DeliveryGroup dg in selected)
            {
                if (dg.Selected)
                {
                    selectedStores.AddRange((from c in db.RuleSelectedStores
                                             join b in db.vValidStores
                                             on new { c.Division, c.Store } equals new { b.Division, b.Store }
                                             where c.RuleSetID == dg.RuleSetID
                                             select c).ToList());
                }
            }
            //reduce allocs to only selected stores.
            allocs = (from a in allocs
                      join b in selectedStores
                      on new { a.Division, a.Store } equals new { b.Division, b.Store }
                      select a).ToList();
            if (model.Rules.Count() == 0)
            {
                //add a temp rule so we can show the and/etc.
                Rule r = new Rule();
                r.RuleSetID = model.RuleSetID;//(new RuleDAO()).GetRuleSetID(model.PlanID, "SizeAlc", User.Identity.Name);

                model.Rules = new List<Rule>();
                model.Rules.Add(r);
                model.RuleToAdd.RuleSetID = r.RuleSetID;

                List<StoreLookup> stores = (new RuleDAO()).GetValidStoresInRuleSet(r.RuleSetID);
                if (stores.Count > 0)
                {
                    //spreadsheet upload
                    model.StoreCount = (from a in stores
                                        join b in allocs
                                        on new { a.Store, a.Division } equals new { b.Store, b.Division }
                                        select a).Distinct().Count();
                    model.SizeAllocations = (from a in allocs
                                             join b in stores
                                             on new { a.Store, a.Division } equals new { b.Store, b.Division }
                                             select a).ToList();
                }
                else
                {
                    //model.StoreCount = model.Plan.StoreCount;
                    model.StoreCount = (from a in allocs
                                        select new { a.Division, a.Store }).Distinct().Count();
                    model.SizeAllocations = allocs;
                }
            }
            else
            {
                model.RuleToAdd.RuleSetID = model.Rules[0].RuleSetID;
                try
                {
                    model.NewStores = GetStoresForRules(model.Rules, model.RangePlan.Id);
                    model.StoreCount = model.NewStores.Count();
                    model.StoreCount = (from a in model.NewStores
                                        join b in allocs
                                        on new { a.Store, a.Division } equals new { b.Store, b.Division }
                                        select a).Distinct().Count();
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                    model.NewStores = new List<StoreLookupModel>();
                }

                model.SizeAllocations = (from a in allocs
                                         join b in model.NewStores
                                         on new { a.Store, a.Division } equals new { b.Store, b.Division }
                                         select a).ToList();
            }

            ViewData["show"] = show;
            if (show == "emptyStartDates")
            {
                model.SizeAllocations = (from a in model.SizeAllocations
                                         where (a.StartDate == null)
                                         select a).ToList();
            }

            model.RuleToAdd.Sort = model.Rules.Count() + 1;
            #endregion

            model.TotalSizeAllocations = GetTotals(model.SizeAllocations);
        }

        private void GetPresentationQtyDeliveryGroups(SkuSetupModel model)
        {
            model.DeliveryGroups = (from a in db.DeliveryGroups
                                    where a.PlanID == model.RangePlan.Id
                                    select a).ToList();

            InitializeDeliveryGroups(model);


            if (Session["selectedDeliveryGroups"] == null)
            {
                model.DeliveryGroups.ForEach(dg => { dg.Selected = true; });
                Session["selectedDeliveryGroups"] = model.DeliveryGroups;
            }
            else
            {
                bool resetSelected = model.RangePlan.Id != ((List<DeliveryGroup>)Session["selectedDeliveryGroups"])[0].PlanID;

                if (resetSelected)
                {
                    model.DeliveryGroups.ForEach(dg => { dg.Selected = true; });
                }
                else
                {
                    List<DeliveryGroup> groups = ((List<DeliveryGroup>)Session["selectedDeliveryGroups"]);
                    foreach (var dg in model.DeliveryGroups)
                    {
                        // retrieve dg from session
                        var sessiondg = groups.Where(d => d.ID.Equals(dg.ID)).FirstOrDefault();
                        if (sessiondg != null)
                        {
                            dg.Selected = sessiondg.Selected;
                        }
                        else
                        {
                            // session doesn't have delivery group (probably just created)
                            // select delivery group as default for new delivery groups
                            dg.Selected = true;
                        }
                    }
                }
                Session["selectedDeliveryGroups"] = model.DeliveryGroups;
            }
        }

        /// <summary>
        /// initializes the range for the presentation qty page
        /// updates store count
        /// </summary>
        /// <param name="planID"></param>
        /// <param name="message"></param>
        /// <param name="page"></param>
        /// <param name="show"></param>
        /// <returns></returns>
        private SkuSetupModel InitPresentationQtyModel(long planID, string message, string page, string show)
        {
            string ruleType = "SizeAlc";

            if (Request.UserAgent.Contains("Chrome") || Request.UserAgent.Contains("Firefox"))
            {
                ViewData["Chrome"] = "true";
            }

            if (!string.IsNullOrEmpty(message))
            {
                ViewData["message"] = message;
            }
            ViewData["planID"] = planID;
            ViewData["ruleType"] = ruleType;
            ViewData["page"] = page;
            ItemMaster i = (from a in db.ItemMasters
                            join b in db.RangePlans
                            on a.ID equals b.ItemID
                            where b.Id == planID
                            select a).FirstOrDefault();
            if (i != null)
            {
                ViewData["LifeCycle"] = i.LifeCycleDays;
            }
            SkuSetupModel model = new SkuSetupModel();
            model.RangePlan = db.RangePlans.Where(rp => rp.Id == planID).First();

            model.RangePlan.PreSaleSKU = db.PreSaleSKUs.Where(pss => pss.ItemID == model.RangePlan.ItemID && pss.Active).Count() > 0 ? "True" : "False";

            var reInitStatus = (from a in db.ReInitializeSKUs
                                where a.ItemID == model.RangePlan.ItemID
                                orderby a.CreateDate descending
                                select a).FirstOrDefault();

            if (reInitStatus != null)
            {
                model.RangePlan.ReInitializeStatus = (reInitStatus.SkuExtracted) ? "SKU Extracted on " + reInitStatus.LastModifiedDate.ToShortDateString() : "Pending to be Extracted";
            }

            if (model.RangePlan != null)
            {
                model.RangePlan.CreatedBy = getFullUserNameFromDatabase(model.RangePlan.CreatedBy.Replace('\\', '/'));
                model.RangePlan.UpdatedBy = getFullUserNameFromDatabase(model.RangePlan.UpdatedBy.Replace('\\', '/'));
            }

            //update the store count
            model.RangePlan.StoreCount = (from a in db.RangePlanDetails
                                          join b in db.vValidStores
                                          on new { a.Division, a.Store } equals new { b.Division, b.Store }
                                          where a.ID == planID
                                          select a).Count();

            var instanceQuery = from ad in db.AllocationDrivers
                                join id in db.InstanceDivisions
                                on ad.Division equals id.Division
                                join cd in db.ControlDates
                                on id.InstanceID equals cd.InstanceID
                                where ad.ConvertDate < cd.RunDate &&
                                      ad.OrderPlanningDate != null &&
                                      cd.RunDate >= ad.OrderPlanningDate &&
                                      ad.Division == model.RangePlan.Division &&
                                      ad.Department == model.RangePlan.Department
                                select ad;

            if (instanceQuery.Count() > 0)
                model.RangePlan.OPDepartment = true;
            else
                model.RangePlan.OPDepartment = false;

            db.SaveChanges();

            return model;
        }

        private void InitializeDeliveryGroups(SkuSetupModel model)
        {
            //if the plan doesn't have any, then let's create a default one with all stores
            if (model.DeliveryGroups.Count() == 0)
            {
                DeliveryGroup newGroup = new DeliveryGroup()
                {
                    Name = "Delivery Group 1",
                    PlanID = model.RangePlan.Id
                };

                db.DeliveryGroups.Add(newGroup);
                db.SaveChanges();

                RuleSet rs = new RuleSet()
                {
                    PlanID = model.RangePlan.Id,
                    Type = "Delivery",
                    CreateDate = DateTime.Now,
                    CreatedBy = currentUser.NetworkID
                };

                db.RuleSets.Add(rs);
                db.SaveChanges();

                newGroup.RuleSetID = rs.RuleSetID;

                List<RangePlanDetail> rangePlanDetails = db.RangePlanDetails.Where(rpd => rpd.ID == newGroup.PlanID).ToList();
                foreach (RangePlanDetail det in rangePlanDetails)
                {
                    RuleSelectedStore newDet = new RuleSelectedStore() 
                    {
                        RuleSetID = newGroup.RuleSetID,
                        Store = det.Store,
                        Division = det.Division,
                        CreateDate = DateTime.Now,
                        CreatedBy = currentUser.NetworkID
                    };

                    db.RuleSelectedStores.Add(newDet);
                }

                db.SaveChanges(currentUser.NetworkID);

                model.DeliveryGroups.Add(newGroup);
            }

            //update counts
            foreach (DeliveryGroup d in model.DeliveryGroups)
            {
                d.StoreCount = (from a in db.RuleSelectedStores
                                join p in db.RangePlanDetails on new { a.Division, a.Store } equals new { p.Division, p.Store }
                                join b in db.vValidStores on new { a.Division, a.Store } equals new { b.Division, b.Store }
                                where (a.RuleSetID == d.RuleSetID) && (p.ID == d.PlanID)
                                select a).Count();
            }

            db.SaveChanges(currentUser.NetworkID);
        }

        public ActionResult DeleteDeliveryGroup(int deliveryGroupID, long planID)
        {
            List<RuleSelectedStore> stores = (from a in db.RuleSelectedStores join c in db.DeliveryGroups on a.RuleSetID equals c.RuleSetID where c.ID == deliveryGroupID select a).ToList();
            foreach (RuleSelectedStore s in stores)
            {
                db.RuleSelectedStores.Remove(s);
            }

            RuleSet rs = (from a in db.RuleSets join c in db.DeliveryGroups on a.RuleSetID equals c.RuleSetID where c.ID == deliveryGroupID select a).First();
            db.RuleSets.Remove(rs);

            DeliveryGroup d = (from a in db.DeliveryGroups where a.ID == deliveryGroupID select a).First();
            db.DeliveryGroups.Remove(d);

            db.SaveChanges(currentUser.NetworkID);
            db.UpdateRangePlanDate(planID, currentUser.NetworkID);

            return RedirectToAction("PresentationQuantities", new { planID = planID });
        }

        public ActionResult AddMissingStoreToDeliveryGroup(string div, string store, long ruleSetID, long planID)
        {
            if (ruleSetID < 0)
            {
                //no ruleset yet, see if there's a delivery group created today
                DeliveryGroup newGroup;
                DateTime today = DateTime.Now.Date;
                var query = (from a in db.DeliveryGroups
                             where ((a.StartDate == today) && (a.PlanID == planID))
                             select a);
                if (query.Count() > 0)
                {
                    newGroup = query.First();
                }
                else
                {
                    newGroup = new DeliveryGroup();
                    CreateNewGroup(planID, newGroup, DateTime.Now.Date);
                }
                ruleSetID = newGroup.RuleSetID;
            }

            RuleSelectedStore rss = new RuleSelectedStore();
            rss.Division = div;
            rss.Store = store;
            rss.RuleSetID = ruleSetID;
            rss.CreatedBy = currentUser.NetworkID;
            rss.CreateDate = DateTime.Now;

            if ((from a in db.RuleSelectedStores
                 where ((a.Store == rss.Store) &&
                        (a.RuleSetID == rss.RuleSetID) &&
                        (a.Division == rss.Division))
                 select a).Count() == 0)
            {
                db.RuleSelectedStores.Add(rss);
            }
            //delete it from any other groups
            List<RuleSelectedStore> existing = (from a in db.RuleSelectedStores
                                                join b in db.RuleSets
                                                  on a.RuleSetID equals b.RuleSetID
                                                where (b.Type == "Delivery") &&
                                                    (b.RuleSetID != ruleSetID) &&
                                                    (b.PlanID == planID) &&
                                                    (a.Store == store) &&
                                                    (a.Division == div)
                                                select a).ToList();

            foreach (RuleSelectedStore delete in existing)
            {
                db.RuleSelectedStores.Remove(delete);
            }


            UpdateStoreDates(div, store, ruleSetID, planID, string.Empty);
            db.SaveChanges(UserName);
            rangePlanDAO.UpdateRangeHeader(planID, currentUser);

            return RedirectToAction("ShowStoresWithoutDeliveryGroup", new { planID = planID });
        }

        public ActionResult ShowStoresWithoutDeliveryGroup(int planID)
        {
            DeliveryGroupMissingModel model = new DeliveryGroupMissingModel();
            model.PlanID = planID;
            model.DeliveryGroups = db.DeliveryGroups.Where(dg => dg.PlanID == planID).ToList();

            DeliveryGroup newGroup = new DeliveryGroup()
            {
                Name = "<New Delivery Group>",
                ID = -1,
                RuleSetID = -1,
                PlanID = planID
            };

            model.DeliveryGroups.Insert(0, newGroup);

            List<StoreLookupModel> list = db.GetStoreLookupsForPlan(planID, currentUser.GetUserDivisionsString(AppName));
            List<RuleSelectedStore> ruleSetStores = (from a in db.RuleSets
                                                     join b in db.RuleSelectedStores
                                                        on a.RuleSetID equals b.RuleSetID
                                                     where a.PlanID == planID &&
                                                           a.Type == "Delivery"
                                                     select b).ToList();

            model.Stores = new List<StoreLookupModel>();
            foreach (StoreLookupModel m in list)
            {
                if ((from a in ruleSetStores
                     where a.Division == m.Division && a.Store == m.Store
                     select a).Count() == 0)
                {
                    //not in a delivery group
                    if ((from a in db.vValidStores
                         where a.Division == m.Division && a.Store == m.Store
                         select a).Count() > 0)
                    {
                        //valid store, so they need to assign it
                        model.Stores.Add(m);
                    }
                }
            }

            return View(model);
        }

        public ActionResult CreateDeliveryGroup(long planID)
        {
            DeliveryGroup newGroup = new DeliveryGroup();
            CreateNewGroup(planID, newGroup, null);
            return RedirectToAction("EditDeliveryGroup", new { planID = planID, deliveryGroupID = newGroup.ID });
        }

        private void CreateNewGroup(long planID, DeliveryGroup newGroup, DateTime? startDate)
        {
            newGroup.PlanID = planID;
            newGroup.Selected = true;
            int count = (from a in db.DeliveryGroups where a.PlanID == planID select a).Count();
            newGroup.Name = "Delivery Group " + (count + 1);
            newGroup.StoreCount = 0;
            if (startDate != null)
            {
                newGroup.StartDate = (DateTime)startDate;
            }
            db.DeliveryGroups.Add(newGroup);
            db.SaveChanges();

            RuleSet rs = new RuleSet();
            rs.PlanID = planID;
            rs.Type = "Delivery";
            rs.CreateDate = DateTime.Now;
            rs.CreatedBy = User.Identity.Name;

            db.RuleSets.Add(rs);
            db.SaveChanges();

            newGroup.RuleSetID = rs.RuleSetID;
            db.SaveChanges();

            rangePlanDAO.UpdateRangeHeader(planID, currentUser);
        }

        [GridAction]
        public ActionResult _DeliveryGroupStores(long planID, long deliveryGroupID)
        {
            List<StoreLookupModel> PlanStores = db.GetStoreLookupsForPlan(planID, currentUser.GetUserDivisionsString(AppName));
            return View(new GridModel(PlanStores));
        }

        public ActionResult EditDeliveryGroup(long planID, long deliveryGroupID)
        {
            DeliveryGroupModel model = new DeliveryGroupModel();
            model.DeliveryGroup = (from a in db.DeliveryGroups where a.ID == deliveryGroupID select a).First();

            if (model.DeliveryGroup.RuleSetID == 0)
            {
                RuleSet rs = new RuleSet();
                rs.PlanID = planID;
                rs.Type = "Delivery";
                rs.CreateDate = DateTime.Now;
                rs.CreatedBy = User.Identity.Name;

                db.RuleSets.Add(rs);
                db.SaveChanges();

                model.DeliveryGroup.RuleSetID = rs.RuleSetID;
                db.SaveChanges();
            }

            ViewData["ruleSetID"] = model.DeliveryGroup.RuleSetID;
            ViewData["ruleType"] = "Delivery";
            ItemMaster i = (from a in db.ItemMasters 
                            join b in db.RangePlans 
                              on a.ID equals b.ItemID 
                            where b.Id == planID 
                            select a).FirstOrDefault();
            if (i != null)
            {
                ViewData["LifeCycle"] = i.LifeCycleDays;
            }

            model.RuleModel = new RuleModel()
            {
                RuleSetID = model.DeliveryGroup.RuleSetID
            };
            
            return View(model);
        }

        [HttpPost]
        public ActionResult EditDeliveryGroup(DeliveryGroupModel model)
        {
            db.Entry(model.DeliveryGroup).State = System.Data.EntityState.Modified;
            if (model.DeliveryGroup.EndDate != null)
            {
                if (((DateTime)model.DeliveryGroup.EndDate).Year < 2000)
                {
                    int centuries = ((DateTime.Now.Year - 1900) / 100) * 100;
                    model.DeliveryGroup.EndDate = ((DateTime)model.DeliveryGroup.EndDate).AddYears(centuries);
                }
            }

            UpdateDeliveryGroupDates(model.DeliveryGroup);
            //note above line will save all changes
            rangePlanDAO.UpdateRangeHeader(model.DeliveryGroup.PlanID, currentUser);

            return RedirectToAction("PresentationQuantities", new { planID = model.DeliveryGroup.PlanID });
        }

        private void UpdateStoreDates(string div, string store, long ruleSetID, long planID, string rangeType)
        {
            //always default to "Both"
            if (string.IsNullOrEmpty(rangeType))
            {
                rangeType = "Both";
            }

            MaxLeadTime lt = (from c in db.MaxLeadTimes
                              where c.Store == store && c.Division == div
                              select c).FirstOrDefault();
            if (lt == null)
            {
                lt = new MaxLeadTime();
                lt.LeadTime = 5;
                lt.Division = div;
                lt.Store = store;
            }
            SizeAllocationDAO dao = new SizeAllocationDAO();
            List<RangePlanDetail> rangePlanDetails = (from a in db.RangePlanDetails
                                                      where a.ID == planID
                                                      select a).ToList();

            var query = (from a in rangePlanDetails
                         where ((a.Division == lt.Division) && (a.Store == lt.Store))
                         select a);

            DeliveryGroup dg = (from a in db.DeliveryGroups
                                where a.RuleSetID == ruleSetID
                                select a).First();

            foreach (RangePlanDetail rpDet in query)
            {
                rpDet.RangeType = rangeType;
                db.Entry(rpDet).State = System.Data.EntityState.Modified;
                //set start/end date
                if (dg.StartDate != null)
                {
                    rpDet.StartDate = ((DateTime)dg.StartDate).AddDays(lt.LeadTime);
                    //db.Entry(rpDet).State = System.Data.EntityState.Modified;
                }
                else
                {
                    rpDet.StartDate = null;
                }
                if (dg.EndDate != null)
                {
                    rpDet.EndDate = ((DateTime)dg.EndDate).AddDays(lt.LeadTime);
                }
                else
                {
                    rpDet.EndDate = null;
                }
            }
        }

        public void FixBadDeliveryGroups()
        {
            RangePlanDetailDAO dao = new RangePlanDetailDAO();
            int count = 0;
            foreach (DeliveryGroup dg in dao.GetBadDeliveryGroups())
            {
                UpdateDeliveryGroupDates(dg);
                count++;
            }
        }

        public void UpdateDeliveryGroupDates(DeliveryGroup model)
        {
            int dts = (from a in db.DirectToStoreSkus
                       join b in db.RangePlans
                       on a.Sku equals b.Sku
                       where b.Id == model.PlanID
                       select b).Count();

            RangePlan plan = db.RangePlans.Where(rp => rp.Id == model.PlanID).FirstOrDefault();

            List<MaxLeadTime> leadTimes;

            leadTimes = (from b in db.RuleSelectedStores
                         join c in db.MaxLeadTimes
                         on new { b.Division, b.Store } equals new { c.Division, c.Store }
                         where b.RuleSetID == model.RuleSetID
                         select c).ToList();

            SizeAllocationDAO dao = new SizeAllocationDAO();
            List<RangePlanDetail> rangePlanDetails = (from a in db.RangePlanDetails
                                                      join b in db.RuleSelectedStores
                                                      on new { a.Division, a.Store } equals new { b.Division, b.Store }
                                                      where a.ID == model.PlanID && b.RuleSetID == model.RuleSetID
                                                      select a).ToList();

            if (plan.Launch)
            {
                foreach (RangePlanDetail det in rangePlanDetails)
                {
                    db.Entry(det).State = System.Data.EntityState.Modified;

                    //set start/end date
                    if (model.StartDate.HasValue)
                    {
                        det.StartDate = model.StartDate.Value;
                        det.EndDate = model.EndDate.Value;
                    }                    
                }
            }
            else if (dts == 0)
            {
                //non dts store, set start date to delivery group start date + lead time
                foreach (var lt in leadTimes)
                {
                    List<RangePlanDetail> rpDetails = rangePlanDetails.Where(rpd => rpd.Division == lt.Division && rpd.Store == lt.Store).ToList();

                    foreach (RangePlanDetail det in rpDetails)
                    {
                        db.Entry(det).State = System.Data.EntityState.Modified;
                        //set start/end date
                        if (model.StartDate.HasValue)                        
                            det.StartDate = model.StartDate.Value.AddDays(lt.LeadTime);                        
                        else                        
                            det.StartDate = model.StartDate;
                        
                        if (model.EndDate.HasValue)                        
                            det.EndDate = model.EndDate.Value.AddDays(lt.LeadTime);                        
                        else                        
                            det.EndDate = model.EndDate;                        
                    }
                }

                //for stores without any leadtimes, just use a default
                var queryLT = rangePlanDetails.Where(p => !db.StoreLeadTimes.Any(p2 => p2.Division == p.Division && p2.Store == p.Store));

                foreach (RangePlanDetail det in queryLT)
                {
                    //set start/end date
                    if (model.StartDate.HasValue)
                    {
                        det.StartDate = model.StartDate.Value.AddDays(_DEFAULT_MAX_LEADTIME);
                        db.Entry(det).State = System.Data.EntityState.Modified;
                    }
                    
                    if (model.EndDate.HasValue)                    
                        det.EndDate = model.EndDate.Value.AddDays(_DEFAULT_MAX_LEADTIME);                    
                }
            }
            else
            {
                foreach (RangePlanDetail det in rangePlanDetails)
                {
                    //set start/end date
                    db.Entry(det).State = System.Data.EntityState.Modified;
                    if (model.StartDate.HasValue)
                    {
                        det.StartDate = model.StartDate;
                        det.EndDate = model.EndDate;
                    }                    
                }
            }

            rangePlanDAO.UpdateRangeHeader(model.PlanID, currentUser);
            db.SaveChanges(currentUser.NetworkID);
        }
        #endregion

        #region "Add/Remove Stores"
        public ActionResult Edit(long planID, string message)
        {
            ViewData["message"] = message;
            RangePlan p = db.RangePlans.Where(rp => rp.Id == planID).First();

            return View(p);
        }

        [HttpPost]
        public ActionResult Edit(RangePlan model)
        {
            db.Entry(model).State = System.Data.EntityState.Modified;
            model.UpdateDate = DateTime.Now;
            model.UpdatedBy = currentUser.NetworkID;
            db.SaveChanges(currentUser.NetworkID);
            List<DeliveryGroup> groups = db.DeliveryGroups.Where(dg => dg.PlanID == model.Id).ToList();
            
            foreach (DeliveryGroup dg in groups)
            {
                UpdateDeliveryGroupDates(dg);
            }
            return RedirectToAction("Index", new { message = "Saved Changes" });
        }

        public ActionResult EditStores(long planID, string message)
        {
            ViewData["planID"] = planID;
            ViewData["gridtype"] = "AllStores";
            ViewData["message"] = message;
            EditStoreModel model = new EditStoreModel()
            {
                plan = db.RangePlans.Where(rp => rp.Id == planID).First(),
                CurrentStores = db.GetStoreLookupsForPlan(planID, currentUser.GetUserDivisionsString(AppName)),
                RemainingStores = db.GetStoreLookupsNotInPlan(planID, currentUser.GetUserDivisionsString(AppName))
            };

            model.plan.ItemMaster = db.ItemMasters.Where(i => i.ID == model.plan.ItemID).FirstOrDefault();

            return View(model);
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public ActionResult UploadStores(IEnumerable<HttpPostedFileBase> attachments, long planID)
        {
            StoreSpreadsheet storeSpreadsheet = new StoreSpreadsheet(appConfig, configService, rangePlanDAO, new RuleDAO());

            string message = string.Empty;
            int successCount = 0;

            foreach (HttpPostedFileBase file in attachments)
            {
                storeSpreadsheet.Save(file, planID);

                if (!string.IsNullOrEmpty(storeSpreadsheet.message))
                    return Content(storeSpreadsheet.message);

                successCount += storeSpreadsheet.validStores.Count();
            }

            return Json(new { message = string.Format("Upload complete. Added {0} store(s)", successCount) }, "application/json");
        }

        public ActionResult StoreTemplate()
        {
            StoreSpreadsheet storeSpreadsheet = new StoreSpreadsheet(appConfig, configService, rangePlanDAO, new RuleDAO());
            Excel excelDocument;

            excelDocument = storeSpreadsheet.GetTemplate();

            excelDocument.Save("StoreTemplate.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

        public ActionResult Excel(long planID)
        {
            SKURangeStoreExport rangeStoreExport = new SKURangeStoreExport(appConfig, rangePlanDAO);
            rangeStoreExport.WriteData(planID);
            rangeStoreExport.excelDocument.Save("SkuRangePlanStores.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

        public ActionResult RemoveAllStores(long planID)
        {
            ClearStoreFromPlan(planID);
            return RedirectToAction("EditStores", new { planID = planID, message = "All stores removed" });
        }

        private void ClearStoreFromPlan(long planID)
        {
            RangePlanDetailDAO dao = new RangePlanDetailDAO();
            dao.DeleteStoresForPlan(planID);
            db.UpdateRangePlanDate(planID, currentUser.NetworkID);            
        }
        #endregion

        #region "Add Stores By Rules"
        public ActionResult RuleList(long planID)
        {
            ViewData["planID"] = planID;
            long rulesetid = (new RuleDAO()).GetRuleSetID(planID, "main", currentUser.NetworkID);

            List<Rule> rules = (from r in db.Rules where r.RuleSetID == rulesetid orderby r.Sort ascending select r).ToList();
            return PartialView(rules);
        }

        [GridAction]
        public ActionResult _RuleList(long planID)
        {
            long rulesetid = (new RuleDAO()).GetRuleSetID(planID, "main", currentUser.NetworkID);
            List<Rule> rules = (from r in db.Rules where r.RuleSetID == rulesetid orderby r.Sort ascending select r).ToList();
            return PartialView(new GridModel(rules));
        }

        /// <summary>
        /// Populates partial view (grid) with list of stores that are in rule, showing which are already in the range plan.
        /// </summary>
        /// <param name="planID"></param>
        /// <returns></returns>
        public ActionResult StoreLookupList(long planID, string gridtype, string ruleType)
        {
            List<StoreLookupModel> list;
            ViewData["planID"] = planID;
            //ViewData["gridtype"] = filter;
            if (gridtype == "AllStores")
            {
                list = db.GetStoreLookupsForPlan(planID, currentUser.GetUserDivisionsString(AppName));
                list.AddRange(db.GetStoreLookupsNotInPlan(planID, currentUser.GetUserDivisionsString(AppName)));
            }
            else
            {
                List<Rule> rules = db.GetRulesForPlan(planID, ruleType);

                try
                {
                    list = GetStoresForRules(rules, planID);
                }
                catch (Exception ex)
                {
                    list = new List<StoreLookupModel>();
                    ShowError(ex);
                }
            }
            return PartialView(list);
        }

        /// <summary>
        /// Populates partial view (grid) with list of stores that are in rule, showing which are already in the range plan.
        /// </summary>
        /// <param name="planID"></param>
        /// <returns></returns>
        [GridAction]
        public ActionResult _StoreLookupList(long planID, string gridtype, string ruleType)
        {
            List<StoreLookupModel> list;

            if (gridtype == "AllStores")
            {
                list = db.GetStoreLookupsForPlan(planID, currentUser.GetUserDivisionsString(AppName));
                list.AddRange(db.GetStoreLookupsNotInPlan(planID, currentUser.GetUserDivisionsString(AppName)));
            }
            else
            {
                List<Rule> rules = db.GetRulesForPlan(planID, ruleType);

                try
                {
                    list = GetStoresForRules(rules, planID);
                }
                catch (Exception ex)
                {
                    list = new List<StoreLookupModel>();
                    ShowError(ex);
                }
                List<StoreLookupModel> currStores = db.GetStoreLookupsForPlan(planID, currentUser.GetUserDivisionsString(AppName));

                var currlist =
                        from n in list
                        join c in currStores on new { n.Division, n.Store } equals new { c.Division, c.Store }
                        select n;
               
                foreach (StoreLookupModel m in currlist)
                {
                    m.InCurrentPlan = true;
                }
            }
            return PartialView(new GridModel(list));
        }

        /// <summary>
        /// Main page for adding stores to a range plan, it is composed of several subpages
        /// - List of rules
        /// - List of Stores (result of rules)
        /// - Place to enter a new rule, move rules up/down, add/remove store from current plan
        /// </summary>
        /// <param name="planID"></param>
        /// <param name="ruleType">Type of rule "Main" is for store plan, "Filter" is for filtering these from Presentation Qty screen</param>
        /// <returns></returns>      
        public ActionResult AddStoresByRule(int planID, string ruleType)
        {
            ViewData["planID"] = planID;
            ViewData["gridtype"] = "AddStoresByRule";

            if (ruleType == null)
            {
                ruleType = "Main";
            }

            ViewData["ruleType"] = ruleType;
            RuleModel model = new RuleModel();

            var ruleSetQuery = (from a in db.RuleSets
                                where (a.PlanID == planID) && (a.Type == ruleType)
                                select a.RuleSetID);

            if (ruleSetQuery.Count() > 0)
            {
                model.RuleSetID = ruleSetQuery.First();
            }
            else
            {
                RuleSet rs = new RuleSet()
                {
                    PlanID = planID,
                    Type = ruleType,
                    CreatedBy = currentUser.NetworkID,
                    CreateDate = DateTime.Now
                };

                db.RuleSets.Add(rs);
                db.SaveChanges();
                model.RuleSetID = rs.RuleSetID;
            }
            ViewData["ruleSetID"] = model.RuleSetID;

            model.PlanID = planID;
            model.Plan = db.RangePlans.Where(rp => rp.Id == planID).First();

            db.UpdateRangePlanDate(planID, currentUser.NetworkID);            

            return View(model);
        }

        private void ShowError(Exception ex)
        {
            if (ex.Message.Contains("rule"))            
                ViewData["rulemessage"] = ex.Message;            
            else            
                ViewData["rulemessage"] = "invalidly formatted rule!";            
        }

        /// <summary>
        /// Test page so we can get to the "Add Stores By Rule" page for a series of different planIDs.
        /// </summary>
        public ActionResult TestRule()
        {
            return View();
        }

        /// <summary>
        /// Add 'and','or','not', '(', ')' to rule list
        /// </summary>
        public ActionResult AddConjuction(string value, long planID, string ruleType)
        {
            Rule newRule = new Rule();
            newRule.RuleSetID = (new RuleDAO()).GetRuleSetID(planID, ruleType, currentUser.NetworkID);
            newRule.Compare = value.Trim();
            newRule.Sort = db.Rules.Where(r => r.RuleSetID == newRule.RuleSetID).Count() + 1;

            db.Rules.Add(newRule);
            db.SaveChanges();

            RuleSet rs = db.RuleSets.Where(r => r.RuleSetID == newRule.RuleSetID).First();
            if (rs.Type == "SizeAlc")            
                return RedirectToAction("PresentationQuantities", new { planID = planID });            
            else            
                return RedirectToAction("AddStoresByRule", new { planID = planID });            
        }

        /// <summary>
        /// delete all rules
        /// </summary>
        public ActionResult ClearRules(string value, long planID, string ruleType)
        {
            Rule newRule = new Rule();
            long ruleSetID = (new RuleDAO()).GetRuleSetID(planID, ruleType, currentUser.NetworkID);

            IEnumerable<Rule> rules = (from a in db.Rules where a.RuleSetID == ruleSetID select a);
            RuleDAO dao = new RuleDAO();
            foreach (Rule rule in rules)
            {
                dao.Delete(rule);
            }
            RuleSet rs = (from a in db.RuleSets where a.RuleSetID == ruleSetID select a).First();
            if (rs.Type == "SizeAlc")
            {
                //delete all the ruleselected stores
                foreach (RuleSelectedStore rss in (from a in db.RuleSelectedStores where a.RuleSetID == ruleSetID select a))
                {
                    db.RuleSelectedStores.Remove(rss);
                }
                db.SaveChanges(UserName);
                return RedirectToAction("PresentationQuantities", new { planID = planID });
            }
            else
            {
                return RedirectToAction("AddStoresByRule", new { planID = planID });
            }
        }

        /// <summary>
        /// Add a new rule specified by user
        /// "Store Equals 07801"
        /// </summary>
        [HttpPost]
        public ActionResult AddRule(RuleModel newRule)
        {
            if (ModelState.IsValid)
            {
                db.Rules.Add(newRule.RuleToAdd);
                db.SaveChanges();
            }
            Int64 planID = (new RuleDAO()).GetPlanID(newRule.RuleToAdd.ID);

            RuleSet rs = (from a in db.RuleSets where a.RuleSetID == newRule.RuleToAdd.RuleSetID select a).FirstOrDefault();

            if (rs.Type == "SizeAlc")
            {
                return RedirectToAction("PresentationQuantities", new { planID = planID });
            }
            else
            {
                return RedirectToAction("AddStoresByRule", new { planID = planID });
            }
        }

        /// <summary>
        /// Add all the stores that meet the new rules to the range plan (planID)
        /// </summary>
        public ActionResult AddAllStores(Int64 planID)
        {
            List<Rule> RulesForPlan = db.GetRulesForPlan(planID, "Main");

            List<StoreLookupModel> StoreList = GetStoresForRules(RulesForPlan, planID);

            List<RangePlanDetail> details = new List<RangePlanDetail>();
            RangePlanDetail det;
            DateTime createDate = DateTime.Now;
            foreach (StoreLookupModel s in StoreList)
            {
                det = new RangePlanDetail();
                det.ID = planID;
                det.Store = s.Store;
                det.Division = s.Division;
                det.CreateDate = createDate;
                det.CreatedBy = currentUser.NetworkID;
                details.Add(det);
            }

            RangePlanDetailDAO dao = new RangePlanDetailDAO();
            dao.AddStores(details);

            db.UpdateRangePlanDate(planID, currentUser.NetworkID);

            return RedirectToAction("AddStoresByRule", new { planID = planID });

        }

        /// <summary>
        /// Add only the stores that are visible in the filtered grid to the planID
        /// TODO:  this is not implemented, it would need to create a lambda expression based on the filter.
        /// </summary>
        public JsonResult AddFilteredStores(long planID, string filter, string ruleType)
        {
            List<Rule> RulesForPlan = db.GetRulesForPlan(planID, ruleType);

            List<StoreLookupModel> StoreList = GetStoresForRules(RulesForPlan, planID);

            List<RangePlanDetail> details = new List<RangePlanDetail>();
            RangePlanDetail det;
            DateTime createDate = DateTime.Now;
            foreach (StoreLookupModel s in StoreList)
            {
                det = new RangePlanDetail();
                det.ID = planID;
                det.Store = s.Store;
                det.Division = s.Division;
                det.CreateDate = createDate;
                det.CreatedBy = currentUser.NetworkID;
                details.Add(det);
            }

            RangePlanDetailDAO dao = new RangePlanDetailDAO();
            dao.AddStores(details);

            db.UpdateRangePlanDate(planID, currentUser.NetworkID);
            return Json("Success");
        }


        /// <summary>
        /// Add a store to the range plan (planID)
        /// </summary>
        public JsonResult AddStore(string store, string div, long planID)
        {
            try
            {
                RangePlanDetail det = new RangePlanDetail();
                det.Store = store;
                det.Division = div;
                det.ID = planID;
                det.CreateDate = DateTime.Now;
                det.CreatedBy = currentUser.NetworkID;

                var deliveryGroupQuery = (from a in db.DeliveryGroups
                                          join c in db.RuleSelectedStores on a.RuleSetID equals c.RuleSetID
                                          where ((a.PlanID == planID) &&
                                                 (c.Store == store) &&
                                                 (c.Division == div))
                                          select a);

                if (deliveryGroupQuery.Count() > 0)
                {
                    DeliveryGroup dg = deliveryGroupQuery.First();
                    MaxLeadTime lt = (from c in db.MaxLeadTimes
                                      where ((c.Store == store) && (c.Division == div))
                                      select c).FirstOrDefault();
                    if (lt == null)
                    {
                        lt = new MaxLeadTime();
                        lt.LeadTime = 5;
                        lt.Division = div;
                        lt.Store = store;
                    }

                    //set start/end date
                    if (dg.StartDate != null)
                    {
                        det.StartDate = ((DateTime)dg.StartDate).AddDays(lt.LeadTime);
                    }
                    if (dg.EndDate != null)
                    {
                        det.EndDate = ((DateTime)dg.EndDate).AddDays(lt.LeadTime);
                    }
                }

                db.RangePlanDetails.Add(det);
                db.SaveChanges();

                db.UpdateRangePlanDate(planID, currentUser.NetworkID);                

                return Json("Success");
            }
            catch (Exception ex)
            {
                return Json(string.Format("Error - {0}", ex.Message));
            }
        }

        /// <summary>
        /// Deletes the store from the range plan (planID)
        /// </summary>
        public JsonResult DeleteStore(string store, string div, long planID)
        {
            try
            {
                RangePlanDetail det = (from a in db.RangePlanDetails where ((a.Store == store) && (a.Division == div) && (a.ID == planID)) select a).First();

                db.RangePlanDetails.Remove(det);
                db.SaveChanges();

                var query2 = (from a in db.RuleSets join b in db.RuleSelectedStores on a.RuleSetID equals b.RuleSetID where ((a.PlanID == planID) && (b.Division == div) && (b.Store == store)) select b);
                if (query2.Count() > 0)
                {
                    //delete it
                    foreach (RuleSelectedStore rss in query2)
                    {
                        db.RuleSelectedStores.Remove(rss);
                    }
                    db.SaveChanges();
                }

                db.UpdateRangePlanDate(planID, currentUser.NetworkID);                

                return Json("Success");
            }
            catch (Exception ex)
            {
                return Json(string.Format("Error: {0}", ex.Message));
            }
        }
        #endregion
        #endregion


        #region Get Store List from Rule List
        private Boolean ValidateRules(List<Rule> rules)
        {
            try
            {
                if (rules.Count > 0)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Finds what stores meet the rules criteria
        /// </summary>
        private List<StoreLookupModel> GetStoresForRules(List<Rule> rules, Int64 planID)
        {
            List<StoreLookupModel> list = new List<StoreLookupModel>();
            List<Rule> finalRules = new List<Rule>();

            RangePlan p = (from a in db.RangePlans
                           where a.Id == planID
                           select a).First();

            //add division criteria to rules
            Rule divRule = new Rule();
            divRule.Compare = "Equals";
            divRule.Field = "Division";
            divRule.Value = p.Sku.Substring(0, 2);

            finalRules.Add(divRule);

            divRule = new Rule();
            divRule.Compare = "and";
            finalRules.Add(divRule);

            foreach (Rule r in rules)
            {
                finalRules.Add(r);
            }

            IQueryable<StoreLookup> queryableData = db.StoreLookups.AsQueryable<StoreLookup>();
            ParameterExpression pe = Expression.Parameter(typeof(StoreLookup), "StoreLookup");

            Expression finalExpression = GetExpression(finalRules, pe);
            // Create an expression tree that represents the expression 
            MethodCallExpression whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { queryableData.ElementType },
                queryableData.Expression,
                Expression.Lambda<Func<StoreLookup, bool>>(finalExpression, new ParameterExpression[] { pe }));
            // ***** End Where ***** 

            IQueryable<StoreLookup> results = queryableData.Provider.CreateQuery<StoreLookup>(whereCallExpression);


            foreach (StoreLookup s in results)
            {
                list.Add(new StoreLookupModel(s, planID, false));
            }

            return list;
        }

        /// <summary>
        /// recursive function to create the lambda expression for the users set of rules
        /// </summary>
        private Expression GetExpression(List<Rule> rules, ParameterExpression pe)
        {
            if (rules.Count() > 0)
            {
                if (rules[0].Field == null)
                {
                    //and,or,(,),not
                    int openCount = 1;
                    if (rules[0].Compare.Equals("("))
                    {
                        for (int i = 1; i < rules.Count(); i++)
                        {
                            if (rules[i].Compare.Equals("("))
                            {
                                openCount++;
                            }
                            else if (rules[i].Compare.Equals(")"))
                            {
                                openCount--;
                            }
                            if (openCount == 0)
                            {
                                //get the expression for this paren block
                                if (i < (rules.Count() - 1))
                                {
                                    //we have more rules
                                    Expression newExp = GetExpression(rules.GetRange(1, i - 1), pe);
                                    return GetCompositeRule(newExp, rules.GetRange(i + 1, (rules.Count() - i - 1)), pe);
                                }
                                else
                                {
                                    //no more rules, just strip out the parens and process the block
                                    return GetExpression(rules.GetRange(1, (i - 1)), pe);
                                }
                            }
                        }
                        throw new Exception("invalidly formatted rule (parens)!");
                    }
                    else if (rules[0].Compare.Equals("not"))
                    {
                        if (rules.Count() > 1)
                        {
                            return Expression.Not(GetExpression(rules.GetRange(1, rules.Count() - 1), pe));
                        }
                        throw new Exception("invalidly formatted rule (not)!");
                    }
                    throw new Exception("invalidly formatted rule (missing first rule)!");
                }
                else if (rules.Count() > 1)
                {
                    //we have more rules
                    return GetCompositeRule(GetExpressionFromSingleRule(rules[0], pe), rules.GetRange(1, rules.Count() - 1), pe);
                    //return GetCompositeRule(rules, 0, pe);
                }
                else
                {
                    //single rule, just evaluate it
                    return GetExpressionFromSingleRule(rules[0], pe);
                }
            }
            else
            {
                //no rules 
                return null;
            }
        }

        /// <summary>
        /// Get a lambda expression from a single rule
        /// </summary>
        private Expression GetExpressionFromSingleRule(Rule rule, ParameterExpression pe)
        {
            Expression exp = null;
            Expression left = null;
            Expression right = null;

            if (rule.Field == "AdHocCode")
            {
            }
            else if ((rule.Field == "StorePlan") || (rule.Field == "RangePlan"))
            {
                //return an expression that will link to the RangePlanDetails
                //TODO:  This is faster than manually creating and/or's but still slow
                //not sure if there is a better way to build the expression tree
                List<string> stores;

                if (rule.Field == "RangePlan")
                {
                    Int64 planID;
                    string sku = Convert.ToString(rule.Value);

                    string myInClause = currentUser.GetUserDivisionsString(AppName);
                    try
                    {
                        planID = (from x in db.RangePlans where (x.Sku == sku) select x).First().Id;
                    }
                    catch
                    {
                        stores = new List<string>();
                        planID = -1;
                    }
                    stores = (from rp in db.RangePlanDetails where ((rp.ID == planID) && (myInClause.Contains(rp.Division))) select rp.Division + rp.Store).Distinct().ToList();
                }
                else
                {
                    stores = (from rp in db.StorePlans where rp.PlanName == rule.Value select rp.Division + rp.Store).Distinct().ToList();
                }

                ConstantExpression foreignKeysParameter = Expression.Constant(stores, typeof(List<string>));

                MethodInfo method = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string), });
                Expression memberExpression = Expression.Property(pe, "Store");
                Expression memberExpression2 = Expression.Property(pe, "Division");
                MethodCallExpression concatExpression = Expression.Call(method, memberExpression2, memberExpression);

                Expression convertExpression = Expression.Convert(concatExpression, typeof(string));  //store and divison
                MethodCallExpression containsExpression = Expression.Call(foreignKeysParameter
                    , "Contains", new Type[] { }, convertExpression);


                switch (rule.Compare)
                {
                    case "Equals":
                        return containsExpression;
                    case "Does Not Equal":
                        return Expression.Not(containsExpression);
                    default:
                        throw new Exception("Unsupported rule:  Operation \"" + rule.Compare + "\" not valid for \"" + rule.Field + "\"");
                }
            }
            else
            {
                left = Expression.Property(pe, typeof(StoreLookup).GetProperty(rule.Field));
                right = Expression.Constant(rule.Value, typeof(string));

                switch (rule.Compare)
                {
                    case "Equals":
                        exp = Expression.Equal(left, right);
                        break;
                    case "Does Not Equal":
                        exp = Expression.NotEqual(left, right);
                        break;
                    case "Is less than":
                        exp = Expression.LessThan(left, right);
                        break;
                    case "Is Greater than":
                        exp = Expression.GreaterThan(left, right);
                        break;
                    case "Contains":
                        var propertyExp = Expression.Property(pe, rule.Field);
                        MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        var someValue = Expression.Constant(rule.Value, typeof(string));
                        exp = Expression.Call(propertyExp, method, someValue);
                        break;
                    case "StartsWith":
                        var propertyExp2 = Expression.Property(pe, rule.Field);
                        MethodInfo method2 = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                        var someValue2 = Expression.Constant(rule.Value, typeof(string));
                        exp = Expression.Call(propertyExp2, method2, someValue2);
                        break;
                    case "EndsWith":
                        var propertyExp3 = Expression.Property(pe, rule.Field);
                        MethodInfo method3 = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
                        var someValue3 = Expression.Constant(rule.Value, typeof(string));
                        exp = Expression.Call(propertyExp3, method3, someValue3);
                        break;
                }
            }

            return exp;
        }


        /// <summary>
        /// Get a lambda expression from two or more rules
        /// </summary>
        private Expression GetCompositeRule(Expression first, List<Rule> rules, ParameterExpression pe)
        {
            Rule rule = rules[0];
            rules.Remove(rule);
            Expression e2 = GetExpression(rules, pe);

            if (rule.Compare.Equals("or"))
            {
                return Expression.OrElse(first, e2);
            }
            else
            {
                if (e2 != null)
                {
                    return Expression.AndAlso(first, e2);
                }
                else
                {
                    throw new Exception("invalid rule, missing predicate.");
                }
            }

        }

        private List<StoreLookupModel> Example(List<Rule> rules)
        {
            List<StoreLookupModel> list = new List<StoreLookupModel>();


            // Add a using directive for System.Linq.Expressions. 

            string[] companies = { "Consolidated Messenger", "Alpine Ski House", "Southridge Video", "City Power & Light",
                               "Coho Winery", "Wide World Importers", "Graphic Design Institute", "Adventure Works",
                               "Humongous Insurance", "Woodgrove Bank", "Margie's Travel", "Northwind Traders",
                               "Blue Yonder Airlines", "Trey Research", "The Phone Company",
                               "Wingtip Toys", "Lucerne Publishing", "Fourth Coffee" };

            // The IQueryable data to query.
            IQueryable<String> queryableData = companies.AsQueryable<string>();

            //TODO, parse the rules into Linq syntax
            // Compose the expression tree that represents the parameter to the predicate.
            ParameterExpression pe = Expression.Parameter(typeof(string), "company");

            // ***** Where(company => (company.ToLower() == "coho winery" || company.Length > 16)) *****
            // Create an expression tree that represents the expression 'company.ToLower() == "coho winery"'.
            Expression left = Expression.Call(pe, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
            Expression right = Expression.Constant("coho winery");
            Expression e1 = Expression.Equal(left, right);

            // Create an expression tree that represents the expression 'company.Length > 16'.
            left = Expression.Property(pe, typeof(string).GetProperty("Length"));
            right = Expression.Constant(16, typeof(int));
            Expression e2 = Expression.GreaterThan(left, right);

            // Combine the expression trees to create an expression tree that represents the 
            // expression '(company.ToLower() == "coho winery" || company.Length > 16)'.
            Expression predicateBody = Expression.OrElse(e1, e2);

            // Create an expression tree that represents the expression 
            // 'queryableData.Where(company => (company.ToLower() == "coho winery" || company.Length > 16))'
            MethodCallExpression whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { queryableData.ElementType },
                queryableData.Expression,
                Expression.Lambda<Func<string, bool>>(predicateBody, new ParameterExpression[] { pe }));
            // ***** End Where ***** 

            // ***** OrderBy(company => company) ***** 
            // Create an expression tree that represents the expression 
            // 'whereCallExpression.OrderBy(company => company)'
            MethodCallExpression orderByCallExpression = Expression.Call(
                typeof(Queryable),
                "OrderBy",
                new Type[] { queryableData.ElementType, queryableData.ElementType },
                whereCallExpression,
                Expression.Lambda<Func<string, string>>(pe, new ParameterExpression[] { pe }));
            // ***** End OrderBy ***** 

            // Create an executable query from the expression tree.
            IQueryable<string> results = queryableData.Provider.CreateQuery<string>(orderByCallExpression);

            // Enumerate the results. 
            foreach (string company in results)
                Console.WriteLine(company);


            return list;
        }

        #endregion

        #region OrderPlanningRequest
        public ActionResult CreateOrderPlanningRequest(long planID)
        {
            RangePlan rangePlan = rangePlanDAO.GetRangePlan(planID);
            int instanceID = configService.GetInstance(rangePlan.Division);
            DateTime start = configService.GetControlDate(instanceID);

            OrderPlanningRequest model = new OrderPlanningRequest()
            {                
                PlanID = planID,
                StartSend = start,
                EndSend = start.AddDays(12)
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult CreateOrderPlanningRequest(OrderPlanningRequest model)
        {
            string message = ValidateOrderPlanningRequest(model, false);
            if (message != null)
            {
                ViewData["message"] = message;
                return View(model);
            }
            db.OrderPlanningRequests.Add(model);
            db.SaveChanges(currentUser.NetworkID);
            db.UpdateRangePlanDate(model.PlanID, currentUser.NetworkID);

            return RedirectToAction("PresentationQuantities", new { planID = model.PlanID });
        }

        private string ValidateOrderPlanningRequest(OrderPlanningRequest model, bool edit)
        {
            RangePlan rangePlan = rangePlanDAO.GetRangePlan(model.PlanID);
            int instanceID = configService.GetInstance(rangePlan.Division);
            DateTime start = configService.GetControlDate(instanceID);

            if (model.StartSend < start)
            {
                if (!currentUser.HasUserRole(AppName, "IT"))
                {
                    if (!edit)                    
                        return "Earliest start date is " + start;                    
                }
            }

            return null;
        }

        public ActionResult DeleteOrderPlanningRequest(long planID)
        {
            OrderPlanningRequest model = db.OrderPlanningRequests.Where(opr => opr.PlanID == planID).First();
            db.OrderPlanningRequests.Remove(model);
            db.SaveChanges(currentUser.NetworkID);
            
            rangePlanDAO.UpdateRangePlanDate(planID, currentUser.NetworkID);
            return RedirectToAction("PresentationQuantities", new { planID = model.PlanID });
        }

        public ActionResult EditOrderPlanningRequest(long planID)
        {
            OrderPlanningRequest model = db.OrderPlanningRequests.Where(opr => opr.PlanID == planID).First();
            return View(model);
        }

        [HttpPost]
        public ActionResult EditOrderPlanningRequest(OrderPlanningRequest model)
        {
            string message = ValidateOrderPlanningRequest(model, true);
            if (message != null)
            {
                ViewData["message"] = message;
                return View(model);
            }

            db.Entry(model).State = System.Data.EntityState.Modified;
            db.SaveChanges(currentUser.NetworkID);
            rangePlanDAO.UpdateRangePlanDate(model.PlanID, currentUser.NetworkID);

            return RedirectToAction("PresentationQuantities", new { planID = model.PlanID });
        }
        #endregion

        #region ALR Request
        public ActionResult StartALR(long planID)
        {
            RangePlan rp = (from a in db.RangePlans where a.Id == planID select a).First();
            rp.ALRStartDate = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == rp.Division select a.RunDate).First().AddDays(1);
            rp.UpdateDate = DateTime.Now;
            rp.UpdatedBy = UserName;
            db.Entry(rp).State = System.Data.EntityState.Modified;

            List<DeliveryGroup> deliveryGroups = db.DeliveryGroups.Where(dg => dg.PlanID == planID).ToList();
            foreach (DeliveryGroup dg in deliveryGroups)
            {
                if ((dg.ALRStartDate == null) || (dg.ALRStartDate > rp.ALRStartDate))
                {
                    dg.ALRStartDate = rp.ALRStartDate;
                    db.Entry(dg).State = System.Data.EntityState.Modified;
                }
            }

            db.SaveChanges(UserName);
            return RedirectToAction("PresentationQuantities", new { planID });
        }

        public ActionResult StopALR(long planID)
        {
            RangePlan rp = rangePlanDAO.GetRangePlan(planID); 
            rp.ALRStartDate = null;
            rp.UpdateDate = DateTime.Now;
            rp.UpdatedBy = currentUser.NetworkID;

            db.Entry(rp).State = System.Data.EntityState.Modified;
            List<DeliveryGroup> deliveryGroups = db.DeliveryGroups.Where(dg => dg.PlanID == planID).ToList();
            foreach (DeliveryGroup dg in deliveryGroups)
            {
                dg.ALRStartDate = null;
                db.Entry(dg).State = System.Data.EntityState.Modified;
            }

            db.SaveChanges(currentUser.NetworkID);
            return RedirectToAction("PresentationQuantities", new { planID });
        }

        public ActionResult StartDeliveryGroup(Int64 deliveryGroupID, Int64 planID)
        {
            DeliveryGroup dg = (from a in db.DeliveryGroups where a.ID == deliveryGroupID select a).First();
            RangePlan rp = (from a in db.RangePlans where a.Id == planID select a).First();
            dg.ALRStartDate = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == rp.Division select a.RunDate).First().AddDays(1);
            db.Entry(dg).State = System.Data.EntityState.Modified;

            var query = (from a in db.DeliveryGroups where ((a.PlanID == planID) && (a.ID != deliveryGroupID) && (a.ALRStartDate == null)) select a);
            if (query.Count() == 0)
            {
                rp.ALRStartDate = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == rp.Division select a.RunDate).First().AddDays(1);
                rp.UpdateDate = DateTime.Now;
                rp.UpdatedBy = currentUser.NetworkID;

                db.Entry(rp).State = System.Data.EntityState.Modified;
            }
            db.SaveChanges(currentUser.NetworkID);
            db.UpdateRangePlanDate(planID, currentUser.NetworkID);

            return RedirectToAction("PresentationQuantities", new { planID = planID });
        }

        public ActionResult StopDeliveryGroup(Int64 deliveryGroupID, Int64 planID)
        {
            DeliveryGroup dg = (from a in db.DeliveryGroups where a.ID == deliveryGroupID select a).First();
            RangePlan rp = (from a in db.RangePlans where a.Id == planID select a).First();
            dg.ALRStartDate = null;
            db.Entry(dg).State = System.Data.EntityState.Modified;

            rp.ALRStartDate = null;
            rp.UpdateDate = DateTime.Now;
            rp.UpdatedBy = currentUser.NetworkID;

            db.Entry(rp).State = System.Data.EntityState.Modified;
            db.SaveChanges(currentUser.NetworkID);
            db.UpdateRangePlanDate(planID, currentUser.NetworkID);

            return RedirectToAction("PresentationQuantities", new { planID = planID });
        }

        #endregion

        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult UploadRange()
        {
            return View();
        }

        [CheckPermission(Roles = "Ecomm PreSale")]
        public ActionResult PreSale()
        {
            List<PreSaleModel> model = (from preSale in db.PreSaleSKUs
                                        join item in db.ItemMasters on preSale.ItemID equals item.ID
                                        where preSale.Active == true
                                        select new PreSaleModel
                                        {
                                            SKU = item.MerchantSku,
                                            SKUDescription = item.Description,
                                            preSaleSKU = preSale
                                        }).ToList();

            foreach (PreSaleModel m in model)
            {
                if (m.preSaleSKU.LastModifiedUser.Contains("CORP"))
                    m.preSaleSKU.LastModifiedUser = currentUser.NetworkID;
            }

            return View(model);
        }

        public ActionResult CreatePreSale()
        {
            PreSaleModel model = new PreSaleModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult CreatePreSale(PreSaleModel model)
        {
            if (!WebSecurityService.UserHasDivisionRole(UserName, "allocation", model.SKU.Substring(0, 2), "Ecomm Presale"))
            {
                ViewData["message"] = "You do not have security to create PreSale records for this division.";
                return View(model);
            }

            PreSaleSKU preSale = new PreSaleSKU();

            preSale.LastModifiedDate = DateTime.Now;
            preSale.LastModifiedUser = currentUser.NetworkID;

            preSale.ItemID = ValidatePreSaleSKU(model.SKU);
            preSale.InventoryArrivalDate = model.preSaleSKU.InventoryArrivalDate;
            preSale.Active = true;

            if (preSale.ItemID == 0)
            {
                ViewData["message"] = "SKU does not exists.";
                return View(model);
            }
            else if (preSale.InventoryArrivalDate >= DateTime.Now.AddDays(30))
            {
                ViewData["message"] = "Inventory Arrival Date cannot be farther than 30 days.";
                return View(model);
            }

            long preSaleItemID = (from a in db.PreSaleSKUs
                                  where a.ItemID == preSale.ItemID && a.Active == true
                                  select a.ItemID).FirstOrDefault();

            if (preSaleItemID > 0)
            {
                ViewData["message"] = "Active Presale already exists for the SKU.";
                return View(model);
            }

            db.PreSaleSKUs.Add(preSale);
            db.SaveChanges();

            return RedirectToAction("PreSale");
        }

        private long ValidatePreSaleSKU(string SKU)
        {
            return (from a in db.ItemMasters
                    where a.MerchantSku == SKU
                    select a.ID).FirstOrDefault();
        }

        public ActionResult EditPreSale(int PreSaleSkuID)
        {
            PreSaleModel model = (from preSale in db.PreSaleSKUs
                                  join item in db.ItemMasters on preSale.ItemID equals item.ID
                                  where preSale.PreSaleSkuID == PreSaleSkuID
                                  select new PreSaleModel
                                  {
                                      SKU = item.MerchantSku,
                                      SKUDescription = item.Description,
                                      preSaleSKU = preSale
                                  }).FirstOrDefault();

            return View(model);
        }

        [HttpPost]
        public ActionResult EditPreSale(PreSaleModel model)
        {
            var record = (from a in db.PreSaleSKUs where a.PreSaleSkuID == model.preSaleSKU.PreSaleSkuID select a).FirstOrDefault();

            record.LastModifiedUser = currentUser.NetworkID;
            record.LastModifiedDate = DateTime.Now;
            record.InventoryArrivalDate = model.preSaleSKU.InventoryArrivalDate;

            if (record.InventoryArrivalDate >= DateTime.Now.AddDays(30))
            {
                ViewData["message"] = "Inventory Arrival Date cannot be farther than 30 days.";
                return View(model);
            }

            db.SaveChanges();
            return RedirectToAction("PreSale");
        }

        public ActionResult DeletePreSale(int PreSaleSkuID)
        {
            var record = (from a in db.PreSaleSKUs
                          where a.PreSaleSkuID == PreSaleSkuID
                          select a).FirstOrDefault();

            record.LastModifiedUser = currentUser.NetworkID;
            record.LastModifiedDate = DateTime.Now;
            record.Active = false;

            db.SaveChanges();
            return RedirectToAction("PreSale");
        }

        public ActionResult ReInitializeSKU()
        {
            List<ReInitializeSKUModel> model = new List<ReInitializeSKUModel>();
            return View(model);
        }

        [GridAction]
        public ActionResult _ReInitializeSKU(bool allSKU)
        {
            List<ReInitializeSKUModel> reInitializeSKU = new List<ReInitializeSKUModel>();
            if (allSKU)
            {
                reInitializeSKU = (from a in db.ReInitializeSKUs
                                   join b in db.ItemMasters on a.ItemID equals b.ID
                                   orderby b.MerchantSku, a.LastModifiedDate descending
                                   select new ReInitializeSKUModel
                                   {
                                       SKU = b.MerchantSku,
                                       SKUDescription = b.Description,
                                       reInitializeSKU = a
                                   }).Distinct().ToList();
            }
            else
            {
                reInitializeSKU = (from a in db.ReInitializeSKUs
                                   join b in db.ItemMasters on a.ItemID equals b.ID
                                   where a.SkuExtracted == false
                                   orderby b.MerchantSku, a.LastModifiedDate descending
                                   select new ReInitializeSKUModel
                                   {
                                       SKU = b.MerchantSku,
                                       SKUDescription = b.Description,
                                       reInitializeSKU = a
                                   }).Distinct().ToList();
            }

            foreach (ReInitializeSKUModel m in reInitializeSKU)
            {
                if (m.reInitializeSKU.CreateUser.Contains("CORP"))
                    m.reInitializeSKU.CreateUser = getFullUserNameFromDatabase(m.reInitializeSKU.CreateUser.Replace('\\', '/'));                
            }

            return View(new GridModel(reInitializeSKU));
        }

        public ActionResult DeleteSKUDetails(string id)
        {
            int cID = int.Parse(id);
            var skuDetail = (from a in db.ReInitializeSKUs
                             where a.ReInitializeSkuID == cID
                             select a).FirstOrDefault();

            long itemID = skuDetail.ItemID;

            db.ReInitializeSKUs.Remove(skuDetail);
            db.SaveChanges();

            return RedirectToAction("ReInitializeSKU");
        }

        public ActionResult CreateReInitializeSKU()
        {
            ReInitializeSKUModel model = new ReInitializeSKUModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateReInitializeSKU(ReInitializeSKUModel model)
        {
            string errorMessage = AddReinitializedSKU(model.SKU, currentUser);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                ViewData["message"] = errorMessage;
                return View(model);
            }

            return RedirectToAction("ReInitializeSKU");
        }

        public string AddReinitializedSKU(string sku, WebUser webUser)
        {
            string errorMessage = string.Empty;
            ReInitializeSKU reInitialize = new ReInitializeSKU()
            {
                LastModifiedDate = DateTime.Now,
                LastModifiedUser = webUser.NetworkID,
                CreateDate = DateTime.Now,
                CreateUser = webUser.NetworkID,
                SkuExtracted = false,
                ItemID = itemDAO.GetItemID(sku)
            };

            if (reInitialize.ItemID == 0)            
                errorMessage = "SKU does not exist.";
            else
            {
                long reInitSkuID = (from a in db.ReInitializeSKUs
                                    where a.ItemID == reInitialize.ItemID && a.SkuExtracted == false
                                    select a.ItemID).FirstOrDefault();

                if (reInitSkuID > 0)
                    errorMessage = "SKU is already pending to be extracted";
                else
                {
                    db.ReInitializeSKUs.Add(reInitialize);
                    db.SaveChanges();
                }
            }

            return errorMessage;
        }

        #region SKU Range Upload
        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult BulkSave(IEnumerable<HttpPostedFileBase> attachments)
        {
            return BulkSaveRange(attachments, false);
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult BulkSaveWithPurge(IEnumerable<HttpPostedFileBase> attachments2)
        {
            return BulkSaveRange(attachments2, true);
        }

        private ActionResult BulkSaveRange(IEnumerable<HttpPostedFileBase> attachments, bool purgeFirst)
        {
            SkuRangeSpreadsheet skuRangeSpreadsheet = new SkuRangeSpreadsheet(appConfig, configService);

            Session["errorList"] = null;
            try
            {
                foreach (HttpPostedFileBase file in attachments)
                {
                    skuRangeSpreadsheet.Save(file, purgeFirst);
                    if (!string.IsNullOrEmpty(skuRangeSpreadsheet.message))
                        return Content(skuRangeSpreadsheet.message);
                    else
                    {
                        if (skuRangeSpreadsheet.errorList.Count > 0)
                        {
                            Session["errorList"] = skuRangeSpreadsheet.errorList;
                            return Content(string.Format("{0} errors ({1} records on sheet)", skuRangeSpreadsheet.errorList.Count.ToString(), 
                                skuRangeSpreadsheet.parsedRanges.Count.ToString()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FLLogger log = new FLLogger();
                log.Log(ex.Message, FLLogger.eLogMessageType.eError);
                log.Log(ex.StackTrace, FLLogger.eLogMessageType.eError);

                return Content(ex.Message);
            }

            return Content("");
        }

        public ActionResult DownloadRangeErrors()
        {
            SkuRangeSpreadsheet skuRangeSpreadsheet = new SkuRangeSpreadsheet(appConfig, configService);
            Excel excelDocument;

            List<BulkRange> errorList = new List<BulkRange>();

            if (Session["errorList"] != null)            
                errorList = (List<BulkRange>)Session["errorList"];            

            excelDocument = skuRangeSpreadsheet.GetErrors(errorList);           
            excelDocument.Save("RangeUploadErrors.xls", Aspose.Excel.SaveType.OpenInExcel, Aspose.Excel.FileFormatType.Default, System.Web.HttpContext.Current.Response);

            return View();
        }

        public ActionResult ExcelRangeTemplate()
        {
            Excel excelDocument;
            SkuRangeSpreadsheet rangeSpreadsheet = new SkuRangeSpreadsheet(appConfig, configService);

            excelDocument = rangeSpreadsheet.GetTemplate();
            excelDocument.Save("RangeUpload.xls", Aspose.Excel.SaveType.OpenInExcel, Aspose.Excel.FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }
        #endregion

        public ActionResult ExcelRange(string sku)
        {
            SKURangeExport exportRange = new SKURangeExport(appConfig, new RangePlanDetailDAO());
            exportRange.WriteData(sku);
            exportRange.excelDocument.Save("RangeUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

        [CheckPermission(Roles = "Merchandiser, Head Merchandiser, Buyer Planner, Director of Allocation, Admin, Support")]
        public ActionResult SkuRPDGUpload()
        {
            return View();
        }

        public ActionResult ExcelDeliveryGroup(int deliveryGroupID)
        {
            SKURangeDeliveryGroupExport exportRange = new SKURangeDeliveryGroupExport(appConfig, new RangePlanDetailDAO());
            exportRange.WriteData(deliveryGroupID);
            exportRange.excelDocument.Save(string.Format("{0}-{1}.xls", exportRange.SKU, exportRange.DGName), SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);

            return View();
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult SaveSkuRPDG(IEnumerable<HttpPostedFileBase> attachments)
        {
            SKURangePlanDGSpreadsheet skuRangePlanDGSpreadsheet = new SKURangePlanDGSpreadsheet(appConfig, configService, rangePlanDAO);

            foreach (HttpPostedFileBase file in attachments)
            {
                skuRangePlanDGSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(skuRangePlanDGSpreadsheet.message))
                    return Content(skuRangePlanDGSpreadsheet.message);
                else
                {
                    if (skuRangePlanDGSpreadsheet.errorList.Count > 0)
                    {
                        Session["errorList"] = skuRangePlanDGSpreadsheet.errorList;
                        return Content(skuRangePlanDGSpreadsheet.errorMessage);
                    }
                }
            }

            string message = string.Format("Success! {0} lines were processed.", skuRangePlanDGSpreadsheet.parsedDeliveryGroups.Count.ToString());
            return Json(new { successMessage = message }, "application/json");
        }

        [CheckPermission(Roles = "Merchandiser, Head Merchandiser, Buyer Planner, Director of Allocation, Admin, Support")]
        public ActionResult ExcelSkuRangePlanDGUploadTemplate()
        {
            SKURangePlanDGSpreadsheet skuRangePlanDGSpreadsheet = new SKURangePlanDGSpreadsheet(appConfig, configService, rangePlanDAO);
            Excel excelDocument;

            excelDocument = skuRangePlanDGSpreadsheet.GetTemplate();
            excelDocument.Save("SkuRangePlanDGUploadTemplate.xls", Aspose.Excel.SaveType.OpenInExcel, Aspose.Excel.FileFormatType.Default, System.Web.HttpContext.Current.Response);

            return View();
        }
    }
}