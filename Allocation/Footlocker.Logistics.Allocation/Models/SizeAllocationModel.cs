using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class SizeAllocationModel : RuleModel
    {
        //public RangePlan Plan { get; set; }

        //public SkuAllocationModel SkuAllocationModel { get; set; }

        public List<SizeAllocationTotal> TotalAllocations { get; set; }

        public List<SizeAllocation> Allocations { get; set; }

        public List<DeliveryGroup> DeliveryGroups { get; set; }

        public OrderPlanningRequest OrderPlanningRequest { get; set; }

        public int StoreCount { get; set; }

        public int DeliveryStoreCount {
            get
            {
                return (from a in DeliveryGroups select a.StoreCount).Sum();
            }
        }
    }
}