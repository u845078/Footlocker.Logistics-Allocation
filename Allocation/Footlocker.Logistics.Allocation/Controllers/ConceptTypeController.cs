using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [HandleAjaxValidationError(Order=3)]
    [LogError(Order = 2)]
    [HandleAjaxError(Order = 1)]
    [HandleError(Order = 0)]
    [CheckPermission(Roles = "Director of Allocation,Admin,Support")]
    public class ConceptTypeController : AppController
    {
        #region Initializations

        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        #endregion

        #region Non-Public Methods

        private bool IsWithAccessToConcept(int conceptTypeID, Footlocker.Logistics.Allocation.DAO.AllocationContext context)
        {
            // Get user's accessible divisons and concept divisions
            var accessibleDivCodes = currentUser.GetUserDivList();
            var fetchedConcept = context.ConceptTypes.Include("Divisions").Single(ct => ct.ID == conceptTypeID);
            var conceptDivisions = fetchedConcept.Divisions.Select(d => d.Division);

            // Determine and retain if user has access (via ALL divisions) to this concept
            return conceptDivisions.Any() ? conceptDivisions.Intersect(accessibleDivCodes).Count() == conceptDivisions.Count() : true;
        }

        private void LoadConcept(ConceptType concept, Footlocker.Logistics.Allocation.DAO.AllocationContext context)
        {
            // Get user's accessible divisons and concept divisions
            concept.IsUserWithAccess = IsWithAccessToConcept(concept.ID, context);
        }

        #endregion

        #region HTTP GET Actions

        [HttpGet]
        public ActionResult Create()
        {
            // Create a new concept type
            var newDomainObject = new ConceptType() { IsUserWithAccess = true };

            return View("Edit", newDomainObject);
        }

        [HttpGet]
        public ActionResult AddDivisions(int ID)
        {
            return View("AddDivisions", ID);
        }

        [HttpGet]
        public ActionResult Edit(int ID)
        {
            ConceptType domainObject = null;

            using (db)
            {
                // Get corresponding concept type to specified id
                domainObject = db.ConceptTypes.Find(ID);
                LoadConcept(domainObject, db);
            }

            return View("Edit", domainObject);
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        #endregion

        #region HTTP POST AJAX Actions

        [HttpPost]
        public ActionResult AddDivisions(int ID, List<string> divs)
        {
            var isUserWithConceptAccess = false;

            using (db)
            {
                // Persist the newly created ConceptTypeDivision
                divs.ForEach(div =>
                {
                    db.ConceptTypeDivisions.Add(new ConceptTypeDivision() { ConceptTypeID = ID, Division = div });
                });

                // Commit
                db.SaveChanges();

                // Determine if the current user has access to this concept
                isUserWithConceptAccess = IsWithAccessToConcept(ID, db);
            }
            
            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) { Data = isUserWithConceptAccess } };
        }

        [HttpPost]
        public ActionResult DeleteDivision(int conceptTypeID, string div)
        {
            var isUserWithConceptAccess = false;

            using (db)
            {
                var paddedDivValue = div.PadLeft(2, '0');
                var conceptTypeDiv = db.ConceptTypeDivisions.Single(ctd => ctd.ConceptTypeID == conceptTypeID && ctd.Division == paddedDivValue);
                db.ConceptTypeDivisions.Remove(conceptTypeDiv);
                db.SaveChanges();

                // Determine if the current user has access to this concept
                isUserWithConceptAccess = IsWithAccessToConcept(conceptTypeID, db);
            }

            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) { Data = isUserWithConceptAccess } };
        }

        [HttpPost]
        public ActionResult Delete(int ID)
        {            
            using (db)
            {
                // Validate no references
                if (db.StoreExtensions.Any(se => se.ConceptTypeID == ID)) { throw new DeleteValidationException(String.Empty, "Concept"); }

                // Perform deletion
                var conceptType = db.ConceptTypes.Find(ID);
                db.ConceptTypes.Remove(conceptType);
                db.SaveChanges();
            }

            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult Save(ConceptType domainObject)
        {
            var isValid = ModelState.IsValid;
            
            using (db)
            {
                // Perform Validation
                if (isValid)
                {
                    // Validate name is unique
                    isValid = !db.ConceptTypes.Where(ct => ct.Name == domainObject.Name && ct.ID != domainObject.ID).AsNoTracking().Any();
                }

                if (isValid)
                {
                    if (domainObject.ID > 0)
                    {
                        // Update the Concept Type
                        db.ConceptTypes.Attach(domainObject);
                        db.Entry(domainObject).State = System.Data.EntityState.Modified;
                    }
                    else
                    {
                        // Persist the newly created Concept Type
                        db.ConceptTypes.Add(domainObject);
                    }

                    // Commit
                    db.SaveChanges();
                }
                else
                {
                    // Validation Failed - throw exception
                    throw new AjaxValidationException(Url.Action("Validation"));
                }
            }

            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) { Data = domainObject.ID } };
        }

        [HttpPost]
        public ActionResult Validation(ConceptType domainObject)
        {
            // Re-apply custom model state errors
            using (db)
            {
                // Validate name is unique
                if (db.ConceptTypes.Where(ct => ct.Name == domainObject.Name && ct.ID != domainObject.ID).AsNoTracking().Any())
                {
                    ModelState.AddModelError("Name", "A Concept with this name already exists.");
                }
            }

            // HACK: A better way to return Razor generated HTML with validation markup to 
            //           view after ajax post has occurred and Model validation failed (besides stuffing HTML into JSON)....
            // Being used as a GET for when the ModelState has errors
            return View("Editor", domainObject);
        }

        #endregion

        #region Telerik Grid Actions

        [GridAction]
        public ActionResult Grid_Index()
        {
            // Get all concepts
            var conceptTypes = db.ConceptTypes.Include("Divisions");

            // HACK: Enumerating is killing the Telerik paging....need to return an IEnumerable from single query which also loads up IsUserWithAccess so Telerik can enumerate...
            // Load the additional user security info
            var conceptsList = conceptTypes.ToList();
            conceptsList.ForEach(c => LoadConcept(c, db));

            return View(new GridModel(conceptsList.OrderByDescending(c => c.IsUserWithAccess).ThenBy(c => c.Name)));
        }

        [GridAction]
        public ActionResult Grid_DivisionsByConceptType(int conceptTypeID)
        {
            // Get all divisions by concept type (WITHOUT Divisional Security applied)
            var conceptTypeDivisions = db.ConceptTypeDivisions.Where(ctd => ctd.ConceptTypeID == conceptTypeID);
            var allConceptDivisons = Footlocker.Common.DivisionService.ListDivisions().Where(div => conceptTypeDivisions.Select(d => d.Division).Contains(div.DivCode));
            var accessibleConceptDivCodes = currentUser.GetUserDivisions().Where(div => conceptTypeDivisions.Select(d => d.Division).Contains(div.DivCode)).Select(d => d.DivCode);
            var viewModelsList = 
                allConceptDivisons.Select(d => new SecuredDivisionModel() { 
                    DivCode = d.DivCode, 
                    DivisionName = d.DivisionName,
                    IsUserWithAccess = accessibleConceptDivCodes.Contains(d.DivCode)
                }).OrderByDescending(dvm => dvm.IsUserWithAccess).ThenBy(dvm => dvm.DivCode);

            return View(new GridModel(viewModelsList));
        }

        [GridAction]
        public ActionResult Grid_DivisionsNotWithConceptType(int conceptTypeID)
        {
            // Get all divisions not associated to concept type (WITH Divisional Security applied)
            var conceptTypeDivisions = db.ConceptTypeDivisions.Where(ctd => ctd.ConceptTypeID == conceptTypeID);
            var divisions = currentUser.GetUserDivisions().Where(div => !conceptTypeDivisions.Select(d => d.Division).Contains(div.DivCode));

            return View(new GridModel(divisions));
        }

        #endregion
    }
}