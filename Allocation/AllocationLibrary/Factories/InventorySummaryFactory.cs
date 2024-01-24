using System;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class InventorySummaryFactory
    {
        public InventorySummary Create(DataRow dr)
        {
            InventorySummary _newObject = new InventorySummary()
            {
                ItemID = Convert.ToInt64(dr["id"]),
                Sku = Convert.ToString(dr["Sku"]),
                Size = Convert.ToString(dr["Size"])
            };

            if (!Convert.IsDBNull(dr["Qty"]))            
                _newObject.Qty = Convert.ToInt32(dr["Qty"]);            

            return _newObject;
        }
    }
}