using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using Telerik.Web.Mvc;
using System.Linq.Expressions;
using System.IO;
using Footlocker.Logistics.Allocation.Spreadsheets;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class RuleController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
        //
        // GET: /Rule/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult _AutoCompleteFilteringAjax(string text, string type)
        {
            IQueryable<string> results;
            switch (type)
            {
                case "Store":
                    IQueryable<StoreLookup> stores = db.StoreLookups.AsQueryable();
                    stores = stores.Where((p) => p.Store.StartsWith(text));
                    return new JsonResult { Data = stores.Select(p => p.Store) };
                case "Division":
                    results = (from a in db.InstanceDivisions where a.Division.StartsWith(text) select a.Division).Distinct();
                    return new JsonResult { Data = results.ToList() };
                case "State":
                    results = (from a in db.StoreLookups where a.State.StartsWith(text) select a.State).Distinct();
                    return new JsonResult { Data = results.ToList() };
                case "DBA":
                    results = (from a in db.StoreLookups where a.DBA.StartsWith(text) select a.DBA).Distinct();
                    return new JsonResult { Data = results.ToList() };
                case "Region":
                    results = (from a in db.StoreLookups where a.Region.StartsWith(text) select a.Region).Distinct();
                    return new JsonResult { Data = results.ToList() };
                case "League":
                    results = (from a in db.StoreLookups where a.League.StartsWith(text) select a.League).Distinct();
                    return new JsonResult { Data = results.ToList() };
                case "City":
                    results = (from a in db.StoreLookups where a.City.StartsWith(text) select a.City).Distinct();
                    return new JsonResult { Data = results.ToList() };
                case "Mall":
                    results = (from a in db.StoreLookups where a.Mall.StartsWith(text) select a.Mall).Distinct();
                    return new JsonResult { Data = results.ToList() };
                case "StoreType":
                    results = (from a in db.StoreLookups where a.StoreType.StartsWith(text) select a.StoreType).Distinct();
                    return new JsonResult { Data = results.ToList() };
                case "StoreStatus":
                    return new JsonResult { Data = new List<string>() };
                case "StorePlan":
                    if (text.Length > 3)
                    {
                        results = (from a in db.StorePlans where a.PlanName.StartsWith(text) select a.PlanName).Distinct();
                        return new JsonResult { Data = results.ToList() };
                    }
                    else                    
                        return new JsonResult { Data = new List<string>() };
                    
                case "RangePlan":
                    results = (from a in db.RangePlans where a.Sku.StartsWith(text) select a.Sku).Distinct();
                    return new JsonResult { Data = results.ToList() };
                case "RangePlanDesc":
                    results = (from a in db.RangePlans where a.Description.StartsWith(text) select (a.Description + " (" + a.Sku + ")")).Distinct();
                    return new JsonResult { Data = results.ToList() };
                case "MarketArea":
                    results = (from a in db.StoreLookups where a.MarketArea.StartsWith(text) select a.MarketArea).Distinct();
                    return new JsonResult { Data = results.ToList() };
                case "Climate":
                    results = (from a in db.StoreLookups where a.Climate.StartsWith(text) select a.Climate).Distinct();
                    return new JsonResult { Data = results.ToList() };
                //case "StoreExtension.ConceptType.Name":
                //    results = (from a in db.ConceptTypes where a.Name.StartsWith(text) select a.Name).Distinct();
                //    return new JsonResult { Data = results.ToList() };
                case "StoreExtension.CustomerType.Name":
                    results = (from a in db.CustomerTypes where a.Name.StartsWith(text) select a.Name).Distinct();
                    return new JsonResult { Data = results.ToList() };
                case "StoreExtension.StrategyType.Name":
                    results = (from a in db.StrategyTypes where a.Name.StartsWith(text) select a.Name).Distinct();
                    return new JsonResult { Data = results.ToList() };
            }

            return new JsonResult { Data = new List<StoreLookup>() };
        }

        [HttpPost]
        public ActionResult GetExamples(string type)
        {
            IQueryable<string> results;
            switch (type)
            {
                case "Store":
                    results = (from a in db.StoreLookups select a.Store).Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "Division":
                    results = (from a in db.InstanceDivisions select a.Division).Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "State":
                    results = (from a in db.StoreLookups select a.State).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "DBA":
                    results = (from a in db.StoreLookups select a.DBA).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "Region":
                    results = (from a in db.StoreLookups select a.Region).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "League":
                    results = (from a in db.StoreLookups select a.League).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "City":
                    results = (from a in db.StoreLookups select a.City).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "Mall":
                    results = (from a in db.StoreLookups select a.Mall).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "StoreType":
                    results = (from a in db.StoreLookups select a.StoreType).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "StorePlan":
                    results = (from a in db.StorePlans select a.Description).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "RangePlan":
                    results = (from a in db.RangePlans select a.Sku).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "RangePlanDesc":
                    results = (from a in db.RangePlans select (a.Description + " (" + a.Sku + ")")).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "MarketArea":
                    results = (from a in db.StoreLookups select a.MarketArea).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "Climate":
                    results = (from a in db.StoreLookups select a.Climate).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                //case "StoreExtension.ConceptType.Name":
                //    results = (from a in db.ConceptTypes select a.Name).Distinct().Take(5);
                //    return new JsonResult { Data = results.ToList() };
                case "StoreExtension.CustomerType.Name":
                    results = (from a in db.CustomerTypes select a.Name).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "StoreExtension.StrategyType.Name":
                    results = (from a in db.StrategyTypes select a.Name).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "AdHoc1":
                    results = (from a in db.StoreLookups select a.AdHoc1).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "AdHoc2":
                    results = (from a in db.StoreLookups select a.AdHoc1).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "AdHoc3":
                    results = (from a in db.StoreLookups select a.AdHoc1).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "AdHoc4":
                    results = (from a in db.StoreLookups select a.AdHoc1).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "AdHoc5":
                    results = (from a in db.StoreLookups select a.AdHoc1).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "AdHoc6":
                    results = (from a in db.StoreLookups select a.AdHoc1).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "AdHoc7":
                    results = (from a in db.StoreLookups select a.AdHoc1).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "AdHoc8":
                    results = (from a in db.StoreLookups select a.AdHoc1).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "AdHoc9":
                    results = (from a in db.StoreLookups select a.AdHoc1).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "AdHoc10":
                    results = (from a in db.StoreLookups select a.AdHoc1).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "AdHoc11":
                    results = (from a in db.StoreLookups select a.AdHoc1).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
                case "AdHoc12":
                    results = (from a in db.StoreLookups select a.AdHoc1).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
            }

            return new JsonResult { Data = new List<string>() };
        }

        public ActionResult Grid(long ruleSetID, string ruleType)
        {
            RuleDAO ruleDAO = new RuleDAO();

            List<Rule> rules = ruleDAO.GetRulesForPlan(ruleSetID, ruleType);
            return View(rules);
        }

        [GridAction]
        public ActionResult _Grid(long ruleSetID, string ruleType)
        {
            List<Rule> rules;
            RuleDAO ruleDAO = new RuleDAO();

            rules = ruleDAO.GetRulesForRuleSet(ruleSetID, ruleType);

            return View(new GridModel(rules));
        }

        public ActionResult GridForPlan(long planID, string ruleType)
        {
            RuleDAO ruleDAO = new RuleDAO();

            List<Rule> rules = ruleDAO.GetRulesForPlan(planID, ruleType);
            return View(rules);
        }

        [GridAction]
        public ActionResult _GridForPlan(long planID, string ruleType)
        {
            List<Rule> rules;
            RuleDAO ruleDAO = new RuleDAO();

            rules = ruleDAO.GetRulesForPlan(planID, ruleType);
            return View(new GridModel(rules));
        }

        /// <summary>
        /// Add a new rule specified by user
        /// "Store Equals 07801"
        /// </summary>
        [HttpPost]
        public ActionResult AddRule(long ruleSetID, string field, string compare, string value, int sort)
        {
            Rule newRule = new Rule()
            {
                RuleSetID = ruleSetID,
                Field = field,
                Compare = compare,
                Value = value
            };

            IOrderedEnumerable<Rule> maxSort = db.Rules.Where(r => r.RuleSetID == ruleSetID).ToList().OrderByDescending(o => o.Sort);
            if (maxSort.Count() > 0)
            {
                Rule lastRule = maxSort.First();
                sort = lastRule.Sort;
                //add an and rule if there isn't one
                if (lastRule.Compare != "and" && lastRule.Compare != "or" && lastRule.Compare != "not")
                {
                    //add an and
                    Rule andRule = new Rule()
                    {
                        RuleSetID = newRule.RuleSetID,
                        Compare = "and"
                    };

                    foreach (Rule r in maxSort)
                    {
                        if (!string.IsNullOrEmpty(r.Field))
                        {
                            if (r.Field == newRule.Field)                            
                                andRule.Compare = "or";
                            
                            break;
                        }
                    }

                    andRule.Sort = sort + 1;
                    sort++;
                    db.Rules.Add(andRule);
                    db.SaveChanges();
                }
            }
            else            
                sort = 0;
            
            newRule.Sort = sort + 1;

            if (ModelState.IsValid)
            {
                db.Rules.Add(newRule);
                db.SaveChanges();
            }
            RuleSet rs = db.RuleSets.Where(r => r.RuleSetID == newRule.RuleSetID).First();
           
            Session["rulesetid"] = -1;

            if (rs.Type == "SizeAlc")
            {
                //delete all the ruleselected stores, because they are using rules and not a spreadsheet upload
                foreach (RuleSelectedStore rss in db.RuleSelectedStores.Where(r => r.RuleSetID == ruleSetID).ToList())
                {
                    db.RuleSelectedStores.Remove(rss);
                }
                db.SaveChanges(currentUser.NetworkID);
            }

            return Json("Success");
        }

        /// <summary>
        /// Add 'and','or','not', '(', ')' to rule list
        /// </summary>
        public ActionResult AddConjunction(string value, long ruleSetID, string ruleType)
        {
            Rule newRule = new Rule()
            {
                RuleSetID = ruleSetID,
                Compare = value.Trim(),
                Sort = db.Rules.Where(r => r.RuleSetID == ruleSetID).Count() + 1
            };

            db.Rules.Add(newRule);
            db.SaveChanges();

            Session["rulesetid"] = -1;
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// delete all rules
        /// </summary>
        public ActionResult ClearRules(string value, long ruleSetID, string ruleType)
        {
            ClearRules(ruleSetID);
            Session["rulesetid"] = -1;
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This will delete all the rules for a ruleset and the RuleSelectedStores
        /// </summary>
        /// <param name="ruleSetID"></param>
        private void ClearRules(long ruleSetID)
        {
            RuleDAO dao = new RuleDAO();

            List<Rule> rules = dao.GetRulesForRuleSet(ruleSetID);
            
            foreach (Rule rule in rules)
            {
                dao.Delete(rule);
            }

            RuleSet rs = dao.GetRuleSet(ruleSetID);
            if (rs.Type == "SizeAlc")
            {
                //delete all the ruleselected stores
                foreach (RuleSelectedStore rss in db.RuleSelectedStores.Where(r => r.RuleSetID == ruleSetID).ToList())
                {
                    db.RuleSelectedStores.Remove(rss);
                }

                db.SaveChanges(currentUser.NetworkID);
            }

            Session["rulesetid"] = -1;
        }

        [HttpPost]
        public ActionResult GetStoreCount(long ruleSetID)
        {
            RuleDAO dao = new RuleDAO();
            List<StoreLookup> storesInRule;

            storesInRule = dao.GetStoresForRules(ruleSetID, currentUser, AppName);
            List<Rule> ruleList = db.Rules.Where(r => r.RuleSetID == ruleSetID).OrderBy(r => r.Sort).ToList();

            bool closedStoreRule = ruleList.Any(a => a.Field == "status" && a.Value == "C");

            try
            {
                storesInRule = storesInRule.Where(r => r.status != "C" || closedStoreRule).ToList();

                int count = storesInRule.Count();
                if (storesInRule != null)
                {                    
                    var divisions = currentUser.GetUserDivisions();                    
                    count = (from a in storesInRule
                             join b in divisions 
                             on a.Division equals b.DivCode 
                             select a).Count();
                }

                return Json(new { Count = count });
            }
            catch
            {
                return Json("error");
            }
        }

        /// <summary>
        /// return the stores that were actually added to the ruleset
        /// </summary>
        /// <param name="ruleSetID"></param>
        /// <returns></returns>
        public List<StoreLookupModel> GetStoresInRuleSet(long ruleSetID)
        {
            List<StoreLookupModel> list = new List<StoreLookupModel>();
            RuleDAO dao = new RuleDAO();
            RangePlanDAO rangePlanDAO = new RangePlanDAO();
            RuleSet rs = dao.GetRuleSet(ruleSetID);
            
            long rangePlanID = 0;
            if (rs.Type == "SizeAlc" || rs.Type == "Main")
                rangePlanID = rs.PlanID.Value;

            foreach (StoreLookup s in dao.GetRuleSelectedStoresInRuleSet(ruleSetID))
            {
                if (s.status != "C" || rs.Type == "rdq")
                {
                    if (rangePlanID > 0)
                    {
                        RangePlanDetail detail = rangePlanDAO.GetRangePlanDetail(s.Division, s.Store, rangePlanID);
                        list.Add(new StoreLookupModel(s, rangePlanID, detail != null));
                    }
                    else                    
                        list.Add(new StoreLookupModel(s));                    
                }
            }

            return list;
        }

        /// <summary>
        /// return the stores that were actually added to the ruleset
        /// </summary>
        /// <param name="ruleSetID"></param>
        /// <returns></returns>
        public ActionResult GetRuleSetStoreCount(long ruleSetID)
        {
            RuleDAO dao = new RuleDAO();
            List<StoreLookup> selectedStores = dao.GetRuleSelectedStoresInRuleSet(ruleSetID);
            List<StoreLookupModel> filteredStores = GetStoresForRules(ruleSetID);
            int count = (from a in selectedStores 
                         join b in filteredStores 
                         on new { a.Division, a.Store } equals new { b.Division, b.Store } 
                         select a).Count();

            return Json(new { Count = count });
        }

        /// <summary>
        /// return the stores that qualify for the current rules on a ruleset
        /// </summary>
        /// <param name="ruleSetID"></param>
        /// <returns></returns>
        public List<StoreLookupModel> GetStoresForRules(long ruleSetID)
        {
            RuleDAO dao = new RuleDAO();
            RangePlanDAO rangePlanDAO = new RangePlanDAO();
            List<StoreLookup> storesInRuleList;

            List<RangePlanDetail> rangePlanDetailStores = null;

            storesInRuleList = dao.GetStoresForRules(ruleSetID, currentUser, AppName);  // this is gettng the stores as defined by rules            

            List<Rule> ruleList = dao.GetRulesForRuleSet(ruleSetID);

            bool closedStoreRule = ruleList.Any(a => a.Field == "status" && a.Value == "C"); // are the rules looking for closed stores?

            try
            {
                List<StoreLookupModel> retList = new List<StoreLookupModel>();
                long rangePlanID = 0;

                RuleSet rs = dao.GetRuleSet(ruleSetID);
                if (rs.PlanID.HasValue)
                {
                    rangePlanID = rs.PlanID.Value;

                    rangePlanDetailStores = rangePlanDAO.GetRangePlanDetails(rangePlanID);

                    // of the stores defined in the rules, which ones are already in the range and which aren't
                    List<StoreLookup> inPlan = storesInRuleList.Where(rls => rangePlanDetailStores.Any(rpds => rpds.Division == rls.Division && rpds.Store == rls.Store)).ToList();
                    List<StoreLookup> notInPlan = storesInRuleList.Where(rls => !rangePlanDetailStores.Any(rpds => rpds.Division == rls.Division && rpds.Store == rls.Store)).ToList();

                    foreach (StoreLookup store in inPlan)
                    {
                        // if looking for closed stores, add all of stores regardless of status, otherwise only add non-closed ones
                        if (store.status != "C" || closedStoreRule)
                            retList.Add(new StoreLookupModel(store, rangePlanID, true));
                    }

                    foreach (StoreLookup store in notInPlan)
                    {
                        // if looking for closed stores, add all of stores regardless of status, otherwise only add non-closed ones
                        if (store.status != "C" || closedStoreRule)
                            retList.Add(new StoreLookupModel(store, rangePlanID, false));
                    }
                }
                else
                {
                    foreach (StoreLookup store in storesInRuleList)
                    {
                        // if looking for closed stores, add all of stores regardless of status, otherwise only add non-closed ones
                        if (store.status != "C" || closedStoreRule)                        
                            retList.Add(new StoreLookupModel(store));                        
                    }
                }

                if (retList != null)
                {
                    // only return stores that the user has divisional privs for
                    retList = (from a in retList 
                               join b in currentUser.GetUserDivisions() 
                               on a.Division equals b.DivCode 
                               select a).ToList();
                }

                Session["rulesetid"] = ruleSetID;
                Session["ruleselectedstores"] = retList;
                return retList;
            }
            catch
            {                
                // TODO: We should really think about, atleast, logging here.....
                return new List<StoreLookupModel>();
            }
        }

        public ActionResult _DeleteFromGrid(long id)
        {
            // Get specified Rule
            List<Rule> ruleList = db.Rules.Where(r => r.ID == id).ToList();

            if (ruleList.Count() == 0)            
                return Json("Already Deleted");
            
            Rule rule = ruleList.First();

            //  Get RuleSet type of Rule
            var ruleSetType = db.RuleSets.Where(rs => rs.RuleSetID == rule.RuleSetID).First().Type.ToUpper();

            // Delete the specified rule from its ruleset
            RuleDAO dao = new RuleDAO();
            long planID = -1;
            try
            {
                planID = dao.GetPlanID(id);
            }
            catch { }
            dao.Delete(rule);
            Session["rulesetid"] = -1;

            return Json("Success");
        }

        public ActionResult Delete(long id)
        {
            RuleDAO dao = new RuleDAO();

            Rule rule = db.Rules.Where(r => r.ID == id).First();
            RuleSet rs = dao.GetRuleSet(rule.RuleSetID);            
            
            long planID = -1;
            try
            {
                planID = dao.GetPlanID(id);
            }
            catch { }
            Session["rulesetid"] = -1;

            dao.Delete(rule);

            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        //
        // POST: /Account/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(long id, FormCollection form)
        {
            RuleDAO dao = new RuleDAO();
            try
            {
                Rule rule = db.Rules.Where(r => r.ID == id).First();
                RuleSet rs = dao.GetRuleSet(rule.RuleSetID);                
                
                long planID = dao.GetPlanID(id);
                dao.Delete(rule);
                Session["rulesetid"] = -1;

                return Json("Success");
            }
            catch
            {
                return Json("Failed");
            }
        }

        //
        // GET: /Account/Edit/5
        public ActionResult Edit(long id)
        {
            Rule rule = db.Rules.Where(r => r.ID == id).First();
            return View(rule);
        }

        //
        // POST: /Account/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, FormCollection form)
        {
            RuleDAO dao = new RuleDAO();
            try
            {
                Rule rule = db.Rules.Where(r => r.ID == id).First();
                long planID = dao.GetPlanID(id);

                TryUpdateModel(rule);

                if (ModelState.IsValid)                
                    db.SaveChanges();
                
                return RedirectToAction("AddStoresByRule", "SkuRange", new { planID });
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Up(long id)
        {
            Rule rule = db.Rules.Where(r => r.ID == id).First();
            RuleDAO ruleDAO = new RuleDAO();
            Rule ruleAbove = null;

            try
            {
                ruleAbove = db.Rules.Where(r => r.RuleSetID == rule.RuleSetID && r.Sort == rule.Sort - 1).First();
            }
            catch { }

            if (ruleAbove != null)
            {
                rule.Sort--;
                ruleAbove.Sort++;
                
                if (ModelState.IsValid)                
                    db.SaveChanges();                
            }

            RuleSet rs = ruleDAO.GetRuleSet(rule.RuleSetID);

            Session["rulesetid"] = -1;
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult Down(long id)
        {
            Rule rule = db.Rules.Where(r => r.ID == id).First();
            RuleDAO ruleDAO = new RuleDAO();
            Rule ruleBelow = null;

            try
            {
                ruleBelow = db.Rules.Where(r => r.RuleSetID == rule.RuleSetID && r.Sort == rule.Sort + 1).First();
            }
            catch { }

            if (ruleBelow != null)
            {
                rule.Sort++;
                ruleBelow.Sort--;
                
                if (ModelState.IsValid)                
                    db.SaveChanges();                
            }

            RuleSet rs = ruleDAO.GetRuleSet(rule.RuleSetID);            

            Session["rulesetid"] = -1;
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Add a store to the range plan (planID)
        /// </summary>
        public JsonResult AddStore(string store, string div, long ruleSetID)
        {
            RuleDAO ruleDAO = new RuleDAO();
            RangePlanDAO rangePlanDAO = new RangePlanDAO();

            try
            {
                ruleDAO.AddStoreToRuleset(div, store, ruleSetID, currentUser.NetworkID);

                //check if it's a rangeplan, save store their too.
                RuleSet rs = ruleDAO.GetRuleSet(ruleSetID);

                if (rs.Type == "Delivery")
                {
                    //clear the session
                    Session["rulesetid"] = -1;
                    //for PresentationQuantities page, need to reset the session vars so they reload
                    Session["pqAllocs"] = null;
                    Session["pqDeliveryGroups"] = null;

                    //delete it from any other groups                   
                    rangePlanDAO.RemoveStoreFromOtherDeliveryGroups(div, store, rs.PlanID.Value, ruleSetID);

                    MaxLeadTime lt = db.MaxLeadTimes.Where(mlt => mlt.Store == store && mlt.Division == div).FirstOrDefault();

                    if (lt == null)
                    {
                        lt = new MaxLeadTime()
                        {
                            Division = div,
                            Store = store,
                            LeadTime = 5
                        };
                    }

                    SizeAllocationDAO dao = new SizeAllocationDAO();
                    RangePlanDetail rangePlanDetail = rangePlanDAO.GetRangePlanDetail(div, store, rs.PlanID.Value);
                    DeliveryGroup dg = db.DeliveryGroups.Where(d => d.RuleSetID == ruleSetID).First();

                    if (rangePlanDetail != null)
                    {
                        //set start/end date
                        if (dg.StartDate != null)
                        {
                            rangePlanDetail.StartDate = ((DateTime)dg.StartDate).AddDays(lt.LeadTime);
                            db.Entry(rangePlanDetail).State = System.Data.EntityState.Modified;
                        }

                        if (dg.EndDate != null)
                            rangePlanDetail.EndDate = ((DateTime)dg.EndDate).AddDays(lt.LeadTime);                        
                    }
                }
                else if (rs.Type == "Main")
                {
                    if (rangePlanDAO.GetRangePlanDetail(div, store, rs.PlanID.Value) == null)                         
                    {
                        //add it if it's not already there
                        RangePlanDetail rpDet = new RangePlanDetail()
                        {
                            Division = div,
                            Store = store,
                            CreateDate = DateTime.Now,
                            CreatedBy = currentUser.NetworkID,
                            ID = rs.PlanID.Value
                        };

                        db.RangePlanDetails.Add(rpDet);                        
                    }
                }

                if (rs.PlanID != null && rs.Type != "SizeAlc")
                    db.UpdateRangePlanDate(rs.PlanID.Value, currentUser.NetworkID);

                db.SaveChanges(currentUser.NetworkID);                

                return Json("Success");
            }
            catch 
            {
                return Json("Error");
            }
        }

        /// <summary>
        /// Add a store to the range plan (planID)
        /// </summary>
        public JsonResult AddAllStores(long ruleSetID, bool verified, bool move)
        {
            RuleDAO ruleDAO = new RuleDAO();

            try
            {
                RuleSet rs = ruleDAO.GetRuleSet(ruleSetID);

                //if plan allows multiple rulesets of same type
                //we can tell if it's in another ruleset (for warning purposes)
                List<StoreLookup> storesInSamePlan = new List<StoreLookup>();

                if (rs.PlanID > 0)
                {
                    List<long> similarRulesets = (from a in db.RuleSets
                                                   where a.PlanID == rs.PlanID &&
                                                         a.Type == rs.Type &&
                                                         a.RuleSetID != rs.RuleSetID
                                                   select a.RuleSetID).ToList();
                    foreach (long similar in similarRulesets)
                    {
                        foreach (StoreLookup s in ruleDAO.GetRuleSelectedStoresInRuleSet(similar))
                        {
                            storesInSamePlan.Add(s);
                        }
                    }
                }

                List<StoreLookupModel> list = GetStoresForRules(ruleSetID);

                var currlist = from n in list
                               join c in storesInSamePlan
                               on new { n.Division, n.Store } equals new { c.Division, c.Store }
                               select n;

                if (!verified && currlist.Count() > 0)                
                    return Json("Verify");                
                
                if (!move)                
                    list = list.Where(p => !storesInSamePlan.Any(p2 => p2.Division == p.Division && p2.Store == p.Store)).ToList();
                
                //change from here down...
                List<StoreBase> dblist = new List<StoreBase>();
                StoreBase sb;
                foreach (StoreLookupModel m in list)
                {
                    sb = new StoreBase()
                    {
                        Division = m.Division,
                        Store = m.Store
                    };

                    dblist.Add(sb);
                }
                if (rs.PlanID != null)                
                    ruleDAO.AddStoresToRuleset(dblist, (long)rs.PlanID, ruleSetID);                
                else                
                    ruleDAO.AddStoresToRuleset(dblist, 0, ruleSetID);
                
                Session["rulesetid"] = -1;
                //for PresentationQuantities page, need to reset the session vars so they reload
                Session["pqAllocs"] = null;
                Session["pqDeliveryGroups"] = null;

                if (rs.PlanID != null && rs.Type != "SizeAlc")
                    db.UpdateRangePlanDate(rs.PlanID.Value, currentUser.NetworkID);                

                return Json("Success");
            }
            catch
            {
                return Json("Error");
            }
        }

        /// <summary>
        /// Add a store to the range plan (planID)
        /// </summary>
        [HttpPost]
        public ActionResult RemoveAllStoresForRules(long ruleSetID)
        {
            RuleDAO dao = new RuleDAO();
            RangePlanDAO rangePlanDAO = new RangePlanDAO();

            try            
            {
                RuleSet rs = dao.GetRuleSet(ruleSetID);

                List<RangePlanDetail> details = null;
                if (rs.Type == "Main")
                    details = rangePlanDAO.GetRangePlanDetails(rs.PlanID.Value);

                List<StoreLookupModel> list = GetStoresForRules(ruleSetID);
                List<StoreBase> dblist = new List<StoreBase>();
                StoreBase sb;

                foreach (StoreLookupModel m in list)
                {
                    sb = new StoreBase()
                    {
                        Division = m.Division,
                        Store = m.Store
                    };

                    dblist.Add(sb);
                }
                
                if (rs.PlanID != null)                
                    dao.RemoveStoresFromRuleset(dblist, rs.PlanID.Value, ruleSetID);                
                else                
                    dao.RemoveStoresFromRuleset(dblist, 0, ruleSetID);                

                Session["rulesetid"] = -1;
                //for PresentationQuantities page, need to reset the session vars so they reload
                Session["pqAllocs"] = null;
                Session["pqDeliveryGroups"] = null;

                if (rs.PlanID != null && rs.Type != "SizeAlc")
                    db.UpdateRangePlanDate(rs.PlanID.Value, currentUser.NetworkID);                

                return Json("Success");
            }
            catch 
            {
                return Json("Error");
            }
        }

        /// <summary>
        /// Deletes the store from the range plan (planID)
        /// </summary>
        public JsonResult DeleteStore(string store, string div, long ruleSetID)
        {
            RuleDAO ruleSetDAO = new RuleDAO();
            RangePlanDAO rangePlanDAO = new RangePlanDAO();

            try
            {
                var detQuery = db.RuleSelectedStores.Where(rss => rss.Store == store && rss.Division == div && rss.RuleSetID == ruleSetID);
                if (detQuery.Count() > 0)
                {
                    RuleSelectedStore det = detQuery.First();
                    db.RuleSelectedStores.Remove(det);
                    db.SaveChanges();
                }

                //check if it's a rangeplan, save store their too.
                RuleSet rs = ruleSetDAO.GetRuleSet(ruleSetID);    
                
                if (rs.Type == "Main")
                {
                    RangePlanDetail rpd = rangePlanDAO.GetRangePlanDetail(div, store, rs.PlanID.Value);
                    if (rpd != null)
                    {
                        db.RangePlanDetails.Remove(rpd);
                        db.SaveChanges();
                    }

                    var query = (from a in db.RuleSets 
                                  join b in db.RuleSelectedStores 
                                  on a.RuleSetID equals b.RuleSetID 
                                  where a.PlanID == rs.PlanID && 
                                        b.Division == div && 
                                        b.Store == store && 
                                        a.Type == "Delivery"
                                  select b);
                    if (query.Count() > 0)
                    {
                        //delete it
                        foreach (RuleSelectedStore rss in query)
                        {
                            db.RuleSelectedStores.Remove(rss);
                        }

                        db.SaveChanges();
                    }
                }
                else if (rs.Type == "Delivery")
                {
                    SizeAllocationDAO dao = new SizeAllocationDAO();
                    RangePlanDetail rangePlanDetail = rangePlanDAO.GetRangePlanDetail(div, store, rs.PlanID.Value);                                        
                    
                    if (rangePlanDetail != null)
                    {
                        rangePlanDetail.StartDate = null;
                        rangePlanDetail.EndDate = null;
                        db.Entry(rangePlanDetail).State = System.Data.EntityState.Modified;
                    }

                    db.SaveChanges(currentUser.NetworkID);
                    //make sure any session storage is cleared.
                    Session["pqDeliveryGroups"] = null;
                }

                if (rs.PlanID != null && rs.Type != "SizeAlc")                
                    db.UpdateRangePlanDate(rs.PlanID.Value, currentUser.NetworkID);                

                return Json("Success");
            }
            catch 
            {
                return Json("Error");
            }
        }

        /// <summary>
        /// Populates partial view (grid) with list of stores that are in rule, showing which are already in the range plan.
        /// </summary>
        /// <param name="ruleSetID"></param>
        /// <returns></returns>
        [GridAction]
        public ActionResult _StoreLookupList(long ruleSetID, string gridtype)
        {
            RuleDAO dao = new RuleDAO();
            List<StoreLookupModel> storeList;
            List<StoreLookupModel> currStores = new List<StoreLookupModel>();

            if (gridtype == "AllStores")
            {
                storeList = db.GetStoreLookupsForPlan(ruleSetID);
                storeList.AddRange(db.GetStoreLookupsNotInPlan(ruleSetID));
            }
            else
            {
                RuleSet ruleSet = dao.GetRuleSet(ruleSetID);

                if (ruleSet.PlanID.HasValue)
                    currStores = db.GetStoreLookupsForPlan(ruleSet.PlanID.Value); //these are the RangePlanDetail recs 
                else
                    currStores = GetStoresInRuleSet(ruleSetID);  // these are the RuleSelectedStores, unless there is a range plan, in which case it is also the RangePlanDetail recs

                try
                {
                    storeList = GetStoresForRules(ruleSetID);
                }
                catch (Exception)
                {
                    storeList = new List<StoreLookupModel>();
                }               

                var currlist = (from n in storeList
                               join c in currStores on new { n.Division, n.Store } equals new { c.Division, c.Store }
                               select n).ToList();

                var currentDeliveryGroupStores = db.RuleSelectedStores.Where(rss => rss.RuleSetID.Equals(ruleSet.RuleSetID)).ToList();

                var currList2 = (from a in currentDeliveryGroupStores
                                 join b in storeList
                                   on new { a.Division, a.Store }
                               equals new { b.Division, b.Store }
                                 select b).ToList();

                foreach (var store in currList2)
                {
                    store.InCurrentDeliveryGroup = true;
                }

                foreach (StoreLookupModel m in storeList)
                {
                    m.InCurrentPlan = false;
                }

                foreach (StoreLookupModel m in currlist)
                {
                    m.InCurrentPlan = true;
                }

                //if plan allows multiple rulesets of same type
                //we can tell if it's in another ruleset (for warning purposes)                
                List<StoreLookup> storesInSimilarplans = new List<StoreLookup>();
                if (ruleSet.PlanID > 0)
                {
                    List<long> similarRulesets = db.RuleSets.Where(r => r.PlanID == ruleSet.PlanID && 
                                                                        r.Type == ruleSet.Type && 
                                                                        r.RuleSetID != ruleSet.RuleSetID)
                                                            .Select(r => r.RuleSetID)
                                                            .ToList();

                    foreach (long similar in similarRulesets)
                    {
                        foreach (StoreLookup s in dao.GetRuleSelectedStoresInRuleSet(similar))
                        {
                            storesInSimilarplans.Add(s);
                        }
                    }
                }

                var currlist2 = (from n in storeList
                                join c in storesInSimilarplans 
                                on new { n.Division, n.Store } equals new { c.Division, c.Store }
                                select n).ToList();

                foreach (StoreLookupModel m in currlist2)
                {
                    m.InSimilarRuleSet = true;
                }
            }

            return PartialView(new GridModel(storeList));
        }

        /// <summary>
        /// An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public ActionResult UploadStores(IEnumerable<HttpPostedFileBase> attachments, long ruleSetID)
        {
            RuleStoreSpreadsheet ruleStoreSpreadsheet = new RuleStoreSpreadsheet(appConfig, new ConfigService());
            List<StoreBase> list = new List<StoreBase>();
            
            RuleDAO dao = new RuleDAO();
            RangePlanDAO rangePlanDAO = new RangePlanDAO();

            RuleSet rs = dao.GetRuleSet(ruleSetID);

            List<StoreLookupModel> StoresInRules = null;
            bool checkStore = true;
            if (rs.Type == "SizeAlc")
            {
                StoresInRules = new List<StoreLookupModel>();
                List<StoreLookup> rangePlanDetailStores = rangePlanDAO.GetStoreLookupsForPlan(rs.PlanID.Value);
                foreach (StoreLookup l in rangePlanDetailStores)
                {
                    StoresInRules.Add(new StoreLookupModel(l, rs.PlanID.Value, true));
                }

                //delete rules
                List<Rule> rules = dao.GetRulesForRuleSet(rs.RuleSetID);
                
                foreach (Rule rule in rules)
                {
                    dao.Delete(rule);
                }
            }
            else if (rs.Type == "Delivery")
            {
                StoresInRules = GetStoresForRules(ruleSetID);
            }
            else            
                checkStore = false;
            
            try
            {
                foreach (HttpPostedFileBase file in attachments)
                {
                    ruleStoreSpreadsheet.Save(file);

                    if (checkStore == false)
                    {
                        //we haven't initialized the stores list because we didn't know the division
                        //pull all stores for that division
                        ClearRules(ruleSetID);
                        Rule newRule = new Rule()
                        {
                            RuleSetID = ruleSetID,
                            Field = "Division",
                            Compare = "Equals",
                            Value = ruleStoreSpreadsheet.MainDivision
                        };

                        db.Rules.Add(newRule);
                        db.SaveChanges(currentUser.NetworkID);

                        StoresInRules = GetStoresForRules(ruleSetID);
                    }

                    foreach (StoreBase store in ruleStoreSpreadsheet.LoadedStores)
                    {
                        if (StoresInRules.Where(sir => sir.Division == store.Division && sir.Store == store.Store).Count() > 0)                                    
                            list.Add(store);                                    
                    }
                }

                if (list.Count > 0)
                {
                    RuleDAO ruleDAO = new RuleDAO();
                    ruleDAO.AddStoresToRuleset(list, rs.PlanID, ruleSetID);

                    //this is for the PresentationQuantities page, since we just updated the details, we'll remove what's saved in the session.
                    Session["pqAllocs"] = null;
                    Session["pqDeliveryGroups"] = null;
                }

                string returnMessage = "Upload complete, added " + list.Count() + " stores";
                
                Session["rulesetid"] = -1;

                if (rs.PlanID != null)                
                    db.UpdateRangePlanDate(rs.PlanID.Value, currentUser.NetworkID);
                
                return Content(returnMessage);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
    }
}