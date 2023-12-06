using System.Collections.Generic;

namespace Footlocker.Logistics.Allocation.Models
{
    public class EditStoreLeadTimeModel
    {
        public bool UpdateEntireZone { get; set; }
        public List<DistributionCenter> Warehouses { get; set; }
        public List<StoreLeadTime> LeadTimes { get; set; }
        public StoreLookup Store { get; set; }
        public StoreLookup BasedOnStore { get; set; }
    }
}
