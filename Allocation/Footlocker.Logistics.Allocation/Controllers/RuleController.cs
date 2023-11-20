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
                case "StoreExtension.ConceptType.Name":
                    results = (from a in db.ConceptTypes where a.Name.StartsWith(text) select a.Name).Distinct();
                    return new JsonResult { Data = results.ToList() };
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
                case "StoreExtension.ConceptType.Name":
                    results = (from a in db.ConceptTypes select a.Name).Distinct().Take(5);
                    return new JsonResult { Data = results.ToList() };
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
            List<Rule> rules = db.GetRulesForPlan(ruleSetID, ruleType);
            return View(rules);
        }

        [GridAction]
        public ActionResult _Grid(long ruleSetID, string ruleType)
        {
            List<Rule> rules;

            rules = db.GetRulesForRuleSet(ruleSetID, ruleType);

            return View(new GridModel(rules));
        }

        public ActionResult GridForPlan(long planID, string ruleType)
        {
            List<Rule> rules = db.GetRulesForPlan(planID, ruleType);
            return View(rules);
        }

        [GridAction]
        public ActionResult _GridForPlan(long planID, string ruleType)
        {
            List<Rule> rules;

            rules = db.GetRulesForPlan(planID, ruleType);
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

            IOrderedEnumerable<Rule> maxSort = (from a in db.Rules 
                                                where a.RuleSetID == ruleSetID 
                                                orderby sort descending 
                                                select a).ToList().OrderByDescending(o => o.Sort);
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

        private void ClearRules(long ruleSetID)
        {
            List<Rule> rules = db.Rules.Where(r => r.RuleSetID == ruleSetID).ToList();
            RuleDAO dao = new RuleDAO();
            foreach (Rule rule in rules)
            {
                dao.Delete(rule);
            }

            RuleSet rs = db.RuleSets.Where(r => r.RuleSetID == ruleSetID).First();
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
            IQueryable<StoreLookup> queryableData = db.StoreLookups.AsQueryable<StoreLookup>();
            ParameterExpression pe = Expression.Parameter(typeof(StoreLookup), "StoreLookup");
            List<Rule> list = (from a in db.Rules where a.RuleSetID == ruleSetID orderby a.Sort select a).ToList();
            List<Rule> finalRules = new List<Rule>();

            RuleSet rs = ((from a in db.RuleSets where a.RuleSetID == ruleSetID select a).First());
            var divQuery = (from a in db.RuleSets join b in db.RangePlans on a.PlanID equals b.Id where (a.RuleSetID == ruleSetID) select b);
            string div = "";
            if (rs.Division != null)
            {
                div = rs.Division;
            }
            else if (divQuery.Count() > 0)
            {
                div = divQuery.First().Division;
            }

            if (div != "")
            {
                //add division criteria to rules
                Rule divRule = new Rule()
                {
                    Compare = "Equals",
                    Field = "Division",
                    Value = div
                };

                finalRules.Add(divRule);

                divRule = new Rule()
                {
                    Compare = "and"
                };
               
                finalRules.Add(divRule);

                foreach (Rule r in list)
                {
                    finalRules.Add(r);
                }
            }
            else            
                finalRules = list;            

            RuleDAO dao = new RuleDAO();
            bool closedStoreRule = finalRules.Any(a => a.Field == "status" && a.Value == "C");

            try
            {
                Expression finalExpression = dao.GetExpression(finalRules, pe, currentUser.GetUserDivisionsString(AppName));
                // Create an expression tree that represents the expression 

                MethodCallExpression whereCallExpression = Expression.Call(
                    typeof(Queryable),
                    "Where",
                    new Type[] { queryableData.ElementType },
                    queryableData.Expression,
                    Expression.Lambda<Func<StoreLookup, bool>>(finalExpression, new ParameterExpression[] { pe }));
                // ***** End Where ***** 

                IQueryable<StoreLookup> results = queryableData.Provider.CreateQuery<StoreLookup>(whereCallExpression);
                results = (from a in results where ((a.status != "C") || (closedStoreRule)) select a);
                int count = results.Count();
                if (results != null)
                {
                    var divisions = (from a in currentUser.GetUserDivisions(AppName) select a);
                    List<StoreLookup> lresults = results.ToList();
                    count = (from a in lresults join b in currentUser.GetUserDivisions(AppName) on a.Division equals b.DivCode select a).Count();
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
            RuleSet rs = dao.GetRuleSet(ruleSetID);
            
            long? rangePlan = null;
            if (rs.Type == "SizeAlc" || rs.Type == "Main")
                rangePlan = rs.PlanID;            

            foreach (StoreLookup s in dao.GetStoresInRuleSet(ruleSetID))
            {
                if (s.status != "C" || rs.Type == "rdq")
                {
                    if (rangePlan != null && rangePlan > 0)
                    {
                        list.Add(new StoreLookupModel(s, (long)rangePlan, (from a in db.RangePlanDetails 
                                                                           where a.ID == rangePlan && 
                                                                                 a.Store == s.Store && 
                                                                                 a.Division == s.Division 
                                                                           select a).Count() > 0));
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
            List<StoreLookup> selectedStores = dao.GetStoresInRuleSet(ruleSetID);
            List<StoreLookupModel> filteredStores = GetStoresForRules(ruleSetID);
            int count = (from a in selectedStores join b in filteredStores on new { a.Division, a.Store } equals new { b.Division, b.Store } select a).Count();
            return Json(new { Count = count });
        }


        public List<StoreLookupModel> GetAllStores()
        {
            List<StoreLookupModel> retlist = new List<StoreLookupModel>();
            var stores = (from a in db.StoreLookups where a.status != "C" select a);
            foreach (StoreLookup item in stores)
            {
                retlist.Add(new StoreLookupModel(item));
            }
            return retlist;
        }
        /// <summary>
        /// return the stores that qualify for the current rules on a ruleset
        /// </summary>
        /// <param name="ruleSetID"></param>
        /// <returns></returns>
        public List<StoreLookupModel> GetStoresForRules(long ruleSetID, bool useCache = true)
        {
            RuleDAO dao = new RuleDAO();

            IQueryable<StoreLookup> queryableData = db.StoreLookups.Include("StoreExtension")
                                                                   .Include("StoreExtension.ConceptType")
                                                                   .Include("StoreExtension.CustomerType")
                                                                   .Include("StoreExtension.PriorityType")
                                                                   .Include("StoreExtension.StrategyType")
                                                                   .AsQueryable<StoreLookup>();

            ParameterExpression pe = Expression.Parameter(typeof(StoreLookup), "StoreLookup");
            List<Rule> list = (from a in db.Rules where a.RuleSetID == ruleSetID orderby a.Sort select a).ToList();

            List<Rule> finalRules = new List<Rule>();

            RuleSet rs = dao.GetRuleSet(ruleSetID);            
            var divQuery = (from a in db.RuleSets 
                            join b in db.RangePlans 
                            on a.PlanID equals b.Id 
                            where a.RuleSetID == ruleSetID 
                            select b);
            string div = "";
            if (rs.Division != null)            
                div = rs.Division;            
            else if (divQuery.Count() > 0)            
                div = divQuery.First().Division;            

            if (div != "")
            {
                //add division criteria to rules
                Rule divRule = new Rule()
                {
                    Compare = "Equals",
                    Field = "Division",
                    Value = div
                };

                finalRules.Add(divRule);

                divRule = new Rule()
                {
                    Compare = "and"
                };
                
                finalRules.Add(divRule);

                foreach (Rule r in list)
                {
                    finalRules.Add(r);
                }
            }
            else            
                finalRules = list;            

            if (rs.Type == "Delivery")
            {
                //add rules to only pull back stores in the range plan if this is a Delivery type.
                Rule deliveryRule = new Rule()
                {
                    Compare = "Equals",
                    Field = "RangePlanID",
                    Value = Convert.ToString(rs.PlanID)
                };

                if (finalRules.Count() == 2)
                {
                    //no user rules
                    //only have default div rules, so just add this one and show all possible stores
                    finalRules.Add(deliveryRule);
                }
                else
                {
                    finalRules.Insert(0, deliveryRule);
                    deliveryRule = new Rule()
                    {
                        Compare = "and"
                    };
                    
                    finalRules.Insert(1, deliveryRule);

                    deliveryRule = new Rule()
                    {
                        Compare = "("
                    };
                    
                    finalRules.Insert(2, deliveryRule);

                    deliveryRule = new Rule()
                    {
                        Compare = ")"
                    };
                    
                    finalRules.Add(deliveryRule);
                }
            }

            bool closedStoreRule = finalRules.Any(a => a.Field == "status" && a.Value == "C");
            
            try
            {
                Expression finalExpression = dao.GetExpression(finalRules, pe, currentUser.GetUserDivisionsString(AppName));

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
                List<StoreLookupModel> retList = new List<StoreLookupModel>();
                long? rangePlan = null;
                if (rs.Type == "SizeAlc" || rs.Type == "Main")
                {
                    rangePlan = rs.PlanID;
                }

                List<RangePlanDetail> planStores = null;
                if ((rangePlan != null) && (rangePlan > 0))
                {
                    planStores = (from a in db.RangePlanDetails where a.ID == rangePlan select a).ToList();
                }
                List<StoreLookup> resultList = results.ToList();

                if (rangePlan != null && rangePlan > 0)
                {
                    List<StoreLookup> inPlan = resultList.Where(p => planStores.Any(p2 => ((p2.Division == p.Division) && (p2.Store == p.Store)))).ToList();
                    List<StoreLookup> notInPlan = resultList.Where(p => !planStores.Any(p2 => ((p2.Division == p.Division) && (p2.Store == p.Store)))).ToList();
                    
                    foreach (StoreLookup item in inPlan)
                    {
                        if (item.status != "C" || closedStoreRule)                        
                            retList.Add(new StoreLookupModel(item, (long)rangePlan, true));                        
                    }

                    foreach (StoreLookup item in notInPlan)
                    {
                        if (item.status != "C" || closedStoreRule)
                            retList.Add(new StoreLookupModel(item, (long)rangePlan, false));                        
                    }
                }
                else
                {
                    foreach (StoreLookup item in results)
                    {
                        if (item.status != "C" || closedStoreRule)                        
                            retList.Add(new StoreLookupModel(item));                        
                    }
                }

                if (retList != null)
                {
                    retList = (from a in retList 
                               join b in currentUser.GetUserDivisions(AppName) 
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
            var ruleQuery = (from a in db.Rules where a.ID == id select a);
            if (ruleQuery.Count() == 0)
            {
                return Json("Already Deleted");
            }
            Rule rule = ruleQuery.First();

            //  Get RuleSet type of Rule
            var ruleSetType = (from a in db.RuleSets where a.RuleSetID == rule.RuleSetID select a).First().Type.ToUpper();

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

            Rule rule = (from a in db.Rules where a.ID == id select a).First();
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
        public ActionResult Delete(long id, FormCollection form)
        {
            RuleDAO dao = new RuleDAO();
            try
            {
                // TODO: Add delete logic here
                Rule rule = (from a in db.Rules where a.ID == id select a).First();
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
            Rule rule = (from a in db.Rules where a.ID == id select a).First();
            return View(rule);
        }

        //
        // POST: /Account/Edit/5
        [HttpPost]
        public ActionResult Edit(long id, FormCollection form)
        {
            try
            {
                // TODO: Add update logic here
                Rule rule = (from a2 in db.Rules where a2.ID == id select a2).First();
                long planID = (new RuleDAO()).GetPlanID(id);

                TryUpdateModel(rule);

                if (ModelState.IsValid)                
                    db.SaveChanges();
                
                return RedirectToAction("AddStoresByRule", "SkuRange", new { planID = planID });
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Up(long id)
        {
            Rule rule = (from a in db.Rules where a.ID == id select a).First();
            RuleDAO ruleDAO = new RuleDAO();
            Rule ruleAbove = null;

            try
            {
                ruleAbove = (from b in db.Rules where b.RuleSetID == rule.RuleSetID && b.Sort == (rule.Sort - 1) select b).First();
            }
            catch { }

            if (ruleAbove != null)
            {
                rule.Sort = rule.Sort - 1;
                ruleAbove.Sort = ruleAbove.Sort + 1;
                
                if (ModelState.IsValid)                
                    db.SaveChanges();                
            }

            RuleSet rs = ruleDAO.GetRuleSet(rule.RuleSetID);

            Session["rulesetid"] = -1;
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult Down(long id)
        {
            Rule rule = (from a in db.Rules where a.ID == id select a).First();
            RuleDAO ruleDAO = new RuleDAO();
            Rule ruleBelow = null;

            try
            {
                ruleBelow = (from b in db.Rules where ((b.RuleSetID == rule.RuleSetID) && (b.Sort == (rule.Sort + 1))) select b).First();
            }
            catch { }

            if (ruleBelow != null)
            {
                rule.Sort = rule.Sort + 1;
                ruleBelow.Sort = ruleBelow.Sort - 1;
                
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
                List<StoreLookup> storesInSimilarplans = new List<StoreLookup>();

                if (rs.PlanID > 0)
                {
                    List<long> similarRulesets = (from a in db.RuleSets
                                                   where a.PlanID == rs.PlanID &&
                                                         a.Type == rs.Type &&
                                                         rs.RuleSetID != a.RuleSetID
                                                   select a.RuleSetID).ToList();
                    foreach (long similar in similarRulesets)
                    {
                        foreach (StoreLookup s in ruleDAO.GetStoresInRuleSet(similar))
                        {
                            storesInSimilarplans.Add(s);
                        }
                    }
                }

                List<StoreLookupModel> list = GetStoresForRules(ruleSetID);

                var currlist = from n in list
                               join c in storesInSimilarplans
                               on new { n.Division, n.Store } equals new { c.Division, c.Store }
                               select n;

                if (!verified && currlist.Count() > 0)                
                    return Json("Verify");                
                
                if (!move)                
                    list = list.Where(p => !storesInSimilarplans.Any(p2 => p2.Division == p.Division && p2.Store == p.Store)).ToList();
                
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

            try            
            {
                RuleSet rs = dao.GetRuleSet(ruleSetID);

                List<RangePlanDetail> details = null;
                if (rs.Type == "Main")                
                    details = db.RangePlanDetails.Where(rpd => rpd.ID == rs.PlanID).ToList();                

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
            List<StoreLookupModel> list;

            if (gridtype == "AllStores")
            {
                list = db.GetStoreLookupsForPlan(ruleSetID, currentUser.GetUserDivisionsString(AppName));
                list.AddRange(db.GetStoreLookupsNotInPlan(ruleSetID, currentUser.GetUserDivisionsString(AppName)));
            }
            else
            {                
                try
                {
                    list = GetStoresForRules(ruleSetID);
                }
                catch (Exception)
                {
                    list = new List<StoreLookupModel>();
                }

                RuleSet ruleSet = dao.GetRuleSet(ruleSetID);
                List<StoreLookupModel> currStores = new List<StoreLookupModel>();
                if (ruleSet.PlanID != null)                
                    currStores = db.GetStoreLookupsForPlan((long)ruleSet.PlanID, currentUser.GetUserDivisionsString(AppName));                
                else                
                    currStores = GetStoresInRuleSet(ruleSetID);                

                var currlist = from n in list
                               join c in currStores on new { n.Division, n.Store } equals new { c.Division, c.Store }
                               select n;

                var currentDeliveryGroupStores = db.RuleSelectedStores.Where(rss => rss.RuleSetID.Equals(ruleSet.RuleSetID)).ToList();

                var currList2 = (from a in currentDeliveryGroupStores
                                 join b in list
                                   on new { Division = a.Division, Store = a.Store }
                               equals new { Division = b.Division, Store = b.Store }
                                 select b).ToList();

                foreach (var store in currList2)
                {
                    store.InCurrentDeliveryGroup = true;
                }

                foreach (StoreLookupModel m in list)
                {
                    m.InCurrentPlan = false;
                }

                foreach (StoreLookupModel m in currlist)
                {
                    m.InCurrentPlan = true;
                }

                //if plan allows multiple rulesets of same type
                //we can tell if it's in another ruleset (for warning purposes)
                RuleSet rs = dao.GetRuleSet(ruleSetID);                
                List<StoreLookup> storesInSimilarplans = new List<StoreLookup>();
                if (rs.PlanID > 0)
                {
                    List<long> similarRulesets = (from a in db.RuleSets where a.PlanID == rs.PlanID && a.Type == rs.Type && rs.RuleSetID != a.RuleSetID select a.RuleSetID).ToList();
                    foreach (long similar in similarRulesets)
                    {
                        foreach (StoreLookup s in dao.GetStoresInRuleSet(similar))
                        {
                            storesInSimilarplans.Add(s);
                        }
                    }
                }

                var currlist2 = from n in list
                                join c in storesInSimilarplans on new { n.Division, n.Store } equals new { c.Division, c.Store }
                                select n;

                foreach (StoreLookupModel m in currlist2)
                {
                    m.InSimilarRuleSet = true;
                }
            }
            return PartialView(new GridModel(list));
        }

        /// <summary>
        /// An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public ActionResult UploadStores(IEnumerable<HttpPostedFileBase> attachments, long ruleSetID)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            int errors = 0;
            string rangeType;
            List<StoreBase> list = new List<StoreBase>();
            
            RuleDAO dao = new RuleDAO();

            RuleSet rs = dao.GetRuleSet(ruleSetID);

            List<StoreLookupModel> StoresInRules = null;
            bool checkStore = true;
            if (rs.Type == "SizeAlc")
            {
                StoresInRules = new List<StoreLookupModel>();
                foreach (StoreLookup l in (from a in db.RangePlanDetails 
                                           join b in db.StoreLookups 
                                           on new { a.Division, a.Store } equals new { b.Division, b.Store } 
                                           where a.ID == rs.PlanID 
                                           select b))
                {
                    StoresInRules.Add(new StoreLookupModel(l, (long)rs.PlanID, true));
                }

                //delete rules
                List<Rule> rules = db.Rules.Where(r => r.RuleSetID == rs.RuleSetID).ToList();
                
                foreach (Rule rule in rules)
                {
                    dao.Delete(rule);
                }
            }
            else if (rs.Type == "Delivery")
            {
                StoresInRules = GetStoresForRules(ruleSetID, true);
            }
            else            
                checkStore = false;
            
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
                            Value = mySheet.Cells[row, 0].Value.ToString().PadLeft(2, '0')
                        };

                        db.Rules.Add(newRule);
                        db.SaveChanges(currentUser.NetworkID);
                        StoresInRules = GetStoresForRules(ruleSetID, false);
                    }

                    if ((mySheet.Cells[0, 0].Value.ToString().Contains("Div")) && (mySheet.Cells[0, 1].Value.ToString().Contains("Store")))
                    {
                        while (mySheet.Cells[row, 0].Value != null)
                        {
                            try
                            {
                                StoreBase newDet = new StoreBase();
                                newDet.Division = mySheet.Cells[row, 0].Value.ToString().PadLeft(2, '0');
                                newDet.Store = mySheet.Cells[row, 1].Value.ToString().PadLeft(5, '0');
                                rangeType = mySheet.Cells[row, 2].StringValue;

                                if (rangeType == "ALR" || rangeType == "OP")                                
                                    newDet.RangeType = rangeType;                                

                                //always default to "Both"
                                if (string.IsNullOrEmpty(newDet.RangeType))                                
                                    newDet.RangeType = "Both";                                

                                if (newDet.Store != "00000")
                                {
                                    if (StoresInRules.Where(sir => sir.Division == newDet.Division && sir.Store == newDet.Store).Count() > 0)                                    
                                        list.Add(newDet);                                    
                                }
                            }
                            catch 
                            {
                                errors++;
                            }
                            row++;
                        }
                    }
                    else                    
                        return Content("Incorrect header, first column must be \"Div\", next \"Store\".");                    
                }

                if (list.Count > 0)
                {
                    RuleDAO ruleDAO = new RuleDAO();
                    ruleDAO.AddStoresToRuleset(list, rs.PlanID, ruleSetID);
                    //this is for the PresentationQuantities page, since we just updated the details, we'll remove what's 
                    //saved in the session.
                    Session["pqAllocs"] = null;
                    Session["pqDeliveryGroups"] = null;
                }

                string returnMessage = "Upload complete, added " + list.Count() + " stores";

                if (errors > 0)                
                    returnMessage += ", " + errors + " errors";
                
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