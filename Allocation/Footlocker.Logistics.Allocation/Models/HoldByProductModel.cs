using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class HoldByProductModel
    {
        public static Int32 lastID;

        public HoldByProductModel(string division, string level, string value, string holdType)
        {
            Division = division;
            Level = level;
            Value = value;
            HoldType = holdType;
            lastID++;
            ID = lastID;
        }

        public string Division { get; set; }
        public string Level { get; set; }
        public string Value { get; set; }
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