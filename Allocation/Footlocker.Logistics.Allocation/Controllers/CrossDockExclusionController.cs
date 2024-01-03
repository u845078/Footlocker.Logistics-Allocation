using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Common;
using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Services;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Support,Logistics")]
    public class CrossDockExclusionController : AppController
    {
        //
        // GET: /CrossDockExclusion/
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        public ActionResult Index()
        {
            List<CrossDockExclusion> model = (from a in db.CrossDockExclusions select a).ToList();
            List<Division> divs  = currentUser.GetUserDivisions(AppName);
            model = (from a in model 
                     join b in divs 
                     on a.Division equals b.DivCode 
                     select a).ToList();
            return View(model);
        }

        [GridAction]
        public ActionResult _Index()
        {
            var userDivCodeList = currentUser.GetUserDivList(AppName);
            var xdockVMs = db.CrossDockExclusions.Include("StoreLookup").Where(cde => userDivCodeList.Contains(cde.Division))
                .Select(cde =>
                    new CrossDockExclusionViewModel() {
                        Division = cde.Division,
                        Store = cde.Store,
                        City = cde.StoreLookup.City,
                        State = cde.StoreLookup.State,
                        CreatedBy = cde.CreatedBy,
                        CreateDate = cde.CreateDate
                    });
            return View(new GridModel(xdockVMs));
        }

        public ActionResult Create()
        {
            CrossDockExclusionModel model = new CrossDockExclusionModel();
            model.Divisions = currentUser.GetUserDivisions(AppName);
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(CrossDockExclusionModel model)
        {
            try
            {
                model.Exclusion.CreateDate = DateTime.Now;
                model.Exclusion.CreatedBy = User.Identity.Name;
                db.CrossDockExclusions.Add(model.Exclusion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                model.Divisions = currentUser.GetUserDivisions(AppName);
                model.ErrorMessage = ex.Message;
                return View(model);
            }
        }

        public ActionResult Delete(string div, string store)
        {
            RDQDAO dao = new RDQDAO();
            dao.DeleteCrossdockRDQs(div, store);

            CrossDockExclusion exc = (from a in db.CrossDockExclusions where ((a.Division == div) && (a.Store == store)) select a).First();
            db.CrossDockExclusions.Remove(exc);
            db.SaveChanges();

            return RedirectToAction("Index");
        }


        public ActionResult DeleteLeaveRDQs(string div, string store)
        {
            CrossDockExclusion exc = (from a in db.CrossDockExclusions where ((a.Division == div) && (a.Store == store)) select a).First();
            db.CrossDockExclusions.Remove(exc);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

    }

    public class CrossDockExclusionViewModel
    {
        #region Initializations

        public CrossDockExclusionViewModel() { }

        #endregion

        #region Public Properties

        public string Division { get; set; }
        public string Store { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }

        #endregion
    }
}
