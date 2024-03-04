using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Services;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Common;
using System;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Driver")]
    public class DriverController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
        AllocationLibraryContext allocDB = new AllocationLibraryContext();

        public ActionResult Index(string div)
        {
            AllocationDriverDAO dao = new AllocationDriverDAO(appConfig.EuropeDivisions, appConfig.DB2PrefixDriver);
            DriverModel model = new DriverModel()
            {
                Divisions = currentUser.GetUserDivisions(AppName)
            };

            if ((div == null) && (model.Divisions.Count() > 0))
            {
                div = model.Divisions[0].DivCode;
            }
            model.CurrentDivision = div;
            model.Drivers = dao.GetAllocationDriverList(div);
            return View(model);
        }

        public ActionResult Create(string div)
        {
            DriverModel model = new DriverModel()
            {
                NewDriver = new AllocationDriver()
                {
                    Division = div
                },
                Divisions = currentUser.GetUserDivisions(AppName),
                Departments = DepartmentService.ListDepartments(div)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DriverModel model)
        {
            AllocationDriverDAO dao = new AllocationDriverDAO(appConfig.EuropeDivisions, appConfig.DB2PrefixDriver);
            dao.Save(model.NewDriver, currentUser.NetworkID, appConfig.UpdateMF);
            return RedirectToAction("Index", new { div = model.NewDriver.Division });
        }

        public ActionResult Edit(string div, string dept)
        {
            AllocationDriverDAO dao = new AllocationDriverDAO(appConfig.EuropeDivisions, appConfig.DB2PrefixDriver);
            DriverModel model = new DriverModel()
            {
                Divisions = currentUser.GetUserDivisions(AppName),
                NewDriver = dao.GetAllocationDriver(div, dept)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DriverModel model)
        {
            AllocationDriverDAO dao = new AllocationDriverDAO(appConfig.EuropeDivisions, appConfig.DB2PrefixDriver);
            dao.Save(model.NewDriver, currentUser.NetworkID, appConfig.UpdateMF);

            return RedirectToAction("Index", new { div = model.NewDriver.Division });
        }

        [CheckPermission(Roles = "IT")]
        public ActionResult Maintenance()
        {
            MaintenanceModel model = new MaintenanceModel();
            
            return View(model);
        }

        [CheckPermission(Roles = "IT")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Maintenance(MaintenanceModel model)
        {
            if (!string.IsNullOrEmpty(model.SQLCommand))
            {
                DataChangeLog changeRec = new DataChangeLog()
                {
                    CommandText = model.SQLCommand,
                    UsedDatabase = model.SelectedDatabase.ToString(),
                    LastModifiedUser = currentUser.NetworkID,
                    LastModifiedDate = DateTime.Now
                };

                allocDB.DataChanges.Add(changeRec);
                allocDB.SaveChanges();

                model.ReturnMessage = "SQL Command Stored";
                model.SQLCommand = string.Empty;
            }
            else
            {
                DataChangeLog command = allocDB.DataChanges.Where(dc => !dc.ExecutedInd).FirstOrDefault();                

                switch (command.UsedDatabase)
                {
                    case "Allocation":
                        command.ChangedRows = allocDB.ExecuteCommand(command.CommandText); 
                        break;
                    case "Footlocker_Common":
                        FootLockerCommonContext commDB = new FootLockerCommonContext();
                        command.ChangedRows = commDB.ExecuteCommand(command.CommandText);
                        break;
                }

                command.ExecutedInd = true;

                allocDB.SaveChanges();

                model.ReturnMessage = string.Format("There were {0} records affected", command.ChangedRows);
            }

            return View(model);
        }

        [CheckPermission(Roles = "IT")]
        public ActionResult Execute()
        {
            MaintenanceModel model = new MaintenanceModel();

            return View(model);
        }


        //[CheckPermission(Roles = "IT")]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Execute(MaintenanceModel model)
        //{
        //    DataChangeLog command = allocDB.DataChanges.Where(dc => !dc.ExecutedInd).FirstOrDefault();

        //    switch (command.UsedDatabase)
        //    {
        //        case "Allocation":
        //            command.ChangedRows = allocDB.ExecuteCommand(command.CommandText);
        //            break;
        //        case "Footlocker_Common":
        //            FootLockerCommonContext commDB = new FootLockerCommonContext();
        //            command.ChangedRows = commDB.ExecuteCommand(command.CommandText);
        //            break;
        //    }

        //    command.ExecutedInd = true;

        //    allocDB.SaveChanges();

        //    model.ReturnMessage = string.Format("There were {0} records affected", command.ChangedRows);

        //    return View(model);
        //}

        public ActionResult Delete(string div, string dept)
        {
            AllocationDriverDAO dao = new AllocationDriverDAO(appConfig.EuropeDivisions, appConfig.DB2PrefixDriver);
            dao.DeleteAllocationDriver(div, dept);

            return RedirectToAction("Index", new { div });
        }

        [CheckPermission(Roles = "Support")]
        public ActionResult Control()
        {
            List<ControlDate> model = db.ControlDates.ToList();
            return View(model);
        }

        [CheckPermission(Roles = "IT")]
        public ActionResult EditControlDate(int instanceID)
        {
            ControlDate model = db.ControlDates.Where(cd => cd.InstanceID == instanceID).First();
            return View(model);
        }

        [HttpPost]
        [CheckPermission(Roles = "IT")]
        [ValidateAntiForgeryToken]
        public ActionResult EditControlDate(ControlDate model)
        {
            db.Entry(model).State = System.Data.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Control");
        }
    }
}
