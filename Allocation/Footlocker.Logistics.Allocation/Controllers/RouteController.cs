using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Services;
using Aspose.Cells;
using System.Data;
using Footlocker.Logistics.Allocation.Spreadsheets;

namespace Footlocker.Logistics.Allocation.Controllers
{
    [CheckPermission(Roles = "Support,Logistics,Advanced Merchandiser Processes")]
    public class RouteController : AppController
    {
        Footlocker.Logistics.Allocation.DAO.AllocationContext db = new DAO.AllocationContext();
        ConfigService configService = new ConfigService();
        //
        // GET: /Route/

        public ActionResult Index(int instanceID = 1)
        {
            RouteModel model = new RouteModel
            {
                InstanceID = instanceID,
                AvailableInstances = new SelectList(db.Instances.ToList(), "ID", "Name", instanceID),
                Routes = db.Routes.Where(r => r.InstanceID == instanceID).ToList()
            };

            if (model.Routes.Count > 0)
            {
                List<string> uniqueNames = (from l in model.Routes
                                            select l.CreatedBy).Distinct().ToList();
                Dictionary<string, string> fullNamePairs = new Dictionary<string, string>();

                foreach (var item in uniqueNames)
                {
                    fullNamePairs.Add(item, getFullUserNameFromDatabase(item.Replace('\\', '/')));
                }

                foreach (var item in fullNamePairs)
                {
                    model.Routes.Where(x => x.CreatedBy == item.Key).ToList().ForEach(y => y.CreatedBy = item.Value);
                }
            }
            return View(model);
        }

        public ActionResult DownloadRoutes(int instanceID)
        {
            RoutesExport routesExport = new RoutesExport(appConfig);
            routesExport.WriteData(instanceID);

            routesExport.excelDocument.Save(System.Web.HttpContext.Current.Response, "Routes.xlsx", ContentDisposition.Attachment, routesExport.SaveOptions);
            return View();
        }

        public ActionResult Create(int instanceID)
        {
            Route r = new Route()
            {
                InstanceID = instanceID
            };
            
            return View(r);
        }

        [HttpPost]
        public ActionResult Create(Route model)
        {
            model.CreatedBy = currentUser.NetworkID;
            model.CreateDate = DateTime.Now;

            db.Routes.Add(model);
            db.SaveChanges();
            return RedirectToAction("Index", new { instanceID = model.InstanceID });
        }

        public ActionResult Delete(int ID)
        {
            List<RouteDistributionCenter> dcs = db.RouteDistributionCenters.Where(rdc => rdc.RouteID == ID).ToList();
            foreach (RouteDistributionCenter dc in dcs)
            {
                db.RouteDistributionCenters.Remove(dc);                
            }

            Route det = db.Routes.Where(r => r.ID == ID).FirstOrDefault();

            db.Routes.Remove(det);
            db.SaveChanges();

            return RedirectToAction("Index", new { instanceID = det.InstanceID });
        }

        public ActionResult EditRoute(int ID)
        {
            EditRouteModel model = new EditRouteModel()
            {
                Route = db.Routes.Where(r => r.ID == ID).FirstOrDefault(),
                DCs = new List<DistributionCenterModel>()
            };

            List<DistributionCenter> temp = (from a in db.DistributionCenters 
                                             join b in db.InstanceDistributionCenters 
                                             on a.ID equals b.DCID 
                                             where b.InstanceID == model.Route.InstanceID 
                                             select a).ToList();

            DistributionCenterModel dcmodel;
            
            foreach (DistributionCenter dc in temp)
            {
                dcmodel = new DistributionCenterModel()
                {
                    DC = dc,
                    Zones = db.RouteDetails.Where(rd => rd.DCID == dc.ID && rd.RouteID == ID).Count()
                };
                                
                model.DCs.Add(dcmodel);
            }
            return View(model);
        }

