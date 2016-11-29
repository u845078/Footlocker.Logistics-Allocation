using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class SkuAllocationModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<SizeAllocationTotal> TotalAllocations { get; set; }

        public List<SizeAllocation> Allocations { get; set; }

    }
}