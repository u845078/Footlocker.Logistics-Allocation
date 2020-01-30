using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Footlocker.Logistics.Allocation.Models
{
    public class PreSaleModel
    {        
        public string SKU { get; set; }
        public string SKUDescription { get; set; }
        public PreSaleSKU preSaleSKU { get; set; }
    }
}