        public ActionResult EditRouteZones(int dcID, int routeID)
        {
            EditRouteZonesModel model = new EditRouteZonesModel();
            model.Route = db.Routes.Where(r => r.ID == routeID).FirstOrDefault();
            model.DC = db.DistributionCenters.Where(dc => dc.ID == dcID).FirstOrDefault();
            model.RouteDetails = db.RouteDetails.Where(rd => rd.RouteID == routeID && rd.DCID == dcID).ToList();
            
            model.CurrentZones = (from b in db.RouteDetails 
                                  join a in db.NetworkZones 
                                  on b.ZoneID equals a.ID 
                                  where b.RouteID == routeID && 
                                        b.DCID == dcID
                                  select a).ToList();

            model.AvailableZones = (from a in db.NetworkZones 
                                    join lt in db.NetworkLeadTimes 
                                    on a.LeadTimeID equals lt.ID 
                                    where ((lt.InstanceID == model.Route.InstanceID) && 
                                           (!(from b in db.RouteDetails 
                                              where b.ZoneID == a.ID && 
                                                    b.RouteID == routeID && 
                                                    b.DCID == dcID 
                                              select b).Any())) 
                                    select a).ToList();
                                                                  
            return View(model);
        }

        public ActionResult AddRouteZone(int dcID, int routeID, int zoneID, int days)
        {
            List<RouteDetail> existingRoutes = db.RouteDetails.Where(rd => rd.RouteID == routeID && rd.ZoneID == zoneID).ToList();
            if (existingRoutes.Count() > 0)
            {
                db.RouteDetails.Remove(existingRoutes.First());
                db.SaveChanges();
            }
            RouteDetail det = new RouteDetail()
            {
                DCID = dcID,
                RouteID = routeID,
                ZoneID = zoneID,
                Days = days
            };

            db.RouteDetails.Add(det);
            db.SaveChanges();

            return RedirectToAction("EditRouteZones", new { dcID, routeID});
        }

        public ActionResult DeleteRouteZone(int dcID, int routeID, int zoneID)
        {            
            RouteDetail det = db.RouteDetails.Where(rd => rd.DCID == dcID && rd.RouteID == routeID && rd.ZoneID == zoneID).FirstOrDefault();

            db.RouteDetails.Remove(det);
            db.SaveChanges();

            return RedirectToAction("EditRouteZones", new { dcID = dcID, routeID = routeID });
        }

        public ActionResult DCIndex(string instanceID)
        {
            DCList model = new DCList
            {
                AvailableInstances = db.Instances.ToList(),
                InstanceID = Convert.ToInt32(instanceID)
            };

            if (model.InstanceID == 0)
            {
                model.InstanceID = 1;
            }

            model.DCs = (from a in db.DistributionCenters
                         join b in db.InstanceDistributionCenters 
                           on a.ID equals b.DCID
                         where b.InstanceID == model.InstanceID
                         select a).ToList();

            Dictionary<string, string> names = new Dictionary<string, string>();
            var users = (from a in model.DCs
                         select a.LastModifiedUser).Distinct();
            foreach (string userID in users)
            {
                names.Add(userID, getFullUserNameFromDatabase(userID.Replace('\\', '/')));
            }
            foreach (var item in model.DCs)
            {
                item.LastModifiedUserName = names[item.LastModifiedUser];
            }

            return View(model);
        }

        public ActionResult EditDC(int id)
        {
            DistributionCenterModel model = new DistributionCenterModel()
            {
                DC = db.DistributionCenters.Where(dc => dc.ID == id).First(),
                WarehouseAllocationTypes = db.WarehouseAllocationTypes.ToList(),
                DistributionCenterRestrictions = db.DistributionCenterRestrictions.ToList(),
                AvailableInstances = db.Instances.ToList(),
                SelectedInstances = new List<CheckBoxModel>()
            };

            bool instChecked;
            foreach (var inst in model.AvailableInstances)
            {
                instChecked = db.InstanceDistributionCenters.Any(i => i.DCID == id && i.InstanceID == inst.ID);
                model.SelectedInstances.Add(new CheckBoxModel 
                                                { 
                                                    ID = inst.ID, 
                                                    Desc = inst.Name, 
                                                    Checked = instChecked 
                                                });
            }
            return View(model);          
        }

