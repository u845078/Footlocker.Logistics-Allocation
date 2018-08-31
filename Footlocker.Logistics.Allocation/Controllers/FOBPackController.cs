using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Telerik.Web.Mvc;

using Footlocker.Logistics.Allocation.Common;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [HandleAjaxValidationError(Order = 3)]
    [LogError(Order = 2)]
    [HandleAjaxError(Order = 1)]
    [HandleError(Order = 0)]
    public class FOBPackController : AppController
    {
        #region HTTP GET Actions

        [HttpGet]
        public ActionResult Edit(int ID)
        {
            FOBPack domainObject = null;

            using (var context = new DAO.AllocationContext())
            {
                // Get corresponding concept type to specified id
                domainObject = context.FOBPacks.Include("FOB").Single(p => p.ID == ID);
            }

            return View("Edit", domainObject);
        }

        #endregion

        #region HTTP POST AJAX Actions

        [HttpPost]
        public ActionResult Save(FOBPack domainObject)
        {
            var isValid = ModelState.IsValid;

            using (var context = new DAO.AllocationContext())
            {
                // Perform Validation
                if (isValid)
                {
                    isValid = domainObject.ID > 0;

                    // Validate name is unique
                    //isValid = !db.ConceptTypes.Where(ct => ct.Name == domainObject.Name && ct.ID != domainObject.ID).AsNoTracking().Any();
                }

                if (isValid)
                {
                    // Clear the fob, as we are not intending to update it
                    domainObject.FOB = null;

                    // Update the Override
                    context.FOBPacks.Attach(domainObject);
                    context.Entry(domainObject).State = System.Data.EntityState.Modified;

                    // Commit
                    context.SaveChanges();
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
        public ActionResult Validation(FOBPack domainObject)
        {
            //// Re-apply custom model state errors
            //using (var context = new DAO.AllocationContext())
            //{
            //    //// Validate name is unique
            //    //if (context.FOBPacks.Where(ct => ct.Name == domainObject.Name && ct.ID != domainObject.ID).AsNoTracking().Any())
            //    //{
            //    //    ModelState.AddModelError("Name", "A FOB Pack with this name already exists.");
            //    //}
            //}

            // HACK: A better way to return Razor generated HTML with validation markup to 
            //           view after ajax post has occurred and Model validation failed (besides stuffing HTML into JSON)....
            // Being used as a GET for when the ModelState has errors
            return View("Editor", domainObject);
        }

        #endregion

        #region Telerik Grid Actions

        [GridAction]
        public ActionResult Grid_PacksByFOB(int FOBID)
        {
            // NOTE: Not donig a 'using' and disposing of context b/c we are allowing Telerik to enumerate to page
            var context = new DAO.AllocationContext();

            // NOTE: This enumerable needed to be enumerated due to custom viewmodel non-entity properties...important to enumerate twice like below to load nav properties...weird
            var packs = (FOBID > 0) ?
                context.FOBPacks.Include("Overrides.FOBDept")
                    .Where(p => p.FOBID == FOBID)
                    .OrderBy(o => o.Quantity)
                    .ToList()
                    .Select(p => new FOBPackModel() { DomainObject = p })
                    .ToList()
                : new List<FOBPackModel>().AsEnumerable();

            return View(new GridModel(packs));
        }

        #endregion
    }
}
