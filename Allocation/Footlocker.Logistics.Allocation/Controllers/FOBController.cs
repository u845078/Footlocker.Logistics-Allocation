using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Footlocker.Logistics.Allocation.Models;

using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Common;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class FOBController : AppController
    {
        #region HTTP GET Actions

        [HttpGet]
        public ActionResult SaveOptions(int ID, decimal newCost)
        {
            return View("SaveOptions", new FOBSaveOptionsModel(ID, newCost));
        }

        #endregion

        #region HTTP POST AJAX Actions

        [HttpPost]
        public ActionResult Save(FOBSaveOptionsModel viewModel)
        {
            FOB fob = null;
            decimal prevCost = -1;

            using (var context = new DAO.AllocationContext())
            {
                fob = context.FOBs.Find(viewModel.FOBID);

                // Retain the current cost
                prevCost = fob.DefaultCost;

                // Update the FOB to have new cost
                fob.DefaultCost = viewModel.NewCost;
                //context.FOBPacks.Attach(domainObject);
                context.Entry(fob).State = System.Data.EntityState.Modified;

                // Get all FOB Packs of FOB and limit to packs of selected save options choice
                var packsEnum = context.FOBPacks.Include("Overrides.FOBDept").Where(p => p.FOBID == viewModel.FOBID);
                if(viewModel.SelectedSaveChoiceID != (int)FOBSaveChoiceEntry.All)
                {
                    packsEnum = packsEnum.Where(p => p.Cost == prevCost);
                }
                
                // Update each pack's cost to the newly set cost value
                packsEnum.ToList().ForEach(p => p.Cost = viewModel.NewCost);
                
                // Commit
                context.SaveChanges();
            }

            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) { Data = fob.ID } };
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

        #region JSON Actions

        [HttpGet]
        public ActionResult Index(int id)
        {
            FOB domainObject = null;
            using (var context = new DAO.AllocationContext())
            {
                // Get specified FOB
                domainObject = context.FOBs.Find(id);
            }

            return new JsonResult() { Data = domainObject, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        #endregion
    }
}
