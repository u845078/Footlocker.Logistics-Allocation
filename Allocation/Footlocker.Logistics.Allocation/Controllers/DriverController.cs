using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Driver")]
    public class DriverController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        public ActionResult Index(string div)
        {
            AllocationDriverDAO dao = new AllocationDriverDAO();
            DriverModel model = new DriverModel();
            model.Divisions = this.Divisions();
            if ((div == null) && (model.Divisions.Count() > 0))
            {
                div = model.Divisions[0].DivCode;
            }
            model.CurrentDivision = div;
            model.Drivers = dao.GetAllocationDriverList(div);
            return View(model);
        }

        //public ActionResult TestInterfaceLog()
        //{
        //    QuantumInstance i = new QuantumInstance(3);
        //    InterfaceLogDAO dao = new InterfaceLogDAO(i);
            
        //    //dao.Insert("RC3977", "TEST", "REQUESTED DISTRIBUTION QUANTITIES", 0);

        //    InterfaceFile file = new InterfaceFile(InterfaceFile.Type.RequestedDistributionQuantity);
        //    dao = new InterfaceLogDAO(i);
        //    dao.Insert(file, "QFileterRDQ", 0);

        //    return View();

        //}

        public ActionResult Create(string div)
        {
            DriverModel model = new DriverModel();
            model.NewDriver = new AllocationDriver();
            model.NewDriver.Division = div;
            model.Divisions = this.Divisions();
            model.Departments = DepartmentService.ListDepartments(div);

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(DriverModel model)
        {
            AllocationDriverDAO dao = new AllocationDriverDAO();
            dao.Save(model.NewDriver, User.Identity.Name);
            return RedirectToAction("Index", new { div = model.NewDriver.Division });
        }

        public ActionResult Edit(string div, string dept)
        {
            AllocationDriverDAO dao = new AllocationDriverDAO();
            DriverModel model = new DriverModel();
            model.Divisions = this.Divisions();
            model.NewDriver = dao.GetAllocationDriver(div, dept);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(DriverModel model)
        {
            AllocationDriverDAO dao = new AllocationDriverDAO();
            dao.Save(model.NewDriver, User.Identity.Name);

            return RedirectToAction("Index", new { div = model.NewDriver.Division });
        }

        public ActionResult Delete(string div, string dept)
        {
            AllocationDriverDAO dao = new AllocationDriverDAO();
            dao.DeleteAllocationDriver(div, dept);

            return RedirectToAction("Index", new { div = div });
        }

        [CheckPermission(Roles = "Support")]
        public ActionResult Control()
        {
            List<ControlDate> model = (from a in db.ControlDates select a).ToList();
            return View(model);
        }

        [CheckPermission(Roles = "IT")]
        public ActionResult EditControlDate(int instanceID)
        {
            ControlDate model = (from a in db.ControlDates where a.InstanceID == instanceID select a).First();
            return View(model);
        }

        [HttpPost]
        [CheckPermission(Roles = "IT")]
        public ActionResult EditControlDate(ControlDate model)
        {
            db.Entry(model).State = System.Data.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Control");
        }

    }
}
