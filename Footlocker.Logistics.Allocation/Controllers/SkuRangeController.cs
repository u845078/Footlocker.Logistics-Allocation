using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace Footlocker.Logistics.Allocation.Controllers
{

    [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Director of Allocation,Admin,Support")]
    public class SkuRangeController : AppController
    {
        public const int _DEFAULT_MAX_LEADTIME = 5;
        //
        // GET: /SkuRange/
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        private Boolean CheckSession(long planID)
        {
            try
            {
                return ((long)Session["pqPlan"] == planID);
            }
            catch
            {
                return false;
            }
        }

        public ActionResult SessionError(long planID)
        {
            ClearSessionVariables();

            return RedirectToAction("PresentationQuantities", new { planID = planID, message = "Session expired.  Please retry.  " });
        }

        private void ClearSessionVariables()
        {
            Session["pqPlan"] = null;
            Session["pqAllocs"] = null;
            Session["pqDeliveryGroups"] = null;
        }
        private void UpdateRangePlanDate(long planID)
        {
            db.UpdateRangePlanDate(planID, User.Identity.Name);
        }

        #region ActionResults

        #region "Sku Range Plan (list of sku's)

        public ActionResult Index(string message)
        {
            ViewData["message"] = message;
            List<RangePlan> model;
            if (Session["SkuSetup"] == null)
            {
                model = GetRangesForUser();
                Session["SkuSetup"] = model;
            }
            else
            {
                model = (List<RangePlan>)Session["SkuSetup"];
            }

            return View(model);
        }

        public ActionResult Refresh()
        {
            Session["SkuSetup"] = null;
            return RedirectToAction("Index");
        }

        private List<RangePlan> GetRangesForUser()
        {
            List<string> divs = (from a in Divisions() select a.DivCode).ToList();
            List<string> temp = new List<String>();

            foreach (string div in divs)
            {
                temp.AddRange((from a in WebSecurityService.ListUserDepartments(UserName, "Allocation", div) select div + '-' + a.DeptNumber).ToList());
            }

            List<RangePlan> model = db.RangePlans.Include("ItemMaster").Where(u => temp.Contains(u.Sku.Substring(0, 5))).OrderBy(a => a.Sku).ToList();
            return model;
        }

        //[GridAction]
        //public ActionResult _Index()
        //{
        //    List<Division> divs = Divisions();
        //    List<RangePlan> list = (from a in db.RangePlans select a).ToList();

        //    List<Department> temp = new List<Department>();

        //    foreach (Division div in divs)
        //    {
        //        temp.AddRange(WebSecurityService.ListUserDepartments(UserName, "Allocation", div.DivCode));
        //    }
        //    list = (from a in list
        //            join d in divs on a.Division equals d.DivCode
        //            join dp in temp on (a.Division + a.Department) equals (dp.DivCode + dp.DeptNumber)
        //            select a).ToList();

        //    List<RangePlanModel> model = new List<RangePlanModel>();
        //    foreach (RangePlan p in list)
        //    {
        //        model.Add(new RangePlanModel(p));
        //    }

        //    return View(new GridModel(model));
        //}

        public ActionResult Manage()
        {
            List<string> divs = (from a in Divisions() select a.DivCode).ToList();

            List<string> temp = new List<String>();

            foreach (string div in divs)
            {
                temp.AddRange((from a in WebSecurityService.ListUserDepartments(UserName, "Allocation", div) select div + '-' + a.DeptNumber).ToList());
            }

            List<RangePlan> model = db.RangePlans.Include("ItemMaster").Where(u => temp.Contains(u.Sku.Substring(0, 5))).ToList();

            return View(model);
        }

        //[GridAction]
        //public ActionResult _Manage()
        //{
        //    List<string> divs = (from a in Divisions() select a.DivCode).ToList();

        //    List<string> temp = new List<String>();

        //    foreach (string div in divs)
        //    {
        //        temp.AddRange((from a in WebSecurityService.ListUserDepartments(UserName, "Allocation", div) select div + '-' + a.DeptNumber).ToList());
        //    }

        //    List<RangePlan> model = db.RangePlans.Where(u => temp.Contains(u.Sku.Substring(0, 5))).ToList();

        //    return View(new GridModel(model));
        //}

        public ActionResult Delete(Int64 planID)
        {
            var rangeQuery = (from a in db.RangePlans where a.Id == planID select a);
            if (rangeQuery.Count() == 0)
            {
                return RedirectToAction("Index", new { message = "Range no longer exists." });
            }
            RangePlan plan = rangeQuery.First();
            db.RangePlans.Remove(plan);
            db.SaveChanges();
            Session["SkuSetup"] = null;
            return RedirectToAction("Index");
            //return Json("Success");
        }

        public ActionResult DeleteConfirm(Int64 planID)
        {
            var rangeQuery = (from a in db.RangePlans where a.Id == planID select a);
            if (rangeQuery.Count() == 0)
            {
                return RedirectToAction("Index", new { message = "Range no longer exists." });
            }
            RangePlan plan = rangeQuery.First();
            return View(plan);
            //return Json("Success");
        }

        public ActionResult CreateRangePlan()
        {
            RangePlan p = new RangePlan();
            return View(p);
        }

        private string ValidateSKU(string SKU)
        {
            string result = "";

            Regex regexSku = new Regex(@"^\d{2}-\d{2}-\d{5}-\d{2}$");
            if (!(regexSku.IsMatch(SKU)))
            {
                result = "Invalid Sku, format should be ##-##-#####-##";
                return result;
            }

            if (db.RangePlans.Any(a => a.Sku == SKU))
            {
                result = "Range Plan Already Exists for this Sku";
                return result;
            }

            if (!(WebSecurityService.UserHasDepartment(UserName, "Allocation", SKU.Substring(0, 2), SKU.Substring(3, 2))))
            {
                result = "You do not have permission for this division/department.";
                return result;
            }

            List<Division> divs = Divisions();

            if ((from d in divs
                 where d.DivCode == SKU.Substring(0, 2)
                 select d).Count() == 0)
            {
                result = "You do not have permission to create a range plan for this division";
                return result;
            }

            return result;
        }

        private long RetreiveOrCreateItemID(string SKU)
        {
            var itemlist = (from a in db.ItemMasters
                            where a.MerchantSku == SKU
                            select a);

            if (itemlist.Count() > 0)
            {
                return itemlist.First().ID;
            }
            else
            {
                Footlocker.Logistics.Allocation.Services.ItemDAO dao = new ItemDAO();
                string div = SKU.Substring(0, 2);
                int instance = (from a in db.InstanceDivisions
                                where a.Division == div
                                select a.InstanceID).First();
                try
                {
                    dao.CreateItemMaster(SKU, instance);
                }
                catch (Exception ex)
                {                    
                    throw new Exception(ex.Message);                    
                }

                return (from a in db.ItemMasters
                        where a.MerchantSku == SKU
                        select a.ID).First();
            }
        }

        [HttpPost]
        public ActionResult CreateRangePlan(RangePlan p)
        {
            p.CreatedBy = User.Identity.Name;
            p.CreateDate = DateTime.Now;

            p.UpdatedBy = User.Identity.Name;
            p.UpdateDate = DateTime.Now;

            string validationMessage;

            validationMessage = ValidateSKU(p.Sku);

            if (!string.IsNullOrEmpty(validationMessage))
            {
                ViewData["message"] = validationMessage;
                return View(p);
            }

            try
            {
                p.ItemID = RetreiveOrCreateItemID(p.Sku);
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                return View(p);
            }

            db.RangePlans.Add(p);
            db.SaveChanges();
            //update ActiveARStatus since we added a new rangeplan
            ItemDAO itemDAO = new ItemDAO();
            itemDAO.UpdateActiveARStatus();
            Session["SkuSetup"] = null; 
            return RedirectToAction("EditStores", new { planID = p.Id });
        }

        public ActionResult CopyRangePlan()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CopyRangePlan(CopyRangePlanModel model)
        {
            RangePlan fromPlan = null;
            if (model.FromSku != "")
            {
                var fromQuery = (from a in db.RangePlans
                                 where a.Sku == model.FromSku
                                 select a);
                if (fromQuery.Count() == 0)
                {
                    model.Message = "From Sku is not ranged.";
                    return View(model);
                }
                fromPlan = fromQuery.First();
            }
            else if (model.FromDescription != "")
            {
                fromPlan = (from a in db.RangePlans
                            where a.Description == model.FromDescription
                            select a).First();
            }
            else
            {
                model.Message = "You must specify either a SKU or Store Range Description.";
                return View(model);
            }

            if (model.FromSku.Substring(0, 2) != model.ToSku.Substring(0, 2))
            {
                model.Message = "You can only copy from a sku in the same division.";
                return View(model);
            }

            //verify the sizes match on old/new sku
            List<String> ToSizes = (from a in db.Sizes
                                    where a.Sku == model.ToSku
                                    select a.Size).OrderBy(p => p).ToList();
            List<String> FromSizes = (from a in db.Sizes
                                      where a.Sku == model.FromSku
                                      select a.Size).OrderBy(p => p).ToList();

            if (ToSizes.Count != FromSizes.Count)
            {
                model.Message = "These skus have different sizes, cannot copy";
                return View(model);
            }

            for (int i = 0; i < ToSizes.Count; i++)
            {
                if (ToSizes[i] != FromSizes[i])
                {
                    model.Message = "These skus have different sizes, cannot copy";
                    return View(model);
                }
            }

            string validationMessage;

            validationMessage = ValidateSKU(model.ToSku);
            if (!string.IsNullOrEmpty(validationMessage))
            {
                model.Message = validationMessage;
                return View(model);
            }

            //create new skurange
            try
            {
                RangePlan newPlan = new RangePlan();
                newPlan.Sku = model.ToSku;
                newPlan.Description = model.ToDescription + "";
                if (newPlan.Description.Length == 0)
                {
                    newPlan.Description = model.ToSku;
                }
                newPlan.CreateDate = DateTime.Now;
                newPlan.CreatedBy = User.Identity.Name;
                newPlan.UpdateDate = DateTime.Now;
                newPlan.UpdatedBy = User.Identity.Name;
                newPlan.PlanType = fromPlan.PlanType;
                newPlan.StoreCount = 0;

                try
                {
                    newPlan.ItemID = RetreiveOrCreateItemID(newPlan.Sku);
                }
                catch (Exception ex)
                {
                    model.Message = ex.Message;
                    return View(model);
                }

                db.RangePlans.Add(newPlan);
                db.SaveChanges(UserName);

                List<long> OldRuleSets = new List<long>();
                List<long> NewRuleSets = new List<long>();
                //copy ruleset
                List<RuleSet> rsFromList = (from a in db.RuleSets
                                            where a.PlanID == fromPlan.Id
                                            select a).ToList();
                foreach (RuleSet rsFrom in rsFromList)
                {
                    RuleSet rs = new RuleSet();
                    rs.PlanID = newPlan.Id;
                    rs.Type = rsFrom.Type;
                    rs.CreatedBy = User.Identity.Name;
                    rs.CreateDate = DateTime.Now;
                    db.RuleSets.Add(rs);
                    db.SaveChanges();

                    OldRuleSets.Add(rsFrom.RuleSetID);
                    NewRuleSets.Add(rs.RuleSetID);

                    List<RuleSelectedStore> stores = (from a in db.RuleSelectedStores
                                                        where a.RuleSetID == rsFrom.RuleSetID
                                                        select a).ToList();
                    foreach (RuleSelectedStore storeFrom in stores)
                    {
                        RuleSelectedStore store = new RuleSelectedStore();
                        store.Division = storeFrom.Division;
                        store.Store = storeFrom.Store;
                        store.RuleSetID = rs.RuleSetID;
                        store.CreatedBy = User.Identity.Name;
                        store.CreateDate = DateTime.Now;
                        db.RuleSelectedStores.Add(store);
                    }
                    db.SaveChanges();

                    List<Rule> rules = (from a in db.Rules where a.RuleSetID == rsFrom.RuleSetID select a).ToList();
                    foreach (Rule fromRule in rules)
                    {
                        Rule rule = new Rule();
                        rule.RuleSetID = rs.RuleSetID;
                        rule.Field = fromRule.Field;
                        rule.Compare = fromRule.Compare;
                        rule.Value = fromRule.Value;
                        rule.Sort = fromRule.Sort;
                        db.Rules.Add(rule);
                    }
                    db.SaveChanges();
                }

                //copy deliverygroup
                List<DeliveryGroup> dgList = (from a in db.DeliveryGroups
                                                where a.PlanID == fromPlan.Id
                                                select a).ToList();
                foreach (DeliveryGroup dg in dgList)
                {
                    DeliveryGroup dgNew = new DeliveryGroup();
                    dgNew.PlanID = newPlan.Id;
                    dgNew.StoreCount = dg.StoreCount;
                    dgNew.StartDate = dg.StartDate;
                    dgNew.EndDate = dg.EndDate;
                    dgNew.Name = dg.Name;
                    dgNew.MinEnd = dg.MinEnd;
                    dgNew.RuleSetID = NewRuleSets[OldRuleSets.IndexOf(dg.RuleSetID)];
                    db.DeliveryGroups.Add(dgNew);
                }
                db.SaveChanges(UserName);

                //copy details
                List<RangePlanDetail> details = (from a in db.RangePlanDetails
                                                    where a.ID == fromPlan.Id
                                                    select a).ToList();
                foreach (RangePlanDetail fromDet in details)
                {
                    RangePlanDetail det = new RangePlanDetail();
                    det.ID = newPlan.Id;
                    det.CreateDate = DateTime.Now;
                    det.CreatedBy = User.Identity.Name;
                    det.Division = fromDet.Division;
                    det.EndDate = fromDet.EndDate;
                    det.StartDate = fromDet.StartDate;
                    det.Store = fromDet.Store;
                    db.RangePlanDetails.Add(det);
                }
                db.SaveChanges();

                //copy presentation qtys
                //TODO:  Verify size exists for new sku
                List<SizeObj> sizes = (from a in db.Sizes
                                        where a.Sku == newPlan.Sku
                                        select a).ToList();
                List<SizeAllocation> list = (from a in db.SizeAllocations
                                                where a.PlanID == fromPlan.Id
                                                select a).ToList();
                foreach (SizeAllocation saFrom in list)
                {
                    if ((from a in sizes
                            where a.Size == saFrom.Size
                            select a).Count() > 0)
                    {
                        SizeAllocation sa = new SizeAllocation();
                        sa.PlanID = newPlan.Id;
                        sa.Days = saFrom.Days;
                        sa.Division = saFrom.Division;
                        sa.EndDate = saFrom.EndDate;
                        sa.InitialDemand = saFrom.InitialDemand;
                        sa.Max = saFrom.Max;
                        sa.Min = saFrom.Min;
                        sa.Range = saFrom.Range;
                        sa.Size = saFrom.Size;
                        sa.StartDate = saFrom.StartDate;
                        sa.Store = saFrom.Store;
                        sa.MinEndDate = saFrom.MinEndDate;
                        db.SizeAllocations.Add(sa);
                    }
                }
                db.SaveChanges(UserName);
                Session["SkuSetup"] = null;
                return RedirectToAction("Index", new { message = "Copied from " + model.FromSku + " to " + model.ToSku });
            }
            catch (Exception ex)
            {
                //we are saving changes periodically to get parent ID's.  
                //maybe not the best, but we don't want to create the parent child relationship at this point
                //so if there is an error, we'll just delete the new range and show an error message.
                try
                {
                    RangePlan plan = (from a in db.RangePlans
                                      where a.Sku == model.ToSku
                                      select a).First();
                    db.RangePlans.Remove(plan);
                    db.SaveChanges();
                }
                catch
                { }
                model.Message = ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult _AutoCompleteSku(string text)
        {
            IQueryable<String> results;
            results = (from a in db.RangePlans where a.Sku.StartsWith(text) select a.Sku).Distinct();
            return new JsonResult { Data = results.ToList() };
        }

        [HttpPost]
        public ActionResult _AutoCompleteDescription(string text)
        {
            IQueryable<String> results;
            results = (from a in db.RangePlans where a.Description.StartsWith(text) select (a.Description + " (" + a.Sku + ")")).Distinct();
            return new JsonResult { Data = results.ToList() };
        }

        #endregion

        #region "Presentation Qty's"
        [HttpPost]
        public ActionResult SaveSkuRange(SizeAllocationModel model)
        {
            RangePlan p = (from a in db.RangePlans where a.Id == model.Plan.Id select a).First();
            p.StartDate = model.Plan.StartDate;
            p.EndDate = model.Plan.EndDate;
            p.UpdateDate = DateTime.Now;
            p.UpdatedBy = User.Identity.Name;
            if (ModelState.IsValid)
            {
                db.SaveChanges();
            }

            return RedirectToAction("PresentationQuantities", "SkuRange", new { planID = model.Plan.Id });
        }

        public ActionResult SaveTotalSizeAllocation(List<SizeAllocationTotal> list)
        {
            long planID = 1;
            if (list.Count() > 0)
            {
                planID = list.First().PlanID;
            }

            if (!(CheckSession(planID)))
            {
                return SessionError(planID);
            }


            SizeAllocationDAO dao = new SizeAllocationDAO();

            List<Rule> rules = db.GetRulesForPlan(planID, "SizeAlc");


            IEnumerable<StoreLookup> stores;

            try
            {
                RuleSet r = (from a in db.RuleSets
                             where a.PlanID == planID && 
                                   a.Type == "SizeAlc"
                             select a).First();
                //check spreadsheet upload
                stores = (new RuleDAO()).GetStoresInRuleSet(r.RuleSetID);
                if (stores.Count() == 0)
                {
                    stores = GetStoresForRules(rules, planID);
                }
            }
            catch
            {
                //invalid rules, so we pull all the stores in the plan
                stores = (from a in db.StoreLookups
                          join b in db.RangePlanDetails 
                          on new {a.Division, a.Store} equals new {b.Division, b.Store}
                          where b.ID == planID
                          select a).ToList();
            }
            List<SizeAllocation> allocs = (List<SizeAllocation>)Session["pqAllocs"];
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
                      select a).ToList();

            stores = (from a in stores
                      join b in allocs 
                      on new {a.Division, a.Store} equals new {b.Division, b.Store}
                      select a).Distinct().ToList();
            int processedCount = 0;
            
            List<SizeAllocationTotal> totals = (List<SizeAllocationTotal>)Session["totals"];
            foreach (SizeAllocationTotal t in list)
            {
                planID = t.PlanID;
                int count = (from a in totals
                             where a.Size == t.Size &&
                                   a.Min != t.Min
                             select a).Count();

                if (count > 0)
                {
                    //we have a change on UI, write update to db
                    processedCount++;
                    dao.SaveMin((SizeAllocation)t, stores);
                }

                count = (from a in totals
                         where a.Size == t.Size &&
                               a.Max != t.Max 
                         select a).Count();

                if (count > 0)
                {
                    //we have a change on UI, write update to db
                    processedCount++;
                    dao.SaveMax((SizeAllocation)t, stores);
                }

                count = (from a in totals
                         where a.Size == t.Size &&
                               a.InitialDemand != t.InitialDemand
                         select a).Count();

                if (count > 0)
                {
                    //we have a change on UI, write update to db
                    processedCount++;
                    dao.SaveInitialDemand((SizeAllocation)t, stores);
                }

                count = (from a in totals
                         where a.Size == t.Size &&
                             a.Range != t.Range
                         select a).Count();

                if (count > 0)
                {
                    //we have a change on UI, write update to db
                    processedCount++;
                    dao.SaveRangeFlag((SizeAllocation)t, stores);
                }

                count = (from a in totals
                         where a.Size == t.Size &&
                              a.MinEndDate != t.MinEndDate
                         select a).Count();

                if (count > 0)
                {
                    //we have a change on UI, write update to db
                    processedCount++;
                    dao.SaveMinEndDate((SizeAllocation)t, stores);
                }
            }

            if ((list[0].EndDate != null) || (list[0].RangeType != "N/A"))
            {
                processedCount++;
                return SaveTotalDates(list);
            }

            //if (list.Count() > 0)
            if (processedCount > 0)
            {
                UpdateRangePlanDate(planID);
            }

            Session["pqAllocs"] = null;
            return RedirectToAction("PresentationQuantities", "SkuRange", new { planID = planID });
        }

        public ActionResult SaveTotalDates(List<SizeAllocationTotal> list)
        {
            long planID = 1;
            if (list.Count() > 0)
            {
                planID = list.First().PlanID;
            }

            if (!(CheckSession(planID)))
            {
                return SessionError(planID);
            }


            SizeAllocationDAO dao = new SizeAllocationDAO();
            List<Rule> rules = db.GetRulesForPlan(planID, "SizeAlc");


            IEnumerable<StoreLookup> stores;

            try
            {
                RuleSet r = (from a in db.RuleSets where ((a.PlanID == planID) && (a.Type == "SizeAlc")) select a).First();
                stores = (new RuleDAO()).GetStoresInRuleSet(r.RuleSetID);

                stores = (new RuleDAO()).GetStoresInRuleSet(r.RuleSetID);
                if ((stores == null) || (stores.Count() == 0))
                {
                    stores = GetStoresForRules(rules, planID);
                }
            }
            catch
            {
                //invalid rules, so we pull all the stores in the plan
                stores = (from a in db.StoreLookups join b in db.RangePlanDetails on new { a.Division, a.Store } equals new { b.Division, b.Store } where b.ID == planID select a).ToList();
            }

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


            ////Now let's update the date of the rangeplan
            List<RangePlanDetail> detList = (from a in db.RangePlanDetails where a.ID == planID select a).ToList();


            //filter it for the stores based on current rules
            if (stores != null)
            {
                //reduce stores to only stores for selected delivery groups.
                stores = (from a in stores
                          join b in selectedStores on new { a.Division, a.Store } equals new { b.Division, b.Store }
                          select a).ToList();

                detList = (from a in stores
                           from b in detList
                           where ((a.Division == b.Division) && (a.Store == b.Store))
                           select b).ToList();
            }

            foreach (RangePlanDetail det in detList)
            {
                //det.StartDate = list.First().StartDate;
                if (list.First().EndDate != null)
                {
                    det.EndDate = list.First().EndDate;
                }
                if (list.First().RangeType != "N/A")
                {
                    det.RangeType = list.First().RangeType;
                }

                //always default to "Both"
                if (string.IsNullOrEmpty(det.RangeType))
                {
                    det.RangeType = "Both";
                }

                fixEndDate(det);
                db.Entry(det).State = System.Data.EntityState.Modified;
            }
            db.SaveChanges();

            if (list.Count() > 0)
            {
                UpdateRangeHeader(planID);
            }

            Session["pqAllocs"] = null;
            return RedirectToAction("PresentationQuantities", "SkuRange", new { planID = planID });
        }


        private void UpdateRangeHeader(long planID)
        {
            RangePlan p = (from a in db.RangePlans
                           where a.Id == planID
                           select a).First();

            try
            {
                p.StartDate = (from a in db.DeliveryGroups
                               where a.PlanID == planID
                               select a.StartDate).Min();
            }
            catch
            { }

            try
            {
                p.EndDate = (from a in db.RangePlanDetails
                             where a.ID == planID
                             select a.EndDate).Max();
            }
            catch
            { }
            p.UpdateDate = DateTime.Now;
            p.UpdatedBy = User.Identity.Name;
            db.SaveChanges(UserName);
        }


        public ActionResult ExcelSizeAllocation(Int64 planID)
        {
            SizeAllocationDAO dao = new SizeAllocationDAO();
            List<SizeAllocation> allocations = dao.GetSizeAllocationList(planID);


            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            //FileStream file = new FileStream(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SkuTypeTemplate"]), FileMode.Open);
            //Byte[] data1 = new Byte[file.Length];
            //file.Read(data1, 0, data1.Length);
            //file.Close();
            //MemoryStream memoryStream1 = new MemoryStream(data1);
            //excelDocument.Open(memoryStream1);
            Worksheet mySheet = excelDocument.Worksheets[0];

            mySheet.Cells[0, 0].PutValue("PlanID");
            mySheet.Cells[0, 0].Style.Font.IsBold = true;
            mySheet.Cells[0, 1].PutValue("Div");
            mySheet.Cells[0, 1].Style.Font.IsBold = true;
            mySheet.Cells[0, 2].PutValue("Store");
            mySheet.Cells[0, 2].Style.Font.IsBold = true;
            mySheet.Cells[0, 3].PutValue("Size");
            mySheet.Cells[0, 3].Style.Font.IsBold = true;
            mySheet.Cells[0, 4].PutValue("Min");
            mySheet.Cells[0, 4].Style.Font.IsBold = true;
            mySheet.Cells[0, 5].PutValue("Max");
            mySheet.Cells[0, 5].Style.Font.IsBold = true;
            mySheet.Cells[0, 6].PutValue("Days");
            mySheet.Cells[0, 6].Style.Font.IsBold = true;

            int row = 1;
            foreach (SizeAllocation sa in allocations)
            {
                mySheet.Cells[row, 0].PutValue(sa.PlanID);
                mySheet.Cells[row, 1].PutValue(sa.Division);
                mySheet.Cells[row, 2].PutValue(sa.Store);
                mySheet.Cells[row, 3].PutValue(sa.Size);
                mySheet.Cells[row, 4].PutValue(sa.Min);
                mySheet.Cells[row, 5].PutValue(sa.Max);
                mySheet.Cells[row, 6].PutValue(sa.Days);

                row++;
            }

            for (int i = 0; i < 8; i++)
            {
                mySheet.AutoFitColumn(i);
            }

            excelDocument.Save("SizeAllocation" + planID + ".xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }



        public ActionResult SaveStoreSizeAllocation(IList<SizeAllocation> list)
        {
            long planID = 1;
            SizeAllocationDAO dao = new SizeAllocationDAO();
            //List<Rule> rules = db.GetRulesForPlan(planID, "SizeAlc");

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

                if ((t.Store != currentStore)||(t.Division != currentDiv))
                {
                    //Now let's update the date of the rangeplan
                    RangePlanDetail det = (from a in db.RangePlanDetails
                                           where ((a.ID == planID) && 
                                                  (a.Division == t.Division) && 
                                                  (a.Store == t.Store))
                                           select a).First();
                    det.StartDate = t.StartDate;
                    det.EndDate = t.EndDate;
                    if (t.RangeType != "N\\A")
                    {
                        det.RangeType = t.RangeType;
                    }

                    //always default to "Both"
                    if (string.IsNullOrEmpty(det.RangeType))
                    {
                        det.RangeType = "Both";
                    }

                    fixEndDate(det);
                    db.Entry(det).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                }

                currentStore = t.Store;
                currentDiv = t.Division;
            }
            if (list.Count() > 0)
            {
                UpdateRangePlanDate(planID);
            }
            return RedirectToAction("PresentationQuantities", "SkuRange", new { planID = planID });
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

                    if ((t.Store != currentStore) || (t.Division != currentDiv))
                    {
                        //Now let's update the date of the rangeplan
                        RangePlanDetail det = (from a in db.RangePlanDetails
                                               where ((a.ID == planID) && 
                                                      (a.Division == t.Division) && 
                                                      (a.Store == t.Store))
                                               select a).First();
                        det.StartDate = t.StartDate;
                        det.EndDate = t.EndDate;
                        //if (t.RangeType != "N\\A")
                        //{
                        //    det.RangeType = t.RangeType;
                        //}

                        //always default to "Both"
                        //if (string.IsNullOrEmpty(det.RangeType))
                        //{
                            det.RangeType = "Both";
                        //}

                        fixEndDate(det);
                        db.Entry(det).State = System.Data.EntityState.Modified;
                        db.SaveChanges();
                    }
                    currentStore = t.Store;
                    currentDiv = t.Division;
                }
            }

            if (list.Count() > 0)
            {
                UpdateRangePlanDate(planID);

                Session["pqAllocs"] = null;
            }

            return Json("Success");
            //return RedirectToAction("PresentationQuantities", "SkuRange", new { planID = planID });
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
                    currentTotal = new SizeAllocationTotal();
                    currentTotal.Division = "NA";
                    currentTotal.Store = "NA";
                    currentTotal.PlanID = sa.PlanID;
                    currentTotal.Days = sa.Days;
                    currentTotal.Min = sa.Min;
                    currentTotal.Max = sa.Max;
                    currentTotal.ModifiedStore = false;
                    currentTotal.Size = sa.Size;
                    currentTotal.Range = sa.Range;
                    currentTotal.InitialDemand = sa.InitialDemand;
                    currentTotal.StartDate = sa.StartDate;
                    currentTotal.EndDate = sa.EndDate;
                    currentTotal.MinEndDate = sa.MinEndDate;
                }

                if ((currentTotal.Days != sa.Days) || 
                    (currentTotal.Min != sa.Min) || 
                    (currentTotal.Max != sa.Max) || 
                    (currentTotal.Range != sa.Range) || 
                    (currentTotal.InitialDemand != sa.InitialDemand) || 
                    (currentTotal.MinEndDate != sa.MinEndDate))
                {
                    //if ((sa.Days != null) && (sa.Min != null) && (sa.Max != null))
                    //{
                        currentTotal.ModifiedStore = true;
                    //}
                }
                if ((currentTotal.StartDate != sa.StartDate) || (currentTotal.EndDate != sa.EndDate))
                { 
                    //TODO, show on screen that all dates aren't the same
                    currentTotal.ModifiedDates = true;
                }
                //if ((currentTotal.Range != sa.Range))
                //{
                //    currentTotal.ModifiedStore = true;
                //}
                prevSize = sa.Size;
            }
            if (currentTotal != null)
            {
                AddTotal(totals, currentTotal);
            }

            //blank this out so UI can work
            if (totals.Count > 0)
            {
                totals[0].EndDate = null;
            }
            return totals;
        }

        private static void AddTotal(List<SizeAllocationTotal> totals, SizeAllocationTotal currentTotal)
        {
            if (currentTotal != null)
            {
                //if (currentTotal.Max < 0)
                //{
                //    currentTotal.Max = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["defaultMax"]);
                //}
                //if (currentTotal.Min < 0)
                //{
                //    currentTotal.Min = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["defaultMin"]);
                //}
                //if (currentTotal.Days < 0)
                //{
                //    currentTotal.Days = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["defaultDays"]);
                //}
                totals.Add(currentTotal);
            }
        }

        public ActionResult ShowQFeed(Int64 planID)
        {
            ViewData["planID"] = planID;
            Session["QFeed"] = null;
            Session["QFeedPlan"] = null;

            RangePlan rp = (from a in db.RangePlans where a.Id == planID select a).First();

            RangeFileItemDAO dao = new RangeFileItemDAO();

            var info = (from a in db.RangePlans join b in db.InstanceDivisions on a.Sku.Substring(0,2) equals b.Division where a.Id == planID select new { sku = a.Sku, instance = b.InstanceID }).First();
            
            dao.SetFirstReceiptDates(info.instance, info.sku);

            QFeedModel model = new QFeedModel();
            model.VerificationMessages = new List<string>();
            int storecount = (from a in db.RangePlanDetails join b in db.vValidStores on new { a.Division, a.Store } equals new { b.Division, b.Store } where a.ID == planID select a).Count();

            model.VerificationMessages.Add(storecount + " stores ranged.");
            model.VerificationMessages.Add("");

            ItemMaster item = (from a in db.ItemMasters where a.ID == rp.ItemID select a).First();
            model.Sku = item.MerchantSku;

            return View(model);
        }

        [GridAction]
        public ActionResult _ShowQFeed(Int64 planID)
        {
            List<RangeFileItem> model=null;
            if ((Session["QFeedPlan"] == null) || (Convert.ToInt64(Session["QFeedPlan"]) != planID))
            {
                RangePlan rp = (from a in db.RangePlans where a.Id == planID select a).First();

                RangeFileItemDAO dao = new RangeFileItemDAO();
                model = dao.GetRangeFileExtract(rp.Division, rp.Department, rp.Sku);
                Session["QFeed"] = model;
                Session["QFeedPlan"] = planID;
            }
            else
            {
                model = (List<RangeFileItem>)Session["QFeed"];
            }
            return View(new GridModel(model));
        }

        public ActionResult ResetQFeed(Int64 planID)
        {
            Session["QFeed"] = null;
            Session["QFeedPlan"] = null;
            return RedirectToAction("ShowQFeed", new { planID = planID });
        }

        public ActionResult ShowRDQIssues(Int64 planID)
        {
            ViewData["planID"] = planID;
            RangePlan rp = (from a in db.RangePlans where a.Id == planID select a).First();

            RangeFileItemDAO dao = new RangeFileItemDAO();

            QFeedModel model = new QFeedModel();
            model.VerificationMessages = new List<string>();
            int storecount = (from a in db.RangePlanDetails join b in db.vValidStores on new { a.Division, a.Store } equals new { b.Division, b.Store } where a.ID == planID select a).Count();

            model.VerificationMessages.Add(storecount + " stores ranged.");
            model.VerificationMessages.Add("");

            //Unit cost and Unit retail on QR_product – the margin needs to be positive or it will not generate positive utility
            ItemMaster item = (from a in db.ItemMasters where a.ID == rp.ItemID select a).First();
            model.Sku = item.MerchantSku;

            int instanceid = (from a in db.InstanceDivisions where a.Division == item.Div select a.InstanceID).First();
            model.Routes = (from a in db.Routes where a.InstanceID == instanceid select a).ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult _CheckPrice(string sku)
        {
            ItemMaster item = (from a in db.ItemMasters where a.MerchantSku == sku select a).First();

            Price price = (from a in db.Prices where ((a.Division == item.Div) && (a.Stock == item.MerchantSku.Substring(0, 11))) select a).First();
            if (price.Stock != null)
            {
                return new JsonResult() { Data = price, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else
            {
                return Json("error");
            }
        }

        [GridAction]
        public ActionResult _CheckRoute(int routeid, int planID)
        {
            //ItemMaster item = (from a in db.ItemMasters where a.MerchantSku == sku select a).First();

            //verify all these zones are in routes
            var zoneids = (from a in db.RangePlanDetails join b in db.NetworkZoneStores on new { a.Division, a.Store } equals new { b.Division, b.Store }
                           join c in db.NetworkZones on b.ZoneID equals c.ID
                           where a.ID == planID
                           select new { b.Division, b.Store, b.ZoneID, c.Name });

            Route route = (from a in db.Routes where a.ID == routeid select a).First();

            //2.  See if all zoneids are in that route
            var routeZones = (from a in db.Routes join b in db.RouteDetails on a.ID equals b.RouteID where a.ID == routeid select new { b.RouteID, b.ZoneID });

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
                issue.Message = "Zone [" + det.Name + "] not in route [" + route.DisplayString + "] for this product";
                issues.Add(issue);
            }


            return View(new GridModel(issues));

        }


        [GridAction]
        public ActionResult _IssueGrid(Int64 planID)
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
                (s.ID == planID)&&
                (!db.NetworkZoneStores.Any(es=>(es.Division==s.Division)&&(es.Store==s.Store)))
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

            ////verify all these zones are in routes
            //var zoneids = (from a in db.RangePlanDetails join b in db.NetworkZoneStores on new { a.Division, a.Store } equals new { b.Division, b.Store } where a.ID == planID select b);
            ////1.  Figure out what route it should be

            //Route route = (from a in db.Routes select a).First();
            ////2.  See if all zoneids are in that route
            //var routeZones = (from a in db.Routes join b in db.RouteDetails on a.ID equals b.RouteID where a.ID == route.ID select new { b.RouteID, b.ZoneID });

            //var missingZones = (from a in zoneids
            // where (!routeZones.Any(b => (b.ZoneID == a.ZoneID)))
            // select a);

            //foreach (var det in missingZones)
            //{
            //    issue = new RangeIssue();
            //    issue.Division = det.Division;
            //    issue.Store = det.Store;
            //    issue.Message = "Zone not in route for this product (" + route.Perspective + " " + route.Pass + " " + route.Name + ")";
            //    issues.Add(issue);
            //}

            return View(new GridModel(issues));
        }


        public ActionResult ShowQFeedText(Int64 planID)
        {
            RangePlan rp = (from a in db.RangePlans where a.Id == planID select a).First();

            RangeFileItemDAO dao = new RangeFileItemDAO();
            List<RangeFileItem> model = dao.GetRangeFileExtract(rp.Division, rp.Department, rp.Sku);

            string results = "";
            foreach (RangeFileItem item in model)
            {
                results += item.ToStringWithQuotes(',') + "\r\n";
            }

            return File(Encoding.UTF8.GetBytes(results),
                         "text/plain",
                          string.Format("{0}.txt", rp.Sku));
        }

        public ActionResult ShowQFeedTextFast(Int64 planID)
        {
            RangePlan rp = (from a in db.RangePlans where a.Id == planID select a).First();
            int instance = (from a in db.InstanceDivisions where a.Division == rp.Division select a.InstanceID).First();

            RangeFileItemDAO dao = new RangeFileItemDAO();
            System.Data.IDataReader reader = dao.GetRangeFileExtractDataReader(rp.Division, rp.Department, rp.Sku);

            RangeReformat reformat = new RangeReformat(instance);
            string results = "";
            while (reader.Read())
            {
                if (reader[11] as int? == 1) //is it ranged
                {
                    results += reformat.Format(reader, instance) + "\r\n";
                }
                else
                {
                    results += reformat.Format(reader, "N", instance) + "\r\n";
                }
            }

            return File(Encoding.UTF8.GetBytes(results),
                         "text/plain",
                          string.Format("{0}.csv", rp.Sku));
        }

        public ActionResult ClearFilteredStores(Int64 planID)
        {
            //delete all the ruleselected stores
            //foreach (RuleSelectedStore rss in (from a in db.RuleSelectedStores where a.RuleSetID == ruleSetID select a))
            //{
            //    db.RuleSelectedStores.Remove(rss);
            //}
            //db.SaveChanges(UserName);

            return RedirectToAction("PresentationQuantities", new { planID = planID});
        }

        public ActionResult ClearCache(Int64 planID)
        {
            ClearSessionVariables();

            return RedirectToAction("PresentationQuantities", new {planID = planID});
        }

        public ActionResult PresentationQuantities(Int64 planID, string message, string page, string show)
        {
            if ((Session["pqPlan"] == null) || ((Int64)Session["pqPlan"] != planID))
            {
                Session["pqPlan"] = planID;
                Session["pqAllocs"] = null;
                Session["pqDeliveryGroups"] = null;
            }
            SizeAllocationModel model = InitPresentationQtyModel(planID, message, page, show);
            GetPresentationQtyDeliveryGroups(model);

            model.OrderPlanningRequest = (from a in db.OrderPlanningRequests
                                          where a.PlanID == planID
                                          select a).FirstOrDefault();
            GetPresentationQtyModelDetails(model, show);

            return View(model);
        }

        public ActionResult SelectDeliveryGroup(Int64 DeliveryGroupID, Int64 planID)
        {
            if (!(CheckSession(planID)))
            {
                return SessionError(planID);
            }
            //SizeAllocationModel model = InitPresentationQtyModel(planID, null,null,null);
            //GetPresentationQtyDeliveryGroups(model);
            if (Session["selectedDeliveryGroups"] != null)
            {
                List<DeliveryGroup> groups = ((List<DeliveryGroup>)Session["selectedDeliveryGroups"]);
                DeliveryGroup updated = (from a in groups where a.ID == DeliveryGroupID select a).First();
                updated.Selected = !(updated.Selected);
                Session["selectedDeliveryGroups"] = groups;
            }
            return RedirectToAction("PresentationQuantities", new { planID = planID });
        }

        //public ActionResult PresentationQuantities(Int64 planID, string message, string page, string show)
        //{
        //    SizeAllocationModel model = GetPresentationQtyModel(planID, message, page, show);

        //    return View(model);
        //}

        private SizeAllocationModel GetPresentationQtyModel(Int64 planID, string message, string page, string show)
        {
            string ruleType = "SizeAlc";

            if ((Request.UserAgent.Contains("Chrome")) || (Request.UserAgent.Contains("Firefox")))
            {
                ViewData["Chrome"] = "true";
            }
            if ((message != null) && (message != ""))
            {
                ViewData["message"] = message;
            }
            ViewData["planID"] = planID;
            ViewData["ruleType"] = ruleType;
            ViewData["page"] = page;
            ItemMaster i = (from a in db.ItemMasters join b in db.RangePlans on a.ID equals b.ItemID where b.Id == planID select a).FirstOrDefault();
            if (i != null)
            {
                ViewData["LifeCycle"] = i.LifeCycleDays;
            }
            SizeAllocationModel model = new SizeAllocationModel();
            model.Plan = (from a in db.RangePlans where a.Id == planID select a).First();
            model.PlanID = planID;

            //update the store count
            model.Plan.StoreCount = (from a in db.RangePlanDetails join b in db.vValidStores on new { a.Division, a.Store } equals new { b.Division, b.Store } where a.ID == planID select a).Count();
            db.SaveChanges();

            #region ruleModel
            var existingRuleSet = (from a in db.RuleSets where (a.PlanID == planID) && (a.Type == ruleType) select a.RuleSetID);
            if (existingRuleSet.Count() > 0)
            {
                model.RuleSetID = existingRuleSet.First();
            }
            else
            {
                RuleSet rs = new RuleSet();
                rs.PlanID = model.PlanID;
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
            List<SizeAllocation> allocs = dao.GetSizeAllocationList(planID);

            if (model.Rules.Count() == 0)
            {
                //add a temp rule so we can show the and/etc.
                Rule r = new Rule();
                r.RuleSetID = model.RuleSetID;

                model.Rules = new List<Rule>();
                model.Rules.Add(r);
                model.RuleToAdd.RuleSetID = r.RuleSetID;

                List<StoreLookup> stores = (new RuleDAO()).GetStoresInRuleSet(r.RuleSetID);
                if (stores.Count > 0)
                {
                    //spreadsheet upload
                    model.StoreCount = (from a in stores join b in allocs on new { a.Store, a.Division } equals new { b.Store, b.Division } select a).Distinct().Count();
                    model.Allocations = (from a in allocs join b in stores on new { a.Store, a.Division } equals new { b.Store, b.Division } select a).ToList();
                }
                else
                {
                    model.StoreCount = model.Plan.StoreCount;
                    model.Allocations = allocs;
                }
            }
            else
            {
                model.RuleToAdd.RuleSetID = model.Rules[0].RuleSetID;
                try
                {
                    model.NewStores = GetStoresForRules(model.Rules, planID);
                    model.StoreCount = model.NewStores.Count();
                    model.StoreCount = (from a in model.NewStores join b in allocs on new { a.Store, a.Division } equals new { b.Store, b.Division } select a).Distinct().Count();
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                    model.NewStores = new List<StoreLookupModel>();
                }

                model.Allocations = (from a in allocs join b in model.NewStores on new { a.Store, a.Division } equals new { b.Store, b.Division } select a).ToList();
            }

            ViewData["show"] = show;
            if (show == "emptyStartDates")
            {
                model.Allocations = (from a in model.Allocations where (a.StartDate == null) select a).ToList();
            }

            model.RuleToAdd.Sort = model.Rules.Count() + 1;
            #endregion

            model.TotalAllocations = GetTotals(model.Allocations);

            model.DeliveryGroups = (from a in db.DeliveryGroups where a.PlanID == planID select a).ToList();
            InitializeDeliveryGroups(model);
            return model;
        }

        private void GetPresentationQtyModelDetails(SizeAllocationModel model, string show)
        {
            string ruleType = "SizeAlc";
            #region ruleModel

            var existingRuleSet = (from a in db.RuleSets
                                   where (a.PlanID == model.PlanID) && (a.Type == ruleType)
                                   select a.RuleSetID);
            if (existingRuleSet.Count() > 0)
            {
                model.RuleSetID = existingRuleSet.First();
            }
            else
            {
                RuleSet rs = new RuleSet();
                rs.PlanID = model.PlanID;
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

            if (Session["pqAllocs"] == null)
            {
                Session["pqAllocs"] = dao.GetSizeAllocationList(model.PlanID);
            }
            List<SizeAllocation> allocs = (List<SizeAllocation>)Session["pqAllocs"];
            //find stores in selected delivery groups
            List<DeliveryGroup> selected = ((List<DeliveryGroup>)Session["selectedDeliveryGroups"]);
            List<RuleSelectedStore> selectedStores = new List<RuleSelectedStore>();
            foreach (DeliveryGroup dg in selected)
            {
                if (dg.Selected)
                {
                    selectedStores.AddRange((from c in db.RuleSelectedStores
                                             join b in db.vValidStores 
                                             on new {c.Division, c.Store} equals new { b.Division, b.Store }
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
                    model.Allocations = (from a in allocs
                                         join b in stores 
                                         on new { a.Store, a.Division } equals new { b.Store, b.Division }
                                         select a).ToList();
                }
                else
                {
                    //model.StoreCount = model.Plan.StoreCount;
                    model.StoreCount = (from a in allocs
                                        select new { a.Division, a.Store }).Distinct().Count();
                    model.Allocations = allocs;
                }
            }
            else
            {
                model.RuleToAdd.RuleSetID = model.Rules[0].RuleSetID;
                try
                {
                    model.NewStores = GetStoresForRules(model.Rules, model.PlanID);
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

                model.Allocations = (from a in allocs
                                     join b in model.NewStores 
                                     on new { a.Store, a.Division } equals new { b.Store, b.Division }
                                     select a).ToList();
            }

            ViewData["show"] = show;
            if (show == "emptyStartDates")
            {
                model.Allocations = (from a in model.Allocations
                                     where (a.StartDate == null)
                                     select a).ToList();
            }

            model.RuleToAdd.Sort = model.Rules.Count() + 1;
            #endregion

            model.TotalAllocations = GetTotals(model.Allocations);
            Session["totals"] = model.TotalAllocations;
        }

        private void GetPresentationQtyDeliveryGroups(SizeAllocationModel model)
        {
            if (Session["pqDeliveryGroups"] == null)
            {
                Session["pqDeliveryGroups"] = (from a in db.DeliveryGroups
                                               where a.PlanID == model.PlanID
                                               select a).ToList();
                model.DeliveryGroups = (List<DeliveryGroup>)Session["pqDeliveryGroups"];
                InitializeDeliveryGroups(model);
                //check if we need to select all stores (first page access for this planid)
                Boolean resetSelected = true;
                if (Session["selectedDeliveryGroups"] != null)
                {
                    resetSelected = model.PlanID != ((List<DeliveryGroup>)Session["selectedDeliveryGroups"])[0].PlanID;
                }
                if (resetSelected)
                {
                    foreach (DeliveryGroup dg in model.DeliveryGroups)
                    {
                        dg.Selected = true;
                    }
                    Session["selectedDeliveryGroups"] = model.DeliveryGroups;
                }
            }
            else
            {
                model.DeliveryGroups = (List<DeliveryGroup>)Session["pqDeliveryGroups"];
            }

            //set the current models selected stores to those saved in the session as selected
            List<DeliveryGroup> groups = ((List<DeliveryGroup>)Session["selectedDeliveryGroups"]);
            foreach (DeliveryGroup dg in model.DeliveryGroups)
            {
                try
                {
                    dg.Selected = ((from a in groups
                                    where a.ID == dg.ID
                                    select a).First().Selected);
                }
                catch 
                {
                    dg.Selected = true;
                    groups.Add(dg);
                    Session["selectedDeliveryGroups"] = groups;
                }
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
        private SizeAllocationModel InitPresentationQtyModel(Int64 planID, string message, string page, string show)
        {
            string ruleType = "SizeAlc";

            if ((Request.UserAgent.Contains("Chrome")) || (Request.UserAgent.Contains("Firefox")))
            {
                ViewData["Chrome"] = "true";
            }
            if ((message != null) && (message != ""))
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
            SizeAllocationModel model = new SizeAllocationModel();
            model.Plan = (from a in db.RangePlans
                          where a.Id == planID
                          select a).First();
            model.PlanID = planID;

            //update the store count
            model.Plan.StoreCount = (from a in db.RangePlanDetails
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
                                      ad.Division == model.Plan.Division &&
                                      ad.Department == model.Plan.Department
                                select ad;

            if (instanceQuery.Count() > 0)
                model.Plan.OPDepartment = true;
            else
                model.Plan.OPDepartment = false;

            db.SaveChanges();

            return model;
        }

        private void InitializeDeliveryGroups(SizeAllocationModel model)
        {
            //if the plan doesn't have any, then let's create a default one with all stores
            if (model.DeliveryGroups.Count() == 0)
            {
                DeliveryGroup newGroup = new DeliveryGroup();
                newGroup.Name = "Delivery Group 1";
                newGroup.PlanID = model.PlanID;
                db.DeliveryGroups.Add(newGroup);
                db.SaveChanges();

                RuleSet rs = new RuleSet();
                rs.PlanID = model.PlanID;
                rs.Type = "Delivery";
                rs.CreateDate = DateTime.Now;
                rs.CreatedBy = User.Identity.Name;

                db.RuleSets.Add(rs);
                db.SaveChanges();

                newGroup.RuleSetID = rs.RuleSetID;
                
                List<RangePlanDetail> rangePlanDetails = (from a in db.RangePlanDetails
                                                          where a.ID == newGroup.PlanID
                                                          select a).ToList();
                foreach (RangePlanDetail det in rangePlanDetails)
                {
                    RuleSelectedStore newDet = new RuleSelectedStore();
                    newDet.RuleSetID = newGroup.RuleSetID;
                    newDet.Store = det.Store;
                    newDet.Division = det.Division;
                    newDet.CreateDate = DateTime.Now;
                    newDet.CreatedBy = User.Identity.Name;
                    db.RuleSelectedStores.Add(newDet);
                }
                db.SaveChanges(UserName);

                model.DeliveryGroups.Add(newGroup);
            }

            //update counts
            foreach (DeliveryGroup d in model.DeliveryGroups)
            {
                d.StoreCount = (from a in db.RuleSelectedStores 
                                join p in db.RangePlanDetails on new { a.Division, a.Store } equals new { p.Division, p.Store } 
                                join b in db.vValidStores on new { a.Division, a.Store } equals new { b.Division, b.Store } 
                                where ((a.RuleSetID == d.RuleSetID) && (p.ID == d.PlanID))
                                select a).Count();
            }
            db.SaveChanges(UserName);
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

            db.SaveChanges(UserName);
            Session["pqDeliveryGroups"] = null;
            UpdateRangePlanDate(planID);

            return RedirectToAction("PresentationQuantities", new { planID = planID });
        }

        //public ActionResult AddMissingStoreToDeliveryGroup(string div, string store, long ruleSetID, long planID, string rangeType)
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
            rss.CreatedBy = User.Identity.Name;
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
            UpdateRangeHeader(planID);
            ClearSessionVariables();

            return RedirectToAction("ShowStoresWithoutDeliveryGroup", new {planID = planID});
        }


        public ActionResult ShowStoresWithoutDeliveryGroup(int planID)
        {
            DeliveryGroupMissingModel model = new DeliveryGroupMissingModel();
            model.PlanID = planID;
            model.DeliveryGroups = (from a in db.DeliveryGroups
                                    where a.PlanID == planID
                                    select a).ToList();

            DeliveryGroup newGroup = new DeliveryGroup();
            newGroup.Name = "<New Delivery Group>";
            newGroup.ID = -1;
            newGroup.RuleSetID = -1;
            newGroup.PlanID = planID;
            model.DeliveryGroups.Insert(0, newGroup);

            List<StoreLookupModel> list = db.GetStoreLookupsForPlan(planID, DivisionList(User.Identity.Name));
            List<RuleSelectedStore> ruleSetStores = (from a in db.RuleSets
                                                     join b in db.RuleSelectedStores 
                                                        on a.RuleSetID equals b.RuleSetID
                                                     where ((a.PlanID == planID) && 
                                                            (a.Type == "Delivery"))
                                                     select b).ToList();

            model.Stores = new List<StoreLookupModel>();
            foreach (StoreLookupModel m in list)
            {
                if ((from a in ruleSetStores
                     where ((a.Division == m.Division) && (a.Store == m.Store))
                     select a).Count() == 0)
                {
                    //not in a delivery group
                    if ((from a in db.vValidStores
                         where ((a.Division == m.Division) && (a.Store == m.Store))
                         select a).Count() > 0)
                    {
                        //valid store, so they need to assign it
                        model.Stores.Add(m);
                    }
                }
            }

            return View(model);
        }

        public ActionResult CreateDeliveryGroup(Int64 planID)
        {
            DeliveryGroup newGroup = new DeliveryGroup();
            CreateNewGroup(planID, newGroup, null);
            Session["pqPlan"] = null;
            Session["selectedDeliveryGroups"] = null;
            return RedirectToAction("EditDeliveryGroup", new { planID = planID, deliveryGroupID = newGroup.ID });
        }

        private void CreateNewGroup(Int64 planID, DeliveryGroup newGroup, DateTime? startDate)
        {
            newGroup.PlanID = planID;
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

            UpdateRangeHeader(planID);
        }

        [GridAction]
        public ActionResult _DeliveryGroupStores(Int64 planID, Int64 deliveryGroupID)
        {
            //DeliveryGroupModel model = new DeliveryGroupModel();
            //model.DeliveryGroup = (from a in db.DeliveryGroups where a.ID == deliveryGroupID select a).First();
            //List<DeliveryGroupDetail> DeliveryGroupDetails = (from a in db.DeliveryGroupDetails where a.DeliveryGroupID == deliveryGroupID select a).ToList();
            List<StoreLookupModel> PlanStores = db.GetStoreLookupsForPlan(planID, DivisionList(User.Identity.Name));
            return View(new GridModel(PlanStores));
        }

        public ActionResult EditDeliveryGroup(Int64 planID, Int64 deliveryGroupID)
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
            ItemMaster i = (from a in db.ItemMasters join b in db.RangePlans on a.ID equals b.ItemID where b.Id == planID select a).FirstOrDefault();
            if (i != null)
            {
                ViewData["LifeCycle"] = i.LifeCycleDays;
            }



            model.RuleModel = new RuleModel();
            model.RuleModel.RuleSetID = model.DeliveryGroup.RuleSetID;
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
                    int centuries = ((DateTime.Now.Year - 1900)/100)*100;
                    model.DeliveryGroup.EndDate = ((DateTime)model.DeliveryGroup.EndDate).AddYears(centuries);
                }
            }
//            var list = (from a in db.SizeAllocations join b in db.RuleSelectedStores on new { a.Division, a.Store } equals new { b.Division, b.Store } join c in db.MaxLeadTimes on new { a.Division, a.Store } equals new { c.Division, c.Store } where (a.PlanID == model.DeliveryGroup.PlanID) select new { sa = a, lt = c }).ToList();
            UpdateDeliveryGroupDates(model.DeliveryGroup);
            //note above line will save all changes
            ClearSessionVariables();
            UpdateRangeHeader(model.DeliveryGroup.PlanID);

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
                              where ((c.Store == store) && (c.Division == div))
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
                       where (b.Id == model.PlanID)
                       select b).Count();

            RangePlan plan = (from a in db.RangePlans
                              where a.Id == model.PlanID
                              select a).FirstOrDefault();

            List<MaxLeadTime> leadTimes;

            leadTimes = (from b in db.RuleSelectedStores
                         join c in db.MaxLeadTimes 
                         on new { b.Division, b.Store } equals new { c.Division, c.Store }
                         where (b.RuleSetID == model.RuleSetID)
                         select c).ToList();

            SizeAllocationDAO dao = new SizeAllocationDAO();
            List<RangePlanDetail> rangePlanDetails = (from a in db.RangePlanDetails
                                                      join b in db.RuleSelectedStores 
                                                      on new { a.Division, a.Store } equals new { b.Division, b.Store }
                                                      where ((a.ID == model.PlanID) && (b.RuleSetID == model.RuleSetID))
                                                      select a).ToList();

            if (plan.Launch)
            {
                foreach (RangePlanDetail det in rangePlanDetails)
                {
                    db.Entry(det).State = System.Data.EntityState.Modified;

                    //set start/end date
                    if (model.StartDate != null)
                    {
                        det.StartDate = ((DateTime)model.StartDate);
                    }
                    det.EndDate = ((DateTime)model.EndDate);
                }
            }
            else if (dts == 0)
            {
                //non dts store, set start date to delivery group start date + lead time
                foreach (var lt in leadTimes)
                {
                    var query = (from a in rangePlanDetails
                                 where ((a.Division == lt.Division) && (a.Store == lt.Store))
                                 select a);

                    foreach (RangePlanDetail det in query)
                    {
                        db.Entry(det).State = System.Data.EntityState.Modified;
                        //set start/end date
                        if (model.StartDate != null)
                        {
                            det.StartDate = ((DateTime)model.StartDate).AddDays(lt.LeadTime);
                        }
                        else
                        {
                            det.StartDate = model.StartDate;
                        }
                        if (model.EndDate != null)
                        {
                            det.EndDate = ((DateTime)model.EndDate).AddDays(lt.LeadTime);
                        }
                        else
                        {
                            det.EndDate = model.EndDate;
                        }
                    }
                }

                //for stores without any leadtimes, just use a default
                var queryLT = rangePlanDetails.Where(p => !db.StoreLeadTimes.Any(p2 => ((p2.Division == p.Division) && (p2.Store == p.Store))));

                foreach (RangePlanDetail det in queryLT)
                {
                    //set start/end date
                    if (model.StartDate != null)
                    {
                        det.StartDate = ((DateTime)model.StartDate).AddDays(_DEFAULT_MAX_LEADTIME);
                        db.Entry(det).State = System.Data.EntityState.Modified;
                    }
                    if (model.EndDate != null)
                    {
                        det.EndDate = ((DateTime)model.EndDate).AddDays(_DEFAULT_MAX_LEADTIME);
                    }
                }
            }
            else
            {
                foreach (RangePlanDetail det in rangePlanDetails)
                {
                    //set start/end date
                    db.Entry(det).State = System.Data.EntityState.Modified;
                    if (model.StartDate != null)
                    {
                        det.StartDate = ((DateTime)model.StartDate);
                    }
                    det.EndDate = ((DateTime)model.EndDate);
                }
            }

            UpdateRangeHeader(model.PlanID);
            db.SaveChanges(UserName);
            Session["pqDeliveryGroups"] = null;
        }
        #endregion

        #region "Add/Remove Stores"
        public ActionResult Edit(Int64 planID, string message)
        {
            ViewData["message"] = message;
            RangePlan p = (from a in db.RangePlans
                           where a.Id == planID
                           select a).First();
            return View(p);
        }

        [HttpPost]
        public ActionResult Edit(RangePlan model)
        {
            db.Entry(model).State = System.Data.EntityState.Modified;
            model.UpdateDate = DateTime.Now;
            model.UpdatedBy = User.Identity.Name;
            db.SaveChanges(UserName);
            List<DeliveryGroup> groups = (from a in db.DeliveryGroups
                                          where a.PlanID == model.Id
                                          select a).ToList();
            foreach (DeliveryGroup dg in groups)
            {
                UpdateDeliveryGroupDates(dg);
            }
            ClearSessionVariables();
            Session["SkuSetup"] = null;
            return RedirectToAction("Index", new { message = "Saved Changes" });
        }

        public ActionResult EditStores(Int64 planID, string message)
        {
            ViewData["planID"] = planID;
            ViewData["gridtype"] = "AllStores";
            ViewData["message"] = message;
            EditStoreModel model = new EditStoreModel();
            RangePlan p = (from a in db.RangePlans where a.Id == planID select a).First();
            model.plan = p;
            model.plan.ItemMaster = (from a in db.ItemMasters where a.ID == p.ItemID select a).FirstOrDefault();
            model.CurrentStores = db.GetStoreLookupsForPlan(planID, DivisionList(User.Identity.Name));
            model.RemainingStores = db.GetStoreLookupsNotInPlan(planID, DivisionList(User.Identity.Name));
            return View(model);
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public ActionResult UploadStores(IEnumerable<HttpPostedFileBase> attachments, Int64 planID)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            int errors = 0;
            List<StoreBase> list = new List<StoreBase>();
            RangePlan p = (from a in db.RangePlans where a.Id == planID select a).First();

            try
            {
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
                    string rangeType;
                    if ((mySheet.Cells[0, 0].Value.ToString().Contains("Div")) && (mySheet.Cells[0, 1].Value.ToString().Contains("Store")))
                    {
                        while (mySheet.Cells[row, 0].Value != null)
                        {
                            try
                            {
                                StoreBase newDet = new StoreBase();
                                newDet.Division = mySheet.Cells[row, 0].Value.ToString().PadLeft(2, '0');
                                newDet.Store = mySheet.Cells[row, 1].Value.ToString().PadLeft(5, '0');
                                if (mySheet.Cells[row, 2].Value != null)
                                {                     
                                    rangeType = mySheet.Cells[row, 2].Value.ToString();
                                }
                                else
                                {
                                    rangeType = "ALR";
                                }

                                if ((rangeType == "ALR") || (rangeType == "OP"))
                                {
                                    newDet.RangeType = rangeType;
                                }
                                
                                //always default to "Both"
                                if (string.IsNullOrEmpty(newDet.RangeType))
                                {
                                    newDet.RangeType = "Both";
                                }

                                ClearSessionVariables();
                                Regex validStoreNumber = new Regex("^[0-9][0-9][0-9][0-9][0-9]$");

                                if ((newDet.Store != "00000")&&(p.Division == newDet.Division)&&(validStoreNumber.IsMatch(newDet.Store)))
                                {
                                    list.Add(newDet);
                                }
                                else
                                {
                                    errors++;
                                }
                            }
                            catch (Exception ex)
                            {
                                errors++;
                            }
                            row++;
                        }
                    }
                    else
                    {
                        return Content("Incorrect header, first column must be \"Div\", next \"Store\".");
                    }
                }

                if (list.Count > 0)
                {
                    RuleDAO ruleDAO = new RuleDAO();
                    ruleDAO.AddStoresToPlan(list, planID);
                }

                string returnMessage = "Upload complete, added " + list.Count() + " stores";
                //if (alreadyThere > 0)
                //{
                //    returnMessage += ", " + alreadyThere + " already in plan";
                //}
                if (errors > 0)
                {
                    returnMessage +=", " + errors + " errors";
                }
                //UpdateStoreCount(planID, count);

                UpdateRangePlanDate(planID);
                Session["rulesetid"] = -1;
                return Content(returnMessage);
            }
            catch (Exception ex)
            { 
                return Content(ex.Message);
            }
        }



        public ActionResult StoreTemplate()
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            //FileStream file = new FileStream(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["ProductTypeTemplate"]), FileMode.Open, System.IO.FileAccess.Read);
            //Byte[] data1 = new Byte[file.Length];
            //file.Read(data1, 0, data1.Length);
            //file.Close();
            //MemoryStream memoryStream1 = new MemoryStream(data1);
            //excelDocument.Open(memoryStream1);
            excelDocument.Worksheets[0].Cells[0, 0].PutValue("Div (##)");
            excelDocument.Worksheets[0].Cells[0, 1].PutValue("Store (#####)");
            excelDocument.Worksheets[0].Cells[0, 0].Style.Font.IsBold = true;
            excelDocument.Worksheets[0].Cells[0, 1].Style.Font.IsBold = true;

            excelDocument.Save("StoreTemplate.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

        public ActionResult Excel(Int64 planID)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            //FileStream file = new FileStream(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SkuTypeTemplate"]), FileMode.Open);
            //Byte[] data1 = new Byte[file.Length];
            //file.Read(data1, 0, data1.Length);
            //file.Close();
            //MemoryStream memoryStream1 = new MemoryStream(data1);
            //excelDocument.Open(memoryStream1);
            Worksheet mySheet = excelDocument.Worksheets[0];

            mySheet.Cells[0, 0].PutValue("Div (##)");
            mySheet.Cells[0, 0].Style.Font.IsBold = true;
            mySheet.Cells[0, 1].PutValue("Store (#####)");
            mySheet.Cells[0, 1].Style.Font.IsBold = true;
            mySheet.Cells[0, 2].PutValue("Region");
            mySheet.Cells[0, 2].Style.Font.IsBold = true;
            mySheet.Cells[0, 3].PutValue("League");
            mySheet.Cells[0, 3].Style.Font.IsBold = true;
            mySheet.Cells[0, 4].PutValue("Mall");
            mySheet.Cells[0, 5].Style.Font.IsBold = true;
            mySheet.Cells[0, 5].PutValue("State");
            mySheet.Cells[0, 5].Style.Font.IsBold = true;
            mySheet.Cells[0, 6].PutValue("City");
            mySheet.Cells[0, 6].Style.Font.IsBold = true;
            mySheet.Cells[0, 7].PutValue("DBA");
            mySheet.Cells[0, 7].Style.Font.IsBold = true;
            mySheet.Cells[0, 8].PutValue("StoreType");
            mySheet.Cells[0, 8].Style.Font.IsBold = true;
            mySheet.Cells[0, 9].PutValue("Climate");
            mySheet.Cells[0, 9].Style.Font.IsBold = true;
            mySheet.Cells[0, 10].PutValue("MarketArea");
            mySheet.Cells[0, 10].Style.Font.IsBold = true;

            int row = 1;
            foreach (StoreLookupModel store in db.GetStoreLookupsForPlan(planID, DivisionList(User.Identity.Name)))
            {
                mySheet.Cells[row, 0].PutValue(store.Division);
                mySheet.Cells[row, 1].PutValue(store.Store);
                mySheet.Cells[row, 2].PutValue(store.Region);
                mySheet.Cells[row, 3].PutValue(store.League);
                mySheet.Cells[row, 4].PutValue(store.Mall);
                mySheet.Cells[row, 5].PutValue(store.State);
                mySheet.Cells[row, 6].PutValue(store.City);
                mySheet.Cells[row, 7].PutValue(store.DBA);
                mySheet.Cells[row, 8].PutValue(store.StoreType);
                mySheet.Cells[row, 9].PutValue(store.Climate);
                mySheet.Cells[row, 10].PutValue(store.MarketArea);
                
                row++;
            }

            for (int i = 0; i < 11;i++)
            {
                mySheet.AutoFitColumn(i);
            }

                excelDocument.Save("SkuRangePlanStores.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

        public ActionResult RemoveAllStores(Int64 planID)
        {
            ClearStoreFromPlan(planID);
            return RedirectToAction("EditStores", new { planID = planID, message = "All stores removed" });
        }

        private void ClearStoreFromPlan(long planID)
        {
            RangePlanDetailDAO dao = new RangePlanDetailDAO();
            dao.DeleteStoresForPlan(planID);
            UpdateRangePlanDate(planID);
        }

        #endregion


        #region "Add Stores By Rules"
        public ActionResult RuleList(Int64 planID)
        {
            ViewData["planID"] = planID;
            Int64 rulesetid = (new RuleDAO()).GetRuleSetID(planID, "main", User.Identity.Name);

            List<Rule> rules = (from r in db.Rules where r.RuleSetID == rulesetid orderby r.Sort ascending select r).ToList();
            return PartialView(rules);
        }

        [GridAction]
        public ActionResult _RuleList(Int64 planID)
        {
            Int64 rulesetid = (new RuleDAO()).GetRuleSetID(planID, "main", User.Identity.Name);
            List<Rule> rules = (from r in db.Rules where r.RuleSetID == rulesetid orderby r.Sort ascending select r).ToList();
            return PartialView(new GridModel(rules));
        }

        /// <summary>
        /// Populates partial view (grid) with list of stores that are in rule, showing which are already in the range plan.
        /// </summary>
        /// <param name="planID"></param>
        /// <returns></returns>
        public ActionResult StoreLookupList(Int64 planID, string gridtype, string ruleType) 
        {
            List<StoreLookupModel> list;
            ViewData["planID"] = planID;
            //ViewData["gridtype"] = filter;
            if (gridtype == "AllStores")
            {
                list = db.GetStoreLookupsForPlan(planID, DivisionList(User.Identity.Name));
                list.AddRange(db.GetStoreLookupsNotInPlan(planID, DivisionList(User.Identity.Name)));
            }
            else
            {
                List<Rule> rules = db.GetRulesForPlan(planID, ruleType);
                    //(from r in db.Rules
                    //                join rs in db.RuleSets
                    //                    on r.RuleSetID equals rs.RuleSetID
                    //                where ((rs.PlanID == planID) && (rs.Type == "main"))
                    //                orderby r.Sort ascending
                    //                select r).ToList();
                
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
        public ActionResult _StoreLookupList(Int64 planID, string gridtype, string ruleType) 
        {
            List<StoreLookupModel> list;// = new List<StoreLookupModel>();
            //ViewData["planID"] = planID;
            //ViewData["gridtype"] = filter;

            if (gridtype == "AllStores")
            {
                list = db.GetStoreLookupsForPlan(planID, DivisionList(User.Identity.Name));
                list.AddRange(db.GetStoreLookupsNotInPlan(planID, DivisionList(User.Identity.Name)));
            }
            else
            {
                List<Rule> rules = db.GetRulesForPlan(planID, ruleType);
                    //(from r in db.Rules 
                    //                join rs in db.RuleSets 
                    //                    on r.RuleSetID equals rs.RuleSetID
                    //                    where ((rs.PlanID == planID)&&(rs.Type == "main")) 
                    //                orderby r.Sort ascending select r).ToList();

                try
                {
                    list = GetStoresForRules(rules, planID);
                }
                catch (Exception ex)
                {
                    list = new List<StoreLookupModel>();
                    ShowError(ex);
                }
                List<StoreLookupModel> currStores = db.GetStoreLookupsForPlan(planID, DivisionList(User.Identity.Name));

                var currlist =
                        from n in list
                        join c in currStores on new { n.Division, n.Store } equals new { c.Division, c.Store }
                        select n;

                //var currlist = (from a in list where (StoreInList(a.Division, a.Store, currStores)) select a);
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
                RuleSet rs = new RuleSet();
                rs.PlanID = planID;
                rs.Type = ruleType;
                rs.CreatedBy = User.Identity.Name;
                rs.CreateDate = DateTime.Now;
                db.RuleSets.Add(rs);
                db.SaveChanges();
                model.RuleSetID = rs.RuleSetID;
            }
            ViewData["ruleSetID"] = model.RuleSetID;


            //fds  model.RuleToAdd = new Rule();
            model.PlanID = planID;
            model.Plan = (from a in db.RangePlans
                          where a.Id == planID
                          select a).First();

            UpdateRangePlanDate(planID);
            //ClearSessionVariables();

            return View(model);
        }

        private void ShowError(Exception ex)
        {
            if (ex.Message.Contains("rule"))
            {
                ViewData["rulemessage"] = ex.Message;
            }
            else
            {
                ViewData["rulemessage"] = "invalidly formatted rule!";
            }
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
        public ActionResult AddConjuction(string value, Int64 planID, string ruleType)
        {
            Rule newRule = new Rule();
            newRule.RuleSetID = (new RuleDAO()).GetRuleSetID(planID, ruleType, User.Identity.Name);
            newRule.Compare = value.Trim();
            newRule.Sort = (from r in db.Rules where r.RuleSetID == newRule.RuleSetID select r).Count() + 1;

            db.Rules.Add(newRule);
            db.SaveChanges();

            RuleSet rs = (from a in db.RuleSets where a.RuleSetID == newRule.RuleSetID select a).First();
            if (rs.Type == "SizeAlc")
            {
                return RedirectToAction("PresentationQuantities", new { planID = planID });
            }
            else
            {
                return RedirectToAction("AddStoresByRule", new { planID = planID });
            } 
            //return RedirectToAction("AddStoresByRule", new { planID = planID });
        }

        /// <summary>
        /// delete all rules
        /// </summary>
        public ActionResult ClearRules(string value, Int64 planID, string ruleType)
        {
            Rule newRule = new Rule();
            long ruleSetID = (new RuleDAO()).GetRuleSetID(planID, ruleType, User.Identity.Name);

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
            //List<Rule> RulesForPlan = (from r in db.Rules where r.PlanID == planID orderby r.Sort ascending select r).ToList();
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
                det.CreatedBy = User.Identity.Name;
                details.Add(det);
            }

            RangePlanDetailDAO dao = new RangePlanDetailDAO();
            dao.AddStores(details);
            
            UpdateRangePlanDate(planID);
            //ClearSessionVariables();
            return RedirectToAction("AddStoresByRule", new { planID = planID });

        }

        /// <summary>
        /// Add only the stores that are visible in the filtered grid to the planID
        /// TODO:  this is not implemented, it would need to create a lambda expression based on the filter.
        /// </summary>
        public JsonResult AddFilteredStores(Int64 planID, string filter, string ruleType)
        {
            //List<Rule> RulesForPlan = (from r in db.Rules where r.PlanID == planID orderby r.Sort ascending select r).ToList();
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
                det.CreatedBy = User.Identity.Name;
                details.Add(det);
            }

            RangePlanDetailDAO dao = new RangePlanDetailDAO();
            dao.AddStores(details);

            UpdateRangePlanDate(planID);
            return Json("Success");
            //return RedirectToAction("AddStoresByRule", new { planID = planID });

        }


        /// <summary>
        /// Add a store to the range plan (planID)
        /// </summary>
        public JsonResult AddStore(string store, string div, Int64 planID)
        {
            try
            {
                RangePlanDetail det = new RangePlanDetail();
                det.Store = store;
                det.Division = div;
                det.ID = planID;
                det.CreateDate = DateTime.Now;
                det.CreatedBy = User.Identity.Name;

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

                //UpdateStoreCount(planID, 1);
                UpdateRangePlanDate(planID);
                ClearSessionVariables();

                return Json("Success");
            }
            catch (Exception ex)
            {
                return Json("Error");
            }
            //return GetGridJson(planID, page);
        }

        /// <summary>
        /// Deletes the store from the range plan (planID)
        /// </summary>
        public JsonResult DeleteStore(string store, string div, Int64 planID)
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

                //UpdateStoreCount(planID, -1);
                UpdateRangePlanDate(planID);
                ClearSessionVariables();

                return Json("Success");
            }
            catch (Exception ex)
            {
                return Json("Error");
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
            divRule.Value = p.Sku.Substring(0,2);

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
            // 'queryableData.Where(company => (company.ToLower() == "coho winery" || company.Length > 16))'
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
                        for (int i=1; i < rules.Count(); i++)
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
                                    return GetCompositeRule(newExp, rules.GetRange(i+1, (rules.Count()-i-1)), pe);
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
                            return Expression.Not(GetExpression(rules.GetRange(1,rules.Count()-1),pe));
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
            Expression exp=null;
            Expression left=null;
            Expression right=null;
            //ParameterExpression pe = Expression.Parameter(typeof(StoreLookup), "StoreLookup");


            if (rule.Field == "AdHocCode") 
            {
            } else if ((rule.Field == "StorePlan") ||  (rule.Field == "RangePlan"))
            {
                //return an expression that will link to the RangePlanDetails
                        //TODO:  This is faster than manually creating and/or's but still slow
                        //not sure if there is a better way to build the expression tree
                        List<string> stores;

                        if (rule.Field == "RangePlan")
                        {
                            Int64 planID;// = Convert.ToInt64(rule.Value);
                            string sku = Convert.ToString(rule.Value);

                            string myInClause = DivisionList(User.Identity.Name);
                            try
                            {
                                planID = (from x in db.RangePlans where (x.Sku == sku) select x).First().Id;
                            }
                            catch {
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

                        //Expression convertExpression = Expression.Convert(memberExpression, typeof(string));  //store only
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

        public ActionResult CreateOrderPlanningRequest(Int64 planID)
        {
            OrderPlanningRequest model = new OrderPlanningRequest();
            model.PlanID = planID;
            DateTime start = (from p in db.RangePlans
                              join i in db.ItemMasters 
                                on p.ItemID equals i.ID
                              join id in db.InstanceDivisions 
                                on i.Div equals id.Division
                              join cd in db.ControlDates 
                                on id.InstanceID equals cd.InstanceID
                              where p.Id == planID
                              select cd.RunDate).First();

            model.StartSend = start.AddDays(2);
            model.EndSend = start.AddDays(12);
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
            db.SaveChanges(UserName);
            UpdateRangePlanDate(model.PlanID);
            return RedirectToAction("PresentationQuantities", new { planID = model.PlanID });
        }

        private string ValidateOrderPlanningRequest(OrderPlanningRequest model, Boolean edit)
        {
            DateTime start = (from p in db.RangePlans
                              join i in db.ItemMasters 
                                 on p.ItemID equals i.ID
                              join id in db.InstanceDivisions 
                                 on i.Div equals id.Division
                              join cd in db.ControlDates 
                                 on id.InstanceID equals cd.InstanceID
                              where p.Id == model.PlanID
                              select cd.RunDate).First();

            if (model.StartSend < start.AddDays(2))
            {
                if (!WebSecurityService.UserHasRole(UserName, "Allocation", "IT"))
                {
                    if (!edit)
                    {
                        return "Earliest start date is " + start.AddDays(2);
                    }
                }
            }

            return null;
        }

        public ActionResult DeleteOrderPlanningRequest(Int64 planID)
        {
            OrderPlanningRequest model = (from a in db.OrderPlanningRequests where a.PlanID == planID select a).First();
            db.OrderPlanningRequests.Remove(model);
            db.SaveChanges(UserName);
            UpdateRangePlanDate(planID);
            return RedirectToAction("PresentationQuantities", new { planID = model.PlanID });
        }


        public ActionResult EditOrderPlanningRequest(Int64 planID)
        {
            OrderPlanningRequest model = (from a in db.OrderPlanningRequests where a.PlanID == planID select a).First();
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
            db.SaveChanges(UserName);
            UpdateRangePlanDate(model.PlanID);

            return RedirectToAction("PresentationQuantities", new { planID = model.PlanID });
        }


        #endregion

        #region ALR Request

        public ActionResult StartALR(Int64 planID)
        {
            RangePlan rp = (from a in db.RangePlans where a.Id == planID select a).First();
            rp.ALRStartDate = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == rp.Division select a.RunDate).First().AddDays(1);
            rp.UpdateDate = DateTime.Now;
            rp.UpdatedBy = UserName;
            db.Entry(rp).State = System.Data.EntityState.Modified;

            List<DeliveryGroup> deliveryGroups = (from a in db.DeliveryGroups where (a.PlanID == planID) select a).ToList();
            foreach (DeliveryGroup dg in deliveryGroups)
            {
                if ((dg.ALRStartDate == null) || (dg.ALRStartDate > rp.ALRStartDate))
                {
                    dg.ALRStartDate = rp.ALRStartDate;
                    db.Entry(dg).State = System.Data.EntityState.Modified;
                }
            }

            db.SaveChanges(UserName);

            //Session["pqPlan"] = null;
            Session["pqDeliveryGroups"] = null;
            Session["SkuSetup"] = null;  //make the index page reload the updated date
            return RedirectToAction("PresentationQuantities", new { planID = planID });
        }

        public ActionResult StopALR(Int64 planID)
        {
            RangePlan rp = (from a in db.RangePlans
                            where a.Id == planID select a).First();
            rp.ALRStartDate = null;
            rp.UpdateDate = DateTime.Now;
            rp.UpdatedBy = User.Identity.Name;

            db.Entry(rp).State = System.Data.EntityState.Modified;
            List<DeliveryGroup> deliveryGroups = (from a in db.DeliveryGroups where (a.PlanID == planID) select a).ToList();
            foreach (DeliveryGroup dg in deliveryGroups)
            {
                dg.ALRStartDate = null;
                db.Entry(dg).State = System.Data.EntityState.Modified;
            }

            db.SaveChanges(UserName);

            //Session["pqPlan"] = null;
            Session["pqDeliveryGroups"] = null;
            Session["SkuSetup"] = null;  //make the index page reload the updated date
            return RedirectToAction("PresentationQuantities", new { planID = planID });
        }

        public ActionResult StartDeliveryGroup(Int64 deliveryGroupID, Int64 planID)
        {
            DeliveryGroup dg = (from a in db.DeliveryGroups where a.ID == deliveryGroupID select a).First();
            RangePlan rp = (from a in db.RangePlans where a.Id == planID select a).First();
            dg.ALRStartDate = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == rp.Division select a.RunDate).First().AddDays(1);
            db.Entry(dg).State = System.Data.EntityState.Modified;
            Session["pqDeliveryGroups"] = null;

            var query = (from a in db.DeliveryGroups where ((a.PlanID == planID) && (a.ID != deliveryGroupID) && (a.ALRStartDate == null)) select a);
            if (query.Count() == 0)
            {
                rp.ALRStartDate = (from a in db.ControlDates join b in db.InstanceDivisions on a.InstanceID equals b.InstanceID where b.Division == rp.Division select a.RunDate).First().AddDays(1);
                rp.UpdateDate = DateTime.Now;
                rp.UpdatedBy = UserName;

                db.Entry(rp).State = System.Data.EntityState.Modified;
                Session["SkuSetup"] = null;  //make the index page reload the updated date
            }
            db.SaveChanges(UserName);
            UpdateRangePlanDate(planID);

            return RedirectToAction("PresentationQuantities", new { planID = planID });
        }

        public ActionResult StopDeliveryGroup(Int64 deliveryGroupID, Int64 planID)
        {
            DeliveryGroup dg = (from a in db.DeliveryGroups where a.ID == deliveryGroupID select a).First();
            RangePlan rp = (from a in db.RangePlans where a.Id == planID select a).First();
            dg.ALRStartDate = null;
            db.Entry(dg).State = System.Data.EntityState.Modified;
            Session["pqDeliveryGroups"] = null;

            rp.ALRStartDate = null;
            rp.UpdateDate = DateTime.Now;
            rp.UpdatedBy = UserName;

            db.Entry(rp).State = System.Data.EntityState.Modified;
            Session["SkuSetup"] = null;  //make the index page reload the updated date
            db.SaveChanges(UserName);
            UpdateRangePlanDate(planID);

            return RedirectToAction("PresentationQuantities", new { planID = planID });
        }

        #endregion


        [CheckPermission(Roles = "Merchandiser,Head Merchandiser,Buyer Planner,Director of Allocation,Admin,Support")]
        public ActionResult UploadRange()
        {
            return View();
        }

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


        private ActionResult BulkSaveRange(IEnumerable<HttpPostedFileBase> attachments, Boolean purgeFirst)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            string division = "";
            Session["errorList"] = null;
            ClearSessionVariables();
            try
            {
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
                    int validCount = 0;
                    int errorCount = 0;
                    if ((Convert.ToString(mySheet.Cells[0, 0].Value).Contains("Division")) &&
                        (Convert.ToString(mySheet.Cells[0, 1].Value).Contains("League")) &&
                        (Convert.ToString(mySheet.Cells[0, 2].Value).Contains("Region")) &&
                        (Convert.ToString(mySheet.Cells[0, 3].Value).Contains("Store")) &&
                        (Convert.ToString(mySheet.Cells[0, 4].Value).Contains("Sku")) &&
                        (Convert.ToString(mySheet.Cells[0, 5].Value).Contains("Size"))
                        && (Convert.ToString(mySheet.Cells[0, 6].Value).Contains("Range Start Date")) &&
                        (Convert.ToString(mySheet.Cells[0, 7].Value).Contains("Delivery Group Name"))
                        && (Convert.ToString(mySheet.Cells[0, 8].Value).Contains("Min")) &&
                        (Convert.ToString(mySheet.Cells[0, 9].Value).Contains("Max"))
                        && (Convert.ToString(mySheet.Cells[0, 10].Value).Contains("Base Demand")) &&
                        (Convert.ToString(mySheet.Cells[0, 11].Value).Contains("Min End Date")) &&
                        (Convert.ToString(mySheet.Cells[0, 12].Value).Contains("End Date"))
                        )
                    {
                        division = Convert.ToString(mySheet.Cells[row, 0].Value).Trim().PadLeft(2, '0');
                        List<BulkRange> updateList = new List<BulkRange>();
                        List<BulkRange> errorList = new List<BulkRange>();
                        BulkRange range;

                        while (HasDataOnRow(mySheet, row))
                        {
                            if (division != Convert.ToString(mySheet.Cells[row, 0].Value).Trim().PadLeft(2, '0'))
                            {
                                division = Convert.ToString(mySheet.Cells[row, 0].Value).Trim().PadLeft(2, '0');
                                if (
                                    !(Footlocker.Common.WebSecurityService.UserHasDivision(
                                        User.Identity.Name.Split('\\')[1], "allocation", division)))
                                {
                                    return Content("You are not authorized to update division " + division);
                                }
                            }
                            range = new BulkRange();
                            range.Division = division;
                            range.Store = Convert.ToString(mySheet.Cells[row, 3].Value).Trim().PadLeft(5, '0');

                            //ensure the store is valid
                            if (!ValidateStore(range.Division, range.Store))
                            {
                                string message = string.Format("Row #{0}: The division and store combination does not exist within the system.", row);
                                return Content(message);
                            }

                            range.Sku = Convert.ToString(mySheet.Cells[row, 4].Value).Trim();
                            range.Size = Convert.ToString(mySheet.Cells[row, 5].Value).Trim().PadLeft(3, '0').ToUpper();
                            range.RangeStartDate = Convert.ToString(mySheet.Cells[row, 6].Value).Trim();
                            range.DeliveryGroupName = Convert.ToString(mySheet.Cells[row, 7].Value).Trim();
                            //range.Range = Convert.ToString(mySheet.Cells[row, 5].Value);
                            //if (range.Range.ToUpper().Equals("TRUE"))
                            //{
                            range.Range = "1";
                            //}
                            //else
                            //{
                            //    range.Range = "0";
                            //}
                            range.Min = Convert.ToString(mySheet.Cells[row, 8].Value);
                            range.Max = Convert.ToString(mySheet.Cells[row, 9].Value);
                            string baseDemand = Convert.ToString(mySheet.Cells[row, 10].Value).Trim();
                            if (!string.IsNullOrEmpty(baseDemand))
                            {
                                range.BaseDemand = Convert.ToDecimal(mySheet.Cells[row, 10].FloatValue).ToString();
                            }
                            else
                            {
                                range.BaseDemand = "";
                            }
                            range.MinEndDateOverride = Convert.ToString(mySheet.Cells[row, 11].Value);
                            range.EndDate = Convert.ToString(mySheet.Cells[row, 12].Value);
                            //doing this to preserve nulls for blank
                            if (range.Min == "")
                            {
                                range.Min = "-1";
                            }
                            if (range.Max == "")
                            {
                                range.Max = "-1";
                            }
                            if (range.BaseDemand == "")
                            {
                                range.BaseDemand = "-1";
                            }
                            if (range.MinEndDateOverride == "")
                            {
                                range.MinEndDateOverride = "-1";
                            }
                            if (range.EndDate == "")
                            {
                                range.EndDate = "-1";
                            }

                            updateList.Add(range);
                            row++;
                        }

                        List<string> skus = (from a in updateList select a.Sku).Distinct().ToList();
                        if (purgeFirst)
                        {
                            if (skus.Count > 1)
                            {
                                return Content("You can only update a single sku when you choose to purge first.");
                            }
                            else if (skus.Count == 1)
                            {
                                try
                                {
                                    long planid;
                                    string sku = skus[0];
                                    List<RangePlan> plans = (from a in db.RangePlans where a.Sku == sku select a).ToList();
                                    planid = plans[0].Id;
                                    ClearStoreFromPlan(plans[0].Id);
                                }
                                catch (Exception ex)
                                {
                                    return Content(ex.Message);
                                }
                            }
                        }

                        RangePlanDetailDAO dao = new RangePlanDetailDAO();
                        errorList = dao.BulkUpdateRange(updateList, User.Identity.Name);

                        if (errorList.Count > 0)
                        {
                            Session["errorList"] = errorList;
                            return
                                Content(errorList.Count + " size level errors (" +
                                        updateList.Count + " records on sheet)");
                        }
                    }
                    else
                    {
                        // Inform of missing/bad header row
                        return Content("Incorrectly formatted or missing header row. Please correct and re-process.");
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

        private bool HasDataOnRow(Worksheet sheet, int row)
        {
            return sheet.Cells[row, 0].Value != null ||
                   sheet.Cells[row, 1].Value != null ||
                   sheet.Cells[row, 2].Value != null ||
                   sheet.Cells[row, 3].Value != null ||
                   sheet.Cells[row, 4].Value != null ||
                   sheet.Cells[row, 5].Value != null ||
                   sheet.Cells[row, 6].Value != null ||
                   sheet.Cells[row, 7].Value != null ||
                   sheet.Cells[row, 8].Value != null ||
                   sheet.Cells[row, 9].Value != null ||
                   sheet.Cells[row, 10].Value != null ||
                   sheet.Cells[row, 11].Value != null ||
                   sheet.Cells[row, 12].Value != null;
        }

        /// <summary>
        /// Validate the store entered from the RangeUpload file
        /// </summary>
        /// <param name="division">division from file</param>
        /// <param name="store">store from file</param>
        /// <returns></returns>
        private bool ValidateStore(string division, string store)
        {
            return (from sl in db.StoreLookups where sl.Division == division && sl.Store == store select sl).Any();
        }

        public ActionResult DownloadRangeErrors()
        {
            List<BulkRange> errorList = new List<BulkRange>();
            if (Session["errorList"] != null)
            {
                errorList = (List<BulkRange>)Session["errorList"];
            }


            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            string templateFilename = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["RangeTemplate"]);
            FileStream file = new FileStream(Server.MapPath("~/") + templateFilename, FileMode.Open, System.IO.FileAccess.Read);
            Byte[] data1 = new Byte[file.Length];
            file.Read(data1, 0, data1.Length);
            file.Close();
            MemoryStream memoryStream1 = new MemoryStream(data1);
            excelDocument.Open(memoryStream1);
            Worksheet mySheet = excelDocument.Worksheets[0];
            int row = 1;
            foreach (BulkRange p in errorList)
            {
                mySheet.Cells[row, 0].PutValue(p.Division);
                mySheet.Cells[row, 1].PutValue(p.League);
                mySheet.Cells[row, 2].PutValue(p.Region);
                mySheet.Cells[row, 3].PutValue(p.Store);
                mySheet.Cells[row, 4].PutValue(p.Sku);
                mySheet.Cells[row, 5].PutValue(p.Size);
                mySheet.Cells[row, 6].PutValue(p.RangeStartDate);
                mySheet.Cells[row, 7].PutValue(p.DeliveryGroupName);
                mySheet.Cells[row, 8].PutValue(p.Min);
                mySheet.Cells[row, 9].PutValue(p.Max);
                mySheet.Cells[row, 10].PutValue(p.BaseDemand);
                mySheet.Cells[row, 11].PutValue(p.MinEndDateOverride);
                mySheet.Cells[row, 12].PutValue(p.EndDate);
                mySheet.Cells[row, 13].PutValue(p.Error);
                mySheet.Cells[row, 14].Style.Font.Color = Color.Red;
                row++;
            }

            excelDocument.Save("RangeUploadErrors.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();

        }

        public ActionResult ExcelRangeTemplate()
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            string templateFilename = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["RangeTemplate"]);
            FileStream file = new FileStream(Server.MapPath("~/") + templateFilename, FileMode.Open, System.IO.FileAccess.Read);
            Byte[] data1 = new Byte[file.Length];
            file.Read(data1, 0, data1.Length);
            file.Close();
            MemoryStream memoryStream1 = new MemoryStream(data1);
            excelDocument.Open(memoryStream1);
            excelDocument.Save("RangeUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();

        }

        public ActionResult ExcelRange(string sku)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            string templateFilename = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["RangeTemplate"]);
            FileStream file = new FileStream(Server.MapPath("~/") + templateFilename, FileMode.Open, System.IO.FileAccess.Read);
            Byte[] data1 = new Byte[file.Length];
            file.Read(data1, 0, data1.Length);
            file.Close();
            MemoryStream memoryStream1 = new MemoryStream(data1);
            excelDocument.Open(memoryStream1);

            List<BulkRange> list = (new RangePlanDetailDAO()).GetBulkRangesForSku(sku);
            int row = 1;
            Worksheet mySheet = excelDocument.Worksheets[0];

            foreach (BulkRange p in list)
            {
                mySheet.Cells[row, 0].PutValue(p.Division);
                mySheet.Cells[row, 1].PutValue(p.League);
                mySheet.Cells[row, 2].PutValue(p.Region);
                mySheet.Cells[row, 3].PutValue(p.Store);
                mySheet.Cells[row, 4].PutValue(p.Sku);
                mySheet.Cells[row, 5].PutValue(p.Size);
                mySheet.Cells[row, 6].PutValue(p.RangeStartDate);
                //mySheet.Cells[row, 7].PutValue(p.OnRangeDate);
                mySheet.Cells[row, 7].PutValue(p.DeliveryGroupName);
                mySheet.Cells[row, 8].PutValue(p.Min);
                mySheet.Cells[row, 9].PutValue(p.Max);
                mySheet.Cells[row, 10].PutValue(p.BaseDemand);
                mySheet.Cells[row, 11].PutValue(p.MinEndDateOverride);
                mySheet.Cells[row, 12].PutValue(p.EndDate);
                row++;
            }

            excelDocument.Save("RangeUpload.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            
            return View();
        }

        public ActionResult ExcelDeliveryGroup(int deliveryGroupID)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            string templateFilename = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["RangeTemplate"]);
            FileStream file = new FileStream(Server.MapPath("~/") + templateFilename, FileMode.Open, System.IO.FileAccess.Read);
            Byte[] data1 = new Byte[file.Length];
            file.Read(data1, 0, data1.Length);
            file.Close();
            MemoryStream memoryStream1 = new MemoryStream(data1);
            excelDocument.Open(memoryStream1);

            // retrieve specific delivery group
            DeliveryGroup dg = (from a in db.DeliveryGroups where a.ID == deliveryGroupID select a).FirstOrDefault();

            // retrieve sku for delivery group to feed into stored procedure
            string sku = (from a in db.RangePlans where a.Id == dg.PlanID select a).Select(rp => rp.Sku).FirstOrDefault();

            List <BulkRange> list = (new RangePlanDetailDAO()).GetBulkRangesForSku(sku)
                                        .Where(q => q.DeliveryGroupName.Equals(dg.Name))
                                        .OrderBy(br => br.Division).ThenBy(br => br.Store).ThenBy(br => br.Size)
                                        .ToList();

            int row = 1;
            Worksheet mySheet = excelDocument.Worksheets[0];

            foreach (BulkRange br in list)
            {
                mySheet.Cells[row, 0].PutValue(br.Division);
                mySheet.Cells[row, 1].PutValue(br.League);
                mySheet.Cells[row, 2].PutValue(br.Region);
                mySheet.Cells[row, 3].PutValue(br.Store);
                mySheet.Cells[row, 4].PutValue(br.Sku);
                mySheet.Cells[row, 5].PutValue(br.Size);
                mySheet.Cells[row, 6].PutValue(br.RangeStartDate);
                mySheet.Cells[row, 7].PutValue(br.DeliveryGroupName);
                mySheet.Cells[row, 8].PutValue(br.Min);
                mySheet.Cells[row, 9].PutValue(br.Max);
                mySheet.Cells[row, 10].PutValue(br.BaseDemand);
                mySheet.Cells[row, 11].PutValue(br.MinEndDateOverride);
                mySheet.Cells[row, 12].PutValue(br.EndDate);
                row++;
            }

            excelDocument.Save(sku + "-" + dg.Name + ".xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }
    }
}
