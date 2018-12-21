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
        string _SKU;
        string _WarehouseID;
        string _Division;
        string _Stock;
        string _Color;
        string _Dept;

        public WarehouseInventoryDAO()
        {
            _database = DatabaseFactory.CreateDatabase("DB2PROD");
            _databaseEurope = DatabaseFactory.CreateDatabase("DB2EURP");
            _sqlDB = DatabaseFactory.CreateDatabase("AllocationContext");
        }


        private void SplitSKU()
        {
            string[] tokens = _SKU.Split('-');
            _Division = tokens[0];
            _Dept = tokens[1];
            _Stock = tokens[2];
            _Color = tokens[3];
        }

        private List<WarehouseInventory> GetMainframeWarehouseInventory()
        {
            string size;
            string DCID;
            int onHandQty;
            int pickReserve;

            List<WarehouseInventory> warehouseInventoryList = new List<WarehouseInventory>();

            Database currDatabase = null;
            if (System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"].Contains(_Division))
            {
                currDatabase = _databaseEurope;
            }
            else
            {
                currDatabase = _database;
            }

            DbCommand SQLCommand;
            string SQL = "select to_char(WHSE_ID_NUM) as WHSE_ID_NUM, lpad(to_char(STK_SIZE_NUM), 3, '0') as Size, ";
            SQL = SQL + " to_number(ALLOCATABLE_BS_QTY) as OnHandQty, pick_rsrv_bs_qty as PickReserve ";
            SQL = SQL + " from TC052002 ";
            SQL = SQL + " where retl_oper_div_cd = '" + _Division + "' ";
            SQL = SQL + "and stk_dept_num = '" + _Dept + "' ";
            SQL = SQL + "and stk_num = '" + _Stock + "' ";
            SQL = SQL + "and stk_wc_num = '" + _Color + "' ";
            SQL = SQL + "and ALLOCATABLE_BS_QTY > 0 ";

            if (_WarehouseID != "-1")
                SQL = SQL + " and WHSE_ID_NUM = " + _WarehouseID;

            SQL = SQL + " union ";
            SQL = SQL + "select to_char(WHSE_ID_NUM) as WHSE_ID_NUM, to_char(CL_SCHED_NUM) as Size, ";
            SQL = SQL + " to_number(ALLOCATABLE_CL_QTY) as OnHandQty, pick_rsrv_cl_qty as PickReserve ";
            SQL = SQL + " from TC052010 ";
            SQL = SQL + " where ";
            SQL = SQL + "retl_oper_div_cd = '" + _Division + "' ";
            SQL = SQL + "and stk_dept_num = '" + _Dept + "' ";
            SQL = SQL + "and stk_num = '" + _Stock + "' ";
            SQL = SQL + "and stk_wc_num = '" + _Color + "' ";
            SQL = SQL + "and ALLOCATABLE_CL_QTY > 0 ";

            if (_WarehouseID != "-1")
                SQL = SQL + " and WHSE_ID_NUM = " + _WarehouseID;

            SQLCommand = currDatabase.GetSqlStringCommand(SQL);

            DataSet data = new DataSet();
            data = currDatabase.ExecuteDataSet(SQLCommand);

            if (data.Tables.Count > 0)
            {                
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    size = dr["Size"].ToString();
                    DCID = dr["WHSE_ID_NUM"].ToString();
                    onHandQty = Convert.ToInt32(dr["OnHandQty"]);
                    pickReserve = Convert.ToInt32(dr["PickReserve"]);

                    warehouseInventoryList.Add(new WarehouseInventory(_SKU, size, DCID, onHandQty, pickReserve));
                }
            }

            return warehouseInventoryList;
        }

        public List<WarehouseInventory> GetWarehouseInventory(string sku, string warehouseID)
        {
            _SKU = sku;
            _WarehouseID = warehouseID;

            SplitSKU();

            long itemID = (from a in db.ItemMasters
                           where a.MerchantSku.Equals(sku)
                           select a.ID).FirstOrDefault();

            List <WarehouseInventory> warehouseInventory;

            warehouseInventory = GetMainframeWarehouseInventory();

            List<string> uniqueDCs = (from wi in warehouseInventory
                                      select wi.DistributionCenterID).Distinct().ToList();

            foreach (string dc in uniqueDCs)
            {
                DistributionCenter tempDC = (from a in db.DistributionCenters
                                             where a.MFCode == dc
                                             select a).FirstOrDefault();
                warehouseInventory.Where(x => x.DistributionCenterID == dc).ToList().ForEach(y => y.distributionCenter = tempDC);
            }

            foreach (WarehouseInventory wi in warehouseInventory)
            {
                wi.itemID = itemID;

                if (wi.size.Length > 3)
                {
                    wi.caseLot = db.ItemPacks.Include("Details").FirstOrDefault(p => p.ItemID == itemID && p.Name == wi.size);
                }
            }

            var reductionDataForSku = (from rd in db.InventoryReductionsByType
                                       where rd.Sku == sku
                                       select rd).ToList();

            var reductionData = (from r in reductionDataForSku
                                 join w in warehouseInventory
                                  on new
                                  {                                      
                                      size = r.Size,
                                      dc = r.MFCode
                                  } equals new
                                  {                                      
                                      w.size,
                                      dc = w.DistributionCenterID
                                  }
                                 select r).ToList();

            foreach (var wi in warehouseInventory)
            {
                var reductionRec = reductionData.Where(x => x.Size == wi.size && x.MFCode == wi.DistributionCenterID).FirstOrDefault();

                if (reductionRec != null)
                {
                    wi.ringFenceQuantity = reductionRec.RingFenceQuantity;
                    wi.rdqQuantity = reductionRec.RDQQuantity;
                    wi.orderQuantity = Convert.ToInt32(reductionRec.OrderQuantity);
                }
            }

            return warehouseInventory;
        }
    }
}
