using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class DCList
    {
        public List<Instance> AvailableInstances { get; set; }
        public int InstanceID { get; set; }
        public List<DistributionCenter> DCs { get; set; }
    }
}