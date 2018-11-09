using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RouteModel
    {
        public int InstanceID { get; set; }
        public List<Instance> AvailableInstances { get; set; }
        public List<Route> Routes { get; set; }
    }
}