        [HttpPost]
        public ActionResult EditDC(DistributionCenterModel model)
        {
            model.DC.LastModifiedDate = DateTime.Now;
            model.DC.LastModifiedUser = User.Identity.Name;
            db.Entry(model.DC).State = EntityState.Modified;

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
                    db.InstanceDistributionCenters.Remove(db.InstanceDistributionCenters
                                                                .Where(idc => idc.DCID == model.DC.ID && 
                                                                              idc.InstanceID == selInst.ID).FirstOrDefault());
                }
            }

            db.SaveChanges();
            return RedirectToAction("DCIndex", new { instanceID = model.DC.InstanceID });
        }


        public ActionResult CreateDC(int instanceID)
        {
            DistributionCenterModel model = new DistributionCenterModel()
            {
                DC = new DistributionCenter()
                {
                    InstanceID = instanceID
                },
                WarehouseAllocationTypes = db.WarehouseAllocationTypes.ToList(),
                AvailableInstances = db.Instances.ToList(),
                DistributionCenterRestrictions = db.DistributionCenterRestrictions.ToList(),
                SelectedInstances = new List<CheckBoxModel>()
            };

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
            model.DC.CreatedBy = currentUser.NetworkID;
            model.DC.LastModifiedDate = DateTime.Now;
            model.DC.LastModifiedUser = currentUser.NetworkID;

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


        public ActionResult RouteDC(int routeID, int instanceID)
        {
            RouteDCModel model = new RouteDCModel()
            {
                Route = db.Routes.Where(r => r.ID == routeID).First(),
                instanceID = instanceID
            };
            
            model.AssignedDCs = (from a in db.DistributionCenters 
                                 join b in db.RouteDistributionCenters 
                                 on a.ID equals b.DCID 
                                 where b.RouteID == routeID 
                                 select a).ToList();

            model.RemainingDCs = (from dc in db.DistributionCenters
                                  join b in db.InstanceDistributionCenters
                                  on dc.ID equals b.DCID
                                  where b.InstanceID == instanceID
                                  select dc).ToList();
            
            foreach (DistributionCenter dc in model.AssignedDCs)
            {
                model.RemainingDCs.Remove(dc);
            }

            return View(model);
        }

        public ActionResult AddDCToRoute(int routeID, int DCID, int instanceID)
        {
            if (db.RouteDistributionCenters.Where(rdc => rdc.RouteID == routeID && rdc.DCID == DCID).Count() == 0)
            {
                RouteDistributionCenter rdc = new RouteDistributionCenter()
                {
                    RouteID = routeID,
                    DCID = DCID,
                    CreatedBy = currentUser.NetworkID,
                    CreateDate = DateTime.Now
                };

                db.RouteDistributionCenters.Add(rdc);
                db.SaveChanges();
            }

            return RedirectToAction("RouteDC", new { routeID, instanceID });
        }

        public ActionResult DeleteDCFromRoute(int routeID, int DCID, int instanceID)
        {
            RouteDistributionCenter routeDistributionCenter = db.RouteDistributionCenters.Where(rdc => rdc.RouteID == routeID && rdc.DCID == DCID).FirstOrDefault();
            
            if (routeDistributionCenter != null)
            {
                db.RouteDistributionCenters.Remove(routeDistributionCenter);
                db.SaveChanges();
            }

            return RedirectToAction("RouteDC", new { routeID, instanceID });
        }

        public ActionResult StoreLeadTimes(string div)
        {
            StoreLeadTimeModel model = new StoreLeadTimeModel()
            {
                Divisions = currentUser.GetUserDivisions(AppName),
                CanUploadData = currentUser.HasUserRole(AppName, "IT")
            };
            
            if ((div == null) && (model.Divisions.Count > 0))
            {
                div = model.Divisions[0].DivCode;
            }

            model.Division = div;
            model.StoreZones = new List<StoreZoneModel>();

            var query1 = from a in db.vValidStores
                          join b in db.NetworkZoneStores 
                            on new { a.Division, a.Store } equals new { b.Division, b.Store } into gj
                          from subset in gj.DefaultIfEmpty()
                          where a.Division == div
                          select new { a.Division, a.Store, a.State, a.Mall, a.City, subset.ZoneID };

            var query2 = from c in query1
                          join d in db.NetworkZones 
                             on c.ZoneID equals d.ID into g2
                          from subset2 in g2.DefaultIfEmpty()
                          orderby subset2.Name
                          select new { c.Division, c.Store, c.State, c.Mall, c.City,
                              ZoneName = subset2 == null ? "<unassigned>" : subset2.Name };

            var query3 = from e in query2
                          join f in db.MinihubStores
                            on new { e.Division, e.Store } equals new { f.Division, f.Store } into oj
                          from f in oj.DefaultIfEmpty()
                          select new { e.Division, e.Store, e.State, e.Mall, e.City, e.ZoneName,
                                       MinihubStore = f != null };

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

                bool ecommwarehouse = false;

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

        public ActionResult DownloadStoreLeadTimes(string div)
        {
            StoreNSSExport storeNSSExport = new StoreNSSExport(appConfig, configService, new NetworkZoneStoreDAO());
            storeNSSExport.WriteData(div);

            storeNSSExport.excelDocument.Save(System.Web.HttpContext.Current.Response, "StoreLeadTimes.xlsx", ContentDisposition.Attachment, storeNSSExport.SaveOptions);
            return View();
        }

        public ActionResult EditStoreLeadTime(string div, string store)
        {
            List<StoreLeadTime> list = db.StoreLeadTimes.Where(slt => slt.Division == div && slt.Store == store).ToList();
            EditStoreLeadTimeModel model = new EditStoreLeadTimeModel();
            
            ViewData["Store"] = store;
            StoreLeadTime lt;
            int rank = 1;
            if (list.Count() == 0)
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
                    lt = new StoreLeadTime()
                    {
                        DCID = dc.ID,
                        Division = div,
                        Store = store,
                        Active = true,
                        Rank = rank
                    };
                    
                    list.Add(lt);
                    rank++;
                }
            }
            else
            {
                rank = list.Count() + 1;

                foreach (DistributionCenter dc in (from a in db.DistributionCenters
                                                   join b in db.InstanceDistributionCenters
                                                   on a.ID equals b.DCID
                                                   join c in db.InstanceDivisions
                                                   on b.InstanceID equals c.InstanceID
                                                   where c.Division == div
                                                   select a).ToList())
                {
                    if (list.Where(l => l.DCID == dc.ID).Count() == 0)
                    {
                        lt = new StoreLeadTime()
                        {
                            DCID = dc.ID,
                            Division = div,
                            Store = store,
                            Active = false,
                            Rank = rank,
                            LeadTime = 5
                        };

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

            model.Store = db.StoreLookups.Where(sl => sl.Store == store && sl.Division == div).First();

            return View(model);
        }

        [HttpPost]
        public ActionResult EditStoreLeadTime(EditStoreLeadTimeModel model)
        {
            string div = UpdateStoreLeadTimesForModel(model);

            return RedirectToAction("StoreLeadTimes", new { div });
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
                List<NetworkZoneStore> networkZoneStores = db.NetworkZoneStores.Where(nzs => nzs.Division == div && nzs.Store == store).ToList();

                if (networkZoneStores.Count() > 0)
                {
                    NetworkZoneStore nzs = networkZoneStores.First();
                    int tz = nzs.ZoneID;

                    db.NetworkZoneStores.Remove(nzs);
                    //db.SaveChanges();

                    List<NetworkZoneStore> remainingZones = db.NetworkZoneStores.Where(nz => nz.ZoneID == tz).ToList();

                    if (remainingZones.Count() == 0)
                    {
                        //delete the zone if it's empty now
                        NetworkZone delNZ = db.NetworkZones.Where(n => n.ID == tz).FirstOrDefault();
                        db.NetworkZones.Remove(delNZ);
                        db.SaveChanges();
                    }
                }
            }

            //save
            foreach (StoreLeadTime lt in model.LeadTimes)
            {
                List<StoreLeadTime> storeLeadTimes = db.StoreLeadTimes.Where(slt => slt.Division == lt.Division && 
                                                                                    slt.Store == lt.Store &&
                                                                                    slt.DCID == lt.DCID).ToList(); 
                if (storeLeadTimes.Count() == 0)
                {
                    lt.CreatedBy = currentUser.NetworkID;
                    lt.CreateDate = DateTime.Now;
                    db.StoreLeadTimes.Add(lt);                    
                }
                else
                {
                    StoreLeadTime oldLT = storeLeadTimes.First();
                    oldLT.LeadTime = lt.LeadTime;
                    oldLT.Rank = lt.Rank;
                    oldLT.Active = lt.Active;
                    oldLT.CreatedBy = currentUser.NetworkID;
                    oldLT.CreateDate = DateTime.Now;                    
                }
            }
            db.SaveChanges();

            if (model.LeadTimes.Count > 0)
            {
                ReassignStartDates(model.LeadTimes[0]);
            }

            //find new zone and save

            NetworkZoneStoreDAO dao = new NetworkZoneStoreDAO();
            int zoneid = dao.GetZoneForStore(div, store);
            NetworkZoneStore zonestore;
            if (zoneid > 0)
            {
                //save it
                zonestore = new NetworkZoneStore()
                {
                    Division = div,
                    Store = store,
                    ZoneID = zoneid
                };
                db.NetworkZoneStores.Add(zonestore);
                db.SaveChanges();
            }
            else
            {
                //create new zone
                NetworkZone zone = new NetworkZone()
                {
                    Name = string.Format("Zone {0}", store),
                    CreateDate = DateTime.Now,
                    CreatedBy = currentUser.NetworkID
                };

                zone.LeadTimeID = (from a in db.InstanceDivisions 
                                   join b in db.NetworkLeadTimes 
                                     on a.InstanceID equals b.InstanceID 
                                   where a.Division == div  
                                   select b.ID).First();

                db.NetworkZones.Add(zone);
                // save to generate ID
                db.SaveChanges();

                zonestore = new NetworkZoneStore()
                {
                    Division = div,
                    Store = store,
                    ZoneID = zone.ID
                };

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
            RerankStoresSpreadsheet rerankStoresSpreadsheet = new RerankStoresSpreadsheet(appConfig, configService, new RangePlanDetailDAO(), new NetworkZoneStoreDAO());
            rerankStoresSpreadsheet.GetTemplate().Save(System.Web.HttpContext.Current.Response, "NetworkScheduleUpdateTemplate.xlsx", ContentDisposition.Attachment, rerankStoresSpreadsheet.SaveOptions);
            return View();
        }

        public ActionResult MassUpdateNSS(IEnumerable<HttpPostedFileBase> attachments)
        {
            RerankStoresSpreadsheet rerankStoresSpreadsheet = new RerankStoresSpreadsheet(appConfig, configService, new RangePlanDetailDAO(), new NetworkZoneStoreDAO());

            foreach (HttpPostedFileBase file in attachments)
            {
                rerankStoresSpreadsheet.Save(file);

                if (!string.IsNullOrEmpty(rerankStoresSpreadsheet.message))
                    return Content(rerankStoresSpreadsheet.message);
                else
                {
                    if (rerankStoresSpreadsheet.errorList.Count() > 0)
                    {
                        Session["NSSDataErrors"] = rerankStoresSpreadsheet.errorList;
                        return Content(string.Format("There were {0} errors", rerankStoresSpreadsheet.errorList.Count()));
                    }
                }
            }

            return Content("");
        }

        public ActionResult DownloadUpdateErrors()
        {
            RerankStoresSpreadsheet rerankStoresSpreadsheet = new RerankStoresSpreadsheet(appConfig, configService, new RangePlanDetailDAO(), new NetworkZoneStoreDAO());

            List<NSSUpload> errorList = (List<NSSUpload>)Session["NSSDataErrors"];
            Workbook excelDocument = rerankStoresSpreadsheet.GetErrors(errorList);

            excelDocument.Save(System.Web.HttpContext.Current.Response, "NSSUploadErrors.xlsx", ContentDisposition.Attachment, rerankStoresSpreadsheet.SaveOptions);

            return View();
        }
        #endregion
    }
}