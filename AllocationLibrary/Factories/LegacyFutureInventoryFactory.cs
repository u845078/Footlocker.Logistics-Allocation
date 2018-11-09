
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class LegacyFutureInventoryFactory
    {
        public LegacyFutureInventory Create(DataRow dr)
        {
            LegacyFutureInventory _newObject = new LegacyFutureInventory();
            _newObject.InstanceID = Convert.ToInt32(dr["instanceid"]);
            _newObject.InventoryType = Convert.ToString(dr["InventoryType"]);
            _newObject.Sku = Convert.ToString(dr["Sku"]);
            _newObject.Size = Convert.ToString(dr["Size"]);
            _newObject.ProductNodeType = Convert.ToString(dr["ProductNodeType"]);
            _newObject.Division = Convert.ToString(dr["Division"]);
            _newObject.Store = Convert.ToString(dr["Store"]);
            _newObject.InventoryID = Convert.ToString(dr["InventoryID"]);
            _newObject.LocNodeType = Convert.ToString(dr["LocNodeType"]);
            _newObject.StockQty = Convert.ToInt32(dr["StockQty"]);

            return _newObject;
        }
    }
}