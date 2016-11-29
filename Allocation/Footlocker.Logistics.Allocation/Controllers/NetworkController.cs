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
                //List<NetworkLeadTime> list = (from a in db.NetworkLeadTimes where a.InstanceID == instanceID select a).ToList();
                //if (list.Count() == 1)
                //{
                    return RedirectToAction("ViewZones", new { instanceID = instanceID });
                //}
            }
            NetworkLeadTimeModel model = new NetworkLeadTimeModel();
            model.AvailableInstances = (from Instance i in db.Instances select i).ToList();
            Instance select = new Instance();
            select.Name = "Select";
            select.ID = -1;
            model.AvailableInstances.Insert(0, select);
            if (instanceID == -1)
            {
                instanceID = model.AvailableInstances.First().ID;
            }
            model.InstanceID = instanceID;
            //model.NetworkLeadTimes = (from a in db.NetworkLeadTimes where a.InstanceID == instanceID select a).ToList();
            return View(model);
        }

        //[HttpPost]
        //public ActionResult CreateLeadTime(NetworkLeadTimeModel model)
        //{
        //    NetworkLeadTime lt = new NetworkLeadTime();
        //    lt.InstanceID = model.InstanceID;
        //    lt.Name = model.NewLeadTime;
        //    lt.CreateDate = DateTime.Now;
        //    lt.CreatedBy = User.Identity.Name;
            
        //    db.NetworkLeadTimes.Add(lt);
        //    db.SaveChanges();
            
        //    return RedirectToAction("Index", new { model.InstanceID });
        //}

        //public ActionResult DeleteLeadTime(int ID)
        //{
        //    NetworkLeadTime lt = (from NetworkLeadTime a in db.NetworkLeadTimes where a.ID == ID select a).First();

        //    db.NetworkLeadTimes.Remove(lt);
        //    db.SaveChanges();

        //    return RedirectToAction("Index", new { lt.InstanceID });
        //}


        public ActionResult ViewZones(int instanceID)
        {
            NetworkZoneModel model = new NetworkZoneModel();
            ViewData["instanceid"] = instanceID;
//            model.NetworkZones = (from NetworkZone nz in db.NetworkZones where nz.LeadTimeID == ID orderby nz.Name select nz).ToList();
            model.NetworkZones = (from NetworkZone nz in db.NetworkZones
                                    join t in db.NetworkZoneStores on nz.ID equals t.ZoneID
                                    join b in db.InstanceDivisions on t.Division equals b.Division
                                    where (
                                    (b.InstanceID == instanceID))
                                    select nz).Distinct().ToList();

            model.Instance = (from Instance i in db.Instances where i.ID == instanceID select i).First();

            model.AvailableZones = (from NetworkZone nz in db.NetworkZones 
                                    join t in db.NetworkZoneStores on nz.ID equals t.ZoneID
                                    join b in db.InstanceDivisions on t.Division equals b.Division
                                    where (
                                    (b.InstanceID == model.Instance.ID)) 
                                    select nz).ToList();
            
            //model.AvailableZones = (from NetworkZone nz in db.NetworkZones join t in db.NetworkLeadTimes on nz.LeadTimeID equals t.ID where ((nz.LeadTimeID != ID)&&(t.InstanceID == model.Instance.ID)) select nz).ToList();
            //model.LeadTime = (from NetworkLeadTime t in db.NetworkLeadTimes where t.ID == ID select t).First();
            //model.LeadTimeID = model.LeadTime.ID;

            return View(model);
        }

        public ActionResult DownnloadZones(int instanceID)
        {

            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            Worksheet mySheet = excelDocument.Worksheets[0];

            List<NetworkZone> list = (from a in db.NetworkZones join b in db.NetworkZoneStores on a.ID equals b.ZoneID join c in db.InstanceDivisions on b.Division equals c.Division where c.InstanceID == instanceID select a).Distinct().ToList();
            List<NetworkZoneStore> storeList;

            int row = 1;
            mySheet.Cells[0, 0].PutValue("Zone");
            mySheet.Cells[0, 1].PutValue("Division");
            mySheet.Cells[0, 2].PutValue("Store");
            //int col = 3;
            //List<Int32> DCs = new List<int>();
            //foreach (DistributionCenter dc in (from a in db.DistributionCenters where a.InstanceID == instanceID select a))
            //{
            //    DCs.Add(dc.ID);
            //    mySheet.Cells[0, col].PutValue(dc.Name);
            //    col++;
            //}

            foreach (NetworkZone z in list)
            {
                storeList = (from a in db.NetworkZoneStores join b in db.InstanceDivisions on a.Division equals b.Division where ((a.ZoneID == z.ID)&&(b.InstanceID == instanceID)) select a).ToList();

                foreach (NetworkZoneStore s in storeList)
                {
                    mySheet.Cells[row, 0].PutValue(z.Name);
                    mySheet.Cells[row, 1].PutValue(s.Division);
                    mySheet.Cells[row, 2].PutValue(s.Store);

                    //foreach (StoreLeadTime slt in (from a in db.StoreLeadTimes where ((a.Store == s.Store) && (a.Division == s.Division)) select a))
                    //{
                    //    col = 3 + DCs.IndexOf(slt.DCID);
                    //    mySheet.Cells[row, col].PutValue(slt.LeadTime);
                    //}

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
            nz.CreatedBy = User.Identity.Name;

            db.NetworkZones.Add(nz);
            db.SaveChanges();

            return RedirectToAction("ViewZones", new { id=model.LeadTimeID });
        }

        public ActionResult MoveZone(int ID, int leadTimeID)
        {
            NetworkZone nz = (from a in db.NetworkZones where a.ID == ID select a).First();
            nz.LeadTimeID = leadTimeID;
            nz.CreateDate = DateTime.Now;
            nz.CreatedBy = User.Identity.Name;

            db.SaveChanges();

            return RedirectToAction("ViewZones", new { id = leadTimeID });
        }

        public ActionResult DeleteZone(int ID)
        {
            NetworkZone lt = (from NetworkZone a in db.NetworkZones where a.ID == ID select a).First();

            db.NetworkZones.Remove(lt);
            db.SaveChanges();

            var stores = (from NetworkZoneStore a in db.NetworkZoneStores where a.ZoneID == ID select a);

            foreach (NetworkZoneStore s in stores)
            {
                db.NetworkZoneStores.Remove(s);
            }
            db.SaveChanges();

            return RedirectToAction("ViewZones", new { id=lt.LeadTimeID });
        }

        public ActionResult ViewStores(int ID, int instanceid, string route, string dc)
        {
            NetworkZoneStoreModel model = new NetworkZoneStoreModel();
            model.Divisions = DivisionService.ListDivisions();

            //if ((addDivision != null)&&(addDivision != ""))
            //{
            //    model.NewStore = addStore;
            //    model.NewDivision = addDivision;
            //    NetworkZone n = (from a in db.NetworkZones where a.ID == oldZone select a).First();
            //    model.Message = addDivision + "-" + addStore + " already exists in Zone \"" + n.Name + "\"(" + n.ID + ")";
            //}
            if (route != null)
            {
                ViewData["Route"] = route;
                ViewData["dc"] = dc;
            }
            model.Stores = (from NetworkZoneStore nz in db.NetworkZoneStores join b in db.StoreLookups on new { nz.Division, nz.Store } equals new { b.Division, b.Store } where nz.ZoneID == ID orderby b.State, b.City,b.Mall select b).ToList();
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
