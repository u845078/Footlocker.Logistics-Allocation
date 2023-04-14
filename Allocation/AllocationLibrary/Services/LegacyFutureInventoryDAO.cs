
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Footlocker.Logistics.Allocation.Models;
using Footlocker.Logistics.Allocation.Factories;
using System.Linq;

namespace Footlocker.Logistics.Allocation.Services
{
    public class LegacyFutureInventoryDAO
    {
        readonly Database _database;
        readonly AllocationLibraryContext db = new AllocationLibraryContext();

        public LegacyFutureInventoryDAO()
        {
            _database = DatabaseFactory.CreateDatabase("AllocationContext");
        }

        public List<LegacyFutureInventory> GetLegacyFutureInventoryForSku(string sku)
        {
            List<LegacyFutureInventory> _que;
            _que = new List<LegacyFutureInventory>();

            DbCommand SQLCommand;
            string SQL = "dbo.GetLegacyFutureInventoryForSku";

            SQLCommand = _database.GetStoredProcCommand(SQL);
            _database.AddInParameter(SQLCommand, "@sku", DbType.String, sku);

            DataSet data = new DataSet();
            data = _database.ExecuteDataSet(SQLCommand);

            LegacyFutureInventoryFactory factory = new LegacyFutureInventoryFactory();

            if (data.Tables.Count > 0)
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    _que.Add(factory.Create(dr));
                }
            }
            return _que;
        }

        public List<LegacyFutureInventory> GetPOInventoryData(string division, string PO)
        {
            string inventoryID = string.Format("{0}-{1}", PO, division);

            return db.LegacyFutureInventory.Where(lfi => lfi.InventoryType == "PO" &&
                                                         lfi.LocNodeType == "WAREHOUSE" &&
                                                         lfi.Division == division &&
                                                         lfi.InventoryID == inventoryID).ToList();
        }
    }
}