using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class DeliveryGroupModel : RuleFilterModel
    {
        public DeliveryGroup DeliveryGroup { get; set; }
        public RuleModel RuleModel { get; set; }
        public List<StoreLookupModel> PlanStores { get; set; }
    }
}