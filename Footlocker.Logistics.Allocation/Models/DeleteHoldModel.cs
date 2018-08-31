using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class DeleteHoldModel
    {
        public Hold Hold { get; set; }
        public List<HoldRelease> HoldReleases { get; set; }
        public List<RDQ> CurrentRDQs { get; set; }

        public int NumberOfHolds { get; set; }
    }
}