using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class CopyRangePlanModel
    {
        public string FromSku {get;set;}
        public string FromDescription { get; set; }

        public string ToSku { get; set; }
        public string ToDescription { get; set; }

        public string Message { get; set; }
        public string PlanType { get; set; }
    }
}