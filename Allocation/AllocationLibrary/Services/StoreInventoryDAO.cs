using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Models.Factories;
using Footlocker.Logistics.Allocation.Factories;
using Footlocker.Logistics.Allocation.Services;
using System.Linq;

namespace Footlocker.Logistics.Allocation.Services
{
    public class StoreInventoryDAO
    {
        Database _database;
        Database _databaseEurope;
        AllocationLibraryContext db = new AllocationLibraryContext();

        public StoreInventoryDAO()
        {
            _database = DatabaseFactory.CreateDatabase("DB2PROD");
            _databaseEurope = DatabaseFactory.CreateDatabase("DB2EURP");
        }

        public List<StoreInventory> GetStoreInventoryBySize(string sku, string store)
        {
            string stock, color, dept, div;

            string[] tokens = sku.Split('-');
            div = tokens[0];
            dept = tokens[1];
            stock = tokens[2];
            color = tokens[3];

            Int32 instanceid = (from a in db.InstanceDivisions where a.Division == div select a.InstanceID).First();
            List<StoreInventory> storeInventoryList = new List<StoreInventory>();

            Database currDatabase = null;
            if (System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"].Contains(div))
            {
                currDatabase = _databaseEurope;
            }
            else
            {
                currDatabase = _database;
            }

            DbCommand SQLCommand;
            string SQL = "SELECT STK_SIZE_NUM AS SIZE, ";  
            SQL = SQL + "  sum(BS_OH_QTY) AS ON_HAND, sum(CURR_STR_BIN_PICK) AS BIN_PICK_RESERVE, ";     
            SQL = SQL + "  sum(CURR_STR_CASE_PICK) AS CASE_PICK_RESRVE ";
            SQL = SQL + " FROM TCCRS001 ";
            SQL = SQL + " WHERE RETL_OPER_DIV_CODE = '" + div + "' AND ";
            SQL = SQL + "    STK_DEPT_NUM = '" + dept + "' AND ";                                   
            SQL = SQL + "    STK_NUM = '" + stock + "' AND ";
            SQL = SQL + "    STK_WDTH_COLOR_NUM = '" + color + "'";

            if (!string.IsNullOrEmpty(store))
            {
                SQL = SQL + " AND STR_NUM = '" + store + "'";
            }

            SQL = SQL + " group by STK_SIZE_NUM";

            SQLCommand = currDatabase.GetSqlStringCommand(SQL);

            DataSet data = new DataSet();
            data = currDatabase.ExecuteDataSet(SQLCommand);
            if (data.Tables.Count > 0)
            {
                StoreInventory inv;

                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    inv = new StoreInventory();

                    //storeID = dr["store"].ToString();
                    //inv.store = (from s in db.StoreLookups
                    //             where s.Store == storeID && s.Division == div
                    //             select s).FirstOrDefault();

                    inv.storeInventorySize = dr["size"].ToString();
                    inv.onHandQuantity = Convert.ToInt32(dr["on_hand"]);
                    inv.binPickReserve = Convert.ToInt32(dr["BIN_PICK_RESERVE"]);
                    inv.caselotPickReserve = Convert.ToInt32(dr["CASE_PICK_RESRVE"]);

                    storeInventoryList.Add(inv);
                }
            }

            return storeInventoryList;
        }

        public List<StoreInventory> GetStoreInventoryForSize(string sku, string store, string size)
        {
            string stock, color, dept, div;

            string[] tokens = sku.Split('-');
            div = tokens[0];
            dept = tokens[1];
            stock = tokens[2];
            color = tokens[3];

            Int32 instanceid = (from a in db.InstanceDivisions where a.Division == div select a.InstanceID).First();
            List<StoreInventory> storeInventoryList = new List<StoreInventory>();

            List<StoreLookup> allStores = (from s in db.StoreLookups
                                           where s.Division == div
                                           select s).ToList();

            Database currDatabase = null;
            if (System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"].Contains(div))
            {
                currDatabase = _databaseEurope;
            }
            else
            {
                currDatabase = _database;
            }

            DbCommand SQLCommand;
            string SQL = "SELECT STR_NUM AS STORE, ";
            SQL = SQL + "  sum(BS_OH_QTY) AS ON_HAND, sum(CURR_STR_BIN_PICK) AS BIN_PICK_RESERVE, ";
            SQL = SQL + "  sum(CURR_STR_CASE_PICK) AS CASE_PICK_RESRVE ";
            SQL = SQL + " FROM TCCRS001 ";
            SQL = SQL + " WHERE RETL_OPER_DIV_CODE = '" + div + "' AND ";
            SQL = SQL + "    STK_DEPT_NUM = '" + dept + "' AND ";
            SQL = SQL + "    STK_NUM = '" + stock + "' AND ";
            SQL = SQL + "    STK_WDTH_COLOR_NUM = '" + color + "' AND ";
            SQL = SQL + "    STK_SIZE_NUM = '" + size + "'";

            if (!string.IsNullOrEmpty(store))
            {
                SQL = SQL + " AND STR_NUM = '" + store + "'";
            }

            SQL = SQL + " group by STR_NUM";

            SQLCommand = currDatabase.GetSqlStringCommand(SQL);

            DataSet data = new DataSet();
            data = currDatabase.ExecuteDataSet(SQLCommand);
            if (data.Tables.Count > 0)
            {
                StoreInventory inv;
                string storeID;

                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    inv = new StoreInventory();

                    storeID = dr["store"].ToString();
                    // I'm just grabbing the mall this way because it performs faster
                    inv.store = new StoreLookup();
                    inv.store.Store = storeID;
                    inv.store.Mall = (from s in allStores
                                      where s.Store == storeID && s.Division == div
                                      select s.Mall).FirstOrDefault();
                    //inv.store = (from s in db.StoreLookups
                    //             where s.Store == storeID && s.Division == div
                    //             select s).FirstOrDefault();

                    inv.onHandQuantity = Convert.ToInt32(dr["on_hand"]);
                    inv.binPickReserve = Convert.ToInt32(dr["BIN_PICK_RESERVE"]);
                    inv.caselotPickReserve = Convert.ToInt32(dr["CASE_PICK_RESRVE"]);

                    storeInventoryList.Add(inv);
                }
            }

            return storeInventoryList;
        }
    }
}
