
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class LegacyInventoryFactory
    {
        public LegacyInventory Create(DataRow dr)
        {
            LegacyInventory _newObject = new LegacyInventory();
            _newObject.InstanceID = Convert.ToInt32(dr["instanceid"]);
            _newObject.ProductTypeCode = Convert.ToString(dr["ProductTypeCode"]);
            _newObject.Sku = Convert.ToString(dr["Sku"]);
            _newObject.Size = Convert.ToString(dr["Size"]);
            _newObject.LocationTypeCode = Convert.ToString(dr["LocationTypeCode"]);
            _newObject.Division = Convert.ToString(dr["Division"]);
            _newObject.Store = Convert.ToString(dr["Store"]);
            _newObject.DayDT = Convert.ToString(dr["DayDT"]);
            _newObject.OnHandQuantity = Convert.ToString(dr["OnHandQuantity"]);

            return _newObject;
        }
    }
}