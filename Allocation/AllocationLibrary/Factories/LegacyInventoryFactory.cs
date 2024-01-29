using System;
using System.Data;
using Footlocker.Logistics.Allocation.Models;

namespace Footlocker.Logistics.Allocation.Factories
{
    public class LegacyInventoryFactory
    {
        public LegacyInventory Create(DataRow dr)
        {
            LegacyInventory _newObject = new LegacyInventory()
            {
                InstanceID = Convert.ToInt32(dr["instanceid"]),
                ProductTypeCode = Convert.ToString(dr["ProductTypeCode"]),
                Sku = Convert.ToString(dr["Sku"]),
                Size = Convert.ToString(dr["Size"]),
                LocationTypeCode = Convert.ToString(dr["LocationTypeCode"]),
                Division = Convert.ToString(dr["Division"]),
                Store = Convert.ToString(dr["Store"]),
                DayDT = Convert.ToString(dr["DayDT"]),
                OnHandQuantity = Convert.ToString(dr["OnHandQuantity"])
            };

            return _newObject;
        }
    }
}