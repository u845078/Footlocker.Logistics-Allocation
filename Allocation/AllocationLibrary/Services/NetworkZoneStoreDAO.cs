using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;
using Footlocker.Common;
using System.Security.Cryptography;
using System.Linq;

namespace Footlocker.Logistics.Allocation.Services
{
    public class NetworkZoneStoreDAO
    {
        readonly Database _database;
        readonly AllocationLibraryContext db = new AllocationLibraryContext();

        public NetworkZoneStoreDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        public int GetZoneForStore(string div, string store)
        {
            DbCommand SQLCommand;
            string SQL = "dbo.[GetZoneForStore]";
            Database _database = DatabaseService.GetSqlDatabase("AllocationContext");
            SQLCommand = DatabaseService.GetStoredProcCommand(_database, SQL);
            _database.AddInParameter(SQLCommand, "@div", DbType.String, div);
            _database.AddInParameter(SQLCommand, "@store", DbType.String, store);

            DataSet data;
            data = _database.ExecuteDataSet(SQLCommand);

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    return Convert.ToInt32(dr["zoneid"]);
                }
            }

            return -1;
        }

        public NetworkZoneStore GetNearestStore(string div, string store)
        {
            DbCommand SQLCommand;
            string SQL = "dbo.[GetNearestStore]";
            Microsoft.Practices.EnterpriseLibrary.Data.Database _database = Footlocker.Common.DatabaseService.GetSqlDatabase("AllocationContext");
            SQLCommand = Footlocker.Common.DatabaseService.GetStoredProcCommand(_database, SQL);
            _database.AddInParameter(SQLCommand, "@div", DbType.String, div);
            _database.AddInParameter(SQLCommand, "@store", DbType.String, store);

            DataSet data;
            data = _database.ExecuteDataSet(SQLCommand);

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    NetworkZoneStore nzstore = new NetworkZoneStore()
                    {
                        ZoneID = Convert.ToInt32(dr["zoneid"]),
                        Division = Convert.ToString(dr["division"]),
                        Store = Convert.ToString(dr["store"])
                    };

                    return nzstore;
                }
            }

            return new NetworkZoneStore();            
        }

        public List<NetworkZone> GetStoreLeadTimes(int instanceID)
        {
            List<NetworkZone> list = (from a in db.NetworkZones
                                      join b in db.NetworkZoneStores
                                        on a.ID equals b.ZoneID
                                      join c in db.InstanceDivisions
                                        on b.Division equals c.Division
                                      where c.InstanceID == instanceID
                                      select a).Distinct().ToList();

            return list;
        }

        public int CreateNewZone(string division, string store, string creatingUser)
        {
            //create new zone
            NetworkZone zone = new NetworkZone
            {
                Name = string.Format("Zone {0}", store),
                LeadTimeID = (from a in db.InstanceDivisions
                              join b in db.NetworkLeadTimes
                              on a.InstanceID equals b.InstanceID
                              where a.Division == division
                              select b.ID).First(),
                CreateDate = DateTime.Now,
                CreatedBy = creatingUser
            };

            // see if there are any zones already out there that have the name of the new zone
            List<NetworkZone> oldZones = db.NetworkZones.Where(nz => nz.Name == "Zone " + store).ToList();

            foreach (NetworkZone netZone in oldZones)
            {
                string newZoneStore = (from nzs in db.NetworkZoneStores
                                       where nzs.ZoneID == netZone.ID
                                       orderby nzs.Store
                                       select nzs.Store).FirstOrDefault();

                netZone.Name = string.Format("Zone {0}", newZoneStore);
                netZone.CreatedBy = creatingUser;
                netZone.CreateDate = DateTime.Now;
                db.Entry(netZone).State = EntityState.Modified;
            }

            db.NetworkZones.Add(zone);
            db.SaveChanges();

            return zone.ID;
        }
    }
}