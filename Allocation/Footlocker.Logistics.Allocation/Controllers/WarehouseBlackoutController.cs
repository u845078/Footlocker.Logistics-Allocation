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
        readonly Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        public ActionResult Index()
        {
            Dictionary<string, string> names = new Dictionary<string, string>();
            var users = (from a in db.WarehouseBlackouts
                         select a.CreatedBy).Distinct();
            foreach (string userID in users)
            {
                names.Add(userID, GetFullUserNameFromDatabase(userID.Replace('\\', '/')));
            }

            foreach (var item in db.WarehouseBlackouts)
            {
                item.CreatedBy = names[item.CreatedBy];
            }

            return View(db.WarehouseBlackouts);
        }

        public ActionResult Create()
        {
            WarehouseBlackoutModel model = new WarehouseBlackoutModel()
            {
                WarehouseBlackout = new WarehouseBlackout(),
                Warehouses = db.DistributionCenters.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(WarehouseBlackoutModel model)
        {
            model.WarehouseBlackout.CreateDate = DateTime.Now;
            model.WarehouseBlackout.CreatedBy = currentUser.NetworkID;

            db.WarehouseBlackouts.Add(model.WarehouseBlackout);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int ID)
        {
            WarehouseBlackoutModel model = new WarehouseBlackoutModel()
            {
                WarehouseBlackout = db.WarehouseBlackouts.Where(wb => wb.ID == ID).First(),
                Warehouses = db.DistributionCenters.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(WarehouseBlackoutModel model)
        {
            model.WarehouseBlackout.CreateDate = DateTime.Now;
            model.WarehouseBlackout.CreatedBy = currentUser.NetworkID;

            db.Entry(model.WarehouseBlackout).State = System.Data.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int ID)
        {
            WarehouseBlackout obj = db.WarehouseBlackouts.Where(wb => wb.ID == ID).First();
            db.WarehouseBlackouts.Remove(obj);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
