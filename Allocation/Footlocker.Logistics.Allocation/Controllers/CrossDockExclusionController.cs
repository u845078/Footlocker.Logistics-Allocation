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
            List<CrossDockExclusion> model = db.CrossDockExclusions.ToList();
            List<Division> divs  = currentUser.GetUserDivisions();
            model = (from a in model 
                     join b in divs 
                     on a.Division equals b.DivCode 
                     select a).ToList();
            return View(model);
        }

        [GridAction]
        public ActionResult _Index()
        {
            List<string> userDivCodeList = currentUser.GetUserDivList();
            List<CrossDockExclusionViewModel> xdockVMs = db.CrossDockExclusions.Include("StoreLookup").Where(cde => userDivCodeList.Contains(cde.Division))
                                                                                                      .Select(cde =>
                                                                                                            new CrossDockExclusionViewModel() {
                                                                                                                Division = cde.Division,
                                                                                                                Store = cde.Store,
                                                                                                                City = cde.StoreLookup.City,
                                                                                                                State = cde.StoreLookup.State,
                                                                                                                CreatedBy = cde.CreatedBy,
                                                                                                                CreateDate = cde.CreateDate
                                                                                                            }).ToList();
            List<string> uniqueNames = (from a in xdockVMs
                                        where !string.IsNullOrEmpty(a.CreatedBy)
                                        select a.CreatedBy).Distinct().ToList();

            Dictionary<string, string> fullNamePairs = LoadUserNames(uniqueNames);

            foreach (var item in fullNamePairs)
            {
                xdockVMs.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
            }

            return View(new GridModel(xdockVMs));
        }

        public ActionResult Create()
        {
            CrossDockExclusionModel model = new CrossDockExclusionModel()
            {
                Divisions = currentUser.GetUserDivisions()
            };
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CrossDockExclusionModel model)
        {
            try
            {
                model.Exclusion.CreateDate = DateTime.Now;
                model.Exclusion.CreatedBy = currentUser.NetworkID;
                db.CrossDockExclusions.Add(model.Exclusion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                model.Divisions = currentUser.GetUserDivisions();
                model.ErrorMessage = ex.Message;
                return View(model);
            }
        }

        public ActionResult Delete(string div, string store)
        {
            RDQDAO dao = new RDQDAO();
            dao.DeleteCrossdockRDQs(div, store);

            CrossDockExclusion exc = db.CrossDockExclusions.Where(cde => cde.Division == div && cde.Store == store).First();
            db.CrossDockExclusions.Remove(exc);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult DeleteLeaveRDQs(string div, string store)
        {
            CrossDockExclusion exc = db.CrossDockExclusions.Where(cde => cde.Division == div && cde.Store == store).First();
            db.CrossDockExclusions.Remove(exc);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
