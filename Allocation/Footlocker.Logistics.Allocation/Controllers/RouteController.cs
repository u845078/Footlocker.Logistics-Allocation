using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Telerik.Web.Mvc;
using Footlocker.Logistics.Allocation.Services;
using Aspose.Excel;
using Aspose.Cells;
using System.IO;
using System.Data;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Support,Logistics,Advanced Merchandiser Processes")]
    public class RouteController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
        //
        // GET: /Route/

        public ActionResult Index(int instanceID = 1)
        {
            RouteModel model = new RouteModel
            {
                InstanceID = instanceID,
                AvailableInstances = (from a in db.Instances
                                      select a).ToList(),
                Routes = (from a in db.Routes
                          where a.InstanceID == instanceID
                          select a).ToList()
            };
            return View(model);
        }

        public ActionResult DownnloadRoutes(int instanceID)
        {

            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            Aspose.Excel.Worksheet mySheet = excelDocument.Worksheets[0];

            List<Route> list = (from a in db.Routes where a.InstanceID == instanceID select a).ToList();

            int row = 1;
            mySheet.Cells[0, 0].PutValue("Route");
            mySheet.Cells[0, 1].PutValue("Perspective");
            mySheet.Cells[0, 2].PutValue("Pass");
            mySheet.Cells[0, 3].PutValue("DC");
            mySheet.Cells[0, 4].PutValue("Zone");
            mySheet.Cells[0, 5].PutValue("Days");
            foreach (Route r in list)
            {
                var rdList = (from a in db.RouteDetails
                              join b in db.NetworkZones on a.ZoneID equals b.ID
                              join c in db.DistributionCenters on a.DCID equals c.ID
                              where a.RouteID == r.ID
                              select new { det = a, zone = b, dc = c }).ToList();

                foreach (var d in rdList)
                {
                    mySheet.Cells[row, 0].PutValue(r.Name);
                    mySheet.Cells[row, 1].PutValue(r.Perspective);
                    mySheet.Cells[row, 2].PutValue(r.Pass);
                    mySheet.Cells[row, 3].PutValue(d.dc.Name);
                    mySheet.Cells[row, 4].PutValue(d.zone.Name);
                    mySheet.Cells[row, 5].PutValue(d.det.Days);
                    row++;
                }
            }

            excelDocument.Save("Routes.xls", Aspose.Excel.SaveType.OpenInExcel, Aspose.Excel.FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();

        }


        public ActionResult Create(int instanceID)
        {
            Route r = new Route();
            r.InstanceID = instanceID;
            return View(r);
        }

        [HttpPost]
        public ActionResult Create(Route model)
        {
            model.CreatedBy = User.Identity.Name;
            model.CreateDate = DateTime.Now;

            db.Routes.Add(model);
            db.SaveChanges();
            return RedirectToAction("Index", new { instanceID = model.InstanceID });
        }

        public ActionResult Delete(int ID)
        {
            List<RouteDistributionCenter> dcs = (from a in db.RouteDistributionCenters where a.RouteID == ID select a).ToList();
            foreach (RouteDistributionCenter dc in dcs)
            {
                db.RouteDistributionCenters.Remove(dc);
                db.SaveChanges();
            }
            Route det = (from a in db.Routes where (a.ID == ID) select a).First();

            db.Routes.Remove(det);
            db.SaveChanges();

            return RedirectToAction("Index", new { instanceID = det.InstanceID });
        }

        public ActionResult EditRoute(int ID)
        {
            EditRouteModel model = new EditRouteModel();
            model.Route = (from a in db.Routes where a.ID == ID select a).First();
            List<DistributionCenter> temp = (from a in db.DistributionCenters join b in db.InstanceDistributionCenters on a.ID equals b.DCID where b.InstanceID == model.Route.InstanceID select a).ToList();

            DistributionCenterModel dcmodel;
            model.DCs = new List<DistributionCenterModel>();
            foreach (DistributionCenter dc in temp)
            {
                dcmodel = new DistributionCenterModel();
                dcmodel.DC = dc;
                dcmodel.Zones = (from a in db.RouteDetails where ((a.DCID == dc.ID) && (a.RouteID == ID)) select a).Count();
                model.DCs.Add(dcmodel);
            }
            return View(model);
        }

        public ActionResult EditRouteZones(int dcID, int routeID)
        {
            EditRouteZonesModel model = new EditRouteZonesModel();
            model.Route = (from a in db.Routes where a.ID == routeID select a).First();
            model.DC = (from a in db.DistributionCenters where a.ID == dcID select a).First(); 
            model.RouteDetails = (from b in db.RouteDetails where ((b.RouteID == routeID) && (b.DCID == dcID)) select b).ToList();
            model.CurrentZones = (from b in db.RouteDetails join a in db.NetworkZones on b.ZoneID equals a.ID where ((b.RouteID == routeID) && (b.DCID == dcID)) select a).ToList();
            model.AvailableZones = (from a in db.NetworkZones join lt in db.NetworkLeadTimes on a.LeadTimeID equals lt.ID where ((lt.InstanceID == model.Route.InstanceID)&&(!(from b in db.RouteDetails where ((b.ZoneID == a.ID)&&(b.RouteID == routeID) && (b.DCID == dcID)) select b).Any())) select a).ToList();
                                                                  
            return View(model);
        }

        public ActionResult AddRouteZone(int dcID, int routeID, int zoneID, int days)
        {
            var existing = (from a in db.RouteDetails where ((a.RouteID==routeID)&&(a.ZoneID==zoneID)) select a);
            if (existing.Count() > 0)
            {
                db.RouteDetails.Remove(existing.First());
                db.SaveChanges();
            }
            RouteDetail det = new RouteDetail();
            det.DCID = dcID;
            det.RouteID = routeID;
            det.ZoneID = zoneID;
            det.Days = days;

            db.RouteDetails.Add(det);
            db.SaveChanges();

            return RedirectToAction("EditRouteZones", new { dcID=dcID, routeID=routeID});
        }

        public ActionResult DeleteRouteZone(int dcID, int routeID, int zoneID)
        {
            RouteDetail det = (from a in db.RouteDetails where ((a.DCID == dcID)&&(a.RouteID==routeID)&&(a.ZoneID==zoneID)) select a).First();

            db.RouteDetails.Remove(det);
            db.SaveChanges();

            return RedirectToAction("EditRouteZones", new { dcID = dcID, routeID = routeID });
        }

        public ActionResult DCIndex(string instanceID)
        {
            DCList model = new DCList();
            model.AvailableInstances = (from a in db.Instances
                                        select a).ToList();
            
            model.InstanceID = Convert.ToInt32(instanceID);
            if (model.InstanceID == 0)
            {
                model.InstanceID = 1;
            }
            model.DCs = (from a in db.DistributionCenters
                         join b in db.InstanceDistributionCenters 
                           on a.ID equals b.DCID
                         where b.InstanceID == model.InstanceID
                         select a).ToList();

            return View(model);
        }

        public ActionResult EditDC(int id)
        {
            DistributionCenterModel model = new DistributionCenterModel();

            model.DC = (from a in db.DistributionCenters
                        where a.ID == id
                        select a).First();

            model.WarehouseAllocationTypes = (from w in db.WarehouseAllocationTypes
                                              select w).ToList();

            model.AvailableInstances = (from i in db.Instances
                                        select i).ToList();

            model.SelectedInstances = new List<CheckBoxModel>();
            bool instChecked;
            foreach (var inst in model.AvailableInstances)
            {
                instChecked = db.InstanceDistributionCenters.Any(i => i.DCID == id && i.InstanceID == inst.ID);
                model.SelectedInstances.Add(new CheckBoxModel { ID = inst.ID, Desc = inst.Name, Checked = instChecked });
            }
            return View(model);          
        }

        [HttpPost]
        public ActionResult EditDC(DistributionCenterModel model)
        {
            model.DC.LastModifiedDate = DateTime.Now;
            model.DC.LastModifiedUser = User.Identity.Name;
            db.Entry(model.DC).State = System.Data.EntityState.Modified;

            foreach (var selInst in model.SelectedInstances)
            {
                bool alreadyThere;
                alreadyThere = db.InstanceDistributionCenters.Any(i => i.DCID == model.DC.ID && 
                                                                       i.InstanceID == selInst.ID);
                if (selInst.Checked && !alreadyThere)
                {
                    db.InstanceDistributionCenters.Add(new InstanceDistributionCenter
                    {
                        DCID = model.DC.ID,
                        InstanceID = selInst.ID
                    });
                }

                if (!selInst.Checked && alreadyThere)
                {
                    db.InstanceDistributionCenters.Remove((from id in db.InstanceDistributionCenters
                                                          where id.DCID == model.DC.ID && 
                                                                id.InstanceID == selInst.ID
                                                          select id).FirstOrDefault());
                }
            }

            db.SaveChanges();
            return RedirectToAction("DCIndex", new { instanceID = model.DC.InstanceID });
        }


        public ActionResult CreateDC(int instanceID)
        {
            DistributionCenterModel model = new DistributionCenterModel();
            model.DC = new DistributionCenter();
            model.DC.InstanceID = instanceID;
            model.WarehouseAllocationTypes = (from w in db.WarehouseAllocationTypes
                                              select w).ToList();

            model.AvailableInstances = (from i in db.Instances
                                        select i).ToList();

            model.SelectedInstances = new List<CheckBoxModel>();
            foreach (var inst in model.AvailableInstances)
            {
                model.SelectedInstances.Add(new CheckBoxModel { ID = inst.ID, Desc = inst.Name, Checked = false });
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateDC(DistributionCenterModel model)
        {
            model.DC.CreateDate = DateTime.Now;
            model.DC.CreatedBy = User.Identity.Name;
            model.DC.LastModifiedDate = DateTime.Now;
            model.DC.LastModifiedUser = User.Identity.Name;

            db.DistributionCenters.Add(model.DC);

            db.SaveChanges();

            foreach (var selInst in model.SelectedInstances)
            {
                if (selInst.Checked)
                {
                    db.InstanceDistributionCenters.Add(new InstanceDistributionCenter
                    {
                        DCID = model.DC.ID,
                        InstanceID = selInst.ID
                    });
                }
            }

            db.SaveChanges();
            return RedirectToAction("DCIndex", new { instanceID = model.DC.InstanceID });
        }


        public ActionResult RouteDC(int routeID)
        {
            RouteDCModel model = new RouteDCModel();
            model.Route = (from a in db.Routes where a.ID == routeID select a).First();
            model.AssignedDCs = (from a in db.DistributionCenters join b in db.RouteDistributionCenters on a.ID equals b.DCID where b.RouteID == routeID select a).ToList();
            model.RemainingDCs = (from a in db.DistributionCenters select a).ToList();

            return View(model);
        }

        public ActionResult AddDCToRoute(int routeID, int DCID)
        {
            if ((from a in db.RouteDistributionCenters where ((a.RouteID == routeID) && (a.DCID == DCID)) select a).Count() == 0)
            {
                RouteDistributionCenter rdc = new RouteDistributionCenter();
                rdc.RouteID = routeID;
                rdc.DCID = DCID;
                rdc.CreatedBy = User.Identity.Name;
                rdc.CreateDate = DateTime.Now;

                db.RouteDistributionCenters.Add(rdc);
                db.SaveChanges();
            }

            return RedirectToAction("RouteDC", new { routeID = routeID });
        }

        public ActionResult DeleteDCFromRoute(int routeID, int DCID)
        {
            var query = (from a in db.RouteDistributionCenters where ((a.RouteID == routeID) && (a.DCID == DCID)) select a);
            if (query.Count() > 0)
            {
                RouteDistributionCenter rdc = query.First();

                db.RouteDistributionCenters.Remove(rdc);
                db.SaveChanges();
            }

            return RedirectToAction("RouteDC", new { routeID = routeID });
        }

        public ActionResult StoreLeadTimes(string div)
        {
            StoreLeadTimeModel model = new StoreLeadTimeModel()
            {
                Divisions = Divisions()
            };

            var permissions = WebSecurityService.ListUserRoles(UserName, "Allocation");
            model.CanUploadData = permissions.Contains("IT");

            if ((div == null) && (model.Divisions.Count > 0))
            {
                div = model.Divisions[0].DivCode;
            }
            model.Division = div;
            model.StoreZones = new List<StoreZoneModel>();

            var query1 = (from a in db.vValidStores
                          join b in db.NetworkZoneStores 
                            on new { a.Division, a.Store } equals new { b.Division, b.Store } into gj
                          from subset in gj.DefaultIfEmpty()
                          where a.Division == div
                          select new { a.Division, a.Store, a.State, a.Mall, a.City, subset.ZoneID });

            var query2 = (from c in query1
                          join d in db.NetworkZones 
                             on c.ZoneID equals d.ID into g2
                          from subset2 in g2.DefaultIfEmpty()
                          orderby subset2.Name
                          select new { c.Division, c.Store, c.State, c.Mall, c.City,
                              ZoneName = (subset2 == null ? "<unassigned>" : subset2.Name) });

            var query3 = (from e in query2
                          join f in db.MinihubStores
                            on new { e.Division, e.Store } equals new { f.Division, f.Store } into oj
                          from f in oj.DefaultIfEmpty()
                          select new { e.Division, e.Store, e.State, e.Mall, e.City, e.ZoneName,
                                       MinihubStore = (f == null ? false : true) });

            foreach (var item in query3.ToList())
            {
                StoreZoneModel m = new StoreZoneModel()
                {
                    Division = item.Division,
                    Store = item.Store,
                    State = item.State,
                    Mall = item.Mall,
                    City = item.City,
                    ZoneName = item.ZoneName,
                    IsMinihubStore = item.MinihubStore
                };

                Boolean ecommwarehouse = false;

                if ((m.Store == "00900") && (m.Division == "31"))
                {
                    //alshaya
                    ecommwarehouse = true;
                }
                if ((m.Store == "00800") && (m.Division == "31"))
                {
                    //ecomm all countries
                    ecommwarehouse = true;
                }
                if (!ecommwarehouse)
                {
                    model.StoreZones.Add(m);
                }
            }

            return View(model);
        }


        public ActionResult DownnloadStoreLeadTimes(string div)
        {
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            Excel excelDocument = new Excel();
            Aspose.Excel.Worksheet mySheet = excelDocument.Worksheets[0];
            int instanceID = (from a in db.InstanceDivisions where a.Division == div select a.InstanceID).FirstOrDefault();

            List<NetworkZone> list = (from a in db.NetworkZones join b in db.NetworkZoneStores on a.ID equals b.ZoneID join c in db.InstanceDivisions on b.Division equals c.Division where c.InstanceID == instanceID select a).Distinct().ToList();
            List<NetworkZoneStore> storeList;

            int row = 1;
            mySheet.Cells[0, 0].PutValue("Zone");
            mySheet.Cells[0, 1].PutValue("Division");
            mySheet.Cells[0, 2].PutValue("Store");
            int col = 3;
            List<Int32> DCs = new List<int>();
            List<DistributionCenter> dcList = (from a in db.DistributionCenters select a).ToList();
            foreach (DistributionCenter dc in dcList)
            {
                DCs.Add(dc.ID);
                mySheet.Cells[0, col].PutValue(dc.Name);
                col++;
            }
            int rankcol = col-1;
            for (int i = 1; i <= 5; i++)
            {
                mySheet.Cells[0, col].PutValue("Rank " + i);
                col++;
            }

            foreach (NetworkZone z in list)
            {
                storeList = (from a in db.NetworkZoneStores where a.ZoneID == z.ID select a).ToList();

                foreach (NetworkZoneStore s in storeList)
                {
                    if (s.Division == div)
                    {
                        mySheet.Cells[row, 0].PutValue(z.Name);
                        mySheet.Cells[row, 1].PutValue(s.Division);
                        mySheet.Cells[row, 2].PutValue(s.Store);

                        foreach (StoreLeadTime slt in (from a in db.StoreLeadTimes where ((a.Store == s.Store) && (a.Division == s.Division)) select a))
                        {
                            col = 3 + DCs.IndexOf(slt.DCID);
                            mySheet.Cells[row, col].PutValue(slt.LeadTime);
                            if ((slt.Rank <= 5)&&(slt.Active))
                            {
                                mySheet.Cells[row, rankcol + slt.Rank].PutValue(
                                    (from a in dcList where a.ID == slt.DCID select a.Name).FirstOrDefault()
                                    );
                            }
                        }

                        row++;
                    }
                }
            }

            excelDocument.Save("StoreLeadTimes.xls", Aspose.Excel.SaveType.OpenInExcel, Aspose.Excel.FileFormatType.Default, System.Web.HttpContext.Current.Response);
            return View();

        }


        public ActionResult EditStoreLeadTime(string div, string store)
        {
            var query = (from a in db.StoreLeadTimes
                         where ((a.Division == div) && (a.Store == store))
                         select a);
            EditStoreLeadTimeModel model = new EditStoreLeadTimeModel();

            List<StoreLeadTime> list = new List<StoreLeadTime>();
            ViewData["Store"] = store;
            StoreLeadTime lt;
            int rank = 1;
            if (query.Count() == 0)
            {
                list = new List<StoreLeadTime>();
                foreach (DistributionCenter dc in (from a in db.DistributionCenters
                                                   join b in db.InstanceDistributionCenters
                                                   on a.ID equals b.DCID
                                                   join c in db.InstanceDivisions                                                                                                       
                                                   on b.InstanceID equals c.InstanceID
                                                   where c.Division == div
                                                   select a).ToList())
                {
                    lt = new StoreLeadTime();
                    lt.DCID = dc.ID;
                    lt.Division = div;
                    lt.Store = store;
                    lt.Active = true;
                    lt.Rank = rank;
                    list.Add(lt);
                    rank++;
                }
            }
            else
            {
                list = query.ToList();
                rank = list.Count() + 1;
                var datar = (from a in db.DistributionCenters
                             join b in db.InstanceDistributionCenters
                             on a.InstanceID equals b.InstanceID
                             join c in db.InstanceDivisions
                             on b.InstanceID equals c.InstanceID
                             where c.Division == div
                             select a).ToList();

                foreach (DistributionCenter dc in (from a in db.DistributionCenters
                                                   join b in db.InstanceDistributionCenters
                                                   on a.ID equals b.DCID
                                                   join c in db.InstanceDivisions
                                                   on b.InstanceID equals c.InstanceID
                                                   where c.Division == div
                                                   select a).ToList())
                {
                    if ((from a in list
                         where a.DCID == dc.ID
                         select a).Count() == 0)
                    {
                        lt = new StoreLeadTime();
                        lt.DCID = dc.ID;
                        lt.Division = div;
                        lt.Store = store;
                        lt.Active = false;
                        lt.Rank = rank;
                        lt.LeadTime = 5;
                        list.Add(lt);
                        rank++;
                    }
                }
            }

            model.LeadTimes = list;

            foreach (StoreLeadTime slt in list)
            {
                slt.Warehouse = (from a in db.DistributionCenters
                                 where a.ID == slt.DCID
                                 select a.Name).First();
            }
            //model.Warehouses = (from a in db.DistributionCenters select a).ToList();
            model.Store = (from a in db.StoreLookups
                           where ((a.Store == store) && (a.Division == div))
                           select a).First();

            return View(model);
        }

        [HttpPost]
        public ActionResult EditStoreLeadTime(EditStoreLeadTimeModel model)
        {

            string div = UpdateStoreLeadTimesForModel(model);

            return RedirectToAction("StoreLeadTimes", new { div = div });
            //return View(model);
        }

        [HttpGet]
        public ActionResult UpdatePRStores()
        {
            EditStoreLeadTimeModel model;
            
            List<StoreLookup> stores = (from a in db.StoreLookups where (a.State == "PR")||(a.status == "VI") select a).ToList();

            foreach (StoreLookup s in stores)
            {
                model = new EditStoreLeadTimeModel();
                model.Store = s;
                model.LeadTimes = (from a in db.StoreLeadTimes where ((a.Store == s.Store) && (a.Division == s.Division)) select a).ToList();

                //go through and update existing lead time
                foreach (StoreLeadTime lt in model.LeadTimes)
                {
                    switch (lt.DCID)
                    { 
                        case 1://JC
                            lt.LeadTime = 4;
                            lt.Rank = 2;
                            break;
                        case 2://Camp Hill
                            lt.LeadTime = 5;
                            lt.Rank = 4;
                            break;
                        case 3://Edison
                            lt.LeadTime = 5;
                            lt.Rank = 5;
                            break;
                        case 4://Gilber
                            lt.LeadTime = 4;
                            lt.Rank = 6;
                            break;
                        case 5://Jacksonville
                            lt.LeadTime = 4;
                            lt.Rank = 1;
                            break;
                        case 6://Team Edition
                            lt.LeadTime = 4;
                            lt.Rank = 3;
                            break;
                    }
                }

                string div = UpdateStoreLeadTimesForModel(model);
            }
            return View();
        }

        [HttpGet]
        public ActionResult UpdateHawaiiStores()
        {
            EditStoreLeadTimeModel model;

            List<StoreLookup> stores = (from a in db.StoreLookups where (a.State == "HI") || (a.status == "GU") select a).ToList();

            foreach (StoreLookup s in stores)
            {
                model = new EditStoreLeadTimeModel();
                model.Store = s;
                model.LeadTimes = (from a in db.StoreLeadTimes where ((a.Store == s.Store) && (a.Division == s.Division)) select a).ToList();

                //go through and update existing lead time
                foreach (StoreLeadTime lt in model.LeadTimes)
                {
                    switch (lt.DCID)
                    {
                        case 1://JC
                            lt.LeadTime = 4;
                            lt.Rank = 2;
                            break;
                        case 2://Camp Hill
                            lt.LeadTime = 5;
                            lt.Rank = 5;
                            break;
                        case 3://Edison
                            lt.LeadTime = 5;
                            lt.Rank = 6;
                            break;
                        case 4://Gilber
                            lt.LeadTime = 4;
                            lt.Rank = 1;
                            break;
                        case 5://Jacksonville
                            lt.LeadTime = 4;
                            lt.Rank = 3;
                            break;
                        case 6://Team Edition
                            lt.LeadTime = 4;
                            lt.Rank = 4;
                            break;
                    }
                }

                string div = UpdateStoreLeadTimesForModel(model);
            }
            return View();
        }

        [HttpGet]
        public ActionResult UpdateEuropeStores()
        {
            EditStoreLeadTimeModel model;

            List<StoreLookup> stores = (from a in db.StoreLookups 
                                        where ((a.Division == "31") && 
                                               a.Region != "45") 
                                        select a).ToList();
            List<StoreLeadTime> alreadyDone = (from a in db.StoreLeadTimes 
                                               where ((a.Division == "31") && 
                                                      (a.LeadTime == 4)) 
                                               select a).ToList();
            int count = 0;
            foreach (StoreLookup s in stores)
            {
                count++;
                if ((from a in alreadyDone where ((a.Store == s.Store)) select a).Count() == 0)
                {
                    model = new EditStoreLeadTimeModel();
                    model.Store = s;
                    model.LeadTimes = (from a in db.StoreLeadTimes where ((a.Store == s.Store) && (a.Division == s.Division)) select a).ToList();

                    //go through and update existing lead time
                    foreach (StoreLeadTime lt in model.LeadTimes)
                    {
                        lt.LeadTime = 4;
                    }
                    if (model.LeadTimes.Count() > 0)
                    {
                        try
                        {
                            string div = UpdateStoreLeadTimesForModel(model);
                        }
                        catch (Exception ex)
                        {
                            int i = 0;
                        }
                    }
                }
            }
            return View();
        }

        private string UpdateStoreLeadTimesForModelNoStartDateUpdating(EditStoreLeadTimeModel model)
        {
            string div = "";
            string store = "";

            if (model.LeadTimes.Count > 0)
            {
                div = model.LeadTimes[0].Division;
                store = model.LeadTimes[0].Store;
                //delete stores current zone assignment
                var deleteQuery = (from a in db.NetworkZoneStores where ((a.Division == div) && (a.Store == store)) select a);

                if (deleteQuery.Count() > 0)
                {
                    NetworkZoneStore nzs = deleteQuery.First();
                    int tz = nzs.ZoneID;

                    db.NetworkZoneStores.Remove(nzs);
                    db.SaveChanges();

                    var deleteQuery2 = (from a in db.NetworkZoneStores where a.ZoneID == tz select a);
                    if (deleteQuery2.Count() == 0)
                    {
                        //delete the zone if it's empty now
                        NetworkZone delNZ = (from a in db.NetworkZones where a.ID == tz select a).First();
                        db.NetworkZones.Remove(delNZ);
                        db.SaveChanges();
                    }
                }
            }

            int originalLeadTime = 0;
            //save
            foreach (StoreLeadTime lt in model.LeadTimes)
            {
                var query = (from a in db.StoreLeadTimes where ((a.Division == lt.Division) && (a.Store == lt.Store) && (a.DCID == lt.DCID)) select a);
                if (query.Count() == 0)
                {
                    lt.CreatedBy = User.Identity.Name;
                    lt.CreateDate = DateTime.Now;
                    db.StoreLeadTimes.Add(lt);
                    db.SaveChanges();
                }
                else
                {
                    StoreLeadTime oldLT = query.First();
                    oldLT.LeadTime = lt.LeadTime;
                    oldLT.Rank = lt.Rank;
                    oldLT.Active = lt.Active;
                    oldLT.CreatedBy = User.Identity.Name;
                    oldLT.CreateDate = DateTime.Now;

                    db.SaveChanges();
                }
            }

            //find new zone and save

            //(from a in db.vValidStores where a.Division == div
            NetworkZoneStoreDAO dao = new NetworkZoneStoreDAO();

            int zoneid = dao.GetZoneForStore(div, store);
            NetworkZoneStore zonestore;
            if (zoneid > 0)
            {
                //save it
                zonestore = new NetworkZoneStore();
                zonestore.Division = div;
                zonestore.Store = store;
                zonestore.ZoneID = zoneid;
                db.NetworkZoneStores.Add(zonestore);
                db.SaveChanges();
            }
            else
            {
                //create new zone
                NetworkZone zone = new NetworkZone();
                zone.Name = "Zone " + store;
                zone.LeadTimeID = (from a in db.InstanceDivisions join b in db.NetworkLeadTimes on a.InstanceID equals b.InstanceID where (a.Division == div) select b.ID).First();
                zone.CreateDate = DateTime.Now;
                zone.CreatedBy = User.Identity.Name;
                db.NetworkZones.Add(zone);
                db.SaveChanges();

                zonestore = new NetworkZoneStore();
                zonestore.Division = div;
                zonestore.Store = store;
                zonestore.ZoneID = zone.ID;
                db.NetworkZoneStores.Add(zonestore);
                db.SaveChanges();

            }
            return div;
        }

        private string UpdateStoreLeadTimesForModel(EditStoreLeadTimeModel model)
        {
            string div = "";
            string store = "";

            if (model.LeadTimes.Count > 0)
            {
                div = model.LeadTimes[0].Division;
                store = model.LeadTimes[0].Store;
                //delete stores current zone assignment
                var deleteQuery = (from a in db.NetworkZoneStores 
                                   where a.Division == div && 
                                         a.Store == store 
                                   select a);

                if (deleteQuery.Count() > 0)
                {
                    NetworkZoneStore nzs = deleteQuery.First();
                    int tz = nzs.ZoneID;

                    db.NetworkZoneStores.Remove(nzs);
                    db.SaveChanges();

                    var deleteQuery2 = (from a in db.NetworkZoneStores 
                                        where a.ZoneID == tz 
                                        select a);

                    if (deleteQuery2.Count() == 0)
                    {
                        //delete the zone if it's empty now
                        NetworkZone delNZ = (from a in db.NetworkZones 
                                             where a.ID == tz 
                                             select a).First();
                        db.NetworkZones.Remove(delNZ);
                        db.SaveChanges();
                    }
                }
            }

            int originalLeadTime = 0;
            //save
            foreach (StoreLeadTime lt in model.LeadTimes)
            {
                var query = (from a in db.StoreLeadTimes where ((a.Division == lt.Division) && (a.Store == lt.Store) && (a.DCID == lt.DCID)) select a);
                if (query.Count() == 0)
                {
                    lt.CreatedBy = User.Identity.Name;
                    lt.CreateDate = DateTime.Now;
                    db.StoreLeadTimes.Add(lt);
                    db.SaveChanges();
                }
                else
                {
                    StoreLeadTime oldLT = query.First();
                    oldLT.LeadTime = lt.LeadTime;
                    oldLT.Rank = lt.Rank;
                    oldLT.Active = lt.Active;
                    oldLT.CreatedBy = User.Identity.Name;
                    oldLT.CreateDate = DateTime.Now;

                    db.SaveChanges();
                }
            }

            if (model.LeadTimes.Count > 0)
            {
                ReassignStartDates(model.LeadTimes[0]);
            }

            //find new zone and save

            //(from a in db.vValidStores where a.Division == div
            NetworkZoneStoreDAO dao = new NetworkZoneStoreDAO();

            int zoneid = dao.GetZoneForStore(div, store);
            NetworkZoneStore zonestore;
            if (zoneid > 0)
            {
                //save it
                zonestore = new NetworkZoneStore();
                zonestore.Division = div;
                zonestore.Store = store;
                zonestore.ZoneID = zoneid;
                db.NetworkZoneStores.Add(zonestore);
                db.SaveChanges();
            }
            else
            {
                //create new zone
                NetworkZone zone = new NetworkZone();
                zone.Name = "Zone " + store;
                zone.LeadTimeID = (from a in db.InstanceDivisions join b in db.NetworkLeadTimes on a.InstanceID equals b.InstanceID where (a.Division == div)  select b.ID).First();
                zone.CreateDate = DateTime.Now;
                zone.CreatedBy = User.Identity.Name;
                db.NetworkZones.Add(zone);
                db.SaveChanges();

                zonestore = new NetworkZoneStore();
                zonestore.Division = div;
                zonestore.Store = store;
                zonestore.ZoneID = zone.ID;
                db.NetworkZoneStores.Add(zonestore);
                db.SaveChanges();

            }
            return div;
        }


        public void ReassignStartDates(StoreLeadTime lt)
        {
            RangePlanDetailDAO dao = new RangePlanDetailDAO();
            dao.ReassignStartDates(lt.Division, lt.Store);
        }

        #region Rank Upload
        public ActionResult UploadChanges()
        {
            return View();
        }

        public ActionResult ExcelRoutingChangeTemplate()
        {
            Aspose.Cells.License license = new Aspose.Cells.License();
            //Set the license 
            license.SetLicense(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["AsposeCellsLicense"]));

            string templateFilename = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["RerankStoresTemplate"]);
            Workbook excelDocument = new Workbook(System.Web.HttpContext.Current.Server.MapPath(templateFilename));

            OoxmlSaveOptions save = new OoxmlSaveOptions(SaveFormat.Xlsx);
            excelDocument.Save(System.Web.HttpContext.Current.Response, "NetworkScheduleUpdateTemplate.xlsx", ContentDisposition.Attachment, save);
            return View();
        }

        public ActionResult MassUpdateNSS(IEnumerable<HttpPostedFileBase> attachments)
        {
            Aspose.Cells.License license = new Aspose.Cells.License();
            license.SetLicense(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["AsposeCellsLicense"]));
            List<NSSUpload> errorData = new List<NSSUpload>();
            List<NSSUpload> inputData = new List<NSSUpload>();
            List<DistributionCenter> DCs;
            AllocationLibraryContext allocationLibraryDB = new AllocationLibraryContext();

            foreach (HttpPostedFileBase file in attachments)
            {
                Workbook workbook = new Workbook(file.InputStream);
                Aspose.Cells.Worksheet worksheet = workbook.Worksheets[0];

                int rows = worksheet.Cells.MaxDataRow;
                int columns = worksheet.Cells.MaxDataColumn;

                ExportTableOptions tableOptions = new ExportTableOptions
                {
                    SkipErrorValue = true,
                    ExportColumnName = true,
                    ExportAsString = true
                };
                
                DataTable excelData = worksheet.Cells.ExportDataTable(0, 0, rows + 1, columns + 1, tableOptions);

                if (!(excelData.Columns[0].ColumnName == "Division" && excelData.Columns[1].ColumnName == "Store" && excelData.Columns[2].ColumnName == "Rank 1" &&
                        excelData.Columns[3].ColumnName == "Rank 2" && excelData.Columns[4].ColumnName == "Rank 3" && excelData.Columns[5].ColumnName == "Rank 4" &&
                        excelData.Columns[6].ColumnName == "Rank 5" && excelData.Columns[7].ColumnName == "Rank 6" && excelData.Columns[8].ColumnName == "Rank 7" &&
                        excelData.Columns[9].ColumnName == "Rank 8" && excelData.Columns[10].ColumnName == "Rank 9" && excelData.Columns[11].ColumnName == "Rank 10" &&
                        excelData.Columns[12].ColumnName == "Leadtime 1" && excelData.Columns[13].ColumnName == "Leadtime 2" && excelData.Columns[14].ColumnName == "Leadtime 3" && 
                        excelData.Columns[15].ColumnName == "Leadtime 4" && excelData.Columns[16].ColumnName == "Leadtime 5" && excelData.Columns[17].ColumnName == "Leadtime 6" &&
                        excelData.Columns[18].ColumnName == "Leadtime 7" && excelData.Columns[19].ColumnName == "Leadtime 8" && excelData.Columns[20].ColumnName == "Leadtime 9" &&
                        excelData.Columns[21].ColumnName == "Leadtime 10"))
                {
                    return Content("Incorrectly formatted or missing header row. Please correct and re-process.");
                }

                DCs = (from d in db.DistributionCenters
                       select d).ToList();

                foreach (DataRow row in excelData.Rows)
                {
                    NSSUpload newDataRow = new NSSUpload(allocationLibraryDB, DCs)
                    {
                        SubmittedDivision = row["Division"].ToString(),
                        SubmittedStore = row["Store"].ToString()
                    };
                    
                    for (int i = 0; i < newDataRow.MaxValues; i++)
                    {
                        newDataRow.SubmittedRank.Add(row[String.Format("Rank {0}", i + 1)].ToString());
                        newDataRow.SubmittedLeadtime.Add(row[String.Format("Leadtime {0}", i + 1)].ToString());
                    }

                    inputData.Add(newDataRow);
                }

                foreach (NSSUpload inputRec in inputData)
                {
                    inputRec.Validate();
                }

                errorData = inputData.Where(id => !id.Valid).ToList();
                ProcessNSSUploads(inputData.Where(id => id.Valid).ToList());

                //db.SaveChanges();

                if (errorData.Count() > 0)
                {
                    Session["NSSDataErrors"] = errorData;
                    return Content(String.Format("There were {0} errors", errorData.Count()));
                }
            }

            return Content("");
        }

        public ActionResult DownloadUpdateErrors()
        {
            List<NSSUpload> errorList = (List<NSSUpload>)Session["NSSDataErrors"];
            Aspose.Cells.License license = new Aspose.Cells.License();
            license.SetLicense(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["AsposeCellsLicense"]));

            Workbook excelDocument = new Workbook();
            Aspose.Cells.Worksheet workSheet = excelDocument.Worksheets[0];

            Aspose.Cells.Style style = excelDocument.CreateStyle();
            style.Font.IsBold = true;
            int row, col;

            row = 0;
            col = 0;

            workSheet.Cells[row, col].PutValue("Division");
            workSheet.Cells[row, col].SetStyle(style);
            col++;
            workSheet.Cells[row, col].PutValue("Store");
            workSheet.Cells[row, col].SetStyle(style);
            col++;

            for (int i = 0; i < errorList[0].MaxValues; i++)
            {
                workSheet.Cells[row, col].PutValue(String.Format("Rank {0}", i + 1));
                workSheet.Cells[row, col].SetStyle(style);
                col++;
            }

            for (int i = 0; i < errorList[0].MaxValues; i++)
            {
                workSheet.Cells[row, col].PutValue(String.Format("Leadtime {0}", i + 1));
                workSheet.Cells[row, col].SetStyle(style);
                col++;
            }

            workSheet.Cells[row, col].PutValue("Error Message");
            workSheet.Cells[row, col].SetStyle(style);
            
            foreach (NSSUpload errorRec in errorList)
            {
                row++;
                col = 0;
                workSheet.Cells[row, col].PutValue(errorRec.SubmittedDivision);
                col++;
                workSheet.Cells[row, col].PutValue(errorRec.SubmittedStore);
                col++;

                for (int i = 0; i < errorRec.MaxValues; i++)
                {
                    workSheet.Cells[row, col].PutValue(errorRec.SubmittedRank[i]);
                    col++;
                }

                for (int i = 0; i < errorRec.MaxValues; i++)
                {
                    workSheet.Cells[row, col].PutValue(errorRec.SubmittedLeadtime[i]);
                    col++;
                }

                workSheet.Cells[row, col].PutValue(String.Join("; ", errorRec.ErrorList));
            }

            excelDocument.Save(System.Web.HttpContext.Current.Response, "NSSUploadErrors.xlsx", ContentDisposition.Attachment, 
                new OoxmlSaveOptions(SaveFormat.Xlsx));

            return View();
        }

        private void RemoveStoreLeadTimes(string division, string store)
        {
            //delete stores current zone assignment
            var deleteQuery = (from a in db.NetworkZoneStores
                               where a.Division == division &&
                                     a.Store == store
                               select a);

            if (deleteQuery.Count() > 0)
            {
                NetworkZoneStore nzs = deleteQuery.First();
                int tz = nzs.ZoneID;

                db.NetworkZoneStores.Remove(nzs);

                var deleteQuery2 = (from a in db.NetworkZoneStores
                                    where a.ZoneID == tz
                                    select a);

                if (deleteQuery2.Count() == 0)
                {
                    //delete the zone if it's empty now
                    NetworkZone delNZ = (from a in db.NetworkZones
                                         where a.ID == tz
                                         select a).First();
                    db.NetworkZones.Remove(delNZ);
                }
            }

            List<StoreLeadTime> storeLeadTimes = (from slt in db.StoreLeadTimes
                                                  where slt.Division == division &&
                                                        slt.Store == store
                                                  select slt).ToList();
            foreach (StoreLeadTime slt in storeLeadTimes)
            {
                db.StoreLeadTimes.Remove(slt);
            }
        }

        private List<StoreLeadTime> BuildStoreLeadTimeRecords(NSSUpload uploadRec)
        {
            List<StoreLeadTime> newLeadtimes = new List<StoreLeadTime>();

            for (int i = 0; i < uploadRec.MaxValues; i++)
            {
                StoreLeadTime slt = new StoreLeadTime
                {
                    Division = uploadRec.Division,
                    Store = uploadRec.Store
                };

                if (uploadRec.DCIDList[i] != -1)
                {
                    slt.DCID = uploadRec.DCIDList[i];
                    slt.LeadTime = uploadRec.LeadtimeList[i];
                    slt.Rank = i + 1;
                    slt.Active = true;
                    slt.CreateDate = DateTime.Now;
                    slt.CreatedBy = User.Identity.Name;

                    newLeadtimes.Add(slt);
                }
            }

            return newLeadtimes;                
        }

        private void SetUploadZones(string division, string store)
        {
            NetworkZoneStoreDAO dao = new NetworkZoneStoreDAO();

            int zoneid = dao.GetZoneForStore(division, store);

            NetworkZoneStore zonestore;
            if (zoneid > 0)
            {
                //save it
                zonestore = new NetworkZoneStore
                {
                    Division = division,
                    Store = store,
                    ZoneID = zoneid
                };
            }
            else
            {
                //create new zone
                NetworkZone zone = new NetworkZone
                {
                    Name = "Zone " + store,
                    LeadTimeID = (from a in db.InstanceDivisions 
                                  join b in db.NetworkLeadTimes 
                                  on a.InstanceID equals b.InstanceID 
                                  where (a.Division == division) 
                                  select b.ID).First(),
                    CreateDate = DateTime.Now,
                    CreatedBy = User.Identity.Name
                };

                // see if there are any zones already out there that have the name of the new zone
                List<NetworkZone> oldZones = (from nz in db.NetworkZones
                                              where nz.Name == "Zone " + store
                                              select nz).ToList();

                foreach (NetworkZone netZone in oldZones)
                {
                    string newZoneStore = (from nzs in db.NetworkZoneStores
                                           where nzs.ZoneID == netZone.ID
                                           orderby nzs.Store
                                           select nzs.Store).FirstOrDefault();

                    netZone.Name = "Zone " + newZoneStore;
                    netZone.CreatedBy = User.Identity.Name;
                    netZone.CreateDate = DateTime.Now;
                    db.Entry(netZone).State = EntityState.Modified;
                }

                db.NetworkZones.Add(zone);
                db.SaveChanges();

                zonestore = new NetworkZoneStore
                {
                    Division = division,
                    Store = store,
                    ZoneID = zone.ID
                };
            }

            db.NetworkZoneStores.Add(zonestore);
            db.SaveChanges();
        }

        private void ProcessNSSUploads(List<NSSUpload> inputData)
        {
            foreach (NSSUpload inputRec in inputData)
            {
                RemoveStoreLeadTimes(inputRec.Division, inputRec.Store);

                foreach (StoreLeadTime slt in BuildStoreLeadTimeRecords(inputRec))
                {
                    db.StoreLeadTimes.Add(slt);
                }
            }

            db.SaveChanges();

            foreach (NSSUpload inputRec in inputData)
            {
                ReassignStartDates(new StoreLeadTime
                {
                    Division = inputRec.Division,
                    Store = inputRec.Store                     
                });

                SetUploadZones(inputRec.Division, inputRec.Store);
            }            
        }

        public ActionResult UploadRanks()
        {
            return View();
        }



        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Admin,Support")]
        public ActionResult SaveRanks(IEnumerable<HttpPostedFileBase> attachments)
        {
            Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");
            
            foreach (HttpPostedFileBase file in attachments)
            {
                //Instantiate a Workbook object that represents an Excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];

                List<int> ErrorRows = new List<int>();
                int row = 1;
                string store;
                string div;
                int wc, ch, jc, jv, ed, te;

                DateTime createdate = DateTime.Now;
                while (mySheet.Cells[row, 0].Value != null)
                {
                    try
                    {
                        store = Convert.ToString(mySheet.Cells[row, 2].Value).PadLeft(5,'0');
                        div = Convert.ToString(mySheet.Cells[row, 1].Value).PadLeft(2, '0');
                        jc = GetRank(mySheet, row, 3);
                        jv = GetRank(mySheet, row, 7);
                        te = GetRank(mySheet, row, 8);
                        ed = GetRank(mySheet, row, 5);
                        ch = GetRank(mySheet, row, 4);
                        wc = GetRank(mySheet, row, 6);
                        EditStoreLeadTimeModel model = new EditStoreLeadTimeModel();
                        model.LeadTimes = new List<StoreLeadTime>();

                        List<StoreLeadTime> newlist = new List<StoreLeadTime>();
                        var query = (from a in db.StoreLeadTimes where ((a.Store == store) && (a.Division == div)) select a);
                        StoreLeadTime s;
                        var subquery = (from a in query where a.DCID == 1 select a);

                        for (int dcid = 1; dcid < 7; dcid++)
                        {
                            subquery = (from a in query where a.DCID == dcid select a);

                            if (subquery.Count() > 0)
                            {
                                s = subquery.First();
                            }
                            else
                            {
                                s = new StoreLeadTime();
                                s.Division = div;
                                s.Store = store;
                                s.DCID = dcid;
                                newlist.Add(s);
                            }
                            switch (dcid)
                            {
                                case 1:
                                    s.Rank = jc;
                                    break;
                                case 2:
                                    s.Rank = ch;
                                    break;
                                case 3:
                                    s.Rank = ed;
                                    break;
                                case 4:
                                    s.Rank = wc;
                                    break;
                                case 5:
                                    s.Rank = jv;
                                    break;
                                case 6:
                                    s.Rank = te;
                                    break;
                            }
                            s.Active = (s.Rank > 0);
                            s.CreateDate = createdate;
                            s.CreatedBy = User.Identity.Name;
                            model.LeadTimes.Add(s);
                        }
                        UpdateStoreLeadTimesForModelNoStartDateUpdating(model);
                        //for now, don't create any new ones since we don't know the lead times
                        //foreach (StoreLeadTime slt in newlist)
                        //{
                        //    db.StoreLeadTimes.Add(slt);
                        //}
                    }
                    catch (Exception ex)
                    {
                        ErrorRows.Add(row);
                    }

                    //db.SaveChanges(UserName);

                    row++;
                }
            }

            return Content("");
        }

        private int GetRank(Aspose.Excel.Worksheet mySheet, int row, int col)
        {
            int rank;
            try
            {
                rank = Convert.ToInt32(mySheet.Cells[row, col].Value);
            }
            catch (Exception ex)
            {
                rank = -1;
            }
            return rank;
        }

        /// <summary>
        /// Save the files to a folder.  An array is used because some browsers allow the user to select multiple files at one time.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [CheckPermission(Roles = "Admin,Support")]
        public ActionResult SaveLeadTimes(IEnumerable<HttpPostedFileBase> leadTimeAttachments)
        {
            Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
            Aspose.Excel.License license = new Aspose.Excel.License();
            //Set the license 
            license.SetLicense("C:\\Aspose\\Aspose.Excel.lic");

            foreach (HttpPostedFileBase file in leadTimeAttachments)
            {
                //Instantiate a Workbook object that represents an Excel file
                Aspose.Excel.Excel workbook = new Aspose.Excel.Excel();
                Byte[] data1 = new Byte[file.InputStream.Length];
                file.InputStream.Read(data1, 0, data1.Length);
                file.InputStream.Close();
                MemoryStream memoryStream1 = new MemoryStream(data1);
                workbook.Open(memoryStream1);
                Aspose.Excel.Worksheet mySheet = workbook.Worksheets[0];

                List<int> ErrorRows = new List<int>();
                int row = 1;
                string store;
                string div;
                int wc, ch, jc, jv, ed, te, he;

                DateTime createdate = DateTime.Now;
                while (mySheet.Cells[row, 0].Value != null)
                {
                    try
                    {
                        store = Convert.ToString(mySheet.Cells[row, 2].Value).PadLeft(5, '0');
                        div = Convert.ToString(mySheet.Cells[row, 1].Value).PadLeft(2, '0');
                        jc = GetRank(mySheet, row, 3);
                        jv = GetRank(mySheet, row, 7);
                        te = GetRank(mySheet, row, 8);
                        ed = GetRank(mySheet, row, 5);
                        ch = GetRank(mySheet, row, 4);
                        wc = GetRank(mySheet, row, 6);
                        he = GetRank(mySheet, row, 9);
                        EditStoreLeadTimeModel model = new EditStoreLeadTimeModel();
                        model.LeadTimes = new List<StoreLeadTime>();

                        List<StoreLeadTime> newlist = new List<StoreLeadTime>();
                        var query = (from a in db.StoreLeadTimes where ((a.Store == store) && (a.Division == div)) select a);
                        StoreLeadTime s;
                        var subquery = (from a in query where a.DCID == 1 select a);

                        for (int dcid = 1; dcid < 8; dcid++)
                        {
                            subquery = (from a in query where a.DCID == dcid select a);

                            if (subquery.Count() > 0)
                            {
                                s = subquery.First();
                            }
                            else
                            {
                                s = new StoreLeadTime();
                                s.Division = div;
                                s.Store = store;
                                s.DCID = dcid;
                                newlist.Add(s);
                            }
                            switch (dcid)
                            {
                                case 1:
                                    s.LeadTime = jc;
                                    break;
                                case 2:
                                    s.LeadTime = ch;
                                    break;
                                case 3:
                                    s.LeadTime = ed;
                                    break;
                                case 4:
                                    s.LeadTime = wc;
                                    break;
                                case 5:
                                    s.LeadTime = jv;
                                    break;
                                case 6:
                                    s.LeadTime = te;
                                    break;
                                case 7:
                                    s.LeadTime = he;
                                    break;
                            }
                            s.Active = (s.LeadTime > 0);
                            s.CreateDate = createdate;
                            s.CreatedBy = User.Identity.Name;
                            if (s.Active)
                            {
                                model.LeadTimes.Add(s);
                            }
                        }
                        UpdateStoreLeadTimesForModel(model);
                        //for now, don't create any new ones since we don't know the lead times
                        //foreach (StoreLeadTime slt in newlist)
                        //{
                        //    db.StoreLeadTimes.Add(slt);
                        //}
                    }
                    catch (Exception ex)
                    {
                        ErrorRows.Add(row);
                    }

                    //db.SaveChanges(UserName);

                    row++;
                }
            }

            return Content("");
        }

        #endregion

    }
}
