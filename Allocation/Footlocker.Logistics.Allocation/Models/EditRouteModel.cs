using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class EditRouteModel
    {
        public List<DistributionCenterModel> DCs { get; set; }
        public Route Route { get; set; }
    }
}