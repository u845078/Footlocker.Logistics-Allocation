namespace Footlocker.Logistics.Allocation.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class RouteDCModel
    {
        public Route Route { get; set; }

        public List<DistributionCenter> AssignedDCs { get; set; }

        public List<DistributionCenter> RemainingDCs { get; set; }
    }
}
