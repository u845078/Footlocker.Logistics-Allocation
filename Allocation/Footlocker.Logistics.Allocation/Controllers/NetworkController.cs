using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Common;
using System.IO;
using Aspose.Excel;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Support,Logistics,Advanced Merchandiser Processes")]
    public class NetworkController : AppController
    {
        //
        // GET: /NetworkLeadTime/
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();

        public ActionResult Index(int instanceID = -1)
        {
            if (instanceID > 0)
            {
                return RedirectToAction("ViewZones", new { instanceID = instanceID });
            }

            NetworkLeadTimeModel model = new NetworkLeadTimeModel();
            model.AvailableInstances = new SelectList(db.Instances.ToList(), "ID", "Name", model.InstanceID);

            model.InstanceID = instanceID;

            return View(model);
        }

        public ActionResult ViewZones(int instanceID)
        {
            NetworkZoneModel model = new NetworkZoneModel();
            ViewData["instanceid"] = instanceID;
            model.NetworkZones = (from NetworkZone nz in db.NetworkZones
                                  join t in db.NetworkZoneStores 
                                    on nz.ID equals t.ZoneID
                                  join b in db.InstanceDivisions 
                                    on t.Division equals b.Division
                                  where b.InstanceID == instanceID
                                  select nz).Distinct().ToList();

            model.Instance = db.Instances.Where(i => i.ID == instanceID).First();

            model.AvailableZones = (from NetworkZone nz in db.NetworkZones 
                                    join t in db.NetworkZoneStores 
                                      on nz.ID equals t.ZoneID
                                    join b in db.InstanceDivisions 
                                      on t.Division equals b.Division
                                    where b.InstanceID == model.Instance.ID
                                    select nz).ToList();
            
            return View(model);
        }

        public ActionResult DownnloadZones(int instanceID)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            Worksheet mySheet = excelDocument.Worksheets[0];

            List<NetworkZone> list = (from a in db.NetworkZones 
                                      join b in db.NetworkZoneStores 
                                        on a.ID equals b.ZoneID 
                                      join c in db.InstanceDivisions 
                                        on b.Division equals c.Division 
                                      where c.InstanceID == instanceID 
                                      select a).Distinct().ToList();
            List<NetworkZoneStore> storeList;

            int row = 1;
            mySheet.Cells[0, 0].PutValue("Zone");
            mySheet.Cells[0, 1].PutValue("Division");
            mySheet.Cells[0, 2].PutValue("Store");

            foreach (NetworkZone z in list)
            {
                storeList = (from a in db.NetworkZoneStores 
                             join b in db.InstanceDivisions 
                               on a.Division equals b.Division 
                             where a.ZoneID == z.ID && b.InstanceID == instanceID
                             select a).ToList();

                foreach (NetworkZoneStore s in storeList)
                {
                    mySheet.Cells[row, 0].PutValue(z.Name);
                    mySheet.Cells[row, 1].PutValue(s.Division);
                    mySheet.Cells[row, 2].PutValue(s.Store);

                    row++;
                }
            }

            excelDocument.Save("ZoneStores.xls", SaveType.OpenInExcel, FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();
        }

        [HttpPost]
        public ActionResult CreateZone(NetworkZoneModel model)
        {
            NetworkZone nz = new NetworkZone();
            nz.LeadTimeID = model.LeadTimeID;
            nz.Name = model.NewZone;
            nz.CreateDate = DateTime.Now;
            nz.CreatedBy = currentUser.NetworkID;

            db.NetworkZones.Add(nz);
            db.SaveChanges();

            return RedirectToAction("ViewZones", new { id=model.LeadTimeID });
        }

        public ActionResult MoveZone(int ID, int leadTimeID)
        {
            NetworkZone nz = (from a in db.NetworkZones where a.ID == ID select a).First();
            nz.LeadTimeID = leadTimeID;
            nz.CreateDate = DateTime.Now;
            nz.CreatedBy = currentUser.NetworkID;

            db.SaveChanges();

            return RedirectToAction("ViewZones", new { id = leadTimeID });
        }

        public ActionResult DeleteZone(int ID)
        {
            NetworkZone lt = db.NetworkZones.Where(nz => nz.ID == ID).FirstOrDefault();

            db.NetworkZones.Remove(lt);

            var stores = db.NetworkZoneStores.Where(nzs => nzs.ZoneID == ID).ToList();

            foreach (NetworkZoneStore s in stores)
            {
                db.NetworkZoneStores.Remove(s);
            }

            db.SaveChanges();

            return RedirectToAction("ViewZones", new { id = lt.LeadTimeID });
        }

        public ActionResult ViewStores(int ID, int instanceid, string route, string dc)
        {
            NetworkZoneStoreModel model = new NetworkZoneStoreModel();
            model.Divisions = DivisionService.ListDivisions();

            if (route != null)
            {
                ViewData["Route"] = route;
                ViewData["dc"] = dc;
            }
            model.Stores = (from NetworkZoneStore nz in db.NetworkZoneStores 
                            join b in db.StoreLookups 
                              on new { nz.Division, nz.Store } equals new { b.Division, b.Store } 
                            where nz.ZoneID == ID 
                            orderby b.State, b.City,b.Mall 
                            select b).ToList();

            model.Zone = (from NetworkZone nz in db.NetworkZones where nz.ID == ID select nz).First();
            model.ZoneID = model.Zone.ID;
            model.Instance = (from Instance i in db.Instances where i.ID == instanceid select i).First();

            return View(model);
        }

        [HttpPost]
        public ActionResult AddStore(NetworkZoneStoreModel model)
        {
            var existing = (from a in db.NetworkZoneStores where ((a.Division == model.NewDivision) && (a.Store == model.NewStore)) select a);
            if (existing.Count() > 0)
            {
                int zone = existing.First().ZoneID;
                return RedirectToAction("ViewStores", new { id = model.ZoneID, addDivision= model.NewDivision, addStore=model.NewStore, oldZone = zone });
            }
            else
            {
                NetworkZoneStore nz = new NetworkZoneStore();
                nz.Division = model.NewDivision;
                nz.Store = model.NewStore;
                nz.ZoneID = model.ZoneID;

                db.NetworkZoneStores.Add(nz);
                db.SaveChanges();

                return RedirectToAction("ViewStores", new { id = model.ZoneID });
            }
        }

        public ActionResult MoveStore(string div, string store, int zoneID)
        {
            NetworkZoneStoreModel model = new NetworkZoneStoreModel();
            model.NewDivision = div;
            model.NewStore = store;
            model.ZoneID = zoneID;

            NetworkZoneStore nz = (from a in db.NetworkZoneStores where ((a.Division == div)&&(a.Store==store)) select a).First();
            nz.ZoneID = model.ZoneID;
            db.SaveChanges();

            return RedirectToAction("ViewStores", new { id = model.ZoneID });
        }

        public ActionResult DeleteStore(string div, string store)
        {
            NetworkZoneStore nz = (from a in db.NetworkZoneStores where ((a.Division == div) && (a.Store == store)) select a).First();

            db.NetworkZoneStores.Remove(nz);
            db.SaveChanges();

            return RedirectToAction("ViewStores", new { id = nz.ZoneID });
        }
    }
}
