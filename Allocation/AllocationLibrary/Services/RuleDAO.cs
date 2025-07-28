using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using Footlocker.Common;
using System.IO;
using System.Xml.Serialization;

namespace Footlocker.Logistics.Allocation.Services
{
    public class RuleDAO
    {
        readonly Database _database;
        readonly AllocationLibraryContext db = new AllocationLibraryContext();

        public RuleDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        public long GetRuleSetID(long planID, string type, string user)
        {
            DbCommand SQLCommand;
            string SQL = "dbo.GetRuleSetID";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@planID", DbType.String, planID);
            _database.AddInParameter(SQLCommand, "@type", DbType.String, type);
            _database.AddInParameter(SQLCommand, "@user", DbType.String, user);

            DataSet data;
            data = _database.ExecuteDataSet(SQLCommand);

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    return Convert.ToInt64(dr["RuleSetID"]);
                }
            }
            return -1;
        }

        public long GetPlanID(long ID)
        {
            DbCommand SQLCommand;
            string SQL = "dbo.GetPlanID";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@ID", DbType.String, ID);

            DataSet data;
            data = _database.ExecuteDataSet(SQLCommand);

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    return Convert.ToInt64(dr["PlanID"]);
                }
            }
            return -1;
        }

        public void Delete(Models.Rule objectToSave)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "deleteRule";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@ID", DbType.Int64, objectToSave.ID);
            _database.AddInParameter(SQLCommand, "@planID", DbType.Int64, objectToSave.RuleSetID);
            _database.AddInParameter(SQLCommand, "@sort", DbType.Int32, objectToSave.Sort);

            _database.ExecuteNonQuery(SQLCommand);
        }

        public void RemoveStoresFromRuleset(List<StoreBase> stores, long planid, long rulesetid)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "[RemoveStoresFromRuleset]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(stores.GetType());
            xs.Serialize(sw, stores);
            string xout = sw.ToString();

            _database.AddInParameter(SQLCommand, "@xmlDetails", DbType.Xml, xout);
            _database.AddInParameter(SQLCommand, "@planid", DbType.String, planid);
            _database.AddInParameter(SQLCommand, "@rulesetid", DbType.String, rulesetid);

            _database.ExecuteNonQuery(SQLCommand);
        }

        public void AddStoreToRuleset(string div, string store, long ruleSetID, string userID)
        {
            RuleSelectedStore det = new RuleSelectedStore()
            {
                Store = store,
                Division = div,
                RuleSetID = ruleSetID,
                CreateDate = DateTime.Now,
                CreatedBy = userID
            };

            db.RuleSelectedStores.Add(det);
            db.SaveChanges();
        }

        public RuleSet GetRuleSet(long ruleSetID)
        {
            return db.RuleSets.Where(r => r.RuleSetID == ruleSetID).FirstOrDefault();
        }

        public string GetDivisionForRuleSet(long ruleSetID)
        {
            string div = string.Empty;

            RuleSet rs = GetRuleSet(ruleSetID);

            if (rs != null)
            {
                if (!string.IsNullOrEmpty(rs.Division))
                    div = rs.Division;
                else
                {
                    RangePlan range = null;

                    if (rs.PlanID.HasValue)
                        range = db.RangePlans.Where(rp => rp.Id == rs.PlanID.Value).FirstOrDefault();

                    if (range != null)
                        div = range.Division;
                }
            }

            return div;
        }

        public List<Models.Rule> GetRulesForPlan(long planID, string ruleType)
        {
            List<Models.Rule> RulesForPlan = (from r in db.Rules
                                              join rs in db.RuleSets
                                               on r.RuleSetID equals rs.RuleSetID
                                              where rs.PlanID == planID && rs.Type == ruleType
                                              orderby r.Sort ascending
                                              select r).ToList();
            return RulesForPlan;
        }

        public List<Models.Rule> GetRulesForRuleSet(long RuleSetID, string ruleType)
        {
            List<Models.Rule> RulesForPlan = (from r in db.Rules
                                              join rs in db.RuleSets
                                                on r.RuleSetID equals rs.RuleSetID
                                              where rs.RuleSetID == RuleSetID && rs.Type == ruleType
                                              orderby r.Sort ascending
                                              select r).ToList();
            return RulesForPlan;
        }

        public List<Models.Rule> GetRulesForRuleSet(long ruleSetID)
        {
            return db.Rules.Where(r => r.RuleSetID == ruleSetID).OrderBy(r => r.Sort).ToList();
        }

        /// <summary>
        /// This will delete all stores for a ruleset and re-add the new ones. 
        /// </summary>
        /// <param name="stores"></param>
        /// <param name="planid"></param>
        /// <param name="rulesetid"></param>
        public void AddStoresToRuleset(List<StoreBase> stores, long? planid, long rulesetid)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "[AddStoresToRuleset]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(stores.GetType());
            xs.Serialize(sw, stores);
            string xout = sw.ToString();

            _database.AddInParameter(SQLCommand, "@xmlDetails", DbType.Xml, xout);
            _database.AddInParameter(SQLCommand, "@planid", DbType.String, planid);
            _database.AddInParameter(SQLCommand, "@rulesetid", DbType.String, rulesetid);

            _database.ExecuteNonQuery(SQLCommand);
        }

        public void AddStoresToPlan(List<StoreBase> stores, long planid)
        {
            DbCommand SQLCommand;
            string SQL;
            SQL = "[AddStoresToPlan]";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            StringWriter sw = new StringWriter();
            XmlSerializer xs = new XmlSerializer(stores.GetType());
            xs.Serialize(sw, stores);
            string xout = sw.ToString();

            _database.AddInParameter(SQLCommand, "@xmlDetails", DbType.Xml, xout);
            _database.AddInParameter(SQLCommand, "@planid", DbType.String, planid);

            _database.ExecuteNonQuery(SQLCommand);
        }

        /// <summary>
        /// recursive function to create the lambda expression for the users set of rules
        /// </summary>
        public Expression GetExpression(List<Models.Rule> rules, ParameterExpression pe, string divisionList)
        {
            if (rules.Count > 0)
            {
                if (rules[0].Field == null)
                {
                    //and,or,(,),not
                    int openCount = 1;
                    if (rules[0].Compare.Equals("("))
                    {
                        for (int i = 1; i < rules.Count; i++)
                        {
                            if (rules[i].Compare.Equals("("))                            
                                openCount++;                            
                            else if (rules[i].Compare.Equals(")"))                            
                                openCount--;
                            
                            if (openCount == 0)
                            {
                                //get the expression for this paren block
                                if (i < (rules.Count - 1))
                                {
                                    //we have more rules
                                    Expression newExp = GetExpression(rules.GetRange(1, i - 1), pe, divisionList);
                                    return GetCompositeRule(newExp, rules.GetRange(i + 1, rules.Count - i - 1), pe, divisionList);
                                }
                                else
                                {
                                    //no more rules, just strip out the parens and process the block
                                    return GetExpression(rules.GetRange(1, (i - 1)), pe, divisionList);
                                }
                            }
                        }
                        throw new Exception("invalidly formatted rule (parens)!");
                    }
                    else if (rules[0].Compare.Equals("not"))
                    {
                        if (rules.Count > 1)                        
                            return Expression.Not(GetExpression(rules.GetRange(1, rules.Count - 1), pe, divisionList));
                        
                        throw new Exception("invalidly formatted rule (not)!");
                    }

                    throw new Exception("invalidly formatted rule (missing first rule)!");
                }
                else if (rules.Count > 1)
                {
                    //we have more rules
                    return GetCompositeRule(GetExpressionFromSingleRule(rules[0], pe, divisionList), rules.GetRange(1, rules.Count - 1), pe, divisionList);
                }
                else
                {
                    //single rule, just evaluate it
                    return GetExpressionFromSingleRule(rules[0], pe, divisionList);
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
        private Expression GetExpressionFromSingleRule(Models.Rule rule, ParameterExpression pe, string divisionList)
        {
            Expression exp = null;
            Expression left = null;
            Expression right = null;

            if ((rule.Field == "StorePlan") || (rule.Field == "RangePlan") || (rule.Field == "RangePlanDesc") || (rule.Field == "RangePlanID"))
            {
                //return an expression that will link to the RangePlanDetails
                //TODO:  This is faster than manually creating and/or's but still slow
                //not sure if there is a better way to build the expression tree
                List<string> stores;

                if (rule.Field == "RangePlan")
                {
                    long planID;
                    string sku = Convert.ToString(rule.Value);

                    string myInClause = divisionList;
                    try
                    {
                        planID = db.RangePlans.Where(rp => rp.Sku == sku).First().Id;
                    }
                    catch
                    {
                        stores = new List<string>();
                        planID = -1;
                    }
                    stores = (from rp in db.RangePlanDetails 
                              where rp.ID == planID && 
                                    myInClause.Contains(rp.Division)
                              select rp.Division + rp.Store).Distinct().ToList();
                }
                else if (rule.Field == "RangePlanID")
                {
                    long planID = Convert.ToInt64(rule.Value);

                    string myInClause = divisionList;
                    stores = (from rp in db.RangePlanDetails 
                              where rp.ID == planID && 
                                    myInClause.Contains(rp.Division)
                              select rp.Division + rp.Store).Distinct().ToList();
                }
                else if (rule.Field == "RangePlanDesc")
                {
                    long planID;
                    string sku = Convert.ToString(rule.Value);
                    //sku is currently desc (sku)
                    sku = sku.Substring(sku.Length - 15, 14);
                    string myInClause = divisionList;
                    try
                    {
                        planID = db.RangePlans.Where(rp => rp.Sku == sku).First().Id;
                    }
                    catch
                    {
                        stores = new List<string>();
                        planID = -1;
                    }
                    stores = (from rp in db.RangePlanDetails 
                              where rp.ID == planID && 
                                    myInClause.Contains(rp.Division)
                              select rp.Division + rp.Store).Distinct().ToList();
                }

                else
                {
                    stores = (from rp in db.StorePlans 
                              where rp.PlanName == rule.Value 
                              select rp.Division + rp.Store).Distinct().ToList();
                }

                ConstantExpression foreignKeysParameter = Expression.Constant(stores, typeof(List<string>));

                MethodInfo method = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string), });
                Expression memberExpression = Expression.Property(pe, "Store");
                Expression memberExpression2 = Expression.Property(pe, "Division");
                MethodCallExpression concatExpression = Expression.Call(method, memberExpression2, memberExpression);
                
                Expression convertExpression = Expression.Convert(concatExpression, typeof(string));  //store and divison
                MethodCallExpression containsExpression = Expression.Call(foreignKeysParameter, "Contains", new Type[] { }, convertExpression);

                switch (rule.Compare)
                {
                    case "Equals":
                        return containsExpression;
                    case "Does Not Equal":
                        return Expression.Not(containsExpression);
                    default:
                        throw new Exception(string.Format("Unsupported rule: Operation \"{0}\" not valid for \"{1}\"", rule.Compare, rule.Field));
                }
            }
            else if (rule.Field.Contains("StoreExtension"))
            {
                // Parse type of store extension field
                var fieldNameIndex = rule.Field.IndexOf(".") + 1;
                var fieldNameLength = rule.Field.LastIndexOf(".") - fieldNameIndex;
                var parsedFieldName = rule.Field.Substring(fieldNameIndex, fieldNameLength);

                // Build left/right sides of expression
                var storeExtExp = Expression.Property(pe, typeof(StoreLookup).GetProperty("StoreExtension"));
                var extExp = Expression.Property(storeExtExp, typeof(StoreExtension).GetProperty(parsedFieldName));
                Type extType = null;
                switch(parsedFieldName.ToUpper())
                {
                    case "CONCEPTTYPE" :
                        extType = typeof(ConceptType);
                        break;
                    case "CUSTOMERTYPE" :
                        extType = typeof(CustomerType);
                        break;
                    case "STRATEGYTYPE" :
                        extType = typeof(StrategyType);
                        break;
                    case "PRIORITYTYPE" :
                        extType = typeof(PriorityType);
                        break;
                    default:
                        throw new NotSupportedException("Unsupported field name on store lookup when attempting to filter by rules.");
                }
                left = Expression.Property(extExp, extType.GetProperty("Name"));
                right = Expression.Constant(rule.Value, typeof(string));

                // Build final comparison expression
                switch (rule.Compare)
                {
                    case "Equals":
                        exp = Expression.Equal(left, right);
                        break;
                    case "Does Not Equal":
                        var outerLeft = Expression.Or(Expression.Equal(storeExtExp, Expression.Constant(null)), Expression.Equal(extExp, Expression.Constant(null)));
                        var outerRight = Expression.NotEqual(left, right);
                        exp = Expression.Or(outerLeft, outerRight);
                        break;
                    case "Is less than":
                        exp = Expression.LessThan(left, right);
                        break;
                    case "Is Greater than":
                        exp = Expression.GreaterThan(left, right);
                        break;
                    case "Contains":
                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        exp = Expression.Call(left, containsMethod, right);
                        break;
                    case "StartsWith":
                        var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                        exp = Expression.Call(left, startsWithMethod, right);
                        break;
                    case "EndsWith":
                        var endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
                        exp = Expression.Call(left, endsWithMethod, right);
                        break;
                    default:
                        throw new NotSupportedException(String.Format("Unrecognized comparator '{0}' when attempting to apply rule.", rule.Compare));
                }
            }
            else
            {
                // Build left/right sides of expression
                left = Expression.Property(pe, typeof(StoreLookup).GetProperty(rule.Field));
                right = Expression.Constant(rule.Value, typeof(string));

                // Build final comparison expression
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

            // Account for Divisional Security -----------------------------------------------------------
            var divConstExp = Expression.Constant(divisionList, typeof(string));
            var divMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var divPropExp = Expression.Property(pe, "Division");
            var divExpression = Expression.Call(divConstExp, divMethod, divPropExp);
            exp = Expression.And(divExpression, exp);
            // --------------------------------------------------------------------------------------------------------

            return exp;
        }

        /// <summary>
        /// Get a lambda expression from two or more rules
        /// </summary>
        private Expression GetCompositeRule(Expression first, List<Footlocker.Logistics.Allocation.Models.Rule> rules, ParameterExpression pe, string divisionList)
        {
            Footlocker.Logistics.Allocation.Models.Rule rule = rules[0];
            rules.Remove(rule);
            Expression e2 = GetExpression(rules, pe, divisionList);

            if (rule.Compare.Equals("or"))            
                return Expression.OrElse(first, e2);            
            else
            {
                if (e2 != null)                
                    return Expression.AndAlso(first, e2);                
                else                
                    throw new Exception("invalid rule, missing predicate.");                
            }
        }

        public List<StoreLookup> GetRuleSelectedStoresInRuleSet(long ruleSetID)
        {
            List<StoreLookup> results = (from a in db.StoreLookups 
                                         join b in db.RuleSelectedStores 
                                           on new { div = a.Division, store = a.Store } equals new { div = b.Division, store = b.Store } 
                                         where b.RuleSetID == ruleSetID
                                         select a).ToList();

            return results; 
        }

        public List<StoreLookup> GetValidStoresInRuleSet(long ruleSetID)
        {
            var results = (from a in db.StoreLookups 
                           join b in db.RuleSelectedStores on new { div = a.Division, store = a.Store } equals new { div = b.Division, store = b.Store }
                           join c in db.ValidStores on new { div = a.Division, store = a.Store } equals new { div = c.Division, store = c.Store }
                           where (b.RuleSetID == ruleSetID) select a);

            return results.ToList();
        }

        /// <summary>
        /// return the stores that qualify for the current rules on a ruleset
        /// </summary>
        /// <param name="ruleSetID"></param>
        /// <returns></returns>
        public List<StoreLookup> GetStoresForRules(long ruleSetID, WebUser currentUser, string AppName)
        {
            string div = "";

            IQueryable<StoreLookup> queryableData = db.StoreLookups.Include("StoreExtension")
                                                                   //.Include("StoreExtension.ConceptType")
                                                                   .Include("StoreExtension.CustomerType")
                                                                   .Include("StoreExtension.PriorityType")
                                                                   .Include("StoreExtension.StrategyType")
                                                                   .AsQueryable();

            ParameterExpression pe = Expression.Parameter(typeof(StoreLookup), "StoreLookup");
            List<Models.Rule> ruleList = db.Rules.Where(r => r.RuleSetID == ruleSetID).OrderBy(r => r.Sort).ToList();

            List<Models.Rule> finalRules = new List<Models.Rule>();

            RuleSet rs = GetRuleSet(ruleSetID);

            div = GetDivisionForRuleSet(ruleSetID);

            if (div != "")
            {
                //add division criteria to rules
                Models.Rule divRule = new Models.Rule()
                {
                    Compare = "Equals",
                    Field = "Division",
                    Value = div
                };

                finalRules.Add(divRule);

                divRule = new Models.Rule()
                {
                    Compare = "and"
                };

                finalRules.Add(divRule);

                foreach (Models.Rule r in ruleList)
                {
                    finalRules.Add(r);
                }
            }
            else
                finalRules = ruleList;

            if (rs.Type == "Delivery")
            {
                //add rules to only pull back stores in the range plan if this is a Delivery type.
                Models.Rule deliveryRule = new Models.Rule()
                {
                    Compare = "Equals",
                    Field = "RangePlanID",
                    Value = rs.PlanID.Value.ToString()
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
                    deliveryRule = new Models.Rule()
                    {
                        Compare = "and"
                    };

                    finalRules.Insert(1, deliveryRule);

                    deliveryRule = new Models.Rule()
                    {
                        Compare = "("
                    };

                    finalRules.Insert(2, deliveryRule);

                    deliveryRule = new Models.Rule()
                    {
                        Compare = ")"
                    };

                    finalRules.Add(deliveryRule);
                }
            }

            try
            {
                Expression finalExpression = GetExpression(finalRules, pe, currentUser.GetUserDivisionsString());

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
                return results.ToList();
            }
            catch
            {
                // TODO: We should really think about, atleast, logging here.....
                return new List<StoreLookup>();
            }
        }
    }
}