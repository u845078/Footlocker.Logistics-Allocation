using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class EditRouteZonesModel
    {
        public DistributionCenter DC { get; set; }
        public Route Route { get; set; }
        public List<RouteDetail> RouteDetails { get; set; }
        public List<NetworkZone> CurrentZones { get; set; }
        public List<NetworkZone> AvailableZones { get; set; }
    }
}