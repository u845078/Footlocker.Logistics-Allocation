using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using System.Linq;
using System.Data.Entity;

namespace Footlocker.Logistics.Allocation.Services
{
    public class WarehouseInventoryDAO
    {
        public enum InventoryListType
        {
            ListAllSizes,
            ListOnlyAvailableSizes
        }

        readonly Microsoft.Practices.EnterpriseLibrary.Data.Database _database;
        readonly Microsoft.Practices.EnterpriseLibrary.Data.Database _databaseEurope;
        Microsoft.Practices.EnterpriseLibrary.Data.Database _currentDB2Database;
        readonly AllocationLibraryContext db = new AllocationLibraryContext();
        readonly SKUStruct _SKU;
        readonly string _WarehouseID;
        List<WarehouseInventory> warehouseInventory;
        private InventoryListType _inventoryListType;
        readonly string europeDivisions;

        public struct WarehouseInventoryLookup
        {
            public string SKU;
            public string Size;
            public string DCCode;
            public string DCID;
            public int OnHandQuantity;
        }

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
        /// <summary>
        /// This is a data access object that will figure out what inventory is available for allocation
        /// </summary>
        /// <param name="sku">The SKU that you need information</param>
        /// <param name="warehouseID">The warehouse for which to restrict inventory. Use -1 if you do not want to restrict inventory to just one warehouse</param>
        public WarehouseInventoryDAO(string sku, string warehouseID, string europeDivisions)
        {
            _database = DatabaseFactory.CreateDatabase("DB2PROD");
            _databaseEurope = DatabaseFactory.CreateDatabase("DB2EURP");

            this.europeDivisions = europeDivisions;

            if (!string.IsNullOrEmpty(sku))
            {
                _SKU = new SKUStruct(sku);
                SetCurrentDB2Database();
            }
                
            _WarehouseID = warehouseID;
        }

        private void SetCurrentDB2Database()
        {
            if (europeDivisions.Contains(_SKU.Division))            
                _currentDB2Database = _databaseEurope;            
            else            
                _currentDB2Database = _database;            
        }
       
