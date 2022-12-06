using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.ComponentModel;

namespace Footlocker.Logistics.Allocation.Models
{
    public class RangePlanModel 
    {
        public RangePlan Range { get; set; }

        public OrderPlanningRequest OPRequest { get; set; }
    }
}