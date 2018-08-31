
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class InventorySummaryFactory
    {
        public InventorySummary Create(DataRow dr)
        {
            InventorySummary _newObject = new InventorySummary();
            _newObject.ItemID = Convert.ToInt64(dr["id"]);
            _newObject.Sku = Convert.ToString(dr["Sku"]);
            _newObject.Size = Convert.ToString(dr["Size"]);
            if (!(Convert.IsDBNull(dr["Qty"])))
            {
                _newObject.Qty = Convert.ToInt32(dr["Qty"]);
            }

            return _newObject;
        }
    }
}