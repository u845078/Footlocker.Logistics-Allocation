using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreZoneModel
    {
        public string Division { get; set; }
        public string Store { get; set; }
        public string Mall { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZoneName { get; set; }
        public int ZoneID { get; set; }

    }
}