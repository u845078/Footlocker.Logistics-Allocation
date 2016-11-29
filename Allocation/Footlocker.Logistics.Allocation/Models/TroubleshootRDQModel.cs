using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class TroubleshootRDQModel
    {
        public int InstanceID { get; set; }
        public List<Instance> AvailableInstances { get; set; }
        public DateTime ControlDate { get; set; }
    }
}