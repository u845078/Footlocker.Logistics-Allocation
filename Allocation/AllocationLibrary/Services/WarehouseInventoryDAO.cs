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
    public class WarehouseInventoryDAO
    {
        readonly Database _database;
        readonly Database _databaseEurope;
        readonly Database _sqlDB;
        AllocationLibraryContext db = new AllocationLibraryContext();
        readonly SKUStruct _SKU;
        readonly string _WarehouseID;
        List<WarehouseInventory> warehouseInventory;

        public struct SKUStruct
        {
            public string Division;
            public string Department;
            public string Stock;
            public string Color;
            public string SKU;

            public SKUStruct(string sku)
            {
                SKU = sku;
                string[] tokens = sku.Split('-');
                Division = tokens[0];
                Department = tokens[1];
                Stock = tokens[2];
                Color = tokens[3];
            }
        }

        public WarehouseInventoryDAO(string sku, string warehouseID)
        {
            _database = DatabaseFactory.CreateDatabase("DB2PROD");
            _databaseEurope = DatabaseFactory.CreateDatabase("DB2EURP");
            _sqlDB = DatabaseFactory.CreateDatabase("AllocationContext");
            _SKU = new SKUStruct(sku);
            _WarehouseID = warehouseID;
        }

        private List<WarehouseInventory> GetMainframeWarehouseInventory()
        {
            string size;
            string DCID;
            int onHandQty;
            int pickReserve;

            List<WarehouseInventory> warehouseInventoryList = new List<WarehouseInventory>();

            Database currDatabase = null;
            if (System.Configuration.ConfigurationManager.AppSettings["EUROPE_DIV"].Contains(_SKU.Division))
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
            SQL = SQL + " where retl_oper_div_cd = '" + _SKU.Division + "' ";
            SQL = SQL + "and stk_dept_num = '" + _SKU.Department + "' ";
            SQL = SQL + "and stk_num = '" + _SKU.Stock + "' ";
            SQL = SQL + "and stk_wc_num = '" + _SKU.Color + "' ";
            SQL = SQL + "and ALLOCATABLE_BS_QTY > 0 ";

            if (_WarehouseID != "-1")
                SQL = SQL + " and WHSE_ID_NUM = " + _WarehouseID;

            SQL = SQL + " union ";
            SQL = SQL + "select to_char(WHSE_ID_NUM) as WHSE_ID_NUM, to_char(CL_SCHED_NUM) as Size, ";
            SQL = SQL + " to_number(ALLOCATABLE_CL_QTY) as OnHandQty, pick_rsrv_cl_qty as PickReserve ";
            SQL = SQL + " from TC052010 ";
            SQL = SQL + " where ";
            SQL = SQL + "retl_oper_div_cd = '" + _SKU.Division + "' ";
            SQL = SQL + "and stk_dept_num = '" + _SKU.Department + "' ";
            SQL = SQL + "and stk_num = '" + _SKU.Stock + "' ";
            SQL = SQL + "and stk_wc_num = '" + _SKU.Color + "' ";
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

                    warehouseInventoryList.Add(new WarehouseInventory(_SKU.SKU, size, DCID, onHandQty, pickReserve));
                }
            }

            return warehouseInventoryList;
        }


        public List<WarehouseInventory> GetWarehouseInventory()
        {
            long itemID = (from a in db.ItemMasters
                           where a.MerchantSku.Equals(_SKU.SKU)
                           select a.ID).FirstOrDefault();

            warehouseInventory = GetMainframeWarehouseInventory();

            SetDistributionCenters();
            SetCaselots(itemID);

            var reductionDataForSku = (from rd in db.InventoryReductionsByType
                                       where rd.Sku == _SKU.SKU &&
                                             rd.PO == ""
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

        private void SetCaselots(long itemID)
        {
            foreach (WarehouseInventory wi in warehouseInventory)
            {
                wi.itemID = itemID;

                if (wi.size.Length > 3)
                {
                    wi.caseLot = db.ItemPacks.Include("Details").FirstOrDefault(p => p.ItemID == itemID && p.Name == wi.size);
                }
            }
        }

        private void SetDistributionCenters()
        {
            List<string> uniqueDCs = (from wi in warehouseInventory
                                      select wi.DistributionCenterID).Distinct().ToList();

            foreach (string dc in uniqueDCs)
            {
                DistributionCenter tempDC = (from a in db.DistributionCenters
                                             where a.MFCode == dc
                                             select a).FirstOrDefault();
                warehouseInventory.Where(x => x.DistributionCenterID == dc).ToList().ForEach(y => y.distributionCenter = tempDC);
            }
        }
    }
}
