using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Support,Logistics")]
    public class WarehouseBlackoutController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        public ActionResult Index()
        {
            return View(db.WarehouseBlackouts);
        }

        public ActionResult Create()
        {
            WarehouseBlackoutModel model = new WarehouseBlackoutModel();
            model.WarehouseBlackout = new WarehouseBlackout();
            model.Warehouses = db.DistributionCenters.ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(WarehouseBlackoutModel model)
        {

            model.WarehouseBlackout.CreateDate = DateTime.Now;
            model.WarehouseBlackout.CreatedBy = User.Identity.Name;

            db.WarehouseBlackouts.Add(model.WarehouseBlackout);
            db.SaveChanges();
            return RedirectToAction("Index");

        }

        public ActionResult Edit(int ID)
        {
            WarehouseBlackoutModel model = new WarehouseBlackoutModel();
            model.WarehouseBlackout = (from a in db.WarehouseBlackouts where a.ID == ID select a).First();
            model.Warehouses = db.DistributionCenters.ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(WarehouseBlackoutModel model)
        {

            model.WarehouseBlackout.CreateDate = DateTime.Now;
            model.WarehouseBlackout.CreatedBy = User.Identity.Name;

            db.Entry(model.WarehouseBlackout).State = System.Data.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");

        }



        public ActionResult Delete(int ID)
        {
            WarehouseBlackout obj = (from a in db.WarehouseBlackouts where a.ID == ID select a).First();
            db.WarehouseBlackouts.Remove(obj);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
