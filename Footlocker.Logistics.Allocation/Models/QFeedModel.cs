using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class QFeedModel
    {
        public List<String> VerificationMessages { get; set; }
        public List<RangeIssue> Issues { get; set; }
        public string Sku { get; set; }
        public List<Route> Routes { get; set; }
    }
}