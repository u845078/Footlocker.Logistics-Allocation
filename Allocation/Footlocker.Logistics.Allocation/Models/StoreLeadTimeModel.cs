using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreLeadTimeModel
    {
        public List<Division> Divisions { get; set; }
        public string Division { get; set; }
        public List<StoreZoneModel> StoreZones { get; set; }
    }
}