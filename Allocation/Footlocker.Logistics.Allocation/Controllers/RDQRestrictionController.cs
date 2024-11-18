using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Common;
using Footlocker.Logistics.Allocation.DAO;
using Telerik.Web.Mvc;
using Aspose.Cells;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Spreadsheets;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Support,Logistics")]
    public class RDQRestrictionController : AppController
    {

        #region Private Members

        private AllocationContext db { get; set; }

        #endregion

        public RDQRestrictionController()
            : base()
        {
            this.db = new AllocationContext();
        }

        public ActionResult Index(string message)
        {
            ViewData["errorMessage"] = message;
            RDQRestrictionModel model = RetrieveModel();
            return View(model);
        }

        private RDQRestrictionModel RetrieveModel()
        {
            RDQRestrictionModel model = new RDQRestrictionModel();
            var permissions = currentUser.GetUserRoles();
            model.CanEdit = permissions.Contains("Support") || permissions.Contains("Logistics");
            return model;
        }

        public ActionResult IndexByProduct(string message)
        {
            ViewData["message"] = message;
            RDQRestrictionModel model = RetrieveModel();
            return View(model);
        }

        [GridAction]
        public ActionResult ExportGrid(GridCommand settings)
        {
            RDQRestrictionsExport rdqRestrictionsExport = new RDQRestrictionsExport(appConfig);
            IList<IFilterDescriptor> filterDescriptors = null;

            if (settings.FilterDescriptors.Any())
                filterDescriptors = settings.FilterDescriptors;
                
            rdqRestrictionsExport.WriteData(filterDescriptors);

            rdqRestrictionsExport.excelDocument.Save(System.Web.HttpContext.Current.Response, "RDQRestrictions.xlsx", ContentDisposition.Attachment, rdqRestrictionsExport.SaveOptions);
            return RedirectToAction("Index");
        }

        public ActionResult IndexByDestination(string message, string destinationType)
        {
            ViewData["message"] = message;
            RDQRestrictionModel model = RetrieveModel();
            model.DestinationTypes = GetDestinationsList();
            if (string.IsNullOrEmpty(destinationType))
            {
                if (model.DestinationTypes.Count() > 0)
                {
                    model.DestinationType = model.DestinationTypes[0].Value;
                }
            }
            else
            {
                model.DestinationType = destinationType;
            }
            return View(model);
        }

        [GridAction]
        public ActionResult _Index()
        {
            List<RDQRestriction> list = db.RDQRestrictions.ToList();
            
            List<string> users = (from a in list
                                  select a.LastModifiedUser).Distinct().ToList();

            Dictionary<string, string> names = LoadUserNames(users);

            foreach (var item in list)
            {
                item.LastModifiedUser = names[item.LastModifiedUser];
            }

            return View(new GridModel(list));
        }

        public ActionResult Create()
        {
            RDQRestrictionModel model = new RDQRestrictionModel();
            model = FillModelLists(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RDQRestrictionModel model)
        {
            string message;

            if (!ValidateRDQRestriction(model, out message))
            {
                model = FillModelLists(model);
                ViewData["errorMessage"] = message;
                return View(model);
            }

            model.RDQRestriction.LastModifiedDate = DateTime.Now;
            model.RDQRestriction.LastModifiedUser = currentUser.NetworkID;
            db.RDQRestrictions.Add(model.RDQRestriction);
            db.SaveChanges();

            ViewData["message"] = "Successfully created an RDQ Restriction";
            model = FillModelLists(model);

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            RDQRestrictionModel model = null;
            string errorMessage = string.Empty;

            RDQRestriction rr
                = db.RDQRestrictions
                    .Where(r => r.RDQRestrictionID.Equals(id))
                    .FirstOrDefault();

            if (rr != null)
            {
                model = new RDQRestrictionModel(rr);
                if (!ValidateCombination(model.RDQRestriction, out errorMessage))
                {
                    ViewData["message"] = errorMessage;
                    model = FillModelLists(model);
                    return View(model);
                }
            }
            else
            {
                errorMessage = "The RDQ restriction you have referenced no longer exists.";
                ViewData["errorMessage"] = errorMessage;
                return RedirectToAction("Index", new { message = errorMessage });
            }

            model = FillModelLists(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RDQRestrictionModel model, int id)
        {
            string message;

            model.RDQRestriction.RDQRestrictionID = id;
            model.RDQRestriction.LastModifiedDate = DateTime.Now;
            model.RDQRestriction.LastModifiedUser = currentUser.NetworkID;

            if (!ValidateRDQRestriction(model, out message))
            {
                model = FillModelLists(model);
                ViewData["errorMessage"] = message;
                return View(model);
            }

            db.Entry(model.RDQRestriction).State = System.Data.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [GridAction]
        public ActionResult _RDQRestrictionStores()
        {
            List<StoreLookup> returnValue = new List<StoreLookup>();

            List<Division> divs = currentUser.GetUserDivisions();
            var list = (from rr in db.RDQRestrictions
                           join sl in db.StoreLookups
                             on new { Division = rr.Division, Store = rr.ToStore } equals
                                new { Division = sl.Division, Store = sl.Store }
                           select sl).Distinct().ToList();

            returnValue = (from a in list
                           join d in divs
                             on a.Division equals d.DivCode
                         select a).ToList();


            return PartialView(new GridModel(returnValue));
        }

        [GridAction]
        public ActionResult _RDQRestrictionRegions()
        {
            List<Region> returnValue = new List<Region>();

            var list = (from rr in db.RDQRestrictions
                        join re in db.Regions
                          on new { Division = rr.Division, Region = rr.ToRegion } equals
                             new { Division = re.Division, Region = re.RegionCode }
                      select re).Distinct().ToList();

            returnValue = (from a in list
                           join d in currentUser.GetUserDivisions()
                             on a.Division equals d.DivCode
                         select a).ToList();

            return PartialView(new GridModel(returnValue));
        }

        [GridAction]
        public ActionResult _RDQRestrictionLeagues()
        {
            List<League> returnValue = new List<League>();

            var list = (from rr in db.RDQRestrictions
                        join le in db.Leagues
                          on new { Division = rr.Division, League = rr.ToLeague } equals
                             new { Division = le.Division, League = le.LeagueCode }
                      select le).Distinct().ToList();

            returnValue = (from a in list
                           join d in currentUser.GetUserDivisions()
                             on a.Division equals d.DivCode
                         select a).ToList();

            return PartialView(new GridModel(returnValue));
        }

        [GridAction]
        public ActionResult _RDQRestrictionDCs()
        {
            List<DistributionCenter> returnValue = new List<DistributionCenter>();

            var list = (from rr in db.RDQRestrictions
                        join id in db.InstanceDivisions
                          on rr.Division equals id.Division
                        join dc in db.DistributionCenters
                          on rr.ToDCCode equals dc.MFCode
                      select new { Division = rr.Division, DistributionCenter = dc}).Distinct().ToList();

            returnValue = (from a in list
                           join d in currentUser.GetUserDivisions()
                             on a.Division equals d.DivCode
                         select a.DistributionCenter).ToList();

            return PartialView(new GridModel(returnValue));
        }

        [GridAction]
        public ActionResult _RDQRestrictionsForStore(string div, string store)
        {
            List<RDQRestriction> returnValue = new List<RDQRestriction>();
            returnValue = db.RDQRestrictions.Where(rr => rr.Division.Equals(div) && rr.ToStore.Equals(store)).ToList();
            return PartialView(new GridModel(returnValue));
        }

        [GridAction]
        public ActionResult _RDQRestrictionsForRegion(string div, string region)
        {
            List<RDQRestriction> returnValue = new List<RDQRestriction>();
            returnValue = db.RDQRestrictions.Where(rr => rr.Division.Equals(div) && rr.ToRegion.Equals(region)).ToList();
            return PartialView(new GridModel(returnValue));
        }

        [GridAction]
        public ActionResult _RDQRestrictionsForLeague(string div, string league)
        {
            List<RDQRestriction> returnValue = new List<RDQRestriction>();
            returnValue = db.RDQRestrictions.Where(rr => rr.Division.Equals(div) && rr.ToLeague.Equals(league)).ToList();
            return PartialView(new GridModel(returnValue));
        }

        [GridAction]
        public ActionResult _RDQRestrictionsForDC(string dc)
        {
            List<RDQRestriction> returnValue = new List<RDQRestriction>();
            returnValue = db.RDQRestrictions.Where(rr => rr.ToDCCode.Equals(dc)).ToList();
            return PartialView(new GridModel(returnValue));
        }

        [GridAction]
        public ActionResult _RDQRestrictionProducts()
        {
            List<RDQRestriction> returnValue = new List<RDQRestriction>();

            List<string> divisions = currentUser.GetUserDivList()
                    .Distinct()
                    .ToList();

            var list  = db.RDQRestrictions
                    .Where(rr => divisions.Contains(rr.Division))
                    .Select(rr => new { rr.Division, rr.Department, rr.Category, rr.Brand, rr.SKU })
                    .Distinct()
                    .ToList();

            returnValue = list
                .Select(rr => new RDQRestriction() { Division = rr.Division, Department = rr.Department, Category = rr.Category, Brand = rr.Brand, SKU = rr.SKU } )
                .OrderBy(rr => rr.Division)
                .ThenBy(rr => rr.Department)
                .ThenBy(rr => rr.Category)
                .ThenBy(rr => rr.Brand)
                .ThenBy(rr => rr.SKU)
                .ToList();

            return PartialView(new GridModel(returnValue));
        }

        [GridAction]
        public ActionResult _RDQRestrictionsForProduct(string div, string dept, string cat, string brand, string sku)
        {
            List<RDQRestriction> returnValue;

            returnValue = RetrieveRDQRestrictionsForProduct(div, dept, cat, brand, sku);

            return PartialView(new GridModel(returnValue));
        }

        public ActionResult DeleteRDQRestrictionsByProduct(string div, string dept, string cat, string brand, string sku)
        {
            List<RDQRestriction> rdqRestrictions = new List<RDQRestriction>();

            rdqRestrictions = RetrieveRDQRestrictionsForProduct(div, dept, cat, brand, sku);

            foreach (var rr in rdqRestrictions)
            {
                db.RDQRestrictions.Remove(rr);
            }

            db.SaveChanges();
            string message = "Deleted " + rdqRestrictions.Count() + " RDQ Restrictions.";           
            return RedirectToAction("IndexByProduct", new { message = message });
        }

        public ActionResult DeleteRDQRestrictionsByStore(string div, string store)
        {
            List<RDQRestriction> rdqRestrictions
                = db.RDQRestrictions
                    .Where(rr => rr.Division.Equals(div) &&
                                 rr.ToStore.Equals(store)).ToList();

            foreach (var rr in rdqRestrictions)
            {
                db.RDQRestrictions.Remove(rr);
            }

            db.SaveChanges();
            string message = "Deleted " + rdqRestrictions.Count() + " RDQ Restrictions.";
            return RedirectToAction("IndexByDestination", new { message = message, destinationType = "Store" });
        }

        public ActionResult DeleteRDQRestrictionsByLeague(string div, string league)
        {
            List<RDQRestriction> rdqRestrictions
                = db.RDQRestrictions
                    .Where(rr => rr.Division.Equals(div) &&
                                 rr.ToLeague.Equals(league)).ToList();

            foreach (var rr in rdqRestrictions)
            {
                db.RDQRestrictions.Remove(rr);
            }

            db.SaveChanges();
            string message = string.Format("Deleted {0} RDQ restrictions", rdqRestrictions.Count());
            return RedirectToAction("IndexByDestination", new { message = message, destinationType = "League" });
        }

        public ActionResult DeleteRDQRestrictionsByRegion(string div, string region)
        {
            List<RDQRestriction> rdqRestrictions
                = db.RDQRestrictions
                    .Where(rr => rr.Division.Equals(div) &&
                                 rr.ToRegion.Equals(region)).ToList();

            foreach (var rr in rdqRestrictions)
            {
                db.RDQRestrictions.Remove(rr);
            }

            db.SaveChanges();
            string message = string.Format("Deleted {0} RDQRestrictions", rdqRestrictions.Count());
            return RedirectToAction("IndexByDestination", new { message = message, destinationType = "Region" });
        }

        public ActionResult DeleteRDQRestrictionsByDC(string dc)
        {
            List<RDQRestriction> rdqRestrictions
                = db.RDQRestrictions
                    .Where(rr => rr.ToDCCode.Equals(dc)).ToList();

            foreach (var rr in rdqRestrictions)
            {
                db.RDQRestrictions.Remove(rr);
            }

            db.SaveChanges();
            string message = string.Format("Deleted {0} RDQ Restrictions.", rdqRestrictions.Count());
            return RedirectToAction("IndexByDestination", new { message = message, destinationType = "DC" });
        }

        private void RevertDefaultValues(RDQRestriction rr)
        {
            const string defaultValue = "N/A";

            rr.Department = rr.Department.Equals(defaultValue) || string.IsNullOrEmpty(rr.Department) ? null : rr.Department;
            rr.Category = rr.Category.Equals(defaultValue) || string.IsNullOrEmpty(rr.Category) ? null : rr.Category;
            rr.Brand = rr.Brand.Equals(defaultValue) || string.IsNullOrEmpty(rr.Brand) ? null : rr.Brand;
            rr.FromDCCode = rr.FromDCCode.Equals(defaultValue) || string.IsNullOrEmpty(rr.FromDCCode) ? null : rr.FromDCCode;
            rr.ToDCCode = rr.ToDCCode.Equals(defaultValue) || string.IsNullOrEmpty(rr.ToDCCode) ? null : rr.ToDCCode;
            rr.RDQType = rr.RDQType.Equals(defaultValue) || string.IsNullOrEmpty(rr.RDQType) ? null : rr.RDQType;
            rr.Vendor = string.IsNullOrEmpty(rr.Vendor) ? null : rr.Vendor;
            rr.ToLeague = string.IsNullOrEmpty(rr.ToLeague) ? null : rr.ToLeague;
            rr.ToRegion = string.IsNullOrEmpty(rr.ToRegion) ? null : rr.ToRegion;
            rr.ToStore = string.IsNullOrEmpty(rr.ToStore) ? null : rr.ToStore;
        }

        private bool ValidateRDQRestriction(RDQRestrictionModel model, out string errorMessage)
        {
            bool result = true;
            errorMessage = null;
            RDQRestriction rr = model.RDQRestriction;
            const string lineBreak = @"<br />";

            // revert any values that have the default value (N/A) to null
            this.RevertDefaultValues(rr);

            // check for duplicate
            bool duplicate
                = db.RDQRestrictions
                    .Any(r => r.Division.Equals(rr.Division) &&
                              ((r.Department == null && rr.Department == null) || r.Department.Equals(rr.Department)) &&
                              ((r.Category == null && rr.Category == null) || r.Category.Equals(rr.Category)) &&
                              ((r.Brand == null && rr.Brand == null) || r.Brand.Equals(rr.Brand)) &&
                              ((r.RDQType == null && rr.RDQType == null) || r.RDQType.Equals(rr.RDQType)) &&
                              ((r.Vendor == null && rr.Vendor == null) || r.Vendor.Equals(rr.Vendor)) &&
                              ((r.SKU == null && rr.SKU == null) || r.SKU.Equals(rr.SKU)) &&
                              ((r.FromDCCode == null && rr.FromDCCode == null) || r.FromDCCode.Equals(rr.FromDCCode)) &&
                              ((r.ToDCCode == null && rr.ToDCCode == null) || r.ToDCCode.Equals(rr.ToDCCode)) &&
                              ((r.ToLeague == null && rr.ToLeague == null) || r.ToLeague.Equals(rr.ToLeague)) &&
                              ((r.ToRegion == null && rr.ToRegion == null) || r.ToRegion.Equals(rr.ToRegion)) &&
                              ((r.ToStore == null && rr.ToStore == null) || r.ToStore.Equals(rr.ToStore)) &&
                              r.RDQRestrictionID != rr.RDQRestrictionID);

            if (duplicate)
            {
                // err
                result = false;
                // err message
                errorMessage = "There is already an existing record for the criteria populated.";
                return result;
            }

            bool hasDepartmentSelected = !string.IsNullOrEmpty(rr.Department);
            bool hasCategorySelected = !string.IsNullOrEmpty(rr.Category);
            bool hasBrandSelected = !string.IsNullOrEmpty(rr.Brand);
            bool hasRDQTypeSelected = !string.IsNullOrEmpty(rr.RDQType);
            bool hasFromDCSelected = !string.IsNullOrEmpty(rr.FromDCCode);
            bool hasToDCSelected = !string.IsNullOrEmpty(rr.ToDCCode);
            bool hasSKUSelected = !string.IsNullOrEmpty(rr.SKU);

            if (hasDepartmentSelected && !hasCategorySelected && !hasBrandSelected)
            {
                // validation for division/department combination
                var validCombination
                    = db.ItemMasters.Any(im => im.Div.Equals(rr.Division) &&
                                               im.Dept.Equals(rr.Department));

                if (!validCombination)
                {
                    // err
                    result = false;
                    string message = "The Division and Department combination is not valid.";
                    errorMessage += (errorMessage == null) ? message : lineBreak + message;
                }
            }
            else if (hasDepartmentSelected && hasCategorySelected && !hasBrandSelected)
            {
                // validation for division/department/category combination
                var validCombination
                    = db.ItemMasters.Any(im => im.Div.Equals(rr.Division) &&
                                               im.Dept.Equals(rr.Department) &&
                                               im.Category.Equals(rr.Category));

                if (!validCombination)
                {
                    // err
                    result = false;
                    string message = "The Division, Department, and Category combination is not valid.";
                    errorMessage += (errorMessage == null) ? message : lineBreak + message;
                }
            }
            else if (hasDepartmentSelected && hasCategorySelected && hasBrandSelected)
            {
                // validation for division/department/category/brand combination
                var validCombination
                    = db.ItemMasters.Any(im => im.Div.Equals(rr.Division) &&
                                               im.Dept.Equals(rr.Department) &&
                                               im.Category.Equals(rr.Category) &&
                                               im.Brand.Equals(rr.Brand));

                if (!validCombination)
                {
                    // err
                    result = false;
                    string message = "The Division, Department, Category, and Brand combination is not valid.";
                    errorMessage += (errorMessage == null) ? message : lineBreak + message;
                }
            }
             
            if (hasDepartmentSelected && hasSKUSelected)
            {
                // validation for division/department/category/brand combination
                var validCombination = db.ItemMasters.Any(im => im.MerchantSku.Equals(rr.SKU) &&
                                                                im.Div.Equals(rr.Division) &&
                                                                im.Dept.Equals(rr.Department) &&
                                                                (im.Category.Equals(rr.Category) || !hasCategorySelected) &&
                                                                (im.Brand.Equals(rr.Brand) || !hasBrandSelected));

                if (!validCombination)
                {
                    // err
                    result = false;
                    string message = "The Division, Department, Category, Brand, and SKU combination is not valid.";
                    errorMessage += (errorMessage == null) ? message : lineBreak + message;
                }
            }

            // validate that only one of the following four properties exist: ToLeague, ToRegion, ToStore, ToDCCode

            int existsCounter = 0;
            existsCounter += !string.IsNullOrEmpty(rr.ToLeague) ? 1 : 0;
            existsCounter += !string.IsNullOrEmpty(rr.ToRegion) ? 1 : 0;
            existsCounter += !string.IsNullOrEmpty(rr.ToStore) ? 1 : 0;
            existsCounter += !string.IsNullOrEmpty(rr.ToDCCode) ? 1 : 0;

            if (existsCounter > 1)
            {
                // err
                result = false;
                string message = "Cannot have more than one of the following properties populated: To League, To Region, To Store, To DC Code.";
                errorMessage += (errorMessage == null) ? message : lineBreak + message;
            }

            // validate vendor
            if (!string.IsNullOrEmpty(rr.Vendor))
            {
                var validVendor = (from v in db.Vendors
                                   join id in db.InstanceDivisions
                                     on v.InstanceID equals id.InstanceID
                                   where id.Division.Equals(rr.Division) &&
                                         v.VendorCode.Equals(rr.Division + "-" + rr.Vendor)
                                   select v).Any();

                if (!validVendor)
                {
                    // err
                    result = false;
                    string message = string.Format("The division/vendor combination is not valid. {0}, {1}", rr.Division, rr.Vendor);
                    errorMessage += (errorMessage == null) ? message : lineBreak + message;
                }
                else               
                {
                    if (!string.IsNullOrEmpty(rr.SKU))
                    {
                        validVendor = db.ItemMasters.Any(im => im.MerchantSku.Equals(rr.SKU) &&
                                                               im.MerchantSku.Equals(rr.Vendor));
                        if (!validVendor)
                        {
                            // err
                            result = false;
                            string message = string.Format("The vendor is not valid for this SKU. {0}, {1}", rr.Vendor, rr.SKU);
                            errorMessage += (errorMessage == null) ? message : lineBreak + message;
                        }
                    }
                }
            }

            // validate store
            if (!string.IsNullOrEmpty(rr.ToStore))
            {
                var validStore
                    = db.StoreLookups.Any(sl => sl.Division.Equals(rr.Division) &&
                                                sl.Store.Equals(rr.ToStore));

                if (!validStore)
                {
                    // err
                    result = false;
                    string message = string.Format("The division/store combination is not valid. {0}-{1}", rr.Division, rr.ToStore);
                    errorMessage += (errorMessage == null) ? message : lineBreak + message;
                }
            }

            // validate region
            if (!string.IsNullOrEmpty(rr.ToRegion))
            {
                var validRegion
                    = db.StoreLookups.Any(sl => sl.Division.Equals(rr.Division) &&
                                                sl.Region.Equals(rr.ToRegion));

                if (!validRegion)
                {
                    // err
                    result = false;
                    string message = string.Format("The division/region combination is not valid. {0}, {1}", rr.Division, rr.ToRegion);
                    errorMessage += (errorMessage == null) ? message : lineBreak + message;
                }
            }

            // validate league
            if (!string.IsNullOrEmpty(rr.ToLeague))
            {
                var validLeague
                    = db.StoreLookups.Any(sl => sl.Division.Equals(rr.Division) &&
                                                sl.League.Equals(rr.ToLeague));

                if (!validLeague)
                {
                    // err
                    result = false;
                    string message = string.Format("The division/league combination is not valid. {0}, {1}", rr.Division, rr.ToLeague);
                    errorMessage += (errorMessage == null) ? message : lineBreak + message;
                }
            }

            // validate dates
            if (rr.FromDate > rr.ToDate)
            {
                result = false;
                string message = string.Format("The from date is greater than the to date.");
                errorMessage += (errorMessage == null) ? message : lineBreak + message;
            }

            return result;
        }

        private RDQRestrictionModel FillModelLists(RDQRestrictionModel model)
        {
            RDQRestriction rr = model.RDQRestriction;

            // populate division
            model.Divisions = GetDivisionsList();
            bool existentDivision = model.Divisions.Any(d => d.Value.Equals(rr.Division));
            if (!existentDivision && model.Divisions.Count() > 0)
            {
                rr.Division = model.Divisions.First().Value;
            }

            // populate department
            model.Departments = GetDepartmentsList(rr.Division);
            bool existentDepartment = model.Departments.Any(d => d.Value.Equals(rr.Department));
            if (!existentDepartment && model.Departments.Count() > 0)
            {
                rr.Department = model.Departments.First().Value;
            }

            // populate category
            model.Categories = GetCategoriesList(rr.Division, rr.Department);
            bool existentCategory = model.Categories.Any(c => c.Value.Equals(rr.Category));
            if (!existentCategory && model.Categories.Count() > 0)
            {
                rr.Category = model.Categories.First().Value;
            }

            // populate brand
            model.Brands = GetBrandsList(rr.Division, rr.Department, rr.Category);
            bool existentBrand = model.Brands.Any(b => b.Value.Equals(rr.Brand));
            if (!existentBrand && model.Brands.Count() > 0)
            {
                rr.Brand = model.Brands.First().Value;
            }

            // populate from distribution centers
            model.DistributionCenters = GetDistributionCentersList(model.RDQRestriction.Division);
            bool existentFromDC = model.DistributionCenters.Any(dc => dc.Value.Equals(rr.FromDCCode));
            if (!existentFromDC && model.DistributionCenters.Count() > 0)
            {
                rr.FromDCCode = model.DistributionCenters.First().Value;
            }

            bool existentToDC = model.DistributionCenters.Any(dc => dc.Value.Equals(rr.ToDCCode));
            if (!existentToDC && model.DistributionCenters.Count() > 0)
            {
                rr.ToDCCode = model.DistributionCenters.First().Value;
            }

            // populate rdq types
            model.RDQTypes = GetRDQTypes();
            bool existentRDQType = model.RDQTypes.Any(r => r.Value.Equals(rr.RDQType));
            if (!existentRDQType && model.DistributionCenters.Count() > 0)
            {
                rr.RDQType = model.RDQTypes.First().Value;
            }

            return model;
        }

        private List<RDQRestriction> RetrieveRDQRestrictionsForProduct(string div, string dept, string cat, string brand, string sku)
        {
            List<RDQRestriction> returnValue = new List<RDQRestriction>();

            bool departmentExists = !string.IsNullOrEmpty(dept);
            bool categoryExists = !string.IsNullOrEmpty(cat);
            bool brandExists = !string.IsNullOrEmpty(brand);
            bool skuExists = !string.IsNullOrEmpty(sku);

            returnValue = db.RDQRestrictions
                .Where(rr => rr.Division.Equals(div) && 
                             ((departmentExists && rr.Department.Equals(dept)) || (!departmentExists && rr.Department == null)) &&
                             ((categoryExists && rr.Category.Equals(cat)) || (!categoryExists && rr.Category == null)) &&
                             ((brandExists && rr.Brand.Equals(brand)) || (!brandExists && rr.Brand == null)) &&
                             ((skuExists && rr.SKU.Equals(sku)) || (!skuExists && (rr.SKU == null)))).ToList();

            return returnValue;
        }

        public ActionResult DeleteRDQRestriction(int id)
        {
            RDQRestriction rr
                = db.RDQRestrictions
                    .Where(r => r.RDQRestrictionID.Equals(id))
                    .FirstOrDefault();

            if (rr != null)
            {
                db.RDQRestrictions.Remove(rr);
                db.SaveChanges();
            }
            else
            {
                return RedirectToAction("Index", new { message = "The RDQ Restriction no longer exists." });
            }

            return RedirectToAction("Index");
        }

        #region Upload restrictions
        public ActionResult Upload(string message)
        {
            ViewData["errorMessage"] = message;
            return View();
        }

        public ActionResult ExcelTemplate()
        {
            RDQRestrictionsSpreadsheet rdqRestrictionsSpreadsheet = new RDQRestrictionsSpreadsheet(appConfig, new ConfigService(), new RDQDAO());
            Workbook excelDocument;

            excelDocument = rdqRestrictionsSpreadsheet.GetTemplate();

            excelDocument.Save(System.Web.HttpContext.Current.Response, "RDQRestrictionUpload.xlsx", ContentDisposition.Attachment, rdqRestrictionsSpreadsheet.SaveOptions);
            return View();
        }

        public ActionResult SaveRDQRestrictions(IEnumerable<HttpPostedFileBase> attachments)
        {
            RDQRestrictionsSpreadsheet rdqRestrictionsSpreadsheet = new RDQRestrictionsSpreadsheet(appConfig, new ConfigService(), new RDQDAO());

            int successCount = 0;
            string message;

            foreach (HttpPostedFileBase file in attachments)
            {
                rdqRestrictionsSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(rdqRestrictionsSpreadsheet.message))
                    return Content(rdqRestrictionsSpreadsheet.message);
                else
                {
                    if (rdqRestrictionsSpreadsheet.errorList.Count() > 0)
                    {
                        Session["errorList"] = rdqRestrictionsSpreadsheet.errorList;

                        message = string.Format("{0} successfully uploaded, {1} Errors", rdqRestrictionsSpreadsheet.validRecs.Count.ToString(),
                            rdqRestrictionsSpreadsheet.errorList.Count.ToString());

                        return Content(message);
                    }
                }

                successCount += rdqRestrictionsSpreadsheet.validRecs.Count();
            }

            return Json(new { message = string.Format("Upload complete. Added {0} record(s)", successCount) }, "application/json");
        }

        public ActionResult DownloadErrors()
        {
            List<Tuple<RDQRestriction, string>> errors = (List<Tuple<RDQRestriction, string>>)Session["errorList"];
            Workbook excelDocument;
            RDQRestrictionsSpreadsheet rdqRestrictionsSpreadsheet = new RDQRestrictionsSpreadsheet(appConfig, new ConfigService(), new RDQDAO());

            if (errors != null)
            {
                excelDocument = rdqRestrictionsSpreadsheet.GetErrors(errors);
                excelDocument.Save(System.Web.HttpContext.Current.Response, "RDQRestrictionErrors.xlsx", ContentDisposition.Attachment, rdqRestrictionsSpreadsheet.SaveOptions);
            }

            return View();
        }
        #endregion

        #region JSON result routines

        public JsonResult GetNewDepartments(string division)
        {
            List<SelectListItem> newDeptList = GetDepartmentsList(division);
            return Json(new SelectList(newDeptList.ToArray(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNewCategories(string division, string department)
        {
            List<SelectListItem> newCategoriesList = GetCategoriesList(division, department);
            return Json(new SelectList(newCategoriesList.ToArray(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNewBrands(string division, string department, string category)
        {
            List<SelectListItem> newBrandsList = GetBrandsList(division, department, category);
            return Json(new SelectList(newBrandsList.ToArray(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNewDistributionCenters(string division)
        {
            List<SelectListItem> newDistributionCenters = GetDistributionCentersList(division);
            return Json(new SelectList(newDistributionCenters.ToArray(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region UI dropdown lists

        private List<SelectListItem> GetDivisionsList()
        {
            List<SelectListItem> divisionList = new List<SelectListItem>();
            List<Division> userDivList = currentUser.GetUserDivisions(); 
            var divsWithDepts = (from a in db.Departments select a.divisionCode).Distinct();


            if (userDivList.Count() > 0)
            {
                foreach (var rec in userDivList)
                {
                    if (divsWithDepts.Contains(rec.DivCode))
                    {
                        divisionList.Add(new SelectListItem { Text = rec.DisplayName, Value = rec.DivCode });
                    }
                }
            }

            return divisionList;
        }

        private List<SelectListItem> GetDepartmentsList(string division)
        {
            List<SelectListItem> departmentList = new List<SelectListItem>();
            List<Departments> departments = new List<Departments>();

            departments = GetValidDepartments(division);

            if (departments.Count() > 0)
            {
                foreach (var rec in departments)
                {
                    departmentList.Add(new SelectListItem { Text = rec.departmentDisplay, Value = rec.departmentCode });
                }
            }
            departmentList = departmentList.OrderBy(d => d.Text).ToList();
            departmentList.Insert(0, new SelectListItem { Text = "Select a Department...", Value = "N/A" });

            return departmentList;
        }

        private List<SelectListItem> GetCategoriesList(string division, string department)
        {
            List<SelectListItem> categoryList = new List<SelectListItem>();
            List<Categories> categories = new List<Categories>();

            categories = GetValidCategories(division, department);

            if (categories.Count() > 0)
            {
                foreach (var rec in categories)
                {
                    categoryList.Add(new SelectListItem { Text = rec.CategoryDisplay, Value = rec.categoryCode });
                }
            }
            categoryList = categoryList.OrderBy(c => c.Text).ToList();
            categoryList.Insert(0, new SelectListItem { Text = "Select a Category...", Value = "N/A" });

            return categoryList;
        }

        private List<SelectListItem> GetBrandsList(string division, string department, string category)
        {
            List<SelectListItem> brandIDList = new List<SelectListItem>();
            List<BrandIDs> brands = new List<BrandIDs>();

            brands = GetValidBrands(division, department, category);

            if (brands.Count() > 0)
            {
                foreach (var rec in brands)
                {
                    brandIDList.Add(new SelectListItem { Text = rec.brandIDDisplay, Value = rec.brandIDCode });
                }
            }
            brandIDList = brandIDList.OrderBy(b => b.Text).ToList();
            brandIDList.Insert(0, new SelectListItem { Text = "Select a Brand...", Value = "N/A" });

            return brandIDList;
        }

        private List<SelectListItem> GetDistributionCentersList(string division)
        {
            List<SelectListItem> distributionCentersList = new List<SelectListItem>();
            List<DistributionCenter> distributionCenters = new List<DistributionCenter>();

            distributionCenters = GetValidDistributionCenters(division);

            if (distributionCenters.Count() > 0)
            {
                foreach (var dc in distributionCenters)
                {
                    distributionCentersList.Add(new SelectListItem { Text = dc.displayValue, Value = dc.MFCode });
                }
            }
            distributionCentersList = distributionCentersList.OrderBy(d => d.Text).ToList();
            distributionCentersList.Insert(0, new SelectListItem { Text = "Select a Distribution Center...", Value = "N/A" });

            return distributionCentersList;
        }

        private List<SelectListItem> GetRDQTypes()
        {
            List<SelectListItem> rdqTypesList = new List<SelectListItem>();
            List<RDQType> rdqTypes = new List<RDQType>();

            rdqTypes = db.RDQTypes.ToList();

            if (rdqTypes.Count() > 0)
            {
                foreach (var rt in rdqTypes)
                {
                    rdqTypesList.Add(new SelectListItem { Text = rt.RDQTypeName, Value = rt.RDQTypeName });
                }
            }
            rdqTypesList = rdqTypesList.OrderBy(r => r.Text).ToList();
            rdqTypesList.Insert(0, new SelectListItem { Text = "Select an RDQ Type", Value = "N/A" });

            return rdqTypesList;
        }

        private List<SelectListItem> GetDestinationsList()
        {
            List<SelectListItem> destinationsList = new List<SelectListItem>()
            {
                new SelectListItem { Text = "Store", Value = "Store" },
                new SelectListItem { Text = "Region", Value = "Region" },
                new SelectListItem { Text = "League", Value = "League" },
                new SelectListItem { Text = "DC", Value = "DC" }
            };       

            return destinationsList;
        }

        #endregion

        #region Valid Combinations

        /// <summary>
        /// Retrieve all valid departments from the ItemMaster given the specified division
        /// </summary>
        /// <param name="division">specified division</param>
        /// <returns>List of valid departments</returns>
        private List<Departments> GetValidDepartments(string division)
        {
            List<Departments> departments = new List<Departments>();

            departments = (from im in db.ItemMasters
                           join d in db.Departments
                             on new { Division = im.Div, Department = im.Dept } equals
                                new { Division = d.divisionCode, Department = d.departmentCode }
                           where im.Div == division &&
                                 // ensure that the brandid associated with ItemMaster record exists in the Brands table.
                                 (from b in db.BrandIDs
                                  where b.brandIDCode == im.Brand &&
                                       b.divisionCode == im.Div &&
                                       b.departmentCode == im.Dept
                                  select b.brandIDCode).Distinct().Contains(im.Brand)
                           select d).Distinct().ToList();

            return departments;
        }

        /// <summary>
        /// Retrieve all valid categories from the ItemMaster table given the specified division and department
        /// </summary>
        /// <param name="division">The specified division</param>
        /// <param name="department">The specified department</param>
        /// <returns>List of valid categories</returns>
        private List<Categories> GetValidCategories(string division, string department)
        {
            List<Categories> categories = new List<Categories>();
            const string defaultValue = "N/A";

            if (department.Equals(defaultValue))
            {
                categories = (from im in db.ItemMasters
                              join c in db.Categories
                                on new { Division = im.Div, Category = im.Category } equals
                                   new { Division = c.divisionCode, Category = c.categoryCode }
                             where im.Div == division &&
                                   (from b in db.BrandIDs
                                   where b.brandIDCode == im.Brand &&
                                         b.divisionCode == im.Div
                                  select b.brandIDCode).Distinct().Contains(im.Brand)
                            select c).Distinct().ToList();
            }
            else
            {
                categories = (from im in db.ItemMasters
                              join c in db.Categories
                                on new { Category = im.Category, Division = im.Div, Department = im.Dept } equals
                                   new { Category = c.categoryCode, Division = c.divisionCode, Department = c.departmentCode }
                             where im.Div == division &&
                                   im.Dept == department &&
                                   // ensure that the brandid associated with ItemMaster record exists in the Brands table.
                                   (from b in db.BrandIDs
                                   where b.brandIDCode == im.Brand &&
                                         b.divisionCode == im.Div &&
                                         b.departmentCode == im.Dept
                                  select b.brandIDCode).Distinct().Contains(im.Brand)
                           select c).Distinct().ToList();
            }

            categories = categories.Select(c => new Categories { divisionCode = c.divisionCode, categoryCode = c.categoryCode, CategoryName = c.CategoryName }).Distinct().ToList();

            return categories;
        }

        /// <summary>
        /// Retrieve all valid brands from the ItemMaster table given the specified division,
        /// department, and category
        /// </summary>
        /// <param name="division">The specified division</param>
        /// <param name="department">The specified department</param>
        /// <param name="category">The specified category</param>
        /// <returns>List of valid brands</returns>
        private List<BrandIDs> GetValidBrands(string division, string department, string category)
        {
            List<BrandIDs> brands = new List<BrandIDs>();
            const string defaultValue = "N/A";

            bool departmentExists = !department.Equals(defaultValue);
            bool categoryExists = !category.Equals(defaultValue);

            brands = (from im in db.ItemMasters
                      join b in db.BrandIDs
                        on new { Division = im.Div, Department = im.Dept, Brand = im.Brand} equals
                           new { Division = b.divisionCode, Department = b.departmentCode, Brand = b.brandIDCode }
                     where im.Div == division &&
                           im.Dept == (departmentExists ? department : im.Dept) &&
                           im.Category == (categoryExists ? category : im.Category)
                    select b).Distinct().ToList();

            brands = brands.Select(b => new BrandIDs { divisionCode = b.divisionCode, brandIDCode = b.brandIDCode, brandIDName = b.brandIDName }).Distinct().ToList();

            return brands;
        }

        private List<DistributionCenter> GetValidDistributionCenters(string division)
        {
            List<DistributionCenter> distributionCenters = new List<DistributionCenter>();

            int instanceID
                = db.InstanceDivisions
                    .Where(id => id.Division.Equals(division))
                    .Select(id => id.InstanceID)
                    .FirstOrDefault();

            if (instanceID > 0)
            {
                distributionCenters = (from idc in db.InstanceDistributionCenters
                                       join dc in db.DistributionCenters
                                         on idc.DCID equals dc.ID
                                      where idc.InstanceID.Equals(instanceID)
                                     select dc).Distinct().ToList();
            }

            return distributionCenters;
        }

        private bool ValidateCombination(RDQRestriction rr, out string errorMessage)
        {
            bool result = true;
            errorMessage = null;

            bool departmentExists = !string.IsNullOrEmpty(rr.Department);
            bool categoryExists = !string.IsNullOrEmpty(rr.Category);
            bool brandExists = !string.IsNullOrEmpty(rr.Brand);

            if (!departmentExists && !categoryExists && !brandExists)
            {
                var comboExists
                    = db.ItemMasters
                        .Any(im => im.Div.Equals(rr.Division));

                if (!comboExists)
                {
                    result = false;
                    errorMessage = string.Format("The division does not exist within the system. {0}", rr.Division);
                }
            }
            else if (departmentExists && !categoryExists && !brandExists)
            {
                var comboExists
                    = db.ItemMasters
                        .Any(im => im.Div.Equals(rr.Division) &&
                                   im.Dept.Equals(rr.Department));

                if (!comboExists)
                {
                    result = false;
                    errorMessage = string.Format(
                        "The division / department combination does not exist within the system. {0}-{1}"
                        , rr.Division
                        , rr.Department);
                }
            }
            else if (departmentExists && categoryExists && !brandExists)
            {
                var comboExists
                    = db.ItemMasters
                        .Any(im => im.Div.Equals(rr.Division) &&
                                   im.Dept.Equals(rr.Department) &&
                                   im.Category.Equals(rr.Category));

                if (!comboExists)
                {
                    result = false;
                    errorMessage = string.Format(
                        "The division / department / category combination does not exist within the system. {0}-{1}-{2}"
                        , rr.Division
                        , rr.Department
                        , rr.Category);
                }
            }
            else if (departmentExists && categoryExists && brandExists)
            {
                var comboExists
                    = db.ItemMasters
                        .Any(im => im.Div.Equals(rr.Division) &&
                                   im.Dept.Equals(rr.Department) &&
                                   im.Category.Equals(rr.Category) &&
                                   im.Brand.Equals(rr.Brand));

                if (!comboExists)
                {
                    result = false;
                    errorMessage = string.Format(
                        "The division / department / category / brand combination does not exist within the system. {0}-{1}-{2}-{3}"
                        , rr.Division
                        , rr.Department
                        , rr.Category
                        , rr.Brand);
                }
            }
            else if (!departmentExists && !categoryExists && brandExists)
            {
                var comboExists
                    = db.ItemMasters
                        .Any(im => im.Div.Equals(rr.Division) &&
                                   im.Brand.Equals(rr.Brand));

                if (!comboExists)
                {
                    result = false;
                    errorMessage = string.Format(
                        "The division / brand combination does not exist within the system. {0}-{1}"
                        , rr.Division
                        , rr.Brand);
                }
            }
            else if (departmentExists && !categoryExists && brandExists)
            {
                var comboExists
                    = db.ItemMasters
                        .Any(im => im.Div.Equals(rr.Division) &&
                                   im.Dept.Equals(rr.Department) &&
                                   im.Brand.Equals(rr.Brand));

                if (!comboExists)
                {
                    result = false;
                    errorMessage = string.Format(
                        "The division / department / brand combination does not exist within the system. {0}-{1}-{2}"
                        , rr.Division
                        , rr.Department
                        , rr.Brand);
                }
            }

            return result;
        }
        #endregion
    }
}
