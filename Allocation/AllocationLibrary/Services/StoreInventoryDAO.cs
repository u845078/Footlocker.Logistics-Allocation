using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using System.Linq;

namespace Footlocker.Logistics.Allocation.Services
{
    public class StoreInventoryDAO
    {
        Database _database;
        Database _databaseEurope;
        AllocationLibraryContext db = new AllocationLibraryContext();
        readonly string europeDivisions;

        public StoreInventoryDAO(string europeDivisions)
        {
            _database = DatabaseFactory.CreateDatabase("DB2PROD");
            _databaseEurope = DatabaseFactory.CreateDatabase("DB2EURP");
            this.europeDivisions = europeDivisions;
        }

        public List<StoreInventory> GetStoreInventoryBySize(string sku, string store)
        {
            string stock, color, dept, div;

            string[] tokens = sku.Split('-');
            div = tokens[0];
            dept = tokens[1];
            stock = tokens[2];
            color = tokens[3];
            
            List<StoreInventory> storeInventoryList = new List<StoreInventory>();

            Database currDatabase;
            if (europeDivisions.Contains(div))            
                currDatabase = _databaseEurope;            
            else            
                currDatabase = _database;            

            DbCommand SQLCommand;
            string SQL = "SELECT STK_SIZE_NUM AS SIZE, ";  
            SQL += "  sum(BS_OH_QTY) AS ON_HAND, sum(CURR_STR_BIN_PICK) AS BIN_PICK_RESERVE, ";     
            SQL += "  sum(CURR_STR_CASE_PICK) AS CASE_PICK_RESRVE ";
            SQL += " FROM TCCRS001 ";
            SQL += " WHERE RETL_OPER_DIV_CODE = ? AND ";
            SQL += "    STK_DEPT_NUM = ? AND ";                                   
            SQL += "    STK_NUM = ? AND ";
            SQL += "    STK_WDTH_COLOR_NUM = ?";

            if (!string.IsNullOrEmpty(store))            
                SQL += " AND STR_NUM = ?";            

            SQL += " group by STK_SIZE_NUM";

            SQLCommand = currDatabase.GetSqlStringCommand(SQL);
            currDatabase.AddInParameter(SQLCommand, "@1", DbType.String, div);
            currDatabase.AddInParameter(SQLCommand, "@2", DbType.String, dept);
            currDatabase.AddInParameter(SQLCommand, "@3", DbType.String, stock);
            currDatabase.AddInParameter(SQLCommand, "@4", DbType.String, color);

            if (!string.IsNullOrEmpty(store))
                currDatabase.AddInParameter(SQLCommand, "@5", DbType.String, store);

            DataSet data;
            data = currDatabase.ExecuteDataSet(SQLCommand);
            if (data.Tables.Count > 0)
            {
                StoreInventory inv;

                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    inv = new StoreInventory()
                    {
                        storeInventorySize = dr["size"].ToString(),
                        onHandQuantity = Convert.ToInt32(dr["on_hand"]),
                        binPickReserve = Convert.ToInt32(dr["BIN_PICK_RESERVE"]),
                        caselotPickReserve = Convert.ToInt32(dr["CASE_PICK_RESRVE"])
                    };

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
           
            List<StoreInventory> storeInventoryList = new List<StoreInventory>();
            List<StoreLookup> allStores = db.StoreLookups.Where(s => s.Division == div).ToList();

            Database currDatabase;
            if (europeDivisions.Contains(div))            
                currDatabase = _databaseEurope;            
            else            
                currDatabase = _database;           

            DbCommand SQLCommand;
            string SQL = "SELECT STR_NUM AS STORE, ";
            SQL += "  sum(BS_OH_QTY) AS ON_HAND, sum(CURR_STR_BIN_PICK) AS BIN_PICK_RESERVE, ";
            SQL += "  sum(CURR_STR_CASE_PICK) AS CASE_PICK_RESRVE ";
            SQL += " FROM TCCRS001 ";
            SQL += " WHERE RETL_OPER_DIV_CODE = ? AND ";
            SQL += "    STK_DEPT_NUM = ? AND ";
            SQL += "    STK_NUM = ? AND ";
            SQL += "    STK_WDTH_COLOR_NUM = ? AND ";
            SQL += "    STK_SIZE_NUM = ?";

            if (!string.IsNullOrEmpty(store))            
                SQL += " AND STR_NUM = ?";            

            SQL += " group by STR_NUM";

            SQLCommand = currDatabase.GetSqlStringCommand(SQL);
            currDatabase.AddInParameter(SQLCommand, "@1", DbType.String, div);
            currDatabase.AddInParameter(SQLCommand, "@2", DbType.String, dept);
            currDatabase.AddInParameter(SQLCommand, "@3", DbType.String, stock);
            currDatabase.AddInParameter(SQLCommand, "@4", DbType.String, color);
            currDatabase.AddInParameter(SQLCommand, "@5", DbType.String, size);

            if (!string.IsNullOrEmpty(store))
                currDatabase.AddInParameter(SQLCommand, "@6", DbType.String, store);

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
                    inv.store = new StoreLookup()
                    {
                        Store = storeID
                    };
                    
                    inv.onHandQuantity = Convert.ToInt32(dr["on_hand"]);
                    inv.binPickReserve = Convert.ToInt32(dr["BIN_PICK_RESERVE"]);
                    inv.caselotPickReserve = Convert.ToInt32(dr["CASE_PICK_RESRVE"]);

                    inv.store.Mall = (from s in allStores
                                      where s.Store == storeID && s.Division == div
                                      select s.Mall).FirstOrDefault();

                    storeInventoryList.Add(inv);
                }
            }

            return storeInventoryList;
        }
    }
}
