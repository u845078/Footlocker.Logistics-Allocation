using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    public class BulkRange
    {
        public string Division { get; set; }
        public string League { get; set; }
        public string Region { get; set; }
        public string Store { get; set; }
        public string Sku { get; set; }
        public string Size { get; set; }
        public string RangeStartDate { get; set; }
        public string DeliveryGroupName { get; set; }
        public string Range { get; set; }
        public string Min { get; set; }
        public string Max { get; set; }
        public string BaseDemand { get; set; }
        public string MinEndDaysOverride { get; set; }
        public string Error { get; set; }
        public string EndDate { get; set; }
    
    }
}
