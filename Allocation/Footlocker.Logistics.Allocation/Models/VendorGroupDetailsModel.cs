using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class VendorGroupDetailsModel
    {
        //public List<VendorGroupDetail> Details { get; set; }
        public VendorGroup Header { get; set; }
        //public List<VendorGroupLeadTime> LeadTimes { get; set; }
        public bool HasDetails { get; set; }
    }
}