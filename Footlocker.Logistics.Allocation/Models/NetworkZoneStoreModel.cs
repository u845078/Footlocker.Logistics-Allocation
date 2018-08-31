using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Footlocker.Common;

namespace Footlocker.Logistics.Allocation.Models
{
    public class NetworkZoneStoreModel
    {
        public NetworkZone Zone { get; set; }
        public NetworkLeadTime LeadTime { get; set; }
        public Instance Instance { get; set; }

        public List<StoreLookup> Stores { get; set; }
        public List<Division> Divisions { get; set; }

        public string NewStore { get; set; }
        public string NewDivision { get; set; }

        public string Message { get; set; }
        public int ZoneID { get; set; }
    }
}