using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Footlocker.Logistics.Allocation.Models;

using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Common;

namespace Footlocker.Logistics.Allocation.Controllers
{
    public class FOBPackOverrideController : AppController
    {
        #region HTTP GET Actions

        [HttpGet]
        public ActionResult Create(int fobPackID)
        {
            // Create a new override for the specified FOB pack
            var newDomainObject = new FOBPackOverride() { FOBPackID = fobPackID };
            var availableDepts = new List<Footlocker.Common.Department>();

            using (var context = new DAO.AllocationContext())
            {
                // Load pack
                newDomainObject.FOBPack = context.FOBPacks
                    .Include("FOB.Departments")
                    .Single(p => p.ID == fobPackID);

                // Load available departments
                var fobDivCode = newDomainObject.FOBPack.FOB.Division;
                var fobDeptIDs = newDomainObject.FOBPack.FOB.Departments.Select(d => d.Department);
                var overridenDeptIDs = context.FOBPackOverrides.Include("FOBDept").Where(o => o.FOBPackID == fobPackID).Select(o => o.FOBDept.Department);
                availableDepts = Footlocker.Common.DepartmentService.ListDepartments()
                    .Where(d => (d.DivCode == fobDivCode) && fobDeptIDs.Contains(d.DeptNumber) && !overridenDeptIDs.Contains(d.DeptNumber))
                    .ToList();
            }

            return View("Edit", new FOBPackOverrideModel(newDomainObject, availableDepts));
        }

        [HttpGet]
        public ActionResult Edit(int ID)
        {
            FOBPackOverride domainObject = null;
            var availableDepts = new List<Footlocker.Common.Department>();

            using (var context = new DAO.AllocationContext())
            {
                // Get specified override pack
                domainObject = context.FOBPackOverrides
                    .Include("FOBDept")
                    .Include("FOBPack.FOB.Departments")
                    .Single(o => o.ID == ID);

                // Load available departments
                var overrideDeptId = domainObject.FOBDept.Department;
                var fobDivCode = domainObject.FOBPack.FOB.Division;
                var fobPackID = domainObject.FOBPack.ID;
                var fobDeptIDs = domainObject.FOBPack.FOB.Departments.Select(d => d.Department);
                var overridenDeptIDs = context.FOBPackOverrides.Include("FOBDept").Where(o => o.FOBPackID == fobPackID).Select(o => o.FOBDept.Department);
                availableDepts = Footlocker.Common.DepartmentService.ListDepartments()
                    .Where(d => (d.DivCode == fobDivCode) 
                        && ((fobDeptIDs.Contains(d.DeptNumber) && !overridenDeptIDs.Contains(d.DeptNumber))
                        || (d.DeptNumber == overrideDeptId)))
                    .ToList();
            }

            return View("Edit", new FOBPackOverrideModel(domainObject, availableDepts));
        }

        #endregion

        #region HTTP POST AJAX Actions

        [HttpPost]
        public ActionResult Delete(int ID)
        {
            using (var context = new DAO.AllocationContext())
            {
                // Perform deletion
                var packOverride = context.FOBPackOverrides.Find(ID);
                context.FOBPackOverrides.Remove(packOverride);
                context.SaveChanges();
            }

            // Return JSON representing Success
            return new JsonResult() { Data = new JsonResultData(ActionResultCode.Success) };
        }

        [HttpPost]
        public ActionResult Save([Bind(Prefix="DomainObject")]FOBPackOverride domainObject)
        {
            var isValid = ModelState.IsValid;

            using (var context = new DAO.AllocationContext())
            {
                // Perform Validation
                if (isValid)
                {
                    // Validate department
                    isValid = (domainObject.FOBDept != null && !String.IsNullOrWhiteSpace(domainObject.FOBDept.Department));
                    
                    // Validate name is unique
                    //isValid = !db.ConceptTypes.Where(ct => ct.Name == domainObject.Name && ct.ID != domainObject.ID).AsNoTracking().Any();
                }

                if (isValid)
                {
                    // Set fob dept id
                    var dept = domainObject.FOBDept.Department;
                    var fobID = context.FOBPacks.Find(domainObject.FOBPackID).FOBID;
                    domainObject.FOBDeptID =
                        context.FOBDepts.Single(d => d.FOBID == fobID && d.Department == dept).ID;

                    // Clear the dept and pack, as we are not intending to update it
                    domainObject.FOBDept = null;
                    domainObject.FOBPack = null;

                    if (domainObject.ID > 0)
                    {
                        // Update the Override
                        context.FOBPackOverrides.Attach(domainObject);
                        context.Entry(domainObject).State = System.Data.EntityState.Modified;
                    }
                    else
                    {
                        // Persist the newly created Override
                        context.FOBPackOverrides.Add(domainObject);
                    }

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
        public ActionResult Validation(FOBPackOverride domainObject)
        {
            //// Re-apply custom model state errors
            //using (var context = new DAO.AllocationContext())
            //{
            //    //// Validate name is unique
            //    //if (context.FOBPackOverrides.Where(ct => ct.Name == domainObject.Name && ct.ID != domainObject.ID).AsNoTracking().Any())
            //    //{
            //    //    ModelState.AddModelError("Name", "A Concept with this name already exists.");
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
        public ActionResult Grid_OverridesByFOBPack(int fobPackID)
        {
            // NOTE: Not donig a 'using' and disposing of context b/c we are allowing Telerik to enumerate to page
            var context = new DAO.AllocationContext();
            var packOverrides = context.FOBPackOverrides.Include("FOBDept").Where(o => o.FOBPackID == fobPackID);

            return View(new GridModel(packOverrides));
        }

        #endregion
    }
}
