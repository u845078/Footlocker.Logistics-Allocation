using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Telerik.Web.Mvc;

using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Common;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Space Planning,Director of Allocation,Admin,Support,Advanced Merchandiser Processes")]
    public class StoreExtensionController : AppController
    {
        #region Initializations

        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        #endregion

        #region Non-Public Methods

        private RuleSet CreateRuleSet()
        {
            RuleSet rs = new RuleSet();
            rs.Type = "hold";
            rs.CreateDate = DateTime.Now;
            rs.CreatedBy = User.Identity.Name;
            db.RuleSets.Add(rs);
            db.SaveChanges();

            return rs;
        }

        private void LoadLookups(StoreBatchModel viewModel)
        {
            // Get concepts of divisions which current user has access to
            var accessibleDivCodes = Divisions().Select(d => d.DivCode);
            var accessibleConcepts = 
                db.ConceptTypes.Include("Divisions").Where(ct => ct.Divisions.Select(d => d.Division).Intersect(accessibleDivCodes).Any());

            // Create lookup lists
            var conceptTypeList = new List<ConceptType>(accessibleConcepts.ToList());
            var customerTypeList = new List<CustomerType>(db.CustomerTypes.ToList());
            var priorityTypeList = new List<PriorityType>(db.PriorityTypes.ToList());
            var strategyTypeList = new List<StrategyType>(db.StrategyTypes.ToList());

            // Seed the lists with a 'N/A' option to represent NULL or Mixed values for the batch update
            conceptTypeList.Insert(0, new ConceptType() { ID = -1, Name = "N/A" });
            customerTypeList.Insert(0, new CustomerType() { ID = -1, Name = "N/A" });
            priorityTypeList.Insert(0, new PriorityType() { ID = -1, Name = "N/A" });
            strategyTypeList.Insert(0, new StrategyType() { ID = -1, Name = "N/A" });

            // Load view model
            viewModel.ConceptTypes = conceptTypeList;
            viewModel.CustomerTypes = customerTypeList;
            viewModel.PriorityTypes = priorityTypeList;
            viewModel.StrategyTypes = strategyTypeList;

            viewModel.MiniHubValues = new List<KeyValuePair<int, string>>();
            viewModel.MiniHubValues.Add(new KeyValuePair<int, string>(-1, "N/A"));
            viewModel.MiniHubValues.Add(new KeyValuePair<int, string>(0, "Do Not Use Minihub Strategy"));
            viewModel.MiniHubValues.Add(new KeyValuePair<int, string>(1, "Use Minihub Strategy"));
        }

        #endregion

        #region Public Methods

        [HttpGet]
        public ActionResult Filter(long ruleSetID, bool isRestrictingToUnassignedCustomer)
        {
            //  HACK: ViewData....
            ViewData["ruleSetID"] = ruleSetID;
            ViewData["gridtype"] = ""; // HACK: Setting used to indicate to load rules grid by ruleset not plan...
            ViewData["ruleType"] = "hold"; // HACK: Setting used to indicate to load rules by ruleset not plan...

            return View(new StoreBatchFilterModel() { RuleSetID = ruleSetID, IsRestrictingToUnassignedCustomer = isRestrictingToUnassignedCustomer });
        }

        [HttpGet]
        public ActionResult BatchUpdate(long ruleSetID = -1, bool isRestrictingToUnassignedCustomer = false)
        {
            // Create rule set for new batch update session
            if (ruleSetID < 1) { ruleSetID = CreateRuleSet().RuleSetID; }

            // Create view model to support UI
            var viewModel = new StoreBatchModel() { RuleSetID = ruleSetID, IsRestrictingToUnassignedCustomer = isRestrictingToUnassignedCustomer };
            LoadLookups(viewModel);

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult Update()
        {
            return View();
            //List<StoreLookup> Stores = (from a in db.StoreLookups.Include("StoreExtension") select a).ToList();
            //Stores = (from a in Stores join b in Divisions() on a.Division equals b.DivCode select a).ToList();

            //return View(Stores);
        }


        [GridAction]
        public ActionResult _Update()
        {
            List<StoreLookup> Stores = (from a in db.StoreLookups.Include("StoreExtension") select a).ToList();
            Stores = (from a in Stores join b in Divisions() on a.Division equals b.DivCode select a).ToList();

            return View(new GridModel(Stores));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction]
        public ActionResult _SaveBatchEditing([Bind(Prefix = "updated")]IEnumerable<StoreLookup> updated)
        {
            StoreExtension se;
            foreach (StoreLookup s in updated)
            {
                var query = (from a in db.StoreExtensions where ((a.Store == s.Store) && (a.Division == s.Division)) select a);

                if (query.Count() > 0)
                {
                    se = query.First();
                }
                else
                {
                    se = new StoreExtension();
                    se.Division = s.Division;
                    se.Store = s.Store;
                    db.StoreExtensions.Add(se);

                }
                se.LastModifiedBy = User.Identity.Name;
                se.LastModifiedDate = DateTime.Now;
                if (s.StoreExtension != null)
                {
                    se.FirstReceipt = s.FirstReceipt;
                    se.ExcludeStore = s.ExcludeStore;
                }
                else
                {
                    se.FirstReceipt = null;
                    se.ExcludeStore = false;
                }
            }
            db.SaveChanges();

            List<StoreLookup> Stores = (from a in db.StoreLookups.Include("StoreExtension") select a).ToList();
            Stores = (from a in Stores join b in Divisions() on a.Division equals b.DivCode select a).ToList();

            return View(new GridModel(Stores));

        }

        [HttpPost]
        public ActionResult Save(StoreBatchModel model)
        {
            // Construct query for stores of rule set
            IEnumerable<StoreLookup> domainObjects = db.RuleSets
                     .Include("Stores.StoreLookup.StoreExtension.ConceptType")
                     .Include("Stores.StoreLookup.StoreExtension.CustomerType")
                     .Include("Stores.StoreLookup.StoreExtension.PriorityType")
                     .Include("Stores.StoreLookup.StoreExtension.StrategyType")
                     .Single(rs => rs.RuleSetID == model.RuleSetID)
                     .Stores
                     .Select(s => s.StoreLookup);

            // Append 'all unassigned' clause if requested
            if (model.IsRestrictingToUnassignedCustomer)
            {
                domainObjects =
                    domainObjects
                        .Where(sl => sl.StoreExtension == null || sl.StoreExtension.CustomerType == null);
            }

            // Perform update on entities
            if (model.SelectedConceptTypeID > 0 
                || model.SelectedCustomerTypeID > 0 
                || model.SelectedPriorityTypeID > 0 
                || model.SelectedStrategyTypeID > 0
                || model.SelectedExcludeStore > 0
                || model.SelectedMinihubStrategyInd > 0)
            {
                var currentDate = DateTime.Now;

                domainObjects.ToList().ForEach(sl =>
                    {
                        // Create extension record if need be
                        if (sl.StoreExtension == null)
                        {
                            sl.StoreExtension = new StoreExtension()
                                {
                                    Division = sl.Division,
                                    Store = sl.Store,
                                    LastModifiedBy = UserName,
                                    LastModifiedDate = currentDate
                                };
                        }

                        // Update extension data fields
                        if (model.SelectedConceptTypeID > 0 && sl.StoreExtension.ConceptTypeID != model.SelectedConceptTypeID)
                            { sl.StoreExtension.ConceptTypeID = model.SelectedConceptTypeID; }
                        if (model.SelectedCustomerTypeID > 0 && sl.StoreExtension.CustomerTypeID != model.SelectedCustomerTypeID) 
                            { sl.StoreExtension.CustomerTypeID = model.SelectedCustomerTypeID; }
                        if (model.SelectedPriorityTypeID > 0 && sl.StoreExtension.PriorityTypeID != model.SelectedPriorityTypeID) 
                            { sl.StoreExtension.PriorityTypeID = model.SelectedPriorityTypeID; }
                        if (model.SelectedStrategyTypeID > 0 && sl.StoreExtension.StrategyTypeID != model.SelectedStrategyTypeID)
                            { sl.StoreExtension.StrategyTypeID = model.SelectedStrategyTypeID; }
                        if (model.SelectedMinihubStrategyInd > 0 && (sl.StoreExtension.MinihubStrategyInd ? 1 : 0) != model.SelectedMinihubStrategyInd)
                        {
                            sl.StoreExtension.MinihubStrategyInd = (model.SelectedMinihubStrategyInd == 1);
                        }
                        if (model.SelectedExcludeStore > 0)
                        {
                            sl.StoreExtension.ExcludeStore = (model.SelectedExcludeStore == 1); 
                        }                        
                    });

                db.SaveChanges();

                // Notify user of success
                model.NotificationMessage = "The update has completed successfully.";
            }

            // Prepare viewModel for UI
            LoadLookups(model);

            return View("BatchUpdate", model);
        }

        [StoreLookupGridAction]
        public ActionResult Grid_BatchUpdate(long ruleSetID, bool isRestrictingToUnassignedCustomer = false)
        {
            // Construct query for stores of rule set
            IEnumerable<StoreLookup> domainObjects = db.RuleSets
                     .Include("Stores.StoreLookup.StoreExtension.ConceptType")
                     .Include("Stores.StoreLookup.StoreExtension.CustomerType")
                     .Include("Stores.StoreLookup.StoreExtension.PriorityType")
                     .Include("Stores.StoreLookup.StoreExtension.StrategyType")                     
                     .Single(rs => rs.RuleSetID == ruleSetID)
                     .Stores
                     .Select(s => s.StoreLookup);

            // Append 'all unassigned' clause if requested
            if (isRestrictingToUnassignedCustomer)
            {
                domainObjects = domainObjects.Where(sl => sl.StoreExtension == null || sl.StoreExtension.CustomerType == null);
            }

            // Get distinct types of stores in batch
            var conceptTypeIDs = domainObjects.Select(sl => (sl.StoreExtension != null) ? sl.StoreExtension.ConceptTypeID : -1).Distinct();
            var customerTypeIDs = domainObjects.Select(sl => (sl.StoreExtension != null) ? sl.StoreExtension.CustomerTypeID : -1).Distinct();
            var strategyTypeIDs = domainObjects.Select(sl => (sl.StoreExtension != null) ? sl.StoreExtension.StrategyTypeID : -1).Distinct();
            var priorityTypeIDs = domainObjects.Select(sl => (sl.StoreExtension != null) ? sl.StoreExtension.PriorityTypeID : -1).Distinct();
            var minihubStrategies = domainObjects.Select(sl => (sl.StoreExtension != null) ? ((sl.StoreExtension.MinihubStrategyInd) ? 1 : 0) : -1).Distinct();

            // Get concepts of divisions which current user has access to
            var accessibleDivCodes = Divisions().Select(d => d.DivCode);
            var accessibleConceptTypeIDs =
                db.ConceptTypes.Include("Divisions")
                    .Where(ct => ct.Divisions.Select(d => d.Division).Intersect(accessibleDivCodes).Any())
                    .Select(ct => ct.ID);

            // Calculate shared Id to use for each type across all stores in batch
            var mixedValue = -1;
            var conceptTypeId = conceptTypeIDs.FirstOrDefault();
            var sharedConceptTypeID = ((conceptTypeIDs.Count() == 1) 
                    && (conceptTypeId != null) 
                    && (accessibleConceptTypeIDs.Contains((int)conceptTypeId))) ?
                (int)conceptTypeId :
                mixedValue;
            var sharedCustomerTypeID = (customerTypeIDs.Count() > 1) ? mixedValue : customerTypeIDs.FirstOrDefault() ?? mixedValue;
            var sharedStrategyTypeID = (strategyTypeIDs.Count() > 1) ? mixedValue : strategyTypeIDs.FirstOrDefault() ?? mixedValue;
            var sharedPriorityTypeID = (priorityTypeIDs.Count() > 1) ? mixedValue : priorityTypeIDs.FirstOrDefault() ?? mixedValue;
            var sharedMinihubStrat = (minihubStrategies.Count() > 1) ? mixedValue : minihubStrategies.FirstOrDefault();

            return View(new StoreLookupGridModel(domainObjects) 
                { 
                    ExtensionData = new StoreLookupGridExtensionData() 
                        {
                            ConceptTypeID = sharedConceptTypeID,
                            CustomerTypeID = sharedCustomerTypeID,
                            PriorityTypeID = sharedPriorityTypeID,
                            StrategyTypeID = sharedStrategyTypeID,
                            MinihubStrategy = sharedMinihubStrat
                        } 
                });
        }

        #endregion
    }
}

public class StoreLookupGridExtensionData
{
    public int ConceptTypeID { get; set; }
    public int CustomerTypeID { get; set; }
    public int PriorityTypeID { get; set; }
    public int StrategyTypeID { get; set; }
    public int MinihubStrategy { get; set; }
}

public class StoreLookupGridModel : GridModel
{
    public StoreLookupGridModel() : base() { }
    public StoreLookupGridModel(IEnumerable data) : base(data) { }

    public StoreLookupGridExtensionData ExtensionData { get; set; }
}

public class StoreLookupGridAction : GridActionAttribute
{
    // HACK: This is not thread safe..not one instance per request (controller instance), instead one per session as I understand...
    private StoreLookupGridExtensionData ExtensionData { get; set; }

    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        // Retain reference to additional data
        ExtensionData = ((filterContext.Result as ViewResultBase).Model as StoreLookupGridModel).ExtensionData;

        base.OnActionExecuted(filterContext);
    }
    
    protected override ActionResult CreateActionResult(object model)
    {
        // Inject our extension data into the dictionary that gets created for the action result in base.OnActionExecuted....
        (model as Dictionary<string, object>).Add("extensionData", ExtensionData);

        return base.CreateActionResult(model);
    }
}