        private List<WarehouseInventory> GetMainframeWarehouseInventory()
        {
            string size;
            string DCID;
            int onHandQty;
            int pickReserve;

            List<WarehouseInventory> warehouseInventoryList = new List<WarehouseInventory>();

            DbCommand SQLCommand;
            string SQL = "select to_char(WHSE_ID_NUM) as WHSE_ID_NUM, lpad(to_char(STK_SIZE_NUM), 3, '0') as Size, ";
            SQL += " to_number(ALLOCATABLE_BS_QTY) as OnHandQty, pick_rsrv_bs_qty as PickReserve ";
            SQL += " from TC052002 ";
            SQL += " where retl_oper_div_cd = ? and ";
            SQL += " stk_dept_num = ? and ";
            SQL += " stk_num = ? and ";
            SQL += " stk_wc_num = ? ";

            if (_inventoryListType == InventoryListType.ListOnlyAvailableSizes)
               SQL += " and ALLOCATABLE_BS_QTY > 0 ";

            if (_WarehouseID != "-1")
                SQL += " and WHSE_ID_NUM = ?";

            SQL += " union ";
            SQL += "select to_char(WHSE_ID_NUM) as WHSE_ID_NUM, to_char(CL_SCHED_NUM) as Size, ";
            SQL += " to_number(ALLOCATABLE_CL_QTY) as OnHandQty, pick_rsrv_cl_qty as PickReserve ";
            SQL += " from TC052010 ";
            SQL += " where retl_oper_div_cd = ? and ";
            SQL += " stk_dept_num = ? and ";
            SQL += " stk_num = ? and ";
            SQL += " stk_wc_num = ? ";

            if (_inventoryListType == InventoryListType.ListOnlyAvailableSizes)
                SQL += " and ALLOCATABLE_CL_QTY > 0 ";

            if (_WarehouseID != "-1")
                SQL += " and WHSE_ID_NUM = ?";

            SQLCommand = _currentDB2Database.GetSqlStringCommand(SQL);
            _currentDB2Database.AddInParameter(SQLCommand, "@1", DbType.String, _SKU.Division);
            _currentDB2Database.AddInParameter(SQLCommand, "@2", DbType.String, _SKU.Department);
            _currentDB2Database.AddInParameter(SQLCommand, "@3", DbType.String, _SKU.Stock);
            _currentDB2Database.AddInParameter(SQLCommand, "@4", DbType.String, _SKU.Color);

            if (_WarehouseID != "-1")
            {
                _currentDB2Database.AddInParameter(SQLCommand, "@5", DbType.String, _WarehouseID);
                _currentDB2Database.AddInParameter(SQLCommand, "@6", DbType.String, _SKU.Division);
                _currentDB2Database.AddInParameter(SQLCommand, "@7", DbType.String, _SKU.Department);
                _currentDB2Database.AddInParameter(SQLCommand, "@8", DbType.String, _SKU.Stock);
                _currentDB2Database.AddInParameter(SQLCommand, "@9", DbType.String, _SKU.Color);
                _currentDB2Database.AddInParameter(SQLCommand, "@10", DbType.String, _WarehouseID);
            }
            else
            {
                _currentDB2Database.AddInParameter(SQLCommand, "@5", DbType.String, _SKU.Division);
                _currentDB2Database.AddInParameter(SQLCommand, "@6", DbType.String, _SKU.Department);
                _currentDB2Database.AddInParameter(SQLCommand, "@7", DbType.String, _SKU.Stock);
                _currentDB2Database.AddInParameter(SQLCommand, "@8", DbType.String, _SKU.Color);
            }

            DataSet data;
            data = _currentDB2Database.ExecuteDataSet(SQLCommand);

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

        public List<WarehouseInventory> GetSQLWarehouseInventory(List<WarehouseInventoryLookup> inventoryLookup)
        {
            List<WarehouseInventory> returnValue = new List<WarehouseInventory>();

            var legacyWarehouseInv = db.LegacyInventories.Where(li => li.LocationTypeCode == "W").ToList();
            var DCList = db.DistributionCenters.ToList();

            var resultLookup = legacyWarehouseInv.Where(li => inventoryLookup.Any(il => il.SKU.Equals(li.Sku) &&
                                                                                        il.Size.Equals(li.Size) &&
                                                                                        il.DCCode.Equals(li.Store))).ToList();
            for (int i = 0; i < inventoryLookup.Count; i++)
            {
                string DCID = (from dcl in DCList
                               where dcl.MFCode == inventoryLookup[i].DCCode
                               select dcl.ID).FirstOrDefault().ToString();

                WarehouseInventory wil = new WarehouseInventory(inventoryLookup[i].SKU,
                                                                inventoryLookup[i].Size,
                                                                DCID,
                                                                0);

                var newValue = resultLookup.Where(rl => rl.Sku == wil.Sku &&
                                                        rl.Size == wil.size &&
                                                        rl.Store == inventoryLookup[i].DCCode).FirstOrDefault();
                if (newValue != null)
                    wil.quantity = Convert.ToInt32(newValue.OnHandQuantityInt);

                returnValue.Add(wil);
            }
            return returnValue;
        }

        public List<WarehouseInventory> GetWarehouseInventory(InventoryListType inventoryList)
        {
            _inventoryListType = inventoryList;

            long itemID = (from a in db.ItemMasters
                           where a.MerchantSku.Equals(_SKU.SKU)
                           select a.ID).FirstOrDefault();

            warehouseInventory = GetMainframeWarehouseInventory();

            SetDistributionCenters();

            warehouseInventory = warehouseInventory.Where(wi => wi.distributionCenter != null).ToList();

            SetCaselots(itemID);

            var AllocationDivision = db.AllocationDivisions.Where(ad => ad.DivisionCode == _SKU.Division).FirstOrDefault();

            var reductionDataForSku = db.InventoryReductionsByType.Where(irbt => irbt.Sku == _SKU.SKU && irbt.PO == "").ToList();

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
                wi.HasSeparateECOMInventory = AllocationDivision.HasSeparateECOMInventory;

                var reductionRec = reductionData.Where(x => x.Size == wi.size && x.MFCode == wi.DistributionCenterID).FirstOrDefault();

                if (reductionRec != null)
                {
                    wi.ringFenceQuantity = reductionRec.RingFenceQuantity;
                    wi.rdqQuantity = reductionRec.RDQQuantity;
                    wi.orderQuantity = reductionRec.OrderQuantity;
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
                DistributionCenter tempDC = db.DistributionCenters.Where(tdc => tdc.MFCode == dc).FirstOrDefault();
                warehouseInventory.Where(x => x.DistributionCenterID == dc).ToList().ForEach(y => y.distributionCenter = tempDC);
            }
        }
    }
}
