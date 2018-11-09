using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class DistributionCenterModel
    {
        public DistributionCenter DC { get; set; }
        public int Zones { get; set; }
        public List<WarehouseAllocationType> WarehouseAllocationTypes { get; set; }

        public List<Instance> AvailableInstances { get; set; }
        public List<CheckBoxModel> SelectedInstances { get; set; }
    }
}