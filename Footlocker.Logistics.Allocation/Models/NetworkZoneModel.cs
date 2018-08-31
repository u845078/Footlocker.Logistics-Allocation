using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class NetworkZoneModel
    {
        public List<NetworkZone> NetworkZones { get; set; }

        public List<NetworkZone> AvailableZones { get; set; }

        public string NewZone { get; set; }

        public Instance Instance { get; set; }
        public NetworkLeadTime LeadTime { get; set; }
        public int LeadTimeID { get; set; }
    }
}