using System;
using System.Linq;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Common;
using Aspose.Cells;
using Footlocker.Logistics.Allocation.Spreadsheets;
using System.Collections.Generic;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Support,Logistics,Advanced Merchandiser Processes")]
    public class NetworkController : AppController
    {
        //
        // GET: /NetworkLeadTime/
        DAO.AllocationContext db = new DAO.AllocationContext();

        public ActionResult Index(int instanceID = -1)
        {
            if (instanceID > 0)            
                return RedirectToAction("ViewZones", new { instanceID });            

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

            List<string> uniqueNames = (from a in model.NetworkZones
                                        where !string.IsNullOrEmpty(a.CreatedBy)
                                        select a.CreatedBy).Distinct().ToList();

            Dictionary<string, string> fullNamePairs = LoadUserNames(uniqueNames);

            foreach (var item in fullNamePairs)
            {
                model.NetworkZones.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
            }

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

        public ActionResult DownloadZones(int instanceID)
        {
            ZoneStoreExport zoneStoreExport = new ZoneStoreExport(appConfig);
            zoneStoreExport.WriteData(instanceID);

            zoneStoreExport.excelDocument.Save(System.Web.HttpContext.Current.Response, "ZoneStores.xlsx", ContentDisposition.Attachment, zoneStoreExport.SaveOptions);
            return View();
        }

        public ActionResult MoveZone(int ID, int leadTimeID)
        {
            NetworkZone nz = db.NetworkZones.Where(n => n.ID == ID).First();
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
            NetworkZoneStoreModel model = new NetworkZoneStoreModel()
            {
                Divisions = DivisionService.ListDivisions()
            };            

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

            model.Zone = db.NetworkZones.Where(nz => nz.ID == ID).First();
            model.ZoneID = model.Zone.ID;
            model.Instance = db.Instances.Where(i => i.ID == instanceid).First();

            return View(model);
        }

        public ActionResult MoveStore(string div, string store, int zoneID)
        {
            NetworkZoneStoreModel model = new NetworkZoneStoreModel() 
            {
                NewDivision = div,
                NewStore = store,
                ZoneID = zoneID
            };

            NetworkZoneStore nz = db.NetworkZoneStores.Where(nzs => nzs.Division == div && nzs.Store == store).First();
            nz.ZoneID = model.ZoneID;
            db.SaveChanges();

            return RedirectToAction("ViewStores", new { id = model.ZoneID });
        }

        public ActionResult DeleteStore(string div, string store)
        {
            NetworkZoneStore nz = db.NetworkZoneStores.Where(nzs => nzs.Division == div && nzs.Store == store).First();

            db.NetworkZoneStores.Remove(nz);
            db.SaveChanges();

            return RedirectToAction("ViewStores", new { id = nz.ZoneID });
        }
    }
}