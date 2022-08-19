namespace Footlocker.Logistics.Allocation.Models
{
    using System.Collections.Generic;

    public class RouteDCModel
    {
        public Route Route { get; set; }
        public int instanceID { get; set; }

        public List<DistributionCenter> AssignedDCs { get; set; }

        public List<DistributionCenter> RemainingDCs { get; set; }
    }
}
