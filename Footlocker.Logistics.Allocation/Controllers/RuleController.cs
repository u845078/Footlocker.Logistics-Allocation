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
        //
        // GET: /Account/Delete/5

        //[HttpPost]
        //public ActionResult _AutoCompleteFilteringAjax2(string text, string type)
        //{
        //    switch (type)
        //    {
        //        case "Store":
        //            IQueryable<StoreLookup> stores = db.StoreLookups.AsQueryable();
        //            stores = stores.Where((p) => p.Store.StartsWith(text));
        //            return new JsonResult
        //            {
        //                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
        //                Data = new
        //                    SelectList(stores.ToList(), "Store", "Store")
        //            };
        //        case "State":
        //            IQueryable<String> states = (from a in db.StoreLookups where a.State.StartsWith(text) select a.State).Distinct();

        //            return new JsonResult
        //            {
        //                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
        //                Data = new
        //                    SelectList(states.ToList())
        //            };

        //        //return new JsonResult { Data = stores.Select(p => p.Store) };
        //    }

        //    return new JsonResult
        //    {
        //        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
        //        Data = new
        //            SelectList(new List<StoreLookup>(), "Store", "Store")
        //    }; 
        //}
        
        [HttpPost]
        public ActionResult _AutoCompleteFilteringAjax(string text, string type)
        {
            IQueryable<String> results;
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
                    return new JsonResult { Data = new List<String>() };
                case "StorePlan":
                    if (text.Length > 3)
                    {
                        results = (from a in db.StorePlans where a.PlanName.StartsWith(text) select a.PlanName).Distinct();
                        return new JsonResult { Data = results.ToList() };
                    }
                    else
                    { 
                        return new JsonResult { Data = new List<String>() }; 
                    }
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

            //return new JsonResult
            //{
            //    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            //    Data = new
            //        SelectList(new List<StoreLookup>(), "Store", "Store")
            //};

        }

        [HttpPost]
        public ActionResult GetExamples(string type)
        {
            IQueryable<String> results;
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
            return new JsonResult { Data = new List<String>() };

        }


        public ActionResult Grid(Int64 ruleSetID, string ruleType)
        {
            List<Rule> rules = db.GetRulesForPlan(ruleSetID, ruleType);
            return View(rules);
        }

        [GridAction]
        public ActionResult _Grid(Int64 ruleSetID, string ruleType)
        {
            List<Rule> rules;

            //if (ruleType == "hold")
            //{
                rules = db.GetRulesForRuleSet(ruleSetID, ruleType);            
            //}
            //else
            //{
            //    rules = db.GetRulesForPlan(ruleSetID, ruleType);
            //}
            return View(new GridModel(rules)); 
        }

        public ActionResult GridForPlan(Int64 planID, string ruleType)
        {
            List<Rule> rules = db.GetRulesForPlan(planID, ruleType);
            return View(rules);
        }

        [GridAction]
        public ActionResult _GridForPlan(Int64 planID, string ruleType)
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
        public ActionResult AddRule(Int64 ruleSetID, string field, string compare,string value, int sort)
        {
            Rule newRule = new Rule();
            newRule.RuleSetID = ruleSetID;
            newRule.Field = field;
            newRule.Compare = compare;
            newRule.Value = value;
            IOrderedEnumerable<Rule> maxSort = (from a in db.Rules where a.RuleSetID == ruleSetID orderby sort descending select a).ToList().OrderByDescending(o => o.Sort);
            if (maxSort.Count() > 0)
            {
                
                Rule lastRule = maxSort.First(); 
                sort = lastRule.Sort;
                //add an and rule if there isn't one
                if ((lastRule.Compare != "and") && (lastRule.Compare != "or") && (lastRule.Compare != "not"))
                { 
                    //add an and
                    Rule andRule = new Rule();
                    andRule.RuleSetID = newRule.RuleSetID;
                    andRule.Compare = "and";
                    foreach (Rule r in maxSort)
                    {
                        if ((r.Field != null) && (r.Field != ""))
                        {
                            if (r.Field == newRule.Field)
                            {
                                andRule.Compare = "or";
                            }
                            break;                        
                        }
                    }
                    andRule.Sort = sort+1;
                    sort++;
                    db.Rules.Add(andRule);
                    db.SaveChanges();

                }
            }
            else
            {
                sort=0;
            }
            newRule.Sort = sort+1;

            if (ModelState.IsValid)
            {
                db.Rules.Add(newRule);
                db.SaveChanges();
            }
            RuleSet rs = (from a in db.RuleSets where a.RuleSetID == newRule.RuleSetID select a).First();
            //List<Rule> rules = (from a in db.Rules where a.RuleSetID == newRule.RuleSetID select a).ToList();
            Session["rulesetid"] = -1;

            if (rs.Type == "SizeAlc")
            {
                //delete all the ruleselected stores, because they are using rules and not a spreadsheet upload
                foreach (RuleSelectedStore rss in (from a in db.RuleSelectedStores where a.RuleSetID == ruleSetID select a))
                {
                    db.RuleSelectedStores.Remove(rss);
                }
                db.SaveChanges(UserName);
            }

            return Json("Success");
        }

        /// <summary>
        /// Add 'and','or','not', '(', ')' to rule list
        /// </summary>
        public ActionResult AddConjunction(string value, Int64 ruleSetID, string ruleType)
        {
            Rule newRule = new Rule();
            //if (ruleType == "hold")
            //{
                newRule.RuleSetID = ruleSetID;
            //}
            //else
            //{
            //    newRule.RuleSetID = (new RuleDAO()).GetRuleSetID(ruleSetID, ruleType, User.Identity.Name);
            //}
            newRule.Compare = value.Trim();
            newRule.Sort = (from r in db.Rules where r.RuleSetID == newRule.RuleSetID select r).Count() + 1;

            db.Rules.Add(newRule);
            db.SaveChanges();

            Session["rulesetid"] = -1;
            return Json("Success", JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// delete all rules
        /// </summary>
        public ActionResult ClearRules(string value, Int64 ruleSetID, string ruleType)
        {
            Rule newRule = new Rule();

            //if (ruleType != "hold")
            //{
            //    ruleSetID = (new RuleDAO()).GetRuleSetID(ruleSetID, ruleType, User.Identity.Name);
            //}
            ClearRules(ruleSetID);
            Session["rulesetid"] = -1;
            return Json("Success", JsonRequestBehavior.AllowGet);

        }

        private void ClearRules(Int64 ruleSetID)
        {
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
            }
            Session["rulesetid"] = -1;
        }

        [HttpPost]
        public ActionResult GetStoreCount(Int64 ruleSetID)
        {
            IQueryable<StoreLookup> queryableData = db.StoreLookups.AsQueryable<StoreLookup>();
            ParameterExpression pe = Expression.Parameter(typeof(StoreLookup), "StoreLookup");
            List<Rule> list = (from a in db.Rules where a.RuleSetID == ruleSetID orderby a.Sort select a).ToList();
            List<Rule> finalRules = new List<Rule>();

            RuleSet rs = ((from a in db.RuleSets where a.RuleSetID == ruleSetID select a).First());
            var divQuery = (from a in db.RuleSets join b in db.RangePlans on a.PlanID equals b.Id where (a.RuleSetID == ruleSetID) select b);
            string div="";
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
                Rule divRule = new Rule();
                divRule.Compare = "Equals";
                divRule.Field = "Division";
                divRule.Value = div;

                finalRules.Add(divRule);

                divRule = new Rule();
                divRule.Compare = "and";
                finalRules.Add(divRule);

                foreach (Rule r in list)
                {
                    finalRules.Add(r);
                }

            }
            else
            {
                finalRules = list;
            }

            RuleDAO dao = new RuleDAO();
            bool closedStoreRule = finalRules.Any(a => (a.Field == "status" && a.Value == "C"));

            try
            {
                Expression finalExpression = dao.GetExpression(finalRules, pe, DivisionList(UserName));
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
                results = (from a in results where ((a.status != "C") || (closedStoreRule)) select a);
                int count = results.Count();
                if (results != null)
                {
                    var divisions = (from a in this.Divisions() select a);
                    List<StoreLookup> lresults = results.ToList();
                    count = (from a in lresults join b in this.Divisions() on a.Division equals b.DivCode select a).Count();
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
        public List<StoreLookupModel> GetStoresInRuleSet(Int64 ruleSetID)
        {          
            //var results = (from a in db.StoreLookups join b in db.RuleSelectedStores on new { div = a.Division, store = a.Store } equals new { div = b.Division, store = b.Store } where (b.RuleSetID == ruleSetID) select a);
            List<StoreLookupModel> list = new List<StoreLookupModel>();
            RuleDAO dao = new RuleDAO();
            RuleSet rs = ((from a in db.RuleSets where a.RuleSetID == ruleSetID select a).First());
            long? rangePlan=null;
            if ((rs.Type == "SizeAlc") || (rs.Type == "Main"))
            {
                rangePlan = rs.PlanID;                
            }

            foreach (StoreLookup s in dao.GetStoresInRuleSet(ruleSetID))
            {
                if ((s.status != "C") || (rs.Type =="rdq"))
                {
                    if ((rangePlan != null) && (rangePlan > 0))
                    {
                        list.Add(new StoreLookupModel(s, (long)rangePlan, (from a in db.RangePlanDetails where ((a.ID == rangePlan) && (a.Store == s.Store) && (a.Division == s.Division)) select a).Count() > 0));
                    }
                    else
                    {
                        list.Add(new StoreLookupModel(s));
                    }
                }
            }

            return list;        
        }

        /// <summary>
        /// return the stores that were actually added to the ruleset
        /// </summary>
        /// <param name="ruleSetID"></param>
        /// <returns></returns>
        public ActionResult GetRuleSetStoreCount(Int64 ruleSetID)
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
        public List<StoreLookupModel> GetStoresForRules(Int64 ruleSetID, Boolean useCache=true)
        {
            if (useCache)
            {
                try
                {
                    if ((Session["rulesetid"] != null) &&
                        ((Int64)Session["rulesetid"] == ruleSetID))
                    {
                        return (List<StoreLookupModel>)Session["ruleselectedstores"];
                    }
                }
                catch { 
                //no cache, just continue
                }
            }
            IQueryable<StoreLookup> queryableData = 
                db.StoreLookups
                .Include("StoreExtension")
                .Include("StoreExtension.ConceptType")
                .Include("StoreExtension.CustomerType")
                .Include("StoreExtension.PriorityType")
                .Include("StoreExtension.StrategyType")
                .AsQueryable<StoreLookup>();


            ParameterExpression pe = Expression.Parameter(typeof(StoreLookup), "StoreLookup");
            List<Rule> list = (from a in db.Rules where a.RuleSetID == ruleSetID orderby a.Sort select a).ToList();

            List<Rule> finalRules = new List<Rule>();

            RuleSet rs = ((from a in db.RuleSets where a.RuleSetID == ruleSetID select a).First());
            var divQuery = (from a in db.RuleSets join b in db.RangePlans on a.PlanID equals b.Id where (a.RuleSetID == ruleSetID) select b);
            string div="";
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
                Rule divRule = new Rule();
                divRule.Compare = "Equals";
                divRule.Field = "Division";
                divRule.Value = div;

                finalRules.Add(divRule);

                divRule = new Rule();
                divRule.Compare = "and";
                finalRules.Add(divRule);

                foreach (Rule r in list)
                {
                    finalRules.Add(r);
                }

            }
            else
            {
                finalRules = list;
            }



            if (rs.Type == "Delivery")
            {
                //add rules to only pull back stores in the range plan if this is a Delivery type.
                Rule deliveryRule = new Rule();
                deliveryRule.Compare = "Equals";
                deliveryRule.Field = "RangePlanID";
                deliveryRule.Value = Convert.ToString(rs.PlanID);

                if (finalRules.Count() == 2)
                {
                    //no user rules
                    //only have default div rules, so just add this one and show all possible stores
                    finalRules.Add(deliveryRule);
                } 
                else
                {
                    finalRules.Insert(0, deliveryRule);
                    deliveryRule = new Rule();
                    deliveryRule.Compare = "and";
                    finalRules.Insert(1, deliveryRule);

                    deliveryRule = new Rule();
                    deliveryRule.Compare = "(";
                    finalRules.Insert(2, deliveryRule);

                    deliveryRule = new Rule();
                    deliveryRule.Compare = ")";
                    finalRules.Add(deliveryRule);
                }

            }
            bool closedStoreRule = finalRules.Any(a => (a.Field == "status" && a.Value == "C"));

            RuleDAO dao = new RuleDAO();
            try
            {
                Expression finalExpression = dao.GetExpression(finalRules, pe, DivisionList(UserName));

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
                long? rangePlan=null;
                if ((rs.Type == "SizeAlc") || (rs.Type == "Main"))
                {
                    rangePlan = rs.PlanID;
                }

                List<RangePlanDetail> planStores = null;
                if ((rangePlan != null) && (rangePlan > 0))
                {
                    planStores = (from a in db.RangePlanDetails where a.ID == rangePlan select a).ToList();
                }
                List<StoreLookup> resultList = results.ToList();
                if ((rangePlan != null) && (rangePlan > 0))
                {
                    List<StoreLookup> inPlan = resultList.Where(p => planStores.Any(p2 => ((p2.Division == p.Division) && (p2.Store == p.Store)))).ToList();
                    List<StoreLookup> notInPlan = resultList.Where(p => !planStores.Any(p2 => ((p2.Division == p.Division) && (p2.Store == p.Store)))).ToList();
                    foreach (StoreLookup item in inPlan)
                    {
                        if ((item.status != "C") || (closedStoreRule))
                        {
                            retList.Add(new StoreLookupModel(item, (long)rangePlan, true));
                        }
                    }
                    foreach (StoreLookup item in notInPlan)
                    {
                        if ((item.status != "C") || (closedStoreRule))
                        {
                            retList.Add(new StoreLookupModel(item, (long)rangePlan, false));
                        }
                    }
                }
                else
                {
                    foreach (StoreLookup item in results)
                    {
                        if ((item.status != "C") || (closedStoreRule))
                        {
                            retList.Add(new StoreLookupModel(item));
                        }
                    }
                }

                if (retList != null)
                {
                    retList = (from a in retList join b in this.Divisions() on a.Division equals b.DivCode select a).ToList();
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

        public ActionResult _DeleteFromGrid(Int64 id)
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
            Int64 planID=-1;
            try
            {
                planID = dao.GetPlanID(id);
            }
            catch { }
            dao.Delete(rule);
            Session["rulesetid"] = -1;
            
            // Return result based on rule set type
            //switch (ruleSetType)
            //{
            //    case "HOLD":
            return Json("Success");
            //    case "SIZEALC":
            //        return RedirectToAction("PresentationQuantities", "SkuRange", new { planID = planID });
            //    default:
            //        return RedirectToAction("AddStoresByRule", "SkuRange", new { planID = planID });
            //}
        }

        public ActionResult Delete(Int64 id)
        {
            Rule rule = (from a in db.Rules where a.ID == id select a).First();
            RuleSet rs = (from a in db.RuleSets where a.RuleSetID == rule.RuleSetID select a).First();
            RuleDAO dao = new RuleDAO();
            Int64 planID=-1;
            try
            {
                planID = dao.GetPlanID(id);
            }
            catch { }
            Session["rulesetid"] = -1;

            dao.Delete(rule);
            //db.Rules.Remove(rule);
            //db.SaveChanges();
            //if (rs.Type == "hold")
            //{
                return Json("Success", JsonRequestBehavior.AllowGet);
            //}


            //return RedirectToAction("AddStoresByRule", "SkuRange", new { planID = planID });

            /*
            Rule rule = (from a in db.Rules where a.ID == id select a).First();

            return View(rule);
             */
        }

        //
        // POST: /Account/Delete/5

        [HttpPost]
        public ActionResult Delete(Int64 id, FormCollection form)
        {
            try
            {
                // TODO: Add delete logic here
                Rule rule = (from a in db.Rules where a.ID == id select a).First();
                RuleSet rs = (from a in db.RuleSets where a.RuleSetID == rule.RuleSetID select a).First();
                RuleDAO dao = new RuleDAO();
                Int64 planID = dao.GetPlanID(id);
                dao.Delete(rule);
                Session["rulesetid"] = -1;
                //if (rs.Type == "hold")
                //{
                return Json("Success");
                //}

                //db.Rules.Remove(rule);
                //db.SaveChanges();
                //return RedirectToAction("AddStoresByRule", "SkuRange", new { planID = planID });
            }
            catch
            {
                return Json("Failed");
            }
        }

        //
        // GET: /Account/Edit/5

        public ActionResult Edit(Int64 id)
        {
            Rule rule = (from a in db.Rules where a.ID == id select a).First();
            return View(rule);
        }

        //
        // POST: /Account/Edit/5

        [HttpPost]
        public ActionResult Edit(Int64 id, FormCollection form)
        {
            try
            {
                // TODO: Add update logic here

                Rule rule = (from a2 in db.Rules where a2.ID == id select a2).First();
                Int64 planID = (new RuleDAO()).GetPlanID(id);

                TryUpdateModel(rule);

                if (ModelState.IsValid)
                {
                    db.SaveChanges();
                }
                return RedirectToAction("AddStoresByRule", "SkuRange", new { planID = planID });
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Up(Int64 id)
        {
            Rule rule = (from a in db.Rules where a.ID == id select a).First();
            
            Rule ruleAbove=null;

            try
            {
                ruleAbove = (from b in db.Rules where ((b.RuleSetID == rule.RuleSetID) && (b.Sort == (rule.Sort - 1))) select b).First();
            }
            catch { }

            if (ruleAbove != null)
            {
                rule.Sort = rule.Sort - 1;
                ruleAbove.Sort = ruleAbove.Sort + 1;
                //TryUpdateModel(rule);
                if (ModelState.IsValid)
                {
                    db.SaveChanges();
                }

            }

            RuleSet rs = (from a in db.RuleSets where a.RuleSetID == rule.RuleSetID select a).First();
            //if (rs.Type == "hold")
            //{
            Session["rulesetid"] = -1;
            return Json("Success", JsonRequestBehavior.AllowGet);
            //}

            //Int64 planID = (new RuleDAO()).GetPlanID(id);

            //if (rs.Type == "SizeAlc")
            //{
            //    return RedirectToAction("PresentationQuantities", "SkuRange", new { planID = planID });
            //}
            //else
            //{
            //    return RedirectToAction("AddStoresByRule", "SkuRange", new { planID = planID });
            //}
            //return RedirectToAction("AddStoresByRule", "SkuRange", new { planID = planID });

        }

        public ActionResult Down(Int64 id)
        {
            Rule rule = (from a in db.Rules where a.ID == id select a).First();

            Rule ruleBelow=null;

            try
            {
                ruleBelow = (from b in db.Rules where ((b.RuleSetID == rule.RuleSetID) && (b.Sort == (rule.Sort + 1))) select b).First();
            }
            catch { }

            if (ruleBelow != null)
            {
                rule.Sort = rule.Sort + 1;
                ruleBelow.Sort = ruleBelow.Sort - 1;
                //TryUpdateModel(rule);
                if (ModelState.IsValid)
                {
                    db.SaveChanges();
                }
            }

            RuleSet rs = (from a in db.RuleSets where a.RuleSetID == rule.RuleSetID select a).First();
            //if (rs.Type == "hold")
            //{
            Session["rulesetid"] = -1;
            return Json("Success", JsonRequestBehavior.AllowGet);
            //}

            //Int64 planID = (new RuleDAO()).GetPlanID(id);

            //if (rs.Type == "SizeAlc")
            //{
            //    return RedirectToAction("PresentationQuantities", "SkuRange", new { planID = planID });
            //}
            //else
            //{
            //    return RedirectToAction("AddStoresByRule", "SkuRange", new { planID = planID });
            //}
            //return RedirectToAction("AddStoresByRule", "SkuRange", new { planID = planID });

        }

        /// <summary>
        /// Add a store to the range plan (planID)
        /// </summary>
        public JsonResult AddStore(string store, string div, Int64 ruleSetID)
        {
            try
            {
                RuleSelectedStore det = new RuleSelectedStore();
                det.Store = store;
                det.Division = div;
                det.RuleSetID = ruleSetID;
                det.CreateDate = DateTime.Now;
                det.CreatedBy = User.Identity.Name;

                db.RuleSelectedStores.Add(det);

                //check if it's a rangeplan, save store their too.
                RuleSet rs = (from a in db.RuleSets where a.RuleSetID == ruleSetID select a).First();

                if (rs.Type == "Delivery")
                {
                    //clear the session
                    Session["rulesetid"] = -1;
                    //for PresentationQuantities page, need to reset the session vars so they reload
                    Session["pqAllocs"] = null;
                    Session["pqDeliveryGroups"] = null;

                    //delete it from any other groups
                    List<RuleSelectedStore> existing = (from a in db.RuleSelectedStores
                     join b in db.RuleSets on a.RuleSetID equals b.RuleSetID
                     where
                        (b.Type == "Delivery") &&
                         (b.RuleSetID != ruleSetID) && 
                         (b.PlanID == rs.PlanID) && 
                         (a.Store == store) && 
                         (a.Division == div)
                     select a).ToList();

                    foreach (RuleSelectedStore delete in existing)
                    {
                        db.RuleSelectedStores.Remove(delete);
                    }

                    MaxLeadTime lt = (from c in db.MaxLeadTimes where ((c.Store == store)&&(c.Division == div)) select c).FirstOrDefault();
                    if (lt == null)
                    {
                        lt = new MaxLeadTime();
                        lt.LeadTime = 5;
                        lt.Division = div;
                        lt.Store = store;
                    }
                    SizeAllocationDAO dao = new SizeAllocationDAO();
                    List<RangePlanDetail> rangePlanDetails = (from a in db.RangePlanDetails where a.ID == rs.PlanID select a).ToList();
                    var query = (from a in rangePlanDetails where ((a.Division == lt.Division) && (a.Store == lt.Store)) select a);
                    DeliveryGroup dg = (from a in db.DeliveryGroups where a.RuleSetID == ruleSetID select a).First();
                    foreach (RangePlanDetail rpDet in query)
                    {
                        //set start/end date
                        if (dg.StartDate != null)
                        {
                            rpDet.StartDate = ((DateTime)dg.StartDate).AddDays(lt.LeadTime);
                            db.Entry(rpDet).State = System.Data.EntityState.Modified;
                        }
                        if (dg.EndDate != null)
                        {
                            rpDet.EndDate = ((DateTime)dg.EndDate).AddDays(lt.LeadTime);
                        }
                    }

                }

                db.SaveChanges(UserName);

                if (rs.Type == "Main")
                {
                    if ((from a in db.RangePlanDetails where ((a.Store == store) && (a.Division == div) && (a.ID == rs.PlanID)) select a).Count() == 0)
                    {
                        //add it if it's not already there
                        RangePlanDetail rpDet = new RangePlanDetail();
                        rpDet.Division = div;
                        rpDet.Store = store;
                        rpDet.CreateDate = DateTime.Now;
                        rpDet.CreatedBy = User.Identity.Name;
                        rpDet.ID = (long)rs.PlanID;

                        db.RangePlanDetails.Add(rpDet);
                        db.SaveChanges();

                    }
                }

                if ((rs.PlanID != null) && (rs.Type != "SizeAlc"))
                {
                    UpdateRangePlanDate(rs.PlanID);
                }

                return Json("Success");
            }
            catch (Exception ex)
            {
                return Json("Error");
            }
            //return GetGridJson(planID, page);
        }

        /// <summary>
        /// Add a store to the range plan (planID)
        /// </summary>
        public JsonResult AddAllStores(Int64 ruleSetID, Boolean verified, Boolean move)
        {
            try
            {
                Boolean updated = false;
                RuleSet rs = (from a in db.RuleSets
                              where a.RuleSetID == ruleSetID
                              select a).First();

                //if plan allows multiple rulesets of same type
                //we can tell if it's in another ruleset (for warning purposes)
                List<StoreLookup> storesInSimilarplans = new List<StoreLookup>();
                RuleDAO dao = new RuleDAO();

                if (rs.PlanID > 0)
                {
                    List<Int64> similarRulesets = (from a in db.RuleSets
                                                   where ((a.PlanID == rs.PlanID) && 
                                                          (a.Type == rs.Type) && 
                                                          (rs.RuleSetID != a.RuleSetID))
                                                   select a.RuleSetID).ToList();
                    foreach (Int64 similar in similarRulesets)
                    {
                        foreach (StoreLookup s in dao.GetStoresInRuleSet(similar))
                        {
                            storesInSimilarplans.Add(s);
                        }
                    }
                }

                List<StoreLookupModel> list = this.GetStoresForRules(ruleSetID);

                var currlist =
                    from n in list
                    join c in storesInSimilarplans 
                    on new { n.Division, n.Store } equals new { c.Division, c.Store }
                    select n;

                if ((!verified) && (currlist.Count() > 0))
                {
                    return Json("Verify");
                }
                if (!move)
                {
                    list = list.Where(p => !storesInSimilarplans.Any(p2 => ((p2.Division == p.Division)&&(p2.Store == p.Store)))).ToList();
                }

                //change from here down...
                List<StoreBase> dblist = new List<StoreBase>();
                StoreBase sb;
                foreach (StoreLookupModel m in list)
                {
                    sb = new StoreBase();
                    sb.Division = m.Division;
                    sb.Store = m.Store;
                    dblist.Add(sb);
                }
                if (rs.PlanID != null)
                {
                    dao.AddStoresToRuleset(dblist, (long)rs.PlanID, ruleSetID);
                }
                else
                {
                    dao.AddStoresToRuleset(dblist, 0, ruleSetID);
                }
                Session["rulesetid"] = -1;
                //for PresentationQuantities page, need to reset the session vars so they reload
                Session["pqAllocs"] = null;
                Session["pqDeliveryGroups"] = null;

                if ((rs.PlanID != null) && (rs.Type != "SizeAlc"))
                {
                    UpdateRangePlanDate(rs.PlanID);
                }

                return Json("Success");
            }
            catch (Exception ex)
            {
                return Json("Error");
            }
            //return GetGridJson(planID, page);
        }

        /// <summary>
        /// Add a store to the range plan (planID)
        /// </summary>
        [HttpPost]
        public ActionResult RemoveAllStoresForRules(Int64 ruleSetID)
        {
            try
            {
                Boolean updated = false;
                RuleSet rs = (from a in db.RuleSets where a.RuleSetID == ruleSetID select a).First();
                List<RangePlanDetail> details = null;
                if (rs.Type == "Main")
                {
                    details = (from a in db.RangePlanDetails where (a.ID == rs.PlanID) select a).ToList();
                }

                List<StoreLookupModel> list = this.GetStoresForRules(ruleSetID);
                List<StoreBase> dblist = new List<StoreBase>();
                StoreBase sb;
                foreach (StoreLookupModel m in list)
                {
                    sb = new StoreBase();
                    sb.Division = m.Division;
                    sb.Store = m.Store;
                    dblist.Add(sb);
                }
                RuleDAO dao = new RuleDAO();
                if (rs.PlanID != null)
                {
                    dao.RemoveStoresFromRuleset(dblist, (long)rs.PlanID, ruleSetID);
                }
                else
                {
                    dao.RemoveStoresFromRuleset(dblist, 0, ruleSetID);                
                }
                /*
                foreach (StoreLookupModel s in this.GetStoresForRules(ruleSetID))
                {
                    var query = (from a in db.RuleSelectedStores where ((a.Store == s.Store) && (a.Division == s.Division) && (a.RuleSetID == ruleSetID)) select a);
                    if (query.Count() > 0)
                    {
                        RuleSelectedStore det = query.First();
                        db.RuleSelectedStores.Remove(det);
                        updated = true;
                    }

                    if (rs.Type == "Main")
                    {
                        var query2 = (from a in details where ((a.Store == s.Store) && (a.Division == s.Division)) select a);
                        if (query2.Count() > 0)
                        {
                            //delete it
                            RangePlanDetail rpDet = query2.First();

                            db.RangePlanDetails.Remove(rpDet);
                            updated = true;
                        }
                    }

                }
                db.SaveChanges(UserName);
                 */
                Session["rulesetid"] = -1;
                //for PresentationQuantities page, need to reset the session vars so they reload
                Session["pqAllocs"] = null;
                Session["pqDeliveryGroups"] = null;

                if ((rs.PlanID != null) && (rs.Type != "SizeAlc"))
                {
                    UpdateRangePlanDate(rs.PlanID);
                }

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
        public JsonResult DeleteStore(string store, string div, Int64 ruleSetID)
        {
            try
            {
                var detQuery = (from a in db.RuleSelectedStores where ((a.Store == store) && (a.Division == div) && (a.RuleSetID == ruleSetID)) select a);
                if (detQuery.Count() > 0)
                {
                    RuleSelectedStore det = detQuery.First();
                    db.RuleSelectedStores.Remove(det);
                    db.SaveChanges();
                }
                //check if it's a rangeplan, save store their too.
                RuleSet rs = (from a in db.RuleSets where a.RuleSetID == ruleSetID select a).First();
                if (rs.Type == "Main")
                {
                    var query = (from a in db.RangePlanDetails where ((a.Store == store) && (a.Division == div) && (a.ID == rs.PlanID)) select a);
                    if (query.Count() > 0)
                    {
                        //delete it
                        RangePlanDetail rpDet = query.First();

                        db.RangePlanDetails.Remove(rpDet);
                        db.SaveChanges();
                    }

                    var query2 = (from a in db.RuleSets join b in db.RuleSelectedStores on a.RuleSetID equals b.RuleSetID where ((a.PlanID == rs.PlanID)&&(b.Division == div)&&(b.Store == store)&&(a.Type=="Delivery")) select b);
                    if (query2.Count() > 0)
                    {
                        //delete it
                        foreach (RuleSelectedStore rss in query2)
                        {
                            db.RuleSelectedStores.Remove(rss);
                        }
                        db.SaveChanges();
                    }
                }
                else if (rs.Type == "Delivery")
                {
                    SizeAllocationDAO dao = new SizeAllocationDAO();
                    List<RangePlanDetail> rangePlanDetails = (from a in db.RangePlanDetails where a.ID == rs.PlanID select a).ToList();
                    var query = (from a in rangePlanDetails where ((a.Division == div) && (a.Store == store)) select a);
                    DeliveryGroup dg = (from a in db.DeliveryGroups where a.RuleSetID == ruleSetID select a).First();
                    foreach (RangePlanDetail rpDet in query)
                    {
                        rpDet.StartDate = null;
                        rpDet.EndDate = null;
                        db.Entry(rpDet).State = System.Data.EntityState.Modified;
                    }
                    db.SaveChanges(UserName);
                    //make sure any session storage is cleared.
                    Session["pqDeliveryGroups"] = null;

                }

                if ((rs.PlanID != null) && (rs.Type != "SizeAlc"))
                {
                    UpdateRangePlanDate(rs.PlanID);
                }

                return Json("Success");
            }
            catch (Exception ex)
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
        public ActionResult _StoreLookupList(long ruleSetID, string gridtype)//, string ruleType)
        {
            List<StoreLookupModel> list;// = new List<StoreLookupModel>();
            //ViewData["planID"] = planID;
            //ViewData["gridtype"] = filter;

            if (gridtype == "AllStores")
            {
                list = db.GetStoreLookupsForPlan(ruleSetID, DivisionList(User.Identity.Name));
                list.AddRange(db.GetStoreLookupsNotInPlan(ruleSetID, DivisionList(User.Identity.Name)));
            }
            else
            {
                RuleDAO dao = new RuleDAO();
                try
                {
                    list = GetStoresForRules(ruleSetID);
                }
                catch (Exception ex)
                {
                    list = new List<StoreLookupModel>();
                    //ShowError(ex);
                }
                var ruleSet = (from a in db.RuleSets where a.RuleSetID == ruleSetID select a).FirstOrDefault();
                List<StoreLookupModel> currStores = new List<StoreLookupModel>();
                if (ruleSet.PlanID != null)
                {
                    currStores = db.GetStoreLookupsForPlan((long)ruleSet.PlanID, DivisionList(User.Identity.Name));
                }
                else
                {
                    currStores = GetStoresInRuleSet(ruleSetID);
                }

                var currlist =
                        from n in list
                        join c in currStores on new { n.Division, n.Store } equals new { c.Division, c.Store }
                        select n;

                //var currlist = (from a in list where (StoreInList(a.Division, a.Store, currStores)) select a);
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
                RuleSet rs = (from a in db.RuleSets where a.RuleSetID == ruleSetID select a).First();
                List<StoreLookup> storesInSimilarplans = new List<StoreLookup>();
                if (rs.PlanID > 0)
                {
                    List<Int64> similarRulesets = (from a in db.RuleSets where ((a.PlanID == rs.PlanID) && (a.Type == rs.Type) && (rs.RuleSetID != a.RuleSetID)) select a.RuleSetID).ToList();
                    foreach (Int64 similar in similarRulesets)
                    {
                        foreach (StoreLookup s in dao.GetStoresInRuleSet(similar))
                        {
                            storesInSimilarplans.Add(s);
                        }
                    }
                }

                var currlist2 =
                    from n in list
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
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public ActionResult UploadStores(IEnumerable<HttpPostedFileBase> attachments, Int64 ruleSetID)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            int errors = 0;
            string rangeType;
            List<StoreBase> list = new List<StoreBase>();

            RuleSet rs = (from a in db.RuleSets where a.RuleSetID == ruleSetID select a).First();

            List<StoreLookupModel> StoresInRules = null;
            bool checkStore = true;
            if (rs.Type == "SizeAlc")
            {
                StoresInRules = new List<StoreLookupModel>();
                foreach (StoreLookup l in (from a in db.RangePlanDetails join b in db.StoreLookups on new { a.Division, a.Store } equals new { b.Division, b.Store } where a.ID == rs.PlanID select b))
                {
                    StoresInRules.Add(new StoreLookupModel(l,(long)rs.PlanID,true));
                }

                //delete rules
                IEnumerable<Rule> rules = (from a in db.Rules where a.RuleSetID == rs.RuleSetID select a);
                RuleDAO dao = new RuleDAO();
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
            {
                checkStore = false;
                //StoresInRules = GetAllStores();
            }
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
                        Rule newRule = new Rule();
                        newRule.RuleSetID = ruleSetID;
                        newRule.Field = "Division";
                        newRule.Compare = "Equals";
                        newRule.Value = mySheet.Cells[row, 0].Value.ToString().PadLeft(2, '0');
                        db.Rules.Add(newRule);
                        db.SaveChanges(UserName);
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
                                if ((rangeType == "ALR") || (rangeType == "OP"))
                                {
                                    newDet.RangeType = rangeType;
                                }

                                //always default to "Both"
                                if (string.IsNullOrEmpty(newDet.RangeType))
                                {
                                    newDet.RangeType = "Both";
                                }

                                if ((newDet.Store != "00000"))                                   
                                {
                                    //if (checkStore)
                                    //{
                                        if ((from a in StoresInRules where ((a.Division == newDet.Division) &&(a.Store == newDet.Store)) select a).Count()>0)
                                        {
                                            list.Add(newDet);
                                        }
                                    //}
                                    //else
                                    //{ 
                                    //    list.Add(newDet);
                                    //}
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
                    ruleDAO.AddStoresToRuleset(list, rs.PlanID, ruleSetID);
                    //this is for the PresentationQuantities page, since we just updated the details, we'll remove what's 
                    //saved in the session.
                    Session["pqAllocs"] = null;
                    Session["pqDeliveryGroups"] = null;

                }

                string returnMessage = "Upload complete, added " + list.Count() + " stores";

                if (errors > 0)
                {
                    returnMessage += ", " + errors + " errors";
                }
                Session["rulesetid"] = -1;

                if (rs.PlanID != null)
                {
                    UpdateRangePlanDate(rs.PlanID);
                }
                return Content(returnMessage);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }


        }

        private void UpdateRangePlanDate(long? planID)
        {
            db.UpdateRangePlanDate((long)planID, UserName);
        }
    }
}
