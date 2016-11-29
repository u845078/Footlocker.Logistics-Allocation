using System;

namespace Footlocker.Logistics.Allocation.Models
{
    public class StoreBatchFilterModel : RuleFilterModel
    {
        public bool IsRestrictingToUnassignedCustomer { get; set; }
    }
}