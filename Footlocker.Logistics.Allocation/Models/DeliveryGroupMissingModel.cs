using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class DeliveryGroupMissingModel
    {
        public List<StoreLookupModel> Stores { get; set; }
        public List<DeliveryGroup> DeliveryGroups { get; set; }
        public int PlanID { get; set; }
        public string RangeType { get; set; }
    }
}