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
    public class WarehouseInventoryDAO
    {
        Database _database;
        Database _databaseEurope;
        Database _sqlDB;
        AllocationLibraryContext db = new AllocationLibraryContext();

        public WarehouseInventoryDAO()
        {
            _database = DatabaseFactory.CreateDatabase("DB2PROD");
            _databaseEurope = DatabaseFactory.CreateDatabase("DB2EURP");
            _sqlDB = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        public List<WarehouseInventory> GetWarehouseInventory(string sku, string warehouseID)
        {
            string stock, color, dept, div;
            string size;
            string DCID;
            int onHandQty;
            int pickReserve;
            int ringfenceQty;
            int rdqQty;
            string[] tokens = sku.Split('-');
            div = tokens[0];
            dept = tokens[1];
            stock = tokens[2];
            color = tokens[3];
            ItemPack caselot;

            Int32 instanceid = (from a in db.InstanceDivisions where a.Division == div select a.InstanceID).First();
            List<WarehouseInventory> warehouseInventoryList = new List<WarehouseInventory>();

            DbCommand reductionSQLCommand;
            reductionSQLCommand = _sqlDB.GetStoredProcCommand("[GetReductionsBySku]");
            _sqlDB.AddInParameter(reductionSQLCommand, "@sku", DbType.String, sku);
            DataSet reductionData = new DataSet();
            reductionData = _sqlDB.ExecuteDataSet(reductionSQLCommand);

            Database currDatabase = null;
            if (System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"].Contains(div))
            {
                currDatabase = _databaseEurope;
            }
            else
            {
                currDatabase = _database;
            }

            long itemID = (from a in db.ItemMasters
                           where a.MerchantSku.Equals(sku)
                           select a.ID).FirstOrDefault();

            DbCommand SQLCommand;
            string SQL = "select to_char(WHSE_ID_NUM) as WHSE_ID_NUM, lpad(to_char(STK_SIZE_NUM), 3, '0') as Size, ";
            SQL = SQL + " to_number(ALLOCATABLE_BS_QTY) as OnHandQty, pick_rsrv_bs_qty as PickReserve ";
            SQL = SQL + " from TC052002 ";
            SQL = SQL + " where retl_oper_div_cd = '" + div + "' ";
            SQL = SQL + "and stk_dept_num = '" + dept + "' ";
            SQL = SQL + "and stk_num = '" + stock + "' ";
            SQL = SQL + "and stk_wc_num = '" + color + "' ";
            SQL = SQL + "and ALLOCATABLE_BS_QTY > 0 ";

            if (warehouseID != "-1")
                SQL = SQL + " and WHSE_ID_NUM = " + warehouseID;

            SQL = SQL + " union ";
            SQL = SQL + "select to_char(WHSE_ID_NUM) as WHSE_ID_NUM, to_char(CL_SCHED_NUM) as Size, ";
            SQL = SQL + " to_number(ALLOCATABLE_CL_QTY) as OnHandQty, pick_rsrv_cl_qty as PickReserve ";
            SQL = SQL + " from TC052010 ";
            SQL = SQL + " where ";
            SQL = SQL + "retl_oper_div_cd = '" + div + "' ";
            SQL = SQL + "and stk_dept_num = '" + dept + "' ";
            SQL = SQL + "and stk_num = '" + stock + "' ";
            SQL = SQL + "and stk_wc_num = '" + color + "' ";
            SQL = SQL + "and ALLOCATABLE_CL_QTY > 0 ";
            
            if (warehouseID != "-1")
                SQL = SQL + " and WHSE_ID_NUM = " + warehouseID;

            SQLCommand = currDatabase.GetSqlStringCommand(SQL);

            DataSet data = new DataSet();
            data = currDatabase.ExecuteDataSet(SQLCommand);
            if (data.Tables.Count > 0)
            {
                WarehouseInventory inv = new WarehouseInventory();
                DistributionCenter distCenter;
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    size = dr["Size"].ToString();
                    DCID = dr["WHSE_ID_NUM"].ToString();
                    onHandQty = Convert.ToInt32(dr["OnHandQty"]);
                    pickReserve = Convert.ToInt32(dr["PickReserve"]);

                    distCenter = (from a in db.DistributionCenters
                                 where a.MFCode.Equals(DCID)
                                select a).FirstOrDefault();

                    if (distCenter != null)
                    {
                        if (size.Length > 3)
                        {
                            caselot = db.ItemPacks.Include("Details").FirstOrDefault(p => p.ItemID == itemID &&
                                p.Name == size);
                        }
                        else
                            caselot = null;

                        ringfenceQty = 0;
                        rdqQty = 0;

                        if (reductionData.Tables.Count > 0)
                        {
                            var reductionRow = (from row in reductionData.Tables[0].AsEnumerable()
                                                where row.Field<string>("size") == size &&
                                                      row.Field<string>("MFCode") == DCID
                                                select row).ToList();
                            if (reductionRow.Count > 0)
                            {
                                ringfenceQty = Convert.ToInt32(reductionRow[0].ItemArray[1]);
                                rdqQty = Convert.ToInt32(reductionRow[0].ItemArray[2]);
                                onHandQty = onHandQty - Convert.ToInt32(reductionRow[0].ItemArray[3]);
                            }
                        }

                        warehouseInventoryList.Add(WarehouseInventoryFactory.Create(distCenter, itemID,
                            size, onHandQty, pickReserve, caselot, ringfenceQty, rdqQty));
                    }
                }
            }

            return warehouseInventoryList;
        }
    }
}
