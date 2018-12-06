using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class SkuSetupModel : RuleModel
    {
        public RangePlan RangePlan { get; set; }
        public List<SizeAllocation> SizeAllocations { get; set; }
        public List<SizeAllocationTotal> TotalSizeAllocations { get; set; }
        public List<DeliveryGroup> DeliveryGroups { get; set; }
        public List<RangeFileItem> QFeedExtract { get; set; }
        public OrderPlanningRequest OrderPlanningRequest { get; set; }
        public int StoreCount { get; set; }
        public int DeliveryStoreCount
        {
            get
            {
                if (this.DeliveryGroups != null)
                {
                    return DeliveryGroups.Sum(dg => dg.StoreCount);
                }
                else
                {
                    return 0;
                }
            }
        }

    }
}