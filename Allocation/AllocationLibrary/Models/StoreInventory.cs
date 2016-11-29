using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreInventory
    {
        public StoreLookup store;
        public string storeInventorySize;
        public int onHandQuantity;
        public int binPickReserve;
        public int caselotPickReserve;
    }
}
