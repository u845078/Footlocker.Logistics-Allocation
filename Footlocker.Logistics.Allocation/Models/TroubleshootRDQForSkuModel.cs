using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class TroubleshootRDQForSkuModel
    {
        public string Sku { get; set; }        
        public DateTime? ControlDate { get; set; }
    }
}