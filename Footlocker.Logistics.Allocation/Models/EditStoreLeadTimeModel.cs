// -----------------------------------------------------------------------
// <copyright file="EditStoreLeadTimeModel.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Footlocker.Logistics.Allocation.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class EditStoreLeadTimeModel
    {
        public Boolean UpdateEntireZone { get; set; }
        public List<DistributionCenter> Warehouses { get; set; }
        public List<StoreLeadTime> LeadTimes { get; set; }
        public StoreLookup Store { get; set; }
        public StoreLookup BasedOnStore { get; set; }
    }
}
