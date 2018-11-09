using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class HoldByStoreModel
    {
        public static Int32 lastID;

        public HoldByStoreModel(string division, string store, string holdType)
        {
            Division = division;
            Store = store;
            HoldType = holdType;
            lastID++;
            ID = lastID;
        }

        public string Division { get; set; }
        public string Store { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string HoldType { get; set; }
        public string Comments { get; set; }
        public Int32 ID { get; set; }
        public Boolean ReserveInventoryBool 
        {
            get
            {
                return HoldType.Contains("Reserve");
            }
        }
    }
}