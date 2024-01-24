using System;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class LegacyFutureInventoryFactory
    {
        public LegacyFutureInventory Create(DataRow dr)
        {
            LegacyFutureInventory _newObject = new LegacyFutureInventory()
            {
                InstanceID = Convert.ToInt32(dr["instanceid"]),
                InventoryType = Convert.ToString(dr["InventoryType"]),
                Sku = Convert.ToString(dr["Sku"]),
                Size = Convert.ToString(dr["Size"]),
                ProductNodeType = Convert.ToString(dr["ProductNodeType"]),
                Division = Convert.ToString(dr["Division"]),
                Store = Convert.ToString(dr["Store"]),
                InventoryID = Convert.ToString(dr["InventoryID"]),
                LocNodeType = Convert.ToString(dr["LocNodeType"]),
                StockQty = Convert.ToInt32(dr["StockQty"])
            };

            return _newObject;
        }
    }
}